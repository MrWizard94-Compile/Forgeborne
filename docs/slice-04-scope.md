# Forgeborne — Slice 4 Scope

**Status: SCOPED 2026-10-29, under delegation. No Slice 4 system is
built by this document. Each step below is built one at a time,
with tests, in this order, and Slice 4 is closed by its own
review — it is not closed by finishing step 2 alone.**

## Scope question

Slice 3 priced and persisted squads, but combat still only
answers them halfway. A squad can shoot and can be hurt by
an enemy that happens to stand next to it, yet no enemy ever
goes after a squad: every enemy moves toward the player and
attacks the player, and the only doctrine is HoldGround, so
no squad ever moves into danger on purpose. Slice 4 closes
that gap before any new system widens the game further. Its
theme is therefore **squads under fire**: enemies that can
answer squads, and one doctrine that puts squads where
enemies can answer them.

## Inputs this scope is decided from

- Slice 3 review (`docs/slice-03-review.md`) ranked candidates,
  carried unchanged from the 0.16.0 scope: enemy AI targeting
  squads + a second doctrine first, then automation tier 2,
  then FPS pitch + 3D markers, then a second squad type. The
  0.19.0 decision recorded that the Step 3 evidence gives no
  reason to reorder them, and this scope keeps that order —
  the top pair becomes Slice 4, the rest become the ranked
  Slice 5 candidates below, not promises.
- The pairing rationale from the 0.16.0 scope, kept intact:
  a doctrine that moves squads into danger is only fair once
  enemies can answer them. That is why targeting comes
  first in this slice and the doctrine second, not the other
  way round: a moving doctrine built before enemies target
  squads would be a free advance, and targeting built after
  a moving doctrine would be a retrofit.
- Current core behaviour this scope must reckon with, not
  ignore: `Enemy.TickToward` moves one voxel per tick toward
  the player only, on the largest-gap axis, and `WorldZone.
  Advance` then damages the player for each adjacent enemy;
  squads move toward a fixed rally in `RunSquads`, fire only
  while holding, and afterwards take 10 damage per enemy
  still adjacent to them. Enemies therefore already hurt
  squads they stand next to, but they never seek a squad
  out. Step 1 must reconcile with that existing damage, not
  stack a second hit on top of it by accident.

## Slice 4, in order

### Step 1 — Enemy AI targeting squads

Give enemies a target choice, decided in that step — not in
this scope document — with the reasoning recorded in
DECISIONS.md:

- The step must choose the targeting rule and justify it:
  which target an enemy pursues when player and squad are
  both in play (nearest, player-first, squad-first, or
  another rule), how ties break, and whether a moving squad,
  a holding squad, and the player are treated alike. The
  scope's starting hypothesis is nearest target with a
  deterministic tie-break, because it needs no new value and
  matches the squad's own nearest-enemy rule — but a
  hypothesis is not a decision: if the step finds nearest
  targeting makes enemies ignore the player whenever a
  squad is anywhere closer, it must say so and choose
  otherwise, with the evidence.
- Movement must not be duplicated: the step must decide
  whether `TickToward` generalises to a target position or
  a squad-target method reuses its axis rule, so there is
  still one movement rule, not a player rule and a squad
  rule that can drift apart. Enemies still move one voxel
  per tick, in real time, with no planning pauses and no
  turn-like behaviour.
- The step must reconcile the two existing damage paths, and
  record the reconciliation: an enemy that moves toward a
  squad and attacks it must not also be counted a second
  time by the squad phase's "10 damage per adjacent enemy"
  rule for the same hit, and the tick order (enemies, then
  squads, as today) must be kept or deliberately changed
  with a reason. A squad that kills its attacker that tick
  must not be hit by it, as today.
- What must NOT change in this step: the player remains a
  valid target (enemies must not abandon the player for
  squads wholesale); dead squads are never targeted; no new
  doctrine, squad type, or enemy type rides along; and
  targeting is derived each tick, so the step must confirm
  whether the save format needs anything new at all — the
  expectation is nothing, since targets are not persisted,
  only positions and health are.

Why first: the doctrine in Step 2 is only a fair choice
once enemies can answer it. Targeting first also bounds
Step 2: the doctrine is judged against enemies that
actually pursue squads, not against the player-chasing
enemies of Slice 2.

### Step 2 — A second doctrine

Add exactly one second doctrine, chosen in that step — not
in this scope document — with the reasoning recorded in
DECISIONS.md:

- The step must choose one doctrine from Advance, Defend
  Core, and Patrol, and justify why that one first. The
  scope's starting hypothesis is Advance — a squad that
  seeks the nearest enemy (or the core objective's
  attackers) instead of holding a fixed rally — because it
  is the moving-into-danger doctrine the pairing rationale
  was about, and therefore the doctrine Step 1 exists to
  answer. A hypothesis is not a decision: if the step finds
  Advance collapses into HoldGround at a moving rally, or
  that Defend Core is the doctrine the core objective
  actually needs first, it must say so and choose otherwise,
  with the evidence. The two doctrines not chosen are
  deferred, not rejected.
- The doctrine must be a real doctrine, not a flag: it
  needs its own movement / engagement behaviour in core,
  its own tests, and a picker selection for it, so Unity
  play can actually choose it once the adapter work for
  that is done. HoldGround / Vanguard behaviour for zones
  and saves that do not use the new doctrine must be
  unchanged.
- Pricing is settled, not reopened: the new doctrine is
  deployed through the Slice 3 paid path — 10 EmberOre, cap
  3 live squads — with no new price invented in this step.
  If a doctrine genuinely needs a different price, that is
  a separate economy decision, not smuggled into a doctrine
  step. No new squad type rides along: the priced unit
  stays Vanguard, and a second type is a Slice 5 candidate,
  not a Step 2 extra.

### Step 3 — Slice 4 demo scenario and review

As with 0.13.0, 0.19.0, and the slices before them: extend
the demo with one composed Slice 4 run (an enemy pursuing
and engaging a squad, and a squad under the new doctrine,
in the same Simulation — not two separate proofs), then
write `docs/slice-04-review.md` as a documentation-only
step. The review closes Slice 4 as tested, integrated
combat answers — not as playtested, balanced, or
Unity-verified — and names the next candidates in priority
order. Slice 5 is not scoped by that review; it is scoped
afterwards, the same way this document follows the Slice 3
review's candidates rather than preceding them.

## Deliberately NOT in Slice 4

- Automation tier 2 (electrical) and the tier 1 inventory
  question it was deferred with. That is the first ranked
  Slice 5 candidate, not Slice 4 scope creep.
- FPS pitch aiming and 3D squad / rally markers. Those are
  the presentation gaps that most limit the picker in Unity,
  and they are the second ranked Slice 5 candidate — but
  they are presentation, and Slice 4 is combat behaviour.
- A second squad type, priced under the Slice 3 economy.
  Third ranked Slice 5 candidate; pricing a second unit
  before the first unit can be answered by enemies would
  repeat the 0.15.0 mistake of widening before the existing
  unit is fairly contested.
- The full 1,500-node skill graph, automation tiers 3-4,
  crafting, fuel/power, per-mill inventory, an Orbital
  camera, or a balance pass. Step 1's targeting rule and
  Step 2's doctrine are first-pass values chosen for
  determinism and testability, exactly as Slice 3's prices
  were, and must be labelled as such, not quoted as final
  balance.
- Enforcement of the free-deploy rule beyond its existing
  documentation. The free `DeploySquad` primitive remains a
  headless-only rule, re-named in the Slice 3 review's gaps,
  not quietly fixed here.
- FPS pitch, licence, monetisation, release status, or any
  change to repo visibility. Those need their own decision
  steps (and, for licence / monetisation / release, Rob).
- Windows Unity verification. It remains open for Slices
  1-4 alike and is blocked on Unity 6000 on Windows, not on
  anything this slice can do headlessly. The checklists in
  `docs/slice-01-playtest-gate.md`, `docs/orbital-view.md`,
  and `docs/orbital-command.md` remain the verification plan.

## Ranked candidates for Slice 5 scoping (not promised)

These are recorded here so the Slice 4 review starts from a
list, not from scratch. Order follows the 0.16.0 / 0.19.0
ranking with the Slice 4 pair removed:

1. Automation tier 2 (electrical), with whatever inventory
   question tier 1 deferred, decided then.
2. FPS pitch aiming + 3D squad / rally markers (the two
   presentation gaps that most limit the picker in Unity).
3. A second squad type, priced under the Slice 3 economy.

## Slice 4 gate (what "done" means)

Slice 4 is done when: every step above is pushed with passing
tests; enemies demonstrably pursue and engage squads in
tests and in the demo, without abandoning the player as a
target and without double-counting the existing adjacent
damage; the second doctrine is selectable, behaves
distinctly from HoldGround, and deploys through the paid
path; HoldGround / Vanguard behaviour in zones and saves
that do not use the new doctrine is unchanged; and
`docs/slice-04-review.md` records the verdict honestly,
including that Slice 4 is still not playtested, balanced,
or Unity-verified. The Slice 1 director playtest gate is
still pending throughout and is not closed by Slice 4.
