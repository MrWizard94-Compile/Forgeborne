# Forgeborne — Slice 2 Review

**Status: REVIEWED HEADLESSLY 2026-10-29, tests 254/254.
Director playtest and Windows Unity verification still pending,
as with Slice 1. This review closes Slice 2 as a reviewed whole;
it does not claim Slice 2 is playtested, balanced, or finished
in every direction it could grow.**

## Review question

Does Slice 2 add depth to the Slice 1 loop (gather -> build ->
survive) without breaking that loop, duplicating logic between
core and Unity adapters, or claiming anything unverified? The
answer headlessly is yes for the systems below, with the gaps
and next candidates listed honestly at the end.

## What Slice 2 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.4.0 loot generation | `LootGenerator`: weapons from Body / Barrel / Stock / Grip / Sight / Elemental Capacitor, Common to Legendary, affixes by rarity, level-scaled damage, deterministic by seed; enemy drops 30%, deterministic by seed + enemy id | Data only at this step | Tests at 0.4.0 |
| 0.5.0 first skill cluster | `SkillGraph.ForgeSurvivorCluster`: 6 nodes only, NOT the full 1,500-node graph. Sturdy Hands +5% damage, Iron Skin +10 shields, Swift Feet +5% move speed; Forge Heart +15% damage / +10 shields; Scavenger +20% gather; Corebreaker +25% core damage / +5 workstation regen. Prereq-gated allocation, Base64 encode/decode | Data only at this step | Tests 111/111 |
| 0.6.0 weapon integration | `Weapon.FromGenerated` + parameterised `Weapon`; `Simulation(seed, equipped?)` and `Simulation.Equip` (null rejected); HUD weapon name follows the equipped weapon. Pulse Rifle remains the default | Simulation is the single place a generated weapon enters gameplay | Tests 128/128 |
| 0.7.0 skill application | `Simulation.ApplySkills` (replaces, never stacks; null rejected). Effective damage / core damage rounded away from zero; `Player.MaxShields`; `FpsController` speed multiplier; `WorldZone` workstation regen; fractional-carry gather bonus (one bonus unit per 5 successful gathers) | Simulation is the single application point, as with weapons | Tests 154/154 |
| 0.8.0 automation tier 1 | `BlockType.KineticMill` (value 7, appended only, old saves load); build cost 10 Stone + 5 Wood; every 5th tick each mill excavates at most one adjacent gatherable block into the player inventory via reused `World.Gather` | Mill output goes to the existing player inventory; no second inventory system | Tests 180/180 |
| 0.9.0 Orbital prototype | `Orbital.cs`: one doctrine (HoldGround), one squad type (Vanguard: 3 members, 30 HP each, 10 damage per living member, range 3 Manhattan). Move 1 voxel/tick toward rally, largest-gap axis; no fire while moving; hold-and-fire at nearest in range; 10 damage per surviving adjacent enemy; dead squads removed | `WorldZone.DeploySquad` + `Simulation.DeployOrbitalSquad` | Tests 224/224 |
| 0.10.0 squad persistence | Squads saved/loaded in the existing int-array save format (position, rally, health, doctrine, kind); wounded squads keep member count and resume marching; malformed/dead entries skipped, never fatal; old saves load with no squads | Existing `SaveSystem`, no second save system | Tests 235/235 |
| 0.11.0 Orbital view | `OrbitalView.Snapshot(world)`: copied, read-only snapshot per live squad (position, rally, health, members, damage, at-rally / moving, doctrine, kind), in deploy order; thin text adapter `OrbitalSquadDisplay.cs`; provisional Unity O key deploys at the player's feet | Rendering cannot mutate core; no squad logic in the adapter | Tests 254/254 |

## What the review verified this run

- Full test suite re-run before writing: **254 passed, 0 failed**.
- Headless demo re-run: unchanged from every Slice 2 step —
  gathered 1, built 1, killed 2, player dead standing still,
  save JSON 43,864 chars, mesher 198 quads / 792 vertices.
  That stability is good news for Slice 1 (nothing regressed)
  and a gap for Slice 2 (see below): the demo does not exercise
  any Slice 2 system.
- No Slice 2 system duplicates logic in a Unity adapter. Core
  owns loot, skills, mills, and squads; adapters render or
  forward a key press only.
- No Slice 2 step changed a saved value's meaning: the mill
  block type was appended, squad entries ride the existing save
  format, and old saves still load.
- Hard limits held: 3D real-time only, no turn-based mechanics
  (squads act once per world tick, with no planning pauses),
  no twin-stick Asteroids loop, no copied assets, lore, code,
  or trademarks. The provisional O key is a verification aid,
  not a game feature, and is documented as reversible.

## Deliberately NOT in Slice 2 (still open, not forgotten)

- Deployment cost or squad cap (O and the headless command
  still deploy unlimited squads).
- Additional doctrines (advance, defend core, patrol) or squad
  types (recon, heavy, flyer); enemy AI targeting squads
  (enemies still chase the player; squads take damage only
  from enemies already adjacent to them).
- 3D squad markers or an Orbital camera; a doctrine menu or
  rally picker to replace the provisional O key.
- Automation tiers 2-4, crafting, fuel/power, per-mill
  inventory; the full 1,500-node skill graph (only the
  6-node Forge Survivor cluster exists).
- Skill builds are not yet saved: a loaded world starts at
  base values until skills are applied again (noted in 0.7.0).
  That is a known gap, not a silent loss — nothing claims a
  build survives a load.

## Gaps this review names, in priority order

1. **The headless demo does not show Slice 2.** Loot, skills,
   mills, and squads are each tested in isolation but never
   demonstrated together in one run. A Slice 2 demo scenario
   is the cheapest proof that the four systems compose.
2. **`GameManager.LoadFromJson` still only logs a load** and
   does not replace `Simulation.World` in place (noted in
   `docs/orbital-view.md`). Until that is fixed, the Unity
   save-then-view loop cannot be verified on Windows, and
   Slice 2's persistence work is headless-only in practice.
3. **No Unity verification at all yet** — for Slice 1 or
   Slice 2. Every adapter (including `OrbitalSquadDisplay`)
   is explicitly uncompiled headlessly and needs Unity 6000
   on Windows. The checklists in `docs/slice-01-playtest-gate.md`
   and `docs/orbital-view.md` are the verification plan, not
   verification.
4. **No balance pass.** Numbers (30% drop rate, mill cost and
   5-tick cadence, squad 90 HP / 30 damage) are first-pass
   values chosen for determinism and testability, not tuned
   by play. Do not quote them as final balance.
5. **Doc debt found by this review:** `docs/slice-01-playtest-gate.md`
   still said loot was "NOT integrated into Simulation". That
   was true at 0.4.1 and false since 0.6.0; it is corrected
   alongside this review (0.12.0), so the gate doc no longer
   contradicts the changelog.

## Slice 2 verdict (headless)

Slice 2 is **reviewed and closed as a set of tested, integrated
core systems**. It is not playtested, not balanced, and not
Unity-verified. The next work, in order, is: (1) a Slice 2 demo
scenario proving the four systems together; (2) the
`GameManager.LoadFromJson` in-place restore; (3) the doctrine
menu / rally picker replacing provisional O (with deployment
cost/cap decided as part of that step, not smuggled in early);
then, one at a time, further doctrines, squad types, or
automation tiers. **Slice 3 is not scoped by this review** —
its scope is the decision for the run after the demo and
load-restore gaps above are closed, so it is chosen with
composition evidence in hand rather than guessed now.
