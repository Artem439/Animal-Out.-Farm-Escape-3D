using System;
using Game.Scripts.Controls;
using Game.Scripts.Entities.Animals.Movement;
using Game.Scripts.Entities.Animals.Spawn;
using Game.Scripts.Environment.Grid.Configuration;
using Game.Scripts.Environment.Grid.Services;
using Game.Scripts.Environment.Grid.Spawner;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.Core
{
    [DefaultExecutionOrder(-100)]
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private GridService _gridService;
        [SerializeField] private FieldLayout _fieldLayout;
        [SerializeField] private PerimeterRoadBuilder _roadBuilder;
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private AnimalDetector _animalDetector;
        [SerializeField] private Spawner _animalSpawner;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterComponent(builder, Resolve(_camera, () => Camera.main));
            RegisterComponent(builder, Resolve(_gridService, Find<GridService>));
            RegisterComponent(builder, Resolve(_fieldLayout, Find<FieldLayout>));
            RegisterComponent(builder, Resolve(_roadBuilder, Find<PerimeterRoadBuilder>));
            RegisterComponent(builder, Resolve(_inputReader, Find<InputReader>));
            RegisterComponent(builder, Resolve(_inputHandler, Find<InputHandler>));
            RegisterComponent(builder, Resolve(_animalDetector, Find<AnimalDetector>));
            RegisterComponent(builder, Resolve(_animalSpawner, Find<Spawner>));
        }

        private static T Resolve<T>(T serialized, Func<T> fallback) where T : Component
        {
            return serialized != null ? serialized : fallback();
        }

        private static T Find<T>() where T : Component
        {
            return FindAnyObjectByType<T>();
        }

        private static void RegisterComponent<T>(IContainerBuilder builder, T component) where T : Component
        {
            if (component == null)
                return;

            builder.RegisterComponent(component);
        }
    }
}
