using UnityEngine;

public class BasePanel
{
    public UIType uiType;
    public GameObject ActiveObj;

    public BasePanel(UIType uiType){
        this.uiType = uiType;
    }

    public virtual void OnStart() {
        UIMethods.instance.GetOrAddComponent<CanvasGroup>(ActiveObj).interactable = true;
    }
    public virtual void OnEnable() {
        UIMethods.instance.GetOrAddComponent<CanvasGroup>(ActiveObj).interactable = true;
    }
    public virtual void OnUpdate() {
    }

    public virtual void OnDisable() {
        UIMethods.instance.GetOrAddComponent<CanvasGroup>(ActiveObj).interactable = false;
    }
    public virtual void OnDestroy() {
        UIMethods.instance.GetOrAddComponent<CanvasGroup>(ActiveObj).interactable = false;
    }
}
