using UnityEngine;

namespace Game.Scripts.Environment.Grid.Render
{
    public class ChessCellMaterialProvider : CellMaterialProvider
    {
        private const int ModuloDivisor = 2;
        private const int ExpectedRemainder = 0;

        [SerializeField] private Material _firstMaterial;
        [SerializeField] private Material _secondMaterial;

        public override Material GetMaterial(int row, int column)
        {
            return IsFirstMaterialCell(row, column)
                ? _firstMaterial
                : _secondMaterial;
        }

        private bool IsFirstMaterialCell(int row, int column)
        {
            return (row + column) % ModuloDivisor == ExpectedRemainder;
        }
    }
}