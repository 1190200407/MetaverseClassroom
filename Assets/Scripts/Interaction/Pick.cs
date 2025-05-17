using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pick : InteractionScript
{
    public int holderId = -1;
    public string itemName;
    public bool isHeld => holderId != -1;
    public string itemKey;
    private OutlineObject outline;

    public Transform originalParent;
    public Vector3 originalPosition;
    public Quaternion originalRotation;

    #region 生命周期
    public override void OnEnable()
    {
        base.OnEnable();
        EventHandler.Register<PickItemCallback>(PickItemCallback);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        EventHandler.Unregister<PickItemCallback>(PickItemCallback);
    }
    public override void Init(SceneElement element)
    {
        base.Init(element);
        itemName = element.interactionContent;
        itemKey = string.Concat("pick_", ClassManager.instance.currentScene, "_", element.id);
        outline = element.gameObject.AddComponent<OutlineObject>();
        outline.enabled = false;
    }
    #endregion

    #region 交互响应
    public override void OnHoverEnter()
    {
        if (isHeld) return;
        outline.enabled = true;
    }
    public override void OnHoverExit()
    {
        outline.enabled = false;
    }
    public override void OnSelectEnter()
    {
        //当被选中，标识选中人
        EventHandler.Trigger(new PickItemEvent(){holderId = PlayerManager.localPlayer.netId,itemKey = itemKey});
    }
    #endregion
    
    #region 本地事件响应函数

    public void PickItemCallback(PickItemCallback callback)
    {
        if (itemKey == callback.itemKey)
        {
            holderId = (int)callback.holderId;
            //保存原始位置
            originalParent = element.transform.parent;
            originalPosition = element.transform.localPosition;
            originalRotation = element.transform.localRotation;
            //隐藏物体
            element.gameObject.SetActive(false);
            //被拾取后监听重置和放下事件
            EventHandler.Register<ResetItemCallback>(ResetItemCallback);
            EventHandler.Register<DropItemCallback>(DropItemCallback);
            //发送UI修改事件
            EventHandler.Trigger(new UIChangeEvent(){text = "持有物品："+itemName});
        }
    }

    public void ResetItemCallback(ResetItemCallback callback)
    {
        if (holderId == callback.holderId)
        {
            holderId = -1;
            //变回原位
            element.transform.SetParent(originalParent);
            element.transform.localPosition = originalPosition;
            element.transform.localRotation = originalRotation;
            //显示物体
            element.gameObject.SetActive(true);
            //重置后取消监听
            EventHandler.Unregister<ResetItemCallback>(ResetItemCallback);
            EventHandler.Unregister<DropItemCallback>(DropItemCallback);
            //发送UI修改事件
            EventHandler.Trigger(new UIChangeEvent(){text = ""});
        }
    }

    public void DropItemCallback(DropItemCallback callback)
    {
        if (holderId == callback.holderId)
        {
            holderId = -1;
            // 保存原始位置
            element.transform.SetParent(originalParent);
            element.transform.position = (callback.position+new Vector3(0,0.5f,0));
            element.transform.localRotation = originalRotation;
            //显示物体
            element.gameObject.SetActive(true);
            //重置后取消监听
            EventHandler.Unregister<ResetItemCallback>(ResetItemCallback);
            EventHandler.Unregister<DropItemCallback>(DropItemCallback);
            //发送UI修改事件
            EventHandler.Trigger(new UIChangeEvent(){text = ""});
        }
    }
    #endregion
}
