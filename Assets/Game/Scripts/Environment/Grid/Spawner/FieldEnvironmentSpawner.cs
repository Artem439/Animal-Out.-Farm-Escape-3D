using Game.Scripts.Resources.Environment;
using UnityEngine;

namespace Game.Scripts.Environment.Grid.Spawner
{
    public class FieldEnvironmentSpawner : MonoBehaviour
    {
        [SerializeField] private Transform _parent;

        private GameObject _fieldInstance;
        private GameObject _roadInstance;
        private GameObject _playingFieldInstance;

        public void Spawn(FieldConfiguration configuration)
        {
            Clear();

            if (configuration == null)
                return;

            Transform parent = _parent != null ? _parent : transform;

            if (configuration.FieldPrefab != null)
            {
                _fieldInstance = Instantiate(configuration.FieldPrefab, parent);
                _fieldInstance.transform.localRotation = Quaternion.identity;
                _fieldInstance.transform.localScale = Vector3.one;
            }

            if (configuration.RoadPrefab != null)
            {
                _roadInstance = Instantiate(configuration.RoadPrefab, parent);
                _roadInstance.transform.localRotation = Quaternion.identity;
                _roadInstance.transform.localScale = Vector3.one;
            }

            if (configuration.PlayingFieldPrefab != null)
            {
                _playingFieldInstance = Instantiate(configuration.PlayingFieldPrefab, parent);
                _playingFieldInstance.transform.localRotation = Quaternion.identity;
                _playingFieldInstance.transform.localScale = Vector3.one;
            }
        }

        public void Clear()
        {
            if (_fieldInstance != null)
            {
                Destroy(_fieldInstance);
                _fieldInstance = null;
            }

            if (_roadInstance != null)
            {
                Destroy(_roadInstance);
                _roadInstance = null;
            }

            if (_playingFieldInstance != null)
            {
                Destroy(_playingFieldInstance);
                _playingFieldInstance = null;
            }
        }
    }
}
