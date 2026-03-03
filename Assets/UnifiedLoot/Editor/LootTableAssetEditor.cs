using UnityEngine;
using UnityEditor;

namespace NS.UnifiedLoot.Editor {
    [CustomEditor(typeof(LootTableAssetBase), true)]
    public class LootTableAssetEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 2f);
            DrawProbabilitySummary();
        }

        private void DrawProbabilitySummary() {
            var entriesProp = serializedObject.FindProperty(LootTableAsset<object>.NameOfEntries);
            if (entriesProp is not { isArray: true })
                return;

            var entryCount = entriesProp.arraySize;

            EditorGUILayout.LabelField("Probability Summary", EditorStyles.boldLabel);

            if (entryCount == 0) {
                EditorGUILayout.HelpBox("Table has no entries.", MessageType.Warning);
                return;
            }

            var weights = new float[entryCount];
            var totalWeight = 0f;
            for (var i = 0; i < entryCount; i++) {
                var w = entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative(LootEntryDataBase.NameOfWeight);
                weights[i] = w?.floatValue ?? 0f;
                totalWeight += weights[i];
            }

            EditorGUILayout.LabelField($"Total weight: {totalWeight:F3}   ·   {entryCount} entries", EditorStyles.miniLabel);
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            // Calculate dynamic widths
            var headerStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
            var contentStyle = EditorStyles.miniLabel;
            var pad = EditorGUIUtility.standardVerticalSpacing * 2f;

            var colWeight = Mathf.Max(headerStyle.CalcSize(new GUIContent("Weight")).x, contentStyle.CalcSize(new GUIContent("000.000")).x) + pad;
            var colPct = Mathf.Max(headerStyle.CalcSize(new GUIContent("%")).x, contentStyle.CalcSize(new GUIContent("100.0%")).x) + pad;
            var colQty = Mathf.Max(headerStyle.CalcSize(new GUIContent("Quantity")).x, contentStyle.CalcSize(new GUIContent("x[000,000]")).x) + pad;

            DrawColumnHeaders(colWeight, colPct, colQty);

            var sepRect = GUILayoutUtility.GetRect(0f, 1f, GUILayout.ExpandWidth(true));
            sepRect.x += EditorGUIUtility.standardVerticalSpacing;
            sepRect.width -= EditorGUIUtility.standardVerticalSpacing * 2f;
            EditorGUI.DrawRect(sepRect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);

            using (new EditorGUI.DisabledScope(true))
                for (var i = 0; i < entryCount; i++)
                    DrawSummaryRow(entriesProp.GetArrayElementAtIndex(i), weights[i], totalWeight, i, colWeight, colPct, colQty);

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            DrawValidationWarnings(entriesProp, entryCount, weights);
        }

        private static void DrawColumnHeaders(float colW, float colPct, float colQty) {
            using (new EditorGUILayout.HorizontalScope()) {
                var headerStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
                EditorGUILayout.LabelField("Name", headerStyle, GUILayout.MinWidth(80f));
                EditorGUILayout.LabelField("Weight", headerStyle, GUILayout.Width(colW));
                EditorGUILayout.LabelField("%", headerStyle, GUILayout.Width(colPct));
                EditorGUILayout.LabelField("Quantity", headerStyle, GUILayout.Width(colQty));
            }
        }

        private static void DrawSummaryRow(SerializedProperty elem, float weight, float totalWeight, int index, float colW, float colPct, float colQty) {
            var itemProp = elem.FindPropertyRelative(LootTableAsset<object>.LootEntryData.NameOfItem);
            var qtyProp = elem.FindPropertyRelative(LootEntryDataBase.NameOfQuantity);

            var itemLabel = GetItemLabel(itemProp, index);
            var pct = totalWeight > 0f ? weight / totalWeight * 100f : 0f;
            var qtyLabel = GetQuantityLabel(qtyProp);

            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField(itemLabel, EditorStyles.miniLabel, GUILayout.MinWidth(80f));
                EditorGUILayout.LabelField($"{weight:F3}", EditorStyles.miniLabel, GUILayout.Width(colW));
                EditorGUILayout.LabelField($"{pct:F1}%", EditorStyles.miniLabel, GUILayout.Width(colPct));
                EditorGUILayout.LabelField(qtyLabel, EditorStyles.miniLabel, GUILayout.Width(colQty));
            }
        }

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