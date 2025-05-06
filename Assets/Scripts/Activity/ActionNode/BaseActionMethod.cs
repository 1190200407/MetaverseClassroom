using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Action
{
    public class BaseActionMethod
    {
        public string actionName;
        public string actionDescription;
        public ActionTreeLeafNode actionNode;

        public virtual void ParseParams(Dictionary<string, object> @params)
        {
        }

        public virtual IEnumerator ExecuteCoroutine()
        {
            Debug.Log($"ExecuteCoroutine {actionName}");
            yield return new WaitForSeconds(1f);
        }
    }
}
