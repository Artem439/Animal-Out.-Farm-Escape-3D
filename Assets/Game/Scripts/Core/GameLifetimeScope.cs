using Game.Scripts.Controls;
using Game.Scripts.Entities.Animals.Movement;
using Game.Scripts.Entities.Animals.Spawn;
using Game.Scripts.Environment.Grid.Services;
using Game.Scripts.Environment.Grid.Spawner;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.Core
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<GridService>();
            builder.RegisterComponentInHierarchy<WayPointsSpawner>();
            builder.RegisterComponentInHierarchy<CellsSpawner>();
            builder.RegisterComponentInHierarchy<InputReader>();
            builder.RegisterComponentInHierarchy<InputHandler>();
            builder.RegisterComponentInHierarchy<AnimalDetector>();
            builder.RegisterComponentInHierarchy<Spawner>();

            builder.RegisterComponent(FindAnyObjectByType<Camera>());
        }
    }
}
