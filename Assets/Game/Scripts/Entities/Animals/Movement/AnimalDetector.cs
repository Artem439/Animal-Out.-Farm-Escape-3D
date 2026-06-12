using UnityEngine;
using VContainer;

namespace Game.Scripts.Entities.Animals.Movement
{
    public class AnimalDetector : MonoBehaviour
    {
        [Inject] private Camera _camera;

        public void Detect(Vector3 mousePosition)
        {
            Ray ray = _camera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
                if (hit.collider.TryGetComponent(out Mover mover))
                    mover.StartMoving();
        }
    }
}
