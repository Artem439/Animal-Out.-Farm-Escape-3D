using Game.Scripts.Entities.Animals;
using UnityEngine;

namespace Game.Scripts.Resources.Entities
{
    [CreateAssetMenu(fileName = "AnimalData", menuName = "Gameplay/New AnimalData")]
    public class AnimalData : ScriptableObject
    {
        public string Name => _name;
        public int SizeX => _sizeX;
        public int SizeZ => _sizeZ;
        public Vector3 BaseRotation => _baseRotation;
        public Animal AnimalPrefab => _animalPrefab;

        [SerializeField] private string _name;
        [SerializeField] private int _sizeX;
        [SerializeField] private int _sizeZ;
        [SerializeField] private Vector3 _baseRotation;
        [SerializeField] private Animal _animalPrefab;
    }
}