using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace NS.UnifiedLoot.Editor {
    [CustomEditor(typeof(LootTableAssetBase), true)]
    public class LootTableAssetEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawDefaultInspector();
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 2f);
            DrawProbabilitySummary();
            DrawCircularDependencyError();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCircularDependencyError() {
            var asset = target as LootTableAssetBase;
            if (asset == null)
                return;

            var stack = new List<LootTableAssetBase>();
            if (!asset.HasCircularDependency(stack))
                return;

            var chain = string.Join(" → ", stack.ConvertAll(s => s != null ? s.name : "(null)"));
            EditorGUILayout.HelpBox($"Circular Dependency Detected!\n{chain}", MessageType.Error);
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

            var headerStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
            var contentStyle = EditorStyles.miniLabel;
            var pad = EditorGUIUtility.standardVerticalSpacing * 2f;

            var weights = new float[entryCount];
            var totalWeight = 0f;
            var maxWeightW = headerStyle.CalcSize(new GUIContent("Weight")).x;
            var maxQtyW = headerStyle.CalcSize(new GUIContent("Quantity")).x;

            for (var i = 0; i < entryCount; i++) {
                var entry = entriesProp.GetArrayElementAtIndex(i);
                var wProp = entry.FindPropertyRelative(LootEntryDataBase.NameOfWeight);
                weights[i] = wProp?.floatValue ?? 0f;
                totalWeight += weights[i];

                var weightW = contentStyle.CalcSize(new GUIContent($"{weights[i]:F3}")).x;
                if (weightW > maxWeightW)
                    maxWeightW = weightW;

                var qtyProp = entry.FindPropertyRelative(LootEntryDataBase.NameOfQuantity);
                var qtyW = contentStyle.CalcSize(new GUIContent(GetQuantityLabel(qtyProp))).x;
                if (qtyW > maxQtyW)
                    maxQtyW = qtyW;
            }

            EditorGUILayout.LabelField($"Total weight: {totalWeight:F3}   ·   {entryCount} entries", EditorStyles.miniLabel);
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            var colWeight = maxWeightW + pad;
            var colPct = Mathf.Max(headerStyle.CalcSize(new GUIContent("%")).x, contentStyle.CalcSize(new GUIContent("100.0%")).x) + pad;
            var colQty = maxQtyW + pad;

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
            var subTableProp = elem.FindPropertyRelative(LootTableAsset<object>.LootEntryData.NameOfSubTable);
            var typeProp = elem.FindPropertyRelative(LootTableAsset<object>.LootEntryData.NameOfEntryType);
            var qtyProp = elem.FindPropertyRelative(LootEntryDataBase.NameOfQuantity);

            string itemLabel;
            string qtyLabel = GetQuantityLabel(qtyProp);

            if (typeProp is { enumValueIndex: (int)LootEntryType.SubTable })
                itemLabel = subTableProp.objectReferenceValue != null ? $"[Table] {subTableProp.objectReferenceValue.name}" : "[Table] (null)";
            else
                itemLabel = GetItemLabel(itemProp, index);

            var pct = totalWeight > 0f ? weight / totalWeight * 100f : 0f;

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

                var entry = entriesProp.GetArrayElementAtIndex(i);
                var itemProp = entry.FindPropertyRelative(LootTableAsset<object>.LootEntryData.NameOfItem);
                var subTableProp = entry.FindPropertyRelative(LootTableAsset<object>.LootEntryData.NameOfSubTable);
                var typeProp = entry.FindPropertyRelative(LootTableAsset<object>.LootEntryData.NameOfEntryType);

                var isSubTable = typeProp is { enumValueIndex: (int)LootEntryType.SubTable };

                if (isSubTable) {
                    if (subTableProp == null || subTableProp.objectReferenceValue == null)
                        EditorGUILayout.HelpBox($"Entry #{i} has a null sub-table reference.", MessageType.Warning);
                } else {
                    var hasItem = itemProp is { propertyType: SerializedPropertyType.ObjectReference }
                        ? itemProp.objectReferenceValue != null
                        : itemProp is not { propertyType: SerializedPropertyType.String } ||
                          !string.IsNullOrEmpty(itemProp.stringValue);

                    if (!hasItem)
                        EditorGUILayout.HelpBox($"Entry #{i} has a null item reference.", MessageType.Warning);
                }
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