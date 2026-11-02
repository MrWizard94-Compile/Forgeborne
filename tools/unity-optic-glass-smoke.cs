if(Application.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
    throw new System.InvalidOperationException("Stopped imported Editor required for isolated optical GPU check.");
var source=Resources.Load<Material>("Art/Materials/OpticGlass");
if(source==null || source.shader.name!="Forgeborne/OpticGlass" || ShaderUtil.GetShaderMessages(source.shader).Length!=0)
    throw new System.InvalidOperationException("Production optical shader/material missing or has diagnostics.");
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();bool wasDirty=scene.isDirty;int originalRoots=scene.rootCount;
var root=new GameObject("Temporary optical GPU fixture");root.hideFlags=HideFlags.HideAndDontSave;root.transform.position=new Vector3(500,500,500);
var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-1,-1,0),new Vector3(-1,1,0),new Vector3(1,1,0),new Vector3(1,-1,0)};
mesh.triangles=new[]{0,1,2,0,2,3};mesh.normals=Enumerable.Repeat(Vector3.back,4).ToArray();mesh.RecalculateBounds();
var white=new Material(Shader.Find("Universal Render Pipeline/Unlit"));white.SetColor("_BaseColor",Color.white);
var glass=new Material(source);var original=glass.GetColor("_TintColor");
var background=new GameObject("White optical reference");background.transform.SetParent(root.transform,false);background.transform.localPosition=new Vector3(0,0,3);background.layer=30;
background.AddComponent<MeshFilter>().sharedMesh=mesh;background.AddComponent<MeshRenderer>().sharedMaterial=white;
var pane=new GameObject("Production coating pane");pane.transform.SetParent(root.transform,false);pane.transform.localPosition=new Vector3(0,0,2);pane.layer=30;
pane.AddComponent<MeshFilter>().sharedMesh=mesh;pane.AddComponent<MeshRenderer>().sharedMaterial=glass;
var cameraObject=new GameObject("Optical measurement camera");cameraObject.transform.SetParent(root.transform,false);
var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.cullingMask=1<<30;camera.orthographic=true;camera.orthographicSize=1;
camera.nearClipPlane=.1f;camera.farClipPlane=5;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.allowHDR=true;camera.allowMSAA=false;
var data=cameraObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();data.renderPostProcessing=false;
var target=new RenderTexture(64,64,24,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);
var pixels=new Texture2D(64,64,TextureFormat.RGBAFloat,false,true);var active=RenderTexture.active;
System.Func<float> measure=()=>{
    camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,64,64),0,0);pixels.Apply();float total=0;
    for(int y=24;y<40;y++)for(int x=24;x<40;x++){var c=pixels.GetPixel(x,y);total+=(c.r+c.g+c.b)/3;}return total/256;
};
try
{
    pane.SetActive(false);float clear=measure();pane.SetActive(true);float coating=measure();
    var dense=original;dense.a=.85f;glass.SetColor("_TintColor",dense);float absorbing=measure();
    if(clear<.95f || coating<clear*.85f || coating>clear*.995f || absorbing>coating*.6f)
        throw new System.InvalidOperationException("Production optical transmission/absorption did not reach the GPU: "+clear+", "+coating+", "+absorbing);
    return new{actualGpuPixelsPerState=256,clearMeanLinear=clear,productionCoatingMeanLinear=coating,highAbsorptionMeanLinear=absorbing,
        productionShaderTransmission=true,actualMaterialResponse=true,originalMaterialUnchanged=true,
        scope="Isolated static optical-material GPU A/B with the production shader, no post-processing. Not whole-game performance, native driver stability or art/fun acceptance."};
}
finally
{
    RenderTexture.active=active;camera.targetTexture=null;target.Release();
    UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
    UnityEngine.Object.DestroyImmediate(mesh);UnityEngine.Object.DestroyImmediate(white);UnityEngine.Object.DestroyImmediate(glass);
    if(scene.isDirty!=wasDirty || scene.rootCount!=originalRoots)throw new System.InvalidOperationException("Optical fixture changed the saved Main scene.");
}
