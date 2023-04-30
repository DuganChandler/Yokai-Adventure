Shader "Custom/Cel"
{
    Properties{
       _MainTex("Texture", 2D) = "white" {}
    }
    SubShader{
        Tags {"Queue" = "Transparent" "RenderType" = "Opaque"}
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                float4 finalColor = 0;
                float3 lightDir = normalize(float3(-1, 1, 1)); // Direction of the light source
                float diffuse = max(dot(i.normal, lightDir), 0); // Compute diffuse shading
                if (diffuse > 0.95) {
                    finalColor = float4(1, 1, 1, 1); // White highlight for the brightest areas
                }
                else if (diffuse > 0.5) {
                    finalColor = float4(0.7, 0.7, 0.7, 1); // Gray for medium bright areas
                }
                else {
                    finalColor = float4(0.3, 0.3, 0.3, 1); // Dark gray for shadows
                }
                return finalColor;
            }

            ENDCG
        }
    }
        FallBack "Diffuse"
}
