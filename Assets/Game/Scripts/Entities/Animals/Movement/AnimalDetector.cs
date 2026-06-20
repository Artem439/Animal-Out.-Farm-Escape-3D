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

            if (Physics.Raycast(ray, out RaycastHit hit) == false)
                return;

            Mover mover = hit.collider.GetComponentInParent<Mover>();

            if (mover != null)
                mover.StartMoving();
        }
    }
}
