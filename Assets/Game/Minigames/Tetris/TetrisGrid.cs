using UnityEngine;

namespace Game.Minigames.Tetris
{
    /// <summary>
    /// Модель сетки для тетриса: хранит занятость клеток и переводит мировые
    /// координаты в индексы клеток и обратно. Физики нет — только геометрия.
    /// Позиция transform этого объекта = центр клетки (0,0), т.е. левый-нижний угол сетки.
    /// </summary>
    public class TetrisGrid : MonoBehaviour
    {
        [Tooltip("Сколько клеток по горизонтали.")]
        [SerializeField] private int columns = 5;

        [Tooltip("Сколько клеток по вертикали.")]
        [SerializeField] private int rows = 5;

        [Tooltip("Размер одной клетки в мировых единицах. Шаг между клетками " +
                 "внутри префабов фигур должен совпадать с этим значением.")]
        [SerializeField] private float cellSize = 1f;

        [Header("Гизмо для удобной настройки в редакторе")]
        [SerializeField] private bool drawGizmos = true;

        // occupants[x, y] — фигура, занимающая клетку, или null если клетка свободна.
        private TetrisPiece[,] _occupants;

        public int Columns => columns;
        public int Rows => rows;
        public float CellSize => cellSize;

        private void Awake()
        {
            ResetGrid();
        }

        /// <summary>Полностью очищает занятость сетки.</summary>
        public void ResetGrid()
        {
            _occupants = new TetrisPiece[columns, rows];
        }

        /// <summary>Клетка внутри границ сетки?</summary>
        public bool IsInside(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < columns &&
                   cell.y >= 0 && cell.y < rows;
        }

        /// <summary>Мировая точка -> индекс ближайшей клетки.</summary>
        public Vector2Int WorldToCell(Vector3 world)
        {
            Vector3 local = world - transform.position;

            return new Vector2Int(
                Mathf.RoundToInt(local.x / cellSize),
                Mathf.RoundToInt(local.y / cellSize));
        }

        /// <summary>Индекс клетки -> мировая позиция её центра.</summary>
        public Vector3 CellToWorld(Vector2Int cell)
        {
            return transform.position + new Vector3(cell.x * cellSize, cell.y * cellSize, 0f);
        }

        /// <summary>
        /// Можно ли разместить набор клеток: все внутри сетки и свободны.
        /// ignore — фигура, чьи собственные клетки не считаем занятыми
        /// (нужно при перекладывании фигуры на новое место).
        /// </summary>
        public bool CanPlace(Vector2Int[] cells, TetrisPiece ignore)
        {
            foreach (Vector2Int c in cells)
            {
                if (!IsInside(c))
                {
                    return false;
                }

                TetrisPiece occupant = _occupants[c.x, c.y];

                if (occupant != null && occupant != ignore)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Помечает клетки занятыми указанной фигурой.</summary>
        public void Occupy(Vector2Int[] cells, TetrisPiece piece)
        {
            foreach (Vector2Int c in cells)
            {
                _occupants[c.x, c.y] = piece;
            }
        }

        /// <summary>Освобождает все клетки, занятые этой фигурой.</summary>
        public void Free(TetrisPiece piece)
        {
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (_occupants[x, y] == piece)
                    {
                        _occupants[x, y] = null;
                    }
                }
            }
        }

        /// <summary>Все клетки заняты?</summary>
        public bool IsFull()
        {
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (_occupants[x, y] == null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Vector3 size = new Vector3(cellSize, cellSize, 0f) * 0.98f;

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Vector3 center = transform.position +
                                     new Vector3(x * cellSize, y * cellSize, 0f);
                    Gizmos.DrawWireCube(center, size);
                }
            }
        }
    }
}
