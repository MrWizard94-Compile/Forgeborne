# Action feedback checkpoint — 0.119.0

Main now presents resolved FPS actions with original sound candidates, muzzle
flash and weapon motion, short pulse traces, impact signals, target labels,
damage messages and suit damage feedback. This is pre-production work. Audible
mix, native input, sustained performance and Rob's quality/fun review are open.

## Behavior and ownership

Core's new read-only `InspectAim` uses the action's existing canonical trace.
`FireAimed(out ShotOutcome)` reports the actual target cell, clamped damage and
destruction after applying the existing rule. The original `bool FireAimed()`
contract remains available. Misses/occlusion/ammo/damage/cadence are unchanged.

FpsInputAdapter calls the existing actions, refreshes changed voxel geometry,
then passes results to PlayerFeedback. Feedback never creates a Simulation,
advances time or applies damage/build/gather rules. It owns its two AudioSources,
pooled trace/impact objects, material and overlay. The one listener stays on the
physical suit's FPS camera, including during orbital command. Footsteps follow
grounded travel; actual shield/health loss drives suit signals. World replacement
resets observations without fabricating a hit.

View/load/restart interaction resets clear traces, action text, tint, recoil and
effects playback immediately, without waiting for another frame. Full-pack gather
retains the canonical surface and reports `PACK FULL · SURFACE KEPT`; it does not
claim consumption or change the existing inventory rule. Reload feedback describes
the existing instant gameplay reload, with a short presentation animation.

`tools/generate-original-audio.py` uses only original oscillator/seeded-noise
synthesis and Python's standard library. Eleven deterministic mono 48 kHz,
16-bit PCM WAVs live in Assets/Resources/Audio. They cover discharge, salvage,
construction, magazine latch, hit, suit impact/breach, refusal, footfall, relay
collapse and a looping foundry bed. No recordings, external samples or generated
third-party assets were used. The [signal manifest](evidence/2026-11-01/audio-signals.json)
records duration, peak/RMS and each source hash. Signal checks and a playing
AudioSource do not establish audible quality.

## Evidence

- Release build: **0 warnings, 0 errors**. Genuine .NET 8.0.33 runs the complete
  **5165 passed / 0 failed** suite. Ten new checks cover read-only inspection,
  canonical target identity, actual ammo/damage, occlusion, clamped overkill,
  miss/dry fire and relay destruction. No prior assertions were weakened.
- The relay fixture initially stood inside authored eastern cover. The trace
  correctly hit that cover; the fixture was moved onto the relay platform and
  now asserts a clear sightline before testing damage. Production cover stayed.
- `tools/build-unity-core.ps1` published all nine DLLs. Installed Core SHA-256:
  `33dbb76b872e99b9995ab2ff9b6e6d8369179a1ca9d4ff9a2e1f3134596e23fa`.
  Core source remains outside Assets; the dynamic no-drift check passes.
- Unity MCP, exact instance ForgeborneUnity@9fc4dca83b83a1f1, imported all eleven
  clips as mono 48 kHz resources and saved feedback references in Main.
- Fresh started/paused Main ran `tools/unity-feedback-smoke.cs`. The production
  adapter paths passed actual damage/ammo, trace/message, playing effects source,
  gathering/building/reload, full-pack refusal with exact state retained and
  immediate signal reset. Refusal/presentation did not alter canonical JSON.
  Fixture cleanup restored the original complete save in the same Simulation.
- `tools/unity-orbital-smoke.cs` also passed again: both perspective handoffs,
  canonical suit pose, no extra collider, exact serialized state and Simulation/
  controller identity, with the sole frame driver enabled.
- Scoped final Unity console: **0 warning/error entries**. These scripted
  component checks do not prove keyboard/mouse play or elapsed-time behavior.

Commands: `dotnet build tests/Forgeborne.Core.Tests/Forgeborne.Core.Tests.csproj
-c Release --no-restore --nologo`, isolated .NET 8 execution of that Release test
DLL, `tools/build-unity-core.ps1`, and `python tools/generate-original-audio.py`.
The live scripts run through the real MCP client `tools/unity-mcp-client.py
--instance ForgeborneUnity@9fc4dca83b83a1f1 --tool execute_code --code-file ...`.

## Packaging and remaining gates

The previous ordinary 0.118 Windows x64/Mono package succeeded with options None,
0 warnings/errors, 101,783,851 reported bytes in 21.35 seconds, output
ForgeborneUnity/Builds/Windows-QA/20261101T084917Z.

The fresh **0.119.0 Windows x64/Mono build succeeded**, options None, **0 errors,
0 warnings, 101,864,587 reported bytes, 53.23 seconds**. Output:
ForgeborneUnity/Builds/Windows-QA/20261101T112210Z/Forgeborne.exe. Its packaged Core
and original Windows bridge hashes match the installed DLLs. The
[build evidence](evidence/2026-11-01/windows-0119-build.json) includes executable
and DLL identities. This player has not been launched or playtested.

After packaging MCP returned one entry classified Warning: `1 URP assets included
in build`. Installed URPPreprocessBuild.cs:73 emits that inventory through
`Debug.Log`, and the native BuildReport has zero warnings. This informational
entry is recorded rather than suppressed; the pre-build runtime console was zero.

Windows automation twice failed to activate the returned Unity window after
fresh selection. No further input was sent. An async request asks Rob to bring
Unity forward and dismiss any Windows security prompt manually. Rob replied that
he is not home; native input checks will wait while independent work continues. No firewall
permission or security setting was changed. Native play, audible output/mix and
1440p frame-time/memory acceptance remain open. Do not infer these from packaging.

Changed-path manifest: Core Simulation/ShotOutcome; ActionFeedbackTests/runner;
Unity PlayerFeedback/FpsInputAdapter/WeaponViewModel/SceneBuilder, saved Main,
Core DLL and version; original Audio WAVs/metas; audio generator and live smoke;
this report, signal manifest, current-work/production status, README and changelog.
No new gameplay package, network service or licence change. Local QA builds stay
ignored. Rollback is the previous coherent branch commit `bdca2e5`; preserve work
with a reviewed revert rather than resetting shared history. Nothing is pushed.

Section 0 for the full goal: 1 FAIL (studio-quality loop unfinished), 2 PASS,
3 PASS for scoped builds/runtime checks, 4 FAIL (actual native/player flow open),
5 PASS, 6 PASS, 7 FAIL (representative hardware measurements open), 8 PASS,
9 PASS for the local package, 10 PASS, 11 PASS with documented
Unity serialization/build variance, 12 PASS (original synthesis/code),
13 N/A (no delegated agents), 14 PASS, 15 PASS.

Self-audit: resolved facts, original resources and production presentation paths
have source and live component evidence. Transient cleanup and full-pack safety
were checked. Current feedback is a candidate, not a studio-quality acceptance.
Native input, heard mix, complete encounter, performance and director review are
still required; the full six-reference vision remains in scope.

Frozen law: Constitution 6.18.0, accepted SHA
`63d99cdc72a1c18b5aa91b9336d02fd30126407b`. Skill evolution: elapsed-time/callback
verification amendment, separate commit `922dcbb`, **staged, not propagated**.
