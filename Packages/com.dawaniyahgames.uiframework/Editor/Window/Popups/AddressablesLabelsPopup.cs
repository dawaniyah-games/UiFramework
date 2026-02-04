using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace UiFramework.Editor.Window.Popups
{
    public sealed class AddressablesLabelsPopup : PopupWindowContent
    {
        private readonly AddressableAssetSettings settings;
        private readonly HashSet<string> selectedLabels;
        private readonly Action<HashSet<string>> onChanged;

        private Vector2 scroll;
        private string newLabel;

        public AddressablesLabelsPopup(AddressableAssetSettings settings, HashSet<string> selectedLabels, Action<HashSet<string>> onChanged)
        {
            this.settings = settings;
            this.selectedLabels = selectedLabels;
            this.onChanged = onChanged;
            newLabel = string.Empty;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 360);
        }

        public override void OnGUI(Rect rect)
        {
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Addressables settings not found.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Addressables Labels", EditorStyles.boldLabel);

            DrawCreateNewLabel();

            EditorGUILayout.Space(6);

            scroll = EditorGUILayout.BeginScrollView(scroll);

            IList<string> labels = settings.GetLabels();
            if (labels == null || labels.Count == 0)
            {
                EditorGUILayout.HelpBox("No labels exist yet.", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < labels.Count; i++)
                {
                    string label = labels[i];
                    bool before = selectedLabels.Contains(label);
                    bool after = EditorGUILayout.ToggleLeft(label, before);

                    if (after != before)
                    {
                        if (after)
                        {
                            selectedLabels.Add(label);
                        }
                        else
                        {
                            selectedLabels.Remove(label);
                        }

                        InvokeChanged();
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select None"))
            {
                if (selectedLabels.Count > 0)
                {
                    selectedLabels.Clear();
                    InvokeChanged();
                }
            }
            if (GUILayout.Button("Close"))
            {
                editorWindow.Close();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCreateNewLabel()
        {
            EditorGUILayout.BeginHorizontal();
            newLabel = EditorGUILayout.TextField(newLabel);
            if (GUILayout.Button("Add", GUILayout.Width(60)))
            {
                TryAddNewLabel(newLabel);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void TryAddNewLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return;
            }

            string trimmed = label.Trim();

            IList<string> labels = settings.GetLabels();
            if (labels == null || !labels.Contains(trimmed))
            {
                settings.AddLabel(trimmed);
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }

            if (!selectedLabels.Contains(trimmed))
            {
                selectedLabels.Add(trimmed);
                InvokeChanged();
            }

            newLabel = string.Empty;
            GUI.FocusControl(null);
        }

        private void InvokeChanged()
        {
            if (onChanged != null)
            {
                onChanged.Invoke(selectedLabels);
            }
        }
    }
}
