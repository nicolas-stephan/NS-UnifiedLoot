using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NS.UnifiedLoot.Editor {
    [CustomPropertyDrawer(typeof(IntRange))]
    public class IntRangeDrawer : PropertyDrawer {
        // Session-only per-property mode preference.
        // Key: "{instanceID}:{propertyPath}"
        private static readonly Dictionary<string, bool> IsRangeMode = new();

        private static readonly string[] ModeOptions = { "Exact", "Range" };
        private const float PopupWidth = 50f;
        private const float DashWidth = 10f;
        private const float Gap = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var minProp = property.FindPropertyRelative(IntRange.NameOfMin);
            var maxProp = property.FindPropertyRelative(IntRange.NameOfMax);

            EditorGUI.BeginProperty(position, label, property);

            var controlRect = label != GUIContent.none ? EditorGUI.PrefixLabel(position, label) : position;
            var key = GetKey(property);
            if (minProp.intValue != maxProp.intValue)
                IsRangeMode[key] = true;
            IsRangeMode.TryGetValue(key, out var isRange);

            var oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            isRange = DrawRangeModeDropdown(controlRect, isRange, maxProp, minProp, key);

            var fieldsRect = new Rect(controlRect.x + PopupWidth + Gap, controlRect.y, controlRect.width - PopupWidth - Gap, controlRect.height);
            if (!isRange)
                DrawExact(fieldsRect, minProp, maxProp);
            else
                DrawRange(fieldsRect, minProp, maxProp);

            EditorGUI.indentLevel = oldIndent;
            EditorGUI.EndProperty();
        }

        private static bool DrawRangeModeDropdown(Rect controlRect, bool isRange, SerializedProperty maxProp, SerializedProperty minProp, string key) {
            var popupRect = new Rect(controlRect.x, controlRect.y, PopupWidth, controlRect.height);
            var newMode = EditorGUI.Popup(popupRect, isRange ? 1 : 0, ModeOptions);

            if (newMode == 0 && isRange)
                maxProp.intValue = minProp.intValue;

            IsRangeMode[key] = newMode == 1;
            isRange = newMode == 1;
            return isRange;
        }

        private static void DrawExact(Rect rect, SerializedProperty minProp, SerializedProperty maxProp) {
            EditorGUI.BeginChangeCheck();
            var val = EditorGUI.IntField(rect, minProp.intValue);
            if (!EditorGUI.EndChangeCheck())
                return;

            minProp.intValue = val;
            maxProp.intValue = val;
        }

        private static void DrawRange(Rect rect, SerializedProperty minProp, SerializedProperty maxProp) {
            var fieldW = (rect.width - DashWidth - Gap * 2f) * 0.5f;
            var minRect = new Rect(rect.x, rect.y, fieldW, rect.height);
            var dashRect = new Rect(rect.x + fieldW + Gap, rect.y, DashWidth, rect.height);
            var maxRect = new Rect(rect.x + fieldW + Gap + DashWidth + Gap, rect.y, fieldW, rect.height);

            EditorGUI.BeginChangeCheck();
            var newMin = EditorGUI.IntField(minRect, minProp.intValue);
            EditorGUI.LabelField(dashRect, "–", EditorStyles.centeredGreyMiniLabel);
            var newMax = EditorGUI.IntField(maxRect, maxProp.intValue);
            if (!EditorGUI.EndChangeCheck())
                return;

            minProp.intValue = newMin;
            maxProp.intValue = Mathf.Max(newMin, newMax);
        }

        private static string GetKey(SerializedProperty prop) => $"{prop.serializedObject.targetObject.GetInstanceID()}:{prop.propertyPath}";
    }
}