using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    public Animator animator;
    public Rigidbody rb;
    public NameText nameText;

    private string characterName;
    public string CharacterName
    {
        get 
        {
            return characterName;
        }
        set
        {
            characterName = value;
            Transform playerVisual = transform.Find("PlayerVisual");
            for (int i = 0; i < playerVisual.childCount; i++)
            {
                Transform child = playerVisual.GetChild(i);
                if (child.name == characterName)
                {
                    child.gameObject.SetActive(true);
                    child.SetAsFirstSibling();
                    animator.Rebind();
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
    
    void Awake()
    {
        animator = transform.Find("PlayerVisual").GetComponent<Animator>();
        nameText = GetComponentInChildren<NameText>();
        rb = GetComponent<Rigidbody>();
    }
}
