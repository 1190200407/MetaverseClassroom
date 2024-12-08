
using System;
using UnityEngine;
using UnityEngine.Rendering;
 
public class OutlineEffect : MonoBehaviour {
    public static Action<CommandBuffer> renderEvent; // 渲染事件
    public float offsetScale = 2; // 模糊处理像素偏移
    public int iterate = 3; // 模糊处理迭代次数
    public float outlineStrength = 3; // 描边强度
 
    private Material blurMaterial; // 模糊材质
    private Material compositeMaterial; // 合成材质
    private CommandBuffer commandBuffer; // 用于渲染模板纹理
    private RenderTexture stencilTex; // 模板纹理
    private RenderTexture blurTex; // 模糊纹理
 
    private void Awake() {
        blurMaterial = new Material(Shader.Find("Custom/Outline/Blur"));
        compositeMaterial = new Material(Shader.Find("Custom/Outline/Composite"));
        commandBuffer = new CommandBuffer();
    }
 
    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (renderEvent != null) {
            RenderStencil(); // 渲染模板纹理
            RenderBlur(source.width, source.height); // 渲染模糊纹理
            RenderComposite(source, destination); // 渲染合成纹理
        } else {
            Graphics.Blit(source, destination); // 保持原图
        }
    }
 
    private void RenderStencil() { // 渲染模板纹理
        stencilTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
        commandBuffer.SetRenderTarget(stencilTex);
        commandBuffer.ClearRenderTarget(true, true, Color.clear); // 设置模板清屏颜色为(0,0,0,0)
        renderEvent.Invoke(commandBuffer);
        Graphics.ExecuteCommandBuffer(commandBuffer);
    }
 
    private void RenderBlur(int width, int height) { // 对模板纹理进行模糊化
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
 
    private void RenderComposite(RenderTexture source, RenderTexture destination) { // 渲染合成纹理
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

// ————————————————
// 版权声明：本文为CSDN博主「little_fat_sheep」的原创文章，遵循CC 4.0 BY-SA版权协议，转载请附上原文出处链接及本声明。
// 原文链接：https://blog.csdn.net/m0_37602827/article/details/127937019