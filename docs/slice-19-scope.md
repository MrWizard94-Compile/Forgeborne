# Forgeborne — Slice 19 Scope

**Status: SCOPED 2026-10-31, under delegation and
under the Slice 18 review's ranked candidates.
No Slice 19 system is built by this document.
Each step below is built one at a time, with
tests, in this order, and Slice 19 is closed by
its own review — it is not closed by finishing
step 1 alone.**

## Scope question

Slice 18 settled the two orbital remainders: what
a patrol does when its waypoint step is refused
(sidestep, then skip, then Stuck after a full
circuit of refusals), and what stops a play path
from using the free deploy primitive (the public
entry points are gone; only the paid path and an
internal test seam remain). What Slice 18
deliberately did not touch is the one display
query its review still carries as open, the
transformer-adjacency invisibility from Slice 17:

- A Conduit standing directly beside a transformer
  alone reads POWER 0 LIVE NETWORKS, 0 LONE
  SOURCES, even though the transformer outputs
  None with no other input and the Conduit feeds
  nothing — because a transformer neighbour
  disqualifies a lone source, whatever the
  transformer delivers, and a transformer group
  carrying no voltage is not a live network.

Slice 19 is therefore the transformer-adjacency
count slice, kept as small as the slices before
it: **the one carried Slice 17 remainder, decided
as its own step with its own tests, then its own
review** — not a delivery redesign, not a new
source, consumer, kind, or doctrine, not a
balance pass, not a material or HUD redesign,
and not a claim of Unity verification made from
a headless run.

## Inputs this scope is decided from

- The Slice 18 review's ranked candidates, in
  order, and what this scope does with each: the
  transformer-adjacency invisibility (candidate
  1) is Slice 19's work. The Slice 13 remainder
  observations (candidate 2: items 25–27 and
  30–37, still unmade as observations) remain
  observations to be made when Windows
  verification runs, not steps to be re-scoped
  blindly — if an observation fails, that
  failure is a fix step with its own decision,
  not a reason to re-scope the proof layer.
  First-pass graphics values (candidate 3: dusk
  azimuth, fog contrast, view model offset, the
  static weapon-agnostic model) are retuned only
  on Windows evidence, not re-decided headlessly
  here: no Slice 19 step moves a light, fog, or
  model value to make a count read better.
- The Slice 18 review's other gaps, in priority
  order, and what this scope does with each:
  Windows Unity verification (gap 1) and the
  director playtest (gap 2) remain open, but
  neither is Slice 19 work in itself — the first
  is blocked on Unity 6000 on Windows, the
  second on the director — and scoping a slice
  does not close, reorder away, or pretend to
  answer either. First-pass graphics values
  (gap 3) are candidate 3 above. The Slice 13
  scene-proof observations (gap 4) are
  candidate 2 above. The transformer-adjacency
  invisibility (gap 5) is Slice 19's work. The
  patrol route's open questions (gap 6: a
  permanently blocked patrol cycles forever
  reading Stuck, with no recall, no re-rally,
  and no player action defined for it; radius 3
  and the five-waypoint loop are still
  first-pass values) are NOT scoped here: they
  are new decisions, not a gap in Slice 18
  Step 1's proof, and this scope names them as
  carried, not promised, so that the next
  scoping step ranks them in the open rather
  than finding them smuggled into a count step.
  Reversing the order — patrol recall before
  transformer counts — would not break either
  decision, and the scope records that honestly:
  transformer-first is chosen because it is the
  review's ranked candidate 1 and because it
  closes the last Slice 17 display-query
  remainder, not because a stuck patrol depends
  on a count.
- Why this question is a display-query question
  and not a delivery question, stated before
  the step decides anything: delivery is what a
  consumer does on a tick — a mill gathers
  powered or kinetically, is destroyed by a
  mismatch, a pylon fires, a workstation grants
  regen — and no Slice 19 step is allowed to
  move it. A count is what the HUD line shows
  for a wiring shape. The carried question is
  exactly about whether the count describes the
  wiring the player built, and the step must
  keep that boundary visible: if it finds its
  honest answer requires a delivery move (for
  example, making a Conduit feed a transformer
  directly), it must say so and defer that move,
  not smuggle it in as a count fix.
- Ground facts this scope is decided from,
  verified this run against the code as it
  stands: `WorldZone.LivePowerCounts` counts
  live networks (a cable component carrying a
  non-None voltage and feeding no consumer, or
  a transformer group with no cable neighbour
  carrying a non-None output and feeding no
  consumer) and lone sources (one Dynamo,
  Alternator, or Conduit cell, per source cell,
  with no cable or transformer neighbour, and
  no consumer neighbour that it feeds directly
  — Dynamo and Alternator feed any adjacent
  consumer directly, rated or mismatched alike,
  so a consumer neighbour still disqualifies
  them; a Conduit feeds no adjacent consumer,
  so it does not, per Slice 17 Step 2).
  `TransformerOutput` takes direct inputs only
  from a Dynamo (LV) and an Alternator (HV);
  it has deliberately no Conduit branch, so a
  transformer beside a Conduit alone outputs
  None; distinct direct inputs resolve to None,
  as do distinct non-direct inputs; a
  transformer touching a cable belongs to that
  cable component's story, never to a standalone
  group. `HudDisplay` renders the counts on one
  existing line ("POWER: n LIVE NETWORKS, m LONE
  SOURCES"), and the counts are derived, never
  saved, so a changed demo save length would
  mean a save rule moved. The motivating case
  follows from those facts together: a Conduit
  beside a transformer alone is disqualified
  from lone by the transformer neighbour, and
  its transformer group carries no voltage, so
  it is neither live nor lone — 0 LIVE, 0 LONE.
  By contrast, a Dynamo beside a transformer
  alone reads 1 LIVE, 0 LONE (the flipped LV,
  HV, feeds no consumer), and an Alternator
  beside a transformer alone reads 1 LIVE, 0
  LONE (the flipped HV, LV) — so the
  invisibility is specific to wiring that
  delivers nothing, not to transformer adjacency
  in general. The next free playtest-gate
  checklist number is 38 (items run to 37, the
  patrol route).

## Slice 19, in order

### Step 1 — Transformer-adjacency count query

Decide what the HUD counts show for a source
beside wiring that delivers nothing, decided in
that step — not in this scope document — with
the reasoning recorded in DECISIONS.md:

- The step must decide the question the Slice 18
  review left open, and record it: should the
  counts distinguish a source beside a
  transformer that delivers nothing (a Conduit
  beside a transformer alone) from a source
  beside a transformer that delivers (a Dynamo
  or Alternator beside a transformer, already
  read as 1 LIVE) — or should the current
  definition stand, with the transformer's None
  output and the Conduit's idle state as the
  sufficient readout for the non-delivering
  case? The step must weigh at least four
  hypotheses, none of them adopted here: keep
  the definition unchanged (a source beside
  wiring is wired in the count's terms, whatever
  the wiring delivers, because adjacency is
  wiring); count a non-delivering
  transformer-adjacent source as lone (lone then
  means "delivering nothing" for wiring
  neighbours too, extending Slice 17 Step 2's
  consumer logic, and the lone rule's wording
  must change to say so); add a distinct readout
  for it (a third number, a changed line, or a
  stated fold-in — see the HUD paragraph
  below); or redefine live to include dead
  wiring (a transformer group carrying None
  counting as live — a hypothesis the step must
  weigh and, if it rejects it, reject with the
  reason that live has meant "carrying a voltage
  that is not None" since Slice 12, and a dead
  wiring counted as live would make the live
  number lie about the very thing it exists to
  report). Keeping the definition is a decision,
  not a default: the step must say why the
  transformer's None output and the Conduit's
  idle state are sufficient, or why they are
  not, with the player's wiring task in mind —
  a player placing a Conduit beside a
  transformer has made a wiring mistake (or a
  deliberate staging choice), and the count is
  the readout that tells them the placement
  went nowhere.
- The step must decide the proof set, and
  record it: a Conduit directly beside a
  transformer alone (the motivating case, 0 LIVE
  / 0 LONE today); a Conduit beside a
  transformer beside a mill (the transformer
  group feeds a consumer, so it is not live
  under any hypothesis — the step must say
  whether its rule changes this case or leaves
  it at 0 / 0, and why); a Conduit beside a live
  transformer (a transformer fed by a Dynamo on
  one side and touching a Conduit on the other:
  the group already reads 1 LIVE, and the step
  must prove its rule does not double-count it
  as both live and lone); a Dynamo beside a
  transformer alone and an Alternator beside a
  transformer alone (live controls — 1 LIVE, 0
  LONE today, and the step must prove neither
  becomes lone under its new rule, whatever the
  rule is); two Conduits beside one dead
  transformer, and one Conduit beside two dead
  transformers (the step must say whether the
  count is per source cell, per transformer, or
  per pair, and prove it — Slice 17 Step 2
  counted per source cell, and any departure
  from that must be justified, not assumed); a
  transformer chain or ring with no source, a
  Conduit beside one cell of it (every output
  None — the step must say whether the answer
  is per group or per source, and prove the
  group is counted once, not once per cell);
  a conflicted transformer (a Dynamo and an
  Alternator touching one transformer: output
  None, group not live, neither source lone
  today — 0 / 0 for two placed sources — and
  the step must say explicitly whether its rule
  covers this case too, or whether conflicted
  transformers stay invisible on their own
  grounds, because a rule that counts a
  Conduit beside a dead transformer but not a
  Dynamo beside a conflicted one must say what
  distinguishes them); a Conduit beside both a
  mill and a dead transformer (today the
  transformer neighbour disqualifies it even
  though Slice 17 Step 2 would count it lone
  for the mill alone — the step must say which
  neighbour wins, and why); a Conduit beside a
  transformer beside a cable (the transformer
  belongs to the cable story, not to a
  standalone group — the step must say whether
  this case is in its rule or stays cable-story,
  for a live cable and for a dead, mixed-voltage
  cable alike); and a lone Conduit with no
  neighbour at all (still lone under every
  hypothesis — the control case the step must
  not break).
- The step must decide the dead-cable
  consistency case, and record it: the Slice 18
  review's condition for this decision is that
  it cover the dead-cable case to be consistent,
  and the step must meet that condition in the
  open, not by analogy alone. The dead-cable
  case is a mixed-voltage cable component — a
  Dynamo and a Conduit touching one component,
  so the component carries None and is not live,
  while both sources are disqualified from lone
  by cable adjacency, reading 0 / 0 today. The
  step must say whether its transformer rule
  extends to cable adjacency (sources beside a
  dead component counting as lone, or as a
  distinct readout), or why cable adjacency
  differs from transformer adjacency (for
  example, a cable component is a shared carrier
  whose deadness is already visible as a
  conflict between its sources, while a lone
  transformer is a single converter with no
  other input — or the reverse argument, if the
  step finds it stronger). A rule that counts
  transformer-adjacent dead sources and leaves
  cable-adjacent dead sources at 0 / 0 without
  a stated distinction has not met the
  consistency condition, and the step must say
  so rather than leaving the asymmetry unnamed.
  The same consistency question applies to a
  cable component fed only by a dead
  transformer: the step must say whether that
  component's deadness is a cable case, a
  transformer case, or neither.
- The step must decide the HUD line, and
  record it: whether the decided count folds
  into the existing POWER line's numbers (same
  line, new counts), needs a new line, or needs
  a new word on the same line — and must weigh
  that against HUD discipline (art-direction
  rule 4: health, shields, objective distance
  and completion, mini-map, ammo — no more),
  which is law, not a variable this step can
  spend. A new HUD line for one edge case is a
  redesign claim and must be justified as such;
  folding a changed lone count into the
  existing line is the starting hypothesis, not
  a decision. The step must also say whether
  the count change touches any other HUD line:
  powered mill, workstation, and pylon counts
  are consumer counts, not source counts, and
  the step must prove they do not move, and
  warned counts must not move either.
- The step must decide the delivery boundary,
  and record it: no delivery rule moves in this
  step, under any hypothesis. A Conduit beside
  a transformer still delivers nothing directly,
  a transformer beside a Conduit alone still
  outputs None, a Dynamo or Alternator beside
  a transformer still delivers exactly as before,
  and no consumer's powered, mismatched, or
  warned predicate moves. A count that made a
  Conduit deliver would not be a count step at
  all, and the step must say so if it finds
  itself tempted. The step must also state its
  relation to Slice 17 Step 2 explicitly: Step 2
  redefined lone for consumer neighbours, and
  this step's cases have a wiring neighbour, so
  Step 2's sentence does not decide them by
  itself — but if this step extends Step 2's
  "delivering nothing" logic to wiring, it must
  say so and restate the lone sentence whole,
  rather than leaving two half-sentences for
  two neighbour kinds. No material, marker,
  light, fog, model, or tint value moves in
  this step: a Conduit beside a transformer
  reads None in PowerState, as sources and
  transformers do by decision, and a step that
  re-tints a source to make its answer look
  right has confused the surface with the query.
- The step must decide the checklist item, and
  record it: a new checklist item (from 38)
  observing the decided count on Windows — a
  Conduit beside a transformer alone, a Dynamo
  beside a transformer alone, and a lone
  Conduit, with the POWER line read aloud for
  each — and it must say whether that item
  supersedes, extends, or leaves standing item
  36's direct-Conduit observation and item 22's
  live-network and lone-source display. The
  step must state its verification honestly:
  like Slice 17's steps, this step has a
  headless-testable claim — a count pair for a
  staged layout — so it adds tests for every
  case in the proof set above, and the test
  suite must rise, not stand, with no failures.
  No demo scenario is required of this step
  alone; the composed demo belongs to Step 2,
  which stages the decided count together with
  its controls.

### Step 2 — Slice 19 demo + review

As with 0.34.0, 0.38.0, 0.43.0, 0.47.0, 0.51.0,
0.55.0, 0.60.0, 0.63.0, 0.66.0, 0.76.0, and
0.80.0 — and unlike Slice 16's review, which was
documentation-only because no graphics claim is
headless-provable — this review IS accompanied
by a new composed demo scenario, because the
Step 1 count is headless-testable and a demo
that stages it together with its controls proves
something a step's isolated tests do not: that
the decided reading agrees with the live,
feeding, and lone controls in one wiring. The
demo scenario, decided in that step, must stage
in one Simulation, at minimum: a Conduit beside
a transformer alone (the Step 1 case, with the
POWER line read before and after); a Dynamo
beside a transformer alone and an Alternator
beside a transformer alone as the live controls;
a lone Conduit as the lone control; a Conduit
beside a mill as the Slice 17 Step 2 control;
and the dead-cable consistency case (a
mixed-voltage component) alongside the decided
transformer case, so a reader can see whether
the two dead wirings read alike or differently
under the decided rule — with a save/load
round-trip proving the reading re-derives on
load (the count is derived, never saved, so a
changed save length would mean a save rule
moved, and the review must say so rather than
quoting the suite as unchanged).
Composed-integration tests mirroring the demo
must be added, and the test suite must rise with
no failures. The review writes
`docs/slice-19-review.md`, checks the slice
against this scope step by step, states the
verdict honestly (one display-query remainder
decided with tests, not playtested, not
Unity-verified), separates headless-tested count
claims from the Windows-checklist count
observation, carries the gaps that remain, and
does NOT scope Slice 20 — Slice 20 is scoped
afterwards, the same way this document follows
the Slice 18 review rather than preceding it.
Every earlier demo save length must stand; the
new scenario's save length is quoted as the new
entry, not as a change to an old one.

## Deliberately NOT in Slice 19

- A delivery, precedence, conflict, rating, cost,
  or phase rule change. Delivery is what a
  consumer does on a tick, and no Slice 19 step
  moves it: if a step finds its honest answer
  needs one, that need is recorded and deferred,
  not made inside a display step.
- A material, shader, marker, light, fog, or view
  model change. The Slice 16 surface renders
  whatever Slice 19 decides; a step that
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
  change. Slice 19 is one display query and its
  review, and the core stands exactly as Slice
  18 left it except for the query itself.
- A patrol recall, re-rally, or player action for
  a stuck patrol, a patrol radius or waypoint
  change, or any other Slice 18 gap 6 decision,
  as a bonus inside a count step. Each is its
  own decision with its own tests, carried after
  Slice 19, not patched in beside a count.
- A scene-proof observation presented as a
  headless result. Items 25–27 and 30–37, and
  the new item Step 1 adds, are Windows
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
  open for Slices 1–19 alike and is blocked on
  Unity 6000 on Windows, not on anything this
  slice can do headlessly.
- A Slice 20 scope. Slice 20 is scoped after the
  Slice 19 review, not inside it.

## Ranked candidates for Slice 20 scoping (not promised)

These are recorded here so the Slice 19 review
starts from a list, not from scratch. Order
follows the Slice 18 review's gaps with the
transformer remainder taken:

1. The patrol route's open questions, carried
   from the Slice 18 review's gap 6 — a recall,
   re-rally, or player action for a permanently
   blocked patrol reading Stuck, or a radius /
   waypoint first-pass value revisited on
   evidence, each as its own decision with its
   own tests, not promised here.
2. Any Slice 13 remainder that Slice 19 does not
   itself settle as an observation (the
   vertex-colour material observation, the
   MeshRenderer wiring observation, and the
   marker-occlusion observation in gap 4, plus
   the dusk and view model observations in items
   33 and 34, plus the tint, count, and patrol
   observations in items 35, 36, and 37, plus
   the new item Step 1 adds) — if those
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

## Slice 19 gate (what "done" means)

Slice 19 is done when: every step above is pushed
with passing tests (for Step 1, with the test
suite risen by the proof-set tests, never
standing still on a query step and never inflated
into a claim about a visible scene); the Step 1
count decision is recorded with rationale, with
its own Windows checklist item from 38, with no
delivery, precedence, conflict, rating, cost, or
phase rule changed, no material, marker, light,
fog, model, or HUD rule changed except the count
line's decided question, and every earlier demo
save length unchanged; and `docs/slice-19-review.md`
records the verdict honestly, separating
headless-tested count claims from the
Windows-checklist count observation, including
that Slice 19 is still not playtested and not
Unity-verified. The Slice 1 director playtest
gate is still pending throughout and is not
closed by Slice 19.
