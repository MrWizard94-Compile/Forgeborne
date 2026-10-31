# Forgeborne — Slice 21 Review

**Status: REVIEWED HEADLESSLY 2026-10-31, tests 3607/3607.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
16, 17, 18, 19, and 20. This review closes Slice 21 as a reviewed
whole; it does not claim Slice 21 is playtested, Unity-verified,
or a visual result.**

## Review question

Does Slice 21 settle the one general re-rally remainder Slice 20
deliberately carried — since Slice 20 scopes its action to stuck
Patrols only, what, if anything, the player can do to re-rally a
live squad that is not a stuck Patrol, at what price, and with
what observable effect per doctrine — as its own decision with
its own tests, then proved together with its controls in one
demo, without a movement, targeting, fire, delivery, precedence,
conflict, rating, cost, or phase rule change beyond the economy
answer Step 1 justifies explicitly, a price or cap change, a
radius or waypoint change, a material, shader, marker, light,
fog, or view model change, a HUD redesign, a new block, kind,
doctrine, source, or consumer, a general squad-command system,
a recall or dismiss API, a balance pass, or a claim of Unity
verification made from a headless run? The answer headlessly is
yes for the action as decided, with the split this review keeps
throughout: the action is tested, the scene use of the action is
still owed on Windows, and the staged deaths that let five roles
fit under a cap of 3 are staging, not a player action.

## What Slice 21 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.87.0 scope | `docs/slice-21-scope.md`: Slice 21 scoped as the general re-rally slice, in order — the generalisation decision first, then demo + review. Radius 3 and five waypoints carried, not re-decided. Recall carried after Slice 21. Ranked Slice 22 candidates recorded, not promised | Decision document only, no system built by it | Tests 3165/3165 unchanged |
| 0.88.0 Step 1 | General re-rally decided: a FREE general re-rally, for any live HoldGround or Patrol squad, stuck or not — and a fail-closed refusal for Advance and Defend Core, whose rally moves nothing. Targeting by squad list index (snapshot order), the two gates standing beside each other, sharing internal `ReRally` and the index rule, differing only in eligibility. On a re-rally the rally changes, a Patrol's waypoint index and refusal counter reset to 0; position, health, doctrine, and kind are untouched, no teleport, no ore charged, no refund, no cap slot freed. A stuck Patrol succeeds under both gates with the identical outcome, so Slice 20 is subsumed, not repriced and not revised. Deploy price (10) and cap (3) stand. Adapter binding PROPOSED ONLY: reuse K with wider eligibility — K while not picking, crosshair on an eligible squad's marker, picker raycast rally reuse, Return to confirm, Escape to cancel — a comment in `FpsInputAdapter.cs` only, not implemented | `WorldZone.TryReRallySquad` (gate, fail-closed) beside unchanged `TryReRallyStuckPatrol`, with `TacticalSquad.ReRally` internal and a thin `Simulation` passthrough; `OrbitalCommand` / `OrbitalView` / HUD / marker rules untouched except the existing snapshot branches reading the post-action words | Tests 3402/3402 (was 3165; +237, including the HoldGround march and fire suspension/resumption, the Holding-at-once case, all three kinds for the core march, the solid-rally hold-short, the non-stuck Patrol mid-loop march and loop turn, the counter-at-2 reset, Recon and Heavy patrol marches, the blocked-loop re-stick from a non-stuck origin, Advance and Defend Core refusals for all three kinds with chase/fire unchanged, the two-gates-agree proof, every refusal class, save/load through the passthrough, and the paid-path proof); new playtest-gate item 40, extending item 39, superseding neither item 39, item 37, item 29, nor orbital-command item 13; demo unchanged, Slice 20 save 44,041 chars, Slice 19 save 43,965 chars, Slice 18 save 44,015 chars, Slice 15 save 44,016 chars, Slice 17 save 43,974 chars |
| 0.89.0 Step 2 | This review, plus a new composed demo scenario staging the decided action together with its controls in one Simulation (see below) | Review document plus CHANGELOG and DECISIONS entries; no core file, no adapter file, no material, marker, light, fog, or view model value, no checklist item added | Tests 3607/3607 (was 3402; +205 composed-integration assertions mirroring the demo); every earlier demo save length unchanged; new Slice 21 scenario save 44,040 chars, quoted as its own entry below, not as a change to any earlier save |

## The Step 2 demo, in numbers

One Simulation, five paid deploys in sequence but never more
than 3 live squads at once, no enemy staged for a fire claim —
the demo proves the action, its refusals, its economy, and its
movement, and nothing else:

- Phase 1, the inert-doctrine refusals: Vanguard Advance,
  rally (10,5,10), deploys at the player (16,5,16), ore 50
  to 40, and reads Idle with no enemy, because Advance has
  no rally fallback; Vanguard DefendCore, rally (10,5,10),
  deploys at the same player cell, sharing it, ore 40 to 30,
  and reads Marching before any tick, because away from the
  core with no attacker it is on its return march, not at
  its post. A general re-rally attempted on each, both to
  (20,5,20), is refused fail-closed: both keep rally
  (10,5,10), position (16,5,16), health 90, and their
  readings, and ore stays 30 — because neither rally moves
  its squad, a field-only change would have been a false
  move order, not an action.
- The cap device, stated plainly so it is not mistaken for
  a feature: both inert squads are then removed by staged
  lethal damage (90 damage each, their full Vanguard health)
  and one world tick, leaving 0 live squads, ore still 30,
  no refund. A death frees a cap slot, as Slice 11 proved;
  a re-rally does not, and no recall or dismiss API is used
  or created here — the staged deaths exist only because
  the cap is 3 and five roles cannot stand at once, and a
  reader who quotes them as a dismiss action has misread
  the demo.
- Phase 2, the HoldGround action: Vanguard HoldGround,
  rally (12,5,16), deploys at the player (16,5,16), ore 30
  to 20, and marches west (15,16), (14,16), (13,16),
  (12,16) over four ticks, reading Holding at its first
  rally. It then accepts a free general re-rally to
  (20,5,16) — rally changed, still at (12,5,16) (no
  teleport), reading Marching at once, ore still 20, live
  squads still 1 — and marches east (13,16) through
  (20,16) over eight ticks, reading Holding at its new
  rally.
- Phase 3, the moving Patrol action: Vanguard Patrol,
  rally (20,5,20), deploys at the player (16,5,16), ore 20
  to 10, and marches (17,16), (17,17), (18,17), (18,18),
  (19,18), (19,19), (20,19), (20,20) over eight ticks,
  reaching its rally reading Patrolling, while the
  HoldGround stays Holding at (20,5,16). One loop tick
  turns it onto its loop: (21,5,20), waypoint 1, target
  (23,5,20) — its rally plus radius 3. It then accepts a
  free general re-rally mid-loop to (26,5,20) — rally
  changed, waypoint index 0, refusal counter 0, still at
  (21,5,20) (no teleport), reading Patrolling, ore still
  10 — and marches east (22,20) through (26,20) over five
  ticks, then turns onto its new loop: (27,5,20),
  waypoint 1, target (29,5,20) — the new rally plus radius
  3, with radius and waypoint count still 3 and 5, not
  retuned.
- Phase 4, the Step 1 case subsumed in one field: Vanguard
  Patrol, rally (12,5,16), deploys at the player (16,5,16),
  ore 10 to 0, cap now full at 3 live squads. The
  route-blocked shape is staged after this deploy, as in
  Slice 18: stone at (15,5,16) west, (16,5,17) south, and
  (16,5,15) north of the squad's cell, with the east step
  cell (17,5,16) left open — the exact Step 1 geometry
  translated to the player cell, so the proof is the same
  shape, not a new one. Over five ticks the stuck patrol
  counts refusals 1 through 5, its waypoint cycling 1, 2,
  3, 4, then back to 0, reading Patrolling through four
  refusals and Stuck at five, unmoved at (16,5,16), while
  the HoldGround reads Holding at (20,5,16) and the moving
  Patrol walks (28,20), (29,20), (28,20), (28,21),
  (27,21) beside it — the westward turn on the third tick
  is the shared tie-break (X before Z on an equal gap),
  not a new movement rule.
- Stuck-before-save load copy: the field saved at Stuck
  loads with all three squads, the stuck patrol reading
  Patrolling at index 0 with its counter at 0 and its old
  rally (12,5,16) kept, and over a fresh five ticks it
  counts refusals 1 through 5 again and reads Stuck again,
  unmoved at (16,5,16) — the load rule Step 1 left standing,
  proved in composition rather than re-asserted.
- The decided action on the main field, through the
  general gate: the stuck patrol (index 2) accepts a free
  general re-rally to (22,5,16) — rally changed, waypoint
  index 0, refusal counter 0, reading Patrolling at once,
  still at (16,5,16) (no teleport), ore still 0 (no charge,
  no refund), live squads still 3 (no cap slot freed).
  The demo uses the general gate here deliberately,
  because Slice 21 subsumes Slice 20: Step 1's
  gates-agreement proof already shows a stuck Patrol
  succeeding under both gates with the identical outcome,
  and both gates are free, so the ore difference between
  them is zero — and ore unchanged at 0 is that difference,
  shown, not assumed. A fourth paid deploy, DefendCore
  Heavy at (10,5,10), is then refused at the full cap, ore
  still 0, squads still 3, stuck rally still (22,5,16) —
  the re-rallies did not make room for a new squad.
- The march, tick by tick: the re-rallied patrol steps
  east (17,16), (18,16), (19,16), (20,16), (21,16), (22,16)
  over six ticks, waypoint 0 throughout with its target
  the new rally itself, while the moving Patrol walks
  (27,22), (26,22), (26,23), (25,23), (25,22), (24,22) and
  the HoldGround stays Holding at (20,5,16). One further
  tick turns it onto the new loop: (23,5,16), waypoint 1,
  target (25,5,16) — the new rally plus radius 3.
- Save/load round-trip after the action: the field saved
  at (23,5,16) loads with all three squads, the re-rallied
  patrol keeping its new rally (22,5,16) — it does not
  revert to (12,5,16) — at index 0 with its counter at 0,
  reading Patrolling, the HoldGround at (20,5,16) reading
  Holding with its new rally (20,5,16), and the moving
  Patrol at (24,5,21) reading Patrolling with its new
  rally (26,5,20). Loaded tick 1 steps the re-rallied
  patrol back to its new rally (22,5,16) at waypoint 0,
  because index 0 targets the rally itself under the load
  rule — stated here so a reader does not misread a step
  toward the rally as a step away from the loop — while
  the moving Patrol steps to (25,5,21) at waypoint 0 and
  the HoldGround stays Holding.

Totals, in the demo's own terms: 50 EmberOre spent on five
paid deploys, 0 charged by any re-rally, 0 refunded by any
re-rally or by the staged deaths, 3 live squads at the end
(and never more than 3 at once), 2 inert refusals, 1 refused
fourth deploy, 8 ticks for the HoldGround to reach its new
rally, 5 ticks for the moving Patrol to reach its new rally,
6 ticks for the stuck patrol to reach its new rally, 1 tick
each to turn onto a new loop, and 2 load round-trips (one
stuck-before-save, one after the action) that both preserve
the field they were taken from. The new scenario's save is
44,040 chars, quoted here as the Slice 21 entry, not as a
change to any earlier save, and not as a claim about why it
differs from any other length.

## What the review verified this run

- Full test suite re-run before writing: **3607 passed, 0
  failed** (was 3402; +205, no failures on the final runs).
  The Step 2 assertions are composed-integration tests
  mirroring the demo layout above, inserted before the test
  runner's summary so they execute — the Slice 13 Step 4
  lesson, that tests appended after the runner's return do
  not run, is observed here, and the count rise from 3402
  to 3607 is the check that they did. A placement error
  during this run first put the new section after the
  summary print, so the printed count stayed 3402 while
  the new assertions ran after it; the section was moved
  before the summary and the suite re-run to 3607/3607
  before any push, so the pushed count is the count the
  runner itself printed.
- Headless demo re-run, save lengths as a no-core-change
  check for the earlier scenarios and as the new entry for
  Slice 21: Slice 1 unchanged (save 43,881 chars); Slice 2
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
  chars); Slice 20 Step 2 scenario unchanged (save 44,041
  chars); Slice 21 Step 2 scenario new (save 44,040 chars).
  A changed earlier save length would have meant a save
  rule moved; none did. Slice 21 adds no save field — the
  re-rally reuses the saved rally field — so the unchanged
  earlier lengths are consistent with the decisions, not
  just coincident with them.
- Slice 21 gate, checked against `docs/slice-21-scope.md`:
  every step pushed with passing tests (for Step 1, with
  the suite risen by the proof-set tests, never standing
  still on an action step, and for Step 2, with the suite
  risen by the composed tests); the Step 1 action decision
  is recorded with rationale in DECISIONS.md, with its own
  Windows checklist item (item 40); no movement,
  targeting, fire, delivery, precedence, conflict, rating,
  cost, phase, or save-format rule changed in any step
  except the action itself and its explicitly justified
  free economy answer; no material, marker, light, fog,
  model, or HUD rule changed in any step except the
  snapshot activity readings Step 1 justifies for the
  moment after the action (Marching, Patrolling, and the
  unchanged refusal words); every earlier demo save length
  unchanged, with the new Slice 21 length quoted as its own
  entry; and this review records the verdict honestly,
  separating headless-tested action claims from the
  Windows-checklist action observation, including that
  Slice 21 is still not playtested and not Unity-verified.
  The Slice 1 director playtest gate is still pending
  throughout and is not closed by Slice 21.
- Checklist relations, stated so a later reader does not
  miscount them: item 40 is the general re-rally
  observation (a Holding squad marching again, a moving
  Patrol's loop moving, and inert-doctrine refusals with
  no marker move). Item 40 extends item 39 (whose
  stuck-patrol readings stand unchanged), and item 40 does
  NOT supersede item 39, item 37, item 29, or
  orbital-command item 13. This review adds no new item,
  because the review exercises no behaviour item 40 does
  not already cover. The next free playtest-gate number
  remains 41. Item 40 is also honestly pending its own
  binding: the proposed K binding, widened in Step 1 to
  eligible HoldGround and Patrol squads, is a comment in
  `FpsInputAdapter.cs` only, not implemented, so the
  observation cannot even be attempted until a later
  adapter step implements it — and the same condition now
  covers item 39's stuck observation through that wider
  proposal, which is why item 40 extends rather than
  replaces it.
- One claim split stated plainly, so it is not over-claimed
  later: the headless evidence for Slice 21 is squad
  states before and after an action for staged layouts,
  refusal counts, waypoint indices, rally values, ore and
  cap counts across five paid deploys, two staged deaths,
  three re-rallies, and one refused fourth deploy, march
  positions across ticks, and two save/load round-trips
  that preserve the field, plus a demo that prints those
  values for the field in sequence. None of that proves a
  player can press K, aim at an eligible marker, pick a new
  rally, and watch the rally marker move in a built scene,
  or that a player understands a re-rally as a move order
  while looking at the field. That is a Windows
  observation (item 40, extending item 39), recorded in
  DECISIONS.md when observed, not closed by a test count.
  The blocked-loop limit is proved the same way Step 1
  proved it — a re-rally corrects the loop, it does not
  march a squad through a wall — and this review does not
  re-prove the enclosed shape in composition: the composed
  stuck case is the route-blocked shape that marches, and
  the enclosed shape's re-stick remains Slice 20 Step 1's
  proof, not a composed claim.
- No Slice 21 step duplicates logic in a Unity adapter.
  The action lives in core `WorldZone.TryReRallySquad`
  beside unchanged `TryReRallyStuckPatrol` and the thin
  `Simulation` passthrough, and renders through the
  existing snapshot and marker rules — Step 1 added only
  a proposed-binding comment to `FpsInputAdapter.cs`, and
  Step 2 edits no adapter file at all. There is no second
  re-rally rule in any adapter that could drift from the
  core rule.
- No Slice 21 step changed a saved value's meaning: no
  save field was added, no block, kind, doctrine, source,
  or consumer value moved, and every earlier demo save
  length above is unchanged. The one value the action does
  change in a save — a squad's rally — already existed as
  a saved field, so a re-rallied squad loads with its new
  rally and an old save loads unchanged.
- Hard limits held: 3D real-time only, no turn-based
  mechanics, no twin-stick Asteroids loop, no copied assets,
  lore, code, or trademarks. Slice 21 adds no block, kind,
  doctrine, source, or consumer, so there is nothing new
  in it that could copy one.

## Deliberately NOT in Slice 21 (still open, not forgotten)

- A radius, waypoint-count, or loop-shape change. The loop
  stays 3 and 5, flat at rally height, unless a later
  decision on recorded evidence changes it — no Step 1 or
  Step 2 proof shows the radius caused a block, so no
  radius finding is carried as a retune here.
- A movement, targeting, or fire rule change.
  MoveTowardRally, MoveForPatrol's sidestep, skip, and
  Stuck threshold, Advance's chase, and Defend Core's
  chase-or-return stand; a re-rallied squad still fires
  under its doctrine's existing rule, and no pathfinder, no
  hold, no wider sidestep, no rally fallback for Advance,
  and no rally consultation for Defend Core is added here.
- A delivery, precedence, conflict, rating, cost, or phase
  rule change beyond the economy question Step 1 answered
  explicitly (a free general re-rally, no refund question
  because there is no recall). The deploy price (10) and
  the cap (3) otherwise stand, and no power, consumer, or
  save-format rule moves.
- A new block, kind, doctrine, source, or consumer, a
  balance pass, or a price or cap change disguised as a
  repositioning. Slice 21 is one generalised action and its
  review, and the core stands exactly as Slice 20 left it
  except for the action itself.
- A material, shader, marker, light, fog, or view model
  change, or a HUD redesign. The Slice 16 surface renders
  whatever Slice 21 decides; a step that re-paletted a
  marker or added a HUD line to make its answer look
  right would have confused the surface with the action.
- A general squad-command system. Selecting, ordering, or
  commanding squads beyond the one decided re-rally action
  — including a general recall, a dismiss, a follow order,
  or a formation command — is a larger slice, carried
  after Slice 21, not patched in beside a generalisation.
- A recall or dismiss API of any kind, for any doctrine.
  Step 1 carried recall as the first Slice 22 candidate,
  and no step in this slice adds one quietly afterwards —
  including, explicitly, the staged deaths in the Step 2
  demo, which are a cap-staging device, not a dismiss
  action a player can invoke.
- A scene-proof observation presented as a headless result.
  Items 25–27 and 30–40 are Windows observations, recorded
  in DECISIONS.md when observed, not closed by a document,
  a script, or a test count.
- A scene file (`.unity`), an imported asset, a texture, or
  a shader file written by a headless run and claimed as a
  built scene. Scene assembly remains Windows checklist
  work.
- A copied asset, model, skyline, HUD layout, or
  trademarked look from any other game.
- Windows Unity verification itself. It remains open for
  Slices 1–21 alike and is blocked on Unity 6000 on Windows,
  not on anything this slice can do headlessly.
- A Slice 22 scope. Slice 22 is scoped after the Slice 21
  review, not inside it.

## Gaps this review names, in priority order

1. **Windows Unity verification, Slices 1–21.** The
   checklists in `docs/slice-01-playtest-gate.md`,
   `docs/orbital-view.md`, `docs/orbital-command.md`, and
   `docs/powered-presence.md` remain the verification plan.
   Slice 21 adds item 40 to that plan, alongside the
   still-open items 25, 26, 27, and 30–39. None of these
   items is claimed verified by any test count in this
   review, and scene assembly is still owed in Unity on
   Windows, not in this repo. Item 40 has the extra
   condition that its proposed K binding is not yet
   implemented in an adapter, so the observation cannot
   even be attempted until a later adapter step implements
   it.
2. **The Slice 1 director playtest gate is still pending.**
   Rob has not yet played Slice 1 in Unity on Windows or
   answered the gate question (fun without extra systems?).
   Slice 21 does not close that gate, and its test count
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
   its own changelog entry. Slice 21 adds a reason this
   matters: a re-rallied squad's rally marker must read
   under that dusk light when it moves to a new rally, and
   only item 40's observation can say whether a player
   reads the moved marker as the move order it is.
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
   (item 38), the decided stuck re-rally reads (item 39),
   and the decided general re-rally reads (item 40), are
   all still to be observed on Windows. If any observation
   fails, that failure is a fix step, recorded as its own
   decision — not a reason to re-scope the proof layer
   blindly, and not closed by this review.
5. **The patrol route's remaining questions, as decided,
   still leave open decisions a later step may take.**
   Carried from the Slice 21 scope's ranked candidates: a
   recall or dismiss as its own decision, with its refund
   (none, full 10, or partial) and confirmation questions
   answered, and its cap effect proved, or a radius or
   waypoint first-pass value revisited on evidence (a Step
   1 finding that the radius caused a block — none was
   found — or a Windows observation that a loop does not
   fit its ground) — each with its own tests, not promised
   here. A general re-rally price on Windows playtest
   evidence, if free repositioning proves abusive kiting,
   is likewise a later decision, recorded as a gap in Step
   1, not set here.

**Closed by this slice, not carried:** the general re-rally
question in the Slice 20 review's first-ranked remainder —
what a player can do to re-rally a live squad that is not
a stuck Patrol — is now decided, with tests, with a Windows
checklist item for the action observation, and proved
together with its inert-doctrine, HoldGround, moving-Patrol,
stuck-Patrol, economy, and save/load controls in one demo,
so the last Slice 20 gap 5 remainder is closed in that
sense, not left in the numbered gaps above. The observation
in gaps 1 and 4 above is a remainder about whether the
decided action works in a built scene, not a reopening of
whether the decision exists.

## Slice 21 verdict (headless)

Slice 21 is **reviewed and closed as one general re-rally
action decided with tests and proved together in one demo (a
live HoldGround or Patrol squad gets a free general re-rally,
stuck or not, by squad list index, with its rally changed,
a Patrol's waypoint index and refusal counter reset to 0,
its position, health, doctrine, and kind untouched, no ore
charged, no cap slot freed, and its march to the new rally
and turn onto the new loop proved beside inert-doctrine
refusals with nothing changed, a stuck Patrol re-rallied
through the same free gate, a fourth deploy refused at the
full cap, and two save/load round-trips that preserve the
field — and the whole field, saved and loaded, re-derives
the same readings, stable across ticks except for the two
staged deaths that are explicitly a cap-staging device, not
a player action)**. It is not playtested, not Unity-verified,
and not a visual result, and its action claims must not be
quoted as if the test count covered the scene action in
item 40.
**Slice 22 is not scoped by this review** — it is scoped
afterwards, the same way this document follows the Slice 20
review rather than preceding it.

## Ranked candidates carried after Slice 21 (starting point, not promised)

Carried from the 0.87.0 scope, in the same order, with the
Slice 21 work taken:

1. The patrol route's remaining questions — a recall or
   dismiss as its own decision, with its refund (none, full
   10, or partial) and confirmation questions answered, and
   its cap effect proved, or a radius or waypoint first-pass
   value revisited on evidence (a Step 1 finding that the
   radius caused a block, or a Windows observation that a
   loop does not fit its ground) — each with its own tests,
   not promised here.
2. Any Slice 13 remainder that Slice 21 does not itself
   settle as an observation (the vertex-colour material
   observation, the MeshRenderer wiring observation, and
   the marker-occlusion observation in the Slice 20 review's
   gap 4, plus the dusk and view model observations in items
   33 and 34, plus the tint, count, patrol, and re-rally
   observations in items 35, 36, 37, 39, and 40) — if those
   observations are still unmade when the review is written,
   they remain observations to be made when Windows
   verification runs, not steps to be re-scoped blindly,
   and not promised here.
3. First-pass graphics values retuned only on Windows
   evidence (dusk azimuth, fog contrast, view model offset,
   static weapon-agnostic model) — a retuning step is
   justified by an observation, not by a headless
   re-decision, and is not promised here.
