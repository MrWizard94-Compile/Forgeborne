# Authored art technical checkpoint — 0.122.0

**World visual acceptance FAIL.** Rob rejected the current cubic presentation and
clarified Minecraft's gather/build/survive/combat influence. All six influences
remain equally important. This checkpoint preserves reusable original models and
rendering repairs; it is not a visual milestone, a native review build, or the
requested finished game. Direction is committed as `ee108e8` on
`codex/unity-main-scene`; no push/main/publication occurred.

## Changed and verified

Original Blender script/source manifests and FBX exports now supply five authored
models: carbine/gauntlets, warden, relay, pressure suit and architectural context.
See art/README.md. Runtime actor/weapon/orbital views load these prefabs below a
neutral wrapper preserving import rest transforms. Their original public feedback
APIs and stable runtime roots remain. Art adds no collider or Simulation.

The importer validates signed X/Y/Z markers, metre scale, model bounds, named
material roles and muzzle position. Producer fixes corrected centimetre-scaled
meshes, overwritten import rotations and an X reflection. All five passed actual
Unity preparation after the final export, using Blender 4.5.15 LTS and Unity
6000.3.27f2/URP 17. Geometry manifests list bounds/counts; current artifact/source
hashes are in evidence/2026-11-01/authored-0122-assets.json.

Original IndustrialSurface/FoundrySky shaders, warm work lights, sky fill, local
reflection and the persisted grade now have real consumers. The earlier profile
and camera flags were insufficient: `ForgeborneRenderer.postProcessData` was null,
which installed URP UniversalRenderer.cs explicitly treats as disabled processing.
The editor operation now assigns URP's own resources, enables HDR grading and
persists contact shading. Reflection/soft-shadow variants match installed URP.

Fresh `unity-art-consumer-smoke.cs` rendered the actual Main camera at 256×144
with exposure 0.35 and 1.35: mean luminance **0.1394023 → 0.2479594**. It restored
exposure/targets and exact canonical JSON. It also verified the actual authored
suit/view handoff, model consumers and one shared Simulation. This measures a
post-process response, not attractiveness, fidelity, input or performance.

`unity-art-lifecycle-smoke.cs` removed only the runtime atmosphere component from
paused Play. Existing scene sky/flat ambient and exact game state were restored.
Play was then stopped, recovering the saved Main component. Scoped console was
zero; all three custom shaders had zero reported messages. Actual Main remains
saved with one manager and Both input. No user slot was written during this work.

Existing production-path feedback, perspective-orbital and construction-guide
smokes passed with the authored consumers and restored the same Simulation's
exact state. JSON outputs are under evidence/2026-11-01/*-authored.json.

Release Core/test build passed with **0 warnings/errors; 5198 passed, 0 failed**
under the genuine isolated .NET 8 runtime. This includes the dynamic no-Core-copy
guard over Assets. Core source/save behavior was not changed; installed Core SHA:
`3004e6def79fac0e3bdefb63f762119e23113aafe12408c63428dc7a155056c1`.

Source diff formatting passed. Unscoped Git formatting reports the Unity
serializer's trailing space on empty `value:` fields in generated prefab variants.
Those fields were classified separately: every reported prefab line is an empty
serialized value, with no other trailing whitespace. The per-command prefab check
omits only blank-at-EOL; no repository/global check setting was changed. Runtime
consumer validation includes the exact unmodified serialized prefab bytes.

## Unmet acceptance and next work

The current courtyard still uses cubic geometry and cannot pass Rob's brief.
Wardens retain cell-stepped motion; the suit has no skeletal animation; squad
markers remain primitive; the architectural context is not traversable/collidable.
The early PNGs action-camera-authored-candidate.png and foundry-fps-candidate.png
are internal rejected work-in-progress captures from before the final pipeline
repairs, not evidence of current final appearance. No current-art native package,
heard mix review, sustained 1440p measurements or director visual/fun approval.
The latest native package is 0.121 mechanics only; it does not contain this art.

Next is the shared surface/geometry migration in world-presentation-migration.md,
then the fully re-authored playable place and integrated native verification.
The original binary renderer and old save rules remain recoverable in Git. No
renderer-only smoothing or invisible cube proxies may substitute for that work.

## Section 0 audit of the full requested result

| Check | Score | Evidence or gap |
|---|---|---|
| 1 Purpose-fit completeness | FAIL | Block world rejected; full game/quality not achieved |
| 2 Prerequisites | PASS | Pinned Blender, actual Unity/MCP and declared URP/runtime verified |
| 3 Owned checks clean | PASS | Release 0/0, 5198 checks, scoped Unity console/shaders 0 |
| 4 Intended behavior | FAIL | Scoped integration passes; replacement world and native art behavior unproven |
| 5 Documentation | PASS | Director override, equal influences, source/consumer limits recorded |
| 6 Authorization/data | PASS | Local branch only; no slot write/publication/security change |
| 7 Performance | FAIL | Current art/representative native workload not measured |
| 8 Stack/platform | PASS | Unity 6000.3.27f2, URP, Both, Core DLL-only, one Simulation |
| 9 Complete package | FAIL | No current-art native package; rejected geometry remains |
| 10 Constraints | PASS | Authorized local tools/IP boundaries respected; no paid service |
| 11 Reproduction | PASS | Original generator/manifests; container variance documented; actual import checks |
| 12 IP hygiene | PASS | Original geometry/shader authoring; engine resources used as dependencies |
| 13 Multi-agent | N/A | No delegation used |
| 14 Review manifest | PASS | Paths, commands, hashes, evidence limits, risks and next actions recorded |
| 15 Self-audit | PASS | Below |

Source/consumer checks establish a reusable art foundation only.
Actual exposure output falsifies the previous saved-profile-only assumption.
Model import signed axes/metres and production adapters were exercised in Unity.
Runtime environment removal restores scene ownership without touching game state.
Native art, full world agreement, animation/performance and director acceptance are missing.
Applicable failures prohibit completion of the requested result.

Frozen Constitution 6.18.0: `63d99cdc72a1c18b5aa91b9336d02fd30126407b`.
Skill evolution: elapsed-time/callback evidence change `922dcbb`, validated and
**staged, not propagated**. This continuation creates no second evolution.
