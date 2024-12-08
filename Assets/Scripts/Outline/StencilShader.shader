Shader "Custom/Outline/Stencil"
{
	Properties
	{
		_StartTime ("startTime", Float) = 0 // _StartTime���ڿ���ÿ��ѡ�еĶ�����ɫ���䲻ͬ��
	}
 
	SubShader
	{
		Pass
		{	
			CGPROGRAM // CG���ԵĿ�ʼ
			// ����ָ�� ��ɫ������ ��������
			#pragma vertex vert // ������ɫ��, ÿ������ִ��һ��
			#pragma fragment frag // Ƭ����ɫ��, ÿ������ִ��һ��
			#pragma fragmentoption ARB_precision_hint_fastest // fragmentʹ����;���, fp16, ������ܺ��ٶ�
 
			// ����ͷ�ļ�
			#include "UnityCG.cginc"
 
			float _StartTime;
 
			struct appdata // ���㺯������ṹ��
			{
				half4 vertex: POSITION; // ��������
			};
 
			struct v2f // ���㺯������ṹ��
			{
				float4 pos : SV_POSITION;
			};
			
			v2f vert(appdata v) // ������ɫ��
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target // Ƭ����ɫ��
			{
				// float t1 = sin(_Time.z - _StartTime); // _Time = float4(t/20, t, t*2, t*3)
				// float t2 = cos(_Time.z - _StartTime);
				// // �����ɫ��ʱ��仯, ���͸������ʱ��仯, �Ӿ��ϸо���������ͺ�����
				// return float4(t1 + 1, t2 + 1, 1 - t1, 1 - t2);
				return float4(1, 1, 1, 1);
			}
 
			ENDCG // CG���ԵĽ���
		}
	}
 
	FallBack off
}
// ��������������������������������
// ��Ȩ����������ΪCSDN������little_fat_sheep����ԭ�����£���ѭCC 4.0 BY-SA��ȨЭ�飬ת���븽��ԭ�ĳ������Ӽ���������
// ԭ�����ӣ�https://blog.csdn.net/m0_37602827/article/details/127937019