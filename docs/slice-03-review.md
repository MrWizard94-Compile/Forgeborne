# Forgeborne — Slice 3 Review

**Status: REVIEWED HEADLESSLY 2026-10-29, tests 466/466.
Director playtest and Windows Unity verification still pending,
as with Slices 1 and 2. This review closes Slice 3 as a reviewed
whole; it does not claim Slice 3 is playtested, balanced, or
finished in every direction it could grow.**

## Review question

Does Slice 3 pay the two debts Slice 2 left behind — things
the game promises and then loses on load, and the one action
it let the player do without limit — without breaking old
saves, duplicating logic between core and Unity adapters, or
claiming anything unverified? The answer headlessly is yes
for the systems below, with the gaps and next candidates
listed honestly at the end.

## What Slice 3 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.16.0 scope | `docs/slice-03-scope.md`: Slice 3 scoped as economy and persistence, in order — save-format completion, squad economy, demo scenario + review. Ranked Slice 4 candidates recorded, not promised | Decision document only, no system built by it | Tests 342/342 unchanged |
| 0.17.0 save-format completion | Core health, applied skill build, and equipped weapon (including in-progress mag) join the existing save format. New fields nullable and last; nulls omitted when writing; old saves load (core 100, no build, Pulse Rifle kept); malformed skill strings and partial weapons skipped, never fatal; build decoded before player restore so shields above the base cap survive | `SaveSystem.LoadSave` returns world + build + weapon; `Simulation.SaveToJson` / `LoadFromJson` are the full-save path; `GameManager.SaveToJson` uses it | Tests 381/381; demo Step 1 scenario: core 42 HP, Titan Long Pulse Rifle 28/120, effective damage 24, shields 100/105, before and after load alike, save 44,016 chars |
| 0.18.0 squad economy | One Vanguard HoldGround squad costs 10 EmberOre; at most 3 live squads at once. `SquadEconomy` holds the constants and queries; `WorldZone.TryDeployPaidSquad` is the single play path, checking bounds, validity, cap, and affordability BEFORE charging, with a refund guard on post-charge failure; picker Confirm deploys through it, failing closed (picker stays open, no squad, no charge). Free `DeploySquad` path kept deliberately for headless use only | Picker Confirm and `Simulation.DeployOrbitalSquadPaid` both use the one paid path; Unity display shows cost/cap but enforces nothing, core does | Tests 437/437; Slice 2 demo squad now deploys through the paid path |
| 0.19.0 demo scenario + review | This review, plus a composed Slice 3 demo scenario: one skilled, loot-equipped Simulation damages its core, deploys 3 paid squads, is refused a 4th through the picker at the cap, then saves and loads — and the loaded game keeps the core, weapon, build, squads, leftover ore, and the cap | Demo and composed-integration tests mirror each other; no new core system rides along with this step | Tests 466/466; demo Step 3 scenario below |

## What the review verified this run

- Full test suite re-run before writing: **466 passed, 0 failed**
  (was 437; +29 composed-integration tests for this step).
- Headless demo re-run: Slice 1 unchanged (gathered 1, built 1,
  killed 2, save 43,881 chars); Slice 2 unchanged (built 1,
  killed 2, kinetic gathered 1, squad intact 3 members / 90 HP,
  squads saved+loaded 1, save 43,908 chars); Step 1 scenario
  unchanged (core 42 HP, weapon 28/120, shields 100/105, save
  44,016 chars). New Step 3 scenario: core 42 HP, Titan Long
  Pulse Rifle 28/120, effective damage 24, shields 100/105,
  3 paid squads at 90 HP, 10 ore left, 4th picker deploy
  REFUSED at cap with picker still open and ore untouched,
  and after load the same core, weapon, damage, shields,
  3 squads (90 HP, 3 members each in snapshot), and 10 ore —
  with `CanDeployOrbitalSquad` false in the loaded game,
  because the cap itself survives the round-trip, not just
  the squads. Save JSON 44,103 chars.
- Slice 3 gate, checked against `docs/slice-03-scope.md`:
  every step pushed with passing tests; old saves still load
  (tested in 0.17.0, untouched since); a damaged core, a skill
  build, and a weapon survive a save/load round-trip in tests
  and in the demo; squad deployment refuses unaffordable and
  over-cap deploys without charging or half-deploying (tested
  in 0.18.0, re-proved composed in 0.19.0); and this review
  records honestly that Slice 3 is still not playtested,
  balanced, or Unity-verified.
- No Slice 3 step duplicates logic in a Unity adapter. Core
  owns the save format and the economy; the picker is core
  state (`OrbitalCommand`), the display only shows its prompt
  and the cost/cap text.
- No Slice 3 step changed a saved value's meaning: new save
  fields were appended as nullable, the mill block type was
  already appended in Slice 2, and squad entries ride the
  existing format. A present core value of 0 means destroyed,
  not missing — that distinction is why the field is nullable.
- Hard limits held: 3D real-time only, no turn-based mechanics
  (squads act once per world tick, with no planning pauses),
  no twin-stick Asteroids loop, no copied assets, lore, code,
  or trademarks. The free deploy primitive is headless-only
  and is documented as such in code, tests, and
  `docs/orbital-command.md`; Unity play goes through the
  picker, which is paid.

## Deliberately NOT in Slice 3 (still open, not forgotten)

- The full 1,500-node skill graph (the 6-node Forge Survivor
  cluster is the persisted unit; growing the graph is a later
  slice, not a persistence fix).
- Automation tiers 2-4, crafting, fuel/power, per-mill
  inventory (tier 1 stays excavation-only into the player
  inventory).
- Additional doctrines (advance, defend core, patrol), squad
  types (recon, heavy, flyer), enemy AI targeting squads,
  3D squad markers, or an Orbital camera. Those are the
  ranked candidates for Slice 4 scoping, not Slice 3 scope
  creep.
- A balance pass. Step 2's prices (10 EmberOre, cap 3) are
  first-pass values chosen for determinism and testability,
  exactly as Slice 2's numbers were, and must be labelled as
  such, not quoted as final balance.
- FPS pitch aiming, licence, monetisation, release status, or
  any change to repo visibility. Those need their own
  decision steps (and, for licence / monetisation / release,
  Rob).
- Windows Unity verification. It remains open for Slices 1-3
  alike and is blocked on Unity 6000 on Windows, not on
  anything this slice can do headlessly. The checklists in
  `docs/slice-01-playtest-gate.md`, `docs/orbital-view.md`,
  and `docs/orbital-command.md` remain the verification plan.

## Gaps this review names, in priority order

1. **No Unity verification at all yet** — for Slices 1, 2,
   or 3. Every adapter is explicitly uncompiled headlessly
   and needs Unity 6000 on Windows. The Slice 3 work most
   exposed to this is the picker: its cost/cap enforcement
   is headless-tested in core, but the O / crosshair / Enter /
   Esc wiring and the cost/cap prompt text have never run
   in Unity.
2. **No director playtest yet.** The Slice 1 playtest gate
   (`docs/slice-01-playtest-gate.md`) is still pending
   throughout Slice 3 and is not closed by it: no one has
   played gather -> build -> survive in Unity and answered
   whether it is fun, and Slice 3 adds no playtest evidence
   of its own beyond headless composition.
3. **No balance pass.** Squad price, cap, mill cost and
   cadence, loot drop rate, and skill numbers are all
   first-pass. Slice 3's economy is proven to enforce, not
   proven to feel right — enforcement and balance are
   different claims, and only the first is made here.
4. **The free deploy primitive remains a documented gap.**
   It is headless-only by rule, not by enforcement: nothing
   in code stops a future play path from calling it. If a
   future step finds a play path reaching it, that is a bug
   in that path, per the 0.18.0 decision — but the review
   notes it again here so it is not forgotten as slices grow.

## Slice 3 verdict (headless)

Slice 3 is **reviewed and closed as tested, integrated
persistence + economy**. It is not playtested, not balanced,
and not Unity-verified. **Slice 4 is not scoped by this
review** — its scope is the decision for the next run, chosen
from the ranked candidates below, so it is chosen with
Slice 3's composition evidence in hand rather than guessed
now.

## Ranked candidates for Slice 4 scoping (starting point, not promised)

Carried from the 0.16.0 scope, in the same order, because
the Step 3 evidence gives no reason to reorder them:

1. Enemy AI targeting squads + a second doctrine (the two
   belong together: a doctrine that moves squads into danger
   is only fair once enemies can answer them).
2. Automation tier 2 (electrical), with whatever inventory
   question tier 1 deferred, decided then.
3. FPS pitch aiming + 3D squad/rally markers (the two
   presentation gaps that most limit the picker in Unity).
4. A second squad type, priced under the Slice 3 economy.
