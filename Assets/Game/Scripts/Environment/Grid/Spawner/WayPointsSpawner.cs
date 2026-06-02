using System.Collections.Generic;
using SplineMesh;
using UnityEngine;

namespace Game.Scripts.Environment.Grid.Spawner
{
    public class WayPointsSpawner : MonoBehaviour
    {
        private const int BorderOffset = 2;
        private const float OffsetY = 0.01f;
        private const float HalfDivider = 2f;
        private const int HouseInsertIndex = 5;
        private const int RingTopEndIndex = 4;
        private const string _splineName = "PerimeterSpline";

        [SerializeField] private FieldCells _fieldCells;
        [SerializeField] private WayPoint _pointPrefab;
        [SerializeField] private Transform _pointsParent;
        [SerializeField] private Transform _exitPoint;

        private WayPoint[,] _points;
        private List<float> _curveStartDistances;
        
        public WayPoint[,] Points => _points;
        public WayPoint ExitWayPoint { get; private set; }
        public Transform ExitPoint => _exitPoint;
        public Spline Spline { get; private set; }

        private void Awake()
        {
            int rows = _fieldCells.Length + BorderOffset;
            int cols = _fieldCells.Width + BorderOffset;
            _points = new WayPoint[rows, cols];

            if (_pointsParent == null)
                _pointsParent = transform;
        }

        private void Start()
        {
            Spawn();
            BuildRing();
            FindExitWayPoint();
            BuildSpline();
        }

        private void Spawn()
        {
            Vector3 startPosition = GetStartSpawnPosition();

            int rows = _fieldCells.Length + BorderOffset;
            int cols = _fieldCells.Width + BorderOffset;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    bool isPerimeter = row == 0 || row == rows - 1 || col == 0 || col == cols - 1;

                    if (isPerimeter == false)
                    {
                        continue;
                    }

                    Vector3 position = new Vector3(
                        startPosition.x + col * _fieldCells.CellSize,
                        startPosition.y,
                        startPosition.z - row * _fieldCells.CellSize
                    );

                    WayPoint point = Instantiate(_pointPrefab, position, Quaternion.identity, _pointsParent);
                    _points[row, col] = point;
                }
            }
        }

        private Vector3 GetStartSpawnPosition()
        {
            float widthOffset = (_fieldCells.Width + BorderOffset - 1) * _fieldCells.CellSize / HalfDivider;
            float lengthOffset = (_fieldCells.Length + BorderOffset - 1) * _fieldCells.CellSize / HalfDivider;

            return new Vector3(
                transform.position.x - widthOffset,
                transform.position.y + OffsetY,
                transform.position.z + lengthOffset
            );
        }

        public Vector3 GetPerimeterEntryPoint(Vector3 position, Vector3 direction)
        {
            direction = new Vector3(direction.x, 0f, direction.z).normalized;

            float halfWidth = (_fieldCells.Width + BorderOffset - 1) * _fieldCells.CellSize * 0.5f;
            float halfLength = (_fieldCells.Length + BorderOffset - 1) * _fieldCells.CellSize * 0.5f;

            float minX = transform.position.x - halfWidth;
            float maxX = transform.position.x + halfWidth;
            float minZ = transform.position.z - halfLength;
            float maxZ = transform.position.z + halfLength;

            float entryX;
            float entryZ;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                entryX = direction.x > 0 ? maxX : minX;
                float t = (entryX - position.x) / direction.x;
                entryZ = Mathf.Clamp(position.z + t * direction.z, minZ, maxZ);
            }
            else
            {
                entryZ = direction.z > 0 ? maxZ : minZ;
                float t = (entryZ - position.z) / direction.z;
                entryX = Mathf.Clamp(position.x + t * direction.x, minX, maxX);
            }

            return new Vector3(entryX, position.y, entryZ);
        }

        private void BuildRing()
        {
            List<WayPoint> perimeter = new List<WayPoint>();
            TraversePerimeter(perimeter);

            for (int i = 0; i < perimeter.Count; i++)
            {
                WayPoint current = perimeter[i];
                current.Next = perimeter[(i + 1) % perimeter.Count];
                current.Previous = perimeter[(i - 1 + perimeter.Count) % perimeter.Count];
            }
        }

        private void TraversePerimeter(List<WayPoint> result)
        {
            int rows = _points.GetLength(0);
            int cols = _points.GetLength(1);

            for (int col = 0; col < cols; col++)
                if (_points[0, col] != null)
                    result.Add(_points[0, col]);
            
            for (int row = 1; row < rows; row++)
                if (_points[row, cols - 1] != null)
                    result.Add(_points[row, cols - 1]);

            for (int col = cols - 2; col >= 0; col--)
                if (_points[rows - 1, col] != null)
                    result.Add(_points[rows - 1, col]);
            
            for (int row = rows - 2; row > 0; row--)
                if (_points[row, 0] != null)
                    result.Add(_points[row, 0]);
        }

        private void FindExitWayPoint()
        {
            float minDistance = float.MaxValue;

            foreach (WayPoint point in _points)
            {
                if (point == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(
                    point.transform.position,
                    _exitPoint.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    ExitWayPoint = point;
                }
            }
        }

        private void BuildSpline()
        {
            GameObject splineGO = new GameObject(_splineName);
            splineGO.transform.SetParent(_pointsParent);
            Spline = splineGO.AddComponent<Spline>();
            Spline.nodes.Clear();
            Spline.curves.Clear();

            List<WayPoint> ring = new List<WayPoint>();
            TraversePerimeter(ring);

            Vector3 housePosition = _exitPoint.position;
            housePosition.y = transform.position.y + OffsetY;

            List<Vector3> positions = new List<Vector3>(ring.Count + 2);

            for (int i = 0; i <= RingTopEndIndex; i++)
            {
                positions.Add(ring[i].transform.position);
            }

            positions.Add(housePosition);

            for (int i = HouseInsertIndex; i < ring.Count; i++)
            {
                positions.Add(ring[i].transform.position);
            }

            positions.Add(ring[0].transform.position);

            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 pos = positions[i];
                Spline.AddNode(new SplineNode(pos, pos));
            }

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