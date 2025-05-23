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
    public Vector3 offset;

    public Transform originalParent;
    public Vector3 originalPosition;
    public Quaternion originalRotation;

    [Serializable]
    public class PickContent
    {
        public string itemName;
        public Vector3 offset;
    }

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
        // interactionContent 中有两个属性，一个是 itemName，一个是 offset
        // 格式和Json一样，需要解析
        var content = JsonUtility.FromJson<PickContent>(element.interactionContent);
        itemName = content.itemName;
        offset = content.offset;
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
        EventHandler.Trigger(new PickItemEvent(){holderId = PlayerManager.localPlayer.netId, itemKey = itemKey, elementId = element.id});
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
            EventHandler.Trigger(new UIChangeEvent(){text = "持有物品：" + itemName, holderId = callback.holderId});
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
            EventHandler.Trigger(new UIChangeEvent(){text = "", holderId = callback.holderId});
            //发送重置物品事件
            EventHandler.Trigger(new ResetItemEvent(){elementId = element.id, holderId = callback.holderId});
        }
    }

    public void DropItemCallback(DropItemCallback callback)
    {
        if (holderId == callback.holderId)
        {
            holderId = -1;
            // 保存原始位置
            element.transform.position = callback.position;
            element.transform.SetParent(originalParent);
            element.transform.localPosition += offset;
            element.transform.localRotation = originalRotation;
            //显示物体
            element.gameObject.SetActive(true);
            //重置后取消监听
            EventHandler.Unregister<ResetItemCallback>(ResetItemCallback);
            EventHandler.Unregister<DropItemCallback>(DropItemCallback);
            //发送UI修改事件
            EventHandler.Trigger(new UIChangeEvent(){text = "", holderId = callback.holderId});
            //发送放下物品事件
            EventHandler.Trigger(new DropItemEvent(){elementId = element.id, position = callback.position, holderId = callback.holderId});
        }
    }
    #endregion
}
