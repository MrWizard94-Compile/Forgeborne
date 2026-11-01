# Current work — continuation checkpoint

The full original studio-quality 3D FPS/orbital voxel ARPG goal remains active.
All three director reference attachments, including the latest
822e6f7b-7290-4abb-bde3-ce79982ac131/goal-objective.md, were read fully.
Do not mark the goal complete merely because its current text says to read that
file. Concept/pre-production continue; full production and fun acceptance are open.

Working branch: codex/unity-main-scene. No pushes, main mutation, visibility or
licence changes. Remote base main was 0.112.0, 3e28b575. Local checkpoints:
b65233e Main/URP/MCP, 292c82b frame pacing/actors, 91e056e courtyard/player flow,
51282d3 exact runtime persistence, fc35868 Windows transfer/native packaging.
The next checkpoint is perspective orbital camera work; inspect Git status.

Governance is frozen and verified at Constitution 6.18.0, accepted SHA
63d99cdc72a1c18b5aa91b9336d02fd30126407b. Byte-exact source is in
C:/Users/Bulkl/.codex/worktrees/forgeborne-law-63d99cdc. Exactly one required
evolution was already completed locally, separate worktree forgeborne-cadence-evolution,
branch codex/realtime-cadence-evidence, commit 922dcbb. Validated/apply-ready,
staged/not propagated, not retroactive law. Do not start another obligation.

Unity 6000.3.27f2 is open on saved Main. The latest orbital QA ended stopped.
MCP for Unity 10.0.0 is pinned to upstream d49ae295... and a task-owned HTTP server
runs on 127.0.0.1:8080 under Python 3.10. It was restarted (current exec session
48745/PID 32036 at last observation). Verify current state before process actions.
Use the real SDK client tools/unity-mcp-client.py and exact instance
ForgeborneUnity@9fc4dca83b83a1f1; probe instances before assuming connection.
Interpreter: C:/Users/Bulkl/.codex/tools/unity-mcp-v10/Server/.venv-py310/Scripts/python.exe.
Use JSON files for complex args and stop dependent commands on nonzero exit.
Refresh/Play acknowledgments can precede import/startup. Restart Play after runtime
script edits; nonserialized Simulation links must come from fresh GameManager.Start.

Core checks: 5155 passed, 0 failed, Release build 0 warnings/errors on genuine
.NET 8.0.33 at C:/Users/Bulkl/.codex/tools/dotnet-runtime-8.0.33/dotnet.exe.
Rerun tests/Forgeborne.Core.Tests/bin/Release/net8.0/Forgeborne.Core.Tests.dll after
building when Core changes. tools/build-unity-core.ps1 publishes netstandard2.1
and installs all nine DLLs. Never copy Core source into Assets.
Current Core SHA-256: 173a8ab63d40bb4891a4c5f9218e213a0c61912e5958960e3ac11f02e162999c.

One GameManager creates the one Simulation, Seed 42, and fills linked adapters.
AdvanceFrame owns 120 Hz physics/candidate 2 Hz world ticks; explicit Tick keeps
its legacy call-based contract. Both input is required (activeInputHandler 2).
The authored courtyard, original shader/material channels, actor views and player
entry/pause/terminal/restart/local saves are integrated. Exact interactive saves
retain pose/motion/look/fractional clocks/queued jump/counters; old saves retain
legacy behavior. Invalid records fail before changing the world.

Windows export/import passes actual Editor UI: cancel, Unicode export, malformed
rejection and exact restore with local slot retained. Original native bridge source
is native/Forgeborne.Windows; tools/build-windows-bridge.ps1 builds its x64 DLL.
Stop Editor/player before replacing a native DLL already loaded. Current native
SHA-256: aac9637da3eb96bb0f683b2eaf0e7978b4f779aa4ab73b518b27109d45222c9c.
Managed COM/Join first stalled the Editor; saved Main was recovered and backups
preserved under ignored Assets/_Recovery. Async native STA is verified in Editor.
Resources materials retain opaque/emissive URP Lit variants in player packaging.

Native 0.117 ordinary Windows/Mono build succeeded: options None, 0 errors/warnings,
101777147 reported bytes, 83.13 s. Output:
ForgeborneUnity/Builds/Windows-QA/20261101T080710Z/Forgeborne.exe.
The earlier Development build 20261101T074701Z also built cleanly (180 MB) and
rendered its 1920x1080 entry menu, then prompted Windows Firewall. The QA process
was closed at its paused entry; no security UI/permission was changed. The Windows
prompt remains open. A pending async request asks Rob to click Cancel manually.
The computer-use skill forbids operating Windows security/permission prompts.
Do not click it, change firewall settings or kill security processes.

The empty request 20261101T075254Z never built: the prompt held delayed callbacks.
Only that task's delay delegate was cancelled before source changes. The successful
ordinary build used Editor update. Build utility now schedules there and rejects
queued/concurrent requests. Use Forgeborne > Build Windows QA Player (via MCP menu
API is valid) with Play stopped and authored scenes saved. Fresh timestamped folder;
Library/forgeborne-last-build.txt points to it, build-report.json proves completion.
Do not infer completion from acknowledgment. No native full play/performance yet.

Perspective orbital camera work is implemented and Main wired. OrbitalCameraRig
reads Manager.Simulation only; FpsInputAdapter stays the sole positive frame driver.
Tab changes mode, WASD pan, Q/E rotate, wheel zoom, F focus suit, existing O/1-7/
Enter/Escape and K/X commands use cursor rays. FPS movement/look/build/fire are
suppressed in orbital mode; body/world stay live by implementation. One camera
is enabled/tagged MainCamera; FPS gun/reticle/controls hide, collision-free suit
follows canonical feet/yaw. Mode switches cancel free pending commands.

Fresh paused Main ran tools/unity-orbital-smoke.cs successfully: both view switches,
perspective/tag/pointer, suit pose/no colliders, FPS restoration, exact full JSON,
controller/Look/Simulation identity and enabled sole driver. Final console 0.
Actual camera-only Unity render (no overlay UI) is
 docs/evidence/2026-11-01/orbital-prototype.png (1100x506, prototype art).
This is scripted component/render evidence, not keyboard/elapsed-time/native/fun
acceptance. Camera source now versioned as candidate 0.118.0; the 0.117 executable
predates it. See docs/orbital-camera-checkpoint.md for evidence/open gates.

Next: finish actual keyboard/cursor/cadence and native flow when the prompt is
manually dismissed; then meaningful gather/build/combat/loot feedback, original
audio, final environment/weapon/actor art and animation, a complete convincing
encounter, native regression and representative 1440p profiling. Retain full six-game
vision. An optional death/world-retention preference is also pending; keep current
load/restart behavior until Rob answers. A passing check does not prove fun.

For Windows UI use computer-use skill and node_repl + @oai/sky. Re-read runtime
docs after compaction. Re-select returned window objects, observe before each
input, never operate the current security prompt. Last Editor window ID 40632432;
old native window 1183036 is closed. No agents were spawned/authorized. Relevant
memory used root routing; final replies need the memory citation block (registry
MEMORY.md:298-314, ad-hoc root-context note lines 3-6). Never update memories unless
Rob explicitly requests it.
