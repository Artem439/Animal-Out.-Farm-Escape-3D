using Game.Scripts.Entities.Animals.Spawn;
using Game.Scripts.Environment.Grid.Configuration;
using Game.Scripts.Environment.Grid.Services;
using Game.Scripts.Environment.Grid.Spawner;
using Game.Scripts.Resources.Levels;
using UnityEngine;

namespace Game.Scripts.Core
{
    [DefaultExecutionOrder(100)]
    public class LevelBootstrap : MonoBehaviour
    {
        [SerializeField] private LevelConfiguration _level;
        [SerializeField] private FieldLayout _fieldLayout;
        [SerializeField] private CellsSpawner _cellsSpawner;
        [SerializeField] private PerimeterWayPointsSpawner _wayPointsSpawner;
        [SerializeField] private PerimeterRoadBuilder _roadBuilder;
        [SerializeField] private GridService _gridService;
        [SerializeField] private Spawner _animalSpawner;
        [SerializeField] private FieldEnvironmentSpawner _fieldEnvironmentSpawner;

        private void Start()
        {
            if (_level != null && _level.Field != null)
            {
                _fieldLayout.ApplyConfiguration(_level.Field);

                if (_fieldEnvironmentSpawner != null)
                    _fieldEnvironmentSpawner.Spawn(_level.Field);
            }

            _cellsSpawner.Build();
            _gridService.BindCells(_cellsSpawner.Cells);
            _wayPointsSpawner.Build();
            _roadBuilder.Build();

            if (_level != null)
                _animalSpawner.Build(_level.AnimalSpawns);
        }
    }
}
