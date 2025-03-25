using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using System.IO;
using UnityEngine;

public class PlayerManager : MonoBehaviourPun
{
    private static string savePath => Path.Combine(Application.persistentDataPath, "playerData.json");
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

    /// <summary>
    /// 保存用户数据
    /// </summary>
    /// <param name="data"></param>
    public static void SaveData(PlayerData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("玩家数据已保存：" + savePath);
    }
    
    /// <summary>
    /// 加载用户数据
    /// </summary>
    /// <returns></returns>
    public static PlayerData LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            Debug.LogWarning("未找到存档文件，使用默认数据");
            return new PlayerData(50,50,50); // 返回默认数据
        }
    }
    
    void Awake()
    {
        animator = transform.Find("PlayerVisual").GetComponent<Animator>();
        nameText = GetComponentInChildren<NameText>();
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
        }
    }

    void Start()
    {
        if (isControllable)
        {
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
