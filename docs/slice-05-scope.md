# Forgeborne — Slice 5 Scope

**Status: SCOPED 2026-10-29, under delegation. No Slice 5 system is
built by this document. Each step below is built one at a time,
with tests, in this order, and Slice 5 is closed by its own
review — it is not closed by finishing step 1 alone.**

## Scope question

Slice 4 closed the combat gap Slice 3 left open: enemies now
pursue squads, and an Advance squad can move into danger on
purpose. Automation has no such closure. It still stops at
tier 1: a Kinetic Mill excavates one adjacent gatherable
block every 5th tick into the player inventory, and nothing
in the game is powered, nothing runs on a supply, and no
building choice after the mill changes how a machine works.
The gather -> build -> survive loop therefore has a building
pillar that stalls at the crank. Slice 5 starts the next
pillar step before any new presentation or combat widening:
its theme is **powered automation** — the first small
electrical loop, not the whole electrical tier.

## Inputs this scope is decided from

- Slice 4 review (`docs/slice-04-review.md`) ranked candidates,
  carried unchanged from the 0.20.0 scope: automation tier 2
  first, then FPS pitch + 3D markers (now including the
  Advance display gap), then a second squad type. The 0.23.0
  decision recorded that the Step 3 evidence gives no reason
  to reorder them, and this scope keeps that order — the
  top candidate becomes Slice 5, the rest become the ranked
  Slice 6 candidates below, not promises.
- The Slice 4 review's gaps, in priority order, and what this
  scope does with each: Unity verification (gap 1) and the
  director playtest (gap 2) remain open, but neither is
  Slice 5 work — the first is blocked on Unity 6000 on
  Windows, the second on the director — and scoping a
  headless slice does not close, reorder away, or pretend
  to answer either. Balance (gap 3) is a later pass, not a
  slice. The Advance display gap (gap 4) belongs to the
  pitch + markers candidate, not to an automation step.
  The free-deploy gap (gap 5) remains documentation, not
  enforcement. Slice 5 therefore starts at the first
  actionable core gap, which is automation tier 2 — not
  because the higher gaps are unimportant, but because
  they are not gaps a headless core step can close.
- Current core behaviour this scope must reckon with, not
  ignore: `BlockType.KineticMill` (value 7, appended only,
  old saves load) builds for 10 Stone + 5 Wood under the
  same air / support rules as walls and workstations; every
  5th tick each mill excavates at most one adjacent
  gatherable block via reused `World.Gather`, so bedrock,
  walls, workstations, and mills are never gathered, a full
  inventory consumes nothing, and shared-block races resolve
  in deterministic chunk-index scan order. Mill output goes
  to the existing player inventory; there is no second
  inventory system, and the mill's session counter is not
  saved. That last sentence is the tier 1 inventory
  question this slice must finally decide, not carry again:
  does powered automation keep the player-inventory rule,
  or does a machine gain a buffer of its own?
- Blueprint vocabulary this scope must not mistake for a
  decision: `docs/architecture.md` names tier 2 as
  "Electrical (LV to EV): alternators, insulated cables,
  transformers; mismatched voltage without transformers
  destroys machines." Those are blueprint names and a
  blueprint failure rule, not a Slice 5 design. Step 1 must
  choose which, if any, of those names maps to the first
  unit, and whether the failure rule belongs in the first
  loop at all — with the reasoning recorded in DECISIONS.md.

## Slice 5, in order

### Step 1 — Automation tier 2, first small electrical loop only

Build the first electrical loop, decided in that step — not
in this scope document — with the reasoning recorded in
DECISIONS.md:

- The step must choose exactly one power source, exactly
  one distribution rule, and exactly one powered behaviour,
  and justify each: what generates power, how power reaches
  a machine (cable blocks, adjacency to a source, a network
  rule, or another deterministic rule), and what a powered
  machine does that an unpowered one does not. The scope's
  starting hypothesis is that the first powered behaviour
  is a powered variant of an existing machine behaviour —
  a mill or workstation that runs only when powered, or
  runs differently when powered — rather than a new machine
  family, because a variant reuses a proven baseline and
  proves power changes something that already exists, while
  a brand-new consumer would need its own baseline proved
  at the same time. A hypothesis is not a decision: if the
  step finds no existing behaviour is a clean powered
  variant, it must say so and choose otherwise, with the
  evidence.
- The step must decide the voltage question, and record it:
  the scope's starting hypothesis is a single voltage for
  the first loop, with no mismatch destruction yet, because
  a first loop must prove power on / off changes gameplay
  before failure modes punish a player for wiring it wrong.
  Transformers, a voltage ladder, and destruction on
  mismatch are deferred to a later tier 2 step, not smuggled
  into the first loop — unless the step finds the first
  loop is meaningless without them, in which case it must
  prove that, not assert it.
- The step must decide the activation rule, and record it:
  the scope's starting hypothesis is to reuse the tier 1
  activation shape (an action on a fixed tick cadence, as
  the mill's every-5th-tick rule) rather than inventing a
  second timing system, and to reuse the existing excavation
  / build path where the powered behaviour touches blocks,
  so there is still one block-gathering rule, not a kinetic
  rule and an electrical rule that can drift apart. Power
  acts once per world tick in real time, with no planning
  pauses and no turn-like behaviour.
- The step must decide the tier 1 inventory question, and
  record it: powered-machine output goes to the player
  inventory under the tier 1 rule, or the machine gains a
  buffer of its own. The scope's starting hypothesis is the
  tier 1 rule — no second inventory system — because tier 1
  proved the player-inventory rule works and a buffer is a
  new system with its own size, pickup, and overflow rules.
  A hypothesis is not a decision: if the step finds a
  buffer is needed (for example, because a powered machine
  must keep working while its output has nowhere to go, or
  because its output is not a block the player inventory
  already holds), it must say so and decide the buffer's
  size, pickup rule, and overflow rule — blocked, never
  silently lost — with the evidence. Whichever rule is
  chosen, it applies to the powered behaviour in this step;
  it does not retroactively change kinetic mill output
  unless the step justifies that change as well.
- The step must decide the build cost, and record it: cost
  is paid in existing gathered resources only, with no new
  currency invented in this step. The scope's starting
  hypothesis is a cost of the same order as the mill's
  10 Stone + 5 Wood, but the step must consider the Slice 3
  pricing rationale before settling it: squads are priced in
  EmberOre precisely so they do not compete with walls and
  mills for Stone and Wood, and an electrical build priced
  in Stone and Wood would compete with exactly those. If
  the step prices in EmberOre, it must say what that
  competition with squads and workstations costs the
  player; if it prices in Stone / Wood, it must say why
  that competition is acceptable here.
- The step must decide the save format, and record it: any
  new block type is appended only, as KineticMill was, so
  old saves still load, and malformed new entries are
  skipped, never fatal, matching the squad rule. The scope's
  starting hypothesis is that powered state is derived at
  load from the blocks and their connections — as targeting
  is derived each tick, not persisted — so no new powered
  flag needs saving, unless the step's buffer decision
  creates contents that must survive a load, in which case
  those contents follow the same old-save / malformed-entry
  rules and the step must say so.
- What must NOT change in this step: kinetic mill behaviour
  in zones without power is unchanged; HoldGround / Advance
  behaviour, squad price (10 EmberOre) and cap (3), loot,
  skills, and core health are unchanged; no second powered
  consumer, no second power source, no transformer or
  voltage tier, no digital or arcane tier, no crafting, and
  no balance claim ride along. Core owns the power rule;
  Unity adapters, if touched at all, only render or forward
  a key press, with no duplicate logic.

Why first: a powered consumer built before a power loop
exists would be a faster mill with a power label, and a
power loop built without a consumer would be wiring with
nothing to prove it works. Step 1 must therefore contain
both ends of the loop — source, distribution, and one
powered behaviour — in one small, tested step, or it has
not built a loop at all.

### Step 2 — Slice 5 demo scenario and review

As with 0.13.0, 0.19.0, and 0.23.0: extend the demo with one
composed Slice 5 run (an electrical loop running inside the
gather -> build -> survive loop — a powered machine doing
what an unpowered machine does not, through a save/load
round-trip, with the kinetic mill still behaving as before
alongside it), then write `docs/slice-05-review.md` as a
documentation-only step. The review closes Slice 5 as
tested, integrated powered automation — not as playtested,
balanced, or Unity-verified — and names the next candidates
in priority order. Slice 6 is not scoped by that review; it
is scoped afterwards, the same way this document follows
the Slice 4 review's candidates rather than preceding them.

## Deliberately NOT in Slice 5

- The rest of automation tier 2: a voltage ladder (LV to
  EV), transformers, mismatched-voltage destruction, and
  additional powered consumers or power sources. Those are
  deferred, not rejected, and are not ranked above the
  pitch + markers candidate by this scope — the Slice 5
  review may reorder on the evidence of Step 1, but this
  scope does not pre-order them first.
- Automation tiers 3-4 (digital / arcane), crafting,
  fuel / power beyond the single source Step 1 chooses, and
  a per-machine inventory system beyond whatever Step 1
  decides for its one powered behaviour. Tier 1 stays
  excavation-only into the player inventory unless Step 1
  justifies changing that rule for its own behaviour.
- FPS pitch aiming and 3D squad / rally markers, including
  the Advance display gap (a hunting Advance squad reading
  as MOVING / HOLDING by rally position). That is the
  first ranked Slice 6 candidate, not Slice 5 scope creep —
  but it is presentation, and Slice 5 is a core loop step.
- A second squad type, priced under the Slice 3 economy.
  Second ranked Slice 6 candidate; pricing a second unit
  before powered automation exists would repeat the
  0.15.0 mistake of widening combat while the building
  pillar stalls.
- Defend Core and Patrol doctrines, the full 1,500-node
  skill graph, an Orbital camera, or a balance pass. Step
  1's source, distribution, powered behaviour, cost, and
  cadence are first-pass values chosen for determinism and
  testability, exactly as Slice 3's prices and Slice 4's
  combat numbers were, and must be labelled as such, not
  quoted as final balance.
- Enforcement of the free-deploy rule beyond its existing
  documentation. The free `DeploySquad` primitive remains a
  headless-only rule, re-named in the Slice 3 review's gaps,
  not quietly fixed here.
- FPS pitch, licence, monetisation, release status, or any
  change to repo visibility. Those need their own decision
  steps (and, for licence / monetisation / release, Rob).
- Windows Unity verification. It remains open for Slices
  1-5 alike and is blocked on Unity 6000 on Windows, not on
  anything this slice can do headlessly. The checklists in
  `docs/slice-01-playtest-gate.md`, `docs/orbital-view.md`,
  and `docs/orbital-command.md` remain the verification plan.

## Ranked candidates for Slice 6 scoping (not promised)

These are recorded here so the Slice 5 review starts from a
list, not from scratch. Order follows the 0.20.0 / 0.23.0
ranking with the Slice 5 candidate removed:

1. FPS pitch aiming + 3D squad/rally markers (the two
   presentation gaps that most limit the picker in Unity —
   now including the Advance display gap named in the
   Slice 4 review).
2. A second squad type, priced under the Slice 3 economy.

## Slice 5 gate (what "done" means)

Slice 5 is done when: every step above is pushed with passing
tests; a powered machine demonstrably does what an
unpowered machine does not, through a deterministic,
real-time power rule in core, in tests and in the demo;
the tier 1 inventory question is decided and recorded, not
carried again; kinetic mill behaviour in zones without power
is unchanged; old saves still load; a powered loop survives
a save/load round-trip in tests and in the demo to the extent
Step 1's save decision requires; and `docs/slice-05-review.md`
records the verdict honestly, including that Slice 5 is still
not playtested, balanced, or Unity-verified. The Slice 1
director playtest gate is still pending throughout and is
not closed by Slice 5.
