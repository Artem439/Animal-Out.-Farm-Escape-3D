using Game.Scripts.Environment.Grid.Configuration;
using Game.Scripts.Environment.Grid.Render;
using UnityEngine;

namespace Game.Scripts.Environment.Grid.Spawner
{
    public class CellsSpawner : MonoBehaviour
    {
        [SerializeField] private Cell _cellPrefab;
        [SerializeField] private FieldLayout _fieldLayout;
        [SerializeField] private Transform _cellsParent;
        [SerializeField] private CellMaterialProvider _materialProvider;

        public Cell[,] Cells { get; private set; }
        public FieldLayout FieldLayout => _fieldLayout;

        private void Awake()
        {
            if (_cellsParent == null)
                _cellsParent = transform;
        }

        public void Build()
        {
            Cells = new Cell[_fieldLayout.Length, _fieldLayout.Width];

            for (int row = 0; row < _fieldLayout.Length; row++)
            {
                for (int column = 0; column < _fieldLayout.Width; column++)
                    Cells[row, column] = SpawnCell(row, column);
            }
        }

        private Cell SpawnCell(int row, int column)
        {
            Vector3 position = _fieldLayout.GetCellPosition(row, column);

            Cell cell = Instantiate(
                _cellPrefab,
                position,
                Quaternion.identity,
                _cellsParent);

            cell.Initialize(row, column);

            if (_fieldLayout.Configuration != null && _fieldLayout.Configuration.HideCellVisuals)
                cell.SetVisualEnabled(false);
            else if (_materialProvider != null)
                cell.SetMaterial(_materialProvider.GetMaterial(row, column));

            return cell;
        }
    }
}
