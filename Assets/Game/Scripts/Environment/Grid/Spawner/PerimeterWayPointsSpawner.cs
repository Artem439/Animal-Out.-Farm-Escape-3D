using System.Collections.Generic;
using Game.Scripts.Environment.Grid.Configuration;
using UnityEngine;

namespace Game.Scripts.Environment.Grid.Spawner
{
    public class PerimeterWayPointsSpawner : MonoBehaviour
    {
        [SerializeField] private FieldLayout _fieldLayout;
        [SerializeField] private WayPoint _pointPrefab;
        [SerializeField] private Transform _pointsParent;

        public WayPoint[,] Points { get; private set; }

        private void Awake()
        {
            if (_pointsParent == null)
                _pointsParent = transform;
        }

        public void Build()
        {
            int rows = _fieldLayout.Length + _fieldLayout.PerimeterBorderOffset;
            int cols = _fieldLayout.Width + _fieldLayout.PerimeterBorderOffset;
            Points = new WayPoint[rows, cols];

            Vector3 startPosition = _fieldLayout.GetPerimeterStartPosition();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    bool isPerimeter = row == 0 || row == rows - 1 || col == 0 || col == cols - 1;

                    if (isPerimeter == false)
                        continue;

                    Vector3 position = new Vector3(
                        startPosition.x + col * _fieldLayout.CellSize,
                        startPosition.y,
                        startPosition.z - row * _fieldLayout.CellSize);

                    WayPoint point = Instantiate(_pointPrefab, position, Quaternion.identity, _pointsParent);
                    Points[row, col] = point;
                }
            }
        }

        public void TraversePerimeter(List<WayPoint> result)
        {
            int rows = Points.GetLength(0);
            int cols = Points.GetLength(1);

            for (int col = 0; col < cols; col++)
                if (Points[0, col] != null)
                    result.Add(Points[0, col]);

            for (int row = 1; row < rows; row++)
                if (Points[row, cols - 1] != null)
                    result.Add(Points[row, cols - 1]);

            for (int col = cols - 2; col >= 0; col--)
                if (Points[rows - 1, col] != null)
                    result.Add(Points[rows - 1, col]);

            for (int row = rows - 2; row > 0; row--)
                if (Points[row, 0] != null)
                    result.Add(Points[row, 0]);
        }
    }
}
