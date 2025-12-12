namespace UiFramework.Core
{
    using UnityEngine;

    public abstract class UiElement : MonoBehaviour, IUiElement
    {
        public virtual void Populate(object context = null) { }
    }
}
