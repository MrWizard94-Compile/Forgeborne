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

    public string SaveToJson()
    {
        if (Simulation == null) return "";
        return SaveSystem.SaveZone(Simulation.World);
    }

    // In-place restore (0.14.0): Simulation.LoadFromJson swaps the
    // world inside the same Simulation object and teleports its FPS
    // controller to the saved player position, so this GameManager
    // and the MeshRenderer keep working with no re-wiring. On
    // success the mesh is refreshed from the restored chunk; on
    // failure the current world is left untouched and a warning is
    // logged instead of a half-loaded state. The equipped weapon is
    // kept (the save format carries no weapon) and an applied skill
    // build is re-applied to the restored world by core.
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
