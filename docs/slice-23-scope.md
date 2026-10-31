# Forgeborne — Slice 23 Scope

**Status: SCOPED 2026-10-31, under delegation and
under the Slice 22 review's ranked candidates.
No Slice 23 system is built by this document.
Each step below is built one at a time, with
tests, in this order, and Slice 23 is closed by
its own review — it is not closed by finishing
step 1 alone.**

## Scope question

Slices 15, 18, 20, 21, and 22 decided a patrol
route piece by piece — the loop itself, the
sidestep and skip and stuck reading, the stuck
rescue, the general re-rally, and the removal
beside the rescue — and every one of those
decisions carried the same two numbers forward
without re-deciding them: a loop radius of 3
and a loop of 5 waypoints. Each carrying step
said the same thing about them, honestly: no
proof in that step showed the radius caused a
block, so no headless retune was justified
there. That honesty has now run out of steps
to hide behind. Either the radius and the
waypoint count are right, and a step should
prove it by auditing every proved stuck case
against them, or one of them is wrong, and a
step should change it with tests — but a
sixth carrying step that repeats "no finding
yet" without doing the audit would not be a
decision at all, it would be a deferral
wearing a scope's clothes.

Slice 23 is therefore the patrol-route
first-pass-values slice, kept as small as the
slices before it: **one radius and waypoint
decision, decided as its own step with its own
tests, then its own review** — not a movement
redesign, not a pathfinder, not a re-rally
repricing, not a dismiss refund, not a reserve
or bench system, not a general squad-command
system, not a graphics retune, not a HUD
redesign, and not a claim of Unity
verification made from a headless run.

## Inputs this scope is decided from

- The Slice 22 review's ranked candidates, in
  order, and what this scope does with each:
  the patrol route's remaining questions
  (candidate 1) are Slice 23's work, in the
  form below: the radius and waypoint values
  revisited on evidence, where the evidence
  is the headless audit Step 1 must run, not
  a Windows observation that has not happened
  yet — and if that audit finds no
  radius-caused block, the decision is KEEP
  with the audit as its proof, not a silent
  carry. The Slice 13 remainder observations
  (candidate 2: the vertex-colour material
  observation, the MeshRenderer wiring
  observation, and the marker-occlusion
  observation, plus the dusk and view model
  observations in items 33 and 34, plus the
  tint, count, patrol, re-rally, and dismiss
  observations in items 35, 36, 37, 39, 40,
  and 41) remain observations to be made when
  Windows verification runs, not steps to be
  re-scoped blindly, and not promised here.
  First-pass graphics values retuned only on
  Windows evidence (candidate 3: dusk
  azimuth, fog contrast, view model offset,
  static weapon-agnostic model) are not
  Slice 23's work: a headless radius decision
  is not a reason to retune a dusk sun. A
  general re-rally price on Windows playtest
  evidence (candidate 4, if free
  repositioning proves abusive kiting) is not
  set here, and the dismiss refund gap
  recorded in Slice 22 Step 1 (if slot-only
  removal proves punishing on Windows
  playtest evidence) is not set here either:
  both are economy decisions waiting on
  playtest evidence that does not exist yet,
  and a radius step must not smuggle either
  price in as a way of paying for a loop
  change.
- The adapter bindings proposed in Slices 20,
  21, and 22 (K for re-rally, X for dismiss)
  are not Slice 23's work. They are comments
  in FpsInputAdapter.cs only, and they stay
  comments through this slice: a radius
  decision must not depend on bindings that
  are not implemented, and implementing them
  is a later adapter step, not a step of this
  slice and not promised here. Until then,
  items 39, 40, and 41 remain honestly
  pending their bindings, as the Slice 22
  review says.

## Ground facts, verified against code this run, not copied from review prose

- The loop is derived, never stored, in
  Orbital.cs: PatrolLoopRadius is a const 3,
  PatrolWaypointCount is a const 5, and
  PatrolTarget is the rally itself at index 0,
  then rally plus 3 east at index 1, rally
  plus 3 south at index 2, rally minus 3 west
  at index 3, and rally minus 3 north at index
  4, all on the rally's own Y plane — a flat
  loop at rally height, not a climb, and a
  loop that returns to its rally between
  circuits because the rally is itself a
  waypoint.
- The radius comment's stated rationale is
  first-pass reasoning Step 1 must re-derive,
  not copy: it says 3 is larger than every
  kind's engagement range except Recon's 5,
  and small enough that a full circuit is 24
  steps or fewer. The range table in code
  (SquadShapes) is Vanguard 3, Recon 5, Heavy
  2 — so the comment overstates the Vanguard
  case, where radius 3 equals the range
  rather than exceeding it. Step 1 must state
  the range relation from the table, not from
  the comment, and must say whether the
  equality for Vanguard changes anything: a
  Vanguard standing on its rally can shoot to
  a waypoint at exactly range 3, a Heavy
  cannot, and a Recon can shoot across its
  own loop. The circuit arithmetic checks:
  rally to east 3, east to south 6, south to
  west 6, west to north 6, north to rally 3,
  totalling 24 steps at radius 3, so the
  "24 steps or fewer" claim is exact at 3 and
  grows by 8 steps per added radius unit on
  this cross shape — a cost Step 1 must
  quote if it proposes a larger radius,
  rather than treating radius as free.
- The stuck rule is coupled to the waypoint
  count by construction, not by coincidence:
  IsPatrolStuck is true when
  PatrolConsecutiveRefusals reaches
  PatrolWaypointCount, so a full circuit of
  refusals is exactly 5 at the current count.
  A Step 1 that changes the count to 4 or 6
  without answering whether the stuck
  threshold moves with it would silently
  redefine what "stuck" means; a Step 1 that
  keeps the count keeps the threshold, and
  must say so. The counter itself resets on
  any successful patrol step (direct or
  sidestep) and on load, and no other
  doctrine reads it.
- An out-of-bounds or solid waypoint is still
  a waypoint, by the PatrolTarget doc:
  MoveForPatrol sidesteps and then skips it
  when its step is refused, rather than the
  derivation pretending such a waypoint
  cannot exist. A waypoint inside a solid
  block is therefore already handled by the
  skip rule, and a waypoint outside the
  ground is handled the same way — so Step 1
  must distinguish three different claims
  that are easy to confuse: a waypoint that
  is skipped (handled, not stuck), a route
  whose every waypoint is refused (stuck,
  proved), and a loop whose shape itself
  causes a refusal (unproved, and the only
  claim that justifies a radius or shape
  change).
- The proved stuck geometries, all from
  placed stone or an enemy-occupied cell, not
  from loop geometry: the route-blocked shape
  (stone west, south, and north of the
  squad's cell, east step cell open — the
  squad counts five refusals, reads Stuck,
  unmoved, then marches on a re-rally); the
  fully enclosed shape (no step cell open —
  the squad re-sticks on a new loop instead
  of marching through a wall); and the
  blocked-loop shape of Slice 21 (a new loop
  itself fully blocked, re-sticking after a
  full circuit). In none of these does the
  radius appear as a cause: the refusals are
  at step cells adjacent to the squad, and
  moving the loop's centre (a re-rally)
  changes the outcome while keeping radius 3
  and count 5 — which is evidence for KEEP,
  not for change, unless Step 1's audit finds
  a case the earlier slices did not stage.
- The economy and the field rules around the
  loop stand and are not this slice's
  questions: deploy price 10 EmberOre, cap 3
  concurrent live squads, re-rally free,
  dismiss refund none with one cap slot
  freed. A radius change creates and destroys
  no squad, charges nothing, and frees no
  slot, so no economy answer rides along
  with it — and if Step 1 finds a radius
  change would need a price to be fair (for
  example, a much larger loop as a paid
  upgrade), that price is a separate later
  decision, recorded as a gap, not set here.

## The radius and waypoint decision: questions, not answers

Step 1 must answer each of these, with tests,
and must not answer any of them by copying a
previous slice's carrying sentence:

- Keep, change, or split: do radius 3 and
  count 5 stay as one pair, does one move
  without the other, or does the radius
  become per-kind (for example, tracking the
  engagement ranges) — and if per-kind, what
  happens to the shared stuck threshold, the
  shared loop derivation, and the snapshot
  words, which currently know nothing about
  kind-specific loops?
- The audit itself: for each proved stuck
  geometry, classify every refusal as
  step-cell-caused (stone or enemy adjacent
  to the squad), waypoint-caused (the
  waypoint cell itself solid or out of
  bounds), or loop-shape-caused (no single
  blocking cell explains the refusal, only
  the loop's geometry does) — and show, for
  the step-cell cases, that removing the
  blocking cells while keeping radius 3 and
  count 5 lets the same squad walk the same
  loop, so the block was the placement, not
  the numbers.
- The missing case, if there is one: stage a
  loop where the radius itself is the only
  difference between walking and sticking —
  for example, a loop whose waypoints fit at
  one radius and not at another with the
  same placed blocks, or a loop near ground
  where a waypoint falls outside the playable
  area at radius 3 but inside it at a smaller
  radius — and if no such case can be staged
  without inventing a new movement rule, say
  so plainly: that failure to stage is itself
  evidence for KEEP, and Step 1 must record
  it as a finding, not hide it by staging a
  placed-stone enclosure again and calling
  it a radius case.
- The coupling answers: if the count moves,
  does the stuck threshold move with it, or
  is the threshold decoupled into its own
  number with its own rationale — and if the
  radius moves, what happens to the circuit
  length (24 steps at 3, plus 8 per added
  unit on the cross shape), to the
  range relation per kind (Vanguard 3, Recon
  5, Heavy 2), and to the flat-loop rule (a
  radius change must not quietly become a
  climbing loop or a differently shaped loop;
  a shape change is a separate decision with
  its own tests, recorded as a gap if Step 1
  finds one wanted)?
- The Windows reopen condition, stated so a
  later reader can tell whether it has been
  met: what observation, on a real map
  rather than a staged enclosure, would
  reopen a KEEP — for example, loops
  routinely failing to fit their ground on
  ordinary terrain, waypoints routinely
  landing inside structures a player did not
  place, or a circuit that takes so long it
  stops feeling like a patrol — and what
  observation would not reopen it (a single
  staged enclosure, a player-built wall
  across a loop, a re-rally to a blocked
  rally: all already answered by the skip
  and stuck rules).
- The comment correction: the radius
  comment's range claim (larger than every
  range except Recon's) is wrong for Vanguard
  as written, since Vanguard's range is
  exactly 3. Step 1 must either correct that
  comment to the table's values or show why
  the wording was meant differently — a
  one-line doc fix is not a radius decision,
  but leaving a known-wrong rationale in the
  code while deciding on the radius would
  be.
- Checklist relation, decided in Step 1, not
  here: if Step 1 keeps the values, no new
  playtest-gate item is needed, because no
  player-visible behaviour changes — the
  existing items 37 (patrol route), 39
  (stuck re-rally), and 40 (general
  re-rally) already observe the loop as kept.
  If Step 1 changes a value, it must add
  its own item from the next free number,
  42, observing the changed loop on Windows
  (a loop walking its new shape, its new
  circuit length felt in play, and its stuck
  reading under the new threshold if the
  threshold moved), stating its relation to
  items 37, 39, and 40 — superseding none of
  them unless it says why — and the next
  free number after that step is 43. A
  keep-decision must not invent an item to
  look like a change, and a change-decision
  must not hide behind the old items.

## Slice 23, in order

- Step 1, the radius and waypoint decision,
  decided in that step, with its own tests:
  the audit, the keep-or-change answer with
  rationale, the coupling answers, the
  comment correction, and the checklist
  relation above. No demo scenario is added
  by this step alone; the composed demo
  belongs to Step 2.
- Step 2, composed demo scenario plus
  documentation-only review closing Slice 23,
  per this scope. No new core system, no
  adapter edit, no checklist item beyond
  what Step 1 decided, by design; Slice 24
  is NOT scoped here.
- Slice 23's Step 2 demo must stage the
  decided values together with their controls
  in one field: a patrol walking a loop at
  the decided radius and count (with the
  circuit length and the range relation
  readable from its path), a stuck geometry
  from the audit beside it (so the review can
  show the audit's classification in one
  field, not just in unit tests), and a
  save/load round-trip preserving the loop's
  centre, so a kept or changed loop is proved
  to persist the same way the rescue and the
  removal already do. If Step 1 keeps the
  values, the demo proves the keep: the same
  loop, the same stuck reading, the same
  rescue — with the audit's classification
  visible, not re-claimed as a new system.

## Deliberately NOT in Slice 23

- A movement, targeting, or fire rule change.
  MoveTowardRally, MoveForPatrol's sidestep,
  skip, and stuck threshold (except as Step 1
  explicitly decides the threshold coupling),
  Advance's chase, and Defend Core's
  chase-or-return stand; no pathfinder, no
  wider sidestep, no hold replacing a skip,
  no rally fallback for Advance, and no rally
  consultation for Defend Core is added here.
  A radius finding that can only be fixed by
  a pathfinder is recorded as a gap for a
  movement slice, not fixed by widening the
  loop and calling it a pathfinder.
- A loop-shape change smuggled inside a radius
  change: a cross loop at a different radius
  is a radius change; a circular, climbing,
  or otherwise reshaped loop is a shape
  decision, and Step 1 must name it as one
  if it proposes it, with its own tests, or
  record it as a gap.
- A re-rally repricing, a dismiss refund of
  any size, a deploy price or cap change, or
  any other economy change. The deploy price
  (10) and the cap (3) stand, the re-rally
  stays free, and the dismiss stays refund
  none with one slot freed.
- A reserve, bench, or redeploy-from-reserve
  system of any kind. No step in this slice
  adds a stored-squad list, a bench cap, or
  a redeploy price quietly afterwards, under
  any name, and a radius decision must not be
  presented as a reserve substitute.
- A general squad-command system. Selecting,
  ordering, or commanding squads beyond the
  decided re-rally and dismiss actions is a
  larger slice, carried after Slice 23, not
  patched in beside a loop audit.
- A material, shader, marker, light, fog, or
  view model change, or a HUD redesign. The
  Slice 16 surface renders whatever Slice 23
  decides; a step that re-paletted a marker
  or added a HUD line to make its answer
  look right would have confused the surface
  with the loop.
- A new block, kind, doctrine, source, or
  consumer, a balance pass, or a price or cap
  change disguised as a loop change.
- An adapter edit or a binding implementation.
  The K and X proposals stay comments in
  FpsInputAdapter.cs through this slice; the
  play path for re-rally and dismiss is
  unchanged, and no headless test proves a
  binding.
- A scene file (.unity), an imported asset, a
  texture, or a shader file written by a
  headless run and claimed as a built scene.
  Scene assembly remains Windows checklist
  work.
- A copied asset, model, skyline, HUD layout,
  or trademarked look from any other game.
- Windows Unity verification itself. It
  remains open for Slices 1–23 alike and is
  blocked on Unity 6000 on Windows, not on
  anything this slice can do headlessly.
- A Slice 24 scope. Slice 24 is scoped after
  the Slice 23 review, not inside it.

## Ranked candidates for Slice 24 scoping (not promised)

These are recorded here so the Slice 23
review starts from a list, not from scratch.
Order follows the Slice 22 review's gaps
with the radius question taken:

1. Any Slice 13 remainder that Slice 23 does
   not itself settle as an observation (the
   vertex-colour material observation, the
   MeshRenderer wiring observation, and the
   marker-occlusion observation, plus the
   dusk and view model observations in items
   33 and 34, plus the tint, count, patrol,
   re-rally, and dismiss observations in
   items 35, 36, 37, 39, 40, and 41, plus
   any new item Step 1 adds) — if those
   observations are still unmade when the
   review is written, they remain
   observations to be made when Windows
   verification runs, not steps to be
   re-scoped blindly, and not promised here.
2. First-pass graphics values retuned only on
   Windows evidence (dusk azimuth, fog
   contrast, view model offset, static
   weapon-agnostic model) — a retuning step
   is justified by an observation, not by a
   headless re-decision, and is not promised
   here.
3. A general re-rally price on Windows
   playtest evidence, if free repositioning
   proves abusive kiting — a later economy
   decision, recorded as a gap in Slice 21
   Step 1, not set here.
4. A dismiss refund on Windows playtest
   evidence, if slot-only removal proves
   punishing — a later economy decision,
   recorded as a gap in Slice 22 Step 1, not
   set here.
5. The adapter bindings implementation (K for
   re-rally, X for dismiss, both currently
   comments in FpsInputAdapter.cs only) as
   its own adapter step, following the Slice
   16 pattern, so that items 39, 40, and 41
   become observable on Windows at all — a
   step whose verification is a Windows
   checklist observation, never a headless
   test count, and not promised here.

## Slice 23 gate (what "done" means)

- Every step pushed with passing tests (for
  Step 1, with the suite risen by the audit
  proof-set tests, never standing still even
  if Step 1 decides KEEP — a keep-decision
  with no new tests is prose, not a decision
  — and for Step 2, with the suite risen by
  the composed tests); the Step 1 radius and
  waypoint decision recorded with rationale
  in DECISIONS.md, with its checklist
  relation stated (no new item for a keep,
  a new item from 42 for a change, with the
  relation to items 37, 39, and 40 stated).
- No movement, targeting, fire, delivery,
  precedence, conflict, rating, phase, or
  save-format rule changed in any step
  except what Step 1 justifies explicitly
  (a radius value, a waypoint count, and the
  stuck-threshold coupling if Step 1 decides
  it moves); no material, marker, light, fog,
  model, or HUD rule changed in any step;
  no economy value changed in any step.
- Every earlier demo save length unchanged,
  with the new Slice 23 length quoted as its
  own entry; a radius or count change that
  moves a patrol's path changes the new
  scenario's save by its positions, and the
  review must say which per-doctrine effects,
  if any, change a save, rather than quoting
  the suite as unchanged.
- This review records the verdict honestly,
  separating headless-tested loop claims
  from the Windows-checklist loop
  observation, including that Slice 23 is
  still not playtested and not
  Unity-verified. The Slice 1 director
  playtest gate is still pending throughout
  and is not closed by Slice 23.
