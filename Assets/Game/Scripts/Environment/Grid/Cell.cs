using System;
using Game.Scripts.Entities.Animals;
using UnityEngine;

namespace Game.Scripts.Environment.Grid
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;

        public int Row { get; private set; }
        public int Column { get; private set; }
        public bool IsOccupied { get; private set; }
        public Animal CurrentAnimal { get; private set; }

        public void Initialize(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public void Occupy(Animal animal)
        {
            IsOccupied = true;
            CurrentAnimal = animal;
        }

        public void Free()
        {
            IsOccupied = false;
            CurrentAnimal = null;
        }

        public void SetMaterial(Material material)
        {
            if (_renderer == null)
                throw new NullReferenceException(nameof(_renderer));

            if (material == null)
                throw new NullReferenceException(nameof(material));

            _renderer.material = material;
        }
    }
}
