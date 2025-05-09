using Unity.VisualScripting;
using UnityEngine;

public class UIHighlight:MonoBehaviour
{
    public string id;
    public GameObject highLightPart;
    private bool isHighlight;
    
    public bool IsHighlight
    {
        get{return isHighlight;}
        set
        {
            highLightPart.SetActive(value);
            isHighlight = value;
        }
    }
    public void OnUIHighlight(UIHighLightEvent @event)
    {
        if (@event.id == id)
        {
            IsHighlight = @event.isHighlighted;
        }
    }
    private void OnEnable()
    {
        EventHandler.Register<UIHighLightEvent>(OnUIHighlight);
    }

    private void OnDisable()
    {
        EventHandler.Unregister<UIHighLightEvent>(OnUIHighlight);
    }
}
