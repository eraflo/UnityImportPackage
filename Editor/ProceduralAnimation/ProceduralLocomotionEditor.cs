using UnityEngine;
using UnityEditor;
using Eraflo.Catalyst.ProceduralAnimation.Components.Locomotion;

namespace Eraflo.Catalyst.ProceduralAnimation.Editor
{
    /// <summary>
    /// Custom inspector for ProceduralLocomotion component.
    /// Provides a cleaner UI with grouped properties and runtime info.
    /// </summary>
    [CustomEditor(typeof(ProceduralLocomotion))]
    [CanEditMultipleObjects]
    public class ProceduralLocomotionEditor : UnityEditor.Editor
    {
        // Serialized properties
        private SerializedProperty _movementInput;
        private SerializedProperty _speed;
        private SerializedProperty _gaitDuration;
        private SerializedProperty _stanceRatio;
        private SerializedProperty _stepHeight;
        private SerializedProperty _stepLength;
        private SerializedProperty _autoCalculateStepParams;
        
        // Speed adaptation
        private SerializedProperty _sprintSpeed;
        private SerializedProperty _sprintStepMultiplier;
        private SerializedProperty _sprintHeightMultiplier;
        private SerializedProperty _restStanceWidth;
        
        // Arm swing
        private SerializedProperty _enableArmSwing;
        private SerializedProperty _armSwingAmplitude;
        private SerializedProperty _armSwingSpeed;
        private SerializedProperty _armRestOffset;
        private SerializedProperty _armSwingAxis;
        private SerializedProperty _armSwingOutward;
        
        private SerializedProperty _targetHeight;
        private SerializedProperty _bobbingAmplitude;
        private SerializedProperty _leanAngle;
        private SerializedProperty _footSpringFrequency;
        private SerializedProperty _footSpringDamping;
        private SerializedProperty _groundMask;
        private SerializedProperty _raycastDistance;
        private SerializedProperty _raycastHeightOffset;
        private SerializedProperty _footHeightOffset;
        private SerializedProperty _enableLegIK;
        private SerializedProperty _ikIterations;
        
        // Foldout states
        private bool _movementFoldout = true;
        private bool _gaitFoldout = true;
        private bool _speedAdaptFoldout = true;
        private bool _armSwingFoldout = true;
        private bool _balanceFoldout = true;
        private bool _springFoldout = false;
        private bool _groundFoldout = false;
        private bool _ikFoldout = true;
        private bool _debugFoldout = false;
        
        private void OnEnable()
        {
            _movementInput = serializedObject.FindProperty("_movementInput");
            _speed = serializedObject.FindProperty("_speed");
            _gaitDuration = serializedObject.FindProperty("_gaitDuration");
            _stanceRatio = serializedObject.FindProperty("_stanceRatio");
            _stepHeight = serializedObject.FindProperty("_stepHeight");
            _stepLength = serializedObject.FindProperty("_stepLength");
            _autoCalculateStepParams = serializedObject.FindProperty("_autoCalculateStepParams");
            
            // Speed adaptation
            _sprintSpeed = serializedObject.FindProperty("_sprintSpeed");
            _sprintStepMultiplier = serializedObject.FindProperty("_sprintStepMultiplier");
            _sprintHeightMultiplier = serializedObject.FindProperty("_sprintHeightMultiplier");
            _restStanceWidth = serializedObject.FindProperty("_restStanceWidth");
            
            // Arm swing
            _enableArmSwing = serializedObject.FindProperty("_enableArmSwing");
            _armSwingAmplitude = serializedObject.FindProperty("_armSwingAmplitude");
            _armSwingSpeed = serializedObject.FindProperty("_armSwingSpeed");
            _armRestOffset = serializedObject.FindProperty("_armRestOffset");
            _armSwingAxis = serializedObject.FindProperty("_armSwingAxis");
            _armSwingOutward = serializedObject.FindProperty("_armSwingOutward");
            
            _targetHeight = serializedObject.FindProperty("_targetHeight");
            _bobbingAmplitude = serializedObject.FindProperty("_bobbingAmplitude");
            _leanAngle = serializedObject.FindProperty("_leanAngle");
            _footSpringFrequency = serializedObject.FindProperty("_footSpringFrequency");
            _footSpringDamping = serializedObject.FindProperty("_footSpringDamping");
            _groundMask = serializedObject.FindProperty("_groundMask");
            _raycastDistance = serializedObject.FindProperty("_raycastDistance");
            _raycastHeightOffset = serializedObject.FindProperty("_raycastHeightOffset");
            _footHeightOffset = serializedObject.FindProperty("_footHeightOffset");
            _enableLegIK = serializedObject.FindProperty("_enableLegIK");
            _ikIterations = serializedObject.FindProperty("_ikIterations");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Fallback to default inspector if properties not found
            if (_movementInput == null || _speed == null)
            {
                DrawDefaultInspector();
                return;
            }
            
            // Header
            EditorGUILayout.Space(5);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("🦵 Procedural Locomotion", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.Space(5);
            
            // Movement Section
            _movementFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_movementFoldout, "Movement");
            if (_movementFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_movementInput, new GUIContent("Input Direction"));
                EditorGUILayout.PropertyField(_speed, new GUIContent("Speed (m/s)"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // Gait Section
            _gaitFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_gaitFoldout, "Gait Cycle");
            if (_gaitFoldout)
            {
                EditorGUI.indentLevel++;
                
                if (_autoCalculateStepParams != null)
                {
                    EditorGUILayout.PropertyField(_autoCalculateStepParams, new GUIContent("Auto-Calculate", "Automatically calculate step parameters based on leg length"));
                    
                    EditorGUILayout.Space(3);
                    
                    // Show step params as read-only if auto-calculating
                    if (_autoCalculateStepParams.boolValue)
                    {
                        GUI.enabled = false;
                        if (_stepLength != null) EditorGUILayout.PropertyField(_stepLength, new GUIContent("Step Length (auto)"));
                        if (_stepHeight != null) EditorGUILayout.PropertyField(_stepHeight, new GUIContent("Step Height (auto)"));
                        GUI.enabled = true;
                        
                        EditorGUILayout.HelpBox("Step parameters are calculated from leg length. Disable Auto-Calculate to edit manually.", MessageType.Info);
                    }
                    else
                    {
                        if (_stepLength != null) EditorGUILayout.PropertyField(_stepLength, new GUIContent("Step Length"));
                        if (_stepHeight != null) EditorGUILayout.PropertyField(_stepHeight, new GUIContent("Step Height"));
                    }
                }
                else
                {
                    if (_stepLength != null) EditorGUILayout.PropertyField(_stepLength, new GUIContent("Step Length"));
                    if (_stepHeight != null) EditorGUILayout.PropertyField(_stepHeight, new GUIContent("Step Height"));
                }
                
                EditorGUILayout.Space(3);
                if (_gaitDuration != null) EditorGUILayout.PropertyField(_gaitDuration, new GUIContent("Cycle Duration (s)"));
                if (_stanceRatio != null) EditorGUILayout.PropertyField(_stanceRatio, new GUIContent("Stance Ratio"));
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // Speed Adaptation Section
            _speedAdaptFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_speedAdaptFoldout, "Speed Adaptation");
            if (_speedAdaptFoldout)
            {
                EditorGUI.indentLevel++;
                if (_sprintSpeed != null) EditorGUILayout.PropertyField(_sprintSpeed, new GUIContent("Sprint Speed (m/s)"));
                if (_sprintStepMultiplier != null) EditorGUILayout.PropertyField(_sprintStepMultiplier, new GUIContent("Sprint Step Multiplier"));
                if (_sprintHeightMultiplier != null) EditorGUILayout.PropertyField(_sprintHeightMultiplier, new GUIContent("Sprint Height Multiplier"));
                if (_restStanceWidth != null) EditorGUILayout.PropertyField(_restStanceWidth, new GUIContent("Rest Stance Width"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // Arm Swing Section
            _armSwingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_armSwingFoldout, "Arm Swing");
            if (_armSwingFoldout)
            {
                EditorGUI.indentLevel++;
                if (_enableArmSwing != null)
                {
                    EditorGUILayout.PropertyField(_enableArmSwing, new GUIContent("Enable Arm Swing"));
                    if (_enableArmSwing.boolValue)
                    {
                        if (_armSwingAmplitude != null) EditorGUILayout.PropertyField(_armSwingAmplitude, new GUIContent("Swing Amplitude (deg)"));
                        if (_armSwingSpeed != null) EditorGUILayout.PropertyField(_armSwingSpeed, new GUIContent("Swing Speed Multiplier"));
                        
                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField("Pose Correction", EditorStyles.boldLabel);
                        if (_armRestOffset != null) EditorGUILayout.PropertyField(_armRestOffset, new GUIContent("Rest Offset", "Rotation to bring arms from T-pose to natural sitting position (X=Pitch, Y=Yaw, Z=Roll)"));
                        
                        EditorGUILayout.Space(2);
                        if (_armSwingAxis != null) 
                        {
                            string[] axisNames = { "X (Pitch)", "Y (Yaw)", "Z (Roll)" };
                            _armSwingAxis.intValue = EditorGUILayout.Popup("Swing Axis", _armSwingAxis.intValue, axisNames);
                        }
                        
                        if (_armSwingOutward != null) EditorGUILayout.PropertyField(_armSwingOutward, new GUIContent("Outward Swing (deg)", "Additional outward angle to prevent arms from clipping through the body."));
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Arm swing properties not found. Re-setup the component.", MessageType.Warning);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // Balance Section
            _balanceFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_balanceFoldout, "Balance");
            if (_balanceFoldout)
            {
                EditorGUI.indentLevel++;
                if (_targetHeight != null) EditorGUILayout.PropertyField(_targetHeight, new GUIContent("Target Height"));
                if (_bobbingAmplitude != null) EditorGUILayout.PropertyField(_bobbingAmplitude, new GUIContent("Bobbing"));
                if (_leanAngle != null) EditorGUILayout.PropertyField(_leanAngle, new GUIContent("Lean Angle"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // IK Section
            _ikFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_ikFoldout, "Inverse Kinematics");
            if (_ikFoldout)
            {
                EditorGUI.indentLevel++;
                if (_enableLegIK != null)
                {
                    EditorGUILayout.PropertyField(_enableLegIK, new GUIContent("Enable Leg IK"));
                    if (_enableLegIK.boolValue && _ikIterations != null)
                    {
                        EditorGUILayout.PropertyField(_ikIterations, new GUIContent("Solver Iterations"));
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // Spring Settings (collapsed by default)
            _springFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_springFoldout, "Spring Settings");
            if (_springFoldout)
            {
                EditorGUI.indentLevel++;
                if (_footSpringFrequency != null) EditorGUILayout.PropertyField(_footSpringFrequency, new GUIContent("Frequency"));
                if (_footSpringDamping != null) EditorGUILayout.PropertyField(_footSpringDamping, new GUIContent("Damping"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // Ground Detection (collapsed by default)
            _groundFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_groundFoldout, "Ground Detection");
            if (_groundFoldout)
            {
                EditorGUI.indentLevel++;
                if (_groundMask != null) EditorGUILayout.PropertyField(_groundMask, new GUIContent("Ground Layer"));
                if (_raycastDistance != null) EditorGUILayout.PropertyField(_raycastDistance, new GUIContent("Raycast Distance"));
                if (_raycastHeightOffset != null) EditorGUILayout.PropertyField(_raycastHeightOffset, new GUIContent("Raycast Offset"));
                if (_footHeightOffset != null) EditorGUILayout.PropertyField(_footHeightOffset, new GUIContent("Foot Height Offset"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // Debug Info (runtime only)
            if (Application.isPlaying && targets.Length == 1)
            {
                _debugFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_debugFoldout, "Runtime Info");
                if (_debugFoldout)
                {
                    EditorGUI.indentLevel++;
                    
                    GUI.enabled = false;
                    
                    var locomotion = (ProceduralLocomotion)target;
                    var footPositions = locomotion.GetFootPositions();
                    if (footPositions != null)
                    {
                        EditorGUILayout.LabelField("Feet", EditorStyles.boldLabel);
                        for (int i = 0; i < footPositions.Length; i++)
                        {
                            EditorGUILayout.Vector3Field($"Foot {i}", footPositions[i]);
                        }
                    }
                    
                    GUI.enabled = true;
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            
            serializedObject.ApplyModifiedProperties();
            
            // Repaint during play mode for live updates
            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}
