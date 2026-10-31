# Forgeborne — Slice 20 Review

**Status: REVIEWED HEADLESSLY 2026-10-31, tests 3165/3165.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
16, 17, 18, and 19. This review closes Slice 20 as a reviewed
whole; it does not claim Slice 20 is playtested, Unity-verified,
or a visual result.**

## Review question

Does Slice 20 settle the one stuck-patrol remainder Slice 19
deliberately carried — what, if anything, the player can do
about a patrol whose loop is permanently blocked, reading
Stuck, with no recall, no re-rally, and no player action
defined for it — as its own decision with its own tests,
then proved together with its controls in one demo, without
a movement, targeting, fire, delivery, precedence, conflict,
rating, cost, or phase rule change, a price or cap change, a
radius or waypoint change, a material, shader, marker, light,
fog, or view model change, a HUD redesign, a new block, kind,
doctrine, source, or consumer, a general squad-command system,
a balance pass, or a claim of Unity verification made from a
headless run? The answer headlessly is yes for the action as
decided, with the split this review keeps throughout: the
action is tested, the scene use of the action is still owed
on Windows, and the fully enclosed limit is a stated proof,
not a gap the decision overlooked.

## What Slice 20 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.84.0 scope | `docs/slice-20-scope.md`: Slice 20 scoped as the stuck-patrol slice, in order — the player-action decision first, then demo + review. Radius 3 and five waypoints carried, not re-decided. Ranked Slice 21 candidates recorded, not promised | Decision document only, no system built by it | Tests 2810/2810 unchanged |
| 0.85.0 Step 1 | Stuck-patrol player action decided: a FREE re-rally, for a stuck Patrol only — not a recall, not both, not neither. Targeting by squad list index (snapshot order). On a re-rally the rally changes, the waypoint index resets to 0, and the refusal counter resets to 0; position, health, doctrine, and kind are untouched, no teleport, no ore charged, no refund, no cap slot freed. Deploy price (10) and cap (3) stand. Adapter binding PROPOSED ONLY: K while not picking, crosshair on the stuck marker, picker raycast rally reuse, Return to confirm, Escape to cancel — a comment in `FpsInputAdapter.cs` only, not implemented | `WorldZone.TryReRallyStuckPatrol` (gate, fail-closed) with `TacticalSquad.ReRally` internal and a thin `Simulation` passthrough; `OrbitalCommand` / `OrbitalView` / HUD / marker rules untouched except the existing snapshot branch reading Patrolling after a re-rally | Tests 2968/2968 (was 2810; +158, including the route-blocked march, Recon and Heavy core effects, the enclosed re-stick, every refusal class, the solid-rally hold-short, a world-tick fire proof for Vanguard, save/load through the passthrough, and the paid-path proof); new playtest-gate item 39, extending item 37, superseding neither item 37, item 29, nor orbital-command item 13; demo unchanged, Slice 19 save 43,965 chars, Slice 18 save 44,015 chars, Slice 15 save 44,016 chars, Slice 17 save 43,974 chars |
| 0.86.0 Step 2 | This review, plus a new composed demo scenario staging the decided action together with its controls in one Simulation (see below) | Review document plus CHANGELOG and DECISIONS entries; no core file, no adapter file, no material, marker, light, fog, or view model value, no checklist item added | Tests 3165/3165 (was 2968; +197 composed-integration assertions mirroring the demo); every earlier demo save length unchanged; new Slice 20 scenario save 44,041 chars, quoted as its own entry below, not as a change to any earlier save |

## The Step 2 demo, in numbers

One Simulation, three paid squads, no enemy, no pocket seeded
beyond the 30 EmberOre the three deploys spend — the demo
proves the action, its refusals, its economy, and its
movement, and nothing else:

- Phase 1, the second-doctrine control marched away first:
  Heavy HoldGround, rally (10,5,10), deploys at the player
  (16,5,16), ore 30 to 20, and marches (15,16), (15,15),
  (14,15), (14,14), (13,14), (13,13) over six ticks, still
  reading Marching. It is deployed first so it is not
  standing on the player cell when the stuck patrol deploys
  there, and so its later Holding reading is a control, not
  an accident of spawn.
- Phase 2, the moving control: Recon Patrol, rally
  (20,5,20), deploys at the player (16,5,16), ore 20 to 10,
  and marches (17,16), (17,17), (18,17), (18,18), (19,18),
  (19,19), (20,19), (20,20) over eight ticks, reaching its
  rally reading Patrolling, while the HoldGround reaches
  (10,5,10) on tick 6 of this phase and reads Holding from
  then on.
- Phase 3, the Step 1 case in one field: Vanguard Patrol,
  rally (12,5,16), deploys at the player (16,5,16), ore 10
  to 0, cap now full at 3 live squads. The route-blocked
  shape is staged after this deploy, as in Slice 18: stone
  at (15,5,16) west, (16,5,17) south, and (16,5,15) north
  of the squad's cell, with the east step cell (17,5,16)
  left open — the exact Step 1 geometry translated to the
  player cell, so the proof is the same shape, not a new
  one. Over five ticks the stuck patrol counts refusals 1
  through 5, its waypoint cycling 1, 2, 3, 4, then back to
  0, reading Patrolling through four refusals and Stuck at
  five, unmoved at (16,5,16), while the HoldGround reads
  Holding at (10,5,10) and the moving Patrol walks its loop
  east to (23,5,20) and turns south to (22,5,21) beside it.
- Stuck-before-save load copy: the field saved at Stuck
  loads with all three squads, the stuck patrol reading
  Patrolling at index 0 with its counter at 0 and its old
  rally (12,5,16) kept, and over a fresh five ticks it
  counts refusals 1 through 5 again and reads Stuck again,
  unmoved at (16,5,16) — the load rule Step 1 left standing,
  proved in composition rather than re-asserted.
- Refusals on the main field, with nothing changed: a
  re-rally attempted on the HoldGround (index 0) and on
  the moving Patrol (index 1), both to (22,5,16), are both
  refused, and both keep their rallies ((10,5,10) and
  (20,5,20)) and the moving Patrol keeps its position
  (22,5,21).
- The decided action on the main field: the stuck patrol
  (index 2) accepts a free re-rally to (22,5,16) — rally
  changed, waypoint index 0, refusal counter 0, reading
  Patrolling at once, still at (16,5,16) (no teleport),
  ore still 0 (no charge, no refund), live squads still 3
  (no cap slot freed). A fourth paid deploy, DefendCore
  Heavy at (10,5,10), is then refused at the full cap, ore
  still 0, squads still 3, stuck rally still (22,5,16) —
  the rescue did not make room for a new squad.
- The march, tick by tick: the re-rallied patrol steps
  east (17,16), (18,16), (19,16), (20,16), (21,16), (22,16)
  over six ticks, waypoint 0 throughout with its target
  the new rally itself, while the moving Patrol walks
  (21,21), (21,22), (20,22), (20,23), (19,23), (19,22) and
  the HoldGround stays Holding at (10,5,10). One further
  tick turns it onto the new loop: (23,5,16), waypoint 1,
  target (25,5,16) — the new rally plus radius 3, with
  radius and waypoint count still 3 and 5, not retuned.
- Save/load round-trip after the action: the field saved
  at (23,5,16) loads with all three squads, the re-rallied
  patrol keeping its new rally (22,5,16) — it does not
  revert to (12,5,16) — at index 0 with its counter at 0,
  reading Patrolling, the HoldGround at (10,5,10) reading
  Holding, and the moving Patrol at (18,5,22) reading
  Patrolling. Loaded tick 1 steps the re-rallied patrol
  back to its new rally (22,5,16) at waypoint 0, because
  index 0 targets the rally itself under the load rule —
  stated here so a reader does not misread a step toward
  the rally as a step away from the loop — while the
  moving Patrol steps to (19,5,22) at waypoint 0 and the
  HoldGround stays Holding.

Totals, in the demo's own terms: 30 EmberOre spent on three
paid deploys, 0 charged by the rescue, 0 refunded by it, 3
live squads before and after the rescue, 1 refused fourth
deploy, 6 ticks to march to the new rally, 1 tick to turn
onto the new loop, and 2 load round-trips (one
stuck-before-save, one after the action) that both preserve
the field they were taken from. The new scenario's save is
44,041 chars, quoted here as the Slice 20 entry, not as a
change to any earlier save, and not as a claim about why it
differs from any other length.

## What the review verified this run

- Full test suite re-run before writing: **3165 passed, 0
  failed** (was 2968; +197, no failures on the final runs).
  The Step 2 assertions are composed-integration tests
  mirroring the demo layout above, inserted before the test
  runner's summary so they execute — the Slice 13 Step 4
  lesson, that tests appended after the runner's return do
  not run, is observed here, and the count rise from 2968
  to 3165 is the check that they did.
- Headless demo re-run, save lengths as a no-core-change
  check for the earlier scenarios and as the new entry for
  Slice 20: Slice 1 unchanged (save 43,881 chars); Slice 2
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
  chars); Slice 17 Step 3 scenario unchanged (save 43,974
  chars); Slice 18 Step 3 scenario unchanged (save 44,015
  chars); Slice 19 Step 2 scenario unchanged (save 43,965
  chars); Slice 20 Step 2 scenario new (save 44,041 chars).
  A changed earlier save length would have meant a save
  rule moved; none did. Slice 20 adds no save field — the
  re-rally reuses the saved rally field — so the unchanged
  earlier lengths are consistent with the decisions, not
  just coincident with them.
- Slice 20 gate, checked against `docs/slice-20-scope.md`:
  every step pushed with passing tests (for Step 1, with
  the suite risen by the proof-set tests, never standing
  still on an action step, and for Step 2, with the suite
  risen by the composed tests); the Step 1 action decision
  is recorded with rationale in DECISIONS.md, with its own
  Windows checklist item (item 39); no movement,
  targeting, fire, delivery, precedence, conflict, rating,
  cost, phase, or save-format rule changed in any step
  except the action itself and its explicitly justified
  free economy answer; no material, marker, light, fog,
  model, or HUD rule changed in any step except the
  snapshot activity reading Step 1 justifies for the
  moment after the action (Patrolling at once on a new
  loop); every earlier demo save length unchanged, with
  the new Slice 20 length quoted as its own entry; and
  this review records the verdict honestly, separating
  headless-tested action claims from the Windows-checklist
  action observation, including that Slice 20 is still not
  playtested and not Unity-verified. The Slice 1 director
  playtest gate is still pending throughout and is not
  closed by Slice 20.
- Checklist relations, stated so a later reader does not
  miscount them: item 39 is the stuck-patrol re-rally
  observation (an enclosed patrol to Stuck, a re-rally to
  open ground to Patrolling with a walk and a moved rally
  marker, a re-rally to a still-blocked rally to Stuck
  again, and refusals on a moving patrol and a HoldGround
  squad). Item 39 extends item 37 (whose sidestep, stuck,
  and block-clears readings stand), and item 37, item 29,
  and orbital-command item 13 stand unchanged and are NOT
  superseded. This review adds no new item, because the
  review exercises no behaviour item 39 does not already
  cover. The next free playtest-gate number remains 40.
  Item 39 is also honestly pending its own binding: it
  cannot be observed until the proposed K binding is
  implemented in a later adapter step, so until then it
  stands as the observation that must be made, not as a
  verified behaviour.
- One claim split stated plainly, so it is not over-claimed
  later: the headless evidence for Slice 20 is squad
  states before and after an action for staged layouts,
  refusal counts, waypoint indices, rally values, ore and
  cap counts across deploys and the rescue, march positions
  across ticks, and two save/load round-trips that preserve
  the field, plus a demo that prints those values for
  three squads in one field. None of that proves a player
  can press K, aim at a stuck marker, pick a new rally,
  and watch the rally marker move in a built scene, or that
  a player understands Stuck as "re-rally me" while looking
  at the field. That is a Windows observation (item 39),
  recorded in DECISIONS.md when observed, not closed by a
  test count. The fully enclosed limit is proved the same
  way Step 1 proved it — a re-rally corrects the loop, it
  does not march a squad through a wall — and this review
  does not re-prove it in composition: the composed stuck
  case is the route-blocked shape that marches, and the
  enclosed shape's re-stick remains Step 1's proof, not a
  composed claim.
- No Slice 20 step duplicates logic in a Unity adapter.
  The action lives in core `WorldZone.TryReRallyStuckPatrol`
  and the thin `Simulation` passthrough, and renders
  through the existing snapshot and marker rules — Step 1
  added only a proposed-binding comment to
  `FpsInputAdapter.cs`, and Step 2 edits no adapter file at
  all. There is no second re-rally rule in any adapter
  that could drift from the core rule.
- No Slice 20 step changed a saved value's meaning: no
  save field was added, no block, kind, doctrine, source,
  or consumer value moved, and every earlier demo save
  length above is unchanged. The one value the action does
  change in a save — a squad's rally — already existed as
  a saved field, so a re-rallied squad loads with its new
  rally and an old save loads unchanged.
- Hard limits held: 3D real-time only, no turn-based
  mechanics, no twin-stick Asteroids loop, no copied assets,
  lore, code, or trademarks. Slice 20 adds no block, kind,
  doctrine, source, or consumer, so there is nothing new
  in it that could copy one.

## Deliberately NOT in Slice 20 (still open, not forgotten)

- A radius, waypoint-count, or loop-shape change. The loop
  stays 3 and 5, flat at rally height, unless a later
  decision on recorded evidence changes it — no Step 1 or
  Step 2 proof shows the radius caused a block, so no
  radius finding is carried as a retune here.
- A movement, targeting, or fire rule change.
  MoveForPatrol's sidestep, skip, and Stuck threshold
  stand; a stuck patrol still fires in passing; no
  pathfinder, no hold, no wider sidestep is added here.
- A delivery, precedence, conflict, rating, cost, or phase
  rule change beyond the economy question Step 1 answered
  explicitly (a free re-rally, no refund question because
  there is no recall). The deploy price (10) and the cap
  (3) otherwise stand, and no power, consumer, or
  save-format rule moves.
- A new block, kind, doctrine, source, or consumer, a
  balance pass, or a price or cap change disguised as a
  rescue. Slice 20 is one stuck-patrol action and its
  review, and the core stands exactly as Slice 19 left it
  except for the action itself.
- A material, shader, marker, light, fog, or view model
  change, or a HUD redesign. The Slice 16 surface renders
  whatever Slice 20 decides; a step that re-paletted a
  marker or added a HUD line to make its answer look
  right would have confused the surface with the action.
- A general squad-command system. Selecting, ordering, or
  commanding squads beyond the one decided stuck-patrol
  action — including a general recall or a general
  re-rally for other doctrines — is a larger slice,
  carried after Slice 20, not patched in beside a rescue.
- A recall or dismiss API of any kind, for any doctrine.
  Step 1 rejected recall with reasons, and no step in this
  slice adds one quietly afterwards.
- A scene-proof observation presented as a headless result.
  Items 25–27 and 30–39 are Windows observations, recorded
  in DECISIONS.md when observed, not closed by a document,
  a script, or a test count.
- A scene file (`.unity`), an imported asset, a texture, or
  a shader file written by a headless run and claimed as a
  built scene. Scene assembly remains Windows checklist
  work.
- A copied asset, model, skyline, HUD layout, or
  trademarked look from any other game.
- Windows Unity verification itself. It remains open for
  Slices 1–20 alike and is blocked on Unity 6000 on Windows,
  not on anything this slice can do headlessly.
- A Slice 21 scope. Slice 21 is scoped after the Slice 20
  review, not inside it.

## Gaps this review names, in priority order

1. **Windows Unity verification, Slices 1–20.** The
   checklists in `docs/slice-01-playtest-gate.md`,
   `docs/orbital-view.md`, `docs/orbital-command.md`, and
   `docs/powered-presence.md` remain the verification plan.
   Slice 20 adds item 39 to that plan, alongside the
   still-open items 25, 26, 27, and 30–38. None of these
   items is claimed verified by any test count in this
   review, and scene assembly is still owed in Unity on
   Windows, not in this repo. Item 39 has the extra
   condition that its proposed K binding is not yet
   implemented in an adapter, so the observation cannot
   even be attempted until a later adapter step implements
   it.
2. **The Slice 1 director playtest gate is still pending.**
   Rob has not yet played Slice 1 in Unity on Windows or
   answered the gate question (fun without extra systems?).
   Slice 20 does not close that gate, and its test count
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
   its own changelog entry. Slice 20 adds a reason this
   matters: a stuck patrol's rally marker must read under
   that dusk light when it moves to a new rally, and only
   item 39's observation can say whether a player reads
   the moved marker as the rescue it is.
4. **The Slice 13 scene-proof observations are still unmade,
   as observations.** Carried from Slice 16: whether the
   chunk material actually multiplies vertex colours (item
   26), whether the adapter refreshes the same renderer
   GameManager links (item 27, with item 31 observing the
   null-fill beside it), whether markers read through
   occlusion (item 25, with item 32 extending it to the
   Step 1 material), and now whether the dusk scene reads
   (item 33), the view model sits correctly in frame (item
   34), the decided tint reads (item 35), the decided
   count reads (item 36), the decided patrol route reads
   (item 37), the decided dead-transformer count reads
   (item 38), and the decided re-rally reads (item 39), are
   all still to be observed on Windows. If any observation
   fails, that failure is a fix step, recorded as its own
   decision — not a reason to re-scope the proof layer
   blindly, and not closed by this review.
5. **The patrol route's remaining questions, as decided,
   still leave open decisions a later step may take.**
   Carried from the Slice 20 scope's ranked candidates: a
   radius or waypoint first-pass value revisited on
   evidence (a Step 1 finding that the radius caused a
   block — none was found — or a Windows observation that
   a loop does not fit its ground), or, since Slice 20
   scopes its action to stuck Patrols only, a general
   recall or re-rally for other doctrines as its own
   decision — each with its own tests, not promised here.

**Closed by this slice, not carried:** the stuck-patrol
action in the Slice 19 review's gap 5 — a patrol reading
Stuck with no player action defined for it — is now
decided, with tests, with a Windows checklist item for the
action observation, and proved together with its
moving-patrol, HoldGround, economy, and save/load controls
in one demo, so the last Slice 18 gap 6 remainder is closed
in that sense, not left in the numbered gaps above. The
observation in gaps 1 and 4 above is a remainder about
whether the decided action works in a built scene, not a
reopening of whether the decision exists.

## Slice 20 verdict (headless)

Slice 20 is **reviewed and closed as one stuck-patrol action
decided with tests and proved together in one demo (a stuck
Patrol gets a free re-rally, stuck Patrol only, by squad
list index, with its rally changed, its waypoint index and
refusal counter reset to 0, its position, health, doctrine,
and kind untouched, no ore charged, no cap slot freed, and
its march to the new rally and turn onto the new loop
proved beside a moving Patrol that refuses the same action,
a HoldGround that refuses it, a fourth deploy refused at
the full cap, and two save/load round-trips that preserve
the field — and the whole field, saved and loaded,
re-derives the same readings, stable across ticks because
no squad here dies and no block here is built or removed
after staging)**. It is not playtested, not Unity-verified,
and not a visual result, and its action claims must not be
quoted as if the test count covered the scene action in
item 39.
**Slice 21 is not scoped by this review** — it is scoped
afterwards, the same way this document follows the Slice 19
review rather than preceding it.

## Ranked candidates carried after Slice 20 (starting point, not promised)

Carried from the 0.84.0 scope, in the same order, with the
Slice 20 work taken:

1. The patrol route's remaining questions — a radius or
   waypoint first-pass value revisited on evidence (a Step
   1 finding that the radius caused a block, or a Windows
   observation that a loop does not fit its ground), or,
   if Slice 20 scopes its action to stuck Patrols only, a
   general recall or re-rally for other doctrines as its
   own decision — each with its own tests, not promised
   here.
2. Any Slice 13 remainder that Slice 20 does not itself
   settle as an observation (the vertex-colour material
   observation, the MeshRenderer wiring observation, and
   the marker-occlusion observation in the Slice 19
   review's gap 4, plus the dusk and view model
   observations in items 33 and 34, plus the tint, count,
   and patrol observations in items 35, 36, and 37, plus
   the new item Step 1 adds in item 39) — if those
   observations are still unmade when the review is written,
   they remain observations to be made when Windows
   verification runs, not steps to be re-scoped blindly,
   and not promised here.
3. First-pass graphics values retuned only on Windows
   evidence (dusk azimuth, fog contrast, view model offset,
   static weapon-agnostic model) — a retuning step is
   justified by an observation, not by a headless
   re-decision, and is not promised here.
