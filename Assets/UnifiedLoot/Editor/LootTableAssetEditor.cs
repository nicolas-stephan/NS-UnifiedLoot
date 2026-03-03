using UnityEngine;
using UnityEditor;
using NS.UnifiedLoot;

namespace NS.UnifiedLoot.Editor {
    [CustomEditor(typeof(LootTableAssetBase), true)]
    public class LootTableAssetEditor : UnityEditor.Editor {
        // Column widths for the summary table
        private const float ColName = 160f;
        private const float ColW = 58f;
        private const float ColPct = 54f;
        private const float ColQty = 100f;

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            EditorGUILayout.Space(8f);
            DrawProbabilitySummary();
        }

        // ── Summary ──────────────────────────────────────────────────────────

        private void DrawProbabilitySummary() {
            var entriesProp = serializedObject.FindProperty(LootTableAsset<object>.NameOfEntries);
            if (entriesProp is not { isArray: true })
                return;

            var entryCount = entriesProp.arraySize;

            // Header label
            EditorGUILayout.LabelField("Probability Summary", EditorStyles.boldLabel);

            if (entryCount == 0) {
                EditorGUILayout.HelpBox("Table has no entries.", MessageType.Warning);
                return;
            }

            // Collect weights for percentage calculation
            var weights = new float[entryCount];
            var totalWeight = 0f;
            for (var i = 0; i < entryCount; i++) {
                var w = entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative(LootEntryDataBase.NameOfWeight);
                weights[i] = w?.floatValue ?? 0f;
                totalWeight += weights[i];
            }

            EditorGUILayout.LabelField($"Total weight: {totalWeight:F3}   ·   {entryCount} entries", EditorStyles.miniLabel);
            EditorGUILayout.Space(4f);

            // Column headers
            DrawColumnHeaders();

            // Separator
            var sepRect = GUILayoutUtility.GetRect(0f, 1f, GUILayout.ExpandWidth(true));
            sepRect.x += 4f;
            sepRect.width -= 8f;
            EditorGUI.DrawRect(sepRect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
            EditorGUILayout.Space(2f);

            // Rows
            using (new EditorGUI.DisabledScope(true)) {
                for (var i = 0; i < entryCount; i++)
                    DrawSummaryRow(entriesProp.GetArrayElementAtIndex(i), weights[i], totalWeight, i);
            }

            EditorGUILayout.Space(4f);
            DrawValidationWarnings(entriesProp, entryCount, weights);
        }

        private static void DrawColumnHeaders() {
            using (new EditorGUILayout.HorizontalScope()) {
                var headerStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
                EditorGUILayout.LabelField("Name", headerStyle, GUILayout.Width(ColName));
                EditorGUILayout.LabelField("Weight", headerStyle, GUILayout.Width(ColW));
                EditorGUILayout.LabelField("%", headerStyle, GUILayout.Width(ColPct));
                EditorGUILayout.LabelField("Quantity", headerStyle, GUILayout.Width(ColQty));
            }
        }

        private static void DrawSummaryRow(SerializedProperty elem, float weight, float totalWeight, int index) {
            var itemProp = elem.FindPropertyRelative(LootTableAsset<object>.LootEntryData.NameOfItem);
            var qtyProp = elem.FindPropertyRelative(LootEntryDataBase.NameOfQuantity);

            var itemLabel = GetItemLabel(itemProp, index);
            var pct = totalWeight > 0f ? weight / totalWeight * 100f : 0f;
            var qtyLabel = GetQuantityLabel(qtyProp);

            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField(itemLabel, EditorStyles.miniLabel, GUILayout.Width(ColName));
                EditorGUILayout.LabelField($"{weight:F3}", EditorStyles.miniLabel, GUILayout.Width(ColW));
                EditorGUILayout.LabelField($"{pct:F1}%", EditorStyles.miniLabel, GUILayout.Width(ColPct));
                EditorGUILayout.LabelField(qtyLabel, EditorStyles.miniLabel, GUILayout.Width(ColQty));
            }
        }

        // ── Warnings ─────────────────────────────────────────────────────────

        private static void DrawValidationWarnings(SerializedProperty entriesProp, int entryCount, float[] weights) {
            if (entryCount == 1)
                EditorGUILayout.HelpBox("Only one entry — weighted selection is degenerate.", MessageType.Warning);

            for (var i = 0; i < entryCount; i++) {
                if (weights[i] <= 0f)
                    EditorGUILayout.HelpBox($"Entry #{i} has weight ≤ 0.", MessageType.Warning);

                var itemProp = entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative(LootTableAsset<object>.LootEntryData.NameOfItem);
                if (itemProp is { propertyType: SerializedPropertyType.ObjectReference }
                    && itemProp.objectReferenceValue == null)
                    EditorGUILayout.HelpBox($"Entry #{i} has a null item reference.", MessageType.Warning);
            }
        }

        // ── Label helpers ─────────────────────────────────────────────────────

        private static string GetItemLabel(SerializedProperty? itemProp, int index) {
            if (itemProp == null)
                return $"Entry #{index}";
            return itemProp.propertyType switch {
                SerializedPropertyType.ObjectReference =>
                    itemProp.objectReferenceValue != null
                        ? itemProp.objectReferenceValue.name
                        : $"(null) #{index}",
                SerializedPropertyType.String =>
                    !string.IsNullOrEmpty(itemProp.stringValue)
                        ? $"\"{itemProp.stringValue}\""
                        : $"(empty) #{index}",
                SerializedPropertyType.Enum =>
                    itemProp.enumDisplayNames.Length > itemProp.enumValueIndex
                        ? itemProp.enumDisplayNames[itemProp.enumValueIndex]
                        : $"#{index}",
                _ => $"Entry #{index}"
            };
        }

        private static string GetQuantityLabel(SerializedProperty? qtyProp) {
            if (qtyProp == null)
                return "×1";
            var minProp = qtyProp.FindPropertyRelative(IntRange.NameOfMin);
            var maxProp = qtyProp.FindPropertyRelative(IntRange.NameOfMax);
            var min = minProp?.intValue ?? 1;
            var max = maxProp?.intValue ?? 1;
            return LootEntryDataDrawer.FormatRange(min, max);
        }
    }
}