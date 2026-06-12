using System;
using UnityEngine;

namespace Game.Scripts.Controls
{
    public class InputReader : MonoBehaviour
    {
        private const KeyCode LeftMouseKey = KeyCode.Mouse0;

        public event Action<Vector3> LeftMouseButtonClicked;

        private void Update()
        {
            if (Input.GetKeyDown(LeftMouseKey))
                LeftMouseButtonClicked?.Invoke(Input.mousePosition);
        }
    }
}
