# Forgeborne — Slice 4 Review

**Status: REVIEWED HEADLESSLY 2026-10-29, tests 579/579.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, and 3. This review closes Slice 4 as a reviewed
whole; it does not claim Slice 4 is playtested, balanced, or
finished in every direction it could grow.**

## Review question

Does Slice 4 close the combat gap Slice 3 left open — squads
that could shoot but were never pursued, and only one doctrine
that never moved a squad into danger on purpose — without
double-counting the existing adjacent damage, abandoning the
player as a target, duplicating a movement rule, reopening the
settled squad price, or claiming anything unverified? The
answer headlessly is yes for the systems below, with the gaps
and next candidates listed honestly at the end.

## What Slice 4 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.20.0 scope | `docs/slice-04-scope.md`: Slice 4 scoped as squads under fire, in order — enemy AI targeting squads, exactly one second doctrine, demo scenario + review. Ranked Slice 5 candidates recorded, not promised | Decision document only, no system built by it | Tests 466/466 unchanged |
| 0.21.0 enemy AI targeting squads | Targeting rule, decided in that step: nearest live target by Manhattan distance, re-derived every tick — player or any live squad, moving and holding alike; ties to the player, squad ties to deploy order; dead squads never targeted. Movement generalised, not duplicated: `Enemy.TickTowardPosition` is the single rule, `TickToward` delegates to it. Damage reconciled by keeping exactly one squad-damage path: the squad phase's 10 per surviving adjacent enemy, after squad fire; the enemy phase adds no squad damage | `WorldZone.ChooseTarget` (private, tested through `Advance`); tick order enemies, then squads, unchanged; no new save field — targets recompute from saved positions and health | Tests 495/495; demo unchanged in that step |
| 0.22.0 second doctrine, Advance | Exactly one second doctrine, chosen in that step from Advance / Defend Core / Patrol: Advance. An Advance Vanguard squad hunts the nearest living enemy, one voxel per tick, re-derived every tick, and fires at the nearest enemy in range 3 whether or not it is at its rally. With no living enemy it stands still — it never falls back to its rally, which is what keeps it from collapsing into HoldGround at a moving rally. Movement is not duplicated: Advance and HoldGround share one step rule in `TacticalSquad`; only the target differs | Picker selection via `OrbitalCommand.SelectDoctrine` and Unity adapter key 2 (1 stays HoldGround); deployed through the Slice 3 paid path — 10 EmberOre, cap 3, no new price; doctrine and rally already persisted, so a loaded Advance squad keeps hunting | Tests 528/528; demo unchanged in that step |
| 0.23.0 demo scenario + review | This review, plus a composed Slice 4 demo scenario: one Simulation in which an enemy pursues a HoldGround squad AND an Advance squad hunts in the same world, both deployed through the paid picker, then a save/load round-trip after which both doctrines keep behaving from the saved positions and health | Demo and composed-integration tests mirror each other; no new core system rides along with this step | Tests 579/579; demo Step 3 scenario below |

## What the review verified this run

- Full test suite re-run before writing: **579 passed, 0 failed**
  (was 528; +51 composed-integration tests for this step).
- Headless demo re-run: Slice 1 unchanged (gathered 1, built 1,
  killed 2, save 43,881 chars); Slice 2 unchanged (built 1,
  killed 2, kinetic gathered 1, squad intact 3 members / 90 HP,
  squads saved+loaded 1, save 43,908 chars); Step 1 scenario
  unchanged (core 42 HP, weapon 28/120, shields 100/105, save
  44,016 chars); Step 3 scenario unchanged (core 42 HP, 3 paid
  squads 90 HP, 10 ore left, 4th picker deploy REFUSED at cap,
  save 44,103 chars). New Step 3 scenario, in full: two paid
  squads (HoldGround rally 20, Advance rally 24, both spawned
  at 16, both weakened to 1 member / 30 HP so their 10-damage
  fire does not kill outright and hide the engagement), player
  moved far away, one enemy east at 22 and one west at 10.
  Tick 1: both enemies step toward the squads (21, 11), both
  squads march east (17), nobody hurt. Tick 2: Advance fires
  the east enemy 30 -> 20 while HoldGround, still marching,
  stays silent. Tick 3: the east enemy reaches adjacency and
  both squads take exactly 10 each (30 -> 20) — single adjacent
  damage, not double — while Advance fires it 20 -> 10.
  Tick 4: HoldGround takes a second 10 (20 -> 10) while
  Advance kills its attacker and takes nothing from it — the
  no-double-count killing case, inside the composed run — and
  the west enemy has pursued to 14. Save/load after tick 4:
  positions (18), health (10 / 20), doctrines, rallies, and
  the spent ore (0) all survive, and the loaded game cannot
  deploy more without ore. Tick 5 (loaded): HoldGround marches
  to 19 while the loaded Advance squad moves WEST to 17 and
  fires the west enemy 30 -> 20 — hunting from the saved
  position, not marching east to its stored rally. Tick 6
  (loaded): HoldGround reaches its rally at 20; the west
  enemy, now nearer the Advance squad than HoldGround,
  retargets to it (15 -> 16); Advance fires it 20 -> 10 and
  takes exactly one adjacent 10 (20 -> 10); HoldGround at its
  rally is unharmed and, the enemy being 4 away, does not
  fire. Save JSON 44,026 chars.
- Slice 4 gate, checked against `docs/slice-04-scope.md`:
  every step pushed with passing tests; enemies demonstrably
  pursue and engage squads in tests and in the demo, without
  abandoning the player as a target (the player remains the
  tie winner and the target whenever nearest, proved in
  0.21.0 and re-proved in composition here by the player
  being far away — had targeting abandoned the player rule,
  nothing in this scenario would change, which is why the
  Step 1 tie and retarget tests, not this demo alone, carry
  that claim) and without double-counting the existing
  adjacent damage (proved in 0.21.0 in isolation and again
  in composition at ticks 3, 4, and 6 above); the second
  doctrine is selectable, behaves distinctly from HoldGround,
  and deploys through the paid path; HoldGround / Vanguard
  behaviour in zones and saves that do not use Advance is
  unchanged (Slice 2 and Slice 3 demo numbers above are
  identical to before Slice 4); and this review records
  honestly that Slice 4 is still not playtested, balanced,
  or Unity-verified.
- No Slice 4 step duplicates logic in a Unity adapter. Core
  owns targeting, movement, and doctrine behaviour; the
  adapter only binds key 2 to the core doctrine selection
  and renders the snapshot.
- No Slice 4 step changed a saved value's meaning: no new
  save field was added at all — doctrine and rally were
  already persisted, targets are recomputed each tick, and
  the Slice 4 demo's round-trip proves the loaded game
  behaves from positions and health alone.
- Hard limits held: 3D real-time only, no turn-based mechanics
  (enemies and squads act once per world tick, with no
  planning pauses), no twin-stick Asteroids loop, no copied
  assets, lore, code, or trademarks. The free deploy primitive
  is headless-only and is documented as such in code, tests,
  and `docs/orbital-command.md`; the Slice 4 demo deploys
  through the picker, which is paid.

## Deliberately NOT in Slice 4 (still open, not forgotten)

- Defend Core and Patrol doctrines. They are deferred, not
  rejected: Advance was chosen first because it is the
  moving-into-danger doctrine the targeting step exists to
  answer. Defend Core would need a core-defence behaviour
  distinct from HoldGround with a preset rally, and Patrol
  would need waypoint state in the save format — neither was
  smuggled into this slice.
- A second squad type, priced under the Slice 3 economy.
  The priced unit stays Vanguard; a second type is a Slice 5
  candidate, not a Slice 4 extra.
- Automation tier 2 (electrical) and the tier 1 inventory
  question it was deferred with. That is the first ranked
  Slice 5 candidate, not Slice 4 scope creep.
- FPS pitch aiming and 3D squad / rally markers. Those are
  the presentation gaps that most limit the picker in Unity,
  and they are the second ranked Slice 5 candidate — but
  they are presentation, and Slice 4 is combat behaviour.
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

## Gaps this review names, in priority order

1. **No Unity verification at all yet** — for Slices 1, 2,
   3, or 4. Every adapter is explicitly uncompiled headlessly
   and needs Unity 6000 on Windows. The Slice 4 work most
   exposed to this is the doctrine selection: its key 2
   binding and the snapshot doctrine line have never run
   in Unity, and the Advance display gap below is a Unity
   presentation question, not a core one.
2. **No director playtest yet.** The Slice 1 playtest gate
   (`docs/slice-01-playtest-gate.md`) is still pending
   throughout Slice 4 and is not closed by it: no one has
   played gather -> build -> survive in Unity and answered
   whether it is fun, and Slice 4 adds no playtest evidence
   of its own beyond headless composition.
3. **No balance pass.** Targeting distances, the 10-damage
   adjacent hit, squad price and cap, mill cost and cadence,
   loot drop rate, and skill numbers are all first-pass.
   Slice 4's combat is proven to resolve deterministically,
   not proven to feel right — resolution and balance are
   different claims, and only the first is made here.
4. **The Advance display gap is still open.** The snapshot's
   MOVING / HOLDING word is rally-relative, so for an Advance
   squad it does not describe hunting: a hunting squad that
   stands still with no living enemy reads as HOLDING at a
   rally it is ignoring, and a squad moving toward an enemy
   reads as MOVING toward a rally it is not going to. This
   is a presentation gap for the Slice 5 presentation
   candidate to fix, not a core behaviour bug — the core
   behaviour is proved in the demo above.
5. **The free deploy primitive remains a documented gap.**
   It is headless-only by rule, not by enforcement: nothing
   in code stops a future play path from calling it. If a
   future step finds a play path reaching it, that is a bug
   in that path, per the 0.18.0 decision — but the review
   notes it again here so it is not forgotten as slices grow.

## Slice 4 verdict (headless)

Slice 4 is **reviewed and closed as tested, integrated
combat answers — enemies that pursue squads, and a second
doctrine that puts squads where enemies can answer them**.
It is not playtested, not balanced, and not Unity-verified.
**Slice 5 is not scoped by this review** — its scope is the
decision for the next run, chosen from the ranked candidates
below, so it is chosen with Slice 4's composition evidence
in hand rather than guessed now.

## Ranked candidates for Slice 5 scoping (starting point, not promised)

Carried from the 0.20.0 scope, in the same order, because
the Step 3 evidence gives no reason to reorder them:

1. Automation tier 2 (electrical), with whatever inventory
   question tier 1 deferred, decided then.
2. FPS pitch aiming + 3D squad/rally markers (the two
   presentation gaps that most limit the picker in Unity —
   now including the Advance display gap named above).
3. A second squad type, priced under the Slice 3 economy.
