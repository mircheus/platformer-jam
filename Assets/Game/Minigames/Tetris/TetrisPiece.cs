using UnityEngine;

namespace Game.Minigames.Tetris
{
    /// <summary>
    /// Перетаскиваемая фигура тетриса. Форма задаётся набором дочерних клеток
    /// (TetrisPieceCell) — никакой генерации, всё авторится в префабе.
    /// Физики нет: при невалидном броске фигура телепортируется на стартовое место.
    /// Контроллер (Tetris) сам опрашивает мышь и дёргает методы Begin/Update/EndDrag.
    /// </summary>
    public class TetrisPiece : MonoBehaviour
    {
        [Tooltip("На сколько поднять sortingOrder спрайтов, пока фигуру тащат, " +
                 "чтобы она рисовалась поверх остальных.")]
        [SerializeField] private int dragSortingBoost = 100;

        private TetrisGrid _grid;
        private TetrisPieceCell[] _cells;

        // Смещения клеток фигуры в индексах сетки (параллельны массиву _cells).
        private Vector2Int[] _offsets;

        private Vector3 _homePosition;
        private bool _placed;

        // Состояние текущего перетаскивания.
        private bool _dragging;
        private TetrisPieceCell _grabbedCell;
        private Vector3 _dragOffset; // transform.position - точка захвата в мире

        // Для подъёма/возврата порядка отрисовки во время drag.
        private SpriteRenderer[] _renderers;
        private int[] _baseSortingOrders;

        public bool IsPlaced => _placed;

        /// <summary>Инициализация контроллером перед стартом игры.</summary>
        public void Init(TetrisGrid grid)
        {
            _grid = grid;
            _homePosition = transform.position;
            _placed = false;
            _dragging = false;

            CacheCells();
            CacheRenderers();
        }

        private void CacheCells()
        {
            _cells = GetComponentsInChildren<TetrisPieceCell>(true);
            _offsets = new Vector2Int[_cells.Length];

            // Смещение каждой клетки выводим из её локальной позиции, округляя к шагу сетки.
            // Абсолютной нормализации не нужно: в проверках всё считается относительно
            // схваченной клетки.
            for (int i = 0; i < _cells.Length; i++)
            {
                Vector3 local = _cells[i].transform.localPosition;

                Vector2Int offset = new Vector2Int(
                    Mathf.RoundToInt(local.x / _grid.CellSize),
                    Mathf.RoundToInt(local.y / _grid.CellSize));

                _offsets[i] = offset;
                _cells[i].Bind(this, offset);
            }
        }

        private void CacheRenderers()
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            _baseSortingOrders = new int[_renderers.Length];

            for (int i = 0; i < _renderers.Length; i++)
            {
                _baseSortingOrders[i] = _renderers[i].sortingOrder;
            }
        }

        public void BeginDrag(TetrisPieceCell grabbedCell, Vector3 worldPoint)
        {
            _dragging = true;
            _grabbedCell = grabbedCell;
            _dragOffset = transform.position - worldPoint;

            // Если фигура уже лежала в сетке — освобождаем её клетки, чтобы
            // можно было переложить её (в т.ч. на пересекающееся со старым место).
            if (_placed)
            {
                _grid.Free(this);
                _placed = false;
            }

            SetSortingBoost(true);
        }

        public void UpdateDrag(Vector3 worldPoint)
        {
            if (!_dragging)
            {
                return;
            }

            // Двигаем жёстко, сохраняя точку захвата под курсором.
            Vector3 target = worldPoint + _dragOffset;
            target.z = _homePosition.z;
            transform.position = target;
        }

        /// <summary>Завершает перетаскивание. Возвращает true, если фигура встала в сетку.</summary>
        public bool EndDrag()
        {
            if (!_dragging)
            {
                return false;
            }

            _dragging = false;
            SetSortingBoost(false);

            // Клетка сетки под схваченной клеткой фигуры -> отсюда восстанавливаем
            // базовую клетку и весь набор клеток фигуры.
            Vector2Int targetCell = _grid.WorldToCell(_grabbedCell.transform.position);
            Vector2Int baseCell = targetCell - _grabbedCell.Offset;
            Vector2Int[] cells = BuildCells(baseCell);

            bool placed = _grid.CanPlace(cells, this);

            if (placed)
            {
                SnapTo(targetCell);
                _grid.Occupy(cells, this);
                _placed = true;
            }
            else
            {
                ReturnHome();
            }

            _grabbedCell = null;
            return placed;
        }

        private Vector2Int[] BuildCells(Vector2Int baseCell)
        {
            var cells = new Vector2Int[_offsets.Length];

            for (int i = 0; i < _offsets.Length; i++)
            {
                cells[i] = baseCell + _offsets[i];
            }

            return cells;
        }

        private void SnapTo(Vector2Int targetCell)
        {
            // Сдвигаем фигуру так, чтобы схваченная клетка точно села в центр targetCell.
            // Остальные клетки лягут по своим центрам, т.к. шаг клеток фигуры = cellSize.
            Vector3 snapDelta = _grid.CellToWorld(targetCell) - _grabbedCell.transform.position;

            Vector3 pos = transform.position + snapDelta;
            pos.z = _homePosition.z;
            transform.position = pos;
        }

        private void ReturnHome()
        {
            transform.position = _homePosition;
        }

        /// <summary>
        /// Полный сброс фигуры на стартовое место: прерывает текущее перетаскивание
        /// (если есть), освобождает занятые клетки сетки и возвращает позицию.
        /// </summary>
        public void ResetToStart()
        {
            if (_dragging)
            {
                _dragging = false;
                _grabbedCell = null;
                SetSortingBoost(false);
            }

            if (_placed)
            {
                _grid.Free(this);
                _placed = false;
            }

            ReturnHome();
        }

        private void SetSortingBoost(bool on)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].sortingOrder = on
                    ? _baseSortingOrders[i] + dragSortingBoost
                    : _baseSortingOrders[i];
            }
        }
    }
}
