if(Application.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
    throw new System.InvalidOperationException("Stopped imported Editor required.");
var type=System.AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("ForgeborneArtAssets")).FirstOrDefault(t=>t!=null);
if(type==null)throw new System.InvalidOperationException("Original art preparer is missing.");
type.GetMethod("Prepare").Invoke(null,null);
var timber=Resources.Load<Material>("Art/Materials/Timber");
if(timber==null || timber.shader.name!="Forgeborne/IndustrialSurface" || timber.GetFloat("_Metallic")!=0)
    throw new System.InvalidOperationException("Original nonmetal timber material did not reach its runtime consumer.");
var ghostShader=Shader.Find("Universal Render Pipeline/Unlit");
if(ghostShader==null || ShaderUtil.ShaderHasError(ghostShader))throw new System.InvalidOperationException("Declared URP unlit shader is unavailable.");
var ghost=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Materials/ConstructionGhost.mat");
if(ghost==null){ghost=new Material(ghostShader);AssetDatabase.CreateAsset(ghost,"Assets/Resources/Materials/ConstructionGhost.mat");}
ghost.shader=ghostShader;ghost.SetFloat("_Surface",1);ghost.SetFloat("_Blend",0);
ghost.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);
ghost.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
ghost.SetFloat("_SrcBlendAlpha",(float)UnityEngine.Rendering.BlendMode.One);
ghost.SetFloat("_DstBlendAlpha",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
ghost.SetFloat("_ZWrite",0);ghost.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
ghost.SetOverrideTag("RenderType","Transparent");ghost.renderQueue=(int)UnityEngine.Rendering.RenderQueue.Transparent;
ghost.SetColor("_BaseColor",new Color(.05f,.9f,1,.30f));EditorUtility.SetDirty(ghost);
PlayerSettings.bundleVersion="0.124.0";AssetDatabase.SaveAssets();
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(scene.path!="Assets/Scenes/Main.unity")throw new System.InvalidOperationException("Open the owned Main scene before preparing the default encounter.");
var manager=UnityEngine.Object.FindFirstObjectByType<GameManager>();
if(manager==null)throw new System.InvalidOperationException("Main's one GameManager is missing.");
manager.UseAuthoredFoundry=true;manager.UseCourtyardEncounter=false;EditorUtility.SetDirty(manager);
var weapon=UnityEngine.Object.FindFirstObjectByType<WeaponViewModel>();
if(weapon!=null){weapon.ViewOffset=new Vector3(.20f,-.22f,.50f);EditorUtility.SetDirty(weapon);}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene))throw new System.InvalidOperationException("Main did not save.");
return new{version=PlayerSettings.bundleVersion,sourcePieces=Forgeborne.Core.AuthoredFoundry.SourcePartCount,
    geometrySha256=Forgeborne.Core.AuthoredFoundry.ContentSha256,originalMaterialsPrepared=true,mainUsesAuthoredFoundry=manager.UseAuthoredFoundry,
    legacyCourtyardDisabled=!manager.UseCourtyardEncounter};
