if(Application.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
    throw new System.InvalidOperationException("Stopped imported Editor required.");
var type=System.AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("ForgeborneArtAssets")).FirstOrDefault(t=>t!=null);
if(type==null)throw new System.InvalidOperationException("Original art preparer unavailable.");
type.GetMethod("Prepare").Invoke(null,null);
var roles=new[]{"Graphite","Steel","Copper","Ceramic","Rubber","Basalt","Paint","Timber"};
foreach(string role in roles)
{
    var material=Resources.Load<Material>("Art/Materials/"+role);
    if(material==null || material.GetTexture("_BaseMap")==null || material.GetTexture("_SurfaceMap")==null
        || material.GetFloat("_TextureStrength")!=1 || material.GetColor("_BaseColor")!=Color.white)
        throw new System.InvalidOperationException("Original surface maps did not reach the material consumer: "+role);
}
PlayerSettings.bundleVersion="0.125.0";AssetDatabase.SaveAssets();
return new{version=PlayerSettings.bundleVersion,roles=roles,pairedMaps=roles.Length*2,
    identityUnchanged=Forgeborne.Core.AuthoredFoundry.ContentSha256=="3aaa044009b8db239b903b52bd3d928a59efd678e813a1b2eb36b25333863272"};
