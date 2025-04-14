using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : UnitySingleton<UIManager>
{
    public Dictionary<string,GameObject> dict_uiObject;
    public Stack<BasePanel> stack_ui;
    public CanvasGroup CanvasObj;
    public CanvasGroup MessageObj;
    private Text messageText;
    private Coroutine showMessageCoroutine;

    private UIMethods uIMethods;
    public UIMethods UIMethods {get =>uIMethods;}

    private void Start() {
        dict_uiObject = new Dictionary<string, GameObject>();
        stack_ui = new Stack<BasePanel>();
        uIMethods = new UIMethods();
        CanvasObj = uIMethods.FindCanvas().GetComponent<CanvasGroup>();
        messageText = MessageObj.GetComponentInChildren<Text>();
        MessageObj.gameObject.SetActive(false);

        Push(new StartPanel(new UIType("Panels/StartPanel", "StartPanel")));
    }

    private void Update() {
        if(stack_ui.Count>0){
            stack_ui.Peek().OnUpdate();
        }
    }

    public GameObject GetSingleObject(UIType ui_info){
        if(dict_uiObject.ContainsKey(ui_info.Name)){
            return dict_uiObject[ui_info.Name];
        }
        if(CanvasObj == null){
            CanvasObj = UIMethods.instance.FindCanvas().GetComponent<CanvasGroup>();
            if(CanvasObj == null) return null;
        }
        GameObject gameObject = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(ui_info.Path),CanvasObj.transform);
        return gameObject;
    }

    public void Push(BasePanel basePanel_push){
        if(stack_ui.Count>0){
            stack_ui.Peek().OnDisable();
        }

        GameObject BasePanel_pushObj = GetSingleObject(basePanel_push.uiType);
        dict_uiObject.Add(basePanel_push.uiType.Name, BasePanel_pushObj);
        basePanel_push.ActiveObj = BasePanel_pushObj;

        if (stack_ui.Count == 0){
            stack_ui.Push(basePanel_push);
        }else{
            if(stack_ui.Peek().uiType.Name != basePanel_push.uiType.Name)
            {
                stack_ui.Push(basePanel_push);
            }
        }

        basePanel_push.OnStart();
        basePanel_push.OnEnable();
        EnableInteraction();
    }
    public void Pop(bool isLoad){
        if(stack_ui.Count>0){
            stack_ui.Peek().OnDisable();
            stack_ui.Peek().OnDestroy();

            GameObject.Destroy(dict_uiObject[stack_ui.Peek().uiType.Name]);
            dict_uiObject.Remove(stack_ui.Peek().uiType.Name);
            stack_ui.Pop();

            if(isLoad)
                Pop(true);
            else{
                if(stack_ui.Count>0)
                    stack_ui.Peek().OnEnable();
            }
        }
    }

    /// <summary>
    /// 弹出直到满足条件
    /// </summary>
    /// <param name="condition">条件</param>
    public void PopUntil(Func<bool> condition)
    {
        while (stack_ui.Count > 0 && !condition())
        {
            Pop(false);
        }
    }

    public void EnableInteraction()
    {
        CanvasObj.interactable = true;
    }

    public void DisableInteraction()
    {
        CanvasObj.interactable = false;
    }

    public void ShowMessage(string message, float time)
    {
        messageText.text = message;
        MessageObj.gameObject.SetActive(true);
        
        showMessageCoroutine = StartCoroutine(ShowMessageCoroutine(time));
    }

    private IEnumerator ShowMessageCoroutine(float time)
    {
        yield return new WaitForSecondsRealtime(time);

        MessageObj.gameObject.SetActive(false);
    }

    public void CloseMessage()
    {
        StopCoroutine(showMessageCoroutine);
        MessageObj.gameObject.SetActive(false);
    }
}
