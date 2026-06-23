using System.Collections.Generic;
using Game.Scripts.Environment.Grid;
using Game.Scripts.Environment.Grid.Spawner;
using Game.Scripts.Resources.Entities;
using Game.Scripts.Resources.Environment;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.Entities.Animals.Spawn
{
    public class Spawner : MonoBehaviour
    {
        private const float SpawnOffsetY = 0.01f;
        private const int RotationStepCount = 4;
        private const int RotationStepDivisor = 2;
        private const float RotationAnglePerStep = 90f;

        [SerializeField] private Pool _pool;
        [SerializeField] private CellsSpawner _cellsSpawner;
        [SerializeField, Min(0f)] private float _centerBiasStrength = 4f;

        [Inject] private IObjectResolver _objectResolver;

        private Cell[,] _cells;

        private readonly List<PlacementCandidate> _candidates = new();

        public void Build(IReadOnlyList<AnimalSpawnEntry> spawnEntries)
        {
            _cells = _cellsSpawner.Cells;

            foreach (AnimalSpawnEntry entry in spawnEntries)
            {
                if (entry.Data == null || entry.Count <= 0)
                    continue;

                for (int i = 0; i < entry.Count; i++)
                    TrySpawnAnimal(entry.Data);
            }
        }

        private void TrySpawnAnimal(AnimalData data)
        {
            if (data.AnimalPrefab == null)
                return;

            if (TryFindWinnablePlacement(data, out PlacementCandidate placement) == false)
                return;

            Vector3 center = GetBlockCenter(placement.Origin, placement.SizeX, placement.SizeZ);
            center.y += SpawnOffsetY;

            Animal animal = _pool.Get(data.AnimalPrefab);

            if (_objectResolver == null)
            {
                _pool.Release(animal);
                return;
            }

            _objectResolver.InjectGameObject(animal.gameObject);
            animal.Reset(center);
            animal.transform.rotation = Quaternion.Euler(0, placement.RotationStep * RotationAnglePerStep, 0);
            animal.Released += OnReleased;

            OccupyBlock(placement.Origin, placement.SizeX, placement.SizeZ, animal);
        }

        private void OnReleased(Animal animal)
        {
            animal.Released -= OnReleased;
            _pool.Release(animal);
        }

        private bool TryFindWinnablePlacement(AnimalData data, out PlacementCandidate placement)
        {
            int rows = _cells.GetLength(0);
            int cols = _cells.GetLength(1);
            int margin = GetSpawnBorderOffset();

            _candidates.Clear();

            for (int rotationStep = 0; rotationStep < RotationStepCount; rotationStep++)
            {
                int sizeX = data.SizeX;
                int sizeZ = data.SizeZ;

                if (rotationStep % RotationStepDivisor != 0)
                {
                    int temp = sizeX;
                    sizeX = sizeZ;
                    sizeZ = temp;
                }

                if (sizeX <= 0 || sizeZ <= 0)
                    continue;

                for (int row = margin; row <= rows - sizeZ - margin; row++)
                {
                    for (int col = margin; col <= cols - sizeX - margin; col++)
                    {
                        Vector2Int origin = new Vector2Int(row, col);

                        if (IsBlockFree(origin, sizeX, sizeZ) == false)
                            continue;

                        if (IsForwardLaneClear(origin, sizeX, sizeZ, rotationStep) == false)
                            continue;

                        _candidates.Add(new PlacementCandidate(origin, rotationStep, sizeX, sizeZ));
                    }
                }
            }

            if (_candidates.Count == 0)
            {
                placement = default;
                return false;
            }

            placement = PickCenterBiased(_candidates, rows, cols, _centerBiasStrength);
            return true;
        }

        private int GetSpawnBorderOffset()
        {
            FieldConfiguration configuration = _cellsSpawner.FieldLayout.Configuration;
            return configuration != null ? configuration.SpawnBorderOffset : 0;
        }

        private static PlacementCandidate PickCenterBiased(
            List<PlacementCandidate> candidates,
            int rows,
            int cols,
            float biasStrength)
        {
            Vector2 fieldCenter = new Vector2((rows - 1) * 0.5f, (cols - 1) * 0.5f);
            float totalWeight = 0f;
            float[] weights = new float[candidates.Count];

            for (int i = 0; i < candidates.Count; i++)
            {
                PlacementCandidate candidate = candidates[i];
                Vector2 blockCenter = new Vector2(
                    candidate.Origin.x + candidate.SizeZ * 0.5f,
                    candidate.Origin.y + candidate.SizeX * 0.5f);
                float distance = Vector2.Distance(blockCenter, fieldCenter);
                weights[i] = 1f / Mathf.Pow(1f + distance, biasStrength);
                totalWeight += weights[i];
            }

            float roll = Random.value * totalWeight;
            float accumulated = 0f;

            for (int i = 0; i < candidates.Count; i++)
            {
                accumulated += weights[i];
                if (roll <= accumulated)
                    return candidates[i];
            }

            return candidates[candidates.Count - 1];
        }

        private bool IsBlockFree(Vector2Int origin, int sizeX, int sizeZ)
        {
            for (int row = origin.x; row < origin.x + sizeZ; row++)
                for (int col = origin.y; col < origin.y + sizeX; col++)
                    if (_cells[row, col].IsOccupied)
                        return false;

            return true;
        }

        private bool IsForwardLaneClear(Vector2Int origin, int sizeX, int sizeZ, int rotationStep)
        {
            int rows = _cells.GetLength(0);
            int cols = _cells.GetLength(1);

            int firstRow = origin.x;
            int lastRow = origin.x + sizeZ - 1;
            int firstCol = origin.y;
            int lastCol = origin.y + sizeX - 1;

            switch (rotationStep)
            {
                case 0:
                    return IsRangeFree(0, firstRow - 1, firstCol, lastCol);
                case 2:
                    return IsRangeFree(lastRow + 1, rows - 1, firstCol, lastCol);
                case 1:
                    return IsRangeFree(firstRow, lastRow, lastCol + 1, cols - 1);
                default:
                    return IsRangeFree(firstRow, lastRow, 0, firstCol - 1);
            }
        }

        private bool IsRangeFree(int rowFrom, int rowTo, int colFrom, int colTo)
        {
            for (int row = rowFrom; row <= rowTo; row++)
                for (int col = colFrom; col <= colTo; col++)
                    if (_cells[row, col].IsOccupied)
                        return false;

            return true;
        }

        private void OccupyBlock(Vector2Int origin, int sizeX, int sizeZ, Animal animal)
        {
            for (int row = origin.x; row < origin.x + sizeZ; row++)
            {
                for (int col = origin.y; col < origin.y + sizeX; col++)
                {
                    _cells[row, col].Occupy(animal);
                    animal.OccupyCell(_cells[row, col]);
                }
            }
        }

        private Vector3 GetBlockCenter(Vector2Int origin, int sizeX, int sizeZ)
        {
            Vector3 firstCell = _cells[origin.x, origin.y].transform.position;
            Vector3 lastCell = _cells[origin.x + sizeZ - 1, origin.y + sizeX - 1].transform.position;

            return (firstCell + lastCell) / 2f;
        }

        private readonly struct PlacementCandidate
        {
            public PlacementCandidate(Vector2Int origin, int rotationStep, int sizeX, int sizeZ)
            {
                Origin = origin;
                RotationStep = rotationStep;
                SizeX = sizeX;
                SizeZ = sizeZ;
            }

            public Vector2Int Origin { get; }
            public int RotationStep { get; }
            public int SizeX { get; }
            public int SizeZ { get; }
        }
    }
}
