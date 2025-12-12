namespace UiFramework.Editor.Window.Tabs
{
    using UnityEngine;
    using UnityEngine.UIElements;
    using UiFramework.Editor.Config;

    public class GeneralTab : BaseVisualElementTab
    {
        public override string TabName => "General";

        private System.Action _onLoadOrCreate;
        private System.Action _onBuildRuntimeUi;
        private Label _statusLabel;


        public void SetLoadOrCreateCallback(System.Action callback)
        {
            _onLoadOrCreate = callback;
        }

        public void SetRuntimeUiBuildCallback(System.Action callback)
        {
            _onBuildRuntimeUi = callback;
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

            root.Add(CreatePathField("UI Setup Asset Path", config?.UiSetupAssetPath));
            root.Add(CreatePathField("Element Output Path", config?.ElementsScriptPath));
            root.Add(CreatePathField("Element Scene Path", config?.ElementsScenePath));
            root.Add(CreatePathField("State Output Path", config?.StatesPath));
            root.Add(CreatePathField("State Registry Path", config?.StateRegistryPath));
            root.Add(CreatePathField("Runtime Config Output Path", config?.RuntimeConfigOutputPath));

            var buttonRow = new VisualElement
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

            var loadBtn = new Button(() => _onLoadOrCreate?.Invoke()) { text = "🔄 Create Config" };

            var buildBtn = new Button(() =>
            {
                _onBuildRuntimeUi?.Invoke();
                _statusLabel.text = "✅ Built runtime UiConfig successfully.";
            })
            { text = "⚙️ Build Runtime UiConfig" };


            foreach (var btn in new[] { loadBtn, buildBtn })
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

            _statusLabel = new Label("");
            _statusLabel.style.marginTop = 6;
            _statusLabel.style.alignSelf = Align.Center;
            _statusLabel.style.color = Color.green;
            _statusLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            _statusLabel.style.fontSize = 12;

            root.Add(_statusLabel);
        }

        private VisualElement CreatePathField(string label, string value)
        {
            var container = new VisualElement { style = { marginBottom = 8 } };
            container.Add(new Label(label));
            var field = new TextField { value = value ?? "", isReadOnly = true };
            container.Add(field);
            return container;
        }
    }
}
