using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actions
{
    public class Sit : BaseActionMethod
    {
        public override void Initialize()
        {
            base.Initialize();

            // 如果是玩家，则提供任务面板
            if (playerId != -1 && playerId == PlayerManager.localPlayer.netId)
            {
                EventHandler.Trigger(new NewTaskEvent() { netId = (uint)playerId, taskDescription = "坐下", actionNodeId = actionNode.id });

                // 监听事件
                EventHandler.Register<InteractionEvent>(OnInteractionEvent);
            }
        }

        private void OnInteractionEvent(InteractionEvent interactionEvent)
        {
            if (interactionEvent.interactionType == "sit")
            {
                // 触发任务完成事件
                EventHandler.Trigger(new TaskCompleteEvent() { netId = PlayerManager.localPlayer.netId, actionNodeId = actionNode.id });
                // 取消监听事件
                EventHandler.Unregister<InteractionEvent>(OnInteractionEvent);
            }
        }

        public override IEnumerator ExecuteCoroutine()
        {
            if (playerId == -1)
            {
                Debug.Log($"控制NPC {role} 坐下");
                yield return new WaitForSeconds(1f);
            }
            else
            {
                Debug.Log($"控制玩家 {playerId} 坐下");
                yield return new WaitUntil(() => actionNode.accomplished);
            }
        }
    }
}
