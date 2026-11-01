# Forgeborne — Integration Step 5 Preparation: Main Scene Windows Checklist

**Status: PREPARATION ONLY, written 2026-10-31, tests 5061/5061
unchanged. This document is a sheet for Rob to follow on
Windows. It is NOT a scene, it does NOT claim a scene was
built, and it does NOT claim Play was pressed or observed.
Integration Step 5 itself — the Main scene, built and played
on Windows — is still owed, and no playtest-gate item is
marked passed by this document.**

## Why this preparation step exists, and what it is not

The CHANGELOG entry for 0.111.0 names Integration Step 5 as
the next step: the Main scene on Windows only, with the
0.110.0 GameManager links wired and Windows Play observation
of the Simulation fill still owed. A headless run cannot do
that step: this VM has no Unity, no Windows, and no way to
press Play, and a scene file (`.unity`) written by a
headless run and claimed as a built scene would be a fake,
not a step — the Slice 26 review says exactly that, and
this step follows it.

So instead of stalling the schedule on a step it cannot
perform, or faking the step, this run writes the one thing
a headless run CAN verify: the exact sheet for the Windows
run, checked field by field against the adapter code as it
stands at 0.111.0. If a field below does not match what
Rob sees in the Inspector, the code — not this sheet — is
the authority, and the mismatch is a finding to record,
not a sheet error to silently work around.

This sheet does not replace the playtest gate. The gate in
`docs/slice-01-playtest-gate.md` is where observations are
recorded and items are passed; this sheet is the
click-by-click route to the scene that gate step 2
describes, updated for everything that changed since that
step was written — above all the 0.110.0 Simulation fill,
the Input Handler rule, and the two wiring gaps found
while writing it (no scene file ships, no save button
ships; see below).

## Ground facts verified in this run, not copied from prose

Every field, default, and file claim below was re-read
from the adapter code or the project files in this run:

- No `.unity` scene file exists anywhere under
  `ForgeborneUnity/Assets/` — only `Plugins/` and
  `Scripts/`. The Main scene is therefore built from
  scratch on Windows, not opened and fixed.
- `ForgeborneUnity/ProjectSettings/` holds only
  `ProjectVersion.txt` (editor `6000.3.27f2`). There is
  no `ProjectSettings.asset` in the repo, so the Active
  Input Handler setting in Part A is a local setting:
  a fresh clone will show the input-backend dialog again,
  and the setting will need redoing. That is expected,
  not a regression.
- Packages, from `ForgeborneUnity/Packages/manifest.json`:
  Entities 1.5.0, Input System 1.11.0, UI 1.0.0, URP
  17.0.0, TextMeshPro 3.0.6. Entities 1.5.0 resolved on
  Windows in the 0.109.0/0.110.0 sequence; URP 17.0.0 is
  still a 6000.0-era pin and its asset is NOT yet
  confirmed — if the scene renders with no URP asset
  assigned, that is a finding to record (Part E), not a
  reason to change a package version inside this step.
- Core DLLs are already on remote from 0.107.0 (9 DLLs
  in `ForgeborneUnity/Assets/Plugins/`). No DLL change
  rides with this step, and a future DLL change travels
  by the website-upload route in the Plugins README,
  never by a snapshot push and never by copying core
  `.cs` files into `Assets/` — the 0.111.0 no-drift
  guard enforces that in every test run.
- `GameManager` fields, from `GameManager.cs`: `Seed`
  (default 42), `Simulation` (a pure C# field, filled
  in code, never Inspector-wired — see Part B),
  `MeshRenderer`, `FpsAdapter`, `HudDisplay`,
  `OrbitalDisplay`, `MarkerDisplay`. In `Start`,
  GameManager creates one `Simulation(Seed)`, hands
  that same object to every linked adapter, refreshes
  the mesh, fills a null `FpsAdapter.MeshRenderer`
  from its own linked renderer, and then spawns a wave
  of 3 enemies (`Simulation.World.SpawnWave(3)`).
  First Play therefore already has enemies in the
  world; that is the code working, not an accident.
- `FpsInputAdapter` fields: `Simulation`,
  `MeshRenderer`, `MouseSensitivity` (default 2). The
  adapter drives its own GameObject's transform
  (position = FPS position + 1.6 eye height), so it
  belongs on the camera object. All of its input is
  legacy Unity input: 37 `Input.GetKey` /
  `Input.GetAxis` / `Input.GetMouseButton` call sites
  on 35 lines were counted in this run, including WASD, Space,
  mouse axes, E / Q / T / Y / U / I / B / N / P / M,
  O, Return, Escape, Alpha1-7, K, X, and mouse click.
  That count is why Part A insists on Both, not
  New-only: under a new-input-only backend, none of
  those calls fire.
- `VoxelMeshRenderer` fields: `Simulation`,
  `ChunkMaterial` (optional). It carries
  `[RequireComponent(typeof(MeshFilter),
  typeof(MeshRenderer), typeof(MeshCollider))]`, so
  adding the script adds those three components
  itself. If `ChunkMaterial` is left unwired, `Awake`
  assigns a runtime material from URP Lit, falling
  back to Standard, and logs a warning only if
  neither shader is found — so an unwired material
  is a valid first Play, and a pink chunk is a
  finding about the material, not a missing step.
- `HudDisplay` fields: `Simulation`, plus `Text`
  fields `HealthText`, `ShieldsText`,
  `ObjectiveText`, `AmmoText`, and the optional
  `PowerText`, `WorkstationPowerText`,
  `LivePowerText`, `PylonPowerText`, `WarningText`.
  An unwired Text field simply leaves that line
  blank; it does not break the HUD. The minimum for
  a first Play is the four core lines (Part C).
- Optional displays, verified but NOT required for a
  first Play: `OrbitalSquadDisplay` (`Simulation`,
  `SquadText`); `SquadMarkerDisplay` (`Simulation`,
  `MarkerRoot`); `WeaponViewModel` (`CameraAnchor`,
  wired to the camera object, `ViewOffset` default
  (0.22, -0.22, 0.60)); `DuskLighting` (`DuskLight`,
  `SkyboxMaterial`, first-pass values SunColor
  (1, 0.82, 0.60), SunIntensity 1.1, SunRotation
  (15, 235, 0), AmbientColor (0.32, 0.38, 0.46),
  FogColor (0.45, 0.52, 0.60), FogDensity 0.008 —
  the script creates its own directional light if
  `DuskLight` is unwired, so it never needs a light
  to be built by hand).
- Save/load gap, found while writing this sheet: no
  adapter wires a UI button, and no scene ships one.
  `GameManager.SaveToJson()` and
  `GameManager.LoadFromJson(string)` exist for UI
  buttons to call, but gate step 9 ("Save to JSON,
  close scene, reopen, load") therefore needs a
  button wired on Windows, or another route Rob
  chooses there. Record which route was used; do
  not assume a button exists, and do not claim
  step 9 from this sheet.

## Part A — Open the project, set input to Both (do this first)

Rob has never used Unity before, so these steps are
written one click at a time, in order. Do not skip
the input setting: it decides whether any key works.

1. On the Windows PC, make sure the local clone is
   on remote main head 0.111.0 (commit
   `70eaadc882f67e77cd09739e526bd48095487d04`).
   If the clone is behind, pull / fetch main first
   and confirm the head before opening Unity, so the
   scene is built against the 0.110.0 GameManager
   fill and not an older wiring.
2. Open Unity Hub, open the `ForgeborneUnity/`
   project with editor 6000.3.27f2. Wait for
   package resolution and compilation to finish
   before touching anything.
3. Read the Console. The last clean Windows compile
   (2026-10-31) showed 0 errors and 1 warning only
   (Input Manager deprecation). If this open shows
   errors, stop here and record them verbatim with
   the Unity version: a scene built under compile
   errors proves nothing, and a new error gets its
   own recorded fix, not a silent edit.
4. If a dialog asks about switching to the new
   input backend, click No. The adapter does not
   use the new Input System at all (see the 35
   legacy call sites above); saying Yes would
   silence every key.
5. Open Edit > Project Settings > Player, find
   Active Input Handler, and set it to Both
   (legacy + new), not Input System Package (New)
   only. Then restart the editor when prompted, or
   close and reopen the project if it does not
   prompt. If the dialog from step 4 appears again
   after the restart, click No again — the two
   settings together are the working combination,
   and the dialog reappearing on a fresh clone is
   the local-settings caveat above, not a failure.
6. Confirm the setting stuck: reopen Player
   Settings and read Active Input Handler = Both
   before building the scene. A scene tested under
   New-only would fail every key for a settings
   reason, and that failure would masquerade as a
   code bug.

## Part B — Create the scene and wire GameManager (the core step)

Build the minimum scene first. Optional displays
are Part D; do not let them block a first Play.

1. File > New Scene. Save it as `Main` (a suggested
   name for the scene file, not a shipped file —
   nothing in the repo names the scene yet, and the
   name is Rob's to confirm when it is saved).
2. Create an empty GameObject, name it
   `GameManager`, and add the `GameManager` script
   to it (Add Component > GameManager).
3. Check the `Seed` field reads 42. That is the
   default and the seed the gate and the headless
   demo use; leave it at 42 for the first Play so
   observations compare with the headless record.
4. Create a second empty GameObject, name it
   `Chunk`, and add the `VoxelMeshRenderer` script
   to it. The MeshFilter, MeshRenderer, and
   MeshCollider arrive with it; do not add them by
   hand. Leave `ChunkMaterial` unwired for the
   first Play (the runtime URP Lit / Standard
   fallback applies); a deliberately built
   vertex-colour material is a later choice, and
   gate items 26 and 30 are the observations that
   settle whether tints show and which shader was
   actually assigned.
5. Select the Main Camera, and add the
   `FpsInputAdapter` script to it. Leave
   `MouseSensitivity` at 2 for the first Play.
   Leave the adapter's `MeshRenderer` field unwired
   or wire it to `Chunk` — either is a valid first
   Play, and the difference is itself an
   observation: GameManager.Start fills a null
   adapter field from its own linked renderer
   (gate item 31 observes exactly this), while a
   wired field is never overwritten.
6. Create a Canvas (GameObject > UI > Canvas) and
   add the `HudDisplay` script to it. Wire its
   four core Text fields as in Part C.
7. Now wire GameManager itself — this is the step
   0.110.0 exists for, and the step the old gate
   text did not spell out. On the `GameManager`
   object, drag: `MeshRenderer` to the `Chunk`
   object, `FpsAdapter` to the camera's
   `FpsInputAdapter`, `HudDisplay` to the Canvas
   `HudDisplay`. Leave `OrbitalDisplay` and
   `MarkerDisplay` unwired unless those displays
   are in the scene (Part D) — unwired, they log
   nothing, because they are later-slice displays
   that may legitimately be absent from a Slice 1
   scene.
8. Do NOT try to drag anything into any
   `Simulation` field. `Simulation` is a pure C#
   class, not a Unity Object, so the Inspector
   cannot hold it. GameManager.Start creates the
   one Simulation and hands it to every adapter
   linked above. A `Simulation` field left empty
   in the Inspector before Play is correct, not a
   missing wire — and a scene that tries to work
   around it by other means is building a second
   wiring the code does not need.
9. Save the scene before pressing Play. An unsaved
   scene that crashes or is closed loses the
   wiring, and the next run would re-litigate
   which links were actually made.

## Part C — HUD Text wiring (minimum for a first Play)

On the Canvas, create one UI Text object per line
below and drag it into the matching `HudDisplay`
field. The four core lines are the minimum; the
rest are optional and unwired lines stay blank.

- `HealthText` — reads `HEALTH <n> HP`.
- `ShieldsText` — reads `SHIELDS: <n>`.
- `ObjectiveText` — reads the objective name,
  distance in metres, and percent completed.
- `AmmoText` — reads weapon name and
  magazine / reserve ammo.
- Optional, wire only if the line is wanted:
  `PowerText` (powered / total mills),
  `WorkstationPowerText`,
  `LivePowerText`, `PylonPowerText`,
  `WarningText` (empty while nothing warns —
  a permanent "0 warned" line would be noise,
  so blank here means safe, not broken).

Positioning is Rob's choice on Windows; no HUD
layout ships in the repo and none is prescribed
here, because a layout claim made headlessly
would be invented. Record the layout only as
observed, if an observation turns on it.

## Part D — Optional second pass (do not block a first Play)

Add these only after the minimum scene plays, or
in the same scene if they are already understood
— each is optional, and each has its own gate
observation, so none of them gates the first Play.

- `OrbitalSquadDisplay`: add to a GameObject,
  wire `SquadText` to a Text line, and link it as
  GameManager's `OrbitalDisplay` if it is in the
  scene. Without the link, its `Simulation` stays
  null and its Update returns early — that is the
  0.110.0 gap in miniature, and the fix is the
  link, not a code change.
- `SquadMarkerDisplay`: add to a GameObject,
  wire `MarkerRoot` to an empty parent transform
  if marker parenting is wanted (the unparented
  marker is a carried Slice 24 gap, not a
  first-Play blocker), and link it as
  GameManager's `MarkerDisplay` if it is in the
  scene. Markers are ordinary opaque primitives
  (cyan cube Vanguard, magenta capsule Recon,
  light-grey cylinder Heavy, orange flat-cube
  rallies, orange pending sphere); gate item 32
  is the occlusion observation for them.
- `WeaponViewModel`: add to the camera object,
  wire `CameraAnchor` to the camera's transform.
  Unwired, no model is built and a warning logs
  — that warning is the signal, not a failure of
  the scene. Its first-pass offset (0.22, -0.22,
  0.60) and its dark-metal / cyan / orange
  materials are first-pass values; gate item 34
  and the graphics retune candidate are the
  observations that judge them, and no retune is
  prescribed here.
- `DuskLighting`: add to any GameObject (no
  GameManager field is needed). It creates its
  own directional light if `DuskLight` is
  unwired, so no light needs building by hand.
  Its first-pass dusk values are listed in the
  ground facts above; gate item 33 is the
  dusk-light observation, and no retune is
  prescribed here without a Windows observation.
- URP asset: if the project has no URP asset
  assigned (Renderer settings empty, or the
  scene renders without URP), record that as the
  finding. Do not invent a URP asset headlessly
  and do not change a package version inside
  this step; the asset question gets its own
  recorded step with the Windows evidence.

## Part E — Press Play: what to observe, in order

Press Play and work through these readings in
order. The order matters: the first three readings
are the 0.110.0 fill diagnostic, and they must be
separated, because a scene can pass one and fail
the next, and the failure tells a different story
each way.

1. The chunk renders: a voxel chunk is visible,
   with no pink / missing material on it. If it
   is pink, record the material finding (which
   shader, if any, the fallback found) — do not
   rewire the material mid-observation and then
   report the rewired scene as the first result.
2. The FPS camera moves: WASD moves, mouse
   looks, Space jumps. If the chunk renders but
   the camera does not move, the first suspect
   is the GameManager `FpsAdapter` link or the
   Part A Both setting — and the Console should
   carry the missing-link warning if the link is
   the cause. A static camera with a rendered
   mesh is exactly the silent failure 0.110.0
   fixed in code; on Windows it now means a
   scene link or a settings cause, and the
   Console warning names which.
3. The HUD updates: health, shields, objective,
   and ammo lines show live values, not blanks.
   If the camera moves but the HUD stays blank,
   the first suspect is the GameManager
   `HudDisplay` link (same warning pattern as
   reading 2). Camera moving + HUD updating
   together is the positive observation of the
   0.110.0 fill: one Simulation reached both
   adapters. Record it as that, with the Unity
   version and GPU — it is not proved by this
   sheet, only by that Play.
4. Gather and build: press E near ground (gather
   succeeds, inventory increases), press Q with
   2+ Stone (wall builds in front-right and
   blocks enemy movement). Each success should
   appear in the scene on its tick; a success
   that counts but does not appear is a mesh
   refresh finding (gate items 27 and 31), not a
   gather/build rule finding.
5. Workstation and survival: build a workstation
   (10 Stone + 5 EmberOre), stand near it
   (shields restore 5/tick, capped at 85);
   survive a wave (enemies approach, attack, and
   can be shot — click, 3 shots = 1 kill at 10
   damage vs 30 HP). Remember Start already
   spawned 3 enemies, so a wave is present from
   the first Play; do not report their presence
   as a surprise spawn.
6. Objective, save, frame time: core at
   (28, 5, 28), 100 HP, HUD distance and percent
   (gate step 8); save/load by whichever route
   was wired on Windows (see the save gap in the
   ground facts — record the route, then the
   round-trip result); FPS with chunk + wave
   active, recorded as the actual number, with
   the 60 FPS blueprint target noted as a target,
   not a Slice 1 fail threshold.
7. Console, at the end: copy every warning and
   error verbatim. The expected clean case is 0
   errors and the Input Manager deprecation
   warning only; anything else is a finding, and
   a missing-link warning (FpsAdapter or
   HudDisplay) is a wiring finding, not noise
   to clear without recording which link was
   missing.

## Part F — Recording: what this sheet does and does not pass

- This sheet passes NOTHING. Gate items pass
  only when their Windows observations are made
  and recorded in DECISIONS.md with the Unity
  version (6000.3.27f2, plus revision hash as
  Unity reports it) and the GPU. That rule is
  the same rule every slice has followed, and
  this sheet does not weaken it for convenience.
- What to record after the Windows run, in
  DECISIONS.md: the scene file name as saved,
  every GameManager link as wired, the Active
  Input Handler reading, the Console log
  verbatim, the FPS number, each gate reading
  from Part E as pass / fail / not attempted,
  and any mismatch between this sheet and the
  Inspector (a mismatched field is a finding
  about the code or the sheet, to be fixed in
  its own step).
- If Play fails at any reading, record the
  reading number, the Console text, and the
  links as wired, and stop the observation
  there: a later reading attempted on a broken
  earlier reading proves nothing about the later
  system, and a fix for the earlier reading gets
  its own step.

## Deliberately NOT in this step

- A `.unity` scene file, an imported asset, a
  texture, a shader file, or a URP asset written
  by a headless run. Scene assembly remains
  Windows checklist work, as the Slice 26 review
  states.
- A claim that Play was pressed, that the camera
  moved, that the HUD updated, or that any gate
  item passed. All of those are Windows
  observations, owed, not made here.
- A graphics retune (dusk azimuth, fog
  contrast, view model offset, static
  weapon-agnostic model), a re-rally price, or a
  dismiss refund. Each is gated on Windows
  playtest evidence that does not exist yet.
- A new playtest-gate item. The Main scene Play
  observation already has its home in gate step
  2 and in items 26–44; the next free gate
  number remains 45, and no item is added here
  for a reading that already has one.
- A core, adapter, save-format, economy,
  movement, targeting, fire, marker, material,
  light, fog, model, or HUD rule change. No code
  file moves in this step, by design.
- A Slice 27 scope. Slice 27 is NOT scoped here;
  its ranked candidates remain those the Slice
  26 review carries, and the Main scene comes
  before any Slice 27 scoping decision.

## Next step

Integration Step 5 itself, on Windows only: build
the Main scene by this sheet, press Play, observe
Part E in order, and record the result in
DECISIONS.md with the Unity version and GPU.
Until that run happens, the Slice 1 director
playtest gate stays pending, as it has through
every slice, and this preparation step is not a
substitute for it.
