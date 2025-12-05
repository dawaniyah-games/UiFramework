namespace UiFramework.Editor.Window.Tabs
{
    using UnityEngine.UIElements;
    using UiFramework.Editor.Config;

    public abstract class BaseVisualElementTab : IVisualElementTab
    {
        public abstract string TabName { get; }
        public abstract void OnCreateGUI(VisualElement root, UiEditorConfig config);
    }
}
