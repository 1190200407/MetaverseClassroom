using UnityEngine;

public class NPCController : MonoBehaviour
{
    public NPCManager npcManager;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float moveSpeed = 5f;
    private float rotationSpeed = 10f;
    private float stoppingDistance = 2f;

    void Update()
    {
        // 更新NPC的位置
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        if (npcManager.IsMoving && Vector3.Distance(transform.position, targetPosition) > stoppingDistance)
        {
            // 移动NPC
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
        else if (npcManager.IsMoving)
        {
            npcManager.IsMoving = false;
        }
    }

    public void SetTargetPosition(Vector3 position, float stoppingDistance = 2f)
    {
        // 如果NPC正在坐下，则不移动
        if (npcManager.IsSitting) return;

        targetPosition = position;
        this.stoppingDistance = stoppingDistance;

        // 控制NPC朝向目标位置，不改变Y轴
        transform.LookAt(new Vector3(targetPosition.x, transform.position.y, targetPosition.z));

        npcManager.IsMoving = true;
    }
}
