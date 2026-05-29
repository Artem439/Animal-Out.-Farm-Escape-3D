using Game.Scripts.Environment.Grid.Render;
using UnityEngine;

namespace Game.Scripts.Environment.Grid.Spawner
{
    public class CellsSpawner : MonoBehaviour
    {
        private const float OffsetY = 0.01f;
        private const float HalfDivider = 2f;

        [SerializeField] private Cell _cellPrefab;
        [SerializeField] private FieldCells _fieldCells;
        [SerializeField] private Transform _cellsParent;
        [SerializeField] private CellMaterialProvider _materialProvider;

        private Cell[,] _cells;

        public Cell[,] Cells => _cells;

        private void Awake()
        {
            _cells = new Cell[_fieldCells.Length, _fieldCells.Width];

            if (_cellsParent == null)
                _cellsParent = transform;
        }

        private void Start()
        {
            Spawn();
        }

        private void Spawn()
        {
            Vector3 startPosition = GetStartSpawnPosition();

            for (int row = 0; row < _fieldCells.Length; row++)
            {
                for (int column = 0; column < _fieldCells.Width; column++)
                {
                    Cell cell = SpawnCell(startPosition, row, column);

                    _cells[row, column] = cell;
                }
            }
        }

        private Cell SpawnCell(Vector3 startPosition, int row, int column)
        {
            Vector3 position = GetCellPosition(startPosition, row, column);

            Cell cell = Instantiate(
                _cellPrefab,
                position,
                Quaternion.identity,
                _cellsParent
            );

            cell.SetMaterial(_materialProvider.GetMaterial(row, column));

            return cell;
        }

        private Vector3 GetStartSpawnPosition()
        {
            float widthOffset = (_fieldCells.Width - 1) * _fieldCells.CellSize / HalfDivider;
            float lengthOffset = (_fieldCells.Length - 1) * _fieldCells.CellSize / HalfDivider;

            return new Vector3(
                transform.position.x - widthOffset,
                transform.position.y + OffsetY,
                transform.position.z + lengthOffset
            );
        }

        private Vector3 GetCellPosition(Vector3 startPosition, int row, int column)
        {
            return new Vector3(
                startPosition.x + column * _fieldCells.CellSize,
                startPosition.y,
                startPosition.z - row * _fieldCells.CellSize
            );
        }
    }
}