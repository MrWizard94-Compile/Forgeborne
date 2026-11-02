if(Application.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating
    || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/Main.unity")
    throw new System.InvalidOperationException("Stopped imported actual Main required.");
var type=System.AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("ForgeborneArtAssets")).FirstOrDefault(t=>t!=null);
if(type==null)throw new System.InvalidOperationException("Original art preparation unavailable.");
type.GetMethod("Prepare").Invoke(null,null);
var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
var layer=tags.FindProperty("layers").GetArrayElementAtIndex(VoxelMeshRenderer.OverheadLayer);
if(!string.IsNullOrEmpty(layer.stringValue) && layer.stringValue!=VoxelMeshRenderer.OverheadLayerName)
    throw new System.InvalidOperationException("The overhead visibility layer is already owned by another feature.");
layer.stringValue=VoxelMeshRenderer.OverheadLayerName;tags.ApplyModifiedPropertiesWithoutUndo();
var floor=Resources.Load<Material>("Art/Materials/CastFloor");var rock=Resources.Load<Material>("Art/Materials/Basalt");
var mineral=Resources.Load<Material>("Art/Materials/EmberMineral");
if(floor==null || rock==null || mineral==null || floor.GetTexture("_BaseMap")!=rock.GetTexture("_BaseMap")
    || floor.GetTexture("_SurfaceMap")!=rock.GetTexture("_SurfaceMap") || floor.GetFloat("_ReliefMetres")>=rock.GetFloat("_ReliefMetres")
    || floor.GetFloat("_RoughnessBias")!=-.16f || mineral.shader.name!="Forgeborne/IndustrialSurface")
    throw new System.InvalidOperationException("V2 floor/mineral material consumers were not prepared.");
var manager=UnityEngine.Object.FindFirstObjectByType<GameManager>();
if(manager==null || manager.Seed!=42 || !manager.UseAuthoredFoundry || manager.UseCourtyardEncounter)
    throw new System.InvalidOperationException("Saved Main must retain its one Seed-42 authored GameManager.");
PlayerSettings.bundleVersion="0.126.0";AssetDatabase.SaveAssets();
if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene()))
    throw new System.InvalidOperationException("Actual Main save failed.");
return new{version=PlayerSettings.bundleVersion,v1Sha256=Forgeborne.Core.AuthoredFoundry.ContentSha256,
    v2Sha256=Forgeborne.Core.AuthoredFoundry.GetContentSha256(Forgeborne.Core.SurfaceProfile.AuthoredFoundryV2),
    v2Parts=Forgeborne.Core.AuthoredFoundry.GetSourcePartCount(Forgeborne.Core.SurfaceProfile.AuthoredFoundryV2),
    retainedV1=true,defaultEntryMustBeVerifiedInPlay=true};
