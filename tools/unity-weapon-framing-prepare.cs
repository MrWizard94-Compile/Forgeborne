if(Application.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating
    || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/Main.unity")
    throw new System.InvalidOperationException("Stopped imported actual Main required.");
var type=System.AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("ForgeborneArtAssets")).FirstOrDefault(t=>t!=null);
if(type==null)throw new System.InvalidOperationException("Original art authoring type missing.");
try{type.GetMethod("Prepare").Invoke(null,null);}
catch(System.Reflection.TargetInvocationException error){throw new System.InvalidOperationException(error.InnerException!=null ? error.InnerException.ToString() : error.ToString());}
var view=UnityEngine.Object.FindFirstObjectByType<WeaponViewModel>();
if(view==null)throw new System.InvalidOperationException("Actual Main weapon view missing.");
view.ViewOffset=new Vector3(.19f,-.10f,.55f);view.ViewAngles=new Vector3(0,-12,0);
EditorUtility.SetDirty(view);PlayerSettings.bundleVersion="0.128.0";AssetDatabase.SaveAssets();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene()))
    throw new System.InvalidOperationException("Main save failed.");
var prefab=Resources.Load<GameObject>("Art/Prefabs/PulseCarbine");
return new{version=PlayerSettings.bundleVersion,offset=view.ViewOffset.ToString(),angles=view.ViewAngles.ToString(),
    triangles=prefab.GetComponentsInChildren<MeshFilter>(true).Sum(f=>f.sharedMesh.triangles.Length/3)
        +prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Sum(s=>s.sharedMesh.triangles.Length/3),
    scope="Actual source/import and saved presentation; independent rendered/native verification still required."};
