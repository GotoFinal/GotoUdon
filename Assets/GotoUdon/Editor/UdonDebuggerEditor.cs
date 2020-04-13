using System;
using UnityEditor;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace GotoUdon.Editor
{
    [CustomEditor(typeof(UdonDebugger))]
    public class UdonDebuggerEditor : UnityEditor.Editor
    {
        private string _eventName;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.BeginHorizontal();
            _eventName = EditorGUILayout.TextField("Event name", _eventName);
            ActionButton("Send event", _eventName, (behaviour, name) => behaviour.SendCustomEvent(name));
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            ActionButton("Interact", behaviour => behaviour.Interact());
            ActionButton("OnEnable", behaviour => behaviour.OnEnable());
            ActionButton("OnDisable", behaviour => behaviour.OnDisable());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            ActionButton("OnDestroy", behaviour => behaviour.OnDestroy());
            ActionButton("OnSpawn", behaviour => behaviour.OnSpawn());
            ActionButton("OnDrop", behaviour => behaviour.OnDrop());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            ActionButton("OnPickup", behaviour => behaviour.OnDestroy());
            ActionButton("OnPickupUseDown", behaviour => behaviour.OnSpawn());
            ActionButton("OnPickupUseUp", behaviour => behaviour.OnDrop());
            GUILayout.EndHorizontal();

            // TODO: more interactions
        }

        private new UdonDebugger target => (UdonDebugger) base.target;

        private void ActionButton(string name, Action<UdonBehaviour> action)
        {
            if (GUILayout.Button(name))
            {
                target.behaviours.ForEach(action);
            }
        }

        private void ActionButton<T1>(string name, T1 arg1, Action<UdonBehaviour, T1> action)
        {
            if (GUILayout.Button(name))
            {
                target.behaviours.ForEach(behaviour => action(behaviour, arg1));
            }
        }
    }
}