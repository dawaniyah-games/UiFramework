using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UiFramework.Editor.Config;

namespace UiFramework.Editor.Window.Tabs
{
    public class GeneralTab : BaseVisualElementTab
    {
        public override string TabName
        {
            get
            {
                return "General";
            }
        }

        private System.Action onLoadOrCreate;
        private System.Action onBuildRuntimeUi;
        private Label statusLabel;
        private UiEditorConfig editorConfig;
        private VisualElement canvasInspectorContainer;


        public void SetLoadOrCreateCallback(System.Action callback)
        {
            onLoadOrCreate = callback;
        }

        public void SetRuntimeUiBuildCallback(System.Action callback)
        {
            onBuildRuntimeUi = callback;
        }

        public override void OnCreateGUI(VisualElement root, UiEditorConfig config)
        {
            editorConfig = config;

            ScrollView content = new ScrollView(ScrollViewMode.Vertical);
            content.style.flexGrow = 1;
            root.Add(content);

            content.Add(new Label("⚙️ General Configuration")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 18,
                    marginTop = 18,
                    marginBottom = 18,
                    marginLeft = 10,
                }
            });

            // Hide internal UI Setup asset path from the editor UI
            content.Add(CreatePathField("Element Output Path", config?.ElementsScriptPath));
            content.Add(CreatePathField("Element Scene Path", config?.ElementsScenePath));
            content.Add(CreatePathField("State Output Path", config?.StatesPath));
            content.Add(CreatePathField("State Definitions Path", config?.StateDefinitionsPath));
            content.Add(CreatePathField("State Registry Path", config?.StateRegistryPath));
            content.Add(CreatePathField("Runtime Config Output Path", config?.RuntimeConfigOutputPath));

            VisualElement buttonRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.Center,
                    alignItems = Align.Center,
                    marginTop = 12,
                    marginBottom = 6
                }
            };

            Button loadBtn = new Button(() =>
            {
                if (onLoadOrCreate != null)
                {
                    onLoadOrCreate.Invoke();
                }
            })
            {
                text = "🔄 Create Config"
            };

            Button buildBtn = new Button(() =>
            {
                if (onBuildRuntimeUi != null)
                {
                    onBuildRuntimeUi.Invoke();
                }

                statusLabel.text = "✅ Built runtime UiConfig successfully.";
            })
            { text = "⚙️ Build Runtime UiConfig" };

            foreach (Button btn in new Button[] { loadBtn, buildBtn })
            {
                btn.style.width = 220;
                btn.style.height = 32;
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                btn.style.fontSize = 13;
                btn.style.marginLeft = 6;
                btn.style.marginRight = 6;
            }

            buttonRow.Add(loadBtn);
            buttonRow.Add(buildBtn);
            content.Add(buttonRow);

            statusLabel = new Label("");
            statusLabel.style.marginTop = 6;
            statusLabel.style.alignSelf = Align.Center;
            statusLabel.style.color = Color.green;
            statusLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            statusLabel.style.fontSize = 12;

            content.Add(statusLabel);

            AddCanvasSettingsSection(content, config);
        }

        private void AddCanvasSettingsSection(VisualElement root, UiEditorConfig config)
        {
            root.Add(new Label("Global Canvas Settings")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14,
                    marginTop = 20,
                    marginBottom = 4,
                    marginLeft = 10
                }
            });

            Label hint = new Label("One set of Canvas and Canvas Scaler values shared by every UI element scene in the project. New elements are created with them, and the Elements tab checks each scene against them (Fix Structure, Reset Canvas, Apply Canvas To All). Scenes are only changed by those actions, and sorting order and cameras are never touched.");
            hint.style.whiteSpace = WhiteSpace.Normal;
            hint.style.fontSize = 11;
            hint.style.marginLeft = 10;
            hint.style.marginRight = 10;
            hint.style.marginBottom = 6;
            root.Add(hint);

            if (config == null)
            {
                Label missing = new Label("Create or select a UI Config above to edit the global canvas settings.");
                missing.style.color = Color.yellow;
                missing.style.marginLeft = 10;
                root.Add(missing);
                return;
            }

            UiGlobalCanvasSettings settings = UiEditorConfigUtility.GetOrCreateCanvasSettings(config);

            ObjectField settingsField = new ObjectField("Settings Asset")
            {
                objectType = typeof(UiGlobalCanvasSettings),
                allowSceneObjects = false,
                value = settings
            };
            settingsField.style.marginLeft = 6;
            settingsField.style.marginRight = 6;
            settingsField.RegisterValueChangedCallback(evt =>
            {
                UiGlobalCanvasSettings picked = evt.newValue as UiGlobalCanvasSettings;
                Undo.RecordObject(editorConfig, "Change Global Canvas Settings Asset");
                editorConfig.GlobalCanvasSettings = picked;
                EditorUtility.SetDirty(editorConfig);
                AssetDatabase.SaveAssets();
                RebuildCanvasInspector(picked);
            });
            root.Add(settingsField);

            canvasInspectorContainer = new VisualElement();
            canvasInspectorContainer.style.marginBottom = 12;
            root.Add(canvasInspectorContainer);
            RebuildCanvasInspector(settings);
        }

        private void RebuildCanvasInspector(UiGlobalCanvasSettings settings)
        {
            if (canvasInspectorContainer == null)
            {
                return;
            }

            canvasInspectorContainer.Clear();

            if (settings == null)
            {
                Label empty = new Label("No settings asset assigned. One is created at " + UiGlobalCanvasSettings.DefaultAssetPath + " the next time it is needed.");
                empty.style.marginLeft = 10;
                canvasInspectorContainer.Add(empty);
                return;
            }

            canvasInspectorContainer.Add(new InspectorElement(settings));
        }

        private VisualElement CreatePathField(string label, string value)
        {
            VisualElement container = new VisualElement { style = { marginBottom = 8 } };
            container.Add(new Label(label));
            TextField field = new TextField { value = value ?? "", isReadOnly = true };
            container.Add(field);
            return container;
        }
    }
}
