using System.Collections.Generic;
using Game.Scripts.Environment.Grid.Road;
using SplineMesh;
using UnityEngine;

namespace Game.Scripts.Environment.Grid.Spawner
{
    public class PerimeterRoadBuilder : MonoBehaviour
    {
        private const string PerimeterSplineName = "PerimeterSpline";
        private const string HouseSplineName = "HouseSpline";
        private const string BranchSplinePrefix = "BranchSpline";

        [SerializeField] private FieldEnvironmentSpawner _fieldEnvironmentSpawner;
        [SerializeField] private Transform _exitPoint;
        [SerializeField] private Transform _pointsParent;
        [SerializeField] private RoadPathSettings _pathSettings = new();

        private readonly List<GameObject> _branchSplineObjects = new();
        private readonly List<Spline> _branchSplines = new();

        private GameObject _perimeterSplineObject;
        private GameObject _houseSplineObject;

        public Transform ExitPoint => _exitPoint;
        public Spline PerimeterSpline { get; private set; }
        public Spline HouseSpline { get; private set; }
        public IReadOnlyList<Spline> BranchSplines => _branchSplines;
        public Vector3 Junction { get; private set; }
        public Spline RightBranchSpline { get; private set; }
        public Spline LeftBranchSpline { get; private set; }
        public int RightBranchAnchorNodeIndex { get; private set; } = -1;
        public int LeftBranchAnchorNodeIndex { get; private set; } = -1;

        public Spline Spline => PerimeterSpline;

        private void Awake()
        {
            if (_pointsParent == null)
                _pointsParent = transform;
        }

        public void Build()
        {
            PerimeterSpline = null;
            HouseSpline = null;
            RightBranchSpline = null;
            LeftBranchSpline = null;
            RightBranchAnchorNodeIndex = -1;
            LeftBranchAnchorNodeIndex = -1;
            Junction = Vector3.zero;
            _branchSplines.Clear();

            if (_perimeterSplineObject != null)
                Destroy(_perimeterSplineObject);

            if (_houseSplineObject != null)
                Destroy(_houseSplineObject);

            foreach (GameObject branchSplineObject in _branchSplineObjects)
            {
                if (branchSplineObject != null)
                    Destroy(branchSplineObject);
            }

            _branchSplineObjects.Clear();

            if (_fieldEnvironmentSpawner == null || _fieldEnvironmentSpawner.RoadInstance == null)
                return;

            List<Transform> waypoints = RoadWayPointsCollector.Collect(_fieldEnvironmentSpawner.RoadInstance.transform);

            if (waypoints.Count < 2)
                return;

            RoadPaths paths = RoadPathBuilder.Build(
                _fieldEnvironmentSpawner.RoadInstance.transform,
                waypoints,
                _exitPoint,
                transform.position.y,
                _pathSettings);

            Junction = paths.Junction;

            PerimeterSpline = CreateSpline(PerimeterSplineName, paths.Perimeter, true);

            if (paths.UseBranches)
            {
                RoadPathMerge merge = _fieldEnvironmentSpawner.RoadInstance.GetComponent<RoadPathMerge>();

                for (int i = 0; i < paths.Branches.Count; i++)
                {
                    Spline branchSpline = CreateSpline($"{BranchSplinePrefix}{i}", paths.Branches[i], false);

                    if (branchSpline == null)
                        continue;

                    _branchSplines.Add(branchSpline);
                }

                if (_branchSplines.Count > 0)
                    RightBranchSpline = _branchSplines[0];

                if (_branchSplines.Count > 1)
                    LeftBranchSpline = _branchSplines[1];

                if (merge != null && PerimeterSpline != null)
                {
                    if (merge.RightBranchWaypoint != null)
                        RightBranchAnchorNodeIndex = FindNearestNodeIndex(PerimeterSpline, merge.RightBranchWaypoint.position);

                    if (merge.LeftBranchWaypoint != null)
                        LeftBranchAnchorNodeIndex = FindNearestNodeIndex(PerimeterSpline, merge.LeftBranchWaypoint.position);
                }
            }

            HouseSpline = CreateSpline(HouseSplineName, paths.House, false);

            if (PerimeterSpline == null || HouseSpline == null)
            {
                Junction = Vector3.zero;
            }
        }

        private Spline CreateSpline(string objectName, List<Vector3> positions, bool isPerimeterSpline)
        {
            GameObject splineObject = new GameObject(objectName);
            splineObject.transform.SetParent(_pointsParent, false);
            splineObject.transform.localPosition = Vector3.zero;
            splineObject.transform.localRotation = Quaternion.identity;
            splineObject.transform.localScale = Vector3.one;

            Spline spline = splineObject.AddComponent<Spline>();
            spline.nodes.Clear();
            spline.curves.Clear();
            spline.IsLoop = false;
            RoadPathBuilder.AddSplineNodes(spline, splineObject.transform, positions, _pathSettings);

            if (spline.nodes.Count < 2)
            {
                Destroy(splineObject);
                return null;
            }

            if (isPerimeterSpline)
                _perimeterSplineObject = splineObject;
            else if (objectName == HouseSplineName)
                _houseSplineObject = splineObject;
            else
                _branchSplineObjects.Add(splineObject);

            return spline;
        }

        private static int FindNearestNodeIndex(Spline spline, Vector3 worldPosition)
        {
            int bestIndex = 0;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < spline.nodes.Count; i++)
            {
                Vector3 nodeWorld = spline.transform.TransformPoint(spline.nodes[i].Position);
                float distance = Vector3.Distance(
                    new Vector3(nodeWorld.x, 0f, nodeWorld.z),
                    new Vector3(worldPosition.x, 0f, worldPosition.z));

                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                bestIndex = i;
            }

            return bestIndex;
        }
    }
}
