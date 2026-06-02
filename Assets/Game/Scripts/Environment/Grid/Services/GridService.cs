using Game.Scripts.Environment.Grid.Spawner;
using UnityEngine;

namespace Game.Scripts.Environment.Grid.Services
{
    public class GridService : MonoBehaviour
    {
        private const float CellDetectionRadius = 0.5f;

        [SerializeField] private CellsSpawner _cellsSpawner;

        private Cell[,] _cells;

        public float CellSize => _cellsSpawner.FieldCells.CellSize;

        private void Start()
        {
            _cells = _cellsSpawner.Cells;
        }

        public Cell GetCellAtPosition(Vector3 position)
        {
            for (int row = 0; row < _cells.GetLength(0); row++)
                for (int col = 0; col < _cells.GetLength(1); col++)
                    if (Vector3.Distance(_cells[row, col].transform.position, position) < CellDetectionRadius)
                        return _cells[row, col];
            
            return null;
        }

        public Cell GetNeighborCell(Cell cell, Vector3 direction)
        {
            for (int row = 0; row < _cells.GetLength(0); row++)
            {
                for (int col = 0; col < _cells.GetLength(1); col++)
                {
                    if (_cells[row, col] != cell)
                    {
                        continue;
                    }

                    int newRow = row + Mathf.RoundToInt(-direction.z);
                    int newCol = col + Mathf.RoundToInt(direction.x);

                    if (newRow < 0 || newRow >= _cells.GetLength(0))
                    {
                        return null;
                    }

                    if (newCol < 0 || newCol >= _cells.GetLength(1))
                    {
                        return null;
                    }

                    return _cells[newRow, newCol];
                }
            }

            return null;
        }
    }
}