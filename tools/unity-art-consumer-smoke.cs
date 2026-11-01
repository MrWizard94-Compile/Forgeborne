// Actual Unity consumer check for reusable authored art/render setup.
// The courtyard's block geometry is rejected. This does not grant visual acceptance.
var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig=UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null
    || !flow.IsMenuOpen || Time.timeScale!=0 || rig==null || rig.IsOrbital)
    throw new System.InvalidOperationException("Started paused FPS Main required.");
var simulation=g.Simulation;
string before=g.SaveToJson();
if(UnityEngine.Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length!=1
    || !object.ReferenceEquals(simulation,g.FpsAdapter.Simulation)
    || !object.ReferenceEquals(simulation,g.MeshRenderer.Simulation))
    throw new System.InvalidOperationException("Authored art changed shared Simulation ownership.");
var modelNames=new[]{"PulseCarbine","EmberWarden","AetherRelay","ForgeSuit","RelayFoundry"};
foreach(var name in modelNames)
{
    var prefab=Resources.Load<GameObject>("Art/Prefabs/"+name);
    if(prefab==null || prefab.GetComponentsInChildren<Renderer>(true).Length==0
        || prefab.GetComponentsInChildren<Collider>(true).Length!=0)
        throw new System.InvalidOperationException("Authored model missing or owns collision: "+name);
}
var weapon=rig.ViewModel.transform.Find("ForgeborneWeaponViewModel");
var suit=rig.transform.Find("Suit position");
if(weapon==null || suit==null || weapon.GetComponentsInChildren<Collider>(true).Length!=0
    || suit.GetComponentsInChildren<Collider>(true).Length!=0)
    throw new System.InvalidOperationException("Actual authored view consumers missing.");
var rendererData=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRendererData>(
    "Assets/Rendering/ForgeborneRenderer.asset");
if(rendererData==null || rendererData.postProcessData==null)
    throw new System.InvalidOperationException("Renderer has no post-processing consumer resources.");
var volume=System.Linq.Enumerable.Single(UnityEngine.Object.FindObjectsByType<UnityEngine.Rendering.Volume>(FindObjectsSortMode.None),
    v=>v.name=="Foundry grade");
UnityEngine.Rendering.Universal.ColorAdjustments grade;
UnityEngine.Rendering.Universal.Tonemapping tone;
if(!volume.sharedProfile.TryGet(out grade) || !volume.sharedProfile.TryGet(out tone)
    || tone.mode.value!=UnityEngine.Rendering.Universal.TonemappingMode.ACES)
    throw new System.InvalidOperationException("Actual grade profile missing.");
var camera=g.FpsAdapter.GetComponent<Camera>();
var data=camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
if(data==null || !data.renderPostProcessing || !camera.allowHDR || QualitySettings.activeColorSpace!=ColorSpace.Linear)
    throw new System.InvalidOperationException("Camera/pipeline is not consuming the declared HDR/linear grade.");
var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
float exposure=grade.postExposure.value;
var target=new RenderTexture(256,144,24);
var pixels=new Texture2D(256,144,TextureFormat.RGB24,false);
double original=0,brighter=0;
try
{
    rig.SetOrbital(true);
    if(!suit.gameObject.activeSelf || weapon.gameObject.activeSelf
        || suit.position!=new Vector3(simulation.Fps.X,simulation.Fps.Y,simulation.Fps.Z))
        throw new System.InvalidOperationException("Authored suit/view handoff failed.");
    rig.SetOrbital(false);
    camera.targetTexture=target;
    System.Func<float,double> measure=value=>
    {
        grade.postExposure.value=value;
        camera.Render();
        RenderTexture.active=target;
        pixels.ReadPixels(new Rect(0,0,256,144),0,0);pixels.Apply();
        double luminance=0;
        foreach(var colour in pixels.GetPixels32())
            luminance+=colour.r*0.2126+colour.g*0.7152+colour.b*0.0722;
        return luminance/(256*144*255);
    };
    original=measure(exposure);
    brighter=measure(exposure+1);
    if(original<0.005 || brighter<original*1.08)
        throw new System.InvalidOperationException("Grade did not affect rendered output: "+original+" -> "+brighter);
    return new{models=modelNames,originalMeanLuminance=original,exposurePlusOneMeanLuminance=brighter,
        actualPostProcessResponse=true,authoredSuitHandoff=true,noArtColliders=true,
        geometryAcceptance="FAIL: rejected cubic courtyard remains",nativeAcceptance=false};
}
finally
{
    grade.postExposure.value=exposure;
    camera.targetTexture=oldTarget;RenderTexture.active=oldActive;
    target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
    rig.SetOrbital(false);
    if(g.SaveToJson()!=before || !object.ReferenceEquals(simulation,g.Simulation) || Time.timeScale!=0)
        throw new System.InvalidOperationException("Art consumer probe changed canonical state/identity/time.");
}
