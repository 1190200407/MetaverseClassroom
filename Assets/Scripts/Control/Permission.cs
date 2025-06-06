using UnityEngine;

[System.Flags]
public enum Permission
{
    None = 0,
    SwitchPPT = 1,  // 切换PPT
    Chat = 2,       // 聊天
    RedDot = 4,     // 红点
    ControlChat = 8, // 控制聊天
    ChangeScene = 16, // 控制场景切换
    Activity = 32, // 开始活动
    SelctRoles = 64, // 选择角色
    ChangePPT = 128, // 切换PPT
}

public class PermissionHolder
{
    private int permission;

    public int GetPermission()
    {
        return permission;
    }

    public bool HasPermission(Permission permission)
    {
        return (this.permission & (int)permission) != 0;
    }

    public void AddPermission(Permission permission)
    {
        this.permission |= (int)permission;
    }

    public void RemovePermission(Permission permission)
    {
        this.permission &= ~(int)permission;
    }

    public void SetPermission(Permission permission)
    {
        this.permission = (int)permission;
    }

    public void SetAllPermission()
    {
        this.permission = ~0;
    }
}
