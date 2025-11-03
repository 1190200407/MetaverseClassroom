using UnityEngine;
using UnityEngine.Rendering;
 
public class OutlineObject : MonoBehaviour {
    private Material stencilMaterial; // 模板材质
 
    private void Awake() {
        stencilMaterial = new Material(Shader.Find("Custom/Outline/Stencil"));
    }
 
    private void OnEnable() {
        OutlineEffect.renderEvent += OnRenderEvent;
        // _StartTime用于控制每个选中的对象颜色渐变不同步
        stencilMaterial.SetFloat("_StartTime", Time.timeSinceLevelLoad * 2);
    }
 
    private void OnDisable() {
        OutlineEffect.renderEvent -= OnRenderEvent;
    }
 
    private void OnRenderEvent(CommandBuffer commandBuffer) {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) {
            commandBuffer.DrawRenderer(r, stencilMaterial); // 将renderer和material提交到主camera的commandbuffer列表进行渲染
        }
    }
}
// ————————————————
// 版权声明：本文为CSDN博主「little_fat_sheep」的原创文章，遵循CC 4.0 BY-SA版权协议，转载请附上原文出处链接及本声明。
// 原文链接：https://blog.csdn.net/m0_37602827/article/details/127937019