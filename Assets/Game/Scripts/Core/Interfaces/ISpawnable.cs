using System;
using UnityEngine;

namespace Game.Scripts.Core.Interfaces
{
    public interface ISpawnable<T> where T : Component
    {
        event Action<T> Released;
        
        void Reset(Vector3 position);
    }
}