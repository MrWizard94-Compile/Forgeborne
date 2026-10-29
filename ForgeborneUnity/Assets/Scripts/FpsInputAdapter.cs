// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// Feeds Unity Input System input into Forgeborne.Core.FpsController and
// Simulation. No movement logic lives here; FpsController is canonical.
using UnityEngine;
using Forgeborne.Core;

public class FpsInputAdapter : MonoBehaviour
{
    public Simulation? Simulation;
    public float MouseSensitivity = 2f;
    private float _yaw;

    void Update()
    {
        if (Simulation == null) return;
        // WASD / arrows, world-aligned in Slice 1 (no yaw rotation of movement yet;
        // yaw is stored for the camera, movement rotation is a later Slice 1 step).
        float forward = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
        float strafe = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        _yaw += Input.GetAxis("Mouse X") * MouseSensitivity;
        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        Simulation.Tick(Time.deltaTime, forward, strafe, jump);
        // Camera follows the FPS controller's feet position + eye height.
        transform.position = new Vector3(Simulation.Fps.X, Simulation.Fps.Y + 1.6f, Simulation.Fps.Z);

        if (Input.GetKeyDown(KeyCode.E)) Simulation.GatherAhead();
        if (Input.GetKeyDown(KeyCode.Q)) Simulation.BuildWallAhead();
        if (Input.GetKeyDown(KeyCode.R)) Simulation.Weapon.Reload();
        // Orbital doctrine menu / rally picker (0.15.0, replaces the
        // provisional O key, which deployed instantly at the player's
        // feet). O opens the picker, or cancels it if already
        // picking. While picking, a centre-screen raycast from the
        // camera updates the pending rally to the voxel cell hit
        // (the voxel mesh carries a MeshCollider for this — see
        // VoxelMeshRenderer); Return deploys the selected squad at
        // that rally, Escape cancels, and 1 re-selects the only
        // current doctrine/kind pair (HoldGround / Vanguard). While
        // picking, the mouse does not fire: Enter is the confirm,
        // so a click cannot shoot while the player is aiming a rally.
        // No deployment cost or squad cap yet (decided, deferred to
        // a Slice 3 economy step — see docs/orbital-command.md).
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (Simulation.OrbitalCommand.IsPicking) Simulation.CancelOrbitalPick();
            else Simulation.BeginOrbitalPick();
        }
        if (Simulation.OrbitalCommand.IsPicking)
        {
            if (Input.GetKeyDown(KeyCode.Return)) Simulation.ConfirmOrbitalDeploy();
            else if (Input.GetKeyDown(KeyCode.Escape)) Simulation.CancelOrbitalPick();
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Simulation.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround);
                Simulation.OrbitalCommand.SelectKind(SquadKind.Vanguard);
            }
            else
            {
                Ray ray = new Ray(transform.position, transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, 30f))
                {
                    Simulation.SetOrbitalRally(
                        Mathf.FloorToInt(hit.point.x),
                        Mathf.FloorToInt(hit.point.y),
                        Mathf.FloorToInt(hit.point.z));
                }
            }
        }
        else if (Input.GetMouseButtonDown(0)) Simulation.FireAtNearestEnemy();
    }
}
