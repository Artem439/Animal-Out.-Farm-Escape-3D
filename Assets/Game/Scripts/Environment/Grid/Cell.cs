using System;
using UnityEngine;

namespace Game.Scripts.Environment.Grid
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;

        public void SetMaterial(Material material)
        {
            if (_renderer == null)
                throw new NullReferenceException(nameof(_renderer));

            if (material == null)
                throw new NullReferenceException(nameof(material));

            _renderer.material = material;
        }
    }
}