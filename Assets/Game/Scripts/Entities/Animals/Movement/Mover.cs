using System.Collections;
using DG.Tweening;
using Game.Scripts.Environment.Grid.Configuration;
using Game.Scripts.Environment.Grid.Services;
using Game.Scripts.Environment.Grid.Spawner;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Entities.Animals.Movement
{
    public class Mover : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 0.3f;
        [SerializeField] private float _perimeterMoveSpeed = 0.5f;
        [SerializeField] private float _rotationSpeed = 180f;

        private Animal _animal;
        private bool _isMoving;
        private Coroutine _moveRoutine;

        private GridService _gridService;
        private FieldLayout _fieldLayout;
        private PerimeterRoadBuilder _roadBuilder;

        private readonly GridMovement _gridMovement = new();
        private readonly PerimeterMovement _perimeterMovement = new();

        [Inject]
        
        private void Construct(GridService gridService, FieldLayout fieldLayout, PerimeterRoadBuilder roadBuilder)
        {
            _gridService = gridService;
            _fieldLayout = fieldLayout;
            _roadBuilder = roadBuilder;
        }

        private void Awake()
        {
            _animal = GetComponent<Animal>();
        }

        private void OnDisable()
        {
            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
                _moveRoutine = null;
            }

            _animal.transform.DOKill();
            _isMoving = false;
        }

        public void StartMoving()
        {
            if (_isMoving)
                return;

            _moveRoutine = StartCoroutine(MoveRoutine());
        }

        private IEnumerator MoveRoutine()
        {
            _isMoving = true;

            yield return _gridMovement.Move(_animal, _gridService, _moveSpeed);

            if (_gridMovement.ReachedEdge)
            {
                _animal.transform.DOKill();

                yield return _perimeterMovement.Move(
                    _animal,
                    _fieldLayout,
                    _roadBuilder,
                    _perimeterMoveSpeed,
                    _rotationSpeed);
            }

            _isMoving = false;
            _moveRoutine = null;
        }
    }
}
