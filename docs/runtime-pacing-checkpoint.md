# Runtime pacing checkpoint — 2026-10-31

Status: verified timing/integration slice; **studio-quality vertical slice remains active**.
Rollback: integration commit `b65233e`. Work continues locally on
`codex/unity-main-scene`; nothing has been pushed.

## Changed behavior

Simulation.AdvanceFrame now decouples rendering from game time: 120 movement
steps/second and a candidate two authoritative world ticks/second. The existing
explicit Tick API retains one world step per call. Render callbacks no longer
multiply enemy attacks or automation production. Hitches contribute at most
250 ms, invalid time is rejected, jump edges are buffered across short frames,
and death/victory stops encounter progression. Legacy load resets the fractional
clock while keeping saved ticks and the same Simulation object.

WorldActorDisplay renders original drone silhouettes and the core directly from
GameManager.Simulation.World. All targets remain in their canonical cells, with
no second collision/damage state. These are working visual views, not final art
or accepted animation. FpsInputAdapter captures/releases the pointer and ignores
game actions while uncaptured or terminal. GameManager.RestartEncounter restores
the initial encounter into the same Simulation, resets pending input and refreshes
the mesh. A player-facing terminal/restart menu is still owed.

Owned runtime materials/mesh now have teardown in FPS input, weapon, chunk and
actor views. The core publish script checks the native publish exit code and
verifies its exact staging parent/name before recursive cleanup. No source is
copied into Assets. Only the rebuilt core DLL changed among the nine shipped DLLs.

## Verification

- Full core rebuild: **0 warnings, 0 errors**. A previously latent nullable warning
  in the funded-patrol fixture surfaced on recompilation; an explicit non-null
  fixture guard fixed its cause without removing any assertion.
- **5094 passed, 0 failed**, run with isolated **.NET 8.0.33**, not runtime
  roll-forward. Runtime ZIP SHA-512 was verified against Microsoft's official
  release metadata before extraction outside the repository.
- Cadence checks compare movement, damage and mill production at 30/60/144/240
  callbacks/second, and cover hitches, invalid time, jump buffering, legacy save
  load, death/victory and the old Tick contract.
- `tools/build-unity-core.ps1` published netstandard2.1 and installed the nine
  dependencies. Core DLL SHA-256:
  `f6e6eb4dd830dee987f5e18dc2e2a7d84376ab4687e8abe4ce12198dd626404d`.
- Actual Unity Main callbacks in a foreground Game view: **3.051 seconds,
  888 rendered frames, 6 world ticks, 100 HP, same Simulation identity**.
  `tools/unity-cadence-smoke.cs` restarts through the production API and records
  that probe in Editor SessionState. This is a short cadence observation, not
  a sustained 1440p performance benchmark.
- The first probe ran while the application was unfocused and recorded **0
  rendered frames / 0 ticks**. That run failed to exercise the game and was
  rejected. The retry explicitly focused the actual Game view; no background
  setting was changed to manufacture a pass.
- Unity saw the AdvanceFrame method from the installed DLL, WorldActorDisplay
  was linked in saved Main, and the live actor root had three enemies plus one
  objective. Final scoped error/warning console queries returned 0 entries.
- A source-import race initially made an MCP in-memory compilation unable to
  resolve WorldActorDisplay. It was not treated as successful scene wiring;
  after import/domain reload, the component linked and the subsequent Play
  used the actual new assembly. Dependent CLI commands now stop on failure.

Reproduce core checks from the repository root:

```powershell
dotnet build tests/Forgeborne.Core.Tests/Forgeborne.Core.Tests.csproj -c Release --no-restore --nologo
# Use a genuine .NET 8 runtime for the net8.0 test assembly.
dotnet tests/Forgeborne.Core.Tests/bin/Release/net8.0/Forgeborne.Core.Tests.dll
.\tools\build-unity-core.ps1
```

The scoped runtime used locally is under the task's external Codex tools folder;
it is neither shipped with the game nor a change to the machine's global PATH.

## Gate and remaining work

Section 0: 1 FAIL (whole vertical slice incomplete), 2 PASS, 3 PASS for scoped
checks, 4 FAIL (complete player flow/recovery still open), 5 PASS, 6 PASS,
7 FAIL (representative sustained performance pending), 8 PASS, 9 PASS for this
checkpoint, 10 PASS, 11 PASS with Unity GUID variance disclosed, 12 PASS,
13 N/A (single integrator), 14 PASS, 15 PASS (audit below).

Self-audit: the original frame-dependent cause was repaired in the real runtime
entry and compared across callback rates. Core checks used exact .NET 8 and the
published DLL was observed in Unity. A failed background-only probe was rejected.
Actor views share canonical state; no second Simulation was created by the fix.
Final-quality animation/materials/world/audio, full input/terminal/persistence
flows, overhead command and native-build QA remain required. No fun acceptance,
studio-quality completion, release or publication is claimed.
