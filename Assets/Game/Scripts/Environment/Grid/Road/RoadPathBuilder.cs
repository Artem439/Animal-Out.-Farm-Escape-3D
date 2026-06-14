using System.Collections.Generic;
using Game.Scripts.Environment.Grid;
using Game.Scripts.Environment.Grid.Spawner;
using SplineMesh;
using UnityEngine;

namespace Game.Scripts.Environment.Grid.Road
{
    public static class RoadPathBuilder
    {
        public static List<Vector3> Build(
            PerimeterWayPointsSpawner wayPointsSpawner,
            Transform exitPoint,
            float originY,
            RoadPathSettings settings)
        {
            float roadY = originY + settings.RoadOffsetY;
            List<Vector3> rawPath = BuildRawPath(wayPointsSpawner, exitPoint, roadY, settings);
            return RoundPathCorners(rawPath, settings);
        }

        public static void AddSplineNodes(Spline spline, List<Vector3> positions, RoadPathSettings settings)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 position = positions[i];
                Vector3 direction = ComputeNodeDirection(positions, i, settings);
                spline.AddNode(new SplineNode(position, direction));
            }
        }

        private static List<Vector3> BuildRawPath(
            PerimeterWayPointsSpawner wayPointsSpawner,
            Transform exitPoint,
            float roadY,
            RoadPathSettings settings)
        {
            List<WayPoint> ring = new List<WayPoint>();
            wayPointsSpawner.TraversePerimeter(ring);

            int cols = wayPointsSpawner.Points.GetLength(1);
            int leftCol = cols / 2 - 1;
            int rightCol = cols / 2;

            WayPoint leftWP = wayPointsSpawner.Points[0, leftCol];
            WayPoint rightWP = wayPointsSpawner.Points[0, rightCol];

            float houseHalfWidth = Mathf.Abs(rightWP.transform.position.x - leftWP.transform.position.x) * 0.5f;
            float houseCenterX = exitPoint.position.x;
            float houseZ = exitPoint.position.z + settings.HouseSplineZOffset;
            Vector3 leftHouse = new Vector3(houseCenterX - houseHalfWidth, roadY, houseZ);
            Vector3 rightHouse = new Vector3(houseCenterX + houseHalfWidth, roadY, houseZ);

            FindTopEntryWaypoints(wayPointsSpawner.Points, houseCenterX, out WayPoint leftEntryWP, out WayPoint rightEntryWP);
            int rightRingIndex = ring.IndexOf(rightEntryWP);
            int leftRingIndex = ring.IndexOf(leftEntryWP);

            if (rightRingIndex < 0)
                rightRingIndex = ring.IndexOf(rightWP);

            if (leftRingIndex < 0)
                leftRingIndex = ring.IndexOf(leftWP);

            Vector3 leftEntry = ToRoadPosition(leftEntryWP.transform.position, roadY);
            Vector3 rightEntry = ToRoadPosition(rightEntryWP.transform.position, roadY);

            List<Vector3> positions = new List<Vector3>
            {
                leftHouse,
                rightHouse,
                Vector3.Lerp(rightHouse, rightEntry, settings.HouseBranchBlend),
                rightEntry
            };

            AddPerimeterArc(positions, ring, rightRingIndex, leftRingIndex, roadY);

            positions.Add(leftEntry);
            positions.Add(Vector3.Lerp(leftEntry, leftHouse, settings.HouseBranchBlend));
            positions.Add(leftHouse);

            return positions;
        }

        private static void FindTopEntryWaypoints(
            WayPoint[,] points,
            float targetX,
            out WayPoint leftEntry,
            out WayPoint rightEntry)
        {
            int cols = points.GetLength(1);
            int leftCol = 0;
            int rightCol = cols - 1;

            for (int col = 0; col < cols; col++)
            {
                WayPoint point = points[0, col];

                if (point == null)
                    continue;

                if (point.transform.position.x <= targetX)
                    leftCol = col;

                if (point.transform.position.x >= targetX)
                {
                    rightCol = col;
                    break;
                }
            }

            leftEntry = points[0, leftCol];
            rightEntry = points[0, rightCol];

            if (leftEntry == null)
                leftEntry = points[0, Mathf.Max(0, rightCol - 1)];

            if (rightEntry == null)
                rightEntry = points[0, Mathf.Min(cols - 1, leftCol + 1)];

            if (leftEntry == rightEntry && leftCol > 0)
                leftEntry = points[0, leftCol - 1];
        }

        private static void AddPerimeterArc(
            List<Vector3> positions,
            List<WayPoint> ring,
            int startIndex,
            int endIndex,
            float roadY)
        {
            int count = ring.Count;
            int current = (startIndex + 1) % count;

            while (current != endIndex)
            {
                positions.Add(ToRoadPosition(ring[current].transform.position, roadY));
                current = (current + 1) % count;
            }
        }

        private static Vector3 ToRoadPosition(Vector3 source, float roadY)
        {
            return new Vector3(source.x, roadY, source.z);
        }

        private static List<Vector3> RoundPathCorners(List<Vector3> path, RoadPathSettings settings)
        {
            if (path.Count < 3)
                return path;

            List<Vector3> rounded = new List<Vector3> { path[0] };

            for (int i = 1; i < path.Count - 1; i++)
            {
                Vector3 previous = rounded[rounded.Count - 1];
                AddCornerFillet(rounded, previous, path[i], path[i + 1], settings);
            }

            rounded.Add(path[path.Count - 1]);
            return rounded;
        }

        private static void AddCornerFillet(
            List<Vector3> result,
            Vector3 previous,
            Vector3 corner,
            Vector3 next,
            RoadPathSettings settings)
        {
            Vector3 incoming = corner - previous;
            Vector3 outgoing = next - corner;
            float incomingLength = incoming.magnitude;
            float outgoingLength = outgoing.magnitude;

            if (incomingLength < 0.001f || outgoingLength < 0.001f)
            {
                result.Add(corner);
                return;
            }

            incoming /= incomingLength;
            outgoing /= outgoingLength;

            if (Vector3.Dot(incoming, outgoing) > 0.99f)
            {
                result.Add(corner);
                return;
            }

            float angle = Vector3.Angle(-incoming, outgoing);

            if (angle < settings.MinCornerAngle)
            {
                result.Add(corner);
                return;
            }

            float maxFillet = Mathf.Min(
                settings.CornerFilletRadius,
                settings.RoadHalfWidth * 0.95f,
                incomingLength * settings.CornerFilletMaxSegmentRatio,
                outgoingLength * settings.CornerFilletMaxSegmentRatio);

            if (maxFillet < 0.01f)
            {
                result.Add(corner);
                return;
            }

            float halfAngleRad = angle * 0.5f * Mathf.Deg2Rad;
            float sinHalf = Mathf.Sin(halfAngleRad);

            if (sinHalf < 0.001f)
            {
                result.Add(corner);
                return;
            }

            Vector3 bisector = (-incoming + outgoing).normalized;
            Vector3 arcCenter = corner + bisector * (maxFillet / sinHalf);
            Vector3 startArc = corner - incoming * maxFillet;
            Vector3 endArc = corner + outgoing * maxFillet;

            result.Add(startArc);

            float startAngle = Mathf.Atan2(startArc.z - arcCenter.z, startArc.x - arcCenter.x);
            float sweep = Mathf.DeltaAngle(
                startAngle * Mathf.Rad2Deg,
                Mathf.Atan2(endArc.z - arcCenter.z, endArc.x - arcCenter.x) * Mathf.Rad2Deg) * Mathf.Deg2Rad;

            for (int segment = 1; segment < settings.CornerArcSegments; segment++)
            {
                float t = segment / (float)settings.CornerArcSegments;
                float arcAngle = startAngle + sweep * t;

                result.Add(new Vector3(
                    arcCenter.x + Mathf.Cos(arcAngle) * maxFillet,
                    Mathf.Lerp(startArc.y, endArc.y, t),
                    arcCenter.z + Mathf.Sin(arcAngle) * maxFillet));
            }

            result.Add(endArc);
        }

        private static Vector3 ComputeNodeDirection(List<Vector3> positions, int index, RoadPathSettings settings)
        {
            Vector3 position = positions[index];
            Vector3 tangent = Vector3.zero;
            float averageDistance = 0f;
            int neighborCount = 0;

            if (index > 0)
            {
                Vector3 toPrevious = position - positions[index - 1];
                averageDistance += toPrevious.magnitude;
                tangent += toPrevious.normalized;
                neighborCount++;
            }

            if (index < positions.Count - 1)
            {
                Vector3 toNext = positions[index + 1] - position;
                averageDistance += toNext.magnitude;
                tangent += toNext.normalized;
                neighborCount++;
            }

            if (tangent.sqrMagnitude < 0.0001f)
                tangent = Vector3.forward;

            float directionLength = neighborCount > 0
                ? Mathf.Max(averageDistance / neighborCount * settings.SplineSmoothCurvature, settings.MinNodeDirection)
                : settings.MinNodeDirection;

            return position + tangent.normalized * directionLength;
        }
    }
}
