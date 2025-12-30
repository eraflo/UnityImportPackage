using UnityEngine;
using UnityEngine.AI;

namespace Eraflo.UnityImportPackage.BehaviourTree
{
    /// <summary>
    /// Moves the agent to a target position using NavMeshAgent.
    /// Returns Running while moving, Success when arrived, Failure if path is invalid.
    /// </summary>
    [BehaviourTreeNode("Actions/Navigation", "Move To")]
    public class MoveTo : ActionNode
    {
        [Header("Target")]
        [Tooltip("Target provider for the destination.")]
        public TargetProvider Target;
        
        [Tooltip("Alternative: Blackboard key for target position/transform.")]
        [BlackboardKey]
        public string BlackboardKey;
        
        [Header("Settings")]
        [Tooltip("How close to get before considering arrived.")]
        public float StoppingDistance = 0.5f;
        
        [Tooltip("Movement speed override. 0 = use NavMeshAgent default.")]
        public float Speed = 0f;
        
        [Tooltip("Update destination every frame (for moving targets).")]
        public bool TrackTarget = false;
        
        private NavMeshAgent _agent;
        private Vector3 _lastDestination;
        private bool _pathSet;
        
        protected override void OnStart()
        {
            _agent = Owner?.GetComponent<NavMeshAgent>();
            _pathSet = false;
            
            if (_agent == null)
            {
                Debug.LogWarning("[BT] MoveTo: No NavMeshAgent found on Owner", Owner);
                return;
            }
            
            if (Speed > 0)
                _agent.speed = Speed;
            
            _agent.stoppingDistance = StoppingDistance;
            
            SetDestination();
        }
        
        protected override NodeState OnUpdate()
        {
            if (_agent == null)
                return NodeState.Failure;
            
            if (!_agent.enabled || !_agent.isOnNavMesh)
                return NodeState.Failure;
            
            // Update destination for moving targets
            if (TrackTarget)
            {
                SetDestination();
            }
            
            // Check if we have a valid path
            if (!_pathSet)
                return NodeState.Failure;
            
            // Check if path is still being calculated
            if (_agent.pathPending)
                return NodeState.Running;
            
            // Check if path is invalid
            if (_agent.pathStatus == NavMeshPathStatus.PathInvalid)
                return NodeState.Failure;
            
            // Check if we've arrived
            if (!_agent.hasPath || _agent.remainingDistance <= StoppingDistance)
            {
                _agent.ResetPath();
                return NodeState.Success;
            }
            
            return NodeState.Running;
        }
        
        protected override void OnStop()
        {
            // Optionally stop the agent when the node completes
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                _agent.ResetPath();
            }
        }
        
        private void SetDestination()
        {
            Vector3 destination = GetTargetPosition();
            
            if (destination == Vector3.zero && Target == null && string.IsNullOrEmpty(BlackboardKey))
            {
                _pathSet = false;
                return;
            }
            
            // Only update if destination changed significantly
            if (_pathSet && Vector3.Distance(_lastDestination, destination) < 0.1f)
                return;
            
            _lastDestination = destination;
            _pathSet = _agent.SetDestination(destination);
        }
        
        private Vector3 GetTargetPosition()
        {
            // Priority 1: TargetProvider
            if (Target != null)
                return Target.GetTargetPosition(this);
            
            // Priority 2: Blackboard key
            if (!string.IsNullOrEmpty(BlackboardKey) && Blackboard != null)
            {
                if (Blackboard.TryGet<Vector3>(BlackboardKey, out var pos))
                    return pos;
                if (Blackboard.TryGet<Transform>(BlackboardKey, out var t))
                    return t != null ? t.position : Vector3.zero;
                if (Blackboard.TryGet<GameObject>(BlackboardKey, out var go))
                    return go != null ? go.transform.position : Vector3.zero;
            }
            
            return Vector3.zero;
        }
    }
}
