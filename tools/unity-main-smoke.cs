// Method body for Unity MCP execute_code, run only with the saved Main scene in Play mode.
// Exercises the real scene and its production save/load entry; never creates a Simulation.
if (!Application.isPlaying) throw new System.InvalidOperationException("Enter Play mode first.");
var managers = UnityEngine.Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None);
if (managers.Length != 1) throw new System.InvalidOperationException("Expected exactly one GameManager.");
var g = managers[0];
var s = g.Simulation;
if (s == null || g.Seed != 42) throw new System.InvalidOperationException("Seed 42 simulation has not started.");
if (!object.ReferenceEquals(s, g.FpsAdapter.Simulation)
    || !object.ReferenceEquals(s, g.MeshRenderer.Simulation)
    || !object.ReferenceEquals(s, g.HudDisplay.Simulation)
    || !object.ReferenceEquals(s, g.OrbitalDisplay.Simulation)
    || !object.ReferenceEquals(s, g.MarkerDisplay.Simulation))
    throw new System.InvalidOperationException("Scene adapters do not share GameManager's Simulation.");
if (!object.ReferenceEquals(g.MeshRenderer, g.FpsAdapter.MeshRenderer))
    throw new System.InvalidOperationException("Mesh refresh null-fill failed.");
var mesh = g.MeshRenderer.GetComponent<MeshFilter>().sharedMesh;
if (mesh == null || mesh.vertexCount == 0 || !object.ReferenceEquals(mesh, g.MeshRenderer.GetComponent<MeshCollider>().sharedMesh))
    throw new System.InvalidOperationException("Chunk geometry and collider are missing or disagree.");
if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline == null)
    throw new System.InvalidOperationException("URP is not active.");
var shader = g.MeshRenderer.GetComponent<MeshRenderer>().sharedMaterial.shader;
if (shader.name != "Universal Render Pipeline/Lit" || !shader.isSupported)
    throw new System.InvalidOperationException("Chunk shader is unsupported.");
if (string.IsNullOrEmpty(g.HudDisplay.HealthText.text) || string.IsNullOrEmpty(g.HudDisplay.AmmoText.text)
    || string.IsNullOrEmpty(g.HudDisplay.ObjectiveText.text) || string.IsNullOrEmpty(g.HudDisplay.ShieldsText.text))
    throw new System.InvalidOperationException("The four HUD bindings did not update.");
var json = g.SaveToJson();
var oldTick = s.World.Tick;
g.LoadFromJson(json);
if (!object.ReferenceEquals(s, g.Simulation) || s.World.Tick != oldTick || g.SaveToJson() != json)
    throw new System.InvalidOperationException("Production save/load round-trip changed canonical state or Simulation identity.");
return new { sharedSimulation = true, meshAndCollider = true, shader = shader.name,
    hudUpdated = true, inPlaceSaveRoundTrip = true, saveLength = json.Length,
    vertices = mesh.vertexCount, triangles = mesh.triangles.Length / 3, tick = s.World.Tick,
    health = s.World.Player.Health, screenWidth = Screen.width, screenHeight = Screen.height };
