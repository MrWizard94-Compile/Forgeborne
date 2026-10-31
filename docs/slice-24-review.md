# Forgeborne — Slice 24 Review

**Status: REVIEWED HEADLESSLY 2026-10-31, tests 4577/4577.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
16, 17, 18, 19, 20, 21, 22, and 23. This review closes Slice 24
as a reviewed whole; it does not claim Slice 24 is playtested,
Unity-verified, or a visual result, and it does not mark any
playtest-gate item passed.**

## Review question

Does Slice 24 implement the two decided squad-action bindings —
K for re-rally, X for dismiss — in adapter code only, read
against the existing routing and gate signatures, without a
core rule change, an economy change, a marker, material,
light, fog, or view model change, a HUD word, a new binding,
a general squad-command system, a demo scenario staged as a
key press in disguise, or a claim of Unity verification made
from a headless run — and does it leave the Windows-checklist
observations that alone can close items 39, 40, 41, 42, and 43
honestly open? The answer headlessly is yes, with the split
this review keeps throughout: the bindings are implemented
and read, not observed; the core actions they call were
already proved by Slices 20–22; and no headless number in
this slice proves a key works in a scene.

## What Slice 24 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.96.0 scope | `docs/slice-24-scope.md`: Slice 24 scoped as the adapter bindings slice, in order — K re-rally first, X dismiss second, then its own documentation-only review. Slice 25 is not scoped there. Ranked Slice 25 candidates recorded, not promised | Decision document only, no system built by it | Tests 4577/4577 unchanged |
| 0.97.0 Step 1 | K re-rally binding implemented in the adapter: general gate alone, own pending flag, marker-centre proximity targeting, SnapshotPosToGateIndex translation, adapter-owned orange pending marker; new gate item 42 | `ForgeborneUnity/Assets/Scripts/FpsInputAdapter.cs` only in code; `docs/orbital-command.md` and `docs/slice-01-playtest-gate.md` beside it | Tests 4577/4577 unchanged, as expected for adapter code the headless runner never compiles; demo unchanged, Slice 23 save 44,041 chars |
| 0.98.0 Step 2 | X dismiss binding implemented in the adapter: own pending flag, refuse-while-pending both ways against the re-rally pick, Step 1 translation reused, no doctrine filter; new gate item 43 | `ForgeborneUnity/Assets/Scripts/FpsInputAdapter.cs` only in code; `docs/orbital-command.md` and `docs/slice-01-playtest-gate.md` beside it | Tests 4577/4577 unchanged, as expected; demo unchanged, Slice 23 save 44,041 chars |
| 0.99.0 Step 3 | This review, documentation only | Review document plus CHANGELOG and DECISIONS entries; no core file, no adapter file, no material, marker, light, fog, or view model value, no checklist item added, no demo scenario added | Tests 4577/4577 unchanged; every earlier demo save unchanged, Slice 23 save 44,041 chars, no new scenario length to quote |

## Scope check, step by step

### Step 1 — K re-rally binding (0.97.0)

The scope asked Step 1 to decide, not merely to implement,
and the step's record answers each question it was given:

- Gate choice: the general gate alone
  (`Simulation.TryReRallySquad`, live HoldGround or Patrol),
  because its eligibility already includes a stuck Patrol and
  both gates call the same free `ReRally` — so one K binding
  serves item 39's stuck readings and item 40's general
  readings without two keys, and the stuck gate's narrower
  requirement adds no reach. The step also changed the
  opening rule in a way the scope allowed it to decide: a
  pick never opens on an Advance or Defend Core marker,
  because doctrine never changes, so a pick the gate would
  certainly refuse is not opened at all; death, a
  same-rally pick, and a list shift remain the confirm-time
  refusals, and the confirm re-validates for exactly those.
- Pending-state shape: an OWN adapter flag
  (`_pendingReRally` plus target and candidate fields),
  never the picker flag. The picker-flag hazard was
  confronted directly: while picking, Return deploys a paid
  squad, so a re-rally pick under that flag would turn a
  Return into a paid deploy at the new rally instead of a
  free re-rally. Routing while pending: O does nothing,
  builders do nothing, picker keys do nothing (their branch
  never runs while pending), the mouse does not fire, and
  E / Q / R keep their pre-existing unsuppressed behaviour,
  as while picking, because the re-rally is free and no
  build can break a confirm's affordability.
- Rally selection and reading: the candidate reuses the
  picker's centre raycast (30f, floored cell) and starts at
  the target's own rally, so an immediate Return is a
  same-rally refusal that clears the pick, never a silent
  move. The candidate reads through an adapter-owned orange
  sphere (0.6 units, the picker's shape, size, and colour),
  because `SquadMarkerDisplay` draws its pending sphere only
  while picking and this pick deliberately does not set the
  picker flag — reusing the reading without reusing the
  flag is the honest split. The target squad's rally marker
  stays on its deployed rally until the gate succeeds, then
  moves under the existing snapshot rendering. No new HUD
  word, no new shape or colour.
- Marker-to-index translation: `SnapshotPosToGateIndex`,
  implemented once in the adapter, walking
  `Simulation.World.Squads` in order and skipping dead
  squads exactly as `OrbitalView.Snapshot` does. A dead
  squad awaiting its sweep therefore cannot shift a snapshot
  position into the wrong gate index, and a stale marker for
  a dead squad cannot exist in this rendering, because the
  marker display renders the snapshot, which skips dead
  squads. Confirm re-validates the stored snapshot position
  by doctrine, kind, and old rally, and clears the pick
  with no gate call on any mismatch, so a death-plus-sweep
  between press and confirm cannot silently re-target the
  squad that shifted into the stored position. Movement
  never invalidates the check, because a walking squad
  keeps its doctrine, kind, and rally.
- Fail-closed cases, each walked to a nothing-changed exit:
  K while picking does nothing (the picker branch reads no
  K); K on no marker opens nothing; K on a rally marker
  selects nothing, because only squad centres are tested;
  K on an Advance or Defend Core marker never opens a
  pick; Return or Escape with no pick open does nothing;
  K again cancels (toggle-off, symmetric with O cancelling
  the picker); a confirm whose target died or shifted
  clears with nothing changed. Pending state does NOT
  auto-clear when the target dies mid-pick — the adapter
  does not poll deaths — and the stale pick is caught at
  confirm instead; the lingering candidate marker marks a
  rally, not the squad, so it is not a false squad marker.
- Checklist relation: item 42 (re-rally pending-state
  routing) was added because the K-again cancel, O-ignored,
  mouse-suppressed, same-rally-clears, and
  Return/Escape-with-no-pick readings had no home in items
  39 or 40. Item 42 EXTENDS items 39 and 40, supersedes
  nothing, and leaves items 39, 40, 37, 29, and
  orbital-command item 13 standing. Items 39 and 40 became
  attemptable, not passed. The cross-pending answer for X
  was deliberately NOT decided here, because X was
  unimplemented; it was left open for Step 2, which is the
  scope's order, not a gap in Step 1's reasoning.
- Scope deviation: none. No core touch was needed — the
  adapter reads only existing public core surface — and no
  core file, economy value, marker, material, light, fog,
  model, or HUD rule moved.

### Step 2 — X dismiss binding (0.98.0)

- Pending-state shape: an OWN adapter flag
  (`_pendingDismiss`, with the target's snapshot position,
  doctrine, kind, and rally), NOT the picker flag and NOT
  shared with the re-rally flag. A shared flag was
  considered and refused: it would need a discriminator
  field, which is a flag plus a tag rather than a simpler
  shape, while two flags with a routing order that reads
  exactly one pending branch (dismiss, then re-rally, then
  picker, then the free branch) make "Return confirms
  exactly one action" provable by reading the Update tail.
  The two flags are mutually exclusive by routing, not by
  a runtime assertion: each opener runs only from the else
  branch where neither flag is set and the picker is
  closed, so no frame can set both.
- Cross-pending, decided both ways as refuse-while-pending:
  X while a re-rally pick is pending does nothing, and K
  while a dismiss is pending does nothing. Cancel-first was
  rejected for the re-rally side (a mistaken X press would
  silently discard an aimed rally pick), and replace was
  rejected both ways for the scope's named hazard (a player
  confirming a dismiss they thought was a re-rally, or the
  reverse), with no in-step indicator distinguishing the
  two pending states. O does nothing while either pending
  state is open, so no deploy picker stacks on top of
  either. A same-frame K+X press opens the re-rally pick
  only, because the else branch checks K first; the step
  records that frame order as routing determinism, not a
  designed key priority, and the player re-presses X after
  closing the re-rally pick to dismiss instead.
- Routing while a dismiss is pending, walked branch by
  branch: builders suppressed, O refused, the mouse does
  not fire, picker keys do nothing, E / Q / R keep their
  pre-existing unsuppressed behaviour (a dismiss charges
  nothing and refunds nothing, so no build can break a
  confirm's affordability). Return confirms, Escape
  cancels, X again cancels (toggle-off). Return or Escape
  with no pending dismiss does nothing, because neither key
  is read in the free branch. X while picking does nothing,
  because the picker branch reads no X key.
- Target selection and translation: Step 1's marker-centre
  proximity rule reused exactly (angle within max(2
  degrees, atan(0.6 / distance)), distance at most 30f,
  occlusion raycast, smallest angle wins, ties to the
  first snapshot position), with NO doctrine filter,
  because `TryDismissSquad` consults no doctrine — an
  Advance or Defend Core marker that K refuses is a valid
  dismiss target, not an error. Step 1's
  `SnapshotPosToGateIndex` is reused, not re-decided, so
  the two bindings agree about which squad "the squad
  under the crosshair" is. The only list-shift source while
  a dismiss is pending is death, because a dismiss cannot
  be the shift source (X is refused while pending and no
  other adapter path dismisses), and the confirm
  re-validation catches it on the same movement-invariant
  triple Step 1 uses — position and health deliberately
  excluded, because a walking squad changes cell every
  tick and an enemy hit mid-decision must not void a
  confirm. Return clears the pending state whether the
  gate succeeds or refuses: a confirm is one attempt.
- What the player reads on confirm: the existing marker
  rule, not a new reading — the squad's text line and both
  its markers vanish with its snapshot entry, as item 41
  already observes. No pending marker and no HUD word are
  added, because a dismiss has no candidate cell to mark.
- Checklist relation: item 43 (dismiss pending-state
  routing) was added because the X-again cancel,
  O-ignored, mouse-suppressed, builder-suppressed,
  cross-pending refuse-both-ways, and
  Return/Escape-with-no-pending readings had no home in
  item 41. Item 43 EXTENDS item 41, supersedes nothing,
  and leaves items 41, 40, 39, 37, 29, item 42, and
  orbital-command item 13 standing; it does not widen
  item 42's re-rally-only routing, and the cross-pending
  readings Step 1 left open now live in item 43, not
  retrofitted into item 42. Item 41 became attemptable,
  not passed. Next free gate number after this slice is 44.
- Scope deviation: none. No core touch, no HUD word, no
  marker change, no material, light, fog, or view model
  change, no new binding, rebinding, or modifier scheme.

### Step 3 — this review (0.99.0)

Documentation only, following the Slice 16 precedent for
claims no headless run can prove — unlike the reviews of
Slices 17–23, which were each accompanied by a composed
demo scenario because their steps were headless-testable.
No demo scenario is added by this step: a demo that staged
K or X would be a core-call demo in disguise, and the core
calls are already proved by Slices 20–22. Every earlier
demo save length stands; there is no new scenario length
to quote. This review does NOT scope Slice 25 — Slice 25
is scoped afterwards, the same way the Slice 24 scope
followed the Slice 23 review rather than preceding it.

## Verification re-checked in this review run

This review does not take Steps 1 and 2 on their word. It
re-read the adapter file and re-ran the standing suite and
demo, and found the step records consistent with the code
as it stands:

- Routing order in `Update`: the O guard reads both
  pending flags; then the dismiss branch, then the
  re-rally branch, then the picker branch, then the free
  branch where K, X, and mouse fire are read in that
  order. A same-frame K+X press therefore opens the
  re-rally pick only, as Step 2 records, and a same-frame
  click with a K or X press does not fire, because the
  free branch never runs on that frame's pending state.
- Both openers test squad centres only, with the
  proximity and occlusion rules the steps record; the
  re-rally opener skips non-HoldGround, non-Patrol
  doctrines, and the dismiss opener applies no doctrine
  filter, exactly the eligibility difference their gates
  require.
- Both confirms re-validate the stored snapshot position
  by doctrine, kind, and rally (old rally for re-rally),
  translate through the single `SnapshotPosToGateIndex`
  helper, and clear their pending state whether the gate
  succeeds or refuses; both cancels clear their flag, and
  the re-rally cancel also hides its marker.
- Brace balance was checked by the steps (45 open, 45
  close, recorded at 0.98.0); this review's reading found
  no unclosed branch in the pending routing, and the
  adapter file's size stands at 46,358 bytes.
- Suite: 4577 passed, 0 failed, re-run in this review run
  before any edit — the same count Steps 1 and 2 report,
  which is the honest outcome for adapter-only steps, not
  a failure to prove. A step that had raised the suite
  would have had to name the headless claim it added.
- Demo: Slice 23 save 44,041 chars, as in Steps 1 and 2;
  every earlier save unchanged (Slice 22 43,990, Slice 21
  44,040, Slice 20 44,041, Slice 19 43,965, Slice 18 44,015,
  Slice 15 44,016, Slice 17 43,974 chars, and all earlier
  saves as in the 0.95.0 entry). No new scenario exists,
  so no new length is quoted here.

What this verification does NOT do, stated plainly: it
does not compile the adapter, does not run a scene, does
not press K or X, and does not observe a marker, a
candidate sphere, or a vanishing squad. Those are Windows
observations, and they remain owed.

## Claim split: implemented versus observed

| Claim | Status after Slice 24 |
|---|---|
| K binding code exists in `FpsInputAdapter.cs`, general gate alone, own pending flag, proximity targeting, shared translation, fail-closed routing as recorded | Implemented, read against the routing and gate signatures; NOT Unity-verified |
| X binding code exists in the same file, own pending flag, refuse-while-pending both ways, translation reused, no doctrine filter | Implemented, read against the routing and gate signatures; NOT Unity-verified |
| Core re-rally and dismiss actions the bindings call | Proved headlessly by Slices 20–22 (free re-rally, general re-rally, dismiss with refund none and one cap slot freed), unchanged by this slice |
| Items 39 (stuck re-rally), 40 (general re-rally), 41 (dismiss), 42 (re-rally pending routing), 43 (dismiss pending routing) | Attemptable, all unchecked; each closes only on its Windows observation, recorded in DECISIONS.md with the Unity version and GPU |
| Slice 1 director playtest gate (gather, then build, then survive fun) | Still pending throughout; not closed by Slice 24 |
| Any claim that K or X works in a scene | NOT made, by any step in this slice |

## Gaps carried, honestly stated

- Windows Unity verification for Slices 1–24 alike, and
  the director playtest, remain open — blocked on Unity
  6000 on Windows and on the director, not on anything
  this slice can do headlessly. Implementing K and X does
  not verify them, and this review does not pretend
  otherwise.
- The identical-twin limit, carried from both binding
  steps: a twin (same doctrine, kind, and rally) shifting
  into the stored snapshot position after the target's
  death and sweep passes re-validation, because no
  adapter-visible field distinguishes the twins without
  inventing a core identity the save does not carry. The
  consequence is bounded — a free re-rally of the wrong
  twin, or dismissing the wrong twin with one slot freed
  and no refund either way — and closing it is a core
  and save-format decision for a later step to weigh, not
  a bindings patch.
- A pending dismiss has no visual indicator: unlike a
  re-rally pick, which marks a candidate rally, a dismiss
  has no candidate cell to mark, and inventing a highlight
  or HUD word would have been a marker or HUD change
  smuggled inside a bindings step. The player reads the
  target's own markers, which do not change until
  confirm, so a pending dismiss can be mistaken for "X
  did nothing" until Return is pressed. If Windows
  playtest finds that confusing, a later display step —
  not a bindings patch — should weigh a pending-dismiss
  reading on that evidence.
- The re-rally pending marker is adapter-owned and
  unparented (this adapter has no MarkerRoot field), one
  reused instance hidden whenever no pick is pending.
  Claiming a new scene wiring requirement for it would
  have been a second, unscoped change; whether its
  unparented placement reads correctly in a scene is a
  Windows observation, not a headless claim.
- Same-frame K+X opens the re-rally pick only, by branch
  order rather than by a designed key priority. The steps
  record this as routing determinism and give the player
  the re-press path; whether that reads as a priority in
  play is a Windows observation.
- Pending state does not auto-clear when the target dies
  mid-decision in either binding, by design: the adapter
  does not poll deaths, and the stale pending state is
  caught at confirm instead. A lingering re-rally
  candidate marker after a target death is therefore
  possible until confirm or cancel, and is a Windows
  observation, not a headless failure.
- First-pass graphics values (dusk azimuth, fog contrast,
  view model offset, static weapon-agnostic model) are
  retuned only on Windows evidence; no step in this slice
  moved one, and no headless re-decision is recorded here.
- Economy questions gated on playtest evidence remain
  open: a general re-rally price, if free repositioning
  proves abusive kiting (Slice 21 gap), and a dismiss
  refund, if slot-only removal proves punishing (Slice 22
  gap). Neither is decided here, and neither price was
  moved to pay for implementing its key.

## Deliberately NOT in Slice 24

- A core rule change of any kind, a save-format change,
  or a bindings-necessity core touch: none was needed,
  and none is claimed.
- An economy change disguised as a binding: the re-rally
  stays free and the dismiss stays refund none, in both
  steps.
- A general squad-command system, a follow order, a
  formation command, or an order queue.
- A new binding beyond K and X, a rebinding of an
  existing key, or a modifier scheme.
- A HUD redesign, a new HUD word, a marker shape or
  colour change, a marker-occlusion change, a material,
  light, fog, or view model change.
- A scene file, an imported asset, a texture, or a shader
  file written by a headless run and claimed as a built
  scene.
- A copied asset, model, skyline, HUD layout, or
  trademarked look from any other game.
- Windows Unity verification itself, the director
  playtest, or a Slice 25 scope.

## Ranked candidates for Slice 25 scoping (not promised)

Carried from the Slice 24 scope, updated for what this
slice settled — items 39, 40, and 41 are now attemptable,
not passed, and items 42 and 43 are added, not closed:

1. Any Slice 13 remainder that Slice 24 does not itself
   settle as an observation (the vertex-colour material
   observation, the MeshRenderer wiring observation, and
   the marker-occlusion observation, plus the dusk and
   view model observations in items 33 and 34, plus the
   tint, count, and patrol observations in items 35, 36,
   and 37, plus items 39, 40, 41, 42, and 43) — if those
   observations are still unmade when Slice 25 is scoped,
   they remain observations to be made when Windows
   verification runs, not steps to be re-scoped blindly,
   and not promised here. Items 39, 40, and 41 leave this
   candidate's "unmade" set only when their Windows
   observations are actually made, not when their
   bindings were implemented.
2. First-pass graphics values retuned only on Windows
   evidence (dusk azimuth, fog contrast, view model
   offset, static weapon-agnostic model) — a retuning step
   is justified by an observation, not by a headless
   re-decision, and is not promised here.
3. A general re-rally price on Windows playtest evidence,
   if free repositioning proves abusive kiting — a later
   economy decision, recorded as a gap in Slice 21 Step 1,
   not set here.
4. A dismiss refund on Windows playtest evidence, if
   slot-only removal proves punishing — a later economy
   decision, recorded as a gap in Slice 22 Step 1, not set
   here.
5. A loop-shape decision, if ever wanted, as its own
   decision with its own tests — carried from the Slice 23
   review's gaps, not promised here, and not to be
   confused with the radius and count values Slice 23 kept.

## Verdict

Slice 24 closes as a reviewed whole: the scope's three
steps were each pushed in order, each with its decisions
recorded with rationale, each checked against the scope
above, and the slice gate is met on its own terms — suite
standing at 4577/4577 through all three steps, no core,
economy, marker, material, light, fog, model, or HUD rule
changed, no demo scenario added, every earlier demo save
unchanged, no checklist item marked passed from headless
work, and no Slice 25 scope smuggled inside the review.
What Slice 24 does not do is equally part of the verdict:
it does not playtest, it does not verify Unity, it does
not prove a key works in a scene, and it does not close
the Slice 1 director playtest gate. The next step is
scoping Slice 25 as its own decision step, chosen from
the ranked candidates above — not promised here, and not
started here.
