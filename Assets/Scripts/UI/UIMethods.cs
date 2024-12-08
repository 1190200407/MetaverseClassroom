using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMethods
{
    public static UIMethods instance;

    public UIMethods(){
        instance = this;
    }

    public GameObject FindCanvas()
    {
        Canvas[] canvas = GameObject.FindObjectsOfType<Canvas>();
        if(canvas.Length==1) return canvas[0].gameObject;
        foreach(Canvas c in canvas){
            if(c.tag == "MainCanvas")
                return c.gameObject;
        }
        return null;
    }

    public GameObject FindObjectInChild(GameObject panel,string child_name){
        Transform[] transforms = panel.GetComponentsInChildren<Transform>();

        foreach(var tra in transforms){
            if(tra.gameObject.name == child_name){
                return tra.gameObject;
            }
        }
        return null;
    }

    public T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
        if(gameObject.GetComponent<T>() == null){
            gameObject.AddComponent<T>();
        }
        return gameObject.GetComponent<T>();
    }

    public T GetOrAddComponentInChild<T>(GameObject gameObject,string child_name) where T : Component
    {
        Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();

        foreach(var tra in transforms){
            if(tra.gameObject.name == child_name){
                if(tra.GetComponent<T>() != null){
                    return tra.GetComponent<T>();
                }else{
                    tra.gameObject.AddComponent<T>();
                    return tra.gameObject.GetComponent<T>();
                }
            }
        }
        return null;
    }
}
