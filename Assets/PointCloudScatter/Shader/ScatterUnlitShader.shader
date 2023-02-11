Shader "Scatter/UnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        // ZWrite Off Cull Off

        Pass
        {           
            CGPROGRAM           
            
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma target 4.5

            #include "UnityCG.cginc"

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct ScatterPoint
            {
                float4x4 transform;
                float3 direction;
                float3 randomize;
            };

            #if SHADER_TARGET >= 45
                StructuredBuffer<ScatterPoint> scatterBuffer;
            #endif

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                
                float3 objectPos : TEXCOORD2;
                float3 worldPos : TEXCOORD3;                
            };

            v2f vert (appdata v, uint instanceID : SV_INSTANCEID)
            {
                #if SHADER_TARGET >= 45
                    float4x4 transformMat = scatterBuffer[instanceID].transform;
                    float3 direction = scatterBuffer[instanceID].direction;
                #else
                    float4x4 transformMat = 0;
                    float3 direction = 0;
                #endif
                
                float3 originPosition = mul(transformMat, float4(0, 0, 0, 1));
                float3 localPosition = mul(transformMat, v.vertex.xyz);
                float3 worldPosition = originPosition + localPosition;

                float4 originVert = mul(UNITY_MATRIX_VP, float4(originPosition, 1.0f));
                
                v2f o;
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
                o.normal = v.normal;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i, uint id : SV_INSTANCEID) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
