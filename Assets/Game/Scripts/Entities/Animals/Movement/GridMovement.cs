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
        public bool ReachedEdge { get; private set; }

        public IEnumerator Move(Animal animal, GridService gridService, float moveSpeed)
        {
            ReachedEdge = false;

            while (animal.isActiveAndEnabled)
            {
                Vector3 direction = GetDirection(animal);
                List<Cell> nextCells = GetNextCells(animal, gridService, direction);

                if (nextCells == null)
                {
                    ReachedEdge = true;
                    yield break;
                }

                if (AreCellsFree(nextCells) == false)
                    yield break;

                List<Cell> finalCells = GetFinalCells(animal, gridService, nextCells, direction);
                UpdateCells(animal, nextCells, finalCells);

                Vector3 targetPosition = GetTargetPosition(animal, gridService, direction);

                yield return animal.transform
                    .DOMove(targetPosition, moveSpeed)
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

        private static bool AreCellsFree(List<Cell> cells)
        {
            foreach (Cell cell in cells)
                if (cell.IsOccupied)
                    return false;

            return true;
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
                cell.Occupy();
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
