using System;
using System.Collections.Generic;
using Eraflo.Catalyst;

namespace Eraflo.Catalyst.Events
{
    /// <summary>
    /// Central event bus managing all event subscriptions.
    /// Can be used as a service via Service Locator.
    /// Thread-safe and async-safe implementation.
    /// </summary>
    [Service(Priority = -10)] // High priority for events
    public class EventBus : IGameService
    {
        private readonly object Lock = new object();
        
        // Stores callbacks per channel instance
        private readonly Dictionary<object, List<Delegate>> ChannelCallbacks = 
            new Dictionary<object, List<Delegate>>();


        #region IGameService

        void IGameService.Initialize()
        {
            // Initialization logic if needed
        }

        void IGameService.Shutdown()
        {
            ClearAll();
        }

        #endregion


        #region Instance Methods

        /// <summary>
        /// Subscribes a callback to a specific event channel.
        /// </summary>
        public void Subscribe<T>(EventChannel<T> channel, Action<T> callback)
        {
            if (channel == null || callback == null) return;
            
            lock (Lock)
            {
                if (!ChannelCallbacks.TryGetValue(channel, out var callbacks))
                {
                    callbacks = new List<Delegate>();
                    ChannelCallbacks[channel] = callbacks;
                }
                
                if (!callbacks.Contains(callback))
                {
                    callbacks.Add(callback);
                }
            }
        }

        /// <summary>
        /// Subscribes a callback (no args) to a specific event channel.
        /// </summary>
        public void Subscribe<T>(EventChannel<T> channel, Action callback)
        {
            if (channel == null || callback == null) return;
            
            lock (Lock)
            {
                if (!ChannelCallbacks.TryGetValue(channel, out var callbacks))
                {
                    callbacks = new List<Delegate>();
                    ChannelCallbacks[channel] = callbacks;
                }
                
                if (!callbacks.Contains(callback))
                {
                    callbacks.Add(callback);
                }
            }
        }

        /// <summary>
        /// Subscribes a callback to a void event channel.
        /// </summary>
        public void Subscribe(EventChannel channel, Action callback)
        {
            if (channel == null || callback == null) return;
            
            lock (Lock)
            {
                if (!ChannelCallbacks.TryGetValue(channel, out var callbacks))
                {
                    callbacks = new List<Delegate>();
                    ChannelCallbacks[channel] = callbacks;
                }
                
                if (!callbacks.Contains(callback))
                {
                    callbacks.Add(callback);
                }
            }
        }

        /// <summary>
        /// Unsubscribes a callback from a specific event channel.
        /// </summary>
        public void Unsubscribe<T>(EventChannel<T> channel, Action<T> callback)
        {
            if (channel == null || callback == null) return;
            
            lock (Lock)
            {
                if (ChannelCallbacks.TryGetValue(channel, out var callbacks))
                {
                    callbacks.Remove(callback);
                    
                    if (callbacks.Count == 0)
                    {
                        ChannelCallbacks.Remove(channel);
                    }
                }
            }
        }

        /// <summary>
        /// Unsubscribes a callback (no args) from a specific event channel.
        /// </summary>
        public void Unsubscribe<T>(EventChannel<T> channel, Action callback)
        {
            if (channel == null || callback == null) return;
            
            lock (Lock)
            {
                if (ChannelCallbacks.TryGetValue(channel, out var callbacks))
                {
                    callbacks.Remove(callback);
                    
                    if (callbacks.Count == 0)
                    {
                        ChannelCallbacks.Remove(channel);
                    }
                }
            }
        }

        /// <summary>
        /// Unsubscribes a callback from a void event channel.
        /// </summary>
        public void Unsubscribe(EventChannel channel, Action callback)
        {
            if (channel == null || callback == null) return;
            
            lock (Lock)
            {
                if (ChannelCallbacks.TryGetValue(channel, out var callbacks))
                {
                    callbacks.Remove(callback);
                    
                    if (callbacks.Count == 0)
                    {
                        ChannelCallbacks.Remove(channel);
                    }
                }
            }
        }

        /// <summary>
        /// Raises an event on a typed channel.
        /// </summary>
        internal void Raise<T>(EventChannel<T> channel, T value)
        {
            if (channel == null) return;
            
            List<Delegate> callbacksCopy;
            
            lock (Lock)
            {
                if (!ChannelCallbacks.TryGetValue(channel, out var callbacks))
                {
                    return;
                }
                
                // Copy to avoid modification during iteration (async-safe)
                callbacksCopy = new List<Delegate>(callbacks);
            }
            
            foreach (var callback in callbacksCopy)
            {
                try
                {
                    if (callback is Action<T> typedCallback)
                    {
                        typedCallback.Invoke(value);
                    }
                    else if (callback is Action noArgsCallback)
                    {
                        noArgsCallback.Invoke();
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Raises an event on a void channel.
        /// </summary>
        internal void Raise(EventChannel channel)
        {
            if (channel == null) return;
            
            List<Delegate> callbacksCopy;
            
            lock (Lock)
            {
                if (!ChannelCallbacks.TryGetValue(channel, out var callbacks))
                {
                    return;
                }
                
                callbacksCopy = new List<Delegate>(callbacks);
            }
            
            foreach (var callback in callbacksCopy)
            {
                try
                {
                    if (callback is Action noArgsCallback)
                    {
                        noArgsCallback.Invoke();
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Gets the number of subscribers for a channel.
        /// </summary>
        public int GetSubscriberCount(object channel)
        {
            if (channel == null) return 0;
            
            lock (Lock)
            {
                return ChannelCallbacks.TryGetValue(channel, out var callbacks) ? callbacks.Count : 0;
            }
        }

        /// <summary>
        /// Clears all subscriptions. Useful for scene transitions.
        /// </summary>
        public void ClearAll()
        {
            lock (Lock)
            {
                ChannelCallbacks.Clear();
            }
        }

        /// <summary>
        /// Clears subscriptions for a specific channel.
        /// </summary>
        public void Clear(object channel)
        {
            if (channel == null) return;
            
            lock (Lock)
            {
                ChannelCallbacks.Remove(channel);
            }
        }

        /// <summary>Alias for ClearAll.</summary>
        public void Clear() => ClearAll();

        #endregion
    }
}
