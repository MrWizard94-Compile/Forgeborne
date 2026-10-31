# Forgeborne — Slice 17 Review

**Status: REVIEWED HEADLESSLY 2026-10-31, tests 2301/2301.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
and 16. This review closes Slice 17 as a reviewed whole; it
does not claim Slice 17 is playtested, Unity-verified, or a
visual result.**

## Review question

Does Slice 17 settle the two display-query remainders Slice 14
left honestly carried and Slice 16 carried again — whether a
single cable cell touching a Conduit should read tinted while
its network is live, and whether a Conduit standing directly
beside a consumer should count as a lone source — each as its
own decision with its own tests, then proved together in one
wiring, without a delivery, precedence, conflict, rating, cost,
or phase rule change, a material, shader, marker, light, fog,
or view model change, a HUD redesign, a new block, kind,
doctrine, source, or consumer, a balance pass, or a claim of
Unity verification made from a headless run? The answer
headlessly is yes for the two queries as decided, with the
split this review keeps throughout: the queries are tested,
the scene observations of them are still owed on Windows.

## What Slice 17 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.73.0 scope | `docs/slice-17-scope.md`: Slice 17 scoped as the display-query slice, in order — single-cable tint query first, direct-Conduit count query second, then demo + review. Ranked Slice 18 candidates recorded, not promised | Decision document only, no system built by it | Tests 2130/2130 unchanged |
| 0.74.0 Step 1 | Single-cable tint query decided: for a cable cell, Energised now means its cable component carries a non-None voltage, not that voltage is delivered to that cell itself. Implemented in `WorldZone.PowerState` only (cable branch reads `CableNetworkVoltage(CableComponent(...), null)`); `DeliveredVoltage` untouched | `PowerState.cs` comment updated to the decided sentence; `World.cs` cable branch only; no delivery, precedence, conflict, rating, cost, or phase rule moves | Tests 2169/2169 (was 2130; +39, including the single-cable, two-cable, Dynamo-, Alternator-, and transformer-fed, mixed-component, unpowered, delivery-unchanged, rewiring, save/load, and mesh-quad proofs); demo Slice 15 save 44,016 chars, Slice 14 print wording updated |
| 0.75.0 Step 2 | Direct-Conduit count query decided: a Conduit beside a consumer counts as one lone source, folded into the existing POWER line's lone number — no new line, no new word. Lone now means a source with no cable or transformer neighbour, and no consumer neighbour that it feeds directly (Dynamo and Alternator feed any adjacent consumer directly; a Conduit feeds none) | `WorldZone.LivePowerCounts` lone loop only, plus its comments and the `WeaponHud` comment; `HudDisplay`'s LivePowerText line text unchanged, no adapter edit needed | Tests 2227/2227 (was 2169; +58, including all three consumers, feeding, conflict, fed-mill, cable-touching, per-source-cell, transformer, lone-control, and save/load proofs); three Slice 14 Step 2 composed lone assertions updated from 0 to 1 with history kept, Slice 14 demo print wording updated |
| 0.76.0 Step 3 | This review, plus a new composed demo scenario staging both decided queries together in one Simulation (see below) | Review document plus CHANGELOG and DECISIONS entries; no core file, no adapter file, no material, marker, light, fog, or view model value, no checklist item added | Tests 2301/2301 (was 2227; +74 composed-integration assertions mirroring the demo); every earlier demo save length unchanged; new Slice 17 scenario save 43,974 chars, quoted as its own entry below, not as a change to any earlier save |

## The Step 3 demo, in numbers

One Simulation, six rows on the y=20 plane, four cells apart
in z so no row can leak voltage into another through a
face-adjacent neighbour:

- Single-cable run (z=10): mill, cable, Conduit, with a
  five-stone pocket around the mill. Before any tick the mill
  reads HV and Mismatched, its cable Energised, and it is one
  of two warned mills.
- Two-cable run (z=14): mill, cable, cable, Conduit, with a
  five-stone pocket. Before any tick the mill reads HV and
  Mismatched, both cables Energised — the already-tinted
  control, proving the Step 1 change did not break the case
  that already read correctly.
- Mixed component (z=18): Dynamo, cable, Conduit, no
  consumer. Before and after every tick its cable reads None
  — the still-untinted control, proving component liveness
  does not resurrect a dead (mixed-voltage) network.
- Direct-Conduit mill (z=22): mill beside a Conduit, with a
  five-stone pocket. Before any tick the mill reads None,
  is neither powered nor warned, survives, and its Conduit
  counts as one lone source — the Step 2 case, with the
  POWER line read before and after the tick (see below).
- Dynamo mill (z=26): mill beside a Dynamo, with a
  five-stone pocket. Before any tick the mill reads LV and
  Powered, gathers every tick, and counts neither live nor
  lone — the feeding control.
- Lone Conduit (z=30): a Conduit with no neighbour at all.
  It is the second lone source in the POWER line — the lone
  control.

Before any tick: 1/4 mills powered, 2 warned, POWER 0 LIVE
NETWORKS, 2 LONE SOURCES. The 0 live is itself a composed
proof: both cable runs feed a mill, so neither counts live,
and no consumer count is double-counted. The 2 lone is the
direct Conduit plus the lone Conduit, and only those two:
every cabled source is disqualified by its cable neighbour,
and the Dynamo beside its mill is disqualified because it
feeds that mill directly.

Tick 1: both cable-fed mills are destroyed (Air), destroyed
2, powered gathered 1 (the Dynamo mill), kinetic gathered 0,
the direct-Conduit mill survives as KineticMill, POWER 2 LIVE
NETWORKS, 2 LONE SOURCES. The live number rose exactly by
the two destroyed mills' runs, which now feed nothing, while
the lone number did not move — the direct Conduit still
counts 1 lone inside the 2, as before the tick. Both
destroyed runs' cables still read Energised, and the mixed
cable still reads None, after the tick.

After 5 ticks: powered gathered 5 (Dynamo pocket exhausted,
every tick), kinetic gathered 1 (direct-Conduit mill, 1 in
5 ticks), destroyed 2, stone 6. Save/load round-trip: both
destroyed mills stay Air (no resurrection), the direct and
Dynamo mills load as KineticMill, POWER still 2 LIVE, 2
LONE, the single cable still Energised, the mixed cable
still None, the direct mill still None, the Dynamo mill
still Powered — both readings re-derived, not saved — and
the session counters reset (powered 0, kinetic 0, destroyed
0) while stone stays 6 and the player is at (16,5,16).
Loaded phase, with the Dynamo pocket re-seeded: powered
gathered 5 again, kinetic gathered 1 again, destroyed 0,
stone 12. The new scenario's save is 43,974 chars. That
length matches several earlier saves by format coincidence
only — it is a different save, quoted here as the Slice 17
entry, not as a change to any earlier entry.

## What the review verified this run

- Full test suite re-run before writing: **2301 passed, 0
  failed** (was 2227; +74, no failures on the final runs).
  The Step 3 assertions are composed-integration tests
  mirroring the demo layout above, inserted before the test
  runner's summary so they execute — the Slice 13 Step 4
  lesson, that tests appended after the runner's return do
  not run, is observed here, and the count rise from 2227
  to 2301 is the check that they did.
- Headless demo re-run, save lengths as a no-core-change
  check for the earlier scenarios and as the new entry for
  Slice 17: Slice 1 unchanged (save 43,881 chars); Slice 2
  unchanged (save 43,921 chars); Step 1 scenario unchanged
  (save 44,016 chars); Step 3 scenario unchanged (save
  44,103 chars); Slice 4 scenario unchanged (save 44,026
  chars); Slice 5 Step 2 scenario unchanged (save 43,974
  chars); Slice 6 Step 4 scenario unchanged (save 44,024
  chars); Slice 7 Step 2 scenario unchanged (save 44,013
  chars); Slice 8 Step 3 scenario unchanged (save 43,974
  chars); Slice 9 Step 4 scenario unchanged (save 43,975
  chars); Slice 10 Step 3 scenario unchanged (save 43,974
  chars); Slice 11 Step 3 scenario unchanged (save 44,038
  chars); Slice 12 Step 3 scenario unchanged (save 43,974
  chars); Slice 13 Step 4 scenario unchanged (save 44,045
  chars); Slice 14 Step 2 scenario unchanged (save 43,974
  chars); Slice 15 Step 2 scenario unchanged (save 44,016
  chars); Slice 17 Step 3 scenario new (save 43,974 chars).
  A changed earlier save length would have meant a save
  rule moved; none did. Both Slice 17 readings are derived,
  never saved, so the unchanged earlier lengths are
  consistent with the decisions, not just coincident with
  them.
- Slice 17 gate, checked against `docs/slice-17-scope.md`:
  every step pushed with passing tests (for Steps 1 and 2,
  with the suite risen by the proof-set tests, never
  standing still on a query step); the Step 1 tint decision
  and the Step 2 count decision are each recorded with
  rationale in DECISIONS.md, each with its own Windows
  checklist item from 35 (item 35 tint, item 36 count),
  with no delivery, precedence, conflict, rating, cost, or
  phase rule changed, no material, marker, light, fog,
  model, or HUD rule changed except the count line's
  decided question (a fold-in, not a redesign), and every
  earlier demo save length unchanged; and this review
  records the verdict honestly, separating headless-tested
  query claims from Windows-checklist tint and count
  observations, including that Slice 17 is still not
  playtested and not Unity-verified. The Slice 1 director
  playtest gate is still pending throughout and is not
  closed by Slice 17.
- Checklist relations, stated so a later reader does not
  miscount them: item 35 is the post-decision tint
  observation (single-cable, two-cable, and mixed runs
  together, dark voxel, dusk light, mill destruction or
  pylon fire as live-network evidence, rewiring in both
  orders). Item 35 supersedes item 28's honest-observation
  clause for the decided case only — an untinted single
  Conduit cable is now evidence the query or refresh did
  not run, not an honest reading to record — and item 28's
  other observations stand, with item 28 left unedited, as
  item 20 was for item 25. Item 35 does not claim item 33's
  dusk observation, and item 32 stands as the pre-decision
  observation, item 35 as its post-decision counterpart.
  Item 36 is the post-decision count observation (a
  Conduit beside a mill, a Dynamo beside a mill, and a
  lone Conduit, with the POWER line read aloud for each).
  Item 36 extends item 22's live-network and lone-source
  display (a lone-Conduit observation added; item 22
  stands) and extends item 28's Conduit observation (item
  28's mill clause — kinetic cadence, no warning — stands;
  item 36 adds the POWER line reading item 28 did not ask
  for). Item 36 does not supersede item 28's mill clause
  and does not claim item 35's tint observation. This
  review adds no new checklist item, because the review
  exercises no query the checklist does not already cover
  — the next free playtest-gate number is 37.
- One claim split stated plainly, so it is not over-claimed
  later: the headless evidence for Slice 17 is query values
  for staged cells and count pairs for staged layouts,
  plus a demo that prints those values in one wiring and
  a save/load round-trip that re-derives them. None of
  that proves a tint reads yellow in the scene, a POWER
  line reads aloud correctly on a screen, or a player
  understands either reading while wiring. Those are
  Windows observations (items 35 and 36), recorded in
  DECISIONS.md when observed, not closed by a test count.
- No Slice 17 step duplicates logic in a Unity adapter.
  The tint decision lives in core `PowerState`, the count
  decision lives in core `LivePowerCounts`, and no adapter
  file was edited in this slice: the Slice 16 surface
  renders whatever these queries decide, and the HUD line
  renders whatever count they return, with no second rule
  in either adapter that could drift from the core rule.
- No Slice 17 step changed a saved value's meaning: no save
  field was added, no block, kind, doctrine, source, or
  consumer value moved, and every earlier demo save length
  above is unchanged. The two superseded readings (the
  single-cable None in Slice 14 Step 1, and the direct-
  Conduit neither-live-nor-lone in Slice 14 Step 2) were
  updated in their composed assertions with history kept,
  not deleted silently — see the Step 1 and Step 2 entries
  in DECISIONS.md for the exact assertions and wording.
- Hard limits held: 3D real-time only, no turn-based
  mechanics, no twin-stick Asteroids loop, no copied assets,
  lore, code, or trademarks. Slice 17 adds no block, kind,
  doctrine, source, or consumer, so there is nothing new
  in it that could copy one.

## Deliberately NOT in Slice 17 (still open, not forgotten)

- A delivery, precedence, conflict, rating, cost, or phase
  rule change. Delivery is what a consumer does on a tick,
  and no Slice 17 step moves it: the tint step proves
  delivery unchanged (a mill beside a Conduit still reads
  None and gathers kinetically, a cable-fed mill is still
  destroyed on tick 1, a cable-fed pylon still fires 10 per
  tick), and the count step proves it the same way.
- A material, shader, marker, light, fog, or view model
  change. The Slice 16 surface renders whatever Slice 17
  decides; a step that re-paletted a tint, retuned dusk, or
  moved a model to make its answer look right would have
  confused the surface with the query. No Slice 17 step
  does.
- A HUD redesign. The count step decides one POWER line
  question (fold-in, justified against HUD discipline) and
  nothing else: no new HUD colour, no new HUD layout, no
  quest list, no second POWER line smuggled in as a
  clarification.
- A new block, kind, doctrine, source, or consumer, a
  movement, targeting, or save rule change, a balance pass,
  or a price or cap change. Slice 17 is two display queries
  and their review, and the core stands exactly as Slice 16
  left it except for the two queries themselves.
- A scene-proof observation presented as a headless result.
  Items 25–27 and 30–34, and the new items 35 and 36, are
  Windows observations, recorded in DECISIONS.md when
  observed, not closed by a document, a script, or a test
  count.
- A scene file (`.unity`), an imported asset, a texture, or
  a shader file written by a headless run and claimed as a
  built scene. Scene assembly remains Windows checklist
  work.
- A copied asset, model, skyline, HUD layout, or
  trademarked look from any other game.
- Windows Unity verification itself. It remains open for
  Slices 1–17 alike and is blocked on Unity 6000 on Windows,
  not on anything this slice can do headlessly.
- The patrol route's stated limits, or an enforcement for
  the free deploy primitive, as a bonus inside a query step.
  Each is its own decision with its own tests, carried
  after Slice 17, not patched in beside a tint.
- A Slice 18 scope. Slice 18 is scoped after the Slice 17
  review, not inside it.

## Gaps this review names, in priority order

1. **Windows Unity verification, Slices 1–17.** The
   checklists in `docs/slice-01-playtest-gate.md`,
   `docs/orbital-view.md`, `docs/orbital-command.md`, and
   `docs/powered-presence.md` remain the verification plan.
   Slice 17 adds items 35 and 36 to that plan, alongside
   the still-open items 25, 26, 27, and 30–34. None of these
   items is claimed verified by any test count in this
   review, and scene assembly is still owed in Unity on
   Windows, not in this repo.
2. **The Slice 1 director playtest gate is still pending.**
   Rob has not yet played Slice 1 in Unity on Windows or
   answered the gate question (fun without extra systems?).
   Slice 17 does not close that gate, and its test count
   must not be quoted as a playtest result.
3. **First-pass graphics values are unproven, not just
   unverified.** Carried from Slice 16: the dusk sun's
   azimuth 235, intensity 1.1, and warm colour; the fog
   density 0.008 and its about-83%-contrast arithmetic at
   the far band; the view model offset 0.22, -0.22, 0.60;
   and the static, weapon-agnostic model itself are
   first-pass values for readability and testability. A
   Windows observation may find the dusk too warm for the
   cyan/green separation, the fog too thick at the far
   band, or the model too large in frame — and that
   observation, not this review, justifies retuning, with
   its own changelog entry. If the light makes any colour
   unread, the light changes; the palette does not move to
   fit the light. Slice 17 adds a reason this matters: the
   Energised tint (yellow 1, 0.85, 0.1) and the Powered
   tint (green) must stay separable under that dusk light,
   and only item 35's observation, under dusk, can say
   whether they do.
4. **The Slice 13 scene-proof observations are still unmade,
   as observations.** Carried from Slice 16: whether the
   chunk material actually multiplies vertex colours (item
   26), whether the adapter refreshes the same renderer
   GameManager links (item 27, with item 31 observing the
   null-fill beside it), whether markers read through
   occlusion (item 25, with item 32 extending it to the
   Step 1 material), and now whether the dusk scene reads
   (item 33), the view model sits correctly in frame (item
   34), the decided tint reads (item 35), and the decided
   count reads (item 36), are all still to be observed on
   Windows. If any observation fails, that failure is a fix
   step, recorded as its own decision — not a reason to
   re-scope the proof layer blindly, and not closed by this
   review.
5. **The transformer-adjacency invisibility is still open.**
   Carried from Step 2, stated there as a deliberate
   asymmetry, not an oversight: a Conduit beside a
   transformer alone still reads 0 LIVE, 0 LONE, even
   though the transformer outputs None with no other input
   and the Conduit feeds nothing — because a transformer
   is wiring, not a consumer endpoint, and folding it into
   lone would also have to cover the dead-cable case to be
   consistent. If a later step decides that second
   invisibility too, it decides it with its own proof set,
   not by inheriting Step 2's sentence.
6. **The patrol route's stated limits are still open.**
   Carried from Slices 15 and 16: a refused step skips its
   waypoint for that circuit, with no detour, and a squad
   whose every waypoint is blocked cycles its index
   without moving — stationary, like HoldGround holding
   short, while still reading Patrolling. A later step may
   add a detour, a hold, or a stuck readout, but only as
   its own decision with its own tests, not as a silent
   change to the route Slice 15 closed.
7. **The free deploy primitive remains a documented gap.**
   It is headless-only by rule, not by enforcement: nothing
   in code stops a future play path from calling it. If a
   future step finds a play path reaching it, that is a bug
   in that path, per the 0.18.0 decision — but the review
   notes it again here so it is not forgotten as slices grow.

**Closed by this slice, not carried:** the two Slice 14
remainders in the Slice 16 review's gaps 5 and 6 — the
single-cable tint finding and the direct-Conduit count
question — are now decided, with tests and with Windows
checklist items, so the display-query question Slice 16 left
open is closed in that sense, not left in the numbered gaps
above. The observations in gaps 1 and 4 above are remainders
about whether those decisions read correctly in a built
scene, not a reopening of whether the decisions exist.

## Slice 17 verdict (headless)

Slice 17 is **reviewed and closed as two display queries
decided with tests and proved together in one wiring (a
single cable cell reads Energised exactly when its component
carries a non-None voltage, so a single-cable Conduit run
reads tinted while its mill is destroyed on tick 1 and a
mixed-voltage run stays untinted; a Conduit beside a
consumer counts as one lone source folded into the existing
POWER line, so the composed wiring reads 0 LIVE, 2 LONE
before the tick and 2 LIVE, 2 LONE after it, with both
readings re-deriving on load — each with its own Windows
checklist item, 35 and 36, and neither observed yet)**. It
is not playtested, not Unity-verified, and not a visual
result, and its query claims must not be quoted as if the
test count covered the scene readings.
**Slice 18 is not scoped by this review** — it is scoped
afterwards, the same way this document follows the Slice 16
review rather than preceding it.

## Ranked candidates carried after Slice 17 (starting point, not promised)

Carried from the 0.73.0 scope, in the same order, with the
Slice 17 work taken:

1. The patrol route's stated limits and the free deploy
   primitive, carried from the Slice 16 review's gaps 7 and
   8 — a detour, hold, or stuck readout for Patrol, or an
   enforcement for the free deploy primitive, each as its
   own decision with its own tests, not promised here.
2. Any Slice 13 remainder that Slice 17 does not itself
   settle as an observation (the vertex-colour material
   observation, the MeshRenderer wiring observation, and
   the marker-occlusion observation in gap 4, plus the new
   dusk and view model observations in items 33 and 34,
   plus the new tint and count observations Steps 1 and 2
   add) — if those observations are still unmade when the
   next slice is scoped, they remain observations to be
   made when Windows verification runs, not steps to be
   re-scoped blindly, and not promised here.
3. First-pass graphics values retuned only on Windows
   evidence (dusk azimuth, fog contrast, view model offset,
   static weapon-agnostic model) — a retuning step is
   justified by an observation, not by a headless
   re-decision, and is not promised here.
