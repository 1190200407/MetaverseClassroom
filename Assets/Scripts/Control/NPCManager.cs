using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    #region NPC列表
    public static List<NPCManager> allNPCs = new List<NPCManager>();
    #endregion

    #region NPC控制器
    public NPCController npcController;
    public Animator animator;
    public Rigidbody rb;
    public Transform handTransform;
    public Transform headTransform;
    #endregion

    #region NPC属性
    public NameText nameText;
    private string npcName;
    public string NPCName
    {
        get { return npcName; }
        set
        {
            npcName = value;
            nameText.SetName(value);
        }
    }

    private string characterName;
    public string CharacterName
    {
        get { return characterName; }
        set
        {
            characterName = value;
            UpdateCharacterVisual(value);
        }
    }

    private bool isMoving = false;
    public bool IsMoving
    {
        get { return isMoving; }
        set 
        { 
            isMoving = value;
            animator.SetFloat("SpeedY", value ? 1 : 0);
        }
    }

    private bool isSitting = false;
    public bool IsSitting
    {
        get { return isSitting; }
        set
        {
            isSitting = value;
            rb.isKinematic = value;
            animator.SetBool("IsSitting", value);
        }
    }

    private string sceneName;
    public string SceneName
    {
        get { return sceneName; }
        set { sceneName = value; }
    }
    #endregion

    #region 生命周期
    void Awake()
    {
        nameText = GetComponentInChildren<NameText>();
        rb = GetComponent<Rigidbody>();
        animator = transform.Find("NPCVisual").GetComponent<Animator>();

        npcController = gameObject.AddComponent<NPCController>();
        npcController.npcManager = this;
        allNPCs.Add(this);
    }

    private void OnDestroy()
    {
        allNPCs.Remove(this);
    }
    #endregion

    #region 其他方法
    public void Init(string name, string character, string sceneName)
    {
        nameText.gameObject.SetActive(true);

        NPCName = name;
        CharacterName = character;
        SceneName = sceneName;

        // 0.1秒后重置位置
        Invoke("ResetTransform", 0.1f);
    }

    public void ResetTransform()
    {
        transform.parent = SceneLoader.instance.PathToSceneObject[sceneName].transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void SpeechOutput()
    {

    }

    private void UpdateCharacterVisual(string newValue)
    {
        Transform npcVisual = transform.Find("NPCVisual");
        for (int i = 0; i < npcVisual.childCount; i++)
        {
            Transform child = npcVisual.GetChild(i);

            if (child.name == newValue)
            {
                child.gameObject.SetActive(true);
                animator.avatar = child.GetComponent<Animator>().avatar;
                child.SetAsFirstSibling();
                animator.Rebind();

                handTransform = child.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
                headTransform = child.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }
    #endregion
}
