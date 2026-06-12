using Game.Scripts.Environment.Grid.Configuration;
using UnityEngine;

namespace Game.Scripts.Resources.Environment
{
    [CreateAssetMenu(fileName = "FieldConfiguration", menuName = "Gameplay/Field Configuration")]
    public class FieldConfiguration : ScriptableObject
    {
        [SerializeField] private FieldSize _size = FieldSize.Large;
        [SerializeField, Min(0.01f)] private float _cellSize = 1f;
        [SerializeField, Min(0)] private int _perimeterBorderOffset = 2;

        public FieldSize Size => _size;
        public float CellSize => _cellSize;
        public int PerimeterBorderOffset => _perimeterBorderOffset;
        public int Width => FieldSizeUtility.GetGridSize(_size);
        public int Length => FieldSizeUtility.GetGridSize(_size);
    }
}
