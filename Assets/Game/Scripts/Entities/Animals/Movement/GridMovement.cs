using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Scripts.Environment.Grid;
using Game.Scripts.Environment.Grid.Services;
using UnityEngine;

namespace Game.Scripts.Entities.Animals.Movement
{
    public class GridMovement
    {
        private readonly List<Animal> _blockingAnimals = new();

        public bool ReachedEdge { get; private set; }
        public IReadOnlyList<Animal> BlockingAnimals => _blockingAnimals;

        public IEnumerator Move(Animal animal, GridService gridService, float moveSpeed)
        {
            ReachedEdge = false;
            _blockingAnimals.Clear();

            while (animal.isActiveAndEnabled)
            {
                Vector3 direction = GetDirection(animal);
                List<Cell> nextCells = GetNextCells(animal, gridService, direction);

                if (nextCells == null)
                {
                    ReachedEdge = true;
                    yield break;
                }

                CollectBlockingAnimals(nextCells, _blockingAnimals);

                if (_blockingAnimals.Count > 0)
                    yield break;

                List<Cell> finalCells = GetFinalCells(animal, gridService, nextCells, direction);
                UpdateCells(animal, nextCells, finalCells);

                Vector3 targetPosition = GetTargetPosition(animal, gridService, direction);
                float stepDuration = moveSpeed > 0f ? 1f / moveSpeed : 0f;

                yield return animal.transform
                    .DOMove(targetPosition, stepDuration)
                    .SetEase(Ease.Linear)
                    .WaitForCompletion();
            }
        }

        private static Vector3 GetDirection(Animal animal)
        {
            return new Vector3(animal.transform.forward.x, 0f, animal.transform.forward.z).normalized;
        }

        private static List<Cell> GetNextCells(Animal animal, GridService gridService, Vector3 direction)
        {
            List<Cell> nextCells = new List<Cell>();

            foreach (Cell cell in animal.OccupiedCells)
            {
                Cell neighbor = gridService.GetNeighborCell(cell, direction);

                if (neighbor == null)
                    return null;

                if (nextCells.Contains(neighbor) == false && animal.OccupiedCells.Contains(neighbor) == false)
                    nextCells.Add(neighbor);
            }

            return nextCells;
        }

        private static void CollectBlockingAnimals(List<Cell> nextCells, List<Animal> blockingAnimals)
        {
            foreach (Cell cell in nextCells)
            {
                if (cell.IsOccupied == false)
                    continue;

                Animal occupant = cell.Occupant;

                if (occupant != null && blockingAnimals.Contains(occupant) == false)
                    blockingAnimals.Add(occupant);
            }
        }

        private static Vector3 GetTargetPosition(Animal animal, GridService gridService, Vector3 direction)
        {
            return new Vector3(
                animal.transform.position.x + direction.x * gridService.CellSize,
                animal.transform.position.y,
                animal.transform.position.z + direction.z * gridService.CellSize);
        }

        private static void UpdateCells(Animal animal, List<Cell> nextCells, List<Cell> finalCells)
        {
            List<Cell> cellsToFree = new List<Cell>();

            foreach (Cell cell in animal.OccupiedCells)
            {
                if (finalCells.Contains(cell) == false)
                    cellsToFree.Add(cell);
            }

            foreach (Cell cell in cellsToFree)
            {
                cell.Free();
                animal.FreeCell(cell);
            }

            foreach (Cell cell in nextCells)
            {
                cell.Occupy(animal);
                animal.OccupyCell(cell);
            }
        }

        private static List<Cell> GetFinalCells(
            Animal animal,
            GridService gridService,
            List<Cell> nextCells,
            Vector3 direction)
        {
            Vector3 oppositeDirection = -direction;
            List<Cell> result = new List<Cell>(animal.OccupiedCells);
            List<Cell> toRemove = new List<Cell>();

            foreach (Cell cell in result)
            {
                Cell behindNeighbor = gridService.GetNeighborCell(cell, oppositeDirection);

                if (behindNeighbor == null || animal.OccupiedCells.Contains(behindNeighbor) == false)
                    toRemove.Add(cell);
            }

            for (int i = 0; i < nextCells.Count && i < toRemove.Count; i++)
                result.Remove(toRemove[i]);

            foreach (Cell cell in nextCells)
                result.Add(cell);

            return result;
        }
    }
}
