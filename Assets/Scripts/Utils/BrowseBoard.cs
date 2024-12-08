using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrowseBoard : MonoBehaviour
{
    private Canvas canvas;
    private Text information;

    private void Start() {
        canvas = GetComponentInChildren<Canvas>();
        information = GetComponentInChildren<Text>();
    }

    public void OpenCanvas() {
        canvas.enabled = true;
    }

    public void CloseCanvas() {
        canvas.enabled = false;
    }

    public void SetInformation(string content) {
        information.text = content;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Camera.main.transform);
    }
}
