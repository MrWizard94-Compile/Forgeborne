# Forgeborne — Slice 26 Review

**Status: REVIEWED HEADLESSLY 2026-10-31, tests 5053/5053.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
16, 17, 18, 19, 20, 21, 22, 23, 24, and 25. This review closes
Slice 26 as a reviewed whole; it does not claim Slice 26 is
playtested, Unity-verified, or a visual result.**

## Review question

Does Slice 26 settle the pending auto-clear question — when a
pending pick's target stops matching its re-validation triple,
does the adapter clear the pick on the next frame, or does
catch-at-confirm stand — as its own decision with its own
adapter implementation, then a documentation-only review,
without a twin identity field, a pending-dismiss visual
indicator, a marker-parenting change, a same-frame K+X
priority redesign, an economy change, a core or save-format
change, a demo scenario staged for a key press no headless
run can press, or a claim of Unity verification made from a
headless run? The answer headlessly is yes for the
AUTO-CLEAR BOTH decision as decided, with the split this
review keeps throughout: the routing is implemented and
read, the scene observation of the routing is still owed on
Windows, and the one new checklist item is an observation,
not a pass.

## What Slice 26 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.103.0 scope | `docs/slice-26-scope.md`: Slice 26 scoped as the pending auto-clear slice, in order — the auto-clear decision plus its adapter implementation first, then a documentation-only review. Slice 27 is not scoped there. Ranked Slice 27 candidates recorded, not promised | Decision document only, no system built by it | Tests 5053/5053 unchanged |
| 0.104.0 Step 1 | Auto-clear decision decided: AUTO-CLEAR BOTH pending picks, symmetrically, with the poll mechanism, placement, cost bound, re-press semantics, twin limit, and checklist relation all recorded, plus the adapter implementation | One code file, `ForgeborneUnity/Assets/Scripts/FpsInputAdapter.cs` (auto-clear polls, shared triple helper, confirms refactored to it, stale "does not poll" comments corrected); `docs/orbital-command.md` (pending auto-clear section); `docs/slice-01-playtest-gate.md` (new item 44) | Tests 5053/5053 unchanged, as expected: adapter code is not compiled headlessly, so no headless test proves a binding, and no demo scenario belongs to this step alone |
| 0.105.0 Step 2 | This review, closing Slice 26 | Review document plus CHANGELOG and DECISIONS entries; no code file, no core file, no adapter file, no checklist item added beyond what Step 1 decided, no demo scenario | Tests 5053/5053 unchanged, documentation-only step; every earlier demo save length unchanged, Slice 25 quoted at 44,041 chars |

## Why there is no Step 2 demo, stated plainly

The scope asked for a documentation-only review, not a
composed demo, and this step follows it for the reason the
scope and the Slice 24 precedent give: adapter code is not
compiled headlessly, so no headless demo can stage a pending
pick, a death while a pick is pending, or a Return on the
death frame. A demo scenario for this slice would have had
to fake a key press in core, which would prove a core path
no player ever walks — the pending state lives in the
adapter, not in core. The proof for this slice is therefore
a code reading plus the suite standing, as Slice 24's steps
were, and the review below re-reads the adapter rather than
repeating Step 1's record on trust. Nothing is claimed here
that a demo would have claimed: no auto-clear is shown
happening, only implemented and read.

## Scope check, step by step

- Scope, 0.103.0: the scope narrowed the Slice 25 review's
  candidate 5 to exactly one gap — pending state not
  auto-clearing on a target death — and carried the other
  four gaps after Slice 26 for the reasons it gives, rather
  than bundling them. Candidates 1 through 4 were disposed
  of as not Slice 26 work: candidate 1 remains Windows
  observations, not steps to be re-scoped blindly; candidate
  2 is a retune justified only by a Windows observation
  that does not exist yet; candidates 3 and 4 are economy
  decisions gated on playtest evidence that does not exist
  yet. The scope's ground facts were read in the adapter
  and in `Orbital.cs` in that run, not copied from review
  prose, and this review re-verified the ones Step 1 leans
  on (see below). No system was built by the scope, and no
  answer was decided there — the auto-clear question was
  kept a question until Step 1. No scope deviation is found
  in the scope step.
- Step 1, the auto-clear decision plus implementation: the
  scope asked for one decision per binding, a poll
  mechanism with its false-clear case, a placement with its
  death-frame Return reading, a per-frame cost stated as a
  bound, re-press semantics, the twin limit stated as not
  closed, and a checklist relation decided in the step, not
  left to this review. The step's record answers each, and
  this review re-read the code behind each answer:
  - One decision per binding, symmetry stated, not assumed.
    Both answers are AUTO-CLEAR, on purpose, with the
    visibility difference weighed and found not to justify
    breaking the symmetry Slice 24 built: a stale re-rally
    pick leaves a visible orange sphere on a cell whose
    squad is gone, while a stale dismiss pick leaves
    nothing visible — but the dismiss target's own text
    line and markers already vanish on death and sweep, so
    the player is not left without a death signal, and one
    rule for both picks is the rule a player can learn
    once. No single-sentence covering of both bindings; the
    two cases are decided separately and land the same way.
  - Poll mechanism: reuse of the confirm's triple exactly,
    through one shared adapter helper, `PendingTargetMatches`
    (snapshot position, doctrine, kind, rally — old rally
    for a re-rally pick; position and health excluded; a
    stored position past the shrunk live list is a
    mismatch). Both confirms were refactored to call the
    same helper, so poll and confirm are one rule in one
    place, not two copies that could drift. The cheaper
    signal was rejected on its false-clear case, as the
    scope warned: a snapshot-count poll would clear a pick
    whose target is still alive whenever ANY squad dies
    and is swept, because the live count drops even though
    the target's own triple still matches. This review
    re-read the helper and both refactored confirms: the
    helper reads a fresh `OrbitalView.Snapshot`, returns
    false past the list end, and compares doctrine, kind,
    and rally only, exactly as the confirms did before the
    refactor, and the confirms keep their fail-closed
    backstop — they still clear pending on any outcome —
    so a mismatch reaching a confirm is a backstop, not a
    first catch, and no confirm path was weakened by the
    refactor.
  - Poll placement and the death-frame Return reading: at
    the top of each `HandlePending` method, before any key
    read, in the branch that runs only while that pick's
    flag is set. This review re-read the Update tail: the
    dismiss branch runs before the re-rally branch, the
    picker branch, and the opener else-branch, and each
    handler returns at once on a poll mismatch, so no key
    is read on the auto-clear frame. The death-frame
    reading therefore holds as code-reading: `Simulation.Tick`
    runs in Update well before the pending branch (line
    139 against the pending branch near line 466), so a
    squad that dies on a tick is already dead when that
    frame's pending branch runs, the poll clears the pick
    on the death frame itself, and a Return pressed on that
    frame does nothing — the pick is already gone —
    instead of attempting a confirm already known to fail.
    The final state matches catch-at-confirm (pick closed,
    nothing changed); the key reading differs, and Step 1
    states that difference rather than letting it be
    discovered in play. The poll can only clear, never
    open: it runs only inside a handler that runs only
    while its flag is set, and the opener else-branch is
    untouched, so no poll can set a flag or re-target
    after clearing.
  - Per-frame cost, stated as a bound: one snapshot list
    allocation per frame while a pick is pending, nothing
    on any other frame. For the re-rally pick that joins
    the per-frame raycast and marker update that already
    exist, so the marginal cost is one list; for the
    dismiss pick it is a new per-frame cost where none
    existed, accepted as bounded for a short-lived player
    decision state, not claimed as merely "cheap". This
    review re-read both handlers: the dismiss handler
    previously read keys only and now polls first, and
    the re-rally handler polls before its raycast and
    marker update, so on an auto-clear frame the marker
    is hidden through the existing cancel path rather than
    updated — the only marker act this slice allows.
  - Re-press semantics after auto-clear, stated, not
    discovered in play: after an auto-clear the flags are
    already down, so the same key press in the opener
    branch opens a FRESH pick on whatever marker the
    crosshair now sits on — a new target, not a cancelled
    old one — exactly as after an Escape cancel. The
    dismiss surprise the scope feared is bounded, as Step 1
    states, by two facts this review re-read in the
    opener: the target's own display is gone, so X again
    aims at a live marker or at nothing (which opens
    nothing), and X-on-a-marker always means "open a
    dismiss pick on this marker" in every other case, so
    the fresh pick is the same action the key always
    performs, not a hidden reinterpretation of a cancel.
  - Twin limit, explicitly NOT closed: a twin (same
    doctrine, kind, and rally) shifted into the stored
    snapshot position after the target's death and sweep
    matches the triple, so the poll keeps the pick and
    confirm acts on the wrong twin, exactly as before. No
    identity field was invented, no core file moved, no
    save format moved. Auto-clear is a routing promptness
    change, not a twin fix, and Step 1 says so in its own
    words in the code comments, in `docs/orbital-command.md`,
    and in DECISIONS.md — not as a footnote, but as a
    stated limit in all three places.
  - Checklist relation, decided in Step 1, not left to
    this review: new item 44 IS added, taking the next
    free number, because the auto-clear readings have no
    home in items 42 or 43 — neither item stages a death
    while a pick is pending. Item 44 EXTENDS items 42 and
    43, supersedes none of them, states explicitly that
    item 43's X-again-cancels reading still applies while
    a dismiss is still live, and states its own Windows
    observation with its staging difficulty: an enemy
    already engaging the target, or a staged lethal hit
    the scene can actually deliver, and if no such staging
    exists in the checklist scene, item 44 stays
    attemptable with that note, not faked from a headless
    run. This review re-read item 44 in full and found its
    relation wording consistent with Step 1's record. The
    next free gate number after this slice is 45.
  - No scope deviation is found in Step 1: one code file
    moved (`FpsInputAdapter.cs`), no core file, no save
    format, no economy value (deploy price 10 and cap 3
    stand, re-rally stays free, dismiss stays refund none
    with one slot freed), no movement, targeting, fire,
    marker, material, light, fog, model, or HUD rule —
    except hiding the existing pending marker through the
    existing cancel path, which the scope allows as the
    auto-clear's own consequence. No new binding, no
    K-before-X opener change, no MarkerRoot field, no
    pending-dismiss visual indicator, no same-frame K+X
    priority redesign. No demo scenario, per the scope and
    the Slice 24 precedent, and the suite stands at
    5053/5053 rather than rising on a claim no headless
    test can make.
- Step 2, this review: documentation only, per the scope
  and the Slice 16 precedent for claims no headless run can
  prove. It checks the scope step by step, separates
  headless code-reading claims from the Windows observations
  that alone can close items 42, 43, and 44, and carries the
  ranked Slice 27 candidates. Slice 27 is NOT scoped here.

## Claim split, honestly kept

Headless-read, in this slice: both pending handlers poll
their target triple at their own top, before any key read,
through one shared helper, and auto-clear a mismatch at
once; the poll can only clear, never open; a Return on the
death frame does nothing because the pick is already gone;
K or X again after an auto-clear opens a fresh pick on the
current crosshair marker, as after any cancel; the confirms
still re-validate through the same helper as a backstop and
still clear pending on any outcome; the per-frame cost is
one snapshot list allocation per frame while a pick is
pending and nothing otherwise; the twin limit is not closed
by any of this. Still owed on Windows, not proved here:
whether the orange candidate sphere hides promptly with no
key press when a re-rally target dies mid-pick, whether
Return then does nothing for both picks, and whether X
again after a dismiss auto-clear opens a fresh pick rather
than cancelling a stale one — item 44, attemptable and
unchecked, with its staging difficulty stated, not faked.
The Slice 1 director playtest gate is still pending
throughout and is not closed by Slice 26.

## Checklist relation

One new playtest-gate item, decided in Step 1: item 44
(pending auto-clear), which EXTENDS items 42 (re-rally
pending-state routing) and 43 (dismiss pending-state
routing) and supersedes none of them, and leaves items 39,
40, 41, 42, and 43 standing with their own observations
still owed. No item is marked passed from headless work in
this slice: items 42 and 43 observe the pending routing as
it stands, item 44 observes the auto-clear readings, and
all three close only on their Windows observations recorded
in DECISIONS.md with the Unity version and GPU. The next
free gate number is now 45.

## Deliberately NOT in Slice 26

- A core rule change of any kind, a save-format change, or
  a squad identity field invented to close the twin limit.
  The twin limit is carried after Slice 26, not closed by it.
- A pending-dismiss visual indicator, a HUD word, a marker
  shape or colour change, or any other display change.
  Hiding the existing pending marker through the existing
  cancel path is the only marker act this slice allows, and
  only as the auto-clear's own consequence.
- A marker-parenting change (no MarkerRoot field), a
  same-frame K+X priority redesign, or a re-press
  rebinding. The K-before-X opener order stands.
- An economy change of any kind. The deploy price (10) and
  the cap (3) stand, the re-rally stays free, and the
  dismiss stays refund none with one slot freed.
- A general squad-command system, a material, shader, light,
  fog, or view model change, a HUD redesign, or a new
  binding beyond K and X.
- A demo scenario staged for a key press, or any claim that
  a headless run pressed a key, observed a marker, or
  verified a binding in a scene.
- A scene file (.unity), an imported asset, a texture, or a
  shader file written by a headless run and claimed as a
  built scene. Scene assembly remains Windows checklist work.
- A copied asset, model, skyline, HUD layout, or trademarked
  look from any other game.
- Windows Unity verification itself, the director playtest,
  or a Slice 27 scope.

## Ranked candidates for Slice 27 scoping (not promised)

These are recorded here so the Slice 27 scoping starts from
a list, not from scratch, in the order the Slice 26 scope
carried them, with the auto-clear gap taken:

1. Any Slice 13 remainder that Slice 26 does not itself
   settle as an observation (the vertex-colour material
   observation, the MeshRenderer wiring observation, and
   the marker-occlusion observation, plus the dusk and view
   model observations in items 33 and 34, plus the tint,
   count, and patrol observations in items 35, 36, and 37,
   plus items 39, 40, 41, 42, and 43, plus item 44) — if
   those observations are still unmade when the scoping is
   written, they remain observations to be made when
   Windows verification runs, not steps to be re-scoped
   blindly, and not promised here. Items 39, 40, 41, 42, 43,
   and 44 leave this candidate's "unmade" set only when
   their Windows observations are actually made, not when
   their bindings were implemented or their routing was
   decided.
2. First-pass graphics values retuned only on Windows
   evidence (dusk azimuth, fog contrast, view model offset,
   static weapon-agnostic model) — a retuning step is
   justified by an observation, not by a headless
   re-decision, and is not promised here.
3. A general re-rally price on Windows playtest evidence, if
   free repositioning proves abusive kiting — a later
   economy decision, recorded as a gap in Slice 21 Step 1,
   not set here.
4. A dismiss refund on Windows playtest evidence, if
   slot-only removal proves punishing — a later economy
   decision, recorded as a gap in Slice 22 Step 1, not set
   here.
5. The remaining Slice 24 adapter gaps, weighed as their own
   steps only on the evidence Slice 26 leaves standing —
   the identical-twin re-validation limit, the invisible
   pending dismiss, the unparented re-rally pending marker,
   and same-frame K+X branch order — carried, not promised
   here, and not to be confused with a shape, economy, or
   graphics decision: a display or routing step for these
   gaps must say which gap it closes and what Windows
   observation would prove it, rather than bundling all
   four into one step.

## Slice 26 gate (what "done" means, and whether it is met)

- Every step pushed with passing tests: scope 5053/5053
  unchanged, Step 1 5053/5053 unchanged, Step 2 5053/5053
  unchanged — met. The suite standing, rather than rising,
  is the expected shape for an adapter-only slice: no
  headless test proves a binding, so a rising count here
  would have been a claim without a mechanism.
- The Step 1 auto-clear decision recorded with rationale in
  DECISIONS.md: one decision per binding, the poll
  mechanism and its false-clear case, the placement and
  the death-frame Return reading, the per-frame cost bound,
  the re-press semantics, the twin limit stated as not
  closed, and the checklist relation — met, and this
  review re-read the code behind each rather than repeating
  the record on trust.
- No core, save-format, economy, movement, targeting, fire,
  marker, material, light, fog, model, or HUD rule changed
  in any step except an auto-clear-necessity change a step
  justifies explicitly (hiding the existing pending marker
  through the existing cancel path, as the scope allows);
  no demo scenario added; every earlier demo save length
  unchanged, with Slice 25 quoted at 44,041 chars — met.
- No checklist item marked passed from headless work: items
  42 and 43 observe the pending routing as it stands, and
  item 44 states its relation to them and its own Windows
  observation. The next free gate number after this slice
  is 45 — met.
- This review records the verdict honestly, separating
  headless-read routing claims from the Windows-checklist
  observations, including that Slice 26 is still not
  playtested and not Unity-verified. The Slice 1 director
  playtest gate is still pending throughout and is not
  closed by Slice 26 — met, as a statement, not as a gate
  passed.

## Constitution note, stated honestly

PROJECT-POINTER.md still records Constitution SHA
6dcb4bc03a356eb226156ee19c2b3cc1b32e12d7 with a note to
re-validate before claiming adoption complete — that full
adoption audit was NOT run in this step, so adoption
completeness is not claimed here.
