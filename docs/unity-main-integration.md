# Windows Main-scene integration checkpoint — 2026-10-31

Status: **partial integration; studio-quality gameplay and director acceptance remain open**.
This record replaces the pre-scene status in the 0.112.0 preparation checklist.
The next production work is tracked in [production-status.md](production-status.md).

## Identity and constraints

- Base: remote `main` 0.112.0, `3e28b575ccd4f0e77f543e47245fdc01ce7a7844`.
  The handoff named 0.110.0; the local checkout initially held 0.109.0 with
  uncommitted Unity-generated metadata/settings. Those files were preserved
  and the settings/asset identities required to reopen Main are versioned here.
- Local branch: `codex/unity-main-scene`. No push, release, visibility or licence change.
- Unity 6000.3.27f2 (`22188f149405`), Windows, DX12, NVIDIA GeForce GTX 1660 Ti.
- Active Input Handling: Both (`activeInputHandler: 2`), read through Unity's
  serialized project settings and retained in ProjectSettings.asset.
- GameManager Seed 42 creates the one Simulation. Core remains DLL-only in
  Assets/Plugins; no gameplay core source was copied into Assets.
- Frozen governing Constitution: 6.18.0,
  `63d99cdc72a1c18b5aa91b9336d02fd30126407b`. Byte-exact load, source validator
  and accepted-main CI were verified. The loader version matched.

## Changed package

- `Assets/Scenes/Main.unity`: serialized GameManager, Chunk, FPS camera, HUD,
  optional orbital/squad adapters, dusk lighting and original primitive weapon.
  GameManager links all five adapters. FpsInputAdapter.MeshRenderer is deliberately
  unassigned in edit mode so its existing production null-fill is exercised.
- `Assets/Editor/ForgeborneSceneBuilder.cs`: real Editor scene assembly and URP
  setup menus. Creation refuses an existing Main, a dirty current scene or Play
  mode. It never constructs a Simulation. The scene is also included directly;
  running the builder is not required to open it.
- `Assets/Rendering` and `Assets/Materials`: Unity-created URP renderer/pipeline
  and procedural sky material. All quality tiers select the same pipeline.
  This sky uses Unity's procedural shader, not the exact authored gradient from
  the art target. No final-art claim is made.
- Unity ProjectSettings, package lock and asset `.meta` files: reproducible input,
  build-scene registration, dependency resolution and serialized asset identities.
  UserSettings, caches, logs and temporary captures remain ignored.
- `Packages/manifest.json`: developer-only MCP for Unity 10.0.0, pinned to
  `d49ae2953580f3481beb1e084a1da2682f0b5610` from CoplayDev/unity-mcp (MIT).
  Existing gameplay package pins and core DLL bytes are unchanged.
- `tools/unity-mcp-client.py`: an actual MCP SDK client for this chat, where
  native Unity tool discovery was unavailable. Every tool call explicitly selects
  `ForgeborneUnity@9fc4dca83b83a1f1`; that identity was confirmed against the
  project's Application.dataPath in the live Editor. A different clone will have
  its own hash and must be selected from the instances resource.
- `tools/unity-main-smoke.cs`: live-scene checks and production in-place save/load
  round-trip, executable through MCP. It creates no Simulation.

## Reopen and use

1. Open `ForgeborneUnity` in Unity **6000.3.27f2**. Let packages import. Verify
   Active Input Handling is **Both**, then open `Assets/Scenes/Main.unity`.
2. Window → MCP for Unity → connect to the loopback HTTP server. The bridge is
   local development tooling, not a shipped game service. Use the pinned upstream
   server source and its lockfile:

   ```powershell
   git clone --branch v10.0.0 https://github.com/CoplayDev/unity-mcp.git <tool-folder>
   # Verify the checkout: d49ae2953580f3481beb1e084a1da2682f0b5610
   $env:DISABLE_TELEMETRY = '1'
   uv run --project <tool-folder>/Server --python 3.10 --frozen mcp-for-unity --transport http --http-host 127.0.0.1 --http-port 8080
   ```

   Python 3.10 was verified. Initial Python 3.14 raised upstream asyncio
   deprecation warnings; the task-owned server was restarted under 3.10,
   and the same Unity project reconnected successfully. No LAN bind is needed.

3. From the repository root, list instances and use the returned exact identity:

   ```powershell
   uv run tools/unity-mcp-client.py --resource mcpforunity://instances
   # Press Play in Unity before the following live check.
   uv run tools/unity-mcp-client.py --instance ForgeborneUnity@9fc4dca83b83a1f1 --tool execute_code --code-file tools/unity-main-smoke.cs
   ```

   The Python client's MCP SDK is pinned in its script metadata. Keep the server
   and client dependencies separate from game builds. Native tool registration
   is not assumed to have hot-reloaded into an already-running chat.

## Actual observations

| Check | Result and boundary |
|---|---|
| First Play | FAILED rendering: magenta chunk/model. Live MCP reported no render pipeline while the material used URP Lit. Observation stopped; no gameplay gate was credited. |
| Corrected Play | Unity-created URP assets removed magenta. Chunk, lit original weapon, sky and HUD were observed in the rendered Game view. |
| Ownership | Live reference-equality checks passed for FPS, mesh, HUD, orbital and marker adapters against GameManager.Simulation. Mesh renderer null-fill passed. |
| Geometry | 748 vertices / 374 triangles in the generated chunk. MeshCollider.sharedMesh and MeshFilter.sharedMesh were the same object. |
| Input | Actual Game-view W input changed feet position from (16,4,16) to approximately (15.947,4,16.059). Mouse motion changed yaw/pitch. A click reduced magazine ammo from 42 to 41. O opened the orbital picker and its live HUD prompt. These are input checks, not a successful survival encounter. |
| HUD | Four core lines updated from canonical state. Health reached 0 and shields 0 during idle pressure; that is a failed usability/pacing finding, not a successful survival test. |
| Save/load API | Live GameManager.SaveToJson → LoadFromJson preserved the same Simulation object, world tick and identical 44,004-character JSON. No file UI, restart or export/import flow was tested. |
| Overwrite protection | Calling CreateMain again in edit mode refused the existing scene; Main's asset GUID was unchanged. |
| Console | After the final imports and Play run, MCP read_console with error/warning filters returned 0 entries. The initial Editor had an inherited Input Manager deprecation banner; legacy input remains intentional. No project-wide warning-free release claim. |
| Core build | `dotnet build tests/Forgeborne.Core.Tests/Forgeborne.Core.Tests.csproj -c Release --no-restore --nologo`: 0 warnings, 0 errors. |
| Core tests | `DOTNET_ROLL_FORWARD=Major dotnet run --no-build --project tests/Forgeborne.Core.Tests -c Release`: 5061 passed, 0 failed, including the no-source-drift guard. Local runtime was .NET 9.0.22; exact .NET 8 execution remains unverified. |
| Performance | One live delta-time sample was 195.34 FPS at a 791×447 Game view. This is neither a sustained benchmark nor a 1440p/60 FPS acceptance result. |

Rendered capture: [Main in Play](evidence/2026-10-31/main-urp-play.png).
SHA-256: `8d0adea9ec09ed462acf95b96215781983f6a80d79ea2d0ad523934e42e62734`.
The capture includes a zero-health state and a rally picker; it is evidence of
current behavior, not a promotional image or an accepted visual target.

## Open production gates

The current adapters advance a world tick per rendered frame; enemy movement,
damage and automation therefore run at a different real-time pace on different
frame rates. This is explicitly mentioned in the inherited playtest document and
was not silently rebalanced in this integration checkpoint. Main also has no
enemy/objective visual adapter, no overhead camera transition, no pointer capture,
and no player-facing save/load/restart flow. The stock chunk shader does not
consume its power vertex colours or distinguish resource materials. The minimal
HUD and primitive world are not final art. Gather/build/workstation/survival,
scene-restart persistence, long-run stability and 1440p performance are unpassed.
The director's fun/quality gate remains pending.

## Section 0 — checkpoint audit

| # | Score | Evidence/limit |
|---|---|---|
| 1 | FAIL | End-to-end studio-quality gameplay remains incomplete; named above. |
| 2 | PASS | Exact Editor, DLL boundary, package lock, pipeline and MCP target observed. |
| 3 | PASS for scoped checks | Core build 0/0; final Unity console query 0 entries. Inherited input banner disclosed. |
| 4 | FAIL | Core/live wiring checks pass; complete player flow and failure/restart paths remain open. |
| 5 | PASS | Current status/setup/evidence docs match this checkpoint; historical records labelled. |
| 6 | PASS | Loopback tooling, explicit instance selection, no credentials or external publication. |
| 7 | FAIL | Representative 1440p frame-time/memory measurements still owed. |
| 8 | PASS | Unity 6000.3.27f2 and Both respected; .NET runtime deviation disclosed. |
| 9 | PASS | Serialized scene/assets/settings, tool source and exact dependency lock included. |
| 10 | PASS | Work used the actual GTX 1660 Ti; no paid generation or external service introduced. |
| 11 | PASS with variance | Saved Unity artifacts plus Editor assembly path; generated asset GUIDs need not be byte-identical. |
| 12 | PASS | Original scene/code/primitive model; MIT development bridge identified; licence unchanged. |
| 13 | N/A | No sub-agent delegation in this checkpoint. |
| 14 | PASS | Manifest, commands, evidence, gaps and next production work recorded. |
| 15 | PASS | Self-audit below. |

Self-audit: live Unity execution established wiring, geometry, input and HUD.
The original magenta failure was observed and repaired at its rendering cause.
Core builds and 5061 tests passed, with the runtime deviation stated.
Production save/load was checked only in memory, not through a player UI/restart.
The game quality, survival, persistence and representative performance gates remain open.
No push, release or director acceptance is implied by this checkpoint.

Skill evolution: elapsed-time/callback-rate verification candidate, local commit
`922dcbb`, validated with source checks and 105 unit tests (104 pass, one
case-sensitive-filesystem test skipped on Windows). Staged, not propagated;
it does not govern this invocation.
