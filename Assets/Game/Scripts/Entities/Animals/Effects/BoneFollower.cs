using UnityEngine;

namespace Game.Scripts.Entities.Animals.Effects
{
    public class BoneFollower : MonoBehaviour
    {
        [SerializeField] private Transform _bone;

        private Vector3 _localOffset;
        private Quaternion _rotationOffset;
        private bool _initialized;

        private void OnEnable()
        {
            if (_bone == null || _initialized)
                return;

            _localOffset = _bone.InverseTransformPoint(transform.position);
            _rotationOffset = Quaternion.Inverse(_bone.rotation) * transform.rotation;
            _initialized = true;
        }

        private void LateUpdate()
        {
            if (_bone == null)
                return;

            transform.position = _bone.TransformPoint(_localOffset);
            transform.rotation = _bone.rotation * _rotationOffset;
        }
    }
}
