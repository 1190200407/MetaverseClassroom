using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Action
{
    public class MoveTo : BaseActionMethod
    {
        public override IEnumerator ExecuteCoroutine()
        {
            Debug.Log($"MoveTo {actionNode.role} to {actionNode.actionParams["position"]}");
            yield return new WaitForSeconds(1f);
        }
    }
}
