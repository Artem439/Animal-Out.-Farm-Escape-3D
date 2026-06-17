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
        [SerializeField] private GameObject _decorationsPrefab;
        [SerializeField] private bool _hideCellVisuals = true;
        [SerializeField, Min(0)] private int _spawnBorderOffset = 1;
        [SerializeField, Min(0.01f)] private float _cellSize = 1f;
        [SerializeField, Min(0)] private int _perimeterBorderOffset = 2;
        [SerializeField, Min(0f)] private float _perimeterPathInset = 0.6f;
        [SerializeField] private float _homeXOffset;
        [SerializeField, Min(0f)] private float _homeZOffsetFromBorder = 3.9f;
        [SerializeField, Min(0f)] private float _homeYOffset = 0.5f;
        [SerializeField] private Vector3 _homeScaleOffset;
        [SerializeField] private Sprite _homeSprite;

        public FieldSize Size => _size;
        public GameObject FieldPrefab => _fieldPrefab;
        public GameObject RoadPrefab => _roadPrefab;
        public GameObject PlayingFieldPrefab => _playingFieldPrefab;
        public GameObject DecorationsPrefab => _decorationsPrefab;
        public bool HideCellVisuals => _hideCellVisuals;
        public int SpawnBorderOffset => _spawnBorderOffset;
        public float CellSize => _cellSize;
        public int PerimeterBorderOffset => _perimeterBorderOffset;
        public float PerimeterPathInset => _perimeterPathInset;
        public float HomeXOffset => _homeXOffset;
        public float HomeZOffsetFromBorder => _homeZOffsetFromBorder;
        public float HomeYOffset => _homeYOffset;
        public Vector3 HomeScaleOffset => _homeScaleOffset;
        public Sprite HomeSprite => _homeSprite;
        public int Width => FieldSizeUtility.GetWidth(_size);
        public int Length => FieldSizeUtility.GetLength(_size);
    }
}
