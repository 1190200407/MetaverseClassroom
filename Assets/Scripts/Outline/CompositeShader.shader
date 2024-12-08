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
			
			CGPROGRAM // CG���ԵĿ�ʼ
			#pragma vertex vert // ������ɫ��, ÿ������ִ��һ��
			#pragma fragment frag // Ƭ����ɫ��, ÿ������ִ��һ��
			#pragma fragmentoption ARB_precision_hint_fastest // fragmentʹ����;���, fp16, ������ܺ��ٶ�
			
			#include "UnityCG.cginc"
		
			sampler2D _MainTex;
			sampler2D _StencilTex;
			sampler2D _BlurTex;
			float _OutlineStrength;
			float4 _MainTex_TexelSize; //_MainTex�����سߴ��С, float4(1/width, 1/height, width, height)
 
			struct appdata // ���㺯������ṹ��
			{
				half4 vertex: POSITION;
				half2 texcoord: TEXCOORD0;
			};
 
			struct v2f // ���㺯������ṹ��
			{
				float4 pos : POSITION;
				half2 uv : TEXCOORD0;
			};
			
			v2f vert (appdata v) // ���㺯������ṹ��
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1 - o.uv.y; // ��Direct3Dƽ̨��, ������ǿ����˿����, ��_MainTex_TexelSize.y ���ɸ�ֵ
				return o;
			}
			
			fixed4 frag(v2f i) : COLOR // ������ɫ��
			{
				fixed4 source = tex2D(_MainTex, i.uv);
				fixed4 stencil = tex2D(_StencilTex, i.uv);
				if (any(stencil.rgb))
				{ // ����ѡ������
					return source;
				}
				else
				{ // ����ѡ�����������ͼ��
					fixed4 blur = tex2D(_BlurTex, i.uv);
					fixed4 color;
					color.rgb = lerp(source.rgb, blur.rgb * _OutlineStrength, saturate(blur.a - stencil.a));
					color.a = source.a;
					return color;
				}
			}
 
			ENDCG // CG���ԵĽ���
		}
	}
	
	Fallback Off
}
// ��������������������������������
// ��Ȩ����������ΪCSDN������little_fat_sheep����ԭ�����£���ѭCC 4.0 BY-SA��ȨЭ�飬ת���븽��ԭ�ĳ������Ӽ���������
// ԭ�����ӣ�https://blog.csdn.net/m0_37602827/article/details/127937019