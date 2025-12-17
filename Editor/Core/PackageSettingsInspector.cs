using UnityEditor;
using UnityEngine;

namespace Eraflo.UnityImportPackage.Editor
{
    /// <summary>
    /// Custom inspector for PackageSettings.
    /// </summary>
    [CustomEditor(typeof(PackageSettings))]
    public class PackageSettingsInspector : UnityEditor.Editor
    {
        private SerializedProperty _enableNetworking;
        private SerializedProperty _networkDebugMode;

        private void OnEnable()
        {
            _enableNetworking = serializedObject.FindProperty("_enableNetworking");
            _networkDebugMode = serializedObject.FindProperty("_networkDebugMode");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Unity Import Package Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Network section
            EditorGUILayout.LabelField("Network Events", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_enableNetworking, 
                    new GUIContent("Enable Networking", 
                        "Auto-instantiate NetworkEventManager singleton on game start"));

                if (_enableNetworking.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_networkDebugMode, 
                        new GUIContent("Debug Mode", 
                            "Log network event messages to console"));
                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(
                        "NetworkEventManager will be automatically instantiated with DontDestroyOnLoad when the game starts.", 
                        MessageType.Info);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
