using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Drink : InteractionScript
{
    private OutlineObject outline;

    public override void Init(SceneElement element)
    {
        base.Init(element);

        if (element.gameObject.GetComponent<OutlineObject>() == null)
        {
            outline = element.gameObject.AddComponent<OutlineObject>();
        }
        else
        {
            outline = element.gameObject.GetComponent<OutlineObject>();
        }
        outline.enabled = false;
    }

    public override void OnHoverEnter()
    {
        outline.enabled = true;
    }
    
    public override void OnHoverExit()
    {
        outline.enabled = false;
    }

    public override void OnSelectEnter()
    {
        base.OnSelectEnter();
        
        // TODO 播放动画和音效

        // 几秒后消失
        element.GetComponent<Collider>().enabled = false;
        element.StartCoroutine(Disappear());
    }

    public IEnumerator Disappear()
    {
        yield return new WaitForSeconds(1f);
        element.gameObject.SetActive(false);
    }
}
