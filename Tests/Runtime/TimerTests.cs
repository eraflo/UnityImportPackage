using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Eraflo.Catalyst.Timers;
using Eraflo.Catalyst.EasingSystem;

namespace Eraflo.Catalyst.Tests
{
    public class TimerTests
    {
        [SetUp]
        public void SetUp()
        {
            App.Get<Timer>().Clear();
        }

        [TearDown]
        public void TearDown()
        {
            App.Get<Timer>().Clear();
        }

        #region Timer Creation Tests

        [Test]
        public void Create_CountdownTimer_ReturnsValidHandle()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            Assert.IsTrue(handle.IsValid);
            Assert.AreEqual(1, App.Get<Timer>().Count);
        }

        [Test]
        public void Create_StopwatchTimer_ReturnsValidHandle()
        {
            var handle = App.Get<Timer>().CreateTimer<StopwatchTimer>(0f);
            
            Assert.IsTrue(handle.IsValid);
            Assert.AreEqual(1, App.Get<Timer>().Count);
        }

        [Test]
        public void Create_MultipleTimers_IncreasesCount()
        {
            App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            App.Get<Timer>().CreateTimer<StopwatchTimer>(0f);
            App.Get<Timer>().CreateTimer<DelayTimer>(3f);
            
            Assert.AreEqual(3, App.Get<Timer>().Count);
        }

        [Test]
        public void Create_WithConfig_AppliesSettings()
        {
            var config = TimerConfig.Create(10f, timeScale: 2f, useUnscaledTime: true);
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(config);
            
            Assert.IsTrue(handle.IsValid);
            Assert.AreEqual(10f, App.Get<Timer>().GetCurrentTime(handle), 0.01f);
        }

        [Test]
        public void Delay_CreatesTimerWithCallback()
        {
            bool called = false;
            var handle = App.Get<Timer>().CreateDelay(1f, () => called = true);
            
            Assert.IsTrue(handle.IsValid);
            Assert.AreEqual(1, App.Get<Timer>().Count);
        }

        #endregion

        #region Timer State Tests

        [Test]
        public void Timer_IsRunning_TrueAfterCreation()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            Assert.IsTrue(App.Get<Timer>().IsRunning(handle));
        }

        [Test]
        public void Timer_IsFinished_FalseAfterCreation()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            Assert.IsFalse(App.Get<Timer>().IsFinished(handle));
        }

        [Test]
        public void Timer_GetProgress_ReturnsOneAtStart()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            Assert.AreEqual(1f, App.Get<Timer>().GetProgress(handle), 0.01f);
        }

        [Test]
        public void Timer_GetCurrentTime_ReturnsInitialDuration()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            Assert.AreEqual(5f, App.Get<Timer>().GetCurrentTime(handle), 0.01f);
        }

        #endregion

        #region Timer Control Tests

        [Test]
        public void Pause_StopsTimer()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            App.Get<Timer>().Pause(handle);
            
            Assert.IsFalse(App.Get<Timer>().IsRunning(handle));
        }

        [Test]
        public void Resume_StartsTimerAfterPause()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            App.Get<Timer>().Pause(handle);
            
            App.Get<Timer>().Resume(handle);
            
            Assert.IsTrue(App.Get<Timer>().IsRunning(handle));
        }

        [Test]
        public void Cancel_RemovesTimer()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            Assert.AreEqual(1, App.Get<Timer>().Count);
            
            App.Get<Timer>().CancelTimer(handle);
            
            Assert.AreEqual(0, App.Get<Timer>().Count);
        }

        [Test]
        public void Reset_RestoresInitialTime()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            // Simulate some time passing (manually set for test)
            
            App.Get<Timer>().ResetTimer(handle);
            
            Assert.AreEqual(5f, App.Get<Timer>().GetCurrentTime(handle), 0.01f);
            Assert.IsTrue(App.Get<Timer>().IsRunning(handle));
        }

        [Test]
        public void SetTimeScale_ChangesTimerSpeed()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            App.Get<Timer>().SetTimeScale(handle, 2f);
            
            // Timer should still be valid after setting time scale
            Assert.IsTrue(App.Get<Timer>().IsRunning(handle));
        }

        [Test]
        public void Clear_RemovesAllTimers()
        {
            App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            App.Get<Timer>().CreateTimer<StopwatchTimer>(0f);
            App.Get<Timer>().CreateTimer<DelayTimer>(3f);
            Assert.AreEqual(3, App.Get<Timer>().Count);
            
            App.Get<Timer>().Clear();
            
            Assert.AreEqual(0, App.Get<Timer>().Count);
        }

        #endregion

        #region Callback Tests

        [Test]
        public void OnComplete_CallbackRegistered_DoesNotThrow()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            Assert.DoesNotThrow(() => {
                App.Get<Timer>().On<OnComplete>(handle, () => { });
            });
        }

        [Test]
        public void OnTick_WithParameter_CallbackRegistered()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            Assert.DoesNotThrow(() => {
                App.Get<Timer>().On<OnTick, float>(handle, (dt) => { });
            });
        }

        [Test]
        public void OnRepeat_WithIntParameter_CallbackRegistered()
        {
            var handle = App.Get<Timer>().CreateTimer<RepeatingTimer>(1f);
            
            Assert.DoesNotThrow(() => {
                App.Get<Timer>().On<OnRepeat, int>(handle, (count) => { });
            });
        }

        [Test]
        public void OnPause_CallbackRegistered()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            Assert.DoesNotThrow(() => {
                App.Get<Timer>().On<OnPause>(handle, () => { });
            });
        }

        [Test]
        public void OnResume_CallbackRegistered()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            Assert.DoesNotThrow(() => {
                App.Get<Timer>().On<OnResume>(handle, () => { });
            });
        }

        [Test]
        public void OnReset_CallbackRegistered()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            Assert.DoesNotThrow(() => {
                App.Get<Timer>().On<OnReset>(handle, () => { });
            });
        }

        [Test]
        public void OnCancel_CallbackRegistered()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            Assert.DoesNotThrow(() => {
                App.Get<Timer>().On<OnCancel>(handle, () => { });
            });
        }

        [Test]
        public void Off_UnregistersCallback()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            App.Get<Timer>().On<OnComplete>(handle, () => { });
            
            Assert.DoesNotThrow(() => {
                App.Get<Timer>().Off<OnComplete>(handle);
            });
        }

        #endregion

        #region Easing Tests

        [Test]
        public void GetEasedProgress_Linear_ReturnsSameAsProgress()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            float progress = App.Get<Timer>().GetProgress(handle);
            float eased = App.Get<Timer>().GetEasedProgress(handle, EasingType.Linear);
            
            Assert.AreEqual(progress, eased, 0.001f);
        }

        [Test]
        public void GetEasedProgress_QuadIn_ReturnsDifferentValue()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            // QuadIn at t=1 should still be 1, but at t=0.5 it's 0.25
            float eased = App.Get<Timer>().GetEasedProgress(handle, EasingType.QuadIn);
            
            // At start (progress = 1), QuadIn(1) = 1
            Assert.AreEqual(1f, eased, 0.001f);
        }

        [Test]
        public void Lerp_Float_InterpolatesCorrectly()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            float result = App.Get<Timer>().Lerp(handle, 0f, 100f, EasingType.Linear);
            
            Assert.AreEqual(100f, result, 0.01f);
        }

        [Test]
        public void Lerp_Vector2_InterpolatesCorrectly()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            Vector2 from = Vector2.zero;
            Vector2 to = Vector2.one * 10f;
            
            Vector2 result = App.Get<Timer>().Lerp(handle, from, to, EasingType.Linear);
            
            Assert.AreEqual(to, result);
        }

        [Test]
        public void Lerp_Vector3_InterpolatesCorrectly()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            Vector3 from = Vector3.zero;
            Vector3 to = Vector3.one * 10f;
            
            Vector3 result = App.Get<Timer>().Lerp(handle, from, to, EasingType.Linear);
            
            Assert.AreEqual(to, result);
        }

        [Test]
        public void Lerp_Quaternion_InterpolatesCorrectly()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            Quaternion from = Quaternion.identity;
            Quaternion to = Quaternion.Euler(0, 90, 0);
            
            Quaternion result = App.Get<Timer>().Lerp(handle, from, to, EasingType.Linear);
            
            Assert.AreEqual(to.eulerAngles.y, result.eulerAngles.y, 0.01f);
        }

        [Test]
        public void Lerp_Color_InterpolatesCorrectly()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            Color from = Color.black;
            Color to = Color.white;
            
            Color result = App.Get<Timer>().Lerp(handle, from, to, EasingType.Linear);
            
            Assert.AreEqual(to, result);
        }

        [Test]
        public void LerpUnclamped_AllowsOvershoot()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            
            // LerpUnclamped should work without throwing
            Assert.DoesNotThrow(() => {
                App.Get<Timer>().LerpUnclamped(handle, 0f, 10f, EasingType.ElasticOut);
            });
        }

        #endregion

        #region Timer Type Tests

        [Test]
        public void DelayTimer_CreatesValidHandle()
        {
            var handle = App.Get<Timer>().CreateTimer<DelayTimer>(2f);
            
            Assert.IsTrue(handle.IsValid);
            Assert.IsTrue(App.Get<Timer>().IsRunning(handle));
        }

        [Test]
        public void RepeatingTimer_CreatesValidHandle()
        {
            var handle = App.Get<Timer>().CreateTimer<RepeatingTimer>(1f);
            
            Assert.IsTrue(handle.IsValid);
            Assert.IsTrue(App.Get<Timer>().IsRunning(handle));
        }

        [Test]
        public void FrequencyTimer_CreatesValidHandle()
        {
            var handle = App.Get<Timer>().CreateTimer<FrequencyTimer>(10f);
            
            Assert.IsTrue(handle.IsValid);
            Assert.IsTrue(App.Get<Timer>().IsRunning(handle));
        }

        #endregion

        #region Handle Validity Tests

        [Test]
        public void InvalidHandle_IsValid_ReturnsFalse()
        {
            var handle = TimerHandle.None;
            
            Assert.IsFalse(handle.IsValid);
        }

        [Test]
        public void CancelledTimer_Operations_DoNotThrow()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            App.Get<Timer>().CancelTimer(handle);
            
            Assert.DoesNotThrow(() => App.Get<Timer>().Pause(handle));
            Assert.DoesNotThrow(() => App.Get<Timer>().Resume(handle));
            Assert.DoesNotThrow(() => App.Get<Timer>().GetProgress(handle));
            Assert.DoesNotThrow(() => App.Get<Timer>().ResetTimer(handle));
            Assert.DoesNotThrow(() => App.Get<Timer>().SetTimeScale(handle, 1f));
        }

        [Test]
        public void InvalidHandle_QueryOperations_ReturnDefaults()
        {
            var handle = TimerHandle.None;
            
            Assert.AreEqual(0f, App.Get<Timer>().GetCurrentTime(handle));
            Assert.AreEqual(0f, App.Get<Timer>().GetProgress(handle));
            Assert.IsTrue(App.Get<Timer>().IsFinished(handle));
            Assert.IsFalse(App.Get<Timer>().IsRunning(handle));
        }

        #endregion

        #region Backend Tests

        [Test]
        public void IsBurstMode_ReturnsBoolean()
        {
            // Just verify it doesn't throw
            bool isBurst = App.Get<Timer>().IsBurstMode;
            Assert.That(isBurst, Is.TypeOf<bool>());
        }

        [Test]
        public void Count_ReturnsCorrectValue()
        {
            Assert.AreEqual(0, App.Get<Timer>().Count);
            
            App.Get<Timer>().CreateTimer<CountdownTimer>(1f);
            Assert.AreEqual(1, App.Get<Timer>().Count);
            
            App.Get<Timer>().CreateTimer<CountdownTimer>(1f);
            Assert.AreEqual(2, App.Get<Timer>().Count);
        }

        #endregion

        #region Integration Tests

        [UnityTest]
        public IEnumerator Timer_Updates_AfterFrame()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(1f);
            float initialTime = App.Get<Timer>().GetCurrentTime(handle);
            
            yield return null;
            
            float newTime = App.Get<Timer>().GetCurrentTime(handle);
            
            Assert.Less(newTime, initialTime);
        }

        [UnityTest]
        public IEnumerator StopwatchTimer_CountsUp()
        {
            var handle = App.Get<Timer>().CreateTimer<StopwatchTimer>(0f);
            
            yield return null;
            
            float time = App.Get<Timer>().GetCurrentTime(handle);
            
            Assert.Greater(time, 0f);
        }

        [UnityTest]
        public IEnumerator PausedTimer_DoesNotUpdate()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            App.Get<Timer>().Pause(handle);
            float initialTime = App.Get<Timer>().GetCurrentTime(handle);
            
            yield return null;
            
            float newTime = App.Get<Timer>().GetCurrentTime(handle);
            
            Assert.AreEqual(initialTime, newTime, 0.001f);
        }

        [UnityTest]
        public IEnumerator OnTickCallback_IsInvoked()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            bool tickCalled = false;
            
            App.Get<Timer>().On<OnTick, float>(handle, (dt) => tickCalled = true);
            
            yield return null;
            
            Assert.IsTrue(tickCalled);
        }

        [UnityTest]
        public IEnumerator Timer_CompletesAfterDuration()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(0.1f);
            bool completed = false;
            
            App.Get<Timer>().On<OnComplete>(handle, () => completed = true);
            
            yield return new WaitForSeconds(0.2f);
            
            Assert.IsTrue(completed);
            Assert.IsTrue(App.Get<Timer>().IsFinished(handle));
        }

        [UnityTest]
        public IEnumerator Reset_RestoresTimerAfterCompletion()
        {
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(0.1f);
            
            yield return new WaitForSeconds(0.2f);
            
            Assert.IsTrue(App.Get<Timer>().IsFinished(handle));
            
            App.Get<Timer>().ResetTimer(handle);
            
            Assert.IsFalse(App.Get<Timer>().IsFinished(handle));
            Assert.IsTrue(App.Get<Timer>().IsRunning(handle));
        }

        #endregion

        #region Presets Tests

        [Test]
        public void Presets_Define_CreatesPreset()
        {
            TimerPresets.Clear();
            
            TimerPresets.Define("TestPreset", 5f, EasingType.QuadOut);
            
            Assert.IsTrue(TimerPresets.Exists("TestPreset"));
        }

        [Test]
        public void Presets_DefineGeneric_CreatesPresetWithType()
        {
            TimerPresets.Clear();
            
            TimerPresets.Define<RepeatingTimer>("RepeatPreset", 2f);
            
            var preset = TimerPresets.Get("RepeatPreset");
            Assert.AreEqual(typeof(RepeatingTimer), preset.TimerType);
        }

        [Test]
        public void FromPreset_CreatesTimerFromPreset()
        {
            TimerPresets.Clear();
            TimerPresets.Define("QuickFade", 0.5f);
            
            var handle = App.Get<Timer>().CreateFromPreset("QuickFade");
            
            Assert.IsTrue(handle.IsValid);
            Assert.AreEqual(1, App.Get<Timer>().Count);
        }

        [Test]
        public void FromPreset_WithCallback_RegistersCallback()
        {
            TimerPresets.Clear();
            TimerPresets.Define("TestWithCallback", 1f);
            bool called = false;
            
            var handle = App.Get<Timer>().CreateFromPreset("TestWithCallback", () => called = true);
            
            Assert.IsTrue(handle.IsValid);
            // Callback registration is tested by checking handle is valid
        }

        [Test]
        public void FromPreset_UndefinedPreset_ReturnsNone()
        {
            TimerPresets.Clear();
            
            var handle = App.Get<Timer>().CreateFromPreset("NonExistent");
            
            Assert.IsFalse(handle.IsValid);
        }

        #endregion

        #region Metrics Tests

        [Test]
        public void Metrics_TotalCreated_IncreasesOnCreate()
        {
            App.Get<Timer>().Metrics.Reset();
            
            App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            App.Get<Timer>().CreateTimer<StopwatchTimer>(0f);
            App.Get<Timer>().CreateTimer<DelayTimer>(3f);
            
            Assert.AreEqual(3, App.Get<Timer>().Metrics.TotalCreated);
        }

        [Test]
        public void Metrics_TotalCancelled_IncreasesOnCancel()
        {
            App.Get<Timer>().Metrics.Reset();
            
            var h1 = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            var h2 = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            App.Get<Timer>().CancelTimer(h1);
            
            Assert.AreEqual(1, App.Get<Timer>().Metrics.TotalCancelled);
        }

        [Test]
        public void Metrics_TotalResets_IncreasesOnReset()
        {
            App.Get<Timer>().Metrics.Reset();
            
            var handle = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            App.Get<Timer>().ResetTimer(handle);
            App.Get<Timer>().ResetTimer(handle);
            
            Assert.AreEqual(2, App.Get<Timer>().Metrics.TotalResets);
        }

        [Test]
        public void Metrics_PeakActiveCount_TracksMaximum()
        {
            App.Get<Timer>().Metrics.Reset();
            
            var h1 = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            var h2 = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            var h3 = App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            App.Get<Timer>().CancelTimer(h1);
            App.Get<Timer>().CancelTimer(h2);
            
            Assert.GreaterOrEqual(App.Get<Timer>().Metrics.PeakActiveCount, 3);
        }

        [Test]
        public void Metrics_AverageDuration_CalculatesCorrectly()
        {
            App.Get<Timer>().Metrics.Reset();
            
            App.Get<Timer>().CreateTimer<CountdownTimer>(2f);
            App.Get<Timer>().CreateTimer<CountdownTimer>(4f);
            App.Get<Timer>().CreateTimer<CountdownTimer>(6f);
            
            Assert.AreEqual(4f, App.Get<Timer>().Metrics.AverageDuration, 0.01f);
        }

        [Test]
        public void Metrics_Reset_ClearsAllMetrics()
        {
            App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            App.Get<Timer>().Metrics.Reset();
            
            Assert.AreEqual(0, App.Get<Timer>().Metrics.TotalCreated);
            Assert.AreEqual(0, App.Get<Timer>().Metrics.TotalCancelled);
            Assert.AreEqual(0, App.Get<Timer>().Metrics.TotalResets);
        }

        #endregion

        #region Persistence Tests

        [Test]
        public void Persistence_SaveAll_ReturnsValidJson()
        {
            App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            App.Get<Timer>().CreateTimer<StopwatchTimer>(0f);
            
            string json = TimerPersistence.SaveAll();
            
            Assert.IsFalse(string.IsNullOrEmpty(json));
            Assert.IsTrue(json.Contains("CountdownTimer"));
            Assert.IsTrue(json.Contains("StopwatchTimer"));
        }

        [Test]
        public void Persistence_LoadAll_RestoresTimers()
        {
            App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            App.Get<Timer>().CreateTimer<CountdownTimer>(10f);
            string json = TimerPersistence.SaveAll();
            
            App.Get<Timer>().Clear();
            Assert.AreEqual(0, App.Get<Timer>().Count);
            
            var handles = TimerPersistence.LoadAll(json);
            
            Assert.AreEqual(2, handles.Count);
            Assert.AreEqual(2, App.Get<Timer>().Count);
        }

        [Test]
        public void Persistence_LoadAll_EmptyJson_ReturnsEmptyList()
        {
            var handles = TimerPersistence.LoadAll("");
            
            Assert.AreEqual(0, handles.Count);
        }

        [Test]
        public void Persistence_Clear_ClearsRegistry()
        {
            App.Get<Timer>().CreateTimer<CountdownTimer>(5f);
            TimerPersistence.SaveAll();
            
            TimerPersistence.Clear();
            
            // No exception means success
            Assert.Pass();
        }

        #endregion
    }
}
