using UnityEngine;

namespace Eraflo.UnityImportPackage.BehaviourTree
{
    /// <summary>
    /// Log action: Logs a message to the console and returns Success.
    /// Useful for debugging.
    /// </summary>
    [CreateAssetMenu(fileName = "Log", menuName = "Behaviour Tree/Actions/Log")]
    public class Log : ActionNode
    {
        /// <summary>The message to log.</summary>
        [TextArea]
        public string Message = "Log";
        
        /// <summary>Log level.</summary>
        public LogType LogType = LogType.Log;
        
        public enum LogType
        {
            Log,
            Warning,
            Error
        }
        
        protected override NodeState OnUpdate()
        {
            string formattedMessage = $"[BT] {Message}";
            
            switch (LogType)
            {
                case LogType.Log:
                    Debug.Log(formattedMessage, Owner);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(formattedMessage, Owner);
                    break;
                case LogType.Error:
                    Debug.LogError(formattedMessage, Owner);
                    break;
            }
            
            return NodeState.Success;
        }
    }
}
