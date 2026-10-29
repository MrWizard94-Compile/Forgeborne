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

    public void LoadFromJson(string json)
    {
        if (Simulation == null) return;
        // Slice 1 limitation, documented: LoadZone creates a new WorldZone.
        // A full in-place restore (replacing Simulation.World) is a later
        // step; for now, loading is verified headlessly in Core tests,
        // and in Unity it is exposed here for future wiring, not claimed
        // as a finished UI flow.
        var loaded = SaveSystem.LoadZone(json);
        Debug.Log($"Loaded Forgeborne save: seed {loaded.Seed}, tick {loaded.Tick}, enemies {loaded.Enemies.Count}");
    }
}
