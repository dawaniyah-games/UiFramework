using System;
using UiFramework.Editor.Config;
using UiFramework.Editor.Scenes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UiFramework.Editor.Window.Tabs
{
    public class CanvasTab : BaseVisualElementTab
    {
        public override string TabName
        {
            get
            {
                return "Canvas";
            }
        }

        private UiEditorConfig editorConfig;
        private VisualElement inspectorContainer;
        private Label statusLabel;

        public override void OnCreateGUI(VisualElement root, UiEditorConfig config)
        {
            editorConfig = config;

            if (root == null)
            {
                return;
            }

            root.Add(new Label("Canvas & Element Scene Contract")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 18,
                    marginTop = 18,
                    marginBottom = 8,
                    marginLeft = 10
                }
            });

            Label description = new Label("Every UI element scene should be <Name>Root (UiElement) → Canvas → one stretched Root that holds all content, so show/hide animations move the whole screen. These canvas settings are written into scenes only when you use Apply, Fix or Reset.");
            description.style.whiteSpace = WhiteSpace.Normal;
            description.style.marginLeft = 10;
            description.style.marginRight = 10;
            description.style.marginBottom = 12;
            root.Add(description);

            if (config == null)
            {
                Label missing = new Label("Select or create a UiEditorConfig (General tab) first.");
                missing.style.color = Color.yellow;
                missing.style.marginLeft = 10;
                root.Add(missing);
                return;
            }

            UiCanvasSettings settings = UiEditorConfigUtility.GetOrCreateCanvasSettings(config);

            ObjectField settingsField = new ObjectField("Canvas Settings Asset")
            {
                objectType = typeof(UiCanvasSettings),
                allowSceneObjects = false,
                value = settings
            };
            settingsField.style.marginBottom = 6;
            settingsField.RegisterValueChangedCallback(evt =>
            {
                UiCanvasSettings picked = evt.newValue as UiCanvasSettings;
                Undo.RecordObject(editorConfig, "Change Canvas Settings Asset");
                editorConfig.CanvasSettings = picked;
                EditorUtility.SetDirty(editorConfig);
                AssetDatabase.SaveAssets();
                RebuildInspector(picked);
            });
            root.Add(settingsField);

            inspectorContainer = new VisualElement();
            inspectorContainer.style.marginBottom = 12;
            root.Add(inspectorContainer);
            RebuildInspector(settings);

            VisualElement buttonRow = new VisualElement();
            buttonRow.style.flexDirection = FlexDirection.Row;
            buttonRow.style.flexWrap = Wrap.Wrap;
            buttonRow.style.justifyContent = Justify.Center;
            buttonRow.style.marginTop = 6;
            buttonRow.style.marginBottom = 6;

            buttonRow.Add(CreateActionButton("Apply Canvas Settings To All", "Write these canvas settings into every element scene, except excluded and already matching ones.", UiElementSceneBatchOperations.ApplyCanvasSettingsToAll));
            buttonRow.Add(CreateActionButton("Validate All Element Scenes", "Check structure and canvas settings of every element scene and log a report.", UiElementSceneBatchOperations.ValidateAll));
            buttonRow.Add(CreateActionButton("Fix Structure In All Scenes", "Fix every auto-fixable structure issue after confirmation. Never deletes objects.", UiElementSceneBatchOperations.FixStructureInAll));
            root.Add(buttonRow);

            statusLabel = new Label(string.Empty);
            statusLabel.style.whiteSpace = WhiteSpace.Normal;
            statusLabel.style.alignSelf = Align.Center;
            statusLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            statusLabel.style.fontSize = 12;
            statusLabel.style.marginTop = 6;
            root.Add(statusLabel);
        }

        private Button CreateActionButton(string text, string tooltip, Func<UiEditorConfig, UiCanvasSettings, string> action)
        {
            Button button = new Button(() =>
            {
                UiCanvasSettings settings = UiEditorConfigUtility.GetCanvasSettingsOrDefaults(editorConfig);
                string result = action(editorConfig, settings);

                if (statusLabel != null)
                {
                    statusLabel.text = result;
                }
            })
            {
                text = text,
                tooltip = tooltip
            };

            button.style.width = 230;
            button.style.height = 30;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.marginLeft = 4;
            button.style.marginRight = 4;
            button.style.marginBottom = 4;
            return button;
        }

        private void RebuildInspector(UiCanvasSettings settings)
        {
            if (inspectorContainer == null)
            {
                return;
            }

            inspectorContainer.Clear();

            if (settings == null)
            {
                inspectorContainer.Add(new Label("No canvas settings asset assigned. One is created at " + UiCanvasSettings.DefaultAssetPath + " the next time it is needed."));
                return;
            }

            inspectorContainer.Add(new InspectorElement(settings));
        }
    }
}
