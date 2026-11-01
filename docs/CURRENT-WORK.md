# Forgeborne continuation checkpoint

Full objective: original studio-quality 3D PC-native FPS + orbital voxel ARPG,
with the strengths of modded Minecraft, StarCraft, Diablo, Path of Exile 2,
Borderlands and BioShock. Rob directs; Codex implements and verifies. First
polished gather → build → survive encounter precedes system/content expansion.
All three director reference attachments were read fully. Reading the objective
file or passing source tests does not complete the game. Full acceptance is open.

## Authority and current checkpoint

Worktree C:/WPAI/Gaming/Forgeborne, branch codex/unity-main-scene, remote base main
0.112.0 / 3e28b575. Prior coherent commits: b65233e integration, 292c82b pacing,
91e056e flow, 51282d3 persistence, fc35868 Windows transfer, bdca2e5 orbital.
Current 0.119 action feedback is ready for its local commit; inspect Git.
No push/main/visibility/licence change or publication is authorized.

Frozen law: Constitution 6.18.0 accepted SHA
63d99cdc72a1c18b5aa91b9336d02fd30126407b. Verified byte-exact source worktree:
C:/Users/Bulkl/.codex/worktrees/forgeborne-law-63d99cdc. Exactly one evolution
obligation is already fulfilled: separate codex/realtime-cadence-evidence commit
922dcbb, elapsed-time/callback verification. Validated apply-ready patch is
staged/not propagated, not retroactive law. Do not start another obligation on
continuation/compaction. Historical changelog/decisions were selectively read.

## Runtime and tooling

Unity 6000.3.27f2 pinned; Both legacy/new input (activeInputHandler 2).
GameManager Seed 42 creates the one Simulation and fills linked adapters.
FpsInputAdapter is the sole positive frame driver in either camera mode.
Core ships as nine DLLs in Assets/Plugins; never copy Core .cs into Assets.
Main GUID 0d109928345a90042a5b2b771ef3134a. Preserve in-place load/restart identity.

Unity MCP 10.0.0 upstream d49ae2953580f3481beb1e084a1da2682f0b5610; task server
Python 3.10 at 127.0.0.1:8080/mcp. Probe mcpforunity://instances; exact instance
ForgeborneUnity@9fc4dca83b83a1f1. Client tools/unity-mcp-client.py, interpreter
C:/Users/Bulkl/.codex/tools/unity-mcp-v10/Server/.venv-py310/Scripts/python.exe.
Use code/JSON argument files; check exit codes and actual import/Play state after
acknowledgments. Stop Play before source/DLL edits; start fresh after import.
Do not create another Simulation to repair stale Play links.

Core verification: dotnet build tests/Forgeborne.Core.Tests/Forgeborne.Core.Tests.csproj
-c Release --no-restore --nologo, then run Release test DLL with genuine isolated
C:/Users/Bulkl/.codex/tools/dotnet-runtime-8.0.33/dotnet.exe. Latest: 5165 pass,
0 fail, build 0 warnings/errors. tools/build-unity-core.ps1 publishes nine DLLs.
0.119 Core SHA-256:
33dbb76b872e99b9995ab2ff9b6e6d8369179a1ca9d4ff9a2e1f3134596e23fa.

## Evidence and open work

Main includes player flow, exact saves, native transfer, authored courtyard,
canonical actors, perspective command view and action feedback. Eleven original
oscillator/noise synthesis clips have a generator and source signal manifest.
Heard mix is unverified. Actual Core gather already preserves a surface at full
inventory; an earlier assumption that it consumes the resource was wrong.

Fresh paused Main passed tools/unity-feedback-smoke.cs through production adapter
paths: actual damage/ammo, trace/message, playing effects source, gather/build,
reload, full-pack exact preservation and immediate reset. Fixture restores full
JSON in the same Simulation. tools/unity-orbital-smoke.cs passed again. These are
scripted live checks, not keyboard/elapsed-time/native/fun acceptance.

Ordinary Windows x64/Mono 0.119 package passed: None, 0 errors/warnings,
101864587 reported bytes, 53.23 s. Output:
ForgeborneUnity/Builds/Windows-QA/20261101T112210Z/Forgeborne.exe. Packaged Core and
Windows bridge match installed DLL hashes. Executable/DLL identities and report:
docs/evidence/2026-11-01/windows-0119-build.json. This player has not been launched.
Use Forgeborne > Build Windows QA Player; verify completed build-report.json.
No parallel/overwritten builds. After packaging Editor can have an unloaded scene;
check for dirty authored scenes before reopening saved Main. Play is stopped.

Pre-build runtime console was zero. Post-build MCP classified URP inventory text
as Warning; installed URPPreprocessBuild.cs:73 emits it with Debug.Log, while
actual BuildReport warnings are zero. Record this classification difference.

Windows helper twice failed to activate fresh returned Unity window 40632432;
no further input was sent. Rob replied he is not home and cannot bring Unity
forward/check prompts. Continue independent MCP/source/build work. Do not keep
requesting manual action now. Previous development player triggered firewall
prompt and was closed at entry; no security permission/setting was changed.
Current prompt state is not freshly established. Do not operate security UI.
Windows UI uses computer-use skill, node_repl + @oai/sky, fresh returned windows;
read runtime docs after compaction. Do not manipulate unrelated user apps.

Next: reproduce placement safety/readability issues before changing contracts,
complete original environment/actor/weapon art/animation and convincing encounter,
then native/input/heard mix/save recovery/1440p performance and Rob's quality/fun
review when available. World.Build inspection found no actor/body occupancy check;
not yet a reproduced fixture. Optional death/world-retention preference unanswered;
retain current load/restart policy. Full six-reference vision remains in scope.
See action-feedback-checkpoint.md, production-status.md, design-pillars.md.

No subagents spawned/authorized. Memory used for independent-repo/root routing;
final replies require memory citation MEMORY.md:308-314. No memory updates without
Rob's explicit request.
