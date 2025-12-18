using UnityEditor;
using UnityEngine;
using Eraflo.UnityImportPackage.Timers;
using System.Collections.Generic;

namespace Eraflo.UnityImportPackage.Editor.Debugging
{
    /// <summary>
    /// Editor window to monitor and debug active timers.
    /// Open via Tools > Unity Import Package > Timer Debugger.
    /// </summary>
    public class TimerDebuggerWindow : EditorWindow
    {
        private Vector2 _scrollPos;
        private List<Timer> _timers = new List<Timer>();
        private bool _autoRefresh = true;
        private double _lastRefreshTime;
        private const double REFRESH_INTERVAL = 0.1; // 100ms

        [MenuItem("Tools/Unity Import Package/Timer Debugger")]
        public static void ShowWindow()
        {
            var window = GetWindow<TimerDebuggerWindow>("Timer Debugger");
            window.minSize = new Vector2(300, 200);
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (!_autoRefresh || !Application.isPlaying) return;
            
            if (EditorApplication.timeSinceStartup - _lastRefreshTime > REFRESH_INTERVAL)
            {
                _lastRefreshTime = EditorApplication.timeSinceStartup;
                Repaint();
            }
        }

        private void OnGUI()
        {
            DrawToolbar();
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see active timers.", MessageType.Info);
                return;
            }

            _timers = TimerManager.GetAllTimers();

            DrawStats();
            DrawTimerList();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            _autoRefresh = GUILayout.Toggle(_autoRefresh, "Auto Refresh", EditorStyles.toolbarButton);
            
            GUILayout.FlexibleSpace();
            
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Pause All", EditorStyles.toolbarButton))
                {
                    foreach (var t in TimerManager.GetAllTimers()) t.Pause();
                }
                if (GUILayout.Button("Resume All", EditorStyles.toolbarButton))
                {
                    foreach (var t in TimerManager.GetAllTimers()) t.Resume();
                }
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
                {
                    TimerManager.Clear();
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStats()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Active Timers: {_timers.Count}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Pooled: {TimerPool.TotalPooledCount}");
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTimerList()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            foreach (var timer in _timers)
            {
                if (timer == null) continue;
                DrawTimerEntry(timer);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawTimerEntry(Timer timer)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Row 1: Icon + Type + State
            EditorGUILayout.BeginHorizontal();
            
            // Status icon
            string icon = timer.IsRunning ? "▶" : (timer.IsFinished ? "⏹" : "⏸");
            var iconStyle = new GUIStyle(EditorStyles.label) { 
                fontSize = 14,
                normal = { textColor = timer.IsRunning ? Color.green : (timer.IsFinished ? Color.gray : Color.yellow) }
            };
            GUILayout.Label(icon, iconStyle, GUILayout.Width(20));
            
            // Type name
            EditorGUILayout.LabelField(timer.GetType().Name, EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            // Time scale badge
            if (Mathf.Abs(timer.TimeScale - 1f) > 0.01f)
            {
                var badge = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.cyan } };
                GUILayout.Label($"x{timer.TimeScale:F1}", badge);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Row 2: Progress bar
            var rect = EditorGUILayout.GetControlRect(false, 16);
            EditorGUI.ProgressBar(rect, timer.Progress, $"{timer.CurrentTime:F2}s / {timer.Progress:P0}");
            
            EditorGUILayout.EndVertical();
        }
    }
}
