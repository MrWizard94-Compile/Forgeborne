if(Application.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
    throw new System.InvalidOperationException("Stopped imported Editor required.");
var shader=Shader.Find("Universal Render Pipeline/Unlit");
if(shader==null)throw new System.InvalidOperationException("Declared URP unlit shader missing.");
var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);quad.name="Temporary material calibration";quad.hideFlags=HideFlags.HideAndDontSave;
quad.transform.position=new Vector3(1000,1000,1000);quad.layer=29;
UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());
var root=new GameObject("Temporary linear calibration camera");root.hideFlags=HideFlags.HideAndDontSave;
var camera=root.AddComponent<Camera>();camera.transform.position=quad.transform.position+Vector3.back*2;
camera.orthographic=true;camera.orthographicSize=.6f;camera.nearClipPlane=.1f;camera.farClipPlane=5;
camera.cullingMask=1<<29;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.allowHDR=false;
var material=new Material(shader);quad.GetComponent<MeshRenderer>().sharedMaterial=material;
var target=new RenderTexture(16,16,0,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);
var pixels=new Texture2D(16,16,TextureFormat.RGBAFloat,false,true);var active=RenderTexture.active;
try
{
    camera.targetTexture=target;material.SetColor("_BaseColor",new Color(.5f,.5f,.5f,1));
    camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,16,16),0,0);pixels.Apply();var color=pixels.GetPixel(8,8);
    material.SetVector("_BaseColor",new Vector4(.5f,.5f,.5f,1));
    camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,16,16),0,0);pixels.Apply();var vector=pixels.GetPixel(8,8);
    var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",new Color(.5f,.5f,.5f,1));quad.GetComponent<MeshRenderer>().SetPropertyBlock(block);
    camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,16,16),0,0);pixels.Apply();var property=pixels.GetPixel(8,8);
    float expected=Mathf.GammaToLinearSpace(.5f);
    if(QualitySettings.activeColorSpace!=ColorSpace.Linear || Mathf.Abs(color.r-expected)>.0005f
        || Mathf.Abs(vector.r-expected)>.0005f || Mathf.Abs(property.r-expected)>.0005f)
        throw new System.InvalidOperationException("Actual Color-property transfer no longer matches the measured calibration contract.");
    return new{colorSpace=QualitySettings.activeColorSpace.ToString(),renderTarget="Linear ARGBFloat; no post-processing",setColor=color.r,setVector=vector.r,propertyBlockColor=property.r,expectedLinear=expected};
}
finally
{
    camera.targetTexture=null;RenderTexture.active=active;target.Release();
    UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);UnityEngine.Object.DestroyImmediate(material);
    UnityEngine.Object.DestroyImmediate(quad);UnityEngine.Object.DestroyImmediate(root);
}
