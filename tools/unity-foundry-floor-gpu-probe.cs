if(Application.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
    throw new System.InvalidOperationException("Stopped imported Editor required.");
var source=Resources.Load<Material>("Art/Materials/CastFloor");
if(source==null || source.shader.name!="Forgeborne/IndustrialSurface")throw new System.InvalidOperationException("Prepared production floor required.");
var material=new Material(source);var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);
quad.hideFlags=HideFlags.HideAndDontSave;quad.transform.position=new Vector3(1000,1000,1000);quad.layer=29;
UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());quad.GetComponent<MeshRenderer>().sharedMaterial=material;
var root=new GameObject("Temporary production-floor GPU probe");root.hideFlags=HideFlags.HideAndDontSave;
var camera=root.AddComponent<Camera>();camera.transform.position=quad.transform.position+Vector3.back*2;
camera.orthographic=true;camera.orthographicSize=.6f;camera.nearClipPlane=.1f;camera.farClipPlane=5;
camera.cullingMask=1<<29;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.allowHDR=false;
var lampRoot=new GameObject("Temporary floor probe lamp");lampRoot.hideFlags=HideFlags.HideAndDontSave;
var lamp=lampRoot.AddComponent<Light>();lamp.type=LightType.Point;lamp.intensity=15;lamp.range=5;lamp.cullingMask=1<<29;lamp.shadows=LightShadows.None;
lamp.transform.position=quad.transform.position+new Vector3(.3f,.3f,-1);
var target=new RenderTexture(64,64,0,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);
var pixels=new Texture2D(64,64,TextureFormat.RGBAFloat,false,true);var active=RenderTexture.active;
try
{
    camera.targetTexture=target;material.SetFloat("_RoughnessBias",0);
    camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,64,64),0,0);pixels.Apply();var baseline=pixels.GetPixels();
    material.SetFloat("_RoughnessBias",-.16f);
    camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,64,64),0,0);pixels.Apply();var changed=pixels.GetPixels();
    double total=0;float maximum=0;
    for(int i=0;i<baseline.Length;i++)for(int channel=0;channel<3;channel++)
    {float difference=Mathf.Abs(baseline[i][channel]-changed[i][channel]);total+=difference;maximum=Mathf.Max(maximum,difference);}
    double mean=total/(baseline.Length*3);
    if(double.IsNaN(mean) || mean<.00001 || maximum<.0001)
        throw new System.InvalidOperationException("The actual production shader did not visibly consume the floor roughness adjustment.");
    return new{actualProductionShader=source.shader.name,linearTargetNoPostProcessing=true,pixels=baseline.Length,
        meanLinearChannelDifference=mean,maximumLinearChannelDifference=maximum,productionRoughnessBias=source.GetFloat("_RoughnessBias"),
        productionReliefMetres=source.GetFloat("_ReliefMetres"),scope="Isolated actual GPU material response; not representative gameplay, performance or AAA+ evidence."};
}
finally
{
    camera.targetTexture=null;RenderTexture.active=active;target.Release();
    UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);UnityEngine.Object.DestroyImmediate(material);
    UnityEngine.Object.DestroyImmediate(quad);UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(lampRoot);
}
