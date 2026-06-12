using System;
using Game.Scripts.Resources.Entities;
using UnityEngine;

namespace Game.Scripts.Entities.Animals.Spawn
{
    [Serializable]
    public class AnimalSpawnEntry
    {
        [SerializeField] private AnimalData _data;
        [SerializeField, Min(0)] private int _count = 1;

        public AnimalData Data => _data;
        public int Count => _count;
    }
}
