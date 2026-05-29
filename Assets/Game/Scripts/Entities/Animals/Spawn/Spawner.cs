using System.Collections.Generic;
using Game.Scripts.Core.Spawn;
using Game.Scripts.Environment.Grid;
using Game.Scripts.Environment.Grid.Spawner;
using Game.Scripts.Resources.Entities;
using UnityEngine;

namespace Game.Scripts.Entities.Animals.Spawn
{
    public class Spawner : Spawner<Animal>
    {
        //TODO
        //Попробывать отрефакторить класс
        private const float SpawnOffsetY = 0.5f;
        
        [SerializeField] private CellsSpawner _cellsSpawner;
        [SerializeField] private List<AnimalData> _animalsToSpawn;
        [SerializeField] private int _countPerType = 1;

        private Cell[,] _cells;
        
        private void OnEnable()
        {
            _cellsSpawner.OnSpawned += Spawn;
        }

        private void OnDisable()
        {
            _cellsSpawner.OnSpawned -= Spawn;
        }

        protected override void Spawn()
        {
            _cells = _cellsSpawner.Cells;

            foreach (AnimalData data in _animalsToSpawn)
            {
                for (int i = 0; i < _countPerType; i++)
                {
                    int rotationStep = Random.Range(0, 4);

                    int currentSizeX = data.SizeX;
                    int currentSizeZ = data.SizeZ;
                    
                    Quaternion targetRotation = Quaternion.Euler(data.BaseRotation);

                    if (rotationStep % 2 != 0)
                    {
                        int temp = currentSizeX;
                        currentSizeX = currentSizeZ;
                        currentSizeZ = temp;
                    }

                    targetRotation *= Quaternion.Euler(0, 0, rotationStep * 90f);

                    if (TryFindFreeBlock(currentSizeX, currentSizeZ, out Vector2Int origin))
                    {
                        OccupyBlock(origin, currentSizeX, currentSizeZ);
                        Vector3 center = GetBlockCenter(origin, currentSizeX, currentSizeZ);
                        center.y += SpawnOffsetY;

                        Animal animal = _entitiesPool.Get();
                        animal.Reset(center);
                        animal.transform.rotation = targetRotation;
                        animal.Released += OnReleased;
                    }
                }
            }
        }

        private bool TryFindFreeBlock(int sizeX, int sizeZ, out Vector2Int origin)
        {
            int rows = _cells.GetLength(0);
            int cols = _cells.GetLength(1);

            List<Vector2Int> candidates = new List<Vector2Int>();

            for (int row = 0; row <= rows - sizeZ; row++)
                for (int col = 0; col <= cols - sizeX; col++)
                    if (IsBlockFree(row, col, sizeX, sizeZ))
                        candidates.Add(new Vector2Int(row, col));

            if (candidates.Count == 0)
            {
                origin = default;
                return false;
            }

            origin = candidates[Random.Range(0, candidates.Count)];
            return true;
        }

        private bool IsBlockFree(int startRow, int startCol, int sizeX, int sizeZ)
        {
            for (int row = startRow; row < startRow + sizeZ; row++)
                for (int col = startCol; col < startCol + sizeX; col++)
                    if (_cells[row, col].IsOccupied)
                        return false;

            return true;
        }

        private void OccupyBlock(Vector2Int origin, int sizeX, int sizeZ)
        {
            for (int row = origin.x; row < origin.x + sizeZ; row++)
                for (int col = origin.y; col < origin.y + sizeX; col++)
                    _cells[row, col].SetOccupied(true);
        }

        private Vector3 GetBlockCenter(Vector2Int origin, int sizeX, int sizeZ)
        {
            Vector3 firstCell = _cells[origin.x, origin.y].transform.position;
            Vector3 lastCell = _cells[origin.x + sizeZ - 1, origin.y + sizeX - 1].transform.position;

            return (firstCell + lastCell) / 2f;
        }
    }
}