using UnityEngine;
using UnityEngine.UIElements;
using Eraflo.Catalyst.BehaviourTree;

namespace Eraflo.Catalyst.Editor.BehaviourTree.Canvas
{
    public class BTPortElement : VisualElement
    {
        public NodePort Port { get; private set; }
        public Node Node { get; private set; }
        
        private VisualElement _handle;
        private Label _label;
        
        public BTPortElement(NodePort port, Node node)
        {
            Port = port;
            Node = node;
            
            // Setup
            AddToClassList("bt-port");
            AddToClassList(port.IsInput ? "input" : "output");
            
            // Handle (The circle)
            _handle = new VisualElement { name = "port-handle" };
            _handle.AddToClassList("port-handle");
            
            // Color by type
            _handle.style.backgroundColor = GetTypeColor(port.DataType);
            
            // Label
            _label = new Label(port.Name);
            _label.AddToClassList("port-label");
            
            // Layout
            if (port.IsInput)
            {
                Add(_handle);
                Add(_label);
            }
            else
            {
                Add(_label);
                Add(_handle);
            }
            
            // Tooltip
            tooltip = $"{port.Name} ({port.DataType.Name})";
        }
        
        public Vector2 GetHandlePosition()
        {
            // Calculate world position of the center of the handle
            return _handle.worldBound.center;
        }
        
        public VisualElement GetHandle() => _handle;
        
        private Color GetTypeColor(System.Type type)
        {
            if (type == typeof(bool)) return new Color(1f, 0.4f, 0.4f); // Red
            if (type == typeof(float) || type == typeof(int)) return new Color(0.4f, 0.8f, 1f); // Blue
            if (type == typeof(string)) return new Color(1f, 0.8f, 0.4f); // Yellow
            if (type == typeof(Vector3) || type == typeof(Vector2)) return new Color(0.6f, 1f, 0.6f); // Green
            if (type == typeof(GameObject) || type == typeof(Transform)) return new Color(0.8f, 0.4f, 1f); // Purple
            
            return new Color(0.8f, 0.8f, 0.8f); // Gray generic
        }
    }
}
