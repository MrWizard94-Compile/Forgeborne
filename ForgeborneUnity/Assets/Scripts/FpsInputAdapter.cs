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
        // WASD / arrows, raw scalars: Slice 8 Step 1 rotates them by
        // the core Look yaw inside Simulation.Tick (W walks the yaw
        // direction, D the yaw right), so no rotation lives here and
        // this adapter needs no change for yaw-rotated movement.
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
        // Gather (E), build (Q), and fire (click) are aimed in
        // core from Slice 8 Step 2: the same E / Q / T / Y keys and
        // mouse click call the same builder / fire methods, which
        // now read the core Look ray — so this adapter needs no
        // new binding for aim, only the renamed fire call below.
        // Movement is yaw-rotated in core (Step 1).
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
        // Powered Unity presence (Slice 6 Step 3): T builds a
        // Kinetic Mill (front-left, via BuildKineticMillAhead) and
        // Y builds a Dynamo (front-centre, via BuildDynamoAhead),
        // wiring keys to builders that are already proved in core
        // — no core build rule is added or changed here. T and Y
        // are free: existing keys are WASD / Space / E / Q / R /
        // O / Return / Esc / 1 / 2 / 3 / 4 / mouse click, and T/Y sit
        // together as the build pair, next to each other on the
        // keyboard, so a mill and its Dynamo are built as a pair
        // without moving between them (which is also what makes
        // them face-adjacent and therefore powered). Both keys are
        // suppressed while the picker is open, like mouse fire:
        // the picker is modal for squad deployment, and a Dynamo
        // build while picking would spend 5 EmberOre — the exact
        // resource a pending Confirm is about to charge 10 of —
        // so an untimed Dynamo key could make the deploy the
        // player is aiming fail for no visible reason. The mill
        // is suppressed with it so the pair behaves as one modal
        // rule, not two. E / Q / R keep their pre-existing
        // unsuppressed behaviour in this step: they spend no
        // EmberOre, so they cannot break a pending Confirm's
        // affordability, and changing them would be a second
        // modal change bundled into a powered-presence step.
        // Mesh visibility of a key build follows the scene's
        // pre-existing refresh path (see docs/powered-presence.md):
        // this step adds no refresh hook, and the live HUD power
        // line (HudDisplay) is the presence proof that updates
        // every frame regardless of mesh refresh.
        // Slice 9 Step 1: U builds a Transformer (via
        // BuildTransformerAhead, 5 EmberOre), next to Y on the
        // keyboard so the electrical trio sits together, and
        // suppressed while the picker is open for the same ore
        // reason as Y — a transformer key while picking could
        // spend the ore a pending Confirm needs. Since Slice 8
        // all three builders place by aim, so U builds in the
        // cell before the aimed surface, like T and Y.
        // Slice 9 Step 2: I builds a Cable (via
        // BuildCableAhead, 1 EmberOre per cell), next to U so
        // the electrical row runs T / Y / U / I, and suppressed
        // while the picker is open for the same ore reason —
        // a cable key while picking could spend the ore a
        // pending Confirm needs, one cell at a time. I is free:
        // existing keys are WASD / Space / E / Q / R / O /
        // Return / Esc / 1 / 2 / 3 / 4 / T / Y / U / mouse
        // click. No drag-to-lay mode: one key press lays one
        // cable cell, by aim, like every other builder.
        if (!Simulation.OrbitalCommand.IsPicking)
        {
            if (Input.GetKeyDown(KeyCode.T)) Simulation.BuildKineticMillAhead();
            if (Input.GetKeyDown(KeyCode.Y)) Simulation.BuildDynamoAhead();
            if (Input.GetKeyDown(KeyCode.U)) Simulation.BuildTransformerAhead();
            if (Input.GetKeyDown(KeyCode.I)) Simulation.BuildCableAhead();
            // Slice 10 Step 1: B builds a workstation by aim, closing
            // the workstation's missing adapter key. B is free: see
            // the key list above. Same picker guard as the other
            // builders, for the same ore reason.
            if (Input.GetKeyDown(KeyCode.B)) Simulation.BuildWorkstationAhead();
            // Slice 10 Step 2: N builds an Alternator by aim, via
            // BuildAlternatorAhead (10 Stone, no EmberOre). N is
            // free: existing keys are WASD / Space / E / Q / R /
            // O / Return / Esc / 1 / 2 / 3 / 4 / T / Y / U / I /
            // B / mouse click. Same picker guard as the other
            // builders: an Alternator spends stone, not ore, so
            // it cannot break a pending Confirm's affordability
            // the way an ore build could — but the guard is kept
            // uniform anyway, so no builder fires while the
            // picker is modal, and the rule stays one rule.
            // No HUD source count rides along: the existing
            // powered-mill and powered-workstation counts are
            // the Alternator's display, as decided in core.
            if (Input.GetKeyDown(KeyCode.N)) Simulation.BuildAlternatorAhead();
            // Slice 12 Step 2: P builds a Shock Pylon by aim, via
            // BuildShockPylonAhead (5 Stone + 10 EmberOre). P is
            // free: existing keys are WASD / Space / E / Q / R /
            // O / Return / Esc / 1 / 2 / 3 / 4 / 5 / 6 / T / Y /
            // U / I / B / N / mouse click. Same picker guard as
            // the other builders, for the same ore reason — a
            // pylon key while picking could spend the ore a
            // pending Confirm needs.
            if (Input.GetKeyDown(KeyCode.P)) Simulation.BuildShockPylonAhead();
        }
        // Orbital doctrine menu / rally picker (0.15.0, replaces the
        // provisional O key, which deployed instantly at the player's
        // feet). O opens the picker, or cancels it if already
        // picking. While picking, a centre-screen raycast from the
        // camera updates the pending rally to the voxel cell hit
        // (the voxel mesh carries a MeshCollider for this — see
        // VoxelMeshRenderer); Return deploys the selected squad at
        // that rally, Escape cancels, 1 selects the HoldGround
        // doctrine and 2 selects Advance (Slice 4 Step 2; an
        // Advance squad ignores its rally after deployment and
        // hunts the nearest enemy — see docs/orbital-command.md),
        // and 3 / 4 / 5 select the squad kind independently of
        // doctrine (Slice 7 Step 1: 3 Vanguard, 4 Recon; Slice 11
        // Step 1: 5 Heavy), so all doctrine-kind combinations are
        // reachable and no key couples a doctrine to a kind.
        // Before Slice 7, 1 and 2 forced Vanguard as well as a
        // doctrine; they no longer do, so changing doctrine
        // keeps the selected kind. 5 is free while picking: the
        // picker keys are 1 / 2 / 3 / 4 plus Return / Esc / O,
        // and no builder key fires while picking, so 5 cannot
        // collide with a builder or with another picker key.
        // Since Slice 11 Step 2 the same holds for 6, the
        // Defend Core doctrine key: picker keys are now
        // 1 / 2 / 3 / 4 / 5 plus Return / Esc / O, and no
        // builder key fires while picking.
        // While picking, the mouse does not fire: Enter is the
        // confirm, so a click cannot shoot while the player is
        // aiming a rally.
        // Since 0.18.0 a picker deploy costs 10 EmberOre and at
        // most 3 live squads may be fielded, enforced in core by
        // Confirm — see docs/orbital-command.md. Since Slice 7
        // Step 1 the 10-ore price and 3-squad cap hold for both
        // kinds then existing, and since Slice 11 Step 1 they
        // hold for Heavy too, so the prompt's COST 10 / MAX 3
        // line stays true.
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
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Simulation.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Simulation.OrbitalCommand.SelectKind(SquadKind.Vanguard);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Simulation.OrbitalCommand.SelectKind(SquadKind.Recon);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Simulation.OrbitalCommand.SelectKind(SquadKind.Heavy);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                Simulation.OrbitalCommand.SelectDoctrine(TacticalDoctrine.DefendCore);
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
        else if (Input.GetMouseButtonDown(0)) Simulation.FireAimed();
    }
}
