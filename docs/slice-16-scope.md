# Forgeborne — Slice 16 Scope

**Status: SCOPED 2026-10-30, under delegation and under
the director queue decision of 2026-10-30 09:27 EDT.
No Slice 16 system is built by this document. Each step
below is built one at a time, with tests where a step
has a headless-testable claim and with Windows-checklist
items where it does not, in this order, and Slice 16 is
closed by its own review — it is not closed by
finishing step 1 alone.**

## Scope question

Slice 15 closed the deferred doctrine Slice 4 and Slice 11
left open: a game with exactly three doctrines now has
exactly four, the fourth proved walking a route and
engaging on it beside an existing doctrine through a
save/load round-trip. What Slice 15 deliberately did
not touch is the scene itself. The core has, since
Slice 13 Step 3, carried a vertex-colour channel on
every chunk quad — Powered green, Mismatched red,
Energised yellow, everything else white — and the
adapter has, since the same step, assigned those
colours to a Unity mesh. But no step has yet settled
the scene-proof layer the director ordered after
Slice 15: whether the chunk material actually
multiplies those vertex colours, whether the refresh
wiring is the same renderer or a silent no-op, whether
markers read through the scene's occlusion at FPS
distance, what light, sky, and fog the scene renders
under, and what the player's weapon looks like in the
lower-right frame. Slice 16 is therefore the graphics
slice, kept as small as the slices before it: **the
scene-proof layer first, then its own review** — not
a display-query redesign, not a new doctrine, kind,
source, or consumer, not a balance pass, not a HUD
redesign, and not a claim of Unity verification made
from a headless run.

## Inputs this scope is decided from

- The director queue decision (Rob, 2026-10-30 09:27
  EDT, in chat — not an Ember decision): graphics is
  the next slice after Slice 15 closes, ahead of the
  previously ranked Slice 16 candidates, which are
  carried after graphics unless a later director
  decision reorders them. Graphics as directed is the
  scene-proof layer: vertex-colour URP material and
  MeshRenderer wiring, marker occlusion, lighting /
  sky / fog to `docs/art-direction.md`, and a first
  FPS weapon view model script — adapter-only where
  Unity is concerned, with Windows checklist items,
  following the Slice 13 pattern. This scope obeys
  that order; it does not re-rank it, and it does
  not reorder the carried remainders among
  themselves.
- The Slice 15 review's gaps, in priority order, and
  what this scope does with each: Unity verification
  (gap 1) and the director playtest (gap 2) remain
  open, but neither is Slice 16 work in itself — the
  first is blocked on Unity 6000 on Windows, the
  second on the director — and scoping a slice does
  not close, reorder away, or pretend to answer
  either. Doctrine balance (gap 3) is not a slice:
  any value a graphics step chooses is a first-pass
  value for determinism and testability, exactly as
  Defend Core's chase radius 8 was, and must be
  labelled as such, not quoted as final feel. The
  scene proof for the visibility adapters (gap 4:
  whether the chunk material multiplies vertex
  colours, whether `FpsInputAdapter.MeshRenderer` is
  wired to the same renderer GameManager links, and
  whether markers read through occlusion) is Slice
  16's work, taken here as its steps 1 and 2 — but
  taken as proof-layer work, not as closed
  observations: a headless run cannot close an
  observation about a scene it cannot run, so the
  steps below build the material, the wiring, and
  the occlusion answer, and the Windows checklist
  items they add are what close, or refute, them on
  Windows. The single-cable tint finding (gap 5: a
  cable cell whose only neighbours are a consumer
  and a Conduit reads None in PowerState while its
  network is live) and the direct-Conduit count
  question (gap 6: a Conduit beside a mill counts
  as neither a live network nor a lone source) are
  display-query decisions, each with its own tests,
  and are carried after graphics, not patched inside
  a graphics step — and there is a reason beyond
  the director's order for that sequence: deciding
  what tint a cable cell should carry before the
  material that renders tints is settled would prove
  a tint against an unsettled renderer, so the
  material step comes first and the query steps
  follow it. The patrol route's stated limits (gap
  7: a refused step skips its waypoint, a fully
  enclosed patrol cycles without moving) and the
  free deploy primitive (gap 8: headless-only by
  rule, not by enforcement) are re-named, not
  quietly fixed here.
- Why graphics before the remainders, stated as a
  decision rather than a ranking habit: the
  remainders are questions about how existing rules
  read, and the scene-proof layer is the surface
  they read on. A tint-query step decided before
  the material exists could pass every headless
  test and still render wrong in the scene, and a
  count step decided before marker occlusion is
  answered could be proved against a scene the
  player cannot read. Graphics first does not make
  the remainders smaller — their checklist lines
  (item 28's honest single-cable observation, and
  the Slice 12 count definitions) already record
  them honestly — but it gives their later steps a
  settled surface to prove against.
- Ground facts this scope is decided from, verified
  this run against the code as it stands:
  `MeshBuilder` carries one RGBA colour per vertex
  in the same order as the vertices, every vertex
  of a quad sharing that quad's `PowerTint` colour,
  untinted quads white (1, 1, 1, 1), so a
  vertex-colour material leaves ordinary stone, ore,
  and wood exactly as before; `VoxelMeshRenderer`
  assigns those colours to `mesh.colors` and states
  openly that they show only if the chunk material
  multiplies vertex colours, with playtest-gate
  item 26 as the observation that settles which
  material the scene actually uses;
  `FpsInputAdapter.MeshRenderer` is a public field
  the scene must wire to the same
  `VoxelMeshRenderer` GameManager links, with
  playtest-gate item 27 as the observation that a
  null field makes every refresh a no-op;
  `SquadMarkerDisplay` renders kind markers as
  ordinary opaque URP Lit primitives (Standard
  fallback) that do NOT show through walls, by
  decision, with playtest-gate item 25 as the
  observation that occlusion is still honest;
  `docs/art-direction.md` rules 1 and 6 set the
  lighting target (dark greys and blacks, storm
  blue-grey sky at dusk, neon cyan primary, orange
  secondary, readable in daylight dusk, not
  horror-dark) and rule 3 sets the weapon target
  (lower-right frame, readable digital sight,
  energy weapons glow, kinetic weapons worn and
  industrial, not pristine); no lighting, sky, fog,
  or weapon view model script exists in the Unity
  adapters today, and no scene file is written by
  a headless run — scene assembly is Windows
  checklist work, not a pushed file. The core
  `Weapon` (Pulse Rifle, 42 mag / 120 reserve, 10
  damage) and `HudState` (weapon name, mag,
  reserve) already exist, so a view model script
  renders core state, never a second ammo rule.
  The next free playtest-gate checklist number is
  30 (items run to 29, the Patrol key).

## Slice 16, in order

### Step 1 — Vertex-colour chunk material and MeshRenderer wiring

Build the material half of the scene-proof layer,
decided in that step — not in this scope document —
with the reasoning recorded in DECISIONS.md:

- The step must decide the material, and record it:
  which URP material the chunk mesh renders with,
  how it is assigned (a small adapter script that
  assigns it at Start/Awake, an Inspector-wired
  material on the existing renderer, or a stated
  combination), and what the fallback is when the
  named shader is missing — the Slice 13 pattern of
  a Standard fallback is a starting hypothesis,
  not a decision. The step must say, precisely,
  whether the chosen material multiplies vertex
  colours, and must not claim the tint observation
  from the choice: playtest-gate item 26 remains
  the observation that settles which material the
  scene actually uses, and this step adds a new
  checklist item (from 30) for the assignment
  itself, not a duplicate of item 26. If the step
  finds that no material can be assigned without a
  scene file a headless run cannot write, it must
  say so and write the assignment as scene-wiring
  notes plus a checklist item, rather than
  pretending a script alone assigned it.
- The step must decide the wiring, and record it:
  whether `FpsInputAdapter.MeshRenderer` stays an
  Inspector-wired field verified by playtest-gate
  item 27, or becomes code-driven (for example
  GameManager assigning it to the same
  `VoxelMeshRenderer` it already links, in Start)
  so an unwired scene cannot silently no-op every
  refresh. A code-driven wiring is a starting
  hypothesis, not a decision: the step must weigh
  it against the existing pattern, in which every
  adapter field is Inspector-wired and the
  checklist is the verification, and must say which
  failure it prevents — a stale mesh after a
  successful build, gather, or destruction — and
  which new failure it could introduce, such as
  overwriting a deliberate second renderer. The
  step must add a checklist item (from 30) for
  whatever wiring it chooses, and must not close
  item 27 by writing code: item 27's observation
  (each builder key, gather, and destruction
  appearing on its tick) is what proves the wiring,
  on Windows.
- The step must decide what does not move, and
  record it: no `MeshBuilder` colour, no
  `PowerTint` value, no `PowerState` query, and no
  refresh event set moves in this step. A material
  step that re-tints a cell to make the material
  look right would move the display the tint query
  steps are proved against, and a wiring step that
  refreshes every frame would spend the Slice 1
  budget the Slice 13 event set was built to
  protect. The refresh event set (successful key,
  gather, destruction, load, Start, and nothing
  else) is reused, not rewritten.
- The step must state its verification honestly,
  and record it: Unity adapter code is not compiled
  headlessly, so this step adds no headless test
  and no demo scenario for the material or the
  wiring — the Slice 13 Step 2 precedent — and the
  test suite must stand unchanged at 2130/2130,
  with every earlier demo save length unchanged.
  Cited headless evidence is only the existing
  colour-channel coverage (MeshBuilder colours
  parallel to vertices, untinted white) and the
  existing refresh-event coverage, proved by
  earlier slices' tests.

### Step 2 — Marker occlusion under the new scene

Decide the occlusion answer, decided in that step —
not in this scope document — with the reasoning
recorded in DECISIONS.md:

- The step must decide whether honest depth
  occlusion stands, and record it: the Slice 13
  Step 2 decision is that markers are ordinary
  opaque primitives and the voxel mesh occludes
  them honestly by depth, and a through-wall marker
  would be a wallhack claim, not a readability aid.
  The step must either reaffirm that decision under
  the Step 1 material and the Step 3 lighting, with
  a checklist item (from 30) re-observing it —
  walk behind a wall so a marker is on the far
  side and observe it hidden, at FPS distance, in
  the new light — or change it, with a stated,
  non-wallhack aid (for example a size, brightness,
  or outline change that still hides behind a wall)
  and the evidence that honest occlusion makes
  markers useless at FPS distance. A silent shader
  change that lets markers show through walls is
  not an option in this step: if Windows testing
  proves occlusion makes markers useless, that
  evidence is recorded and the aid is decided
  openly, not smuggled in as a material tweak.
- The step must decide the readability question
  the occlusion answer depends on, and record it:
  at what FPS distance, against what background
  (dark voxel, dusk sky, tinted machines), each
  kind marker (cyan cube 0.6, magenta capsule 0.5
  x 0.8, light-grey cylinder 0.8 x 0.7), each rally
  marker (orange flat cube), and the pending marker
  (orange sphere) still reads as itself and not as
  another marker or as a tint. Tint colours are
  part of this question: Powered green must not
  read as the cyan squad cube, Mismatched red must
  not read as the orange rally marker, and
  Energised yellow must not read as a floating
  marker — the Step 3 lighting must preserve those
  separations, and this step's checklist item must
  observe them together, not one marker at a time.
- The step must state its verification honestly,
  and record it: like Step 1, this step adds no
  headless test and no demo scenario for occlusion
  — a printed snapshot line does not prove a
  visible marker, and a headless run cannot see
  through a wall it cannot render. The existing
  item 25 observation is not duplicated blindly:
  if this step's new item re-observes occlusion,
  it must say what is new about the observation
  (the Step 1 material, the Step 3 light, or both)
  and whether item 25 is superseded, extended, or
  left standing alongside it.

### Step 3 — Lighting, sky, and fog to art-direction

Build the scene's light, decided in that step — not
in this scope document — with the reasoning recorded
in DECISIONS.md:

- The step must decide the lighting setup, and
  record it: a directional dusk light, sky, and fog
  that match `docs/art-direction.md` rules 1 and 6
  — storm blue-grey sky at dusk, dark greys and
  blacks for voxel stone and metal, neon cyan as
  the primary energy colour and orange as the
  secondary accent, readable in daylight dusk, not
  clean, not cartoon, not horror-dark. The step
  must decide the form: a small adapter script
  that sets the light, sky, and fog values at
  Start (a starting hypothesis, following the
  adapter-only pattern), Inspector values recorded
  as scene-wiring notes with a checklist item, or
  a stated combination — and must say which values
  are first-pass (chosen for readability and
  testability, not final feel) and which are
  pinned by the art direction. No imported skybox,
  no copied skyline, no planet asset rides along:
  the sky is an original gradient or colour, not
  the reference image's planet, and the city, squad,
  and tank art of the reference remain later
  slices, not this one.
- The step must decide the readability contract,
  and record it: under the new light, voxel edges
  must still read at FPS distance (art-direction
  rule 2: large, chunky, clear edges, not smooth
  photoreal terrain), the three tints must still
  separate from each other and from the markers
  (Step 2's question, answered here in light), the
  HUD colours must still read against the scene
  (cyan health and shields, orange objective), and
  smoke or fire atmosphere, if any is added, must
  never make targets or voxel geometry unreadable
  (rule 6). A light that flatters the sky while
  washing out a red mismatch tint has failed its
  contract, and the step must say so rather than
  quoting the sky as the result.
- The step must decide what does not move, and
  record it: no HUD layout, no HUD colour, no
  marker colour or shape, and no tint value moves
  in this step. If the light makes a colour unread,
  the step changes the light or records the colour
  as evidence to revisit — it does not quietly
  re-palette the game to fit the light, because
  the palette is art-direction law and the light
  is the new variable.
- The step must state its verification honestly,
  and record it: like Steps 1 and 2, this step adds
  no headless test and no demo scenario for light,
  sky, or fog, and adds a checklist item (from 30)
  observing the scene under the new light — voxel
  edges, tint separation, marker separation, HUD
  readability, and the dusk mood — on Windows, in
  Unity 6000, with the version and GPU recorded in
  DECISIONS.md when observed. The test suite must
  stand unchanged, with every earlier demo save
  length unchanged.

### Step 4 — First FPS weapon view model script

Build the first weapon view model, decided in that
step — not in this scope document — with the
reasoning recorded in DECISIONS.md:

- The step must decide the model, and record it:
  a simple, original view model in the lower-right
  frame (art-direction rule 3), built from original
  primitives, not imported art and not the
  reference rifle copied — worn dark metal with a
  cyan energy-glow accent, a readable digital sight
  in the art-direction's spirit, not its
  pixel-for-pixel layout. The step must decide the
  form: a script that builds the model from
  primitives at Start and parents it to the camera
  (a starting hypothesis, following the marker
  pattern of original primitives built in code),
  Inspector-wired parts, or a stated combination —
  and must decide the first-pass scope: a static
  model that reads in the frame is the starting
  hypothesis, and any sway, bob, or recoil is a
  second decision the step must either include with
  a reason or defer openly, not smuggle in as
  polish. A view model that obstructs the
  crosshair, the rally picker, or the objective
  readout has failed, however good it looks.
- The step must decide the data it renders, and
  record it: the view model renders core state
  only — the equipped `Weapon` through `HudState`
  (name, mag, reserve), read the way `HudDisplay`
  already reads it — and holds no ammo, damage, or
  reload rule of its own. Fire (mouse click) and
  reload (R) already act in core through
  `FpsInputAdapter`; the view model must show
  their effect (mag falling, reload restoring it)
  through the same snapshot the HUD shows, so the
  model and the HUD ammo line can never contradict
  each other. A view model with its own ammo count
  is a second weapon, not a view, and is not an
  option in this step. The step must also decide
  whether the model changes with the equipped
  weapon (a generated weapon's name and mag size
  are core state, and the model must either reflect
  them or say plainly that the first model is
  weapon-agnostic) — the Slice 1 art scope's "one
  FPS weapon view model (simple, original)" is the
  starting point, not a promise of a model per
  weapon.
- The step must decide the adapter line, and
  record it: how the model is parented to the
  camera the adapter already moves
  (`FpsInputAdapter` sets the camera from the core
  `Look` forward vector), how it is wired in the
  scene (Inspector field, script-built, or stated
  combination), and a checklist item (from 30)
  observing it on Windows — lower-right frame,
  sight readable, no crosshair or picker
  obstruction, mag falling on fire and restoring
  on reload in step with the HUD ammo line, and
  the model surviving a save/load round-trip
  without duplicating or resetting ammo.
- The step must state its verification honestly,
  and record it: like Steps 1–3, this step adds no
  headless test and no demo scenario for the model's
  look — a printed ammo line does not prove a
  visible model — and the test suite must stand
  unchanged, with every earlier demo save length
  unchanged. If the step finds a headless-testable
  claim (for example a core query the model needs
  that does not yet exist), it must decide that
  query as its own stated addition with its own
  tests, not as a silent core change inside an
  adapter step.

### Step 5 — Slice 16 review

As with 0.34.0, 0.38.0, 0.43.0, 0.47.0, 0.51.0,
0.55.0, 0.60.0, 0.63.0, and 0.66.0, with one honest
difference stated here so it is not mistaken for an
omission: this review is documentation-only and is
NOT accompanied by a new composed demo scenario,
because no graphics claim in Steps 1–4 is
headless-provable — a demo scenario that printed
ammo numbers or tint colours would prove the core
numbers again, not the material, the wiring, the
occlusion, the light, or the model. The review
writes `docs/slice-16-review.md`, checks the slice
against this scope, states the verdict honestly
(scene-proof layer written as adapters plus
Windows checklist items, not playtested, not
Unity-verified, not a visual result), separates
headless-tested core claims from
Windows-checklist adapter claims, carries the gaps
that remain, and does NOT scope Slice 17 — Slice
17 is scoped afterwards, the same way this document
follows the Slice 15 review's queue decision
rather than preceding it. The full test suite is
re-run for the review, and every earlier demo save
length must stand; a changed save length in a
graphics slice would mean a core rule moved, and
the review must say so rather than quoting the
suite as unchanged.

## Deliberately NOT in Slice 16

- A second graphics system, a city, a planet, a
  squad or tank model, an enemy silhouette, or a
  mini-map, added alongside the scene-proof layer
  as a bonus. One scene-proof layer is Slice 16;
  the Slice 1 art scope's remaining pieces (enemy
  silhouette, mini-map beyond placeholder) are
  later slices, not bonuses here.
- A display-query redesign of the single-cable tint
  or the direct-Conduit count. Either is a carried
  candidate after graphics, with its own tests, not
  a patch inside a material step: a material step
  must not move the query it renders, and a query
  step must not move the material it is rendered
  by.
- A scene-proof observation presented as a headless
  result. Items 25, 26, and 27, and the new items
  Steps 1–4 add, are Windows observations, recorded
  in DECISIONS.md when observed, not closed by a
  document, a script, or a test count.
- A scene file (`.unity`), an imported asset, a
  texture, or a shader file written by a headless
  run and claimed as a built scene. Adapter scripts
  and scene-wiring notes are what a headless run
  can honestly produce; scene assembly is Windows
  checklist work, and the review must say which
  parts are script and which parts are still
  assembly to be done in Unity on Windows.
- A copied asset, model, skyline, HUD layout, or
  trademarked look from the reference image or from
  any other game. The reference is a mood and
  composition target; every model, material, and
  light value in Slice 16 is original, and the
  review must be able to say so piece by piece.
- A HUD redesign. HUD discipline (art-direction
  rule 4: health, shields, objective distance and
  completion, mini-map, ammo — no more) is law,
  not a graphics variable, and no new HUD line, no
  new HUD colour, and no quest list rides along
  with a light or a view model.
- A core change of any kind: no new block, kind,
  doctrine, source, or consumer, no movement,
  targeting, delivery, or save rule change, no
  balance pass, no price or cap change. Slice 16
  is adapter-only where Unity is concerned, and
  the core stands exactly as Slice 15 left it.
- Windows Unity verification itself. It remains open
  for Slices 1–16 alike and is blocked on Unity 6000
  on Windows, not on anything this slice can do
  headlessly. The checklists in
  `docs/slice-01-playtest-gate.md`,
  `docs/orbital-view.md`, `docs/orbital-command.md`,
  and `docs/powered-presence.md` remain the
  verification plan, and Slice 16 steps must add
  their own checklist items to those docs rather
  than claiming verification.

## Ranked candidates for Slice 17 scoping (not promised)

These are recorded here so the Slice 16 review starts
from a list, not from scratch. Order follows the
0.66.0 queue decision with graphics taken, and the
carried remainders keep their relative order:

1. Any Slice 14 remainder the Slice 15 review names
   (the single-cable tint query question in its gap
   5, and the direct-Conduit count question in its
   gap 6) — each a display-query decision step with
   its own tests, not a patch inside another step,
   and not promised here.
2. Any Slice 13 remainder the Slice 15 review names
   that Slice 16 does not itself settle (the
   vertex-colour material observation, the
   MeshRenderer wiring observation, and the
   marker-occlusion observation in its gap 4) — if
   Slice 16's steps build the proof layer but the
   Windows observations are still unmade when the
   review is written, they remain observations to be
   made when Windows verification runs, not steps
   to be re-scoped blindly, and not promised here.
3. The patrol route's stated limits and the free
   deploy primitive, carried from the Slice 15
   review's gaps 7 and 8 — a detour, hold, or stuck
   readout for Patrol, or an enforcement for the
   free deploy primitive, each as its own decision
   with its own tests, not promised here.

## Slice 16 gate (what "done" means)

Slice 16 is done when: every step above is pushed
with passing tests (or, for the adapter-only steps,
with the test suite unchanged for those steps and
the adapter-only verification stated, never inflated
into a test claim); the Step 1 material and wiring
decisions, the Step 2 occlusion decision, the Step 3
lighting, sky, and fog decisions, and the Step 4
view model decisions are each recorded with
rationale, each with its own Windows checklist item
from 30, with no `MeshBuilder` colour, no
`PowerTint` value, no `PowerState` query, no refresh
event set, no HUD layout or colour, and no core rule
changed, and every earlier demo save length
unchanged; and `docs/slice-16-review.md` records the
verdict honestly, separating headless-tested core
claims from Windows-checklist adapter claims,
including that Slice 16 is still not playtested and
not Unity-verified. The Slice 1 director playtest
gate is still pending throughout and is not closed
by Slice 16.
