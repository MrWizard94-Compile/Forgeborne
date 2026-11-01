Shader "Forgeborne/VoxelSurface"
{
    Properties
    {
        _BaseColor("Surface tint", Color) = (1,1,1,1)
        _GridStrength("Block edge definition", Range(0,0.5)) = 0.16
        _SurfaceVariation("Surface variation", Range(0,0.4)) = 0.18
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "VoxelForward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _GridStrength;
                float _SurfaceVariation;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float2 blockType : TEXCOORD1;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half4 color : COLOR;
                float4 shadowCoord : TEXCOORD2;
                half fog : TEXCOORD3;
                nointerpolation float blockType : TEXCOORD4;
            };
            float Grain(float3 samplePosition)
            {
                float3 cell = floor(samplePosition);
                float3 part = frac(samplePosition);
                part = part * part * (3.0 - 2.0 * part);
                float result = 0;
                [unroll] for (int x = 0; x < 2; x++)
                [unroll] for (int y = 0; y < 2; y++)
                [unroll] for (int z = 0; z < 2; z++)
                {
                    float3 corner = float3(x,y,z);
                    float3 blend = lerp(1.0 - part, part, corner);
                    float noise = frac(sin(dot(cell + corner, float3(27.91,53.17,91.07))) * 15437.351);
                    result += noise * blend.x * blend.y * blend.z;
                }
                return result;
            }
            Varyings Vertex(Attributes input)
            {
                Varyings output;
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.color = input.color;
                output.shadowCoord = TransformWorldToShadowCoord(position.positionWS);
                output.fog = ComputeFogFactor(position.positionCS.z);
                output.blockType = input.blockType.x;
                return output;
            }
            half4 Fragment(Varyings input) : SV_Target
            {
                half3 normal = normalize(input.normalWS);
                float3 absoluteNormal = abs(normal);
                float2 plane = absoluteNormal.y > 0.5 ? input.positionWS.xz :
                    (absoluteNormal.x > 0.5 ? input.positionWS.yz : input.positionWS.xy);
                float2 centreDistance = abs(frac(plane) - 0.5);
                float edgeDistance = max(centreDistance.x, centreDistance.y);
                float edge = smoothstep(0.465 - max(fwidth(edgeDistance),0.002),0.495,edgeDistance);
                float grain = Grain(input.positionWS * 7.0) * 0.7 + Grain(input.positionWS * 25.0) * 0.3;
                half3 surface = input.color.rgb * _BaseColor.rgb;
                surface *= 1.0 + (grain - 0.5) * _SurfaceVariation;
                surface *= 1.0 - edge * _GridStrength;
                Light sun = GetMainLight(input.shadowCoord);
                half diffuse = saturate(dot(normal,sun.direction));
                half3 illumination = SampleSH(normal) + sun.color * diffuse * sun.distanceAttenuation * sun.shadowAttenuation;
                half3 view = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half highlight = pow(saturate(dot(normal,normalize(sun.direction + view))),32.0) * 0.10;
                half3 colour = surface * max(illumination,0.08) + highlight * sun.color * sun.shadowAttenuation;
                if (input.blockType > 1.5 && input.blockType < 2.5)
                    colour += surface * (0.08 + grain * grain * 0.10);
                return half4(MixFog(colour,input.fog),1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
