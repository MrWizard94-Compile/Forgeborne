# Exact runtime persistence checkpoint — 0.116.0

This is a verified persistence contribution to the active studio-quality goal.
Rob has not accepted the game as fun or final-quality. Work remains on branch
`codex/unity-main-scene`; nothing was pushed or published.

## Behavior

Interactive saves contain an optional, versioned Runtime record: exact feet pose,
vertical velocity/grounding, yaw/pitch, fractional movement clock/world-step phase,
queued jump, gathering remainder and earned gather/build/kill counters. Restoration
uses the existing Simulation, FpsController and Look objects. All linked adapters
continue reading GameManager's one Simulation. Input remains legacy/Both.

Runtime fields must be complete, finite, in supported ranges and consistent with
the saved player grid. Unsupported versions, missing required fields and invalid
clocks fail before the world is changed. The writer also rejects an invalid record.
Legacy/world-only saves retain their established grid-position, current-look and
session-counter reset behavior. No Core source was copied into Unity Assets.

## Evidence

Release build: **0 warnings / 0 errors**. **5145 checks passed, 0 failed** on
genuine .NET 8.0.33. New checks earn resources/builds/kills through production
rules, preserve fractional state and a pending jump, and compare 240 subsequent
frames against uninterrupted execution. Rejected records retain world identity
and exact serialized state. Existing legacy assertions remain intact.

An initial implementation changed the constructor's legacy spawn grid and broke
440 existing squad fixtures. That behavior was corrected at the interactive entry
boundary; tests were not weakened to accept the regression.

The rebuilt netstandard2.1 DLL was imported through real Unity MCP into Unity
6000.3.27f2. Its SHA-256 is
`6be89a70d6480a18b068f31453dcf642b19c5954c03f8d4a88419e6ae206b8ff`.
The saved Main scene passed shared-Simulation, mesh/collider, URP,
resource material-channel, four-HUD-binding and in-place JSON round-trip checks.

For a precise fixture, MCP advanced the paused canonical controller by 0.137 s
with forward/strafe/jump and changed aim. Windows UI then clicked Save Slot.
The slot matched the expected entire JSON snapshot. MCP changed motion and aim;
the actual Load Slot button restored the exact expected snapshot:

| Field | Restored value |
|---|---:|
| Feet X | 15.8399963 |
| Feet Y | 4.877778 |
| Feet Z | 16.5599976 |
| Yaw | 17.375 |
| Pitch | -12.125 |
| World tick | 0 |

After stopping and re-entering Play, clicking Load Slot again restored the same
entire snapshot, paused for explicit resume. These were actual menu interactions;
fixture setup and serialized-state assertions were scripted. The path contains
QA slots created during this invocation. Final scoped console query returned
**0 errors/warnings**. An earlier smoke call correctly refused to run outside Play;
it was rerun successfully after entering Play.

## Open delivery gates

Export/import, an actual overhead command camera, coherent gather/build/combat
feedback, original audio and final art/animation, a convincing complete encounter,
native Windows QA and sustained representative 1440p profiling remain required.
Core/Editor evidence does not establish native performance, final visual quality
or fun. Death/world-retention policy awaits director preference; no new policy
was inferred from silence.

Section 0: 1 FAIL (full experience incomplete), 2 PASS, 3 PASS for scoped checks,
4 FAIL (whole player/native flow open), 5 PASS, 6 PASS, 7 FAIL (representative
performance), 8 PASS, 9 PASS for this checkpoint, 10 PASS, 11 PASS with Unity
asset GUID variance, 12 PASS, 13 N/A (single integrator), 14 PASS, 15 PASS.

Governance remains frozen at Constitution 6.18.0, accepted SHA
`63d99cdc72a1c18b5aa91b9336d02fd30126407b`. The invocation's single evolution
candidate is already staged locally in the separate constitutional worktree,
commit `922dcbb`; not propagated and not retroactive law. Self-audit: exact
runtime persistence is verified within the stated boundaries; the full product
and director quality gate remain open.
