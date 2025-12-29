using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using Eraflo.UnityImportPackage.BehaviourTree;

namespace Eraflo.UnityImportPackage.Editor.BehaviourTree
{
    /// <summary>
    /// Panel that displays the blackboard contents.
    /// Shows real-time values during play mode.
    /// </summary>
    public class BlackboardView : VisualElement
    {
        private Eraflo.UnityImportPackage.BehaviourTree.BehaviourTree _tree;
        private VisualElement _keyListContainer;
        private Label _emptyLabel;
        
        public BlackboardView()
        {
            style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
            style.paddingLeft = 5;
            style.paddingRight = 5;
            style.paddingTop = 5;
            style.paddingBottom = 5;
            
            // Toolbar for adding keys
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.marginBottom = 5;
            
            var addButton = new Button(() => ShowAddKeyMenu()) { text = "+ Add Key" };
            addButton.style.flexGrow = 1;
            toolbar.Add(addButton);
            
            var clearButton = new Button(() => ClearBlackboard()) { text = "Clear" };
            clearButton.style.width = 50;
            toolbar.Add(clearButton);
            
            Add(toolbar);
            
            // Key list container with scroll
            var scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            
            _keyListContainer = new VisualElement();
            scrollView.Add(_keyListContainer);
            Add(scrollView);
            
            // Empty message
            _emptyLabel = new Label("(Blackboard is empty)");
            _emptyLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
            _emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _emptyLabel.style.marginTop = 20;
            _keyListContainer.Add(_emptyLabel);
        }
        
        public void UpdateView(Eraflo.UnityImportPackage.BehaviourTree.BehaviourTree tree)
        {
            _tree = tree;
            RefreshKeys();
        }
        
        private void RefreshKeys()
        {
            _keyListContainer.Clear();
            
            if (_tree == null || _tree.Blackboard == null)
            {
                _keyListContainer.Add(_emptyLabel);
                return;
            }
            
            var keys = _tree.Blackboard.GetAllKeys();
            
            if (keys.Length == 0)
            {
                _keyListContainer.Add(_emptyLabel);
                return;
            }
            
            foreach (var key in keys)
            {
                var keyRow = CreateKeyRow(key);
                _keyListContainer.Add(keyRow);
            }
        }
        
        private VisualElement CreateKeyRow(string key)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 2;
            row.style.paddingLeft = 3;
            row.style.paddingRight = 3;
            row.style.paddingTop = 2;
            row.style.paddingBottom = 2;
            row.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            row.style.borderTopLeftRadius = 3;
            row.style.borderTopRightRadius = 3;
            row.style.borderBottomLeftRadius = 3;
            row.style.borderBottomRightRadius = 3;
            
            // Key label
            var keyLabel = new Label(key);
            keyLabel.style.flexGrow = 1;
            keyLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            keyLabel.style.fontSize = 11;
            row.Add(keyLabel);
            
            // Value (try to get it)
            string valueStr = "(unknown)";
            if (_tree.Blackboard.TryGet<int>(key, out int intVal))
                valueStr = intVal.ToString();
            else if (_tree.Blackboard.TryGet<float>(key, out float floatVal))
                valueStr = floatVal.ToString("F2");
            else if (_tree.Blackboard.TryGet<bool>(key, out bool boolVal))
                valueStr = boolVal.ToString();
            else if (_tree.Blackboard.TryGet<string>(key, out string strVal))
                valueStr = $"\"{strVal}\"";
            else if (_tree.Blackboard.TryGet<Vector3>(key, out Vector3 v3Val))
                valueStr = v3Val.ToString("F1");
            
            var valueLabel = new Label(valueStr);
            valueLabel.style.color = new Color(0.6f, 0.8f, 0.6f);
            valueLabel.style.fontSize = 11;
            row.Add(valueLabel);
            
            // Delete button
            var deleteBtn = new Button(() =>
            {
                _tree.Blackboard.Remove(key);
                RefreshKeys();
            }) { text = "×" };
            deleteBtn.style.width = 20;
            deleteBtn.style.height = 18;
            deleteBtn.style.marginLeft = 5;
            row.Add(deleteBtn);
            
            return row;
        }
        
        private void ShowAddKeyMenu()
        {
            if (_tree == null) return;
            
            var menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("Bool"), false, () => AddKey<bool>("newBool", false));
            menu.AddItem(new GUIContent("Int"), false, () => AddKey<int>("newInt", 0));
            menu.AddItem(new GUIContent("Float"), false, () => AddKey<float>("newFloat", 0f));
            menu.AddItem(new GUIContent("String"), false, () => AddKey<string>("newString", ""));
            menu.AddItem(new GUIContent("Vector3"), false, () => AddKey<Vector3>("newVector3", Vector3.zero));
            
            menu.ShowAsContext();
        }
        
        private void AddKey<T>(string key, T value)
        {
            // Find unique key name
            int counter = 1;
            string uniqueKey = key;
            while (_tree.Blackboard.Contains(uniqueKey))
            {
                uniqueKey = $"{key}{counter}";
                counter++;
            }
            
            _tree.Blackboard.Set(uniqueKey, value);
            RefreshKeys();
        }
        
        private void ClearBlackboard()
        {
            if (_tree == null) return;
            
            if (EditorUtility.DisplayDialog("Clear Blackboard", 
                "Are you sure you want to clear all blackboard data?", 
                "Clear", "Cancel"))
            {
                _tree.Blackboard.Clear();
                RefreshKeys();
            }
        }
    }
}
