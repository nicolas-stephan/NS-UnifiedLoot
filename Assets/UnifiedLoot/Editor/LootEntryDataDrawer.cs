using UnityEngine;
using UnityEditor;

namespace NS.UnifiedLoot.Editor {
    [CustomPropertyDrawer(typeof(LootEntryDataBase), true)]
    public class LootEntryDataDrawer : PropertyDrawer {
        private static float VerticalGap => EditorGUIUtility.standardVerticalSpacing;
        private static float HorizontalGap => EditorGUIUtility.standardVerticalSpacing * 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            (EditorGUIUtility.singleLineHeight + VerticalGap) * 2 + VerticalGap;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var weightProp = property.FindPropertyRelative(LootEntryDataBase.NameOfWeight);
            var quantityProp = property.FindPropertyRelative(LootEntryDataBase.NameOfQuantity);
            var itemProp = property.FindPropertyRelative(LootTableAsset<object>.LootEntryData.NameOfItem);

            EditorGUI.BeginProperty(position, label, property);
            var oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var x = position.x;
            var lh = EditorGUIUtility.singleLineHeight;
            var rowH = lh + VerticalGap;

            DrawWeightQuantityRow(new Rect(x, position.y + VerticalGap, position.width, lh), weightProp, quantityProp);
            if (itemProp != null)
                DrawItemRow(new Rect(x, position.y + rowH + VerticalGap, position.width, lh), itemProp);

            EditorGUI.indentLevel = oldIndent;
            EditorGUI.EndProperty();
        }

        private static (int frame, int objId, string path, float total) _totalWeightCache = (-1, -1, "", 0f);

        private static float GetTotalWeight(SerializedProperty? weightProp) {
            if (weightProp == null)
                return -1f;
            var path = weightProp.propertyPath;
            var arrayIdx = path.LastIndexOf(".Array.data[", System.StringComparison.Ordinal);
            if (arrayIdx < 0)
                return -1f;

            var listPath = path[..arrayIdx];
            var obj = weightProp.serializedObject.targetObject;
            if (obj == null)
                return -1f;
            var objId = obj.GetInstanceID();
            var frame = Time.frameCount;

            if (_totalWeightCache.frame == frame && _totalWeightCache.objId == objId && _totalWeightCache.path == listPath)
                return _totalWeightCache.total;

            var listProp = weightProp.serializedObject.FindProperty(listPath);
            if (listProp is not { isArray: true })
                return -1f;

            var total = 0f;
            for (var i = 0; i < listProp.arraySize; i++) {
                var element = listProp.GetArrayElementAtIndex(i);
                var w = element.FindPropertyRelative(LootEntryDataBase.NameOfWeight);
                if (w != null)
                    total += w.floatValue;
            }

            _totalWeightCache = (frame, objId, listPath, total);
            return total;
        }

        private static void DrawWeightQuantityRow(Rect row, SerializedProperty? weightProp, SerializedProperty? quantityProp) {
            var x = row.x;
            var y = row.y;
            var h = row.height;

            var weightLabelText = "Weight";
            if (weightProp != null) {
                var totalWeight = GetTotalWeight(weightProp);
                if (totalWeight > 0f) {
                    var pct = weightProp.floatValue / totalWeight * 100f;
                    weightLabelText = $"Weight ({pct:F1}%)";
                }
            }

            var weightLabel = new GUIContent(weightLabelText);
            var weightLabelW = EditorStyles.miniLabel.CalcSize(weightLabel).x + VerticalGap;

            var qtyLabel = new GUIContent("Quantity");
            var qtyLabelW = EditorStyles.miniLabel.CalcSize(qtyLabel).x + VerticalGap;

            var totalW = row.width - (HorizontalGap * 2f + 1);
            var weightAreaW = totalW * 0.33f;

            EditorGUI.LabelField(new Rect(x, y, weightLabelW, h), weightLabel, EditorStyles.miniLabel);
            x += weightLabelW;

            var weightFieldW = weightAreaW - weightLabelW;
            if (weightProp != null && weightFieldW > 0f)
                EditorGUI.PropertyField(new Rect(x, y, weightFieldW, h), weightProp, GUIContent.none);
            x = row.x + weightAreaW;

            x += HorizontalGap;
            EditorGUI.DrawRect(new Rect(x, y + VerticalGap, 1f, h - VerticalGap * 2f), new Color(0.5f, 0.5f, 0.5f, 0.5f));
            x += HorizontalGap;

            EditorGUI.LabelField(new Rect(x, y, qtyLabelW, h), qtyLabel, EditorStyles.miniLabel);
            x += qtyLabelW;

            var qtyFieldW = row.xMax - x;
            if (quantityProp != null && qtyFieldW > 0f)
                EditorGUI.PropertyField(new Rect(x, y, qtyFieldW, h), quantityProp, GUIContent.none);
        }

        private static void DrawItemRow(Rect row, SerializedProperty itemProp) {
            var x = row.x;
            var y = row.y;
            var h = row.height;

            var labelContent = new GUIContent("Item");
            var labelW = EditorStyles.miniLabel.CalcSize(labelContent).x + VerticalGap;
            EditorGUI.LabelField(new Rect(x, y, labelW, h), labelContent, EditorStyles.miniLabel);
            x += labelW;

            EditorGUI.PropertyField(new Rect(x, y, row.xMax - x, h), itemProp, GUIContent.none);
        }

        internal static string FormatRange(int min, int max) => min == max ? $"\u00d7{min}" : $"\u00d7[{min},{max}]";
    }
}