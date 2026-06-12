using Game.Scripts.Entities.Animals.Movement;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Controls
{
    public class InputHandler : MonoBehaviour
    {
        [Inject] private AnimalDetector _animalDetector;
        [Inject] private InputReader _inputReader;

        private void Start()
        {
            _inputReader.LeftMouseButtonClicked += _animalDetector.Detect;
        }

        private void OnDestroy()
        {
            if (_inputReader != null)
                _inputReader.LeftMouseButtonClicked -= _animalDetector.Detect;
        }
    }
}
