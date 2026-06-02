using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Scripts.Environment.Grid;
using Game.Scripts.Environment.Grid.Services;
using Game.Scripts.Environment.Grid.Spawner;
using SplineMesh;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Entities.Animals.Movement
{
    public class Mover : MonoBehaviour
    {
        private const float MinDistanceThreshold = 0.01f;
        private const float MinTangentMagnitude = 0.001f;

        [SerializeField] private float _moveSpeed = 0.3f;
        [SerializeField] private float _perimeterMoveSpeed = 0.5f;
        [SerializeField] private float _rotationSpeed = 180f;

        private Animal _animal;
        private bool _isMoving = false;
        private bool _reachedEdge;
        private Coroutine _moveRoutine;

        private GridService _gridService;
        private WayPointsSpawner _wayPointsSpawner;

        [Inject]
        private void Construct(GridService gridService, WayPointsSpawner wayPointsSpawner)
        {
            _gridService = gridService;
            _wayPointsSpawner = wayPointsSpawner;
        }

        private void Awake()
        {
            _animal = GetComponent<Animal>();
        }

        private void OnDisable()
        {
            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
                _moveRoutine = null;
            }

            _animal.transform.DOKill();
            _isMoving = false;
        }

        public void StartMoving()
        {
            if (_isMoving)
                return;

            _moveRoutine = StartCoroutine(MoveRoutine());
        }

        private IEnumerator MoveRoutine()
        {
            _isMoving = true;

            yield return StartCoroutine(MoveOnGrid());

            if (_reachedEdge)
                yield return StartCoroutine(MoveOnPerimeter());

            _isMoving = false;
            _moveRoutine = null;
        }

        private IEnumerator MoveOnGrid()
        {
            _reachedEdge = false;

            while (enabled)
            {
                Vector3 direction = GetDirection();
                List<Cell> nextCells = GetNextCells(direction);

                if (nextCells == null)
                {
                    _reachedEdge = true;
                    break;
                }

                if (AreCellsFree(nextCells) == false)
                {
                    break;
                }

                List<Cell> finalCells = GetFinalCells(nextCells, direction);
                UpdateCells(nextCells, finalCells);

                Vector3 targetPosition = GetTargetPosition(direction);

                yield return _animal.transform
                    .DOMove(targetPosition, _moveSpeed)
                    .SetEase(Ease.Linear)
                    .WaitForCompletion();
            }
        }

        private IEnumerator MoveOnPerimeter()
        {
            _animal.FreeAllCells();

            Spline spline = _wayPointsSpawner.Spline;

            if (spline == null || spline.nodes.Count < 2)
            {
                yield break;
            }

            Vector3 entryDir = GetDirection();
            Vector3 entryPosition = _wayPointsSpawner.GetPerimeterEntryPoint(_animal.transform.position, entryDir);

            yield return _animal.transform
                .DOMove(entryPosition, _perimeterMoveSpeed)
                .SetEase(Ease.Linear)
                .WaitForCompletion();

            CurveSample entrySample = spline.GetProjectionSample(entryPosition);
            float entryDist = _wayPointsSpawner.GetTotalDistance(entrySample);

            Vector3 housePosition = _wayPointsSpawner.ExitPoint.position;
            housePosition.y = _animal.transform.position.y;
            CurveSample houseSample = spline.GetProjectionSample(housePosition);
            float houseDist = _wayPointsSpawner.GetTotalDistance(houseSample);

            float forwardDist = houseDist >= entryDist
                ? houseDist - entryDist
                : spline.Length - entryDist + houseDist;

            float backwardDist = spline.Length - forwardDist;
            bool goForward = forwardDist <= backwardDist;
            float totalDist = Mathf.Min(forwardDist, backwardDist);

            if (totalDist < MinDistanceThreshold)
            {
                yield break;
            }

            float currentDist = entryDist;
            float traveled = 0f;
            float speed = 1f / _perimeterMoveSpeed;

            while (traveled < totalDist)
            {
                float moveStep = speed * Time.deltaTime;

                if (traveled + moveStep > totalDist)
                {
                    moveStep = totalDist - traveled;
                }

                traveled += moveStep;
                currentDist += goForward ? moveStep : -moveStep;

                if (currentDist > spline.Length)
                {
                    currentDist -= spline.Length;
                }
                else if (currentDist < 0)
                {
                    currentDist += spline.Length;
                }

                CurveSample sample = spline.GetSampleAtDistance(Mathf.Clamp(currentDist, 0f, spline.Length));
                _animal.transform.position = new Vector3(
                    sample.location.x,
                    _animal.transform.position.y,
                    sample.location.z);

                Vector3 tangent = new Vector3(sample.tangent.x, 0f, sample.tangent.z).normalized;

                if (goForward == false)
                {
                    tangent = -tangent;
                }

                if (tangent.sqrMagnitude > MinTangentMagnitude)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(tangent) * Quaternion.Euler(_animal.Data.BaseRotation);
                    _animal.transform.rotation = Quaternion.RotateTowards(_animal.transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
                }

                yield return null;
            }
        }

        private Vector3 GetDirection()
        {
            return new Vector3(_animal.transform.up.x, 0f, _animal.transform.up.z).normalized;
        }

        private List<Cell> GetNextCells(Vector3 direction)
        {
            List<Cell> nextCells = new List<Cell>();

            foreach (Cell cell in _animal.OccupiedCells)
            {
                Cell neighbor = _gridService.GetNeighborCell(cell, direction);

                if (neighbor == null)
                {
                    return null;
                }

                if (nextCells.Contains(neighbor) == false && _animal.OccupiedCells.Contains(neighbor) == false)
                {
                    nextCells.Add(neighbor);
                }
            }

            return nextCells;
        }

        private bool AreCellsFree(List<Cell> cells)
        {
            foreach (Cell cell in cells)
                if (cell.IsOccupied)
                    return false;

            return true;
        }

        private Vector3 GetTargetPosition(Vector3 direction)
        {
            return new Vector3(
                _animal.transform.position.x + direction.x * _gridService.CellSize,
                _animal.transform.position.y,
                _animal.transform.position.z + direction.z * _gridService.CellSize
            );
        }

        private void UpdateCells(List<Cell> nextCells, List<Cell> finalCells)
        {
            List<Cell> cellsToFree = new List<Cell>();

            foreach (Cell cell in _animal.OccupiedCells)
            {
                if (finalCells.Contains(cell) == false)
                {
                    cellsToFree.Add(cell);
                }
            }

            foreach (Cell cell in cellsToFree)
            {
                cell.Free();
                _animal.FreeCell(cell);
            }

            foreach (Cell cell in nextCells)
            {
                cell.Occupy(_animal);
                _animal.OccupyCell(cell);
            }
        }

        private List<Cell> GetFinalCells(List<Cell> nextCells, Vector3 direction)
        {
            Vector3 oppositeDirection = -direction;

            List<Cell> result = new List<Cell>(_animal.OccupiedCells);
            List<Cell> toRemove = new List<Cell>();

            foreach (Cell cell in result)
            {
                Cell behindNeighbor = _gridService.GetNeighborCell(cell, oppositeDirection);

                if (behindNeighbor == null || _animal.OccupiedCells.Contains(behindNeighbor) == false)
                {
                    toRemove.Add(cell);
                }
            }

            for (int i = 0; i < nextCells.Count && i < toRemove.Count; i++)
            {
                result.Remove(toRemove[i]);
            }

            foreach (Cell cell in nextCells)
            {
                result.Add(cell);
            }

            return result;
        }
    }
}