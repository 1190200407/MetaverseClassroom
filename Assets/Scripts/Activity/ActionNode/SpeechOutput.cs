using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actions
{
    public class SpeechOutput : BaseActionMethod
    {
        private string text;
        
        public override void Initialize()
        {
            base.Initialize();
            text = actionNode.actionParams["text"].ToString();
            
            // 如果是玩家，则提供任务面板
            if (playerId != -1 && playerId == PlayerManager.localPlayer.netId)
            {
                EventHandler.Trigger(new NewTaskEvent() { netId = (uint)playerId, taskDescription = "说" + text, actionNodeId = actionNode.id });
            }
        }

        public override IEnumerator ExecuteCoroutine()
        {
            if (playerId == -1)
            {
                Debug.Log($"控制NPC {role} Speak {text}");
                yield return new WaitForSeconds(1f);
            }
            else
            {
                Debug.Log($"控制玩家 {playerId} Speak {text}");
                yield return new WaitUntil(() => actionNode.accomplished);
            }
        }
    }
}
