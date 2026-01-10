using System;

namespace Eraflo.Catalyst
{
    /// <summary>
    /// Attribute used to mark classes for auto-discovery by the Service Locator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ServiceAttribute : Attribute
    {
        /// <summary>
        /// Initialization and update priority. Lower value means higher priority.
        /// </summary>
        public int Priority { get; set; }

        public ServiceAttribute(int priority = 0)
        {
            Priority = priority;
        }
    }
}
