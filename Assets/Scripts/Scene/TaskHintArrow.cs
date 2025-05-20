using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

// 任务提示箭头
public class TaskHintArrow : MonoBehaviour
{
    private Vector3 tempVec;

    void Start()
    {
        // 不停上下摆动
        transform.DOMoveY(transform.position.y + 0.1f, 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    // Update is called once per frame
    void Update()
    {
        // 面向摄像头， 保持Y轴不变
        tempVec = -Camera.main.transform.forward;
        tempVec.y = 0;
        transform.forward = tempVec;
    }

    void OnDestroy()
    {
        // 停止所有在这个对象上的DOTween动画
        DOTween.Kill(transform);
    }
}
