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
            root.Add(new Label("⚙️ General Configuration")
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
            root.Add(CreatePathField("Element Output Path", config?.ElementsScriptPath));
            root.Add(CreatePathField("Element Scene Path", config?.ElementsScenePath));
            root.Add(CreatePathField("State Output Path", config?.StatesPath));
            root.Add(CreatePathField("State Definitions Path", config?.StateDefinitionsPath));
            root.Add(CreatePathField("State Registry Path", config?.StateRegistryPath));
            root.Add(CreatePathField("Runtime Config Output Path", config?.RuntimeConfigOutputPath));

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
            root.Add(buttonRow);

            statusLabel = new Label("");
            statusLabel.style.marginTop = 6;
            statusLabel.style.alignSelf = Align.Center;
            statusLabel.style.color = Color.green;
            statusLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            statusLabel.style.fontSize = 12;

            root.Add(statusLabel);
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
