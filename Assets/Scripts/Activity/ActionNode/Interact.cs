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
            interactWay = actionNode.actionParams["interactWay"]; // 目前没用
            taskDescription = actionNode.actionParams["taskDescription"];

            // 如果是玩家，则提供任务面板
            if (playerId != -1 && playerId == PlayerManager.localPlayer.netId)
            {
                EventHandler.Trigger(new NewTaskEvent() { netId = (uint)playerId, taskDescription = this.taskDescription, actionNodeId = actionNode.id });

                // 监听事件
                EventHandler.Register<InteractionEvent>(OnInteractionEvent);
            }
            // 如果是NPC，则监听NPC交互事件
            else if (playerId == -1)
            {
                EventHandler.Register<NPCInteractionEvent>(OnNPCInteractionEvent);
            }
        }

        private bool IsInteractionMatch(string interactType, string interactWay, string elementId)
        {
            return interactType.ToLower() == this.interactType.ToLower()  
                && (string.IsNullOrEmpty(this.targetElementId) || elementId.ToLower() == this.targetElementId.ToLower());
        }

        private void OnInteractionEvent(InteractionEvent interactionEvent)
        {
            // 判断交互是否满足条件(忽略大小写)
            if (IsInteractionMatch(interactionEvent.interactType, interactionEvent.interactWay, interactionEvent.elementId))
            {
                // 触发任务完成事件
                EventHandler.Trigger(new TaskCompleteEvent() { netId = PlayerManager.localPlayer.netId, actionNodeId = actionNode.id });
            }
        }

        private void OnNPCInteractionEvent(NPCInteractionEvent npcInteractionEvent)
        {
            // 因为每个NPC都是在本地里执行，所以不需要触发任务完成事件，直接设置任务完成状态为true
            if (IsInteractionMatch(npcInteractionEvent.interactionType, npcInteractionEvent.interactWay, npcInteractionEvent.elementId))
            {
                actionNode.accomplished = true;
            }
        }

        private SceneElement SearchSceneElement()
        {
            // 如果targetElementId为空串，说明不明确要求交互的元素,找一个最近的带相关交互的元素
            if (string.IsNullOrEmpty(targetElementId))
            {
                // 遍历场景中的所有元素，找到最近的带相关交互的元素
                float minDistance = float.MaxValue;
                SceneElement nearestElement = null;
                foreach (var element in SceneLoader.instance.IdToElement)
                {
                    if (element.Key.StartsWith(ClassManager.instance.currentScene) && element.Value.interactType.ToLower() == interactType.ToLower())
                    {
                        float distance = Vector3.Distance(PlayerManager.localPlayer.transform.position, element.Value.transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestElement = element.Value;
                        }
                    }   
                }
                return nearestElement;
            }
            SceneElement sceneElement = SceneLoader.instance.IdToElement[targetElementId];
            if (sceneElement == null)
            {
                Debug.LogError($"SceneElement {targetElementId} not found");
                return null;
            }
            return sceneElement;
        }

        public override IEnumerator ExecuteCoroutine()
        {
            if (playerId == -1)
            {
                NPCManager npcManager = player.GetComponent<NPCManager>();
                SceneElement sceneElement = SearchSceneElement();

                // 控制NPC走向目标元素
                npcManager.npcController.SetTargetPosition(sceneElement.transform.position);
                yield return new WaitUntil(() => !npcManager.IsMoving);

                // 控制NPC交互
                sceneElement.OnNPCInteract(npcManager, interactWay);
                yield return new WaitUntil(() => actionNode.accomplished);
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
