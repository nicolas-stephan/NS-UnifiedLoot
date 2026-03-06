using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Core;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Preview;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Strategies;
using NS.UnifiedLoot.UnifiedLoot.Runtime.Tables;

namespace NS.UnifiedLoot.Editor {
    [CustomEditor(typeof(LootTableAssetBase), true)]
    public class LootTableAssetEditor : UnityEditor.Editor {
        private bool _showPreview;
        private float _previewWeightMultiplier = 2f;

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawDefaultInspector();
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 2f);
            DrawProbabilitySummary();
            DrawCircularDependencyError();
            
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 2f);
            DrawPipelinePreview();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPipelinePreview() {
            _showPreview = EditorGUILayout.BeginFoldoutHeaderGroup(_showPreview, "Pipeline Preview Simulation");
            if (_showPreview) {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.HelpBox("This simulates a simple 'ModifyWeightStrategy' that multiplies all weights in the table. " +
                                       "It demonstrates how the pipeline can change probabilities before rolling.", MessageType.Info);

                using (new EditorGUILayout.HorizontalScope()) {
                    EditorGUILayout.PrefixLabel("Weight Multiplier");
                    _previewWeightMultiplier = EditorGUILayout.FloatField(_previewWeightMultiplier);
                }

                if (GUILayout.Button("Show Preview Results")) {
                    // Logic to show preview
                }
                
                var asset = target as LootTableAssetBase;
                if (asset != null) {
                    // Use reflection to call ToTable on the generic LootTableAsset<TItem>
                    // For simplicity in this editor, we'll try to find the generic type TItem.
                    var genericType = asset.GetType().BaseType;
                    while (genericType != null && (!genericType.IsGenericType || genericType.GetGenericTypeDefinition() != typeof(LootTableAsset<>))) {
                        genericType = genericType.BaseType;
                    }

                    if (genericType != null) {
                        var tItem = genericType.GetGenericArguments()[0];
                        var toTableMethod = genericType.GetMethod("ToTable");
                        if (toTableMethod != null) {
                            var table = toTableMethod.Invoke(asset, null);
                            
                            // We need to call LootPreviewer.GetPreview<TItem>
                            var previewerType = typeof(LootPreviewer);
                            var getPreviewMethod = previewerType.GetMethod("GetPreview");
                            if (getPreviewMethod != null) {
                                var genericGetPreview = getPreviewMethod.MakeGenericMethod(tItem);
                                
                                // Create a pipeline for simulation
                                var pipelineType = typeof(LootPipeline<>).MakeGenericType(tItem);
                                var pipeline = System.Activator.CreateInstance(pipelineType, new object[] { null });
                                
                                // Add a ModifyWeightStrategy<TItem>
                                var modifyWeightType = typeof(ModifyWeightStrategy<>).MakeGenericType(tItem);
                                var multiplierMethod = modifyWeightType.GetMethod("Multiplier");
                                if (multiplierMethod != null) {
                                    var strategy = multiplierMethod.Invoke(null, new object[] { _previewWeightMultiplier });
                                    var addStrategyMethod = pipelineType.GetMethod("AddStrategy");
                                    if (addStrategyMethod != null) {
                                        addStrategyMethod.Invoke(pipeline, new object[] { strategy });
                                    }
                                }
                                
                                var preview = genericGetPreview.Invoke(null, new object[] { pipeline, table, null });
                                if (preview != null) {
                                    DrawPreviewTable(preview, tItem);
                                }
                            }
                        }
                    }
                }
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawPreviewTable(object preview, System.Type tItem) {
            var entriesProp = preview.GetType().GetProperty("Entries");
            var totalWeightProp = preview.GetType().GetProperty("TotalWeight");
            if (entriesProp == null || totalWeightProp == null) return;

            var entries = entriesProp.GetValue(preview) as System.Collections.IEnumerable;
            var totalWeight = (float)totalWeightProp.GetValue(preview);

            if (entries == null) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Simulated Total weight: {totalWeight:F3}", EditorStyles.miniBoldLabel);

            var headerStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
            var contentStyle = EditorStyles.miniLabel;
            
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField("Item", headerStyle, GUILayout.MinWidth(80f));
                EditorGUILayout.LabelField("Orig. W", headerStyle, GUILayout.Width(50f));
                EditorGUILayout.LabelField("Mod. W", headerStyle, GUILayout.Width(50f));
                EditorGUILayout.LabelField("New %", headerStyle, GUILayout.Width(50f));
                EditorGUILayout.LabelField("New Q", headerStyle, GUILayout.Width(50f));
            }

            foreach (var entry in entries) {
                var itemProp = entry.GetType().GetProperty("Item");
                var origWProp = entry.GetType().GetProperty("OriginalWeight");
                var modWProp = entry.GetType().GetProperty("ModifiedWeight");
                var probProp = entry.GetType().GetProperty("Probability");
                var modQtyProp = entry.GetType().GetProperty("ModifiedQuantity");

                var itemValue = itemProp.GetValue(entry);
                var origW = (float)origWProp.GetValue(entry);
                var modW = (float)modWProp.GetValue(entry);
                var prob = (float)probProp.GetValue(entry);
                var modQty = modQtyProp.GetValue(entry);

                using (new EditorGUILayout.HorizontalScope()) {
                    EditorGUILayout.LabelField(itemValue?.ToString() ?? "None", contentStyle, GUILayout.MinWidth(80f));
                    EditorGUILayout.LabelField($"{origW:F2}", contentStyle, GUILayout.Width(50f));
                    EditorGUILayout.LabelField($"{modW:F2}", contentStyle, GUILayout.Width(50f));
                    EditorGUILayout.LabelField($"{prob * 100f:F1}%", contentStyle, GUILayout.Width(50f));
                    EditorGUILayout.LabelField(modQty?.ToString() ?? "1", contentStyle, GUILayout.Width(50f));
                }
            }
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
                SerializedPropertyType.Generic => 
                    GetGenericLabel(itemProp, index),
                _ => $"Entry #{index}"
            };
        }

        private static string GetGenericLabel(SerializedProperty prop, int index) {
            var nameProp = prop.FindPropertyRelative("ItemName") 
                        ?? prop.FindPropertyRelative("Name") 
                        ?? prop.FindPropertyRelative("name");
            
            if (nameProp != null && nameProp.propertyType == SerializedPropertyType.String) {
                return !string.IsNullOrEmpty(nameProp.stringValue) ? nameProp.stringValue : $"(empty name) #{index}";
            }
            
            return $"Entry #{index}";
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