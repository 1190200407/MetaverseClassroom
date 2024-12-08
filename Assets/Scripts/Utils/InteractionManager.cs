using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : UnitySingleton<InteractionManager>
{
    public bool RaycastClosed = false;
    private SceneElement target = null;

    void Update()
    {
        SceneElement element = null;
        GameObject hitObj = getHitObj();

        // 检测新的射线命中对象并触发事件
        if (hitObj != null)
        {
            element = hitObj.GetComponent<SceneElement>();
            if (element != target)  // 新的目标元素
            {
                // 如果先前有目标对象，触发 `OnHoverExit` 事件
                target?.OnHoverExit();
                target = element;

                // 对新的目标对象，触发 `OnHoverEnter` 事件
                target.OnHoverEnter();
            }
        }
        else
        {
            // 没有命中对象时，触发 `OnHoverExit` 并重置目标
            if (target != null)
            {
                target.OnHoverExit();
                target = null;
            }
        }

        // 处理点击事件
        if (Input.GetMouseButtonDown(0) && target != null)
        {
            // 左键点击，触发 `OnSelectEnter` 事件
            target.OnSelectEnter();
        }
        if (Input.GetMouseButtonUp(0) && target != null)
        {
            // 左键点击，触发 `OnSelectEnter` 事件
            target.OnSelectExit();
        }
    }

    // 获取当前射线命中的对象
    private GameObject getHitObj()
    {
        if (RaycastClosed) return null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        
        float minDistance = float.MaxValue;
        GameObject closestHitObject = null;

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.TryGetComponent<SceneElement>(out SceneElement element) && element.interactionScript != null)
            {
                float distance = hit.distance;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestHitObject = hit.collider.gameObject;
                }
            }
        }

        return closestHitObject;
    }
}
