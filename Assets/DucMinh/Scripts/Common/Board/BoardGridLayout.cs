using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DucMinh
{
    public class BoardGridLayout : IBoardLayout
    {
        private int _rowCount, _columnCount;
        private float _cellWidth, _cellHeight;
        private float _scale;
        private float _spacing;
        public float _unScaledCellWidth, _unScaledCellHeight;
        public Vector3 _topLeftCorner, _bottomRightCorner;

        private float _boardWidth, _boardHeight;
        public Vector3 _boardCenter;

        public int CellCount => _rowCount * _columnCount;
        public int RowCount => _rowCount;
        public int ColumnCount => _columnCount;
        public float CellWidth => _cellWidth;
        public float CellHeight => _cellHeight;
        public float BoardWidth => _boardWidth;
        public float BoardHeight => _boardHeight;
        public Vector3 BoardCenter => _boardCenter;
        public float Scale => _scale;
        public float Spacing => _spacing;

        public BoardGridLayout(int rowCount, int columnCount, float unScaledCellWidth,
            float unScaledCellHeight, Vector3 topLeftCorner, Vector3 bottomRightCorner, float spacing = 0f)
        {
            _rowCount = rowCount;
            _columnCount = columnCount;
            _unScaledCellWidth = unScaledCellWidth;
            _unScaledCellHeight = unScaledCellHeight;
            _topLeftCorner = topLeftCorner;
            _bottomRightCorner = bottomRightCorner;
            _spacing = spacing;
            
            Recalculate();
        }

        private void Recalculate()
        {
            //Calculate size of the cells
            var playWidth = _bottomRightCorner.x - _topLeftCorner.x;
            var playHeight = _topLeftCorner.y - _bottomRightCorner.y;

            // Account for spacing in scale so board fits within play area
            var unscaledBoardWidth  = _unScaledCellWidth  * _columnCount + _spacing * (_columnCount - 1);
            var unscaledBoardHeight = _unScaledCellHeight * _rowCount    + _spacing * (_rowCount    - 1);

            _scale = Mathf.Min(playWidth / unscaledBoardWidth, playHeight / unscaledBoardHeight);
            _cellWidth  = _unScaledCellWidth  * _scale;
            _cellHeight = _unScaledCellHeight * _scale;
            _spacing   *= _scale;
            
            //Calculate size of the board
            _boardWidth  = _cellWidth  * _columnCount + _spacing * (_columnCount - 1);
            _boardHeight = _cellHeight * _rowCount    + _spacing * (_rowCount    - 1);
            
            //Calculate the top left and bottom right corners of the board
            var centerX = (_bottomRightCorner.x + _topLeftCorner.x) * 0.5f;
            var centerY = (_bottomRightCorner.y + _topLeftCorner.y) * 0.5f;
            _boardCenter = new Vector3(centerX, centerY, 0f);
            _topLeftCorner = new Vector2(centerX - _boardWidth * 0.5f, centerY + _boardHeight * 0.5f);
            _bottomRightCorner = new Vector2(centerX + _boardWidth * 0.5f, centerY - _boardHeight * 0.5f);
        }

        public bool GetIndicesCell(Vector3 position, out int index)
        {
            index = -1;
            if (position.x < _topLeftCorner.x || position.y > _topLeftCorner.y || 
                position.x > _bottomRightCorner.x || position.y < _bottomRightCorner.y)
            {
                return false;
            }
            
            // Calculate the index of the cell based on the position
            var col = Mathf.CeilToInt((position.x - _topLeftCorner.x) / _cellWidth) - 1;
            var row = Mathf.CeilToInt((_topLeftCorner.y - position.y) / _cellHeight) - 1;
            index = row * _columnCount + col;
            return true;
        }

        public bool GetCellPosition(int index, out Vector3 position)
        {
            position = default;
            if (index < 0 || index >= CellCount)
            {
                return false;
            }
            
            //Calculate indices of the cell based on the index
            var y = index / _columnCount;
            var x = index % _columnCount;
            
            // Calculate the position of the cell based on the indices
            var posX = _topLeftCorner.x + (x + 0.5f) * _cellWidth + x * _spacing;
            var posY = _topLeftCorner.y - (y + 0.5f) * _cellHeight - y * _spacing;
            position = new Vector3(posX, posY, 0f);
            return true;
        }

        public void OnDrawGizmos()
        {
#if UNITY_EDITOR
            GizmosHelper.DrawGrid(
                topLeft:    _topLeftCorner,
                rows:       _rowCount,
                columns:    _columnCount,
                cellWidth:  _cellWidth,
                cellHeight: _cellHeight,
                spacing:    _spacing,
                boardColor: Color.yellow,
                cellColor:  new Color(1f, 1f, 1f, 0.4f)
            );

            // Draw center point
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_boardCenter, Mathf.Min(_cellWidth, _cellHeight) * 0.05f);

            // Draw cell indices
            for (var i = 0; i < CellCount; i++)
            {
                if (!GetCellPosition(i, out var pos)) continue;
                GizmosHelper.DrawText(pos, i.ToString(), Color.cyan, 10);
            }
#endif
        }
    }
}