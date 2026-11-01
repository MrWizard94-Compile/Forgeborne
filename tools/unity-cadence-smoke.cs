if (!Application.isPlaying) throw new System.InvalidOperationException("Enter Main Play mode first.");
var manager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
if (!manager.RestartEncounter()) throw new System.InvalidOperationException("Production restart failed.");
var flow = UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
if (flow != null) flow.Resume();
var simulation = manager.Simulation;
double started = Time.realtimeSinceStartupAsDouble;
int initialFrame = Time.frameCount;
int initialTick = simulation.World.Tick;
UnityEditor.SessionState.SetString("Forgeborne.CadenceProbe", "sampling");
EditorApplication.CallbackFunction sample = null;
sample = () => {
    if (!EditorApplication.isPlaying)
    {
        EditorApplication.update -= sample;
        UnityEditor.SessionState.SetString("Forgeborne.CadenceProbe", "interrupted before completion");
        return;
    }
    double elapsed = Time.realtimeSinceStartupAsDouble - started;
    if (elapsed < 3.05) return;
    int ticks = simulation.World.Tick - initialTick;
    int frames = Time.frameCount - initialFrame;
    UnityEditor.SessionState.SetString("Forgeborne.CadenceProbe", string.Format(
        System.Globalization.CultureInfo.InvariantCulture,
        "elapsedSeconds={0:F3}; worldTicks={1}; renderFrames={2}; health={3}; sameSimulation={4}; expectedTicks=6",
        elapsed, ticks, frames, simulation.World.Player.Health, object.ReferenceEquals(simulation, manager.Simulation)));
    EditorApplication.update -= sample;
};
EditorApplication.update += sample;
return "Sampling real Main Update callbacks for 3.05 seconds; read SessionState Forgeborne.CadenceProbe afterwards.";
