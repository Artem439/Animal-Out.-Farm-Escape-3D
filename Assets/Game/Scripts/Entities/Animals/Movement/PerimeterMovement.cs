using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Scripts.Environment.Grid.Configuration;
using Game.Scripts.Environment.Grid.Spawner;
using SplineMesh;
using UnityEngine;

namespace Game.Scripts.Entities.Animals.Movement
{
    public class PerimeterMovement
    {
        private const float MinDistanceThreshold = 0.01f;
        private const float MinTangentMagnitude = 0.001f;
        private const float ForwardSearchStep = 0.25f;
        private const float LookAheadProgressOffset = 0.04f;
        private const float MinForwardAmount = 0.35f;
        private const float MaxLateralDistance = 2.5f;
        private const float LateralScorePenalty = 0.35f;

        private readonly struct RoutePlan
        {
            public RoutePlan(
                Spline approachSpline,
                bool goForward,
                int startNodeIndex,
                int endNodeIndex,
                Spline secondLegSpline,
                int secondLegEndNodeIndex,
                Spline houseSpline,
                int houseEndNodeIndex)
            {
                ApproachSpline = approachSpline;
                GoForward = goForward;
                StartNodeIndex = startNodeIndex;
                EndNodeIndex = endNodeIndex;
                SecondLegSpline = secondLegSpline;
                SecondLegEndNodeIndex = secondLegEndNodeIndex;
                HouseSpline = houseSpline;
                HouseEndNodeIndex = houseEndNodeIndex;
            }

            public Spline ApproachSpline { get; }
            public bool GoForward { get; }
            public int StartNodeIndex { get; }
            public int EndNodeIndex { get; }
            public Spline SecondLegSpline { get; }
            public int SecondLegEndNodeIndex { get; }
            public Spline HouseSpline { get; }
            public int HouseEndNodeIndex { get; }
        }

        public IEnumerator Move(
            Animal animal,
            FieldLayout fieldLayout,
            PerimeterRoadBuilder roadBuilder,
            float moveSpeed,
            float rotationSpeed)
        {
            animal.FreeAllCells();
            animal.transform.DOKill();

            Spline perimeterSpline = roadBuilder.PerimeterSpline;

            if (perimeterSpline == null || perimeterSpline.nodes.Count < 2)
                yield break;

            float cellSize = fieldLayout.CellSize;
            float searchDistance = GetSearchDistance(fieldLayout);
            Vector3 approachDirection = GetDirection(animal);
            float y = animal.transform.position.y;

            Vector3 edgePosition = fieldLayout.GetPerimeterEntryPoint(animal.transform.position, approachDirection);
            edgePosition.y = y;

            yield return TweenTo(
                animal,
                edgePosition,
                approachDirection,
                cellSize,
                moveSpeed);

            Vector3 roadPointAhead = FindForwardRoadPoint(
                perimeterSpline,
                animal.transform.position,
                approachDirection,
                searchDistance);
            roadPointAhead.y = y;

            Vector3 roadEntry = ComputeAxisAlignedTarget(
                animal.transform.position,
                approachDirection,
                roadPointAhead);

            yield return TweenTo(
                animal,
                roadEntry,
                approachDirection,
                cellSize,
                moveSpeed);

            RoutePlan route = SelectRoute(roadBuilder, perimeterSpline, animal.transform.position);

            if (route.ApproachSpline == null)
                yield break;

            List<Vector3> path = BuildWorldPath(animal.transform.position, y, route);

            if (path.Count < 2)
                yield break;

            path[0] = new Vector3(animal.transform.position.x, y, animal.transform.position.z);

            float pathLength = GetPathLength(path);
            float duration = GetDuration(pathLength, cellSize, moveSpeed);

            if (duration <= MinDistanceThreshold)
                yield break;

            Quaternion baseRotation = Quaternion.Euler(animal.Data.BaseRotation);
            float progress = 0f;

            Tween pathTween = DOTween.To(
                () => progress,
                value =>
                {
                    progress = value;
                    Vector3 position = GetPointOnPath(path, progress, out _);
                    Vector3 lookAhead = GetPointOnPath(path, Mathf.Min(1f, progress + LookAheadProgressOffset), out _);
                    animal.transform.position = position;
                    Vector3 forward = Flatten(lookAhead - position);

                    if (forward.sqrMagnitude <= MinTangentMagnitude)
                        return;

                    animal.transform.rotation = Quaternion.LookRotation(forward.normalized) * baseRotation;
                },
                1f,
                duration)
                .SetEase(Ease.Linear)
                .SetTarget(animal.transform);

            yield return pathTween.WaitForCompletion();
        }

        private static float GetSearchDistance(FieldLayout fieldLayout)
        {
            return (fieldLayout.Width + fieldLayout.PerimeterBorderOffset) * fieldLayout.CellSize;
        }

        private static Vector3 FindForwardRoadPoint(
            Spline spline,
            Vector3 origin,
            Vector3 forward,
            float searchDistance)
        {
            forward = Flatten(forward).normalized;
            Vector3 bestPoint = ToWorld(spline, ProjectOnSpline(spline, origin).location);
            float bestScore = float.MinValue;

            for (float distance = ForwardSearchStep; distance <= searchDistance; distance += ForwardSearchStep)
            {
                Vector3 probe = origin + forward * distance;
                CurveSample sample = ProjectOnSpline(spline, probe);
                Vector3 splineWorld = ToWorld(spline, sample.location);
                Vector3 toSpline = Flatten(splineWorld - origin);
                float forwardAmount = Vector3.Dot(toSpline.normalized, forward);
                float lateral = FlattenDistance(probe, splineWorld);

                if (forwardAmount < MinForwardAmount || lateral > MaxLateralDistance)
                    continue;

                float score = forwardAmount - lateral * LateralScorePenalty;

                if (score <= bestScore)
                    continue;

                bestScore = score;
                bestPoint = splineWorld;
            }

            return bestPoint;
        }

        private static Vector3 ComputeAxisAlignedTarget(Vector3 origin, Vector3 forward, Vector3 target)
        {
            forward = Flatten(forward).normalized;
            float y = origin.y;

            if (Mathf.Abs(forward.z) >= Mathf.Abs(forward.x))
                return new Vector3(origin.x, y, target.z);

            return new Vector3(target.x, y, origin.z);
        }

        private static List<Vector3> BuildWorldPath(Vector3 startPosition, float y, RoutePlan route)
        {
            List<Vector3> path = new List<Vector3>(64);
            AddPoint(path, startPosition, y);

            AddNodePolyline(
                path,
                route.ApproachSpline,
                route.StartNodeIndex,
                route.EndNodeIndex,
                route.GoForward,
                y);

            if (route.SecondLegSpline != null)
            {
                AddNodePolyline(
                    path,
                    route.SecondLegSpline,
                    0,
                    route.SecondLegEndNodeIndex,
                    true,
                    y);
            }
            else if (route.HouseSpline != null && route.HouseEndNodeIndex > 0)
            {
                AddNodePolyline(
                    path,
                    route.HouseSpline,
                    0,
                    route.HouseEndNodeIndex,
                    true,
                    y);
            }

            return path;
        }

        private static void AddNodePolyline(
            List<Vector3> path,
            Spline spline,
            int startNodeIndex,
            int endNodeIndex,
            bool goForward,
            float y)
        {
            if (spline == null || spline.nodes.Count < 2)
                return;

            startNodeIndex = Mathf.Clamp(startNodeIndex, 0, spline.nodes.Count - 1);
            endNodeIndex = Mathf.Clamp(endNodeIndex, 0, spline.nodes.Count - 1);

            if (goForward)
            {
                if (startNodeIndex <= endNodeIndex)
                {
                    for (int index = startNodeIndex; index <= endNodeIndex; index++)
                    {
                        if (index != startNodeIndex || path.Count == 0)
                            AddPoint(path, ToWorld(spline, spline.nodes[index].Position), y);
                    }

                    return;
                }

                for (int index = startNodeIndex; index >= endNodeIndex; index--)
                {
                    if (index != startNodeIndex || path.Count == 0)
                        AddPoint(path, ToWorld(spline, spline.nodes[index].Position), y);
                }

                return;
            }

            for (int index = startNodeIndex; index >= endNodeIndex; index--)
            {
                if (index != startNodeIndex || path.Count == 0)
                    AddPoint(path, ToWorld(spline, spline.nodes[index].Position), y);
            }
        }

        private static void AddPoint(List<Vector3> path, Vector3 point, float y)
        {
            point.y = y;

            if (path.Count > 0 && FlattenDistance(path[path.Count - 1], point) < MinDistanceThreshold)
                return;

            path.Add(point);
        }

        private static RoutePlan SelectRoute(
            PerimeterRoadBuilder roadBuilder,
            Spline perimeterSpline,
            Vector3 worldPosition)
        {
            int entryNode = FindNearestNodeIndex(perimeterSpline, worldPosition);

            if (roadBuilder.RightBranchSpline != null
                && roadBuilder.LeftBranchSpline != null
                && roadBuilder.RightBranchAnchorNodeIndex >= 0
                && roadBuilder.LeftBranchAnchorNodeIndex >= 0)
            {
                Vector3 rightAnchorPosition = ToWorld(
                    perimeterSpline,
                    perimeterSpline.nodes[roadBuilder.RightBranchAnchorNodeIndex].Position);
                Vector3 leftAnchorPosition = ToWorld(
                    perimeterSpline,
                    perimeterSpline.nodes[roadBuilder.LeftBranchAnchorNodeIndex].Position);

                bool useRightBranch = FlattenDistance(worldPosition, rightAnchorPosition)
                    <= FlattenDistance(worldPosition, leftAnchorPosition);
                int anchorNode = useRightBranch
                    ? roadBuilder.RightBranchAnchorNodeIndex
                    : roadBuilder.LeftBranchAnchorNodeIndex;
                Spline branchSpline = useRightBranch
                    ? roadBuilder.RightBranchSpline
                    : roadBuilder.LeftBranchSpline;
                bool goForward = anchorNode >= entryNode;

                return new RoutePlan(
                    perimeterSpline,
                    goForward,
                    entryNode,
                    anchorNode,
                    branchSpline,
                    branchSpline.nodes.Count - 1,
                    null,
                    0);
            }

            Spline houseSpline = roadBuilder.HouseSpline;
            int houseEndNode = houseSpline != null && houseSpline.nodes.Count > 1
                ? houseSpline.nodes.Count - 1
                : 0;

            if (roadBuilder.Junction == Vector3.zero)
            {
                return new RoutePlan(
                    perimeterSpline,
                    true,
                    entryNode,
                    perimeterSpline.nodes.Count - 1,
                    null,
                    0,
                    houseSpline,
                    houseEndNode);
            }

            int junctionNode = FindNearestNodeIndex(perimeterSpline, roadBuilder.Junction);
            bool forwardToJunction = junctionNode >= entryNode;

            return new RoutePlan(
                perimeterSpline,
                forwardToJunction,
                entryNode,
                junctionNode,
                null,
                0,
                roadBuilder.HouseSpline,
                houseEndNode);
        }

        private static int FindNearestNodeIndex(Spline spline, Vector3 worldPosition)
        {
            int bestIndex = 0;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < spline.nodes.Count; i++)
            {
                Vector3 nodeWorld = ToWorld(spline, spline.nodes[i].Position);
                float distance = FlattenDistance(worldPosition, nodeWorld);

                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                bestIndex = i;
            }

            return bestIndex;
        }

        private static Vector3 GetPointOnPath(List<Vector3> path, float progress, out Vector3 forward)
        {
            progress = Mathf.Clamp01(progress);
            forward = Vector3.forward;

            if (path.Count < 2)
                return path.Count == 1 ? path[0] : Vector3.zero;

            float totalLength = GetPathLength(path);
            float targetDistance = totalLength * progress;
            float walked = 0f;

            for (int i = 1; i < path.Count; i++)
            {
                Vector3 segmentStart = path[i - 1];
                Vector3 segmentEnd = path[i];
                float segmentLength = FlattenDistance(segmentStart, segmentEnd);

                if (segmentLength <= MinDistanceThreshold)
                    continue;

                if (walked + segmentLength >= targetDistance)
                {
                    float segmentProgress = (targetDistance - walked) / segmentLength;
                    forward = segmentEnd - segmentStart;
                    return Vector3.Lerp(segmentStart, segmentEnd, segmentProgress);
                }

                walked += segmentLength;
            }

            forward = path[path.Count - 1] - path[path.Count - 2];
            return path[path.Count - 1];
        }

        private static float GetPathLength(List<Vector3> path)
        {
            float length = 0f;

            for (int i = 1; i < path.Count; i++)
                length += FlattenDistance(path[i - 1], path[i]);

            return length;
        }

        private static float GetDuration(float distance, float cellSize, float moveSpeed)
        {
            if (distance <= MinDistanceThreshold || moveSpeed <= 0f)
                return 0f;

            float cells = distance / Mathf.Max(cellSize, MinDistanceThreshold);

            return cells / moveSpeed;
        }

        private static IEnumerator TweenTo(
            Animal animal,
            Vector3 targetPosition,
            Vector3 approachDirection,
            float cellSize,
            float moveSpeed)
        {
            float distance = FlattenDistance(animal.transform.position, targetPosition);

            if (distance < MinDistanceThreshold)
                yield break;

            float duration = GetDuration(distance, cellSize, moveSpeed);
            Quaternion baseRotation = Quaternion.Euler(animal.Data.BaseRotation);
            Vector3 flatApproach = Flatten(approachDirection);
            Quaternion targetRotation = Quaternion.LookRotation(flatApproach) * baseRotation;

            Sequence sequence = DOTween.Sequence();
            sequence.Join(animal.transform.DOMove(targetPosition, duration).SetEase(Ease.Linear));
            sequence.Join(animal.transform.DORotateQuaternion(targetRotation, duration).SetEase(Ease.Linear));

            yield return sequence.WaitForCompletion();
        }

        private static CurveSample ProjectOnSpline(Spline spline, Vector3 worldPosition)
        {
            return spline.GetProjectionSample(spline.transform.InverseTransformPoint(worldPosition));
        }

        private static Vector3 ToWorld(Spline spline, Vector3 localPosition)
        {
            return spline.transform.TransformPoint(localPosition);
        }

        private static Vector3 GetDirection(Animal animal)
        {
            return Flatten(animal.transform.forward).normalized;
        }

        private static Vector3 Flatten(Vector3 vector)
        {
            return new Vector3(vector.x, 0f, vector.z);
        }

        private static float FlattenDistance(Vector3 left, Vector3 right)
        {
            float deltaX = left.x - right.x;
            float deltaZ = left.z - right.z;
            return Mathf.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
        }
    }
}
