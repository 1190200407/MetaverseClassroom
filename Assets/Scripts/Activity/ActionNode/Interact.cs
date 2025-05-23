using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actions
{
    public class Interact : BaseActionMethod
    {
        public string targetElementId;
        public string interactType;
        public string interactWay;
        public string taskDescription;

        public override void Initialize()
        {
            base.Initialize();

            // 获取属性
            targetElementId = actionNode.actionParams["targetElementId"];
            // 如果targetElementId为空串，说明不明确要求交互的元素
            // 如果targetElementId不为空串，说明是场景中的元素
            if (!string.IsNullOrEmpty(targetElementId))
            {
                targetElementId = ClassManager.instance.currentScene + targetElementId;
            }
            interactType = actionNode.actionParams["interactType"];
            interactWay = actionNode.actionParams["interactWay"];
            taskDescription = actionNode.actionParams["taskDescription"];

            // 如果是玩家，则提供任务面板
            if (playerId != -1 && playerId == PlayerManager.localPlayer.netId)
            {
                EventHandler.Trigger(new NewTaskEvent() { netId = (uint)playerId, taskDescription = this.taskDescription, actionNodeId = actionNode.id });

                // 监听事件
                EventHandler.Register<InteractionEvent>(OnInteractionEvent);
            }
        }

        private void OnInteractionEvent(InteractionEvent interactionEvent)
        {
            // 判断交互是否满足条件(忽略大小写)
            if (interactionEvent.interactType.ToLower() == this.interactType.ToLower() && interactionEvent.interactWay.ToLower() == this.interactWay.ToLower() 
             && (string.IsNullOrEmpty(this.targetElementId) || interactionEvent.elementId.ToLower() == this.targetElementId.ToLower()))
            {
                // 触发任务完成事件
                EventHandler.Trigger(new TaskCompleteEvent() { netId = PlayerManager.localPlayer.netId, actionNodeId = actionNode.id });
            }
        }

        public override IEnumerator ExecuteCoroutine()
        {
            if (playerId == -1)
            {
                Debug.Log($"控制NPC {role} {taskDescription}");
                yield return new WaitForSeconds(1f);
            }
            else
            {
                Debug.Log($"控制玩家 {playerId} {taskDescription}");
                yield return new WaitUntil(() => actionNode.accomplished);

                if (playerId != -1 && playerId == PlayerManager.localPlayer.netId)
                {
                    // 取消监听事件
                    EventHandler.Unregister<InteractionEvent>(OnInteractionEvent);
                }
            }
        }
    }
}
