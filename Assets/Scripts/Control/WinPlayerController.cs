using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class WinPlayerController : PlayerController
{
    public float minX = -360.0f;
    public float maxX = 360.0f;

    public float minY = -60.0f;
    public float maxY = 60.0f;

    public float cameraSensitivity;

    public float moveSpeed;

    float rotationY = 0.0f;
    float rotationX = 0.0f;

    float moveHoldTimer = 0f;
    float moveHoldThreshold = 0.5f;

    bool pause;    
    
    public float fovMin = 30f;    // 最小视距
    public float fovMax = 90f;    // 最大视距
    public float fovSpeed = 10f;  // 滚动速度

    public GameObject monitor;

    private float targetRotationY = 0f;
    private float currentRotationY = 0f;
    public float rotationSmoothSpeed = 10f; // 调整这个值来控制平滑程度
    private int currentFrameCount = 0;
    public int syncFrameCount = 10; // 同步间隔的帧数

    protected override void Start()
    {
        base.Start();    

        if (!isLocalPlayer) return;
        
        playerCamera = Camera.main;
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = Vector3.up * 1.8f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        pause = false;

        monitor = GameObject.Find("Monitor");
    }

    protected override void initialize()
    {
        base.initialize();
        cameraSensitivity = playerData.cameraSensitivity;
        moveSpeed = playerData.moveSpeed;
    }

    protected override void changeData(PlayerData data)
    {
        base.changeData(data);
        cameraSensitivity = data.cameraSensitivity;
        moveSpeed = data.moveSpeed;
    }
    
    public void OnEnable() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnDisable() {
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

    private void Update()
    {
        if (!isLocalPlayer) { return; }
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
                
                // 更新网络同步的旋转值
                currentFrameCount++;
                if (currentFrameCount >= syncFrameCount)
                {
                    currentFrameCount = 0;
                    CmdSyncRotation(rotationY);
                }
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
        }

        // 更新红点的位置
        if (redDot != null)
        {
            // 没有权限时，隐藏红点
            if (!HavePermission(Permission.RedDot) && isShowingRedDot)
            {
                redDot.SetActive(false);
                isShowingRedDot = false;
            }
            
            if (HavePermission(Permission.RedDot))
            {
                // 检测右键按下和松开
                if (Input.GetMouseButtonDown(1)) // 右键按下
                {
                    isShowingRedDot = true;
                }

                if (Input.GetMouseButtonUp(1)) // 右键松开
                {
                    isShowingRedDot = false;
                    redDot.transform.position = Vector3.down * 10000f;
                }

                if (isShowingRedDot)
                {
                    UpdateRedDotPosition();
                }
            }
        }
    }

    [Command]
    public void CmdSyncRotation(float rotationY)
    {
        targetRotationY = rotationY;
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer)
        {
            // 平滑插值到目标旋转值
            currentRotationY = Mathf.Lerp(currentRotationY, targetRotationY, Time.deltaTime * rotationSmoothSpeed);
            
            if (monitor != null)
            {
                monitor.transform.localEulerAngles = new Vector3(-currentRotationY, 0, 0);
            }
        }
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) { return; }
        if (!pause && !isSitting) {
            var x = Input.GetAxis("Horizontal");
            var y = Input.GetAxis("Vertical");
            Vector3 move = (Vector3.right * x + Vector3.forward * y) * moveSpeed / 500;
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
    
    private void UpdateRedDotPosition()
    {
        if (playerCamera != null && redDot != null)
        {
            // 通过相机的正前方来设置红点位置
            Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * 5f; // 5f 是红点距离相机的距离，可以根据需要调整
            redDot.transform.position = targetPosition;
        }

        if (playerCamera != null && redDot != null)
        {
            RaycastHit hit;
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward); // 从摄像机出发，沿摄像机的正前方发射射线

            if (Physics.Raycast(ray, out hit))
            {
                // 红点停留在碰撞点的位置
                redDot.transform.position = hit.point;

                // 旋转红点，使其与碰撞表面法线对齐
                redDot.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            else
            {
                // 如果射线没有碰到物体，红点会停在射线的最大距离处
                redDot.transform.position = ray.GetPoint(100f); // 100f 是射线的最大距离
                redDot.transform.rotation = Quaternion.identity; // 保持默认的旋转
            }
        }
    }
}
