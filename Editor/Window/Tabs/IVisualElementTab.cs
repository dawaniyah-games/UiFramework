namespace UiFramework.Editor.Window.Tabs
{
    using UnityEngine.UIElements;
    using UiFramework.Editor.Config;

    public interface IVisualElementTab
    {
        string TabName { get; }
        void OnCreateGUI(VisualElement root, UiEditorConfig config);
    }
}
