Shader "Forgeborne/FoundrySky"
{
    Properties
    {
        _Zenith("Storm zenith",Color)=(0.055,0.09,0.115,1)
        _Horizon("Haze",Color)=(0.28,0.34,0.37,1)
        _CloudDark("Cloud shadow",Color)=(0.075,0.11,0.135,1)
        _CloudLight("Cloud light",Color)=(0.30,0.34,0.35,1)
        _SunDirection("Sun direction",Vector)=(0.5,0.4,-0.6,0)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _Zenith,_Horizon,_CloudDark,_CloudLight,_SunDirection;
            CBUFFER_END
            struct Input { float4 positionOS:POSITION; };
            struct Output { float4 positionCS:SV_POSITION; float3 direction:TEXCOORD0; };
            Output Vertex(Input input)
            {
                Output output; output.positionCS=TransformObjectToHClip(input.positionOS.xyz);
                output.direction=input.positionOS.xyz; return output;
            }
            float Field(float2 samplePosition)
            {
                float2 cell=floor(samplePosition),f=frac(samplePosition);
                f=f*f*(3-2*f);
                float a=frac(sin(dot(cell,float2(41.71,17.29)))*12713.17);
                float b=frac(sin(dot(cell+float2(1,0),float2(41.71,17.29)))*12713.17);
                float c=frac(sin(dot(cell+float2(0,1),float2(41.71,17.29)))*12713.17);
                float d=frac(sin(dot(cell+1,float2(41.71,17.29)))*12713.17);
                return lerp(lerp(a,b,f.x),lerp(c,d,f.x),f.y);
            }
            half4 Fragment(Output input):SV_Target
            {
                float3 ray=normalize(input.direction);
                float elevation=saturate(ray.y);
                float3 sky=lerp(_Horizon.rgb,_Zenith.rgb,pow(elevation,0.55));
                float2 cloudPosition=ray.xz/max(ray.y+0.22,0.10)+float2(_Time.y*0.002,0);
                float cloud=Field(cloudPosition*2.7)*0.58+Field(cloudPosition*5.9+17)*0.28+Field(cloudPosition*12.3)*0.14;
                float density=smoothstep(0.35,0.74,cloud)*smoothstep(-0.08,0.18,ray.y);
                float sunFacing=saturate(dot(ray,normalize(_SunDirection.xyz)));
                float3 cloudColour=lerp(_CloudDark.rgb,_CloudLight.rgb,saturate(cloud*0.7+pow(sunFacing,8)*0.4));
                sky=lerp(sky,cloudColour,density*0.92);
                sky+=float3(0.38,0.22,0.10)*pow(sunFacing,24)*(1-density*0.7);
                sky+=float3(1.4,0.95,0.55)*pow(sunFacing,1800)*(1-density);
                sky=lerp(_Horizon.rgb*0.45,sky,smoothstep(-0.2,0.02,ray.y));
                return half4(sky,1);
            }
            ENDHLSL
        }
    }
}
