using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actions
{
    public class BaseActionMethod
    {
        public string actionName;
        public string actionDescription;
        public ActionTreeLeafNode actionNode;

        public int playerId;
        public GameObject player;
        public bool isLocalAction;
        public string role;

        /// <summary>
        /// 初始化，一般包括解析参数，修改任务UI或者触发事件等等
        /// </summary>
        public virtual void Initialize()
        {
            string roleId = actionNode.role;

            // 系统角色, 用于执行系统任务
            if (roleId == "system")
            {
                role = "系统";
                playerId = -2;
                isLocalAction = true;
                return;
            }

            role = ClassManager.instance.roleList[roleId];
            playerId = ClassManager.instance.roleOccupied[roleId];
            isLocalAction = playerId == -1 || playerId == PlayerManager.localPlayer.netId;
            
            // 如果playerId为-1，则通过名称获取GameObject
            if (playerId == -1)
            {
                player = GameObject.Find("NPC" + roleId);
            }
            // 如果playerId大于0，则通过netId获取GameObject
            else if (playerId > 0)
            {
                player = Mirror.Utils.GetSpawnedInServerOrClient((uint)playerId).gameObject;
            }
        }

        public virtual void OnComplete()
        {
        }

        public virtual IEnumerator ExecuteCoroutine()
        {
            Debug.Log($"ExecuteCoroutine {actionName}");
            // 默认动作：等待改节点执行完成，如果其他玩家没有执行完成，则等待1秒
            float startTime = Time.time;
            while (!actionNode.accomplished && Time.time - startTime < 1f)
            {
                yield return null;
            }
        }
    }
}
