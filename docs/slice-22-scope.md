# Forgeborne — Slice 22 Scope

**Status: SCOPED 2026-10-31, under delegation and
under the Slice 21 review's ranked candidates.
No Slice 22 system is built by this document.
Each step below is built one at a time, with
tests, in this order, and Slice 22 is closed by
its own review — it is not closed by finishing
step 1 alone.**

## Scope question

Slice 21 decided repositioning: a live
HoldGround or Patrol squad gets a free
general re-rally, stuck or not, and an
Advance or Defend Core squad refuses it
with nothing changed. A re-rallied squad
is still on the field, still counts toward
the cap of 3, and still costs nothing to
keep. What Slice 21 deliberately did not
touch — and what every earlier slice left
open — is the other half of the field
question: removal. As the code stands, the
only way a squad leaves the field is death.
A player who deployed three squads and
wants a different third squad, or who
deployed a squad in the wrong place and
would rather have the cap slot than the
squad, has no player action for that at
all: the cap slot is held until something
kills the squad, and the deploy price is a
sunk cost with no refund on death or
otherwise.

Slice 22 is therefore the removal slice,
kept as small as the slices before it:
**one removal decision, decided as its own
step with its own tests, then its own
review** — not a general squad-command
system, not a reserve or bench system, not
a radius retune, not a movement,
targeting, or fire redesign, not a HUD
redesign, and not a claim of Unity
verification made from a headless run.

## Inputs this scope is decided from

- The Slice 21 review's ranked candidates, in
  order, and what this scope does with each:
  the patrol route's remaining questions
  (candidate 1) are Slice 22's work, in the
  narrowed form below: the recall or
  dismiss decision first, with the radius
  and waypoint values carried, not
  re-decided, for the reason given under
  "Radius and waypoints" below. The Slice 13
  remainder observations (candidate 2: the
  vertex-colour material observation, the
  MeshRenderer wiring observation, and the
  marker-occlusion observation, plus the
  dusk and view model observations in items
  33 and 34, plus the tint, count, patrol,
  and re-rally observations in items 35, 36,
  37, 39, and 40) remain observations to be
  made when Windows verification runs, not
  steps to be re-scoped blindly — if an
  observation fails, that failure is a fix
  step with its own decision, not a reason
  to re-scope the proof layer. First-pass
  graphics values (candidate 3: dusk
  azimuth, fog contrast, view model offset,
  the static weapon-agnostic model) are
  retuned only on Windows evidence, not
  re-decided headlessly here: no Slice 22
  step moves a light, fog, or model value
  to make a removed squad read better.
- The Slice 21 review's other gaps, in
  priority order, and what this scope does
  with each: Windows Unity verification
  (gap 1) and the director playtest (gap 2)
  remain open, but neither is Slice 22 work
  in itself — the first is blocked on Unity
  6000 on Windows, the second on the
  director — and scoping a slice does not
  close, reorder away, or pretend to answer
  either. First-pass graphics values (gap 3)
  are candidate 3 above. The Slice 13
  scene-proof observations (gap 4) are
  candidate 2 above. The patrol route's
  remaining questions (gap 5) are Slice 22's
  work in the narrowed form below: the
  removal decision, with a general
  re-rally price on Windows playtest
  evidence (if free repositioning proves
  abusive kiting) carried as a later
  decision, not set here — Step 1 must not
  reprice a re-rally to pay for a removal,
  or price a removal to punish re-rally
  use, without saying so and justifying it
  as its own economy decision.
- Ground facts this scope is decided from,
  verified this run against the code as it
  stands: there is NO live-squad removal
  API in core — a search of the core
  sources finds no Recall, Dismiss,
  RemoveSquad, or TryRemove entry point,
  and the only squad removal in the game is
  death: `WorldZone.Advance` ends with
  `Squads.RemoveAll(s => s.IsDead)`, so a
  squad at 0 health leaves the list on the
  next world tick, and only then does its
  cap slot free, as Slice 11 proved. The
  economy is fixed around the question:
  10 EmberOre per squad of any kind
  (`SquadEconomy.EmberOreCost`), cap 3
  concurrent live squads
  (`SquadEconomy.MaxConcurrentSquads`),
  `LiveSquadCount` counting only squads
  that are not dead, deploy refused at the
  cap or when the player cannot pay, with
  check-before-charge so a refused deploy
  charges nothing. The picker
  (`OrbitalCommand.Confirm`) deploys
  through that paid path, and since Slice 18
  Step 2 there is no public free spawn
  primitive at all — the internal
  `SpawnSquad` seam is friend-assembly
  only, for tests. The re-rally gates
  (`WorldZone.TryReRallyStuckPatrol` and
  `WorldZone.TryReRallySquad`, each with a
  thin `Simulation` passthrough) target a
  squad by its order in the squad list
  (snapshot order) and refuse fail-closed
  with nothing changed, in gate order:
  index in range, squad live, doctrine
  eligible, and (for the stuck gate) stuck,
  plus rally in bounds and different from
  the old rally. The next free
  playtest-gate checklist number is 41
  (items run to 40, the general re-rally
  observation, which EXTENDS item 39).
  The proposed K binding for re-rally is a
  comment in `FpsInputAdapter.cs` only, not
  implemented, and K is therefore taken by
  proposal even though it is free against
  the implemented keys.

## Recall, dismiss, or one removal by another name

Step 1 must name the action before it
prices it, because the two names promise
different things and the game currently
has neither:

- A "recall" promises a return: a squad
  called back to somewhere — to the player,
  to the core, to a reserve it can be
  deployed from again. No reserve exists,
  no redeploy-from-reserve rule exists, and
  no squad has a home cell to return to
  except its rally, which a re-rally already
  moves. A recall that simply despawns the
  squad is a dismiss wearing a kinder name,
  and Step 1 must say so if that is what it
  decides, rather than letting the name
  imply a reserve the game does not have.
- A "dismiss" promises an ending: the squad
  leaves the field and is gone, its cap
  slot freed, its deploy price dealt with
  by the refund answer. That is an honest
  name for a despawn, and it is the name
  this scope uses as its default — but Step
  1 may still decide "recall", or two
  distinct actions, if it justifies the
  distinction with a mechanism, not a word.
- A reserve or bench is a second system,
  not a name: stored squads, a bench cap,
  a redeploy price, a bench save format.
  If Step 1 finds that removal is
  meaningless without a reserve, it must
  say so and defer itself to a reserve
  slice, not invent a bench quietly inside
  a removal step. The scope question is
  removal from the field; what, if
  anything, happens to a removed squad
  after it leaves is part of the answer
  Step 1 must state explicitly, even if
  the answer is "nothing — it is gone."

## Refund and cap: the two-gifts problem

Slice 20 recorded this interaction and
Slice 21 restated it, and Step 1 must
answer it, not rediscover it mid-proof:
a removal that both refunds the deploy
price AND frees a cap slot gives the
player two gifts for one action — the ore
back and the room to spend it on a new
squad. That may still be the right answer,
but it is not a free answer, and neither
is its opposite:

- Refund none: removal frees the slot and
  nothing else. The deploy price stays a
  sunk cost, exactly as on death, and the
  only gain is the slot. This is the
  smallest economy change, and Step 1 must
  weigh it against the case that motivates
  removal at all — a wrong deploy the
  player wants undone, not merely
  vacated.
- Refund full 10: removal returns the whole
  deploy price and frees the slot. The
  player can then redeploy at once, which
  makes a deploy freely reversible — and
  Step 1 must say whether free
  reversibility breaks anything the sunk
  cost was protecting, such as the cap
  being a commitment rather than a menu,
  or a deploy-then-recall loop farming
  nothing (there is nothing to farm if the
  refund is exactly the price, but the step
  must check that claim, not assume it).
- Refund partial: a defined fraction or
  flat amount, or a health-based amount
  (a damaged squad worth less, a full
  squad worth more). A partial refund must
  be defined exactly — how much, rounded
  how, on what basis — because "partial"
  without a number is not a decision, and
  a health-based refund must say what
  health means for a squad whose members
  share one health pool (Vanguard 90,
  Recon 40, Heavy 100 at full strength).
- Cap effect, stated separately from
  refund: a removed squad frees its cap
  slot, or it does not. A removal that
  frees no slot would leave the player
  with fewer squads and no room for a new
  one — a rule so close to a bug that
  Step 1 must justify it explicitly if it
  decides it, and prove the cap count
  either way. The natural answer is that
  removal frees the slot, proved by a
  redeploy succeeding immediately after a
  removal at a full cap — but "natural"
  is not a proof, and Step 1 must run it.
- Refund timing and failure: a refund, if
  any, is paid at once into the player's
  EmberOre inventory, or it is not a
  refund the player can use. There is no
  cannot-pay failure for a removal the way
  there is for a deploy, and Step 1 must
  not invent a debt or a negative balance
  to make the question harder than it is.

## Confirmation: where a destructive action is confirmed

Removal is irreversible in a way a
re-rally is not: a re-rallied squad can be
re-rallied back, a removed squad cannot be
un-removed without paying again (if a
refund was given, the player still pays a
new deploy price, or a new price if the
refund was partial or none). Step 1 must
decide where confirmation lives, and be
honest about what headless tests can and
cannot prove about it:

- If confirmation lives in core, Step 1
  must define the state: a pending
  removal, what selects it, what confirms
  it, what cancels it, whether it expires
  (on tick, on another action, never), and
  what a second removal request does while
  one is pending. Core confirmation is
  headless-testable, and Step 1 must test
  it — including cancel leaving the squad,
  the ore, and the cap exactly as they were.
- If confirmation lives in the adapter
  only (a double key, a hold, a picker
  confirm step), Step 1 must say so
  plainly, and must not claim core tests
  prove it: the core API would then remove
  on a single call, and the safety would
  be a Windows observation in the new
  checklist item, not a test count. The
  adapter proposal is a comment only, as
  with K, until a later adapter step
  implements it.
- Either way, Step 1 must state the
  fail-closed cases for the confirm step
  itself: confirming with no pending
  removal, cancelling with none pending,
  and a pending removal whose squad dies
  before confirmation — each with nothing
  changed except what the decision
  justifies and proves.

## Scope, targeting, and timing

- Scope per doctrine and kind: Step 1 must
  say whether removal applies to every live
  squad of every doctrine and kind, or to
  a narrower scope it justifies. Unlike
  re-rally, there is no inert-rally
  question here — removal does not consult
  a rally, so no doctrine is inert to it —
  but Step 1 must still prove the scope it
  claims rather than asserting "all" and
  testing one: a stuck Patrol removed as
  an alternative rescue, a Holding squad
  removed mid-hold, a marching squad
  removed mid-march, a chasing Advance
  squad, and a returning Defend Core squad
  are different cases, and the step must
  name which it proves. All three kinds
  must be proved, or a stated reason why
  kind does not matter here (kinds differ
  in health, damage, and range, not in
  list membership, and the step must say
  whether that is enough to prove one kind
  or all three, including where a damaged
  squad's health changes a refund, if the
  refund is health-based).
- Targeting: Step 1 must decide the rule
  and record it — the re-rally index rule
  (squad list order, snapshot order) is the
  standing rule for a squad action, and a
  removal that used a different rule would
  need to justify why two squad actions
  name squads differently. If it reuses
  the index rule, it must prove the
  removal gate and the re-rally gates
  cannot disagree about which squad an
  index names, including after a removal
  shifts the list: removing index 1 must
  not silently retarget a later re-rally
  meant for the old index 2, and Step 1
  must say whether that shift is a player
  responsibility, a snapshot-refresh rule,
  or a reason for a different identity.
  Refusals, each with nothing changed: a
  dead squad, out-of-range and negative
  indices, an empty squad list, and a
  removal attempted while the picker is
  open, if Step 1 decides picker-open is a
  refusal rather than an independent state.
- Timing against the world tick: Step 1
  must decide whether removal is immediate
  (the squad is gone before the next tick)
  or next-tick (the squad acts once more,
  then leaves), and prove the tick
  interaction it claims — in particular,
  a squad removed while it would have
  fired, and a squad removed while an enemy
  would have fired at it, must not leave a
  half-applied tick behind. A removal that
  is immediate in core but next-tick in
  the adapter's confirm step is two timings,
  and Step 1 must name both.
- Interaction with the re-rally gates:
  removal changes nothing about re-rally
  eligibility, price, or effect, unless
  Step 1 justifies a change and proves it.
  The Slice 20 and Slice 21 proof sets for
  the stuck and general cases must still
  pass unchanged, or Step 1 must say
  explicitly what it changed and why. A
  stuck Patrol that is removed instead of
  re-rallied is a different player choice,
  not a revision of the rescue: Step 1
  must say whether removal is available to
  a stuck Patrol at all, and if it is,
  whether the stuck rescue stays free and
  available beside it, so a player is never
  forced to pay (in ore or in a lost squad)
  for a rescue the earlier slices gave
  free.
- Removal is not death, and Step 1 must
  keep that distinction visible: a removed
  squad is not killed, so no death counter
  (if any exists — Step 1 must verify
  whether any squad-death counter exists
  in core, rather than assuming none does)
  increments, no death-triggered effect
  fires, and no "killed" reading appears in
  the snapshot. The snapshot after a
  removal simply no longer lists the squad;
  Step 1 must verify that reading from
  `OrbitalView.Snapshot` code, not assume
  that a missing list entry renders as
  nothing rather than as a stale marker.

## Radius and waypoints, carried not re-decided

`PatrolLoopRadius` is 3 and
`PatrolWaypointCount` is 5, a flat loop at
rally height: the rally, then 3 east, south,
west, and north of it, in that order. Both
are first-pass values from Slice 15 Step 1,
not balance claims. Slice 22 does NOT
retune either value headlessly, and the
reason is unchanged from Slices 20 and 21:
no step has found a stuck case whose cause
was the radius itself — the stuck cases
were blocked cells, not a loop that could
not fit where it was placed — so there is
still no evidence behind a headless retune.
A removal moves no loop, changes no
centre, and proves nothing about radius;
a Step 1 finding that the radius caused a
block is recorded as a gap for a later
radius decision, not acted on by widening.
A Windows observation that a loop does
not fit its ground is the other evidence
route, and it is likewise recorded, not
acted on headlessly here.

## Slice 22, in order

### Step 1 — Removal decision

Decide what removal, if any, the player
has for live squads, decided in that step
— not in this scope document — with the
reasoning recorded in DECISIONS.md:

- The step must decide the name and the
  mechanism, and record them: recall,
  dismiss, or one action under a justified
  name; a live-squad removal API in core
  (with its gate, in gate order) and a
  thin `Simulation` passthrough, following
  the re-rally pattern, or a different
  shape it justifies; and whether a
  reserve exists (it does not, unless the
  step defers itself to a reserve slice).
- The step must decide the scope, and
  record it per doctrine and kind, with
  the stuck-Patrol interaction stated
  explicitly (removal available beside a
  free rescue, or refused for stuck
  squads, with reason), and with the
  in-combat, marching, holding, chasing,
  and returning cases named, not assumed
  under "all squads".
- The step must decide the refund and the
  cap effect, and record them as two
  separate answers: refund none, full 10,
  or a defined partial (with its number,
  rounding, and basis, if health-based),
  paid at once; and cap slot freed or not,
  proved by a redeploy at a full cap after
  a removal. A step that answers only one
  of the two has not decided removal.
- The step must decide confirmation, and
  record where it lives (core state or
  adapter only), with its pending,
  confirm, cancel, and expiry rules, and
  with the honesty note about what core
  tests prove and what only a Windows
  observation proves.
- The step must decide targeting and
  timing, and record them: index rule
  reused or replaced, with the
  post-removal list-shift answer; refusal
  classes with nothing changed; immediate
  versus next-tick removal, with the
  firing-tick interaction proved.
- The step must decide the state changes,
  and record them: the squad leaves the
  squad list; the snapshot no longer lists
  it; no other squad's position, health,
  doctrine, kind, rally, waypoint index,
  or refusal counter changes; no death
  counter increments (verified, not
  assumed); the save after a removal
  contains no entry for the removed squad,
  and an old save (taken before any
  removal) still loads unchanged, since no
  new save field is added unless the step
  justifies one and proves old saves still
  load.
- The step must decide the proof set, and
  record it: a removal for each doctrine
  (HoldGround, Patrol, Advance, Defend
  Core) and each kind, or a stated reason
  why kind does not matter; a stuck
  Patrol removed (or refused, as decided)
  beside a stuck Patrol re-rallied free,
  so the two choices are visibly distinct;
  a removal at a full cap followed by a
  successful paid redeploy at 10 ore, with
  ore and cap counts shown before and
  after each step; a removal refused for a
  dead squad, out-of-range and negative
  indices, and an empty list, each with
  nothing changed; a confirmation proof
  (cancel leaves everything unchanged, and
  a pending removal whose squad dies is
  handled as decided) if confirmation lives
  in core; a save/load round-trip (a
  removal persists — the squad does not
  return on load — and a pre-removal save
  loads with the squad still present);
  the paid deploy path unchanged (price
  10, cap 3, picker Confirm still the only
  play path); and the re-rally gates
  unchanged (a re-rally before and after a
  removal behaves as decided in Slices 20
  and 21, with the list-shift answer
  proved). In every case: no movement,
  targeting, fire, delivery, precedence,
  conflict, rating, cost, or phase rule
  moves except the removal itself and its
  explicitly decided refund and cap
  answers.
- The step must decide the movement
  boundary, and record it: no movement
  rule moves in this step, under any
  hypothesis. `MoveTowardRally`,
  `MoveForPatrol`'s sidestep, skip, and
  Stuck threshold, Advance's nearest-enemy
  chase, and Defend Core's chase-or-return
  stand; no pathfinder is added; no hold
  replaces a skip; no rally fallback is
  added for Advance, and no rally
  consultation is added for Defend Core.
  A removal that teleported a squad to
  the player before despawning it would
  not be a removal at all, and a removal
  staged as lethal damage in a test would
  be a death, not a removal proof — the
  staged deaths in Slices 20 and 21 were
  cap-staging devices, and Step 1 must not
  quote them as removal evidence.
- The step must decide the checklist item,
  and record it: a new checklist item
  (from 41) observing the decided removal
  on Windows, and it must say whether that
  item supersedes, extends, or leaves
  standing item 40's general re-rally
  observation, item 39's stuck-patrol
  observation, item 37's patrol-route
  observation, and item 29's and
  orbital-command item 13's patrol
  observations. If the removal reuses a
  binding near K, the step must say
  whether item 40's observation already
  covers it or a new item is still needed
  for the removal readings; if it proposes
  a different binding, the new item
  observes that binding. The step must
  state its verification honestly: like
  Slices 20 and 21, this step has a
  headless-testable claim — squad lists,
  ore, and cap counts before and after a
  removal, for staged layouts — so it adds
  tests for every case in the proof set
  above, and the test suite must rise, not
  stand, with no failures. No demo scenario
  is required of this step alone; the
  composed demo belongs to Step 2, which
  stages the decided removal together with
  its controls.
- The step must decide the display
  relation, and record it: no HUD line is
  added, moved, or reworded in this step,
  and no new snapshot word is added unless
  the step justifies one and proves it.
  The existing squad markers render the
  snapshot; a removed squad's marker must
  go away with its snapshot entry under the
  existing marker rule, which the step must
  verify per doctrine rather than assume —
  in particular, it must say what a player
  is supposed to read when a marker simply
  vanishes, and whether that vanishing
  needs any HUD word at all, or whether a
  HUD word would be a HUD decision smuggled
  inside a removal step.
- The step must decide the adapter binding
  proposal, and record it as a Windows
  checklist item, not as a headless proof:
  the proposed K binding for re-rally (K
  while not picking, crosshair on an
  eligible squad's marker, picker raycast
  rally reuse, Return to confirm, Escape
  to cancel) is a comment in
  `FpsInputAdapter.cs` only, not
  implemented, and K is therefore taken by
  proposal. A removal binding must be
  checked against the taken keys (WASD /
  Space / E / Q / R / O / Return / Esc /
  Alpha1-7 / T / Y / U / I / B / N / P / M /
  mouse click) and against the K proposal:
  a different key, a K-plus-modifier, or a
  K-then-confirm sequence, with the reason
  why, and with the fail-closed cases
  stated (doing nothing while picking,
  nothing when the raycast hits no
  eligible marker, nothing when the target
  squad refuses the action, nothing on a
  cancelled confirmation). No binding is
  claimed as working in a scene from a core
  test.

### Step 2 — Slice 22 demo + review

As with 0.34.0, 0.38.0, 0.43.0, 0.51.0,
0.55.0, 0.60.0, 0.63.0, 0.66.0, 0.76.0,
0.80.0, 0.83.0, 0.86.0, and 0.89.0 — and
unlike Slice 16's review, which was
documentation-only because no graphics
claim is headless-provable — this review
IS accompanied by a new composed demo
scenario, because the Step 1 removal is
headless-testable and a demo that stages
it together with its controls proves
something a step's isolated tests do not:
that the decided removal, the untouched
movement rules, the untouched re-rally
gates, and the untouched economy agree in
one field. The demo scenario, decided in
that step, must stage in one Simulation,
at minimum: a removal at a full cap
followed by a successful paid redeploy, so
the freed slot is visible; a stuck Patrol
re-rallied free beside a squad removed,
so the rescue and the removal are visibly
distinct choices; for each doctrine Step 1
covers, a removal with nothing else
changed for the squads that remain; for
each refusal class Step 1 decides, a
refusal with nothing changed; and a
save/load round-trip proving the removal
persists as decided (the removed squad
does not return on load, a pre-removal
save still loads with the squad present) —
with the paid deploy path exercised beside
it, so a reader can see the cap and price
did not move to accommodate a removal.
Composed-integration tests mirroring the
demo must be added, and the test suite
must rise with no failures. The review
writes `docs/slice-22-review.md`, checks
the slice against this scope step by step,
states the verdict honestly (one removal
decision with tests, not playtested, not
Unity-verified), separates headless-tested
removal claims from the Windows-checklist
removal observation, carries the gaps that
remain, and does NOT scope Slice 23 —
Slice 23 is scoped afterwards, the same way
this document follows the Slice 21 review
rather than preceding it. Every earlier
demo save length must stand; the new
scenario's save length is quoted as its own
entry, not as a change to an old one.

## Deliberately NOT in Slice 22

- A general squad-command system.
  Selecting, ordering, or commanding squads
  beyond the one decided removal action —
  including a follow order, a formation
  command, or a general order queue — is a
  larger slice, carried after Slice 22,
  not patched in beside a removal.
- A reserve, bench, or redeploy-from-reserve
  system of any kind, unless Step 1 defers
  itself to a reserve slice and says so.
  No step in this slice adds a stored-squad
  list, a bench cap, or a redeploy price
  quietly afterwards, under any name, and
  a removal must not be presented as a
  recall-to-reserve substitute: a removed
  squad is gone, unless Step 1 decides and
  proves otherwise as a named, larger slice.
- A radius, waypoint-count, or loop-shape
  change. The loop stays 3 and 5, flat at
  rally height, unless a later decision on
  recorded evidence changes it — see
  "Radius and waypoints" above.
- A movement, targeting, or fire rule
  change. `MoveTowardRally`,
  `MoveForPatrol`'s sidestep, skip, and
  Stuck threshold, Advance's chase, and
  Defend Core's chase-or-return stand; a
  remaining squad still fires under its
  doctrine's existing rule, and no
  pathfinder, no hold, no wider sidestep,
  and no rally fallback for Advance or
  rally consultation for Defend Core is
  added here.
- A delivery, precedence, conflict, rating,
  cost, or phase rule change beyond the
  economy question Step 1 must answer
  explicitly (a removal refund, or none,
  and its cap effect). The deploy price
  (10) and the cap (3) otherwise stand, and
  no power, consumer, or save-format rule
  moves: a removal deletes a squad entry,
  and no new save field is added unless
  Step 1 justifies one and proves old saves
  still load.
- A re-rally repricing. Slice 22 does not
  charge for a re-rally, free or otherwise,
  to pay for a removal, and does not
  reprice the stuck rescue: the Slice 20
  and Slice 21 answers stand unless Step 1
  justifies a change as its own economy
  decision and proves the old proof sets
  still pass.
- A new block, kind, doctrine, source, or
  consumer, a balance pass, or a price or
  cap change disguised as a removal. Slice
  22 is one removal decision and its
  review, and the core stands exactly as
  Slice 21 left it except for the removal
  itself.
- A material, shader, marker, light, fog,
  or view model change, or a HUD redesign.
  The Slice 16 surface renders whatever
  Slice 22 decides; a step that re-palettes
  a marker or adds a HUD line to make its
  answer look right has confused the
  surface with the removal.
- A scene-proof observation presented as a
  headless result. Items 25–27 and 30–40,
  and the new item Step 1 adds, are Windows
  observations, recorded in DECISIONS.md
  when observed, not closed by a document,
  a script, or a test count.
- A scene file (`.unity`), an imported asset,
  a texture, or a shader file written by a
  headless run and claimed as a built scene.
  Scene assembly remains Windows checklist
  work.
- A copied asset, model, skyline, HUD layout,
  or trademarked look from any other game.
- Windows Unity verification itself. It
  remains open for Slices 1–22 alike and is
  blocked on Unity 6000 on Windows, not on
  anything this slice can do headlessly.
- A Slice 23 scope. Slice 23 is scoped after
  the Slice 22 review, not inside it.

## Ranked candidates for Slice 23 scoping (not promised)

These are recorded here so the Slice 22
review starts from a list, not from scratch.
Order follows the Slice 21 review's gaps
with the removal question taken:

1. The patrol route's remaining questions —
   a radius or waypoint first-pass value
   revisited on evidence (a Step 1 finding
   that the radius caused a block, or a
   Windows observation that a loop does not
   fit its ground) — with its own tests,
   not promised here.
2. Any Slice 13 remainder that Slice 22 does
   not itself settle as an observation (the
   vertex-colour material observation, the
   MeshRenderer wiring observation, and the
   marker-occlusion observation in the Slice
   21 review's gap 4, plus the dusk and view
   model observations in items 33 and 34,
   plus the tint, count, patrol, and
   re-rally observations in items 35, 36,
   37, 39, and 40, plus the new item Step 1
   adds) — if those observations are still
   unmade when the review is written, they
   remain observations to be made when
   Windows verification runs, not steps
   to be re-scoped blindly, and not promised
   here.
3. First-pass graphics values retuned only on
   Windows evidence (dusk azimuth, fog
   contrast, view model offset, static
   weapon-agnostic model) — a retuning step
   is justified by an observation, not by a
   headless re-decision, and is not promised
   here.
4. A general re-rally price on Windows
   playtest evidence, if free repositioning
   proves abusive kiting — a later economy
   decision, recorded as a gap in Slice 21
   Step 1, not set here.

## Slice 22 gate (what "done" means)

- Every step pushed with passing tests (for
  Step 1, with the suite risen by the proof-
  set tests, never standing still on an
  action step, and for Step 2, with the suite
  risen by the composed tests); the Step 1
  removal decision recorded with rationale in
  DECISIONS.md, with its own Windows
  checklist item (item 41, or the number
  Step 1 decides if it finds 41 already
  taken, with the relation to items 40, 39,
  37, 29, and orbital-command item 13 stated).
- No movement, targeting, fire, delivery,
  precedence, conflict, rating, phase, or
  save-format rule changed in any step
  except the removal itself and the economy
  answers Step 1 justifies explicitly (a
  refund, or none, and its cap effect); no
  material, marker, light, fog, model, or
  HUD rule changed in any step except the
  snapshot entry disappearing for a removed
  squad, if Step 1 justifies a change there.
- Every earlier demo save length unchanged,
  with the new Slice 22 length quoted as its
  own entry; a removal that deletes a squad
  entry changes the new scenario's save by
  that entry, and the review must say which
  per-doctrine effects, if any, change a
  save, rather than quoting the suite as
  unchanged.
- This review records the verdict honestly,
  separating headless-tested removal claims
  from the Windows-checklist removal
  observation, including that Slice 22 is
  still not playtested and not
  Unity-verified. The Slice 1 director
  playtest gate is still pending throughout
  and is not closed by Slice 22.
