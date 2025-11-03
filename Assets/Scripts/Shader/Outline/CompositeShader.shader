Shader "Custom/Outline/Composite"
{
	Properties
	{
		_MainTex ("source", 2D) = "" {}
		_StencilTex ("stencil", 2D) = "" {}
		_BlurTex ("blur", 2D) = "" {}
		_OutlineStrength ("OutlineStrength", Range(1, 5)) = 3
	}
	
	SubShader
	{
		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off
			Lighting Off
			Fog { Mode off }
			
			CGPROGRAM // CG语言的开始
			#pragma vertex vert // 顶点着色器, 每个顶点执行一次
			#pragma fragment frag // 片段着色器, 每个像素执行一次
			#pragma fragmentoption ARB_precision_hint_fastest // fragment使用最低精度, fp16, 提高性能和速度
			
			#include "UnityCG.cginc"
		
			sampler2D _MainTex;
			sampler2D _StencilTex;
			sampler2D _BlurTex;
			float _OutlineStrength;
			float4 _MainTex_TexelSize; //_MainTex的像素尺寸大小, float4(1/width, 1/height, width, height)
 
			struct appdata // 顶点函数输入结构体
			{
				half4 vertex: POSITION;
				half2 texcoord: TEXCOORD0;
			};
 
			struct v2f // 顶点函数输出结构体
			{
				float4 pos : POSITION;
				half2 uv : TEXCOORD0;
			};
			
			v2f vert (appdata v) // 顶点函数输出结构体
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1 - o.uv.y; // 在Direct3D平台下, 如果我们开启了抗锯齿, 则_MainTex_TexelSize.y 会变成负值
				return o;
			}
			
			fixed4 frag(v2f i) : COLOR // 顶点着色器
			{
				fixed4 source = tex2D(_MainTex, i.uv);
				fixed4 stencil = tex2D(_StencilTex, i.uv);
				if (any(stencil.rgb))
				{ // 绘制选中物体
					return source;
				}
				else
				{ // 绘制选中物体以外的图像
					fixed4 blur = tex2D(_BlurTex, i.uv);
					fixed4 color;
					color.rgb = lerp(source.rgb, blur.rgb * _OutlineStrength, saturate(blur.a - stencil.a));
					color.a = source.a;
					return color;
				}
			}
 
			ENDCG // CG语言的结束
		}
	}
	
	Fallback Off
}
// ————————————————
// 版权声明：本文为CSDN博主「little_fat_sheep」的原创文章，遵循CC 4.0 BY-SA版权协议，转载请附上原文出处链接及本声明。
// 原文链接：https://blog.csdn.net/m0_37602827/article/details/127937019