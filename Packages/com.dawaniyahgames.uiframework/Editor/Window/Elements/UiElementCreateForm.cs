using System;
using System.IO;
using UiFramework.Editor.Scenes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UiFramework.Editor.Window.Elements
{
    // "New UI Element" form. Collects the inputs and hands them to the owner; it doesn't create anything itself.
    public sealed class UiElementCreateForm
    {
        private readonly Action<UiElementSceneCreator.Options> onCreate;
        private readonly Action onCancel;

        // Remembered between openings of the form.
        private bool includeParams;
        private bool includeReference;

        public UiElementCreateForm(Action<UiElementSceneCreator.Options> onCreate, Action onCancel)
        {
            this.onCreate = onCreate;
            this.onCancel = onCancel;
        }

        public VisualElement Build()
        {
            VisualElement section = UiEditorStyles.CreateSection("New UI Element", "Generates the UiElement script and a scene laid out as <Name>Root → Canvas → Root, using the Global Canvas Settings (General tab). Put all content under Root.");
            SceneAsset selectedScene = null;

            TextField nameField = new TextField("Name")
            {
                tooltip = "Class and scene name, e.g. ProfileUiElement"
            };
            UiEditorStyles.SetLabelWidth(nameField);
            section.Add(nameField);

            ObjectField sceneField = new ObjectField("From Existing Scene (optional)")
            {
                objectType = typeof(SceneAsset),
                allowSceneObjects = false,
                tooltip = "Turn an existing scene into an element. Its root objects are moved under <Name>Root, and you are offered a structure fix."
            };
            UiEditorStyles.SetLabelWidth(sceneField);
            sceneField.RegisterValueChangedCallback(evt =>
            {
                selectedScene = evt.newValue as SceneAsset;
            });
            section.Add(sceneField);

            Toggle paramsToggle = new Toggle("Include Parameters Class") { value = includeParams };
            UiEditorStyles.SetLabelWidth(paramsToggle);
            paramsToggle.RegisterValueChangedCallback(evt =>
            {
                includeParams = evt.newValue;
            });
            section.Add(paramsToggle);

            Toggle referenceToggle = new Toggle("Include Reference Class") { value = includeReference };
            UiEditorStyles.SetLabelWidth(referenceToggle);
            referenceToggle.RegisterValueChangedCallback(evt =>
            {
                includeReference = evt.newValue;
            });
            section.Add(referenceToggle);

            VisualElement buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.justifyContent = Justify.FlexEnd;
            buttons.style.marginTop = 8;

            Button cancel = UiEditorStyles.CreateButton("Cancel", null, () =>
            {
                onCancel?.Invoke();
            }, false);
            cancel.style.minWidth = 90;
            cancel.style.height = 26;
            buttons.Add(cancel);

            Button create = UiEditorStyles.CreateButton("➕ Create UI Element", null, () =>
            {
                string elementName = nameField.value != null ? nameField.value.Trim() : string.Empty;

                if (selectedScene != null && string.IsNullOrEmpty(elementName))
                {
                    elementName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(selectedScene));
                }

                if (string.IsNullOrEmpty(elementName))
                {
                    EditorUtility.DisplayDialog("New UI Element", "Enter a name for the new element.", "OK");
                    return;
                }

                onCreate(new UiElementSceneCreator.Options
                {
                    ElementName = elementName,
                    ExistingScene = selectedScene,
                    IncludeParams = includeParams,
                    IncludeReference = includeReference
                });
            }, true);
            create.style.minWidth = 160;
            create.style.height = 26;
            buttons.Add(create);

            section.Add(buttons);
            return section;
        }
    }
}
