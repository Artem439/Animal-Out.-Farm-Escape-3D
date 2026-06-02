using Game.Scripts.Entities.Animals;
using UnityEngine;

namespace Game.Scripts.Environment.Grid
{
    public class HouseTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Animal animal))
                animal.Release();
        }
    }
}