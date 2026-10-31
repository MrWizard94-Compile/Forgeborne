# Forgeborne — Slice 26 Scope

**Status: SCOPED 2026-10-31, under delegation and
under the Slice 25 review's ranked candidates.
No Slice 26 system is built by this document.
Each step below is built one at a time, in this
order, and Slice 26 is closed by its own review
— it is not closed by finishing step 1 alone.**

## Scope question

Slice 24 implemented the K re-rally pick and the
X dismiss pick, and both steps decided the same
pending-state rule: the adapter does not poll
deaths, and a stale pending state is caught at
confirm instead. The consequence is stated in
both steps and carried in the Slice 24 and Slice
25 reviews, and it is still open: while a pick
is pending, the target squad can die and be
swept, and the pending state lingers until the
player presses Return, Escape, or K or X again.
For the dismiss pick the lingering is invisible
— a dismiss has no candidate cell to mark — so
the player only learns the pick is stale when
confirm clears it with nothing changed. For the
re-rally pick the lingering is visible: the
adapter-owned orange candidate sphere stays on
its last aimed cell after the squad it was
meant to move is gone, until confirm or cancel
hides it. The question no slice has yet
answered is therefore a routing question, not
a display question and not a core question: when
a pending pick's target stops matching the
re-validation triple, should the pending state
auto-clear on the next frame, or does
catch-at-confirm stand? Auto-clear would hide
the stale marker promptly and would make a
later Return do nothing instead of attempting a
confirm that is already known to fail; keeping
catch-at-confirm would keep the player in
explicit control of closing the pick, at the
cost of the lingering marker and the stale
pick. Slice 26 is therefore the pending
auto-clear slice, kept as small as the slices
before it: **decide the auto-clear question
first, with its adapter implementation, then
a documentation-only review** — not a twin
identity field, not a pending-dismiss visual
indicator, not a marker-parenting change, not
a same-frame key-priority redesign, not an
economy change, and not a claim of Unity
verification made from a headless run.
Deciding auto-clear does not pass any
playtest-gate item, and no step in this slice
marks one passed from headless work. If Step
1 keeps catch-at-confirm, the slice's value is
that the keep is now a decided answer with its
own weighing, not a first-pass routing choice
carried by inertia; if Step 1 implements
auto-clear, the slice's value is that the
change is named, bounded, and observed by a
new checklist item, not smuggled inside
another step.

## Inputs this scope is decided from

- The Slice 25 review's ranked candidates, in
  order, and what this scope does with each:
  candidate 1 (any Slice 13 remainder that
  Slice 25 does not itself settle as an
  observation — the vertex-colour material
  observation, the MeshRenderer wiring
  observation, and the marker-occlusion
  observation, plus the dusk and view model
  observations in items 33 and 34, plus the
  tint, count, and patrol observations in
  items 35, 36, and 37, plus items 39, 40, 41,
  42, and 43) remains observations to be made
  when Windows verification runs, not steps
  to be re-scoped blindly — and Slice 26
  changes that standing in no way: no step
  here observes a scene, because Unity 6000
  on Windows is still not ready (Rob
  confirmed 2026-10-31 that he hit a snag
  and then got busy with work), and scoping
  an observation as a headless step would
  manufacture a step that cannot move.
  Candidate 2 (first-pass graphics values
  retuned only on Windows evidence — dusk
  azimuth, fog contrast, view model offset,
  static weapon-agnostic model) is not
  Slice 26 work: no Windows observation has
  yet justified a retune, and a routing step
  must not retune a light, fog, or model
  value to pay for deciding a pending rule.
  Candidate 3 (a general re-rally price on
  Windows playtest evidence, if free
  repositioning proves abusive kiting) and
  candidate 4 (a dismiss refund on Windows
  playtest evidence, if slot-only removal
  proves punishing) are not Slice 26 work
  either: both are economy decisions gated
  on playtest evidence that does not exist
  yet, and a routing step must not reprice
  either action. Candidate 5 (the Slice 24
  adapter gaps, weighed as their own step
  only on the evidence Slice 25 leaves
  standing — the identical-twin re-validation
  limit, the invisible pending dismiss, the
  unparented re-rally pending marker,
  same-frame K+X branch order, and pending
  state not auto-clearing on a target death)
  IS Slice 26's work, in the narrowed form
  below. It is the only ranked candidate
  that contains a headlessly decidable
  routing step rather than a Windows
  observation, a Windows-evidence-gated
  economy decision, or a display change
  gated on playtest confusion. The narrowing
  is the point of this scope, and Step 1
  must respect it: of the five gaps, only
  the auto-clear gap is taken here. The
  other four are carried after Slice 26, not
  bundled into it, for the reasons given
  under "The other four gaps" below. A
  step that closes more than one gap at
  once would be exactly the bundling the
  Slice 25 review forbids, and Step 2 must
  call it out as a scope deviation if it
  happens.
- The other four gaps, and why none of them
  is Slice 26 work. The identical-twin limit
  is a core and save-format decision, not an
  adapter routing patch: closing it needs a
  stable squad identity the save does not
  carry, and its consequence is bounded —
  a free re-rally of the wrong twin, or
  dismissing the wrong twin with one slot
  freed and no refund either way — so it is
  weighed by a later step on its own, not
  paid for by an auto-clear step that uses
  the same triple and therefore passes the
  same twin. The invisible pending dismiss
  is a display question gated on playtest
  evidence: the Slice 24 review says a
  later display step should weigh a
  pending-dismiss reading IF Windows
  playtest finds the invisibility confusing,
  and no playtest has found that yet, so
  inventing a highlight or HUD word now
  would be a display change without its
  evidence. The unparented re-rally pending
  marker is a Windows observation, not a
  headless claim: whether its unparented
  placement reads correctly in a scene can
  only be observed there, and giving the
  adapter a MarkerRoot field would be a
  second, unscoped scene-wiring change.
  Same-frame K+X branch order is routing
  determinism recorded as such, and whether
  it reads as a key priority in play is a
  Windows observation, not a headless
  re-decision. None of these four is
  demoted; each waits on the evidence or
  the decision type its own review names,
  and all four are carried as the ranked
  Slice 27 candidates in the review's own
  terms, not promised here.
- Ground facts this scope is decided from,
  verified this run against the code as it
  stands, not copied from review prose: the
  Update tail in `FpsInputAdapter.cs` runs
  the pending branches first — dismiss
  first, then re-rally, then the picker
  branch, then the opener else-branch — so
  no frame can set both pending flags, and
  Return confirms exactly one pending
  action. In the opener else-branch K is
  checked before X, so a same-frame K+X
  press opens the re-rally pick only, by
  branch order, not by a designed key
  priority. `HandlePendingDismiss` reads
  only Return, Escape, and X again, and
  does no per-frame snapshot work at all.
  `HandlePendingReRally` reads Return,
  Escape, and K again, and otherwise runs
  the centre raycast and `ShowReRallyMarker`
  every frame, so the re-rally pick already
  pays a per-frame cost while pending and
  the dismiss pick currently pays none.
  Both confirms re-validate the same way:
  a fresh `OrbitalView.Snapshot` is read,
  and the stored snapshot position must
  still hold a squad with the same doctrine,
  kind, and rally (the old rally, for a
  re-rally pick). Position and health are
  deliberately excluded — a walking squad
  changes cell every tick, and an enemy hit
  mid-decision must not void a confirm —
  and the pending state is cleared whether
  the gate succeeds or refuses, so a
  confirm is one attempt, never a retry
  loop. `SnapshotPosToGateIndex` walks
  `World.Squads` skipping dead squads,
  exactly as the snapshot does, and returns
  -1 when the live list has shrunk past the
  stored position. `OrbitalView.Snapshot`
  in `Orbital.cs` allocates a new list on
  every call and walks the squads skipping
  dead ones, so a per-frame poll is one
  list allocation per frame, but only
  while a pending flag is set — never on
  ordinary frames, where no pending state
  exists to poll. `CancelPendingReRally`
  hides the pending marker as well as
  clearing the flag; `CancelPendingDismiss`
  clears only the flag, because a dismiss
  has no marker to hide. While either pick
  is pending, O, the builders, the picker
  keys, and mouse fire are all suppressed,
  and E, Q, and R keep their pre-existing
  behaviour. The deploy price (10 EmberOre)
  and the cap (3 live squads) stand, the
  re-rally stays free, and the dismiss
  stays refund none with one slot freed.
  The next free playtest-gate number is 44
  (items run to 43). Tests stand at
  5053/5053 before this scope, with the
  Slice 25 demo save at 44,041 chars; a
  scope step is expected to leave both
  standing, for the reason given under
  "Verification" below, and a step that
  moves either must say why.

## The auto-clear question Step 1 must answer

This is the one question no earlier slice
answers, and it must be decided in Step 1
and implemented, not assumed — or Step 1
must say why auto-clear cannot be decided
headlessly and defer itself, rather than
quietly keeping a routing rule it has not
weighed. The question is not "which
behaviour feels nicer". It is: when the
target a pending pick names stops matching
its re-validation triple, does the adapter
clear the pick on the next frame, or does
it keep the pick until the player closes
it? Step 1 must answer it in this order,
with a mechanism for each answer, not a
paragraph of taste:

- One decision per binding, with symmetry
  stated, not assumed. Slice 24 kept the
  two bindings symmetric wherever the
  routing allowed it, but the auto-clear
  case is not symmetric in what the player
  sees: a stale re-rally pick leaves a
  visible orange sphere on a cell whose
  squad is gone, while a stale dismiss
  pick leaves nothing visible at all.
  Step 1 must therefore decide the
  re-rally case and the dismiss case
  separately, and must say, for each, whether
  auto-clear, keep, or a third routing is
  chosen — and if the two answers differ,
  what mechanism justifies breaking the
  symmetry Slice 24 built. A single
  sentence covering both bindings is not
  a decision for both bindings.
- Poll mechanism, if auto-clear is chosen
  for either binding: reuse the confirm's
  triple check exactly, or use a cheaper
  signal, and why. The triple — same
  doctrine, kind, and rally at the stored
  snapshot position — is the only check
  already proved against the confirm path,
  and reusing it keeps one rule in one
  place: a poll that passed what confirm
  would refuse, or refused what confirm
  would pass, would be two rules wearing
  one name. A cheaper signal must be
  weighed against its false-clear case:
  a snapshot-count check, for example,
  would auto-clear a pick whose target is
  still alive whenever ANY squad dies and
  is swept, because the live count drops
  even though the target's own triple
  still matches — so a count-only poll is
  not a shortcut to the triple, it is a
  different rule that clears live picks.
  Step 1 must name the mechanism it picks
  and its false-clear case, or keep
  catch-at-confirm on the ground that no
  mechanism is clean enough.
- Poll placement, if auto-clear is chosen:
  at the top of each `HandlePending`
  method before any key read, or in the
  Update tail before the pending branch,
  and what happens on the death frame
  itself. Placement changes what a Return
  on the death frame does: a poll before
  key handling makes that Return do
  nothing (the pick is already gone), while
  a poll after key handling lets that
  Return attempt a confirm that clears
  with nothing changed — the same final
  state, but a different reading of the
  key the player just pressed. Step 1 must
  pick one placement and state the
  death-frame Return reading, and must
  place the poll so it can only clear a
  pending state, never open one: a poll
  that ran in the opener branch, or that
  re-targeted after clearing, would be a
  new opener smuggled inside a clearer.
- Per-frame cost, stated as a bound, not a
  feeling. A triple poll costs one
  snapshot list allocation per frame while
  a pick is pending, and nothing on any
  other frame. For the re-rally pick that
  cost joins a per-frame raycast and
  marker update that already exist, so the
  marginal cost is one list. For the
  dismiss pick it is a new per-frame cost
  where none exists today, because
  `HandlePendingDismiss` currently reads
  keys only. Step 1 must state whether
  that bound is acceptable for a
  short-lived player decision state, and
  must not claim a cost it has not
  bounded: "once per frame while pending"
  is the bound, "cheap" is not.
- Re-press semantics after auto-clear,
  stated, not discovered in play. Today,
  K again or X again cancels a stale pick.
  After an auto-clear, the flags are
  already down, so the same key press in
  the opener branch opens a FRESH pick on
  whatever marker the crosshair now sits
  on — a new target, not a cancelled old
  one. Step 1 must say whether that
  re-press reading is acceptable, and for
  the dismiss case in particular whether
  the invisibility of the auto-clear makes
  the re-press surprising: the player who
  pressed X, watched nothing visible
  change, and presses X again after the
  target's death would open a new dismiss
  pick without ever having seen the first
  one close. If that surprise is a reason
  to keep catch-at-confirm for dismiss
  while auto-clearing re-rally, Step 1
  must say so; if it is not, Step 1 must
  say why the re-press path is safe.
- Twin limit, explicitly NOT closed.
  Whatever poll Step 1 picks, if it reuses
  the confirm's triple it passes the same
  identical twin that confirm passes: a
  twin (same doctrine, kind, and rally)
  shifting into the stored snapshot
  position after the target's death and
  sweep matches the triple, so the poll
  would keep the pick and confirm would
  act on the wrong twin, exactly as today.
  Step 1 must state this in its own words,
  must not claim auto-clear as a twin fix,
  and must invent no identity field to
  close it: that field is a core and
  save-format decision carried after
  Slice 26, and a routing step that
  invented one would have smuggled a core
  change inside an adapter step.
- Checklist relation, decided in Step 1,
  not left to Step 2. Items 42 (re-rally
  pending-state routing) and 43 (dismiss
  pending-state routing) already observe
  the pending routing an auto-clear change
  touches. Step 1 must state whether an
  auto-clear implementation needs a new
  item: a new item only if Step 1 finds a
  player-visible auto-clear reading with
  no home in items 42 or 43 — the prompt
  hiding of a stale candidate marker, and
  the re-press reading after an
  auto-clear, are the readings to weigh —
  in which case the new item takes the
  next free number, 44, EXTENDS items 42
  and 43 as applicable, supersedes none of
  them, and states its own Windows
  observation (see below). If Step 1
  keeps catch-at-confirm for both
  bindings, no new item is needed, as a
  keep needed none in Slice 23 and Slice
  25, and item 44 stays free. An
  auto-clear that adds no item without
  saying why, or a keep that adds one
  anyway, is a scope deviation Step 2
  must call out in its review.
- What Windows observation would prove it.
  Step 1 must write the observation its
  item 44 (if added) would carry, honestly
  including its staging difficulty: in one
  scene, open a pick on a live squad's
  marker, have that squad die and be swept
  while the pick is still pending, and
  observe — for re-rally, that the orange
  candidate sphere hides promptly without
  any key press, and that Return then does
  nothing; for dismiss, that Return then
  does nothing and that X again opens a
  fresh pick rather than cancelling a
  stale one. The staging itself must be
  named, not gestured at: a target death
  on demand needs an enemy already
  engaging the target, or a staged lethal
  hit the scene can actually deliver, and
  if no such staging exists in the
  checklist scene, item 44 stays
  attemptable with that staging note, not
  faked from a headless run. No headless
  test proves a binding, and Step 1 must
  not claim otherwise.

## Steps, in order

- Step 1, the pending auto-clear decision
  plus its adapter implementation, decided
  in that step: auto-clear, keep, or a
  third routing, decided separately for
  the re-rally pick and the dismiss pick,
  on the weighing above, with the poll
  mechanism, placement, cost bound,
  re-press semantics, twin limit, and
  checklist relation all recorded in
  DECISIONS.md, with the same decision
  summarised in CHANGELOG. Adapter-only:
  one code file is expected
  (`FpsInputAdapter.cs`), no core file
  moves, no save format moves, no economy,
  movement, targeting, fire, marker,
  material, light, fog, model, or HUD rule
  moves except what the auto-clear decision
  itself requires and proves — and hiding
  the existing pending marker through the
  existing cancel path is not a new marker
  or display change, while a new highlight,
  HUD word, or marker shape would be, and
  is not this step. No demo scenario
  belongs to Step 1 alone, per the Slice
  24 precedent: adapter code is not
  compiled headlessly, so no headless demo
  can stage a pending pick, and the proof
  for this step is a code reading plus the
  suite standing, as Slice 24's steps
  were. If Step 1 keeps catch-at-confirm
  for both bindings, no code file needs to
  move except a comment correction Step 1
  justifies explicitly, as Slice 23 Step 1
  corrected the radius comment.
- Step 2, the documentation-only review
  closing Slice 26, per this scope and
  following the Slice 16 precedent for
  claims no headless run can prove: a
  review checking this scope step by step,
  separating headless code-reading claims
  from the Windows observations that alone
  can close items 42, 43, and any new item
  44 Step 1 adds, and carrying the ranked
  Slice 27 candidates. Slice 27 is NOT
  scoped here; it is scoped afterwards,
  chosen from the ranked candidates the
  review carries.

## Verification

- Adapter code is not compiled headlessly,
  so a scope step's proof is a code reading
  and a suite run, recorded here: every
  ground fact above was read in
  `FpsInputAdapter.cs` and `Orbital.cs`
  this run (Update tail branch order,
  dismiss-first ordering, K-before-X
  opener order, the two `HandlePending`
  methods, the two confirms' triple
  re-validation, `SnapshotPosToGateIndex`,
  the snapshot allocation rule, the two
  cancel methods, and the while-pending
  suppression set), every candidate
  disposition was checked against the
  Slice 25 review's ranked list, and the
  standing suite and demo were re-run
  before any edit. A scope step that moves
  a test count or a demo save length must
  say why; this scope moves neither,
  because a document decides no routing
  and proves no key.
- Standing suite and demo re-run unchanged
  in this scoping run: 5053/5053 tests,
  Slice 25 demo save 44,041 chars, every
  earlier save unchanged (Slice 23 44,041,
  Slice 22 43,990, Slice 21 44,040, Slice
  20 44,041, Slice 19 43,965, Slice 18
  44,015, Slice 15 44,016, Slice 17 43,974
  chars, and all earlier saves as in the
  0.102.0 entry). No new scenario exists,
  so no new length is quoted. This
  re-check does not compile an adapter,
  run a scene, press a key, or observe a
  marker — those remain Windows work, and
  no step in Slice 26 claims otherwise.

## Slice 26 gate (what "done" means)

- Every step pushed with passing tests —
  for this slice, with the suite standing
  at 5053/5053 through the scope unless a
  step justifies a rise with a headless
  claim it names, and never falling; the
  Step 2 review pushed with the suite
  standing, as a documentation-only step.
- The Step 1 auto-clear decision recorded
  with rationale in DECISIONS.md: one
  decision per binding, the poll mechanism
  and its false-clear case, the placement
  and the death-frame Return reading, the
  per-frame cost bound, the re-press
  semantics, the twin limit stated as not
  closed, and the checklist relation, each
  stated, not implied.
- No core, save-format, economy, movement,
  targeting, fire, marker, material, light,
  fog, model, or HUD rule changed in any
  step except an auto-clear-necessity change
  a step justifies explicitly and proves
  against the standing proof sets; no demo
  scenario added; every earlier demo save
  length unchanged, with Slice 25 quoted
  at 44,041 chars.
- No checklist item marked passed from
  headless work: items 42 and 43 observe
  the pending routing as it stands, and
  any new item (44, if Step 1 finds a
  reading with no home) states its relation
  to the standing items. The next free gate
  number after this slice is 44, or 45 if
  item 44 is taken.
- The Step 2 review records the verdict
  honestly, separating headless-proved
  claims from the Windows-checklist
  observations, including that Slice 26
  is still not playtested and not
  Unity-verified. The Slice 1 director
  playtest gate is still pending
  throughout and is not closed by Slice 26.

## Deliberately NOT in Slice 26

- A core rule change of any kind, a
  save-format change, or a squad identity
  field invented to close the twin limit.
  The twin limit is carried after Slice 26,
  not closed by it.
- A pending-dismiss visual indicator, a
  HUD word, a marker shape or colour
  change, or any other display change.
  Hiding the existing pending marker
  through the existing cancel path is the
  only marker act this slice allows, and
  only as the auto-clear's own consequence.
- A marker-parenting change (no MarkerRoot
  field), a same-frame K+X priority
  redesign, or a re-press rebinding. The
  K-before-X opener order stands unless
  Step 1 justifies a change as an
  auto-clear necessity, which it must
  prove rather than assume.
- An economy change of any kind. The
  deploy price (10) and the cap (3)
  stand, the re-rally stays free, and the
  dismiss stays refund none with one slot
  freed.
- A general squad-command system, a
  material, shader, light, fog, or view
  model change, a HUD redesign, or a new
  binding beyond K and X.
- A scene file (.unity), an imported asset,
  a texture, or a shader file written by a
  headless run and claimed as a built
  scene. Scene assembly remains Windows
  checklist work.
- A copied asset, model, skyline, HUD
  layout, or trademarked look from any
  other game.
- Windows Unity verification itself, the
  director playtest, or a Slice 27 scope.

## Ranked candidates for Slice 27 scoping (not promised)

These are recorded here so the Slice 26
review starts from a list, not from
scratch, in the order the Slice 25 review
carried them, with the auto-clear gap taken:

1. Any Slice 13 remainder that Slice 26
   does not itself settle as an observation
   (the vertex-colour material observation,
   the MeshRenderer wiring observation, and
   the marker-occlusion observation, plus
   the dusk and view model observations in
   items 33 and 34, plus the tint, count,
   and patrol observations in items 35, 36,
   and 37, plus items 39, 40, 41, 42, and
   43, plus any new item a later step
   adds) — if those observations are still
   unmade when the review is written, they
   remain observations to be made when
   Windows verification runs, not steps to
   be re-scoped blindly, and not promised
   here. Items 39, 40, 41, 42, and 43 leave
   this candidate's "unmade" set only when
   their Windows observations are actually
   made, not when their bindings were
   implemented or their routing was decided.
2. First-pass graphics values retuned only
   on Windows evidence (dusk azimuth, fog
   contrast, view model offset, static
   weapon-agnostic model) — a retuning step
   is justified by an observation, not by
   a headless re-decision, and is not
   promised here.
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
5. The remaining Slice 24 adapter gaps,
   weighed as their own steps only on the
   evidence Slice 26 leaves standing — the
   identical-twin re-validation limit, the
   invisible pending dismiss, the
   unparented re-rally pending marker, and
   same-frame K+X branch order — carried,
   not promised here, and not to be
   confused with a shape, economy, or
   graphics decision: a display or routing
   step for these gaps must say which gap
   it closes and what Windows observation
   would prove it, rather than bundling
   all four into one step.
