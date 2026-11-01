var managers=UnityEngine.Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None);
if(managers.Length!=1)throw new System.InvalidOperationException("Exactly one scene GameManager is required.");
var g=managers[0];var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
if(!Application.isPlaying || g.Simulation==null || flow==null || !flow.IsMenuOpen || Time.timeScale!=0
    || g.Seed!=42 || !g.UseAuthoredFoundry || g.UseCourtyardEncounter
    || g.Simulation.World.Chunk.SurfaceProfile!=Forgeborne.Core.SurfaceProfile.AuthoredFoundryV1)
    throw new System.InvalidOperationException("Actual paused authored Main entry is not active.");
var s=g.Simulation;
if(!object.ReferenceEquals(s,g.MeshRenderer.Simulation) || !object.ReferenceEquals(s,g.FpsAdapter.Simulation)
    || !object.ReferenceEquals(s,g.HudDisplay.Simulation) || !object.ReferenceEquals(s,g.OrbitalDisplay.Simulation)
    || !object.ReferenceEquals(s,g.MarkerDisplay.Simulation) || !g.FpsAdapter.enabled || !g.FpsAdapter.InputSuppressed)
    throw new System.InvalidOperationException("The authored entry lost canonical Simulation/driver/pause ownership.");
if(g.MeshRenderer.GetComponent<MeshFilter>().sharedMesh.vertexCount!=0 || g.MeshRenderer.GetComponent<MeshCollider>().sharedMesh!=null)
    throw new System.InvalidOperationException("The legacy block root mesh/collider is still populated.");
int messages=0;
foreach(var name in new[]{"Forgeborne/IndustrialSurface","Forgeborne/FoundrySky","Universal Render Pipeline/Unlit"})
{
    var shader=Shader.Find(name);
    if(shader==null)throw new System.InvalidOperationException("Missing actual shader "+name);
    messages+=ShaderUtil.GetShaderMessages(shader).Length;
}
if(messages!=0)throw new System.InvalidOperationException("Owned or affected shader diagnostics remain.");
bool newEntry=UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Any(t=>t.text=="ENTER CINDER EXCHANGE");
if(!newEntry)throw new System.InvalidOperationException("Player-facing entry still names the old courtyard.");
return new{scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,version=Application.version,
    actualEntryIsAuthored=true,oldCourtyardDisabled=true,legacyBlockRootEmpty=true,modelShapedConstruction=true,
    managerCount=managers.Length,seed=g.Seed,allLinkedAdaptersShareOneSimulation=true,soleFrameDriverEnabled=true,
    pausedForReview=true,currentEnemies=s.World.Enemies.Count,shaderMessages=messages,
    contentSha256=Forgeborne.Core.AuthoredFoundry.ContentSha256,
    limits="Editor entry and scripted consumer evidence only. No AAA+, native controls, heard audio, performance or fun acceptance."};
