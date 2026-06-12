using System.Collections;
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

        public IEnumerator Move(
            Animal animal,
            FieldLayout fieldLayout,
            PerimeterRoadBuilder roadBuilder,
            float moveSpeed,
            float rotationSpeed)
        {
            animal.FreeAllCells();

            Spline spline = roadBuilder.Spline;

            if (spline == null || spline.nodes.Count < 2)
                yield break;

            Vector3 entryDir = GetDirection(animal);
            Vector3 entryPosition = fieldLayout.GetPerimeterEntryPoint(animal.transform.position, entryDir);

            yield return animal.transform
                .DOMove(entryPosition, moveSpeed)
                .SetEase(Ease.Linear)
                .WaitForCompletion();

            CurveSample entrySample = spline.GetProjectionSample(entryPosition);
            float entryDist = roadBuilder.GetTotalDistance(entrySample);

            Vector3 housePosition = roadBuilder.ExitPoint.position;
            housePosition.y = animal.transform.position.y;
            CurveSample houseSample = spline.GetProjectionSample(housePosition);
            float houseDist = roadBuilder.GetTotalDistance(houseSample);

            float forwardDist = houseDist >= entryDist
                ? houseDist - entryDist
                : spline.Length - entryDist + houseDist;

            float backwardDist = spline.Length - forwardDist;
            bool goForward = forwardDist <= backwardDist;
            float totalDist = Mathf.Min(forwardDist, backwardDist);

            if (totalDist < MinDistanceThreshold)
                yield break;

            float currentDist = entryDist;
            float traveled = 0f;
            float speed = 1f / moveSpeed;

            while (traveled < totalDist)
            {
                float moveStep = speed * Time.deltaTime;

                if (traveled + moveStep > totalDist)
                    moveStep = totalDist - traveled;

                traveled += moveStep;
                currentDist += goForward ? moveStep : -moveStep;

                if (currentDist > spline.Length)
                    currentDist -= spline.Length;
                else if (currentDist < 0)
                    currentDist += spline.Length;

                CurveSample sample = spline.GetSampleAtDistance(Mathf.Clamp(currentDist, 0f, spline.Length));
                animal.transform.position = new Vector3(
                    sample.location.x,
                    animal.transform.position.y,
                    sample.location.z);

                Vector3 tangent = new Vector3(sample.tangent.x, 0f, sample.tangent.z).normalized;

                if (goForward == false)
                    tangent = -tangent;

                if (tangent.sqrMagnitude > MinTangentMagnitude)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(tangent) * Quaternion.Euler(animal.Data.BaseRotation);
                    animal.transform.rotation = Quaternion.RotateTowards(
                        animal.transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime);
                }

                yield return null;
            }
        }

        private static Vector3 GetDirection(Animal animal)
        {
            return new Vector3(animal.transform.forward.x, 0f, animal.transform.forward.z).normalized;
        }
    }
}
