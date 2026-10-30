# Forgeborne — Slice 5 Review

**Status: REVIEWED HEADLESSLY 2026-10-30, tests 633/633.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, and 4. This review closes Slice 5 as a reviewed
whole; it does not claim Slice 5 is playtested, balanced, or
finished in every direction it could grow.**

## Review question

Does Slice 5 close the automation gap Slice 4 left open — a
building pillar that stalled at the crank, with nothing powered
and no building choice after the mill changing how a machine
works — with exactly one power source, one distribution rule,
and one powered behaviour, without inventing a second inventory
system, a voltage ladder, or a balance claim, and without
breaking kinetic behaviour, old saves, or anything earlier
slices proved? The answer headlessly is yes for the systems
below, with the gaps and next candidates listed honestly at
the end.

## What Slice 5 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.24.0 scope | `docs/slice-05-scope.md`: Slice 5 scoped as powered automation, in order — automation tier 2, first loop only, then demo scenario + review. Ranked Slice 6 candidates recorded, not promised | Decision document only, no system built by it | Tests 579/579 unchanged |
| 0.25.0 automation tier 2, first loop | Exactly one power source (`BlockType.Dynamo`, value 8, appended only, 5 EmberOre), exactly one distribution rule (face-adjacency, no cables, no chaining, no diagonals), exactly one powered behaviour (a face-adjacent Kinetic Mill runs every tick instead of every 5th, through the same reused `World.Gather` into the same player inventory). Single voltage, always on while placed; no fuel, no tiers, no mismatch destruction yet. No new save field: power is derived from placement, and the Dynamo survives as a chunk byte | Core owns the power rule (`WorldZone.IsPowered`, `RunPoweredAutomation`); `Simulation.BuildDynamoAhead` (front-centre) joins the mill builder; no Unity adapter touched, so no new Windows verification item rides along | Tests 610/610; demo unchanged in that step |
| 0.26.0 demo scenario + review | This review, plus a composed Slice 5 demo scenario: one Simulation in which a powered mill and an unpowered mill run side by side, through a save/load round-trip, with the loaded loop running again at the powered rate | Demo and composed-integration tests mirror each other; no new core system rides along with this step | Tests 633/633; demo Step 2 scenario below |

## What the review verified this run

- Full test suite re-run before writing: **633 passed, 0 failed**
  (was 610; +23 composed-integration tests for this step).
- Headless demo re-run: Slice 1 unchanged (gathered 1, built 1,
  killed 2, save 43,881 chars); Slice 2 unchanged (built 1,
  killed 2, kinetic gathered 1, squad intact 3 members / 90 HP,
  squads saved+loaded 1, save 43,908 chars); Step 1 scenario
  unchanged (core 42 HP, weapon 28/120, shields 100/105, save
  44,016 chars); Step 3 scenario unchanged (core 42 HP, 3 paid
  squads 90 HP, 10 ore left, 4th picker deploy REFUSED at cap,
  save 44,103 chars); Slice 4 scenario unchanged (HoldGround
  at rally, Advance hunting, save 44,026 chars). New Step 2
  scenario, in full: one Simulation, one tick to settle, then
  a Kinetic Mill built front-left (15,4,17) and a Dynamo built
  front-centre (16,4,17) through the real Simulation builders,
  with no move between builds — the pair is powered by
  face-adjacency, and the Dynamo spends its 5 EmberOre (0 left).
  The mill's five other neighbours are seeded as stone, and
  an unpowered mill pocket is placed at (10,20,10) with five
  stone neighbours as the kinetic baseline. After 5 ticks:
  powered gathered 5, kinetic gathered 1, stone 0 -> 6, and
  the Dynamo is still standing at (16,4,17) — the powered
  mill never excavates its own source. Save/load: the mill,
  the Dynamo, and the gathered stone all survive, the loaded
  mill is powered from placement with no flag, and both
  session counters reset to 0 (they are not saved, matching
  the tier 1 rule). Five fresh stone neighbours are then
  seeded around the loaded powered mill — its first pocket
  is spent, as it should be, and this is labelled as fresh
  seeding, not as leftover blocks — and 5 more ticks give
  loaded powered gathered 5 and loaded kinetic gathered 1,
  stone 12 in total. Save JSON 43,974 chars.
- Slice 5 gate, checked against `docs/slice-05-scope.md`:
  every step pushed with passing tests; a powered machine
  demonstrably does what an unpowered machine does not —
  5 blocks in 5 ticks against 1, in the same world, before
  and after a save/load round-trip — through a deterministic,
  real-time power rule in core, in tests and in the demo;
  the tier 1 inventory question is decided and recorded, not
  carried again (KEEP the player-inventory rule, decided in
  0.25.0, and re-proved here by both mills' output landing
  in the player inventory, stone 0 -> 6 -> 12, with no buffer
  anywhere); kinetic mill behaviour in zones without power is
  unchanged (the unpowered pocket's 1-in-5 rate is identical
  before and after load, and the Slice 2 demo's kinetic
  number is identical to before Slice 5); old saves still
  load (no save field was added, and every earlier demo save
  length above is unchanged); and this review records
  honestly that Slice 5 is still not playtested, balanced,
  or Unity-verified.
- No Slice 5 step duplicates logic in a Unity adapter. Core
  owns the power rule; no adapter was touched at all in this
  slice, so there is no new adapter behaviour awaiting
  Windows verification — but equally, no Dynamo or powered
  mill has ever been rendered or built by a key press in
  Unity, which is a presentation gap, not a core one.
- No Slice 5 step changed a saved value's meaning: the only
  new saved thing is the Dynamo chunk byte itself, appended
  as value 8, and a save without it loads exactly as before.
- Hard limits held: 3D real-time only, no turn-based mechanics
  (powered and kinetic mills act once per world tick, with no
  planning pauses), no twin-stick Asteroids loop, no copied
  assets, lore, code, or trademarks. The free deploy primitive
  is headless-only and is documented as such in code, tests,
  and `docs/orbital-command.md`; it is untouched by this slice.

## Deliberately NOT in Slice 5 (still open, not forgotten)

- The rest of automation tier 2: a voltage ladder (LV to EV),
  transformers, mismatched-voltage destruction, and additional
  powered consumers or power sources. Those are deferred, not
  rejected, and are not ranked above the pitch + markers
  candidate by this review — the Slice 5 scope left that
  ordering to this review, and the Step 2 evidence gives no
  reason to reorder: power is proved at one voltage, and the
  presentation gaps that most limit the picker in Unity are
  older and more exposed than a second voltage.
- Automation tiers 3-4 (digital / arcane), crafting,
  fuel / power beyond the single Dynamo, and a per-machine
  inventory system beyond the decided player-inventory rule.
- FPS pitch aiming and 3D squad / rally markers, including
  the Advance display gap (a hunting Advance squad reading
  as MOVING / HOLDING by rally position). That is the first
  ranked Slice 6 candidate, not Slice 5 scope creep — but it
  is presentation, and Slice 5 is a core loop step.
- A second squad type, priced under the Slice 3 economy.
  Second ranked Slice 6 candidate; pricing a second unit
  before powered automation existed would have repeated the
  0.15.0 mistake of widening combat while the building
  pillar stalled — Slice 5 removes that reason, so the
  candidate now stands on its own.
- Defend Core and Patrol doctrines, the full 1,500-node
  skill graph, an Orbital camera, or a balance pass. Step 1's
  Dynamo cost (5 EmberOre), distribution (face-adjacency),
  and cadence (every tick vs every 5th) are first-pass values
  chosen for determinism and testability, exactly as Slice 3's
  prices and Slice 4's combat numbers were, and must be
  labelled as such, not quoted as final balance.
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

## Gaps this review names, in priority order

1. **No Unity verification at all yet** — for Slices 1, 2,
   3, 4, or 5. Every adapter is explicitly uncompiled headlessly
   and needs Unity 6000 on Windows. The Slice 5 work most
   exposed to this is not a changed adapter — none was
   changed — but the absence of one: no Dynamo build key, no
   powered-mill rendering, and no power indicator exists in
   Unity at all, so the loop proved here has never been seen
   or built by hand in the game itself.
2. **No director playtest yet.** The Slice 1 playtest gate
   (`docs/slice-01-playtest-gate.md`) is still pending
   throughout Slice 5 and is not closed by it: no one has
   played gather -> build -> survive in Unity and answered
   whether it is fun, and Slice 5 adds no playtest evidence
   of its own beyond headless composition.
3. **No balance pass.** Dynamo cost, face-adjacency reach,
   the 5-to-1 powered-to-kinetic rate, squad price and cap,
   mill cost and cadence, loot drop rate, and skill numbers
   are all first-pass. Slice 5's power is proven to change
   gameplay deterministically, not proven to feel right —
   resolution and balance are different claims, and only the
   first is made here.
4. **The Advance display gap is still open.** The snapshot's
   MOVING / HOLDING word is rally-relative, so for an Advance
   squad it does not describe hunting: a hunting squad that
   stands still with no living enemy reads as HOLDING at a
   rally it is ignoring, and a squad moving toward an enemy
   reads as MOVING toward a rally it is not going to. This
   is a presentation gap for the Slice 6 presentation
   candidate to fix, not a core behaviour bug — the core
   behaviour is proved in the Slice 4 demo.
5. **The free deploy primitive remains a documented gap.**
   It is headless-only by rule, not by enforcement: nothing
   in code stops a future play path from calling it. If a
   future step finds a play path reaching it, that is a bug
   in that path, per the 0.18.0 decision — but the review
   notes it again here so it is not forgotten as slices grow.

## Slice 5 verdict (headless)

Slice 5 is **reviewed and closed as tested, integrated
powered automation — one Dynamo, one face-adjacency rule,
and one powered behaviour that demonstrably outruns its
kinetic baseline before and after a save/load round-trip**.
It is not playtested, not balanced, and not Unity-verified.
**Slice 6 is not scoped by this review** — its scope is the
decision for the next run, chosen from the ranked candidates
below, so it is chosen with Slice 5's composition evidence
in hand rather than guessed now.

## Ranked candidates for Slice 6 scoping (starting point, not promised)

Carried from the 0.24.0 scope, in the same order, because
the Step 2 evidence gives no reason to reorder them:

1. FPS pitch aiming + 3D squad/rally markers (the two
   presentation gaps that most limit the picker in Unity —
   now including the Advance display gap named above, and
   the new Slice 5 note that powered automation has no Unity
   presence at all yet).
2. A second squad type, priced under the Slice 3 economy.
