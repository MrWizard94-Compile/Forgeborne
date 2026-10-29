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

    void Start()
    {
        Simulation = new Simulation(Seed);
        if (MeshRenderer != null)
        {
            MeshRenderer.Simulation = Simulation;
            MeshRenderer.RefreshMesh();
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
