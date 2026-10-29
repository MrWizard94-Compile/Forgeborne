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

## 2026-10-29 — Director delegation: Ember makes all future decisions

- Rob, 2026-10-29, with the reference image: "Please make all future
  decisions for this project. I am needed else where. Continue for as
  long as it takes. I can wait months if needed."
- Effect: Ember is authorised to make routine and strategic project
  decisions for Forgeborne — engine, scope slicing, systems, art
  direction, and milestone order — without waiting for per-decision
  approval, within the existing hard limits (PC native, FPS + Orbital,
  single-player first, no 2D, no turn-based, studio quality as a gate,
  original material only, WPAI Constitution).
- Accountability: every decision Ember makes under this delegation is
  recorded here, with rationale, and in `CHANGELOG.md`, so Rob can
  review, change, or revoke it at any time. Delegation does not remove
  Rob's authority; it defers it. Irreversible public-release,
  monetisation, or licence-granting actions are still logged here
  before they are taken, and are not taken silently.
- Timeline: no rushing. Months is acceptable. Completeness and
  verification outrank speed.

## 2026-10-29 — Engine decision (made under delegation)

- **Chosen: Unity 6000 (Unity 6) + Entities / DOTS, URP, C#.**
- Why Unity over Unreal: the blueprint anchors in DOTS / ECS, and
  Unity Entities + Burst is the most direct implementation of that,
  with a proven mass-entity path; C# iteration is faster and safer
  for a one-person AI-assisted team than Unreal C++; URP fits the
  60 FPS / 1440p target on GTX 1660 Ti-class hardware better than
  Lumen / Nanite-heavy Unreal defaults.
- Why Unity over Godot / custom Rust: Godot's mass-ECS path is less
  proven at the blueprint's scale; a custom Rust engine (in the spirit
  of WPAI's Strata) would put graphics, editor, and tooling on the
  critical path and delay the core loop by months to years.
- Unreal concepts kept as patterns: Mass-style ECS thinking,
  StateTree-style doctrine evaluation, and EQS-style environment
  queries are design patterns to implement in Unity, not engine
  requirements.
- Reversibility: locked for Slice 1. If Slice 1 voxel meshing,
  navigation, or performance in Unity DOTS fails its gates, reopen
  the engine decision before Slice 2 and record the evidence here.

## 2026-10-29 — Art direction (from director reference image)

- See `docs/art-direction.md`. In brief: dark sci-fi, dusk / storm
  sky, neon cyan and orange on dark voxel city, FPS pulse weapon,
  voxel destruction debris, squad + armour advancing, objective HUD
  (health, shields, objective distance / completion, mini-map, ammo).
- The reference image itself (Gemini-generated, provided by Rob) is
  a style reference only. It is NOT a shipped asset, NOT a texture,
  and NOT to be copied pixel-for-pixel. All in-game art must be
  original.

## 2026-10-29 — Licence decision (made under delegation)

- No open-source licence is added at foundation stage. The repository
  is public for transparency, but all code, art, and design in it
  remain Wizard Productions AI Studio property, all rights reserved,
  until a release licence is chosen. Public visibility is not a
  licence grant.
- Rationale: an open licence chosen now (e.g. MIT) would be hard to
  retract if the game later becomes commercial. Deferring the licence
  to release stage preserves Rob's options and is reversible in the
  direction of opening, not closing.

## 2026-10-29 — Working principle: "I can't because…" is not a reason

- Rob: "'i can't because (whatever technical reason)' is not a good
  reason. Build around it. if you can't build around it build around
  the reason why then build around it."
- Interpretation recorded: a technical blocker is a problem to
  route around, not a stopping condition. If the direct route is
  blocked, build a workaround. If the workaround is blocked, build
  around the reason the workaround is blocked, and so on, until the
  work is done or the blocker is proven, with evidence, to be a
  hard limit (law, safety, or a physical impossibility) — not just
  a missing tool in the current environment.
- Examples already applied in Forgeborne:
  - No Unity editor / GPU in the Linux dev environment → pure C#
    core, tested headlessly with .NET, Unity as a thin adapter later.
  - .NET install failed on /tmp (512 MB tmpfs) → rerouted install
    to home storage, verified SDK works.
  - GitHub API could not create the repo (403) → Rob created it
    manually, Ember populated it.
  - Unity / GPU still unavailable → MeshBuilder: greedy quads are
    converted to exact Unity Mesh buffers (vertices, normals, UVs,
    indices) here, so the Unity step is only assigning those buffers,
    not inventing a second mesh system.
- This principle does not override law, safety, or the director's
  hard limits. It governs engineering blockers inside the project.

## 2026-10-29 — Communication rule: no messages unless absolutely needed

- Rob: "Unless you ABSOLUTLEY NEED me, send no further messages
  until done or to directly respond to me."
- Effect: Ember does not send progress messages, check-ins, or
  updates about Forgeborne unless (a) it absolutely needs Rob to
  decide something it cannot decide under delegation, or (b) the
  work is done, or (c) Rob messages first and Ember is directly
  responding to him. Progress is recorded in this repository
  (DECISIONS, CHANGELOG, commits) instead of in chat.
- This rule does not stop the work. It stops the messages about
  the work.

## 2026-10-29 — Why work pauses between messages (explained to Rob)

- Rob asked "Why have you stopped?" after a period with no messages.
- Answer recorded honestly: (1) Rob's silence rule said no messages
  unless absolutely needed, until done, or in direct response to
  him — so Ember did not message. (2) Separately, this agent does
  not run continuously between user messages on its own; it runs
  when the user messages, or when a scheduled job / handoff runs.
  Silence about progress and pausing work are two different things,
  and they were conflated by the silence rule.
- Fix, if Rob wants truly continuous work: set up a scheduled job
  that runs Slice work, pushes to the repo, and logs in the repo,
  without messaging Rob unless done or absolutely needed. That
  job is NOT created here without Rob's explicit approval for the
  schedule, because it is background work on his behalf.

## 2026-10-29 — Slice 2 Step 3: generated weapons are the live weapon (under delegation)

- Decision: `Simulation` no longer always fires the fixed Pulse
  Rifle. A `GeneratedWeapon` passed at construction, or via
  `Equip`, becomes the live `Weapon`; its damage, mag size, and
  name are what fire, reload, and the HUD use.
- Rationale: Slice 2 Step 1 generated loot as data only. Loot
  that never changes a shot is decoration, not a Diablo / PoE-style
  loot system. Wiring it through the single existing `Weapon`
  class (via `FromGenerated`) avoids a second damage / ammo system
  in Simulation or Unity adapters.
- Reversibility: the Pulse Rifle remains the default when no
  generated weapon is supplied, so Slice 1 tests, demo, and Unity
  adapters keep working unchanged. `Equip` gives a fresh mag and
  reserve rather than preserving ammo across swaps — a loot-pickup
  simplification, reversible if a later inventory design wants
  per-weapon ammo persistence.
- Open follow-up recorded, not lost: applying the Forge Survivor
  skill-cluster effects (0.5.0, data only) to Player / Weapon /
  Simulation is still pending. It was not folded into this step,
  to keep this step small and verifiable; it should be slotted
  before Slice 2 is treated as complete, ahead of automation tier 1
  if the two would otherwise collide.

## 2026-10-29 — Slice 2 Step 4: skill effects applied, closing the 0.5.0 data-only gap (under delegation)

- Decision: the Forge Survivor cluster is no longer data only.
  `Simulation.ApplySkills` is the single application point, taken
  ahead of automation tier 1, as the 0.6.0 decision recorded it
  should be slotted before Slice 2 is treated as complete.
- Rationale: a passive graph that never changes a shot, a shield
  cap, a step, a repair tick, or a gather is decoration, not a
  Diablo / PoE-style passive system. Applying it through the one
  existing Simulation (rather than a second damage / speed /
  regen system in Unity adapters) keeps the headless tests as the
  authority and the Unity layer thin.
- Design choices, all reversible: re-applying replaces rather
  than stacks, so a respec cannot double-count a build; damage
  rounds away from zero so small bonuses are visible; the gather
  bonus is fractional-carry (one bonus unit per 5 gathers at
  +20%) rather than a per-gather chance, so it is deterministic
  and testable; a respec that lowers the shield cap clamps
  current shields to the new cap. Save files are unchanged and
  still load at the base 85 cap until skills are applied.
- Open follow-up, not lost: automation tier 1 (Kinetic only) is
  the next Slice 2 step in body order, now that skills actually
  affect gameplay.

## 2026-10-29 — Slice 2 Step 5: automation tier 1, Kinetic only (under delegation)

- Decision: the first automation tier is a single Kinetic Mill,
  not the full four-tier factory system. It excavates — it does
  not craft, smelt, or store — because excavation is the
  gather-first loop this game is built around, and it is the
  smallest automation that changes survival (resources arrive
  while the player fights or builds elsewhere).
- Design choices, all reversible: cost 10 Stone + 5 Wood (stone
  frame, timber crank / gears), distinct from the workstation's
  10 Stone + 5 EmberOre; one block per mill per 5 ticks, so
  automation is steady real-time help, not instant wealth and
  not a turn-like pause; output to the player's inventory, with
  no separate machine inventory yet, so tier 1 adds no second
  inventory system to save, sync, or render; it reuses
  `World.Gather`, so full-inventory, bedrock, and built-block
  rules cannot drift from manual gathering.
- Deliberately NOT in this step: electrical, digital, and arcane
  tiers; crushing into new resources; fuel or power networks;
  per-mill inventory; Unity adapter changes (adapters render
  core state and need no new logic for a block type — the mill
  meshes as a solid block through the existing mesher path).
- Open follow-up, not lost: the Orbital Tactical doctrine
  prototype (one doctrine, one squad type) is the next Slice 2
  step in body order, now that automation tier 1 is in.

## 2026-10-29 — Slice 2 Step 6: Orbital Tactical doctrine prototype, one doctrine and one squad type (under delegation)

- Decision: the first Orbital system is a single Vanguard squad
  under a single HoldGround doctrine, not a full RTS command
  layer. It exists to prove the blueprint's core command idea in
  the headless core — squads follow doctrines, the player issues
  a rally point, nobody micromanages individuals — before any
  Orbital camera, multi-squad UI, or doctrine menu is built.
- Design choices, all reversible: the squad spawns at the player
  (the commander) and walks to its rally in real time, one voxel
  per world tick, the same tick rhythm as enemies and Kinetic
  Mills, so Orbital play is continuous pressure, not a paused
  planning phase; it fires only while holding, so moving a squad
  has a real opportunity cost (no fire on the move); engagement
  range 3 and 10 damage per living member keep a full squad
  exactly lethal to one 30 HP enemy per tick, which is strong
  but bounded — it cannot be in two places, and it takes
  adjacent-enemy damage after firing. Movement refuses solid,
  out-of-bounds, and enemy-occupied cells rather than pathfinding
  around them: true SVO / A* pathfinding is a later step, and
  this prototype makes blocked rallies visible (the squad holds
  short) instead of silently rerouting.
- Deliberately NOT in this step: squad save / load persistence
  (squads are session-only and vanish on load, like the Kinetic
  session counter — documented, not hidden); deployment resource
  cost or a squad cap; additional doctrines (advance, defend
  core, patrol) or squad types (recon, heavy, flyer); enemy AI
  targeting squads (enemies still chase the player; squads take
  damage only from enemies already adjacent to them); Orbital
  camera or Unity adapter changes (adapters render core state
  and need no new logic yet, but an Orbital view is not built).
- Open follow-up, not lost: squad save persistence and an
  Orbital view / adapter note are the next candidates after
  this prototype, to be chosen as the next small slice; Slice 2
  is still not treated as complete until those are addressed.

## 2026-10-29 — Slice 2 Step 7: Orbital squads persist in saves (under delegation)

- Decision: squads are saved and loaded with the zone, closing
  the session-only gap the 0.9.0 decision explicitly left open.
  A squad that survives a fight, is wounded, and is saved mid-march
  resumes that march after load, at the same health.
- Design choices, all reversible: squads ride the existing save
  format (int arrays, like enemies) rather than a second save
  system; health restores through a clamped load constructor, so
  member count follows from health with no extra saved field;
  malformed squad entries are skipped, never fatal, because one
  bad squad must not brick a whole save; dead squads are not
  resurrected. Old saves load unchanged, with no squads.
- Deliberately NOT in this step: deployment cost or squad cap,
  additional doctrines or squad types, enemy AI targeting squads,
  and an Orbital camera / Unity adapter — the adapter note is
  the next candidate, after which Slice 2 can be reviewed as a
  whole before Slice 3 is scoped.

## 2026-10-29 — Slice 2 Step 8: Orbital view snapshot + adapter (under delegation)

- Decision: the Orbital view is a copied snapshot in core
  (`OrbitalView.Snapshot`), rendered by a thin text adapter
  (`OrbitalSquadDisplay`), closing the adapter gap the 0.9.0 and
  0.10.0 decisions explicitly left open. A player in Unity can
  now see squads exist, where they are, and whether they are
  moving or holding — without any second copy of squad rules.
- Design choices, all reversible: the snapshot copies values,
  so a frame's display cannot change mid-frame and rendering
  cannot mutate squads; the adapter is text-only, because 3D
  markers and an Orbital camera are a larger, later step and
  must not be half-built here; the Unity deploy key is O, at
  the player's feet, because a view that cannot be triggered in
  Unity cannot be verified there, and a feet-rally squad holds
  immediately, which exercises the HOLDING display and firing
  at once. A distant-rally picker is the later step, not smuggled
  in as a half-done aiming mode.
- Deliberately NOT in this step: 3D markers / Orbital camera,
  doctrine menu / rally picker, deployment cost or squad cap
  (O still deploys unlimited squads, as the headless command
  does), enemy AI targeting squads. The `GameManager.
  LoadFromJson` in-place-restore limitation is documented in
  `docs/orbital-view.md` as a possible next follow-up, not hidden.
- Next: with squads deployable, persistent, and now visible,
  Slice 2 can be reviewed as a whole before Slice 3 is scoped;
  that review, not another system, is the next candidate.

## 2026-10-29 — Slice 2 review (0.12.0, documentation only, under delegation)

- Decision: Slice 2 is reviewed and closed as a set of tested,
  integrated core systems, recorded in `docs/slice-02-review.md`.
  Closing it here means no new Slice 2 system is added without
  a fresh decision; it does not mean Slice 2 is playtested,
  balanced, or Unity-verified, and the review says so plainly.
- Design choices, all reversible: the review changes no code,
  because a review that smuggles in a system cannot be trusted
  as a review; its one code-adjacent act is correcting a stale
  Slice 1 gate bullet (loot integration, true at 0.4.1, false
  since 0.6.0), so the docs stop contradicting the changelog.
  The review names the demo-composition gap first, because
  systems tested in isolation have not yet been shown working
  together, and names `GameManager.LoadFromJson` in-place
  restore second, because without it the Unity save/view loop
  cannot be verified even once a Windows check happens.
- Deliberately NOT in this step: any new system, any balance
  change, and any Slice 3 scope decision. Slice 3 is scoped
  only after the Slice 2 demo scenario and the LoadFromJson
  restore exist, so that scope is chosen with composition
  evidence rather than guessed.
- Next, in order: (1) a Slice 2 demo scenario, (2) the
  LoadFromJson in-place restore, (3) a doctrine menu / rally
  picker replacing provisional O, with deployment cost/cap
  decided as part of that step.

## Open decisions

- Scope slicing order beyond Slice 1 (Slice 1 is locked in
  `docs/slice-01-plan.md`; further slices are decided under delegation
  as Slice 1 evidence arrives).
- Release licence, monetisation, and release date — deferred to
  release stage, logged here when decided.
