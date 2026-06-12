using System;
using UnityEngine;

namespace Game.Scripts.Environment.Grid.Road
{
    [Serializable]
    public class RoadPathSettings
    {
        [SerializeField] private float _roadOffsetY = 0.04f;
        [SerializeField] private float _roadHalfWidth = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _splineSmoothCurvature = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _houseBranchBlend = 0.35f;
        [SerializeField, Min(0.01f)] private float _minNodeDirection = 0.15f;
        [SerializeField, Min(0.05f)] private float _cornerFilletRadius = 0.4f;
        [SerializeField, Range(0.1f, 0.49f)] private float _cornerFilletMaxSegmentRatio = 0.45f;
        [SerializeField, Range(2, 12)] private int _cornerArcSegments = 5;
        [SerializeField, Range(5f, 45f)] private float _minCornerAngle = 15f;
        [SerializeField] private float _houseSplineZOffset = 0f;

        public float RoadOffsetY => _roadOffsetY;
        public float RoadHalfWidth => _roadHalfWidth;
        public float SplineSmoothCurvature => _splineSmoothCurvature;
        public float HouseBranchBlend => _houseBranchBlend;
        public float MinNodeDirection => _minNodeDirection;
        public float CornerFilletRadius => _cornerFilletRadius;
        public float CornerFilletMaxSegmentRatio => _cornerFilletMaxSegmentRatio;
        public int CornerArcSegments => _cornerArcSegments;
        public float MinCornerAngle => _minCornerAngle;
        public float HouseSplineZOffset => _houseSplineZOffset;
    }
}
