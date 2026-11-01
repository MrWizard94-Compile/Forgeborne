# Forgeborne production status

The full director brief is mapped in [design-pillars.md](design-pillars.md),
including the explicit Path of Exile 2 reference in the latest objective.

Director: Rob. Integrator: Codex. Current phase: **pre-production / vertical slice**.
PC-native 3D FPS + orbital command, single-player first, original game code/art,
Unity 6000.3.27f2 + URP. One GameManager-owned Simulation. Legacy input requires
Both. Core is distributed only as DLLs in Assets/Plugins. No remote publication,
licence or visibility change is authorized by this tracker.

The director's studio-production reference was read in full on 2026-10-31.
Adopt its discipline ownership, design contracts, dependency gates, short evidence
handoffs and engine/runtime verification. Treat its claimed timelines, guaranteed
hallucination elimination and coordination complexity as unverified assertions.
Do not equate an agent count, a schema or a green test suite with studio quality.

| Phase | Current gate | Owner/evidence |
|---|---|---|
| Concept / feasibility | Direction locked; complete feasibility/performance case still open | Rob directs; README, MANIFEST, DECISIONS and architecture targets |
| Pre-production / vertical slice | ACTIVE; final-quality gather → build → survive encounter not accepted | Codex integrates design, gameplay, art, audio and QA; Rob judges quality/fun |
| Full production | UNSTARTED; requires accepted vertical slice | No asset-volume expansion before the loop works |
| Validation / platform readiness | UNSTARTED as a product gate | PC build, regression, save recovery, hardware/profile matrix; console certification is outside current PC scope |
| Launch | UNSTARTED; director controls distribution/release | No storefront deployment or public marketing action implied |
| Post-launch | UNSTARTED | Maintenance planning follows an accepted release scope |

## Dependency order for the first reviewable vertical slice

1. **Integrated foundation — partial checkpoint.** Saved Main, shared ownership,
   rendered URP, input/HUD and MCP connection observed; see
   [the exact evidence](unity-main-integration.md). This is not a gameplay milestone.
2. **Real-time gameplay contract — verified checkpoint, feel review open.**
   Fixed movement/world clocks passed equal-time core tests and live Main
   observation; see [runtime-pacing-checkpoint.md](runtime-pacing-checkpoint.md).
   The candidate two-tick/second pace is still subject to encounter playtesting.
3. **Readable encounter.** Visible enemies and objective from the same canonical
   world, pointer capture/release, legible gathering/building targets and feedback,
   clear player death/restart, continuous real-time pressure and an attainable win.
   [Construction safety](construction-safety-checkpoint.md) now rejects occupied
   placement before spending, with source and production adapter evidence.
4. **A cohesive original place.** Resource/material distinction, purposeful cover
   and sightlines, industrial dusk palette, weapon/impact feedback, lighting and
   original audio. Check all assets at FPS and orbital distances; do not add
   decorative systems to hide missing interactions.
   Resolved action feedback and eleven original synthesis candidates now have
   [scoped live component evidence](action-feedback-checkpoint.md); heard mix and
   final presentation remain open.
5. **Persistent player flow.** Save/load/export/import through the actual UI,
   safe malformed-save handling, scene restart, retained inventory/builds/enemies,
   correct adapter identity after load and a recoverable failure path.
6. **Orbital integration.** Actual 3D overhead command view plus return to FPS,
   coherent selection/rally/deploy/cancel feedback and existing doctrine rules.
7. **Runtime hardening and review build.** Native Windows build, repeated sessions,
   intended device/input paths, 1440p frame-time and memory measurements on the
   GTX 1660 Ti, then director playtest. A passing technical gate cannot pass fun.

Each checkpoint needs a coherent commit, changed-path manifest, runtime evidence,
known gaps and rollback. Work from the current branch; preserve unrelated local
files and do not reset shared history. The saved integration scene/commit is the
rollback point before changing runtime cadence or encounter behavior.

## Completion boundary

The studio-quality goal stays active until the intended experience is reviewable
and the required technical and director gates are met. Do not relabel this tracker,
the inherited 26 source-only slices or the initial Main scene as a completed game.
