// Destructive only to a paused Play instance; stop Play afterward to restore saved Main.
// Removing the presentation owner must restore the scene lighting and keep game state.
var g=UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow=UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var atmosphere=UnityEngine.Object.FindFirstObjectByType<FoundryAtmosphere>();
if(!Application.isPlaying || g==null || g.Simulation==null || flow==null || !flow.IsMenuOpen
    || Time.timeScale!=0 || atmosphere==null || atmosphere.Lighting==null)
    throw new System.InvalidOperationException("Started paused Main required; stop Play after this probe.");
var simulation=g.Simulation;
string before=g.SaveToJson();
var lighting=atmosphere.Lighting;
if(RenderSettings.skybox==lighting.SkyboxMaterial)
    throw new System.InvalidOperationException("Runtime sky was never installed.");
UnityEngine.Object.DestroyImmediate(atmosphere);
if(RenderSettings.skybox!=lighting.SkyboxMaterial
    || RenderSettings.ambientMode!=UnityEngine.Rendering.AmbientMode.Flat
    || RenderSettings.ambientLight!=lighting.AmbientColor)
    throw new System.InvalidOperationException("Removing atmosphere did not restore the existing scene environment.");
if(g.SaveToJson()!=before || !object.ReferenceEquals(simulation,g.Simulation)
    || UnityEngine.Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length!=1)
    throw new System.InvalidOperationException("Presentation removal changed canonical state/ownership.");
return new{removedRuntimeOwner=true,existingSkyRestored=true,existingAmbientRestored=true,
    exactStatePreserved=true,sameSimulation=true,stopPlayRequired=true};
