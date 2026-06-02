using System.Collections.Generic;
using Game.Scripts.Core.Interfaces;
using UnityEngine;

namespace Game.Scripts.Core.Spawn
{
    public abstract class Spawner<T> : MonoBehaviour where T : Component, ISpawnable<T>
    {
        [SerializeField] private Pool<T> _entitiesPool;
        [SerializeField] private List<Transform> _spawnPoints;

        protected Pool<T> EntitiesPool => _entitiesPool;

        protected virtual void Spawn()
        {
            for (int i = 0; i < _spawnPoints.Count; i++)
            {
                Vector3 spawnPosition = _spawnPoints[i].position;
                T entity = _entitiesPool.Get();
                entity.Reset(spawnPosition);
                entity.Released += OnReleased;
            }
        }

        protected virtual void OnReleased(T entity)
        {
            entity.Released -= OnReleased;
            _entitiesPool.Release(entity);
        }
    }
}