using System.Collections.Generic;
using Game.Scripts.Environment.Grid.Road;
using SplineMesh;
using UnityEngine;

namespace Game.Scripts.Environment.Grid.Spawner
{
    public class PerimeterRoadBuilder : MonoBehaviour
    {
        private const string SplineName = "PerimeterSpline";
        
        [SerializeField] private PerimeterWayPointsSpawner _wayPointsSpawner;
        [SerializeField] private Transform _exitPoint;
        [SerializeField] private Transform _pointsParent;
        [SerializeField] private Material _perimeterMaterial;
        [SerializeField] private RoadPathSettings _pathSettings = new();
        
        [Header("Road Mesh")]
        [SerializeField] private float _roadHalfWidth = 0.5f;
        [SerializeField] private float _roadThickness = 0.03f;
        [SerializeField, Min(0.01f)] private float _sampleSpacing = 0.08f;
        
        private List<float> _curveStartDistances;
        private GameObject _splineObject;
        
        public Transform ExitPoint => _exitPoint;

        public Spline Spline { get; private set; }
        
        private void Awake()
        {
            if (_pointsParent == null)
                _pointsParent = transform;
        }
        
        public void Build()
        {
            if (_splineObject != null)
                Destroy(_splineObject);
            
            List<Vector3> positions = RoadPathBuilder.Build(
                _wayPointsSpawner,
                _exitPoint,
                transform.position.y,
                _pathSettings);
            
            Spline = RoadPathBuilder.BuildSplineMesh(
                _pointsParent,
                SplineName,
                _perimeterMaterial,
                _roadHalfWidth,
                _roadThickness,
                _sampleSpacing,
                positions,
                _pathSettings);
            
            _splineObject = Spline.gameObject;
            CacheCurveDistances();
        }
        
        private void CacheCurveDistances()
        {
            _curveStartDistances = new List<float>(Spline.curves.Count);
            float distance = 0f;
            
            foreach (CubicBezierCurve curve in Spline.curves)
            {
                _curveStartDistances.Add(distance);
                distance += curve.Length;
            }
        }
        
        public float GetTotalDistance(CurveSample sample)
        {
            for (int i = 0; i < Spline.curves.Count; i++)
                if (Spline.curves[i] == sample.curve)
                    return _curveStartDistances[i] + sample.distanceInCurve;
            
            return 0f;
        }
    }
}