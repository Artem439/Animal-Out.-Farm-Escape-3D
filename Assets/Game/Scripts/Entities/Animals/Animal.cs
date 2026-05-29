using System;
using Game.Scripts.Core.Interfaces;
using Game.Scripts.Resources.Entities;
using UnityEngine;

namespace Game.Scripts.Entities.Animals
{
    public class Animal : MonoBehaviour, ISpawnable<Animal>
    {
        [SerializeField] private AnimalData _data;
        
        public AnimalData Data => _data;
        
        public event Action<Animal> Released;

        public void Reset(Vector3 position)
        {
            Reset(position, 0);
        }
        
        public void Reset(Vector3 position, int angle)
        {
            transform.position = position;

            transform.rotation = Quaternion.Euler(
                _data.BaseRotation.x,
                _data.BaseRotation.y + angle,
                _data.BaseRotation.z
            );
        }
    }
}