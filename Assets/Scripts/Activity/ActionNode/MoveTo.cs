using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actions
{
    public class MoveTo : BaseActionMethod
    {
        private Vector3 pos;
        private Vector3 worldPos;

        //TODO 之后要改成路径式的移动方式
        public override void Initialize()
        {
            base.Initialize();
            
            // 场景坐标转为世界坐标
            pos = (Vector3)actionNode.actionParams["position"];
            worldPos = pos + SceneLoader.instance.PathToSceneObject[ClassManager.instance.currentScene].transform.position;

            // 如果是玩家，则提供任务面板
            EventHandler.Trigger(new NewTaskEvent() { netId = (uint)playerId, taskDescription = "移动到" + pos, actionNodeId = actionNode.id });
        }

        public override IEnumerator ExecuteCoroutine()
        {
            if (playerId == -1)
            {
                Debug.Log($"控制NPC {role} 移动到 {pos}");
            }
            else if (isLocalAction)
            {

                Debug.Log($"控制玩家 {role} 移动到 {pos}");
                do
                {
                    if (player == null || Vector3.SqrMagnitude(player.transform.position - worldPos) <= 0.1f)
                    {
                        // 如果玩家已经到达目标位置，则任务完成
                        EventHandler.Trigger(new TaskCompleteEvent() { netId = (uint)playerId, actionNodeId = actionNode.id });
                    }
                    yield return null;
                } 
                while (!actionNode.accomplished);
                Debug.Log($"玩家 {role} 移动到 {pos} 完成");
            }
            else
            {
                //TODO 之后要考虑玩家突然掉线的问题
                Debug.Log($"等待玩家 {role} 移动到 {pos}");
                do
                {
                    yield return null;
                } 
                while (player != null && !actionNode.accomplished);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}
