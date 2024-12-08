using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class WinPlayerController : PlayerController
{
    public float minX = -360.0f;
    public float maxX = 360.0f;

    public float minY = -60.0f;
    public float maxY = 60.0f;

    public float sens = 200.0f;

    public float speed = 5f;

    float rotationY = 0.0f;
    float rotationX = 0.0f;

    float moveHoldTimer = 0f;
    float moveHoldThreshold = 0.5f;

    bool pause;

    protected override void Start()
    {
        base.Start();    

        if (!photonView.IsMine) return;
        
        playerCamera = Camera.main.gameObject;
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = Vector3.up * 1.8f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        pause = false;
    }

    void OnEnable() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnDisable() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void OnPause() {
        pause = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void OnResume() {
        pause = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //if (!isLocalPlayer) { return; }
        if (!photonView.IsMine && PhotonNetwork.IsConnected) { return; }
        if(!pause) {
            var x = Input.GetAxis("Mouse X");
            var y = Input.GetAxis("Mouse Y");
            if (x != 0 || y != 0)
            {
                rotationX += x * sens * Time.deltaTime;
                rotationY += y * sens * Time.deltaTime;
                rotationY = Mathf.Clamp(rotationY, minY, maxY);
                transform.localEulerAngles = new Vector3(0, rotationX, 0);
                playerCamera.transform.localEulerAngles = new Vector3(-rotationY, 0, 0);
            }
        }
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected) { return; }
        if (!pause && !isSitting) {
            var x = Input.GetAxis("Horizontal");
            var y = Input.GetAxis("Vertical");
            Vector3 move = (Vector3.right * x + Vector3.forward * y) * speed / 50;
            rb.MovePosition(transform.position + transform.TransformDirection(move));
            anim.SetFloat("MoveX", x);
            anim.SetFloat("MoveY", y);
        }
        if (!pause && isSitting)
        {
            var x = Input.GetAxis("Horizontal");
            var y = Input.GetAxis("Vertical");
            if (x != 0 || y != 0)
            {
                moveHoldTimer += Time.fixedDeltaTime;
                if (moveHoldTimer > moveHoldThreshold)
                {
                    moveHoldTimer = 0f;
                    Sit.currentSitting.ResetSeat();
                    IsSitting = false;
                }
            }
            else
                moveHoldTimer = 0f;
        }
    }

    public void SetRotation(float rotationX, float rotationY)
    {
        this.rotationX = rotationX;
        this.rotationY = rotationY;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);
        transform.localEulerAngles = new Vector3(0, rotationX, 0);
        if (playerCamera != null)
            playerCamera.transform.localEulerAngles = new Vector3(-rotationY, 0, 0);
    }
}
