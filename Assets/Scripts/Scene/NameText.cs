using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class NameText : MonoBehaviour
{
    private TMP_Text txt;
    private Vector3 tempVec;

    void Awake()
    {
        txt = GetComponent<TMP_Text>();
    }

    void Update()
    {
        tempVec = Camera.main.transform.forward;
        tempVec.y = 0;
        transform.forward = tempVec;
    }

    public void SetName(string name)
    {
        txt.text = name;
    }
}
