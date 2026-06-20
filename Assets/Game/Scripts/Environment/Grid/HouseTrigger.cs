using Game.Scripts.Entities.Animals;
using UnityEngine;

namespace Game.Scripts.Environment.Grid
{
    public class HouseTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Animal animal = other.GetComponentInParent<Animal>();

            if (animal != null)
                animal.Release();
        }
    }
}