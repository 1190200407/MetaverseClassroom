using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerManager : MonoBehaviourPun
{
    public bool isControllable = false;
    public Animator animator;

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
    }

    void Start()
    {
        if (isControllable)
        {
            if (GameSettings.instance.isVR)
            {
                gameObject.AddComponent<VRPlayerController>();
            }
            else 
            {
                gameObject.AddComponent<WinPlayerController>();
            }

            nameText = GetComponentInChildren<NameText>();

            if (!photonView.IsMine) 
            { 
                nameText.SetName(photonView.Owner.NickName);
                CharacterName = photonView.Owner.CustomProperties["CharacterName"] as string;
                return; 
            }
            
            nameText.gameObject.SetActive(false);
            CharacterName = string.Empty;
        }
    }
}
