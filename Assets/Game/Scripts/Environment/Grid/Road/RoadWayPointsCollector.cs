using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Environment.Grid.Road
{
    public static class RoadWayPointsCollector
    {
        private const string WayPointPrefix = "WayPoint";

        public static List<Transform> Collect(Transform roadRoot)
        {
            List<(int index, Transform transform)> ordered = new List<(int, Transform)>();

            foreach (Transform child in roadRoot.GetComponentsInChildren<Transform>(true))
            {
                if (child == roadRoot)
                    continue;

                if (TryParseIndex(child.name, out int index) == false)
                    continue;

                ordered.Add((index, child));
            }

            ordered.Sort((left, right) => left.index.CompareTo(right.index));

            List<Transform> waypoints = new List<Transform>(ordered.Count);

            foreach ((int _, Transform transform) in ordered)
                waypoints.Add(transform);

            return waypoints;
        }

        public static Transform FindClosestWaypoint(IReadOnlyList<Transform> waypoints, Vector3 targetPosition)
        {
            Transform closest = waypoints[0];
            float closestSqrDistance = SqrDistanceXZ(closest.position, targetPosition);

            for (int i = 1; i < waypoints.Count; i++)
            {
                float sqrDistance = SqrDistanceXZ(waypoints[i].position, targetPosition);

                if (sqrDistance >= closestSqrDistance)
                    continue;

                closestSqrDistance = sqrDistance;
                closest = waypoints[i];
            }

            return closest;
        }

        private static bool TryParseIndex(string name, out int index)
        {
            index = 0;

            if (name.StartsWith(WayPointPrefix) == false)
                return false;

            return int.TryParse(name.Substring(WayPointPrefix.Length), out index);
        }

        private static float SqrDistanceXZ(Vector3 left, Vector3 right)
        {
            float deltaX = left.x - right.x;
            float deltaZ = left.z - right.z;
            return deltaX * deltaX + deltaZ * deltaZ;
        }
    }
}
