# Forgeborne — Slice 22 Review

**Status: REVIEWED HEADLESSLY 2026-10-31, tests 4109/4109.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
16, 17, 18, 19, 20, and 21. This review closes Slice 22 as a reviewed
whole; it does not claim Slice 22 is playtested, Unity-verified,
or a visual result.**

## Review question

Does Slice 22 settle the one removal remainder Slice 21
deliberately carried — since Slice 21 decides repositioning
but leaves a player with no action except death to free a
cap slot, what, if anything, the player can do to remove a
live squad, at what refund and cap effect, with what
confirmation, targeting, and timing, and with what
observable effect per doctrine — as its own decision with
its own tests, then proved together with its controls in one
demo, without a movement, targeting, fire, delivery,
precedence, conflict, rating, cost, or phase rule change
beyond the economy answers Step 1 justifies explicitly, a
price or cap change, a radius or waypoint change, a
material, shader, marker, light, fog, or view model change,
a HUD redesign, a new block, kind, doctrine, source, or
consumer, a general squad-command system, a reserve or
bench system, a balance pass, or a claim of Unity
verification made from a headless run? The answer headlessly
is yes for the action as decided, with the split this review
keeps throughout: the action is tested, the scene use of
the action is still owed on Windows, and the one staged
death that lets the dead-refusal proof run is staging, not
a player action.

## What Slice 22 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.90.0 scope | `docs/slice-22-scope.md`: Slice 22 scoped as the removal slice, in order — the removal decision first, then demo + review. Radius 3 and five waypoints carried, not re-decided. A reserve is not smuggled into the removal step. Ranked Slice 23 candidates recorded, not promised | Decision document only, no system built by it | Tests 3607/3607 unchanged |
| 0.91.0 Step 1 | Removal decided: DISMISS, not recall — any live squad of any doctrine and any kind leaves the field at once, by squad list index, for refund NONE and one freed cap slot, with confirmation in the adapter only, by proposal (X while not picking, crosshair on any live marker, Return to confirm, Escape to cancel — a comment only). List-shift answered as player responsibility: re-read the snapshot before acting again. Timing immediate, not next-tick. Picker-open is an independent state, not a core refusal. Stuck Patrol may be dismissed or re-rallied free, as two distinct choices. Deploy price (10) and cap (3) stand | `WorldZone.TryDismissSquad` (gate, fail-closed) with a thin `Simulation` passthrough, following the re-rally pattern; `OrbitalCommand` / `OrbitalView` / HUD / marker rules untouched except the snapshot entry disappearing for a removed squad under the existing rule | Tests 3828/3828 (was 3607; +221, including the paid chain, the twelve-combination doctrine x kind matrix, the named holding, marching, patrolling, chasing, returning, and guarding cases, the stuck dismiss beside a free re-rally, every refusal class, the list-shift proof, the immediate-timing proof, the picker-independence proof, and the save/load proof); new playtest-gate item 41, standing beside items 40, 39, 37, 29, and orbital-command item 13, superseding and extending none of them; demo unchanged, Slice 21 save 44,040 chars, Slice 20 save 44,041 chars, Slice 19 save 43,965 chars, Slice 18 save 44,015 chars, Slice 15 save 44,016 chars, Slice 17 save 43,974 chars |
| 0.92.0 Step 2 | This review, plus a new composed demo scenario staging the decided removal together with its controls in one Simulation (see below) | Review document plus CHANGELOG and DECISIONS entries; no core file, no adapter file, no material, marker, light, fog, or view model value, no checklist item added | Tests 4109/4109 (was 3828; +281 composed-integration assertions mirroring the demo); every earlier demo save length unchanged; new Slice 22 scenario save 43,990 chars, quoted as its own entry below, not as a change to any earlier save |

## The Step 2 demo, in numbers

One Simulation, seven paid deploys in sequence but never more
than 3 live squads at once, no enemy staged for a fire claim —
the demo proves the removal, its refusals, its economy, its
rescue distinction, and its movement, and nothing else:

- Phase 1, the march to readings: Vanguard HoldGround,
  rally (12,5,16), deploys at the player (16,5,16), ore 70
  to 60, and marches west (15,16), (14,16), (13,16),
  (12,16) over four ticks, reading Holding at its first
  rally. Recon Patrol, rally (20,5,20), deploys at the
  player, ore 60 to 50, and marches (17,16), (17,17),
  (18,17), (18,18), (19,18), (19,19), (20,19), (20,20)
  over eight ticks, reaching its rally reading Patrolling.
  Heavy Advance, rally (10,5,10), deploys at the player,
  ore 50 to 40, cap now full at 3, and stays at (16,5,16)
  reading Idle with no enemy, because Advance has no
  rally fallback. One loop tick turns the Patrol onto its
  loop: (21,5,20), waypoint 1, target (23,5,20) — its
  rally plus radius 3.
- Phase 2, the refusals before any removal: a dismiss at
  index 3 and a dismiss at index -1 are both refused
  fail-closed — ore still 40, squads still 3, hold still
  at (12,5,16), patrol still at (21,5,20), advance still
  at (16,5,16) — because the index rule, not the squad,
  refuses them.
- Phase 3, the Advance removal and the freed slot: the
  Heavy Advance at index 2 dismisses — live squads 2,
  ore still 40 (refund none), hold still at (12,5,16)
  HP 90 reading Holding, patrol still at (21,5,20)
  HP 40 reading Patrolling, nothing else changed. A
  Heavy DefendCore, rally (10,5,20), then redeploys
  through the picker at the freed slot — ore 40 to 30,
  a fresh 10 at the unchanged price, cap full again at
  3, deploying at the player (16,5,16) and reading
  Marching before any tick, because away from the core
  with no attacker it is on its return march, not at
  its post. The redeploy is the cap proof, not a claim
  about the DefendCore's destination: its rally is a
  deployment formality the picker still requires, and
  the squad never goes to it.
- Phase 4, the HoldGround removal: the Vanguard
  HoldGround at index 0 dismisses — live squads 2, ore
  still 30 (refund none), patrol still at (21,5,20)
  HP 40 reading Patrolling, defendcore still at
  (16,5,16) HP 100 reading Marching, nothing else
  changed. Two doctrines are now covered by dismissals
  in composition, and the field still holds two squads
  whose readings did not move.
- Phase 5, the Step 1 case staged in one field: Vanguard
  Patrol, rally (12,5,16), deploys at the player
  (16,5,16), ore 30 to 20, cap now full at 3 live
  squads. The route-blocked shape is staged after this
  deploy, as in Slice 18: stone at (15,5,16) west,
  (16,5,17) south, and (16,5,15) north of the squad's
  cell, with the east step cell (17,5,16) left open —
  the exact Step 1 geometry translated to the player
  cell, so the proof is the same shape, not a new one.
  Over five ticks the stuck patrol counts refusals 1
  through 5, its waypoint cycling 1, 2, 3, 4, then back
  to 0, reading Patrolling through four refusals and
  Stuck at five, unmoved at (16,5,16), while the moving
  Patrol walks (22,20), (23,20), (22,20), (22,21),
  (21,21) beside it — the westward turn on the third
  tick is the shared tie-break (X before Z on an equal
  gap), not a new movement rule — and the Defend Core
  marches toward the core (17,16), (17,17), (18,17),
  (18,18), (19,18), its diagonal return march, not a
  rally march.
- Pre-removal save: the field saved at Stuck loads with
  all three squads, the Defend Core at (19,5,18) reading
  Marching, and the stuck patrol reading Patrolling at
  index 0 with its counter at 0 and its old rally
  (12,5,16) kept — the load rule Step 1 left standing,
  proved in composition for presence. The load copy is
  read, not ticked: its re-stick after a fresh full
  circuit is Slice 20 Step 1's proof, cited, not
  re-claimed as a composed result, and the enclosed
  stuck shape's re-stick likewise remains Step 1's
  proof, not a composed claim — the composed stuck case
  is the route-blocked shape that marches.
- The rescue beside the removal, the distinction the
  slice exists to keep visible: the stuck patrol
  (index 2) accepts a FREE re-rally through the stuck
  gate to (22,5,16) — rally changed, waypoint index 0,
  refusal counter 0, reading Patrolling at once, still
  at (16,5,16) (no teleport), ore still 20 (no charge,
  no refund), live squads still 3 (no cap slot freed).
  The demo uses the stuck gate here deliberately,
  because the distinction is between a rescue that
  keeps the squad and a removal that ends it: the
  general gate would give the identical outcome for a
  stuck Patrol (Step 1's gates-agreement proof, cited,
  not re-staged), and using the stuck gate names the
  rescue as the rescue. Then the Heavy DefendCore at
  index 1 dismisses beside the rescued patrol — live
  squads 2, ore still 20 (refund none), patrol still
  at (21,5,21) HP 40 rally (20,5,20) reading Patrolling,
  rescued patrol still at (16,5,16) HP 90 rally
  (22,5,16) reading Patrolling, nothing else changed —
  so the rescue stays free and available beside the
  removal, and a player is never forced to pay, in ore
  or in a lost squad, for a rescue the earlier slices
  gave free. Three doctrines are now covered by
  dismissals in composition.
- Post-removal save: the field saved after the
  DefendCore dismiss loads with 2 squads and no
  DefendCore entry, the Patrol keeping rally (20,5,20)
  and the rescued patrol keeping its new rally
  (22,5,16), ore still 20 — the removal persists, and
  the pre-removal save above still loads with the
  DefendCore present, so the two saves together prove
  both halves of the save claim, with no new save
  field: a removal deletes a squad entry, and the
  review says so rather than quoting the suite as
  unchanged.
- The march, tick by tick: the rescued patrol steps
  east (17,16), (18,16), (19,16), (20,16), (21,16),
  (22,16) over six ticks, waypoint 0 throughout with
  its target the new rally itself, while the moving
  Patrol walks (21,22), (20,22), (20,23), (19,23),
  (19,22), (18,22). One further tick turns it onto the
  new loop: (23,5,16), waypoint 1, target (25,5,16) —
  the new rally plus radius 3, with radius and
  waypoint count still 3 and 5, not retuned.
- Phase 6, the Patrol removal and the dead refusal:
  the moving Recon Patrol at index 0 dismisses — live
  squads 1, ore still 20 (refund none), rescued patrol
  still at (23,5,16) HP 90 rally (22,5,16) waypoint 1
  reading Patrolling, nothing else changed. All four
  doctrines are now covered by dismissals in
  composition. A sacrificial Vanguard HoldGround,
  rally (20,5,16), then deploys through the picker —
  ore 20 to 10, cap 2 — and is killed without a tick
  (90 damage, its full Vanguard health) so that a
  dismiss at its index is refused as a dead squad:
  accepted False, list still 2, ore still 10, rescued
  patrol still at (23,5,16) HP 90. The staged death is
  stated plainly so it is not mistaken for a feature:
  a demo TakeDamage call is not a player action, a
  player cannot damage their own squad, and the dead
  squad is swept by the next tick, not by the player.
  One sweep tick does exactly that — live squads 1,
  ore still 10, rescued patrol stepping to (24,5,16)
  waypoint 1 reading Patrolling — while the sweep, not
  a dismiss, removes the dead squad.
- Phase 7, the empty field and the rebuild: the rescued
  patrol itself dismisses at index 0 — live squads 0,
  ore still 10 (refund none), snapshot listing 0
  squads. A dismiss at index 0 on the empty field is
  then refused fail-closed — squads still 0, ore still
  10, nothing changed. A final Vanguard HoldGround,
  rally (20,5,16), redeploys through the picker with
  the last 10 ore — ore 10 to 0, cap 1, deploying at
  the player (16,5,16) — and marches east (17,16),
  (18,16), (19,16), (20,16) over four ticks, reading
  Holding at its rally, rebuilding the emptied field
  at the unchanged price. The rebuild is a closing
  proof that an emptied field is recoverable by the
  paid path, not a claim that a dismiss is reversible:
  every dismissed squad in this demo is gone, and the
  rebuilt squad is a new paid deploy, not a return.
- Save/load round-trip after the rebuild: the field
  saved at (20,5,16) loads with 1 squad, the rebuilt
  HoldGround at (20,5,16) rally (20,5,16) reading
  Holding, ore 0, and loaded tick 1 leaves it Holding
  at its rally, as before the save.

Totals, in the demo's own terms: 70 EmberOre spent on seven
paid deploys, 0 charged by the free re-rally, 0 refunded by
any dismiss or by the staged death, 3 live squads at most at
once, 5 dismissals in composition (one per doctrine, plus
the rescued patrol as the fifth), 4 refusal classes proved
(out-of-range, negative, dead, empty), 1 free stuck re-rally
beside 1 removal, 1 refused fourth-deploy equivalent in
Step 1's proof set (no refused deploy is staged here,
because every composed deploy succeeds at a non-full cap
or a freed slot — the full-cap refusal remains Step 1's
paid-chain proof, cited, not re-staged), 6 ticks for the
rescued patrol to reach its new rally, 4 ticks for the
rebuilt HoldGround to reach its rally, 1 tick each to turn
onto a new loop, and 3 load round-trips (one pre-removal,
one post-removal, one after the rebuild) that all preserve
the field they were taken from. The new scenario's save is
43,990 chars, quoted here as the Slice 22 entry, not as a
change to any earlier save, and not as a claim about why it
differs from any other length — a removal deletes a squad
entry, so a save taken with fewer squads is shorter for
that reason, and the review states the per-doctrine effect
plainly: no doctrine changes a save except by the presence
or absence of its squad entry, and no save field is added.

## What the review verified this run

- Full test suite re-run before writing: **4109 passed, 0
  failed** (was 3828; +281, no failures on the final runs).
  The Step 2 assertions are composed-integration tests
  mirroring the demo layout above, inserted before the test
  runner's summary so they execute — the Slice 13 Step 4
  lesson, that tests appended after the runner's return do
  not run, is observed here, and the count rise from 3828
  to 4109 is the check that they did.
- Headless demo re-run, save lengths as a no-core-change
  check for the earlier scenarios and as the new entry for
  Slice 22: Slice 1 unchanged (save 43,881 chars); Slice 2
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
  chars); Slice 21 Step 2 scenario unchanged (save 44,040
  chars); Slice 22 Step 2 scenario new (save 43,990 chars).
  A changed earlier save length would have meant a save
  rule moved; none did. Slice 22 adds no save field — a
  removal deletes a squad entry, and the saved rally field
  is reused by the rescue — so the unchanged earlier
  lengths are consistent with the decisions, not just
  coincident with them.
- Slice 22 gate, checked against `docs/slice-22-scope.md`:
  every step pushed with passing tests (for Step 1, with
  the suite risen by the proof-set tests, never standing
  still on an action step, and for Step 2, with the suite
  risen by the composed tests); the Step 1 removal decision
  is recorded with rationale in DECISIONS.md, with its own
  Windows checklist item (item 41); no movement,
  targeting, fire, delivery, precedence, conflict, rating,
  cost, phase, or save-format rule changed in any step
  except the removal itself and its explicitly justified
  refund-none and cap-freed economy answers; no material,
  marker, light, fog, model, or HUD rule changed in any
  step except the snapshot entry disappearing for a
  removed squad, which Step 1 justifies under the existing
  marker rule; every earlier demo save length unchanged,
  with the new Slice 22 length quoted as its own entry;
  and this review records the verdict honestly,
  separating headless-tested removal claims from the
  Windows-checklist removal observation, including that
  Slice 22 is still not playtested and not Unity-verified.
  The Slice 1 director playtest gate is still pending
  throughout and is not closed by Slice 22.
- Checklist relations, stated so a later reader does not
  miscount them: item 41 is the dismiss observation (a
  squad leaving the field, its marker vanishing under the
  existing rule, a freed slot taking a paid redeploy, and
  the proposed X confirm step). Item 41 stands beside
  item 40 (general re-rally), item 39 (stuck re-rally),
  item 37 (patrol route), item 29 (patrol), and
  orbital-command item 13 (patrol command) — it
  supersedes none of them and extends none of them,
  because a removal is a different action from a
  re-rally, not a wider form of one. This review adds no
  new item, because the review exercises no behaviour
  item 41 does not already cover. The next free
  playtest-gate number remains 42. Item 41 is also
  honestly pending its own binding: the proposed X
  binding is a comment in `FpsInputAdapter.cs` only, not
  implemented, so the observation cannot even be
  attempted until a later adapter step implements it —
  and until then the core call removes on a single call,
  which is why the play path must expose dismiss only
  through the proposed binding.
- One claim split stated plainly, so it is not over-claimed
  later: the headless evidence for Slice 22 is squad
  states before and after a removal for staged layouts,
  refusal counts, waypoint indices, rally values, ore and
  cap counts across seven paid deploys, five dismissals,
  one free re-rally, and four refusal classes, march
  positions across ticks, and three save/load round-trips
  that preserve the field, plus a demo that prints those
  values for the field in sequence. None of that proves a
  player can press X, aim at a live marker, confirm with
  Return, watch the marker vanish, and understand the
  freed slot as a correction rather than a loss while
  looking at the field. That is a Windows observation
  (item 41), recorded in DECISIONS.md when observed, not
  closed by a test count. The blocked-loop limit is
  proved the same way Step 1 proved it — a re-rally
  corrects the loop, it does not march a squad through a
  wall — and this review does not re-prove the enclosed
  shape in composition: the composed stuck case is the
  route-blocked shape that marches, and the enclosed
  shape's re-stick remains Slice 20 Step 1's proof, not a
  composed claim.
- No Slice 22 step duplicates logic in a Unity adapter.
  The removal lives in core `WorldZone.TryDismissSquad`
  beside unchanged `TryReRallyStuckPatrol` and
  `TryReRallySquad` and the thin `Simulation`
  passthrough, and renders through the existing snapshot
  and marker rules — Step 1 added only a proposed-binding
  comment to `FpsInputAdapter.cs`, and Step 2 edits no
  adapter file at all. There is no second removal rule in
  any adapter that could drift from the core rule.
- No Slice 22 step changed a saved value's meaning: no
  save field was added, no block, kind, doctrine, source,
  or consumer value moved, and every earlier demo save
  length above is unchanged. The one value a removal
  changes in a save — the presence of a squad entry —
  is the deletion the decision justifies, so a
  post-removal save loads without the squad and a
  pre-removal save loads with it, and an old save loads
  unchanged.
- Hard limits held: 3D real-time only, no turn-based
  mechanics, no twin-stick Asteroids loop, no copied assets,
  lore, code, or trademarks. Slice 22 adds no block, kind,
  doctrine, source, or consumer, so there is nothing new
  in it that could copy one.

## Deliberately NOT in Slice 22 (still open, not forgotten)

- A radius, waypoint-count, or loop-shape change. The loop
  stays 3 and 5, flat at rally height, unless a later
  decision on recorded evidence changes it — no Step 1 or
  Step 2 proof shows the radius caused a block, so no
  radius finding is carried as a retune here.
- A movement, targeting, or fire rule change.
  MoveTowardRally, MoveForPatrol's sidestep, skip, and
  Stuck threshold, Advance's chase, and Defend Core's
  chase-or-return stand; a remaining squad still fires
  under its doctrine's existing rule, and no pathfinder, no
  hold, no wider sidestep, no rally fallback for Advance,
  and no rally consultation for Defend Core is added here.
- A delivery, precedence, conflict, rating, cost, or phase
  rule change beyond the economy question Step 1 answered
  explicitly (refund none, cap slot freed). The deploy
  price (10) and the cap (3) otherwise stand, and no power,
  consumer, or save-format rule moves.
- A new block, kind, doctrine, source, or consumer, a
  balance pass, or a price or cap change disguised as a
  removal. Slice 22 is one removal decision and its
  review, and the core stands exactly as Slice 21 left it
  except for the removal itself.
- A material, shader, marker, light, fog, or view model
  change, or a HUD redesign. The Slice 16 surface renders
  whatever Slice 22 decides; a step that re-paletted a
  marker or added a HUD line to make its answer look
  right would have confused the surface with the removal.
- A general squad-command system. Selecting, ordering, or
  commanding squads beyond the one decided removal action
  — including a follow order, a formation command, or a
  general order queue — is a larger slice, carried after
  Slice 22, not patched in beside a removal.
- A reserve, bench, or redeploy-from-reserve system of any
  kind. No step in this slice adds a stored-squad list, a
  bench cap, or a redeploy price quietly afterwards, under
  any name, and a removal must not be presented as a
  recall-to-reserve substitute: a removed squad is gone,
  unless a later slice decides and proves otherwise as a
  named, larger slice — including, explicitly, the staged
  death in the Step 2 demo, which is a cap-staging device
  for the dead-refusal proof, not a dismiss action a
  player can invoke.
- A re-rally repricing. Slice 22 does not charge for a
  re-rally, free or otherwise, to pay for a removal, and
  does not reprice the stuck rescue: the Slice 20 and
  Slice 21 answers stand.
- A scene-proof observation presented as a headless result.
  Items 25–27 and 30–41 are Windows observations, recorded
  in DECISIONS.md when observed, not closed by a document,
  a script, or a test count.
- A scene file (`.unity`), an imported asset, a texture, or
  a shader file written by a headless run and claimed as a
  built scene. Scene assembly remains Windows checklist
  work.
- A copied asset, model, skyline, HUD layout, or
  trademarked look from any other game.
- Windows Unity verification itself. It remains open for
  Slices 1–22 alike and is blocked on Unity 6000 on
  Windows, not on anything this slice can do headlessly.
- A Slice 23 scope. Slice 23 is scoped after the Slice 22
  review, not inside it.

## Gaps this review names, in priority order

1. **Windows Unity verification, Slices 1–22.** The
   checklists in `docs/slice-01-playtest-gate.md`,
   `docs/orbital-view.md`, `docs/orbital-command.md`, and
   `docs/powered-presence.md` remain the verification plan.
   Slice 22 adds item 41 to that plan, alongside the
   still-open items 25, 26, 27, and 30–40. None of these
   items is claimed verified by any test count in this
   review, and scene assembly is still owed in Unity on
   Windows, not in this repo. Item 41 has the extra
   condition that its proposed X binding is not yet
   implemented in an adapter, so the observation cannot
   even be attempted until a later adapter step implements
   it.
2. **The Slice 1 director playtest gate is still pending.**
   Rob has not yet played Slice 1 in Unity on Windows or
   answered the gate question (fun without extra systems?).
   Slice 22 does not close that gate, and its test count
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
   its own changelog entry. Slice 22 adds a reason this
   matters: a removed squad's marker must vanish cleanly
   under that dusk light, and only item 41's observation
   can say whether a player reads a vanishing marker as a
   dismissal rather than a bug.
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
   the decided general re-rally reads (item 40), and the
   decided dismiss reads (item 41), are all still to be
   observed on Windows. If any observation fails, that
   failure is a fix step, recorded as its own decision —
   not a reason to re-scope the proof layer blindly, and
   not closed by this review.
5. **The patrol route's remaining questions, as decided,
   still leave open decisions a later step may take.**
   Carried from the Slice 22 scope's ranked candidates: a
   radius or waypoint first-pass value revisited on
   evidence (a Step 1 finding that the radius caused a
   block — none was found — or a Windows observation that
   a loop does not fit its ground) — with its own tests,
   not promised here. A general re-rally price on Windows
   playtest evidence, if free repositioning proves abusive
   kiting, is likewise a later decision, recorded as a gap
   in Slice 21 Step 1, not set here. A dismiss refund on
   Windows playtest evidence, if slot-only removal proves
   punishing, is likewise a later decision, recorded as a
   gap in Slice 22 Step 1, not set here.

**Closed by this slice, not carried:** the removal question
in the Slice 21 review's first-ranked remainder — what, if
anything, the player can do to remove a live squad, at what
refund and cap effect, with what confirmation, targeting,
and timing — is now decided, with tests, with a Windows
checklist item for the removal observation, and proved
together with its inert-refusal, rescue-distinction,
economy, and save/load controls in one demo, so the last
Slice 21 gap 5 remainder is closed in that sense, not left
in the numbered gaps above. The observation in gaps 1 and 4
above is a remainder about whether the decided removal
works in a built scene, not a reopening of whether the
decision exists.

## Slice 22 verdict (headless)

Slice 22 is **reviewed and closed as one removal action decided
with tests and proved together in one demo (any live squad of
any doctrine and any kind leaves the field at once, by squad
list index, for refund NONE and one freed cap slot, with
confirmation in the adapter only, by proposal, timing
immediate, picker-open an independent state, and a stuck
Patrol still re-ralliable free beside the removal — with
five dismissals in composition, one per doctrine plus the
rescued patrol, four refusal classes proved, a free stuck
re-rally beside a removal, a full-cap removal plus paid
redeploy at the unchanged price, and three save/load
round-trips that preserve the field — and the whole field,
saved and loaded, re-derives the same readings, stable
across ticks except for the one staged death that is
explicitly a dead-refusal staging device, not a player
action)**. It is not playtested, not Unity-verified, and
not a visual result, and its removal claims must not be
quoted as if the test count covered the scene action in
item 41.
**Slice 23 is not scoped by this review** — it is scoped
afterwards, the same way this document follows the Slice 21
review rather than preceding it.

## Ranked candidates carried after Slice 22 (starting point, not promised)

Carried from the 0.90.0 scope, in the same order, with the
Slice 22 work taken:

1. The patrol route's remaining questions — a radius or
   waypoint first-pass value revisited on evidence (a Step 1
   finding that the radius caused a block, or a Windows
   observation that a loop does not fit its ground) — with
   its own tests, not promised here.
2. Any Slice 13 remainder that Slice 22 does not itself
   settle as an observation (the vertex-colour material
   observation, the MeshRenderer wiring observation, and
   the marker-occlusion observation in the Slice 21 review's
   gap 4, plus the dusk and view model observations in items
   33 and 34, plus the tint, count, patrol, and re-rally
   observations in items 35, 36, 37, 39, and 40, plus the
   new item Step 1 adds, item 41) — if those observations
   are still unmade when the review is written, they
   remain observations to be made when Windows verification
   runs, not steps to be re-scoped blindly, and not promised
   here.
3. First-pass graphics values retuned only on Windows
   evidence (dusk azimuth, fog contrast, view model offset,
   static weapon-agnostic model) — a retuning step is
   justified by an observation, not by a headless
   re-decision, and is not promised here.
4. A general re-rally price on Windows playtest evidence, if
   free repositioning proves abusive kiting — a later economy
   decision, recorded as a gap in Slice 21 Step 1, not set
   here.
