using Game.Scripts.Environment.Grid.Configuration;
using UnityEngine;

namespace Game.Scripts.Environment.Grid
{
    public class AnimalHomePlacer : MonoBehaviour
    {
        [SerializeField] private FieldLayout _fieldLayout;
        [SerializeField, Min(0f)] private float _zOffsetFromBorder = 1f;
        [SerializeField] private float _yPosition = 0.5f;

        private void Awake()
        {
            Place();
        }

        private void Place()
        {
            if (_fieldLayout == null)
                return;

            Vector3 position = transform.position;
            position.x = _fieldLayout.transform.position.x;
            position.y = _yPosition;
            position.z = _fieldLayout.GetFrontBorderZ() + _zOffsetFromBorder;
            transform.position = position;
        }
    }
}
