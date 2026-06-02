using Game.Scripts.Entities.Animals.Movement;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Controls
{
    public class InputHandler : MonoBehaviour
    {
        [Inject] private AnimalDetector _animalDetector;
        [Inject] private InputReader _inputReader;

        private void OnEnable()
        {
            _inputReader.LeftMouseButtonClicked += _animalDetector.Detect;
        }

        private void OnDisable()
        {
            _inputReader.LeftMouseButtonClicked -= _animalDetector.Detect;
        }
    }
}