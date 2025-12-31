using UnityEngine;
using UnityEngine.UIElements;
using Eraflo.Catalyst.BehaviourTree;

namespace Eraflo.Catalyst.Editor.BehaviourTree.Canvas
{
    public class BTDataEdgeElement : VisualElement
    {
        public BTNodeElement FromNode { get; private set; }
        public BTNodeElement ToNode { get; private set; }
        public NodePort FromPort { get; private set; }
        public NodePort ToPort { get; private set; }
        
        public bool IsSelected { get; private set; }
        public System.Action<BTDataEdgeElement> OnSelected;
        
        private Color _edgeColor = Color.gray;
        private const float EdgeWidth = 2f;
        
        public BTDataEdgeElement(BTNodeElement from, NodePort fromPort, BTNodeElement to, NodePort toPort)
        {
            FromNode = from;
            FromPort = fromPort;
            ToNode = to;
            ToPort = toPort;
            
            name = "bt-data-edge";
            style.position = Position.Absolute;
            pickingMode = PickingMode.Position;
            
            // Set initial color based on type
            _edgeColor = GetTypeColor(fromPort.DataType);
            
            generateVisualContent += OnGenerateVisualContent;
            
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            
            // Watch geometry changes
            if (FromNode != null) FromNode.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            if (ToNode != null) ToNode.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
        
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            UpdatePath();
        }
        
        public void UpdatePath()
        {
            if (FromNode == null || ToNode == null || parent == null) return;
            
            var fromPortElem = FromNode.GetPortElement(FromPort.Name);
            var toPortElem = ToNode.GetPortElement(ToPort.Name);
            
            if (fromPortElem == null || toPortElem == null) return;
            
            var startWorld = fromPortElem.GetHandlePosition();
            var endWorld = toPortElem.GetHandlePosition();
            
            var startPos = parent.WorldToLocal(startWorld);
            var endPos = parent.WorldToLocal(endWorld);
            
            // Adjust bounds
            float padding = 50f;
            float xMin = Mathf.Min(startPos.x, endPos.x) - padding;
            float yMin = Mathf.Min(startPos.y, endPos.y) - padding;
            float width = Mathf.Abs(endPos.x - startPos.x) + padding * 2;
            float height = Mathf.Abs(endPos.y - startPos.y) + padding * 2;
            
            style.left = xMin;
            style.top = yMin;
            style.width = width;
            style.height = height;
            
            MarkDirtyRepaint();
        }
        
        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            if (FromNode == null || ToNode == null) return;
            
            var fromPortElem = FromNode.GetPortElement(FromPort.Name);
            var toPortElem = ToNode.GetPortElement(ToPort.Name);
             
            if (fromPortElem == null || toPortElem == null) return;
            
            var startWorld = fromPortElem.GetHandlePosition();
            var endWorld = toPortElem.GetHandlePosition();
            
            // Convert to parent space
            var startParent = parent.WorldToLocal(startWorld);
            var endParent = parent.WorldToLocal(endWorld);
            
            // Convert to element space
            var offset = new Vector2(resolvedStyle.left, resolvedStyle.top);
            var startPos = startParent - offset;
            var endPos = endParent - offset;
            
            var painter = ctx.painter2D;
            painter.strokeColor = IsSelected ? Color.white : _edgeColor;
            painter.lineWidth = EdgeWidth;
            painter.lineCap = LineCap.Round;
            
            painter.BeginPath();
            painter.MoveTo(startPos);
            
            // Horizontal Bezier
            float dist = Mathf.Abs(endPos.x - startPos.x);
            float tangentStrength = Mathf.Min(dist * 0.5f, 100f);
            
            // Curve out to right, in from left
            var cp1 = startPos + new Vector2(tangentStrength, 0);
            var cp2 = endPos - new Vector2(tangentStrength, 0);
            
            painter.BezierCurveTo(cp1, cp2, endPos);
            painter.Stroke();
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                OnSelected?.Invoke(this);
                evt.StopPropagation();
            }
        }
        
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            MarkDirtyRepaint();
        }
        
        public override bool ContainsPoint(Vector2 localPoint)
        {            
            var offset = new Vector2(resolvedStyle.left, resolvedStyle.top);
            Vector2 worldPos = localPoint + offset;
            
            var fromPortElem = FromNode.GetPortElement(FromPort.Name);
            var toPortElem = ToNode.GetPortElement(ToPort.Name);
            if (fromPortElem == null || toPortElem == null) return false;
            
            var startPos = fromPortElem.GetHandlePosition();
            var endPos = toPortElem.GetHandlePosition();
            
            float dist = Mathf.Abs(endPos.x - startPos.x);
            float tangentStrength = Mathf.Min(dist * 0.5f, 100f);
            var cp1 = startPos + new Vector2(tangentStrength, 0);
            var cp2 = endPos - new Vector2(tangentStrength, 0);
            
            const int samples = 20;
            float minDistanceSq = float.MaxValue;
            Vector2 lastPoint = startPos;
            
            for (int i = 1; i <= samples; i++)
            {
                float t = i / (float)samples;
                Vector2 currentPoint = GetBezierPoint(startPos, cp1, cp2, endPos, t);
                 
                float distSq = DistancePointToSegmentSq(worldPos, lastPoint, currentPoint);
                if (distSq < minDistanceSq) minDistanceSq = distSq;
                lastPoint = currentPoint;
            }
            
            return minDistanceSq < 100f; // 10px radius
        }
        
        private Vector2 GetBezierPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1 - t;
            return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
        }

        private float DistancePointToSegmentSq(Vector2 p, Vector2 a, Vector2 b)
        {
            float l2 = (a - b).sqrMagnitude;
            if (l2 == 0) return (p - a).sqrMagnitude;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, b - a) / l2);
            return (p - (a + t * (b - a))).sqrMagnitude;
        }

        private Color GetTypeColor(System.Type type)
        {
            // Same colors as PortElement
            if (type == typeof(bool)) return new Color(1f, 0.4f, 0.4f); 
            if (type == typeof(float) || type == typeof(int)) return new Color(0.4f, 0.8f, 1f); 
            if (type == typeof(string)) return new Color(1f, 0.8f, 0.4f);
            if (type == typeof(Vector3) || type == typeof(Vector2)) return new Color(0.6f, 1f, 0.6f);
            return new Color(0.8f, 0.8f, 0.8f);
        }
    }
}
