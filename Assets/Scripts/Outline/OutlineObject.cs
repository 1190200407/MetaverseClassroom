using UnityEngine;
using UnityEngine.Rendering;
 
public class OutlineObject : MonoBehaviour {
    private Material stencilMaterial; // ģ�����
 
    private void Awake() {
        stencilMaterial = new Material(Shader.Find("Custom/Outline/Stencil"));
    }
 
    private void OnEnable() {
        OutlineEffect.renderEvent += OnRenderEvent;
        // _StartTime���ڿ���ÿ��ѡ�еĶ�����ɫ���䲻ͬ��
        stencilMaterial.SetFloat("_StartTime", Time.timeSinceLevelLoad * 2);
    }
 
    private void OnDisable() {
        OutlineEffect.renderEvent -= OnRenderEvent;
    }
 
    private void OnRenderEvent(CommandBuffer commandBuffer) {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) {
            commandBuffer.DrawRenderer(r, stencilMaterial); // ��renderer��material�ύ����camera��commandbuffer�б������Ⱦ
        }
    }
}
// ��������������������������������
// ��Ȩ����������ΪCSDN������little_fat_sheep����ԭ�����£���ѭCC 4.0 BY-SA��ȨЭ�飬ת���븽��ԭ�ĳ������Ӽ���������
// ԭ�����ӣ�https://blog.csdn.net/m0_37602827/article/details/127937019