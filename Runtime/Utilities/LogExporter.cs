using System;
using System.Text;
using System.IO;
using UnityEngine;
using Eraflo.Catalyst;

namespace Eraflo.Catalyst.Utilities
{
    /// <summary>
    /// Utility service that captures Unity logs in memory and allows on-demand export to a file.
    /// </summary>
    [Service(Priority = 100)]
    public class LogExporter : IGameService
    {
        private const int MaxLogLines = 5000;
        private readonly StringBuilder _logBuffer = new StringBuilder();
        private int _lineCount;
        private readonly object _lock = new object();

        #region IGameService

        void IGameService.Initialize()
        {
            Application.logMessageReceivedThreaded += OnLogReceived;
        }

        void IGameService.Shutdown()
        {
            Application.logMessageReceivedThreaded -= OnLogReceived;
        }

        #endregion

        private void OnLogReceived(string condition, string stackTrace, LogType type)
        {
            lock (_lock)
            {
                if (_lineCount >= MaxLogLines)
                {
                    // For simplicity, we just stop adding or clear oldest. 
                    // Let's just keep the last 5000 lines by clearing periodically or similar.
                    // Simplified: Reset if too large to avoid huge memory spikes for now.
                    _logBuffer.Clear();
                    _lineCount = 0;
                    _logBuffer.AppendLine("--- Log Buffer Reset (Max Capacity Reached) ---");
                }

                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                string logLine = $"[{timestamp}] [{type}] {condition}";
                
                _logBuffer.AppendLine(logLine);
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                {
                    _logBuffer.AppendLine(stackTrace);
                }
                
                _lineCount++;
            }
        }

        /// <summary>
        /// Exports the current log buffer to a file in the PersistentDataPath.
        /// </summary>
        /// <returns>The path to the exported file.</returns>
        public string Export()
        {
            string folderPath = Path.Combine(Application.persistentDataPath, "Logs");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = Path.Combine(folderPath, fileName);

            lock (_lock)
            {
                File.WriteAllText(filePath, _logBuffer.ToString());
            }

            Debug.Log($"[LogExporter] Logs exported to: {filePath}");
            return filePath;
        }

        /// <summary>
        /// Clears the current log buffer.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _logBuffer.Clear();
                _lineCount = 0;
            }
        }
    }
}
