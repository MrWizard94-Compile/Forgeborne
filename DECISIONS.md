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

## 2026-10-29 — Slice 2 demo scenario (0.13.0, under delegation)

- Decision: the Slice 2 demo scenario is step (1) from the
  0.12.0 review, not a new Slice 2 system, so it does not reopen
  the closed Slice 2 scope — it evidences it. The demo keeps
  the Slice 1 run byte-identical in behaviour and appends the
  composed run, so a Slice 2 regression cannot hide behind a
  changed Slice 1 baseline, and the composed run's numbers are
  printed as evidence rather than asserted as balance targets.
- Design choices, all reversible: the demo uses the same
  discriminating seed-0 loot and full cluster as the tests, so
  demo and test evidence agree; the squad rallies on the player
  position (hold-and-fire immediately) rather than marching, so
  the 20-tick window shows engagement, not travel; the mill's
  single excavated block is reported as-is (1, not a target),
  because a demo that inflates its numbers would be worse than
  no demo. No balance change is made on the strength of this
  run — the numbers are first-pass values, still untuned.
- Deliberately NOT in this step: any new system, any balance
  change, any Slice 3 scope, and the LoadFromJson restore,
  which remains next. The demo's save/load round-trip uses the
  headless `SaveSystem.LoadZone`, not Unity's `GameManager`,
  so it does not claim the Unity restore gap is closed.
- Next, in order: (2) the `GameManager.LoadFromJson` in-place
  restore, then (3) a doctrine menu / rally picker replacing
  provisional O.

## 2026-10-29 — GameManager.LoadFromJson in-place restore (0.14.0, under delegation)

- Decision: the restore is a core API, not adapter logic. The
  blocker was structural — `Simulation.World` get-only, FPS
  position private-set — so the fix adds `FpsController.Teleport`,
  `Simulation.RestoreWorld`, and `Simulation.LoadFromJson` to core
  and keeps `GameManager` to one call, one mesh refresh, and one
  log line. An adapter that rebuilt the Simulation would have
  broken every holder of the old object; in-place restore is the
  only shape that keeps GameManager + MeshRenderer wiring intact.
- Design choices, all reversible: the Fps object is never
  replaced, only teleported, because identity is the point of
  "in place"; session counters reset on load, because they
  describe the pre-load session, while the equipped weapon is
  kept, because the save format carries no weapon and dropping
  the player's loadout on load would be a silent punishment;
  an applied skill build is re-applied to the new world, because
  the alternative — cap/speed/regen silently reverting to base
  while the build still shows as applied — is incoherent, and
  the 0.7.0 note about loaded worlds starting at base values
  described LoadZone alone, not a Simulation that already holds
  a build. `LoadFromJson` fails closed on malformed input rather
  than throwing into a UI button handler, since a corrupt save
  must never produce a half-loaded world.
- Deliberately NOT in this step: changing the save format to
  carry weapon or core health (the fresh-100-HP-core-on-load
  limit is documented in the changelog as a follow-up candidate,
  not fixed by stealth here), any Unity scene work, and Slice 3
  scope. The Windows Unity verification of the restore itself
  remains open — headless tests prove the core path, not the
  Unity compile or the button wiring.
- Next, in order: (3) a doctrine menu / rally picker replacing
  provisional O, with deployment cost/cap decided as part of
  that step.

## 2026-10-29 — Doctrine menu / rally picker (0.15.0, under delegation)

- Decision: the picker is a core state machine, not Unity UI
  logic. The provisional O key was an adapter shortcut that
  bypassed any choice; replacing it in the adapter alone would
  have left the choice untestable headlessly, so `OrbitalCommand`
  holds selection + pending rally in core and the adapter only
  binds keys and a crosshair raycast to it. That keeps the
  "Unity scripts are thin adapters, no duplicate logic" rule
  intact and makes every picker rule — fail-closed confirm,
  stale-rally clearing, bounds-only rally check matching
  DeploySquad's solid-rally acceptance — provable in tests.
- Design choices, all reversible: BeginPick clears a stale
  rally, because the failure mode of keeping it (Enter deploying
  a point the player picked minutes ago, in a different fight)
  is worse than re-aiming; a failed Confirm keeps the picker
  open, because losing the menu on a failure would punish the
  player for the system's rejection; while picking, the mouse
  does not fire, because Enter is the confirm and a click
  during aiming must not also be a shot; the raycast uses the
  hit cell itself (even if solid), consistent with DeploySquad,
  so the squad holds short visibly instead of the picker
  silently refusing a face the player clearly aimed at.
- Cost/cap decided here, as the 0.12.0 review required: none
  yet, deferred to a Slice 3 economy step. Inventory holds
  blocks, not a currency, so any price now would be invented
  balance, and a cap without an economy would be an arbitrary
  limit. Recording "none, deferred with reason" closes the
  decision for this step without pretending a system exists.
- Deliberately NOT in this step: extra doctrines/kinds (the
  selection API is ready for them, the values are not added),
  3D squad or rally markers, pitch aiming for the raycast
  (camera yaws only in Slice 1, so the ray is horizontal from
  eye height — documented in docs/orbital-command.md, not
  hidden), and Slice 3 scope. The Windows Unity verification
  of the picker, display prompt, and MeshCollider remains open
  — headless tests prove the core path, not the Unity compile
  or the raycast wiring.
- Next, in order: scope Slice 3 as its own decision step, with
  the Slice 2 review's deliberately-not-done list and the
  picker/cost decision as its inputs — not as a system smuggled
  into this picker step.

## 2026-10-29 — Slice 3 scope (0.16.0, under delegation)

- Decision: Slice 3 is economy and persistence, in that
  theme order's reverse — persistence first, then economy.
  Slice 2 left two debts that converge on one slice: things
  a load silently resets (fresh 100-HP core, base skill
  values on a fresh Simulation) and the one unlimited action
  (squad deployment, cost/cap deferred from 0.15.0 for want
  of an economy). Widening the game with a second doctrine,
  a second automation tier, or a bigger skill graph before
  paying those debts would price and persist fiction, so
  Slice 3 is deliberately narrow: two systems and a review.
- Design choices, all reversible: persistence precedes
  economy, because an economy priced against a build that
  vanishes on load is not an economy; the equipped weapon
  is left as a candidate for Step 1 to decide, because the
  0.14.0 rationale (kept on in-place restore, no format
  field) is defensible either way and the scope step should
  not pre-decide a save-format question it has not examined;
  resource pricing is named as the economy step's starting
  hypothesis, not its decision, because the step — not the
  scope — must test whether squads competing with walls and
  mills for Stone distorts gathering; the Slice 4 candidate
  ranking pairs enemy AI targeting squads with a second
  doctrine, because a doctrine that moves squads into danger
  is only fair once enemies can answer them.
- Deliberately NOT in this step: any Slice 3 system (this
  document scopes, it does not build), any price or cap value
  (those are Step 2's decision with evidence), any Slice 4
  promise (the ranking is a starting list for the Slice 3
  review, which may reorder it), and any claim about the
  pending Slice 1 director playtest gate, which Slice 3 does
  not close.
- Next, in order: Slice 3 Step 1, save-format completion
  (core health + skill build, weapon decided there).

## 2026-10-29 — Slice 3 Step 1: save-format completion (0.17.0, under delegation)

- Decision, closing the question the 0.16.0 scope left open:
  the equipped weapon JOINS the save format, with core health
  and the applied skill build. The 0.14.0 rationale for leaving
  it out (kept on in-place restore, dropped on a fresh load)
  was defensible for a restore step, but it made a fresh load
  silently downgrade the player — loot weapon lost, build
  lost, core healed — which is exactly the debt Slice 3 exists
  to pay. Persisting all three together is the coherent unit:
  a load now restores the game that was saved, not a
  base-values approximation of it. The cost is a format change,
  contained by the rules below, not a new system.
- Design choices, all reversible: new fields are nullable and
  declared last, so old saves (fields absent) still load —
  core at 100, no build, no weapon, and the loading
  Simulation keeps its current build and weapon, preserving
  the 0.14.0 in-place semantics; a present core value of 0
  means destroyed, not missing, which is why the field is
  nullable instead of a plain int defaulting to 0; null fields
  are omitted when writing, so a world-only save grows only
  by the CoreHealth field; the skill build is decoded before
  the player is restored, so the raised shield cap is in place
  before shields are restored — otherwise a skilled save with
  100 shields would clamp to the base 85 cap and the build
  would restore the cap without the shields; malformed skill
  strings and partial weapons are skipped, never fatal,
  matching the 0.10.0 squad rule; the weapon is saved as live
  state (name, damage, mag capacity, current mag, reserve),
  not as a generator seed, because a default Pulse Rifle has
  no generated equivalent and a half-empty mag must not
  refill for free; an explicitly saved empty build is a respec
  to base values, distinct from no build at all.
- Deliberately NOT in this step: any squad cost or cap (that
  is Step 2's decision with its own evidence), any new
  doctrine, kind, automation tier, or skill node, any balance
  change, and any claim about Windows Unity verification —
  the GameManager save path now uses the full format, but
  its Unity compile is still unverified headlessly, as before.
- Next, in order: Slice 3 Step 2, squad economy (cost and
  cap, pricing basis decided there).

## 2026-10-29 — Slice 3 Step 2: squad economy (0.18.0, under delegation)

- Decision, closing the Slice 3 scope's Step 2 question: the
  pricing basis is gathered resources, confirming the scope's
  starting hypothesis, and NO new currency is introduced —
  a currency would need sources and sinks of its own, which
  is a new system, not an economy step. The price is 10
  EmberOre, EmberOre only, for one Vanguard HoldGround squad.
  Why not Stone, the obvious alternative: Stone is the
  gather -> build loop's core material (walls 2, mills and
  workstations 10 each), so pricing squads in Stone would
  make every squad cost a mill frame or five walls and
  distort the pillar the game is built around. EmberOre is
  already the game's valuable resource (~4% of ground
  cells, so a chunk holds on the order of 120 ore and 10
  ore is deliberate gathering, not incidental pickup), and
  its only other use is the workstation's 5-ore ingredient —
  so the squad competes with shield-regen infrastructure, a
  real but bounded tradeoff, not with basic building.
  Why 10, twice a workstation: a squad is three armed
  members, so it should cost more than one machine, and 10
  is large enough to be felt and small enough to be earned
  in one gathering run. Labelled first-pass, not balance:
  the scope forbids presenting Step 2 prices as final.
- Decision: the cap limits concurrent LIVE squads, to 3, and
  at the cap a deploy is REFUSED. Alternatives rejected:
  oldest-squad recall would silently destroy a paid
  investment and its rally position, and a total-deployed
  cap would permanently punish early experimentation.
  Dead squads do not count, so a replacement can deploy
  after a death without waiting for Advance to clean up.
  No refund exists, on death or otherwise, because there is
  no recall mechanic to refund through; deployment is a
  sunk cost, which is also what makes the cap a real limit
  rather than a revolving door.
- Layering decision, and its honest limit: enforcement lives
  in one place, `WorldZone.TryDeployPaidSquad`, which the
  picker Confirm and the new Simulation paid path both use,
  checking cap and affordability BEFORE charging, with a
  refund guard on the impossible post-charge failure — so
  a refused deploy can never half-charge or half-deploy.
  The free `DeploySquad` / `DeployOrbitalSquad` path is
  KEPT, deliberately, for headless tests and the demo: all
  pre-0.18.0 deploy tests pass unchanged on it, and
  retrofitting ~30 of them to fund every deploy would have
  been noise that obscured this step's own tests. The
  limit is stated plainly in code, tests, and
  docs/orbital-command.md: the free path is headless-only
  and Unity play must never call it, because play goes
  through the picker. If a future step finds a play path
  reaching the free primitive, that is a bug in that path,
  not a reason to re-litigate the price here. The three
  0.15.0 picker tests whose confirms were supposed to
  succeed were updated to fund them — that is the intended
  behaviour change, and the unfunded refusal is itself
  tested in the new section.
- Deliberately NOT in this step: any second doctrine, kind,
  or priced unit (HoldGround / Vanguard is the priced unit);
  any refund, recall, or cap-raising mechanic; any balance
  change to existing build costs; any claim about Windows
  Unity verification — the display prompt now shows the
  cost and cap, but its Unity compile is still unverified
  headlessly, as before.
- Next, in order: Slice 3 Step 3, demo scenario + review.

## 2026-10-29 — Slice 3 Step 3: demo scenario + review (0.19.0, under delegation)

- Decision: Step 3 is composition + review only, no new
  system, following the 0.12.0 / 0.13.0 pattern and the
  0.16.0 scope's own wording ("demo scenario and a
  documentation-only review that closes the slice"). A
  review that smuggled in a system would not be a review,
  and a demo that only re-ran Steps 1 and 2 separately
  would not prove they compose — so the demo is one
  Simulation running both together: skilled build + loot
  weapon + damaged core (Step 1) and paid, capped squads
  with a picker refusal at the cap (Step 2), through one
  save/load round-trip. The loaded game's cap check is
  asserted too, not just the loaded squads, because a
  cap that reset on load would silently un-price the
  economy the step before just built.
- Decision, on the review's candidate order: the ranked
  Slice 4 candidates are carried from the 0.16.0 scope in
  the same order (enemy AI targeting squads + second
  doctrine first, then automation tier 2, then pitch +
  markers, then a second squad type). The Step 3 evidence
  gives no reason to reorder them: nothing in composition
  testing argues that markers outrank enemy answers to
  squads, or that a second squad type should be priced
  before enemies can answer the first. Reordering, if a
  later run finds a reason, belongs to the Slice 4 scoping
  step, not to this review.
- Deliberately NOT in this step: any Slice 4 system, any
  balance change to the 10-ore price or the cap of 3, any
  enforcement of the free-path rule beyond its existing
  documentation (the review re-names that gap in priority
  order instead of quietly fixing it here), and any claim
  about Windows Unity verification or the pending Slice 1
  director playtest gate — both remain open, and Slice 3
  does not close either.
- Next, in order: scoping Slice 4 as its own decision step.

## 2026-10-29 — Slice 4 scope (0.20.0, under delegation)

- Decision: Slice 4 is scoped as "squads under fire", taking
  the top-ranked pair from the 0.16.0 scope and the 0.19.0
  review in the same order, with no reordering: enemy AI
  targeting squads first, then a second doctrine. The order
  is the decision, not just the list — the 0.16.0 pairing
  rationale (a doctrine that moves squads into danger is
  only fair once enemies can answer them) only holds if
  targeting is built and tested before the doctrine is
  judged against it. Scoping the doctrine first would have
  made Step 1 a retrofit to a doctrine already chosen.
- Decision, on what this scope deliberately leaves open:
  the targeting rule and the doctrine choice are hypotheses
  here, not decisions. Nearest target (deterministic
  tie-break) is the targeting hypothesis because it needs no
  new value and matches the squad's own nearest-enemy rule;
  Advance is the doctrine hypothesis because it is the
  moving-into-danger doctrine the pairing rationale was
  about. Either may be overturned in its step with evidence
  — nearest targeting that makes enemies ignore the player,
  or an Advance that collapses into HoldGround at a moving
  rally, would each be a reason to choose otherwise, and
  the step must record that reasoning rather than defending
  the hypothesis.
- Decision, on the reconciliation Step 1 must make: the
  scope names it instead of solving it, because the answer
  depends on how targeting is implemented. Enemies already
  hurt adjacent squads through the squad phase (10 per
  adjacent enemy, after the squad fires), while never
  seeking squads out — so a targeting step that simply adds
  an enemy attack on squads risks counting the same hit
  twice. The step must record how it avoids that, and
  whether the enemies-then-squads tick order stands.
- Decision, on pricing: Slice 4 reopens no economy question.
  The second doctrine deploys through the Slice 3 paid path
  (10 EmberOre, cap 3) as-is; a doctrine-specific price
  would be a new economy decision, and this scope refuses
  to smuggle one into a doctrine step. Likewise the scope
  refuses to smuggle in tier 2, pitch/markers, or a second
  squad type — those are ranked Slice 5 candidates, in the
  0.16.0 order minus the pair Slice 4 consumes.
- Deliberately NOT in this step: any Slice 4 system, any
  balance change, any enforcement of the free-path rule
  beyond its existing documentation, and any claim about
  Windows Unity verification or the pending Slice 1 director
  playtest gate — both remain open, and Slice 4 scoping does
  not close either.
- Next, in order: Slice 4 Step 1, enemy AI targeting squads.

## 2026-10-29 — Slice 4 Step 1: enemy AI targeting squads (under delegation)

- Decision, closing the 0.20.0 scope's Step 1 questions:
  enemies target the NEAREST live target by Manhattan
  distance, re-derived every tick, with the player winning
  ties and the first squad in deploy order winning
  squad-squad ties. This confirms the scope's starting
  hypothesis rather than overturning it, for a reason the
  hypothesis alone did not state: nearest is the only rule
  in the set that needs no new value, matches the squad's
  own nearest-enemy rule, and still leaves the player a
  real target — because "nearest" is per-tick, not a
  permanent allegiance. The scope's warning (enemies
  ignoring the player whenever a squad is anywhere
  closer) is answered, not evaded: yes, a strictly nearer
  squad draws the enemy, and that is the intended
  interception behaviour — a squad placed between the
  player and a wave, or marching toward a wave, SHOULD
  draw pressure. Wholesale abandonment would be an enemy
  that never returns to the player; this enemy returns
  the tick the squad dies, moves away, or stops being
  nearest, and it chooses the player on every tie.
- Decision, on moving vs holding squads: treated ALIKE.
  A moving squad that could not be targeted would make
  the Step 2 moving doctrine untargetable exactly while
  it moves into danger, which would defeat the Slice 4
  pairing rationale. Holding squads are not safer by
  rule; they are safer only in the sense that they do not
  walk toward enemies.
- Decision, on movement generalisation: one rule, in
  `Enemy.TickTowardPosition`, with the old player-only
  `TickToward` delegating to it. A second squad-target
  movement method would have been a drift risk (two
  tie-breaks, two block rules). The occupied-cell refusal
  (player + live squads) is new but narrow: it prevents
  an enemy pursuing one target from overlapping another
  target it passes, which the old player-only movement
  could already do to a squad standing in its path.
- Decision, on the damage reconciliation: NO second hit
  is added, so there is nothing to de-duplicate. Squad
  damage keeps its single existing path (squad phase,
  after squad fire, per surviving adjacent enemy), and
  the enemy phase damages only the player, by proximity,
  as before. Alternatives considered and rejected: an
  enemy-phase squad attack plus a "skip already-hit
  enemies" set in the squad phase would have been a
  second damage system with per-tick bookkeeping to keep
  it honest, for no behaviour the single path does not
  already produce — a pursued enemy that stands adjacent
  to a squad IS hit by that squad's phase damage, and a
  squad that kills its attacker IS unhit, exactly as
  before. The one asymmetry this accepts is stated
  plainly: a moving squad can step away from an adjacent
  enemy before the squad phase, so that enemy deals
  nothing that tick — real-time kiting by marching to a
  rally, not a bug, and holding squads (the only doctrine
  so far) cannot do it.
- Decision, on saves: nothing new is persisted. Targets
  are not state; they are a function of positions and
  health, both already saved, so a loaded world targets
  correctly on its first tick. Adding a target field
  would have risked loading a stale target for a squad
  that died or moved, for no benefit.
- Test evidence, and the one changed test: the Slice 2
  range test was updated for the intended behaviour
  change — under nearest targeting its enemy walks into
  squad range and dies, which is itself targeting
  evidence, so the test now blocks that step with a solid
  block to keep isolating the range rule, and the new
  pursuit tests cover walking into range. No other
  pre-existing test changed, and no test was weakened to
  pass: the killing-case test (full squad, adjacent
  enemy, player far) is the sharp proof that the enemy
  phase adds no squad damage, because an enemy-phase hit
  would have left the squad at 80 despite killing its
  attacker.
- Deliberately NOT in this step: any second doctrine,
  kind, or enemy type (Step 2 chooses the doctrine);
  any price or cap change (Slice 3 economy stands);
  any balance change to damage, range, or speed; any
  claim about Windows Unity verification or the pending
  Slice 1 director playtest gate — both remain open.
- Next, in order: Slice 4 Step 2, exactly one second
  doctrine, chosen in that step from Advance / Defend
  Core / Patrol, deployed through the Slice 3 paid path.

## 2026-10-29 — Slice 4 Step 2: second doctrine, Advance (under delegation)

- Decision, closing the 0.20.0 scope's Step 2 question:
  the second doctrine is ADVANCE. This confirms the scope's
  starting hypothesis, for a reason the hypothesis alone did
  not fully state: Advance is the only candidate whose value
  depends on Step 1 having happened first. Before targeting,
  a hunting squad would have advanced for free — enemies
  would never have answered it. After Step 1, an advancing
  squad walks toward enemies that now pursue squads, so it
  is contested the whole way in, and it can also draw
  pressure off the player by being the nearer target. That
  is the Slice 4 pairing rationale paying off in the order
  it was scoped in, not a doctrine chosen in isolation.
- Decision, on why not Defend Core first: Defend Core would
  have been HoldGround with a preset rally — march to the
  fixed core, hold, fire in range. It reuses the existing
  holding behaviour almost exactly, so it would have proved
  very little about squads under fire beyond a different
  destination, and the core objective is not yet under a
  distinct attack pressure that makes core defence the
  urgent doctrine. It is deferred, not rejected: once Advance
  shows how contested movement behaves, Defend Core can be
  judged as a real contrast (a squad that must stay put
  under fire) rather than as HoldGround renamed.
- Decision, on why not Patrol first: Patrol needs waypoint
  state — a route, a current leg, a loop rule — and that
  state would need a save-format decision (persist the leg
  or restart the route on load) for a doctrine whose combat
  value is unproven. Advance needs no new state at all:
  its target is re-derived every tick from enemy positions
  already in the world, so its save behaviour falls out of
  the existing format. Patrol is deferred, not rejected; it
  is the natural third doctrine once a moving doctrine and
  a fixed-point doctrine both exist to contrast it with.
- Decision, on the collapse risk the scope named: Advance
  does NOT collapse into "HoldGround at a moving rally",
  because it has no rally semantics after deployment at
  all. A moving-rally doctrine would still be a waypoint
  doctrine — go here, then hold and fire — with the waypoint
  merely updated to the enemy's last position. Advance is
  instead a target doctrine: every tick it re-derives the
  nearest living enemy, moves one voxel toward it, and
  fires in range regardless of any rally. The sharp test of
  the difference is the empty battlefield: a moving-rally
  squad would march to its last waypoint, while an Advance
  squad stands still, and a second sharp test is the kill:
  after its prey dies, Advance does not resume marching to
  the deployment rally. Both are tested, and both pass.
- Decision, on firing while moving: HoldGround's silence
  while moving exists because relocation is its order —
  firing mid-march would let it fight its way to a rally it
  was ordered to reach intact. Advance has no relocation
  order, so silencing it until it reaches a rally it ignores
  would be nonsense: an advancing squad in range is already
  doing its job. It therefore fires after moving, in the
  same tick it enters range, and fires while adjacent even
  though its movement step that tick was refused (onto an
  enemy-occupied cell). Range, damage, and speed are
  unchanged — Advance is a behaviour difference, not a
  stat boost: same 3 range, same 10 per living member, same
  1 voxel per tick.
- Decision, on movement sharing: one step rule, in
  TacticalSquad.MoveTowardPosition, with MoveTowardRally and
  MoveTowardNearestEnemy as target-choosers only. A second
  axis/tie-break/block rule for Advance would have been the
  same drift risk Step 1 removed from enemy movement, and
  the tests for blocked Advance movement reuse the same
  expectations as HoldGround (solid block holds the squad,
  fire still happens in range). Enemy-occupied refusal is
  what makes an advancing squad stop adjacent to its prey
  instead of overlapping it, and adjacent-enemy damage then
  applies through the single existing squad-phase path —
  no second damage system, no reconciliation problem to
  solve twice.
- Decision, on the picker and the rally it still requires:
  the picker is a deploy mechanism, not a doctrine: it needs
  a point to spawn a squad at, and its Confirm path is
  shared by both doctrines. For Advance that point is a
  deployment formality, recorded in the save (rally fields
  were already persisted) but ignored by the doctrine
  afterwards. This is stated in the code, the docs, and the
  tests rather than hidden, because a player picking a
  rally for an Advance squad and expecting it to go there
  would otherwise be misled. If a future doctrine needs
  the rally to mean something different again, that meaning
  is a doctrine decision, not a picker change.
- Decision, on pricing: none invented. Advance deploys
  through TryDeployPaidSquad at the settled 10 EmberOre /
  cap 3, tested through the picker, because the scope
  refused to smuggle an economy decision into a doctrine
  step. A doctrine-specific price would need its own
  justification (is Advance stronger than HoldGround in a
  way a price should reflect?) and that justification does
  not exist yet — Advance trades a holding position for
  exposure to pursuing enemies, so it is not obviously
  stronger, only differently risky. Balance remains a
  later pass, not a claim in this step.
- Decision, on saves: nothing new is persisted, for the
  same reason as Step 1 — doctrine and rally were already
  saved fields, and Advance's target is derived, not stored.
  A loaded Advance squad hunts from its saved position on
  its first tick, tested. The one known presentation gap is
  also recorded, not fixed here: OrbitalSquadDisplay words a
  squad MOVING / HOLDING by rally position, which for
  Advance is not a hunting state (an Advance squad standing
  adjacent to an enemy and firing still reads MOVING unless
  it happens to stand on its rally). The doctrine name in
  the same line is the reliable marker; a hunting-state
  display word belongs with the Slice 5 presentation work
  (pitch / markers), not smuggled into a combat step.
- Test evidence: no pre-existing test changed and none
  weakened — the 33 new tests are all additive, and the
  full suite passes on the first run of the new code. The
  stale adapter comment claiming no cost or cap existed
  (false since 0.18.0) was corrected in the same step, as
  a documentation fix, not a behaviour change.
- Deliberately NOT in this step: Defend Core and Patrol
  (deferred, not rejected); any second squad type; any
  price, cap, damage, range, or speed change; any
  waypoint / route state; any claim about Windows Unity
  verification (adapter key 2 is written, not compiled or
  run in Unity — checklist item 8 in docs/orbital-command.md)
  or about the pending Slice 1 director playtest gate —
  both remain open.
- Next, in order: Slice 4 Step 3, demo scenario +
  documentation-only Slice 4 review.

## 2026-10-29 — Slice 4 Step 3: demo scenario + review (0.23.0, under delegation)

- Decision: Step 3 is composition + review only, no new
  system, following the 0.13.0 / 0.19.0 pattern and the
  0.20.0 scope's own wording ("an enemy pursuing and
  engaging a squad, and a squad under the new doctrine, in
  the same Simulation — not two separate proofs"). A demo
  that re-ran Step 1 and Step 2 separately would not prove
  they compose — in particular, it would not prove that an
  enemy pursuing a HoldGround squad and an Advance squad
  hunting the same enemy resolve through the single
  adjacent-damage path without double-counting, or that a
  loaded Advance squad keeps hunting instead of falling
  back to its stored rally. So the demo is one Simulation
  running both together: two paid picker squads (HoldGround
  rally 20, Advance rally 24), both weakened to 1 member so
  their 10-damage fire does not kill outright and hide the
  engagement, player moved far away so every enemy step is
  a squad pursuit, one enemy east and one west, 4 ticks,
  save/load, then 2 more ticks in the loaded game.
- Decision, on what the composition run is allowed to
  claim: the tick-6 retarget — enemy B steps to the nearer
  Advance squad instead of the HoldGround squad it was
  pursuing — is reported as nearest-rule behaviour in
  composition, not as a new rule. It re-proves the Step 1
  rule (nearest live target, re-derived every tick) in a
  world where the nearest target changes because a squad
  diverged, which the Step 1 isolation tests could not
  show: there, squads marched in lockstep or alone. The
  player-as-target claim, by contrast, is NOT re-proved by
  this demo (the player is far away throughout), and the
  review says so plainly: that claim still rests on the
  Step 1 tie and retarget tests, not on this scenario.
- Decision, on the review's candidate order: the ranked
  Slice 5 candidates are carried from the 0.20.0 scope in
  the same order (automation tier 2 first, then pitch +
  markers, then a second squad type). The Step 3 evidence
  gives no reason to reorder them, with one addition that
  is a gap, not a reordering: the Advance display gap
  (rally-relative MOVING / HOLDING does not describe
  hunting) is folded into the pitch + markers candidate,
  because it is a presentation question for that step to
  fix, not a reason to promote presentation above
  automation tier 2. Reordering, if a later run finds a
  reason, belongs to the Slice 5 scoping step, not to this
  review.
- Deliberately NOT in this step: any Slice 5 system, any
  balance change to targeting, damage, price, or cap, any
  fix to the Advance display gap (named in the review's
  gaps instead of quietly fixed here), any enforcement of
  the free-path rule beyond its existing documentation,
  and any claim about Windows Unity verification or the
  pending Slice 1 director playtest gate — both remain
  open, and Slice 4 does not close either.
- Next, in order: scoping Slice 5 as its own decision step.

## 2026-10-29 — Slice 5 scope (0.24.0, under delegation)

- Decision: Slice 5 is scoped as "powered automation",
  taking the top-ranked candidate from the 0.20.0 scope and
  the 0.23.0 review in the same order, with no reordering:
  automation tier 2 first, then pitch + markers, then a
  second squad type. The order is the decision, not just
  the list — automation is the building pillar of the
  gather -> build -> survive loop, and that pillar stalls
  at the kinetic crank, while pitch + markers improve the
  presentation of a loop that already runs and a second
  squad type would widen combat that Slice 4 just closed.
  Widening either before the building pillar moves would
  repeat the 0.15.0 mistake of widening before the existing
  system is fairly complete.
- Decision, on the gaps this scope does NOT claim to close:
  Unity verification and the director playtest are higher
  in the Slice 4 review's priority order, and this scope
  does not reorder them away — it records that neither is
  closable by a headless core step (one is blocked on Unity
  6000 on Windows, the other on the director), so Slice 5
  starts at the first actionable core gap. A future scope
  that treated a headless document as closing either gap
  would be mislabelling, not progress.
- Decision, on what this scope deliberately leaves open:
  the source, distribution rule, powered behaviour, voltage
  handling, inventory rule, build cost, and save handling
  are hypotheses here, not decisions. The hypotheses are:
  a powered variant of an existing machine behaviour (so
  the loop is proved against a known baseline); a single
  voltage with no mismatch destruction yet (so the first
  loop proves power on / off before failure modes punish
  wiring); the tier 1 activation shape and block-gathering
  path reused, not duplicated; the tier 1 player-inventory
  rule kept unless the step proves a buffer is needed; a
  build cost of the same order as the mill's, with the
  Stone / Wood versus EmberOre competition stated either
  way; and powered state derived at load, not persisted,
  unless a buffer decision creates contents that must
  survive. Each may be overturned in Step 1 with evidence,
  and the step must record that reasoning rather than
  defending the hypothesis.
- Decision, on the inventory question: it is decided in
  Step 1, not carried again. Tier 1 deferred it, Slice 3
  carried it, and Slice 4 carried it — a fourth carry
  would be scope debt pretending to be a plan. If Step 1
  chooses a buffer, it must also decide size, pickup, and
  overflow (blocked, never silently lost), and its save
  consequences; if it keeps the player-inventory rule, it
  must say why a buffer is not needed for this one powered
  behaviour. Either answer closes the question for this
  loop; it does not retroactively rewrite kinetic mills
  unless the step justifies that too.
- Decision, on Slice 6 ranking: pitch + markers (including
  the Advance display gap) first, then a second squad type.
  The rest of tier 2 — voltage ladder, transformers,
  mismatch destruction, additional consumers and sources —
  is deferred, not rejected, but is not ranked above those
  two by this scope, mirroring how Defend Core and Patrol
  were deferred after Slice 4 without outranking the next
  candidates. The Slice 5 review may reorder on Step 1
  evidence; this scope does not pre-order the remainder
  first.
- Deliberately NOT in this step: any Slice 5 system, any
  balance change, any enforcement of the free-path rule
  beyond its existing documentation, and any claim about
  Windows Unity verification or the pending Slice 1 director
  playtest gate — both remain open, and Slice 5 scoping
  does not close either.
- Next, in order: Slice 5 Step 1, automation tier 2, first
  small electrical loop only.

## 2026-10-29 — Slice 5 Step 1 (0.25.0, under delegation)

- Decision, on the power source: one Dynamo block, not a
  blueprint alternator and not a cable network. The scope's
  blueprint vocabulary ("alternators, insulated cables,
  transformers") was names, not a design, and the smallest
  source that proves a loop is a block that generates power
  where it stands. Naming it Dynamo (a generic word, not a
  borrowed trademark or lore name) keeps the first loop
  about placement, not about a wiring system that does not
  exist yet. A second source, a cable block, and any network
  walk are deferred — if the next electrical step needs
  range, it must prove adjacency is insufficient, not just
  add cables because the blueprint names them.
- Decision, on distribution: face-adjacency only, derived
  every tick, never persisted. A mill next to a mill next
  to a Dynamo is deliberately NOT powered: chaining would
  smuggle a network rule (propagation order, cycles, range)
  into a step whose job is to prove power changes one
  machine. Diagonals are excluded for the same reason — the
  game's movement, squad, and targeting rules are all
  Manhattan / face-based, and a diagonal power rule would
  be the one rule in the game that is not. Derivation, not
  a flag, is what makes power loss and save/load honest:
  the tests remove a Dynamo and watch the mill fall back,
  and load a Dynamo and watch the mill power up, with no
  flag to go stale between the two.
- Decision, on the powered behaviour: cadence, not a new
  excavation. A powered Kinetic Mill runs every tick instead
  of every 5th, through the same Gather path, into the same
  inventory. This follows the scope's hypothesis, and the
  evidence for keeping it is the baseline: kinetic mills
  already prove excavation works, so the only new claim is
  the rate change, and the tests can isolate it in identical
  pockets (5 in 5 ticks powered, 1 in 5 unpowered). A new
  consumer would have needed its own baseline proved at the
  same time, and a powered workstation (faster regen) would
  have proved power changes a defensive effect, not the
  building pillar Slice 5 exists to move. Power replaces the
  kinetic cadence rather than stacking with it, because a
  mill that fired twice on a 5th tick would make the powered
  rate 6-in-5, not 5-in-5, and would hide whether the
  powered phase or the kinetic phase did the work — the
  separate PoweredBlocksGathered counter exists so the two
  phases can never be confused in a test.
- Decision, on voltage: one voltage, always on, no fuel, no
  mismatch, no destruction — the scope's hypothesis kept,
  not because voltage is uninteresting, but because a first
  loop that destroys machines for wrong wiring would prove
  punishment before it proves power. The failure rule belongs
  to a later tier 2 step, once a second voltage exists to
  mismatch against; with one voltage there is nothing to
  mismatch, and inventing a fake second voltage just to
  destroy machines would be theatre, not a system.
- Decision, on the tier 1 inventory question — CLOSED, as
  the scope required: keep the player-inventory rule, no
  machine buffer. The buffer case the scope named (a powered
  machine must keep working while output has nowhere to go,
  or output is not a block the inventory already holds)
  does not arise here: a powered mill produces exactly the
  blocks a kinetic mill produces, the inventory already
  holds them, and when it is full the mill stops consuming,
  exactly as a kinetic mill does. A buffer would therefore
  add size, pickup, and overflow rules — and a save format
  for its contents — to solve a problem this behaviour
  does not have. This closes the question for this loop
  only; a future powered behaviour whose output is not an
  inventory block must re-argue it, not inherit this answer
  silently. Kinetic mill output is unchanged by this decision.
- Decision, on cost: 5 EmberOre only. Stone / Wood pricing
  was rejected because it would compete with the mills and
  walls whose materials define the basic build loop, while
  EmberOre pricing competes with workstations (5 ore) and
  squads (10 ore) — a real tradeoff, but a bounded one: one
  Dynamo costs one workstation's ore, or half a squad, so
  the player feels power as an investment without it
  starving basic building. A mixed Stone + EmberOre cost was
  rejected as complexity without a reason: the Dynamo is
  not a frame machine, and a single-resource price is the
  same shape as the squad price, which the game already
  proves is readable. All costs remain first-pass values
  for determinism and testability, not final balance.
- Decision, on save format: no new field. The Dynamo is a
  chunk byte (value 8, appended only), so old saves load
  unchanged — they simply contain no byte 8 — and powered
  state is recomputed from placement on the first tick
  after load. Session counters (KineticBlocksGathered,
  PoweredBlocksGathered) are not saved, matching the tier 1
  rule. If a future buffer decision creates contents that
  must survive, that step must add its field under the same
  old-save / malformed-entry rules; this step creates none.
- Deliberately NOT in this step: cables, networks, voltage
  tiers, transformers, mismatch destruction, a second source
  or consumer, crafting, fuel, a machine buffer, any Unity
  adapter change, any balance claim, and any change to squad
  price / cap, loot, skills, or core health. The Unity
  verification checklist and the Slice 1 director playtest
  gate remain open, untouched by this headless step.
- Next, in order: Slice 5 Step 2, demo scenario +
  documentation-only review.

## 2026-10-30 — Slice 5 Step 2 (0.26.0, under delegation)

- Decision: Step 2 is composition + review only, as with
  0.13.0, 0.19.0, and 0.23.0. No new core system rides along
  with a demo scenario and a review; a review that smuggled
  in a system would make its own verdict unverifiable. The
  demo's seeded stone pockets are a controlled-rate device,
  not a gameplay claim: the powered pair itself is built
  through the real Simulation builders, and the pockets exist
  so the 5-to-1 rate comparison is not at the mercy of
  generated terrain. The loaded-phase re-seeding is labelled
  in the demo, the tests, and the review as fresh seeding —
  the first pocket is spent, and pretending otherwise would
  overstate what the round-trip proves.
- Decision: Slice 6 candidate order is carried unchanged
  from the 0.24.0 scope (pitch + markers first, second squad
  type second). The Step 2 evidence gives no reason to
  reorder: power is proved at one voltage, and the
  presentation gaps that most limit the picker in Unity are
  older and more exposed than a second voltage. One new
  note is added to the first candidate, not a reordering:
  Slice 5 touched no Unity adapter, so powered automation
  has no Unity presence at all yet — that absence belongs
  with the pitch + markers candidate when Slice 6 is scoped.
- Deliberately NOT in this step: any Slice 6 system, any
  balance claim, any Unity adapter change, any change to
  Dynamo cost / distribution / cadence, and any change to
  repo visibility, licence, monetisation, or release status.
  The Unity verification checklist and the Slice 1 director
  playtest gate remain open, untouched by this headless step.
- Next, in order: scoping Slice 6 as its own decision step.

## 2026-10-30 — Slice 6 scope (0.27.0, under delegation)

- Decision: Slice 6 is scoped as "FPS presentation",
  taking the top-ranked candidate from the 0.24.0 scope and
  the 0.26.0 review in the same order, with no reordering:
  pitch + markers first, then a second squad type. The
  order is the decision, not just the list — the picker
  that deploys a squad cannot yet be aimed down at flat
  ground, a squad cannot yet be seen or read in the scene,
  and the powered loop Slice 5 proved cannot yet be built
  by hand, while a second squad type would widen combat
  through a picker that is still text-only. Widening before
  the presentation of the existing systems is usable would
  repeat the 0.15.0 mistake in a new form.
- Decision, on folding powered presence into Slice 6 as its
  own step (step 3) rather than leaving it as a footnote:
  the Slice 5 review attached the powered absence to this
  candidate, not to a separate candidate, so a Slice 6 that
  closed pitch and markers and still left no Dynamo key
  would claim its candidate done while its newest note was
  untouched. A separate step, rather than a fold into the
  markers step, keeps each step to one presentation system:
  markers render squads, the powered step wires keys and an
  indicator, and neither step's review has to excuse the
  other's size. The step is small only because core already
  answers the hard questions — builders exist, `IsPowered`
  exists — and the scope forbids it from extending power
  (no voltage, no second source or consumer) precisely so
  it stays that small.
- Decision, on what this scope deliberately leaves open:
  pitch's home (core Look versus adapter-only), what pitch
  changes first, the clamp and sensitivity, yaw-rotated
  movement, the snapshot activity word and marker set, the
  build keys, and the power indicator form are hypotheses
  here, not decisions. The hypotheses are: a minimal core
  Look with a direction vector, because a pitch that exists
  only as a camera angle is a rendering fact, not an aim
  fact, and the picker raycast is what actually consumes a
  direction; camera plus picker only in step 1, because
  switching gather, build, and fire to aim in the same step
  would change the proven Slice 1 loop while pitch is still
  unproved; a clamp that prevents flipping past vertical,
  reusing the existing sensitivity, with no invert option
  claimed; yaw movement left out of step 1, because pitch
  is vertical aim and the movement rewrite is already a
  flagged later step, not part of pitch; a core-derived
  activity word (marching / holding / hunting / idle) with
  the hunting target re-derived each tick, not saved, so
  movement, fire, and display can never disagree about the
  prey; one marker per squad, one per rally, one pending
  rally marker, in original primitives coloured cyan for
  squads and orange for rallies per the art direction, with
  no through-wall markers unless step 2 proves occlusion
  makes them useless; and a HUD / display power line derived
  from `IsPowered`, with a mesh tint only if step 3 proves
  it small. Each may be overturned in its step with
  evidence, and the step must record that reasoning rather
  than defending the hypothesis.
- Decision, on the honesty constraint: Slice 6 is the first
  slice whose main value is adapter code that cannot be
  compiled headlessly, and its gate says so in words. Prior
  slices could lean on the test count because their systems
  lived in core; Slice 6 steps lean on it only for their
  core pieces (Look math, activity word, indicator
  derivation), and their adapter pieces are Windows
  checklist items, full stop. A Slice 6 review that reported
  "633 tests passing" as if it covered a marker rendering
  would be the same mislabelling the 0.24.0 scope warned
  against for the playtest gate, in a smaller font. The
  scope therefore requires each step to add its own
  checklist items to the orbital / playtest docs, rather
  than letting the checklist stay at its Slice 4 size while
  the adapter code grows.
- Decision, on the gaps this scope does NOT claim to close:
  Unity verification and the director playtest are higher
  in the Slice 5 review's priority order, and this scope
  does not reorder them away — it records that neither is
  closable by a headless step (one is blocked on Unity 6000
  on Windows, the other on the director), so Slice 6 starts
  at the first actionable presentation gap and shrinks the
  verification checklist honestly, instead of pretending a
  scope document verifies anything. Balance remains a later
  pass, and the free-deploy gap remains documentation.
- Decision, on Slice 7 ranking: a second squad type first,
  then aim-based gather / build / fire plus yaw-rotated
  movement as one candidate (they are one question — does
  the player's aim, not a fixed +Z cell, decide what the
  player touches?), then the rest of tier 2. The review may
  reorder on Slice 6 evidence; this scope does not
  pre-order the remainder first.
- Deliberately NOT in this step: any Slice 6 system, any
  balance change, any enforcement of the free-path rule
  beyond its existing documentation, and any claim about
  Windows Unity verification or the pending Slice 1 director
  playtest gate — both remain open, and Slice 6 scoping
  does not close either.
- Next, in order: Slice 6 Step 1, FPS pitch aiming.

## Open decisions

- Scope slicing order beyond Slice 1 (Slice 1 is locked in
  `docs/slice-01-plan.md`; further slices are decided under delegation
  as Slice 1 evidence arrives).
- Release licence, monetisation, and release date — deferred to
  release stage, logged here when decided.
