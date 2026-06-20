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
        public Animal Occupant { get; private set; }

        public void Initialize(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public void Occupy(Animal animal)
        {
            IsOccupied = true;
            Occupant = animal;
        }

        public void Free()
        {
            IsOccupied = false;
            Occupant = null;
        }

        public void SetVisualEnabled(bool enabled)
        {
            if (_renderer != null)
                _renderer.enabled = enabled;
        }
    }
}
