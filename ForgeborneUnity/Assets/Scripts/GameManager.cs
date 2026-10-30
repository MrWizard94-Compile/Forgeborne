// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// GameManager owns one Simulation for the whole scene. It creates the
// world, spawns the first wave, and exposes save / load to UI buttons.
// Save format is Forgeborne.Core.SaveSystem JSON, not a Unity format,
// so saves remain readable headlessly.
using UnityEngine;
using Forgeborne.Core;

public class GameManager : MonoBehaviour
{
    public int Seed = 42;
    public Simulation? Simulation;
    public VoxelMeshRenderer? MeshRenderer;
    // Slice 16 Step 1: the FPS adapter whose refresh
    // wiring this step null-fills, decided 0.68.0
    // (rationale in DECISIONS.md). Inspector-wired and
    // optional, like every other adapter field: unwired,
    // nothing changes, and no adapter is conjured by
    // search. Wired, Start fills a null
    // FpsInputAdapter.MeshRenderer with the same
    // VoxelMeshRenderer this GameManager already links
    // — the exact failure playtest-gate item 27 names
    // (an unwired field makes every refresh a no-op)
    // cannot then survive a scene in which the chunk
    // renderer is wired here but forgotten there. A
    // non-null adapter field is never overwritten, so a
    // deliberate second renderer survives, and Start
    // runs before any adapter Update, so the fill lands
    // before the first tick's refresh check. This does
    // not wire the adapter's Simulation field, which
    // stays Inspector-wired as before — one wiring
    // change, not a scene-rewiring pass. If the adapter
    // is still unwired after the fill, a warning logs
    // naming the stale-mesh consequence, rather than a
    // silent first mesh that never refreshes.
    public FpsInputAdapter? FpsAdapter;

    void Start()
    {
        Simulation = new Simulation(Seed);
        if (MeshRenderer != null)
        {
            MeshRenderer.Simulation = Simulation;
            MeshRenderer.RefreshMesh();
        }
        if (FpsAdapter != null && FpsAdapter.MeshRenderer == null && MeshRenderer != null)
        {
            FpsAdapter.MeshRenderer = MeshRenderer;
        }
        if (FpsAdapter != null && FpsAdapter.MeshRenderer == null)
        {
            Debug.LogWarning("Forgeborne mesh wiring: FpsInputAdapter.MeshRenderer is still null after GameManager.Start — refresh calls will no-op and the chunk mesh will stay stale after builds, gathers, and destructions. Wire FpsAdapter to the same VoxelMeshRenderer GameManager links, or wire the adapter field directly — see playtest-gate items 27 and 31.");
        }
        Simulation.World.SpawnWave(3);
    }

    // Full save (0.17.0): Simulation.SaveToJson carries the world,
    // the applied skill build, and the equipped weapon, so a Unity
    // save no longer drops them the way the old world-only save did.
    public string SaveToJson()
    {
        if (Simulation == null) return "";
        return Simulation.SaveToJson();
    }

    // In-place restore (0.14.0, extended 0.17.0): Simulation.LoadFromJson swaps the
    // world inside the same Simulation object and teleports its FPS
    // controller to the saved player position, so this GameManager
    // and the MeshRenderer keep working with no re-wiring. On
    // success the mesh is refreshed from the restored chunk; on
    // failure the current world is left untouched and a warning is
    // logged instead of a half-loaded state. A skill build or weapon
    // carried by the save replaces the current ones; an old save
    // carrying neither keeps them, as before.
    public void LoadFromJson(string json)
    {
        if (Simulation == null) return;
        if (Simulation.LoadFromJson(json))
        {
            if (MeshRenderer != null) MeshRenderer.RefreshMesh();
            Debug.Log($"Loaded Forgeborne save: seed {Simulation.World.Seed}, tick {Simulation.World.Tick}, enemies {Simulation.World.Enemies.Count}");
        }
        else
        {
            Debug.LogWarning("Forgeborne load failed: save was null, empty, or malformed; current world unchanged.");
        }
    }
}
