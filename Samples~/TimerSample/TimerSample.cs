using UnityEngine;
using Eraflo.Catalyst.Timers;
using Eraflo.Catalyst.EasingSystem;

namespace Eraflo.Catalyst.Samples.Timers
{
    /// <summary>
    /// Sample demonstrating the Timer system.
    /// Attach to any GameObject in the scene.
    /// </summary>
    public class TimerSample : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float countdownDuration = 5f;
        [SerializeField] private float delayDuration = 2f;
        [SerializeField] private float repeatInterval = 1f;

        private TimerHandle _countdownHandle;
        private TimerHandle _stopwatchHandle;
        private TimerHandle _repeatingHandle;

        private float _countdownProgress;
        private float _stopwatchTime;
        private int _repeatCount;

        private void Start()
        {
            Debug.Log("[Timer Sample] Started. Use UI buttons to test timers.");
        }

        private void OnDestroy()
        {
            // Clean up timers
            var timer = App.Get<Timer>();
            if (timer == null) return;

            if (_countdownHandle.IsValid) timer.CancelTimer(_countdownHandle);
            if (_stopwatchHandle.IsValid) timer.CancelTimer(_stopwatchHandle);
            if (_repeatingHandle.IsValid) timer.CancelTimer(_repeatingHandle);
        }

        private void Update()
        {
            var timer = App.Get<Timer>();
            // Update displays
            if (_countdownHandle.IsValid && timer.IsRunning(_countdownHandle))
            {
                _countdownProgress = timer.GetEasedProgress(_countdownHandle, EasingType.QuadOut);
            }

            if (_stopwatchHandle.IsValid && timer.IsRunning(_stopwatchHandle))
            {
                _stopwatchTime = timer.GetCurrentTime(_stopwatchHandle);
            }
        }

        public void StartCountdown()
        {
            var timer = App.Get<Timer>();
            if (_countdownHandle.IsValid) timer.CancelTimer(_countdownHandle);

            _countdownHandle = timer.CreateTimer<CountdownTimer>(countdownDuration);
            timer.On<OnTick, float>(_countdownHandle, (dt) => _countdownProgress = timer.GetProgress(_countdownHandle));
            timer.On<OnComplete>(_countdownHandle, () => Debug.Log("<color=green>[COUNTDOWN]</color> Completed!"));

            Debug.Log($"[COUNTDOWN] Started {countdownDuration}s countdown");
        }

        public void StartStopwatch()
        {
            var timer = App.Get<Timer>();
            if (_stopwatchHandle.IsValid) timer.CancelTimer(_stopwatchHandle);

            _stopwatchHandle = timer.CreateTimer<StopwatchTimer>(0f);
            Debug.Log("[STOPWATCH] Started");
        }

        public void StopStopwatch()
        {
            var timer = App.Get<Timer>();
            if (_stopwatchHandle.IsValid)
            {
                timer.Pause(_stopwatchHandle);
                Debug.Log($"[STOPWATCH] Stopped at {_stopwatchTime:F2}s");
            }
        }

        public void StartRepeating()
        {
            var timer = App.Get<Timer>();
            if (_repeatingHandle.IsValid) timer.CancelTimer(_repeatingHandle);

            _repeatCount = 0;
            _repeatingHandle = timer.CreateTimer<RepeatingTimer>(repeatInterval);
            timer.On<OnRepeat, int>(_repeatingHandle, (count) =>
            {
                _repeatCount = count;
                Debug.Log($"<color=cyan>[REPEAT]</color> Tick #{count}");
            });

            Debug.Log($"[REPEAT] Started repeating every {repeatInterval}s");
        }

        public void SimpleDelay()
        {
            App.Get<Timer>().CreateDelay(delayDuration, () => Debug.Log($"<color=yellow>[DELAY]</color> Fired after {delayDuration}s!"));
            Debug.Log($"[DELAY] Scheduled for {delayDuration}s");
        }

        private void OnGUI()
        {
            var timer = App.Get<Timer>();
            GUILayout.BeginArea(new Rect(10, 10, 300, 350));
            GUILayout.Box("Timer Sample");

            // Countdown
            GUILayout.Label($"Countdown: {_countdownProgress:P0}");
            if (GUILayout.Button("Start Countdown")) StartCountdown();

            GUILayout.Space(10);

            // Stopwatch
            GUILayout.Label($"Stopwatch: {_stopwatchTime:F2}s");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start")) StartStopwatch();
            if (GUILayout.Button("Stop")) StopStopwatch();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Repeating
            GUILayout.Label($"Repeat Count: {_repeatCount}");
            if (GUILayout.Button("Start Repeating")) StartRepeating();

            GUILayout.Space(10);

            // Simple Delay
            if (GUILayout.Button("Simple Delay")) SimpleDelay();

            GUILayout.Space(10);

            // Metrics
            var m = timer.Metrics;
            GUILayout.Label($"Active: {timer.Count} | Created: {m.TotalCreated}");

            GUILayout.EndArea();
        }
    }
}
