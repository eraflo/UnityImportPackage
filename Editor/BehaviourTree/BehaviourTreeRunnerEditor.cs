using UnityEngine;
using UnityEditor;
using Eraflo.UnityImportPackage.BehaviourTree;

namespace Eraflo.UnityImportPackage.Editor.BehaviourTree
{
    /// <summary>
    /// Custom inspector for BehaviourTreeRunner MonoBehaviour.
    /// Shows runtime tree state during play mode.
    /// </summary>
    [CustomEditor(typeof(BehaviourTreeRunner))]
    public class BehaviourTreeRunnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var runner = target as BehaviourTreeRunner;
            
            // Draw default inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            
            // Runtime info (play mode only)
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Runtime State", EditorStyles.boldLabel);
                
                var state = runner.TreeState;
                GUI.color = state switch
                {
                    NodeState.Running => Color.yellow,
                    NodeState.Success => Color.green,
                    NodeState.Failure => Color.red,
                    _ => Color.white
                };
                EditorGUILayout.LabelField($"Tree State: {state}");
                GUI.color = Color.white;
                
                EditorGUILayout.Space();
                
                // Blackboard viewer
                if (runner.Blackboard != null)
                {
                    EditorGUILayout.LabelField("Blackboard", EditorStyles.boldLabel);
                    
                    var keys = runner.Blackboard.GetAllKeys();
                    if (keys.Length == 0)
                    {
                        EditorGUILayout.LabelField("(empty)");
                    }
                    else
                    {
                        EditorGUI.indentLevel++;
                        foreach (var key in keys)
                        {
                            EditorGUILayout.LabelField($"{key}: (value)");
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                
                EditorGUILayout.Space();
                
                // Controls
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Manual Tick"))
                {
                    runner.Tick();
                }
                
                if (GUILayout.Button("Reset Tree"))
                {
                    runner.ResetTree();
                }
                
                EditorGUILayout.EndHorizontal();
                
                // Force repaint for live updates
                Repaint();
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see runtime state.", MessageType.Info);
            }
        }
    }
}
