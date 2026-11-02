if(Application.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating
    || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/Main.unity")
    throw new System.InvalidOperationException("Stopped imported actual Main required.");
var type=System.AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("ForgeborneArtAssets")).FirstOrDefault(t=>t!=null);
if(type==null)throw new System.InvalidOperationException("Original art preparation unavailable.");
try{type.GetMethod("Prepare").Invoke(null,null);}
catch(System.Reflection.TargetInvocationException error){throw new System.InvalidOperationException(error.InnerException!=null ? error.InnerException.ToString() : error.ToString());}
AssetDatabase.SaveAssets();
var prefab=Resources.Load<GameObject>("Art/Prefabs/PulseCarbine");
var skins=prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
var rows=new System.Collections.Generic.List<object>();
if(skins.Length!=2)throw new System.InvalidOperationException("Both original skins required.");
foreach(var skin in skins)
{
    string side=skin.name.StartsWith("Left") ? "Left" : "Right";
    int index=System.Array.FindIndex(skin.bones,b=>b.name==side+"Forearm");
    int rubber=System.Array.FindIndex(skin.sharedMaterials,m=>m.name=="Rubber");
    if(index<0 || rubber<0)throw new System.InvalidOperationException("Actual cuff role or forearm missing.");
    var vertices=skin.sharedMesh.GetTriangles(rubber).Distinct().ToArray();var weights=skin.sharedMesh.boneWeights;
    if(vertices.Length<640 || skin.quality!=SkinQuality.Bone4 || !skin.updateWhenOffscreen)
        throw new System.InvalidOperationException("Imported cuff/skin consumers invalid.");
    foreach(int vertex in vertices)
    {
        var weight=weights[vertex];
        if(weight.boneIndex0!=index || Mathf.Abs(weight.weight0-1)>.00001f || weight.weight1+weight.weight2+weight.weight3>.00001f)
            throw new System.InvalidOperationException("A real imported cuff vertex is not bound to its sleeve forearm.");
    }
    int maximumInfluences=0;float maximumWeightSumError=0;var active=new System.Collections.Generic.HashSet<int>();
    foreach(var weight in weights)
    {
        float sum=weight.weight0+weight.weight1+weight.weight2+weight.weight3;
        maximumWeightSumError=Mathf.Max(maximumWeightSumError,Mathf.Abs(sum-1));
        var values=new[]{weight.weight0,weight.weight1,weight.weight2,weight.weight3};
        var indices=new[]{weight.boneIndex0,weight.boneIndex1,weight.boneIndex2,weight.boneIndex3};
        int influences=0;
        for(int i=0;i<4;i++)if(values[i]>0)
        {
            if(indices[i]<0 || indices[i]>=skin.bones.Length || !skin.bones[indices[i]].name.StartsWith(side))
                throw new System.InvalidOperationException("Imported glove binds to another hand or an invalid joint.");
            influences++;active.Add(indices[i]);
        }
        maximumInfluences=Mathf.Max(maximumInfluences,influences);
        if(influences==0 || sum<.99999f || sum>1.00001f)throw new System.InvalidOperationException("Imported glove weights are not normalized.");
    }
    if(active.Count!=17)throw new System.InvalidOperationException("All seventeen original joints must reach their own glove.");
    rows.Add(new{skin=skin.name,cuffVertices=vertices.Length,cuffBoundTo=side+"Forearm",fourWeightSkin=true,
        actualImportedVertices=skin.sharedMesh.vertexCount,activeJoints=active.Count,maximumInfluences=maximumInfluences,
        maximumWeightSumError=maximumWeightSumError});
}
return new{version=PlayerSettings.bundleVersion,cuffs=rows,
    importedTriangles=prefab.GetComponentsInChildren<MeshFilter>(true).Sum(f=>f.sharedMesh.triangles.Length/3)+skins.Sum(s=>s.sharedMesh.triangles.Length/3),
    scope="Actual imported Rubber-role vertices and binding consumer; rendered bend and native review remain separate."};
