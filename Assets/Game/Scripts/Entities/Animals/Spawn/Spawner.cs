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

        [Inject] private IObjectResolver _objectResolver;

        private Cell[,] _cells;

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

            int rotationStep = Random.Range(0, RotationStepCount);
            int currentSizeX = data.SizeX;
            int currentSizeZ = data.SizeZ;

            if (rotationStep % RotationStepDivisor != 0)
            {
                int temp = currentSizeX;
                currentSizeX = currentSizeZ;
                currentSizeZ = temp;
            }

            Quaternion targetRotation = Quaternion.Euler(0, rotationStep * RotationAnglePerStep, 0);

            if (TryFindFreeBlock(currentSizeX, currentSizeZ, out Vector2Int origin) == false)
                return;

            Vector3 center = GetBlockCenter(origin, currentSizeX, currentSizeZ);
            center.y += SpawnOffsetY;

            Animal animal = _pool.Get(data.AnimalPrefab);

            if (_objectResolver == null)
            {
                _pool.Release(animal);
                return;
            }

            _objectResolver.InjectGameObject(animal.gameObject);
            animal.Reset(center);
            animal.transform.rotation = targetRotation;
            animal.Released += OnReleased;

            OccupyBlock(origin, currentSizeX, currentSizeZ, animal);
        }

        private void OnReleased(Animal animal)
        {
            animal.Released -= OnReleased;
            _pool.Release(animal);
        }

        private bool TryFindFreeBlock(int sizeX, int sizeZ, out Vector2Int origin)
        {
            int rows = _cells.GetLength(0);
            int cols = _cells.GetLength(1);
            int margin = GetSpawnBorderOffset();

            List<Vector2Int> candidates = new List<Vector2Int>();

            for (int row = margin; row <= rows - sizeZ - margin; row++)
            {
                for (int col = margin; col <= cols - sizeX - margin; col++)
                {
                    if (IsBlockFree(row, col, sizeX, sizeZ))
                        candidates.Add(new Vector2Int(row, col));
                }
            }

            if (candidates.Count == 0)
            {
                origin = default;
                return false;
            }

            origin = PickCenterBiasedOrigin(candidates, rows, cols);
            return true;
        }

        private int GetSpawnBorderOffset()
        {
            FieldConfiguration configuration = _cellsSpawner.FieldLayout.Configuration;
            return configuration != null ? configuration.SpawnBorderOffset : 0;
        }

        private static Vector2Int PickCenterBiasedOrigin(List<Vector2Int> candidates, int rows, int cols)
        {
            Vector2 fieldCenter = new Vector2((rows - 1) * 0.5f, (cols - 1) * 0.5f);
            float totalWeight = 0f;
            float[] weights = new float[candidates.Count];

            for (int i = 0; i < candidates.Count; i++)
            {
                Vector2Int candidate = candidates[i];
                Vector2 blockCenter = new Vector2(
                    candidate.x + 0.5f,
                    candidate.y + 0.5f);
                float distance = Vector2.Distance(blockCenter, fieldCenter);
                weights[i] = 1f / (1f + distance);
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

        private bool IsBlockFree(int startRow, int startCol, int sizeX, int sizeZ)
        {
            for (int row = startRow; row < startRow + sizeZ; row++)
                for (int col = startCol; col < startCol + sizeX; col++)
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
    }
}
