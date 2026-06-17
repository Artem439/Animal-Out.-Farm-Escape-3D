using Game.Scripts.Environment.Grid.Configuration;
using Game.Scripts.Resources.Environment;
using UnityEngine;

namespace Game.Scripts.Environment.Grid
{
    public class AnimalHomePlacer : MonoBehaviour
    {
        private const int HomeSpriteSortingOrder = 50;

        [SerializeField] private FieldLayout _fieldLayout;

        private Vector3 _initialHomeVisualLocalScale;

        private void Awake()
        {
            Transform homeVisual = GetHomeVisual();
            if (homeVisual != null)
                _initialHomeVisualLocalScale = homeVisual.localScale;
        }

        public void Place()
        {
            if (_fieldLayout == null || _fieldLayout.Configuration == null)
                return;

            FieldConfiguration configuration = _fieldLayout.Configuration;
            Transform homeVisual = GetHomeVisual();
            float visualForwardOffset = homeVisual != null ? homeVisual.localPosition.z : 0f;

            Vector3 position = transform.position;
            position.x = _fieldLayout.transform.position.x + configuration.HomeXOffset;
            position.y = configuration.HomeYOffset;
            position.z = _fieldLayout.GetFrontBorderZ()
                + configuration.HomeZOffsetFromBorder
                - visualForwardOffset;
            transform.position = position;

            if (homeVisual == null)
                return;

            homeVisual.localScale = _initialHomeVisualLocalScale + configuration.HomeScaleOffset;

            SpriteRenderer homeSprite = homeVisual.GetComponent<SpriteRenderer>();
            if (homeSprite == null)
                return;

            if (configuration.HomeSprite != null)
                homeSprite.sprite = configuration.HomeSprite;

            homeSprite.sortingOrder = HomeSpriteSortingOrder;
        }

        private Transform GetHomeVisual()
        {
            return transform.childCount > 0 ? transform.GetChild(0) : null;
        }
    }
}
