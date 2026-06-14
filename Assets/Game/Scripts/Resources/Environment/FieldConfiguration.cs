using Game.Scripts.Environment.Grid.Configuration;
using UnityEngine;

namespace Game.Scripts.Resources.Environment
{
    [CreateAssetMenu(fileName = "FieldConfiguration", menuName = "Gameplay/Field Configuration")]
    public class FieldConfiguration : ScriptableObject
    {
        [SerializeField] private FieldSize _size = FieldSize.Field8x8;
        [SerializeField] private GameObject _fieldPrefab;
        [SerializeField] private GameObject _roadPrefab;
        [SerializeField] private GameObject _playingFieldPrefab;
        [SerializeField] private bool _hideCellVisuals = true;
        [SerializeField, Min(0.01f)] private float _cellSize = 1f;
        [SerializeField, Min(0)] private int _perimeterBorderOffset = 2;

        public FieldSize Size => _size;
        public GameObject FieldPrefab => _fieldPrefab;
        public GameObject RoadPrefab => _roadPrefab;
        public GameObject PlayingFieldPrefab => _playingFieldPrefab;
        public bool HideCellVisuals => _hideCellVisuals;
        public float CellSize => _cellSize;
        public int PerimeterBorderOffset => _perimeterBorderOffset;
        public int Width => FieldSizeUtility.GetWidth(_size);
        public int Length => FieldSizeUtility.GetLength(_size);
    }
}
