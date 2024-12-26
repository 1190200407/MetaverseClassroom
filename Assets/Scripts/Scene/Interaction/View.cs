using UnityEngine;

public class View : InteractionScript
{
    private Whiteboard whiteboard; 

    public override void Init(SceneElement element, string interactionType, string interactionContent)
    {
        base.Init(element, interactionType, interactionContent);
        whiteboard = element.gameObject.GetComponent<Whiteboard>();
    }

    public override void OnSelectEnter()
    {
        base.OnSelectEnter();
        whiteboard.NextSlide();
    }
}