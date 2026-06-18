using System;
using System.Collections.Generic;
using Game.Scripts.Core.Interfaces;
using Game.Scripts.Environment.Grid;
using Game.Scripts.Resources.Entities;
using UnityEngine;

namespace Game.Scripts.Entities.Animals
{
    public class Animal : MonoBehaviour, ISpawnable<Animal>
    {
        [SerializeField] private AnimalData _data;
        
        private List<Cell> _occupiedCells;
        
        public event Action<Animal> Released;
        
        public AnimalData Data => _data;
        public IReadOnlyList<Cell> OccupiedCells => _occupiedCells;

        private void Awake()
        {
            _occupiedCells = new List<Cell>();
        }

        public void Reset(Vector3 position)
        {
            transform.position = position;
            _occupiedCells.Clear();
        }

        public void Release()
        {
            FreeAllCells();
            Released?.Invoke(this);
        }

        public void OccupyCell(Cell cell)
        {
            _occupiedCells.Add(cell);
        }

        public void FreeAllCells()
        {
            foreach (Cell cell in _occupiedCells)
                cell.Free();

            _occupiedCells.Clear();
        }

        public void FreeCell(Cell cell)
        {
            _occupiedCells.Remove(cell);
        }
    }
}