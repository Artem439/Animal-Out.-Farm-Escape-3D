using UnityEngine;

namespace Game.Scripts.Environment.Grid.Road
{
    public class RoadPathMerge : MonoBehaviour
    {
        [SerializeField] private Transform _mergeWaypoint;
        [SerializeField] private Transform _rightBranchWaypoint;
        [SerializeField] private Transform _leftBranchWaypoint;

        public Transform MergeWaypoint => _mergeWaypoint;
        public Transform RightBranchWaypoint => _rightBranchWaypoint;
        public Transform LeftBranchWaypoint => _leftBranchWaypoint;
    }
}
