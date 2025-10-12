using NUnit.Framework;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace SRF.UI.Editor
{
    [CustomEditor(typeof (LongPressButton), true)]
    [CanEditMultipleObjects]
    public class LongPressButtonEditor : ButtonEditor
    {
        private SerializedProperty _onLongPressProperty;
        private SerializedProperty _timeDuration ;
        protected override void OnEnable()
        {
            base.OnEnable();
            _onLongPressProperty = serializedObject.FindProperty("_onLongPress");
            _timeDuration = serializedObject.FindProperty("_longPressDuration");
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            serializedObject.Update();
            EditorGUILayout.PropertyField(_onLongPressProperty);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_timeDuration);
            _timeDuration.floatValue = EditorGUILayout.Slider(_timeDuration.floatValue, 0.2f,2.0f);
            serializedObject.ApplyModifiedProperties();

        }
    }
}
