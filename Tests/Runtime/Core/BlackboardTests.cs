using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Eraflo.Catalyst.Core.Blackboard;
using Eraflo.Catalyst.Core.Save;

namespace Eraflo.Catalyst.Tests
{
    public class BlackboardTests
    {
        [SetUp]
        public void SetUp()
        {
            // BlackboardManager is auto-discovered, but for testing we might want to ensure it's fresh
            // Since it's a service, we can initialize it manually if needed or use App.Get
            var bm = App.Get<BlackboardManager>();
            ((IGameService)bm).Shutdown();
            ((IGameService)bm).Initialize();
        }

        [Test]
        public void GlobalBlackboard_IsAccessible_ViaApp()
        {
            Assert.IsNotNull(App.Get<BlackboardManager>().Global);
        }

        [Test]
        public void Blackboard_ScopedLookup_FindsValueInParent()
        {
            var bm = App.Get<BlackboardManager>();
            var global = bm.Global;
            var scoped = bm.CreateScoped();

            global.Set("TestKey", 42);

            Assert.AreEqual(42, scoped.Get<int>("TestKey"));
            Assert.IsTrue(scoped.Contains("TestKey"));
        }

        [Test]
        public void Blackboard_Set_IsLocalOnly()
        {
            var bm = App.Get<BlackboardManager>();
            var global = bm.Global;
            var scoped = bm.CreateScoped();

            scoped.Set("LocalKey", "Hello");

            Assert.AreEqual("Hello", scoped.Get<string>("LocalKey"));
            Assert.IsFalse(global.Contains("LocalKey"));
        }

        [Test]
        public void Blackboard_Override_ValueInChild_TakesPrecedence()
        {
            var bm = App.Get<BlackboardManager>();
            var global = bm.Global;
            var scoped = bm.CreateScoped();

            global.Set("SharedKey", 10);
            scoped.Set("SharedKey", 20);

            Assert.AreEqual(20, scoped.Get<int>("SharedKey"));
            Assert.AreEqual(10, global.Get<int>("SharedKey"));
        }

        [Test]
        public void Blackboard_Contains_ChecksRecursively()
        {
            var bm = App.Get<BlackboardManager>();
            var global = bm.Global;
            var scoped = bm.CreateScoped();

            global.Set("GlobalKey", true);
            
            Assert.IsTrue(scoped.Contains("GlobalKey"));
        }

        [Test]
        public void Blackboard_Persistence_CapturesAndRestoresGlobalState()
        {
            var bm = App.Get<BlackboardManager>();
            var global = bm.Global;
            
            global.Set("SaveMe", "SecretData");
            global.Set("Score", 100);

            var state = ((ISaveable)bm).SaveState();
            Assert.IsNotNull(state);

            // Simulate fresh start
            global.Clear();
            Assert.IsFalse(global.Contains("SaveMe"));

            // Restore
            ((ISaveable)bm).LoadState(state);

            Assert.AreEqual("SecretData", global.Get<string>("SaveMe"));
            Assert.AreEqual(100, global.Get<int>("Score"));
        }

        [Test]
        public void Blackboard_RegisterListener_TriggersOnSet()
        {
            var bb = new Blackboard();
            bool triggered = false;
            string keyReceived = "";
            object oldVal = null;
            object newVal = null;

            bb.OnValueChanged += (k, o, n) => {
                triggered = true;
                keyReceived = k;
                oldVal = o;
                newVal = n;
            };

            bb.Set("MyKey", 123);

            Assert.IsTrue(triggered);
            Assert.AreEqual("MyKey", keyReceived);
            Assert.AreEqual(123, newVal);
        }

        [Test]
        public void Blackboard_Clone_CopiesInitialValues()
        {
            var bb = new Blackboard();
            bb.Set("IntVal", 100);
            bb.Set("StrVal", "Test");

            var clone = bb.Clone();

            Assert.AreEqual(100, clone.Get<int>("IntVal"));
            Assert.AreEqual("Test", clone.Get<string>("StrVal"));
        }

        [Test]
        public void Blackboard_GetAllKeys_ReturnsCorrectKeys()
        {
            var bb = new Blackboard();
            bb.Set("Key1", 1);
            bb.Set("Key2", 2);

            var keys = bb.GetAllKeys();

            Assert.AreEqual(2, keys.Count);
            Assert.Contains("Key1", keys);
            Assert.Contains("Key2", keys);
        }
    }
}
