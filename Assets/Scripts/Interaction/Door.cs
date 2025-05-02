using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Door : InteractionScript
{
    private bool isOpen = false;
    public Animator openandclose;

    public override void Init(SceneElement element)
    {
        base.Init(element);
        openandclose = element.GetComponent<Animator>();
        isOpen = false;
    }

    public override void OnSelectEnter()
    {
        if (isOpen)
        {
            element.StartCoroutine(closing());
        }
        else
        {
            element.StartCoroutine(opening());
        }
    }

    IEnumerator opening()
    {
        openandclose.Play("Opening");
        isOpen = true;
        yield return new WaitForSeconds(.5f);
    }

    IEnumerator closing()
    {
        openandclose.Play("Closing");
        isOpen = false;
        yield return new WaitForSeconds(.5f);
    }
}
