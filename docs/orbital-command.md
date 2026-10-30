# Forgeborne — Orbital doctrine menu / rally picker

**Status: CORE HEADLESS-TESTED. UNITY ADAPTER CODE WRITTEN,
NOT YET COMPILED OR RUN IN UNITY ON WINDOWS.**
Do not claim the picker is verified in Unity until the
checklist below is completed there and recorded in DECISIONS.md.
This step replaces the provisional O key from 0.11.0, which
deployed a squad instantly at the player's feet. That binding
is gone: in play, a deploy is now always a picked rally.

## What the picker is

`OrbitalCommand` in `src/Forgeborne.Core/Orbital.cs` is the
menu state, in pure C# core — no rendering, no second copy of
squad rules. It holds the selected doctrine (HoldGround by
default, Advance since Slice 4 Step 2, Defend Core since
Slice 11 Step 2) and squad kind
(Vanguard by default, Recon since Slice 7 Step 1, Heavy
since Slice 11 Step 1), and a
pending rally while picking. Flow:

1. `BeginPick()` opens the picker and clears any stale pending
   rally, so Enter cannot deploy yesterday's point.
2. `SetRally(x, y, z)` updates the pending rally while picking.
   Only bounds are checked, matching `WorldZone.DeploySquad`:
   a rally inside a solid block is accepted and the squad then
   holds short of it, so a bad rally is visible, not silent.
   SetRally outside picking, or out of bounds, fails closed.
3. `Confirm(world)` deploys one squad with the selected
   doctrine/kind at the pending rally, then closes the picker
   and clears the rally. Confirm with no pick or no rally
   deploys nothing; a failed deploy keeps the picker open for
   a re-pick instead of losing the menu.
4. `CancelPick()` abandons the pick: no squad, rally dropped.
5. `SelectDoctrine` / `SelectKind` reject unknown values and
   leave the selection unchanged.

`Simulation` exposes the same operations as
`BeginOrbitalPick`, `SetOrbitalRally`, `CancelOrbitalPick`,
and `ConfirmOrbitalDeploy`. The direct `DeployOrbitalSquad`
path remains for headless use and the demo; the Unity adapter
no longer uses it.

## Unity bindings (thin adapters only)

`ForgeborneUnity/Assets/Scripts/FpsInputAdapter.cs`:

- **O** — open the picker, or cancel it if already picking.
- **While picking** — a centre-screen raycast from the camera
  (forward, 30 m) updates the pending rally to the voxel cell
  hit. `VoxelMeshRenderer` now carries a `MeshCollider` sharing
  the built mesh, so the raycast actually hits the chunk; without
  it the ray would pass through the rendered voxels and never
  set a rally. Since Slice 6 Step 1 (0.28.0) the camera also
  pitches: the adapter feeds mouse deltas into the core
  `Look` and renders from its forward vector, so the ray
  follows the pitched camera and a rally on flat ground ahead
  can be picked by looking down at it, not by aiming at the
  face beyond it. Gather, build, and fire still keep their
  Slice 1 rules in that step — pitch changes camera + picker
  only — and movement stays world-aligned.
- **Enter** — deploy the selected squad at the pending rally.
- **Esc** — cancel the pick, no squad.
- **1** — select the HoldGround doctrine (kind unchanged).
- **2** — select the Advance doctrine (kind unchanged).
- **3** — select the Vanguard kind (doctrine unchanged).
- **4** — select the Recon kind (doctrine unchanged).
- **5** — select the Heavy kind (doctrine unchanged).
- **6** — select the Defend Core doctrine (kind unchanged).
  Since Slice 11 Step 2, the picker offers three doctrines
  independently of kind, at the same 10-ore price
  and 3-squad cap, so all nine doctrine-kind combinations
  deploy through the picker. The prompt line already
  shows the selected doctrine verbatim, so it reads e.g.
  `VANGUARD DEFENDCORE`.
  Since Slice 11 Step 1, the picker offers three kinds
  independently of doctrine, at the same 10-ore price
  and 3-squad cap, so all six doctrine-kind combinations
  deploy through the picker. The prompt line already
  shows the selected kind verbatim, so it reads e.g.
  `HEAVY ADVANCE`.
  Since Slice 7 Step 1, doctrine and kind are two
  independent choices: before it, 1 and 2 also forced
  Vanguard, so the fourth combinations were unreachable;
  now all four doctrine-kind combinations deploy through
  the picker. The prompt line already shows the selected
  kind verbatim, so it reads e.g. `RECON ADVANCE`.
- While picking, the mouse does not fire: Enter is the confirm,
  so a click cannot shoot while the player is aiming a rally.

## Second doctrine — Advance (Slice 4 Step 2, 0.22.0)

Chosen in that step from Advance / Defend Core / Patrol, with
the reasoning in DECISIONS.md. Advance is a hunting doctrine,
not HoldGround at a moving rally:

- Each tick the squad moves one voxel toward its NEAREST living
  enemy (Manhattan, first in list order on ties), re-derived
  every tick — the same nearest-enemy rule as its fire, so
  movement and fire never disagree about the prey.
- It fires at the nearest enemy in range 3 whether or not it is
  at its rally. HoldGround's "do not fire while moving" rule
  does not apply to Advance, because an advancing squad in
  range is engaging, not relocating.
- The rally the picker still requires to deploy is IGNORED
  after deployment for Advance. With no living enemy the squad
  stands still; it does not march to its rally, and it stops
  hunting (and stands still) once its prey dies. That
  no-rally-fallback is the behaviour that keeps Advance from
  collapsing into HoldGround.
- Movement itself is not a second rule: Advance and HoldGround
  share one step rule (largest-gap axis, X/Y/Z tie-break,
  solid and enemy-occupied cells refused), only the target
  differs. Adjacent-enemy damage is unchanged — one path, in
  the squad phase, after the squad fires.
- Pricing is settled, not reopened: Advance deploys through
  the Slice 3 paid path, 10 EmberOre, cap 3 live squads, no
  new price. Since Slice 7 Step 1 both kinds work under
  both doctrines, at the same price and cap.
- Display note, not a core rule: `OrbitalSquadDisplay` still
  words a squad MOVING / HOLDING by its rally position, so for
  an Advance squad that word is rally-relative, not a hunting
  state — it can read MOVING while the squad is standing
  adjacent to an enemy and firing. The doctrine name in the
  same line (`VANGUARD ADVANCE`) is the reliable marker. A
  hunting-state display word is a presentation gap, not a
  core behaviour gap.

`OrbitalSquadDisplay.cs` shows a picker prompt line first while
picking (`ORBITAL: PICKING RALLY - VANGUARD HOLDGROUND ->
(x,y,z) - ENTER TO DEPLOY, ESC TO CANCEL`, or `AIM AT A TARGET
CELL` when no rally is set yet), then the squad list unchanged.
With no squads and no pick in progress it reads
`ORBITAL: NO SQUADS (press O to pick a rally)`.

## Deployment cost / cap — Slice 3 Step 2 (0.18.0)

Cost and cap now exist, decided in the Slice 3 economy step
(rationale in DECISIONS.md, values labelled first-pass, not
final balance):

- **Cost: 10 EmberOre per squad, either kind**, paid from the
  player inventory at deploy time. Slice 7 Step 1 keeps one
  price for Vanguard and Recon alike (see below), so the
  picker prompt's COST 10 line stays true for both kinds. Pricing basis: gathered
  resources, not a new currency. EmberOre only, not Stone, so
  squads do not compete with walls and mills for the
  gather -> build loop's core material; the competition is
  with workstations (5 EmberOre each), a real but bounded
  tradeoff. Ore is ~4% of ground cells, so 10 ore is
  deliberate gathering, not incidental pickup.
- **Cap: 3 concurrent live squads.** At the cap a deploy is
  refused — no oldest-squad recall, because recall would
  silently destroy a paid investment and its position. Dead
  squads do not count toward the cap, so a replacement can
  deploy after a squad dies. No refund exists, on death or
  otherwise: deployment is a sunk cost.
- **Picker enforcement (fail-closed):** `Confirm` deploys
  through `WorldZone.TryDeployPaidSquad`, which checks cap
  and affordability BEFORE charging. An unaffordable or
  over-cap confirm deploys nothing, charges nothing, and
  leaves the picker open for a re-pick after gathering more
  ore or after a slot frees up.
- The direct `DeployOrbitalSquad` / `WorldZone.DeploySquad`
  path remains the free spawn primitive for headless tests
  and the demo only; it is not the play path and Unity play
  must never call it. The demo's Slice 2 squad now deploys
  through the paid path.

## Second squad type — Recon (Slice 7 Step 1, 0.33.0)

Chosen in that step from the recon / heavy / flyer pool the
code already named, with the reasoning in DECISIONS.md.
Recon is a fragile long-range screen: 2 members, 20 HP
each (40 total), 6 damage per living member (12 at full
strength), engagement range 5 — against Vanguard's 3
members, 30 HP each (90), 10 damage each (30), range 3.
It is better than Vanguard in exactly one dimension
(reach) and worse in the other two (health, damage), so
it is a choice, not a replacement: it fires from cells
Vanguard cannot reach, but it cannot hold ground under
fire the way Vanguard can. Movement is the shared rule
(one voxel per tick, largest-gap axis, solid and
enemy-occupied cells refused) for both kinds, and both
kinds work under both doctrines with the same activity
words (Marching / Holding / Hunting / Idle).

- Stats live in a per-kind shape (`SquadShape` /
  `SquadShapes.For` in `Orbital.cs`), read from the
  squad's kind by every rule — movement, fire, snapshot,
  and load — instead of a second set of shared constants
  or a branch in each rule. Vanguard's numbers are
  unchanged by the refactor.
- Price: 10 EmberOre for both kinds, cap 3 live squads
  in total (not 3 per kind), so the prompt's COST 10 and
  MAX 3 lines stay true with no display change.
- Binding: keys 3 / 4 select kind independently of
  doctrine (see above); keys 1 / 2 no longer force
  Vanguard. All four doctrine-kind combinations deploy.
- Save: no new field — kind was already index 8 of the
  saved squad array. The load clamp is now per-kind
  (Vanguard 0..90, Recon 0..40); a Recon squad is never
  trimmed to Vanguard's health, or vice versa, on load.
  Malformed and unknown kinds are still skipped, never
  fatal.
- Targeting: kind-blind, unchanged — `ChooseTarget`
  picks the nearest live target regardless of kind, so a
  Recon screen draws fire exactly as a Vanguard would
  from the same cell, and dies faster for it.
- Markers: unchanged — the same cyan squad cube for
  both kinds, kind readable from the text line, per the
  Slice 6 one-marker-per-squad discipline. A distinct
  marker shape is not added on a hypothetical confusion;
  it would need a play-observed reason.
- Adapter code is written, NOT compiled or run in Unity
  on Windows: see checklist item 10 below.

## Deliberately NOT in this step

- No extra doctrines or squad types (still HoldGround only,
  Vanguard only); the selection API exists so adding them does
  not reshape the picker.
- No 3D squad markers, rally marker in the scene, or Orbital
  camera — the rally is shown as text coordinates only.
- No deployment cost or squad cap beyond the 0.18.0 economy
  (see above): still no refund, no recall, no second priced
  unit.
- No enemy AI targeting squads (unchanged from 0.9.0/0.10.0).
- No Slice 3 scope: with the picker done, Slice 2's three
  review follow-ups (demo scenario, in-place restore, picker)
  are all complete, but Slice 3 is scoped by a later step, not
  by this one.

## Unity verification checklist — to be done on Windows, Unity 6000

1. [ ] Project opens with no compile errors in the edited
   `FpsInputAdapter.cs`, `OrbitalSquadDisplay.cs`, and
   `VoxelMeshRenderer.cs` (including its new MeshCollider).
2. [ ] With no squads, the display reads
   `ORBITAL: NO SQUADS (press O to pick a rally)`.
3. [ ] Press O: the picking prompt appears; Esc cancels it
   with no squad deployed.
4. [ ] Press O, aim the crosshair at a voxel face, Enter: one
   squad deploys, marches to the rally, and the prompt clears.
5. [ ] Press O, aim nowhere hittable (open sky), Enter: no
   squad deploys and the picker stays open.
6. [ ] While picking, a mouse click does not fire the weapon.
7. [ ] Save, close, reopen, load: a picked squad returns
   (persistence is headless-tested in 0.10.0, restore in
   0.14.0; this step checks the Unity path shows it).
8. [ ] Advance, added 0.22.0 (adapter code written, NOT yet
   compiled or run in Unity on Windows): press O, press 2,
   aim, Enter — the prompt shows VANGUARD ADVANCE, the squad
   deploys for 10 EmberOre, and it hunts the nearest enemy
   instead of marching to the picked rally; with no enemy it
   stands still. Do not claim this verified in Unity until
   this item is completed there and recorded in DECISIONS.md.
9. [ ] FPS pitch, added 0.28.0 (adapter code written, NOT yet
   compiled or run in Unity on Windows; the core Look math
   is headless-tested, the adapter rendering is NOT):
   moving the mouse up looks up and moving it down looks
   down (if the sign is inverted in Unity, flip the Mouse Y
   sign in the adapter call, not the core convention);
   looking up then down stops at the clamp and never flips
   the camera past vertical; press O and look down at flat
   ground ahead — the pending rally becomes the ground cell
   under the crosshair, not the face beyond it; gather (E),
   build (Q), and fire (click) still act on their Slice 1
   cells while pitched; and movement stays world-aligned
   while yawed. Do not claim this verified in Unity until
   this item is completed there and recorded in DECISIONS.md.
10. [ ] Second kind, added 0.33.0 (adapter code written, NOT
    yet compiled or run in Unity on Windows; the core kind
    rules are headless-tested, the adapter binding is NOT):
    press O, press 4 — the prompt shows RECON with the
    current doctrine; press 1 — the prompt shows RECON
    HOLDGROUND, proving doctrine changed without changing
    kind; press 2, then 3 — the prompt shows VANGUARD
    ADVANCE; Enter deploys the shown combination for 10
    EmberOre; a deployed Recon squad's text line reads its
    kind, members, health, and damage, and its scene
    marker is the same cyan cube as a Vanguard's. Do not
    claim this verified in Unity until this item is
    completed there and recorded in DECISIONS.md.
11. [ ] Third kind, added 0.49.0 (adapter code written, NOT
    yet compiled or run in Unity on Windows; the core kind
    rules are headless-tested, the adapter binding is NOT):
    press O, press 5 — the prompt shows HEAVY with the
    current doctrine; press 1 — the prompt shows HEAVY
    HOLDGROUND, proving doctrine changed without changing
    kind; press 2, then 4 — the prompt shows RECON
    ADVANCE; Enter deploys the shown combination for 10
    EmberOre; a deployed Heavy squad's text line reads its
    kind, members, health, and damage, and its scene
    marker is the same cyan cube as a Vanguard's or a
    Recon's, by the stated Slice 11 Step 1 marker
    decision. Do not claim this verified in Unity until
    this item is completed there and recorded in
    DECISIONS.md.
12. [ ] Third doctrine, added 0.50.0 (adapter code written, NOT
    yet compiled or run in Unity on Windows; the core Defend
    Core rules are headless-tested, the adapter binding is NOT):
    press O, press 6 — the prompt shows DEFENDCORE with the
    current kind; press 3 — the prompt shows VANGUARD
    DEFENDCORE, proving kind changed without changing doctrine;
    Enter deploys the shown combination for 10 EmberOre; the
    deployed squad marches to the core (28,5,28), not to the
    picked rally, chases an attacker near the core, and reads
    Guarding at the core, Hunting while chasing, and Marching
    on the return. Do not claim this verified in Unity until
    this item is completed there and recorded in DECISIONS.md.

Record the result of each step, with Unity version and GPU, in
DECISIONS.md.
