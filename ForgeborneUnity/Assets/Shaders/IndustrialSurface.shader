Shader "Forgeborne/IndustrialSurface"
{
    Properties
    {
        _BaseColor("Surface",Color)=(0.08,0.10,0.12,1)
        _Metallic("Metal",Range(0,1))=0.8
        _Smoothness("Smoothness",Range(0,1))=0.55
        _WearColor("Exposed surface",Color)=(0.34,0.39,0.41,1)
        _Weather("Weathering",Range(0,1))=0.35
        _WorldWeather("World-space architectural wear",Range(0,1))=0
        _EmissionColor("Emission",Color)=(0,0,0,1)
        _BaseMap("Original pigment (sRGB)",2D)="white"{}
        _SurfaceMap("Original height / roughness / occlusion / metal",2D)="white"{}
        _TextureStrength("Authored surface maps",Range(0,1))=0
        _SurfaceTiling("Repeats per metre",Float)=1
        _ReliefMetres("Material relief in metres",Float)=0.001
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "IndustrialForward"
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
                float4 _BaseColor, _WearColor, _EmissionColor;
                float _Metallic, _Smoothness, _Weather, _WorldWeather;
                float _TextureStrength, _SurfaceTiling, _ReliefMetres;
            CBUFFER_END
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_SurfaceMap); SAMPLER(sampler_SurfaceMap);
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings
            {
                float4 positionCS:SV_POSITION;
                float3 positionWS:TEXCOORD0;
                half3 normalWS:TEXCOORD1;
                float3 patternPosition:TEXCOORD2;
                half3 vertexLight:TEXCOORD3;
                half fog:TEXCOORD4;
                half3 patternNormal:TEXCOORD5;
            };
            float Lattice(float3 cell)
            {
                return frac(sin(dot(cell,float3(17.17,83.71,29.47)))*31571.37);
            }
            float Noise(float3 samplePosition)
            {
                float3 cell=floor(samplePosition), f=frac(samplePosition);
                f=f*f*(3-2*f);
                float value=0;
                [unroll] for(int x=0;x<2;x++)
                [unroll] for(int y=0;y<2;y++)
                [unroll] for(int z=0;z<2;z++)
                {
                    float3 corner=float3(x,y,z);
                    float3 w=lerp(1-f,f,corner);
                    value+=Lattice(cell+corner)*w.x*w.y*w.z;
                }
                return value;
            }
            Varyings Vertex(Attributes input)
            {
                Varyings output;
                VertexPositionInputs position=GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS=position.positionCS; output.positionWS=position.positionWS;
                output.normalWS=TransformObjectToWorldNormal(input.normalOS);
                output.patternPosition=input.positionOS.xyz;
                output.patternNormal=input.normalOS;
                output.vertexLight=VertexLighting(position.positionWS,output.normalWS);
                output.fog=ComputeFogFactor(position.positionCS.z);
                return output;
            }
            void OriginalSurface(float3 p,half3 n,out half3 pigment,out half4 packed)
            {
                half3 weight=pow(abs(n),4);weight/=max(dot(weight,half3(1,1,1)),0.0001);
                float3 uv=p*_SurfaceTiling;
                pigment=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv.zy).rgb*weight.x
                    +SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv.xz).rgb*weight.y
                    +SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv.xy).rgb*weight.z;
                packed=SAMPLE_TEXTURE2D(_SurfaceMap,sampler_SurfaceMap,uv.zy)*weight.x
                    +SAMPLE_TEXTURE2D(_SurfaceMap,sampler_SurfaceMap,uv.xz)*weight.y
                    +SAMPLE_TEXTURE2D(_SurfaceMap,sampler_SurfaceMap,uv.xy)*weight.z;
            }
            half4 Fragment(Varyings input):SV_Target
            {
                float3 p=lerp(input.patternPosition,input.positionWS,_WorldWeather);
                half3 base;half metal,smooth,occlusion=1;float height;
                if(_TextureStrength>0.5)
                {
                    half3 pigment;half4 packed;
                    OriginalSurface(p,normalize(lerp(input.patternNormal,input.normalWS,_WorldWeather)),pigment,packed);
                    float variation=0.91+Noise(p*0.27)*0.16;
                    base=pigment*_BaseColor.rgb*variation;
                    metal=packed.a;smooth=1-packed.g;occlusion=packed.b;
                    height=(packed.r-0.5)*_ReliefMetres;
                }
                else
                {
                    float broad=Noise(p*0.43)*0.65+Noise(p*3.1)*0.35;
                    float grain=Noise(p*170.0);
                    float scratches=smoothstep(0.93,0.985,Noise(p*float3(260,38,3)));
                    float worn=saturate((broad-0.35)*0.6+scratches*0.65)*_Weather;
                    base=lerp(_BaseColor.rgb,_WearColor.rgb,worn)*(0.92+grain*0.15);
                    float streak=Noise(p*float3(2.7,0.075,2.7));
                    float grime=smoothstep(0.54,0.82,broad*0.5+streak*0.5)*_Weather;
                    base*=1-grime*0.55;metal=_Metallic;
                    smooth=saturate(_Smoothness+(grain-0.5)*0.12-worn*0.25);
                    height=(grain-0.5)*0.00018*_Weather;
                }
                half3 normal=normalize(input.normalWS);
                float3 dx=ddx(input.positionWS),dy=ddy(input.positionWS);
                float determinant=dot(normal,cross(dx,dy));
                if(abs(determinant)>0.00000001)
                {
                    float3 gradient=(cross(dy,normal)*ddx(height)+cross(normal,dx)*ddy(height))/determinant;
                    gradient*=min(1.0,0.8/max(length(gradient),0.0001));
                    normal=normalize(normal-gradient);
                }
                InputData lighting=(InputData)0;
                lighting.positionWS=input.positionWS; lighting.positionCS=input.positionCS;
                lighting.normalWS=normal; lighting.viewDirectionWS=GetWorldSpaceNormalizeViewDir(input.positionWS);
                lighting.shadowCoord=TransformWorldToShadowCoord(input.positionWS);
                lighting.vertexLighting=input.vertexLight; lighting.bakedGI=SampleSH(normal);
                lighting.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(input.positionCS);
                lighting.shadowMask=half4(1,1,1,1);
                SurfaceData surface=(SurfaceData)0;
                surface.albedo=base; surface.metallic=metal;surface.smoothness=smooth;
                surface.normalTS=half3(0,0,1); surface.alpha=1; surface.occlusion=occlusion;
                surface.emission=_EmissionColor.rgb;
                half4 colour=UniversalFragmentPBR(lighting,surface);
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
