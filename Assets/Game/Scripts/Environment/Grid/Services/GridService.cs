using Game.Scripts.Environment.Grid.Configuration;
using UnityEngine;

namespace Game.Scripts.Environment.Grid.Services
{
    public class GridService : MonoBehaviour
    {
        [SerializeField] private FieldLayout _fieldLayout;

        private Cell[,] _cells;

        public float CellSize => _fieldLayout.CellSize;

        public void BindCells(Cell[,] cells)
        {
            _cells = cells;
        }

        public Cell GetNeighborCell(Cell cell, Vector3 direction)
        {
            if (_cells == null)
                return null;

            int newRow = cell.Row + Mathf.RoundToInt(-direction.z);
            int newCol = cell.Column + Mathf.RoundToInt(direction.x);

            if (newRow < 0 || newRow >= _cells.GetLength(0))
                return null;

            if (newCol < 0 || newCol >= _cells.GetLength(1))
                return null;

            return _cells[newRow, newCol];
        }
    }
}
