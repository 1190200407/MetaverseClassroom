
using System;
using UnityEngine;
using UnityEngine.Rendering;
 
public class OutlineEffect : MonoBehaviour {
    public static Action<CommandBuffer> renderEvent; // ��Ⱦ�¼�
    public float offsetScale = 2; // ģ����������ƫ��
    public int iterate = 3; // ģ�������������
    public float outlineStrength = 3; // ���ǿ��
 
    private Material blurMaterial; // ģ������
    private Material compositeMaterial; // �ϳɲ���
    private CommandBuffer commandBuffer; // ������Ⱦģ������
    private RenderTexture stencilTex; // ģ������
    private RenderTexture blurTex; // ģ������
 
    private void Awake() {
        blurMaterial = new Material(Shader.Find("Custom/Outline/Blur"));
        compositeMaterial = new Material(Shader.Find("Custom/Outline/Composite"));
        commandBuffer = new CommandBuffer();
    }
 
    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (renderEvent != null) {
            RenderStencil(); // ��Ⱦģ������
            RenderBlur(source.width, source.height); // ��Ⱦģ������
            RenderComposite(source, destination); // ��Ⱦ�ϳ�����
        } else {
            Graphics.Blit(source, destination); // ����ԭͼ
        }
    }
 
    private void RenderStencil() { // ��Ⱦģ������
        stencilTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
        commandBuffer.SetRenderTarget(stencilTex);
        commandBuffer.ClearRenderTarget(true, true, Color.clear); // ����ģ��������ɫΪ(0,0,0,0)
        renderEvent.Invoke(commandBuffer);
        Graphics.ExecuteCommandBuffer(commandBuffer);
    }
 
    private void RenderBlur(int width, int height) { // ��ģ���������ģ����
        blurTex = RenderTexture.GetTemporary(width, height, 0);
        RenderTexture temp = RenderTexture.GetTemporary(width, height, 0);
        blurMaterial.SetFloat("_OffsetScale", offsetScale);
        Graphics.Blit(stencilTex, blurTex, blurMaterial);
        for (int i = 0; i < iterate; i ++) {
            Graphics.Blit(blurTex, temp, blurMaterial);
            Graphics.Blit(temp, blurTex, blurMaterial);
        }
        RenderTexture.ReleaseTemporary(temp);
    }
 
    private void RenderComposite(RenderTexture source, RenderTexture destination) { // ��Ⱦ�ϳ�����
        compositeMaterial.SetTexture("_MainTex", source);
        compositeMaterial.SetTexture("_StencilTex", stencilTex);
        compositeMaterial.SetTexture("_BlurTex", blurTex);
        compositeMaterial.SetFloat("_OutlineStrength", outlineStrength);
        Graphics.Blit(source, destination, compositeMaterial);
        RenderTexture.ReleaseTemporary(stencilTex);
        RenderTexture.ReleaseTemporary(blurTex);
        stencilTex = null;
        blurTex = null;
    }
}

// ��������������������������������
// ��Ȩ����������ΪCSDN������little_fat_sheep����ԭ�����£���ѭCC 4.0 BY-SA��ȨЭ�飬ת���븽��ԭ�ĳ������Ӽ���������
// ԭ�����ӣ�https://blog.csdn.net/m0_37602827/article/details/127937019