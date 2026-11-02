Shader "Forgeborne/OpticGlass"
{
    Properties
    {
        _TintColor("Glass tint and absorption",Color)=(0.76,0.90,0.94,0.035)
        _Roughness("Optical surface roughness",Range(0,1))=0.08
        _ReflectionStrength("Coating reflection",Range(0,1))=0.15
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Name "Optical coating"
            Tags { "LightMode"="UniversalForward" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
                half _Roughness,_ReflectionStrength;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS:POSITION;
                float3 normalOS:NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS:SV_POSITION;
                float3 positionWS:TEXCOORD0;
                half3 normalWS:TEXCOORD1;
                half fog:TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            Varyings Vertex(Attributes input)
            {
                Varyings output;UNITY_SETUP_INSTANCE_ID(input);UNITY_TRANSFER_INSTANCE_ID(input,output);
                VertexPositionInputs positions=GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS=positions.positionCS;output.positionWS=positions.positionWS;
                output.normalWS=TransformObjectToWorldNormal(input.normalOS);
                output.fog=ComputeFogFactor(positions.positionCS.z);return output;
            }
            half4 Fragment(Varyings input):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half3 view=GetWorldSpaceNormalizeViewDir(input.positionWS);
                half3 normal=normalize(input.normalWS);
                half grazing=1-saturate(abs(dot(view,normal)));
                half fresnel=.04h+.96h*grazing*grazing*grazing*grazing*grazing;
                half alpha=saturate(_TintColor.a+.20h*fresnel);
                half3 reflection=GlossyEnvironmentReflection(reflect(-view,normal),input.positionWS,_Roughness,1);
                half3 colour=_TintColor.rgb*SampleSH(normal)*alpha+reflection*fresnel*_ReflectionStrength;
                colour=MixFog(colour/max(alpha,.001h),input.fog)*alpha;
                return half4(colour,alpha);
            }
            ENDHLSL
        }
    }
}
