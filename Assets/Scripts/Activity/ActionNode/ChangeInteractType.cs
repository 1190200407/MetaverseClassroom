using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actions
{
    // 系统任务, 用于改变元素的交互类型
    public class ChangeInteractType : BaseActionMethod
    {
        public string targetElementId;
        public string newInteractType;
        public string newInteractionContent;

        public override void Initialize()
        {
            base.Initialize();

            // 获取属性
            targetElementId = ClassManager.instance.currentScene +  actionNode.actionParams["targetElementId"];
            newInteractType = actionNode.actionParams["newInteractType"];
            newInteractionContent = actionNode.actionParams["newInteractionContent"];
        }

        public override IEnumerator ExecuteCoroutine()
        {
            SceneElement element = SceneLoader.instance.IdToElement[targetElementId];
            element.SetInteactionType(newInteractType, newInteractionContent);

            yield return null;
        }
    }
}
