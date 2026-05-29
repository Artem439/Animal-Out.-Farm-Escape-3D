using UnityEngine;

namespace Game.Scripts.Environment.Grid.Render
{
    public abstract class CellMaterialProvider : MonoBehaviour
    {
        public abstract Material GetMaterial(int row, int column);
    }
}