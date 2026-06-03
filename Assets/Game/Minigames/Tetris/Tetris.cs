using System.Collections;
using Minigames;
using Minigames.Contract;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Minigames.Tetris
{
    /// <summary>
    /// Мини-игра «тетрис»: перетаскиваем заранее заданные фигуры мышью и
    /// складываем их в сетку 5x5. Никакой генерации и физики — фигуры лежат на
    /// сцене префабами, при невалидном броске возвращаются на старт.
    /// Победа — когда сетка заполнена целиком.
    /// </summary>
    public class Tetris : MinigameBase
    {
        [SerializeField] private TetrisGrid grid;

        [Tooltip("Камера для пикинга мышью. Если пусто — берётся Camera.main " +
                 "(её ведёт Cinemachine во время World-миниигры).")]
        [SerializeField] private Camera worldCamera;

        [Tooltip("Слой(и), на котором лежат клетки фигур. Клик ищет коллайдеры " +
                 "только здесь, чтобы не цепляться за коллайдеры основного геймплея.")]
        [SerializeField] private LayerMask pieceLayer;

        [Header("Win Condition")]
        [Tooltip("Пауза перед завершением после заполнения сетки, чтобы переход " +
                 "не был резким, сек.")]
        [SerializeField] private float winDelay = 0.4f;

        [Header("Debug")]
        [Tooltip("Подробные логи для отладки Drag&Drop. Выключить, когда всё заработает.")]
        [SerializeField] private bool debugLogs = true;

        private TetrisPiece[] _pieces;
        private Camera _camera;
        private bool _active;
        private TetrisPiece _dragging;

        protected override void OnBegin(MinigameContext ctx)
        {
            Debug.Log($"[Tetris] Begin | source: {ctx.SourceObjectID}");

            if (grid == null)
            {
                grid = GetComponentInChildren<TetrisGrid>(true);
            }

            if (grid == null)
            {
                Debug.LogError("[Tetris] TetrisGrid не найден! Игра не стартует. " +
                               "Проставь поле grid или положи TetrisGrid в дочерние объекты.");
                return;
            }

            _camera = worldCamera != null ? worldCamera : Camera.main;

            if (_camera == null)
            {
                Debug.LogError("[Tetris] Камера не найдена (worldCamera пуст и Camera.main == null). " +
                               "Пропиши worldCamera в инспекторе.");
                return;
            }

            grid.ResetGrid();

            _pieces = GetComponentsInChildren<TetrisPiece>(true);

            foreach (TetrisPiece piece in _pieces)
            {
                piece.Init(grid);
            }

            if (pieceLayer.value == 0)
            {
                Debug.LogWarning("[Tetris] pieceLayer не задан — пикинг не найдёт ни одной фигуры. " +
                                 "Выбери в инспекторе слой клеток фигур.");
            }

            _dragging = null;
            _active = true;

            Debug.Log($"[Tetris] Готово. camera='{_camera.name}' (ortho={_camera.orthographic}), " +
                      $"фигур={_pieces.Length}, gridZ={grid.transform.position.z}, " +
                      $"pieceLayer.value={pieceLayer.value}, " +
                      $"queriesHitTriggers={Physics2D.queriesHitTriggers}");
        }

        private void Update()
        {
            if (!_active)
            {
                return;
            }

            Mouse mouse = Mouse.current;

            if (mouse == null)
            {
                if (debugLogs)
                {
                    Debug.LogWarning("[Tetris] Mouse.current == null (нет мыши / Input System не активен?)");
                }

                return;
            }

            if (_camera == null)
            {
                return;
            }

            Vector3 world = ScreenToWorld(mouse.position.ReadValue());

            if (_dragging == null)
            {
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    if (debugLogs)
                    {
                        Debug.Log($"[Tetris] ЛКМ нажата | screen={mouse.position.ReadValue()} -> world={world}");
                    }

                    TryBeginDrag(world);
                }
            }
            else if (mouse.leftButton.isPressed)
            {
                _dragging.UpdateDrag(world);
            }
            else
            {
                // Кнопку отпустили (или ввод потерян) — завершаем перетаскивание.
                EndDrag();
            }
        }

        private void TryBeginDrag(Vector3 world)
        {
            Collider2D hit = Physics2D.OverlapPoint(world, pieceLayer);

            if (hit == null)
            {
                if (debugLogs)
                {
                    // Покажем, сколько коллайдеров нужного слоя рядом — чтобы понять,
                    // дело в позиции точки или в слое/коллайдерах.
                    Collider2D[] near = Physics2D.OverlapCircleAll(world, 1.5f, pieceLayer);
                    Debug.Log($"[Tetris] OverlapPoint({(Vector2)world}) на слое pieceLayer -> ничего. " +
                              $"Коллайдеров нужного слоя в радиусе 1.5: {near.Length}");
                }

                return;
            }

            TetrisPieceCell cell = hit.GetComponentInParent<TetrisPieceCell>();

            if (cell == null)
            {
                if (debugLogs)
                {
                    Debug.Log($"[Tetris] Попали в коллайдер '{hit.name}', но на нём нет TetrisPieceCell.");
                }

                return;
            }

            if (cell.Piece == null)
            {
                Debug.LogWarning($"[Tetris] Клетка '{cell.name}' не привязана к фигуре " +
                                 "(piece.Init не вызывался?).");
                return;
            }

            _dragging = cell.Piece;
            _dragging.BeginDrag(cell, world);
        }

        private void EndDrag()
        {
            bool placed = _dragging.EndDrag();

            if (debugLogs)
            {
                Debug.Log($"[Tetris] Отпустили '{_dragging.name}' -> " +
                          $"{(placed ? "уложено" : "возврат на старт")}.");
            }

            _dragging = null;

            if (placed && grid.IsFull())
            {
                Win();
            }
        }

        /// <summary>
        /// Возвращает все фигуры на стартовые места и очищает сетку.
        /// Публичный — чтобы вешать на OnClick UI-кнопки сброса.
        /// </summary>
        public void ResetAll()
        {
            if (_pieces == null)
            {
                return;
            }

            // Сбрасываем ссылку на тащимую фигуру: её ResetToStart ниже корректно
            // завершит перетаскивание сам.
            _dragging = null;

            foreach (TetrisPiece piece in _pieces)
            {
                piece.ResetToStart();
            }

            if (debugLogs)
            {
                Debug.Log("[Tetris] Сброс всех фигур на старт.");
            }
        }

        public void Win()
        {
            Debug.Log("[Tetris] Сетка заполнена — победа!");

            PlaySuccessSound();

            _active = false;
            StartCoroutine(CompleteAfterDelay());
        }

        private IEnumerator CompleteAfterDelay()
        {
            if (winDelay > 0f)
            {
                yield return new WaitForSeconds(winDelay);
            }

            Complete(new MinigameResult());
        }

        private Vector3 ScreenToWorld(Vector2 screen)
        {
            // Универсально для ортографической и перспективной камеры: пускаем луч из
            // экранной точки и пересекаем его с плоскостью доски (z = gridZ).
            Ray ray = _camera.ScreenPointToRay(screen);
            float boardZ = grid != null ? grid.transform.position.z : 0f;
            Plane boardPlane = new Plane(Vector3.forward, new Vector3(0f, 0f, boardZ));

            if (boardPlane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }

            // Запасной путь, если луч параллелен плоскости.
            Vector3 screenPoint = new Vector3(screen.x, screen.y, _camera.nearClipPlane);
            return _camera.ScreenToWorldPoint(screenPoint);
        }
    }
}
