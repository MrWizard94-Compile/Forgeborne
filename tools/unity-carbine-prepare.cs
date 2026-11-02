if(Application.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating
    || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/Main.unity")
    throw new System.InvalidOperationException("Stopped imported actual Main required.");
var type=System.AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("ForgeborneArtAssets")).FirstOrDefault(t=>t!=null);
if(type==null)throw new System.InvalidOperationException("Original art preparation unavailable.");
try{type.GetMethod("Prepare").Invoke(null,null);}
catch(System.Reflection.TargetInvocationException error)
{throw new System.InvalidOperationException(error.InnerException!=null ? error.InnerException.ToString() : error.ToString());}
var prefab=Resources.Load<GameObject>("Art/Prefabs/PulseCarbine");
var skins=prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
var material=Resources.Load<Material>("Art/Materials/GloveFabric");
if(skins.Length!=2 || skins.Any(s=>s.bones.Length>34 || s.quality!=SkinQuality.Bone4 || !s.updateWhenOffscreen)
    || material==null || material.GetTexture("_BaseMap")==null || material.GetTexture("_SurfaceMap")==null)
    throw new System.InvalidOperationException("Original glove rig/material consumers missing.");
PlayerSettings.bundleVersion="0.127.0";AssetDatabase.SaveAssets();
var view=UnityEngine.Object.FindFirstObjectByType<WeaponViewModel>();
if(view==null)throw new System.InvalidOperationException("Actual Main weapon view missing.");
view.ViewOffset=new Vector3(.20f,-.17f,.70f);EditorUtility.SetDirty(view);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene()))
    throw new System.InvalidOperationException("Main weapon framing save failed.");
return new{version=PlayerSettings.bundleVersion,skins=skins.Length,bonePalettes=skins.Select(s=>s.bones.Length).ToArray(),activeBonesPerHand=17,
    material=material.name,skinnedVertices=skins.Select(s=>s.sharedMesh.vertexCount).ToArray(),
    importedTriangles=prefab.GetComponentsInChildren<MeshFilter>(true).Sum(f=>f.sharedMesh.triangles.Length/3)+skins.Sum(s=>s.sharedMesh.triangles.Length/3),
    coreSha256=Forgeborne.Core.AuthoredFoundry.GetContentSha256(Forgeborne.Core.SurfaceProfile.AuthoredFoundryV2),
    scope="Import and declared rig consumers; actual deformation, camera, runtime and quality still require inspection."};
