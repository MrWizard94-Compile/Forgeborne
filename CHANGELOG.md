# Forgeborne — Changelog

## 0.88.0 — 2026-10-31 — Slice 21 Step 1:
general re-rally decided — free, HoldGround and
Patrol only, Advance and Defend Core refused

- Step taken, decided in this step: Slice 21 Step 1,
  the general re-rally decision. In one quotable
  sentence: a live HoldGround or Patrol squad gets a
  FREE general re-rally, stuck or not — and an Advance
  or Defend Core squad gets a fail-closed refusal,
  because its rally moves nothing.
- Decision, with its rejected hypotheses: a priced
  general re-rally rejected, because any price would
  be invented balance with no evidence behind it, and
  because a re-rally creates and destroys no presence
  — a move order to an already-paid squad, not a
  second deploy — so charging would charge twice for
  one presence, and a cannot-pay failure would freeze
  squads a player already owns (poverty already blocks
  new deploys; it does not also freeze existing ones);
  a price remains a later decision on Windows playtest
  evidence, for example if free repositioning proves
  abusive kiting, and is recorded as a gap, not set
  here. An all-doctrine scope with a field-only change
  for Advance and Defend Core rejected, because neither
  doctrine consults its rally for movement, so the only
  visible effect of a field-only change would be the
  squad's orange rally marker moving while the squad
  stayed put — SquadMarkerDisplay renders one rally
  marker per snapshot squad from its rally — and a
  marker that moves while its squad does not is a false
  move order, not an action; adding a movement rule to
  make those rallies meaningful would be a movement
  redesign, not a generalisation, and is forbidden by
  the scope. Merging the two gates into one API
  rejected, because merging would either loosen the
  stuck gate (a non-stuck Patrol would succeed through
  it, breaking the Slice 20 proof set) or narrow the
  general gate; the two gates therefore stand beside
  each other, sharing one mechanism (internal ReRally)
  and one index rule, differing only in eligibility by
  design. A general recall or dismiss is not added
  here, under any name; recall stays the first Slice 22
  candidate.
- Scope, decided in this step: HoldGround and Patrol
  only, any live squad of those doctrines. A stuck
  Patrol is inside that scope, and succeeds under both
  gates with the identical outcome (free, rally changed,
  index and counter reset to 0, no teleport, no ore, no
  cap change), so Slice 20 is subsumed, not repriced
  and not revised — and the Slice 20 proof set still
  passes unchanged, because no Slice 20 test was touched
  and the full suite passes. For a non-stuck Patrol the
  counter question is answered by reset, including a
  counter above 0 but below 5 (proved at 2): the loop
  is new, so its progress is new too, and carrying two
  refusals onto a loop that refused nothing would be a
  stale counter masquerading as route truth.
- Targeting, decided in this step: squad list index
  (the snapshot's order), reused from Slice 20, for the
  same reasons — position is ambiguous when squads share
  a cell and rally when they share a rally, while an
  index names one list entry; a stale index after a
  death or removal fails closed instead of re-rallying
  the wrong squad. No stable squad identity is invented,
  so no save field is added. The gates-agreement proof
  (a world where two squads share one cell) shows both
  gates naming the same list entries: the stuck gate
  refuses the HoldGround squad at index 1 while the
  general gate accepts that same index-1 squad, and the
  stuck gate accepts the stuck Patrol at index 0 while
  the general gate accepts that same index-0 squad
  again, now as an ordinary Patrol.
- State changes, decided in this step, via
  WorldZone.TryReRallySquad (thin Simulation
  passthrough; TacticalSquad.ReRally is internal and
  performs no gate of its own): the rally changes; a
  Patrol's waypoint index and refusal counter reset to
  0; position, health, doctrine, and kind are untouched.
  No teleport: a HoldGround squad walks to its new rally
  under the unchanged MoveTowardRally rule, and a Patrol
  walks to its new rally and then onto the new loop
  under the unchanged MoveForPatrol rule, at the
  unchanged radius 3 and five waypoints. Snapshot words
  immediately after, from OrbitalView.Snapshot code, not
  assumed: a HoldGround squad away from its new rally
  reads Marching, a HoldGround squad re-rallied to the
  cell it already stands on reads Holding at once, a
  Patrol reads Patrolling at once (or Stuck again after
  a full circuit of refusals if the new loop is itself
  blocked, proved for a non-stuck origin as well as the
  Slice 20 stuck origin), and a refused Advance squad
  keeps its word (Hunting with a living enemy, Idle
  without one) while a refused Defend Core squad keeps
  its word (Hunting with a core attacker, Guarding at
  its post without one).
- Economy, decided in this step: free, with no refund
  question because there is no recall and no second
  deploy. A general re-rally charges 0 EmberOre, refunds
  nothing, and frees or takes no cap slot — the squad
  never left the field. The cannot-pay case therefore
  does not arise: there is no debt and no poverty
  failure for repositioning. Deploy price (10) and cap
  (3) stand, and the paid path is unchanged: a re-rallied
  squad still counts toward the cap, and a fourth deploy
  at a full cap is still refused with ore untouched, even
  after a general re-rally.
- Movement boundary, decided in this step: no movement
  rule moves. MoveTowardRally, MoveForPatrol's sidestep,
  skip, and five-refusal Stuck threshold, Advance's
  nearest-enemy chase, and Defend Core's chase-or-return
  stand; no pathfinder, no wider sidestep, no hold
  replaces a skip, no rally fallback for Advance, no
  rally consultation for Defend Core. A HoldGround squad
  re-rallied to a solid rally cell is accepted, as deploy
  accepts one, and holds short of it visibly; a Patrol
  re-rallied to a solid rally cell is accepted alike,
  holds short, and skips that waypoint on the new loop.
- Display relation, decided in this step: no HUD line
  added, moved, or reworded, no new activity word, no
  marker rule changed. The rally marker follows the new
  rally for HoldGround and Patrol squads because it
  already renders the snapshot's rally — verified from
  SquadMarkerDisplay.cs, not assumed — and a refused
  Advance or Defend Core squad's marker correctly stays
  put, because its rally stands.
- Adapter binding, PROPOSED ONLY, decided in this
  step: reuse K, with wider eligibility — K while not
  picking, crosshair on an eligible (live HoldGround or
  Patrol) squad's marker, opening a re-rally pick that
  reuses the picker's raycast rally selection, Return to
  confirm, Escape to cancel, doing nothing while picking
  and nothing when the raycast hits no eligible marker
  or the target squad refuses. K is free against the
  taken keys (WASD, Space, E, Q, R, O, Return, Esc,
  Alpha1–Alpha7, T, Y, U, I, B, N, P, M, mouse click).
  This binding is a comment in FpsInputAdapter.cs only
  in this step: it is NOT implemented, NOT claimed as
  working in a scene, and observed only under new
  playtest-gate item 40 once implemented on Windows.
- Checklist relation, decided in this step: new
  playtest-gate item 40, the general re-rally
  observation. Item 40 EXTENDS item 39's stuck-patrol
  observation — item 39's stuck readings stand unchanged,
  and item 40 adds only the general readings (a Holding
  squad marching again, a moving Patrol's loop moving,
  and inert-doctrine refusals with no marker move). Item
  40 does NOT supersede item 39, item 37, item 29, or
  orbital-command item 13. Next free playtest-gate
  number is now 41. Item 39 alone does not cover the
  general action: its observation text is stuck-specific
  (enclose, STUCK, re-rally, STUCK again), so the
  non-stuck and other-doctrine cases need their own item.
- Proof set, decided in this step (all in
  tests/Forgeborne.Core.Tests/Program.cs, no demo
  scenario required of this step alone; the composed
  demo belongs to Step 2): a HoldGround Vanguard
  marching to a new rally and Holding on arrival, with
  fire suspended while marching (FindTarget null
  despite an in-range enemy) and resuming once holding
  (FindTarget on the enemy by the new rally, 30 damage);
  a HoldGround squad re-rallied to its own cell reading
  Holding at once; HoldGround Recon and Heavy marching
  to a new rally with damage values unchanged (12, 16);
  a HoldGround solid-rally hold-short; a non-stuck
  Patrol re-rallied mid-loop marching to its new rally
  and turning onto the new loop (east waypoint new
  rally plus radius 3); a Patrol with a counter of 2
  re-rallied west, with the counter reset to 0 (after a
  same-rally refusal proved the counter untouched at 2),
  marching to the new rally and turning onto the new
  loop; Patrol Recon and Heavy core marches with fire
  in passing; a Patrol solid-rally hold-short with a
  skip to the east waypoint; a non-stuck Patrol
  re-rallied to a blocked loop reading Stuck again after
  a full circuit, unmoved, with the new rally kept;
  Advance and Defend Core refusals for all three kinds
  each, with rally, position, and health unchanged, plus
  chase/fire-unchanged proofs (Advance Hunting then
  stepping toward its enemy, Advance Idle with no enemy
  and no move, Defend Core Hunting then stepping toward
  its attacker, Defend Core Guarding at its post with no
  move); the two-gates-agree proof in a shared-cell
  world; fail-closed refusals (dead squad, out-of-range
  and negative indices, out-of-bounds rallies, same
  rally again for both covered doctrines, each with
  nothing changed); save/load proofs (generally
  re-rallied HoldGround and Patrol loading with their
  new rallies through the Simulation passthrough and
  marching on loaded tick 1, a stuck-before-save patrol
  still loading Patrolling at index 0 and re-sticking
  after a fresh circuit, as before); and a paid-path
  proof (three paid deploys at 10 ore each, cap full at
  3, general re-rallies free with ore and count
  unchanged, inert re-rally refused, fourth deploy still
  refused). No save-format change: the save array already
  carries the rally, so a general re-rally persists on
  the next save with no new field, and old saves load
  unchanged. Fire is proved via FindTarget for
  HoldGround, as Slice 20 proved via FindTarget for
  Recon and Heavy — a world-tick kill is proved for the
  Slice 20 patrol, not re-claimed here for HoldGround.
- Changes: src/Forgeborne.Core/World.cs (new
  TryReRallySquad gate; TryReRallyStuckPatrol
  unchanged); src/Forgeborne.Core/Simulation.cs (thin
  TryReRallySquad passthrough); src/Forgeborne.Core/
  Orbital.cs (ReRally and economy doc notes updated to
  the two gates; no logic change);
  tests/Forgeborne.Core.Tests/Program.cs (+237
  assertions); docs/orbital-command.md (general
  re-rally section; stuck section's "no general action"
  sentence updated); docs/orbital-view.md (no new
  word/marker note, with the marker rule verified from
  SquadMarkerDisplay.cs); docs/slice-01-playtest-gate.md
  (item 40); ForgeborneUnity/Assets/Scripts/
  FpsInputAdapter.cs (proposed-binding comment only, no
  code); CHANGELOG 0.88.0 and DECISIONS.md entries
  (next step: Slice 21 Step 2, composed demo + review
  closing Slice 21).
- Tests: 3402 passed, 0 failed (was 3165; +237, no
  failures on the final runs). Demo unchanged: Slice
  20 scenario save 44,041 chars, Slice 19 save 43,965
  chars, Slice 18 save 44,015 chars, Slice 15 save
  44,016 chars, Slice 17 save 43,974 chars. No demo
  scenario is added by this step alone, per the Slice 21
  scope; the composed demo belongs to Step 2.
- Constitution note, stated honestly:
  PROJECT-POINTER.md still records Constitution SHA
  6dcb4bc03a356eb226156ee19c2b3cc1b32e12d7 with a
  note to re-validate before claiming adoption
  complete — that full adoption audit was NOT run in
  this step, so adoption completeness is not claimed
  here.

## 0.87.0 — 2026-10-31 — Slice 21 scope:
general re-rally, documentation only

- Step taken, decided in this step: the Slice 21
  scope, documentation-only, per
  docs/slice-21-scope.md. No system built, no
  answer decided, by design. Slice 22 is NOT
  scoped here; it is scoped afterwards, the same
  way the Slice 21 scope follows the Slice 20
  review rather than preceding it.
- Scope decided: Slice 21 is the general re-rally
  slice, in order — Step 1, the general re-rally
  decision (scope per doctrine, interaction with
  the Slice 20 stuck rescue's free price, targeting,
  state changes, economy, movement boundary, proof
  set, checklist item from 40, display relation,
  and adapter binding proposal, all decided in
  that step, not here); Step 2, composed demo
  scenario plus documentation-only review closing
  Slice 21.
- Why general re-rally, and why now: Slice 20
  proved a re-rally mechanism (internal ReRally,
  saved rally field, walk-to-new-rally proof) for
  one state only, and its review carries the
  generalisation as the first-ranked remainder.
  Generalising a proved mechanism is the smaller
  step; recall, which has no mechanism, no
  confirmation design, and an open refund/cap
  question, is carried as the first Slice 22
  candidate, not patched in beside it.
- The decision inside the scope, stated plainly:
  the rally is not the same thing for every
  doctrine, so Step 1 must answer per doctrine.
  HoldGround's rally is its destination; Patrol's
  rally is its loop centre; Advance's rally is
  not consulted by movement at all (no rally
  fallback when no enemy lives); Defend Core's
  rally is a deployment formality, not a place
  the squad ever goes. A scope that said "all
  doctrines, same effect" would be wrong on two
  of the four, so Step 1 must say, for Advance
  and Defend Core, whether a re-rally is a valid
  field-only action, a fail-closed refusal, or
  outside the scope — with chase and fire proofs
  either way.
- Radius and waypoints carried, not re-decided:
  radius 3 and five waypoints stay first-pass
  values, because Slice 20 Step 1 found no stuck
  case whose cause was the radius itself, so
  there is still no evidence behind a headless
  retune. A general re-rally moves a loop's
  centre, never its radius or count, and a Step 1
  radius finding is recorded as a gap, not acted
  on by widening.
- Candidates and gaps carried: Slice 13 remainder
  observations (items 25-27, 30-39, plus the new
  item Step 1 adds) remain Windows observations,
  not re-scoped steps; first-pass graphics values
  are retuned only on Windows evidence; Windows
  Unity verification (Slices 1-21) and the Slice 1
  director playtest gate remain open and are not
  closed, reordered away, or answered by scoping.
- Tests: 3165 passed, 0 failed (unchanged,
  documentation-only step). Demo unchanged: Slice
  20 scenario save 44,041 chars, Slice 19 save
  43,965 chars, Slice 18 save 44,015 chars, Slice
  15 save 44,016 chars, Slice 17 save 43,974 chars.

## 0.86.0 — 2026-10-31 — Slice 20 Step 2:
composed demo + review, closing Slice 20

- Step taken, decided in this step: Slice 20 Step 2,
  the composed demo scenario plus documentation-only
  review closing Slice 20, per docs/slice-20-scope.md.
  No new core system, no adapter edit, no checklist
  item, by design. Slice 21 is NOT scoped here; it is
  scoped afterwards, the same way the Slice 20 scope
  followed the Slice 19 review rather than preceding
  it.
- Composition, decided in this step: one Simulation,
  three paid squads, no enemy, no pocket seeded beyond
  the 30 EmberOre the three deploys spend — a Heavy
  HoldGround marched away first (rally (10,5,10),
  Holding at its rally from the sixth patrol-march
  tick on), a Recon Patrol marched to its rally and
  walking its loop (rally (20,5,20), at its rally
  after eight march ticks), and then a Vanguard Patrol
  deployed at the player cell in the exact route-
  blocked shape Step 1 proved, translated to the
  player cell (rally (12,5,16); stone west, south,
  and north of the squad's cell, east step cell open),
  so it counts five refusals and reads Stuck, unmoved,
  while the other two keep their readings. The demo
  then proves, in order: a stuck-before-save load copy
  that reads Patrolling at index 0 and re-sticks after
  a fresh full circuit; re-rally refusals on the
  HoldGround and on the moving Patrol, with nothing
  changed; the stuck patrol accepting a free re-rally
  to (22,5,16) with no charge, no refund, and no cap
  slot freed, and not teleported; a fourth paid deploy
  still refused at the full cap; the re-rallied patrol
  marching east to its new rally in six ticks and
  turning onto the new loop on the next tick (east
  waypoint (25,5,16), the new rally plus radius 3);
  and a save/load round-trip after the action that
  loads the new rally, with patrol indices reset to 0
  as before — the loaded re-rallied patrol steps back
  to its new rally on loaded tick 1, because index 0
  targets the rally itself.
- Why the route-blocked shape, not the fully enclosed
  shape, is the composed stuck case, stated plainly:
  Step 1 proved both shapes and they behave
  differently — the route-blocked patrol marches after
  a re-rally, the fully enclosed patrol re-sticks on
  the new loop instead of marching through a wall —
  and a composition that staged only the enclosed shape
  would show a re-rally followed by a re-stick, which
  is the honest limit but not the march the action is
  for. The composed demo therefore stages the shape
  that marches, and the enclosed shape's re-stick
  remains Step 1's proof, cited in the review, not
  re-claimed as a composed result. Staging the stuck
  patrol at the player cell, after the other two have
  marched away, is what makes the translation exact:
  the squad starts at its own cell with its rally four
  cells west, exactly as Step 1's squad started at
  (24,5,20) with its rally at (20,5,20).
- Claim split, stated plainly: the headless evidence is
  squad states before and after the action for staged
  layouts, refusal counts, waypoint indices, rally
  values, ore and cap counts across deploys and the
  rescue, march positions across ticks, and two save/
  load round-trips that preserve the field, plus a demo
  that prints those values for three squads in one
  field. None of that proves a player can press K, aim
  at a stuck marker, pick a new rally, and watch the
  rally marker move in a built scene. That is a Windows
  observation (item 39), which is also honestly pending
  its own binding: the proposed K binding is not yet
  implemented in an adapter, so the observation cannot
  even be attempted until a later adapter step
  implements it.
- Checklist relation, decided in this step: no new
  playtest-gate item. Item 39 already observes the
  decided action (enclosed patrol to Stuck, re-rally to
  open ground to Patrolling with a walk and a moved
  rally marker, re-rally to a still-blocked rally to
  Stuck again, and refusals on a moving patrol and a
  HoldGround squad), and this step exercises no
  behaviour item 39 does not already cover. Item 39
  EXTENDS item 37; item 37, item 29, and orbital-
  command item 13 stand unchanged and are NOT
  superseded. Next free playtest-gate number remains
  40.
- Tests: 3165 passed, 0 failed (was 2968; +197 composed-
  integration assertions mirroring the demo, inserted
  before the test runner's summary so they execute).
  Demo: every earlier demo save length unchanged
  (Slice 1 43,881; Slice 2 43,921; Step 1 44,016; Step 3
  44,103; Slice 4 44,026; Slice 5 43,974; Slice 6
  44,024; Slice 7 44,013; Slice 8 43,974; Slice 9
  43,975; Slice 10 43,974; Slice 11 44,038; Slice 12
  43,974; Slice 13 44,045; Slice 14 43,974; Slice 15
  44,016; Slice 17 43,974; Slice 18 44,015; Slice 19
  43,965). New Slice 20 scenario save 44,041 chars,
  quoted as its own entry, not as a change to any
  earlier save.
- Review: docs/slice-20-review.md, checking the slice
  against docs/slice-20-scope.md step by step, verdict
  reviewed and closed as one stuck-patrol action
  decided with tests and proved together in one demo —
  not playtested, not Unity-verified, not a visual
  result. Gaps carried in priority order: Windows Unity
  verification (Slices 1–20, items 25–27 and 30–39
  still open, item 39 pending its binding); the Slice 1
  director playtest gate still pending; first-pass
  graphics values unproven; Slice 13 scene-proof
  observations still unmade; patrol route remaining
  questions (radius on evidence, or a general recall/
  re-rally for other doctrines as its own decision).
  Closed by this slice, not carried: the stuck-patrol
  action remainder from the Slice 19 review's gap 5 /
  Slice 18 gap 6.

## 0.85.0 — 2026-10-31 — Slice 20 Step 1:
stuck-patrol player action decided — free re-rally,
stuck Patrol only

- Step taken, decided in this step: Slice 20 Step 1,
  the stuck-patrol player action. In one quotable
  sentence: a stuck patrol gets a FREE re-rally, for
  a stuck Patrol only — not a recall, not both, not
  neither.
- Decision, with its rejected hypotheses: recall
  rejected, because it destroys a paid investment and
  its position and would need a refund and a
  confirmation design (a refund plus a freed cap slot
  would be two gifts, and a recall without a refund
  is a punitive disposal, not a rescue), and because
  it is a general dismiss system, not a stuck rescue;
  a general recall or dismiss is carried after Slice
  20, not patched in here. Both rejected, because
  adding dismiss beside rescue doubles the slice into
  a general command system the scope forbids. Neither
  rejected, because a permanently blocked route is a
  placement the player cannot currently correct, and
  leaving death as the only exit turns a placement
  mistake into a permanent loss of route function —
  that is a trap, not skill. Re-rally chosen, because
  a patrol's task is its route, and a route that
  cannot be walked is exactly what a loop-centre
  correction answers, without creating a squad,
  destroying one, or repricing anything.
- Scope, decided in this step: stuck Patrol only.
  A non-stuck Patrol is refused fail-closed, and
  HoldGround, Advance, and Defend Core squads are
  refused alike, because no other doctrine has a
  route failure to correct — a general re-rally
  would let the player reposition any squad without
  paying a deploy, which is a general command
  system, not a rescue. The scope's wider-scope
  question is therefore answered narrowly, with
  the wider decision carried, not assumed.
- Targeting, decided in this step: squad list index
  (the snapshot's order). Position is ambiguous when
  two squads share a cell (deploy at the player's
  feet is common) and rally is ambiguous when two
  squads share a rally, while an index names one
  list entry; the gate is re-checked at call time, so
  a stale index after a death or removal fails closed
  instead of re-rallying the wrong squad. No stable
  squad identity is invented, so no save field is
  added.
- State changes, decided in this step, via
  WorldZone.TryReRallyStuckPatrol (thin Simulation
  passthrough; TacticalSquad.ReRally is internal and
  performs no gate of its own): the rally changes,
  the waypoint index resets to 0, and the refusal
  counter resets to 0; position, health, doctrine,
  and kind are untouched. No teleport: the squad
  walks to the new rally under the unchanged
  MoveForPatrol rule, and the loop re-derives from
  the new rally at the unchanged radius 3 and five
  waypoints. The snapshot reads Patrolling at once,
  honestly, because the loop is new, not because the
  block vanished. A re-rally to the same rally is
  refused, or it would clear the Stuck word for one
  circuit without correcting anything; a rally inside
  a solid block is accepted, as deploy accepts one,
  and the squad holds short of it visibly.
- Economy, decided in this step: free, with no refund
  question because there is no recall. A re-rally
  charges 0 EmberOre, refunds nothing, and frees no
  cap slot — the squad never left the field, so a
  priced re-rally would charge twice for one presence
  (and a stuck-only price would be a rescue tax that
  leaves an unaffordable player exactly where neither
  left them). Deploy price (10) and cap (3) stand,
  and the paid path is unchanged: a re-rallied squad
  still counts toward the cap, and a fourth deploy at
  a full cap is still refused with ore untouched.
- Movement boundary, decided in this step: no movement
  rule moves. MoveForPatrol's sidestep, skip, and
  five-refusal Stuck threshold stand; HoldGround,
  Advance, and Defend Core movement stand; no
  pathfinder, no wider sidestep, no hold replaces a
  skip; fire in passing is unchanged for stuck and
  re-rallied patrols alike. The honest limit, proved
  rather than hidden: a fully cell-enclosed squad
  re-rallied to an outside rally still cannot step
  out, so it reads Stuck again after a full circuit
  on the new loop — a re-rally corrects the loop, it
  does not march a squad through a wall, and that
  squad needs a step cell cleared as well as a new
  loop. No radius finding: no Step 1 proof shows the
  radius (3) caused a block, so the radius stays a
  carried first-pass value, not a retune.
- Display relation, decided in this step: no HUD line
  added, moved, or reworded, no new activity word, no
  marker rule changed. The snapshot's existing Patrol
  branch reads Patrolling after a re-rally and Stuck
  again after a full circuit on a blocked new loop,
  and the existing rally marker follows the new rally
  because it already renders the snapshot's rally.
- Adapter binding, PROPOSED ONLY, decided in this
  step: K while not picking, crosshair on the stuck
  squad's marker, opening a re-rally pick that reuses
  the picker's raycast rally selection, Return to
  confirm, Escape to cancel, doing nothing while
  picking and nothing when the raycast hits no stuck
  marker. K is free against the taken keys (WASD,
  Space, E, Q, R, O, Return, Esc, Alpha1–Alpha7, T,
  Y, U, I, B, N, P, M, mouse click). This binding is
  a comment in FpsInputAdapter.cs only in this step:
  it is NOT implemented, NOT claimed as working in a
  scene, and observed only under new playtest-gate
  item 39 once implemented on Windows.
- Checklist relation, decided in this step: new
  playtest-gate item 39, the stuck-patrol re-rally
  observation. Item 39 EXTENDS item 37's patrol-route
  observation; item 37, item 29, and orbital-command
  item 13 stand unchanged and are NOT superseded.
  Next free playtest-gate number is now 40.
- Proof set, decided in this step (all in
  tests/Forgeborne.Core.Tests/Program.cs, no demo
  scenario required of this step alone; the composed
  demo belongs to Step 2): a route-blocked Vanguard
  patrol (east neighbour open) reading Stuck, then
  re-rallied to a new rally, marching to it in six
  steps and turning onto the new loop with the east
  waypoint re-derived at new-rally-plus-3; Recon and
  Heavy proved for the same core effect and for
  FindTarget fire targeting with their damage values
  (Recon 12, Heavy 16; a world-tick kill is proved for
  Vanguard below, not claimed for Recon and Heavy here);
  a fully enclosed patrol re-rallied outside and
  reading Stuck again after five refusals on the new
  loop, unmoved; refusals for a non-stuck Patrol,
  HoldGround, Advance, and Defend Core squads (with
  the stuck patrol in the same world succeeding after
  them, proving index discrimination), an out-of-range
  and a negative index, a dead squad, an out-of-bounds
  rally, and the same rally again, each with nothing
  changed; a solid rally cell accepted with the squad
  holding short of it; a world-tick fire proof (a
  re-rallied Vanguard walks east and kills in passing);
  save/load proofs (a re-rallied patrol loads with its
  new rally and marches from index 0 through the
  Simulation passthrough, with no ore charged; a
  stuck-before-save patrol still loads Patrolling at
  index 0 and re-sticks after a full circuit, as
  before); and a paid-path proof (three paid deploys
  at 10 ore each, cap full at 3, re-rally free with ore
  and count unchanged, fourth deploy still refused).
  No save-format change: the save array already
  carries the rally, so a re-rally persists on the
  next save with no new field, and old saves load
  unchanged.
- Changes: src/Forgeborne.Core/Orbital.cs (Rally
  private-set with a ReRally internal, economy doc
  note that a re-rally costs 0 and frees no slot);
  src/Forgeborne.Core/World.cs (TryReRallyStuckPatrol
  gate); src/Forgeborne.Core/Simulation.cs (thin
  passthrough); tests/Forgeborne.Core.Tests/Program.cs
  (+158 assertions); docs/orbital-command.md (re-rally
  section); docs/orbital-view.md (no new word/marker
  note); docs/slice-01-playtest-gate.md (item 39);
  ForgeborneUnity/Assets/Scripts/FpsInputAdapter.cs
  (proposed-binding comment only, no code);
  CHANGELOG 0.85.0 and DECISIONS.md entries (next
  step: Slice 20 Step 2, composed demo + review
  closing Slice 20).
- Tests: 2968 passed, 0 failed (was 2810; +158, no
  failures on the final runs). Demo unchanged: Slice
  19 scenario save 43,965 chars, Slice 18 save 44,015
  chars, Slice 15 save 44,016 chars, Slice 17 save
  43,974 chars. No demo scenario is added by this step
  alone, per the Slice 20 scope; the composed demo
  belongs to Step 2.
- Constitution note, stated honestly:
  PROJECT-POINTER.md still records Constitution SHA
  6dcb4bc03a356eb226156ee19c2b3cc1b32e12d7 with a
  note to re-validate before claiming adoption
  complete — that full adoption audit was NOT run in
  this step, so adoption completeness is not claimed
  here.

## 0.84.0 — 2026-10-31 — Slice 20 scope:
stuck-patrol player action, decided in later steps

- Step taken, decided in this step: Slice 20
  scope, documentation-only, 0.84.0. No system
  built, no answer decided, by design; Slice 21
  is NOT scoped here.
- Scope decided: Slice 20 is the first-ranked
  Slice 19 remainder, in order — Step 1, the
  stuck-patrol player action (recall, re-rally,
  both, or neither, decided in that step, with
  its scope, targeting, state changes, economy
  relation, movement boundary, display relation,
  and checklist item from 39 all stated as
  questions, not answers); Step 2, composed demo
  scenario plus documentation-only review closing
  Slice 20. Radius 3 and the five-waypoint loop
  are carried as first-pass values, NOT re-decided
  headlessly: a re-rally moves the loop's centre,
  never its radius or waypoint count, and a Step 1
  finding that the radius caused a block is
  recorded as a gap for a later radius decision,
  not a quiet widening.
- Ground facts verified against code this run,
  not copied from review prose: IsPatrolStuck is
  Patrol-only at PatrolConsecutiveRefusals >=
  PatrolWaypointCount (5), counter reset on any
  successful Patrol step and on load, waypoint
  index live progress reset on load, PatrolTarget
  re-derived from rally and index so a changed
  rally re-derives the loop with no new save field
  (save already carries doctrine, kind, position,
  and rally); Stuck is display only, still firing
  in passing; no recall, dismiss, or live-squad
  re-rally API exists (OrbitalCommand.SetRally is
  pending-deploy only, fail-closed outside picking);
  picker deploys only (O, Return, Escape, Alpha1–
  Alpha7); economy fixed at 10 EmberOre, cap 3,
  cap refusal with no oldest-squad recall, no
  refund on death or otherwise, dead squads free
  the cap; next free playtest-gate number is 39.
- Changes: new docs/slice-20-scope.md (scope
  question, inputs, radius carried-not-re-decided,
  Step 1 proof set incl. enclosed-patrol starting
  state, non-stuck and second-doctrine controls,
  all-kinds-or-reason, save/load, paid-path
  unchanged, re-rally-to-unreachable re-stuck
  proof, checklist item from 39 and its relation
  to items 37, 29, and orbital-command item 13;
  Step 2 composed demo requirements; deliberately-
  not list; Slice 21 ranked candidates; gate);
  CHANGELOG 0.84.0 and DECISIONS.md entries
  (theme, order with reversal honesty, questions-
  not-answers, boundary, deliberately-not list,
  next step: Slice 20 Step 1, the stuck-patrol
  player action, decided in that step).
- Tests: 2810 passed, 0 failed (unchanged,
  documentation-only step). Demo unchanged: Slice
  19 scenario save 43,965 chars, Slice 18 save
  44,015 chars, Slice 15 save 44,016 chars, Slice
  17 save 43,974 chars.
- Constitution note, stated honestly:
  PROJECT-POINTER.md still records Constitution SHA
  6dcb4bc03a356eb226156ee19c2b3cc1b32e12d7 with a
  note to re-validate before claiming adoption
  complete — that full adoption audit was NOT run in
  this step, so adoption completeness is not claimed
  here.

## 0.83.0 — 2026-10-31 — Slice 19 Step 2:
composed demo + review, closing Slice 19

- Step taken, decided in this step: Slice 19 Step 2, the
  composed demo scenario plus the documentation-only
  review closing Slice 19. No new core system, no new
  query, no adapter edit, no checklist item, by design;
  Slice 20 is NOT scoped here.
- Composition, decided in this step: demo/Forgeborne.Demo
  gains a Slice 19 Step 2 scenario after the Slice 18
  scenario — one Simulation, six rows on the y=20 plane
  four cells apart in z, carrying the Step 1 case (a
  Conduit beside a transformer alone), the two live
  controls (a Dynamo beside a transformer alone and an
  Alternator beside a transformer alone), the lone control
  (a lone Conduit), the Slice 17 Step 2 control (a mill
  beside a Conduit), and the dead-cable consistency case
  (a Dynamo-plus-Conduit mixed-voltage cable component)
  alongside the decided transformer case, with the POWER
  line read before and after a tick and a save/load
  round-trip proving the reading re-derives on load.
- Composed numbers pinned: before any tick 0 of 1 MILLS
  POWERED, WARNED 0 MILLS, POWER 2 LIVE NETWORKS, 3 LONE
  SOURCES; every staged cell's power state None; tick 1
  leaves the Step 2 mill standing, all counters 0, and
  the POWER line still 2 / 3; after load every staged cell
  loads, the POWER line re-derives as 2 / 3, counters
  reset, player at (16,5,16); loaded tick 1 re-proves the
  same stability. The stability is the composition's
  point: no mill here is mismatched, so nothing is
  destroyed and nothing starts feeding.
- Review: new docs/slice-19-review.md (gate checked
  against the scope, verdict: reviewed and closed as one
  display-query remainder decided with tests and proved
  together in one demo, not playtested, not
  Unity-verified, gaps carried in priority order, no new
  playtest-gate checklist item — next free number remains
  39); CHANGELOG 0.83.0 and DECISIONS.md entries
  (composition, stability rationale, claim split,
  deliberately-not list, next step: scoping Slice 20 as
  its own decision step).
- Tests: 2810 passed, 0 failed (was 2733; +77, no failures
  on the final runs). Demo: all earlier demo saves as in
  the 0.82.0 entry (Slice 1 43,881; Slice 2 43,921; Step 1
  44,016; Step 3 44,103; Slice 4 44,026; Slice 5 43,974;
  Slice 6 44,024; Slice 7 44,013; Slice 8 43,974; Slice 9
  43,975; Slice 10 43,974; Slice 11 44,038; Slice 12 43,974;
  Slice 13 44,045; Slice 14 43,974; Slice 15 44,016; Slice
  17 43,974; Slice 18 44,015). New scenario save 43,965
  chars, quoted as its own entry, not as a change to any
  earlier save.

## 0.82.0 — 2026-10-31 — Slice 19 Step 1:
transformer-adjacency count query, decided

- Decision, decided in this step (rationale in
  DECISIONS.md): a Conduit beside a dead
  standalone transformer counts as lone, folded
  into the existing POWER line's lone number —
  no new line, no new word. The exception holds
  only while the transformer group is dead (every
  cell outputs None) and standalone (no cable
  neighbour), and only for a Conduit, because a
  Conduit is absent from TransformerOutput's
  direct branch (Dynamo LV and Alternator HV
  only): a transformer is not wiring for a
  Conduit the way it is for a Dynamo or
  Alternator, by the same branch logic Slice 17
  Step 2 used for consumers. A Conduit beside a
  live transformer gains no lone count (1 LIVE,
  0 LONE, no double count, feeding or not); a
  Dynamo or Alternator beside any transformer
  still gains none, even a conflicted one, so a
  conflict is not hidden behind a lone number;
  a cable neighbour still disqualifies every
  source, even a dead component, because every
  source kind is a cable input (Dynamo LV,
  Alternator HV, Conduit HV) — the dead-cable
  asymmetry is that branch difference, stated
  openly, and the dead-cable case stays 0 / 0.
  A transformer touching a cable stays
  cable-story, not standalone, for a live cable
  and a dead mixed-voltage cable alike; a cable
  component fed only by a dead transformer is a
  cable case, not a transformer case, and reads
  0 / 0. Counting is per source cell: two
  Conduits beside one dead transformer are two
  lone sources, one Conduit beside two dead
  transformers is one, and one Conduit beside a
  dead chain or ring is one. A consumer touching
  a dead group does not block the count, and a
  Conduit beside both a mill and a dead
  transformer counts lone under both rules at
  once — neither neighbour disqualifies.
- Hypotheses weighed, decided in this step: keep
  unchanged (rejected — the motivating case
  then stays invisible on every count); count as
  lone (adopted, in the branch-limited form
  above); a distinct readout (rejected — HUD
  discipline, art-direction rule 4, and the lone
  number already means a placed source
  delivering nothing since Slice 17 Step 2);
  redefine live to include dead wiring (rejected
  — live has meant carrying a non-None voltage
  since Slice 12, and a dead group counted as
  live would make the live number lie about the
  very thing it exists to report).
- Delivery boundary, decided in this step: no
  delivery, precedence, conflict, rating, cost,
  or phase rule moves. A Conduit beside a
  transformer still delivers nothing directly, a
  transformer beside a Conduit alone still
  outputs None, a Dynamo-fed transformer still
  delivers stepped-up HV to its mill, and no
  consumer's powered, mismatched, or warned
  predicate moves — proved by delivery and HUD
  consumer assertions in the proof set, not
  asserted beside it. No material, marker,
  light, fog, model, or tint value moves; the
  lone sentence is restated whole in
  LivePowerCounts, not left as two
  half-sentences for two neighbour kinds.
- Superseded assertion, kept as history: the
  Slice 17 Step 2 transformer world (Conduit
  beside a transformer beside a mill) moves from
  0 / 0 to 0 / 1 under this decision, with its
  comment rewritten to say so; its live
  counterpart (Dynamo-fed transformer feeding a
  mill) still reads 0 / 0, unchanged. No test
  weakened — one reading re-decided, with the
  reason recorded.
- Checklist: new playtest-gate item 38 (Windows
  only) observing the decided count — a Conduit
  beside a transformer alone, a Dynamo beside a
  transformer alone, and a lone Conduit, with
  the POWER line read aloud for each. Item 38
  extends item 36 and item 22 and supersedes
  neither; next free gate number is 39. No
  adapter edit was needed: LivePowerText already
  renders LoneSources. No demo scenario in this
  step, by the Slice 19 scope: the composed demo
  belongs to Step 2.
- Tests: 2733 passed, 0 failed (was 2690; +43,
  no failures on the final runs). Demo
  unchanged: Slice 18 scenario save 44,015
  chars, Slice 15 scenario save 44,016 chars,
  Slice 17 scenario save 43,974 chars, and all
  others as in the 0.80.0 entry.

## 0.81.0 — 2026-10-31 — Slice 19 scope: transformer-adjacency
count remainder, documentation-only

- Scope, decided in this step (rationale in
  DECISIONS.md): new docs/slice-19-scope.md scopes
  Slice 19 as the one carried Slice 17 display-query
  remainder from the Slice 18 review, in order —
  Step 1, the transformer-adjacency count query
  (what the HUD counts show for a source beside
  wiring that delivers nothing, with the dead-cable
  consistency case stated as a condition, not an
  analogy); Step 2, a composed demo scenario plus
  a documentation-only review, which unlike the
  Slice 16 review is accompanied by a demo because
  the count decision is headless-testable. The
  scope decides theme, order, and the questions
  the step must answer — it decides neither
  answer: Step 1 must choose among keeping the
  definition unchanged, counting a non-delivering
  transformer-adjacent source as lone, adding a
  distinct readout, or redefining live to include
  dead wiring, each with a stated proof set.
- Order rationale, decided in this step (rationale
  in DECISIONS.md): transformer-first because it
  is the Slice 18 review's ranked candidate 1 and
  because it closes the last Slice 17 display-query
  remainder, not because a stuck patrol depends on
  a count; reversing the order with the patrol
  recall question would not break either decision,
  and the scope says so. The Slice 18 gap 6 patrol
  questions (recall, re-rally, player action for a
  stuck patrol; radius and waypoint first-pass
  values) are named as carried, not promised, so
  the next scoping step ranks them in the open
  rather than finding them smuggled into a count
  step.
- Boundary, decided in this step (rationale in
  DECISIONS.md): Slice 19 is one display query and
  its review only. No delivery, precedence,
  conflict, rating, cost, or phase rule moves; no
  material, marker, light, fog, model, or tint
  value moves; the count step decides one POWER
  line question (fold-in, new line, or new word,
  with justification under HUD discipline). The
  step must prove its rule does not double-count
  a live transformer case, must state per-source,
  per-transformer, or per-pair counting, must say
  whether conflicted transformers and dead cables
  follow the same rule or differ with a stated
  distinction, and must take a Windows checklist
  item from 38. No Slice 20 scope is included.
- Tests: 2690 passed, 0 failed (unchanged,
  documentation-only step). Demo unchanged: Slice
  18 scenario save 44,015 chars, Slice 15 scenario
  save 44,016 chars, Slice 17 scenario save 43,974
  chars, and all others as in the 0.80.0 entry.

## 0.80.0 — 2026-10-31 — Slice 18 Step 3: composed demo
plus review, closing Slice 18, decided

- Composition, decided in this step (rationale in
  DECISIONS.md): one Simulation, two paid patrols, no
  enemy, no free deploy path anywhere — a Recon Patrol
  to rally (20,5,20) that sidesteps a diagonal march
  block at (18,5,17) on tick 3, skips the single-axis
  loop leg east blocked at (21,5,20) on tick 9, and
  resets its refusal counter on walking south again on
  tick 10; then a Vanguard Patrol to rally (16,5,16),
  the player cell, deployed only after the first
  patrol has marched away, enclosed on all four sides
  after it deploys, so it cycles through five
  consecutive refusals and reads Stuck, unmoved, while
  the first patrol keeps walking beside it; then a
  save/load round-trip under Step 1's load rule, in
  which both patrols reset waypoint index and refusal
  counter to 0, the enclosed patrol reads Patrolling
  on arrival and earns Stuck again only after a fresh
  full circuit, and the walking patrol marches to its
  rally first and skips the blocked east leg again.
- Composed numbers pinned: phase 1 ticks 1–10, march
  (17,16), (17,17), sidestep (17,18) with distance 6
  to 5, (18,18), (19,18), (19,19), (20,19), rally
  (20,20), skip unmoved at (20,20) with waypoint 2
  and 1 refusal, then (20,21) with counter 0;
  enclosed phase, walking patrol (20,22), (20,23),
  (19,23), (19,22), (18,22) while the enclosed patrol
  stays at (16,16) through waypoints 2, 3, 4, 0, 2
  and refusals 1–5, Patrolling through 4 and Stuck
  at 5; loaded phase, walking patrol (19,22),
  (19,21), (20,21), rally (20,20), then skip again
  unmoved at (20,20) with waypoint 2 and 1 refusal,
  while the enclosed patrol repeats waypoints 2, 3,
  4, 0, 2 and refusals 1–5, Stuck again at 5. Ore:
  20 in, 10 left after the first patrol, 0 after
  the second — the paid cost is visible in the
  demo's numbers, not asserted beside it.
- Review: new docs/slice-18-review.md checks the
  slice against docs/slice-18-scope.md step by step,
  states the verdict honestly (two orbital remainders
  decided with tests and proved together in one demo,
  not playtested, not Unity-verified), separates
  headless-tested route and boundary claims from the
  Windows-checklist patrol observation (item 37,
  still unmade), carries the gaps that remain in
  priority order, and does NOT scope Slice 19.
- Checklist relation: no new playtest-gate item —
  the review exercises no behaviour item 37 does not
  already cover, and Step 2's boundary takes no item
  because a compile error is not a scene behaviour,
  so the next free gate number remains 38. No core
  file, no adapter file, no material, marker, light,
  fog, model, or HUD rule moves in this step. Demo
  unchanged except for the new scenario: every
  earlier demo save length stands (Slice 15 44,016
  chars, Slice 17 43,974 chars, and all others as in
  the 0.79.0 entry's run); new Slice 18 scenario save
  44,015 chars, quoted as its own entry — one char
  shorter than the Slice 15 save, a different save,
  not a change to it.

## 0.79.0 — 2026-10-31 — Slice 18 Step 2: free deploy
primitive enforcement, hard removal plus internal
test seam, decided

- Decision, decided in this step (rationale in
  DECISIONS.md): the public free deploy entry
  points are removed — `WorldZone.DeploySquad`
  and `Simulation.DeployOrbitalSquad` no longer
  exist — and headless tests deploy through the
  internal seam `WorldZone.SpawnSquad`, exposed
  to the test assembly only through
  `InternalsVisibleTo("Forgeborne.Core.Tests")`.
  A play path that tries a free deploy now fails
  to compile instead of deploying a free squad.
  The paid path (`TryDeployPaidSquad`, picker
  `Confirm`, `DeployOrbitalSquadPaid`) is
  unchanged, as are the price (10 EmberOre), the
  cap (3 live squads), every doctrine, kind, and
  movement rule.
- Corrected history: this step first tried the
  scope's compiler-marking hypothesis, strengthened
  to `[Obsolete(..., error: true)]` with a test-file
  pragma as the seam, and the build proved a pragma
  cannot suppress a CS0619 error (278 errors with
  the pragma present). The mechanism was therefore
  changed to removal plus the internal seam rather
  than weakened to a warning; no Obsolete wrapper
  and no pragma remain.
- Migration cost, counted in the open: the test
  file called the free primitive on 133
  `WorldZone` lines and 5 `Simulation` lines
  (grep for `.DeploySquad(` and
  `.DeployOrbitalSquad(`), and all 138 move to the
  seam by rename only — no test seeds ore, no
  economy assertion churn, because those tests
  isolate movement, fire, and targeting from the
  economy by design. The demo needs no re-seeding:
  verified this run, it already deploys through
  the paid path with zero free-path calls, which
  corrects the scope's ground fact that the demo
  used the free path. Renaming the public primitive
  was rejected (a name alone stops no call, same
  138-site churn); keeping the rule-only status was
  rejected (a comment is not a boundary).
- Proof set, in tests: paid-path matrix, all four
  doctrines by all three kinds — funded succeeds
  with doctrine and kind kept and exactly 10 ore
  charged, unfunded refused with no squad and no
  charge; cap through the paid path for Advance
  and DefendCore (HoldGround and Patrol caps rest
  on earlier slices); reflection proofs that the
  old public names are absent and the seam is
  internal, not public; a picker Patrol that keeps
  doctrine and kind and pays; and the paid-path
  patrol identity the scope demanded — the Step 1
  fully enclosed setup, deployed paid, reads
  Patrolling after four refusals and Stuck,
  unmoved, after the fifth. Play-path audit
  re-verified: picker through `TryDeployPaidSquad`,
  adapter Enter through `ConfirmOrbitalDeploy`,
  no adapter file references the old names or the
  seam. Forbidden-path proof outside the suite: a
  throwaway non-friend project calling the old
  names and the seam failed to build with CS1061
  on all three call sites. Post-charge refund
  guard: unreachable through the public API after
  its checks pass, so it is a code reading, not a
  test claim — refused-before-charge charging
  nothing is what the matrix and cap tests prove.
- Checklist relation: no new playtest-gate item —
  a compile error is not a scene behaviour to
  observe, so the next free gate number remains 38
  after Step 1's item 37. No material, shader,
  marker, light, fog, model, or HUD rule moves;
  the only adapter change is an audit comment.
  Demo unchanged: Slice 15 save 44,016 chars,
  Slice 17 save 43,974 chars.

## 0.78.0 — 2026-10-31 — Slice 18 Step 1: patrol route
limits, sidestep plus stuck readout, decided

- Decision, decided in this step (rationale in
  DECISIONS.md): a refused patrol step first
  tries a distance-reducing sidestep on a
  remaining gap axis, in MoveForPatrol only, and
  only if every gap axis is refused does it skip
  that waypoint for the circuit, as Slice 15
  decided; a patrol that refuses, with no sidestep
  move, on every waypoint of a full circuit reads
  Stuck, not Patrolling. The sidestep is one
  voxel, largest gap first with the shared X,
  then Y, then Z tie-break, refused into a solid
  block, out of bounds, or onto a living enemy's
  cell, and it reduces the Manhattan distance to
  the waypoint by exactly one, so it can never
  wander away from the route; it keeps the
  waypoint index, so the next tick retries the
  same waypoint from the closer cell. A
  single-axis block therefore still skips, because
  there is no second gap axis to sidestep onto.
- Stuck readout, decided in this step (rationale
  in DECISIONS.md): a new SquadActivity value,
  Stuck, appended as 6, derived in
  OrbitalView.Snapshot from a live counter,
  PatrolConsecutiveRefusals, which increments on
  each refused step with no sidestep move and
  resets to 0 on any successful patrol step.
  Stuck is true at or above PatrolWaypointCount
  (5) consecutive refusals — exactly one full
  circuit tried and refused — and four refusals
  still read Patrolling. The counter is live
  progress like the waypoint index: it starts at
  0 on load, so a loaded patrol is never stuck
  on arrival. Stuck changes the display word
  only: a stuck patrol fires exactly as a moving
  patrol fires, and the Unity text line already
  renders Activity.ToString(), so no adapter
  edit, marker change, or HUD change was needed.
- Rejected in this step (rationale in
  DECISIONS.md): hold (freezes the whole loop on
  one block and still misreads while stuck); keep
  the skip rule unchanged (leaves the enclosed
  patrol reading Patrolling while never moving);
  and a full pathfinder (a new system, not a
  sidestep, with search work and route state this
  step has no need to pay for). The mechanism
  lives in MoveForPatrol alone, through a new
  private TryStepOnAxis helper; the shared
  MoveTowardPosition rule is untouched, and
  contrast tests prove HoldGround, Advance, and
  Defend Core do not sidestep. No price, cap,
  radius, waypoint count, fire rule, engagement
  range, delivery, precedence, conflict, rating,
  cost, or phase rule moves; no new save field
  is added, so old saves load unchanged.
- Proof set, in tests: single blocked waypoint
  (solid single-axis skip unchanged, diagonal
  sidestep, both gap axes blocked falling back
  to skip); fully enclosed patrol (Patrolling
  through four refusals, Stuck at five, still
  Stuck after ten ticks, unstuck by clearing one
  wall); enemy-blocked steps (single-axis skip
  then south even after the enemy leaves, proving
  skip rather than hold; diagonal sidestep then
  the rally); next-circuit retry and resumption
  after clearing (retry refused again while the
  block stands, then stepping east successfully
  once cleared); fire while stuck through
  WorldZone.Advance for all three kinds (Recon
  12 per tick over three ticks, Vanguard 30 in
  one tick, Heavy 16 per tick over two ticks);
  all three kinds sidestepping; save/load (stuck
  before the save, Patrolling with index 0 and
  counter 0 after load, Stuck again after a fresh
  full circuit); and other-doctrine contrasts
  (HoldGround, Advance, Defend Core diagonal
  blocked steps do not sidestep). No demo
  scenario is added for this step alone, per the
  scope; the composed demo belongs to Step 3.
- Playtest-gate checklist item 37 added
  (Windows-only, in docs/slice-01-playtest-gate.md):
  an enclosed patrol reading STUCK while still
  firing, a diagonal sidestep, and a block-clears
  resumption, marker and text line read for each.
  Item 37 EXTENDS item 29 and orbital-command
  item 13; neither is superseded. Next free gate
  number is now 38.
- Tests: 2433 passed, 0 failed (was 2301; +132,
  no failures on the final runs). Demo unchanged:
  Slice 15 scenario save 44,016 chars, Slice 17
  scenario save 43,974 chars, all earlier demo
  saves as in the 0.77.0 entry.

## 0.77.0 — 2026-10-31 — Slice 18 scope: orbital remainders,
documentation-only

- Scope, decided in this step (rationale in
  DECISIONS.md): new docs/slice-18-scope.md scopes
  Slice 18 as the two carried orbital remainders
  from the Slice 17 review, in order — Step 1, the
  patrol route limits (what a patrol does when
  its waypoint step is refused, and what a fully
  blocked patrol does and reads as); Step 2, the
  free deploy primitive enforcement (what, in
  code, stops a play path from using the free,
  unpriced, uncapped deploy primitive, or why a
  documented rule remains enough); Step 3, a
  composed demo scenario plus a documentation-only
  review, which unlike the Slice 16 review is
  accompanied by a demo because both decisions
  are headless-testable. The scope decides theme,
  order, and the questions each step must answer
  — it decides neither answer: Step 1 must choose
  among a one-step sidestep, a hold, keeping the
  skip rule with a stuck readout, or keeping the
  skip rule unchanged with a stated reason, and
  Step 2 must choose among hard removal or
  privatisation, renaming, compiler marking, or
  keeping the rule-only status with a recorded
  play-path audit, each with a stated proof set.
- Order rationale, decided in this step (rationale
  in DECISIONS.md): patrol before deploy because
  the patrol question changes what a player sees
  a squad do on the field, while the deploy
  question changes what code paths are allowed
  and no play path uses the free path today, so
  its answer changes no player's field behaviour
  until a future play path is written; reversing
  the order would not break either step, and the
  scope says so, but the visible behaviour
  outranks a boundary rule with no current
  violator. Step 2's cases must be stated
  independently of Step 1's answer: a patrol
  deployed through the paid path must behave
  exactly as Step 1 decided, or Step 2 must say
  why the deploy path changes a doctrine's
  movement — it should not.
- Boundary, decided in this step (rationale in
  DECISIONS.md): both steps are orbital-remainder
  steps only. No price, cap, radius, waypoint,
  fire, targeting, delivery, precedence, conflict,
  rating, cost, or phase rule moves in Slice 18
  except as a step explicitly justifies and
  records; a sidestep in the shared movement rule
  would move every doctrine, so Step 1 must say
  whether its mechanism lives in MoveForPatrol
  alone or in the shared rule and prove the other
  doctrines unchanged, and a full pathfinder is a
  new system, not a sidestep, and must be
  justified as one. The deploy step must count
  its migration cost in the open: the test file
  calls DeploySquad on over a hundred lines, so
  hard removal is a suite-wide migration, not a
  one-line tightening, and any earlier demo save
  length it moves must be quoted as a new entry
  with rationale, never silently absorbed.
- Deliberately not moved in this step: no code, no
  demo scenario, no test, no checklist item, no
  patrol or deploy answer, no scene file, no shader
  file, no imported asset, no HUD, marker, tint,
  light, or core change, and no Slice 19 scope.
  This step is documentation-only by the scope
  pattern, so the test suite must stand unchanged.
  Windows Unity verification, the director
  playtest gate, Slice 13 observations, first-pass
  graphics retuning, and the transformer-adjacency
  invisibility are carried, not closed or re-scoped
  here; the transformer gap is the first candidate
  for Slice 19 scoping. The next free playtest-gate
  number is 37.
- Tests: 2301 passed, 0 failed (unchanged,
  documentation-only step). Demo unchanged: Slice 17
  scenario save 43,974 chars, Slice 15 scenario save
  44,016 chars, all earlier demo saves as in the
  0.76.0 entry.
- Next, in order: Slice 18 Step 1, the patrol route
  limits, decided in that step.

## 0.76.0 — 2026-10-31 — Slice 17 Step 3: composed demo
plus documentation-only review, closing Slice 17

- Step, decided in this step (rationale in
  DECISIONS.md): the composed demo scenario
  plus documentation-only review closing Slice
  17, as the Slice 17 scope requires. No new
  core system, no new query, no adapter edit,
  no checklist item, by design; Slice 18 is
  NOT scoped here.
- Demo, decided in this step (rationale in
  DECISIONS.md): demo/Forgeborne.Demo gains a
  Slice 17 Step 3 scenario after the Slice 15
  scenario — one Simulation, six rows on the
  y=20 plane four cells apart in z, so no row
  can leak voltage into another: a single-cable
  Conduit run to a mill (Step 1 case, cable
  Energised, mill Mismatched and destroyed on
  tick 1); a two-cable Conduit run to a mill
  (already-tinted control, both cables
  Energised, mill also destroyed on tick 1); a
  mixed-voltage Dynamo-plus-Conduit cable
  component with no consumer (still-untinted
  control, cable None); a mill directly beside
  a Conduit (Step 2 case, mill None, surviving,
  Conduit counting as one lone source, POWER
  line read before and after); a mill fed LV
  directly by a Dynamo (feeding control,
  Powered, gathering every tick, counting
  neither live nor lone); and a lone Conduit
  with no neighbour (lone control, the second
  lone source). No enemy is staged: the mill
  destruction is the live-network evidence
  beside the tint reading, as the scope
  requires, and a pylon proof would prove
  firing, not agreement between the two
  queries.
- Composed numbers, pinned so they are not
  over-claimed later: before any tick, 1/4
  mills powered, 2 warned, POWER 0 LIVE
  NETWORKS, 2 LONE SOURCES (0 live is itself
  a proof — both cable runs feed a mill, so
  neither counts live and no consumer count is
  double-counted; 2 lone is the direct Conduit
  plus the lone Conduit, and only those two).
  Tick 1: both cable-fed mills Air, destroyed
  2, powered gathered 1, kinetic gathered 0,
  direct-Conduit mill survives, POWER 2 LIVE,
  2 LONE (the two destroyed runs now feed
  nothing, so they count live; the lone number
  does not move). After 5 ticks: powered
  gathered 5 (Dynamo pocket exhausted), kinetic
  gathered 1 (direct-Conduit mill, 1 in 5
  ticks), destroyed 2, stone 6. After load:
  both destroyed mills stay Air, direct and
  Dynamo mills load, POWER still 2 LIVE, 2
  LONE, single cable still Energised, mixed
  cable still None, direct mill still None,
  Dynamo mill still Powered — both readings
  re-derived, not saved — counters reset
  (powered 0, kinetic 0, destroyed 0), stone
  stays 6, player at (16,5,16). Loaded phase,
  Dynamo pocket re-seeded: powered gathered 5
  again, kinetic gathered 1 again, destroyed 0,
  stone 12.
- Tests: 2301 passed, 0 failed (was 2227; +74
  composed-integration assertions mirroring
  the demo, inserted before the test runner's
  summary so they execute — the Slice 13 Step
  4 lesson — with no failures on the final
  runs). Demo: all earlier demo saves as in
  the 0.75.0 entry (Slice 1 43,881; Slice 2
  43,921; Step 1 44,016; Step 3 44,103; Slice 4
  44,026; Slice 5 43,974; Slice 6 44,024; Slice
  7 44,013; Slice 8 43,974; Slice 9 43,975;
  Slice 10 43,974; Slice 11 44,038; Slice 12
  43,974; Slice 13 44,045; Slice 14 43,974;
  Slice 15 44,016). New scenario save 43,974
  chars, quoted as its own entry, not as a
  change to any earlier save (it matches
  several earlier lengths by format coincidence
  only, a different save).
- Review: new docs/slice-17-review.md (gate
  checked against the scope, verdict: reviewed
  and closed as two display queries decided
  with tests and proved together in one wiring,
  not playtested, not Unity-verified, gaps
  carried in priority order, no new
  playtest-gate checklist item — next free
  number remains 37). Checklist relations
  restated, not moved: item 35 supersedes
  item 28's honest-observation clause for the
  decided tint case only, item 36 extends
  items 22 and 28, and neither new item claims
  the other's observation.
- Next, in order: scoping Slice 18 as its own
  decision step, chosen from the ranked
  candidates this review carries (patrol route
  limits and the free deploy primitive first,
  Slice 13 remainders as observations second,
  first-pass graphics retuning on Windows
  evidence third) — not promised here, and not
  scoped in this step.

## 0.75.0 — 2026-10-30 — Slice 17 Step 2: direct-Conduit count query decided

- Decision, decided in this step (rationale in
  DECISIONS.md): a Conduit standing directly
  beside a consumer now counts as one lone
  source, folded into the existing POWER line's
  lone number — no new line, no new word, no
  HUD redesign. Lone now means, in one
  sentence: a source with no cable or
  transformer neighbour, and no consumer
  neighbour that it feeds directly. A Dynamo or
  Alternator beside a consumer still counts
  neither live nor lone, because it feeds that
  consumer directly (rated or mismatched
  alike); a Conduit feeds no consumer directly,
  because it is absent from the direct branch,
  so a consumer neighbour alone no longer
  disqualifies it. A cable or transformer
  neighbour still disqualifies every source,
  Conduit included, even when that wiring
  delivers nothing — a transformer is wiring,
  not a consumer endpoint. The count is per
  source cell, not per consumer or per pair:
  two Conduits beside one mill are two lone
  sources, one Conduit beside two mills is one.
- Proof set, decided in this step (rationale in
  DECISIONS.md): the Slice 17 scope's cases,
  each proved by test — a Conduit beside a mill,
  a workstation, and a pylon (one rule for all
  three consumers, each reading 0 LIVE / 1
  LONE with its consumer's delivery still None
  and its consumer counts unmoved); a Dynamo
  beside a mill (LV, powered, 0 / 0) and an
  Alternator beside a mill (HV, mismatched and
  warned, 0 / 0), plus a Dynamo beside a pylon
  (LV mismatch, 0 / 0) and an Alternator beside
  a pylon (HV powered, 0 / 0); a Dynamo plus an
  Alternator beside one mill (direct-source
  conflict, None, 0 / 0, neither counted as
  lone, so the conflict is not hidden behind a
  lone number); a Conduit beside a Dynamo-fed
  mill (LV, 0 / 1) and beside an
  Alternator-fed mill (HV mismatched, 0 / 1);
  a Conduit beside a mill that also touches a
  cable (fed network 0 / 0, free network
  1 / 0 — never double-counted as both); two
  Conduits beside one mill (0 / 2) and one
  Conduit beside two mills (0 / 1); a Conduit
  beside a transformer alone (0 / 0, probe
  mill reads None) and beside a Dynamo-fed
  transformer (0 / 0, probe mill reads stepped
  HV); a lone Conduit with no neighbour (0 / 1,
  the control case); and save/load
  re-derivation (0 / 1 before and after load,
  HUD lone 1, consumer counts unmoved).
- Step 1 independence, stated explicitly: Step
  1 decides cable tint (component liveness),
  and this step's cases have no cable, so
  Step 1's answer changes no Step 2 case. The
  counts keep the Slice 12 meaning of "live"
  (carrying a non-None voltage, feeding no
  consumer) and do not adopt Step 1's tint
  vocabulary for counts: a direct Conduit is
  not a cable component, so component liveness
  has nothing to attach to here.
- HUD line, decided in this step (rationale in
  DECISIONS.md): the decided count folds into
  the existing POWER line's lone number, so
  HudDisplay's LivePowerText line text is
  unchanged and no adapter edit was needed.
  The rejected alternatives were keeping the
  definition unchanged (0 / 0 for a placement
  that went nowhere, indistinguishable from no
  source at all) and adding a distinct third
  readout (a second number for the same
  delivery fact — a source feeding nothing —
  split only by adjacency history, at the cost
  of a new HUD word against HUD discipline).
  Powered, warned, and total consumer counts
  do not move in any case: they are consumer
  counts, not source counts, and every case
  above proves them alongside the POWER line.
- Delivery boundary, decided in this step: no
  delivery, precedence, conflict, rating, cost,
  or phase rule moves. A Conduit beside a mill
  still delivers nothing and its mill still
  gathers kinetically and survives its tick; a
  Dynamo or Alternator beside a mill still
  delivers exactly as before. No material,
  shader, marker, light, fog, or view model
  value moves in this step.
- Checklist relation, decided in this step
  (rationale in DECISIONS.md): new playtest-gate
  checklist item 36, a Windows-only observation
  of the decided count — a Conduit beside a
  mill, a Dynamo beside a mill, and a lone
  Conduit, with the POWER line read aloud for
  each. Item 36 extends item 22's live-network
  and lone-source display (a lone-Conduit
  observation added; item 22 stands) and
  extends item 28's Conduit observation (item
  28's mill clause — kinetic cadence, no
  warning — stands; item 36 adds the POWER
  line reading item 28 did not ask for). It
  does not supersede item 28's mill clause and
  does not claim item 35's tint observation.
  The next free playtest-gate number is now 37.
- Superseded assertions, stated openly: three
  earlier assertions that recorded the old
  reading (the Slice 14 Step 2 composed lone
  counts before the tick, after tick 1, and
  after load) were updated from 0 to 1, each
  with its history kept in the assertion text
  and in the section header comment, and the
  Slice 14 demo's printed text was updated the
  same way — no assertion was deleted silently
  and no history was rewritten as if the old
  reading had never existed. No test was
  weakened to pass: the three updated
  assertions assert a stronger, decided
  reading, and the suite rose by 58 new
  assertions on top of them, with no failures.
- Tests: 2227 passed, 0 failed (was 2169; +58,
  no failures on the final runs). Demo
  unchanged except for the Slice 14 print
  wording: Slice 15 scenario save 44,016 chars,
  all earlier demo saves as in the 0.74.0 entry.
- Next, in order: Slice 17 Step 3, the composed
  demo scenario plus documentation-only review
  closing Slice 17, decided in that step.

## 0.74.0 — 2026-10-30 — Slice 17 Step 1: single-cable tint query decided

- Decision, decided in this step (rationale in
  DECISIONS.md): for a cable cell, Energised now
  means its cable component carries a non-None
  voltage — component liveness, not cell delivery.
  Implemented in WorldZone.PowerState only: the
  cable branch now reads
  CableNetworkVoltage(CableComponent(x, y, z),
  null) instead of DeliveredVoltage(x, y, z), so a
  single cable cell touching a Conduit reads
  Energised (HV) where it read None before, a
  two-cable run stays Energised on both cells, and
  a mixed-voltage component still reads None on
  every cell. DeliveredVoltage is untouched, so no
  delivery, precedence, conflict, rating, cost, or
  phase rule moves: a mill beside a Conduit still
  reads None and gathers kinetically, a cable-fed
  mill is still destroyed on tick 1, a cable-fed
  pylon still fires 10 per tick, and warnings still
  warn the same machines.
- Proof set, decided in this step (rationale in
  DECISIONS.md): the Slice 17 scope's cases, each
  proved by test — single cable Conduit-to-mill
  (Energised cable, Mismatched mill, HV delivery,
  one warned mill, destroyed on tick 1, cable
  still Energised after the destruction);
  single cable Conduit-to-air (Energised, one
  live network, no lone source); two Conduits on
  one single cable (Energised); two-cable Conduit
  run (both cells stay Energised); Dynamo-fed and
  Alternator-fed single cables (stay Energised);
  transformer-fed single cables from both an
  Alternator and a Dynamo (stay Energised);
  mixed Dynamo-Conduit, Dynamo-Alternator, and
  two-cable Dynamo-Conduit components (all stay
  None, so the component reading does not
  resurrect a dead network); unpowered cables
  (None, alone and beside stone only); rewiring at
  query level in both placement orders (cable
  alone None, Energised when a Conduit is placed
  beside it, None again when it is removed); and
  save/load re-derivation (Energised cable and
  Mismatched mill re-derive after load).
- Refresh consequence, decided in this step
  (rationale in DECISIONS.md): no new refresh
  event is needed. Both placement orders are
  successful build keys (I for cable, M for
  Conduit), which the existing event set already
  refreshes on; gather cannot rewire, because
  only Stone, EmberOre, and Wood are gatherable
  and no cable or source is; and no source is
  ever destroyed by any rule — only mills are,
  and a mill destruction already refreshes — so
  there is no source-removal path whose tint
  change could go unrefreshed. A future removal
  verb, if one is added, must refresh like the
  build keys do; that requirement is recorded,
  not implemented here.
- Checklist relation, decided in this step
  (rationale in DECISIONS.md): new playtest-gate
  checklist item 35, a Windows-only post-decision
  observation of a single-cable run, a two-cable
  run, and a mixed run together, against dark
  voxel and under dusk light, with the mill's
  destruction or the pylon's fire as the
  live-network evidence beside the tint, and
  with rewiring in both placement orders. Item
  35 supersedes item 28's honest-observation
  clause for the decided case only — an untinted
  single Conduit cable is now evidence the query
  or the refresh did not run, not an expected
  finding — while item 28's other observations
  stand; item 32 stands as the pre-decision
  observation, and item 35 is its post-decision
  counterpart, not a re-observation of item 32
  or of item 33's dusk light. The next free
  playtest-gate number is now 36.
- Superseded assertions, stated openly: four
  earlier assertions that recorded the old
  reading (one Slice 14 Step 1 single-cable
  assertion and three Slice 14 Step 2 composed
  assertions, including after load) were updated
  to Energised, each with its history kept in a
  comment, and the Slice 14 demo's printed text
  was updated to match — no assertion was
  deleted silently and no history was rewritten
  as if the old reading had never existed. No
  material, shader, marker, light, fog, view
  model, or HUD value moves in this step: the
  Slice 16 surface renders whatever this query
  decides, and the Energised tint colour itself
  is unchanged.
- Tests: 2169 passed, 0 failed (was 2130; +39,
  no failures on the final runs). Demo unchanged
  except for the Slice 14 print wording: Slice 15
  scenario save 44,016 chars, all earlier demo
  saves as in the 0.73.0 entry.
- Next, in order: Slice 17 Step 2, the
  direct-Conduit count query, decided in that
  step.

## 0.73.0 — 2026-10-30 — Slice 17 scope: electrical display-query remainders
Slice 17, documentation-only

- Scope, decided in this step (rationale in
  DECISIONS.md): new docs/slice-17-scope.md scopes
  Slice 17 as the two carried Slice 14 remainders
  from the Slice 16 review, in order — Step 1, the
  single-cable tint query (whether a cable cell's
  power state should also count its own component's
  voltage, so a single cable touching a Conduit
  reads Energised when its component carries HV);
  Step 2, the direct-Conduit count query (whether
  the HUD counts should distinguish a source beside
  a consumer that delivers nothing from one that
  delivers); Step 3, a composed demo scenario plus
  a documentation-only review, which unlike the
  Slice 16 review is accompanied by a demo because
  both queries are headless-testable. The scope
  decides theme, order, and the questions each step
  must answer — it decides neither query answer:
  Step 1 must choose between a cell-delivery
  reading and a component-liveness reading for
  Energised, and Step 2 must choose among keeping
  the lone definition, counting a non-delivering
  consumer-adjacent source as lone, or adding a
  distinct readout, each with a stated proof set.
- Order rationale, decided in this step (rationale
  in DECISIONS.md): tint before count because the
  tint is the visible misread in the scene — a live
  wire rendering dead, already observed by item 32's
  combined scene — while the count is a HUD-number
  invisibility one step removed from the rendered
  wire; reversing the order would not break either
  step, and the scope says so, but the rendered
  misread outranks the counted one. Step 2's cases
  have no cable, so Step 1's answer does not change
  them, and Step 2 must state that independence
  (and any shared-word adoption) explicitly rather
  than inheriting Step 1's vocabulary by accident.
- Boundary, decided in this step (rationale in
  DECISIONS.md): both steps are display-query steps
  only. No delivery, precedence, conflict, rating,
  cost, or phase rule moves in Slice 17; if a step
  finds its honest answer needs a delivery move, it
  records and defers that move, not smuggles it in.
  No material, shader, marker, light, fog, or view
  model value moves — the Slice 16 surface renders
  whatever Slice 17 decides — and no HUD redesign
  beyond the count step's single POWER line question.
  Slice 13 observations, Windows Unity verification,
  the director playtest gate, patrol limits, and the
  free deploy primitive are carried, not closed or
  re-scoped here; the next free playtest-gate number
  is 35.
- Deliberately not moved in this step: no code, no
  demo scenario, no test, no checklist item, no
  query answer, no scene file, no shader file, no
  imported asset, no HUD, marker, tint, light, or
  core change, and no Slice 18 scope. This step is
  documentation-only by the scope pattern, so the
  test suite must stand unchanged.
- Tests: 2130 passed, 0 failed (unchanged,
  documentation-only step). Demo unchanged: Slice 15
  scenario save 44,016 chars, all earlier demo saves
  as in the 0.72.0 entry.
- Next, in order: Slice 17 Step 1, the single-cable
  tint query, decided in that step.

## 0.72.0 — 2026-10-30 — Slice 16 Step 5: review closing
Slice 16, documentation-only

- Review, decided in this step (rationale in
  DECISIONS.md): new docs/slice-16-review.md checks
  Slice 16 against docs/slice-16-scope.md and closes
  it as a reviewed whole — a written scene-proof
  layer plus Windows checklist items, not a
  playtested, Unity-verified, or visual result.
  This step is documentation-only by the scope's own
  terms, with one honest difference from earlier
  reviews stated in the review itself: it is NOT
  accompanied by a new composed demo scenario,
  because no graphics claim in Steps 1–4 is
  headless-provable — a demo scenario printing ammo
  numbers or tint colours would prove the core
  numbers again, not the material, the wiring, the
  occlusion, the light, or the model. The review
  re-runs the full suite and the whole demo instead,
  and quotes the demo save lengths as a
  no-core-change check only.
- Gate check, decided in this step (rationale in
  DECISIONS.md): every Slice 16 step pushed with
  passing tests (for the adapter-only steps, with
  the suite unchanged and the adapter-only
  verification stated, never inflated into a test
  claim); each step's decision recorded with
  rationale and its own Windows checklist item
  (item 30 material assignment, item 31 wiring
  null-fill, item 32 combined occlusion and
  readability under the Step 1 material, item 33
  the same combined scene under dusk light, item 34
  the view model); no MeshBuilder colour, no
  PowerTint value, no PowerState query, no refresh
  event set, no HUD layout or colour, and no core
  rule changed; every earlier demo save length
  unchanged. Checklist relations are stated, not
  left to miscount: item 26 and item 27 are NOT
  closed by Steps 1–4 (items 30 and 31 observe
  beside them); item 32 does NOT supersede item 25
  or orbital-view item 8; item 33 does NOT supersede
  item 32; item 34 does NOT claim a per-weapon
  model. This review adds no new checklist item —
  the next free playtest-gate number is 35 — and
  it does NOT scope Slice 17.
- Gaps carried, in priority order (rationale in
  DECISIONS.md): Windows Unity verification for
  Slices 1–16 (items 25–27 and 30–34, plus scene
  assembly including the editor-built gradient
  skybox material); the Slice 1 director playtest
  gate, still pending; first-pass graphics values
  unproven (dusk azimuth, fog contrast arithmetic,
  view model offset, static weapon-agnostic model);
  the Slice 13 scene-proof observations still unmade
  as observations; the single-cable tint finding;
  the direct-Conduit count invisibility; the patrol
  route's stated limits; the free deploy primitive.
  Closed by this slice in the build sense only: the
  director-ordered scene-proof build itself — the
  Slice 14 remainders in gaps 5 and 6 are now the
  front of the queue again, unless a later director
  decision reorders them.
- Deliberately not moved in this step: no code, no
  demo scenario, no test, no checklist item, no
  scene file, no shader file, no imported asset, no
  HUD, marker, tint, light, or core change, and no
  Slice 17 scope. Unity adapter code is not compiled
  headlessly, following the Slice 13 Step 2
  precedent, so this step adds no headless test for
  the review itself.
- Tests: 2130 passed, 0 failed (unchanged,
  documentation-only step). Demo unchanged: Slice 15
  scenario save 44,016 chars, all earlier demo saves
  as in the 0.71.0 entry.
- Next, in order: scoping Slice 17 as its own
  decision step, chosen from the ranked candidates
  this review carries, decided in that step.

## 0.71.0 — 2026-10-30 — Slice 16 Step 4: first FPS weapon
view model script

- View model, decided in this step (rationale in
  DECISIONS.md): new adapter
  ForgeborneUnity/Assets/Scripts/WeaponViewModel.cs
  builds a first, static, original view model from
  primitives at Start and parents it to the camera
  object carrying FpsInputAdapter (CameraAnchor
  field, camera-local offset 0.22, -0.22, 0.60, so
  the model sits lower-right with the crosshair
  clear). Shape: worn dark-metal receiver, barrel,
  stock, and grip; a bright cyan energy strip along
  the receiver and a small cyan sight dot in a dark
  sight housing; one small orange accent panel on
  the receiver's right side. No imported art, no
  copied rifle, no reproduced AMMO / TARGET readout
  layout — the reference remains style only. If the
  anchor is unwired, no model is built and a warning
  logs, so an unwired scene shows a visible absence
  rather than a rifle floating at the world origin.
  Colliders are removed from every part, so the
  model cannot block the picker raycast.
- Data, decided in this step (rationale in
  DECISIONS.md): the model carries no number and no
  ammo, damage, or reload rule of its own. Numeric
  ammo — name, mag, reserve — stays in HudDisplay's
  AmmoText from HudState, so the model and the HUD
  ammo line can never contradict each other, and
  the script takes no Simulation field rather than
  polling HudState and discarding it or inventing a
  glow rule. Geometry is weapon-agnostic, stated
  plainly: one shape for the Pulse Rifle and for
  any GeneratedWeapon, whose stats change the HUD
  line and nothing about the model. A per-weapon
  model is a later slice, not promised here.
- Motion, deferred openly: no sway, bob, or recoil.
  Each is a motion rule needing tuning against the
  camera motion FpsInputAdapter already owns and a
  Windows observation of its own, so none is
  smuggled in as polish on a static first-pass.
- Readability contract, decided in this step
  (rationale in DECISIONS.md): new playtest-gate
  item 34 observes the model on Windows — frame
  placement and sight readability, non-obstruction
  of crosshair, ammo line, picker, and objective
  together, mag falling on fire and restoring on
  reload in the HUD line while the model stays
  unchanged, exactly one model after a save/load
  round-trip with HUD ammo matching the save, and
  honest wall-clipping observation at close range
  as evidence for a later viewmodel-layer step.
- Deliberately not moved in this step: no core file,
  no MeshBuilder colour, no PowerTint value, no
  PowerState query, no refresh event set, no HUD
  layout or colour, no marker colour, shape, size,
  or position, no light, sky, or fog value, no
  FpsInputAdapter or GameManager edit, no camera
  change, no scene file, no shader file, no
  imported asset. Unity adapter code is not compiled
  headlessly, following the Slice 13 Step 2 precedent,
  so this step adds no headless test and no demo
  scenario for the model's look — a printed ammo
  line does not prove a visible model.
- Tests: 2130 passed, 0 failed (unchanged,
  adapter-only step). Demo unchanged: Slice 15
  scenario save 44,016 chars, all earlier demo saves
  as in the 0.70.0 entry.
- Next, in order: Slice 16 Step 5, composed review
  closing Slice 16, documentation-only with no new
  demo scenario, decided in that step.

## 0.70.0 — 2026-10-30 — Slice 16 Step 3: lighting, sky,
and fog to art-direction

- Lighting, decided in this step (rationale in
  DECISIONS.md): new adapter
  ForgeborneUnity/Assets/Scripts/DuskLighting.cs
  sets the scene's dusk light in code at Awake — a
  directional sun, warm (1.00, 0.82, 0.60),
  intensity 1.1, low at about 15 degrees above the
  horizon, azimuth 235 first-pass, soft shadows;
  flat ambient, storm blue-grey (0.32, 0.38, 0.46),
  so shadow sides read blue-grey instead of black;
  and fog, exponential, density 0.008, storm
  blue-grey (0.45, 0.52, 0.60), giving depth beyond
  the fight while leaving about 83% contrast at the
  far observation band of about 24 cells. All values
  are public fields on the script, so the scene
  author tunes through the Inspector without editing
  code, and all are labelled first-pass — chosen
  for readability and testability, not final feel.
  An Inspector-wired light is used when present;
  unwired, the script creates one named
  ForgeborneDuskLight. No GameManager field is
  needed; the script stands alone like
  SquadMarkerDisplay.
- Sky, decided in this step (rationale in
  DECISIONS.md): a stated combination, not a pure
  script and not pure notes. Light, fog, and ambient
  are code-set; the sky is an Inspector-wired
  material, because a gradient skybox needs an
  editor-built material and a headless run writes
  no shader file and no scene file and claims no
  built scene. The decided gradient spec, recorded
  in the script header, in docs/art-direction.md,
  and in DECISIONS.md, is zenith storm blue-grey
  (0.20, 0.26, 0.34) to horizon lightened storm
  blue-grey (0.52, 0.54, 0.58), original only, no
  planet, no skyline. If no material is wired, the
  script logs a warning and leaves the scene's
  existing skybox untouched, so a deliberate sky is
  never replaced by a runtime guess — and item 33
  observes the sky as actually built, not the spec.
- Readability contract, decided in this step
  (rationale in DECISIONS.md): new playtest-gate
  item 33 re-observes item 32's whole combined scene
  under this dusk light — voxel edges at near, mid,
  and far bands, all tint and marker separations
  together, HUD cyan and orange against the dusk
  scene, dusk mood readable and not horror-dark,
  and fog depth confirmed or refuted at the far
  band. The separations are predicted, not claimed,
  from the light arithmetic in the script header:
  warm light keeps cyan apart from Powered green on
  the blue channel and Mismatched red apart from
  orange rally on the green channel. If the light
  makes any colour unread, the light changes — the
  palette does not move to fit the light.
- Deliberately not moved in this step: no core file,
  no MeshBuilder colour, no PowerTint value, no
  PowerState query, no refresh event set, no HUD
  layout or colour, no marker colour, shape, size,
  or position, no scene file, no shader file, no
  imported asset, and no camera change — the camera
  stays FpsInputAdapter's. Art-direction rules 1–7
  are unchanged; only a first-pass values note for
  rules 1 and 6 is appended to docs/art-direction.md.
  Unity adapter code is not compiled headlessly,
  following the Slice 13 Step 2 precedent, so this
  step adds no headless test and no demo scenario
  for light, sky, or fog — a printed value does not
  prove a visible dusk. Item 32 stands alongside
  item 33 as the pre-dusk observation; item 33
  extends it, as item 32 itself promised.
- Tests: 2130 passed, 0 failed (unchanged,
  adapter-only step). Demo unchanged: Slice 15
  scenario save 44,016 chars, all earlier demo saves
  as in the 0.69.0 entry.
- Next, in order: Slice 16 Step 4, first FPS weapon
  view model script, decided in that step.

## 0.69.0 — 2026-10-30 — Slice 16 Step 2: marker occlusion
under the new scene

- Occlusion, decided in this step (rationale in
  DECISIONS.md): honest depth occlusion is REAFFIRMED
  under the Step 1 material, not changed. Step 1
  changed only the chunk mesh's material; markers
  carry their own opaque URP Lit materials (Standard
  fallback) with default depth behaviour, so the
  occluder changed but the occlusion mechanism did
  not. No renderQueue, depth-test, or shader change
  is made here, and no marker colour, shape, size,
  or position moves. A through-wall marker would
  still be a wallhack claim, and no Windows evidence
  yet exists that honest occlusion makes markers
  useless at FPS distance, so no readability aid is
  adopted now — size, brightness, and outline aids
  are named as candidates for a later step only if
  item 32's observation proves uselessness.
- Readability, decided in this step (rationale in
  DECISIONS.md): new playtest-gate item 32 observes
  occlusion and readability together, in one combined
  scene, not one marker at a time — all three kind
  markers, rally and pending markers, a green Powered
  mill, a red Mismatched workstation, and a yellow
  Energised cable run, at near (2-3 cells), mid
  (about 10 cells), and far (bench-to-core scale,
  about 24 cells Manhattan) FPS distance bands,
  against dark voxel and the scene's current sky.
  The separations are grounded in the values already
  in code: Powered green (0.25, 0.9, 0.25) is never
  cyan, Mismatched red (1, 0.15, 0.15) is never
  orange, Energised yellow (1, 0.85, 0.1) sits on
  ground cable cells and never floats, and the
  light-grey Heavy cylinder separates from untinted
  white voxels by shape and centre position, not
  colour alone. Item 32 does NOT observe the Step 3
  dusk light, which is not yet built — Step 3's own
  checklist item re-observes this same combined
  scene under that light. Item 32 extends, and does
  not supersede, item 25 (kind distinction and slot
  reuse) or orbital-view item 8 (baseline occlusion);
  both stand alongside it, and a note to that effect
  is added to docs/orbital-view.md.
- Deliberately not moved in this step: no core file,
  no MeshBuilder colour, no PowerTint value, no
  PowerState query, no refresh event set, no HUD
  line, no marker, no light, no sky, no fog, and no
  scene file. The only adapter change is a comment
  in SquadMarkerDisplay.cs recording the
  reaffirmation, so the decision is visible where
  the markers are built. Unity adapter code is not
  compiled headlessly, following the Slice 13 Step
  2 precedent, so this step adds no headless test
  and no demo scenario for occlusion — a printed
  snapshot line does not prove a visible marker.
- Tests: 2130 passed, 0 failed (unchanged,
  adapter-only step). Demo unchanged: Slice 15
  scenario save 44,016 chars, all earlier demo saves
  as in the 0.68.0 entry.
- Next, in order: Slice 16 Step 3, lighting, sky,
  and fog to docs/art-direction.md, decided in that
  step.

## 0.68.0 — 2026-10-30 — Slice 16 Step 1: vertex-colour chunk
material and MeshRenderer wiring

- Material, decided in this step (rationale in
  DECISIONS.md): VoxelMeshRenderer.Awake now assigns
  the chunk material, before GameManager.Start calls
  the first RefreshMesh. An Inspector-wired
  ChunkMaterial wins when present; unwired, Awake
  creates a runtime material from the Universal Render
  Pipeline/Lit shader, falling back to Standard — the
  same shader pair SquadMarkerDisplay already uses,
  and the project already depends on URP 17.0.0, so
  no new shader asset, texture, or imported material
  rides along. If neither shader is found the
  Inspector material is left untouched and a warning
  logs, rather than a null material silently
  rendering pink. Assigning a material does NOT prove
  it multiplies vertex colours: playtest-gate item
  26 remains the observation that settles the tint,
  and new item 30 records which shader was actually
  assigned and whether untinted stone, ore, and wood
  still render exactly as before.
- Wiring, decided in this step (rationale in
  DECISIONS.md): GameManager gains an optional,
  Inspector-wired FpsAdapter field, and Start
  null-fills a null FpsInputAdapter.MeshRenderer
  with the same VoxelMeshRenderer it already links
  — the exact failure item 27 names. A non-null
  adapter field is never overwritten, so a
  deliberate second renderer survives; Start runs
  before any adapter Update, so the fill lands
  before the first tick's refresh check; and if the
  adapter is still unwired after the fill a warning
  logs naming the stale-mesh consequence, rather
  than a silent first mesh that never refreshes.
  The adapter's Simulation field is NOT rewired
  here — it stays Inspector-wired as before, so
  this is one wiring change, not a scene-rewiring
  pass. New playtest-gate item 31 observes the
  fill, its non-overwrite rule, and its warning;
  item 27 is NOT closed by this step and remains
  the observation that each builder key, gather,
  and destruction appears on its tick.
- Deliberately not moved in this step: no
  MeshBuilder colour, no PowerTint value, no
  PowerState query, and no refresh event set — the
  Slice 13 event set (successful key, gather,
  destruction, load, Start, and nothing else) is
  reused, not rewritten, so no frame refreshes and
  no re-tint flatters the material. No core file
  touched, no save field added, no HUD line, marker,
  or light changed. Unity adapter code is not
  compiled headlessly, following the Slice 13 Step
  2 precedent, so this step adds no headless test
  and no demo scenario for the material or the
  wiring.
- Tests: 2130 passed, 0 failed (unchanged,
  adapter-only step). Demo unchanged: Slice 15
  scenario save 44,016 chars, all earlier demo saves
  as in the 0.67.0 entry.
- Next, in order: Slice 16 Step 2, marker occlusion
  under the new scene, decided in that step.

## 0.67.0 — 2026-10-30 — Slice 16 scope: graphics scene-proof layer

- Scope, decided in this step (rationale in
  DECISIONS.md): new docs/slice-16-scope.md. Slice 16
  is the graphics slice the director ordered after
  Slice 15 closes (Rob, 2026-10-30 09:27 EDT), ahead
  of the previously ranked Slice 16 candidates, which
  are carried after graphics as Slice 17 candidates
  in their relative order. In order: (1) a
  vertex-colour chunk material and MeshRenderer
  wiring step, deciding the material, its assignment
  and fallback, and whether the adapter wiring stays
  Inspector-wired or becomes code-driven, with new
  checklist items from 30 and items 26/27 left as the
  Windows observations that prove them; (2) a marker
  occlusion step, deciding whether honest depth
  occlusion stands under the new material and light
  or changes by a stated, non-wallhack aid; (3) a
  lighting, sky, and fog step to docs/art-direction.md
  rules 1 and 6, with a readability contract for
  voxel edges, tints, markers, and HUD; (4) a first
  FPS weapon view model script, simple and original,
  lower-right frame, rendering core Weapon state
  through HudState with no second ammo rule; (5) a
  documentation-only review, with no new composed
  demo scenario, because no graphics claim is
  headless-provable. Adapter-only where Unity is
  concerned, following the Slice 13 pattern.
- Deliberately not in this scope: a display-query
  redesign of the single-cable tint or the
  direct-Conduit count (carried after graphics); a
  scene file, imported asset, texture, or shader
  claimed as a built scene from a headless run; a
  copied asset or look from the reference image or
  any other game; a HUD redesign; any core change;
  or Windows Unity verification itself, which stays
  open for Slices 1–16 alike.
- Tests: 2130 passed, 0 failed (unchanged,
  documentation-only step). Demo unchanged: Slice 15
  scenario save 44,016 chars, all earlier demo saves
  as in the 0.66.0 entry.
- Next, in order: Slice 16 Step 1, the vertex-colour
  chunk material and MeshRenderer wiring, decided in
  that step.

## 0.66.0 — 2026-10-30 — Slice 15 Step 2: composed demo + review, closing Slice 15

- Composed demo, decided in this step (rationale in
  DECISIONS.md): one Simulation proves the Step 1 Patrol
  beside the doctrines the reader already trusts, not as
  separate proofs. A Recon Patrol (range 5, 12 damage per
  tick, so a 30-HP enemy takes three hits) deploys paid
  through the picker to rally (20,5,20), and a Heavy
  HoldGround deploys paid through the picker to rally
  (10,5,10); both deploy at the player (16,5,16), 20
  EmberOre spent, 0 left. One enemy staged east of the
  loop at (26,5,20) walks toward the nearest squad and
  meets the patrol on its route: tick 5 at (19,18) the
  enemy drops 30 -> 18 HP, tick 6 at (19,19) it drops
  18 -> 6 HP standing on the rally cell itself, tick 7
  at (20,19) it dies — three passing shots, no chase,
  the patrol still on route and reading Patrolling
  with no hunting target throughout. The patrol reaches
  its rally on tick 8, the east waypoint on tick 11
  (turning without dwelling on tick 12), the south
  waypoint on tick 17, the west on tick 23, the north
  on tick 29, and its rally again on tick 32 — a full
  24-tick circuit, so the route repeats. The Heavy
  reaches its rally on tick 12 and holds there at full
  100 HP. Through a save/load round-trip the patrol
  loads at its rally with its waypoint index reset to
  0, proved by its first loaded tick turning east
  where a persisted waypoint 4 would have stepped
  north; it reaches the east waypoint again on loaded
  tick 3 and is still patrolling on loaded tick 8.
  No new core system rides along with this step.
- Review, decided in this step: new
  docs/slice-15-review.md (gate checked against the
  scope, verdict: reviewed and closed as a tested
  fourth doctrine plus a composed demo, not
  playtested, balanced, or Unity-verified, Slice 15
  remainders carried in priority order, no new
  playtest-gate checklist item — the demo exercises
  no adapter binding item 29 / orbital-command item
  13 do not already cover).
- Queue decision recorded in this step, director-made
  2026-10-30 09:27 EDT (not an Ember decision): graphics
  is the next slice after Slice 15 closes, ahead of
  the previously ranked Slice 16 candidates, which
  are carried after graphics unless a later director
  decision reorders them. Graphics as directed is the
  scene-proof layer: vertex-colour URP material and
  MeshRenderer wiring, marker occlusion, lighting /
  sky / fog to docs/art-direction.md, and a first FPS
  weapon view model script — adapter-only where Unity
  is concerned, with Windows checklist items,
  following the Slice 13 pattern. Recorded in the
  Slice 15 review and in DECISIONS.md, so it is
  visible in the repo, not only in a schedule body.
  This step does NOT scope the graphics slice: its
  own scope document is the next decision step.
- Tests: 2130 passed, 0 failed (was 2069; +61, no
  failures on the final runs). Demo unchanged except
  for the new scenario: Slice 14 scenario save 43,974
  chars, all earlier demo saves as in the 0.65.0
  entry. New scenario save 44,016 chars.
- Next, in order: scoping the graphics slice as its
  own decision step, per the director queue decision
  recorded above.

## 0.65.0 — 2026-10-30 — Slice 15 Step 1: fourth doctrine Patrol, a rally loop walked and repeated

- Doctrine, decided in this step (rationale in
  DECISIONS.md): new TacticalDoctrine.Patrol = 3,
  appended only, so HoldGround 0, Advance 1, and
  Defend Core 2 are unchanged and old saves still
  load. Patrol walks a closed loop derived from the
  rally alone, in order: the rally itself, then 3
  east, 3 south, 3 west, and 3 north of it
  (PatrolLoopRadius 3, PatrolWaypointCount 5, flat at
  rally height), then back to the rally, forever. A
  squad deploys marching to waypoint 0 like
  HoldGround, then keeps walking instead of holding.
- Route storage and save answer, decided in this
  step: the route is derived from the rally on every
  tick, never stored and never saved — no save field
  is added, moved, or reinterpreted, and the loader's
  9-int squad array and its rejection of undefined
  doctrines are unchanged. The only new state is the
  live waypoint index (PatrolWaypointIndex), which
  starts at 0 on deployment and resets to 0 on load,
  so a loaded patrol marches to its rally first and
  then resumes the loop from there. A spawn-derived
  route was rejected because the squad does not
  store its spawn, so it would have needed a new save
  field for what the rally alone already defines; a
  second picker point was rejected for the same
  reason, plus a picker change no proof needed.
- Endpoint and blocked-step behaviour, decided in
  this step: at a waypoint the squad turns and
  continues in the same tick (no dwell), through the
  shared one-voxel-per-tick step rule (same axis
  choice, tie-break, and solid / enemy-occupied
  refusals as every other doctrine). A refused step
  skips that waypoint for this circuit (index
  advances, no move that tick) instead of holding at
  a wall forever; the skipped waypoint is tried again
  on the next circuit. No pathfinder is added: a wall
  on the direct axis causes a skip, not a detour,
  and a squad whose every waypoint is blocked cycles
  its index without moving — stated as a limit, not
  hidden as a hold.
- Engagement, decided in this step: fire whenever an
  enemy is in the kind's range (Advance's and Defend
  Core's rule, not HoldGround's hold-only rule), and
  shoot in passing without chasing — movement stays
  route-bound, so a patrol that fires is still on
  route on the next tick, and there is no chase to
  return from. This is Advance's opposite on
  engagement: Advance walks to its prey, Patrol walks
  its route and shoots what the route brings into
  range. FindTarget needed no new branch: only
  HoldGround is restricted there, so Patrol already
  fired whenever in range, and this step locks that
  reading with tests instead of adding a rule.
- Kinds, price, and cap, decided in this step: all
  three kinds patrol (Vanguard, Recon, Heavy proved),
  with no kind re-priced or re-ranged; deployment
  stays the paid path, 10 EmberOre, cap 3 live
  squads, through the picker and TryDeployPaidSquad
  unchanged. Display: new SquadActivity.Patrolling =
  5, appended only — its own word, not a reuse of
  Marching (which already means both a HoldGround
  rally march and a Defend Core return march) and
  not Hunting (which would claim a prey the squad
  never walks toward); a patrolling squad reads
  Patrolling with no hunting target even with an
  enemy in range. No new marker shape: the Slice 13
  kind markers stand, the text line carries the
  doctrine word. Adapter: Alpha7 selects Patrol while
  picking (confirmed free: picker keys were 1–6 plus
  Return / Esc / O, no builder fires while picking),
  playtest-gate checklist item 29 and
  orbital-command checklist item 13 (both
  Windows-only).
- Tests: 2069 passed, 0 failed (was 1988; +81, no
  failures on the final runs). Demo unchanged: Slice
  14 scenario save 43,974 chars, all earlier demo
  saves as in the 0.64.0 entry.
- Next, in order: Slice 15 Step 2, composed demo
  scenario + documentation-only review, closing
  Slice 15.

## 0.64.0 — 2026-10-30 — Slice 15 scope: the deferred doctrine, Patrol

- Scope, decided in this step (rationale in
  DECISIONS.md): new docs/slice-15-scope.md — Slice 15
  theme is the deferred doctrine, in order: (1) exactly
  one fourth doctrine, decided in its step, with Patrol
  as the starting pool (its route question — loop,
  back-and-forth, or waypoints, and where the route is
  stored — still unanswered after deferrals in Slice 4
  and Slice 11); (2) Slice 15 demo scenario +
  documentation-only review. Slice 14 remainders
  (single-cable tint query, direct-Conduit count) and
  Slice 13 scene-proof remainders are carried as ranked
  Slice 16 candidates, not built here, because a
  display-query redesign inside a doctrine step would
  move the display the doctrine is proved against.
- Tests: 1988 passed, 0 failed (unchanged,
  documentation-only step). Demo unchanged: Slice 14
  scenario save 43,974 chars, all earlier demo saves
  as in the 0.63.0 entry.
- Next, in order: Slice 15 Step 1, exactly one fourth
  doctrine, decided in that step.

## 0.63.0 — 2026-10-30 — Slice 14 Step 2: composed demo + review, closing Slice 14

- Composed demo, decided in this step (rationale in
  DECISIONS.md): one Simulation proves the Step 1 Conduit
  beside the sources the reader already trusts, not as
  separate proofs. On the y=20 plane a mill fed HV through
  a Conduit -> cable -> cable run (destroyed tick 1), a
  mill fed LV directly by a Dynamo with a five-stone
  pocket (powered, 5 in 5 ticks), and a mill standing
  directly beside a Conduit with no cable (reads None,
  warns nothing, survives, gathers 1 kinetic stone in 5
  ticks); on the y=5 plane a Shock Pylon fed HV through a
  single Conduit -> cable run (powered, kills its staged
  enemy in three 10-damage ticks) and a workstation fed LV
  through a Conduit -> cable -> transformer step-down
  beside the player spawn (powered, bench regen 10 from
  the first tick), through a save/load round-trip with a
  loaded 5-tick phase repeating powered 5 and kinetic 1.
- Two findings, proved in composition and carried, not
  fixed: the single cable cell touching a Conduit reads
  None in PowerState (untinted) while its pylon fires and
  its mill is destroyed, because DeliveredVoltage for a
  cell counts only its neighbours and a Conduit is not a
  direct source, while a two-cable Conduit run reads
  Energised on both cells; and a Conduit standing
  directly beside a mill counts as neither a live network
  nor a lone source, because the Slice 12 lone rule
  disqualifies any source beside a consumer, whatever it
  delivers — so the demo's POWER line reads 0 LIVE, 0 LONE
  before the tick and 1 LIVE, 0 LONE after it and after
  load. Neither finding is a delivery failure, and neither
  is patched inside this demo step; both are remainders
  for Slice 15 scoping, with checklist item 28 already
  asking the scene observer to record the single-cable
  cell honestly. No new playtest-gate checklist item is
  added by this step.
- Review: new docs/slice-14-review.md, documentation-only
  review closing Slice 14 (gate checked against the scope,
  verdict: reviewed and closed as a tested third source
  plus a composed demo, not playtested, balanced, or
  Unity-verified, Slice 15 NOT scoped, candidates carried
  in scope order). Demo staging notes pinned in the
  review: TrySet staging, no squads in this demo (so it
  proves electrical composition only), no bench walk (the
  step-down bench sits beside the spawn), and the pylon
  kill at shrinking range, not a range-5 boundary proof.
- Tests: 1988 passed, 0 failed (was 1894; +94 composed-
  integration tests mirroring the demo, no failures on
  the final runs). Demo numbers: HUD before 1/3 mills,
  1/1 workstations, 1/1 pylons, WARNED 1/0/0, POWER 0
  LIVE, 0 LONE; tick 1 mill Air, destroyed 1, powered 1,
  kinetic 0, shields 65 -> 75, enemy (11,5,17) 20 HP,
  single cable None; tick 3 pylon kill, enemies 0; after
  5 ticks powered 5, kinetic 1, destroyed 1, stone 6,
  shields capped 85; after load HUD 1/2 mills, 1/1
  workstations, 1/1 pylons, WARNED 0/0/0, POWER 1 LIVE,
  0 LONE, counters reset, stone 6, player back at bench
  reading regen 10; loaded phase powered 5, kinetic 1,
  destroyed 0, stone 12, shields capped 85, enemies 0.
  All earlier demo saves unchanged (Slice 1 43,881; Slice
  2 43,921; Step 1 44,016; Step 3 44,103; Slice 4 44,026;
  Slice 5 43,974; Slice 6 44,024; Slice 7 44,013; Slice 8
  43,974; Slice 9 43,975; Slice 10 43,974; Slice 11 44,038;
  Slice 12 43,974; Slice 13 44,045). New scenario save
  43,974 chars.
- Next, in order: scoping Slice 15 as its own decision
  step, chosen from the ranked candidates this review
  carries (Patrol first, Slice 14 remainders second,
  Slice 13 scene-proof remainders third).

## 0.62.0 — 2026-10-30 — Slice 14 Step 1: third power source, the Conduit (cable-only HV)

- Source, decided in this step (rationale in DECISIONS.md):
  new BlockType.Conduit = 13, appended only, output HV
  (VoltageRules.ConduitOutput), cost 5 Stone + 5 EmberOre,
  Simulation.BuildConduitAhead, adapter key M (suppressed
  while the picker is open), playtest-gate checklist item 28
  (Windows-only). Its distinctness is its delivery shape,
  not its voltage and not its price: a Conduit delivers
  nothing face-adjacent — a mill, workstation, pylon, or
  transformer standing beside one reads None — and delivers
  HV only through a Cable network touching it. Safe use for
  LV consumers runs through a transformer step-down fed by
  that cable, exactly as the Alternator's does.
- Generalisation, with precedence and conflict unchanged:
  DeliveredVoltage, TransformerOutput, and
  CableNetworkVoltage now know three sources, but the
  direct set stays two: the Conduit is absent from both
  direct branches by decision, so a Dynamo beside the same
  mill as a Conduit still delivers LV (no direct conflict,
  because the Conduit contributes nothing directly), while
  a Conduit and a Dynamo both touching one cable component
  resolve that component to None (mixed network), a Conduit
  and an Alternator on one component agree at HV, and two
  Conduits on one component agree at HV. No delivery,
  precedence, conflict, rating, cost, or phase rule for any
  existing block moves; the ladder stays LV / HV / None.
- Non-output behaviour, stated precisely: a Conduit that
  touches no cable is not broken, fuelled, or conditional —
  it is unwired, and it reads None everywhere face-adjacent
  until cabled. There is no per-source state, so there is
  no save question beyond chunk bytes: a Conduit and its
  cable chain re-derive HV on load, and old saves load
  unchanged (earlier block values untouched).
- Readouts, proved not assumed: the existing warned counts
  and PowerState query cover the Conduit through its cable
  delivery (a cable-fed mill warns and reads Mismatched, a
  cable-fed pylon reads Powered, a cable-fed workstation
  reads Mismatched), and the existing live-network /
  lone-source counts cover it with no new HUD count (a lone
  Conduit counts 1 lone source, two adjacent Conduits count
  2, a Conduit cabled to nothing counts 1 live network, a
  Conduit cabled to a mill counts neither). One finding is
  recorded, not fixed, per the scope: a single cable cell
  touching a Conduit reads None in PowerState (untinted),
  because DeliveredVoltage for a cell counts only its
  neighbours and a Conduit is not a direct source — the
  network is live (its mill is destroyed) while its only
  cable cell reads untinted. A two-cable Conduit run reads
  Energised on both cells. This remainder is carried to the
  Slice 14 review; redesigning the query inside a source
  step would have violated the scope.
- Tests: 1894 passed, 0 failed (was 1807; +87, no failures
  on the final runs). Demo unchanged: Slice 12 scenario save
  43,974 chars, Slice 13 scenario save 44,045 chars, all
  earlier demo saves as in the 0.60.0 entry.
- Next, in order: Slice 14 Step 2, composed demo scenario
  plus documentation-only review closing Slice 14, with the
  single-cable tint finding carried as a remainder.

## 0.61.0 — 2026-10-30 — Slice 14 scope: deferred third source, then demo + review

- Scope, decided in this step (rationale in DECISIONS.md):
  new docs/slice-14-scope.md, documentation only, no code
  changed, no system built by it. Slice 14 is the deferred
  electrical widening, in order: Step 1, exactly one third
  power source, decided in that step; Step 2, a composed
  demo scenario plus a documentation-only review closing
  Slice 14. The scope takes the Slice 13 review's ranked
  candidates unchanged — third source first, Patrol second,
  Slice 13 remainders third — because the Slice 13
  composition evidence gives no reason to reorder them.
- Scope question, grounded in current code facts verified
  this run: two sources, both always-on while placed
  (Dynamo LV for 5 EmberOre, Alternator HV for 10 Stone),
  three consumers on a two-level ladder LV / HV / None
  (mill and workstation rated LV, Shock Pylon rated HV),
  block types appended through ShockPylon = 12. The third
  source must therefore deliver LV or HV, not a new level:
  a third voltage level would be a ladder redesign, not a
  single source step, and is out of scope for Step 1. Its
  distinctness must come from elsewhere — a condition, a
  fuel, a cost shape, a delivery shape, or a pulsed
  output, none of them chosen here — and must be proved
  by a test neither existing source can pass, the way the
  Alternator's HV and the pylon's HV rating were proved.
  A cost-only difference at the same voltage with the
  same always-on behaviour is named in the scope as the
  reskin risk Step 1 must answer, not assume away.
- Gaps handled openly in the scope: Unity verification
  and the Slice 1 director playtest gate stay open and
  are not Slice 14 work; electrical balance is not a
  slice, so any cost, condition, or output shape Step 1
  chooses is first-pass; the Slice 13 scene-proof gap
  (vertex-colour material, MeshRenderer wiring, marker
  occlusion) is carried as a Slice 15 candidate, but
  stated as three Windows observations inside checklist
  items 25, 26, and 27, not as a headless step, because a
  headless run cannot close an observation about a scene
  it cannot run; Patrol stays deferred with its route
  question intact; the free deploy primitive remains a
  headless-only rule, re-named, not enforced here.
- Verification, stated honestly: documentation-only step,
  so no new test and no demo scenario — the scope forbids
  claiming otherwise. Cited headless evidence is only the
  existing suite, re-run unchanged before writing.
- Tests: 1807 passed, 0 failed (unchanged,
  documentation-only step). Demo unchanged: Slice 12
  scenario save 43,974 chars, Slice 13 scenario save
  44,045 chars, all earlier demo saves as in the 0.60.0
  entry.

## 0.60.0 — 2026-10-30 — Slice 13 Step 4: composed demo + review, closing Slice 13

- Composed demo, decided in this step (rationale in
  DECISIONS.md): one Simulation proves the Step 1 warning, the
  Step 2 snapshot kinds, and the Step 3 power-state query
  together, not as separate proofs. Three paid squads through
  the picker (Vanguard, Recon, Heavy, 10 EmberOre each, ore
  left 0, all HoldGround) beside a threatened mill (HV direct
  from an Alternator), a powered mill (LV direct from a Dynamo,
  five-stone pocket), a live cable run (Dynamo -> cable ->
  cable, free end, energised), a threatened workstation (HV
  direct), a powered workstation (LV through Alternator ->
  transformer step-down), and a threatened pylon (LV direct),
  with one enemy staged in the pylon's range. Before any tick
  the HUD reads 1/2 mills, 1/2 workstations, 0/1 pylons, WARNED
  1/1/1, POWER 1 LIVE NETWORK, 0 LONE SOURCES; the query reads
  Mismatched for all three threatened cells, Powered for both
  powered consumers, Energised for the cable, and None for a
  Dynamo and for the transformer; the snapshot carries all
  three kinds, reading Marching, because picker squads deploy
  at the player and march to their rallies. Tick 1 destroys
  the threatened mill, leaves the enemy at 30 HP (the disabled
  pylon fires nothing), holds shields at 65 (regen 0 at the
  disabled bench), and drops the warnings to 0/1/1. Rewiring
  the workstation (Alternator -> Dynamo) and the pylon (Dynamo
  -> Alternator) clears the warnings to 0/0/0 and both queries
  to Powered, proving warning and tint agree; the rewired
  pylon then kills the same enemy in three ticks (30 -> 20 ->
  10 -> 0, exactly 10 per tick, so no squad fired), the powered
  mill gathers 5 in 5 ticks, and the rewired bench reads regen
  10. Save/load keeps the destroyed mill gone, re-derives all
  counts and queries (1/1 mills, 2/2 workstations, 1/1 pylons,
  WARNED 0/0/0, POWER 1 LIVE NETWORK, 1 LONE SOURCE — the lone
  source is the destroyed mill's orphaned Alternator, not a
  new source), keeps all three kinds Marching en route, and
  repeats in the loaded phase (powered 5 again, destroyed 0,
  stone 10, shields capped at 85, enemy stays dead).
- Review, decided in this step: new docs/slice-13-review.md,
  documentation only, checks Slice 13 against its scope and
  closes it as tested warning core and power-state query plus
  written adapters, not playtested, balanced, or
  Unity-verified. Slice 12 gaps 4 (no warning), 5 (tint and
  refresh), and 6 (shared marker) are recorded as closed in
  that sense, with the scene proof for the adapters carried
  as a new gap 4 (material, wiring, and occlusion
  observations, items 26, 27, and 25). Slice 14 is NOT scoped
  by this review; its ranked candidates are the deferred
  third source first, Patrol second, and any Slice 13 remainder
  third. No new playtest-gate checklist item, because the demo
  exercises no adapter binding the checklist does not already
  cover.
- Verification, stated honestly, not inflated: 99 composed-
  integration tests mirror the demo; the demo proves core
  rules and HUD numbers, not that a warning line, a marker
  shape, a tint, or a refresh reads or fires in the scene —
  those stay in checklist items 24, 25, 26, and 27, Windows-
  only. One test-file repair, not a code fix: the composed
  tests were first appended after the runner's top-level
  return, so they did not execute and the count still read
  1708; moving the summary and return to the end of the file
  made them run. No core rule, rating, cost, phase, delivery,
  or save-format change in this step, and no new save field.
- Tests: 1807 passed, 0 failed (was 1708; +99, no failures on
  the final runs). Demo unchanged except for the new scenario:
  Slice 11 scenario save 44,038 chars, Slice 12 scenario save
  43,974 chars, all earlier demo saves as in the 0.59.0 entry.
  New scenario save 44,045 chars.

## 0.59.0 — 2026-10-30 — Slice 13 Step 3: mesh tint and mesh refresh for power state

- Power-state query, decided in this step (rationale in
  DECISIONS.md): new WorldZone.PowerState, derived from
  placement and delivered voltage on every call, never
  saved — Powered for a consumer delivered its rated
  voltage (mill LV, workstation LV, pylon HV), Mismatched
  for a consumer delivered a non-None voltage that is not
  its rating, Energised for a Cable cell in a component
  carrying a non-None voltage, and None for everything
  else (unpowered, conflicted, sources, transformers,
  non-electrical blocks, air). Sources and transformers
  are deliberately None, not Energised: a lone source
  feeds nothing, and tinting it would claim a delivery
  that reaches no cell. Agreement with Step 1 is by
  construction, not a second rule: Mismatched here is
  exactly the predicate that makes HudState warn, so a
  machine the HUD warns about is the machine tinted
  Mismatched, and rewiring clears both together.
- Tint channel, decided in this step: vertex colours,
  not a per-machine material split. MeshData gains a
  Colors list parallel to Vertices (one RGBA per vertex,
  always the same count), MeshQuad gains a PowerState
  set by Mesher.GreedyMesh when a provider is passed,
  and that state joins the greedy merge key, so quads
  never merge across a tint boundary — two adjacent
  mills, one powered and one mismatched, keep separate
  top quads instead of one ambiguously coloured quad.
  With no provider every quad is None and merge
  behaviour is exactly as before, so every earlier
  quad-count test stands. New MeshBuilder.BuildFromChunk
  overload takes the provider (VoxelMeshRenderer passes
  WorldZone.PowerState). Tints, first-pass values, named
  against the marker palette: Powered green (0.25, 0.9,
  0.25), never cyan, so it cannot read as a squad
  marker; Mismatched red (1, 0.15, 0.15), never orange,
  so it cannot read as a rally marker; Energised yellow
  (1, 0.85, 0.1), brighter and greener than orange, on
  cable cells, never as a floating marker; None white,
  which leaves a material's own colour unchanged.
- Refresh event set, decided in this step, named, not
  "always": VoxelMeshRenderer.RefreshMesh now builds
  the tinted mesh and assigns mesh.colors, and
  FpsInputAdapter gains a MeshRenderer field (wire it
  to the same renderer GameManager links) and refreshes
  only on a successful gather, a successful build key
  (E, Q, T, Y, U, I, B, N, P), a mill destruction on a
  tick (DestroyedMills rose across Simulation.Tick),
  load, or Start — never every frame, and never on a
  failed key, a reload, a shot, or a squad deploy, none
  of which changes a voxel. This event set is complete
  for chunk and wiring changes, because power state is
  placement-derived: the only tick-time voxel change is
  a destruction, and every placement change arrives
  through a key, a gather, or a load. Honest material
  limit, not hidden: mesh.colors only shows if the
  chunk material multiplies vertex colours, so the
  scene must wire a vertex-colour material, and
  playtest-gate item 26 is the observation that
  settles which material the scene actually uses.
- Verification, stated honestly, not inflated: the
  query, the colour channel, the merge separation, the
  tint/warning agreement, and the save/load re-derivation
  are headless-tested (44 new tests). The tint itself
  and the refresh wiring are adapter-only Unity code,
  so their verification is the written adapter code
  plus playtest-gate checklist items 26 (tint) and 27
  (refresh), Windows-only — the tests do not prove a
  tint reads in the scene, and no demo scenario is
  added, because a printed power state would not prove
  a visible tint. No delivery, precedence, conflict,
  rating, cost, or phase rule changes in this step, and
  no new save field: tint and refresh are derived and
  rendered, never saved.
- Tests: 1708 passed, 0 failed (was 1664; +44, no
  failures on the final runs). Demo unchanged: Slice 11
  scenario save 44,038 chars, Slice 12 scenario save
  43,974 chars, all earlier demo saves as in the
  0.58.0 entry.

## 0.58.0 — 2026-10-30 — Slice 13 Step 2: scene-marker distinction for the squad kinds

- Marker distinction, decided in this step (rationale in
  DECISIONS.md): shape first, colour second. Vanguard keeps
  the recognised cyan cube, 0.6 units; Recon renders as a
  magenta capsule, 0.5 wide x 0.8 tall; Heavy renders as a
  light-grey cylinder, 0.8 wide x 0.7 tall. All three shapes
  differ, so the kinds stay apart for a player who cannot
  tell the colours apart, and no kind colour reads as
  orange, so no squad marker is mistaken for an orange
  rally or pending marker. Rally (orange flat cube) and
  pending (orange sphere) markers are unchanged.
- Adapter only, by the scope's own terms: SquadMarkerDisplay
  already read SquadSnapshot.Kind and ignored it; it now
  renders that Kind, with no core rule change — no kind
  shape, cost, cap, movement, targeting, or doctrine rule
  moves, and OrbitalSquadDisplay keeps reading kind from
  the same snapshot, so text and markers agree word for
  word. The pooled budget is preserved, not grown: squad
  markers are 3 slots, each a parent holding one child
  primitive for its current kind, swapped only when a slot
  is reused by a later deployment of a different kind
  (a deployed squad's kind never changes), never every
  frame; an unknown kind falls back to the Vanguard cube
  rather than disappearing. Playtest-gate checklist item
  25 (Windows-only) added; it supersedes item 20's
  statement that a Heavy marker is the same cyan cube.
- Verification, stated honestly, not inflated: this step
  adds no headless test and no demo scenario, because no
  test or printed kind can prove a colour or a shape reads
  in the scene. The cited headless evidence is only the
  existing snapshot Kind coverage (deploy, picker, save,
  and view tests), and the marker proof is item 25 in
  Unity on Windows. Occlusion stays honest: markers do
  not show through walls.
- Tests: 1664 passed, 0 failed (unchanged, adapter-only
  step — Unity code is not compiled headlessly). Demo
  unchanged: Slice 11 scenario save 44,038 chars, Slice
  12 scenario save 43,974 chars, all earlier demo saves
  as in the 0.57.0 entry.

## 0.57.0 — 2026-10-30 — Slice 13 Step 1: pre-destruction / pre-disablement warning

- Warning readout, decided in this step (rationale in
  DECISIONS.md): HudState gains three derived counts,
  never saved — WarnedMills, WarnedWorkstations,
  WarnedPylons — counted in the same snapshot scan as
  the powered/total counts, from the three existing
  mismatch predicates (IsMismatched, IsWorkstation-
  Mismatched, IsPylonMismatched). The counts are split
  per outcome, not folded into one number, because a
  mill about to be destroyed and a workstation about
  to be disabled are not the same loss, and one number
  would hide which machine the player built. A powered,
  unpowered, or conflicted (None) machine never warns:
  it loses nothing on the next tick, so warning for it
  would cry wolf every snapshot, and None is neither
  powered nor mismatched by the existing predicates.
  Rewiring clears a warning on the next snapshot; a
  destroyed mill's warning disappears because the mill
  is gone, while a disabled workstation or pylon keeps
  warning while it stays mismatched, because it
  survives to be rewired.
- Timing, stated so the warning is not a death notice
  in disguise: the readout is a snapshot taken between
  placement and tick, proved by tests that snapshot
  before Advance, then advance, then snapshot again.
  No delivery, precedence, conflict, rating, cost, or
  phase rule moves in this step, and no save-format
  change: warnings re-derive after load, and a warned
  mill still dies on the next tick if it stays wired —
  the warning postpones nothing, which is what makes
  it a readout and not a grace period smuggled in as
  a display step. Cable-fed mismatches warn too,
  because the readout follows DeliveredVoltage, not
  just face-adjacent sources; non-consumer cells
  (sources, cables, transformers) never warn.
- Adapter, decided in this step: HudDisplay gains a
  fifth Text field, WarningText, with its own line,
  not folded into an existing POWER line — "WARNING:
  n MILLS WILL BE DESTROYED, n WORKSTATIONS WILL BE
  DISABLED, n PYLONS WILL BE DISABLED" — and the line
  is empty, not a zero warning, while no machine
  warns, so a wired HUD shows nothing until there is
  danger. Playtest-gate checklist item 24 (Windows-
  only) added. Honesty limit, recorded in DECISIONS.md
  and in that item, not hidden: in Unity a tick runs
  every frame, so a mill wired to HV is expected to
  show its warning for at most one frame before
  destruction, while workstation and pylon warnings
  persist until rewiring — the mill warning is
  headless-proved but is NOT claimed usable in live
  play until tick pacing is decided, and that pacing
  question is carried, not answered, here. The line
  is also not a scene tint and not a mesh refresh: a
  warned machine is warned about in the HUD, not
  thereby visible in the scene — Gap 5 stays open
  after this step, by decision, for Step 3.
- Tests: 1664 passed, 0 failed (was 1622; +42, no
  failures on the final runs). Demo unchanged: Slice
  11 scenario save 44,038 chars, Slice 12 scenario
  save 43,974 chars, all earlier demo saves as in the
  0.56.0 entry.

## 0.56.0 — 2026-10-30 — Slice 13 scope: adapter visibility and fairness, warning first, then marker distinction, then mesh tint and refresh

- Scope, documentation only, no code changed, by design:
  new docs/slice-13-scope.md — Slice 13 theme adapter
  visibility and fairness, in order: (1) a
  pre-destruction / pre-disablement warning, decided in
  that step (all three mismatch outcomes as starting
  hypothesis, because all three already have a core
  mismatch predicate: mill destroyed, workstation
  disabled, pylon disabled; a readout only, never a
  delay, grace period, or salvage rule); (2) a
  scene-marker distinction for the squad kinds,
  decided in that step (adapter-only: SquadSnapshot
  already carries Kind, SquadMarkerDisplay already
  reads the snapshot but ignores Kind, rendering one
  cyan cube for all three kinds; rally orange flat
  and pending orange sphere stay unchanged unless the
  step justifies otherwise); (3) mesh tint and mesh
  refresh for power state, decided in that step (a
  core per-cell power-state query, headless-testable,
  plus adapter tint and refresh wiring, checklist
  only: MeshData carries no colour channel today, and
  RefreshMesh is called only at Start and on load,
  never after a build key or destruction tick);
  (4) Slice 13 demo scenario + documentation-only
  review. Scope grounded in current code facts
  verified this run: BlockType appended through
  ShockPylon = 12; HudDisplay renders four POWER
  lines (mills, workstations, live networks / lone
  sources, pylons), so a warning needs a fifth Text
  field or a stated fold-in; builder keys run T / Y /
  U / I / B / N / P with picker Alpha1-6, so no step
  may assume a free binding; playtest-gate checklist
  items run through 23 (22 live line, 23 P key).
  Ranked Slice 14 candidates recorded, not promised:
  deferred third source first, Patrol second, any
  Slice 13 remainder third.
- Tests: 1622 passed, 0 failed (unchanged,
  documentation-only step). Demo unchanged: Slice 11
  scenario save 44,038 chars, Slice 12 scenario save
  43,974 chars, all earlier demo saves as in the
  0.55.0 entry.

## 0.55.0 — 2026-10-30 — Slice 12 Step 3: composed demo + review, closing Slice 12

- Demo, decided in this step (rationale in
  DECISIONS.md): one composed Slice 12 scenario
  appended to demo/Forgeborne.Demo, after the Slice
  11 scenario — not three separate proofs. On the
  y=20 plane: a live network (Dynamo -> cable ->
  cable, free end, feeding nothing), an LV mill fed
  directly by a Dynamo with a five-stone pocket, and
  a lone Alternator with nothing adjacent to it. On
  the y=5 plane: a Shock Pylon fed HV directly by an
  Alternator, and a powered workstation fed LV
  through an Alternator -> transformer step-down.
  The player parks far away for the pylon's three
  firing ticks (so no bench regen confounds them)
  and then walks to the bench to read its regen,
  because regen is player-centred while pylon fire
  is machine-centred and one position cannot prove
  both — stated openly, as in Slice 10. The layout
  is staged directly with TrySet, as the Step 1-2
  tests stage theirs; aim placement of each block
  was proved in its own step's tests.
- Demo numbers, in full: before any tick, voltages
  mill LV, pylon HV, workstation LV, and the HUD
  reads 1/1 MILLS POWERED, 1/1 WORKSTATIONS
  POWERED, 1/1 PYLONS POWERED, and POWER: 1 LIVE
  NETWORKS, 1 LONE SOURCES — the live network and
  the lone source appear in no older count, and the
  pylon's own Alternator is counted as neither,
  because the pylon count already shows that
  delivery. An enemy staged 5 from the pylon steps
  player-ward and is fired on at ranges 4, 3, then 2,
  dying on tick 3 (30 -> 20 -> 10 -> 0); shields stay
  65 through the pylon ticks, then the bench walk
  reads regen 10 and, after 5 ticks total, the mill
  has gathered 5 (pocket exhausted), nothing is
  destroyed, stone is 5, and shields are capped at
  85. Save/load round-trip: the live cable, the lone
  Alternator, the mill, the pylon, and the
  workstation all load; the HUD re-derives to the
  same counts (derived, not saved); session counters
  reset; stone stays 5; the player is back at the
  bench reading regen 10. Loaded phase (20 damage,
  pocket re-seeded): 5 ticks gather 5 again, nothing
  destroyed, stone 10 (5 + 5, no refund on load),
  shields capped at 85 again, enemies stay 0.
- Review: new docs/slice-12-review.md, documentation
  only, checking the slice against its scope with
  the verdict stated honestly (tested display and
  widening core plus written adapters, not
  playtested, balanced, or Unity-verified), the
  staging facts pinned so they are not over-claimed
  (TrySet staging; parked-player bench walk; the
  pylon's first demo shot is at range 4, not 5,
  because enemies move before the pylon phase, so
  the stationary range-5 boundary rests on Step 2's
  tests, not on this demo), the Slice 11 live-network
  invisibility gap recorded as closed by Step 1, and
  the remaining gaps carried in priority order.
  Slice 13 is NOT scoped by this review. No new
  playtest-gate checklist item: the demo exercises
  no adapter binding items 22 and 23 do not already
  cover. No core system rides along with this step.
- Tests: 1622 passed, 0 failed (was 1575; +47
  composed-integration tests mirroring the demo, no
  failures on the final runs). Demo unchanged except
  the new scenario: Slice 11 save 44,038 chars, all
  earlier demo saves as in the 0.54.0 entry. New
  scenario save 43,974 chars.

## 0.54.0 — 2026-10-30 — Slice 12 Step 2: third consumer, HV-rated Shock Pylon

- Consumer, decided in this step (rationale in
  DECISIONS.md): new BlockType.ShockPylon = 12,
  appended only, rated HV (VoltageRules.PylonRated,
  its own constant, not borrowed from any other
  consumer) — the first consumer that actually wants
  HV, so the Alternator gains a safe direct use and
  HV stops being a voltage that only ever punishes
  LV consumers. A powered pylon fires once per tick
  at the nearest living enemy within Manhattan range
  5, for 10 damage, through the shared AttackEnemy
  path, in a new pylon phase last in the tick; two
  pylons on one enemy stack to 20 in a tick, and
  ties resolve to the first enemy in list order.
  Its effect is combat only: it gathers nothing and
  regens nothing, which is what makes it a third
  consumer rather than a re-rated mill or
  workstation.
- Mismatch, decided in this step, following the
  workstation rather than the mill: a pylon
  delivered LV (the wrong level on this ladder) is
  disabled for that tick, not destroyed, and fires
  again as soon as it is rewired to HV; no voltage
  (None) idles, and a conflicted (None) pylon is
  neither powered nor mismatched. RunMismatched-
  Destruction is untouched: it checks IsMismatched,
  which is mill-only by construction, so no pylon
  is ever destroyed by a wiring mistake.
- Delivery, proved both ways like the Alternator
  was: direct Alternator HV powers, direct Dynamo
  LV mismatches, step-up Dynamo -> transformer ->
  pylon powers, step-down Alternator -> transformer
  -> pylon mismatches, cable-fed HV powers and
  cable-fed LV mismatches. No delivery, precedence,
  conflict, rating, cost, or phase rule for any
  other consumer moves in this step, and no
  save-format change: a pylon is a chunk byte, its
  power re-derives after load, and a loaded pylon
  fires on its first tick.
- Cost, decided in this step: 5 Stone + 10 EmberOre,
  a new combination — half a workstation's stone
  frame, and exactly a squad's deploy price in ore,
  so a pylon competes with a squad for ore (a static
  defence bought instead of a mobile one), while
  competing with no machine for Wood and with no
  source for stone at its scale. Check-before-spend,
  like every other machine.
- Display and placement, decided in this step:
  HudState gains PoweredPylons / TotalPylons, and
  HudDisplay gains an optional PylonPowerText line
  ("POWER: x/y PYLONS POWERED"), unwired = no change.
  IsConsumer now includes the pylon, so the Slice 12
  Step 1 anti-double-count rule extends to it: a
  network feeding a pylon is shown by the pylon
  count, not counted again as a live network, and a
  lone pylon counts as no live network and no lone
  source, because it is a consumer, not a source.
  Simulation.BuildShockPylonAhead places by aim;
  adapter key P, suppressed while the rally picker
  is open, like the other builders; playtest-gate
  checklist item 23 (Windows-only) added. No
  per-block mesh colour exists for any machine, so
  the pylon shares the generic scene rendering of
  the other machines — that is a stated rendering
  fact, not a pylon-specific gap.
- Behaviour proofs: direct HV powered / direct LV
  mismatched / lone idle / non-pylon cells neither;
  step-up powers, step-down mismatches, cable HV
  powers, cable LV mismatches, conflicting direct
  sources deliver None; powered pylon 10 per tick,
  30-HP enemy dead and removed in 3 ticks; range 5
  fires, range 6 does not; nearest only, ties to
  first in list, two pylons stack 20 in a tick;
  mismatched pylon fires nothing, survives, and
  fires on the tick after rewiring to HV; unpowered
  pylon idles and survives; cost spends exactly,
  one ore short refuses with nothing spent; aimed
  build places by the Slice 10 Step 2 aim; pylon
  not gatherable; HUD 1/2 powered for one fed and
  one lone pylon; live counts (0,0) for a fed
  network and for a lone pylon, (1,0) for one fed
  plus one free network; save/load chunk equal,
  powered after load, firing on the first tick.
- Tests: 1575 passed, 0 failed (was 1519; +56, no
  failures on the final runs). Demo unchanged:
  Slice 11 save 44,038 chars, all earlier demo
  saves as in the 0.53.0 entry.

## 0.53.0 — 2026-10-30 — Slice 12 Step 1: live-network and lone-source display

- Display, decided in this step (rationale in
  DECISIONS.md): WorldZone.LivePowerCounts, derived
  on every snapshot, never saved — LiveNetworks
  counts live electrical structures that no
  consumer count already shows, LoneSources counts
  lone Dynamo / Alternator cells. Live means
  carrying a voltage that is not None, never merely
  placed; the count is of networks, not cells (a
  ten-cable LV run is one network); a network
  feeding a consumer is not counted again, so one
  delivery is never two live things; LV and HV
  count alike, with no voltage split. No delivery,
  precedence, conflict, rating, cost, or phase rule
  moves in this step, and no save-format change:
  counts re-derive after load.
- Counting path, decided in this step: the existing
  CableComponent / CableNetworkVoltage walk is
  reused through a new public WorldZone method, not
  re-walked in the HUD; a standalone transformer
  group (connected transformers with no cable
  neighbour, one network per group, not per stage)
  covers live outputs feeding nothing. Stated
  approximations, not hidden: "feeds" is direct
  adjacency or one transformer hop, so a cable,
  transformer, transformer, mill chain still reads
  as a live network; a transformer touching a cable
  is never standalone, so a live output into a
  dead (conflicted) component shows nothing.
- Adapter, decided in this step: HudDisplay gains
  a third Text field, LivePowerText, rendering its
  own line ("POWER: n LIVE NETWORKS, m LONE
  SOURCES"), not folded into an existing line,
  because the two counts answer a different player
  question than powered/total consumers; unwired,
  nothing changes. Playtest-gate checklist item 22
  (Windows-only) added, and it supersedes item
  18's statement that a lone Alternator shows
  nothing on the HUD — that was the decided
  behaviour before this display existed. A HUD
  line is not a mesh tint, not a mesh refresh, and
  not a warning: an HV network counted here is
  live, not safe, and gaps 4 and 5 stay open.
- Behaviour proofs: empty world 0/0; lone Dynamo
  and lone Alternator each (0,1); adjacent Dynamos
  (0,2); Dynamo beside a wall still lone; live
  cable network (1,0); ten-cable LV run still one
  network; sourceless cable (0,0); mixed-voltage
  network dead (0,0); HV network (1,0), no split;
  network feeding a mill, a workstation, or a mill
  through a transformer (either direction) reads
  (0,0) while the consumer count still shows
  powered; one fed + one free network reads (1,0);
  standalone transformer (1,0); transformer chain
  one network; unpowered transformer and sourceless
  ring (0,0); transformer feeding a mill (0,0);
  live output into a mixed component (0,0), proved
  by a probe mill reading None; removing the source
  drops the count on the next snapshot; save/load
  re-derives (1,1); HudState carries both counts.
- Tests: 1519 passed, 0 failed (was 1485; +34, no
  failures on the final runs). Demo unchanged:
  Slice 11 save 44,038 chars, all earlier demo
  saves as in the 0.52.0 entry.

## 0.52.0 — 2026-10-30 — Slice 12 scope: electrical widening, display first, then one widening

- Scoping, decided in this step (rationale in
  DECISIONS.md): Slice 12 is electrical widening,
  taken from the Slice 11 review's ranked
  candidates in their order, not re-ranked on this
  run's preference. Documentation only, no code
  changed, by design.
- Order, decided in this step: (1) a display for
  live networks feeding no consumer and for lone
  sources — the Slice 11 review's gap 6, which no
  earlier slice closed, because the HUD counts
  only consumers and a live network with no
  consumer, and a lone Dynamo or Alternator, read
  nothing; (2) exactly one further electrical
  widening, decided in its step, with a third
  consumer as starting hypothesis (both existing
  consumers are LV-rated, so HV has no safe direct
  use) and a third source as the deferred
  alternative; (3) Slice 12 demo scenario +
  documentation-only review, closing Slice 12.
- Gaps, handled openly: Unity verification and the
  Slice 1 director playtest gate stay open and are
  not Slice 12 work; warning, mesh tint, and mesh
  refresh stay named gaps, not fixed by a HUD
  count; the three-kind shared scene marker stays
  a Slice 13 visibility question, not a Slice 12
  extra; Patrol stays deferred with its route
  question intact. Ranked Slice 13 candidates
  recorded in the scope, not promised: adapter
  visibility and fairness first, the deferred
  electrical widening second, Patrol third.
- Tests: 1485 passed, 0 failed (unchanged,
  documentation-only step). Demo unchanged: Slice
  1 save 43,881 chars, Slice 2 43,921, Slice 3
  Step 1 44,016, Slice 3 Step 3 44,103, Slice 4
  44,026, Slice 5 43,974, Slice 6 44,024, Slice 7
  44,013, Slice 8 43,974, Slice 9 43,975, Slice 10
  43,974, Slice 11 44,038.

## 0.51.0 — 2026-10-30 — Slice 11 Step 3: demo scenario + documentation-only review, closing Slice 11

- Composed demo, decided in this step (rationale in
  DECISIONS.md): one Simulation with a Heavy Defend
  Core squad beside a Vanguard HoldGround squad,
  both through the paid picker at 10 EmberOre each.
  The Vanguard reaches its rally (20,5,8) on tick 4
  and kills its lane enemy at exactly range 3; the
  Heavy marches 24 ticks to the core (28,5,28),
  passing through its dummy rally cell (20,5,20)
  at tick 8 reading Marching — not holding there —
  and reading Guarding only at its post. A core
  attacker staged at (26,5,28) after the guard is
  posted reads Hunting, steps to (27,5,28), is
  fired on for 16 at range 2 while the guard takes
  one 10-damage adjacent hit (100 -> 90 HP), and
  dies on the second tick, after which the guard
  reads Guarding again. A third picker squad,
  weakened to 10 HP with an enemy adjacent, fires
  once for 10 and dies in the same tick, proving
  removal in play order, not just in a unit test.
- Save/load round-trip, proved, not changed: the
  Heavy (Defend Core, at the core, 90 HP) and the
  Vanguard (HoldGround, at its rally, 90 HP) load
  with kind and doctrine re-derived from the saved
  ints, the dead squad stays dead (2 squads loaded,
  not 3, cap freed by the death), the surviving
  enemy keeps its 20 HP, ore is 10, and one loaded
  tick still reads Guarding / Holding — with no new
  save field. Save JSON 44,038 chars, a composition
  fingerprint for this scenario only.
- Review, documentation only: new
  docs/slice-11-review.md closes Slice 11 as tested
  squad widening — third kind, third doctrine
  proved against all three kinds, composed demo
  through save/load — and states honestly that it
  is still not playtested, balanced, or
  Unity-verified. It records the B-key checklist gap
  as closed by Step 1 (gate item 19), carries the
  shared marker as a grown gap (three kinds, not
  two), keeps Patrol deferred with its route
  question intact, and does NOT scope Slice 12.
- Tests: 1485 passed, 0 failed (was 1447; +38
  composed-integration tests mirroring the demo
  step for step, no failures on the final run, and
  the demo ran correctly on its first execution).
  Earlier demo scenarios unchanged: Slice 1 save
  43,881 chars, Slice 2 43,921, Slice 3 Step 1
  44,016, Slice 3 Step 3 44,103, Slice 4 44,026,
  Slice 5 43,974, Slice 6 44,024, Slice 7 44,013,
  Slice 8 43,974, Slice 9 43,975, Slice 10 43,974.

## 0.50.0 — 2026-10-30 — Slice 11 Step 2: third doctrine — Defend Core, guard the objective core

- Third doctrine, decided in this step (rationale in
  DECISIONS.md): Defend Core, appended as
  TacticalDoctrine = 2 only, chosen from the scope's
  deferred pair (Defend Core / Patrol) over Patrol,
  because the core objective already exists
  (28,5,28, 100 HP) and no squad today orders itself
  to it, while Patrol still needs a route definition
  the core does not yet have. Patrol is deferred,
  not rejected. HoldGround and Advance behaviour
  for zones and saves that do not use Defend Core
  is unchanged, proved by a one-tick contrast test:
  same rally, no enemies — HoldGround walks west
  to its rally, Advance stands still, Defend Core
  walks east to the core.
- Behaviour, decided in this step: the guard post is
  the core cell itself, not the rally the picker
  still requires to deploy — a Defend Core squad
  never marches to its rally. It marches one voxel
  per tick toward the core through the shared step
  rule (same axis choice, tie-break, and solid /
  enemy-occupied refusals, so a blocked post is held
  short of, not stepped through). A living enemy
  within 8 Manhattan of the core is an attacker the
  guard chases, one voxel per tick, through a single
  decided move per tick (a blocked chase step does
  not fall through to the core in the same tick);
  an enemy 9 or more from the core is ignored,
  however near it stands to the guard, proved at
  the 8 / 9 boundary and by a far enemy the guard
  leaves at its post while an Advance squad hunts
  it. With no attacker the guard returns to the
  core, proved by a chase, a kill in range, and a
  return step on the next ticks. The 8 radius is a
  first-pass value, not a balance claim: larger
  than every kind's engagement range (Vanguard 3,
  Recon 5, Heavy 2), so a guard can intercept
  before an attacker reaches the core, and far
  smaller than the spawn-to-core distance (24), so
  the perimeter is local to the core.
- Engagement, decided in this step: Defend Core
  fires whenever an enemy is in its kind's
  engagement range, Advance's fire rule, not
  HoldGround's hold-only rule. A chasing or
  returning guard therefore shoots what it passes
  in range — proved by a returning guard that fires
  at an enemy 21 from the core while still stepping
  core-ward — but it does not chase that target:
  movement stays core-bound, so firing at a
  non-attacker is not hunting it. At its post an
  adjacent attacker cannot be stepped onto, so the
  guard stays on the core cell and fires there.
- Kinds served, decided in this step: the doctrine
  is picked independently of kind and is proved
  against all three kinds, not just the new one —
  Vanguard (chase, kill, return; range-3 fire),
  Heavy (at-post fire for 16; range-3 silence),
  and Recon (chase from radius 8, silence beyond
  range 5). Price stays 10 EmberOre and the cap
  stays 3 live squads, through the paid picker
  path, with no new price invented.
- Display and adapter binding, decided in this
  step: a Defend Core squad reads Guarding at its
  post with no attacker, Hunting while chasing a
  core attacker (with that attacker's cell), and
  Marching on the return march — an enemy far from
  the core reads Guarding, not Hunting, at the
  post, so display and movement agree about the
  chase radius. Alpha6 selects Defend Core while
  picking, after Alpha1 HoldGround and Alpha2
  Advance — confirmed free while picking, since
  the picker keys were 1 / 2 / 3 / 4 / 5 plus
  Return / Esc / O and no builder key fires while
  picking. Playtest-gate checklist item 21
  (Defend Core key, Windows-only) and
  orbital-command checklist item 12 are added.
- Shared movement fix, found by this step's tests,
  not a behaviour change for existing doctrines:
  the shared squad step rule now returns false
  when already at its target, instead of reporting
  a phantom step to its own cell. HoldGround
  already guarded its at-rally case before calling
  the rule and Advance cannot share a cell with its
  prey, so no existing doctrine changes behaviour;
  the fix is what lets a guard at its post report
  no move. Two new tests first failed on the old
  return value (radius-9 ignored and dead-attacker
  ignored both reported a move at post) and pass
  on the fixed rule, with no test changed for it.
- Save rule, proved, not changed: doctrine is
  stored as an int and the loader already rejects
  undefined doctrines, so no new save field is
  invented. A Defend Core Heavy at the core and a
  Defend Core Vanguard marching both load, and the
  loaded Vanguard marches toward the core, not its
  rally.
- Tests: 1447 passed, 0 failed (was 1370; +77, no
  failures on the final run). Demo unchanged: Slice
  10 Step 3 scenario save 43,974 chars.

## 0.49.0 — 2026-10-30 — Slice 11 Step 1: third squad kind — Heavy, close-range anchor

- Third kind, decided in this step (rationale in
  DECISIONS.md): Heavy, appended as SquadKind = 2
  only, with shape 2 members, 50 HP each, 8 damage
  each, engagement range 2 — full health 100, full
  damage 16 per tick. It is the anchor the other
  kinds cannot be: a 95-damage hit kills a Vanguard
  (90 HP) outright and leaves a Heavy at 5 HP with
  1 member, but a Heavy is silent at range 3 where
  a Vanguard fires for 30, and deals barely half a
  Vanguard's damage where it does fire, so it is a
  trade, not a replacement. Flyer was not chosen:
  it would need a movement rule with targeting and
  occupancy consequences, which would make the kind
  untestable in isolation — the scope's own warning,
  carried from Slice 7. Movement stays the shared
  one-voxel-per-tick rule for all three kinds.
- Shape, price, and cap, decided in this step:
  Vanguard's numbers (3 x 30, 10 damage, range 3)
  and Recon's numbers (2 x 20, 6 damage, range 5)
  are unchanged; Heavy reads its shape from
  `SquadShapes.For`, like the other kinds, not from
  a branch in every rule. Price stays 10 EmberOre
  and the cap stays 3 live squads in total, not per
  kind, so the picker's COST 10 / MAX 3 line stays
  true for all three kinds. No new doctrine rides
  along: Heavy deploys under HoldGround and Advance
  exactly as the existing kinds do, and a third
  doctrine is Step 2 work.
- Adapter binding and display, decided in this
  step: Alpha5 selects Heavy while picking, after
  Alpha3 Vanguard and Alpha4 Recon — confirmed
  free while picking, since no builder key fires
  while the picker is open and the picker keys were
  1 / 2 / 3 / 4 plus Return / Esc / O. Playtest-gate
  checklist item 20 (Heavy key, Windows-only) and
  orbital-command checklist item 11 are added. The
  headless display already prints kind, doctrine,
  members, health, damage, range, and activity, so
  no new display line is added. Scene marker,
  decided in this step (Slice 10 review gap 8):
  Heavy keeps the shared cyan cube marker, stated
  openly in SquadMarkerDisplay.cs, not inherited by
  accident — a per-kind marker distinction is a
  display system for Slice 12 scoping to decide as
  one visibility question, and until then kind is
  read from the text display line, not the marker.
- Gate-doc gap closed, as assigned by the 0.48.0
  scope: Slice 10 review gap 7 is closed by this
  step, because Step 1 touches the gate doc for its
  own checklist item — playtest-gate checklist item
  19 (workstation B key, added 0.45.0) is added
  alongside item 20, named here so the repair is a
  stated part of this step and not a quiet extra.
- Save rule, proved, not changed: kind is stored as
  an int and the loader already rejects undefined
  kinds, so no new save field is invented. A 999-HP
  Heavy loads clamped to 100, a 999-HP Vanguard
  still loads at 90, and a mixed Vanguard + Recon +
  Heavy save round-trips all three kinds with their
  own health and member counts.
- Tests: 1370 passed, 0 failed (was 1300; +70, no
  failures on the final run). Demo unchanged: Slice
  10 Step 3 scenario save 43,974 chars.
- Test fix during the run, not a code fix: the
  Vanguard range-3 comparison first asserted
  Enemies[0].Health after a killing 30-damage hit,
  but Advance removes dead enemies from the list,
  so the assertion is now Enemies.Count == 0; no
  core code was changed for it.

## 0.48.0 — 2026-10-30 — Slice 11 scope: squad widening — third kind, then one deferred doctrine, then demo + review

- Scoping Slice 11 as its own decision step, decided in
  this step (rationale in DECISIONS.md): Slice 11 is
  squad widening, in order — (1) exactly one third
  squad kind, decided in that step from the pool the
  code already names (heavy, flyer, Recon already
  spent), proved as a distinct tactical choice with
  its own shape and not as a Vanguard or Recon
  reskin, with Vanguard's and Recon's numbers
  unchanged and the 10-ore price and 3-squad cap held
  as the starting hypothesis; (2) exactly one deferred
  doctrine, decided in that step from Defend Core and
  Patrol, with Defend Core as the starting hypothesis
  because the core objective already exists and no
  squad today orders itself to it, proved against more
  than one kind; (3) demo scenario + documentation-
  only review composing the kind and the doctrine in
  one Simulation. Kind first, then doctrine, so the
  doctrine step inherits a three-kind field to prove
  against instead of a two-kind field it must later
  re-prove.
- Scope grounded in current code facts verified this
  run: SquadKind has Vanguard (3 members, 30 HP each,
  10 damage each, range 3) and Recon (2 members, 20 HP
  each, 6 damage each, range 5); TacticalDoctrine has
  HoldGround (march one voxel per tick to a rally,
  fire only once holding) and Advance (hunt the
  nearest living enemy one voxel per tick, fire
  whenever in range, rally ignored after deployment);
  the picker uses Alpha1 / Alpha2 for doctrines and
  Alpha3 / Alpha4 for kinds, so both Step 1 and Step 2
  must name non-colliding bindings; saves already
  store kind and doctrine as ints with undefined
  values rejected on load, so new enum values are
  appended only; the headless display already prints
  kind, doctrine, members, health, damage, range, and
  activity.
- Gap handling, decided in this step: the Slice 10
  review's gap 7 (the workstation B key has no
  playtest-gate checklist item) is assigned to Step 1,
  because Step 1 adds a checklist item for its own
  kind key and therefore touches the gate doc for its
  own reason — the 0.47.0 rule for closing that gap.
  The shared scene marker (gap 8) is a stated Step 1
  decision, not an inherited silence, since a third
  kind grows it. Ranked Slice 12 candidates recorded,
  not promised: further electrical widening first,
  adapter visibility and fairness (warning, tint /
  refresh, marker distinction) second.
- No core system rides along with this step: no new
  kind, doctrine, block type, voltage level, consumer,
  source, price, cap, or balance pass, no warning,
  tint, refresh, camera, or save-format change.
- Tests: 1300 passed, 0 failed (unchanged,
  documentation-only step). Demo unchanged: Slice 10
  Step 3 scenario save 43,974 chars.

## 0.47.0 — 2026-10-30 — Slice 10 Step 3: demo scenario + review, closing Slice 10

- Composed Slice 10 demo, decided in this step (rationale
  in DECISIONS.md): one Simulation proving the second
  consumer and the second source together, not separately.
  Three mills on the y=20 plane — a cable step-down
  Alternator mill (Alternator -> cable -> transformer ->
  cable -> mill, LV, every tick), a direct-HV Alternator
  mill destroyed on tick 1, and a kinetic mill with no
  power — plus three workstations on the y=5 plane beside
  the player's spawn — a step-down Alternator workstation
  (LV, base + 5 regen), an unpowered workstation (base
  regen), and a direct-HV workstation (disabled, regen 0,
  surviving). Voltages before any tick: mill LV / HV /
  None, workstation LV / None / HV; HUD 1/3 mills powered
  and 1/3 workstations powered, so neither HV consumer is
  ever displayed as powered. Tick 1: HV mill destroyed,
  destroyed 1, powered gathered 1, kinetic 0, stone 1, HV
  pocket stone untouched, HV Alternator and HV workstation
  surviving, shields 65 -> 75. The player then walks
  between the benches with no tick passing — regen 5 at
  the unpowered bench, 0 at the HV bench, 10 again at the
  powered bench — because regen is player-centred and one
  position cannot prove all three benches at once; the
  walk is stated as a measurement procedure, not a
  gameplay claim. After 5 ticks: powered gathered 5,
  kinetic 1, destroyed 1, stone 6 (5 + 1, no refund),
  shields capped at 85. Save/load keeps the surviving
  blocks and the destroyed mill gone, re-derives mill LV,
  powered workstation LV, and HV workstation HV with no
  stored voltage flag, resets the session counters, keeps
  stone 6, and returns the player to the powered bench
  (16,5,16) with effective regen 10; the loaded phase
  (20 damage, powered pocket re-seeded as fresh seeding)
  gathers 5 + 1 again, destroys nothing new, reaches
  stone 12, and caps shields at 85. Demo and
  composed-integration tests mirror each other; the
  layout is staged with TrySet, as the Step 1-2 tests
  stage theirs, because aim placement of each block type
  was proved in its own step's tests.
- Review, decided in this step: new
  `docs/slice-10-review.md` closes Slice 10 as tested
  electrical widening (one second consumer, one distinct
  source) plus written adapters (B / N keys, powered-mill
  and powered-workstation HUD counts) — not as playtested,
  balanced, or Unity-verified. No new playtest-gate
  checklist item is added for the demo itself, because it
  exercises no adapter binding the checklist does not
  already cover, except for the B key, which has no
  checklist item at all: that gap, observed in Step 2,
  is carried in the review as gap 7 rather than silently
  closed by a review step. Slice 11 is not scoped by this
  review; its ranked candidates, carried unchanged from
  the 0.44.0 scope, are a third squad type and the
  deferred doctrines first, and further electrical widening
  second.
- No core system rides along with this step: no new block
  type, no new voltage level, no new consumer, source,
  doctrine, or squad type, no balance pass, no warning,
  tint, or refresh, and no save-format change. Earlier
  demo scenarios are unchanged; the new scenario's save
  is 43,974 chars, the same length as the Slice 5 and
  Slice 8 saves by coincidence of format, not the same
  save.
- Tests: 1300 passed, 0 failed (was 1239; +61, no
  failures on the first full run). Demo: Slice 9 Step 4
  scenario save 43,975 chars unchanged; new Slice 10 Step
  3 scenario save 43,974 chars, powered 5 + loaded 5,
  kinetic 1 + loaded 1, destroyed 1.

## 0.46.0 — 2026-10-30 — Slice 10 Step 2: Alternator, second power source

- Alternator, decided in this step (rationale in
  DECISIONS.md): a native HV generator, not a Dynamo
  reskin. New `BlockType.Alternator = 11`, appended
  only, so old saves still load. Its output is HV
  (`VoltageRules.AlternatorOutput`), against the
  Dynamo's LV: a mill fed directly by an Alternator is
  destroyed on its tick, and a workstation fed directly
  is disabled that tick, exactly by the Step 1 mismatch
  rules — the source is proved against both consumers'
  failure rules, not just their powered rules. Safe use
  runs through one transformer step-down, face-adjacent
  and through cable, and both consumers run powered
  that way (mill every tick, workstation base + 5).
  Always-on while placed, like a Dynamo: no fuel or
  other condition is invented in this step, so a failed
  delivery stays unambiguous (unwired, conflicted, or
  mismatched, never "out of fuel"). Cost 10 Stone, no
  EmberOre — the mirror image of the Dynamo's 5 ore,
  so the Alternator competes with walls and mills for
  stone, not with workstations and squads for ore, and
  can be staged before the player has ore; safe use
  still costs a transformer's 5 ore, so the safe-use
  ore total ties a Dynamo's and the Alternator is not
  a cheaper Dynamo. New `Simulation.BuildAlternatorAhead`
  plus an N adapter key (suppressed while the picker is
  open, like the other builders) and playtest-gate
  checklist item 18, Windows-only.
- Precedence, decided in this step, in
  `DeliveredVoltage` and `TransformerOutput`: "direct
  source" now means Dynamo or Alternator together. A
  direct source still beats any converted or cabled
  voltage, whichever source it is — a mill touching a
  direct Alternator and an LV-outputting transformer
  receives HV. Two distinct direct sources touching
  one cell, one transformer, or one cable network
  resolve to None, not to a silent winner: a
  Dynamo-plus-Alternator mill, transformer, or network
  is visibly dead (kinetic / base behaviour), never
  destroyed, because no voltage delivered is not a
  mismatch. Cable networks carry Alternator HV like
  any other voltage, with the same mixed-network rule.
  No change to the ladder, conversion, cable loss or
  length rules, the mill's or workstation's ratings,
  powered behaviours, mismatch rules, or costs, Dynamo
  cost or output, or the save format: power is still
  derived from placement, so old saves load unchanged
  and Alternator step-down chains re-derive LV on load.
- Display, decided in this step: no new HUD source
  count or line. Sources are not counted, consumers
  are, as with the Dynamo — the existing powered-mill
  and powered-workstation counts are the Alternator's
  display, and they prove it feeding both consumers.
  Stated openly: a lone Alternator feeding no consumer
  shows nothing, the same named gap as an energised
  cable feeding no mill. Observed, not fixed here: the
  Step 1 workstation key (B) has no playtest-gate
  checklist item in the gate doc, while every other
  builder key does — carried as a gap for a later step
  to close, not bundled into this source step.
- Tests: 1239 passed, 0 failed (was 1157; +82, no
  failures on the first full run). Demo unchanged:
  Slice 9 Step 4 scenario save 43,975 chars, voltages
  LV/LV/HV/None, HUD 2/4 mills before and 2/3 after
  tick 1, powered 10 + kinetic 1 + destroyed 1 in 5
  ticks, stone 11.

## 0.45.0 — 2026-10-30 — Slice 10 Step 1: powered Workstation, second powered consumer

- Powered Workstation, decided in this step (rationale in
  DECISIONS.md): the existing Workstation block, not a new
  block type, is the second powered consumer. Its rating
  is LV, stated as its own `VoltageRules.WorkstationRated`
  constant, not borrowed from the mill's rating.
  `IsWorkstationPowered` / `IsWorkstationMismatched`
  are new, consumer-specific checks — `IsPowered` /
  `IsMismatched` stay mill-only, and a mismatched
  workstation is disabled (no regen that tick), not
  destroyed, and works again when rewired to LV or left
  unpowered. Powered effect: base regen + 5 per tick,
  additive with the skill bonus already in
  `WorkstationRegenPerTick`, in the existing workstation
  phase (after the enemy phase, before the squad phase),
  not in the powered-automation phase, because it is a
  player effect, not an excavation — and it produces
  nothing, so no per-machine inventory is built.
  Unpowered behaviour is unchanged (base regen).
  New `Simulation.BuildWorkstationAhead` closes the
  workstation's missing aimed builder (10 Stone + 5
  EmberOre, same cost as Slice 1), plus a B adapter key
  and a `HudState` powered/total workstation count (HV
  counts as NOT powered, as with mills). No save-format
  change: the workstation is an existing block type and
  power is derived from placement, so old saves load
  unchanged.
- Tests: 1157 passed, 0 failed (was 1135; +22, no
  failures). Demo unchanged: Slice 9 Step 4 scenario
  save 43,975 chars, voltages LV/LV/HV/None, HUD 2/4
  mills before and 2/3 after tick 1, powered 10 +
  kinetic 1 + destroyed 1 in 5 ticks, stone 11.

## 0.44.0 — 2026-10-30 — Slice 10 scope: electrical widening (second consumer, second source)

- Slice 10 scoped as its own decision step (rationale in
  DECISIONS.md), following the 0.43.0 decision that named
  scoping Slice 10 as next, from the ranked candidates
  carried in the Slice 9 review (additional powered
  consumers and power sources first, third kind and
  deferred doctrines second). New `docs/slice-10-scope.md`
  — documentation only, no code changed, by design.
  Slice 10 theme: electrical widening against the mill
  that already exists — one second powered consumer
  first, one distinct power source second (proved feeding
  both consumers, not the mill alone), closed by a
  composed demo and review third. Each step's decisions
  are left to that step, with starting hypotheses stated
  in the scope, not decisions smuggled into it: powered
  Workstation as the leading consumer candidate (with
  its missing aimed builder and key, and its
  player-centred regen, named as questions the step must
  answer), a stated strengthening over an unchanged
  unpowered behaviour, a consumer rating and mismatch
  rule that cannot simply inherit the mill-only
  `IsPowered` / `IsMismatched`, a per-step display
  decision against the mill-only HUD count, and the
  blueprint's alternator name taken as a name only for
  a source that must prove it is not a Dynamo reskin
  (output level, precedence in `DeliveredVoltage`,
  requirements, cost, and proof against both consumers).
  Deliberately NOT in Slice 10: a third consumer or a
  second source, tiers 3-4, crafting, a fuel system or a
  per-machine inventory unless a step justifies one for
  its own behaviour, a per-hop loss, cable length limit,
  or drag-to-lay mode, a third squad type or a new
  doctrine, a balance pass, mesh tint / refresh, a
  pre-destruction warning, free-deploy enforcement, or
  Windows Unity verification.
- Tests: 1135 passed, 0 failed (unchanged,
  documentation-only step). Demo unchanged: Slice 1
  gathered 1, built 1, killed 2, save 43,881 chars; Slice 2
  save 43,921 chars; Step 1 scenario save 44,016 chars;
  Step 3 scenario save 44,103 chars; Slice 4 scenario save
  44,026 chars; Slice 5 Step 2 scenario save 43,974 chars;
  Slice 6 Step 4 scenario save 44,024 chars; Slice 7 Step 2
  scenario save 44,013 chars; Slice 8 Step 3 scenario save
  43,974 chars; Slice 9 Step 4 scenario save 43,975 chars.

## 0.43.0 — 2026-10-30 — Slice 9 Step 4: demo scenario + documentation-only review

- Slice 9 demo scenario + review, decided in this step
  (rationale in DECISIONS.md), following the 0.42.0 decision
  that named Slice 9 Step 4 as next. No new core system
  rides along with this step, by design. The demo extends
  demo/Forgeborne.Demo with one composed Slice 9 run: one
  Simulation with four mills on the y=20 plane, each with
  a five-stone pocket — an LV cable mill (Dynamo -> cable
  -> cable -> mill), a round-trip LV mill (Dynamo ->
  transformer -> transformer -> mill), an HV cable mill
  (Dynamo -> transformer -> cable -> mill, destroyed on
  its first tick), and a kinetic mill with no power at
  all — through a save/load round-trip and a loaded
  5-tick phase. Voltages before any tick are LV, LV, HV,
  and None; HUD 2/4 MILLS POWERED, only the two LV mills
  counting. Tick 1 destroys the HV mill (cell Air,
  destroyed 1, powered gathered 2, HV pocket stone
  untouched, cable / transformer / Dynamo surviving);
  HUD drops to 2/3 with no new HUD line. After 5 ticks:
  powered gathered 10 (5 + 5), kinetic gathered 1,
  destroyed still 1, stone 11 (5 + 5 + 1, no refund).
  After load: the three surviving mills and all wiring
  survive, the HV mill's cell stays Air, power still 2/3,
  voltages re-derived as LV and LV with no stored voltage
  flag, session counters reset to 0, stone still 11 (no
  refund on load). After fresh re-seeding of the two
  powered pockets (labelled as fresh seeding, not
  leftovers) and 5 loaded ticks: powered gathered 10
  again, kinetic gathered 1 again, destroyed 0, stone 22.
  New docs/slice-09-review.md records the verdict honestly:
  Slice 9 is reviewed and closed as tested electrical
  tier 2 core (ladder, transformer, cable, destruction)
  plus written adapters, not as playtested, balanced, or
  Unity-verified; Slice 10 is NOT scoped by the review.
  Two facts are stated openly in the demo and the review,
  not smuggled: the scope's Step 4 wording ("the second
  mill runs at the converted voltage") cannot mean a mill
  surviving on HV after Step 3, so converted voltage is
  proved two honest ways — the round-trip mill surviving
  on HV converted back to LV, and the HV mill destroyed
  by HV itself — and the demo stages its four-row layout
  directly with TrySet, exactly as the Step 1-3 tests
  stage theirs, because aim placement of a transformer
  and of a cable were each proved in their own steps.
  No new playtest-gate checklist item is added, because
  the demo exercises no adapter binding items 15, 16,
  and 17 do not already cover. The ladder, conversion,
  cable distribution, kinetic behaviour, matched-voltage
  every-tick cadence, Dynamo cost, and squad, loot,
  skill, and core health rules are unchanged.
- Tests: 1135 passed, 0 failed (was 1099; +36 composed-
  integration tests mirroring the demo, no failures
  during the run). Demo: Slice 1 gathered 1, built 1,
  killed 2, save 43,881 chars; Slice 2 save 43,921 chars;
  Step 1 scenario save 44,016 chars; Step 3 scenario save
  44,103 chars; Slice 4 scenario save 44,026 chars;
  Slice 5 Step 2 scenario save 43,974 chars; Slice 6
  Step 4 scenario save 44,024 chars; Slice 7 Step 2
  scenario save 44,013 chars; Slice 8 Step 3 scenario
  save 43,974 chars; new Step 4 scenario save 43,975 chars.

## 0.42.0 — 2026-10-30 — Slice 9 Step 3: mismatched-voltage destruction

- Mismatched-voltage destruction, decided in this step
  (rationale in DECISIONS.md), following the 0.41.0 decision
  that named Slice 9 Step 3 as next. A Kinetic Mill is rated
  LV only, so a mill delivered HV — through a transformer,
  through a cable run, or both — is destroyed on the tick
  its mismatched voltage is delivered: its cell becomes Air
  during a new destruction phase that runs every tick, in
  chunk-index scan order, before either automation phase,
  so a destroyed mill excavates nothing on that tick and
  never again, not even on a 5th tick when the kinetic
  phase would otherwise have run it. This replaces, as a
  recorded behaviour change, the Step 1 / Step 2 interim
  rule under which an HV-fed mill fell back to kinetic
  cadence: the two Step 1 / Step 2 tests that pinned that
  fallback (the direct HV mill, and the converted cable
  mill in the transformer-in-run test) are rewritten in
  this step for destruction, and the HV delivery, precedence,
  and save/load assertions around them stand unchanged —
  delivery is still HV, a direct Dynamo still beats it, and
  a loaded HV mill still re-derives HV before its first
  tick destroys it. No voltage delivered (None: no source,
  a broken cable, or a mixed-voltage conflict) is NOT a
  mismatch, so a starved or conflicted mill still idles at
  kinetic cadence and is never destroyed; on this two-level
  ladder there is no delivered voltage below LV, so
  undervoltage and no delivery are the same observable
  state, None, and undervoltage therefore destroys nothing.
  Salvage is none — no Stone, Wood, or EmberOre is refunded
  on destruction or on load, so a wiring mistake costs a
  whole mill — and cascade is none: a destroyed mill's
  cable, transformer, Dynamo, and neighbouring mills all
  survive, each judged on its own delivered voltage, so a
  destroyed branch mill does not take its surviving branch
  with it. Transformers are not destroyed by HV input —
  they convert both levels, so a round-trip Dynamo ->
  transformer -> transformer -> mill survives intact — and
  cables are voltage-blind carriers that never break.
  Destruction is world state, not a flag: a destroyed mill
  stays destroyed across save/load, while only the new
  session counter `DestroyedMills` resets on load, like the
  gather counters. No new adapter binding: destruction is a
  core rule on a tick, not a key press, and no new HUD line
  is added — the HUD power line already drops, because a
  destroyed mill leaves TotalMills — but playtest-gate
  checklist item 17 covers a destroyed mill's disappearance
  in the scene, Windows-only, not claimed verified, because
  the mesh-refresh gap means a counted destruction may
  precede its visible disappearance. The ladder, conversion,
  cable distribution, kinetic behaviour, matched-voltage
  every-tick cadence, Dynamo cost, and squad, loot, skill,
  and core health rules are unchanged.
- Tests: 1099 passed, 0 failed (was 1023; +76, including
  the two rewritten fallback tests and one test-setup fix
  during the run: a save/load salvage test first failed
  because its surviving LV mill legitimately gathered a
  terrain stone before the save, so its other neighbours
  were cleared to Air to isolate the no-refund proof — no
  core code changed for it). Demo unchanged: Slice 1
  gathered 1, built 1, killed 2, save 43,881 chars; Slice 2
  save 43,921 chars; Step 1 scenario save 44,016 chars;
  Step 3 scenario save 44,103 chars; Slice 4 scenario save
  44,026 chars; Slice 5 Step 2 scenario save 43,974 chars;
  Slice 6 Step 4 scenario save 44,024 chars; Slice 7 Step 2
  scenario save 44,013 chars; Slice 8 Step 3 scenario save
  43,974 chars.

## 0.41.0 — 2026-10-30 — Slice 9 Step 2: cable distribution

- Cable distribution, decided in this step (rationale
  in DECISIONS.md), following the 0.40.0 decision that
  named Slice 9 Step 2 as next. New block type Cable =
  10, appended only, costing 1 EmberOre per cell: a cable
  that cost nothing and occupied nothing would not be
  distribution, it would be a longer face-adjacency rule
  wearing a name, and a cable priced at a full Dynamo's
  5 ore per cell would make even a short run cost more
  than its source, so 1 ore is the first-pass per-cell
  value, not a balance claim. `WorldZone.DeliveredVoltage`
  is widened, not replaced: direct Dynamo-against-mill
  still delivers LV exactly as in Slice 5, and a mill
  face-adjacent to an energised cable runs at the cable's
  voltage through the same powered phase and the same
  every-tick cadence. Voltage travels by a deterministic
  network walk over face-adjacent Cable cells, face only,
  no diagonals, with no loss per hop and no length limit
  in this step — a ten-cable run carries LV, proved by
  test, so a failed delivery here means unconnected or
  conflicted, never "too far" or "too lossy". Only Cable
  cells conduct: a wall, workstation, or mill between
  cables breaks the network, proved, and a mill never
  excavates its own wiring. Branching is proved: one
  cable network feeds two mills at the same voltage, 5
  gathers each in 5 ticks. A transformer sits in a cable
  run: the source-side mill still runs at LV, while the
  mill beyond the transformer runs at the converted HV
  and falls back to kinetic cadence under the Step 1
  interim rule — the transformer does not overwrite its
  source side. Network voltage resolution is decided
  openly, not left to scan order: a cable network touched
  by two distinct voltages carries None, and a mill
  touching two distinct voltages receives None, so a
  wiring conflict is visible as a dead mill; removing one
  source resolves the network to the remaining voltage
  on the next check, proved, because voltage is derived
  every tick, never stored. The one deliberate
  precedence that survives from Step 1 is direct-cell
  precedence: a mill or transformer touching a Dynamo
  directly still converts from / receives the Dynamo
  before any converted or cabled voltage, so adding a
  cable or transformer next to an existing powered pair
  never unpowers it. Simulation gains `BuildCableAhead`
  through the existing aimed builder, one cell per press,
  with no drag-to-lay mode; the Unity adapter gains the
  I key, next to U, suppressed while the picker is open
  like T / Y / U; HudState keeps its one rated-voltage
  power count, so a cable-fed LV mill counts as powered
  and a cable-fed HV mill counts as not; playtest-gate
  checklist item 16 covers the I key, Windows-only, not
  claimed verified. No new HUD cable or energised-network
  count is added in this step: an energised cable feeding
  no mill shows nothing in the HUD, carried as a named
  gap, not claimed.
- Tests: 1023 passed, 0 failed (was 975; +48, no failures
  during the run). Demo unchanged: Slice 1 gathered 1,
  built 1, killed 2, save 43,881 chars; Slice 2 save
  43,921 chars; Step 1 scenario save 44,016 chars; Step 3
  scenario save 44,103 chars; Slice 4 scenario save
  44,026 chars; Slice 5 Step 2 scenario save 43,974
  chars; Slice 6 Step 4 scenario save 44,024 chars; Slice 7
  Step 2 scenario save 44,013 chars; Slice 8 Step 3
  scenario save 43,974 chars.

## 0.40.0 — 2026-10-30 — Slice 9 Step 1: voltage ladder and transformer

- Voltage ladder and transformer, decided in this step
  (rationale in DECISIONS.md), following the 0.39.0
  decision that named Slice 9 Step 1 as next. New
  `src/Forgeborne.Core/Voltage.cs`: exactly two levels,
  LV and HV, because two suffice — LV is the Dynamo's
  output and the Kinetic Mill's rating, HV exists only
  because a transformer can produce it. New block type
  Transformer = 9, appended only, costing 5 EmberOre:
  one bidirectional block that flips its input voltage
  (LV to HV, HV to LV), face-adjacency only, with a
  direct Dynamo beating a converted voltage and a
  transformer ring with no Dynamo delivering None.
  `WorldZone.DeliveredVoltage` derives voltage every
  tick from placement, so no new save field exists and
  old saves load with their Dynamos at LV; `IsPowered`
  now means delivered voltage equals the mill's rated
  LV, so a Dynamo-against-mill pair behaves exactly as
  before, a Dynamo -> transformer -> transformer -> mill
  round-trip runs the mill at the every-tick cadence,
  and an HV-fed mill falls back to kinetic cadence in
  this step (destruction is Step 3's decision, and
  these tests pin the interim fallback openly).
  Simulation gains `BuildTransformerAhead` through the
  existing aimed builder; the Unity adapter gains the U
  key, suppressed while the picker is open like T and Y;
  HudState keeps one power count, now stated as
  rated-voltage only, so an HV-fed mill counts as not
  powered; playtest-gate checklist item 15 covers the U
  key, Windows-only, not claimed verified.
- Tests: 975 passed, 0 failed (was 929; +46, no failures
  during the run). Demo unchanged: Slice 1 gathered 1,
  built 1, killed 2, save 43,881 chars; Slice 2 save
  43,921 chars; Step 1 scenario save 44,016 chars; Step 3
  scenario save 44,103 chars; Slice 4 scenario save
  44,026 chars; Slice 5 Step 2 scenario save 43,974
  chars; Slice 6 Step 4 scenario save 44,024 chars;
  Slice 7 Step 2 scenario save 44,013 chars; Slice 8
  Step 3 scenario save 43,974 chars.

## 0.39.0 — 2026-10-30 — Slice 9 scope: electrical tier 2 (voltage, cable, destruction)

- Slice 9 scoped as its own decision step (rationale in
  DECISIONS.md), following the 0.38.0 decision that named
  scoping Slice 9 as next, from the ranked candidates
  carried in the Slice 8 review (rest of automation tier
  2 first, third kind and deferred doctrines second).
  New `docs/slice-09-scope.md` — documentation only, no
  code changed, by design. Slice 9 theme: electrical tier
  2 completion against the mill that already exists — a
  voltage ladder with a transformer first, cable
  distribution that carries a voltage beyond
  face-adjacency second, mismatched-voltage destruction
  third, closed by a composed demo and review fourth.
  Each step's decisions are left to that step, with
  starting hypotheses stated in the scope, not decisions
  smuggled into it: ladder count and names (blueprint
  LV-to-EV vocabulary, not a decision), existing Dynamo
  output level and old-save load rule, transformer form
  and direction, interim unmatched-voltage fallback in
  Step 1 (kinetic fallback, destruction deferred to Step
  3, with the change to be named openly when it lands),
  cable form and network voltage resolution, mill rating
  and undervoltage handling, destruction salvage and
  cascade, and save/display/adapter rules per step.
  Deliberately NOT in Slice 9: additional powered
  consumers or sources (first ranked Slice 10 candidate),
  tiers 3-4, crafting, fuel, a per-machine inventory,
  a third kind or doctrine, a balance pass, mesh tint /
  refresh, free-deploy enforcement, or Windows Unity
  verification.
- Tests: 929 passed, 0 failed (unchanged, documentation-
  only step). Demo unchanged: Slice 1 gathered 1, built 1,
  killed 2, save 43,881 chars; Slice 2 save 43,921 chars;
  Step 1 scenario save 44,016 chars; Step 3 scenario save
  44,103 chars; Slice 4 scenario save 44,026 chars;
  Slice 5 Step 2 scenario save 43,974 chars; Slice 6 Step 4
  scenario save 44,024 chars; Slice 7 Step 2 scenario save
  44,013 chars; Slice 8 Step 3 scenario save 43,974 chars.

## 0.38.0 — 2026-10-30 — Slice 8 Step 3: demo scenario + review

- Slice 8 composed demo, decided in this step (rationale in
  DECISIONS.md): the demo gains a Slice 8 Step 3 scenario
  after the Slice 7 scenario — one Simulation, facing +X
  throughout, that yaws 90 degrees, walks one tick (W walks
  +X, feet 16.0 -> 16.6, Z unchanged), pitches down 45
  degrees, gathers the +X ground cell (18,3,16), builds a
  wall back into that same cell, pitches level, and fires
  the +X enemy dead in 3 shots, then saves and loads.
  Numbers: gathered 1, built 1, killed 1, ammo 39/120,
  stone left 9 (10 funded + 1 gathered - 2 wall), save
  43,974 chars. The gather cell is (18,3,16), not the
  feet-level yaw-90 cell (17,3,16) from Step 2, because
  the walk moves the eye 0.6 m east before the
  pitch-down ray is traced and the descending ray strikes
  the ground one cell further east — printed and tested,
  not assumed. No new core system rides along with this
  step, by design.
- Save/load view rule, stated not smuggled: the loaded
  run is a fresh Simulation, so its Look restarts at yaw 0
  / pitch 0 (not the pre-save yaw 90), its feet restart at
  the cell corner (16, 4, 16 — the fractional 0.6 m walk is
  not saved), and its session counters reset to 0, while
  the world survives (wall at (18,3,16), enemies left 0).
  One loaded W tick then walks +Z to (16, 4, 16.6),
  proving the fresh-view rule in the same run. In-place
  restore keeping the current view (0.28.0 / 0.14.0) is
  unchanged; the demo loads into a fresh Simulation, so
  its view is fresh by construction, and the review says
  so rather than claiming a load preserves a view.
- Review: new `docs/slice-08-review.md` checks Slice 8
  against its scope gate (every step pushed with passing
  tests; yaw 0 unchanged and yaw 90 W +X, strafe sign and
  diagonal question decided and tested; gather, build,
  and fire each reading the core Look by the stated ray
  with per-action no-target refusals and no +Z fallback;
  neutral-aim compatibility proved as a split; builders
  distinct by block type; weapon, squad, Dynamo, mill,
  loot, skill, and save rules unchanged; composed demo
  through a save/load round-trip) and closes Slice 8 as
  tested aim interaction plus written (or unchanged)
  adapters — not playtested, balanced, or Unity-verified.
  No new playtest-gate checklist item is added for the
  demo itself, because it exercises no adapter binding
  the checklist does not already cover (items 13 and 14).
  Slice 9 is NOT scoped by this review; its ranked
  candidates are carried unchanged from the 0.35.0 scope
  (rest of automation tier 2 first, third kind and
  deferred doctrines second).
- All earlier demo numbers unchanged in this run: Slice 1
  gathered 1, built 1, killed 2, save 43,881 chars; Slice 2
  built 1, killed 1, kinetic gathered 1, squad intact,
  save 43,921 chars; Step 1 scenario save 44,016 chars;
  Step 3 scenario save 44,103 chars; Slice 4 scenario save
  44,026 chars; Slice 5 Step 2 scenario save 43,974 chars
  (same length as the new Slice 8 save by coincidence of
  format, not the same save); Slice 6 Step 4 scenario save
  44,024 chars; Slice 7 Step 2 scenario save 44,013 chars.
- Tests: 929 passed, 0 failed (was 908; +21 composed-
  integration tests mirroring the demo, no failures
  during the run).

## 0.37.0 — 2026-10-30 — Slice 8 Step 2: aim-based gather / build / fire

- Aim-based interaction, decided in this step (rationale in
  DECISIONS.md): gather, build, and fire now read the core
  `Look` direction through one shared ray, `AimRay.Trace`
  (new `src/Forgeborne.Core/Aim.cs`), replacing the fixed
  +Z cells and the Manhattan-nearest fire rule. The ray
  starts at the eye (feet + 1.6 m, the height the Unity
  adapter already renders the camera at), walks the Look
  forward vector in 0.05 m steps — small enough that a
  cell change is always face-by-face — and reaches 10 m,
  one reach for all three actions, a first-pass value for
  determinism and testability, not a feel claim. It
  matches the old FireAtCore 10-metre bubble, so fire
  keeps its old range while gaining aim; an unbounded
  ray was refused, since it would let a key press strip
  a distant hillside or hit an enemy anywhere.
- Gather: the first solid along the ray is the target,
  through the existing `World.Gather` rules (bedrock,
  walls, workstations, and mills never gathered; a full
  inventory consumes nothing; the skill gather bonus
  still accrues only on successful gathers). Looking
  down at the ground gathers ground, looking level
  gathers what is ahead at eye height, and looking up
  gathers nothing (Air) — no fallback to +Z. A solid
  stops the ray, so gather cannot strip a block through
  a wall; a ray that leaves the chunk is OutOfBounds.
  Actors do not block a gather: the ray passes through
  enemy and core cells to the first solid behind them.
- Build: the build cell is the last air cell before the
  first solid along the ray (face-adjacent to it, by
  the face-by-face walk), through the existing
  `World.Build` air / support / cost rules. The old
  front-right / front-left / front-centre offsets retire
  in this step: the three builders are now distinguished
  by block type, not by offset, because keeping offsets
  under an aim rule would mean the aim decides one cell
  and the offset silently moves it to another. A ray
  with no solid within reach has no aimed surface, so
  the build fails Floating and spends nothing; a ray
  that leaves the chunk fails OutOfBounds. The
  Dynamo-mill face-adjacency power rule is unchanged:
  a powered pair is still a mill face-adjacent to a
  Dynamo, however the two blocks were placed. A mill
  aimed at the ground cell (15,3,17) lands at
  (15,4,17) and a Dynamo aimed front lands at
  (16,4,17), so the powered pair is unchanged in
  placement — proved by the Slice 5 / Slice 6 tests and
  demo, not by keeping the offsets.
- Fire: one aim rule replaces BOTH old methods, which
  are removed, not kept as aliases — keeping a method
  named "nearest" beside an aim rule would leave the
  aim-blind rule alive under its old name. The new
  `FireAimed` hits the first enemy cell or core cell
  the ray reaches within 10 m; a solid before either
  stops the shot, so fire cannot hit through a wall;
  a shot with nothing aimed still consumes ammo and
  hits nothing, as before. Weapon damage, magazine,
  and reload rules do not change, and squad fire rules
  are untouched: aim changes what is hit, not how
  hard the weapon hits.
- Neutral-aim compatibility, stated openly, not
  silently: at yaw 0, pitch -45, the eye ray reaches
  the old gather cell (16,3,17), proved by test — pitch
  is what moved gather off the old feet-level rule,
  because the eye is 1.6 m above the feet and a level
  ray passes over the ground. At yaw 0, pitch 0, gather
  and build therefore do NOT reproduce the Slice 1
  cells, and the Slice 1 demo is updated for that
  (aimed gather / build, aimed fire at the first enemy)
  instead of keeping numbers the new rule no longer
  produces. Slice 1 demo numbers stand: gathered 1,
  built 1, killed 2, save 43,881 chars. Slice 2 demo
  numbers change openly: killed 1 (was 2), save
  43,921 chars (was 43,908) — aimed fire is
  range-bounded at 10 m where the nearest rule was
  not, so one enemy the old rule would have hit is
  now out of reach at the demo's fire ticks. All other
  demo saves are unchanged: Step 1 scenario 44,016,
  Step 3 scenario 44,103, Slice 4 scenario 44,026,
  Slice 5 Step 2 scenario 43,974, Slice 6 Step 4
  scenario 44,024, Slice 7 Step 2 scenario 44,013.
- Adapter: no new binding. E / Q / T / Y and mouse
  click call the same methods, which now read the core
  Look ray; the click call is renamed from
  `FireAtNearestEnemy` to `FireAimed`, and adapter
  comments are updated to stop claiming Slice 1 cells.
  The aim claim is checklist item 14 in
  `docs/slice-01-playtest-gate.md`, not a Unity claim.
  What must NOT change in this step is unchanged:
  weapon, squad, Dynamo, mill, loot, skill, and core
  health rules; enemy AI and targeting; the gather /
  build / survive loop itself; and the save format,
  which gains no field — `Look` stays session state.
  No third kind, no new doctrine, no voltage ladder,
  no balance pass, and no licence / monetisation /
  release change rides along. One test-authoring
  lesson is recorded in DECISIONS.md: a yaw -45 /
  pitch -45 mill aim lands one cell west of the old
  front-left cell, because the ray crosses the x=15
  face before it reaches the ground — the tests now
  aim at the (15,3,17) ground cell itself (yaw -18.4,
  pitch -53), and the suite caught the first aim.
- Tests: **908 passed, 0 failed** (was 889; +19 net:
  20 new Slice 8 Step 2 tests, plus the pitch and
  asymmetry blocks rewritten for the intended behaviour
  change, minus the old expectations they replaced).
  Two pre-existing blocks changed for the intended
  behaviour change, not for bugs: the 0.28.0 pitched
  gather / build / fire non-change test now proves the
  skyward aim (gather Air, build Floating with no
  spend, fire misses but consumes ammo), and the
  0.36.0 yaw-90 asymmetry test now proves aim-based
  gather / build / fire at yaw 90. The skill gather
  bonus failed-gather setup changed with them: with
  aim, an emptied cell lets the ray reach the next
  ground cell, so air is no longer a reliable failure
  — the failed gather now aims at a Wall (NotGatherable).

## 0.36.0 — 2026-10-30 — Slice 8 Step 1: yaw-rotated movement

- Yaw-rotated FPS movement, decided in this step
  (rationale in DECISIONS.md): `Simulation.Tick` now
  rotates the adapter's raw W/S and A/D scalars by the
  core `Look` yaw before they reach the FPS controller
  — forward (sin yaw, cos yaw) and right
  (cos yaw, -sin yaw) in XZ, so at yaw 0 the result is
  exactly Slice 1 (W +Z, D +X), at yaw 90 W walks +X,
  and at yaw 180 W walks -Z. The strafe sign is decided
  and pinned by test: facing +X, D walks -Z (the
  player's right) and A walks +Z, not the other way.
  Pitch is horizontal-only by construction: looking up
  45 or down 45 at yaw 90 still walks level +X with no
  vertical change, proved, not asserted. Diagonal speed
  is PRESERVED, not normalised: W+D at yaw 0 is
  (0.6, 0.6), at yaw 90 is (0.6, -0.6), and at yaw 45 is
  (0.849, 0) — same magnitude, rotated — so this
  direction step smuggles in no speed change. To make
  the rotation readable instead of a swapped-argument
  trick, `FpsController` gains `UpdateWorld` (world +X /
  +Z components, X-then-Z axis-separated sliding, same
  vertical code) and the old `Update` (forward +Z,
  strafe +X) delegates to it, so direct-controller
  callers keep exactly their Slice 1 behaviour and
  there is still one movement implementation.
- What does NOT change in this step, proved by test:
  base speed 6, gravity -20, jump velocity 8, the AABB
  (half-width 0.3, height 1.8), axis-separated sliding,
  grounded logic, and the skill speed multiplier (Swift
  Feet +5% walks 0.63 in +X at yaw 90); squad movement,
  enemy movement, and enemy targeting; and — deliberately
  for exactly this step, per the scope — gather, build,
  and fire, which at yaw 90 still use their fixed +Z
  cells and nearest-enemy rule, so a movement failure
  and a targeting failure stay distinguishable until
  Step 2. The Unity adapter changes only in comments:
  it still passes raw scalars, and no adapter logic
  changes, so the new behaviour is checklist item 13 in
  `docs/slice-01-playtest-gate.md`, not a Unity claim.
  One pre-existing test changed for the intended
  behaviour change, not for a bug: the 0.28.0 test that
  asserted yaw-90 movement stays world-aligned +Z is
  replaced by the new yaw tests, and its comment now
  says so. One test-authoring error was caught by the
  suite, not by review: a diagonal sliding test used a
  single wall cell, which a diagonal walk legitimately
  slides around; it now uses a full wall plane, plus a
  pure-X blocked check. No save-format field is added:
  `Look` stays session state, not saved.
- Adapter code is written, NOT compiled or run in Unity on
  Windows: this step adds no adapter logic change, and
  its adapter claim (mouse turn + W/D feel) is covered
  by the new checklist item 13 above. The test count
  below covers the core rotation only; it does not
  cover adapter keys or rendering, and this entry does
  not claim otherwise. What must NOT change in this step
  is unchanged: weapon, squad, Dynamo, mill, loot,
  skill, and core health rules; the gather / build /
  survive loop; and the save format, which gains no
  field in this step. No third kind, no new doctrine,
  no voltage ladder, no balance pass, and no licence /
  monetisation / release change rides along.
- Tests: **889 passed, 0 failed** (was 862; +27 net:
  29 new yaw-rotation tests, minus the 2 checks of the
  replaced 0.28.0 world-aligned expectation). Demo
  unchanged, as the scope predicted for a movement-only
  step at neutral yaw: Slice 1 gathered 1, built 1,
  killed 2, save 43,881 chars; Slice 2 save 43,908 chars;
  Step 1 scenario save 44,016 chars; Step 3 scenario save
  44,103 chars; Slice 4 scenario save 44,026 chars;
  Slice 5 Step 2 scenario save 43,974 chars; Slice 6
  Step 4 scenario save 44,024 chars; Slice 7 Step 2
  scenario save 44,013 chars.
  Next in order: Slice 8 Step 2, aim-based gather /
  build / fire.

## 0.35.0 — 2026-10-30 — Slice 8 scope: aim-based interaction

- New `docs/slice-08-scope.md` (documentation only,
  no new core system, by design). Slice 8 theme:
  aim-based interaction — the player's aim (core
  `Look` yaw and pitch) decides what the player touches
  and which way the player walks, closing the gap Slice 7
  left open: a player who can look anywhere but still
  gathers, builds, fires, and walks only at fixed +Z.
  In order: (1) yaw-rotated movement, decided in that
  step (core rotation hypothesis, horizontal yaw only,
  strafe sign and diagonal speed each decided and
  tested, interaction cells deliberately unchanged for
  that step); (2) aim-based gather / build / fire,
  decided in that step (ray origin, traversal, and
  reach stated; gather, build, and fire targeting each
  decided; no silent fallback to fixed +Z cells;
  neutral-aim compatibility proved or demo numbers
  updated openly; no adapter logic change expected);
  (3) Slice 8 demo scenario + documentation-only review.
  Ranked Slice 9 candidates recorded, not promised:
  rest of automation tier 2 first, third kind and
  deferred doctrines second. Unity verification and the
  director playtest explicitly left open, not closed by
  this scope.
- Tests: **862 passed, 0 failed** (unchanged,
  documentation-only step). Demo unchanged: Slice 1
  gathered 1, built 1, killed 2, save 43,881 chars;
  Slice 2 save 43,908 chars; Step 1 scenario save 44,016
  chars; Step 3 scenario save 44,103 chars; Slice 4
  scenario save 44,026 chars; Slice 5 Step 2 scenario
  save 43,974 chars; Slice 6 Step 4 scenario save 44,024
  chars; Slice 7 Step 2 scenario save 44,013 chars.
- Adapter code is written, NOT compiled or run in Unity on
  Windows: this step adds no adapter change, and adds no
  new checklist item, because it scopes work rather than
  adding anything to verify in Unity. The test count
  above covers no new system; it confirms the existing
  suite still passes under the unchanged code.
- What must NOT change in this step is unchanged:
  movement, gather, build, and fire rules; weapon, squad,
  Dynamo, mill, loot, skill, and save rules; the gather /
  build / survive loop; and the save format, which gains
  no field in this step. No third kind, no new doctrine,
  no voltage ladder, no balance pass, and no licence /
  monetisation / release change rides along.

## 0.34.0 — 2026-10-30 — Slice 7 Step 2: demo scenario + review

- Composed Slice 7 demo scenario (demo/Forgeborne.Demo),
  plus `docs/slice-07-review.md` (documentation only,
  no new core system, by design). One Simulation proves
  Vanguard and Recon together, not separately: both
  deploy through the paid picker (10 EmberOre each,
  20 spent, 0 left), Vanguard HoldGround on the z=16
  lane (3 members, 90 HP, 30 damage, range 3) and Recon
  Advance on the z=20 lane (2 members, 40 HP, 12 damage,
  range 5) — the Recon spawns on its own lane only
  because the player walks to z=20 between deploys,
  since a squad spawns at the player's feet. One enemy
  per lane starts at (27,5,z) with the player parked far
  at (28,5,4), so every enemy step pursues a squad.
  Tick 3: the marching Vanguard is silent while the
  Recon fires its lane enemy at exactly range 5 for 12
  (30 -> 18), a distance Vanguard's range 3 cannot
  reach. Tick 4: the Vanguard reaches its rally and
  kills its lane enemy at exactly range 3 for 30, while
  the Recon fires its lane enemy to 6 HP. Tick 5: the
  Recon kills its lane enemy, both squads finish intact
  at full health (90 and 40), no enemies left. A third
  picker deploy is then REFUSED unaffordable, picker
  stays open, nothing charged. Save/load round-trip
  keeps both kinds, doctrines, positions, and health
  intact, ore still 0, a further unfunded deploy still
  refused; one loaded tick with no enemies leaves the
  Vanguard Holding and the Recon Idle, by derivation.
  An old save with its Squads field stripped loads with
  0 squads and no crash. The demo exercises both
  doctrines across the two kinds (Vanguard HoldGround,
  Recon Advance); the other two combinations are proved
  in the Step 1 tests, not in this demo, and the review
  says so plainly.
- Review verdict (docs/slice-07-review.md): Slice 7
  reviewed and closed as tested second-kind core plus
  written adapters — not playtested, balanced, or
  Unity-verified. Slice 8 NOT scoped by this review;
  candidates carried in scope order: aim-based gather /
  build / fire + yaw-rotated movement first, rest of
  automation tier 2 second, third kind and deferred
  doctrines third. Gaps named in priority order: no
  Unity verification, no director playtest, no balance
  pass, two kinds sharing one marker (decided, pending
  a play-observed confusion), mesh tint / mesh refresh
  (from 0.30.0), free-deploy primitive documented not
  enforced.
- Adapter code is written, NOT compiled or run in Unity on
  Windows: this step adds no adapter change, and its
  adapter claims are already covered by checklist item 10
  in docs/orbital-command.md (kind keys 3 / 4,
  doctrine-kind independence, prompt kind word, Recon text
  line, same cyan marker). The test count below covers
  the core demo composition only; it does not cover
  adapter keys or rendering, and this entry does not claim
  otherwise. What must NOT change in this step is
  unchanged: Vanguard's stats, movement, fire, targeting,
  and save behaviour; the two doctrines and their rules;
  Dynamo cost, distribution, and cadence; kinetic mill
  behaviour; loot, skills, and core health; the gather /
  build / survive loop; and the save format, which gains
  no field in this step. No third kind, no new doctrine,
  no voltage ladder, no balance pass, and no licence /
  monetisation / release change rides along.
- Tests: **862 passed, 0 failed** (was 810; +52, with one
  test-authoring error caught on re-read before the first
  run, not a code fix: a lane assertion first read the
  Recon's X where its Z was meant and was corrected to
  Z = 20 before running). Demo unchanged except the new
  scenario: Slice 1 gathered 1, built 1, killed 2, save
  43,881 chars; Slice 2 save 43,908 chars; Step 1 scenario
  save 44,016 chars; Step 3 scenario save 44,103 chars;
  Slice 4 scenario save 44,026 chars; Slice 5 Step 2
  scenario save 43,974 chars; Slice 6 Step 4 scenario
  save 44,024 chars; new Step 2 scenario save 44,013 chars.
  Next in order: scoping Slice 8 as its own decision step.

## 0.33.0 — 2026-10-30 — Slice 7 Step 1: second squad type, Recon

- Exactly one second squad type, decided in this step
  (rationale in DECISIONS.md): Recon, chosen from the
  recon / heavy / flyer pool the code already named.
  Recon is a fragile long-range screen — 2 members, 20 HP
  each (40 total), 6 damage per living member (12 at full
  strength), engagement range 5 — against Vanguard's
  unchanged 3 members, 30 HP each (90 total), 10 damage
  each (30), range 3. Recon is better than Vanguard in
  exactly one dimension (reach) and worse in the other
  two (health, damage), so it is a choice, not a
  replacement: it fires from cells Vanguard cannot reach
  (proved at range 5, silent at range 6, Vanguard silent
  at the same range-4 geometry), but it cannot hold
  ground under fire the way Vanguard can. Heavy was not
  chosen (a tougher anchor under HoldGround is Vanguard
  with more HP, not a new tactical choice); flyer was
  not chosen (it would need a new movement rule with
  targeting and occupancy consequences, untestable in
  isolation in this step).
- Stats representation, decided in this step: a per-kind
  shape (`SquadShape` / `SquadShapes.For` in
  `src/Forgeborne.Core/Orbital.cs`), read from the
  squad's kind by movement, fire, snapshot, and load
  alike — not a second set of shared constants, and not
  a branch in every rule. The old Vanguard constants are
  gone; every rule that read them now reads the shape,
  and Vanguard's numbers are unchanged (proved by the
  shape tests and by every pre-existing Vanguard test
  passing unchanged). Movement stays the shared rule:
  one voxel per tick, largest-gap axis, solid and
  enemy-occupied cells refused, for both kinds and both
  doctrines. Unknown kinds have no shape and throw at
  the boundary, rather than silently returning Vanguard.
- Price and cap, decided in this step: 10 EmberOre for
  both kinds (`SquadEconomy.CostForKind`, charged through
  the existing paid path), and the cap stays 3 live
  squads in total, not 3 per kind — a 4th squad is
  refused at the total cap even when it is a different
  kind, and a refused deploy charges nothing. The
  picker prompt's COST 10 / MAX 3 lines therefore stay
  true for both kinds with no display change, and the
  text display already renders kind verbatim, so no
  text change was needed. A price difference was
  refused for now: the kinds trade role for role, not
  price for power, and a price gap with no balance
  evidence behind it would invent an economy
  distinction, not express one.
- Picker binding, decided in this step: doctrine and
  kind are now two independent choices. Adapter keys 1
  / 2 select doctrine only (HoldGround / Advance) and
  keys 3 / 4 select kind only (Vanguard / Recon); before
  this step 1 and 2 also forced Vanguard, so the
  doctrine-kind combinations with a second kind were
  unreachable. All four combinations now deploy through
  the paid picker (proved in core via SelectKind /
  SelectDoctrine / Confirm; the adapter keys themselves
  are written, NOT compiled or run in Unity on Windows
  — see checklist item 10 in docs/orbital-command.md).
  Doctrine interaction: both kinds work under both
  doctrines, with the same activity words, and a kind
  restricted to one doctrine would have needed its own
  refusal rule — none was invented.
- Save and targeting, decided in this step: no new save
  field, since kind was already index 8 of the saved
  squad array — but the load clamp is now per-kind
  (Vanguard 0..90, Recon 0..40), because the old 0..90
  clamp would have been a save bug for a 40-HP kind
  only in the other direction (it would not trim Recon,
  but it would let a corrupt Recon save claim Vanguard
  health — now clamped to its own maximum). Malformed
  and unknown kinds keep the existing rule: skipped,
  never fatal. Targeting stays kind-blind and unchanged:
  `ChooseTarget` picks the nearest live target
  regardless of kind, proved by an enemy pursuing a
  nearer Recon squad over the farther player.
- Markers, decided in this step: unchanged — the same
  cyan squad cube for both kinds, kind readable from
  the text line, per the Slice 6 one-marker-per-squad
  discipline. No distinct marker shape is added on a
  hypothetical confusion; that would need a
  play-observed reason, not a guess.
- Adapter code is written, NOT compiled or run in Unity on
  Windows: see the new checklist item 10 in
  docs/orbital-command.md (kind keys 3 / 4, doctrine-kind
  independence, prompt kind word, Recon text line, same
  cyan marker). The test count below covers the core
  kind rules only; it does not cover adapter keys or
  rendering, and this entry does not claim otherwise.
  What must NOT change in this step is unchanged:
  Vanguard's stats, movement, fire, targeting, and save
  behaviour; the two doctrines and their rules; Dynamo
  cost, distribution, and cadence; kinetic mill
  behaviour; loot, skills, and core health; the gather /
  build / survive loop; and the save format, which gains
  no field in this step. No third kind, no new doctrine,
  no voltage ladder, no balance pass, and no licence /
  monetisation / release change rides along.
- Tests: **810 passed, 0 failed** (was 746; +64, with one
  test-authoring fix during the run, not a code fix: two
  new variable names collided with earlier top-level test
  variables and were renamed). Demo unchanged: Slice 1
  gathered 1, built 1, killed 2, save 43,881 chars;
  Slice 2 save 43,908 chars; Step 1 scenario save
  44,016 chars; Step 3 scenario save 44,103 chars;
  Slice 4 scenario save 44,026 chars; Slice 5 Step 2
  scenario save 43,974 chars; Slice 6 Step 4 scenario
  save 44,024 chars.
  Next in order: Slice 7 Step 2, demo scenario +
  documentation-only review.

## 0.32.0 — 2026-10-30 — Slice 7 scope: second squad type

- Scoped Slice 7, plus `docs/slice-07-scope.md`
  (documentation only, no new system, by design). Slice 7
  theme is exactly one second squad type, taken from the
  Slice 6 review's top-ranked candidate in unchanged
  order: (1) one second kind, decided in that step —
  its role chosen from the recon / heavy / flyer pool
  the code already names (or another original role only
  with a recorded reason none of the three fits), its
  shape, per-kind stats representation, price under the
  Slice 3 economy, total-vs-per-kind cap, independent
  picker binding, doctrine interaction, save-clamp, and
  targeting rules all decided in that step, not here,
  with Vanguard's numbers (3 members, 30 HP each,
  10 damage each, range 3) unchanged by it; then (2) a
  Slice 7 demo scenario plus documentation-only review
  proving both kinds together through a save/load
  round-trip. The scope states up front why the second
  type was deferred until now and is actionable now:
  Slice 6's pitched picker and readable markers remove
  the 0.15.0-style reason for waiting, while the facts
  that make step 1 non-trivial are named honestly —
  stats are shared Vanguard constants, not a per-kind
  table; kind is already saved (index 8) but the load
  clamp 0..90 is Vanguard-shaped; adapter keys 1/2 both
  force Vanguard, so Unity cannot select a second kind
  today; and the picker prompt hardcodes COST 10 and
  MAX 3, so a different price or cap must update it or
  it lies. Ranked Slice 8 candidates recorded, not
  promised: aim-based interaction plus yaw-rotated
  movement first, then the rest of automation tier 2,
  then a third kind and the deferred doctrines.
- Tests: **746 passed, 0 failed** (unchanged,
  documentation-only step; re-run this run, see below).
  Demo unchanged: Slice 1 gathered 1, built 1, killed 2,
  save 43,881 chars; Slice 2 save 43,908 chars; Step 1
  scenario save 44,016 chars; Step 3 scenario save
  44,103 chars; Slice 4 scenario save 44,026 chars;
  Slice 5 Step 2 scenario save 43,974 chars; Slice 6
  Step 4 scenario save 44,024 chars.
  Next in order: Slice 7 Step 1, exactly one second
  squad type.

## 0.31.0 — 2026-10-30 — Slice 6 Step 4: demo scenario + review

- Slice 6 demo scenario + review, decided in this step
  (rationale in DECISIONS.md): no new core system, by
  design — this step composes steps 1–3 in one demo run
  and writes `docs/slice-06-review.md`. The demo proves,
  headlessly, the core presentation pieces only: a clamped
  Look and its forward vector (start (0, 0, 1), 90 yaw /
  45 pitch giving (0.707, 0.707, 0), +100 pitch more
  clamping at 89 with forward almost straight up); a
  HoldGround squad reading Marching at spawn and Holding
  at its rally after exactly 4 ticks, with no enemy present
  so the march is a clean reading; an Advance squad
  reading Hunting with a live prey and Idle after its
  full-strength squad kills the 30-HP prey in one
  engagement tick, at (16,5,17) rather than marching to
  its ignored rally (24,5,16); and a powered pair built
  through the real Simulation builders reading 1/1 on the
  power indicator, gathering 5 in 5 ticks from a seeded
  stone pocket, then gathering 5 again in the loaded game
  after a save/load round-trip that also preserves both
  squads (still Holding and Idle by derivation) while the
  Look correctly resets to a fresh view, because Look is
  session input state and is not saved. Save JSON 44,024
  chars. The demo cannot prove adapter rendering — no
  headless run can show a marker in a scene, a key press
  building a Dynamo, or a HUD text line — so the review's
  verdict separates the two claims in words: the core
  presentation pieces are tested, the adapter pieces are
  written and checklist-listed, and neither claim borrows
  the other's evidence. The review closes Slice 6 as
  tested presentation core plus written adapters, not as
  playtested, balanced, or Unity-verified, and it adds no
  new playtest-gate checklist item because its adapter
  claims are already covered by gate items 11–12 and the
  orbital / powered checklists. Slice 7 is NOT scoped by
  this review; its scope is the next run's decision, from
  the ranked candidates the review carries unchanged.
  What must NOT change in this step is unchanged: squad
  movement, fire, targeting, price, and cap; Dynamo cost,
  distribution, and cadence; kinetic mill behaviour; loot,
  skills, and core health; and the save format, which
  gains no field in this step.
- Adapter code is written, NOT compiled or run in Unity on
  Windows: nothing in this step changes an adapter file,
  and this entry does not claim otherwise. The test count
  below covers the composed core pieces only; it does not
  cover adapter keys, markers, camera rendering, or HUD
  text, and the review states that split explicitly.
- Tests: **746 passed, 0 failed** (was 709; +37, with no
  test failures during the run: Look forward values and
  clamp, HoldGround Marching then Holding at its rally,
  Advance Hunting then Idle with its prey cell, both paid
  deploys spending 20 ore, powered builders and 1/1
  indicator, powered rate 5 in 5 ticks before and after
  load, both squads and both blocks surviving load with
  their derived readings, Look resetting on a fresh load,
  and the demo save length 44,024 chars as a composition
  fingerprint). Demo unchanged except for the new
  scenario: Slice 1 gathered 1, built 1, killed 2, save
  43,881 chars; Slice 2 save 43,908 chars; Step 1 scenario
  save 44,016 chars; Step 3 scenario save 44,103 chars;
  Slice 4 scenario save 44,026 chars; Slice 5 Step 2
  scenario save 43,974 chars; new Step 4 scenario save
  44,024 chars.

## 0.30.0 — 2026-10-30 — Slice 6 Step 3: powered Unity presence

- Powered Unity presence, decided in this step (rationale
  in DECISIONS.md): the Slice 5 loop gains a hand in Unity
  — T builds a Kinetic Mill (front-left, via the existing
  `BuildKineticMillAhead`, 10 Stone + 5 Wood) and Y builds
  a Dynamo (front-centre, via the existing
  `BuildDynamoAhead`, 5 EmberOre), in
  `ForgeborneUnity/Assets/Scripts/FpsInputAdapter.cs`.
  No core build rule is added or changed: the builders,
  their cells, costs, and support rules already existed,
  and this step only wires keys to them. T and Y are free
  (existing keys are WASD / Space / E / Q / R / O /
  Return / Esc / 1 / 2 / mouse click) and sit together as
  the build pair, so a mill and a Dynamo built with no
  move between them are face-adjacent and therefore
  powered. Both keys are suppressed while the rally
  picker is open, like mouse fire: a Dynamo build while
  picking would spend 5 EmberOre that a pending Confirm
  is about to charge 10 of, so an untimed key could make
  the aimed deploy fail for no visible reason; the mill
  is suppressed with it so the pair behaves as one modal
  rule. E / Q / R keep their pre-existing unsuppressed
  behaviour (they spend no EmberOre, so they cannot break
  a pending Confirm's affordability).
- The power indicator, decided in this step: `HudState`
  gains `PoweredMills` / `TotalMills`, derived in core on
  every snapshot by counting Kinetic Mill cells in the
  chunk and asking `WorldZone.IsPowered` per mill — no
  new core rule, no new save field. `HudDisplay` renders
  it as `POWER: {powered}/{total} MILLS POWERED` from
  its new `PowerText` field, wired in the Inspector. A
  Dynamo alone, or a wall next to a Dynamo, counts as
  neither; only mills are consumers. The counts are
  derived, not saved: removing a Dynamo drops the count
  on the next snapshot, and a loaded pair counts again
  because the Dynamo block survives as a chunk byte.
- Mesher check, decided in this step: the mesher does NOT
  already render a Dynamo distinctly from stone or mills
  (`MeshBuilder` assigns every quad the same unit UVs,
  with no per-block colour, atlas, or material split), so
  no distinct mesh tint is added or claimed here — a tint
  would need per-block UV / material work plus Unity
  material wiring, which is not small and cannot be
  proved headlessly. The text indicator above is the
  presence proof, per the scope, and the tint is carried
  as a named gap in `docs/powered-presence.md`, not
  quietly dropped. A second pre-existing gap is named
  there too: no adapter refreshes the voxel mesh on any
  key build (E / Q / T / Y alike; `GameManager`
  refreshes on start and load only), so mesh visibility
  of a key build is a checklist question, not an
  assumption — the HUD line updates every frame
  regardless of mesh refresh.
- Adapter code is written, NOT compiled or run in Unity on
  Windows: see the new checklist in
  `docs/powered-presence.md` (keys, indicator counts,
  picker suppression, mesh visibility, tint observation)
  and item 12 added to
  `docs/slice-01-playtest-gate.md`. The test count below
  covers the core indicator counts only; it does not
  cover adapter keys or rendering, and this entry does
  not claim otherwise. What must NOT change in this step
  is unchanged: Dynamo cost (5 EmberOre), face-adjacency
  distribution, powered cadence (every tick, replacing
  not stacking with kinetic cadence), kinetic mill
  behaviour, squad rules, loot, skills, core health, and
  the save format, which gains no field in this step.
- Tests: **709 passed, 0 failed** (was 685; +24, with no
  test failures during the run: fresh 0/0, unpowered
  0/1, powered 1/1, two mills 1/2, diagonal 0 powered,
  Dynamo-alone and wall-next-to-Dynamo count as no mill,
  count drops on Dynamo removal, counts survive
  save/load by derivation, and the Simulation builder
  pair reads 1/1 through `Hud()` while a lone builder
  mill reads 0/1). Demo unchanged: Slice 1 gathered 1,
  built 1, killed 2, save 43,881 chars; Slice 2 save
  43,908 chars; Step 1 scenario save 44,016 chars;
  Step 3 scenario save 44,103 chars; Slice 4 scenario
  save 44,026 chars; Slice 5 Step 2 scenario save
  43,974 chars.

## 0.29.0 — 2026-10-30 — Slice 6 Step 2: 3D squad / rally markers and the Advance display fix

- 3D squad / rally markers, decided in this step (rationale
  in DECISIONS.md): a new Unity adapter
  `ForgeborneUnity/Assets/Scripts/SquadMarkerDisplay.cs`
  renders `OrbitalView.Snapshot` only — one marker per live
  squad, one per squad rally, and one pending-rally marker
  while the picker is open, no more. Forms are original
  primitives built in the adapter: a cyan cube (0.6 units)
  at the squad cell centre, an orange flat cube
  (0.9 x 0.12 x 0.9) at the bottom of the rally cell so it
  stays visible under a squad standing on its own rally,
  and an orange sphere (0.6 units) for a pending rally so
  a pick in progress cannot be mistaken for a deployed
  rally. Colours follow `docs/art-direction.md` (neon cyan
  for squads, orange for rallies and objectives). Markers
  are pooled (at most 3 + 3 + 1 for the 3-squad cap), their
  colliders are removed on creation so they never block
  the picker raycast or player movement, and they do NOT
  show through walls: ordinary opaque URP Lit material
  (Standard fallback), occluded honestly by the voxel mesh
  by depth — a through-wall marker would be a wallhack
  claim, not a readability aid, and the Windows checklist
  records occlusion as a check, not an assumption.
- The Advance display fix, decided in this step: the
  rally-relative `MOVING` / `HOLDING` word is replaced, not
  kept alongside, by a core-derived `SquadActivity` on the
  snapshot — HoldGround reads `MARCHING` away from its
  rally and `HOLDING` at it; Advance reads `HUNTING`
  whenever a living enemy exists (moving toward it or
  standing adjacent firing, with no range limit, because
  hunting is not engagement) and `IDLE` when none does
  (standing still, ignoring its rally, as Advance is
  documented to behave). A hunting snapshot also carries
  its prey cell, re-derived on every snapshot from the
  same nearest-enemy rule (Manhattan, first in list order
  on ties) that Advance movement and fire already use, so
  movement, fire, and display can never disagree about the
  prey. Activity and prey are derived, not saved: no save
  field is created for either, and a loaded Advance squad
  reads `HUNTING` at the same prey again because the world
  it loads still contains that prey. The text display
  renders the activity verbatim (a hunting line also shows
  its prey cell), so text and markers read the same word
  from the same snapshot. `AtRally` / `IsMoving` remain on
  the snapshot as position facts, but they are no longer
  the display word.
- Adapter code is written, NOT compiled or run in Unity on
  Windows: see the new checklist items 7-9 in
  `docs/orbital-view.md` (marker appearance and activity
  words, occlusion, marker hygiene) and the marker wiring
  step added to that doc. The test count below covers the
  core activity and prey derivation only; it does not
  cover adapter rendering, and this entry does not claim
  otherwise. What must NOT change in this step is
  unchanged: squad movement, fire, targeting, doctrine,
  and kind rules; squad price and cap; the picker flow
  and its keys; Dynamo and mill behaviour; and the save
  format, which gains no field in this step.
- Tests: **685 passed, 0 failed** (was 665; +20, with no
  test failures during the run: HoldGround marching and
  holding, Advance hunting beyond engagement range and
  adjacent to its prey, Advance idle with no enemy and
  with only a dead enemy, hunting tie to first in list
  order, prey re-derived after a kill with the old
  snapshot keeping its old prey, and hunting activity and
  prey surviving save/load by derivation). Demo unchanged:
  Slice 1 gathered 1, built 1, killed 2, save 43,881 chars;
  Slice 2 save 43,908 chars; Step 1 scenario save 44,016
  chars; Step 3 scenario save 44,103 chars; Slice 4
  scenario save 44,026 chars; Slice 5 Step 2 scenario save
  43,974 chars.
  Next in order: Slice 6 Step 3, powered Unity presence.

## 0.28.0 — 2026-10-30 — Slice 6 Step 1: FPS pitch aiming

- FPS pitch aiming, decided in this step (rationale in
  DECISIONS.md): pitch lives in a new testable core `Look`
  in `src/Forgeborne.Core/Look.cs`, together with yaw —
  not as an adapter-only camera angle, because the
  direction vector is what the picker raycast, and any
  future aim rule, actually consumes. `Look` holds yaw
  (unbounded, wrapping naturally in trigonometry) and
  pitch in degrees, clamped to [-89, 89] so the camera
  cannot flip past vertical, with a unit forward vector
  derived as (cos pitch * sin yaw, sin pitch, cos pitch *
  cos yaw): yaw 0 faces +Z, the same "front" as the
  Slice 1 builders, yaw 90 faces +X, positive pitch looks
  up. Yaw is moved into `Look`, not duplicated beside it:
  the Unity adapter no longer stores its own `_yaw`, it
  feeds already-scaled mouse deltas (mouse delta times its
  existing `MouseSensitivity`, so there is no second
  sensitivity value in core) into
  `Simulation.ApplyLook` / `Simulation.Look`, and renders
  the camera from the core forward vector via
  `Quaternion.LookRotation`, so no aim math is duplicated
  in the adapter. The adapter's mouse-Y sign (positive =
  look up, per the core convention) is a Windows checklist
  item, not a headless claim: if it is inverted in Unity,
  the adapter call flips, not the core convention.
- What pitch changes first, decided as camera + picker
  only, per the scope hypothesis: the picker raycast
  already follows the camera's forward, so a pitched
  camera immediately lets a rally be picked by looking
  down at flat ground, which is the concrete picker
  failure pitch exists to fix. Gather, build, and fire
  keep their Slice 1 cells and rules, and movement stays
  world-aligned — yaw-rotated movement is explicitly NOT
  bundled in, and is carried forward as a Slice 7
  candidate with aim-based interaction. Tests prove the
  non-changes, not just assert them: a Simulation pitched
  up 45 degrees still gathers ahead, builds a wall ahead,
  and kills with fire at the nearest enemy, and a
  Simulation yawed 90 degrees still moves in +Z, not +X.
- Save question, decided as session state, not saved:
  `Look` gains no save field, so a fresh Simulation (and
  a load into one) starts at yaw 0, pitch 0. An in-place
  `RestoreWorld` does NOT reset it, for the same identity
  reason it keeps the same Fps object: a Unity load into
  the same Simulation keeps the player's current view
  instead of snapping the camera to +Z. This refines the
  scope's "starts wherever the adapter starts it" wording
  — a new scene's adapter starts at 0, 0; a continuing
  session keeps its view — and the distinction is recorded
  in DECISIONS.md, not hidden behind the hypothesis.
- Adapter code is written, NOT compiled or run in Unity on
  Windows: see the new checklist item 9 in
  `docs/orbital-command.md` and item 11 in
  `docs/slice-01-playtest-gate.md` (mouse sign, clamp,
  ground-rally picking, unchanged gather / build / fire,
  world-aligned movement). The test count below covers
  the core Look math only; it does not cover adapter
  rendering, and this entry does not claim otherwise.
  What must NOT change in this step is unchanged: weapon
  damage, range, and mag rules; the fire targeting rule;
  gather and build cells; squad movement, fire, targeting,
  price, and cap; Dynamo cost, distribution, and cadence;
  and the save format.
- Tests: **665 passed, 0 failed** (was 633; +32, with no
  test failures during the run: forward vectors at yaw 0 /
  90 / 180 / -90, pitch +/-45, the combined yaw 90 +
  pitch 45 vector, both clamps including a stepped 80 + 20
  stopping at 89, yaw accumulation and 360-degree return
  to +Z, unit length, Simulation ownership, Look not
  saved but surviving in-place restore, and the pitched /
  yawed non-change proofs above). Demo unchanged: Slice 1
  gathered 1, built 1, killed 2, save 43,881 chars; Slice 2
  save 43,908 chars; Step 1 scenario save 44,016 chars;
  Step 3 scenario save 44,103 chars; Slice 4 scenario save
  44,026 chars; Slice 5 Step 2 scenario save 43,974 chars.
  Next in order: Slice 6 Step 2, 3D squad / rally markers
  and the Advance display fix.

## 0.27.0 — 2026-10-30 — Slice 6 scope: FPS presentation

- Scoped Slice 6, plus `docs/slice-06-scope.md`
  (documentation only, no new system, by design). Slice 6
  theme is FPS presentation, taken from the Slice 5 review's
  top-ranked candidate in unchanged order: (1) FPS pitch
  aiming, decided in that step (core Look versus
  adapter-only pitch, camera plus picker first, gather /
  build / fire and yaw-rotated movement explicitly NOT
  bundled into the pitch step); (2) 3D squad / rally
  markers, including the Advance display fix (a core-derived
  activity word — marching / holding / hunting / idle —
  replacing the rally-relative MOVING / HOLDING word, with
  the hunting target re-derived each tick, not saved);
  (3) powered Unity presence, folded in as its own step
  because the Slice 5 review attached the powered absence
  to this candidate (mill and Dynamo build keys, plus a
  power indicator derived from the existing `IsPowered`
  rule, with no change to Dynamo cost, distribution, or
  cadence); then (4) a Slice 6 demo scenario plus
  documentation-only review. The scope states up front that
  Slice 6 is the first slice whose main value is adapter
  code that cannot be compiled headlessly, so its review
  must separate headless-tested core claims from
  Windows-checklist adapter claims, and must not let either
  borrow the other's evidence. A second squad type is the
  first ranked Slice 7 candidate, followed by aim-based
  interaction plus yaw-rotated movement, then the rest of
  automation tier 2.
- Tests: **633 passed, 0 failed** (unchanged,
  documentation-only step). Demo unchanged: Slice 1 gathered
  1, built 1, killed 2, save 43,881 chars; Slice 2 save
  43,908 chars; Step 1 scenario save 44,016 chars; Step 3
  scenario save 44,103 chars; Slice 4 scenario save 44,026
  chars; Slice 5 Step 2 scenario save 43,974 chars.
  Next in order: Slice 6 Step 1, FPS pitch aiming.

## 0.26.0 — 2026-10-30 — Slice 5 Step 2: demo scenario + review

- Composed Slice 5 demo scenario, plus `docs/slice-05-review.md`
  (documentation only, no new core system, by design). One
  Simulation proves Step 1 together with the loop it joins,
  not separately: a Kinetic Mill and a Dynamo built through
  the real Simulation builders (front-left and front-centre,
  face-adjacent with no move between builds, Dynamo spending
  its 5 EmberOre), a seeded stone pocket around the powered
  mill, and an unpowered mill pocket alongside as the kinetic
  baseline. After 5 ticks the powered mill has gathered 5
  blocks and the kinetic mill 1, stone 0 -> 6, and the Dynamo
  still stands — the powered mill never excavates its own
  source. Save/load keeps the mill, the Dynamo, and the
  gathered stone, the loaded mill is powered from placement
  with no flag, and both session counters reset to 0, as
  they are not saved. Five fresh stone neighbours are then
  seeded around the loaded powered mill (its first pocket
  is spent, as it should be — labelled as fresh seeding,
  not leftover blocks), and 5 more ticks give loaded powered
  5 and loaded kinetic 1, stone 12 in total. Save JSON
  43,974 chars.
- Review verdict: Slice 5 is **reviewed and closed as tested,
  integrated powered automation** — not playtested, balanced,
  or Unity-verified. It carries the ranked Slice 6 candidates
  from the 0.24.0 scope in the same order, as the starting
  point for Slice 6 scoping, not as a promise, and adds one
  new note to the first candidate: powered automation has no
  Unity presence at all yet (no Dynamo build key, no powered
  rendering, no power indicator), since Slice 5 touched no
  adapter. Slice 6 is NOT scoped by this review.
- Demo otherwise unchanged: Slice 1 gathered 1, built 1,
  killed 2, save 43,881 chars; Slice 2 built 1, killed 2,
  kinetic gathered 1, squad intact 3 members / 90 HP,
  squads saved+loaded 1, save 43,908 chars; Step 1 scenario
  core 42 HP, weapon 28/120, shields 100/105, save 44,016
  chars; Step 3 scenario core 42 HP, 3 paid squads 90 HP,
  10 ore left, 4th picker deploy REFUSED at cap, save 44,103
  chars; Slice 4 scenario HoldGround at rally, Advance
  hunting, save 44,026 chars.
- Tests: **633 passed, 0 failed** (was 610; +23 composed-
  integration tests mirroring the new demo scenario, with
  no test failures during the run). Next in order: scoping
  Slice 6 as its own decision step.

## 0.25.0 — 2026-10-29 — Slice 5 Step 1: automation tier 2, first electrical loop

- The first powered loop, decided in this step (rationale in
  DECISIONS.md): exactly one power source, one distribution
  rule, and one powered behaviour. The source is a new block,
  `BlockType.Dynamo` (value 8, appended only — earlier values
  unchanged, so old saves still load), built for 5 EmberOre
  only under the same air / support rules as other machines.
  Distribution is face-adjacency only: a cell is powered when
  at least one of its six face neighbours holds a Dynamo —
  no cables, no network walk, no chaining through another
  mill, no diagonals. The powered behaviour is a powered
  variant of an existing machine, not a new machine family:
  a Kinetic Mill face-adjacent to a Dynamo runs every tick
  instead of every 5th tick, through the same reused
  `World.Gather` path into the same player inventory, so
  there is still one block-gathering rule and no second
  inventory system. Power replaces the kinetic cadence, it
  does not stack with it: a powered mill is handled only by
  the powered phase, so it excavates once per tick, not
  twice on a 5th tick, and it counts in a separate session
  counter (`PoweredBlocksGathered`, not saved) so the two
  rates stay separable. A Dynamo is solid and not gatherable,
  so no mill — powered or kinetic — ever excavates its own
  power source, and a full inventory consumes nothing, as
  with kinetic mills.
- Decisions this step closes, per the Slice 5 scope: voltage
  is a single voltage, always on while placed — no fuel, no
  voltage tiers, no transformers, no mismatch destruction
  yet, because a first loop must prove power on / off changes
  gameplay before failure modes punish wiring it wrong; the
  tier 1 inventory question is decided as KEEP the player-
  inventory rule (no machine buffer — the powered behaviour
  produces ordinary gathered blocks the inventory already
  holds, so a buffer would add size, pickup, and overflow
  rules for no new capability); the save format gains no new
  field, because powered state is derived at load from the
  Dynamo blocks themselves, exactly as targeting is derived
  each tick — remove a Dynamo and the mill falls back to
  kinetic cadence on the next tick, and a loaded powered
  mill gathers on its first tick. The build cost (5 EmberOre)
  deliberately competes with workstations (also 5 EmberOre)
  and squads (10 EmberOre = two Dynamos), not with walls or
  mills for Stone and Wood, so power is a deliberate ore
  investment, not a free upgrade to a mill already owned.
- Also: `Simulation.BuildDynamoAhead` (front-centre at feet
  level, distinct from the wall front-right and mill
  front-left builders). Core owns the power rule; no Unity
  adapter was touched, so no new Windows Unity verification
  item rides along — the Unity verification checklist and
  the Slice 1 director playtest gate remain open, as before.
  Kinetic mill behaviour in zones without power is unchanged;
  squads, loot, skills, and core health are unchanged.
- Tests: **610 passed, 0 failed** (was 579; +31: Dynamo cost
  / failure / floating rules, face-only / diagonal / chaining
  distribution, powered 5-in-5 vs kinetic 1-in-5 in identical
  pockets, no double activation on tick 5, power loss falling
  back to kinetic cadence, full-inventory no-consume, Dynamo
  survival and powered-first-tick after save/load, and the
  Simulation Dynamo builder). Demo unchanged: Slice 1
  gathered 1, built 1, killed 2, save 43,881 chars; Slice 2
  save 43,908 chars; Step 1 scenario save 44,016 chars;
  Step 3 scenario save 44,103 chars; Slice 4 scenario save
  44,026 chars. Next in order: Slice 5 Step 2, demo scenario
  + documentation-only review.

## 0.24.0 — 2026-10-29 — Slice 5 scope: powered automation

- New `docs/slice-05-scope.md` (documentation only, no new
  core system, by design). Slice 5 theme: powered automation
  — the first small electrical loop, not the whole electrical
  tier. In order: (1) automation tier 2, first loop only —
  exactly one power source, one distribution rule, and one
  powered behaviour, with the voltage question, the tier 1
  inventory question, the build cost, and the save format
  all decided in that step, not in this scope; (2) Slice 5
  demo scenario + documentation-only review. Ranked Slice 6
  candidates carried in the same order (FPS pitch + 3D
  markers including the Advance display gap, then a second
  squad type); the rest of tier 2 is deferred, not ranked
  above them by this scope.
- Tests: **579 passed, 0 failed** (unchanged). Demo
  unchanged: Slice 1 gathered 1, built 1, killed 2, save
  43,881 chars; Slice 2 save 43,908 chars; Step 1 scenario
  save 44,016 chars; Step 3 scenario save 44,103 chars;
  Slice 4 scenario save 44,026 chars.
  Next in order: Slice 5 Step 1, automation tier 2, first
  small electrical loop only.

## 0.23.0 — 2026-10-29 — Slice 4 Step 3: demo scenario + review

- Composed Slice 4 demo scenario, plus `docs/slice-04-review.md`
  (documentation only, no new core system, by design). One
  Simulation proves Step 1 and Step 2 together, not
  separately: a HoldGround squad and an Advance squad, both
  deployed through the paid picker (10 EmberOre each, 0 left),
  both weakened to 1 member so their 10-damage fire does not
  kill outright and hide the engagement, player moved far
  away, one enemy east and one west. Tick 1: both enemies
  pursue the squads, both squads march east, nobody hurt.
  Tick 2: Advance fires the east enemy 30 -> 20 while
  HoldGround, still marching, stays silent. Tick 3: the east
  enemy reaches adjacency and both squads take exactly 10
  each (single adjacent damage, not double) while Advance
  fires it 20 -> 10. Tick 4: HoldGround takes a second 10
  while Advance kills its attacker and takes nothing from
  it, and the west enemy has pursued to 14. Save/load after
  tick 4 keeps positions, health, doctrines, rallies, and
  spent ore, and the loaded game cannot deploy more without
  ore. Tick 5 (loaded): HoldGround marches on while Advance
  moves WEST toward the west enemy and fires it — hunting
  from the saved position, not marching to its stored rally.
  Tick 6 (loaded): HoldGround reaches its rally; the west
  enemy retargets to the nearer Advance squad, Advance fires
  it to 10 and takes exactly one adjacent 10, HoldGround at
  its rally unharmed. Save JSON 44,026 chars.
- Review verdict: Slice 4 is **reviewed and closed as tested,
  integrated combat answers** — not playtested, balanced, or
  Unity-verified. Slice 5 is NOT scoped by this review; its
  ranked candidates are carried from the 0.20.0 scope in the
  same order (automation tier 2, then FPS pitch + 3D markers
  including the Advance display gap, then a second squad
  type), with gaps named in priority order in the review.
- Tests: **579 passed, 0 failed** (was 528; +51: paid picker
  deploys for both doctrines, per-tick positions / health /
  damage in the composed run, save/load preservation of
  doctrines / rallies / health / spent ore, loaded-game
  hunting and retargeting). Demo unchanged except the new
  scenario: Slice 1 gathered 1, built 1, killed 2, save
  43,881 chars; Slice 2 save 43,908 chars; Step 1 scenario
  save 44,016 chars; Step 3 scenario save 44,103 chars.
  Next in order: scoping Slice 5 as its own decision step.

## 0.22.0 — 2026-10-29 — Slice 4 Step 2: second doctrine, Advance

- Exactly one second doctrine, chosen in this step from
  Advance / Defend Core / Patrol (rationale in DECISIONS.md):
  Advance. An Advance Vanguard squad hunts the nearest living
  enemy, one voxel per tick, re-derived every tick (Manhattan,
  first in list order on ties), and fires at the nearest enemy
  in range 3 whether or not it is at its rally. The rally the
  picker still requires to deploy is ignored after deployment
  for Advance: with no living enemy the squad stands still,
  and it stands still again once its prey dies — it never
  falls back to marching to its rally, which is what keeps
  Advance from collapsing into "HoldGround at a moving
  rally". Movement is not duplicated: Advance and HoldGround
  share one step rule in TacticalSquad (largest-gap axis,
  X/Y/Z tie-break, solid and enemy-occupied cells refused);
  only the target differs. Adjacent-enemy damage is unchanged
  (one path, squad phase, after squad fire), and HoldGround
  behaviour is unchanged.
- Picker and economy: Advance is selectable in core via
  OrbitalCommand.SelectDoctrine and in the Unity adapter via
  key 2 (1 stays HoldGround); a picker confirm deploys it
  through the Slice 3 paid path — 10 EmberOre, cap 3 live
  squads — with no new price invented. Save/load needs no
  new field: doctrine and rally were already persisted, and
  a loaded Advance squad keeps hunting from its saved
  position. The Unity adapter code is written, NOT compiled
  or run in Unity on Windows; see the new checklist item 8
  in docs/orbital-command.md.
- Also corrected a stale adapter comment that still claimed
  no deployment cost or squad cap existed (settled in 0.18.0).
- Tests: **528 passed, 0 failed** (was 495; +33: Advance
  deploy, no-enemy standstill, moves toward enemy not rally,
  fires while not at rally, move-then-fire entering range,
  nearest-enemy list-order tie, solid-block hold with fire,
  stops after prey dies with no rally fallback, save/load
  hunting, paid picker deploy at the settled price, snapshot
  doctrine). Demo unchanged: Slice 1 gathered 1, built 1,
  killed 2, save 43,881 chars; Slice 2 save 43,908 chars;
  Step 1 scenario save 44,016 chars; Step 3 scenario save
  44,103 chars. Next in order: Slice 4 Step 3, demo scenario
  + documentation-only review.
- Defend Core and Patrol are deferred, not rejected; the
  display's MOVING / HOLDING word remains rally-relative, so
  for Advance it is not a hunting state — recorded as a
  presentation gap in DECISIONS.md and docs/orbital-command.md.

## 0.21.0 — 2026-10-29 — Slice 4 Step 1: enemy AI targeting squads

- Enemies now pursue squads, not only the player. Targeting
  rule, decided in this step (rationale in DECISIONS.md):
  nearest live target by Manhattan distance, re-derived
  every tick — the player or any live squad, moving and
  holding squads alike. Ties go to the player, and ties
  between squads go to the first squad in deploy order.
  Dead squads are never targeted. The player remains a
  valid target, chosen whenever nearest or tied and
  re-chosen as soon as a nearer squad dies or moves away,
  so an enemy ignores the player only for the ticks a
  squad is strictly nearer — which is what lets a squad
  intercept or draw pressure off the player, not abandon
  the player wholesale.
- Movement is generalised, not duplicated: `Enemy.
  TickTowardPosition` now holds the single movement rule
  (one voxel per tick, largest-gap axis, ties X then Y
  then Z, no move when adjacent to the target, solid or
  occupied cells refused), and the player-only
  `TickToward` delegates to it. An enemy pursuing one
  target cannot walk through and overlap another target
  on its way there.
- Damage reconciliation, the scope's named risk, is by
  keeping exactly one squad-damage path: the squad phase's
  existing "10 per surviving adjacent enemy, after the
  squad fires". The enemy phase adds no squad damage, so
  a pursued squad is never counted twice for the same
  hit, and a squad that kills its attacker still takes
  nothing from it. Player damage stays proximity-based in
  the enemy phase, as before. Tick order (enemies, then
  squads) is unchanged. No new save field: targets are
  recomputed from saved positions and health, and a
  save/load round-trip still targets correctly.
- One pre-existing test changed for the intended behaviour
  change, not for a bug: the Slice 2 range test's enemy
  would now walk toward the nearer squad into range 3 and
  be killed, so its step is blocked by a solid block to
  keep isolating the range rule; pursuit-into-range is
  covered by the new tests instead.
- Tests: **495 passed, 0 failed** (was 466; +29: nearer-
  squad pursuit, player-nearer, player tie-break, dead
  squad never targeted, moving-squad pursuit over two
  ticks, exactly-once adjacent damage in both the
  surviving and the killing case, retargeting after a
  squad dies, direct movement-generalisation cases, and
  targeting derived after a save/load round-trip).
  Demo unchanged: Slice 1 gathered 1, built 1, killed 2,
  save 43,881 chars; Slice 2 save 43,908 chars; Step 1
  scenario save 44,016 chars; Step 3 scenario save
  44,103 chars. Next in order: Slice 4 Step 2, exactly
  one second doctrine, chosen in that step from Advance /
  Defend Core / Patrol.

## 0.20.0 — 2026-10-29 — Slice 4 scope (documentation only)

- Added `docs/slice-04-scope.md`: Slice 4 is scoped, as its
  own decision step, as **squads under fire** — the combat
  answers Slice 3 left open. In order: (1) enemy AI
  targeting squads, with the targeting rule, the movement
  generalisation, and the reconciliation with the existing
  squad-phase adjacent damage all decided in that step, not
  here; (2) exactly one second doctrine, chosen in that step
  from Advance / Defend Core / Patrol, with Advance as the
  starting hypothesis because it is the moving-into-danger
  doctrine the targeting step exists to answer, deployed
  through the Slice 3 paid path with no new price invented;
  (3) a Slice 4 demo scenario and a documentation-only
  Slice 4 review that closes the slice. Ranked Slice 5
  candidates are recorded for the review to start from, not
  promised.
- Deliberately NOT in Slice 4: automation tier 2, FPS pitch
  + 3D markers, a second squad type, the full 1,500-node
  graph, a balance pass, or any licence / monetisation /
  release change. Windows Unity verification remains open
  for Slices 1-4 alike.
- No code changed in this step, by design: a scope document
  must not smuggle in a system, and the targeting rule and
  the doctrine choice are explicitly left to their own steps
  to decide with evidence. Tests re-run unchanged: **466
  passed, 0 failed**. Demo unchanged: Slice 1 gathered 1,
  built 1, killed 2, save 43,881 chars; Slice 2 save 43,908
  chars; Step 1 scenario save 44,016 chars; Step 3 scenario
  save 44,103 chars. Next in order: Slice 4 Step 1, enemy
  AI targeting squads.

## 0.19.0 — 2026-10-29 — Slice 3 Step 3: demo scenario + review

- Closes Slice 3 with the composition proof its scope asked
  for, and with its own review. No new core system rides
  along with this step, by design: the demo scenario and the
  review prove Steps 1 and 2 together, they do not widen the
  game.
- Demo: a new Slice 3 Step 3 scenario after the Step 1
  scenario — one skilled, loot-equipped Simulation damages
  its core with two shots, deploys 3 squads through the paid
  path, is refused a 4th deploy through the picker at the
  cap (picker stays open, ore untouched), then saves and
  loads into a fresh Simulation. Evidence: core 42 HP,
  Titan Long Pulse Rifle 28/120, effective damage 24,
  shields 100/105, 3 paid squads at 90 HP, 10 ore left,
  before and after the load alike — and the loaded game
  still cannot deploy more (`CanDeployOrbitalSquad` false),
  because the cap survives the round-trip, not just the
  squads. Save JSON 44,103 chars.
- Review: new `docs/slice-03-review.md` checks the Slice 3
  gate against the scope (all steps pushed with passing
  tests, old saves load, core / build / weapon survive a
  round-trip in tests and demo, unaffordable and over-cap
  deploys refuse without charging or half-deploying) and
  records the verdict honestly: Slice 3 is reviewed and
  closed as tested, integrated persistence + economy — not
  playtested, not balanced, not Unity-verified. It carries
  the ranked Slice 4 candidates from the 0.16.0 scope in
  the same order, as the starting point for Slice 4 scoping,
  not as a promise. Slice 4 is NOT scoped by this review.
- Demo otherwise unchanged: Slice 1 gathered 1, built 1,
  killed 2, save 43,881 chars; Slice 2 built 1, killed 2,
  kinetic gathered 1, squad intact 3 members / 90 HP,
  squads saved+loaded 1, save 43,908 chars; Step 1 scenario
  core 42 HP, weapon 28/120, shields 100/105, save
  44,016 chars.
- Tests: **466 passed, 0 failed** (was 437; +29 composed-
  integration tests mirroring the new demo scenario, with
  no test failures during the run). Next in order: scoping
  Slice 4 as its own decision step.

## 0.18.0 — 2026-10-29 — Slice 3 Step 2: squad economy (cost and cap)

- Closes the second Slice 3 debt: Orbital deployment is no
  longer unlimited. One Vanguard squad under HoldGround now
  costs 10 EmberOre from the player inventory, and at most 3
  live squads may be fielded at once. Both are first-pass
  values for determinism and testability, not final balance,
  per the Slice 3 scope's no-balance-pass rule.
- How it is enforced, without a second copy of the rules:
  `SquadEconomy` in `Orbital.cs` holds the two constants
  (`MaxConcurrentSquads = 3`, `EmberOreCost = 10`) and the
  live-count / affordability queries; `WorldZone.
  TryDeployPaidSquad` is the single play deployment path and
  checks rally bounds, doctrine/kind validity, the cap, and
  affordability BEFORE charging, so a refused deploy charges
  nothing and adds no squad, and an unexpected failure after
  charging refunds in full. `OrbitalCommand.Confirm` now
  deploys through that paid path, so a picker confirm the
  player cannot afford, or that exceeds the cap, fails closed
  exactly like a failed Confirm in 0.15.0 — picker stays
  open, no squad, no charge. `Simulation` gains
  `CanDeployOrbitalSquad` and `DeployOrbitalSquadPaid` as the
  headless equivalents of a picker Confirm.
- What deliberately did NOT change: `WorldZone.DeploySquad`
  and `Simulation.DeployOrbitalSquad` remain the free spawn
  primitives for headless tests and the demo only — every
  pre-0.18.0 test that deploys squads still passes unchanged
  on that path, and the distinction is documented in code,
  in tests, and in `docs/orbital-command.md`, rather than
  hidden. Unity play goes through the picker, so play is
  priced; the demo's Slice 2 squad now deploys through the
  paid path (funded with 10 EmberOre first). The Unity
  display's picker prompt now shows the cost and cap; it
  enforces nothing, core does.
- Demo: Slice 1 unchanged (gathered 1, built 1, killed 2,
  save 43,881 chars); Slice 2 unchanged in its reported
  numbers (built 1, killed 2, kinetic gathered 1, squad
  intact 3 members / 90 HP, squads saved+loaded 1, save
  43,908 chars), with the squad now deployed through the
  paid path. Next in order: Slice 3 Step 3, demo scenario
  + review.
- Tests: **437 passed, 0 failed** (was 381; +56). Three
  pre-existing 0.15.0 picker tests were updated, not the
  code, for the intended behaviour change: their confirms
  are now funded with 10 EmberOre, because an unfunded
  picker confirm is exactly what this step refuses — the
  unfunded refusal itself is covered by the new tests.
  Every other pre-existing deploy test passes unchanged
  on the free headless path.

## 0.17.0 — 2026-10-29 — Slice 3 Step 1: save-format completion

- Closes the first Slice 3 debt: a load no longer silently
  resets what the game promises. The save format now carries,
  in addition to everything it carried before: the core
  objective's health, the applied skill build, and the
  equipped weapon — the weapon question the 0.16.0 scope left
  open is decided here, in DECISIONS.md, as YES, it joins the
  format. A damaged core stays damaged across a load (a
  destroyed core stays destroyed); a fresh Simulation loading
  a save restores the build instead of starting at base values
  until someone re-applies it; a loot weapon, including its
  in-progress mag, survives instead of reverting to the Pulse
  Rifle.
- How it is built, without breaking old saves: new fields are
  nullable and declared last in `SaveData` (`CoreHealth`,
  `SkillBuild` as the existing Base64 encoding, and five
  weapon fields — name, damage, mag capacity, current mag,
  reserve — all together or none), and null fields are omitted
  when writing, so a world-only save grows only by the
  CoreHealth field. `SaveSystem.LoadSave` returns the world
  plus the build and weapon, if any; `LoadZone` still returns
  just the world for existing callers. `Simulation.SaveToJson`
  is the full save (world + build + weapon) and
  `Simulation.LoadFromJson` restores all three, replacing the
  current build and weapon when the save carries them and
  keeping them when it does not (an old save). `WorldZone.
  RestoreCoreHealth` clamps to 0..100 and `Weapon.FromSaved`
  restores a live weapon with its current mag, under the
  constructor's clamps. The skill build is decoded before the
  player is restored, so the raised shield cap is in place
  before shields are restored — a skilled save with 100
  shields no longer clamps to the base 85 cap on load.
  Malformed skill strings and partial weapons are skipped,
  never fatal, matching the 0.10.0 squad rule; out-of-range
  core values clamp to 0..100. `GameManager.SaveToJson` now
  uses the full Simulation save instead of the world-only
  save, so Unity saves stop dropping the build and weapon too
  (Unity compile itself is still unverified headlessly).
- Demo: a Slice 3 Step 1 scenario after the Slice 2 scenario —
  a skilled, loot-equipped Simulation fires twice at its core
  from near it (FPS teleported there as the headless stand-in
  for walking), then saves and loads into a fresh Simulation.
  Evidence: core 42 HP, Titan Long Pulse Rifle 28/120,
  effective damage 24, shields 100/105, before and after the
  load alike; full save JSON 44,016 chars.
- Tests: **381 passed, 0 failed** (was 342; +39). One test
  failure during the run was a test-arithmetic error, not a
  code bug: with the weapon restore skipped, the fresh
  Simulation keeps the 10-damage Pulse Rifle, so the restored
  build gives 12, not the loot weapon's 24 — the expectation
  was corrected to 12. Demo otherwise unchanged: Slice 1
  gathered 1, built 1, killed 2, save 43,881 chars (was 43,864;
  the +17 is the CoreHealth field); Slice 2 save 43,908 chars
  (was 43,891; same +17). Next in order: Slice 3 Step 2,
  squad economy (cost and cap).

## 0.16.0 — 2026-10-29 — Slice 3 scope (documentation only)

- Added `docs/slice-03-scope.md`: Slice 3 is scoped, as its
  own decision step, as **economy and persistence** — the two
  debts Slice 2 left behind. In order: (1) save-format
  completion — core objective health and the applied skill
  build persisted in the existing format, old saves still
  loading, equipped weapon decided in that step, not here;
  (2) squad economy — deployment cost and cap decided in
  that step, with resource pricing as the starting hypothesis
  but not a decision, and fail-closed enforcement in the
  picker; (3) a Slice 3 demo scenario and a documentation-only
  Slice 3 review that closes the slice. Ranked Slice 4
  candidates are recorded for the review to start from, not
  promised.
- Deliberately NOT in Slice 3: the full 1,500-node graph,
  automation tiers 2-4, crafting, fuel/power, extra doctrines
  or squad types, enemy AI targeting squads, 3D markers or an
  Orbital camera, a balance pass, FPS pitch, or any licence /
  monetisation / release change. Windows Unity verification
  remains open for Slices 1-3 alike.
- No code changed in this step, by design: a scope document
  must not smuggle in a system, and the economy's pricing
  basis is explicitly left to its own step to decide with
  evidence. Tests re-run unchanged: **342 passed, 0 failed**.
  Demo unchanged: Slice 1 gathered 1, built 1, killed 2, save
  43,864 chars; Slice 2 save 43,891 chars. Next in order:
  Slice 3 Step 1, save-format completion.

## 0.15.0 — 2026-10-29 — Doctrine menu / rally picker replacing provisional O

- Closes Slice 2 review follow-up (3): the provisional O key
  from 0.11.0, which deployed a squad instantly at the player's
  feet, is gone from the Unity adapter. Core now holds the menu
  as `OrbitalCommand` in `src/Forgeborne.Core/Orbital.cs`:
  selected doctrine/kind (HoldGround / Vanguard, the only pair
  so far), `BeginPick` (opens the picker, clearing any stale
  pending rally), `SetRally` (bounds-checked, picking-only),
  `Confirm` (deploys one squad at the pending rally, then
  closes and clears; no pick or no rally deploys nothing and
  a failed deploy keeps the picker open for a re-pick),
  `CancelPick` (no squad, rally dropped), and
  `SelectDoctrine` / `SelectKind` (unknown values rejected,
  selection unchanged). `Simulation` exposes the same flow as
  `BeginOrbitalPick`, `SetOrbitalRally`, `CancelOrbitalPick`,
  and `ConfirmOrbitalDeploy`; the direct `DeployOrbitalSquad`
  path remains for headless use and the demo only.
- Unity adapters stay thin: `FpsInputAdapter` binds O (open /
  cancel picker), crosshair raycast while picking (forward,
  30 m, voxel cell hit), Enter (deploy), Esc (cancel), and 1
  (re-select HoldGround / Vanguard), and suppresses weapon fire
  while picking so a click cannot shoot during rally aiming.
  `OrbitalSquadDisplay` shows a picker prompt line first while
  picking, then the squad list unchanged; with no squads it now
  reads `ORBITAL: NO SQUADS (press O to pick a rally)`.
  `VoxelMeshRenderer` gains a `MeshCollider` sharing the built
  mesh, so the picker's raycast actually hits the chunk.
  All three adapters are still NOT compiled headlessly — see
  `docs/orbital-command.md` for the Windows verification
  checklist, including the honest limit that the camera only
  yaws in Slice 1 (no pitch yet), so the ray runs horizontally
  from eye height and picks a rally by aiming at a face, not by
  looking down at flat ground.
- Deployment cost / cap decided, deferred: none yet, because
  inventory holds blocks, not a currency, so a price invented
  here would be a balance guess with no economy to price
  against. Cost and cap belong to a Slice 3 economy step, not
  to the picker. Repeated picker use still deploys repeatedly
  — a known, documented gap, not a balance decision.
- Updated `docs/orbital-view.md` (provisional-O section now says
  replaced, wiring and checklist steps use the picker) and added
  `docs/orbital-command.md` (menu flow, bindings, checklist).
- Tests: **342 passed, 0 failed** (was 297; +45). Demo unchanged:
  Slice 1 gathered 1, built 1, killed 2, save 43,864 chars;
  Slice 2 save 43,891 chars. With this step, all three Slice 2
  review follow-ups are complete (demo scenario, in-place
  restore, picker) — but Slice 3 is still NOT scoped by this
  step; scoping it is the next decision, not smuggled in here.

## 0.14.0 — 2026-10-29 — GameManager.LoadFromJson in-place restore

- Closed the Slice 1 limitation named in 0.3.0 and repeated in
  the 0.12.0 review: loading a save in Unity previously only
  logged it, because `Simulation.World` was get-only and the FPS
  position was private-set, so no adapter could restore in place.
  Core now supports it, and the Unity adapter is still thin:
  `FpsController.Teleport(x, y, z)` (clears vertical velocity,
  resets grounded), `Simulation.RestoreWorld(loaded)` (swaps the
  world inside the same Simulation object — the Fps object is
  never replaced, so GameManager and MeshRenderer need no
  re-wiring — teleports Fps to the saved player position, resets
  session counters, keeps the equipped weapon since the save
  format carries none, and re-applies an applied skill build to
  the new world so caps, speed, and regen do not silently revert
  to base values), and `Simulation.LoadFromJson(json)` (fail-closed:
  null, empty, or malformed JSON returns false and leaves the
  current world untouched, because the new world is fully parsed
  before any swap). `GameManager.LoadFromJson` now calls it,
  refreshes the mesh on success, and logs a warning on failure.
- Inherited save-format limit, documented not hidden: the core
  objective's health is not in the save format, so a loaded world
  starts with a fresh 100-HP core. Fixing that is a save-format
  step, not smuggled into this restore step.
- Updated `docs/orbital-view.md` step 6: the Unity save/load
  checklist step no longer carries the "only logs a load"
  caveat; the restore itself is now headless-tested, and only
  the Windows Unity verification of it remains open.
- Tests: **297 passed, 0 failed** (was 263; +34). Demo unchanged:
  Slice 1 gathered 1, built 1, killed 2, save 43,864 chars;
  Slice 2 save 43,891 chars. Next in order: (3) a doctrine menu /
  rally picker replacing provisional O. Slice 3 is still NOT scoped.

## 0.13.0 — 2026-10-29 — Slice 2 demo scenario (composition evidence)

- Extended `demo/Forgeborne.Demo` with a Slice 2 scenario after
  the unchanged Slice 1 run: one Simulation composed with every
  Slice 2 system at once — seed-0 loot equipped (Titan Long
  Pulse Rifle, 20 damage, 30 mag), the full Forge Survivor
  cluster applied, a Kinetic Mill built, a Vanguard HoldGround
  squad deployed at the player position, 20 ticks of combined
  play, then a save/load round-trip. This closes the
  composition gap named first by the 0.12.0 review: Slice 2
  systems are now shown working together, not only in isolation.
- Demo evidence this run: effective damage 24, core damage 29,
  shields cap 105, speed 6.3 m/s; mill built and 1 block
  excavated by tick 4; squad intact at 3 members / 90 HP through
  tick 19; 2 enemies killed, player alive; squad survives the
  save/load round-trip (1 squad loaded); Slice 2 save JSON
  43,891 chars. Slice 1 demo unchanged: gathered 1, built 1,
  killed 2, save JSON 43,864 chars.
- Added 9 composed-integration tests: loot name, skilled damage
  20 -> 24 and core damage 20 -> 29, shields cap 85 -> 105, mill
  build, squad deploy, snapshot count, and save/load keeping the
  squad — all through one Simulation, mirroring the demo.
- Tests: **263 passed, 0 failed** (was 254; +9). Next in order:
  (2) the `GameManager.LoadFromJson` in-place restore, then (3)
  a doctrine menu / rally picker replacing provisional O. Slice 3
  is still NOT scoped.

## 0.12.0 — 2026-10-29 — Slice 2 review as a whole (documentation only)

- Added `docs/slice-02-review.md`: a headless review of all
  eight Slice 2 steps (0.4.0 loot generation, 0.5.0 skill
  cluster, 0.6.0 weapon integration, 0.7.0 skill application,
  0.8.0 Kinetic automation, 0.9.0 Orbital prototype, 0.10.0
  squad persistence, 0.11.0 Orbital view), with the integration
  point and test evidence for each, the deliberately-not-done
  list, and gaps in priority order. Verdict: Slice 2 is reviewed
  and closed as tested, integrated core systems — not
  playtested, not balanced, not Unity-verified.
- Review findings, not new systems: the headless demo still
  does not exercise any Slice 2 system (it is unchanged since
  Slice 1, which proves no regression but not composition);
  `GameManager.LoadFromJson` still only logs a load instead
  of restoring `Simulation.World` in place; no Unity
  verification exists yet for Slice 1 or Slice 2; Slice 2
  numbers are first-pass, untuned values. Next in order: (1)
  a Slice 2 demo scenario, (2) the LoadFromJson in-place
  restore, (3) a doctrine menu / rally picker replacing
  provisional O. Slice 3 is NOT scoped by this review.
- Corrected doc debt found by the review: `docs/
  slice-01-playtest-gate.md` still said loot was "NOT
  integrated into Simulation" — true at 0.4.1, false since
  0.6.0. That bullet now says the integration happened in
  0.6.0, so the gate doc no longer contradicts this changelog.
- No code changed in this step, by design: a review must not
  smuggle in a system. Tests re-run unchanged: **254 passed,
  0 failed**. Demo unchanged: gathered 1, built 1, killed 2,
  save JSON 43,864 chars.

## 0.11.0 — 2026-10-29 — Slice 2 Step 8: Orbital view snapshot + Unity adapter

- Added `OrbitalView.Snapshot(world)` in `src/Forgeborne.Core/
  Orbital.cs`: a copied, read-only snapshot of every live squad
  (position, rally, health, living members, damage per tick,
  at-rally / moving state, doctrine, kind), in deploy order.
  It is a copy, not live references, so rendering cannot mutate
  core state and a pre-tick snapshot keeps its pre-tick position.
  Dead squads never appear (Advance already removes them).
- Added Unity adapter `ForgeborneUnity/Assets/Scripts/
  OrbitalSquadDisplay.cs` (NOT compiled headlessly, to be opened
  in Unity 6000 on Windows): renders the snapshot as one text
  line per squad, or `ORBITAL: NO SQUADS (press O to deploy)`
  when empty. No squad logic lives in the adapter.
- Provisional Unity command in `FpsInputAdapter`: **O** deploys
  a Vanguard squad under HoldGround at the player's current
  position, so it holds and fires there. This binding exists so
  the view can actually be verified in Unity; it is reversible
  and will be replaced by a doctrine menu / rally picker later.
- Added `docs/orbital-view.md`: wiring steps, what the snapshot
  carries, the provisional O key, and a Windows Unity verification
  checklist (including the honest note that `GameManager.
  LoadFromJson` still only logs a load and does not yet replace
  `Simulation.World` in place, so the save-then-view step may
  surface that as the next follow-up).
- Still deliberately NOT in this step: 3D squad markers or an
  Orbital camera, a doctrine menu or rally picker, deployment
  cost or a squad cap, and enemy AI targeting squads. With the
  view in place, Slice 2 can now be reviewed as a whole before
  Slice 3 is scoped.
- Tests: **254 passed, 0 failed** (was 235; +19). Demo unchanged:
  still gathered 1, built 1, killed 2, save JSON 43,864 chars.

## 0.10.0 — 2026-10-29 — Slice 2 Step 7: Orbital squad save / load persistence

- Squads are no longer session-only: `SaveData` gains a `Squads`
  list, one entry per squad — position, rally, health, doctrine,
  kind — in the same int-array style as the existing enemy entries,
  and `SaveSystem.LoadZone` restores them. A wounded squad keeps
  its member count (30 HP = 1 member still firing) and keeps
  moving toward its rally on the first tick after load.
- `TacticalSquad` gains a load constructor with restored health,
  clamped to 0..90, so a corrupt save cannot create a super-squad.
  The loader skips malformed entries instead of failing the whole
  load: wrong length, out-of-bounds position or rally, unknown
  doctrine or kind, and dead (0 HP) squads, which are not
  resurrected. Old saves without the `Squads` field still load,
  with no squads, as before.
- Still deliberately NOT in this step: deployment cost or squad
  cap, additional doctrines or squad types, enemy AI targeting
  squads, and an Orbital camera / Unity adapter. The next
  candidate is the Orbital view / adapter note.
- Tests: **235 passed, 0 failed** (was 224; +11). Demo unchanged:
  still gathered 1, built 1, killed 2, save JSON 43,864 chars
  (was 43,852; the +12 is the empty `Squads` array in the save).

## 0.9.0 — 2026-10-29 — Slice 2 Step 6: Orbital Tactical doctrine prototype (one doctrine, one squad type)

- Added `src/Forgeborne.Core/Orbital.cs`: `TacticalDoctrine.
  HoldGround` (only doctrine in this step) and `SquadKind.
  Vanguard` (only squad type in this step), as `TacticalSquad`:
  3 members, 30 HP each (90 total), 10 damage per living member
  per engagement tick, engagement range 3 (Manhattan).
- HoldGround behaviour, real-time, one world tick at a time, no
  individual micromanagement and no turn-like pauses: the squad
  moves one voxel per tick toward its rally point (largest-gap
  axis first, ties X then Y then Z, as with Enemy movement),
  refusing steps out of bounds, into solid blocks, or into an
  enemy-occupied cell; it does NOT fire while moving. Once
  holding the rally it fires once per tick at the nearest enemy
  in range, then takes 10 damage per enemy still adjacent to it
  — so a squad that kills its adjacent attacker that tick is not
  hit by it, and firepower steps down 30 -> 20 -> 10 as members
  fall. Dead squads are removed by `WorldZone.Advance`.
- `WorldZone.DeploySquad(rally, ...)` spawns a squad at the
  player's position; out-of-bounds rallies and unknown doctrines
  are rejected (null, no squad added). `Simulation.
  DeployOrbitalSquad(...)` exposes the same command to the
  headless loop. Squads are session-only in this prototype:
  they are NOT yet saved by SaveSystem, and there is no Orbital
  camera / Unity adapter yet — those are the next follow-ups,
  not claimed here. No deployment resource cost yet, also a
  follow-up, not a balance decision.
- Tests: **224 passed, 0 failed** (was 180; +44). Demo unchanged:
  still gathered 1, built 1, killed 2, save JSON 43,852 chars
  (mesher 27 ms this run, timing varies).

## 0.8.0 — 2026-10-29 — Slice 2 Step 5: automation tier 1 (Kinetic only)

- Added `BlockType.KineticMill` (value 7, appended only — earlier
  values unchanged, so old saves still load) and `World.Build`
  support for it: 10 Stone + 5 Wood, same air-target and support
  rules as walls and workstations. A mill is solid and is itself
  not gatherable.
- Kinetic automation, in `WorldZone.Advance` every 5th tick: each
  mill excavates at most one adjacent gatherable block, in fixed
  neighbour order, into the player's inventory, by reusing
  `World.Gather` — so bedrock, walls, workstations, and other
  mills are never excavated, and a full inventory consumes
  nothing. Mills are scanned in chunk-index order, so two mills
  reaching for the same block resolve deterministically (first
  in scan order wins; exactly 1 block, not 2).
- `Simulation.BuildKineticMillAhead` (front-left, mirroring
  `BuildWallAhead`'s front-right) and `WorldZone.
  KineticBlocksGathered` (session counter, not saved) expose the
  system to the headless loop. No separate machine inventory yet:
  tier 1 output goes to the player's inventory, to be revisited
  when digital storage (tier 3) arrives. Tiers 2–4 (electrical,
  digital, arcane) are NOT in this step.
- Tests: **180 passed, 0 failed** (was 154; +26). Demo unchanged:
  still gathered 1, built 1, killed 2, save JSON 43,852 chars
  (mesher timing varies run to run; 86 ms this run).

## 0.7.0 — 2026-10-29 — Slice 2 Step 4: Forge Survivor skill effects applied to gameplay

- `Simulation.ApplySkills(SkillBuild)` (also a third
  `Simulation(seed, equipped?, skills?)` argument) replaces any
  previous build — it never stacks — and applies the 0.5.0
  cluster for real: DamagePct scales weapon damage, CoreDamagePct
  adds on top for the core only, ShieldBonus raises the shield
  cap (85 -> 105 for the full cluster), MoveSpeedPct scales FPS
  speed (6 -> 6.3 m/s with Swift Feet), WorkstationRegenBonus
  adds to workstation regen (5 -> 10 per tick), and GatherPct
  accrues a fractional bonus unit (Scavenger +20% = one bonus
  unit per 5 successful gathers; failed gathers do not advance
  it). Null is rejected and leaves the build unchanged; an empty
  build respecs back to base values.
- Supporting core changes, no duplicate systems: `Player`
  gains `MaxShields` + `SetMaxShields` (which clamps current
  shields if a respec lowers the cap), `FpsController` gains a
  `SpeedMultiplier` / `EffectiveSpeed`, and `WorldZone` gains
  `WorkstationRegenPerTick`. Save/load is unchanged: a loaded
  world still starts at the base 85 cap until skills are applied.
- Damage rounding is away from zero, so a small bonus is visible:
  a 10-damage weapon with +5% deals 11, not 10. Discriminating
  test weapon is again seed-0 loot (20 damage): full cluster
  gives 24 enemy damage and 29 core damage.
- Tests: **154 passed, 0 failed** (was 128). Demo unchanged:
  still gathered 1, built 1, killed 2, save JSON 43,852 chars.

## 0.6.0 — 2026-10-29 — Slice 2 Step 3: GeneratedWeapon integrated into Simulation

- `Weapon.FromGenerated(GeneratedWeapon)`: a generated weapon's
  name, damage, and mag size now become a live weapon — no
  duplicate damage / mag logic, and Simulation fires that Weapon,
  so generated stats are the stats that actually hit.
- `Simulation(seed, equipped?)`: pass a GeneratedWeapon to start
  equipped with it; no argument still gives the Pulse Rifle
  (42 mag / 120 reserve, 10 damage). `Simulation.Equip(...)`
  swaps to a generated weapon mid-run (fresh full mag, 120
  reserve); null is rejected and leaves the weapon unchanged.
- HUD weapon name now follows the equipped weapon
  (upper-cased, as before for the Pulse Rifle), instead of a
  hard-coded "PULSE RIFLE".
- Tests use a deliberately discriminating loot seed (seed 0:
  Titan Long Pulse Rifle, 20 damage, 30 mag) so the integration
  cannot pass by accident on the Pulse Rifle's 10 / 42 stats.
- Tests: **128 passed, 0 failed** (was 111). Demo unchanged:
  still gathered 1, built 1, killed 2, save JSON 43,852 chars.

## 0.5.0 — 2026-10-29 — Slice 2 Step 2: first passive skill cluster

- Added `SkillGraph.ForgeSurvivorCluster`: 6 nodes only, NOT the
  full 1,500-node graph. Travel: Sturdy Hands (+5% damage), Iron
  Skin (+10 shields), Swift Feet (+5% move speed). Notable:
  Forge Heart (+15% damage, +10 shields, requires S1+S2),
  Scavenger (+20% gather, requires S3). Keystone: Corebreaker
  (+25% core damage, +5 workstation regen, requires N1+N2).
- `SkillBuild`: prereq-gated allocation, duplicate/unknown
  rejected, summed effects, Base64 encode/decode round-trip.
  Effects are data only in this step — applying them to
  Player/Weapon/Simulation is the next Slice 2 step, so the
  graph can be tested before it changes gameplay.
- Tests: **111 passed, 0 failed** (was 100).

## 0.4.1 — 2026-10-29 — Workstation real effect + Slice 1 playtest gate doc

- Workstation now has a real, distinct effect: within Manhattan
  distance 3, it restores 5 shields per tick, capped at 85. A wall
  only blocks movement; a workstation repairs shields, so building
  one and defending it is a real decision. Tested: near = 65 -> 70
  shields, far = no restore.
- Added `docs/slice-01-playtest-gate.md`: headless evidence table,
  demo numbers, Unity verification checklist for Windows, and an
  honest pending director playtest section. Slice 1 is NOT claimed
  approved until Rob plays it in Unity and answers the gate
  question.
- Tests: **100 passed, 0 failed** (was 96).

## 0.4.0 — 2026-10-29 — Slice 2 Step 1: procedural loot

- Added `LootGenerator`: deterministic procedural weapons from
  Body / Barrel / Stock / Grip / Sight / Capacitor (element),
  rarity Common to Legendary, affixes by rarity, level-scaled
  damage. Same seed + level always gives the same weapon.
  Enemy drop: 30% chance, deterministic by seed + enemy id.
- Note on continuity: work pauses between user messages because
  this agent only runs on a user message or a scheduled job; it
  does not run continuously in the background on its own. The
  silence rule (no messages unless absolutely needed) is about
  messages, not about work — but without a scheduled job or a new
  user message, there is no new turn to do work in. See DECISIONS.
- Tests: **96 passed, 0 failed** (was 89).

## 0.3.0 — 2026-10-29 — Slice 1 Step 4: Unity adapters + headless demo

- Added Unity adapter scripts (NOT compiled headlessly, to be opened
  in Unity 6000 on Windows): `VoxelMeshRenderer` (assigns MeshBuilder
  buffers to a Unity Mesh, no duplicate logic), `FpsInputAdapter`
  (WASD, mouse yaw, Space jump, click fire, E gather, Q build, R
  reload), `HudDisplay` (health, shields, objective, ammo, in the
  reference HUD layout), `GameManager` (one Simulation, spawn wave,
  save to JSON). Load-from-JSON in Unity is documented as not yet
  a finished UI flow; headless load is verified in Core tests.
- Added `Simulation.FireAtCore` (10m range, 10 damage, consumes ammo
  even out of range) and a mesher benchmark test.
- Added `demo/Forgeborne.Demo`: headless 60-tick run — gather,
  build, fight, objective, mesher timing, save length. Verified run:
  gathered 1, built 1, killed 2, player died to enemy pressure
  (standing still after tick 20), mesher 198 quads / 792 vertices
  in 74 ms on the second run, save JSON 43,852 chars. Player death
  in the demo is pressure working, not a bug — movement and building
  are the counterplay, and the demo deliberately stops moving.
- Tests: **89 passed, 0 failed** (was 86). Mesher benchmark:
  13 ms for a generated chunk in tests (< 500 ms gate).

## 0.2.2 — 2026-10-29 — Simulation loop + silence rule

- Added `Simulation`: headless playable loop (FPS controller,
  world, weapon, objective) — move, gather, build, fire, kill,
  objective complete — tested headlessly as the build-around for
  no playable Unity build in this environment.
- Fixed a test-sequence support bug (not core logic): building a
  wall on a just-gathered cell correctly returned Floating, so
  `BuildWallAhead` now targets front-right, supported by ground.
- Communication rule recorded: no messages unless absolutely
  needed, until done, or in direct response to Rob. Progress is
  recorded here, not in chat.
- Tests: **86 passed, 0 failed** (was 78).

## 0.2.1 — 2026-10-29 — Build-around principle + MeshBuilder

- Working principle recorded (Rob): "I can't because…" is not a
  reason. Build around it; if you can't, build around the reason
  why, then build around that. See `DECISIONS.md`.
- Added `MeshBuilder`: converts greedy quads to Unity-ready mesh
  buffers (vertices, normals, UVs, indices) in pure C#. This builds
  around Unity / GPU not being available headlessly: mesh data is
  verified here, Unity adapter later only assigns the buffers.
- Tests: **78 passed, 0 failed** (was 70). Single voxel: 24 vertices,
  36 indices, unit axis normals, all indices in range. Generated
  world: 748 vertices, 1122 indices for 187 quads.

## 0.2.0 — 2026-10-29 — Slice 1 Steps 2-3: mesher, FPS, weapon, HUD

- Added `Mesher.GreedyMesh`: standard greedy rectangle merging.
  Verified: single voxel 6 quads, 2x2x2 cube 6 quads, flat 32x32
  slab 6 quads, generated world 187 quads vs 2608 naive faces.
  Empty chunk 0 quads. Quads carry axis, slice, bounds, type, and
  facing, with 4 corners.
- Added `FpsController`: speed 6, jump 8, gravity -20, AABB
  0.3 half-width x 1.8 height, axis-separated movement with wall
  sliding, grounded detection. Fixed a real tunnelling bug found
  by tests: a large fall step (4.4 -> 2.8) previously snapped to
  Y=3 instead of ground top Y=4; landing now snaps to the highest
  solid block at or below the previous feet position.
- Added `Weapon` (Pulse Rifle, 42 mag / 120 reserve, damage 10,
  fire / reload) and `HudState` (health, shields, DESTROY CORE
  objective distance and percent, ammo, enemy count, 32x32
  mini-map solid data). Core objective added to `WorldZone`
  (core at 28,5,28, health 100, DamageCore).
- Fixed a test arithmetic error (not code): reload reserve is
  120 - 42 = 78, not 79.
- Tests: **70 passed, 0 failed** (was 42). Still no Unity scene,
  no GPU rendering, no Orbital mode — those remain later steps,
  not claimed here.

## 0.1.0 — 2026-10-29 — Slice 1 Step 1: deterministic voxel core

- Added `src/Forgeborne.Core`: 32^3 chunk, deterministic seeded
  generation (bedrock, stone, ~4% EmberOre, sparse wood), gather
  (stone / ore / wood, bedrock ungatherable, inventory-full does not
  consume the block), build (wall 2 stone, workstation 10 stone +
  5 ore, support rule, no floating builds), player (100 HP, 85
  shields, shields-first damage, death at 0), enemies (move toward
  player, attack adjacent for 10, death removes), waves, and JSON
  save / load (chunk Base64, player, inventory, enemies with health,
  seed, tick).
- Added `tests/Forgeborne.Core.Tests`: headless console test runner,
  no external packages. Verified 2026-10-29 with .NET 8 SDK
  (8.0.427, installed locally in the dev environment, not in the
  repo): **42 passed, 0 failed.**
- Added `ForgeborneUnity/` skeleton: Packages manifest (Entities,
  URP, Input System), ProjectVersion placeholder, and an integration
  note. NOT compiled in the Linux headless environment; to be opened
  in Unity 6000 on Windows. Canonical state remains
  `Forgeborne.Core.WorldZone`.
- Known limits of this step: no Unity scene, no FPS input, no voxel
  meshing / rendering, no HUD, no Orbital mode. Those are Slice 1
  Steps 2-4, not claimed here.

## 0.0.2 — 2026-10-29 — Delegation, engine, art direction

- Director delegation recorded: Rob authorised Ember to make all
  future project decisions, with months acceptable, no rushing.
  Decisions are logged in `DECISIONS.md` for review / revocation.
- Engine locked for Slice 1: Unity 6000 (Unity 6) + Entities / DOTS,
  URP, C#. Rationale in `DECISIONS.md` and `README.md`.
- Art direction added (`docs/art-direction.md`) from the
  director-provided reference image. Reference is style only, not a
  shipped asset. All in-game art must be original.
- Licence: no open-source licence added at foundation stage.
  Repository is public for transparency; contents remain WPAI
  property, all rights reserved, until a release licence is chosen.

## 0.0.1 — 2026-10-29 — Foundation

- Repository created manually by Rob at
  https://github.com/MrWizard94-Compile/Forgeborne.git (public, main).
  API creation attempt by Ember failed with 403; see `DECISIONS.md`.
- Initial foundation commit: README, WPAI Constitution project pointer,
  MANIFEST, DECISIONS, CHANGELOG, architecture summary, and Slice 1 plan.
- No playable game, no engine choice, and no performance claim in this
  version. Foundation only.
