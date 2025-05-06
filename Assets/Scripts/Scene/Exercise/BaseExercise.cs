using System;
using System.Collections;
using System.Collections.Generic;
using Mirror.Examples.TopDownShooter;
using Scene.Action;
using UnityEngine;
using UnityEngine.Windows.WebCam;

public class BaseExercise : MonoBehaviour
{
    private ActionStructure root; //行为树根节点
    public List<String> roleList = new List<string>(); //角色列表

    private void Start()
    {
        ((RoleSetter)gameObject.GetComponent<SceneElement>().interactionScript).roleList = roleList;
    }

    public String GetGoal(String Role)
    {
        //TODO：根据角色名，在行为树中识别
        return "待完成";
    }
}
