using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoleText : MonoBehaviour
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
        tempVec.y = 0.3f;
        transform.forward = tempVec;
    }

    public void SetRole(string role)
    {
        txt.text = role;
        Debug.Log("角色名为" + role);
    }
}
