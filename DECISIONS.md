# Forgeborne — Decisions

Dated, director-made decisions. Superseded proposals are kept here so
the history is honest; they are not deleted to make the current plan
look inevitable.

## 2026-10-29 — New foundation, not a reforge

- Do NOT reforge Ember Forge. Classify Ember Forge as a learning
  experience. Build an entirely new foundation with an entirely
  different gameplay concept.
- GitHub may be used to create a new repo. Full use of tech / software
  within legal bounds, bound by the WPAI Constitution. No rushing;
  Rob is willing to wait weeks.

## 2026-10-29 — Feedback that set the bar

- Ember Forge was good but "very basic graphically and gameplay-wise —
  it says 1990s, not almost 2027." Incremental systems do not count as
  improvement if core presentation and feel stay dated.
- Ember Forge "kinda just feels like Astroids" — a genre problem, not
  a polish problem. Polishing a twin-stick arena shooter does not make
  it modern.
- Rob absolutely despises turn-based things; in his view there is no
  skill in it. Tower defense is borderline-acceptable only if done
  properly: real-time, continuous pressure, no long turn-like pauses.

## 2026-10-29 — Game tastes and merged vision

- Rob enjoys survivors, loves modded Minecraft, Diablo, Path of Exile,
  StarCraft, Skyrim, Borderlands, BioShock, and many others.
- Merged vision: modded Minecraft + StarCraft + Diablo / PoE +
  BioShock + Borderlands, as mechanics and feel only, in an original
  game. See `docs/architecture.md`.

## 2026-10-29 — Hard requirements and limits

- Studio quality and production grade are requirements, not
  suggestions, for every milestone.
- 2D is strictly off limits. No 2D game, no 2D top-down survivors, no
  flat 2D arena. 2.5D / flat-sprite workarounds do not satisfy this
  unless Rob explicitly says otherwise.
- Emphasis on resource gathering, building, surviving, in that order.

## 2026-10-29 — Locked platform, perspective, multiplayer

- Platform: PC native. Web-first / TypeScript + Three.js proposal
  SUPERSEDED.
- Perspective: FPS Tactile Mode + Orbital Overhead Tactical Mode.
  Third-person survivors proposal SUPERSEDED.
- Multiplayer: single-player first, networking deferred. Deterministic
  ECS runs locally first.

## 2026-10-29 — Architecture blueprint, Version 3 canonical

- Canonical blueprint: "Technical and Systems Architecture for a
  Studio-Grade FPS-RTS Voxel ARPG Hybrid", Version 3, explicitly as a
  blueprint for Ember (muse.ai) to build. Summarised in
  `docs/architecture.md`.
- A separate Version 2 draft added Microsoft MUSE / WHAM neural world
  modeling, MaaG consistency, and a VM-based agent loop. That layer is
  NOT part of the canonical blueprint. It is research reference only,
  unless Rob re-adopts it. Canonical state remains ECS / SVO; neural
  output, if ever used, is suggestion only and must be validated
  before affecting gameplay, saves, or progression. Microsoft Muse
  is not Ember / Meta Muse.

## 2026-10-29 — Approval and repository creation

- Rob approved Forgeborne, the canonical Version 3 blueprint, and the
  locked decisions above.
- Visibility was not specified in that approval. Ember attempted API
  creation as private; it failed with 403 "Resource not accessible by
  integration". Rob then created the repository manually at
  https://github.com/MrWizard94-Compile/Forgeborne.git
- As created by Rob and verified 2026-10-29, the repository is
  **public** (search result: private = false, default branch main,
  created 2026-10-29T12:14:05Z, id 1438374127). Visibility can be
  changed by the owner on GitHub; it is not changed by agents here
  without director instruction.

## Open decisions

- Exact native engine (Unity DOTS implied by the blueprint, not locked).
- Licence for this repository (not added by agents without direction).
- Scope slicing order beyond Slice 1 (Slice 1 is locked in
  `docs/slice-01-plan.md`).
