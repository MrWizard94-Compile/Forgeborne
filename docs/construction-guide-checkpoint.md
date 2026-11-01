# Mechanical construction guide — 0.121

This is a preserved mechanical checkpoint. Rob rejected the actual 0.120
presentation as amateur and specified AAA+ for the finished game. This work
does not pass visual/experience/fun acceptance. Feature expansion is paused
while the original 3D environment, models, lighting, animation and atmosphere
are brought up to the requested bar.

V toggles the construction tool; wheel selects among the nine existing
structures, and LMB places the selected type. Movement/world pressure continue.
Direct build keys remain. The collision-free outline is the canonical cell,
with have/need resources and ready/short/blocked status. Pause, load/reset,
orbital handoff, terminal state and pending command modes cancel the tool.
Build mode routes primary click to construction; it does not consume ammo.
The post-frame input gate also rejects actions after a lethal/completing tick.

Core now owns a single immutable price table, read-only World.InspectBuild and
Simulation.InspectBuildAhead. Actual placement revalidates the live world and
payment, so a stale preview cannot build through a new occupant or spend an
outdated resource balance. No new structure, price, damage, timer or save schema.
The existing recipes are preserved:

| Structure | Stone | Ember | Timber |
|---|---:|---:|---:|
| Wall | 2 | 0 | 0 |
| Workstation | 10 | 5 | 0 |
| Kinetic mill | 10 | 0 | 5 |
| Dynamo / Transformer | 0 | 5 | 0 |
| Cable | 0 | 1 | 0 |
| Alternator | 10 | 0 | 0 |
| Shock pylon | 5 | 10 | 0 |
| Conduit | 5 | 5 | 0 |

The original trade-offs remain: walls/mills/alternators compete for stone;
workstations/electrical devices/squads compete for ore; mills also need timber.
An alternator is native HV priced in stone; transformers add a separate ore
investment. Cable is priced per cell. A pylon's ten ore competes with a squad.
The table replaces duplicated price/payment branches, not the balance rationale.

Release build: 0 warnings/errors; genuine .NET 8.0.33: **5198 pass / 0 fail**.
The unchanged prior suite and 17 new preview/payment checks cover read-only
state, stale occupancy/resources, actual payment, all nine types, unsupported
types and skyward targeting. Core source remains outside Assets; all nine DLLs
published. Installed/packaged Core SHA-256:
3004e6def79fac0e3bdefb63f762119e23113aafe12408c63428dc7a155056c1.

Unity MCP saved the guide on the existing Main FPS adapter. Fresh paused Main
ran tools/unity-construction-guide-smoke.cs: exact canonical outline/no collider,
read-only resource-short/funded status, all nine choices/wrap, actual primary
placement with two-stone cost and no ammo loss, pause/view cancellation, exact
full state restoration in the one Simulation. Scoped console: zero entries.
This is scripted consumer evidence, not native V/wheel/click acceptance.

Windows x64/Mono packaging succeeded: options None, **0 warnings/errors,
101875659 reported bytes, 56.33 seconds**, output
Builds/Windows-QA/20261101T153052Z. Packaged Core matches installed Core. See
[build evidence](evidence/2026-11-01/windows-0121-build.json). This executable was
not launched. The earlier 0.120 native recovery evidence is a separate scope.

Changed paths: Core Construction/Simulation/World and preview tests/runner;
ConstructionGuide, FPS/flow/builder wiring, Main, Core DLL and version; consumer
smoke, this report, current-work/production/README/changelog and build evidence.
Rollback baseline: 0.120/9dcf10f. Changes are local, with no push or main mutation.

Section 0 for the full goal: 1 FAIL (AAA+ target), 2 PASS, 3 PASS scoped,
4 FAIL (complete native loop/experience), 5 PASS, 6 PASS, 7 FAIL (performance),
8 PASS, 9 PASS local package, 10 PASS, 11 PASS with Unity build variance,
12 PASS original code, 13 N/A, 14 PASS, 15 PASS. Self-audit: the guide shares
canonical prices/validation and has real consumer evidence. It does not solve
the rejected presentation or prove the finished game. No completion claim.

Frozen law: Constitution 6.18.0 accepted SHA
63d99cdc72a1c18b5aa91b9336d02fd30126407b. Skill evolution unchanged: elapsed-time
verification, 922dcbb, staged/not propagated; no new continuation obligation.
