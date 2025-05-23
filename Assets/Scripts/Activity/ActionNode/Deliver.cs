using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actions
{
    public class Deliver : BaseActionMethod
    {
        private string targetElementId;
        private SceneElement targetElement;
        private GameObject taskHintArrow;

        public override void Initialize()
        {
            base.Initialize();

            // 如果是玩家，则提供任务面板
            if (playerId != -1 && playerId == PlayerManager.localPlayer.netId)
            {
                string resources = actionNode.actionParams["resources"];
                string targetRole = actionNode.actionParams["targetRole"];
                string taskDescription = "将" + resources + "交给" + ClassManager.instance.roleList[targetRole];
                EventHandler.Trigger(new NewTaskEvent() { netId = (uint)playerId, taskDescription = taskDescription, actionNodeId = actionNode.id });

                targetElementId = ClassManager.instance.currentScene + actionNode.actionParams["targetElementId"];
                if (SceneLoader.instance.IdToElement.TryGetValue(targetElementId, out targetElement))
                {
                    // 在目标元素上添加一个任务提示箭头
                    taskHintArrow = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/TaskHintArrow"));
                    taskHintArrow.transform.SetParent(targetElement.transform);
                    taskHintArrow.transform.localPosition = Vector3.up * 0.5f;
                }

                // 注册物品放置回调
                EventHandler.Register<DropItemEvent>(OnDropItemEvent);
            }
        }

        public void OnDropItemEvent(DropItemEvent @event)
        {
            if (@event.elementId == targetElementId)
            {
                EventHandler.Trigger(new TaskCompleteEvent() { netId = (uint)playerId, actionNodeId = actionNode.id });
            }
        }

        public override IEnumerator ExecuteCoroutine()
        {
            if (playerId == -1)
            {
                Debug.Log($"控制NPC {role} 交付");
                yield return new WaitForSeconds(1f);
            }
            else
            {
                Debug.Log($"控制玩家 {playerId} 交付");
                yield return new WaitUntil(() => actionNode.accomplished);

                if (playerId != -1 && playerId == PlayerManager.localPlayer.netId)
                {
                    // 销毁任务提示箭头
                    GameObject.Destroy(taskHintArrow);

                    // 取消注册物品放置回调
                    EventHandler.Unregister<DropItemEvent>(OnDropItemEvent);
                }
            }
        }
    }
}
