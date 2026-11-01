// Run through Unity MCP in started Main with the player menu open.
// Scripted component verification, not a keyboard/render/native acceptance.
if (!Application.isPlaying) throw new System.InvalidOperationException("Started Play mode required.");
var g = UnityEngine.Object.FindFirstObjectByType<GameManager>();
var flow = UnityEngine.Object.FindFirstObjectByType<PlayerFlow>();
var rig = UnityEngine.Object.FindFirstObjectByType<OrbitalCameraRig>();
if (g == null || g.Simulation == null || flow == null || !flow.IsMenuOpen || Time.timeScale != 0 || rig == null)
    throw new System.InvalidOperationException("A started, paused Main and its one rig are required.");
var simulation = g.Simulation;
var fps = simulation.Fps;
var look = simulation.Look;
string before = g.SaveToJson();
bool previousMode = rig.IsOrbital;
try
{
    rig.SetOrbital(true);
    var camera = rig.GetComponent<Camera>();
    if (!rig.IsOrbital || !g.FpsAdapter.IsOrbitalView || !g.FpsAdapter.enabled || !camera.enabled
        || camera.orthographic || rig.FirstPersonCamera.enabled || Camera.main != camera)
        throw new System.InvalidOperationException("Perspective camera/input handoff failed.");
    if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
        throw new System.InvalidOperationException("Command cursor is not free.");
    if (rig.FirstPersonReticle.activeSelf || rig.FirstPersonControls.activeSelf
        || rig.ViewModel.transform.Find("ForgeborneWeaponViewModel").gameObject.activeSelf)
        throw new System.InvalidOperationException("FPS-only views leaked into command mode.");
    var suit = rig.transform.Find("Suit position");
    if (suit == null || !suit.gameObject.activeSelf || suit.position != new Vector3(fps.X, fps.Y, fps.Z)
        || suit.GetComponentsInChildren<Collider>().Length != 0)
        throw new System.InvalidOperationException("Suit view disagrees with canonical pose/collision ownership.");
    if (Mathf.Abs(camera.transform.eulerAngles.x - 55) > 0.01f || camera.transform.position.y < 20)
        throw new System.InvalidOperationException("Command camera did not reach its 3D overhead pose.");
    rig.SetOrbital(false);
    if (g.FpsAdapter.IsOrbitalView || camera.enabled || !rig.FirstPersonCamera.enabled
        || Camera.main != rig.FirstPersonCamera || !rig.FirstPersonReticle.activeSelf
        || !rig.FirstPersonControls.activeSelf || suit.gameObject.activeSelf)
        throw new System.InvalidOperationException("Return to FPS view failed.");
    if (g.SaveToJson() != before || !object.ReferenceEquals(simulation, g.Simulation)
        || !object.ReferenceEquals(fps, simulation.Fps) || !object.ReferenceEquals(look, simulation.Look)
        || Time.timeScale != 0 || !g.FpsAdapter.enabled)
        throw new System.InvalidOperationException("View switching changed canonical state/identity/time ownership.");
    return new { perspectiveHandoff = true, cursorFree = true, suitUsesCanonicalPose = true,
        noExtraCollisionOwner = true, fpsRestored = true, exactStatePreserved = true,
        simulationIdentityPreserved = true, soleFrameDriverEnabled = true };
}
finally { rig.SetOrbital(previousMode); }
