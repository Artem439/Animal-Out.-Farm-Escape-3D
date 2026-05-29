using UnityEngine;
using UnityEngine.Pool;

namespace Game.Scripts.Core.Spawn
{
    public abstract class Pool<T> : MonoBehaviour where T : Component
    {
        [SerializeField] private T _entityPrefab;
        [SerializeField] private int _capacity;
        [SerializeField] private int _maxSize;
        
        private ObjectPool<T> _pool;
    
        private void Awake()
        {
            _pool = new ObjectPool<T>(
                createFunc: () => CreateObject(),
                actionOnGet: (obj) => OnGetObject(obj),
                actionOnRelease: (obj) => OnReleaseObject(obj),
                actionOnDestroy: (obj) => Destroy(obj.gameObject),
                collectionCheck: true,
                defaultCapacity: _capacity,
                maxSize: _maxSize);
        }
    
        public T Get()
        {
            return _pool.Get();
        }

        public void Release(T entity)
        {
            _pool.Release(entity);
        }

        private T CreateObject()
        {
            return Instantiate(_entityPrefab);
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