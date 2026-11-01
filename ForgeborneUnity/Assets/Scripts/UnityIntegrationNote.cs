// Unity 6000.3.27f2 integration: open Assets/Scenes/Main.unity.
// GameManager creates the only Simulation and shares it with its linked
// rendering/input/HUD/squad adapters. Core ships as DLLs in Assets/Plugins;
// never copy src/Forgeborne.Core into Assets. Active Input Handling is Both:
// the FPS adapter uses legacy Input.GetKey/GetAxis/GetMouseButton.
// Windows integration evidence and remaining gameplay gates are recorded in
// docs/unity-main-integration.md. GPU meshing, studio presentation, and
// director gameplay acceptance are not established by the Main scene.
namespace Forgeborne.Unity
{
    public static class UnityIntegrationNote
    {
        public const string Engine = "Unity 6000 + Entities / DOTS, URP, C#";
        public const string CanonicalState = "Forgeborne.Core.WorldZone";
    }
}
