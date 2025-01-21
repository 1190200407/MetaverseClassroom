Shader "Custom/UnlitGlowShader"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0, 0, 1)      // 红色基础颜色
        _EmissionColor ("Emission Color", Color) = (1, 0, 0, 1)  // 发光颜色
        _MainTex ("Base Texture", 2D) = "white" {}   // 基础纹理（可选）
        _GlowRadius ("Glow Radius", Float) = 5.0      // 控制发光效果的半径
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" }

        Pass
        {
            // 禁用光照计算，使其不受光照影响
            Tags { "LightMode"="Always" }

            // 开启透明度混合和设置 Z 写入
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

            // 设置 Shader 程序
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 属性声明
            float4 _Color;
            float4 _EmissionColor;
            sampler2D _MainTex;
            float _GlowRadius;  // 控制发光半径

            // 顶点函数
            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float3 localPos : TEXCOORD1;  // 物体相对于自己的局部坐标
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = _Color; // 设置基础颜色
                o.uv = v.uv;

                // 传递物体相对于中心的局部坐标
                o.localPos = v.vertex.xyz;

                return o;
            }

            // 片元函数
            half4 frag(v2f i) : SV_Target
            {
                // 计算当前像素与物体中心的距离（使用局部坐标）
                float distanceToCenter = sqrt(i.localPos.x * i.localPos.x + i.localPos.z * i.localPos.z);  // 计算局部坐标系下的距离

                // 根据距离调整发光强度（从中心向外衰减）
                float glowFactor = saturate(1.0 - (distanceToCenter / _GlowRadius));

                // 获取纹理颜色
                half4 col = _EmissionColor * glowFactor;

                // 使用 glowFactor 来调整透明度
                col.a = glowFactor;  // 透明度会根据距离衰减

                return col;
            }

            ENDCG
        }
    }

    Fallback "Diffuse"
}
