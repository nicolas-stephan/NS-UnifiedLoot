using System.Collections.Generic;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using UnityEditor;
using UnityEngine;

namespace NS.UnifiedLoot.Editor {
    [CustomPropertyDrawer(typeof(IntRange))]
    public class IntRangeDrawer : PropertyDrawer {
        private static readonly Dictionary<string, bool> IsRangeMode = new();

        private static readonly string[] ModeOptions = { "Exact", "Range" };
        private static float HorizontalGap => EditorGUIUtility.standardVerticalSpacing * 2f;

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

            var popupWidth = GetPopupWidth();
            isRange = DrawRangeModeDropdown(controlRect, isRange, maxProp, minProp, key, popupWidth);

            var fieldsRect = new Rect(controlRect.x + popupWidth + HorizontalGap, controlRect.y, controlRect.width - popupWidth - HorizontalGap, controlRect.height);
            if (!isRange)
                DrawExact(fieldsRect, minProp, maxProp);
            else
                DrawRange(fieldsRect, minProp, maxProp);

            EditorGUI.indentLevel = oldIndent;
            EditorGUI.EndProperty();
        }

        private static float GetPopupWidth() {
            var style = EditorStyles.popup;
            var w1 = style.CalcSize(new GUIContent(ModeOptions[0])).x;
            var w2 = style.CalcSize(new GUIContent(ModeOptions[1])).x;
            return Mathf.Max(w1, w2) + EditorGUIUtility.standardVerticalSpacing * 2f;
        }

        private static bool DrawRangeModeDropdown(Rect controlRect, bool isRange, SerializedProperty maxProp, SerializedProperty minProp, string key, float popupWidth) {
            var popupRect = new Rect(controlRect.x, controlRect.y, popupWidth, controlRect.height);
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
            var dashContent = new GUIContent("–");
            var dashStyle = EditorStyles.centeredGreyMiniLabel;
            var dashW = dashStyle.CalcSize(dashContent).x + EditorGUIUtility.standardVerticalSpacing;
            var fieldW = (rect.width - dashW - HorizontalGap * 2f) * 0.5f;

            var minRect = new Rect(rect.x, rect.y, fieldW, rect.height);
            var dashRect = new Rect(rect.x + fieldW + HorizontalGap, rect.y, dashW, rect.height);
            var maxRect = new Rect(rect.x + fieldW + HorizontalGap + dashW + HorizontalGap, rect.y, fieldW, rect.height);

            EditorGUI.BeginChangeCheck();
            var newMin = EditorGUI.IntField(minRect, minProp.intValue);
            EditorGUI.LabelField(dashRect, dashContent, dashStyle);
            var newMax = EditorGUI.IntField(maxRect, maxProp.intValue);
            if (!EditorGUI.EndChangeCheck())
                return;

            minProp.intValue = newMin;
            maxProp.intValue = Mathf.Max(newMin, newMax);
        }

        private static string GetKey(SerializedProperty prop) => $"{prop.serializedObject.targetObject.GetInstanceID()}:{prop.propertyPath}";
    }
}