using System;
using UnityEngine;

namespace Game.Scripts.Environment.Grid
{
    [Serializable]
    public class FieldCells
    {
        private const int MinSize = 1;
        private const float MinCellSize = 0.01f;

        [SerializeField, Min(MinSize)] private int _width = 8;
        [SerializeField, Min(MinSize)] private int _length = 8;
        [SerializeField, Min(MinCellSize)] private float _cellSize = 1f;

        public int Width => _width;
        public int Length => _length;
        public float CellSize => _cellSize;
    }
}