Shader "Custom/Outline/Stencil"
{
	Properties
	{
		_StartTime ("startTime", Float) = 0 // _StartTime用于控制每个选中的对象颜色渐变不同步
	}
 
	SubShader
	{
		Pass
		{	
			CGPROGRAM // CG语言的开始
			// 编译指令 着色器名称 函数名称
			#pragma vertex vert // 顶点着色器, 每个顶点执行一次
			#pragma fragment frag // 片段着色器, 每个像素执行一次
			#pragma fragmentoption ARB_precision_hint_fastest // fragment使用最低精度, fp16, 提高性能和速度
 
			// 导入头文件
			#include "UnityCG.cginc"
 
			float _StartTime;
 
			struct appdata // 顶点函数输入结构体
			{
				half4 vertex: POSITION; // 顶点坐标
			};
 
			struct v2f // 顶点函数输出结构体
			{
				float4 pos : SV_POSITION;
			};
			
			v2f vert(appdata v) // 顶点着色器
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target // 片段着色器
			{
				// float t1 = sin(_Time.z - _StartTime); // _Time = float4(t/20, t, t*2, t*3)
				// float t2 = cos(_Time.z - _StartTime);
				// // 描边颜色随时间变化, 描边透明度随时间变化, 视觉上感觉描边在膨胀和收缩
				// return float4(t1 + 1, t2 + 1, 1 - t1, 1 - t2);
				return float4(1, 1, 1, 1);
			}
 
			ENDCG // CG语言的结束
		}
	}
 
	FallBack off
}
// ————————————————
// 版权声明：本文为CSDN博主「little_fat_sheep」的原创文章，遵循CC 4.0 BY-SA版权协议，转载请附上原文出处链接及本声明。
// 原文链接：https://blog.csdn.net/m0_37602827/article/details/127937019