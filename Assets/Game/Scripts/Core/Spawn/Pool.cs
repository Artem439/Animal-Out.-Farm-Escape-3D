using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Scripts.Core.Spawn
{
    public abstract class Pool<T> : MonoBehaviour where T : Component
    {
        [SerializeField] private int _capacity;
        [SerializeField] private int _maxSize;

        private readonly Dictionary<T, ObjectPool<T>> _pools = new();
        private readonly Dictionary<T, T> _prefabByInstance = new();

        public T Get(T prefab)
        {
            return GetPool(prefab).Get();
        }

        public void Release(T entity)
        {
            if (_prefabByInstance.TryGetValue(entity, out T prefab) == false)
                return;

            if (_pools.TryGetValue(prefab, out ObjectPool<T> pool))
                pool.Release(entity);
        }

        private ObjectPool<T> GetPool(T prefab)
        {
            if (_pools.TryGetValue(prefab, out ObjectPool<T> pool))
                return pool;

            pool = new ObjectPool<T>(
                createFunc: () => CreateObject(prefab),
                actionOnGet: OnGetObject,
                actionOnRelease: OnReleaseObject,
                actionOnDestroy: DestroyObject,
                collectionCheck: true,
                defaultCapacity: _capacity,
                maxSize: _maxSize);

            _pools.Add(prefab, pool);

            return pool;
        }

        private T CreateObject(T prefab)
        {
            T instance = Instantiate(prefab);
            _prefabByInstance.Add(instance, prefab);

            return instance;
        }

        private void DestroyObject(T entity)
        {
            _prefabByInstance.Remove(entity);
            Destroy(entity.gameObject);
        }

        private void OnGetObject(T entity)
        {
            entity.gameObject.SetActive(true);
        }

        private void OnReleaseObject(T entity)
        {
            entity.gameObject.SetActive(false);
        }
    }
}
