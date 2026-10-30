# Forgeborne — Slice 7 Scope

**Status: SCOPED 2026-10-30, under delegation. No Slice 7 system is
built by this document. Each step below is built one at a time,
with tests, in this order, and Slice 7 is closed by its own
review — it is not closed by finishing step 1 alone.**

## Scope question

Slice 6 closed the presentation gap Slice 5 left open: the
player can now pitch the camera, see squads and rallies as
scene markers, read an Advance squad by its prey instead of
a rally it ignores, and build a powered pair by key with a
power indicator. What Slice 6 did not touch is the combat
shape itself. There is still exactly one squad type. Every
squad in the game is a Vanguard: 3 members, 30 HP per member,
10 damage per living member per engagement tick, range 3,
one voxel per tick, under HoldGround or Advance. The picker
can deploy that one shape at two doctrines, and no other
tactical choice exists — a player who wants a faster screen,
a tougher anchor, or any other role has no second unit to
choose, and no price to weigh against Vanguard. Slice 7 is
therefore a combat-widening slice, kept as small as the
slices before it: **exactly one second squad type, priced
under the Slice 3 economy, deployed through the existing
paid picker, proved together with Vanguard, and closed by
its own review** — not a second doctrine, not a third unit,
not a rework of Vanguard.

## Inputs this scope is decided from

- Slice 6 review (`docs/slice-06-review.md`) ranked candidates,
  carried unchanged from the 0.27.0 scope: a second squad
  type first, then aim-based gather / build / fire plus
  yaw-rotated movement, then the rest of automation tier 2.
  The 0.31.0 decision recorded that the Step 4 evidence
  gives no reason to reorder them, and this scope keeps
  that order — the top candidate becomes Slice 7, the rest
  become the ranked Slice 8 candidates below, not promises.
  Slice 6 removes the reason the second type was deferred
  in the first place: pricing a second unit before the
  picker that deploys it could be aimed and read in the
  scene would have repeated the 0.15.0 mistake of widening
  combat while the existing system's presentation was
  unfinished. The picker can now be aimed by pitch, and a
  deployed squad can now be read by marker, activity word,
  and text line — so the second type now stands on its own.
- The Slice 6 review's gaps, in priority order, and what
  this scope does with each: Unity verification (gap 1)
  and the director playtest (gap 2) remain open, but
  neither is Slice 7 work in itself — the first is blocked
  on Unity 6000 on Windows, the second on the director —
  and scoping a slice does not close, reorder away, or
  pretend to answer either. What Slice 7 does, like Slice 6
  before it, is write only adapter code the checklists will
  one day run (a kind-selection binding, if the step adds
  one) and record, step by step, which parts are
  headless-tested and which parts are Windows-checklist
  only. Balance (gap 3) is a later pass, not a slice: the
  second type's numbers are first-pass values chosen for
  determinism and testability, exactly as Vanguard's were.
  The mesh tint and mesh refresh gaps (gap 4) remain named
  gaps from 0.30.0, not Slice 7 work: a second squad type
  does not need a tint or a refresh hook, and bundling
  either into this slice would make it two systems at once.
  The free-deploy gap (gap 5) remains documentation, not
  enforcement.
- Current behaviour this scope must reckon with, not
  ignore: `SquadKind` in `src/Forgeborne.Core/Orbital.cs`
  holds Vanguard = 0 only, and its own comment names
  recon, heavy, and flyer as later steps, not claimed
  here. Vanguard's shape is not a per-kind table but
  shared constants on `TacticalSquad` — `StartingMembers`
  3, `MemberHealth` 30, `MemberDamage` 10,
  `EngagementRange` 3 — so every rule that reads those
  constants today reads Vanguard's numbers, including
  `DamagePerTick` (living members times 10), the snapshot,
  and the load path. The save format already carries kind:
  each saved squad is an int array whose index 8 is
  `(int)Kind`, and the loader skips entries whose doctrine
  or kind is undefined, so an old Vanguard-only save loads
  and an unknown kind never resurrects a squad — but the
  load constructor clamps health to 0..90, which is
  Vanguard-shaped (3 times 30), not kind-shaped. The
  economy is a single price: `SquadEconomy.EmberOreCost`
  10 and `MaxConcurrentSquads` 3, with no per-kind price
  and no per-kind cap, and `TryDeployPaidSquad` checks
  cap and affordability before charging, through the same
  paid path the picker Confirm uses. The picker itself
  selects kind only by forcing it: adapter keys 1 and 2
  select HoldGround / Advance and both set kind to
  Vanguard, so there is no independent kind-selection
  binding in Unity, even though core `OrbitalCommand.
  SelectKind` already exists and already rejects unknown
  values. The text display already renders kind verbatim
  (`{Kind} {Doctrine} - ...`), and the picker prompt
  already shows kind — but it hardcodes `COST 10
  EMBERORE, MAX 3 SQUADS`, so a different price or cap
  for a second kind would make that prompt lie unless
  the step updates it. Scene markers do not distinguish
  kind at all: every squad marker is the same cyan cube,
  every rally the same orange slab, by the Slice 6 marker
  discipline of one marker per squad and no extra layer.
  Enemy targeting is kind-blind today: `ChooseTarget`
  picks the nearest live target, player or squad, with
  player winning ties and first-in-deploy-order winning
  squad ties, and squad movement and fire are shared
  rules (one voxel per tick, largest-gap axis, X/Y/Z
  tie-break, solid and enemy-occupied cells refused),
  differing only by doctrine target. Those facts are why
  step 1 is small in some directions and not in others:
  the picker, the save field, the snapshot, and the
  display already accept a kind — the stats, the price,
  the binding, and the load clamp do not yet.

## Slice 7, in order

### Step 1 — Exactly one second squad type

Add one second kind, decided in that step — not in this
scope document — with the reasoning recorded in
DECISIONS.md:

- The step must choose the kind, and record it: the
  scope's starting pool is the one the code already
  names — recon, heavy, flyer — and the step chooses
  exactly one from it, or another original role only if
  it records why none of the three fits the gap it is
  closing. A hypothesis is not a decision: the pool is
  a starting point, not a vote, and the step must say
  what tactical choice the new kind adds that Vanguard
  cannot already make — a faster screen is not a tougher
  anchor, and a tougher anchor is not a flying scout,
  so the choice must name the choice, not just the name.
  What the step must NOT do is add two kinds and call
  the second one a bonus, or add a kind that is Vanguard
  with one number changed and no distinct role to play.
- The step must decide the kind's shape, and record it:
  members, HP per member, damage per living member,
  engagement range, and movement — each as a stated
  number, with the test that proves it. The scope's
  starting hypothesis is that the new kind keeps the
  shared one-voxel-per-tick movement rule and the
  shared step refusals (solid and enemy-occupied cells),
  differing in its numbers and in what it is for, not
  in how it walks — because a new movement rule in the
  same step as a new kind would make the kind
  untestable in isolation. If the step chooses a
  flyer-like role, it must confront this hypothesis
  directly: a unit that ignores solid cells, or moves
  in the air, is a movement decision with targeting
  and occupancy consequences, and the step must prove
  those consequences with tests, not assert that flight
  is a stat. The same discipline applies to range and
  damage: a kind that out-ranges or out-damages Vanguard
  in every case is not a choice, it is a replacement,
  and the step must say what it gives up for what it
  gains.
- The step must decide how stats are represented, and
  record it: the scope's starting hypothesis is a
  per-kind shape the squad reads from its kind, rather
  than a second set of shared constants or a branch in
  every rule that reads Vanguard's numbers. Vanguard's
  numbers themselves must NOT change in this step:
  3 members, 30 HP each, 10 damage each, range 3 — a
  second kind that silently rebalances the first kind
  would break every Vanguard test and demo number that
  proves the game so far. If the step finds a per-kind
  table would force a save, snapshot, or display change
  it cannot justify, it must say so and choose otherwise,
  with the evidence.
- The step must decide the price, and record it: the
  new kind is priced under the Slice 3 economy, which
  means it deploys through the paid path, never through
  the free headless primitive in play — but whether its
  price is Vanguard's 10 EmberOre or a different
  per-kind price is decided in the step, not here. The
  scope's starting hypothesis is that a distinct role
  earns a distinct price only if the step can state the
  trade-off that price expresses (a tougher or longer-
  ranged kind costing more ore is a choice; the same
  price for a strictly better kind is a lie about the
  economy). Whatever the price, the picker prompt text
  must match it: the hardcoded `COST 10 EMBERORE`
  line is a display fact the step must update or prove
  still true, not leave to contradict the core rule.
- The step must decide the cap, and record it: the
  scope's starting hypothesis is that the cap stays 3
  live squads in total, not 3 per kind, because the cap
  was decided in 0.18.0 as a fielded-squad limit, not a
  per-unit allowance — 3 Vanguards plus 3 of a second
  kind would be a cap change disguised as a kind
  addition. If the step finds a per-kind cap is the
  honest shape, it must say so, change the cap openly
  with its own rationale and tests, and update the
  prompt's `MAX 3 SQUADS` text to match.
- The step must decide the picker binding, and record
  it: core `SelectKind` already exists, but adapter keys
  1 and 2 both force Vanguard, so Unity today cannot
  select a second kind at all. The scope's starting
  hypothesis is an independent kind selection — a key
  (or key pair) that changes kind without changing
  doctrine, so doctrine and kind become two choices
  instead of two doctrine presets that happen to share
  a kind — because coupling them again would repeat
  the 1/2 pattern with a third preset and leave the
  fourth combination (new kind under the other doctrine)
  unreachable. If the step keeps doctrine-coupled keys,
  it must prove every doctrine-kind combination it
  claims is reachable, and record which combinations it
  deliberately leaves out. The binding is adapter code,
  written not compiled headlessly, so it rides with a
  Windows checklist item, not a test-count claim.
- The step must decide the doctrine interaction, and
  record it: the scope's starting hypothesis is that
  the new kind works under both HoldGround and Advance,
  like Vanguard, with the same movement, fire, and
  activity-word rules (Marching / Holding / Hunting /
  Idle derived in the snapshot), because a kind that
  works under only one doctrine is half a unit and
  would need its own doctrine restriction stated as a
  decision, not discovered as a missing combination.
  If the step restricts a doctrine, it must say which
  one, why, and what the picker does when that
  combination is selected (refusal, not a silent
  Vanguard fallback).
- The step must decide the save and targeting rules,
  and record them: no new save field is expected, since
  kind is already index 8 of the saved squad array —
  but the step must deal with the Vanguard-shaped load
  clamp (0..90) openly. If the new kind's maximum health
  differs from 90, the clamp must become per-kind or
  the step must prove the same maximum; a clamp that
  silently trims the new kind to Vanguard's health on
  every load is a save bug, not a balance choice.
  Malformed and unknown kinds keep the existing rule:
  skipped, never fatal. Targeting is kind-blind under
  the starting hypothesis: `ChooseTarget` already picks
  the nearest live squad regardless of kind, and a new
  kind that draws fire differently (a scout that is
  ignored, a heavy that taunts) would be a targeting
  rule change, not a stat — the step must either keep
  targeting kind-blind and prove it with a test, or
  decide a priority rule openly with its own tests.
- The step must decide the display and marker question,
  and record it: the text display already renders kind
  verbatim, so no text change is expected unless the
  price or cap text changes (see above). The scene
  marker question is open: the scope's starting
  hypothesis is the same cyan squad cube for the new
  kind, with kind readable from the text line, because
  Slice 6's marker discipline was one marker per squad
  and no extra layer — but if the step finds two kinds
  indistinguishable in the scene is a real play
  confusion (not a hypothetical one), it may distinguish
  them with an original primitive shape or a colour
  within `docs/art-direction.md`, and must record the
  choice and its checklist item. A marker change is
  adapter code, not a test-count claim.
- What must NOT change in this step: Vanguard's stats,
  movement, fire, targeting, and save behaviour; the
  two doctrines and their rules; Dynamo cost,
  distribution, and cadence; kinetic mill behaviour;
  loot, skills, and core health; the gather / build /
  survive loop; and the save format, unless the clamp
  decision above creates a field it must then justify.
  No third kind, no new doctrine, no voltage ladder,
  no balance pass, and no licence / monetisation /
  release change rides along.

Why first, and why alone: a second unit is the combat
choice Slice 6's presentation now makes readable — the
picker can be aimed at a rally, the squad can be seen
as a marker, and its kind is already a text word. Adding
it before aim-based interaction is deliberate: aim-based
interaction changes how the player touches the world,
which would change the ground every squad stands on, and
a new unit proved first gives that later step a second
shape to prove its interaction against, not just Vanguard.

### Step 2 — Slice 7 demo scenario and review

As with 0.13.0, 0.19.0, 0.23.0, 0.26.0, and 0.31.0:
extend the demo with one composed Slice 7 run, then write
`docs/slice-07-review.md` as a documentation-only step.
The demo proves, headlessly, the new kind together with
Vanguard, not separately: both kinds deployed through
the paid picker, their distinct shapes visible in the
same run (members, health, damage, range, and price as
the step decided them), both doctrines exercised across
the two kinds or the restriction proved if the step
restricted one, a save/load round-trip that keeps both
kinds with their own health and kind intact, and an old
Vanguard-only save still loading unchanged. The demo
cannot prove adapter rendering — no headless run can
show a kind-selection key press or a distinct marker in
a scene — so the review's verdict must separate the two
claims in words: the core kind rules are tested, the
adapter binding and any marker distinction are written
and checklist-listed, and neither claim is allowed to
borrow the other's evidence. The review closes Slice 7
as tested second-kind core plus written adapters — not
as playtested, balanced, or Unity-verified — and names
the next candidates in priority order. Slice 8 is not
scoped by that review; it is scoped afterwards, the
same way this document follows the Slice 6 review's
candidates rather than preceding them.

## Deliberately NOT in Slice 7

- A third squad type, or a second kind added alongside
  the first as a bonus. One new kind is the slice; the
  next kind, if any, is a Slice 8-or-later decision with
  this slice's evidence in hand.
- A new doctrine (Defend Core, Patrol, or any other).
  Doctrines are a separate widening with their own
  targeting and movement questions, decided in Slice 4
  style, not smuggled in as a kind's special behaviour.
- Aim-based gather / build / fire, and yaw-rotated
  movement. Both are the first ranked Slice 8 candidate
  below, not Slice 7 work: switching the proven Slice 1
  interaction cells or the movement axes in the same
  step as a new kind would make the kind untestable in
  isolation, exactly as pitch discipline required in
  Slice 6 step 1.
- The rest of automation tier 2: a voltage ladder (LV to
  EV), transformers, mismatched-voltage destruction, and
  additional powered consumers or power sources. Those
  are deferred, not rejected, and are ranked behind the
  aim candidate by this scope — the Slice 7 evidence
  may reorder them, but this scope does not pre-order
  them first.
- An Orbital camera, or a mini-map beyond the Slice 1
  placeholder. Slice 7 markers remain scene markers in
  the FPS view, not a second camera.
- A balance pass. The new kind's numbers, its price,
  and the unchanged cap are first-pass values chosen
  for determinism and testability, exactly as Slice 3's
  prices and Slice 4's combat numbers were, and must be
  labelled as such, not quoted as final feel.
- Enforcement of the free-deploy rule beyond its
  existing documentation. The free `DeploySquad`
  primitive remains a headless-only rule, re-named in
  the Slice 3 review's gaps, not quietly fixed here.
- A mesh tint for Dynamo / powered mills, and a mesh
  refresh on any key build. Both remain named gaps from
  0.30.0, not fixed by a squad-type step.
- Windows Unity verification itself. It remains open
  for Slices 1–7 alike and is blocked on Unity 6000 on
  Windows, not on anything this slice can do headlessly.
  The checklists in `docs/slice-01-playtest-gate.md`,
  `docs/orbital-view.md`, `docs/orbital-command.md`, and
  `docs/powered-presence.md` remain the verification
  plan, and Slice 7 steps must add their own checklist
  items to those docs rather than claiming verification.

## Ranked candidates for Slice 8 scoping (not promised)

These are recorded here so the Slice 7 review starts
from a list, not from scratch. Order follows the
0.27.0 / 0.31.0 ranking with the Slice 7 candidate
removed:

1. Aim-based gather / build / fire, and yaw-rotated
   movement — the interaction follow-ups Slice 6 step 1
   carries forward, as one candidate because they are
   one question (does the player's aim, not a fixed +Z
   cell, decide what the player touches?), not two
   unrelated systems.
2. The rest of automation tier 2 (voltage ladder,
   transformers, mismatch destruction, additional
   consumers and sources), deferred, not rejected.
3. A third squad type, and the deferred doctrines
   (Defend Core, Patrol) — widening candidates that
   Slice 7's second-kind evidence will inform, not
   promises made before that evidence exists.

## Slice 7 gate (what "done" means)

Slice 7 is done when: every step above is pushed with
passing tests; exactly one second squad type exists,
with its shape, price, cap, picker binding, doctrine
interaction, save, and targeting rules each decided in
step 1 and recorded in DECISIONS.md; Vanguard's stats,
behaviour, and demo numbers are unchanged; both kinds
deploy through the paid picker (the free primitive
remaining headless-only by rule, not by enforcement);
old Vanguard-only saves still load, and a mixed-kind
save round-trips both kinds with their own health
intact; the composed demo proves both kinds together
through a save/load round-trip; and
`docs/slice-07-review.md` records the verdict honestly,
separating headless-tested core claims from
Windows-checklist adapter claims, including that Slice 7
is still not playtested, balanced, or Unity-verified.
The Slice 1 director playtest gate is still pending
throughout and is not closed by Slice 7.
