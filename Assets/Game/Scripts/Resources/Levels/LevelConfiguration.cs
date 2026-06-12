using System.Collections.Generic;
using Game.Scripts.Entities.Animals.Spawn;
using Game.Scripts.Resources.Environment;
using UnityEngine;

namespace Game.Scripts.Resources.Levels
{
    [CreateAssetMenu(fileName = "LevelConfiguration", menuName = "Gameplay/Level Configuration")]
    public class LevelConfiguration : ScriptableObject
    {
        [SerializeField] private FieldConfiguration _field;
        [SerializeField] private List<AnimalSpawnEntry> _animalSpawns = new();

        public FieldConfiguration Field => _field;
        public IReadOnlyList<AnimalSpawnEntry> AnimalSpawns => _animalSpawns;
    }
}
