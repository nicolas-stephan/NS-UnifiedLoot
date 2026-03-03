using UnityEngine;
using UnityEditor;

namespace NS.UnifiedLoot.Editor {
    [CustomPropertyDrawer(typeof(LootEntryDataBase), true)]
    public class LootEntryDataDrawer : PropertyDrawer {
        private const float Pad = 2f;
        private const float LabelW = 46f;
        private const float QtyLabelW = 58f;
        private const float WeightW = 52f;
        private const float PctW = 40f;
        private const float BadgeW = 48f;

        private static float RowH => EditorGUIUtility.singleLineHeight + Pad;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => RowH * 2 + Pad;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var weightProp = property.FindPropertyRelative(LootEntryDataBase.NameOfWeight);
            var quantityProp = property.FindPropertyRelative(LootEntryDataBase.NameOfQuantity);
            var itemProp = property.FindPropertyRelative(LootTableAsset<object>.LootEntryData.NameOfItem);

            var totalWeight = ComputeTotalWeight(property);
            var thisWeight = weightProp?.floatValue ?? 0f;
            var pct = totalWeight > 0f ? thisWeight / totalWeight * 100f : 0f;

            // Background tint
            var origBg = GUI.backgroundColor;
            GUI.backgroundColor = GetRowColor(thisWeight, totalWeight);
            GUI.Box(position, GUIContent.none);
            GUI.backgroundColor = origBg;

            EditorGUI.BeginProperty(position, label, property);
            var oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var x = position.x;
            var w = position.width;

            // ── Row 1: Weight [__]  xx.x%  │  Quantity [IntRange] ────────────
            var lh = EditorGUIUtility.singleLineHeight;
            DrawWeightQuantityRow(new Rect(x, position.y + Pad * 0.5f, w, lh), weightProp, quantityProp, pct);

            // ── Row 2: Item [field] ───────────────────────────────────────────
            if (itemProp != null)
                DrawItemRow(new Rect(x, position.y + RowH + Pad, w, lh), itemProp);

            EditorGUI.indentLevel = oldIndent;
            EditorGUI.EndProperty();
        }

        // ── Row helpers ──────────────────────────────────────────────────────

        private static void DrawWeightQuantityRow(Rect row, SerializedProperty? weightProp,
            SerializedProperty? quantityProp, float pct) {
            var x = row.x;
            var y = row.y;
            var h = row.height;

            var dimStyle = new GUIStyle(EditorStyles.miniLabel) {
                normal = { textColor = new Color(0.55f, 0.55f, 0.55f) }
            };

            // Percentage — dimmed, % prefix
            EditorGUI.LabelField(new Rect(x, y, PctW, h), $"%{pct:F1}", dimStyle);
            x += PctW + 3f;

            // Range badge — read-only (×1 or [min, max])
            if (quantityProp != null) {
                var minVal = quantityProp.FindPropertyRelative(IntRange.NameOfMin)?.intValue ?? 1;
                var maxVal = quantityProp.FindPropertyRelative(IntRange.NameOfMax)?.intValue ?? 1;
                EditorGUI.LabelField(new Rect(x, y, BadgeW, h), FormatRange(minVal, maxVal), dimStyle);
            }

            x += BadgeW + 4f;

            // "Weight:" label + field
            EditorGUI.LabelField(new Rect(x, y, LabelW, h), "Weight:", EditorStyles.miniLabel);
            x += LabelW + 2f;
            if (weightProp != null)
                EditorGUI.PropertyField(new Rect(x, y, WeightW, h), weightProp, GUIContent.none);
            x += WeightW + 4f;

            // Thin vertical divider
            EditorGUI.DrawRect(new Rect(x, y + 2f, 1f, h - 4f), new Color(0.5f, 0.5f, 0.5f, 0.5f));
            x += 5f;

            // "Quantity:" label + IntRange drawer — fills remaining width
            EditorGUI.LabelField(new Rect(x, y, QtyLabelW, h), "Quantity:", EditorStyles.miniLabel);
            x += QtyLabelW + 2f;
            var remaining = row.x + row.width - x;
            if (quantityProp != null && remaining > 0f)
                EditorGUI.PropertyField(new Rect(x, y, remaining, h), quantityProp, GUIContent.none);
        }

        private static void DrawItemRow(Rect row, SerializedProperty itemProp) {
            var x = row.x;
            var y = row.y;
            var h = row.height;

            // "Item" label
            EditorGUI.LabelField(new Rect(x, y, LabelW, h), "Item", EditorStyles.miniLabel);
            x += LabelW + 2f;

            // Item field — fills remaining width
            EditorGUI.PropertyField(new Rect(x, y, row.x + row.width - x, h), itemProp, GUIContent.none);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        internal static string FormatRange(int min, int max)
            => min == max ? $"\u00d7{min}" : $"\u00d7[{min},{max}]";

        private static float ComputeTotalWeight(SerializedProperty property) {
            var path = property.propertyPath;
            var bracket = path.LastIndexOf('[');
            if (bracket <= 0)
                return 0f;

            var withoutIndex = path[..bracket];
            const string suffix = ".Array.data";
            if (!withoutIndex.EndsWith(suffix))
                return 0f;

            var arrayPath = withoutIndex[..^suffix.Length];
            var arrayProp = property.serializedObject.FindProperty(arrayPath);
            if (arrayProp is not { isArray: true })
                return 0f;

            var total = 0f;
            for (var i = 0; i < arrayProp.arraySize; i++) {
                var w = arrayProp.GetArrayElementAtIndex(i).FindPropertyRelative(LootEntryDataBase.NameOfWeight);
                if (w != null)
                    total += w.floatValue;
            }

            return total;
        }

        private static Color GetRowColor(float weight, float totalWeight) {
            if (weight <= 0f)
                return new Color(0.55f, 0.55f, 0.55f, 0.25f);
            if (totalWeight <= 0f)
                return Color.white;
            var ratio = weight / totalWeight;
            return ratio switch {
                >= 0.4f => new Color(0.4f, 0.85f, 0.4f, 0.25f),
                >= 0.15f => new Color(0.85f, 0.85f, 0.3f, 0.25f),
                _ => Color.white
            };
        }
    }
}