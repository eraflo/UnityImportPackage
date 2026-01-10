using UnityEditor;
using UnityEngine;
using Eraflo.Catalyst;
using Eraflo.Catalyst.Utilities;

namespace Eraflo.Catalyst.Editor.Utilities
{
    public static class LogExporterEditor
    {
        [MenuItem("Tools/Catalyst/Export Logs", priority = 100)]
        public static void ExportLogs()
        {
            // Ensure we have the service locator initialized (usually it is in editor too but just in case)
            var exporter = App.Get<LogExporter>();
            if (exporter == null)
            {
                Debug.LogError("[LogExporter] Could not find LogExporter service. Make sure the Service Locator is active.");
                return;
            }

            string path = exporter.Export();
            if (string.IsNullOrEmpty(path)) return;

            // Show in explorer
            EditorUtility.RevealInFinder(path);
            
            Debug.Log($"[LogExporter] Success! Logs exported to {path}");
        }
    }
}
