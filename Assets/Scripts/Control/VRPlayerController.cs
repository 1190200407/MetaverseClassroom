using UnityEngine;
using Photon.Pun;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using System.Collections.Generic;

public class VRPlayerController : PlayerController
{
    private XRInputSubsystem xrInputSubsystem;
    public Transform headTransform;
    public float moveSpeed; // 移动速度
    public float rotationSpeed; // 旋转速度
    
    float moveHoldTimer = 0f;
    float moveHoldThreshold = 0.5f;

    protected override void Start()
    {
        base.Start();

        if (!photonView.IsMine) return;
        GameObject XROrigin = GameObject.Find("XR Origin");
        XROrigin.transform.SetParent(transform);
        XROrigin.transform.localPosition = Vector3.zero;

        // 设定VR相机的位置
        headTransform = Camera.main.transform;
        transform.GetChild(0).gameObject.SetActive(false);

        // 获取当前激活的 XRInputSubsystem 实例
        if (XRGeneralSettings.Instance.Manager.activeLoader != null)
        {
            xrInputSubsystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRInputSubsystem>();
        }
    }

    protected override void initialize()
    {
        base.initialize();
        rotationSpeed = playerData.cameraSensitivity;
        moveSpeed = playerData.moveSpeed;
    }

    protected override void changeData(PlayerData data)
    {
        base.changeData(data);
        rotationSpeed = data.cameraSensitivity;
        moveSpeed = data.moveSpeed;
    }
    void Update()
    {
        if (!photonView.IsMine) return;

        // 控制移动和旋转
        HandleMovement();
    }

    private void HandleMovement()
    {
        // 获取左手柄摇杆输入
        Vector2 leftStickInput = GetStickInput(InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller);
        // 左摇杆控制移动
        if (leftStickInput != Vector2.zero)
        {
            if (!isSitting)
            {
                Vector3 moveDirection = new Vector3(leftStickInput.x, 0, leftStickInput.y).normalized;
                transform.Translate(moveDirection * moveSpeed / 10 * Time.deltaTime, Space.Self);
            }
            else
            {
                moveHoldTimer += Time.fixedDeltaTime;
                if (moveHoldTimer > moveHoldThreshold)
                {
                    moveHoldTimer = 0f;
                    Sit.currentSitting.ResetSeat();
                    IsSitting = false;
                }
            }
        }
        else
            moveHoldTimer = 0f;

        // 获取右手柄摇杆输入
        Vector2 rightStickInput = GetStickInput(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller);
        // 右摇杆控制旋转
        if (rightStickInput.x != 0)
        {
            float rotateY = rightStickInput.x * rotationSpeed * 2 * Time.deltaTime;
            transform.Rotate(0, rotateY, 0);
        }
    }

    private Vector2 GetStickInput(InputDeviceCharacteristics characteristics)
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(characteristics, devices);

        foreach (var device in devices)
        {
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 stickInput))
            {
                return stickInput;
            }
        }

        return Vector2.zero;
    }
}