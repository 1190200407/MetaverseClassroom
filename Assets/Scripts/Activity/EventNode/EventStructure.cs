using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventStructure : MonoBehaviour
{
    public struct EventNode
    {
        public string eventName;

        //事件块区域
        //public EventZone eventZone;

        //事件块
        public ActionTree actionTree;
    }

}
