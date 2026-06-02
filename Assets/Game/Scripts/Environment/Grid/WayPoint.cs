using UnityEngine;

namespace Game.Scripts.Environment.Grid
{
    public class WayPoint : MonoBehaviour
    {
        public WayPoint Next { get; set; }
        public WayPoint Previous { get; set; }
    }
}