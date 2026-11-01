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
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
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
                half3 vertexLight : TEXCOORD5;
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
            float MineralEdge(float2 p)
            {
                float2 cell=floor(p),part=frac(p);
                float closest=8,second=8;
                [unroll] for(int x=-1;x<=1;x++)
                [unroll] for(int y=-1;y<=1;y++)
                {
                    float2 neighbour=float2(x,y);
                    float2 seed=cell+neighbour;
                    float2 jitter=frac(sin(float2(dot(seed,float2(127.1,311.7)),dot(seed,float2(269.5,183.3))))*43758.5453);
                    float distance=length(neighbour+jitter-part);
                    if(distance<closest) {second=closest;closest=distance;}
                    else second=min(second,distance);
                }
                return second-closest;
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
                output.vertexLight = VertexLighting(position.positionWS,output.normalWS);
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
                half metal=input.blockType>3.5 ? 0.5 : 0.04;
                half smoothness=input.blockType>3.5 ? 0.48 : 0.22;
                half3 emission=0;
                float height=(grain-0.5)*0.001-edge*0.006;
                if (input.blockType > 1.5 && input.blockType < 2.5)
                {
                    float fracture=MineralEdge(plane*5.5);
                    float antialias=max(fwidth(fracture),0.003);
                    float vein=1-smoothstep(0.008,0.023+antialias,fracture);
                    vein*=smoothstep(0.32,0.54,Grain(input.positionWS*2.1));
                    half3 mineral=half3(0.20,0.18,0.15)*(0.72+grain*0.5);
                    surface=lerp(mineral,surface*0.65,vein);
                    emission=input.color.rgb*vein*0.18;
                    height-=vein*0.0015;
                    metal=0.34;smoothness=0.43;
                }
                float3 dx=ddx(input.positionWS),dy=ddy(input.positionWS);
                float determinant=dot(normal,cross(dx,dy));
                if(abs(determinant)>0.00000001)
                    normal=normalize(normal-(cross(dy,normal)*ddx(height)+cross(normal,dx)*ddy(height))/determinant);
                InputData lighting=(InputData)0;
                lighting.positionWS=input.positionWS;lighting.positionCS=input.positionCS;
                lighting.normalWS=normal;lighting.viewDirectionWS=GetWorldSpaceNormalizeViewDir(input.positionWS);
                lighting.shadowCoord=input.shadowCoord;lighting.vertexLighting=input.vertexLight;
                lighting.bakedGI=SampleSH(normal);lighting.shadowMask=half4(1,1,1,1);
                lighting.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(input.positionCS);
                SurfaceData material=(SurfaceData)0;
                material.albedo=surface;material.metallic=metal;
                material.smoothness=saturate(smoothness+(grain-0.5)*0.13);
                material.normalTS=half3(0,0,1);material.occlusion=1;material.alpha=1;material.emission=emission;
                half4 colour=UniversalFragmentPBR(lighting,material);
                colour.rgb=MixFog(colour.rgb,input.fog);
                return colour;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
