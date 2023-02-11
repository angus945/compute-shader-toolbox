Shader "Scatter/PreviewShader"
{
    Properties
    {
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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                
                float3 objectPos : TEXCOORD1;
                float3 worldPos : TEXCOORD2;

                fixed4 color : TEXCOORD3;
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
                // o.color = fixed4(_Color.rgb * dot(_WorldSpaceLightPos0, direction), 1);
                o.color = fixed4(abs(v.normal.rgb), 1);
                // o.color = fixed4(direction, 1);
                return o;
            }

            fixed4 frag (v2f i, uint id : SV_INSTANCEID) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
