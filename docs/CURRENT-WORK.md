# Current work — continuation checkpoint

Objective: the full original studio-quality 3D FPS/orbital voxel ARPG vision in
`design-pillars.md`. All three director reference attachments were read in full,
including the latest `822e6f7b-7290-4abb-bde3-ce79982ac131/goal-objective.md`.
The studio goal is active, not complete. Do not mark it achieved merely because
the current objective text says to read its file.

Working branch: `codex/unity-main-scene`; never push to main or alter visibility/
licence without Rob. Base remote main was 0.112.0 (`3e28b575...`). Integration
commit `b65233e`, runtime/cadence commit `292c82b`, courtyard/player flow `91e056e`.
The next checkpoint contains exact runtime persistence; inspect Git status first.

Canonical law for this continuing invocation is frozen and verified at
`63d99cdc72a1c18b5aa91b9336d02fd30126407b`, Constitution 6.18.0. Byte-exact source
is in `~/.codex/worktrees/forgeborne-law-63d99cdc`. One required evolution was
already completed locally in the separate `forgeborne-cadence-evolution` worktree,
branch `codex/realtime-cadence-evidence`, commit `922dcbb`. It is validated and
apply-ready, staged/not propagated, and cannot govern this invocation. Do not
start a second evolution obligation for this continuation.

Unity 6000.3.27f2 is open on saved Main. After the last QA it was stopped.
MCP for Unity 10.0.0 is pinned to upstream `d49ae295...`; a task-owned loopback
server runs on port 8080 under Python 3.10. Tools are reached through the actual
MCP SDK client in `tools/unity-mcp-client.py`, selecting the exact instance
`ForgeborneUnity@9fc4dca83b83a1f1`. No native Unity namespace was exposed in the
initial chat. Probe instances before assuming live connection.

Local client interpreter:
`~/.codex/tools/unity-mcp-v10/Server/.venv-py310/Scripts/python.exe`.
Example: `<python> tools/unity-mcp-client.py --instance ForgeborneUnity@9fc4dca83b83a1f1 --tool execute_code --code-file tools/unity-main-smoke.cs`.
Use JSON files for complex tool arguments; always stop dependent shell commands
on nonzero exit. Asset refresh and Play tools can acknowledge before domain
reload/import/startup has completed; verify readiness and actual scene state.
An unfocused Game view can produce zero rendered frames; such a probe is not
runtime acceptance. Foreground the real Game view for time/input observations.

Core tests: build with installed SDK, then run the net8.0 assembly with the
isolated `~/.codex/tools/dotnet-runtime-8.0.33/dotnet.exe`. That runtime was
downloaded from Microsoft and SHA-512 verified. Latest **5145 pass, 0 fail;
build 0 warnings/errors**. `tools/build-unity-core.ps1` publishes netstandard2.1
and installs all nine DLLs. Never copy core source into Assets. Rebuild after
core changes, import, then observe the actual installed API in Unity.

One GameManager creates the one Simulation; all linked adapters share it.
AdvanceFrame supplies 120 Hz movement and candidate 2 Hz world progression,
with bounded hitches/jump buffering/terminal stop. The explicit Tick API retains
its old call-based contract. Input is legacy, Active Input Handling must be Both.
CourtyardEncounter populates the canonical chunk only. WorldActorDisplay reads
it; no separate state owner. Material block IDs and power tints reach the Unity
mesh. Custom shader is `Forgeborne/VoxelSurface`, currently 0 shader messages.

PlayerFlow builds real entry/pause/terminal/slot UI, controls pointer suppression
and normal pause time, handles restart confirmation, save-on-quit failure and
bounded atomic LocalSaveStore. The new application data path had no existing
slot before QA; the slot/backup now contain QA snapshots only and are untracked.
Actual UI save/load/death/restart and fresh-scene load passed. Negative load and
backup/confirmation methods also passed in live Unity. See the current evidence
report; do not promote it to full studio/fun acceptance.

Precise runtime persistence is now verified: exact pose/velocity/look, fractional
movement/world clock, queued jump and counters survive local saves and fresh-scene
UI load. Legacy/world-only saves keep their previous contract. See
runtime-persistence-checkpoint.md. Next work: export/import, real overhead command,
meaningful gather/build/combat feedback,
original audio and final art/animation, a native Windows build and 1440p profiling.
Windows standalone build support is installed. Full scope stays tracked in
production-status.md; Rob must judge the quality/fun gate.

For Windows UI, the computer-use skill was fully read. Use node_repl + @oai/sky,
not PowerShell UI automation. After context compaction read the relevant runtime
documentation before reusing window bindings. Select returned Unity window objects
and observe before each input; current title includes ForgeborneUnity / Main.
Do not re-use stale screenshot coordinates. No sub-agents were used or authorized.
