# Forgeborne — Slice 16 Review

**Status: REVIEWED HEADLESSLY 2026-10-30, tests 2130/2130.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, and 15.
This review closes Slice 16 as a reviewed whole; it does not claim
Slice 16 is playtested, Unity-verified, or a visual result.**

## Review question

Does Slice 16 settle the scene-proof layer the director ordered
after Slice 15 — whether the chunk material actually multiplies
the vertex colours the core has carried since Slice 13 Step 3,
whether the MeshRenderer wiring is the same renderer or a silent
no-op, whether markers read through the scene's occlusion at FPS
distance, what light, sky, and fog the scene renders under, and
what the player's weapon looks like in the lower-right frame —
without a display-query redesign, a new doctrine, kind, source,
or consumer, a balance pass, a HUD redesign, a core change of
any kind, a scene file, an imported asset, or a claim of Unity
verification made from a headless run? The answer headlessly is
split, and this review keeps the split: yes for the layer as
written adapter code with its Windows checklist items, no for
the layer as an observed scene — because no headless run can
observe a scene it cannot run, and every Slice 16 observation
is still owed on Windows. That split is the verdict, not a
qualification added to it.

## One honest difference from earlier reviews

This review is documentation-only and is NOT accompanied by a
new composed demo scenario, by the scope's own terms. No
graphics claim in Steps 1–4 is headless-provable: a demo
scenario that printed ammo numbers or tint colours would prove
the core numbers again, not the material, the wiring, the
occlusion, the light, or the model. So this review re-runs the
full suite and the whole demo instead of adding a scenario
that would look like graphics evidence and prove nothing about
graphics. The demo save lengths below are therefore a
no-core-change check, not a graphics result, and they are
quoted as exactly that.

## What Slice 16 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.67.0 scope | `docs/slice-16-scope.md`: Slice 16 scoped as the graphics scene-proof layer, in order — material and wiring, occlusion, light / sky / fog, view model, then review. Ranked Slice 17 candidates recorded, not promised | Decision document only, no system built by it | Tests 2130/2130 unchanged |
| 0.68.0 Step 1 | Vertex-colour chunk material and MeshRenderer wiring: `VoxelMeshRenderer.Awake` assigns the chunk material (Inspector-wired `ChunkMaterial` wins; unwired, a runtime URP Lit material, Standard fallback, warning if neither shader is found); `GameManager.Start` null-fills a null `FpsInputAdapter.MeshRenderer` with the same renderer it already links, never overwriting a non-null field, warning if still unwired | Adapters only: no `MeshBuilder` colour, no `PowerTint` value, no `PowerState` query, no refresh event set, no core file, no save field | Tests 2130/2130 unchanged; demo unchanged, Slice 15 save 44,016 chars |
| 0.69.0 Step 2 | Marker occlusion decision, reaffirmed, not changed: markers stay ordinary opaque URP Lit primitives (Standard fallback) with default depth behaviour, so the Step 1 material changes the occluder but not the mechanism; no renderQueue, depth-test, or shader change, no marker colour, shape, size, or position move; no readability aid adopted, only named as candidates for a later step | Adapter comment only in `SquadMarkerDisplay.cs`, plus a note in `docs/orbital-view.md`; no core file, no shader file | Tests 2130/2130 unchanged; demo unchanged, Slice 15 save 44,016 chars |
| 0.70.0 Step 3 | Dusk light, sky spec, and fog: new adapter `DuskLighting.cs` sets a directional warm dusk sun (1.00, 0.82, 0.60, intensity 1.1, about 15 degrees, azimuth 235 first-pass, soft shadows), flat storm blue-grey ambient (0.32, 0.38, 0.46), and exponential fog (density 0.008, storm blue-grey 0.45, 0.52, 0.60), all as public Inspector fields; the sky is an editor-built gradient material to a recorded spec (zenith 0.20, 0.26, 0.34 to horizon 0.52, 0.54, 0.58), original only, no planet, no skyline, and an unwired sky material leaves the existing skybox untouched with a warning | Adapter only, plus a first-pass values note in `docs/art-direction.md` (rules 1–7 unchanged); no HUD, marker, tint, or core change, no scene or shader file | Tests 2130/2130 unchanged; demo unchanged, Slice 15 save 44,016 chars |
| 0.71.0 Step 4 | First FPS weapon view model: new adapter `WeaponViewModel.cs` builds a static, original model from primitives at Start and parents it to the camera object carrying `FpsInputAdapter` (offset 0.22, -0.22, 0.60), worn dark-metal receiver, barrel, stock, and grip, bright cyan energy strip and cyan sight dot, one small orange accent panel; colliders removed so the model cannot block the picker raycast; no number, no ammo, damage, or reload rule of its own; weapon-agnostic by decision; no sway, bob, or recoil | Adapter only, plus a first-pass model note in `docs/art-direction.md` (rule 3 unchanged); no `FpsInputAdapter` or `GameManager` edit, no camera change, no core file | Tests 2130/2130 unchanged; demo unchanged, Slice 15 save 44,016 chars |
| 0.72.0 Step 5 | This review, documentation-only | Review document plus CHANGELOG and DECISIONS entries; no code, no demo scenario, no checklist item added | Tests 2130/2130 re-run for the review; every earlier demo save length unchanged |

## What the review verified this run

- Full test suite re-run before writing: **2130 passed, 0 failed**
  (unchanged through the whole slice — no Slice 16 step added,
  removed, or changed a headless test, because no Slice 16 step
  makes a headless-testable claim about a visible scene, and
  the Slice 13 Step 2 precedent forbids inflating an adapter
  step into a test count).
- Headless demo re-run, save lengths as a no-core-change check
  only: Slice 1 unchanged (save 43,881 chars); Slice 2 unchanged
  (save 43,921 chars); Step 1 scenario unchanged (save 44,016
  chars); Step 3 scenario unchanged (save 44,103 chars); Slice 4
  scenario unchanged (save 44,026 chars); Slice 5 Step 2 scenario
  unchanged (save 43,974 chars); Slice 6 Step 4 scenario
  unchanged (save 44,024 chars); Slice 7 Step 2 scenario
  unchanged (save 44,013 chars); Slice 8 Step 3 scenario
  unchanged (save 43,974 chars); Slice 9 Step 4 scenario
  unchanged (save 43,975 chars); Slice 10 Step 3 scenario
  unchanged (save 43,974 chars); Slice 11 Step 3 scenario
  unchanged (save 44,038 chars); Slice 12 Step 3 scenario
  unchanged (save 43,974 chars); Slice 13 Step 4 scenario
  unchanged (save 44,045 chars); Slice 14 Step 2 scenario
  unchanged (save 43,974 chars); Slice 15 Step 2 scenario
  unchanged (save 44,016 chars). A changed save length in a
  graphics slice would have meant a core rule moved; none did.
- Slice 16 gate, checked against `docs/slice-16-scope.md`: every
  step pushed with passing tests (for the adapter-only steps,
  with the suite unchanged and the adapter-only verification
  stated, never inflated into a test claim); the Step 1
  material and wiring decisions, the Step 2 occlusion decision,
  the Step 3 lighting, sky, and fog decisions, and the Step 4
  view model decisions are each recorded with rationale in
  DECISIONS.md, each with its own Windows checklist item from
  30 (item 30 material assignment, item 31 wiring null-fill,
  item 32 combined occlusion and readability under the Step 1
  material, item 33 the same combined scene under dusk light,
  item 34 the view model), with no `MeshBuilder` colour, no
  `PowerTint` value, no `PowerState` query, no refresh event
  set, no HUD layout or colour, and no core rule changed; and
  this review records the verdict honestly, separating
  headless-tested core claims from Windows-checklist adapter
  claims. The Slice 1 director playtest gate is still pending
  throughout and is not closed by Slice 16.
- Checklist relations, stated so a later reader does not
  miscount them: item 26 (which material the scene actually
  uses, and whether tints show) and item 27 (each builder key,
  gather, and destruction appearing on its tick) are NOT closed
  by Steps 1–4 — items 30 and 31 observe the assignment and the
  fill beside them, not instead of them. Item 32 does NOT
  supersede item 25 (kind distinction and slot reuse) or
  orbital-view item 8 (baseline occlusion); it extends the
  occlusion question to the Step 1 material as occluder plus
  combined tint-and-marker readability. Item 33 does NOT
  supersede item 32; it re-observes item 32's whole combined
  scene under dusk light, as item 32 itself promised, and item
  32 stands as the pre-dusk observation. Item 34 does NOT claim
  a per-weapon model: the first model is weapon-agnostic by
  decision, so a generated weapon equipped is observed as a HUD
  line change with the model unchanged.
- One claim split stated plainly, so it is not over-claimed
  later: the headless evidence cited by Slice 16 steps is only
  the existing core coverage — `MeshBuilder` colours parallel
  to vertices with untinted quads white, the Slice 13 refresh
  event set (successful key, gather, destruction, load, Start,
  and nothing else), markers as ordinary opaque primitives
  with their own materials, the `DuskLighting` field values
  and fog arithmetic in its header, and the existing core
  weapon and HUD coverage (`HudDisplay`'s AmmoText reading
  name, mag, and reserve from `HudState`, fire and reload
  acting in core). None of that proves a material multiplies
  vertex colours, a renderer is wired, a marker hides behind a
  wall, a dusk sky reads, or a model sits in the lower-right
  frame. Those are adapter claims with Windows checklist items
  (30, 31, 32, 33, 34, alongside 25, 26, and 27), and this
  review adds no new checklist item, because the review
  exercises no adapter binding the checklist does not already
  cover — the next free playtest-gate number is 35.
- No Slice 16 step duplicates logic in a Unity adapter. Core
  owns the colour channel, the tint query, the refresh event
  set, the weapon stats, and the HUD state; the adapters only
  assign a material, fill a renderer field, render kind
  markers, set light and fog values, and build a static model
  from primitives. The view model in particular holds no second
  ammo rule: the only weapon numbers the player sees are the
  HUD line's, so the model and the HUD line cannot contradict
  each other — and that agreement is by absence of a second
  number, not by animation, as item 34 is written.
- No Slice 16 step changed a saved value's meaning: no save
  field was added, no block, kind, doctrine, source, or
  consumer value moved, and every earlier demo save length
  above is unchanged. The slice's own first-pass values — the
  dusk sun's azimuth 235, the fog density 0.008 and its
  about-83%-contrast arithmetic at the far band, the view
  model offset 0.22, -0.22, 0.60 — are labelled first-pass in
  their steps, chosen for readability and testability, not
  final feel, and must not be quoted as tuned results.
- Hard limits held: 3D real-time only, no turn-based
  mechanics, no twin-stick Asteroids loop, no copied assets,
  lore, code, or trademarks. Every model, material, and light
  value in Slice 16 is original: the view model is primitives
  in worn dark metal with cyan and orange accents, not the
  reference rifle; the sky is a gradient spec with no planet
  and no skyline, not the reference image's planet; the
  reference sight's AMMO / TARGET layout is deliberately not
  reproduced. The reference remains a mood and composition
  target throughout.

## Deliberately NOT in Slice 16 (still open, not forgotten)

- A second graphics system, a city, a planet, a squad or tank
  model, an enemy silhouette, or a mini-map, added alongside
  the scene-proof layer as a bonus. One scene-proof layer is
  Slice 16; the Slice 1 art scope's remaining pieces (enemy
  silhouette, mini-map beyond placeholder) are later slices,
  not bonuses here.
- A display-query redesign of the single-cable tint or the
  direct-Conduit count. Either is a carried candidate after
  graphics, with its own tests, not a patch inside a material
  step: a material step must not move the query it renders,
  and a query step must not move the material it is rendered
  by.
- A scene-proof observation presented as a headless result.
  Items 25, 26, and 27, and the new items 30, 31, 32, 33, and
  34, are Windows observations, recorded in DECISIONS.md when
  observed, not closed by a document, a script, or a test
  count.
- A scene file (`.unity`), an imported asset, a texture, or a
  shader file written by a headless run and claimed as a built
  scene. Adapter scripts and scene-wiring notes are what a
  headless run can honestly produce; scene assembly is Windows
  checklist work, and the checklist items above are what say
  which parts are script and which parts are still assembly
  to be done in Unity on Windows — including the editor-built
  gradient skybox material, which exists as a spec in
  `docs/art-direction.md` and in the `DuskLighting` header,
  not as a built material.
- A copied asset, model, skyline, HUD layout, or trademarked
  look from the reference image or from any other game.
- A HUD redesign. HUD discipline (art-direction rule 4:
  health, shields, objective distance and completion,
  mini-map, ammo — no more) is law, not a graphics variable,
  and no new HUD line, no new HUD colour, and no quest list
  rides along with a light or a view model.
- A core change of any kind: no new block, kind, doctrine,
  source, or consumer, no movement, targeting, delivery, or
  save rule change, no balance pass, no price or cap change.
  Slice 16 is adapter-only where Unity is concerned, and the
  core stands exactly as Slice 15 left it.
- Windows Unity verification itself. It remains open for
  Slices 1–16 alike and is blocked on Unity 6000 on Windows,
  not on anything this slice can do headlessly.
- A new composed demo scenario, for the reason stated at the
  top of this review: it would prove core numbers again, not
  the material, the wiring, the occlusion, the light, or the
  model.
- View model motion (sway, bob, recoil), a per-weapon model,
  and a mag-driven model element. Each is a later step's
  stated decision with its own Windows observation, not a
  hidden part of the static first model — and a mag-driven
  element carries a capacity question Step 4 named openly:
  `HudState` carries mag and reserve but no mag capacity, so
  a proportional gauge cannot be derived from `HudState`
  alone.

## Gaps this review names, in priority order

1. **Windows Unity verification, Slices 1–16.** The
   checklists in `docs/slice-01-playtest-gate.md`,
   `docs/orbital-view.md`, `docs/orbital-command.md`, and
   `docs/powered-presence.md` remain the verification plan.
   Slice 16 adds items 30, 31, 32, 33, and 34 to that plan,
   alongside the still-open items 25, 26, and 27. None of
   these items is claimed verified by any test count in this
   review, and scene assembly — wiring the adapter fields
   in the Inspector, building the gradient skybox material
   to the Step 3 spec — is still owed in Unity on Windows,
   not in this repo.
2. **The Slice 1 director playtest gate is still pending.**
   Rob has not yet played Slice 1 in Unity on Windows or
   answered the gate question (fun without extra systems?).
   Slice 16 does not close that gate, and its test count
   must not be quoted as a playtest result.
3. **First-pass graphics values are unproven, not just
   unverified.** The dusk sun's azimuth 235, intensity 1.1,
   and warm colour; the fog density 0.008 and its
   about-83%-contrast arithmetic at the far band; the view
   model offset 0.22, -0.22, 0.60; and the static,
   weapon-agnostic model itself are first-pass values for
   readability and testability. A Windows observation may
   find the dusk too warm for the cyan/green separation, the
   fog too thick at the far band, or the model too large in
   frame — and that observation, not this review, justifies
   retuning, with its own changelog entry. If the light
   makes any colour unread, the light changes; the palette
   does not move to fit the light.
4. **The Slice 13 scene-proof observations are still unmade,
   as observations.** Slice 16 built the proof layer, but
   whether the chunk material actually multiplies vertex
   colours (item 26), whether the adapter refreshes the same
   renderer GameManager links (item 27, with item 31
   observing the null-fill beside it), whether markers read
   through occlusion (item 25, with item 32 extending it to
   the Step 1 material), and now whether the dusk scene
   reads (item 33) and the view model sits correctly in
   frame (item 34), are all still to be observed on Windows.
   If any observation fails, that failure is a fix step,
   recorded as its own decision — not a reason to re-scope
   the proof layer blindly, and not closed by this review.
5. **The single-cable tint finding is still open.**
   Carried from Slice 14: a single cable cell touching a
   Conduit reads None in PowerState (untinted) while its
   network is live — its pylon fires, its mill is destroyed
   — because `DeliveredVoltage` for a cell counts only its
   neighbours and a Conduit is not a direct source, while
   a two-cable Conduit run reads Energised on both cells.
   The question is whether the power-state query for a
   cable cell should also count its own component's voltage,
   which would change tint behaviour for every existing
   source's cables as well and must therefore be decided
   as its own step, with its own tests, not patched inside
   a review. Until then, checklist item 28 asks the scene
   observer to record the untinted cell honestly, and item
   32's combined scene observes it alongside the tints.
6. **The direct-Conduit count invisibility is still open.**
   Carried from Slice 14: a Conduit standing directly
   beside a mill counts as neither a live network nor a
   lone source, because the Slice 12 lone rule
   disqualifies any source beside a consumer, whatever it
   delivers — so a player who places a Conduit beside a
   mill sees POWER 0 LIVE, 0 LONE, while the mill reads
   None and gathers kinetically. That is the Slice 12
   definition applied consistently, not a miscount, and
   this review does not call it a bug: the count answers
   "placed sources with no wiring", where wiring includes
   consumer adjacency. But it is a genuine visibility
   question — whether the counts should distinguish a
   source beside a consumer that delivers nothing from one
   that delivers, or whether the mill's None state and
   kinetic cadence are sufficient readout — and it must be
   decided with its own tests, not by redefining a count
   inside a review.
7. **The patrol route's stated limits are still open.**
   Carried from Slice 15: a refused step skips its waypoint
   for that circuit, with no detour, and a squad whose
   every waypoint is blocked cycles its index without
   moving — stationary, like HoldGround holding short,
   while still reading Patrolling. A later step may add a
   detour, a hold, or a stuck readout, but only as its own
   decision with its own tests, not as a silent change to
   the route Slice 15 closed.
8. **The free deploy primitive remains a documented gap.**
   It is headless-only by rule, not by enforcement: nothing
   in code stops a future play path from calling it. If a
   future step finds a play path reaching it, that is a bug
   in that path, per the 0.18.0 decision — but the review
   notes it again here so it is not forgotten as slices grow.

**Closed by this slice, not carried:** the director-ordered
scene-proof build from the Slice 15 review's queue decision —
the material and wiring (Step 1), the occlusion answer
(Step 2), the light, sky, and fog (Step 3), and the first view
model (Step 4) are now written as adapters with Windows
checklist items, so the build question Slice 15 left open is
closed in that sense, not left in the numbered gaps above.
The observations in gap 4 above are remainders about whether
that written layer reads correctly in a built scene, not a
reopening of whether the layer exists — and the Slice 14
remainders in gaps 5 and 6, which the queue decision carried
after graphics, are now the front of the queue again, unless
a later director decision reorders them.

## Slice 16 verdict (headless)

Slice 16 is **reviewed and closed as a written scene-proof
layer plus Windows checklist items (a URP Lit / Standard
fallback chunk material assigned in `VoxelMeshRenderer.Awake`
with an Inspector-wired material winning, a
`GameManager.Start` null-fill that links an unwired adapter
to the same renderer without overwriting a deliberate second
renderer, honest depth occlusion reaffirmed under that
material with no shader or depth change and no readability
aid adopted, a warm dusk sun with storm blue-grey ambient
and exponential fog to a recorded gradient sky spec, and a
static, original, weapon-agnostic view model built from
primitives and parented to the camera — each with its own
checklist item, 30, 31, 32, 33, and 34, and none of them
observed yet)**. It is not playtested, not Unity-verified,
and not a visual result, and its adapter claims must not be
quoted as if the test count covered them.
**Slice 17 is not scoped by this review** — it is scoped
afterwards, the same way this document follows the Slice 15
review's queue decision rather than preceding it.

## Ranked candidates carried after graphics (starting point, not promised)

Carried from the 0.67.0 scope, in the same order, with the
queue decision now spent — graphics is done as a build, and
these are what follows it, unless the director reorders them
again:

1. Any Slice 14 remainder this review names (the
   single-cable tint query question in gap 5, and the
   direct-Conduit count question in gap 6) — each a
   display-query decision step with its own tests, not a
   patch inside another step, and not promised here.
2. Any Slice 13 remainder this review names that Slice 16
   does not itself settle as an observation (the
   vertex-colour material observation, the MeshRenderer
   wiring observation, and the marker-occlusion
   observation in gap 4, plus the new dusk and view model
   observations in items 33 and 34) — if those observations
   are still unmade when the next slice is scoped, they
   remain observations to be made when Windows verification
   runs, not steps to be re-scoped blindly, and not
   promised here.
3. The patrol route's stated limits and the free deploy
   primitive, carried from the Slice 15 review's gaps 7 and
   8 — a detour, hold, or stuck readout for Patrol, or an
   enforcement for the free deploy primitive, each as its
   own decision with its own tests, not promised here.
