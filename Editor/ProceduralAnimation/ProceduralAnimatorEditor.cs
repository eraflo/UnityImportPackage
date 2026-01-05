using UnityEngine;
using UnityEditor;
using Eraflo.Catalyst.ProceduralAnimation.Perception;

namespace Eraflo.Catalyst.ProceduralAnimation.Editor
{
    /// <summary>
    /// Custom editor for ProceduralAnimator.
    /// Provides easy-to-use buttons for skeleton analysis and setup.
    /// </summary>
    [CustomEditor(typeof(ProceduralAnimator))]
    [CanEditMultipleObjects]
    public class ProceduralAnimatorEditor : UnityEditor.Editor
    {
        private ProceduralAnimator _animator;
        private bool _showSetupOptions = true;
        private bool _showDebugInfo = false;
        
        // Setup toggles
        private bool _enableLocomotion = true;
        private bool _enableIK = true;
        private bool _enableSpines = true;
        
        private void OnEnable()
        {
            _animator = (ProceduralAnimator)target;
        }
        
        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            
            // === ANALYSIS SECTION ===
            EditorGUILayout.LabelField("Skeleton Analysis", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("Analyze Skeleton", GUILayout.Height(30)))
            {
                Undo.RecordObject(_animator, "Analyze Skeleton");
                _animator.AnalyzeSkeleton();
                EditorUtility.SetDirty(_animator);
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            // Show analysis results
            if (_animator.Topology != null && _animator.Topology.AllBones != null)
            {
                EditorGUILayout.HelpBox(
                    $"Found: {_animator.Topology.AllBones.Count} bones, " +
                    $"{_animator.Topology.Limbs?.Count ?? 0} limbs, " +
                    $"{_animator.Topology.Spines?.Count ?? 0} spines",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Click 'Analyze Skeleton' to detect bones and limbs.",
                    MessageType.Warning);
            }
            
            EditorGUILayout.Space(10);
            
            // === SETUP SECTION ===
            _showSetupOptions = EditorGUILayout.Foldout(_showSetupOptions, "Quick Setup", true, EditorStyles.foldoutHeader);
            
            if (_showSetupOptions)
            {
                EditorGUI.indentLevel++;
                
                _enableLocomotion = EditorGUILayout.Toggle("Enable Locomotion", _enableLocomotion);
                _enableIK = EditorGUILayout.Toggle("Enable Arm IK", _enableIK);
                _enableSpines = EditorGUILayout.Toggle("Enable Secondary Spines", _enableSpines);
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginHorizontal();
                
                GUI.backgroundColor = new Color(0.3f, 0.6f, 1f);
                if (GUILayout.Button("Setup All", GUILayout.Height(35)))
                {
                    SetupAll();
                }
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                // Individual setup buttons
                EditorGUILayout.BeginHorizontal();
                
                GUI.enabled = _animator.Topology != null;
                
                if (GUILayout.Button("Setup Locomotion"))
                {
                    Undo.RecordObject(_animator.gameObject, "Setup Locomotion");
                    _animator.SetupLocomotion();
                    EditorUtility.SetDirty(_animator);
                }
                
                if (GUILayout.Button("Setup Arm IK"))
                {
                    Undo.RecordObject(_animator.gameObject, "Setup Arm IK");
                    _animator.SetupArmIK();
                    EditorUtility.SetDirty(_animator);
                }
                
                if (GUILayout.Button("Setup Spines"))
                {
                    Undo.RecordObject(_animator.gameObject, "Setup Spines");
                    _animator.SetupSecondarySpines();
                    EditorUtility.SetDirty(_animator);
                }
                
                GUI.enabled = true;
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                // Remove All button
                GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("Remove All Components", GUILayout.Height(25)))
                {
                    if (EditorUtility.DisplayDialog(
                        "Remove All Procedural Animation Components",
                        "This will remove all procedural animation components (Locomotion, IK, Spines, Controller) from this GameObject. Are you sure?",
                        "Remove All",
                        "Cancel"))
                    {
                        RemoveAllComponents();
                    }
                }
                GUI.backgroundColor = Color.white;
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // === DEBUG SECTION ===
            _showDebugInfo = EditorGUILayout.Foldout(_showDebugInfo, "Debug Info", true, EditorStyles.foldoutHeader);
            
            if (_showDebugInfo && _animator.Topology != null)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Detected Structure", EditorStyles.boldLabel);
                
                // Limbs
                if (_animator.Topology.Limbs != null)
                {
                    foreach (var limb in _animator.Topology.Limbs)
                    {
                        string icon = limb.Type == LimbType.Leg ? "🦿" : "💪";
                        EditorGUILayout.LabelField($"{icon} {limb.Name}", $"{limb.Bones?.Length ?? 0} bones");
                    }
                }
                
                // Spines
                if (_animator.Topology.Spines != null)
                {
                    foreach (var spine in _animator.Topology.Spines)
                    {
                        string icon = spine.Type == SpineType.Tail ? "🦎" : "🦴";
                        EditorGUILayout.LabelField($"{icon} {spine.Type}", $"{spine.Bones?.Length ?? 0} bones");
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void SetupAll()
        {
            // Analyze first if not done
            if (_animator.Topology == null || _animator.Topology.AllBones == null)
            {
                Undo.RecordObject(_animator, "Analyze Skeleton");
                _animator.AnalyzeSkeleton();
            }
            
            Undo.RecordObject(_animator.gameObject, "Setup All Procedural Animation");
            _animator.SetupAll(_enableLocomotion, _enableIK, _enableSpines);
            EditorUtility.SetDirty(_animator);
            
            Debug.Log($"[ProceduralAnimator] Setup complete! " +
                      $"Locomotion: {_enableLocomotion}, IK: {_enableIK}, Spines: {_enableSpines}");
        }
        
        private void RemoveAllComponents()
        {
            Undo.RecordObject(_animator.gameObject, "Remove All Procedural Animation");
            
            // Remove all procedural animation components
            var locomotion = _animator.GetComponent<Components.Locomotion.ProceduralLocomotion>();
            var controller = _animator.GetComponent<Components.SimpleCharacterController>();
            var limbIKs = _animator.GetComponents<Components.IK.LimbIK>();
            var spines = _animator.GetComponents<Components.Locomotion.VerletSpine>();
            
            int count = 0;
            
            if (locomotion != null) { DestroyImmediate(locomotion); count++; }
            if (controller != null) { DestroyImmediate(controller); count++; }
            
            foreach (var ik in limbIKs)
            {
                if (ik != null) { DestroyImmediate(ik); count++; }
            }
            
            foreach (var spine in spines)
            {
                if (spine != null) { DestroyImmediate(spine); count++; }
            }
            
            EditorUtility.SetDirty(_animator);
            Debug.Log($"[ProceduralAnimator] Removed {count} components.");
        }
    }
}
