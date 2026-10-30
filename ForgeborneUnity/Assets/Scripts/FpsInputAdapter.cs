// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// Feeds Unity Input System input into Forgeborne.Core.FpsController and
// Simulation. No movement logic lives here; FpsController is canonical.
using UnityEngine;
using Forgeborne.Core;

public class FpsInputAdapter : MonoBehaviour
{
    public Simulation? Simulation;
    public float MouseSensitivity = 2f;

    void Update()
    {
        if (Simulation == null) return;
        // WASD / arrows, world-aligned in Slice 1 (no yaw rotation of movement yet;
        // movement rotation is a later Slice 1 step, not bundled into pitch).
        float forward = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
        float strafe = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        // FPS pitch aiming (Slice 6 Step 1): the adapter holds NO yaw
        // or pitch of its own any more. It feeds already-scaled mouse
        // deltas into the core Look (positive Mouse Y = look up, per
        // the core convention — verify the sign on Windows, see the
        // checklist item 9 in docs/orbital-command.md) and renders
        // the camera from the core forward vector via LookRotation,
        // so no aim math is duplicated here. The picker raycast below
        // already follows transform.forward, so a pitched camera
        // immediately lets a rally be picked by looking down at flat
        // ground — the concrete picker failure pitch exists to fix.
        // Gather (E), build (Q), and fire (click) keep their Slice 1
        // cells/rules in this step; pitch changes camera + picker
        // only, and movement stays world-aligned.
        Simulation.ApplyLook(
            Input.GetAxis("Mouse X") * MouseSensitivity,
            Input.GetAxis("Mouse Y") * MouseSensitivity);
        Vector3 lookForward = new Vector3(
            Simulation.Look.ForwardX, Simulation.Look.ForwardY, Simulation.Look.ForwardZ);
        transform.rotation = Quaternion.LookRotation(lookForward);
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
        // that rally, Escape cancels, 1 re-selects HoldGround /
        // Vanguard, and 2 selects Advance / Vanguard (Slice 4 Step
        // 2; an Advance squad ignores its rally after deployment
        // and hunts the nearest enemy — see docs/orbital-command.md).
        // While picking, the mouse does not fire: Enter is the
        // confirm, so a click cannot shoot while the player is
        // aiming a rally.
        // Since 0.18.0 a picker deploy costs 10 EmberOre and at
        // most 3 live squads may be fielded, enforced in core by
        // Confirm — see docs/orbital-command.md.
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
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Simulation.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance);
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
