using UnityEngine;

namespace DucMinh
{
    /// <summary>
    /// Interface for defining the layout of a board.
    /// This interface allows for different implementations of board layouts,
    /// such as grid layouts, hexagonal layouts, etc.
    /// It provides methods to set the layout, get cell indices based on world position,
    /// get the world position of a cell based on its indices, and clear the layout.
    /// </summary>
    public interface IBoardLayout
    {
        /// <summary>
        /// Get total number of cells in the board layout.
        /// </summary>
        int CellCount { get; }
        /// <summary>
        /// Get the cell indices based on the position in world space.
        /// If the position is outside the board, return false and set x, y to -1.
        /// </summary>
        /// <param name="position">world position</param>
        /// <param name="index">index of cell</param>
        /// <returns></returns>
        bool GetIndicesCell(Vector3 position, out int index);

        /// <summary>
        /// Get the world position of a cell based on its indices.
        /// If the indices are out of bounds, return false and set position to Vector3.zero
        /// </summary>
        /// <param name="index">index of cell</param>
        /// <param name="position">world position of cell</param>
        /// <returns></returns>
        bool GetCellPosition(int index, out Vector3 position);
        void OnDrawGizmos();
    }
}