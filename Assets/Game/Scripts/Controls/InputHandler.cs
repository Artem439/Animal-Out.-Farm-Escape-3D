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
            if (_inputReader != null && _animalDetector != null)
                _inputReader.LeftMouseButtonClicked += _animalDetector.Detect;
        }

        private void OnDisable()
        {
            if (_inputReader != null && _animalDetector != null)
                _inputReader.LeftMouseButtonClicked -= _animalDetector.Detect;
        }
    }
}
