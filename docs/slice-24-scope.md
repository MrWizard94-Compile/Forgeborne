# Forgeborne — Slice 24 Scope

**Status: SCOPED 2026-10-31, under delegation and
under the Slice 23 review's ranked candidates.
No Slice 24 system is built by this document.
Each step below is built one at a time, in this
order, and Slice 24 is closed by its own review
— it is not closed by finishing step 1 alone.**

## Scope question

Slices 20, 21, and 22 each decided a player
action in core and left its play path as a
comment: the stuck-patrol free re-rally
(Slice 20 Step 1), the general free re-rally
(Slice 21 Step 1), and the dismiss with
refund none and one cap slot freed (Slice 22
Step 1). The core actions exist, tested
headlessly, with thin `Simulation`
passthroughs. The play path does not: in
`ForgeborneUnity/Assets/Scripts/FpsInputAdapter.cs`
the K binding for re-rally and the X binding
for dismiss are PROPOSED ONLY comments, and a
search of that file finds no `KeyCode.K` and
no `KeyCode.X` handling at all. The
playtest-gate items that observe those
actions — item 39 (stuck re-rally), item 40
(general re-rally), and item 41 (dismiss) —
therefore say, each in its own words, that
the observation cannot even be attempted
until a later adapter step implements the
binding. Three decided actions with no key
behind them are not playable actions, and
no headless test count can change that:
adapter scripts are NOT compiled headlessly,
and no headless test proves a proposed key
works in a scene.

Slice 24 is therefore the bindings slice,
kept as small as the slices before it:
**implement the K re-rally binding first,
then the X dismiss binding, then its own
review** — not a general squad-command
system, not a core rule change, not an
economy change, not a HUD redesign, not a
marker redesign, and not a claim of Unity
verification made from a headless run.
Implementing a binding makes items 39, 40,
and 41 attemptable on Windows; it does not
pass them, and no step in this slice marks
them passed from headless work.

## Inputs this scope is decided from

- The Slice 23 review's ranked candidates, in
  order, and what this scope does with each:
  candidate 1 (any Slice 13 remainder that
  Slice 23 does not itself settle as an
  observation — the vertex-colour material
  observation, the MeshRenderer wiring
  observation, and the marker-occlusion
  observation, plus the dusk and view model
  observations in items 33 and 34, plus the
  tint, count, patrol, re-rally, and dismiss
  observations in items 35, 36, 37, 39, 40,
  and 41) remains observations to be made
  when Windows verification runs, not steps
  to be re-scoped blindly — and Slice 24
  changes that standing in one specific
  way only: the re-rally and dismiss
  observations in items 39, 40, and 41
  become attemptable once Steps 1 and 2
  implement their bindings, while the
  remaining observations in that candidate
  stay unmade until Windows verification
  runs, exactly as before. Candidate 2
  (first-pass graphics values retuned only
  on Windows evidence — dusk azimuth, fog
  contrast, view model offset, static
  weapon-agnostic model) is not Slice 24
  work: no step here moves a light, fog, or
  model value, because no Windows
  observation has yet justified a retune.
  Candidate 3 (a general re-rally price on
  Windows playtest evidence, if free
  repositioning proves abusive kiting) and
  candidate 4 (a dismiss refund on Windows
  playtest evidence, if slot-only removal
  proves punishing) are not Slice 24 work
  either: both are economy decisions gated
  on playtest evidence that does not exist
  yet, and a bindings step must not reprice
  either action to pay for implementing its
  key. Candidate 5 (the adapter bindings
  implementation — K for re-rally, X for
  dismiss, both currently comments in
  `FpsInputAdapter.cs` only — as its own
  adapter step, following the Slice 16
  pattern, so that items 39, 40, and 41
  become observable on Windows at all) IS
  Slice 24's work, in the narrowed form
  below. It is the only ranked candidate
  that is a buildable step rather than a
  Windows observation or a
  Windows-evidence-gated economy decision,
  and that is why it is first: the other
  candidates are not demoted, they are
  waiting on evidence this slice cannot
  manufacture headlessly.
- The Slice 23 review's other gaps, in
  priority order, and what this scope does
  with each: Windows Unity verification
  (Slices 1–23, items 25–27 and 30–41 still
  open) and the director playtest remain
  open, but neither is Slice 24 work in
  itself — the first is blocked on Unity
  6000 on Windows, the second on the
  director — and scoping a slice does not
  close, reorder away, or pretend to answer
  either. First-pass graphics values are
  candidate 2 above. The Slice 13
  scene-proof observations are candidate 1
  above. The re-rally price and the
  dismiss refund are candidates 3 and 4
  above. The K/X bindings are candidate 5
  above, taken here. A loop-shape decision,
  if ever wanted, is a later decision with
  its own tests, carried after Slice 24,
  not patched inside a bindings step: a
  bindings step moves no loop, changes no
  centre, and proves nothing about radius.
- Ground facts this scope is decided from,
  verified this run against the code as it
  stands: the K proposal (Slice 20 Step 1,
  widened by Slice 21 Step 1) is K while
  NOT picking, crosshair on an eligible
  squad's marker, opening a re-rally pick
  that reuses the picker's raycast rally
  selection, with Return confirming the new
  rally and Escape cancelling — doing
  nothing while picking, nothing when the
  raycast hits no eligible marker, and
  nothing when the target squad refuses
  the action. The X proposal (Slice 22
  Step 1) is X while NOT picking, crosshair
  on any live squad's marker, opening an
  adapter pending dismiss that Return
  confirms and Escape cancels — the
  pending state lives in the adapter, not
  in core, so the core call itself still
  removes on a single call and no headless
  test proves the confirm step — doing
  nothing while picking, nothing when the
  raycast hits no live marker, nothing
  when the target squad refuses, nothing
  on Return with no pending dismiss, and
  nothing on a cancelled confirmation.
  Both keys are free against the taken
  keys (WASD / Space / E / Q / R / O /
  Return / Esc / Alpha1-7 / T / Y / U / I /
  B / N / P / M / mouse click), and K is
  taken by the re-rally proposal, so X
  collides with neither. The core gates
  are `WorldZone.TryReRallyStuckPatrol`
  (stuck Patrol only, by squad-list index),
  `WorldZone.TryReRallySquad` (live
  HoldGround or Patrol, by squad-list
  index), and `WorldZone.TryDismissSquad`
  (any live squad, by squad-list index),
  each with a thin `Simulation`
  passthrough, each fail-closed with
  nothing changed on refusal. The
  deploy price (10 EmberOre) and the cap
  (3 live squads) stand, the re-rally
  stays free, and the dismiss stays refund
  none with one slot freed. Radius 3 and
  waypoint count 5 are carried, not
  re-decided. The next free playtest-gate
  number is 42 (items run to 41). Tests
  stand at 4577/4577 before this scope,
  with the Slice 23 demo save at 44,041
  chars; a bindings step is expected to
  leave both standing, for the reason
  given under "Verification" below, and a
  step that moves either must say why.

## The marker-to-index question both steps must answer

This is the one question neither proposal
answers, and it must be decided in Step 1
and reused, not re-decided, in Step 2 — or
Step 1 must say why the two bindings need
different answers. The proposals say
"crosshair on a marker", but a marker is
not a squad-list index, and the two
numberings diverge in exactly one case,
which Step 1 must prove it has handled
rather than assumed away:

- `OrbitalView.Snapshot` builds its list
  from `WorldZone.Squads` in order but
  SKIPS dead squads, so a snapshot position
  is a live-only position. The core gates
  index `WorldZone.Squads` directly, which
  still contains a dead squad until the
  next world tick's `Squads.RemoveAll`
  sweep removes it. A dead squad in the
  list therefore shifts every later
  squad's gate index by one relative to
  its snapshot position, and a binding
  that passes a snapshot position to a
  gate as if it were a list index names
  the wrong squad whenever a dead squad
  is waiting for its sweep.
- `SquadMarkerDisplay` renders the
  snapshot in order into pooled slots (at
  most 3 squad markers, 3 rally markers,
  and 1 pending marker), one slot per
  snapshot position, so a marker slot
  number is a snapshot position, not a
  gate index, by the same divergence.
- Step 1 must therefore decide the
  translation and record it: how the
  adapter maps the raycast-hit marker to
  the gate index it passes to core — by
  walking `Simulation.World.Squads` in
  order and skipping dead squads the way
  the snapshot does, by another mapping it
  justifies, or by a different targeting
  rule it justifies with a reason why two
  squad actions (deploy picker, re-rally,
  dismiss) may name squads differently.
  The translation must be stated for the
  dead-in-list case explicitly, including
  what the binding does when its target
  squad dies between the key press and
  the confirm (the gate refuses a dead
  squad with nothing changed, and Step 1
  must say whether the pending state
  clears, stays, or re-targets — it must
  not silently re-target a different live
  squad). A binding that works only when
  no dead squad is in the list has not
  answered the question.

## Slice 24, in order

### Step 1 — K re-rally binding

Implement the K binding in
`FpsInputAdapter.cs`, decided in that step
— not in this scope document — with the
reasoning recorded in DECISIONS.md:

- The step must decide which core gate K
  calls, and record it: the general gate
  alone, the stuck gate alone, or one
  tried before the other, with the reason.
  The ground fact it decides from is that
  the general gate's eligibility (live
  HoldGround or Patrol) already includes a
  stuck Patrol — the stuck gate adds a
  stuck requirement, not a wider reach —
  and both gates call the same `ReRally`
  with the same free price, so a single K
  binding can serve both item 39's stuck
  readings and item 40's general readings
  without two keys. If Step 1 finds a
  case where the two gates disagree about
  the same squad and rally, it must name
  that case and say which gate wins, not
  let the call order decide silently.
- The step must decide the pending-state
  shape, and record it: whether the
  re-rally pick carries its own adapter
  flag or reuses the picker's `IsPicking`
  flag, and what each choice does to the
  existing routing. Reusing the picker
  flag is dangerous in a way Step 1 must
  confront directly: while picking, Return
  deploys a squad at the pending rally
  and O cancels the pick, so a re-rally
  pick that merely sets `IsPicking` would
  turn a Return into a paid deploy at the
  new rally instead of a free re-rally of
  the target squad — unless Step 1
  reroutes Return (and O, and the
  doctrine/kind keys, and the mouse)
  while a re-rally is pending, and proves
  the rerouting by reading the adapter
  code, not by asserting it. An own-flag
  shape avoids that collision but must
  still answer the same routing questions
  for its own flag: what Return, Escape,
  O, the builder keys, and the mouse do
  while a re-rally is pending, and what
  clears the pending state (confirm,
  cancel, and any other exit Step 1
  decides, such as the target squad dying
  or the player pressing K again).
- The step must decide the rally
  selection, and record it: the re-rally
  pick reuses the picker's centre-screen
  raycast (30f, floored to a voxel cell,
  as the picker does), or it uses a
  different selection it justifies. It
  must say what the player reads while
  picking a new rally — whether the
  pending marker (orange sphere, as the
  picker uses) marks the candidate rally,
  whether the target squad's rally marker
  stays, moves, or hides during the pick,
  and whether any new marker or HUD word
  is added (the scope's expectation is
  none: the existing pending marker and
  the existing rally marker already read
  the two rallies, and a new word would
  be a HUD decision smuggled inside a
  bindings step). It must say what a
  same-rally confirm does: the core gate
  refuses it with nothing changed, and
  Step 1 must say whether the pending
  state clears on that refusal or stays
  open for a different rally.
- The step must decide the fail-closed
  cases, and record each with nothing
  changed except what the decision
  justifies: K while picking; K with the
  crosshair on no marker; K on a rally
  marker rather than a squad marker (a
  rally marker is not a squad, and Step 1
  must say whether it selects that
  squad's rally or selects nothing);
  K on an Advance or Defend Core marker
  (the general gate refuses, and Step 1
  must say whether the pick opens and
  then refuses on confirm, or never
  opens); K on a dead squad's stale
  marker, if one can exist between a
  death and the next marker refresh;
  Return with no pending re-rally; Escape
  with no pending re-rally; and a confirm
  whose target squad refuses by the time
  Return is pressed (moved doctrine is
  impossible — doctrine never changes —
  but death, and a same-rally pick, are
  possible, and Step 1 must name which
  refusals it handles). The mouse-fire
  question belongs here too: while a
  re-rally pick is pending, a click
  currently fires the weapon, and Step 1
  must decide whether it still fires or
  is suppressed, with the reason — a
  pending pick is a pick-like state, and
  the picker suppresses mouse fire while
  picking, so "fires anyway" needs a
  reason, not a shrug.
- The step must decide the checklist
  relation, and record it: no new item is
  expected, because items 39 and 40
  already contain the K binding
  observations including their fail-closed
  cases — but if Step 1 finds a binding
  behaviour with no home in either item
  (a cross-pending behaviour is the most
  likely candidate, if Step 2 has not yet
  decided it), it adds item 42 for that
  reading only, and says whether item 42
  extends, supersedes, or leaves standing
  items 39, 40, 37, 29, and
  orbital-command item 13. Implementation
  alone does NOT mark items 39 or 40
  passed: both stay unchecked until a
  Windows observation is made and
  recorded in DECISIONS.md, and Step 1
  must say so in its own entry rather
  than leaving the checklist ambiguous.
- The step must decide the verification,
  and record it honestly: adapter code is
  not compiled headlessly, so no test
  count proves K. Step 1's verification
  is a code reading (the new code paths
  traced against the existing `Update`
  routing, the picker routing, and the
  gate signatures), plus the standing
  suite and demo re-run unchanged. If
  Step 1 touches core at all — to add a
  helper, a passthrough, or a snapshot
  field for the marker-to-index
  translation — it must justify that
  touch as a bindings necessity, prove
  the existing proof sets still pass
  unchanged, and say so plainly: the
  scope's expectation is an adapter-only
  step, and a core touch is a scope
  deviation to be owned, not smuggled.

### Step 2 — X dismiss binding

Implement the X binding in
`FpsInputAdapter.cs`, decided in that step
— not in this scope document — with the
reasoning recorded in DECISIONS.md:

- The step must decide the pending-state
  shape, and record it: an adapter pending
  dismiss, as the proposal states, with
  Return confirming and Escape cancelling,
  and the core call (`TryDismissSquad`
  by squad-list index, via its
  `Simulation` passthrough) made only on
  confirm, never on X alone. It must say
  whether the pending dismiss reuses any
  part of Step 1's pending re-rally state
  (a shared pending-action flag, a shared
  confirm/cancel routing) or carries its
  own flag, and what that choice does to
  the routing Step 1 decided: in
  particular, Return and Escape must
  confirm or cancel exactly one pending
  action, never both, and never a deploy.
- The step must decide the cross-pending
  cases, and record each: X pressed while
  a re-rally pick is pending, and K
  pressed while a dismiss is pending.
  The candidates are cancel-first (the
  new key cancels the old pending state
  and opens its own), refuse-while-pending
  (the new key does nothing until the old
  pending state is confirmed or
  cancelled), and replace (the new key
  silently swaps the pending action) —
  and replace must be justified against
  the obvious hazard, which is a player
  confirming a dismiss they thought was
  a re-rally. Step 2 must also decide O
  while either pending state is open
  (the picker toggle must not open a
  deploy pick on top of a pending
  re-rally or dismiss), and the builder
  keys and mouse fire while a dismiss is
  pending, on the same reasoning Step 1
  used for a re-rally pick.
- The step must decide the
  marker-to-index translation, and record
  it: Step 1's translation reused, or a
  different translation justified — and
  if different, why two bindings that
  both target "the squad under the
  crosshair" may disagree about which
  squad that is. Eligibility differs from
  K and Step 2 must say so plainly: every
  live squad of every doctrine qualifies
  for dismiss, including Advance and
  Defend Core markers that K refuses, so
  an X-on-marker that K would refuse is
  not an error, it is the dismiss scope
  working. The dead-in-list divergence
  and the death-between-press-and-confirm
  case apply exactly as in Step 1, and
  Step 2 must say what a confirm does
  when its target died meanwhile (the
  gate refuses with nothing changed, and
  the pending state must clear, not
  re-target).
- The step must decide the fail-closed
  cases, and record each: X while
  picking; X with the crosshair on no
  marker; X on a rally marker rather
  than a squad marker; Return with no
  pending dismiss; Escape with no pending
  dismiss; a confirm whose target refuses
  (dead, or out of range after a list
  shift — Step 2 must say whether a list
  shift between press and confirm can
  retarget the confirm, and what prevents
  it if it cannot); and a cancelled
  confirmation leaving squad, ore, cap,
  and markers exactly as they were. It
  must also say what the player reads on
  confirm: the proposal adds no HUD word,
  and the scope's expectation is none —
  the squad's text line and both its
  markers vanish with its snapshot entry
  under the existing marker rule, as item
  41 already observes, and a new word
  would be a HUD decision smuggled
  inside a bindings step.
- The step must decide the checklist
  relation, and record it: no new item is
  expected, because item 41 already
  contains the X binding observation
  including its fail-closed cases — but
  if Step 2 finds a binding behaviour with
  no home in item 41 (a cross-pending
  behaviour Step 1 left open is the most
  likely candidate), it adds item 42 for
  that reading only, if Step 1 did not
  already take number 42, and states its
  relation to items 41, 40, 39, 37, 29,
  and orbital-command item 13. As with
  Step 1, implementation alone does NOT
  mark item 41 passed: it stays unchecked
  until a Windows observation is made
  and recorded in DECISIONS.md.
- The step must decide the verification,
  and record it honestly: as with Step 1,
  a code reading plus the standing suite
  and demo re-run unchanged, and no test
  count proves X. Any core touch is a
  scope deviation to be owned, as in
  Step 1.

### Step 3 — Slice 24 review

As with Slice 16's review — and unlike
the reviews of Slices 17–23, which were
each accompanied by a composed demo
scenario because their steps were
headless-testable — this review is
DOCUMENTATION ONLY, because no bindings
claim is headless-provable: the adapter
code is not compiled headlessly, no demo
scenario can stage a key press, and no
test count can prove a binding. The
review writes `docs/slice-24-review.md`,
checks the slice against this scope step
by step, states the verdict honestly
(two bindings implemented in adapter
code, read against the existing routing
and gate signatures, not playtested, not
Unity-verified), separates
adapter-implemented claims from the
Windows-checklist observations that alone
can close items 39, 40, and 41, carries
the gaps that remain, and does NOT scope
Slice 25 — Slice 25 is scoped afterwards,
the same way this document follows the
Slice 23 review rather than preceding it.
No demo scenario is added by this step:
a demo that staged K or X would be a
core-call demo in disguise, and the core
calls are already proved by Slices 20–22.
Every earlier demo save length must stand;
there is no new scenario length to quote.

## Verification, stated once for the whole slice

A bindings slice inverts the usual gate:
the test suite standing still is the
honest outcome, not a failure to prove.
Steps 1 and 2 add Unity adapter code that
the headless runner never compiles, so
the suite is expected to stand at
4577/4577 through both steps, and the
demo is expected to stand with every
earlier save unchanged, including Slice
23 at 44,041 chars. A step that raises
the suite must say what headless claim
it added and why a bindings step has one;
a step that lowers it, or breaks it, has
failed and is not pushed. What Steps 1
and 2 prove instead is a code reading,
recorded in DECISIONS.md: every new
branch traced against the existing
`Update` routing (movement, look, gather,
build, fire, picker, confirm/cancel),
every gate call checked against its core
signature and eligibility, every
fail-closed case walked to a "nothing
changed" exit, and the marker-to-index
translation walked through the
dead-in-list case. The Windows checklist
items 39, 40, and 41 are what close the
bindings, on Windows, with the Unity
version and GPU recorded in DECISIONS.md
— and until then, no step in this slice
claims a binding works in a scene.

## Deliberately NOT in Slice 24

- A core rule change of any kind. No
  movement, targeting, fire, delivery,
  precedence, conflict, rating, cost, or
  phase rule moves; no gate eligibility,
  price, cap, refund, radius, waypoint
  count, or loop shape moves; no save
  format moves. A core touch, if Step 1
  or 2 justifies one, is a bindings
  necessity (such as a translation
  helper), not a rule change, and must be
  labelled as such.
- An economy change disguised as a
  binding. The re-rally stays free and the
  dismiss stays refund none, in both
  steps, unless a later economy decision
  on Windows playtest evidence changes
  them — this slice implements keys, it
  does not reprice actions.
- A general squad-command system.
  Selecting, ordering, or commanding
  squads beyond the two decided bindings
  — including a follow order, a formation
  command, or a general order queue — is
  a larger slice, carried after Slice 24,
  not patched in beside a key binding.
- A new binding beyond K and X, a
  rebinding of an existing key, or a
  modifier scheme (such as K-plus-key)
  unless Step 1 or 2 justifies it as the
  answer to a routing collision it names
  — and a modifier invented to avoid
  deciding the pending-state routing is
  not a justification.
- A HUD redesign, a new HUD word, a
  marker shape or colour change, a
  marker-occlusion change, a material,
  light, fog, or view model change. The
  Slice 16 surface renders whatever Slice
  24 decides; a step that re-palettes a
  marker or adds a HUD line to make its
  binding read better has confused the
  surface with the binding.
- A scene file (`.unity`), an imported
  asset, a texture, or a shader file
  written by a headless run and claimed
  as a built scene. Scene assembly
  remains Windows checklist work.
- A copied asset, model, skyline, HUD
  layout, or trademarked look from any
  other game.
- Windows Unity verification itself. It
  remains open for Slices 1–24 alike and
  is blocked on Unity 6000 on Windows,
  not on anything this slice can do
  headlessly. Implementing K and X does
  not verify them.
- A Slice 25 scope. Slice 25 is scoped
  after the Slice 24 review, not inside
  it.

## Ranked candidates for Slice 25 scoping (not promised)

These are recorded here so the Slice 24
review starts from a list, not from
scratch, in the order the Slice 23 review
carried them, with the bindings taken:

1. Any Slice 13 remainder that Slice 24
   does not itself settle as an
   observation (the vertex-colour
   material observation, the MeshRenderer
   wiring observation, and the
   marker-occlusion observation, plus the
   dusk and view model observations in
   items 33 and 34, plus the tint, count,
   and patrol observations in items 35, 36,
   and 37, plus any new item a later step
   adds) — if those observations are
   still unmade when the review is
   written, they remain observations to
   be made when Windows verification runs,
   not steps to be re-scoped blindly, and
   not promised here. Items 39, 40, and
   41 leave this candidate's "unmade"
   set only when their Windows
   observations are actually made, not
   when their bindings are implemented.
2. First-pass graphics values retuned only
   on Windows evidence (dusk azimuth, fog
   contrast, view model offset, static
   weapon-agnostic model) — a retuning
   step is justified by an observation,
   not by a headless re-decision, and is
   not promised here.
3. A general re-rally price on Windows
   playtest evidence, if free
   repositioning proves abusive kiting —
   a later economy decision, recorded as
   a gap in Slice 21 Step 1, not set here.
4. A dismiss refund on Windows playtest
   evidence, if slot-only removal proves
   punishing — a later economy decision,
   recorded as a gap in Slice 22 Step 1,
   not set here.
5. A loop-shape decision, if ever wanted,
   as its own decision with its own tests
   — carried from the Slice 23 review's
   gaps, not promised here, and not to be
   confused with the radius and count
   values Slice 23 kept.

## Slice 24 gate (what "done" means)

- Every step pushed with passing tests —
  for this slice, with the suite standing
  at 4577/4577 through Steps 1 and 2
  unless a step justifies a rise with a
  headless claim it names, and never
  falling; the Step 3 review pushed with
  the suite standing, as a
  documentation-only step.
- The K binding implemented in adapter
  code, read against the existing routing
  and gate signatures, with its gate
  choice, pending-state shape, rally
  selection, marker-to-index translation
  (including the dead-in-list case), and
  fail-closed cases recorded with
  rationale in DECISIONS.md.
- The X binding implemented in adapter
  code on the same standard, with its
  pending-state shape, cross-pending
  answers, marker-to-index translation,
  and fail-closed cases recorded with
  rationale in DECISIONS.md.
- No core, economy, marker, material,
  light, fog, model, or HUD rule changed
  in any step except a bindings-necessity
  core touch a step justifies explicitly
  and proves against the standing proof
  sets; no demo scenario added; every
  earlier demo save length unchanged,
  with Slice 23 quoted at 44,041 chars.
- No checklist item marked passed from
  headless work: items 39, 40, and 41
  become attemptable, not passed, and any
  new item (42, if a step finds a reading
  with no home) states its relation to
  the standing items. The next free gate
  number after this slice is 42, or 43
  if item 42 is taken.
- This review records the verdict
  honestly, separating adapter-implemented
  claims from the Windows-checklist
  observations, including that Slice 24
  is still not playtested and not
  Unity-verified. The Slice 1 director
  playtest gate is still pending
  throughout and is not closed by
  Slice 24.
