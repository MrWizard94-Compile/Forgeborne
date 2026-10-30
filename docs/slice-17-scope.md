# Forgeborne — Slice 17 Scope

**Status: SCOPED 2026-10-30, under delegation and
under the spent director queue decision of
2026-10-30 09:27 EDT. No Slice 17 system is built
by this document. Each step below is built one at
a time, with tests, in this order, and Slice 17 is
closed by its own review — it is not closed by
finishing step 1 alone.**

## Scope question

Slice 16 settled the surface the player reads on:
the chunk material that multiplies vertex colours,
the MeshRenderer wiring that keeps a refresh from
silently no-opping, honest marker occlusion, dusk
light with sky and fog, and a first weapon view
model. What Slice 16 deliberately did not touch is
what two display queries say about the wiring
itself. Two readings are still wrong, or at least
still misleading, and both were carried honestly
rather than patched inside a graphics step:

- A single cable cell touching a Conduit reads
  untinted in the scene while its network is live
  — its mill is destroyed on the next tick, its
  pylon fires — because the power-state query
  reads None for that cell.
- A Conduit standing directly beside a mill counts
  as neither a live network nor a lone source,
  because the lone-source rule disqualifies any
  source beside a consumer, whatever it delivers —
  so a player who places a Conduit beside a mill
  sees POWER 0 LIVE, 0 LONE, while the mill reads
  None and gathers at kinetic cadence.

Slice 17 is therefore the display-query slice, kept
as small as the slices before it: **the two
carried Slice 14 remainders, each decided as its
own step with its own tests, then its own
review** — not a delivery redesign, not a new
source, consumer, kind, or doctrine, not a balance
pass, not a material or HUD redesign, and not a
claim of Unity verification made from a headless
run.

## Inputs this scope is decided from

- The director queue decision (Rob, 2026-10-30
  09:27 EDT, in chat — not an Ember decision),
  now spent: graphics was ordered after Slice 15
  and ahead of the remainders, and Slice 16 built
  it. The Slice 16 review's verdict is explicit
  that the queue decision is spent and the Slice
  14 remainders in its gaps 5 and 6 are now the
  front of the queue again, unless a later
  director decision reorders them. This scope
  obeys that order; it does not re-rank it, and
  it does not reorder the two remainders among
  themselves without a reason — the reason for
  the order below is stated, not assumed.
- The Slice 16 review's gaps, in priority order,
  and what this scope does with each: Windows
  Unity verification (gap 1) and the director
  playtest (gap 2) remain open, but neither is
  Slice 17 work in itself — the first is blocked
  on Unity 6000 on Windows, the second on the
  director — and scoping a slice does not close,
  reorder away, or pretend to answer either.
  First-pass graphics values (gap 3: dusk azimuth,
  fog contrast arithmetic, view model offset, the
  static weapon-agnostic model) are unproven, but
  they are retuned only on Windows evidence, not
  re-decided headlessly here: no Slice 17 step
  moves a light, fog, or model value to make a
  tint or a count read better. The Slice 13
  scene-proof observations (gap 4: items 25–27
  and 30–34, still unmade as observations) remain
  observations to be made when Windows
  verification runs, not steps to be re-scoped
  blindly — if an observation fails, that failure
  is a fix step with its own decision, not a
  reason to re-scope the proof layer. The
  single-cable tint finding (gap 5) and the
  direct-Conduit count question (gap 6) are Slice
  17's work, in that order, for the reason below.
  The patrol route's stated limits (gap 7: a
  refused step skips its waypoint, a fully
  enclosed patrol cycles without moving) and the
  free deploy primitive (gap 8: headless-only by
  rule, not by enforcement) are re-named, not
  quietly fixed here, and are carried as the
  front candidates for Slice 18 scoping.
- Why the tint step comes before the count step,
  stated as a decision rather than a ranking
  habit: the tint question is the visible misread
  in the scene — a live wire that renders dead —
  and it is the one item 32's combined scene
  already observes alongside the tints, so its
  answer changes what a Windows observer sees on
  the next scene check. The count question is a
  HUD-number invisibility, real but one step
  removed from the rendered wire. The tint step
  also settles, one way or the other, what a cable
  cell's state means — a cell-delivery reading or
  a component-liveness reading — and the count
  step's cases (a Conduit beside a mill, with no
  cable at all) are stated to be independent of
  that settlement, so the count step must say so
  explicitly rather than inheriting the tint
  step's vocabulary by accident. Reversing the
  order would not break either step, and the
  scope records that honestly: tint-first is
  chosen because the rendered misread outranks
  the counted one, not because the count depends
  on the tint.
- Why these two questions are display-query
  questions and not delivery questions, stated
  before either step decides anything: delivery
  is what a consumer does on a tick — a mill
  gathers powered or kinetically, is destroyed by
  a mismatch, a pylon fires, a workstation grants
  regen — and no Slice 17 step is allowed to move
  it. A tint is what the mesh shows for a cell,
  and a count is what the HUD line shows for a
  wiring shape. The two carried questions are
  exactly about whether the tint and the count
  describe the wiring the player built, and both
  steps must keep that boundary visible: if a
  step finds its honest answer requires a delivery
  move, it must say so and defer that move, not
  smuggle it in as a display fix.
- Ground facts this scope is decided from,
  verified this run against the code as it stands:
  `WorldZone.PowerState` for a Cable cell returns
  Energised exactly when `DeliveredVoltage` for
  that cell is not None, and None otherwise, with
  `PowerTint` mapping Energised to yellow (1,
  0.85, 0.1), Powered to green, and Mismatched to
  red, and `MeshBuilder` carrying one colour per
  vertex from that query; `DeliveredVoltage`
  counts only face-adjacent neighbours, and its
  direct branch knows Dynamo (LV) and Alternator
  (HV) but deliberately has no Conduit branch,
  because a Conduit delivers nothing
  face-adjacent — so a mill touching both a Dynamo
  and a Conduit reads LV, and a mill touching a
  Conduit alone reads None and gathers
  kinetically; a cable cell's delivery, by
  contrast, comes through the second branch, from
  a neighbouring Transformer output or from
  `CableNetworkVoltage` of a neighbouring cable
  component, and `CableNetworkVoltage` is
  component-wide — a Conduit touching any cell of
  a component is a full HV source for that
  component, mixed-voltage conflict rules
  included, with no loss per hop and no length
  limit. The single-cable case follows from those
  two facts together: a cable cell whose only
  neighbours are a Conduit and a consumer (or a
  Conduit and air, or two Conduits) has no direct
  neighbour, no transformer neighbour, and no
  cable neighbour, so its own `DeliveredVoltage`
  is None even though its component's voltage is
  HV — while a two-cable Conduit run reads
  Energised on both cells, because each cable cell
  sees the other cable's component voltage.
  `WorldZone.LivePowerCounts` counts live networks
  (a cable component, or a transformer group with
  no cable neighbour, carrying a non-None voltage
  and feeding no consumer) and lone sources (one
  Dynamo, Alternator, or Conduit cell with no
  cable, transformer, or consumer face-adjacent
  to it), rendered by `HudDisplay` as one POWER
  line ("POWER: n LIVE NETWORKS, m LONE
  SOURCES"), and the direct-Conduit case follows
  from the lone rule: a Conduit beside a mill is
  disqualified by consumer adjacency, whatever it
  delivers, while a Dynamo beside the same mill
  is also not lone — it is feeding. The refresh
  event set is unchanged since Slice 13
  (successful key, gather, destruction, load,
  Start, and nothing else), and the Slice 16
  material multiplies vertex colours, so a tint
  answer decided here renders on the settled
  surface rather than against an unsettled one —
  which is the sequence reason Slice 16's scope
  gave for deciding the material first. The next
  free playtest-gate checklist number is 35
  (items run to 34, the view model).

## Slice 17, in order

### Step 1 — Single-cable tint query

Decide what a cable cell's power state means,
decided in that step — not in this scope document
— with the reasoning recorded in DECISIONS.md:

- The step must decide the question the Slice 16
  review left open, and record it: should the
  power-state query for a cable cell also count
  its own component's voltage, so a single cable
  cell touching a Conduit reads Energised when
  its component carries HV — or should it keep
  reading the cell's own delivery, so the cell
  stays untinted because nothing is delivered to
  that cell itself? The component-voltage reading
  is a starting hypothesis, not a decision: the
  step must weigh it against the current reading's
  honesty (untinted means "no delivery to this
  cell", which is true of the cell even while the
  network is live) and against the player's
  reading (an untinted wire on a live network
  looks dead, which is false of the network).
  The step must say, in one sentence a later
  reader can quote, what Energised now means for
  a cable cell — cell delivery, or component
  liveness — because every later tint question
  will be answered in that sentence's terms.
- The step must decide the scope of the change,
  and record it: a component-voltage reading
  changes tint behaviour for every existing
  source's cables, not only Conduit cables, so
  the step must prove the change (or the
  deliberate no-change) case by case, not only
  for the Conduit case that motivated it. The
  proof set must include, at minimum: a single
  cable touching a Conduit and a mill (the
  motivating case, live network, mill destroyed
  on tick 1); a single cable touching a Conduit
  and air, and two Conduits on one single cable
  (component live, no consumer at all); a
  two-cable Conduit run (already Energised on
  both cells — the step must prove it stays so,
  not assume it); a Dynamo-fed single cable and
  an Alternator-fed single cable (already
  Energised by direct delivery — the step must
  prove they stay so under either reading);
  a transformer-fed single cable (already
  Energised through the second branch — same
  proof); a mixed-voltage component (Dynamo and
  Conduit, or Dynamo and Alternator, touching one
  component — component voltage None, so the
  cable must stay untinted under either reading,
  and the step must prove the component reading
  does not resurrect a dead network); and an
  unpowered component (cable with no source —
  None under either reading). If the step keeps
  the current reading, it must prove the same
  set reads as it does today, and must say what
  the player is expected to read instead of the
  tint for a live single cable — the mill's
  destruction, the pylon's fire, or the HUD
  warning — rather than leaving the misread
  unnamed.
- The step must decide where the change lives,
  and record it: a display-query change in
  `PowerState` only, or a delivery change in
  `DeliveredVoltage` — and the step must not
  blur the two. If the change is in `PowerState`
  alone, delivery is unchanged by construction,
  and the step must prove it: a mill beside a
  Conduit still reads None and gathers
  kinetically, a cable-fed mill is still
  destroyed on tick 1, a cable-fed pylon still
  fires 10 per tick, warnings still warn the same
  machines, and no consumer's powered or
  mismatched predicate moves. If the step finds
  the honest answer cannot be expressed in
  `PowerState` alone, it must say so and defer
  the delivery change, not make it inside a
  display step. No voltage, precedence, conflict,
  rating, cost, or phase rule moves in this step,
  under either reading.
- The step must decide the refresh consequence,
  and record it: a tint that changes on rewiring
  must appear on the next refresh, and the step
  must say whether the existing event set covers
  it or a new event is needed, with a reason.
  Cable and Conduit placement are successful
  keys, so placement itself refreshes — but the
  step must check the rewiring cases explicitly
  (a Conduit placed beside an existing cable, a
  cable placed beside an existing Conduit, a
  cable removed or a source removed by
  destruction) and must not assume the placement
  case covers them. A new refresh event, if one
  is needed, is a stated addition with its own
  tests, not a silent widening of the event set
  Slice 13 built to protect the frame budget.
- The step must decide the checklist relation,
  and record it: a new checklist item (from 35)
  observing the decided reading on Windows — a
  single-cable Conduit run, a two-cable run, and
  a mixed run, against dark voxel and under dusk
  light, with the mill's destruction or the
  pylon's fire as the live-network evidence
  beside the tint — and it must say whether that
  item supersedes, extends, or leaves standing
  item 28's honest-observation clause, which told
  the observer to record the untinted single
  cable honestly. Item 28's clause must not be
  left dangling: if the query changes, the clause
  is superseded for the decided case and the new
  item says so; if the query stands, the clause
  stands and the new item observes the kept
  reading, not a wished-for one. The step must
  also say how the new item relates to item 32's
  combined scene, which observes the single-cable
  cell alongside the tints: item 32 stands as the
  pre-decision observation, and the new item is
  the post-decision observation, or the step must
  say why they are the same observation.
- The step must state its verification honestly,
  and record it: unlike Slice 16's adapter steps,
  this step has a headless-testable claim — a
  query value for a staged cell — so it adds
  tests for every case in the proof set above,
  and the test suite must rise, not stand, with
  no failures. No demo scenario is required of
  this step alone; the composed demo belongs to
  Step 3, which stages both decided queries
  together. No material, shader, marker, light,
  or model value moves in this step: the Slice
  16 surface renders whatever this step decides,
  and a step that re-palettes a tint to make its
  answer look right has failed its own question.

### Step 2 — Direct-Conduit count query

Decide what the HUD counts show for a source
beside a consumer, decided in that step — not in
this scope document — with the reasoning recorded
in DECISIONS.md:

- The step must decide the question the Slice 16
  review left open, and record it: should the
  counts distinguish a source beside a consumer
  that delivers nothing (a Conduit beside a mill)
  from a source beside a consumer that delivers
  (a Dynamo beside a mill, LV; an Alternator
  beside a mill, HV and destructive) — or should
  the current definition stand, with the mill's
  None state and kinetic cadence as the
  sufficient readout for the non-delivering case?
  The step must weigh at least three hypotheses,
  none of them adopted here: keep the definition
  unchanged (a source beside a consumer is wired
  in the count's terms, whatever it delivers,
  because adjacency is wiring); count a
  non-delivering consumer-adjacent source as lone
  (lone then means "delivering nothing", not
  "adjacent to nothing", and the lone rule's
  wording must change to say so); or add a
  distinct readout for it (a third number, a
  changed line, or a stated fold-in — see the
  HUD paragraph below). Keeping the definition
  is a decision, not a default: the step must say
  why the mill's None reading and kinetic cadence
  are sufficient, or why they are not, with the
  player's wiring task in mind — a player
  placing a Conduit beside a mill has made a
  wiring mistake (or a deliberate staging choice),
  and the count is the readout that tells them
  the placement went nowhere.
- The step must decide the proof set, and record
  it: a Conduit directly beside a mill (the
  motivating case, 0 LIVE / 0 LONE today); the
  same Conduit beside a workstation and beside a
  pylon (the step must say whether the answer is
  per-consumer or one rule for all three
  consumers, and prove it for each, not assume
  the mill case generalises); a Dynamo beside a
  mill and an Alternator beside a mill (feeding
  cases — neither is lone today, and the step
  must prove neither becomes lone under its new
  rule, whatever the rule is); a Conduit beside
  a mill that also touches a cable (a live
  network feeding a consumer, not a lone case at
  all — the step must prove the count does not
  double-count it as both); two Conduits beside
  one mill, and one Conduit beside two mills
  (the step must say whether the count is per
  source cell, per consumer, or per pair, and
  prove it); a Conduit beside a transformer
  (transformer adjacency also disqualifies a
  lone source today — the step must say
  explicitly whether its rule covers that case
  too, or whether transformer adjacency stays
  disqualifying on its own grounds, because a
  transformer beside a Conduit with no other
  input outputs None, so the Conduit beside a
  transformer is also delivering nothing); and a
  lone Conduit with no neighbour at all (still
  lone under every hypothesis — the control case
  the step must not break). If the step adds a
  distinct readout, it must prove the old POWER
  line's numbers for every case above as well as
  the new readout, so a reader can tell whether
  the old numbers moved or only the new one was
  added.
- The step must decide the HUD line, and record
  it: whether the decided count folds into the
  existing POWER line's numbers (same line, new
  counts), needs a new line, or needs a new word
  on the same line — and must weigh that against
  HUD discipline (art-direction rule 4: health,
  shields, objective distance and completion,
  mini-map, ammo — no more), which is law, not a
  variable this step can spend. A new HUD line
  for one edge case is a redesign claim and must
  be justified as such; folding a changed lone
  count into the existing line is the starting
  hypothesis, not a decision. The step must also
  say whether the count change touches any other
  HUD line: powered mill, workstation, and pylon
  counts are consumer counts, not source counts,
  and the step must prove they do not move.
- The step must decide the delivery boundary,
  and record it: no delivery rule moves in this
  step, under any hypothesis. A Conduit beside a
  mill still delivers nothing directly, a mill
  beside a Conduit still gathers kinetically, and
  a Dynamo or Alternator beside a mill still
  delivers exactly as before. A count that made a
  Conduit deliver would not be a count step at
  all, and the step must say so if it finds
  itself tempted. The step must also state its
  independence from Step 1 explicitly: Step 1
  decides cable tint, and this step's cases have
  no cable, so Step 1's answer does not change
  any Step 2 case — but if Step 1 changed the
  meaning of a shared word (for example what
  "live" means), this step must say whether it
  adopts that meaning for counts or keeps the
  Slice 12 meaning ("carrying a voltage that is
  not None, feeding no consumer"), and why.
- The step must decide the checklist item, and
  record it: a new checklist item (from 35, or
  from 36 if Step 1 already took 35) observing
  the decided count on Windows — a Conduit
  beside a mill, a Dynamo beside a mill, and a
  lone Conduit, with the POWER line read aloud
  for each — and it must say whether that item
  supersedes or extends any earlier count
  observation (item 22's live-network and
  lone-source display, and item 28's Conduit
  observation). The step must state its
  verification honestly: like Step 1, this step
  has a headless-testable claim — a count pair
  for a staged layout — so it adds tests for
  every case in the proof set above, and the test
  suite must rise, not stand, with no failures.
  No material, marker, light, or model value
  moves in this step.

### Step 3 — Slice 17 demo + review

As with 0.34.0, 0.38.0, 0.43.0, 0.47.0, 0.51.0,
0.55.0, 0.60.0, 0.63.0, and 0.66.0 — and unlike
Slice 16's review, which was documentation-only
because no graphics claim is headless-provable —
this review IS accompanied by a new composed demo
scenario, because both Slice 17 queries are
headless-testable and a demo that stages them
together proves something a step's isolated tests
do not: that the two decided readings agree in
one wiring. The demo scenario, decided in that
step, must stage in one Simulation, at minimum:
a single-cable Conduit run to a mill (the Step 1
case, with the mill's destruction as the
live-network evidence beside the tint reading);
a two-cable Conduit run as the already-tinted
control; a mixed-voltage component as the
still-untinted control; a Conduit directly beside
a mill (the Step 2 case, with the POWER line
read before and after); and a Dynamo beside a
mill and a lone Conduit as the feeding and lone
controls — with a save/load round-trip proving
both readings re-derive on load (both queries are
derived, never saved, so a changed save length
would mean a save rule moved, and the review
must say so rather than quoting the suite as
unchanged). Composed-integration tests mirroring
the demo must be added, and the test suite must
rise with no failures. The review writes
`docs/slice-17-review.md`, checks the slice
against this scope step by step, states the
verdict honestly (two display queries decided
with tests, not playtested, not Unity-verified),
separates headless-tested query claims from
Windows-checklist count and tint observations,
carries the gaps that remain, and does NOT scope
Slice 18 — Slice 18 is scoped afterwards, the
same way this document follows the Slice 16
review rather than preceding it. Every earlier
demo save length must stand; the new scenario's
save length is quoted as the new entry, not as a
change to an old one.

## Deliberately NOT in Slice 17

- A delivery, precedence, conflict, rating, cost,
  or phase rule change. Delivery is what a
  consumer does on a tick, and no Slice 17 step
  moves it: if a step finds its honest answer
  needs one, that need is recorded and deferred,
  not made inside a display step.
- A material, shader, marker, light, fog, or view
  model change. The Slice 16 surface renders
  whatever Slice 17 decides; a step that
  re-palettes a tint, retunes dusk, or moves a
  model to make its answer look right has
  confused the surface with the query.
- A HUD redesign. The count step decides one
  POWER line question (fold-in, new line, or new
  word, with justification) and nothing else: no
  new HUD colour, no new HUD layout, no quest
  list, no second POWER line smuggled in as a
  "clarification".
- A new block, kind, doctrine, source, or
  consumer, a movement, targeting, or save rule
  change, a balance pass, or a price or cap
  change. Slice 17 is two display queries and
  their review, and the core stands exactly as
  Slice 16 left it except for the two queries
  themselves.
- A scene-proof observation presented as a
  headless result. Items 25–27 and 30–34, and the
  new items Steps 1 and 2 add, are Windows
  observations, recorded in DECISIONS.md when
  observed, not closed by a document, a script,
  or a test count.
- A scene file (`.unity`), an imported asset, a
  texture, or a shader file written by a headless
  run and claimed as a built scene. Scene
  assembly remains Windows checklist work.
- A copied asset, model, skyline, HUD layout, or
  trademarked look from any other game.
- Windows Unity verification itself. It remains
  open for Slices 1–17 alike and is blocked on
  Unity 6000 on Windows, not on anything this
  slice can do headlessly.
- The patrol route's stated limits, or an
  enforcement for the free deploy primitive, as a
  bonus inside a query step. Each is its own
  decision with its own tests, carried after
  Slice 17, not patched in beside a tint.
- A Slice 18 scope. Slice 18 is scoped after the
  Slice 17 review, not inside it.

## Ranked candidates for Slice 18 scoping (not promised)

These are recorded here so the Slice 17 review
starts from a list, not from scratch. Order
follows the Slice 16 review's gaps with the two
Slice 14 remainders taken:

1. The patrol route's stated limits and the free
   deploy primitive, carried from the Slice 16
   review's gaps 7 and 8 — a detour, hold, or
   stuck readout for Patrol, or an enforcement
   for the free deploy primitive, each as its own
   decision with its own tests, not promised here.
2. Any Slice 13 remainder that Slice 17 does not
   itself settle as an observation (the
   vertex-colour material observation, the
   MeshRenderer wiring observation, and the
   marker-occlusion observation in gap 4, plus
   the new dusk and view model observations in
   items 33 and 34, plus the new tint and count
   observations Steps 1 and 2 add) — if those
   observations are still unmade when the review
   is written, they remain observations to be
   made when Windows verification runs, not steps
   to be re-scoped blindly, and not promised here.
3. First-pass graphics values retuned only on
   Windows evidence (dusk azimuth, fog contrast,
   view model offset, static weapon-agnostic
   model) — a retuning step is justified by an
   observation, not by a headless re-decision,
   and is not promised here.

## Slice 17 gate (what "done" means)

Slice 17 is done when: every step above is pushed
with passing tests (for Steps 1 and 2, with the
test suite risen by the proof-set tests, never
standing still on a query step and never inflated
into a claim about a visible scene); the Step 1
tint decision and the Step 2 count decision are
each recorded with rationale, each with its own
Windows checklist item from 35, with no delivery,
precedence, conflict, rating, cost, or phase rule
changed, no material, marker, light, fog, model,
or HUD rule changed except the count line's
decided question, and every earlier demo save
length unchanged; and `docs/slice-17-review.md`
records the verdict honestly, separating
headless-tested query claims from
Windows-checklist tint and count observations,
including that Slice 17 is still not playtested
and not Unity-verified. The Slice 1 director
playtest gate is still pending throughout and is
not closed by Slice 17.
