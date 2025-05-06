using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Action
{
    public class Speak : BaseActionMethod
    {
        public override IEnumerator ExecuteCoroutine()
        {
            Debug.Log($"Speak {actionNode.role} {actionNode.actionParams["text"]}");
            yield return new WaitForSeconds(1f);
        }
    }
}
