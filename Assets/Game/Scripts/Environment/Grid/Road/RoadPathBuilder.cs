using System.Collections.Generic;
using SplineMesh;
using UnityEngine;

namespace Game.Scripts.Environment.Grid.Road
{
    public sealed class RoadPaths
    {
        public List<Vector3> Perimeter { get; }
        public IReadOnlyList<List<Vector3>> Branches { get; }
        public List<Vector3> House { get; }
        public Vector3 Junction { get; }

        public bool UseBranches => Branches != null && Branches.Count > 0;

        public RoadPaths(List<Vector3> perimeter, List<Vector3> house, Vector3 junction)
            : this(perimeter, null, house, junction)
        {
        }

        public RoadPaths(
            List<Vector3> perimeter,
            IReadOnlyList<List<Vector3>> branches,
            List<Vector3> house,
            Vector3 junction)
        {
            Perimeter = perimeter;
            Branches = branches;
            House = house;
            Junction = junction;
        }
    }

    public static class RoadPathBuilder
    {
        private const float MinPointDistance = 0.05f;

        public static RoadPaths Build(
            Transform roadRoot,
            IReadOnlyList<Transform> waypoints,
            Transform exitPoint,
            float originY,
            RoadPathSettings settings)
        {
            float roadY = originY + settings.RoadOffsetY;
            RoadPathMerge merge = roadRoot != null ? roadRoot.GetComponent<RoadPathMerge>() : null;
            List<Vector3> perimeter = BuildPerimeterPath(waypoints, roadY, merge);

            if (merge != null && merge.MergeWaypoint != null)
            {
                Vector3 junction = ToRoadPosition(merge.MergeWaypoint.position, roadY);
                List<List<Vector3>> branches = BuildBranchPaths(merge, exitPoint, roadY, junction);

                if (branches.Count > 0)
                {
                    return new RoadPaths(
                        perimeter,
                        branches,
                        BuildHousePath(exitPoint, roadY, junction),
                        junction);
                }
            }

            Transform junctionWaypoint = merge != null && merge.MergeWaypoint != null
                ? merge.MergeWaypoint
                : FindClosestWaypoint(waypoints, exitPoint.position);
            Vector3 fallbackJunction = ToRoadPosition(junctionWaypoint.position, roadY);

            return new RoadPaths(
                perimeter,
                BuildHousePath(exitPoint, roadY, fallbackJunction),
                fallbackJunction);
        }

        private static List<List<Vector3>> BuildBranchPaths(
            RoadPathMerge merge,
            Transform exitPoint,
            float roadY,
            Vector3 junction)
        {
            List<List<Vector3>> branches = new List<List<Vector3>>(2);
            Vector3 houseCenter = new Vector3(exitPoint.position.x, roadY, exitPoint.position.z);

            if (merge.RightBranchWaypoint != null)
            {
                List<Vector3> rightBranch = BuildBranchPath(merge.RightBranchWaypoint, junction, houseCenter, roadY);

                if (rightBranch.Count >= 2)
                    branches.Add(rightBranch);
            }

            if (merge.LeftBranchWaypoint != null)
            {
                List<Vector3> leftBranch = BuildBranchPath(merge.LeftBranchWaypoint, junction, houseCenter, roadY);

                if (leftBranch.Count >= 2)
                    branches.Add(leftBranch);
            }

            return branches;
        }

        private static List<Vector3> BuildBranchPath(
            Transform branchWaypoint,
            Vector3 junction,
            Vector3 houseCenter,
            float roadY)
        {
            return SanitizePath(new List<Vector3>
            {
                ToRoadPosition(branchWaypoint.position, roadY),
                junction,
                houseCenter
            });
        }

        public static void AddSplineNodes(
            Spline spline,
            Transform splineTransform,
            List<Vector3> worldPositions,
            RoadPathSettings settings)
        {
            List<Vector3> positions = SanitizePath(worldPositions);

            if (positions.Count < 2)
                return;

            AddSplineNodes(positions, settings, (worldPosition, worldDirection) =>
            {
                Vector3 localPosition = splineTransform.InverseTransformPoint(worldPosition);
                Vector3 localDirection = splineTransform.InverseTransformPoint(
                    EnsureValidDirection(worldPosition, worldDirection, settings.MinNodeDirection));

                spline.AddNode(new SplineNode(localPosition, localDirection));
            });
        }

        private static void AddSplineNodes(List<Vector3> positions, RoadPathSettings settings, System.Action<Vector3, Vector3> addNode)
        {
            if (positions.Count == 2)
            {
                AddStraightSplineNodes(positions, settings, addNode);
                return;
            }

            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 position = positions[i];
                Vector3 direction = ComputeNodeDirection(positions, i, settings);
                addNode(position, direction);
            }
        }

        private static List<Vector3> BuildPerimeterPath(
            IReadOnlyList<Transform> waypoints,
            float roadY,
            RoadPathMerge merge)
        {
            List<Vector3> positions = new List<Vector3>(waypoints.Count);
            Vector3? mergePosition = merge != null && merge.MergeWaypoint != null
                ? ToRoadPosition(merge.MergeWaypoint.position, roadY)
                : null;

            foreach (var t in waypoints)
            {
                if (t == null)
                    continue;

                if (merge != null && merge.MergeWaypoint != null && t == merge.MergeWaypoint)
                    continue;

                Vector3 position = ToRoadPosition(t.position, roadY);

                if (mergePosition.HasValue && Distance(position, mergePosition.Value) < MinPointDistance)
                    continue;

                positions.Add(position);
            }

            return SanitizePath(positions);
        }

        private static List<Vector3> BuildHousePath(Transform exitPoint, float roadY, Vector3 junction)
        {
            Vector3 houseCenter = new Vector3(exitPoint.position.x, roadY, exitPoint.position.z);

            return SanitizePath(new List<Vector3>
            {
                junction,
                houseCenter
            });
        }

        private static List<Vector3> SanitizePath(List<Vector3> positions)
        {
            List<Vector3> sanitized = new List<Vector3>(positions.Count);

            foreach (Vector3 position in positions)
            {
                if (IsFinite(position) == false)
                    continue;

                if (sanitized.Count > 0 && Distance(sanitized[^1], position) < MinPointDistance)
                    continue;

                sanitized.Add(position);
            }

            return sanitized;
        }

        private static void AddStraightSplineNodes(List<Vector3> positions, RoadPathSettings settings, System.Action<Vector3, Vector3> addNode)
        {
            Vector3 start = positions[0];
            Vector3 end = positions[1];
            Vector3 delta = end - start;
            float handle = Mathf.Max(delta.magnitude / 3f, settings.MinNodeDirection);
            Vector3 forward = delta.sqrMagnitude > 0.0001f ? delta.normalized * handle : Vector3.forward * handle;

            addNode(start, EnsureValidDirection(start, start + forward, settings.MinNodeDirection));
            addNode(end, EnsureValidDirection(end, end + forward, settings.MinNodeDirection));
        }

        private static Vector3 ToRoadPosition(Vector3 source, float roadY)
        {
            return new Vector3(source.x, roadY, source.z);
        }

        private static Transform FindClosestWaypoint(IReadOnlyList<Transform> waypoints, Vector3 targetPosition)
        {
            return RoadWayPointsCollector.FindClosestWaypoint(waypoints, targetPosition);
        }

        private static Vector3 ComputeNodeDirection(List<Vector3> positions, int index, RoadPathSettings settings)
        {
            Vector3 position = positions[index];
            Vector3 tangent;
            float directionLength;
            float smoothness = settings.SplineSmoothCurvature;

            if (index == 0)
            {
                tangent = positions[1] - position;
                directionLength = tangent.magnitude;
            }
            else if (index == positions.Count - 1)
            {
                tangent = position - positions[index - 1];
                directionLength = tangent.magnitude;
            }
            else
            {
                Vector3 toNext = positions[index + 1] - position;
                Vector3 toPrev = position - positions[index - 1];
                float minNeighbor = Mathf.Min(toNext.magnitude, toPrev.magnitude);
                float straightness = toNext.sqrMagnitude < 0.0001f || toPrev.sqrMagnitude < 0.0001f
                    ? 1f
                    : Vector3.Dot(toNext.normalized, -toPrev.normalized);

                if (toNext.sqrMagnitude < 0.0001f || toPrev.sqrMagnitude < 0.0001f)
                {
                    tangent = toNext.sqrMagnitude >= 0.0001f ? toNext : toPrev;
                    directionLength = Mathf.Max(minNeighbor, settings.MinNodeDirection);
                }
                else if (straightness > 0.995f)
                {
                    tangent = toNext;
                    directionLength = minNeighbor;
                }
                else if (straightness < 0.5f)
                {
                    tangent = toNext.normalized + toPrev.normalized;

                    if (tangent.sqrMagnitude < 0.0001f)
                        tangent = toNext;

                    directionLength = minNeighbor * Mathf.Lerp(0.18f, 0.35f, smoothness);
                }
                else
                {
                    tangent = toNext.normalized + toPrev.normalized;

                    if (tangent.sqrMagnitude < 0.0001f)
                        tangent = toNext;

                    directionLength = minNeighbor * Mathf.Lerp(0.3f, 0.55f, straightness * smoothness);
                }

                directionLength = Mathf.Min(directionLength, minNeighbor * Mathf.Lerp(0.35f, 0.5f, smoothness));
            }

            if (tangent.sqrMagnitude < 0.0001f)
                tangent = index < positions.Count - 1
                    ? positions[index + 1] - position
                    : position - positions[index - 1];

            if (tangent.sqrMagnitude < 0.0001f)
                tangent = Vector3.forward;

            directionLength = Mathf.Clamp(
                directionLength * Mathf.Lerp(0.85f, 1f, smoothness),
                settings.MinNodeDirection * 0.25f,
                settings.MinNodeDirection * 4f);

            return EnsureValidDirection(position, position + tangent.normalized * directionLength, settings.MinNodeDirection);
        }

        private static Vector3 EnsureValidDirection(Vector3 position, Vector3 direction, float minSeparation)
        {
            if (IsFinite(position) == false || IsFinite(direction) == false)
                return position + Vector3.forward * minSeparation;

            float minSqrDistance = minSeparation * minSeparation;

            if ((direction - position).sqrMagnitude >= minSqrDistance)
                return direction;

            return position + Vector3.forward * minSeparation;
        }

        private static bool IsFinite(Vector3 vector)
        {
            return float.IsFinite(vector.x)
                && float.IsFinite(vector.y)
                && float.IsFinite(vector.z);
        }

        private static float Distance(Vector3 left, Vector3 right)
        {
            return Vector3.Distance(
                new Vector3(left.x, 0f, left.z),
                new Vector3(right.x, 0f, right.z));
        }
    }
}
