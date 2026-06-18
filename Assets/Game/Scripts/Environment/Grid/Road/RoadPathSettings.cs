using System;
using UnityEngine;

namespace Game.Scripts.Environment.Grid.Road
{
    [Serializable]
    public class RoadPathSettings
    {
        [SerializeField] private float _roadOffsetY = 0.04f;
        [SerializeField, Range(0f, 1f)] private float _splineSmoothCurvature = 0.35f;
        [SerializeField, Min(0.01f)] private float _minNodeDirection = 0.15f;

        public float RoadOffsetY => _roadOffsetY;
        public float SplineSmoothCurvature => _splineSmoothCurvature;
        public float MinNodeDirection => _minNodeDirection;
    }
}
