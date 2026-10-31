// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// Feeds Unity Input System input into Forgeborne.Core.FpsController and
// Simulation. No movement logic lives here; FpsController is canonical.
using UnityEngine;
using Forgeborne.Core;

public class FpsInputAdapter : MonoBehaviour
{
    public Simulation? Simulation;
    // Slice 13 Step 3: wire this to the same VoxelMeshRenderer
    // GameManager links, in the scene. It is the refresh trigger:
    // a null renderer makes every refresh call below a no-op,
    // so an unwired scene renders the first mesh and never
    // refreshes it — playtest-gate item 27 names that wiring
    // as a step, not an assumption. Slice 16 Step 1 adds
    // a code-driven null-fill, decided 0.68.0 (rationale
    // in DECISIONS.md): GameManager.Start fills a null
    // field here with the renderer it already links, and
    // never overwrites a non-null field, so Inspector
    // wiring still wins and a deliberate second renderer
    // survives. The fill does not close item 27, and
    // item 31 is the observation that settles whether
    // the fill or the Inspector wiring is what the
    // scene actually runs on.
    public VoxelMeshRenderer? MeshRenderer;
    public float MouseSensitivity = 2f;

    // Slice 24 Step 1 (0.97.0): K re-rally pending state, decided
    // in that step (rationale in DECISIONS.md). This is an OWN
    // adapter flag, deliberately NOT OrbitalCommand.IsPicking:
    // reusing the picker flag would turn a Return into a paid
    // deploy at the new rally instead of a free re-rally of the
    // target squad, because the picker branch calls
    // ConfirmOrbitalDeploy on Return. The pending state stores
    // the target's snapshot position plus its doctrine, kind,
    // and old rally — the adapter-visible fields that do not
    // change when the squad moves — so Confirm can re-validate
    // the target instead of silently re-targeting a different
    // squad after a death sweep shifts the list. The candidate
    // rally is adapter state too: OrbitalCommand.SetRally
    // refuses outside picking, so the picker's rally storage
    // cannot carry a re-rally candidate. The pending marker
    // is adapter-owned for the same reason SquadMarkerDisplay
    // shows its pending sphere only while picking: one orange
    // sphere, 0.6 units, at the candidate rally cell centre,
    // reusing the picker's pending-marker reading (same shape,
    // size, and colour), unparented because this adapter has
    // no MarkerRoot field and no new scene wiring is claimed.
    private bool _pendingReRally;
    private int _pendingReRallySnapshotPos;
    private TacticalDoctrine _pendingReRallyDoctrine;
    private SquadKind _pendingReRallyKind;
    private int _pendingReRallyOldX;
    private int _pendingReRallyOldY;
    private int _pendingReRallyOldZ;
    private int _pendingReRallyX;
    private int _pendingReRallyY;
    private int _pendingReRallyZ;
    private GameObject? _pendingReRallyMarker;
    private Material? _pendingReRallyMaterial;

    // Slice 24 Step 2 (0.98.0): X dismiss pending state, decided
    // in that step (rationale in DECISIONS.md). An OWN adapter
    // flag, separate from the re-rally flag above and from
    // OrbitalCommand.IsPicking, for the same reason Step 1
    // refused the picker flag: Return must confirm exactly
    // one pending action, never a paid deploy and never
    // both a dismiss and a re-rally. The pending state
    // stores the target's snapshot position plus its
    // doctrine, kind, and rally — the adapter-visible
    // fields that do not change when the squad moves — so
    // Confirm can re-validate the target instead of
    // silently dismissing a different squad after a death
    // sweep shifts the list. There is NO pending marker
    // for a dismiss: a dismiss has no candidate cell to
    // mark, and inventing a highlight or HUD word would
    // be a marker / HUD change smuggled inside a bindings
    // step. While pending, the player reads the target
    // squad's own text line and markers, which vanish
    // with its snapshot entry on confirm under the
    // existing marker rule; the invisible-pending
    // readability gap is owned as a known limit in
    // DECISIONS.md, for a later display step to weigh
    // on Windows evidence, not patched here.
    private bool _pendingDismiss;
    private int _pendingDismissSnapshotPos;
    private TacticalDoctrine _pendingDismissDoctrine;
    private SquadKind _pendingDismissKind;
    private int _pendingDismissRallyX;
    private int _pendingDismissRallyY;
    private int _pendingDismissRallyZ;

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
        // Slice 13 Step 3 refresh event set, named, not "always":
        // a tick refreshes the mesh only when a mill was destroyed
        // on that tick (DestroyedMills rose), never every frame —
        // a full 32^3 re-mesh every frame would spend the Slice 1
        // budget for no visible change on frames where nothing
        // changed, and power state itself changes only when
        // placement changes, which the build / gather events
        // below already cover. This check also answers, in
        // playtest-gate item 27, the item-17 question of whether
        // a destruction is visible on its tick or only after a
        // later refresh: with this hook it should be on its tick,
        // and item 27 is the observation that proves or refutes
        // that expectation.
        int destroyedBefore = Simulation.World.DestroyedMills;
        Simulation.Tick(Time.deltaTime, forward, strafe, jump);
        if (Simulation.World.DestroyedMills != destroyedBefore && MeshRenderer != null) MeshRenderer.RefreshMesh();
        // Camera follows the FPS controller's feet position + eye height.
        transform.position = new Vector3(Simulation.Fps.X, Simulation.Fps.Y + 1.6f, Simulation.Fps.Z);

        // Slice 13 Step 3 refresh event set, continued: a
        // successful gather or build refreshes the mesh at once,
        // because that key press is exactly what changed the
        // chunk or the wiring — a failed key (no resources, no
        // target, floating, not air) refreshes nothing, so a
        // wasted press costs no re-mesh. Reload and fire change
        // no voxel, so they never refresh; a squad deploy changes
        // markers, not voxels, so it never refreshes either.
        // Together with the destruction check above, load, and
        // Start, this is the whole event set: every chunk or
        // wiring change in the game reaches a refresh through
        // one of these paths, and no path refreshes blindly.
        if (Input.GetKeyDown(KeyCode.E) && Simulation.GatherAhead() == GatherResult.Success && MeshRenderer != null) MeshRenderer.RefreshMesh();
        if (Input.GetKeyDown(KeyCode.Q) && Simulation.BuildWallAhead() == BuildResult.Success && MeshRenderer != null) MeshRenderer.RefreshMesh();
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
        // Mesh visibility of a key build: Slice 13 Step 3
        // supersedes the pre-Step-3 statement that this step
        // adds no refresh hook (see docs/powered-presence.md
        // item 6, now answered by playtest-gate item 27) — a
        // successful key build refreshes the mesh at once, by
        // the event set above, and the live HUD power line
        // (HudDisplay) still updates every frame regardless.
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
        // Slice 24 Step 1: builders are suppressed while a
        // re-rally pick is pending, as while the picker is
        // open — the pending pick is a pick-like state, and
        // a builder firing while the player aims a rally
        // would spend resources mid-pick (and a Q wall could
        // land in the candidate cell). E / Q / R above keep
        // their pre-existing unsuppressed behaviour, as
        // while picking: the re-rally is free, so no build
        // can break a pending confirm's affordability, and
        // a solid candidate rally is accepted by the gate
        // anyway, so a Q wall in that cell changes nothing
        // the gate would refuse.
        // Slice 24 Step 2: builders are suppressed while a
        // dismiss is pending on the same rule — a pending
        // dismiss is a pick-like modal state, and a builder
        // firing while the player decides on a removal
        // would spend resources mid-decision for no reason.
        if (!Simulation.OrbitalCommand.IsPicking && !_pendingReRally && !_pendingDismiss)
        {
            // Slice 13 Step 3: each builder refreshes on success
            // only, by the event set above — see the E / Q note.
            if (Input.GetKeyDown(KeyCode.T) && Simulation.BuildKineticMillAhead() == BuildResult.Success && MeshRenderer != null) MeshRenderer.RefreshMesh();
            if (Input.GetKeyDown(KeyCode.Y) && Simulation.BuildDynamoAhead() == BuildResult.Success && MeshRenderer != null) MeshRenderer.RefreshMesh();
            if (Input.GetKeyDown(KeyCode.U) && Simulation.BuildTransformerAhead() == BuildResult.Success && MeshRenderer != null) MeshRenderer.RefreshMesh();
            if (Input.GetKeyDown(KeyCode.I) && Simulation.BuildCableAhead() == BuildResult.Success && MeshRenderer != null) MeshRenderer.RefreshMesh();
            // Slice 10 Step 1: B builds a workstation by aim, closing
            // the workstation's missing adapter key. B is free: see
            // the key list above. Same picker guard as the other
            // builders, for the same ore reason.
            if (Input.GetKeyDown(KeyCode.B) && Simulation.BuildWorkstationAhead() == BuildResult.Success && MeshRenderer != null) MeshRenderer.RefreshMesh();
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
            if (Input.GetKeyDown(KeyCode.N) && Simulation.BuildAlternatorAhead() == BuildResult.Success && MeshRenderer != null) MeshRenderer.RefreshMesh();
            // Slice 12 Step 2: P builds a Shock Pylon by aim, via
            // BuildShockPylonAhead (5 Stone + 10 EmberOre). P is
            // free: existing keys are WASD / Space / E / Q / R /
            // O / Return / Esc / 1 / 2 / 3 / 4 / 5 / 6 / T / Y /
            // U / I / B / N / mouse click. Same picker guard as
            // the other builders, for the same ore reason — a
            // pylon key while picking could spend the ore a
            // pending Confirm needs.
            if (Input.GetKeyDown(KeyCode.P) && Simulation.BuildShockPylonAhead() == BuildResult.Success && MeshRenderer != null) MeshRenderer.RefreshMesh();
            // Slice 14 Step 1: M builds a Conduit by aim, via
            // BuildConduitAhead (5 Stone + 5 EmberOre). M is
            // free: existing keys are WASD / Space / E / Q / R /
            // O / Return / Esc / 1 / 2 / 3 / 4 / 5 / 6 / T / Y /
            // U / I / B / N / P / mouse click. Same picker guard
            // as the other builders, for the same ore reason —
            // a Conduit key while picking could spend the ore
            // a pending Confirm needs. No HUD source count rides
            // along: the existing live-network and lone-source
            // counts are the Conduit's display, as decided in
            // core, and a Conduit with no cable counts as a lone
            // source like a lone Dynamo or Alternator.
            if (Input.GetKeyDown(KeyCode.M) && Simulation.BuildConduitAhead() == BuildResult.Success && MeshRenderer != null) MeshRenderer.RefreshMesh();
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
        // Since Slice 15 Step 1 the same holds for 7, the
        // Patrol doctrine key: picker keys are now
        // 1 / 2 / 3 / 4 / 5 / 6 plus Return / Esc / O, and no
        // builder key fires while picking, so 7 cannot
        // collide with a builder or with another picker key.
        // While picking, the mouse does not fire: Enter is the
        // confirm, so a click cannot shoot while the player is
        // aiming a rally.
        // Slice 24 Step 1 (0.97.0): K re-rally binding,
        // IMPLEMENTED in this adapter, NOT Unity-verified
        // and NOT claimed as working in a scene — adapter
        // code is not compiled headlessly, so no test
        // count proves this key. It serves both the stuck
        // action (Slice 20 Step 1) and the general action
        // (Slice 21 Step 1) through the GENERAL gate alone
        // (Simulation.TryReRallySquad): a stuck Patrol
        // succeeds identically under both gates, so no
        // squad-and-rally exists where the stuck gate
        // succeeds and the general gate fails, and a
        // stuck-first fallback would add a branch with no
        // different outcome. K while NOT picking, with
        // the crosshair on an eligible (live HoldGround
        // or Patrol) squad's marker, opens a re-rally
        // pick; Return confirms, Escape cancels, K again
        // cancels (toggle-off, like O cancels the picker).
        // K is free: used keys are WASD / Space / E /
        // Q / R / O / Return / Esc / Alpha1-7 / T / Y /
        // U / I / B / N / P / M / mouse click.
        // "Crosshair on a marker" is implemented as
        // marker-centre proximity, NOT as a raycast hit
        // on a marker, because SquadMarkerDisplay destroys
        // every marker collider on creation: a raycast
        // can never hit a marker, and giving markers
        // colliders would make them block the picker's
        // own raycast. On K, each eligible squad's marker
        // centre (cell centre, as rendered) is tested for
        // angle from the centre ray within max(2 degrees,
        // atan(0.6 / distance)) — the marker's angular
        // size plus slack, floored so a far marker stays
        // aimable — for distance at most 30f (the picker
        // range), and for a clear line: a raycast toward
        // that centre that hits a voxel first means the
        // squad is occluded and is not selected. The
        // smallest angle wins; ties go to the first
        // snapshot position. A rally marker never
        // selects: only squad centres are tested, so a
        // crosshair on a rally cell whose squad stands
        // elsewhere opens nothing, and a squad standing
        // on its own rally still selects by its squad
        // centre. The marker-to-index translation walks
        // Simulation.World.Squads in order skipping dead
        // squads, exactly as OrbitalView.Snapshot does,
        // so a dead squad awaiting its sweep cannot shift
        // a snapshot position into the wrong gate index.
        // The pending pick is the own-flag state above,
        // never the picker flag; while it is open, O does
        // nothing, builders do nothing, the picker keys
        // do nothing, and the mouse does not fire — the
        // picker modal rule, extended to this pick-like
        // state — while E / Q / R keep their pre-existing
        // unsuppressed behaviour, as while picking. The
        // candidate rally reuses the picker's centre
        // raycast (30f, floored cell) and its pending
        // marker reading (adapter-owned orange sphere, see
        // the fields above); the target squad's rally
        // marker stays until the gate succeeds, then moves
        // under the existing snapshot rendering. Confirm
        // re-validates the target (snapshot position still
        // holds the same doctrine, kind, and old rally)
        // and then calls the general gate with the freshly
        // translated gate index; any mismatch clears the
        // pending state with nothing changed, never
        // re-targeting a different live squad. Return
        // clears the pending state whether the gate
        // succeeds or refuses (including a same-rally
        // refusal): a confirm is one attempt, and a retry
        // re-selects its target fresh. Observed only
        // under playtest-gate items 39 and 40, plus the
        // pending-routing readings in item 42, never from
        // a headless test. The X dismiss binding below is
        // IMPLEMENTED in Slice 24 Step 2, and the
        // cross-pending answer is decided there:
        // refuse-while-pending both ways — X while a
        // re-rally pick is pending does nothing, and K
        // while a dismiss is pending does nothing — so
        // Return can never confirm an action the player
        // thought was the other one.
        // Slice 24 Step 2 (0.98.0): X dismiss binding,
        // IMPLEMENTED in this adapter, NOT Unity-verified
        // and NOT claimed as working in a scene — adapter
        // code is not compiled headlessly, so no test
        // count proves this key. It serves the dismiss
        // action (Slice 22 Step 1) through its gate alone
        // (Simulation.TryDismissSquad): any live squad of
        // any doctrine and any kind, by snapshot index,
        // refund none, one cap slot freed. X while NOT
        // picking, with the crosshair on any live squad's
        // marker (every doctrine is eligible, unlike K's
        // re-rally, so an Advance or Defend Core marker
        // qualifies here), opens an adapter pending
        // dismiss that Return confirms and Escape cancels
        // — the pending state lives in the adapter, not in
        // core, so the core call itself still removes on
        // a single call and no headless test proves the
        // confirm step. X is free: used keys are WASD /
        // Space / E / Q / R / O / Return / Esc / Alpha1-7 /
        // T / Y / U / I / B / N / P / M / K / mouse click,
        // and K is taken by the re-rally binding above,
        // so X collides with neither. Targeting reuses
        // Step 1's marker-centre proximity rule exactly
        // (angle within max(2 degrees, atan(0.6 /
        // distance)), distance at most 30f, occlusion
        // raycast, smallest angle wins, ties to the first
        // snapshot position) with the one difference the
        // dismiss scope requires: NO doctrine filter, so
        // an Advance or Defend Core marker that K refuses
        // is a valid dismiss target, not an error. The
        // marker-to-index translation is Step 1's
        // SnapshotPosToGateIndex, reused, not re-decided:
        // a dead squad awaiting its sweep cannot shift a
        // snapshot position into the wrong gate index.
        // The binding does nothing while picking, nothing
        // when the crosshair selects no live marker,
        // nothing when the target squad refuses, nothing
        // on Return with no pending dismiss, and nothing
        // on a cancelled confirmation — and the whole
        // binding is observed only under playtest-gate
        // item 41, plus the pending-routing readings in
        // item 43, never from a headless test.
        // Since 0.18.0 a picker deploy costs 10 EmberOre and at
        // most 3 live squads may be fielded, enforced in core by
        // Confirm — see docs/orbital-command.md. Since Slice 7
        // Step 1 the 10-ore price and 3-squad cap hold for both
        // kinds then existing, and since Slice 11 Step 1 they
        // hold for Heavy too, so the prompt's COST 10 / MAX 3
        // line stays true.
        // Slice 24 Step 1: O does nothing while a re-rally
        // pick is pending — opening a deploy pick on top
        // of a pending re-rally would stack two pending
        // states under two flags, and the picker branch
        // below never runs while pending, so its O-cancel
        // cannot clear the re-rally pick. Escape (or K
        // again) cancels the re-rally pick; O works again
        // once it is closed.
        // Slice 24 Step 2: O does nothing while a dismiss
        // is pending, on the same refuse-while-pending
        // rule, and the K/X cross-pending answer is now
        // decided symmetrically: X while a re-rally pick
        // is pending does nothing (the re-rally branch
        // above reads no X key), and K while a dismiss
        // is pending does nothing (the dismiss branch
        // below reads no K key). The two pending flags
        // are therefore mutually exclusive by routing:
        // each opener runs only from the else branch
        // where neither flag is set, so no frame can set
        // both, and Return confirms exactly one action.
        if (Input.GetKeyDown(KeyCode.O) && !_pendingReRally && !_pendingDismiss)
        {
            if (Simulation.OrbitalCommand.IsPicking) Simulation.CancelOrbitalPick();
            else Simulation.BeginOrbitalPick();
        }
        if (_pendingDismiss)
        {
            HandlePendingDismiss();
        }
        else if (_pendingReRally)
        {
            HandlePendingReRally();
        }
        else if (Simulation.OrbitalCommand.IsPicking)
        {
            // Slice 18 Step 2 audit note: Enter deploys ONLY
            // through ConfirmOrbitalDeploy (paid). The former
            // free primitives (WorldZone.DeploySquad,
            // Simulation.DeployOrbitalSquad) were removed in
            // that step, so calling one from this file would
            // fail to compile — this adapter must never do so.
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
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                Simulation.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol);
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
        else
        {
            // Slice 24 Step 1: K opens a re-rally pick only
            // here — not picking (that branch is above) and
            // not already pending (that branch is above it).
            // A K press on the same frame as a click opens
            // the pick and does not fire: the pick-like
            // state suppresses the mouse from its first
            // frame, as the picker does.
            // Slice 24 Step 2: X opens a dismiss pick only
            // here, on the same terms — not picking, not
            // re-rally pending, not dismiss pending — and
            // a same-frame click does not fire, for the
            // same reason. K is checked first, so a
            // same-frame K+X press opens the re-rally
            // pick only; that frame order is routing
            // determinism, not a designed key priority,
            // and the player re-presses X after closing
            // the re-rally pick to dismiss instead.
            if (Input.GetKeyDown(KeyCode.K)) TryOpenReRallyPick();
            else if (Input.GetKeyDown(KeyCode.X)) TryOpenDismissPick();
            else if (Input.GetMouseButtonDown(0)) Simulation.FireAimed();
        }
    }

    // Slice 24 Step 1 helpers. No core rule lives here: these
    // methods only select a target, hold pending state, and
    // call the Simulation passthrough, whose gate decides.
    private void TryOpenReRallyPick()
    {
        if (Simulation == null) return;
        var snapshot = OrbitalView.Snapshot(Simulation.World);
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;
        int bestPos = -1;
        float bestAngle = float.PositiveInfinity;
        for (int i = 0; i < snapshot.Count; i++)
        {
            var s = snapshot[i];
            // General-gate eligibility, decided at press so a
            // pick the gate will certainly refuse never opens:
            // doctrine never changes, so an Advance or Defend
            // Core marker refused here would be refused on
            // confirm too. Only death, a same-rally pick, or a
            // list shift can still refuse at confirm time, and
            // Confirm re-validates for exactly those.
            if (s.Doctrine != TacticalDoctrine.HoldGround
                && s.Doctrine != TacticalDoctrine.Patrol) continue;
            Vector3 centre = new Vector3(s.X + 0.5f, s.Y + 0.5f, s.Z + 0.5f);
            Vector3 toSquad = centre - origin;
            float dist = toSquad.magnitude;
            if (dist > 30f) continue;
            // Occlusion, exact for this squad's centre line:
            // a voxel hit before the centre means a wall
            // stands between the crosshair and the marker,
            // and an honestly occluded marker (see
            // SquadMarkerDisplay) is not selectable. Markers
            // carry no colliders, so they never appear as a
            // hit here; the squad's own cell is air, so no
            // hit lands exactly on the centre.
            if (Physics.Raycast(origin, toSquad.normalized, out RaycastHit blockHit, dist)
                && blockHit.distance < dist) continue;
            float angle = Vector3.Angle(forward, toSquad);
            float tolerance = Mathf.Max(2f, Mathf.Atan(0.6f / dist) * Mathf.Rad2Deg);
            if (angle > tolerance) continue;
            if (angle < bestAngle)
            {
                bestAngle = angle;
                bestPos = i;
            }
        }
        if (bestPos < 0) return;
        var target = snapshot[bestPos];
        _pendingReRally = true;
        _pendingReRallySnapshotPos = bestPos;
        _pendingReRallyDoctrine = target.Doctrine;
        _pendingReRallyKind = target.Kind;
        _pendingReRallyOldX = target.RallyX;
        _pendingReRallyOldY = target.RallyY;
        _pendingReRallyOldZ = target.RallyZ;
        // The candidate starts at the target's own rally, so
        // the pending marker reads a real cell from the first
        // frame, and an immediate Return is a same-rally
        // refusal that clears the pick — never a silent move.
        _pendingReRallyX = target.RallyX;
        _pendingReRallyY = target.RallyY;
        _pendingReRallyZ = target.RallyZ;
        ShowReRallyMarker();
    }

    // Slice 24 Step 2 helpers. No core rule lives here: these
    // methods only select a target, hold pending state, and
    // call the Simulation passthrough, whose gate decides.
    // Targeting mirrors TryOpenReRallyPick's proximity loop
    // with the dismiss scope's one difference: NO doctrine
    // filter, because TryDismissSquad consults no doctrine.
    // A rally marker never selects: only squad centres are
    // tested, so a crosshair on a rally cell whose squad
    // stands elsewhere opens nothing.
    private void TryOpenDismissPick()
    {
        if (Simulation == null) return;
        var snapshot = OrbitalView.Snapshot(Simulation.World);
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;
        int bestPos = -1;
        float bestAngle = float.PositiveInfinity;
        for (int i = 0; i < snapshot.Count; i++)
        {
            var s = snapshot[i];
            Vector3 centre = new Vector3(s.X + 0.5f, s.Y + 0.5f, s.Z + 0.5f);
            Vector3 toSquad = centre - origin;
            float dist = toSquad.magnitude;
            if (dist > 30f) continue;
            // Occlusion, as in the re-rally pick: a voxel hit
            // before the centre means an honestly occluded
            // marker is not selectable. Markers carry no
            // colliders, so they never appear as a hit here.
            if (Physics.Raycast(origin, toSquad.normalized, out RaycastHit blockHit, dist)
                && blockHit.distance < dist) continue;
            float angle = Vector3.Angle(forward, toSquad);
            float tolerance = Mathf.Max(2f, Mathf.Atan(0.6f / dist) * Mathf.Rad2Deg);
            if (angle > tolerance) continue;
            if (angle < bestAngle)
            {
                bestAngle = angle;
                bestPos = i;
            }
        }
        if (bestPos < 0) return;
        var target = snapshot[bestPos];
        _pendingDismiss = true;
        _pendingDismissSnapshotPos = bestPos;
        _pendingDismissDoctrine = target.Doctrine;
        _pendingDismissKind = target.Kind;
        _pendingDismissRallyX = target.RallyX;
        _pendingDismissRallyY = target.RallyY;
        _pendingDismissRallyZ = target.RallyZ;
        // No pending marker is created: a dismiss has no
        // candidate cell to mark (see the fields above).
    }

    private void HandlePendingDismiss()
    {
        // Deliberately unread here: K (refuse-while-pending —
        // opening a re-rally pick on top of a dismiss would
        // stack two pending states and leave Return
        // ambiguous), O (refuse-while-pending, as in Step 1),
        // the builder keys (suppressed by the guard above),
        // the picker keys (the picker branch never runs
        // while pending), and the mouse (the else branch
        // that fires never runs while pending). Return,
        // Escape, and X again are the whole dismiss
        // routing: confirm, cancel, and toggle-off cancel,
        // symmetric with K again cancelling a re-rally.
        if (Input.GetKeyDown(KeyCode.Return)) ConfirmPendingDismiss();
        else if (Input.GetKeyDown(KeyCode.Escape)) CancelPendingDismiss();
        else if (Input.GetKeyDown(KeyCode.X)) CancelPendingDismiss();
    }

    private void ConfirmPendingDismiss()
    {
        if (Simulation != null)
        {
            // Re-validation before any gate call, on the
            // same movement-invariant triple Step 1 uses:
            // the stored snapshot position must still hold
            // a squad with the same doctrine, kind, and
            // rally. Position is NOT part of the check —
            // a walking squad changes cell every tick —
            // and health is not either, because an enemy
            // hit mid-decision must not void a confirm.
            // A target that died and was swept shifts
            // every later live position by one, so the
            // stored position then names a different squad
            // or no squad at all, and this check catches
            // that shift unless the shifted-in squad is an
            // identical twin (same doctrine, kind, and
            // rally), which no adapter-visible field can
            // distinguish without inventing a core identity
            // the save does not carry. That twin case is
            // the known limit, recorded in DECISIONS.md:
            // it needs a twin death plus a sweep between
            // press and confirm, and its consequence is
            // bounded to dismissing the wrong twin — one
            // slot freed and no refund either way, never a
            // charge or a second deletion. A dismiss can
            // never be the shift source itself while one
            // is pending, because X is refused while
            // pending and no other adapter path dismisses.
            // Pending state does NOT auto-clear when the
            // target dies mid-decision: the adapter does
            // not poll deaths, and the stale pending is
            // caught at confirm instead, and cleared there
            // with nothing changed.
            var snapshot = OrbitalView.Snapshot(Simulation.World);
            if (_pendingDismissSnapshotPos < snapshot.Count)
            {
                var s = snapshot[_pendingDismissSnapshotPos];
                if (s.Doctrine == _pendingDismissDoctrine
                    && s.Kind == _pendingDismissKind
                    && s.RallyX == _pendingDismissRallyX
                    && s.RallyY == _pendingDismissRallyY
                    && s.RallyZ == _pendingDismissRallyZ)
                {
                    int gateIndex = SnapshotPosToGateIndex(_pendingDismissSnapshotPos);
                    if (gateIndex >= 0)
                    {
                        // Dismiss gate only. Its result is
                        // not branched on: success and refusal
                        // alike clear the pending state, so a
                        // confirm is one attempt and a second
                        // Return cannot re-apply a stale pick.
                        // On success the squad's text line
                        // and both markers vanish with its
                        // snapshot entry under the existing
                        // marker rule; no HUD word is added.
                        Simulation.TryDismissSquad(gateIndex);
                    }
                }
            }
        }
        CancelPendingDismiss();
    }

    private void CancelPendingDismiss()
    {
        _pendingDismiss = false;
    }

    private void HandlePendingReRally()
    {
        if (Input.GetKeyDown(KeyCode.Return)) ConfirmPendingReRally();
        else if (Input.GetKeyDown(KeyCode.Escape)) CancelPendingReRally();
        else if (Input.GetKeyDown(KeyCode.K)) CancelPendingReRally();
        else
        {
            // Rally selection reuses the picker's centre
            // raycast (30f, floored cell). No hit within
            // range leaves the candidate unchanged, as the
            // picker leaves its pending rally unchanged —
            // the player reads the pending marker, which
            // stays on the last aimed cell.
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 30f))
            {
                _pendingReRallyX = Mathf.FloorToInt(hit.point.x);
                _pendingReRallyY = Mathf.FloorToInt(hit.point.y);
                _pendingReRallyZ = Mathf.FloorToInt(hit.point.z);
            }
            ShowReRallyMarker();
        }
    }

    private void ConfirmPendingReRally()
    {
        if (Simulation != null)
        {
            // Re-validation before any gate call: the stored
            // snapshot position must still hold a squad with
            // the same doctrine, kind, and old rally. A target
            // that died and was swept shifts every later live
            // position by one, so the stored position then
            // names a different squad or no squad at all —
            // and this check catches that shift unless the
            // shifted-in squad is an identical twin (same
            // doctrine, kind, and rally), which no
            // adapter-visible field can distinguish without
            // inventing a core identity the save does not
            // carry. That twin case is the known limit,
            // recorded in DECISIONS.md; it needs a twin
            // death plus a sweep between press and confirm.
            // Movement never invalidates: a walking squad
            // keeps its doctrine, kind, and rally.
            var snapshot = OrbitalView.Snapshot(Simulation.World);
            if (_pendingReRallySnapshotPos < snapshot.Count)
            {
                var s = snapshot[_pendingReRallySnapshotPos];
                if (s.Doctrine == _pendingReRallyDoctrine
                    && s.Kind == _pendingReRallyKind
                    && s.RallyX == _pendingReRallyOldX
                    && s.RallyY == _pendingReRallyOldY
                    && s.RallyZ == _pendingReRallyOldZ)
                {
                    int gateIndex = SnapshotPosToGateIndex(_pendingReRallySnapshotPos);
                    if (gateIndex >= 0)
                    {
                        // General gate only (see the Slice 24
                        // Step 1 note above). Its result is
                        // not branched on: success and refusal
                        // alike clear the pending state, so a
                        // confirm is one attempt and a second
                        // Return cannot re-apply a stale pick.
                        Simulation.TryReRallySquad(
                            gateIndex,
                            _pendingReRallyX, _pendingReRallyY, _pendingReRallyZ);
                    }
                }
            }
        }
        CancelPendingReRally();
    }

    // The marker-to-index translation, in one place, reused
    // by Step 2's dismiss binding as decided there: walk
    // Squads in order skipping dead squads,
    // exactly as OrbitalView.Snapshot does, and count live
    // positions until the stored snapshot position. -1 means
    // the position no longer exists (a sweep shrank the
    // live list past it), and the caller clears pending
    // with nothing changed.
    private int SnapshotPosToGateIndex(int snapshotPos)
    {
        if (Simulation == null) return -1;
        int livePos = -1;
        var squads = Simulation.World.Squads;
        for (int i = 0; i < squads.Count; i++)
        {
            if (squads[i].IsDead) continue;
            livePos++;
            if (livePos == snapshotPos) return i;
        }
        return -1;
    }

    private void CancelPendingReRally()
    {
        _pendingReRally = false;
        if (_pendingReRallyMarker != null) _pendingReRallyMarker.SetActive(false);
    }

    private void ShowReRallyMarker()
    {
        if (_pendingReRallyMarker == null)
        {
            _pendingReRallyMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _pendingReRallyMarker.name = "ReRallyPendingMarker";
            if (_pendingReRallyMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                _pendingReRallyMaterial = new Material(shader) { color = new Color(1f, 0.55f, 0f) };
            }
            _pendingReRallyMarker.GetComponent<Renderer>().sharedMaterial = _pendingReRallyMaterial;
            // Display only, as in SquadMarkerDisplay: the
            // marker must not block the rally raycast.
            Destroy(_pendingReRallyMarker.GetComponent<Collider>());
            _pendingReRallyMarker.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        }
        _pendingReRallyMarker.transform.position = new Vector3(
            _pendingReRallyX + 0.5f, _pendingReRallyY + 0.5f, _pendingReRallyZ + 0.5f);
        _pendingReRallyMarker.SetActive(true);
    }
}
