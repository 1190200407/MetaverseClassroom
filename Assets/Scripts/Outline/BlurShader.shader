Shader "Custom/Outline/Blur"
{
	Properties
	{
		_MainTex ("stencil", 2D) = "" {}
		_OffsetScale ("offsetScale", Range (0.1, 3)) = 2 // ģ������ƫ��
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
			
			CGPROGRAM // CG���ԵĿ�ʼ
			#pragma vertex vert // ������ɫ��, ÿ������ִ��һ��
			#pragma fragment frag // Ƭ����ɫ��, ÿ������ִ��һ��
			#pragma fragmentoption ARB_precision_hint_fastest // fragmentʹ����;���, fp16, ������ܺ��ٶ�
			
			#include "UnityCG.cginc"
 
			sampler2D _MainTex;
			half _OffsetScale;
			half4 _MainTex_TexelSize; //_MainTex�����سߴ��С, float4(1/width, 1/height, width, height)
 
			struct appdata // ���㺯������ṹ��
			{
				half4 vertex: POSITION;
				half2 texcoord: TEXCOORD0;
			};
 
			struct v2f // ���㺯������ṹ��
			{
				float4 pos : POSITION;
				half2 uv[4] : TEXCOORD0;
			};
			
			v2f vert (appdata v) // ������ɫ��
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				half2 offs = _MainTex_TexelSize.xy * _OffsetScale;
				// uv������������ɢ
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
 
			fixed4 frag(v2f i) : COLOR // Ƭ����ɫ��
			{
				fixed4 color1 = tex2D(_MainTex, i.uv[0]);
				fixed4 color2 = tex2D(_MainTex, i.uv[1]);
				fixed4 color3 = tex2D(_MainTex, i.uv[2]);
				fixed4 color4 = tex2D(_MainTex, i.uv[3]);
				fixed4 color;
				// max: 2��������ÿ��������ȡ�ϴ���, ����ͨ��max������ģ��ı�Ե������, rgb=stencil.rgb
				color.rgb = max(color1.rgb, color2.rgb);
				color.rgb = max(color.rgb, color3.rgb);
				color.rgb = max(color.rgb, color4.rgb);
				color.a = (color1.a + color2.a + color3.a + color4.a) / 4; // ͸���������𽥼�С
				return color;
			}
			
			ENDCG // CG���ԵĽ���
		}
	}
	
	Fallback off
}
// ��������������������������������
// ��Ȩ����������ΪCSDN������little_fat_sheep����ԭ�����£���ѭCC 4.0 BY-SA��ȨЭ�飬ת���븽��ԭ�ĳ������Ӽ���������
// ԭ�����ӣ�https://blog.csdn.net/m0_37602827/article/details/127937019