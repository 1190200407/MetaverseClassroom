using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mirror;
using UnityEngine;

public class WinPlayerController : PlayerController
{
    public float minX = -360.0f;
    public float maxX = 360.0f;

    public float minY = -60.0f;
    public float maxY = 60.0f;

    float rotationY = 0.0f;
    float rotationX = 0.0f;

    float moveHoldTimer = 0f;
    float moveHoldThreshold = 0.5f;

    bool pause;    
    
    public float fovMin = 30f;    // 最小视距
    public float fovMax = 90f;    // 最大视距
    public float fovSpeed = 10f;  // 滚动速度

    protected override void Start()
    {
        base.Start();    

        if (!playerManager.isLocalPlayer) return;
        
        playerCamera = Camera.main;
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = Vector3.up * 1.8f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        pause = false;
    }

    protected override void initialize()
    {
        base.initialize();
        InitializeData();
        cameraSensitivity = playerData.cameraSensitivity;
        moveSpeed = playerData.moveSpeed;
    }

    public override void ResetTransform(Transform sceneTransform)
    {
        base.ResetTransform(sceneTransform);
        rotationX = 0;
        rotationY = 0;
    }
    
    public void OnEnable() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        EventHandler.Register<PlayerChangeDataEvent>(changePlayerData); //注册用户修改基本参数事件
    }

    public void OnDisable() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnDestroy()
    {
        EventHandler.Unregister<PlayerChangeDataEvent>(changePlayerData); //注销用户修改基本参数事件
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

    private void Update()
    {
        if (!playerManager.isLocalPlayer) { return; }
        if(!pause) {
            var x = Input.GetAxis("Mouse X");
            var y = Input.GetAxis("Mouse Y");
            if (x != 0 || y != 0)
            {
                rotationX += x * cameraSensitivity * 5 * Time.deltaTime;
                rotationY += y * cameraSensitivity * 5 * Time.deltaTime;
                rotationY = Mathf.Clamp(rotationY, minY, maxY);
                transform.localEulerAngles = new Vector3(0, rotationX, 0);
                playerCamera.transform.localEulerAngles = new Vector3(-rotationY, 0, 0);
            }

            // 使用鼠标滚轮调整视距 (FOV)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                float fov = playerCamera.fieldOfView;
                fov -= scroll * fovSpeed; // 改变 FOV
                fov = Mathf.Clamp(fov, fovMin, fovMax); // 限制 FOV 在 min 和 max 之间
                playerCamera.fieldOfView = fov; // 应用新的 FOV
            }
            
            // 按下F键，重置当前玩家持有的物体

            if (Input.GetKeyDown(KeyCode.F))
            {
                EventHandler.Trigger(new GrabResetEvent());
            }
            
            // 按下R键，重置当前玩家持有的物体
            if (Input.GetKeyDown(KeyCode.R))
            {
                EventHandler.Trigger(new ResetItemEvent(){holderId = PlayerManager.localPlayer.netId});
            }

            // 按下空格键，放置玩家持有的物体
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3? pos = GetRayHitPos();
                if (pos.HasValue)
                {
                    EventHandler.Trigger(new DropItemEvent(){holderId = PlayerManager.localPlayer.netId,position = pos.Value});
                }
            }
        }

        // 更新红点的位置
        if (playerManager.redDot != null)
        {
            // 没有权限时，隐藏红点
            if (!playerManager.HavePermission(Permission.RedDot) && playerManager.isShowingRedDot)
            {
                playerManager.redDot.SetActive(false);
                playerManager.isShowingRedDot = false;
            }
            
            if (playerManager.HavePermission(Permission.RedDot))
            {
                // 检测右键按下和松开
                if (Input.GetMouseButtonDown(1)) // 右键按下
                {
                    playerManager.isShowingRedDot = true;
                }

                if (Input.GetMouseButtonUp(1)) // 右键松开
                {
                    playerManager.isShowingRedDot = false;
                    playerManager.redDot.transform.position = Vector3.down * 10000f;
                }

                if (playerManager.isShowingRedDot)
                {
                    UpdateRedDotPosition();
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!playerManager.isLocalPlayer) { return; }
        if (!pause && !playerManager.IsSitting) {
            var x = Input.GetAxis("Horizontal");
            var y = Input.GetAxis("Vertical");
            Vector3 move = (Vector3.right * x + Vector3.forward * y) * moveSpeed / 500;
            rb.MovePosition(transform.position + transform.TransformDirection(move));
            animator.SetFloat("SpeedX", x);
            animator.SetFloat("SpeedY", y);
        }
        if (!pause && playerManager.IsSitting)
        {
            var x = Input.GetAxis("Horizontal");
            var y = Input.GetAxis("Vertical");
            if (x != 0 || y != 0)
            {
                moveHoldTimer += Time.fixedDeltaTime;
                if (moveHoldTimer > moveHoldThreshold)
                {
                    moveHoldTimer = 0f;

                    // 位移一小段距离，防止玩家卡在椅子上
                    playerManager.transform.position += Vector3.right * x + Vector3.forward * y;
                    Sit.currentSitting.ResetSeat();
                    playerManager.IsSitting = false;
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

    private Vector3? GetRayHitPos()
    {
        if (playerCamera != null)
        {
            RaycastHit hit;
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward); // 从摄像机出发，沿摄像机的正前方发射射线

            if (Physics.Raycast(ray, out hit))
            {
                return hit.point;
            }
        }
        return null;
    }
    
    private void UpdateRedDotPosition()
    {
        if (playerCamera != null && playerManager.redDot != null)
        {
            // 通过相机的正前方来设置红点位置
            Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * 5f; // 5f 是红点距离相机的距离，可以根据需要调整
            playerManager.redDot.transform.position = targetPosition;
        }

        if (playerCamera != null && playerManager.redDot != null)
        {
            RaycastHit hit;
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward); // 从摄像机出发，沿摄像机的正前方发射射线

            if (Physics.Raycast(ray, out hit))
            {
                // 红点停留在碰撞点的位置
                playerManager.redDot.transform.position = hit.point;

                // 旋转红点，使其与碰撞表面法线对齐
                playerManager.redDot.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            else
            {
                // 如果射线没有碰到物体，红点会停在射线的最大距离处
                playerManager.redDot.transform.position = ray.GetPoint(100f); // 100f 是射线的最大距离
                playerManager.redDot.transform.rotation = Quaternion.identity; // 保持默认的旋转
            }
        }
    }
}
