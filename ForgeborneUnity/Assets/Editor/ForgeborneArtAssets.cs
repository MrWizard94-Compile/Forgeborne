using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>Imports original authored meshes into reusable runtime prefabs. Does not edit Main.</summary>
public static class ForgeborneArtAssets
{
    private const string Root = "Assets/Resources/Art";

    [MenuItem("Forgeborne/Art/Apply Foundry Atmosphere")]
    public static void ApplyAtmosphere()
    {
        var scene=SceneManager.GetActiveScene();
        if(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating
            || scene.path!=ForgeborneSceneBuilder.MainPath || scene.isDirty)
            throw new InvalidOperationException("Open saved Main with Play stopped and imports finished.");
        var shader=Shader.Find("Forgeborne/FoundrySky");
        if(shader==null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Storm sky shader must compile.");
        if(Resources.Load<GameObject>("Art/Prefabs/RelayFoundry")==null) throw new InvalidOperationException("Prepare original foundry model first.");
        var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layer=tags.FindProperty("layers").GetArrayElementAtIndex(30);
        if(!string.IsNullOrEmpty(layer.stringValue) && layer.stringValue!="ForgeborneView")
            throw new InvalidOperationException("Layer 30 already belongs to another feature; review before changing it.");
        layer.stringValue="ForgeborneView";tags.ApplyModifiedPropertiesWithoutUndo();
        string path=Root+"/Materials/FoundrySky.mat";
        var sky=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(sky==null) {sky=new Material(shader);AssetDatabase.CreateAsset(sky,path);}
        sky.shader=shader;
        var lighting=UnityEngine.Object.FindObjectsByType<DuskLighting>(FindObjectsSortMode.None).Single();
        lighting.SunColor=new Color(1,0.85f,0.67f);lighting.SunIntensity=1.8f;
        lighting.SunRotation=new Vector3(23,218,0);
        lighting.AmbientColor=new Color(0.18f,0.25f,0.32f);
        lighting.FogColor=new Color(0.27f,0.32f,0.34f);lighting.FogDensity=0.014f;
        lighting.SkyboxMaterial=sky;RenderSettings.skybox=sky;
        var atmosphere=lighting.GetComponent<FoundryAtmosphere>();
        if(atmosphere==null) atmosphere=lighting.gameObject.AddComponent<FoundryAtmosphere>();
        atmosphere.Lighting=lighting;
        const string gradePath="Assets/Rendering/FoundryGrade.asset";
        var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(gradePath);
        if(profile==null) {profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,gradePath);}
        Tonemapping tone; if(!profile.TryGet(out tone)) tone=profile.Add<Tonemapping>(true);
        tone.mode.Override(TonemappingMode.ACES);
        Bloom bloom; if(!profile.TryGet(out bloom)) bloom=profile.Add<Bloom>(true);
        bloom.threshold.Override(1.2f);bloom.intensity.Override(0.22f);bloom.scatter.Override(0.55f);
        ColorAdjustments colour; if(!profile.TryGet(out colour)) colour=profile.Add<ColorAdjustments>(true);
        colour.contrast.Override(4);colour.saturation.Override(-5);colour.postExposure.Override(0.35f);
        foreach(var component in profile.components)
        {
            if(!AssetDatabase.Contains(component)) AssetDatabase.AddObjectToAsset(component,profile);
            EditorUtility.SetDirty(component);
        }
        var volume=UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsSortMode.None).FirstOrDefault(v=>v.name=="Foundry grade");
        if(volume==null) volume=new GameObject("Foundry grade").AddComponent<Volume>();
        volume.isGlobal=true;volume.priority=5;volume.sharedProfile=profile;
        foreach(var camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            camera.allowHDR=true;camera.allowMSAA=true;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
            EditorUtility.SetDirty(camera);
        }
        var pipeline=GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
        if(pipeline==null) throw new InvalidOperationException("Declared URP pipeline missing.");
        pipeline.supportsHDR=true;pipeline.maxAdditionalLightsCount=8;
        pipeline.colorGradingMode=ColorGradingMode.HighDynamicRange;
        var pipelineSettings=new SerializedObject(pipeline);
        pipelineSettings.FindProperty("m_ReflectionProbeBoxProjection").boolValue=true;
        pipelineSettings.FindProperty("m_ReflectionProbeBlending").boolValue=true;
        pipelineSettings.FindProperty("m_SoftShadowsSupported").boolValue=true;
        pipelineSettings.ApplyModifiedPropertiesWithoutUndo();
        var rendererData=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Rendering/ForgeborneRenderer.asset");
        if(rendererData==null) throw new InvalidOperationException("Declared URP renderer missing.");
        rendererData.postProcessData=AssetDatabase.LoadAssetAtPath<PostProcessData>(
            "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
        if(rendererData.postProcessData==null) throw new InvalidOperationException("URP post-process consumer resources missing.");
        var occlusion=rendererData.rendererFeatures.OfType<ScreenSpaceAmbientOcclusion>().SingleOrDefault();
        if(occlusion==null)
        {
            occlusion=ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();
            occlusion.name="Foundry contact shading";
            AssetDatabase.AddObjectToAsset(occlusion,rendererData);
            rendererData.rendererFeatures.Add(occlusion);
        }
        // Exact URP 17 serialized settings; interleaved noise needs no imported noise art.
        var settings=new SerializedObject(occlusion);
        var ao=settings.FindProperty("m_Settings");
        ao.FindPropertyRelative("AOMethod").enumValueIndex=1;
        ao.FindPropertyRelative("Downsample").boolValue=true;
        ao.FindPropertyRelative("Source").enumValueIndex=0;
        ao.FindPropertyRelative("NormalSamples").enumValueIndex=2;
        ao.FindPropertyRelative("Intensity").floatValue=1.2f;
        ao.FindPropertyRelative("DirectLightingStrength").floatValue=0.12f;
        ao.FindPropertyRelative("Radius").floatValue=0.25f;
        ao.FindPropertyRelative("Falloff").floatValue=40;
        settings.ApplyModifiedPropertiesWithoutUndo();
        occlusion.SetActive(true);rendererData.SetDirty();
        EditorUtility.SetDirty(occlusion);EditorUtility.SetDirty(rendererData);
        EditorUtility.SetDirty(pipeline);EditorUtility.SetDirty(lighting);EditorUtility.SetDirty(atmosphere);
        EditorUtility.SetDirty(profile);EditorUtility.SetDirty(sky);
        PlayerSettings.colorSpace=ColorSpace.Linear;
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }

    [MenuItem("Forgeborne/Art/Prepare Authored Models")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            throw new InvalidOperationException("Stopped, imported Editor required.");
        var shader=Shader.Find("Forgeborne/IndustrialSurface");
        if(shader==null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Industrial shader must import cleanly.");
        Folder(Root,"Materials"); Folder(Root,"Prefabs");
        var materials=new Dictionary<string,Material>();
        materials.Add("Graphite",Surface(shader,"Graphite",new Color(0.12f,0.145f,0.16f),0.42f,0.52f,0.5f));
        materials.Add("Steel",Surface(shader,"Steel",new Color(0.31f,0.39f,0.42f),0.88f,0.68f,0.32f));
        materials.Add("Copper",Surface(shader,"Copper",new Color(0.43f,0.19f,0.075f),0.77f,0.60f,0.4f));
        materials.Add("Ceramic",Surface(shader,"Ceramic",new Color(0.57f,0.63f,0.61f),0.12f,0.40f,0.22f));
        materials.Add("Rubber",Surface(shader,"Rubber",new Color(0.018f,0.025f,0.027f),0,0.18f,0.1f));
        materials.Add("Cyan",Surface(shader,"Cyan",new Color(0.025f,0.62f,0.85f),0.25f,0.65f,0,2.5f));
        materials.Add("Amber",Surface(shader,"Amber",new Color(1,0.19f,0.018f),0.2f,0.62f,0,2.5f));
        materials.Add("Basalt",Surface(shader,"Basalt",new Color(0.12f,0.145f,0.16f),0.03f,0.22f,0.5f));
        materials.Add("Paint",Surface(shader,"Paint",new Color(0.10f,0.22f,0.24f),0.55f,0.42f,0.42f));
        materials.Add("Warm",Surface(shader,"Warm",new Color(1,0.43f,0.15f),0.1f,0.5f,0,1.8f));
        materials.Add("Glass",Surface(shader,"Glass",new Color(0.12f,0.20f,0.24f),0.55f,0.82f,0.12f,0.08f));
        foreach(var name in new[]{"PulseCarbine","EmberWarden","AetherRelay","ForgeSuit","RelayFoundry"})
        {
            string path=Root+"/Models/"+name+".fbx";
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(model==null) throw new InvalidOperationException("Missing original model: "+path);
            var preview=EditorSceneManager.NewPreviewScene();
            try
            {
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(model,preview);
                instance.name=name;
                foreach(var renderer in instance.GetComponentsInChildren<Renderer>(true))
                {
                    var slots=renderer.sharedMaterials;
                    for(int i=0;i<slots.Length;i++)
                    {
                        Material replacement;
                        if(slots[i]==null || !materials.TryGetValue(slots[i].name,out replacement))
                            throw new InvalidOperationException("Unmapped authored material in "+name+": "+(slots[i]!=null ? slots[i].name : "null"));
                        slots[i]=replacement;
                    }
                    renderer.sharedMaterials=slots;
                    renderer.shadowCastingMode=name=="PulseCarbine" ? ShadowCastingMode.Off : ShadowCastingMode.On;
                }
                if(instance.GetComponentsInChildren<Collider>(true).Length!=0)
                    throw new InvalidOperationException("Art must not create parallel collision ownership.");
                var axes=instance.GetComponentsInChildren<Transform>(true);
                if((axes.Single(t=>t.name=="ArtRight").position-Vector3.right*0.25f).sqrMagnitude>0.0001f
                    || (axes.Single(t=>t.name=="ArtUp").position-Vector3.up*0.25f).sqrMagnitude>0.0001f
                    || (axes.Single(t=>t.name=="ArtForward").position-Vector3.forward*0.25f).sqrMagnitude>0.0001f)
                    throw new InvalidOperationException("Authored handedness/axis conversion failed: "+name);
                var renderers=instance.GetComponentsInChildren<Renderer>(true);
                if(renderers.Length==0) throw new InvalidOperationException("No mesh consumer in "+name);
                var bounds=renderers[0].bounds;
                foreach(var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
                float limit=name=="RelayFoundry" ? 200 : 2;
                if(bounds.size.magnitude>limit || bounds.size.magnitude<0.25f)
                    throw new InvalidOperationException("Authored metre conversion failed: "+name+" "+bounds);
                if(name=="PulseCarbine")
                {
                    var muzzle=instance.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="MuzzleSocket");
                    if(Mathf.Abs(muzzle.position.z-0.665f)>0.01f || Mathf.Abs(muzzle.position.y-0.016f)>0.01f)
                        throw new InvalidOperationException("Authored forward/up conversion failed: "+muzzle.position);
                }
                PrefabUtility.SaveAsPrefabAsset(instance,Root+"/Prefabs/"+name+".prefab");
                Debug.Log("Forgeborne authored model prepared: "+name+" "+bounds);
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }
        AssetDatabase.SaveAssets();
    }

    static Material Surface(Shader shader,string name,Color colour,float metal,float smooth,float weather,float emission=0)
    {
        string path=Root+"/Materials/"+name+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null) { material=new Material(shader); AssetDatabase.CreateAsset(material,path); }
        material.name=name; material.shader=shader;
        material.SetColor("_BaseColor",colour);
        material.SetFloat("_Metallic",metal); material.SetFloat("_Smoothness",smooth); material.SetFloat("_Weather",weather);
        material.SetColor("_WearColor",name=="Copper" ? new Color(0.14f,0.25f,0.19f) : new Color(0.34f,0.39f,0.41f));
        material.SetColor("_EmissionColor",colour*emission);
        EditorUtility.SetDirty(material);
        return material;
    }
    static void Folder(string parent,string child)
    {
        if(!AssetDatabase.IsValidFolder(parent+"/"+child)) AssetDatabase.CreateFolder(parent,child);
    }
}
