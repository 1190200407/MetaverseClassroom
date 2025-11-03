Shader "Custom/Outline/Blur"
{
	Properties
	{
		_MainTex ("stencil", 2D) = "" {}
		_OffsetScale ("offsetScale", Range (0.1, 3)) = 2 // 模糊采样偏移
	}
	
	SubShader
	{
		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off
			Lighting Off
			Fog { Mode Off }
			
			CGPROGRAM // CG语言的开始
			#pragma vertex vert // 顶点着色器, 每个顶点执行一次
			#pragma fragment frag // 片段着色器, 每个像素执行一次
			#pragma fragmentoption ARB_precision_hint_fastest // fragment使用最低精度, fp16, 提高性能和速度
			
			#include "UnityCG.cginc"
 
			sampler2D _MainTex;
			half _OffsetScale;
			half4 _MainTex_TexelSize; //_MainTex的像素尺寸大小, float4(1/width, 1/height, width, height)
 
			struct appdata // 顶点函数输入结构体
			{
				half4 vertex: POSITION;
				half2 texcoord: TEXCOORD0;
			};
 
			struct v2f // 顶点函数输出结构体
			{
				float4 pos : POSITION;
				half2 uv[4] : TEXCOORD0;
			};
			
			v2f vert (appdata v) // 顶点着色器
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				half2 offs = _MainTex_TexelSize.xy * _OffsetScale;
				// uv坐标向四周扩散
				o.uv[0].x = v.texcoord.x - offs.x;
				o.uv[0].y = v.texcoord.y - offs.y;
				o.uv[1].x = v.texcoord.x + offs.x;
				o.uv[1].y = v.texcoord.y - offs.y;
				o.uv[2].x = v.texcoord.x + offs.x;
				o.uv[2].y = v.texcoord.y + offs.y;
				o.uv[3].x = v.texcoord.x - offs.x;
				o.uv[3].y = v.texcoord.y + offs.y;
				return o;
			}
 
			fixed4 frag(v2f i) : COLOR // 片段着色器
			{
				fixed4 color1 = tex2D(_MainTex, i.uv[0]);
				fixed4 color2 = tex2D(_MainTex, i.uv[1]);
				fixed4 color3 = tex2D(_MainTex, i.uv[2]);
				fixed4 color4 = tex2D(_MainTex, i.uv[3]);
				fixed4 color;
				// max: 2个向量中每个分量都取较大者, 这里通过max函数将模板的边缘向外扩, rgb=stencil.rgb
				color.rgb = max(color1.rgb, color2.rgb);
				color.rgb = max(color.rgb, color3.rgb);
				color.rgb = max(color.rgb, color4.rgb);
				color.a = (color1.a + color2.a + color3.a + color4.a) / 4; // 透明度向外逐渐减小
				return color;
			}
			
			ENDCG // CG语言的结束
		}
	}
	
	Fallback off
}
// ————————————————
// 版权声明：本文为CSDN博主「little_fat_sheep」的原创文章，遵循CC 4.0 BY-SA版权协议，转载请附上原文出处链接及本声明。
// 原文链接：https://blog.csdn.net/m0_37602827/article/details/127937019