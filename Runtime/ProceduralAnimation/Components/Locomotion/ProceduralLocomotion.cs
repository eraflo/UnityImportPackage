using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Eraflo.Catalyst.ProceduralAnimation;
using Eraflo.Catalyst.ProceduralAnimation.Jobs;
using Eraflo.Catalyst.ProceduralAnimation.Perception;
using Eraflo.Catalyst.ProceduralAnimation.SignalProcessing;
using Eraflo.Catalyst.ProceduralAnimation.Solvers;

namespace Eraflo.Catalyst.ProceduralAnimation.Components.Locomotion
{
    /// <summary>
    /// Procedural locomotion using the job system.
    /// Implements IProceduralAnimationJob for integration with AnimationJobManager.
    /// </summary>
    [AddComponentMenu("Catalyst/Procedural Animation/Procedural Locomotion")]
    [RequireComponent(typeof(ProceduralAnimator))]
    public class ProceduralLocomotion : MonoBehaviour, IProceduralAnimationJob
    {
        [Header("Movement")]
        [Tooltip("Movement input (normalized direction).")]
        [SerializeField] private Vector3 _movementInput;
        
        [Tooltip("Movement speed in m/s.")]
        [SerializeField] private float _speed = 2f;
        
        [Header("Gait")]
        [Tooltip("Gait cycle duration.")]
        [SerializeField] private float _gaitDuration = 0.8f;
        
        [Tooltip("Stance ratio (0-1).")]
        [SerializeField, Range(0.3f, 0.8f)] private float _stanceRatio = 0.6f;
        
        [Tooltip("Step height for walking.")]
        [SerializeField] private float _stepHeight = 0.15f;
        
        [Tooltip("Step length for walking (how far ahead to step).")]
        [SerializeField] private float _stepLength = 0.4f;
        
        [Tooltip("Auto-calculate step parameters based on leg length.")]
        [SerializeField] private bool _autoCalculateStepParams = true;
        
        [Header("Speed Adaptation")]
        [Tooltip("Speed threshold for sprinting (m/s).")]
        [SerializeField] private float _sprintSpeed = 5f;
        
        [Tooltip("Step length multiplier when sprinting.")]
        [SerializeField] private float _sprintStepMultiplier = 1.5f;
        
        [Tooltip("Step height multiplier when sprinting.")]
        [SerializeField] private float _sprintHeightMultiplier = 1.3f;
        
        [Tooltip("Rest stance width multiplier (spread feet at rest).")]
        [SerializeField] private float _restStanceWidth = 1.2f;
        
        [Header("Arm Swing")]
        [Tooltip("Enable arm swing during walking.")]
        [SerializeField] private bool _enableArmSwing = true;
        
        [Tooltip("Arm swing amplitude in degrees.")]
        [SerializeField] private float _armSwingAmplitude = 30f;
        
        [Tooltip("Arm swing speed multiplier.")]
        [SerializeField] private float _armSwingSpeed = 1f;
        
        [Tooltip("Arm rest rotation offset (X pitch, Y yaw, Z roll) to bring arms from T-pose to natural position.")]
        [SerializeField] private Vector3 _armRestOffset = new Vector3(0, 0, -70f);  // Default: arms down by sides
        
        [Tooltip("Swing rotation axis: 0=X (pitch), 1=Y (yaw), 2=Z (roll).")]
        [SerializeField, Range(0, 2)] private int _armSwingAxis = 0;
        
        [Tooltip("Additional outward swing angle to prevent clipping with the body.")]
        [SerializeField] private float _armSwingOutward = 10f;
        
        [Header("Balance")]
        [Tooltip("Target height above ground.")]
        [SerializeField] private float _targetHeight = 1f;
        
        [Tooltip("Bobbing amplitude.")]
        [SerializeField] private float _bobbingAmplitude = 0.02f;
        
        [Tooltip("Movement lean angle.")]
        [SerializeField, Range(0f, 20f)] private float _leanAngle = 5f;
        
        [Header("Spring Settings")]
        [Tooltip("Foot spring frequency.")]
        [SerializeField] private float _footSpringFrequency = 5f;
        
        [Tooltip("Foot spring damping.")]
        [SerializeField] private float _footSpringDamping = 0.8f;
        
        [Header("Ground Detection")]
        [Tooltip("Layer mask for ground detection.")]
        [SerializeField] private LayerMask _groundMask = ~0;  // Everything by default
        
        [Tooltip("Maximum raycast distance.")]
        [SerializeField] private float _raycastDistance = 2f;
        
        [Tooltip("Offset from hip for raycast origin.")]
        [SerializeField] private float _raycastHeightOffset = 0.5f;
        
        [Tooltip("Foot height offset (distance from ankle bone to sole of foot).")]
        [SerializeField] private float _footHeightOffset = 0.08f;
        
        // Native arrays
        private NativeArray<float3> _footTargets;
        private NativeArray<float3> _footPositions;
        private NativeArray<float3> _footVelocities;
        private NativeArray<bool> _footInSwing;
        private NativeArray<float> _footSwingProgress;
        private NativeArray<float3> _footSwingStart;
        private NativeArray<float> _footStepHeights;
        private NativeArray<bool> _footGrounded;
        
        private NativeArray<float3> _bodyPosition;
        private NativeArray<quaternion> _bodyRotation;
        
        // Two-Bone IK arrays for leg solving
        private NativeArray<float3> _legJointPositions;  // All joints for all legs (hip, knee, ankle)
        private NativeArray<float3> _legOriginalPositions;  // Original positions before IK
        private NativeArray<float> _legBoneLengths;
        private NativeArray<quaternion> _legRotations;
        private NativeArray<quaternion> _legOriginalRotations;
        private NativeArray<int2> _legChainRanges;       // Start index + length per leg
        private NativeArray<float3> _legRootPositions;
        private NativeArray<float3> _legPoleTargets;     // Knee direction hints
        private NativeArray<quaternion> _footBindRotations;  // Original foot rotations to preserve
        private TransformAccessArray _legTransformAccess;
        private Transform[] _allLegBones;
        
        // Runtime
        private ProceduralAnimator _animator;
        private LimbChain[] _legs;
        private float[] _legPhases;
        private GaitCycle _gaitCycle;
        private float3 _velocity;
        private bool _initialized;
        private bool _needsUpdate;
        private float _deltaTime;
        private bool[] _wasInStance;
        private float3[] _localStepOffsets; // Rest position of feet relative to character
        private bool _isMoving;  // Track if character is currently moving
        
        // Arm swing
        private LimbChain[] _arms;
        private quaternion[] _armBindRotations;  // Original arm rotations
        
        private SpringCoefficients _springConfig;
        private InertializationBlender _velocityInertializer;
        private float3 _smoothedVelocity;
        private int _totalLegBones;
        private float _groundHeight;
        private float _maxLegReach;  // Maximum distance the foot can reach from the hip
        private NativeArray<float3> _legBindPositionsLocal;
        private NativeArray<quaternion> _legBindRotationsLocal;
        
        [Header("IK")]
        [Tooltip("Enable FABRIK IK for leg bones.")]
        [SerializeField] private bool _enableLegIK = true;
        
        [Tooltip("FABRIK solver iterations.")]
        [SerializeField, Range(1, 15)] private int _ikIterations = 8;
        
        /// <summary>
        /// Movement input direction.
        /// </summary>
        public Vector3 MovementInput
        {
            get => _movementInput;
            set => _movementInput = value;
        }
        
        /// <summary>
        /// Movement speed.
        /// </summary>
        public float Speed
        {
            get => _speed;
            set => _speed = value;
        }
        
        /// <summary>
        /// Current velocity.
        /// </summary>
        public float3 Velocity => _velocity;
        
        /// <summary>
        /// Current gait phase (0-1).
        /// </summary>
        public float GaitPhase => _gaitCycle?.Phase ?? 0f;
        
        #region IProceduralAnimationJob Implementation
        
        public bool NeedsUpdate => _needsUpdate && _initialized;
        
        public void Prepare(float deltaTime)
        {
            _deltaTime = deltaTime;
            
            // Update velocity with inertialization
            float3 inputDir = math.normalizesafe(new float3(_movementInput.x, 0, _movementInput.z));
            float3 targetVelocity = inputDir * _speed;
            
            _velocityInertializer.Update(deltaTime);
            _smoothedVelocity = _velocityInertializer.ApplyPosition(targetVelocity);
            _velocity = _smoothedVelocity;
            
            // Update gait cycle
            float movementSpeed = math.length(_velocity);
            _gaitCycle.Update(deltaTime, movementSpeed);
            
            // Movement hysteresis and stabilization
            float moveThreshold = _isMoving ? 0.05f : 0.15f; 
            _isMoving = movementSpeed > moveThreshold;
            
            for (int i = 0; i < _legs.Length; i++)
            {
                // The gait cycle methods already apply the global phase to the leg's offset
                float legOffset = _legs[i].GaitPhase;
                
                // Get gait cycle state
                bool inStance = true;
                bool inSwing = false;
                float swingProgress = 0f;
                
                if (_isMoving)
                {
                    inStance = _gaitCycle.IsInStance(legOffset);
                    inSwing = _gaitCycle.IsInSwing(legOffset);
                    swingProgress = _gaitCycle.GetSwingProgress(legOffset);
                    _legPhases[i] = _gaitCycle.GetLegPhase(legOffset);
                }
                else
                {
                    _legPhases[i] = 0f;
                }
                
                // Calculate hip position
                float3 hipPos = _legs[i].Root != null ? (float3)_legs[i].Root.position : (float3)transform.position;
                
                // ===== FOOT PLACEMENT =====
                // Basic approach: feet follow a position relative to hips with offset based on movement
                
                // Calculate speed-adaptive step parameters
                float currentSpeed = math.length(_velocity);
                float speedRatio = math.saturate(currentSpeed / _sprintSpeed);  // 0 at walk, 1 at sprint
                
                // Interpolate step parameters based on speed
                float adaptiveStepLength = math.lerp(_stepLength, _stepLength * _sprintStepMultiplier, speedRatio);
                float adaptiveStepHeight = math.lerp(_stepHeight, _stepHeight * _sprintHeightMultiplier, speedRatio);
                
                // Base position: directly below hip with the leg's natural lateral offset
                // Apply rest stance width multiplier when stationary
                float3 localOffset = _localStepOffsets[i];
                if (!_isMoving)
                {
                    localOffset.x *= _restStanceWidth;  // Spread feet wider at rest
                }
                float3 basePos = hipPos + (float3)transform.TransformDirection(localOffset);
                basePos.y = _groundHeight + _footHeightOffset;
                
                float3 footTarget;
                
                if (_isMoving)
                {
                    // Movement direction
                    float3 moveDir = math.normalizesafe(_velocity);
                    
                    if (inSwing)
                    {
                        // SWING: Foot moves forward with an arc
                        // Start position: behind the hip
                        // End position: ahead of the hip
                        float3 backPos = basePos - moveDir * adaptiveStepLength * 0.5f;
                        float3 frontPos = basePos + moveDir * adaptiveStepLength * 0.5f;
                        
                        // Interpolate from back to front
                        footTarget = math.lerp(backPos, frontPos, swingProgress);
                        
                        // Add arc height
                        float arcHeight = math.sin(swingProgress * math.PI) * adaptiveStepHeight;
                        footTarget.y += arcHeight;
                    }
                    else
                    {
                        // STANCE: Foot stays behind the hip (simulating drag)
                        // The foot gradually moves from front to back as the body passes over it
                        float stanceProgress = _gaitCycle.GetStanceProgress(legOffset);
                        float3 frontPos = basePos + moveDir * adaptiveStepLength * 0.3f;
                        float3 backPos = basePos - moveDir * adaptiveStepLength * 0.3f;
                        
                        footTarget = math.lerp(frontPos, backPos, stanceProgress);
                    }
                }
                else
                {
                    // Stationary: feet at rest position (already with wider stance)
                    footTarget = basePos;
                }
                
                // Raycast to find ground height
                float groundY = _groundHeight;
                Vector3 rayOrigin = new Vector3(footTarget.x, hipPos.y + _raycastHeightOffset, footTarget.z);
                
                if (UnityEngine.Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, _raycastDistance, _groundMask))
                {
                    groundY = hit.point.y;
                }
                
                // Apply ground height (but preserve arc height during swing)
                if (inSwing)
                {
                    float arcHeight = footTarget.y - (_groundHeight + _footHeightOffset);
                    footTarget.y = groundY + _footHeightOffset + arcHeight;
                }
                else
                {
                    footTarget.y = groundY + _footHeightOffset;
                }
                
                // Clamp to max leg reach
                float3 hipToTarget = footTarget - hipPos;
                float distToTarget = math.length(hipToTarget);
                
                if (distToTarget > _maxLegReach)
                {
                    footTarget = hipPos + math.normalizesafe(hipToTarget) * _maxLegReach;
                }
                
                // Set the final target
                _footTargets[i] = footTarget;
                
                // For the spring system, we'll bypass it and set position directly for now
                _footPositions[i] = footTarget;
                _footVelocities[i] = 0;
                
                // Store state
                _wasInStance[i] = inStance;
                _footInSwing[i] = inSwing;
                _footGrounded[i] = inStance;
                _footSwingProgress[i] = swingProgress;
                _footStepHeights[i] = _stepHeight;
                
                // Prepare Two-Bone IK data
                if (_enableLegIK && _legRootPositions.IsCreated)
                {
                    _legRootPositions[i] = hipPos;
                    
                    // Calculate pole target based on CURRENT knee position
                    // This preserves the natural bending direction of the leg
                    var leg = _legs[i];
                    var ikBones = leg.GetIKBones();
                    
                    if (ikBones.Length >= 2)
                    {
                        // Get current knee position
                        float3 kneePos = ikBones[1].position;
                        float3 anklePos = leg.Effector != null ? (float3)leg.Effector.position : _footPositions[i];
                        
                        // Calculate the plane defined by hip-knee-ankle
                        float3 hipToKnee = kneePos - hipPos;
                        float3 hipToAnkle = anklePos - hipPos;
                        
                        // The pole target should be in the direction the knee is already pointing
                        // Project knee position onto the plane perpendicular to hip-ankle line
                        float3 hipAnkleDir = math.normalizesafe(hipToAnkle);
                        float3 kneeProj = hipToKnee - hipAnkleDir * math.dot(hipToKnee, hipAnkleDir);
                        
                        // Pole target is the knee position extended further out
                        float3 midpoint = (hipPos + anklePos) * 0.5f;
                        _legPoleTargets[i] = midpoint + math.normalizesafe(kneeProj) * 0.5f;
                    }
                    else
                    {
                        // Fallback: use forward direction
                        float3 anklePos = leg.Effector != null ? (float3)leg.Effector.position : _footPositions[i];
                        float3 midpoint = (hipPos + anklePos) * 0.5f;
                        _legPoleTargets[i] = midpoint + (float3)transform.forward * 0.5f;
                    }
                    
                    var range = _legChainRanges[i];
                    for (int j = 0; j < range.y; j++)
                    {
                        int idx = range.x + j;
                        _legJointPositions[idx] = _allLegBones[idx].position;
                        
                        // Use stable bind pose transformed to world space
                        _legOriginalPositions[idx] = transform.TransformPoint(_legBindPositionsLocal[idx]);
                        _legOriginalRotations[idx] = math.mul((quaternion)transform.rotation, _legBindRotationsLocal[idx]);
                    }
                }
            }
        }
        
        public JobHandle Schedule(JobHandle dependency)
        {
            // Schedule foot placement job
            var footJob = new FootPlacementJob
            {
                TargetPositions = _footTargets,
                IsInSwing = _footInSwing,
                SwingProgress = _footSwingProgress,
                SwingStartPositions = _footSwingStart,
                StepHeights = _footStepHeights,
                Positions = _footPositions,
                Velocities = _footVelocities,
                SpringConfig = _springConfig,
                DeltaTime = _deltaTime
            };
            
            var footHandle = footJob.Schedule(_legs.Length, 4, dependency);
            
            // Schedule Two-Bone IK for leg bones
            JobHandle ikHandle = footHandle;
            if (_enableLegIK && _totalLegBones > 0)
            {
                // Use TwoBoneIK for proper knee direction
                var twoBoneJob = new TwoBoneIKJob
                {
                    ChainRanges = _legChainRanges,
                    RootPositions = _legRootPositions,
                    TargetPositions = _footPositions,  // Foot positions are the targets
                    PoleTargets = _legPoleTargets,      // Knee direction hints
                    BoneLengths = _legBoneLengths,
                    JointPositions = _legJointPositions
                };
                
                var ikSolveHandle = twoBoneJob.Schedule(_legs.Length, dependency: footHandle);
                
                // Convert positions to rotations using delta rotation
                var rotationJob = new PositionToRotationJob
                {
                    ChainRanges = _legChainRanges,
                    JointPositions = _legJointPositions,
                    OriginalPositions = _legOriginalPositions,
                    OriginalRotations = _legOriginalRotations,
                    Rotations = _legRotations
                };
                
                ikHandle = rotationJob.Schedule(_totalLegBones, 4, ikSolveHandle);
            }
            
            // Schedule body balance job
            var balanceJob = new BodyBalanceJob
            {
                FootPositions = _footPositions,
                FootGrounded = _footGrounded,
                Velocity = _velocity,
                GaitPhase = _gaitCycle.Phase,
                TargetHeight = _targetHeight,
                BobbingAmplitude = _bobbingAmplitude,
                MaxTiltAngle = 15f,
                MovementLeanAngle = _leanAngle,
                OutputPosition = _bodyPosition,
                OutputRotation = _bodyRotation
            };
            
            return balanceJob.Schedule(ikHandle);
        }
        
        public void Apply()
        {
            // Apply leg bone rotations from IK first (hip, knee only - not ankle)
            if (_enableLegIK && _totalLegBones > 0)
            {
                for (int i = 0; i < _legs.Length; i++)
                {
                    var range = _legChainRanges[i];
                    // Apply rotations to hip and knee only (first 2 bones)
                    for (int j = 0; j < math.min(2, range.y); j++)
                    {
                        int idx = range.x + j;
                        if (_allLegBones[idx] != null)
                        {
                            _allLegBones[idx].rotation = _legRotations[idx];
                        }
                    }
                }
            }
            
            // Apply foot positions and preserve original foot rotation
            for (int i = 0; i < _legs.Length; i++)
            {
                if (_legs[i].Effector != null)
                {
                    // Position the foot at the spring target
                    _legs[i].Effector.position = _footPositions[i];
                    
                    // Preserve the original foot orientation relative to the character
                    // This prevents the foot from rotating weirdly due to IK
                    if (_footBindRotations.IsCreated)
                    {
                        quaternion worldFootRot = math.mul((quaternion)transform.rotation, _footBindRotations[i]);
                        _legs[i].Effector.rotation = worldFootRot;
                    }
                }
            }
            
            // Apply arm swing
            if (_enableArmSwing && _arms != null && _arms.Length > 0 && _armBindRotations != null)
            {
                float movementSpeed = math.length(_velocity);
                float speedFactor = math.saturate(movementSpeed / (_sprintSpeed * 0.5f));  // Full swing at half sprint speed
                
                // Calculate swing angle based on gait cycle
                float swingPhase = _gaitCycle.Phase * 2 * math.PI * _armSwingSpeed;
                
                // Pre-calculate the rest offset rotation (brings arms from T-pose to natural position)
                quaternion restOffsetRot = quaternion.Euler(math.radians(_armRestOffset));
                
                for (int i = 0; i < _arms.Length; i++)
                {
                    if (_arms[i].Root == null) continue;
                    
                    // Arms swing opposite to their corresponding leg
                    // Left arm swings with right leg and vice versa
                    bool isLeftArm = _arms[i].Side == BodySide.Left || _arms[i].Side == BodySide.BackLeft || _arms[i].Side == BodySide.FrontLeft;
                    
                    // Flip the rest offset for right arm (mirror X and Z)
                    quaternion armRestOffset = isLeftArm ? restOffsetRot : 
                        quaternion.Euler(math.radians(new float3(_armRestOffset.x, -_armRestOffset.y, -_armRestOffset.z)));
                    
                    // Phase offset: left arm should move with right leg (180° offset)
                    float phaseOffset = isLeftArm ? 0f : math.PI;
                    
                    // Calculate swing angle
                    float swingAngle = math.sin(swingPhase + phaseOffset) * _armSwingAmplitude * speedFactor;
                    
                    // Create swing rotation based on selected axis
                    float3 swingEuler = float3.zero;
                    swingEuler[_armSwingAxis] = math.radians(swingAngle);
                    
                    // Add outward swing (roll) to prevent clipping
                    // We can use a small constant outward angle or scale it with movement
                    float outwardAngle = _armSwingOutward * speedFactor;
                    // For biped characters, outward is usually negative for left and positive for right (or vice versa depending on bone setup)
                    // But our rest offset already handles the basic orientation.
                    // Let's add a dynamic outward component that is strongest at the center of the swing
                    float dynamicOutward = math.cos(swingPhase + phaseOffset) * (_armSwingOutward * 0.5f) * speedFactor;
                    float totalOutward = math.radians(_armSwingOutward + dynamicOutward);
                    
                    // Z is usually the axis to move the arm away from the body in Mixamo/Unity standard
                    swingEuler.z += isLeftArm ? -totalOutward : totalOutward;
                    
                    quaternion swingRot = quaternion.Euler(swingEuler);
                    
                    // Combine: bind rotation + rest offset + swing
                    quaternion bindLocalRot = _armBindRotations[i];
                    quaternion finalLocalRot = math.mul(math.mul(armRestOffset, swingRot), bindLocalRot);
                    
                    _arms[i].Root.localRotation = finalLocalRot;
                }
            }
            
            // Note: Body position is available in _bodyPosition[0] for use by other systems
        }
        
        #endregion
        
        private void Awake()
        {
            _animator = GetComponent<ProceduralAnimator>();
        }
        
        private void Start()
        {
            if (_animator.IsAnalyzed)
            {
                Initialize();
            }
        }
        
        private void OnEnable()
        {
            if (_initialized)
            {
                AnimationJobManager.Instance?.Register(this);
                _needsUpdate = true;
            }
        }
        
        private void OnDisable()
        {
            _needsUpdate = false;
            AnimationJobManager.Instance?.Unregister(this);
        }
        
        private void OnDestroy()
        {
            Dispose();
        }
        
        private void Update()
        {
            // Initialize if analyzer completed
            if (!_initialized && _animator.IsAnalyzed)
            {
                Initialize();
            }
        }
        
        private void Initialize()
        {
            if (_initialized) return;
            
            var topology = _animator.Topology;
            _legs = topology.GetLegs();
            
            if (_legs.Length == 0)
            {
                Debug.LogWarning($"[ProceduralLocomotion] No legs found on {gameObject.name}");
                return;
            }
            
            int legCount = _legs.Length;
            
            // Allocate native arrays
            _footTargets = new NativeArray<float3>(legCount, Allocator.Persistent);
            _footPositions = new NativeArray<float3>(legCount, Allocator.Persistent);
            _footVelocities = new NativeArray<float3>(legCount, Allocator.Persistent);
            _footInSwing = new NativeArray<bool>(legCount, Allocator.Persistent);
            _footSwingProgress = new NativeArray<float>(legCount, Allocator.Persistent);
            _footSwingStart = new NativeArray<float3>(legCount, Allocator.Persistent);
            _footStepHeights = new NativeArray<float>(legCount, Allocator.Persistent);
            _footGrounded = new NativeArray<bool>(legCount, Allocator.Persistent);
            
            _bodyPosition = new NativeArray<float3>(1, Allocator.Persistent);
            _bodyRotation = new NativeArray<quaternion>(1, Allocator.Persistent);
            
            // Initialize with current positions
            _legPhases = new float[legCount];
            _wasInStance = new bool[legCount];
            _localStepOffsets = new float3[legCount];
            
            for (int i = 0; i < legCount; i++)
            {
                _legPhases[i] = _legs[i].GaitPhase;
                _wasInStance[i] = true;
                
                if (_legs[i].Effector != null)
                {
                    float3 worldFeet = (float3)_legs[i].Effector.position;
                    _footPositions[i] = worldFeet;
                    _footSwingStart[i] = worldFeet;
                    
                    // Capture local offset relative to character root
                    float3 localPos = transform.InverseTransformPoint(worldFeet);
                    _localStepOffsets[i] = new float3(localPos.x, 0, localPos.z);
                    
                    // Track ground height (use lowest foot Y)
                    if (i == 0 || worldFeet.y < _groundHeight)
                    {
                        _groundHeight = worldFeet.y;
                    }
                }
            }
            
            _springConfig = SpringCoefficients.Create(_footSpringFrequency, _footSpringDamping, 0f);
            
            // Initialize Two-Bone IK arrays for leg solving
            if (_enableLegIK)
            {
                InitializeLegIK(legCount);
            }
            
            // Auto-calculate step parameters based on leg length
            if (_autoCalculateStepParams && _legs.Length > 0)
            {
                float avgLegLength = 0f;
                foreach (var leg in _legs)
                {
                    avgLegLength += leg.TotalLength;
                }
                avgLegLength /= _legs.Length;
                
                // Max leg reach: ~90% of total leg length to avoid full extension
                _maxLegReach = avgLegLength * 0.9f;
                
                // Step length: ~25-35% of leg length for natural walking
                _stepLength = avgLegLength * 0.3f;
                
                // Step height: ~8-12% of lower leg length
                _stepHeight = avgLegLength * 0.1f;
                
                Debug.Log($"[ProceduralLocomotion] Auto-calculated: leg length={avgLegLength:F2}m, maxReach={_maxLegReach:F2}m, stepLength={_stepLength:F2}m, stepHeight={_stepHeight:F2}m");
            }
            else
            {
                // Default max reach if not auto-calculating
                _maxLegReach = 1.0f;
            }
            
            // Initialize gait cycle
            _gaitCycle = new GaitCycle
            {
                CycleDuration = _gaitDuration,
                StanceDutyFactor = _stanceRatio,
                StepHeight = _stepHeight
            };
            
            // Initialize arm swing
            if (_enableArmSwing)
            {
                _arms = topology.GetArms();
                if (_arms != null && _arms.Length > 0)
                {
                    _armBindRotations = new quaternion[_arms.Length];
                    for (int i = 0; i < _arms.Length; i++)
                    {
                        if (_arms[i].Root != null)
                        {
                            // Store LOCAL rotation relative to parent (shoulder)
                            _armBindRotations[i] = _arms[i].Root.localRotation;
                        }
                    }
                    Debug.Log($"[ProceduralLocomotion] Arm swing enabled with {_arms.Length} arms");
                }
            }
            
            // Initialize velocity inertialization
            _velocityInertializer = InertializationBlender.Create(0.1f);
            
            _initialized = true;
            _needsUpdate = true;
            
            AnimationJobManager.Instance?.Register(this);
            
            Debug.Log($"[ProceduralLocomotion] Initialized with {legCount} legs using GaitCycle + FABRIK IK");
        }
        
        private void InitializeLegIK(int legCount)
        {
            // Count total IK bones across all legs (excluding toes)
            _totalLegBones = 0;
            foreach (var leg in _legs)
            {
                _totalLegBones += leg.IKBoneCount;  // Use IKBoneCount instead of full Bones.Length
            }
            
            if (_totalLegBones == 0) return;
            
            _legJointPositions = new NativeArray<float3>(_totalLegBones, Allocator.Persistent);
            _legOriginalPositions = new NativeArray<float3>(_totalLegBones, Allocator.Persistent);
            _legBoneLengths = new NativeArray<float>(_totalLegBones, Allocator.Persistent);
            _legRotations = new NativeArray<quaternion>(_totalLegBones, Allocator.Persistent);
            _legOriginalRotations = new NativeArray<quaternion>(_totalLegBones, Allocator.Persistent);
            _legBindPositionsLocal = new NativeArray<float3>(_totalLegBones, Allocator.Persistent);
            _legBindRotationsLocal = new NativeArray<quaternion>(_totalLegBones, Allocator.Persistent);
            _legChainRanges = new NativeArray<int2>(legCount, Allocator.Persistent);
            _legRootPositions = new NativeArray<float3>(legCount, Allocator.Persistent);
            _legPoleTargets = new NativeArray<float3>(legCount, Allocator.Persistent);
            _footBindRotations = new NativeArray<quaternion>(legCount, Allocator.Persistent);
            
            // Build bone list and chain ranges (only IK bones, no toes)
            _allLegBones = new Transform[_totalLegBones];
            int boneIndex = 0;
            
            for (int i = 0; i < legCount; i++)
            {
                var leg = _legs[i];
                var ikBones = leg.GetIKBones();  // Get only Root → Effector bones
                int chainStart = boneIndex;
                int chainLength = ikBones.Length;
                
                _legChainRanges[i] = new int2(chainStart, chainLength);
                
                // Store original foot rotation (relative to character)
                if (leg.Effector != null)
                {
                    _footBindRotations[i] = math.mul(math.inverse((quaternion)transform.rotation), (quaternion)leg.Effector.rotation);
                }
                
                // Calculate initial pole target (in front of the knee)
                // The pole target should be in front of the leg to make the knee bend forward
                if (ikBones.Length >= 2)
                {
                    float3 hipPos = ikBones[0].position;
                    float3 anklePos = ikBones[ikBones.Length - 1].position;
                    float3 midpoint = (hipPos + anklePos) * 0.5f;
                    
                    // Pole target is in front of the character, at the knee height
                    float3 forward = transform.forward;
                    _legPoleTargets[i] = midpoint + forward * 0.5f;
                }
                
                for (int j = 0; j < ikBones.Length; j++)
                {
                    var bone = ikBones[j];
                    _allLegBones[boneIndex] = bone;
                    
                    if (bone != null)
                    {
                        // Store local bind pose relative to character root
                        _legBindPositionsLocal[boneIndex] = transform.InverseTransformPoint(bone.position);
                        _legBindRotationsLocal[boneIndex] = math.mul(math.inverse((quaternion)transform.rotation), (quaternion)bone.rotation);
                        
                        _legJointPositions[boneIndex] = bone.position;
                        _legOriginalPositions[boneIndex] = bone.position;
                        _legRotations[boneIndex] = bone.rotation;
                        _legOriginalRotations[boneIndex] = bone.rotation;
                        
                        // Calculate bone length (to next bone)
                        if (j < ikBones.Length - 1 && ikBones[j + 1] != null)
                        {
                            _legBoneLengths[boneIndex] = Vector3.Distance(bone.position, ikBones[j + 1].position);
                        }
                    }
                    
                    boneIndex++;
                }
            }
            
            _legTransformAccess = new TransformAccessArray(_allLegBones);
        }
        
        /// <summary>
        /// Sets up locomotion from a BodyTopology.
        /// Called by ProceduralAnimator.SetupLocomotion().
        /// </summary>
        public void SetupFromTopology(BodyTopology topology)
        {
            if (topology == null || topology.GetLegs().Length == 0)
            {
                Debug.LogWarning("[ProceduralLocomotion] Cannot setup: no topology or legs.");
                return;
            }
            
            // Force re-initialization with new topology
            if (_initialized)
            {
                Dispose();
            }
            
            _legs = topology.GetLegs();
            
            // Copy leg phases from topology
            int legCount = _legs.Length;
            _legPhases = new float[legCount];
            _wasInStance = new bool[legCount];
            
            for (int i = 0; i < legCount; i++)
            {
                _legPhases[i] = _legs[i].GaitPhase;
            }
            
            // Continue with standard initialization
            _footTargets = new NativeArray<float3>(legCount, Allocator.Persistent);
            _footPositions = new NativeArray<float3>(legCount, Allocator.Persistent);
            _footVelocities = new NativeArray<float3>(legCount, Allocator.Persistent);
            _footInSwing = new NativeArray<bool>(legCount, Allocator.Persistent);
            _footSwingProgress = new NativeArray<float>(legCount, Allocator.Persistent);
            _footSwingStart = new NativeArray<float3>(legCount, Allocator.Persistent);
            _footStepHeights = new NativeArray<float>(legCount, Allocator.Persistent);
            _footGrounded = new NativeArray<bool>(legCount, Allocator.Persistent);
            _bodyPosition = new NativeArray<float3>(1, Allocator.Persistent);
            _bodyRotation = new NativeArray<quaternion>(1, Allocator.Persistent);
            
            for (int i = 0; i < legCount; i++)
            {
                _wasInStance[i] = true;
                if (_legs[i].Effector != null)
                {
                    _footPositions[i] = _legs[i].Effector.position;
                    _footSwingStart[i] = _footPositions[i];
                }
            }
            
            _springConfig = SpringCoefficients.Create(_footSpringFrequency, _footSpringDamping, 0f);
            _initialized = true;
            _needsUpdate = true;
            
            AnimationJobManager.Instance?.Register(this);
            
            Debug.Log($"[ProceduralLocomotion] Setup from topology with {legCount} legs");
        }
        
        public void Dispose()
        {
            AnimationJobManager.Instance?.Unregister(this);
            
            if (_footTargets.IsCreated) _footTargets.Dispose();
            if (_footPositions.IsCreated) _footPositions.Dispose();
            if (_footVelocities.IsCreated) _footVelocities.Dispose();
            if (_footInSwing.IsCreated) _footInSwing.Dispose();
            if (_footSwingProgress.IsCreated) _footSwingProgress.Dispose();
            if (_footSwingStart.IsCreated) _footSwingStart.Dispose();
            if (_footStepHeights.IsCreated) _footStepHeights.Dispose();
            if (_footGrounded.IsCreated) _footGrounded.Dispose();
            if (_bodyPosition.IsCreated) _bodyPosition.Dispose();
            if (_bodyRotation.IsCreated) _bodyRotation.Dispose();
            
            // Dispose Two-Bone IK arrays
            if (_legJointPositions.IsCreated) _legJointPositions.Dispose();
            if (_legOriginalPositions.IsCreated) _legOriginalPositions.Dispose();
            if (_legBoneLengths.IsCreated) _legBoneLengths.Dispose();
            if (_legRotations.IsCreated) _legRotations.Dispose();
            if (_legOriginalRotations.IsCreated) _legOriginalRotations.Dispose();
            if (_legChainRanges.IsCreated) _legChainRanges.Dispose();
            if (_legRootPositions.IsCreated) _legRootPositions.Dispose();
            if (_legPoleTargets.IsCreated) _legPoleTargets.Dispose();
            if (_footBindRotations.IsCreated) _footBindRotations.Dispose();
            if (_legBindPositionsLocal.IsCreated) _legBindPositionsLocal.Dispose();
            if (_legBindRotationsLocal.IsCreated) _legBindRotationsLocal.Dispose();
            if (_legTransformAccess.isCreated) _legTransformAccess.Dispose();
            
            _initialized = false;
        }
        
        /// <summary>
        /// Gets the current foot positions.
        /// </summary>
        public float3[] GetFootPositions()
        {
            if (!_initialized || !_footPositions.IsCreated) return Array.Empty<float3>();
            return _footPositions.ToArray();
        }
        
        /// <summary>
        /// Gets the computed body position.
        /// </summary>
        public float3 GetBodyPosition()
        {
            return _bodyPosition.IsCreated && _bodyPosition.Length > 0 
                ? _bodyPosition[0] 
                : (float3)transform.position;
        }
        
        /// <summary>
        /// Gets the computed body rotation.
        /// </summary>
        public quaternion GetBodyRotation()
        {
            return _bodyRotation.IsCreated && _bodyRotation.Length > 0 
                ? _bodyRotation[0] 
                : (quaternion)transform.rotation;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_initialized || !_footPositions.IsCreated) return;
            
            for (int i = 0; i < _legs.Length; i++)
            {
                bool grounded = _footGrounded.IsCreated && _footGrounded[i];
                
                Gizmos.color = grounded ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(_footPositions[i], 0.03f);
                
                if (_footTargets.IsCreated)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(_footPositions[i], _footTargets[i]);
                }
            }
        }
#endif
    }
}
