using UnityEngine;

public class View : InteractionScript
{
    private Whiteboard whiteboard; 

    public override void Init(SceneElement element)
    {
        base.Init(element);
        whiteboard = element.gameObject.GetComponent<Whiteboard>();
    }

    public override void OnSelectEnter()
    {
        base.OnSelectEnter();
        if (PlayerManager.localPlayer.HavePermission(Permission.SwitchPPT))
        {
            whiteboard.ChangeSlide(1);
        }
    }
}