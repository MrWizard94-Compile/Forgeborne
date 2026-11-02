#nullable enable
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Original off-yard architecture, practical lighting and sky. Never owns game rules.</summary>
public sealed class FoundryAtmosphere : MonoBehaviour
{
    public DuskLighting? Lighting;
    private GameObject? _architecture;
    private GameObject? _fill;
    private GameObject? _probeObject;
    private Material? _sky;
    private Material? _previousSky;
    private AmbientMode _previousAmbientMode;
    private Color _previousAmbientSky,_previousAmbientEquator,_previousAmbientGround;
    private DefaultReflectionMode _previousReflectionMode;
    private float _previousReflectionIntensity;
    private bool _ownsEnvironment;
    private GameManager? _manager;

    void Start()
    {
        _manager=UnityEngine.Object.FindFirstObjectByType<GameManager>();
        if(Lighting==null || Lighting.DuskLight==null || Lighting.SkyboxMaterial==null)
            throw new InvalidOperationException("Wire authored foundry lighting before Play.");
        var prefab=Resources.Load<GameObject>("Art/Prefabs/RelayFoundry");
        if(prefab==null) throw new InvalidOperationException("Prepare original foundry architecture before Play.");
        _architecture=AuthoredSkin.Create(prefab,transform,"Forgeborne relay foundry context");
        _architecture.transform.position=new Vector3(16,4,16);
        var weather=new MaterialPropertyBlock();weather.SetFloat("_WorldWeather",1);
        foreach(var renderer in _architecture.GetComponentsInChildren<Renderer>(true)) renderer.SetPropertyBlock(weather);
        foreach(var anchor in _architecture.GetComponentsInChildren<Transform>(true))
        {
            if(!anchor.name.StartsWith("Lamp_",StringComparison.Ordinal)) continue;
            var practical=anchor.gameObject.AddComponent<Light>();
            practical.type=LightType.Point; practical.color=new Color(1,0.53f,0.24f);
            practical.intensity=10; practical.range=12; practical.shadows=LightShadows.None;
        }
        _sky=new Material(Lighting.SkyboxMaterial){name="Forgeborne foundry runtime sky"};
        _sky.SetVector("_SunDirection",-Lighting.DuskLight.transform.forward);
        _previousSky=RenderSettings.skybox;_previousAmbientMode=RenderSettings.ambientMode;
        _previousAmbientSky=RenderSettings.ambientSkyColor;
        _previousAmbientEquator=RenderSettings.ambientEquatorColor;
        _previousAmbientGround=RenderSettings.ambientGroundColor;
        _previousReflectionMode=RenderSettings.defaultReflectionMode;
        _previousReflectionIntensity=RenderSettings.reflectionIntensity;
        RenderSettings.skybox=_sky;_ownsEnvironment=true;
        RenderSettings.ambientMode=AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(0.35f,0.43f,0.51f);
        RenderSettings.ambientEquatorColor=new Color(0.22f,0.27f,0.29f);
        RenderSettings.ambientGroundColor=new Color(0.12f,0.13f,0.14f);
        RenderSettings.defaultReflectionMode=DefaultReflectionMode.Skybox;
        RenderSettings.reflectionIntensity=1.0f;
        _fill=new GameObject("Foundry sky fill");_fill.transform.SetParent(transform,false);
        var fill=_fill.AddComponent<Light>();fill.type=LightType.Directional;
        fill.color=new Color(0.65f,0.79f,0.93f);fill.intensity=0.36f;fill.shadows=LightShadows.None;
        _fill.transform.rotation=Quaternion.Euler(35,45,0);
        _probeObject=new GameObject("Foundry local reflection");_probeObject.transform.SetParent(transform,false);
        _probeObject.transform.position=new Vector3(16,8,16);
        var probe=_probeObject.AddComponent<ReflectionProbe>();
        probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.ViaScripting;
        probe.resolution=256;probe.size=new Vector3(50,30,50);probe.boxProjection=true;
        probe.cullingMask=~(1<<30);
        probe.clearFlags=ReflectionProbeClearFlags.Skybox;
        StartCoroutine(CaptureReflection(probe));
    }
    void LateUpdate()
    {
        if(_architecture!=null && _manager!=null && _manager.Simulation!=null)
            _architecture.SetActive(_manager.Simulation.World.Chunk.SurfaceProfile!=Forgeborne.Core.SurfaceProfile.AuthoredFoundryV1);
    }
    IEnumerator CaptureReflection(ReflectionProbe probe)
    {
        yield return null;
        if(probe!=null) probe.RenderProbe();
    }
    void OnDestroy()
    {
        // Do not overwrite a later scene/owner that has already installed its sky.
        if(_ownsEnvironment && RenderSettings.skybox==_sky)
        {
            RenderSettings.skybox=_previousSky;RenderSettings.ambientMode=_previousAmbientMode;
            RenderSettings.ambientSkyColor=_previousAmbientSky;
            RenderSettings.ambientEquatorColor=_previousAmbientEquator;
            RenderSettings.ambientGroundColor=_previousAmbientGround;
            RenderSettings.defaultReflectionMode=_previousReflectionMode;
            RenderSettings.reflectionIntensity=_previousReflectionIntensity;
        }
        if(_architecture!=null) Destroy(_architecture);
        if(_fill!=null) Destroy(_fill);
        if(_probeObject!=null) Destroy(_probeObject);
        if(_sky!=null) Destroy(_sky);
    }
}
