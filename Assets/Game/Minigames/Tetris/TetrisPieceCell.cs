using UnityEngine;

namespace Game.Minigames.Tetris
{
    /// <summary>
    /// Маркер одной клетки фигуры. Висит на каждом дочернем квадрате префаба
    /// (рядом со SpriteRenderer и Collider2D). Через него клик по любой части
    /// фигуры сопоставляется с конкретной клеткой и родительской фигурой —
    /// поэтому хватать фигуру можно за любую её клетку.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TetrisPieceCell : MonoBehaviour
    {
        public TetrisPiece Piece { get; private set; }

        // Смещение этой клетки относительно базовой клетки фигуры (в индексах сетки).
        public Vector2Int Offset { get; private set; }

        public void Bind(TetrisPiece piece, Vector2Int offset)
        {
            Piece = piece;
            Offset = offset;
        }
    }
}
