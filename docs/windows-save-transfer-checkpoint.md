# Windows save transfer checkpoint — 0.117.0

Export/import is verified in the actual Unity Editor player interface. Native
Windows player packaging/runtime verification is in progress. This is a scoped
contribution to the full studio-quality goal, not product or fun acceptance.

## Behavior and ownership

The pause/entry menu offers Export Save and Import Save. Export writes the current
canonical snapshot through the existing bounded, atomic UTF-8 store. Import reads
the chosen file through that same 2 MiB boundary and validates it before replacing
the encounter. Import leaves the local slot unchanged. Cancellation leaves the
current encounter and slot untouched. The player remains paused until Resume.

An original, small Windows x64 platform DLL uses the OS Common Item Dialog.
It owns no simulation, JSON parsing, save storage or gameplay. A background STA
and asynchronous Unity continuation keep the owner window responsive. The menu
blocks duplicate transfer actions while a picker is open. The native bridge uses
static CRT linkage and the Windows SDK; it adds no paid/service dependency.

Import also rejects null collections, unsafe extreme legacy player/enemy
coordinates and oversized complete weapon names. Null enemy/squad entries follow
the established malformed-entry skip rule. Enemy health clamps before subtraction
to avoid overflow. Weapon labels render as literal text. Legacy valid saves and
GameManager's one shared Simulation remain supported. Core source stays outside
Assets; the Unity core is installed as DLLs.

## Evidence so far

- Release core build: **0 warnings/errors; 5155 passed, 0 failed**, .NET 8.0.33.
- Original native bridge: MSVC x64 `/W4 /WX`, static CRT, successful build.
  Installed SHA-256:
  `aac9637da3eb96bb0f683b2eaf0e7978b4f779aa4ab73b518b27109d45222c9c`.
- Installed core SHA-256:
  `173a8ab63d40bb4891a4c5f9218e213a0c61912e5958960e3ac11f02e162999c`.
- Real Unity MCP revalidated the scene, mesh/collider, material channels, HUD,
  shared Simulation and exact in-place snapshot recovery.
- Actual Windows UI opened Import, cancelled it and returned to a paused menu;
  canonical JSON and slot bytes were unchanged.
- Actual Export chose `Forgeborne-テスト.json`; the resulting file matched the
  entire canonical JSON and left the local slot unchanged.
- After scripted movement/aim changes, actual Import chose malformed JSON. It
  reported rejection and retained the entire encounter and slot.
- Actual Import then chose the exported Unicode file. It restored the entire
  snapshot exactly, retained the shared Simulation and left the slot unchanged.

The first managed COM approach blocked Unity while joining a dialog thread.
Only the verified non-responsive task Editor was stopped; the saved Main scene
was versioned and its recovery backup was preserved locally. The replacement
native/async bridge first reported an apartment mismatch safely; explicitly
requesting STA corrected it. Those failed attempts are not passing evidence.
Editor startup also reported the expected legacy-input deprecation and an asset
timestamp race while source changed during import. Input Both remains required;
the changed source was reimported. A build-report utility's initial int/uint
compile error was corrected before packaging.

## Native packaging and external UI boundary

Unity built the first Windows x64/Mono development player successfully in
143.52 s: **0 errors, 0 warnings, 180,264,678 reported bytes**. The packaged Core
and Windows DLL hashes matched the installed hashes above. It launched and
rendered the entry menu at 1920 x 1080 on GTX 1660 Ti / Direct3D 12.

Windows then presented a Firewall permission prompt for that development player.
No security UI was operated and no network permission was granted. The task QA
player was closed at its paused entry screen. The build utility now uses ordinary
`BuildOptions.None`, records backend/options and does not enable Unity's development
connection. The remaining Windows prompt requires manual dismissal under the
computer-use tool's safety restrictions.

The prompt also held Unity's delayed callbacks. A second build request had not
started: direct inspection showed `building=false` and the task build delegate
still queued. Only that delegate was cancelled before continuing source work.
Its empty `20261101T075254Z` output folder is not a completed build. Native
movement, pause, transfer and recovery remain unverified; reschedule a fresh
ordinary build after the prompt is dismissed. No full native-flow acceptance is
claimed from the development player's entry menu.

Later follow-up: the ordinary 0.117 build passed with options `None`, zero
errors/warnings and 101,777,147 reported bytes using the helper on Editor update.
Output/evidence and the continuing native-input boundary are recorded in
orbital-camera-checkpoint.md. No security UI was operated.

Unity serialized its default volume components, runtime settings, shader
prefilter values and standalone batching during packaging. These generated
settings are retained with the verified pipeline; they are not an authored final
visual grade. Runtime shader references are described below.

## Packaging and remaining gates

Runtime-generated opaque/emissive actor materials use URP Lit. Two referenced
Resources materials preserve those shader variants in a player build; Editor
Shader.Find success alone does not establish packaging. The native plugin is
enabled only for Windows x64 and the Windows Editor. Native rendering, file
transfer, movement and recovery still need verification in the built player.

Overhead command, complete gather/build/combat feedback and encounter design,
original audio, final art/animation, sustained representative 1440p profiling,
and Rob's fun/quality review remain required. Current source checks and Editor
observations do not close those gates.

Section 0: 1 FAIL (full goal incomplete), 2 PASS, 3 PASS for scoped checks,
4 FAIL (whole native/player flow open), 5 PASS, 6 PASS, 7 FAIL (representative
performance), 8 PASS, 9 PASS for scoped recovery, 10 PASS, 11 PASS with Unity
asset/build identity variance, 12 PASS (original work/framework APIs), 13 N/A,
14 PASS, 15 PASS. No push, licence/visibility mutation or publication occurred.

Frozen law remains Constitution 6.18.0, accepted SHA
`63d99cdc72a1c18b5aa91b9336d02fd30126407b`. The invocation's single evolution
candidate remains staged/not propagated in separate commit `922dcbb`.

Primary implementation references: Microsoft's [Common Item Dialog](https://learn.microsoft.com/en-us/windows/win32/shell/common-file-dialog),
[dialog options](https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/ne-shobjidl_core-_fileopendialogoptions)
and [shell path ownership](https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-ishellitem-getdisplayname);
Unity 6000.3 [shader lookup and inclusion](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/shader/find)
and [BuildPlayer](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/buildpipeline/buildplayer).
