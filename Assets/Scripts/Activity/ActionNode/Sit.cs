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
