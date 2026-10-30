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

## 2026-10-30 — Slice 6 Step 1: FPS pitch aiming (0.28.0, under delegation)

- Decision, on where pitch lives: in core, as `Look`,
  confirming the scope hypothesis, for the reason the
  scope gave and the step's tests now prove: the forward
  vector is what a raycast consumes, and a pitch that
  existed only as a camera angle could not be tested
  headlessly at all. The duplication risk the scope named
  is answered by moving yaw, not copying it: the adapter's
  `_yaw` is deleted, and `Simulation.Look` is the one yaw.
  FpsController is deliberately not the home, because
  Look is view state and FpsController is feet movement —
  putting view angles on the movement object would be the
  step that forces movement to rotate with yaw, which this
  step refuses to do.
- Decision, on sensitivity: reuse the adapter's existing
  `MouseSensitivity`, with no second sensitivity in core.
  `Look.Add` takes already-scaled deltas, so the number
  of sensitivity values in the game stays one, and a
  future sensitivity change is an adapter change, not a
  core change. No invert option is claimed: if Windows
  testing shows the Mouse Y sign inverted, the adapter
  call flips and the core convention (positive pitchDelta
  = look up) stays put, so headless tests never have to
  be rewritten for a Unity sign.
- Decision, on the clamp: [-89, 89], one degree short of
  vertical on each side. At exactly +/-90 the horizontal
  forward component is zero, the camera's up direction
  becomes undefined for LookRotation, and the view flips;
  89 keeps a small but nonzero horizontal component
  (0.0175) while being, for play purposes, straight up or
  down. It is a first-pass value for testability, not a
  feel claim.
- Decision, on what pitch changes first: camera + picker
  only, confirming the scope hypothesis. The picker
  raycast already follows `transform.forward`, so rendering
  the camera from `Look` is the whole picker fix — no
  picker code had to change at all, which is evidence the
  hypothesis was right about where the failure lived. The
  alternative (switching gather, build, or fire to aim in
  this step) was rejected because it would have changed
  the proven Slice 1 loop in the same step that introduces
  pitch, and no test could then say which change broke it.
  The step proves the non-change instead: pitched gather,
  build, and fire tests, and a yawed-movement test, are in
  the suite, not in a comment.
- Decision, on yaw-rotated movement: NOT bundled, per the
  scope hypothesis, and the yawed-movement test is the
  proof that the refusal is real — a Simulation yawed 90
  degrees still moves in +Z. Pitch is vertical aim; the
  movement rewrite is a flagged later Slice 1 step, and
  bundling it would have made step 1 a small FPS rewrite,
  not a small step. It is carried forward with aim-based
  interaction as a Slice 7 candidate, in the scope's
  order, not quietly dropped.
- Decision, on the save question: Look is session input
  state, not saved — and, refining the scope's wording,
  "not saved" does not mean "reset on every load". A
  fresh Simulation starts at 0, 0 (which is what a new
  Unity scene's adapter sees, and what a load into a fresh
  Simulation proves in tests); an in-place RestoreWorld
  keeps the current view, because RestoreWorld's whole
  contract is identity preservation (same Simulation, same
  Fps object) and snapping the camera to +Z on a Unity
  load would break that contract for no gameplay reason.
  The two cases are tested separately so the distinction
  cannot silently collapse later.
- Decision, on adapter rendering: `Quaternion.LookRotation`
  from the core forward vector, not `Quaternion.Euler`
  with a hand-negated pitch. Euler sign conventions for
  pitch are exactly the kind of Unity-specific fact a
  headless step cannot verify, and LookRotation sidesteps
  it: the core vector is the truth, and the adapter only
  points the camera along it. The sign question that
  remains (Mouse Y up vs down) is therefore a single
  checklist item, not a second copy of the math.
- Deliberately NOT in this step: aim-based gather / build
  / fire, yaw-rotated movement, any change to weapon,
  squad, Dynamo, or save rules, any balance value, and
  any claim about Windows Unity verification or the
  pending Slice 1 director playtest gate — the adapter
  code is written, the checklist items are added to
  `docs/orbital-command.md` (item 9) and
  `docs/slice-01-playtest-gate.md` (item 11), and both
  remain open.
- Next, in order: Slice 6 Step 2, 3D squad / rally markers
  and the Advance display fix.

## 2026-10-30 — Slice 6 Step 2: 3D squad / rally markers and the Advance display fix (0.29.0, under delegation)

- Decision, on where the activity word lives: in core, on
  the snapshot, confirming the scope hypothesis. The
  alternative — an adapter-computed word — was rejected
  because it would have given text and markers two chances
  to disagree, and because the adapter cannot know the
  prey without re-implementing the nearest-enemy rule.
  OrbitalView.Snapshot already reads the world, so it
  already has the enemies list; deriving activity there
  adds no new data path and no second targeting rule.
  The snapshot's existing copy semantics extend naturally:
  activity and prey are copied values at snapshot time,
  so a frame's display cannot change mid-frame.
- Decision, on the activity definitions: HoldGround is
  rally truth, Advance is prey truth, and the two are not
  merged into one rule. A single rule ("moving vs at
  target") cannot describe Advance, because Advance has
  no holding point: its "target" for movement is the prey
  while one lives and nothing at all when none does.
  Hence Marching/Holding for HoldGround, Hunting/Idle for
  Advance, as four words of one enum rather than two
  parallel booleans that could combine into nonsense
  (e.g. moving-and-idle). Idle is labelled as documented
  behaviour, not a bug: an Advance squad with no prey
  standing still is doing exactly what 0.22.0 decided it
  should do, and hiding it behind HOLDING would have
  been a second display lie replacing the first.
- Decision, on the hunting range question: hunting has
  NO range limit, deliberately, and this is where the
  step departs from a naive reading of "in range". The
  scope says Advance "with a living enemy reads as
  hunting whether it is moving toward that enemy or
  standing adjacent and firing" — a squad 10 cells from
  its prey is moving toward it, so it is hunting, even
  though EngagementRange 3 means it cannot fire yet.
  Limiting Hunting to range 3 would have made a marching
  hunter read Idle, which is worse than the old lie. The
  test proves the distinction: an Advance squad 4 away
  (outside range 3) reads HUNTING.
- Decision, on the prey cell: included in the snapshot,
  re-derived every snapshot, not stored on the squad and
  not saved. The scope's "re-derived each tick" is
  honoured in the stronger form "re-derived on every
  snapshot": snapshots are the only consumer, and a
  snapshot taken between ticks must already show the
  current prey. Storing the prey on the squad would have
  created a second targeting state that could go stale
  (the squad moves, the prey dies, the stored cell
  lingers) and a save-format question with no gameplay
  answer. The save/load test proves the derivation: a
  loaded Advance squad reads the same prey because the
  loaded world still contains it, with no new field.
- Decision, on the marker set: exactly one squad marker,
  one rally marker, and (while picking) one pending
  marker, confirming the scope hypothesis. No per-member
  markers (a squad is one commanded unit, not three
  figures), no enemy markers (enemies are already voxel
  figures in the scene), no path lines (they would be a
  second display system, not a marker). Marker form is
  primitive-only by scope constraint, and shape, not just
  colour, distinguishes pending from deployed: a sphere
  cannot be mistaken for a flat rally marker even by a
  player who cannot tell the two oranges apart, and the
  flat rally marker sits at the bottom of its cell so it
  is not hidden by a squad cube standing on the same
  cell — the common case, since a holding squad is on its
  rally by definition.
- Decision, on through-wall markers: NO, confirming the
  scope hypothesis, with the honest caveat recorded in
  the checklist rather than hidden. Ordinary opaque
  material means occlusion comes free from depth testing
  — no shader work, no render-queue trick, no wallhack
  claim. The counter-argument (markers exist to be found)
  was considered and not dismissed: if Windows testing
  shows occlusion makes markers unreadable at FPS
  distance, that evidence, recorded against checklist
  item 8, is what justifies revisiting the decision in a
  later step. A readability aid that cannot be tested
  headlessly must not be smuggled in as a shader default.
- Decision, on marker hygiene: colliders removed on
  creation, and pooling with deactivation rather than
  per-frame rebuild. A marker with a collider would block
  the picker raycast — the exact system markers exist to
  serve — and could block player movement; removing it is
  a one-line decision with a testable Windows check
  (checklist item 9), not a physics-layer redesign.
  Pooling caps allocation at the 3-squad cap (3 + 3 + 1
  objects) and matches the existing adapter pattern of
  rendering a snapshot without owning game state.
- Decision, on the old word: replaced, not kept
  alongside, per the scope. `AtRally` and `IsMoving` stay
  because they are position facts with existing tests,
  not display words — deleting them would have broken
  unrelated tests for no display benefit — but the text
  display no longer reads them, and no adapter code
  computes a state word from them any more. Keeping both
  words would have let a future edit show MARCHING in
  text and HOLDING in a marker tooltip and call both
  correct.
- Deliberately NOT in this step: any change to squad
  movement, fire, targeting, doctrine, or kind; squad
  price or cap; the picker flow or keys; Dynamo or mill
  behaviour; any save-format field; unit models, an
  Orbital camera, enemy markers, or path lines; and any
  claim about Windows Unity verification or the pending
  Slice 1 director playtest gate — the adapter code is
  written, the checklist items are added to
  `docs/orbital-view.md` (items 7-9), and they remain open.
- Next, in order: Slice 6 Step 3, powered Unity presence.

## 0.30.0 — 2026-10-30 — Slice 6 Step 3: powered Unity presence

- Decision, on the build keys: T for the Kinetic Mill,
  Y for the Dynamo, confirming the scope hypothesis that
  this step wires keys to already-proved builders rather
  than adding a build rule. The choice was checked
  against the full existing key set (WASD / Space / E /
  Q / R / O / Return / Esc / 1 / 2 / mouse click), so
  there is no collision to discover on Windows, and the
  pair sits together on the keyboard for the reason that
  matters mechanically: front-left (mill) vs
  front-centre (Dynamo) with no move between builds is
  face-adjacent, so the pair a player builds by hand is
  powered by construction, not by a second step. Either
  order works, because adjacency is symmetric.
- Decision, on picker suppression: both new keys are
  suppressed while the picker is open, and the rationale
  is ore-specific, not generic modality. Mouse fire is
  already suppressed while picking; a Dynamo key is the
  same hazard in resource form — 5 EmberOre spent
  mid-pick can drop the player below the 10 a pending
  Confirm is about to charge, turning an aimed deploy
  into an unexplained refusal. The mill is suppressed
  with the Dynamo even though it spends no ore, because
  the two keys are one build pair and a pair with two
  modal rules would be a trap (build a mill while
  picking, then wonder why its Dynamo key does nothing).
  E / Q / R were deliberately left unsuppressed: they
  spend no EmberOre, so they cannot break a pending
  Confirm, and changing them would bundle a second modal
  change into a powered-presence step. If a later step
  makes the picker fully modal, it should say so and
  change all of them at once, with tests for the new rule.
- Decision, on the indicator form: a count pair on
  HudState (`PoweredMills` / `TotalMills`), rendered as
  one `POWER: x/y MILLS POWERED` line, confirming the
  scope hypothesis of a HUD line derived from IsPowered.
  A count was chosen over a per-mill mark because the
  scope offered both and the count is the one that
  survives the common case: several mills across a base,
  where a mark on each mill line would need a mill list
  the HUD does not have, while a count answers the
  player's actual question (is my automation powered?)
  in one line. The derivation reuses the exact rule the
  mills themselves run on, so indicator and behaviour
  cannot disagree the way a second power computation
  could. Only mills are counted: a Dynamo alone is a
  source with no consumer, and a wall next to a Dynamo
  is not a consumer either — counting either would make
  1/1 lie about a base with no powered machine.
- Decision, on the mesher: NO distinct Dynamo / mill
  tint in this step, with the check recorded rather than
  assumed. MeshBuilder gives every quad identical unit
  UVs and no per-block colour or atlas, and
  VoxelMeshRenderer puts all quads in one mesh, so there
  is no existing distinctness to switch on — a tint
  would be new UV / material work plus Unity material
  wiring, unprovable headlessly, in a step whose job is
  keys plus a text line. The scope explicitly allowed
  this outcome ("carried as a named gap, not claimed"),
  and the gap is named in docs/powered-presence.md
  alongside its checklist observation, so a later step
  starts from a confirmed gap, not a rediscovery.
- Decision, on the mesh-refresh gap: named, not fixed,
  for the same scope discipline. No adapter refreshes
  the mesh on any key build — E / Q have always built
  core state without a refresh hook, and GameManager
  refreshes only on start and load — so wiring a refresh
  for T / Y alone would have made the new keys behave
  differently from the old ones, and wiring one for all
  builders would have been a second system in this step.
  The honest claim is therefore split: the HUD power
  line updates every frame from Hud() and is the presence
  proof, while mesh visibility of a key build is a
  checklist question (item 6) whose answer decides a
  refresh-on-build follow-up for all builders at once.
- Deliberately NOT in this step: any change to Dynamo
  cost, distribution, cadence, or kinetic mill behaviour;
  squad rules, loot, skills, or core health; any
  save-format field; a voltage ladder, second source,
  second consumer, crafting, or fuel; a mesh tint; a
  mesh-refresh hook; any change to E / Q / R modality;
  and any claim about Windows Unity verification or the
  pending Slice 1 director playtest gate — the adapter
  code is written, the checklist is added to
  docs/powered-presence.md and item 12 to
  docs/slice-01-playtest-gate.md, and they remain open.
- Next, in order: Slice 6 Step 4, demo scenario +
  documentation-only review.

## 0.31.0 — 2026-10-30 — Slice 6 Step 4: demo scenario + review

- Decision, on composition order: the demo runs the core
  pieces in scope order — Look, then squads one at a time,
  then the powered pair — rather than all at once, because
  a simultaneous run would let combat crossfire muddy the
  activity readings the demo exists to prove. The
  HoldGround march runs with no enemy at all, so
  Marching -> Holding is a clean reading; the Advance
  engagement runs with the HoldGround squad already at
  its rally, 8 cells from the prey, so it cannot fire or
  be targeted into the reading. Sequential composition
  is still composition: one Simulation, one save, and
  the loaded game re-proves every piece together.
- Decision, on the Advance kill: the demo uses a
  full-strength Advance squad against a 30-HP enemy, so
  the prey dies in one engagement tick and Hunting ->
  Idle is proved in a single tick, with the squad still
  at 90 HP because the prey dies from squad fire before
  any adjacent damage. A weakened squad would have made
  the same transition take three ticks and added damage
  numbers the activity question does not need; the
  Slice 4 demo already proved weakened-squad combat in
  detail, so this demo does not re-prove it.
- Decision, on the loaded Look: the demo loads into a
  fresh Simulation, so its Look is the default view
  (yaw 0, pitch 0, forward +Z), and the tests assert
  exactly that. This is the session-state rule from
  0.28.0, not a save bug: Look is not saved, a fresh
  game starts facing +Z, and an in-place restore into
  an existing Simulation keeps the current view. The
  review states both halves, so the fresh-view number
  is not misread as a lost view.
- Decision, on the review's claim split: the review
  closes Slice 6 as tested presentation core plus
  written adapters, and it adds no new playtest-gate
  checklist item, because steps 1–3 already added theirs
  (gate items 11–12, plus the orbital and powered
  checklists). Adding a review-step checklist item would
  have implied the review itself added something to
  verify in Unity, when it adds only a verdict about
  steps already listed.
- Decision, on Slice 7 candidates: carried unchanged
  from the 0.27.0 scope — second squad type first,
  aim-based interaction + yaw movement second, rest of
  tier 2 third — because the Step 4 evidence gives no
  reason to reorder them. The review does not scope
  Slice 7: scoping is the next run's decision, made with
  this composition evidence in hand.
- Deliberately NOT in this step: any new core system,
  squad, doctrine, kind, power tier, or consumer; any
  change to squad rules, price, or cap; Dynamo cost,
  distribution, or cadence; kinetic mill behaviour;
  loot, skills, or core health; any save-format field;
  any adapter file change; a mesh tint or a mesh-refresh
  hook (both remain named gaps from 0.30.0, not fixed by
  a composition step); a balance pass; any claim about
  Windows Unity verification or the pending Slice 1
  director playtest gate.
- Next, in order: scoping Slice 7 as its own decision
  step, from the ranked candidates carried in
  docs/slice-06-review.md.

## 0.32.0 — 2026-10-30 — Slice 7 scope: second squad type

- Decision, on the Slice 7 theme: the second squad type,
  taken from the Slice 6 review's top-ranked candidate
  in unchanged order, because the Step 4 evidence gives
  no reason to reorder and Slice 6 removes the deferral
  reason itself. In 0.27.0 the second type was deferred
  because pricing a unit before the picker that deploys
  it could be aimed and read in the scene would repeat
  the 0.15.0 mistake; Slice 6 gives the picker pitch
  aiming, markers, activity words, and a text kind word,
  so a second kind can now be deployed and read, not
  just spawned headlessly. The aim candidate is not
  promoted above it, because aim changes the ground
  every squad stands on — a second shape proved first
  gives that later step two kinds to prove its
  interaction against, not just Vanguard.
- Decision, on scope shape: two steps, not four — one
  system step plus demo + review, like Slice 5, rather
  than Slice 6's four-step presentation sequence. A
  second kind is one system with several decisions
  inside it (role, shape, price, cap, binding, save,
  targeting, display), and splitting it into separate
  stats / price / binding steps would let each step
  claim a kind that cannot yet be deployed, priced, or
  read as a whole. The scope therefore keeps the step
  whole and forces its decisions to be recorded
  together, with the demo + review step as the
  composition proof — the same discipline that kept
  Slice 5's loop whole.
- Decision, on what is left to step 1 and what is
  fixed here: only the theme, the order, the
  deliberately-not-in list, and the Vanguard-unchanged
  constraint are decisions of this scope. The kind
  choice, its numbers, its price, its cap shape, its
  picker binding, its doctrine interaction, its save
  clamp, and its targeting and marker rules are all
  explicitly left to step 1, with starting hypotheses
  (recon / heavy / flyer pool, shared movement rule,
  per-kind stats table, total cap 3, independent kind
  selection, both doctrines, kind-blind targeting, same
  cyan marker) stated as hypotheses, not decisions.
  The hypotheses are there so step 1 starts from a
  falsifiable position — if it finds, for example, that
  a flyer needs a movement rule or a heavy needs a
  per-kind cap, it must prove that with tests and say
  so, not quietly accept or reject the hypothesis.
  A hypothesis that step 1 overturns with evidence is
  a successful scope, not a broken one.
- Decision, on the hard facts the scope names rather
  than smooths over: the scope document records, with
  file-level grounding, that stats are shared constants
  (not a table), that kind is already saved at index 8
  but the load clamp is Vanguard-shaped at 0..90, that
  adapter keys 1/2 both force Vanguard, and that the
  picker prompt hardcodes COST 10 and MAX 3. Naming
  those four facts in the scope is deliberate: a scope
  that said "add a kind" without them would let step 1
  discover the clamp bug, the missing binding, or the
  lying prompt as a surprise, and a surprise in step 1
  is how a small step grows into a rewrite. The scope
  also records that the text display already renders
  kind verbatim, so step 1 owes no text change unless
  its price or cap decision changes the prompt facts.
- Decision, on Slice 8 ranking: aim-based interaction
  plus yaw movement first, rest of tier 2 second, and
  a third kind plus Defend Core / Patrol third — the
  third entry is new, not carried, and is there because
  Slice 7's evidence is what will judge whether a third
  kind or a new doctrine is the next widening worth
  doing. Ranking them third is not a rejection of
  either; it is a refusal to promise a third widening
  before the second one has been proved.
- Deliberately NOT in this step: any new core system,
  kind, doctrine, power tier, or consumer; any change
  to Vanguard's stats or behaviour; any change to
  squad price, cap, or picker bindings; any save-format
  field; any adapter file change; a mesh tint or a
  mesh-refresh hook (both remain named gaps from
  0.30.0); a balance pass; any claim about Windows
  Unity verification or the pending Slice 1 director
  playtest gate.
- Next, in order: Slice 7 Step 1, exactly one second
  squad type, decided in that step.

## 0.33.0 — 2026-10-30 — Slice 7 Step 1: second squad type, Recon

- Decision, on the kind: Recon, from the scope's recon /
  heavy / flyer pool. The choice names the tactical choice
  Vanguard cannot make: engaging from range 4-5, outside
  Vanguard's range 3 and outside the enemy's adjacent-only
  damage, so a Recon screen can fire while the enemy is
  still walking toward it. Heavy was rejected, not
  deferred silently, because its role collapses: a tougher
  anchor under HoldGround is Vanguard with more HP and
  less damage, and the test that would prove it distinct
  from Vanguard is just a numbers test, not a new choice
  a player makes on the field. Flyer was rejected for
  this step, not as a bad idea, because the scope's own
  warning holds: flight is a movement decision (ignoring
  solids, or moving in the air), with targeting and
  occupancy consequences that cannot be proved by a stat
  table alone, and bundling it into the first second-kind
  step would make the kind untestable in isolation. Recon
  needs no movement change at all, so its step proves a
  kind, not a kind plus a movement system.
- Decision, on the shape: 2 members, 20 HP each, 6 damage
  each, range 5. Each number is a trade stated against
  Vanguard, not a tune: range 5 is the only dimension
  where Recon beats Vanguard, and it pays for it with
  less than half the health (40 vs 90) and less than half
  the full-strength damage (12 vs 30). A Recon squad that
  also kept Vanguard's health or damage would be a
  replacement, not a choice, and the tests prove the
  trade in both directions (fires at 5, silent at 6,
  Vanguard silent at 4). The numbers are first-pass for
  determinism and testability, not final balance, exactly
  as Vanguard's were — the Slice 7 gate still forbids a
  balance pass from riding along.
- Decision, on representation: a per-kind shape table
  (`SquadShape` / `SquadShapes.For`), read from Kind by
  every rule. The alternative — a second set of shared
  constants, or a branch in every rule that reads
  Vanguard's numbers — was rejected because it makes the
  next kind a rewrite of every rule it touches, while a
  table makes it one new row. The old constants are
  removed, not kept as aliases, so there is no second
  source of Vanguard's numbers to drift from the table;
  the cost is that any outside reader of those constants
  must read the shape instead, and the repo has no such
  reader outside Orbital.cs (verified by search before
  the refactor). Unknown kinds throw from For rather
  than falling back to Vanguard, because a silent
  fallback would deploy a Vanguard wearing a Recon name.
- Decision, on price: 10 EmberOre for both kinds, via an
  explicit `CostForKind` that returns the same value for
  both. Keeping one price is itself the decision: the
  kinds trade role for role, and a price gap would claim
  an economy distinction (Recon worth more, or less,
  ore than Vanguard) that no play evidence yet supports.
  The explicit method, rather than leaving the old
  single constant to imply the answer, is deliberate: a
  later step that does price kinds differently changes
  one method with its own rationale and tests, instead
  of discovering the price was never per-kind at all.
  The prompt's COST 10 line staying true is a
  consequence, not the reason — the reason is that an
  invented price gap would be a balance claim disguised
  as an economy rule.
- Decision, on cap: 3 live squads in total, not 3 per
  kind. The 0.18.0 cap was decided as a fielded-squad
  limit, and a per-kind cap would let this step double
  the field to 6 by adding one kind — a cap change
  disguised as a kind addition. The total cap is proved
  by a mixed-kind test (Vanguard + Recon + Recon, then a
  4th Recon refused with ore untouched), not just by
  re-asserting the old Vanguard-only cap test.
- Decision, on binding: independent kind selection, keys
  3 / 4, with keys 1 / 2 reduced to doctrine-only. The
  scope's hypothesis is accepted as stated: coupling
  kind to doctrine again (a 3rd preset) would leave the
  fourth combination unreachable and repeat the 1 / 2
  pattern that made this step necessary. Changing 1 / 2
  semantics is a behaviour change to existing keys, and
  it is recorded here rather than hidden: a player who
  learned 1 = HoldGround+Vanguard now gets HoldGround
  with whatever kind is selected, which is the point —
  doctrine and kind are two choices, and a key that
  silently resets one choice while making the other is
  not a menu, it is a preset list. The binding is
  adapter code, written not compiled headlessly, so it
  rides with checklist item 10, not with the test count.
- Decision, on doctrine interaction: both kinds under
  both doctrines, with no restriction and no refusal
  rule invented. A restricted kind would have needed a
  picker refusal for the forbidden combination (not a
  silent Vanguard fallback), and no such restriction has
  a tactical reason yet: a long-range Advance squad is
  a hunting screen, and a long-range HoldGround squad is
  a rally guard that fires early — both are coherent,
  and both are proved (Advance Recon hunts west and
  fires; HoldGround Recon holds and fires at range 5).
- Decision, on save: no new field, per-kind clamp. Kind
  was already index 8, so the format question is only
  the clamp — and the clamp had to change, because the
  old 0..90 bound is Vanguard's maximum, not a generic
  bound: it would not trim a legitimate Recon squad,
  but it would let a corrupt Recon save claim 90 HP and
  therefore 5 apparent members. The clamp is now the
  kind's own maximum, assigned after Kind in the load
  constructor for exactly that reason; the ordering is
  load-bearing and is commented as such. Malformed and
  unknown kinds keep the skip-never-fatal rule, and the
  unknown-kind skip logic itself is unchanged — only the
  defined set grew by one.
- Decision, on targeting: kind-blind, unchanged. No
  priority rule (no scout-ignored, no heavy-taunt) was
  invented, because targeting priority is a combat rule
  change, not a stat, and Recon's fragility already
  expresses its risk through the existing rule: an enemy
  that pursues a Recon screen kills it faster than a
  Vanguard from the same cell, which is the trade the
  player accepted when deploying it. The test proves
  pursuit of the nearer Recon squad, not a priority.
- Decision, on markers: same cyan cube, kind from the
  text line, per the Slice 6 discipline of one marker
  per squad and no extra layer. Two kinds sharing a
  marker is a real readability question, but it is not
  yet a play-observed confusion — and adding a distinct
  shape or colour on a guess would spend the marker
  discipline on a hypothetical. If a Windows playtest
  shows players mistaking a Recon for a Vanguard in the
  scene, that observation, not this step, justifies a
  distinction, with its own checklist item.
- Decision, on what this step does not do, stated
  plainly so the review can check it: it does not change
  Vanguard in any number or rule (every pre-existing
  Vanguard test passes unchanged, and the shape tests
  pin Vanguard's numbers); it does not add a third kind,
  a doctrine, a voltage tier, or a balance pass; it does
  not change Dynamo, mill, loot, skill, or core-health
  rules; it does not add a save field; and it does not
  claim Unity verification for keys 3 / 4, the prompt,
  or the marker — those are checklist item 10, to be
  completed on Windows and recorded here.
- Test-authoring note, not a code fix: the first test
  run failed to compile because two new top-level test
  variable names (`pickSim`, `picked`) collided with
  variables from the 0.15.0 picker tests in the same
  file. They were renamed (`reconPickSim`,
  `reconPicked`); no core code changed for the collision,
  and the full suite then passed 810 / 810.
- Next, in order: Slice 7 Step 2, demo scenario +
  documentation-only review.

## 0.34.0 — 2026-10-30 — Slice 7 Step 2: demo scenario + review

- Decision, on composition order: the demo runs two
  parallel lanes, not sequential squads, because the
  Slice 7 question is comparative — what Recon does that
  Vanguard cannot — and a sequential run would prove
  each kind alone, which Step 1 already did. The lanes
  close at the same rate (both enemies start at 27,
  both squads step east once per tick), so the range
  contrast is a controlled comparison: the Recon's first
  shot lands at exactly range 5 on tick 3 while the
  Vanguard, still marching, is silent, and the Vanguard's
  kill lands at exactly range 3 on tick 4. A simultaneous
  run risks crossfire muddying the readings, and the
  lanes are the control against that: each enemy is
  pursued by its own lane's squad, and the player is
  parked far away so no enemy step is player chasing.
- Decision, on the lane setup: the player walks to
  z=20 between deploys, because a squad spawns at the
  player's feet and deploying both squads from one spot
  stacks them. This is recorded as a scenario-scripting
  fact, not a core rule: the first draft of this
  scenario deployed both from (16,5,16), and its output
  showed the Recon hunting the Vanguard-lane enemy
  instead of its own. The fix was the player move, and
  the test pins the Recon's spawn lane (Z = 20) so the
  mistake cannot silently return. No deployment rule
  changed for this.
- Decision, on the demo's scope reading: the demo
  exercises both doctrines across the two kinds
  (Vanguard HoldGround, Recon Advance), not all four
  combinations in one run, and the review says so
  plainly. The other two combinations are proved in the
  Step 1 tests. Claiming all four from a two-squad demo
  would have been an over-claim, and deploying all four
  would have needed a fourth squad slot at a cap of 3
  or a second run — neither is justified for a review
  step, whose job is composition, not re-proof.
- Decision, on the affordability refusal: the demo
  spends exactly 20 ore on two deploys and then refuses
  a third picker deploy for affordability, with the
  picker staying open and nothing charged. The cap
  refusal was already proved in the Slice 3 demo, so
  this demo proves the other refusal path instead of
  re-proving the cap — and it proves it in the mixed-
  kind context, where a reader might otherwise wonder
  whether a second kind escapes the economy. It does
  not: price, cap, and refusal are kind-blind except
  for the per-kind price lookup, which returns 10 for
  both.
- Decision, on the review's claim split: the review
  closes Slice 7 as tested second-kind core plus
  written adapters, and it adds no new playtest-gate
  checklist item, because Step 1 already added the
  adapter claims to checklist item 10 in
  docs/orbital-command.md (kind keys 3 / 4,
  doctrine-kind independence, prompt kind word, Recon
  text line, same cyan marker). Adding a review-step
  checklist item would have implied the review itself
  added something to verify in Unity, when it adds
  only a verdict about steps already listed.
- Decision, on Slice 8 candidates: carried unchanged
  from the 0.32.0 scope — aim-based interaction + yaw
  movement first, rest of tier 2 second, third kind and
  deferred doctrines third — because the Step 2 evidence
  gives no reason to reorder them. The range contrast
  proves two kinds differ, not that interaction should
  move before or after power work, and the review does
  not scope Slice 8: scoping is the next run's decision,
  made with this composition evidence in hand.
- Deliberately NOT in this step: any new core system,
  squad, doctrine, kind, power tier, or consumer; any
  change to squad rules, price, or cap; Dynamo cost,
  distribution, or cadence; kinetic mill behaviour;
  loot, skills, or core health; any save-format field;
  any adapter file change; a distinct Recon marker (kept
  at the shared cyan cube per Step 1, pending a
  play-observed confusion); a mesh tint or a mesh-refresh
  hook (both remain named gaps from 0.30.0, not fixed by
  a composition step); a balance pass; any claim about
  Windows Unity verification or the pending Slice 1
  director playtest gate.
- Next, in order: scoping Slice 8 as its own decision
  step, from the ranked candidates carried in
  docs/slice-07-review.md.

## 0.35.0 — 2026-10-30 — Slice 8 scope: aim-based interaction

- Decision, on Slice 8 order: movement before aim, body
  before hands. Movement rotation is the smaller rule —
  one rotation of two scalars, testable on yaw alone —
  while the aim step's tests will want to position and
  face the player freely; proving the body first means
  the aim step walks a yawed player with the real rule
  instead of teleporting around a world-aligned walk it
  is about to replace. The reverse order would work
  too, and the scope says so by giving the aim step no
  dependence on movement it cannot set up with yaw
  alone — but the chosen order keeps each step's failure
  attributable to exactly one rule, which is the
  discipline every slice since Slice 6 step 1 has used.
- Decision, on taking the top candidate unchanged: the
  Slice 7 review's ranking is kept — aim interaction
  first, rest of tier 2 second, third kind and deferred
  doctrines third — because the Step 2 evidence gives
  no reason to reorder it. The range contrast proves
  two kinds differ; it says nothing about whether power
  work should move before interaction work, and a scope
  that reordered on no evidence would be guessing, not
  deciding. The tier-2 and widening candidates are
  carried as ranked Slice 9 candidates, not promises.
- Decision, on the picker as precedent, not as work: the
  rally picker already aims by the core `Look` vector
  (adapter raycast along camera forward, pitch-aware
  since 0.28.0), so Slice 8 extends an existing aim
  system to the player's own actions rather than
  inventing a second one. The picker itself is not
  reworked by this scope: its ray, its bounds, and its
  fail-closed Confirm are settled, and an aim step that
  touched the picker would be two systems at once.
- Hypotheses recorded, not decisions, for the steps to
  settle with tests: rotation lives in core
  (`Simulation.Tick`, adapter still raw); movement uses
  the horizontal yaw projection only, never pitch;
  the aim ray is voxel-stepped from a stated eye origin
  with a stated, bounded reach; offsets retire and the
  three builders are distinguished by block type; a shot
  that hits nothing still consumes ammo, as today; a
  no-target action refuses with existing failure results
  and never silently falls back to fixed +Z cells; the
  adapter needs no logic change in Step 2. Each is
  labelled a starting hypothesis in the scope so a step
  that overturns one does so openly, with rationale and
  tests, rather than quietly.
- Questions carried into the steps, not answered here:
  the strafe sign at yaw 90; whether diagonal speed is
  preserved or normalised under rotation; the eye-height
  origin for the aim ray; whether one reach covers all
  three actions or each has its own; what looking up
  gathers; whether fire uses one aim rule for enemies
  and the core or two stated rules; and whether neutral
  aim reproduces the Slice 1 cells or moves the demo
  numbers openly. A scope that answered these without
  tests would be deciding what the steps exist to prove.
- Deliberately NOT in this scope step: any new core
  system, movement mode, weapon rule, squad, doctrine,
  kind, power tier, or consumer; any change to movement,
  gather, build, fire, squad, Dynamo, mill, loot, skill,
  or save rules; any save-format field; any adapter file
  change; a distinct Recon marker (kept at the shared
  cyan cube per 0.33.0, pending a play-observed
  confusion); a mesh tint or a mesh-refresh hook (both
  remain named gaps from 0.30.0, not fixed by a scoping
  step); a balance pass; any claim about Windows Unity
  verification or the pending Slice 1 director playtest
  gate.
- Next, in order: Slice 8 Step 1, yaw-rotated movement.

## 0.36.0 — 2026-10-30 — Slice 8 Step 1: yaw-rotated movement

- Decision, on where the rotation lives: in core, in
  `Simulation.Tick`, following the scope's starting
  hypothesis — because Tick is the one place that already
  knows both the raw input scalars and the current Look,
  and a rotation there is headless-testable, while an
  adapter-side rotation would be a second, untestable
  movement rule beside the core one. The scope allowed
  putting it inside FpsController instead with a reason;
  that was refused, not because it could not work, but
  because a controller that takes a yaw would need a Look
  (or a yaw parameter every caller must supply), and
  every direct-controller test would then be testing a
  rotation it never asked for. Instead the controller
  gains a world-space entry point, `UpdateWorld`
  (+X / +Z components), and the old `Update` delegates
  to it — so there is still one movement implementation,
  the controller stays a pure world-space mover with no
  Look, and the rotation is exactly one formula in Tick.
  The adapter passes raw scalars, unchanged in logic.
- Decision, on the rotation itself: yaw only, horizontal
  only. Forward is (sin yaw, cos yaw) and right is
  (cos yaw, -sin yaw) in XZ, derived from the same
  convention Look already uses (yaw 0 faces +Z, yaw
  increases toward +X). Pitch contributes nothing: the
  formula never reads `Look.Pitch` or `ForwardY`, so
  looking up cannot climb and looking down cannot dive,
  and a pitched yaw-90 walk is proved level. A full-3D
  forward vector would have made pitch a movement input
  by accident, and a movement mode (climb, flight) is
  explicitly not in Slice 8.
- Decision, on the strafe sign at yaw 90: D walks -Z.
  This is the right-hand side of facing +X under the
  game's axes (right = up × forward = (cos yaw, -sin
  yaw)), and it is pinned by tests in both directions
  (D -Z, A +Z) rather than left to a reader to infer
  from a sign in the formula. The opposite sign would
  have made D walk to the player's left, which no
  FPS player would accept, and a test that only checked
  W would have let that error hide until play.
- Decision, on diagonal speed: PRESERVED, not
  normalised. Today forward and strafe are applied at
  full speed each, so a diagonal is sqrt(2) times a
  straight walk; normalising it would have been a feel
  and balance change disguised as a direction fix, and
  Slice 8 is explicitly not a balance pass. Preserving
  it keeps Slice 1 feel, and the tests prove the
  magnitude is preserved under rotation (yaw 0, 90,
  and 45 diagonals all measure the same speed), so a
  later balance step, if any, starts from a known
  number rather than an accidental one.
- Decision, on what does NOT change, and the deliberate
  asymmetry: speed, gravity, jump, AABB, sliding,
  grounded logic, and the skill multiplier are unchanged
  (the first five by sharing the one UpdateWorld code
  path, the last proved by a Swift Feet rotated walk);
  squad and enemy movement and targeting are untouched,
  since a yawed player is the same target at the same
  position; and gather, build, and fire deliberately
  keep their fixed +Z / nearest-enemy rules for exactly
  this step, per the scope, so a movement failure and
  a targeting failure stay distinguishable. The tests
  prove that asymmetry openly (yaw-90 gather, build,
  and fire all still behave Slice-1), rather than
  letting a reader mistake it for a missed update —
  Step 2 is where those rules learn to aim.
- Decision, on the changed pre-existing test: the 0.28.0
  test asserting yaw-90 movement stays world-aligned
  is replaced, not kept alongside the new tests, because
  it asserted the exact behaviour this step closes. Its
  replacement comment says so, and the pitched-gather /
  build / fire non-change tests from the same block are
  kept unchanged. A test-authoring error in this step's
  own new tests is recorded the same way: a diagonal
  sliding test first used a single wall cell, which the
  suite failed because a diagonal walk legitimately
  slides around a corner cell; the test now uses a full
  wall plane plus a pure-X blocked check, and no core
  code changed for it.
- Deliberately NOT in this step: aim-based gather /
  build / fire (Step 2); any adapter logic change (only
  adapter comments changed, to stop claiming
  world-aligned movement); any movement mode, vertical
  movement by pitch, sprint, or crouch; any speed,
  gravity, or jump change; any squad, doctrine, kind,
  weapon, Dynamo, mill, loot, skill, or save change;
  any save-format field; a balance pass; any claim
  about Windows Unity verification (new checklist item
  13 in `docs/slice-01-playtest-gate.md` only) or the
  pending Slice 1 director playtest gate.
- Next, in order: Slice 8 Step 2, aim-based gather /
  build / fire.

## 0.37.0 — 2026-10-30 — Slice 8 Step 2: aim-based gather / build / fire

- Decision, on the ray: one shared ray, `AimRay.Trace`,
  in core — not three per-action rays that could drift
  apart, and not an adapter raycast, which would be
  untestable headlessly. Origin is the eye, feet +
  1.6 m: the scope allowed feet with a reason, but the
  adapter already renders the camera at Fps.Y + 1.6,
  so core and adapter agreeing on the eye is better
  than core inventing a second eye height. Traversal
  is a 0.05 m stepped walk, not a DDA: at that step a
  cell change is always face-by-face, so the previous
  cell is always face-adjacent to the hit solid, which
  is exactly what the build rule needs, and a sampled
  point inside a cell cannot skip a thin target. Reach
  is one number, 10 m, for all three actions: it
  matches the old core bubble, so fire keeps its range
  while gaining aim, and a single reach keeps the step
  from smuggling a per-action range decision in
  disguise. It is a first-pass value, labelled as such
  in the changelog, not a feel claim.
- Decision, on gather targeting: the first solid along
  the ray, through the existing World.Gather rules.
  Pitch is confronted, not discovered: down gathers
  ground, level gathers what is ahead at eye height,
  up gathers nothing (Air). The scope's alternative —
  gathering the ground under the aimed point — was
  refused: it would let a level aim gather a block
  10 m away under a point in the sky, which is a
  cursor, not a hand. Actors do not block a gather:
  an enemy standing in front of a wall should not make
  the wall ungatherable, and the ray passes through
  actor cells because they are air.
- Decision, on build targeting: the last air cell
  before the first solid — the standard voxel
  placement rule, and the only one that gives both a
  ground-top placement (aim down at ground, build on
  top of it) and a face placement (aim at a wall side,
  build beside it) from one rule. The three offsets
  retire: keeping front-right / front-left /
  front-centre under aim would mean the aim decides
  one cell and the offset silently moves it to
  another, which is the same hidden-default bug this
  slice closes. No-target is Floating (spending
  nothing, since World.Build is never reached), and
  out-of-chunk is OutOfBounds — both reuse existing
  results, because inventing a NoTarget result would
  be a new API for a case the existing results already
  describe as "cannot place here".
- Decision, on fire targeting: one rule for enemies
  and the core, so the old two methods are removed,
  not kept as aliases. A method named "nearest"
  beside an aim rule would leave the aim-blind rule
  alive under its old name, and a reader could call
  it by habit. The first actor cell wins over any
  solid behind it because the walk reaches it first;
  a solid first stops the shot, so fire cannot hit
  through a wall. A miss still consumes ammo, as
  before: making misses free would be an economy
  change, not an aim fix, and the tests pin it for
  blocked, out-of-reach, and skyward shots alike.
- Decision, on no-target behaviour: refusal with
  existing failure results, never a silent fallback
  to the fixed +Z cells — the scope's starting
  hypothesis, kept. Gather returns Air (or
  OutOfBounds past the chunk edge), build returns
  Floating (or OutOfBounds), fire consumes the round
  and hits nothing. Any one of these falling back to
  +Z would hide every aim failure and reintroduce,
  as a hidden default, the exact bug this slice
  closes, so the tests pin the refusals, not just
  the successes.
- Decision, on neutral-aim compatibility: proved and
  reported as a split, not a single claim. Pitch -45
  at yaw 0 reproduces the old gather cell (16,3,17),
  because the eye ray at that angle strikes the
  ground one cell ahead — so the old cell is not
  lost, it is now reached by aiming down, as an FPS
  player would. Level aim at yaw 0 does NOT
  reproduce the Slice 1 cells, because the eye is
  1.6 m above the feet and a level ray passes over
  the ground; the Slice 1 demo is therefore updated
  (aimed gather / build, aimed fire) and its numbers
  stand (gathered 1, built 1, killed 2, save 43,881).
  The Slice 2 demo numbers do change, and are
  reported openly in the changelog: killed 1 (was 2),
  save 43,921 (was 43,908), because aimed fire is
  range-bounded at 10 m where the nearest rule was
  not. Only a silent mismatch would have been
  dishonest; a changed number with a stated reason
  is the step working as intended.
- Decision, on the adapter question: no new binding.
  E / Q / T / Y and mouse click call the same
  methods, which now read the core Look ray, so the
  only adapter change is the renamed fire call plus
  comments that stop claiming Slice 1 cells. The aim
  claim is checklist item 14, Windows-only, not a
  Unity claim — the same honesty split every slice
  has kept between headless-tested core and
  checklist adapter claims.
- Test-authoring error caught by the suite, recorded
  the same way as in 0.36.0: a yaw -45 / pitch -45
  mill aim was first used for the powered pair, and
  the suite failed because that ray crosses the x=15
  face before it reaches the ground, landing the mill
  at (14,4,17) — one cell west of the old front-left
  cell, and therefore not face-adjacent to the Dynamo
  at (16,4,17). The tests now aim at the (15,3,17)
  ground cell itself (yaw -18.4, pitch -53), which
  lands the mill at (15,4,17) and keeps the pair
  powered; no core code changed for this, and the
  geometry is now pinned by the Slice 5 / Slice 6
  placement assertions, not by the builder succeeding
  somewhere unspecified.
- Deliberately NOT in this step: any weapon damage,
  magazine, or reload change; any squad, doctrine,
  kind, price, or cap change; any Dynamo cost,
  distribution, or cadence change; any kinetic mill
  change; any loot, skill, or core health change;
  any enemy AI or targeting change; any movement
  mode or vertical movement by pitch; any save-format
  field (`Look` stays session state); a balance pass;
  any claim about Windows Unity verification (new
  checklist item 14 only) or the pending Slice 1
  director playtest gate.
- Next, in order: Slice 8 Step 3, demo scenario +
  documentation-only review.

## 0.38.0 — 2026-10-30 — Slice 8 Step 3: demo scenario + review

- Decision, on composition order: the demo faces +X
  throughout — yaw 90 walk, pitch -45 gather, same-aim
  build, level fire — rather than walking yawed and then
  turning back to +Z to interact. Turning back would
  still exercise both systems in one run, but it would
  leave the composed claim as "movement in one direction,
  interaction in another", which is exactly the Step 1
  asymmetry the slice was built to close. Facing +X for
  every action proves the two steps compose, not just
  coexist, and the one direction change in the run is
  the loaded fresh view walking +Z, which is itself a
  rule being proved, not a fallback.
- Decision, on the gather cell: (18,3,16) is reported,
  not corrected to the Step 2 cell (17,3,16). The walk
  moves the eye east before the ray is traced, so the
  descending ray's ground strike moves east with it;
  "correcting" the demo to (17,3,16) would mean moving
  the player back, aiming differently, or editing the
  cell by hand — any of which would falsify the run.
  The review names this geometry explicitly so a future
  reader does not treat the Step 2 cell as the composed
  cell.
- Decision, on the loaded view: the review states the
  fresh-view rule from the demo's construction (a fresh
  Simulation loads the save), and keeps it distinct from
  the in-place-restore rule (RestoreWorld keeps the
  current view). Conflating the two would claim either
  that loads reset views (false for in-place restore)
  or that loads preserve views (false for a fresh load).
  Both rules are true, for different load paths, and
  the demo proves the fresh-load one while the 0.28.0
  tests prove the in-place one.
- Decision, on session counters: the loaded counters
  resetting to 0 is reported as the rule, not as a loss
  of the kill. What survives a save is the world — wall
  present, enemy absent — not the pre-load counts, which
  describe the pre-load session by definition (0.14.0).
  Reporting "killed 1" for the loaded run would have
  been a category error, so the demo reports the loaded
  world state and the reset counters side by side.
- Decision, on no new checklist item: the demo adds
  none, following the Slice 7 review's rule. A checklist
  item is for an adapter binding or feel claim; the demo
  exercises no binding that items 13 (mouse turn + W/D
  feel) and 14 (aimed gather / build / fire feel) do not
  already cover, so adding one would inflate the
  checklist without adding a check. The review says this
  explicitly rather than leaving the absence unexplained.
- Decision, on the save-length coincidence: the Slice 8
  save (43,974 chars) equals the Slice 5 scenario's save
  length, and the changelog says so rather than letting
  a reader mistake equal length for equal content. Save
  length is a composition fingerprint within one scenario
  across runs, not an identity check across scenarios.
- Deliberately NOT in this step: any core system, any
  weapon / squad / Dynamo / mill / loot / skill / save
  change, any balance pass, any new checklist item, any
  claim about Windows Unity verification or the pending
  Slice 1 director playtest gate, and any Slice 9 scope.
- Next, in order: scoping Slice 9 as its own decision
  step, from the ranked candidates carried in the review
  (rest of automation tier 2 first, third kind and
  deferred doctrines second).

## 0.39.0 — 2026-10-30 — Slice 9 scope: electrical tier 2 (voltage, cable, destruction)

- Decision, on Slice 9 order: ladder + transformer first,
  cable distribution second, mismatch destruction third,
  demo + review fourth. The order is a dependency chain,
  not a preference list: a ladder without distribution can
  only mismatch a mill touching its own Dynamo, a
  distribution rule without a ladder can only carry the one
  voltage that already exists, and a destruction rule
  without either can never fire, because a single voltage
  delivered by face-adjacency alone can never mismatch.
  Each step therefore creates the condition the next step
  needs, and a failure in Step 4's composed demo can be
  attributed to one step's rule, not to three rules proved
  at once.
- Decision, on what Slice 9 closes and what it leaves:
  Slice 9 closes the tier 2 core — ladder, conversion,
  distribution, and the blueprint failure rule — against
  the one consumer that already exists, the Kinetic Mill.
  It does NOT close tier 2 widening: additional powered
  consumers or sources (a powered workstation, an
  alternator as a distinct source) are the first ranked
  Slice 10 candidate, not Slice 9 scope creep, because a
  second consumer would need its own baseline proved at
  the same time as the ladder, the cable, and the
  destruction rule. Recording the split here, in the scope
  and in the changelog, is what keeps "rest of tier 2"
  from silently shrinking to "whatever Slice 9 happened to
  build".
- Decision, on hypotheses vs decisions in the scope: every
  step question the scope lists — ladder count and names,
  Dynamo output level, transformer form and direction,
  cable form and network resolution, mill rating and
  undervoltage, salvage and cascade — is a hypothesis or
  an open question for its step, not a decision made in
  this document. The two places the scope does commit are
  deliberate and stated as such: the step order above, and
  the interim unmatched-voltage rule for Step 1 (fallback
  to kinetic cadence, destruction deferred to Step 3), so
  that Step 3's change from fallback to destruction is a
  recorded behaviour change with a rationale, in the Slice
  8 Step 2 manner, rather than a test quietly rewritten.
  If Step 1 finds destruction must already exist, it must
  say why Step 3 then exists at all and re-scope before
  building — the scope names that escape hatch rather
  than leaving Step 1 to smuggle Step 3's system in.
- Decision, on placement: new Slice 9 blocks are placed by
  the Slice 8 aimed builder, distinct by block type, not by
  a revived front-left / front-right / front-centre offset
  and not by a new placement rule per block. The offsets
  were retired for a reason — aim decides one cell and an
  offset silently moving it to another was exactly the bug
  Slice 8 closed — and a transformer or cable that reintroduced
  an offset would reopen it. Adapter keys for new block types
  are decided in their steps, following the T / Y precedent
  (aimed builder, suppressed while the picker is open).
- Decision, on save and display conservatism: the scope's
  starting hypothesis, not a decision, is that voltage and
  cable networks are derived at load from the blocks
  themselves, like powered state today, with no new saved
  flag — unless a step's design creates stored state (for
  example, a selectable transformer direction), which then
  follows the existing old-save / malformed-entry rules.
  The scope also warns Steps 1 and 3 against a display lie:
  a count that calls a mismatched mill "powered" before
  Step 3 destroys it would misstate the step's own rule, so
  each step must state what its HudState count means.
- Decision, on gaps carried unchanged: Windows Unity
  verification and the Slice 1 director playtest gate remain
  open and are not closed, reordered away, or answered by
  scoping; aim feel is a later Windows observation, not a
  Slice 9 retune; mesh tint / mesh refresh, the shared
  marker, and the free-deploy primitive remain named gaps
  from 0.30.0 / 0.33.0, not Slice 9 work. Scoping a slice
  adds a verification plan (checklist items per step), not
  a verification claim.
- Deliberately NOT in this step: any core system, any
  weapon / squad / Dynamo / mill / loot / skill / save
  change, any balance pass, any new checklist item beyond
  what the scope names as per-step decisions, any claim
  about Windows Unity verification or the pending Slice 1
  director playtest gate, and any Slice 9 system build —
  the systems are built one at a time, in scope order,
  starting next run.
- Next, in order: Slice 9 Step 1, voltage ladder and
  transformer, decided in that step.

## 0.40.0 — 2026-10-30 — Slice 9 Step 1: voltage ladder and transformer

- Decision, on the ladder: two levels, LV and HV, because
  two suffice and more would be unused names — the scope
  said to build two if two suffice, not four for the
  blueprint's initials. LV is the Dynamo's output and the
  mill's rating; HV exists only because a transformer can
  produce it, so Step 2 has a second voltage to carry and
  Step 3 has a voltage to punish. None is not a third
  level: it means no voltage is delivered.
- Decision, on the existing Dynamo: it outputs LV, stated
  in VoltageRules.DynamoOutput, so a world built in Slice
  5 keeps working and an old save's Dynamo — which carries
  no voltage field — loads at LV by rule, not by guess.
  Re-rating existing Dynamos would have changed every
  powered demo number for no system gain, and the scope's
  hypothesis was to keep them working; this step keeps it.
- Decision, on the transformer: one new block type,
  Transformer = 9, appended only, that flips its input
  voltage — LV in gives HV out, HV in gives LV out. A
  single bidirectional block, not a pair of one-way blocks
  and not a Dynamo mode, because direction from input
  needs no stored state: no new save field, no direction
  setting to load, malformed, or desync, and the same
  block proves both conversions. The ladder-ends question
  therefore has no undefined edge: each level converts to
  the other, and None in gives None out, so an unpowered
  transformer converts nothing and powers nothing.
- Decision, on transformer input precedence: a direct
  Dynamo beats a converted voltage, for the mill and for
  the transformer itself. A mill touching both a Dynamo
  and an HV-outputting transformer receives LV, so adding
  a transformer next to an existing powered pair never
  unpowers it; a transformer touching both a Dynamo and
  another transformer's output converts from the Dynamo.
  Without that rule, placement order would decide the
  voltage by scan accident. A ring of transformers with no
  Dynamo reports None, proved by a test that also proves
  the recursion terminates, via a visited set.
- Decision, on mill voltage acceptance in this step: LV
  powers at the every-tick cadence, anything else does
  not power. IsPowered is therefore redefined, not
  replaced: it now means "delivered voltage equals the
  mill's rated LV", and the powered and kinetic phases
  keep their old split through it, so an HV-fed mill
  falls back to kinetic cadence (1 in 5 ticks, proved)
  exactly as the scope's interim rule requires, and a
  Dynamo-against-mill pair behaves exactly as before.
  Destruction is NOT decided here: Step 3 replaces this
  fallback for overvoltage and must name these tests as
  the changed behaviour, in the Slice 8 Step 2 manner.
- Decision, on what the transformer buys: 5 EmberOre, the
  same order as a Dynamo, so electrical builds keep
  competing with workstations and squads for ore, not
  with walls and mills for Stone and Wood. The double
  cost is stated openly: a transformer alone powers
  nothing, and what the second 5 ore buys is the second
  voltage itself — the only way to produce HV, which a
  second Dynamo cannot give — plus, already in this step,
  indirect LV one cell further out, through a Dynamo ->
  transformer -> transformer -> mill round-trip that runs
  the mill at the every-tick cadence, proved by test.
- Decision, on save and display: voltage is derived at
  load from the blocks and their neighbours, like powered
  state before it, so no new saved flag exists and a
  loaded transformer chain re-derives HV or LV on its
  first tick, proved by test. HudState keeps one power
  count and no voltage split: PoweredMills now counts
  mills receiving rated LV, and an HV-fed mill counts as
  NOT powered, because it runs at kinetic cadence — a
  count that called it powered would be a display lie
  about the cadence the count describes, and Step 3 will
  destroy it. The count's meaning is stated in code, not
  left for a reader to guess from the old wording.
- Decision, on placement and adapter: the transformer is
  placed by the existing aimed builder (new
  BuildTransformerAhead), distinct by block type, with no
  revived offset and no placement rule of its own — the
  Slice 8 reason for retiring offsets applies unchanged.
  Adapter key U, next to T and Y so the electrical trio
  sits together, suppressed while the picker is open for
  the same ore reason as Y (a transformer key while
  picking could spend the ore a pending Confirm needs).
  Playtest-gate checklist item 15 covers its key feel,
  Windows-only, not claimed verified.
- Deliberately NOT in this step: cable distribution
  (Step 2 — power still travels by face-adjacency only,
  plus transformer hops, with no network walk); any
  destruction rule (Step 3); a second consumer or source;
  a per-hop loss or length limit; a HUD voltage line; a
  mesh tint or refresh hook; any change to kinetic
  behaviour, matched-voltage cadence, Dynamo cost, squad,
  loot, skill, or core health; and any claim about
  Windows Unity verification or the pending Slice 1
  director playtest gate.
- Next, in order: Slice 9 Step 2, cable distribution,
  decided in that step.

## 0.41.0 — 2026-10-30 — Slice 9 Step 2: cable distribution

- Decision, on the cable: one new block type, Cable =
  10, appended only, solid and not gatherable like a
  Dynamo, so no mill ever excavates its own wiring, and
  placed by the existing aimed builder one cell at a
  time, with no drag-to-lay mode — a multi-cell placement
  mode is a new interaction, not a cable rule, per the
  scope. A radius around a Dynamo or a property of
  existing blocks was refused: distribution that costs
  nothing and occupies nothing is a longer face-adjacency
  rule wearing a name, not a wiring system.
- Decision, on cable cost: 1 EmberOre per cell, so a run
  is priced by its length and electrical builds keep
  competing with workstations and squads for ore, not
  with walls and mills for Stone and Wood, per the Step
  1 pricing rationale scaled to a per-cell block. 5 ore
  per cell was refused: a short run would then cost more
  than the Dynamo that sources it. 1 ore is a first-pass
  value for determinism and testability, not a balance
  claim, and the run that proves it is priced in the
  tests, not assumed.
- Decision, on how voltage travels: a deterministic
  network walk over face-adjacent Cable cells, face only,
  no diagonals, with no loss per hop and no length limit
  in this step. A loss or limit in the same step as the
  network itself would have made a failed delivery
  ambiguous (too far, or too lossy, or unconnected?),
  and the scope required a boundary test for either —
  the ten-cable LV test is the no-limit proof in the
  other direction. Only Cable cells conduct: a wall,
  workstation, or mill between cables breaks the network,
  proved, rather than passing voltage through silently.
  Chaining through another mill was therefore refused
  again, as in Slice 5: a mill is a consumer, not a wire.
- Decision, on what direct face-adjacency still means:
  Dynamo-against-mill still powers exactly as in Slice 5,
  and cables extend that rule, not replace it — proved
  by the existing direct tests passing unchanged and by
  a cable-fed mill running at the same every-tick
  cadence. A mill face-adjacent to an energised cable
  runs at the cable's voltage, through the same powered
  phase, with the same session counters.
- Decision, on network voltage resolution: one voltage
  per connected network, and a network touched by two
  distinct voltages carries None — not the higher voltage,
  and not the first voltage the scan reaches. Higher-wins
  was refused because it would silently discard one source
  and let a mismatched mill be destroyed in Step 3 for a
  wiring error the player could not see as a conflict;
  first-in-scan-order was refused because determinism
  without a stated rule is still an accident with a test
  around it. None is visible: the mills on a conflicted
  network fall back to kinetic cadence in this step, and
  removing one source resolves the network on the next
  check, proved, because voltage is derived, never stored.
  The same rule applies at a single mill: a mill touching
  an LV cable and an HV transformer output at once
  receives None, not the first neighbour's voltage.
- Decision, on the deliberate precedence exception:
  direct-cell precedence from Step 1 survives unchanged
  alongside the conflict rule, and the difference is
  stated, not hidden. A single mill or transformer touching
  a Dynamo directly still takes the Dynamo's LV before
  any converted or cabled voltage, because that is the
  Step 1 rule every powered demo depends on and changing
  it would unpower existing pairs. A shared cable network
  is different from a single consumer: it has no single
  cell for a direct source to decide, so its two sources
  conflict instead of one winning. Both rules are proved
  by tests in this step, so a reader can see where each
  applies.
- Decision, on transformer placement in a network: a
  transformer sits in a cable run as a boundary between
  two cable components, not as a conductor joining them.
  Its output is computed with the component it feeds
  excluded as its input, and a component's voltage is
  computed with the transformer it is fed by excluded as
  its source — the two exclusions are what stop a
  transformer feeding itself through a cable and what
  stop it overwriting its source side. Proved in one run:
  the source-side mill still runs at LV while the mill
  beyond the transformer runs at HV. Step 1's
  face-adjacency transformer rule needed no extension
  for this: the same flipped-output rule, applied through
  cable components, is what places it in the run.
- Decision, on save and display: cable networks are
  derived at load from the blocks themselves, so no
  network field is saved and a loaded cable run powers on
  its first tick exactly as a loaded Dynamo pair does,
  proved for both LV and HV runs. HudState gains no cable
  or energised-network count in this step: the existing
  rated-voltage powered-mill count is enough to prove a
  cable-fed mill, and an energised cable feeding no mill
  shows nothing — carried as a named gap, not claimed as
  visible, because a display that showed nothing for an
  energised run would otherwise be an untestable claim.
- Decision, on placement and adapter: cable is placed by
  the existing aimed builder (new BuildCableAhead),
  distinct by block type, with no revived offset and no
  placement rule of its own. Adapter key I, next to U so
  the electrical row runs T / Y / U / I, suppressed while
  the picker is open for the same ore reason as Y and U
  (a cable key while picking could spend the ore a pending
  Confirm needs). Playtest-gate checklist item 16 covers
  its key feel, Windows-only, not claimed verified.
- Deliberately NOT in this step: mismatched-voltage
  destruction (Step 3 — an HV-fed mill still falls back
  to kinetic cadence under the Step 1 interim rule, and
  a conflicted network's mills do the same); a per-hop
  loss, a length limit, or a drag-to-lay mode; additional
  powered consumers or sources; a HUD cable count; a mesh
  tint or refresh hook; any change to the ladder and
  conversion rules from Step 1, to kinetic behaviour, to
  matched-voltage cadence, to Dynamo cost, or to squad,
  loot, skill, or core health; and any claim about
  Windows Unity verification or the pending Slice 1
  director playtest gate.
- Next, in order: Slice 9 Step 3, mismatched-voltage
  destruction, decided in that step.

## 2026-10-30 — Slice 9 Step 3 (0.42.0): mismatched-voltage destruction

- Decision, on the rating and what counts as a mismatch:
  the Kinetic Mill is rated LV only, and a mismatch is a
  delivered voltage that is neither None nor LV — on this
  two-level ladder, exactly HV. The scope's starting
  hypothesis is kept: overvoltage destroys, undervoltage
  does not, because a machine fed too much breaks and a
  machine fed too little idles. The ladder collapses the
  undervoltage question further: there is no delivered
  voltage below LV, so undervoltage is not a separate
  delivered level to decide, it is the None state — no
  source, a broken cable, or a mixed-voltage conflict —
  and None mills keep the kinetic cadence they always had,
  proved for both a conflicted mill and an unpowered mill
  over 5 ticks. A rating that every delivered voltage
  violates would not be a game, and a rating no voltage
  can violate would not be a rating; LV-only with HV as
  the one violator is the smallest rating that is both.
- Decision, on destruction semantics: a mismatched mill is
  removed on the tick its mismatched voltage is delivered
  — its cell becomes Air — in a destruction phase that
  runs every tick, in chunk-index scan order, before
  either automation phase. Before, not after, is the
  deliberate part: destruction after the kinetic phase
  would let a mismatched mill gather once on every 5th
  tick before dying, which would be a failure rule that
  still pays the player for wiring wrong one tick in
  five; destruction first means the mill excavates
  nothing on its destruction tick, proved on an ordinary
  first tick and on a 5th tick set up with RestoreTick(4).
  Scan order is the same y, then z, then x order as the
  automation phases, so two mismatched mills resolve
  deterministically and a reader can reproduce which
  went first. IsPowered is unchanged — an HV mill is
  still "not powered" — but that state is now transient:
  the mill is destroyed later in the same tick, so no
  test or HUD line should ever treat "HV, not powered"
  as a cadence, and the two Step 1 / Step 2 tests that
  did are rewritten here and named in the changelog.
- Decision, on salvage: none. Nothing of the mill's
  10 Stone + 5 Wood returns to the inventory on
  destruction, and nothing returns on load either, proved
  in both cases with an inventory snapshot. A refund
  would make destruction a tax the player can ignore by
  rebuilding for free, and a partial refund would be a
  second economy rule smuggled into a failure rule; the
  honest first-pass cost of a wiring mistake is a whole
  mill, and a rebuild costs a whole mill again. This is
  labelled first-pass, not final balance, like every
  other price in this slice — a later balance pass may
  revisit it, but only as its own decision with its own
  tests, not by quietly softening this rule.
- Decision, on cascade: none. A destroyed mill's cable,
  transformer, Dynamo, and neighbouring mills all survive,
  proved in a single-branch, a two-branch, and a
  side-by-side world. The scope's starting hypothesis is
  kept: a failure rule that eats its own source would
  punish the player for building the source correctly,
  and a destroyed consumer is not a conductor, so its
  removal cannot break the network that fed it — the
  surviving branch in the branch test keeps gathering
  at powered cadence after its sibling is destroyed.
  Each mill is judged on its own delivered voltage on
  the same tick, so a branch can lose one mill and keep
  the other without any special "branch survives" rule.
- Decision, on the transformer's own rating: a transformer
  has none to violate, because it converts both levels —
  LV in gives HV out and HV in gives LV out, so both
  delivered voltages are valid inputs to it, not
  overvoltages. A transformer fed HV therefore survives
  and converts, proved by the round-trip world, where
  the second transformer is fed HV and still delivers
  LV to its mill. Giving the transformer a single rated
  voltage would contradict the Step 1 decision that made
  it one bidirectional block, and inventing a rating for
  it here would be a second failure rule in a step that
  exists to prove one. Cables likewise have no rating:
  they are voltage-blind carriers, proved by every cable
  surviving its mill's destruction, including the cable
  beyond the transformer in the branch test.
- Decision, on save and display: destruction is world
  state in the chunk bytes, so a destroyed mill stays
  destroyed across save/load with no resurrection and no
  refund on load, proved, while only the new
  DestroyedMills session counter resets on load, like the
  gather counters — a loaded world starts its destruction
  count at 0 even though its destroyed mills stay gone,
  and that split is stated, not hidden, because a counter
  that survived load would double-count a destruction
  that already happened. No HUD destruction line is added
  in this step: the existing power line already drops,
  because a destroyed mill leaves TotalMills, proved
  (2 mills / 1 powered before, 1 mill / 1 powered after),
  and a line that merely repeated the session counter
  would be a second display for one fact. The scene
  question is not claimed: the mesh-refresh gap from
  0.30.0 means a counted destruction may precede its
  visible disappearance in Unity, so playtest-gate
  checklist item 17 covers that disappearance,
  Windows-only, not claimed verified, and no review may
  claim the scene shows destruction until that item is
  completed there.
- Decision, on the adapter: no new binding at all, by the
  scope's starting hypothesis. Destruction fires on a
  tick, not on a key press, so there is nothing to bind,
  and a warning flash, sound, or marker before
  destruction would be a second feature — fairness
  signalling — not part of this core rule; if a later
  step finds destruction unfair without warning, it must
  name that work as its own adapter step with its own
  checklist item, not retrofit it here. The electrical
  key row therefore stays T / Y / U / I, and checklist
  item 17 is a scene-observation item, not a key item.
- Deliberately NOT in this step: a HUD destruction line
  or counter display; a pre-destruction warning; a
  partial or full salvage refund; any cascade to cables,
  transformers, Dynamos, or other mills; a rating for
  transformers or cables; a third voltage level to make
  undervoltage a delivered level rather than None; any
  change to the ladder, conversion, or cable rules from
  Steps 1 and 2 except the acceptance change this step
  itself justifies (the HV kinetic fallback is replaced
  by destruction, with the changed tests named openly);
  any change to kinetic behaviour, matched-voltage
  cadence, Dynamo cost, or squad, loot, skill, or core
  health; and any claim about Windows Unity verification
  or the pending Slice 1 director playtest gate.
- Next, in order: Slice 9 Step 4, demo scenario +
  documentation-only review, decided in that step.

## 0.43.0 — Slice 9 Step 4: demo scenario + documentation-only review

- Decision, on composition: one Simulation proves the
  ladder, the cable, and the destruction rule together,
  not separately, as the scope's Step 4 requires — an LV
  cable mill, a round-trip LV mill, an HV cable mill
  destroyed on tick 1, and a kinetic mill, in four rows
  on the y=20 plane, through a save/load round-trip and
  a loaded 5-tick phase. Four rows, not four separate
  worlds, is the deliberate part: only one run can show
  that a destroyed mill's neighbours keep gathering, that
  the HUD total drops while its powered count holds, and
  that a loaded world re-derives power for the survivors
  while keeping the destroyed mill gone — separate worlds
  proved each of those already in Steps 1-3, and repeating
  them separately would prove nothing new.
- Decision, on the scope-wording correction: the scope's
  Step 4 said the second mill "runs at the converted
  voltage", and after Step 3 that sentence cannot be
  taken literally, because a mill delivered HV is
  destroyed on its delivery tick. This step does not
  quietly rewrite the scope or pretend HV powers a mill:
  it proves converted voltage two honest ways and says
  so in the demo, the tests, and the review — the
  round-trip mill survives on HV converted back to LV,
  and the HV cable mill is destroyed by HV itself. A
  future reader who quotes the scope sentence alone gets
  the correction in the same step's documents, not a
  contradiction discovered by running the demo.
- Decision, on staging: the demo stages its four-row
  layout directly with TrySet, exactly as the Step 1-3
  tests stage theirs, rather than placing each block by
  aim. Aim placement of a transformer and of a cable
  were each proved in their own steps'
  `BuildTransformerAhead` / `BuildCableAhead` tests, so
  walking four aim placements here would prove walking,
  not wiring, and would risk a geometry error masking
  as a wiring failure. The review states this staging
  openly and claims composition of the core rules only,
  not aim placement of this exact layout.
- Decision, on the review verdict: Slice 9 is reviewed
  and closed as tested electrical tier 2 core (ladder,
  transformer, cable, destruction) plus written adapters
  — not as playtested, balanced, or Unity-verified.
  The claim split is kept split in words: core rules
  are tested, adapter keys (U, I) and the rated-voltage
  HUD count are written and checklist-listed (items 15,
  16, 17), and neither claim borrows the other's
  evidence. No new checklist item is added for the demo
  itself, because it exercises no adapter binding those
  items do not already cover, following the Slice 7 and
  Slice 8 reviews' no-new-item rule.
- Decision, on gaps and candidates: the review carries
  the gaps in priority order — Windows Unity verification
  (Slices 1-9), the pending Slice 1 director playtest
  gate, electrical balance as unproven first-pass values,
  destruction with no warning by decision, mesh tint and
  mesh refresh, an energised cable feeding no mill
  showing nothing, two kinds sharing one marker, and the
  free-deploy primitive — and carries the Slice 10
  candidates in the 0.39.0 order (additional powered
  consumers and sources first; a third squad type and
  the deferred doctrines second), because the Step 4
  evidence gives no reason to reorder them. Slice 10 is
  NOT scoped by this review, by the same rule as every
  review before it: scoping is its own decision step,
  chosen with the composition evidence in hand.
- Deliberately NOT in this step: any new core system;
  any change to the ladder, conversion, cable, kinetic,
  cadence, Dynamo cost, or squad, loot, skill, or core
  health rules; any save-format field; a pre-destruction
  warning retrofitted as a core rule; a mesh tint or
  mesh refresh; a HUD cable or destruction line; any
  claim about Windows Unity verification or the pending
  Slice 1 director playtest gate.
- Next, in order: scoping Slice 10 as its own decision
  step, decided in that step.

## 0.44.0 — Slice 10 scope: electrical widening

- Decision, on the slice theme: Slice 10 is electrical
  widening, not electrical rework — one second powered
  consumer, then one distinct power source, closed by a
  composed demo and review. The Slice 9 review's top
  candidate becomes the slice, and the rest (third kind,
  deferred doctrines) become the ranked Slice 11
  candidates, in the same order, because the Step 4
  evidence gives no reason to reorder them. Widening
  was chosen over the third kind precisely because Slice
  9 proved its rules against one load and one generator:
  a ladder, a cable, and a destruction rule whose
  generality is still an assumption are the gap the
  next slice should close, before combat widens again.
- Decision, on the order: consumer first, source second,
  because each step creates the next step's proof burden.
  A source proved only against the mill would prove a
  generator, not a widening; a consumer proved against
  the Dynamo alone still leaves the source side singular,
  but it gives the source step a second load it must
  feed. The reverse order would let the source step pass
  on mill-only evidence and leave the consumer step to
  prove both systems at once. This order is a scoping
  decision about proof, not a claim about which system
  matters more.
- Decision, on the consumer hypothesis: a powered
  Workstation is the leading candidate, not the decision,
  because it is the only existing machine with a real
  effect power could strengthen without inventing a new
  production chain — but the scope names, rather than
  hides, the two facts that make it non-trivial: the
  workstation has no aimed builder and no adapter key
  (unlike every Slice 5 / Slice 9 machine, which has
  both), and its regen is player-centred (any
  workstation within Manhattan 3 of the player), not
  machine-centred. Step 1 must therefore decide the
  placement path and the meaning of "powered" for this
  machine, or choose a new machine instead with a
  recorded reason none of the existing machines fits.
  Recording the hypothesis and its obstacles together
  is deliberate: a scope that named the workstation
  without its missing key would let Step 1 inherit a
  placement gap as if it were already solved.
- Decision, on the rating question: the scope states
  openly that `IsPowered` and `IsMismatched` are
  mill-only and cannot be reused for a second consumer
  by accident. A scope that stayed silent on this would
  let Step 1 write a powered-workstation check that
  quietly reuses the mill's rating, and a reader could
  not tell whether the shared rating was decided or
  inherited. Step 1 must decide its consumer's rating
  and mismatch semantics (destroyed like a mill,
  disabled to unpowered behaviour, or unaffected) with
  tests, including the destruction phase order.
- Decision, on the source hypothesis: the blueprint's
  alternator is taken as a name only, not a design,
  because the blueprint names alternators without
  stating output, cost, or requirements. The scope
  therefore requires Step 2 to prove its source is not
  a Dynamo reskin — a source outputting LV at Dynamo
  cost through Dynamo placement would pass mill tests
  while adding nothing — and to state its output level
  and its place in `DeliveredVoltage` precedence, where
  a direct Dynamo today beats any converted or cabled
  voltage. Fuel is not assumed: a fuel rule is a new
  system, and Step 2 must justify it as its own decision
  with boundary tests if it wants one, rather than
  inheriting it from blueprint vocabulary.
- Decision, on the gaps carried: Unity verification and
  the director playtest remain open and are not closed,
  reordered away, or answered by scoping; balance
  remains a later pass with first-pass values only; the
  destruction warning, mesh tint, and mesh refresh
  remain named gaps, not Slice 10 work; the energised
  cable feeding no mill showing nothing is the one gap
  Slice 10 must reckon with directly, because its
  display question recurs for a powered workstation and
  a new source — so each step must decide its own
  display against the mill-only HUD count, rather than
  inheriting silence. The shared-marker and free-deploy
  gaps remain documentation, not enforcement.
- Deliberately NOT in this step: any new core system;
  any change to the ladder, conversion, cable, kinetic,
  cadence, Dynamo cost and output, or squad, loot,
  skill, or core health rules; any save-format field;
  a third consumer, a second source, tiers 3-4,
  crafting, a fuel system, or a per-machine inventory
  decided here rather than in their steps; a balance
  pass; a mesh tint or refresh; a pre-destruction
  warning; free-deploy enforcement; any claim about
  Windows Unity verification or the pending Slice 1
  director playtest gate.
- Next, in order: Slice 10 Step 1, one second powered
  consumer, decided in that step.

## 0.45.0 — Slice 10 Step 1: powered Workstation
- Decision, on the consumer: the existing Workstation
  block is the second powered consumer, not a new block
  type. A new machine would have needed a new cost, a
  new effect, and a new save question; the workstation
  already has a real, distinct, player-centred effect
  (shield regen within Manhattan 3), so power can
  strengthen an existing decision (build and defend a
  bench) instead of inventing a second one.
- Decision, on the rating: LV, as its own
  `WorkstationRated` constant. Same value as the mill
  by decision, not by inheritance, so a future rating
  change for one consumer cannot silently change the
  other, and the workstation's power checks state
  their own rating.
- Decision, on mismatch: HV disables, it does not
  destroy. A repair bench tripping offline on
  overvoltage is a recoverable wiring mistake; destroying
  the player's shield bench for the same mistake that
  destroys a mill would double-punish a support
  structure and remove the counterplay (regen) a player
  needs to survive the mistake. The workstation works
  again as soon as it is rewired to LV or left
  unpowered, with no rebuild and no salvage question.
  `IsMismatched` stays mill-only, so this rule cannot
  be confused with mill destruction.
- Decision, on the powered effect: base regen + 5 per
  tick, additive with the skill bonus, in the existing
  workstation phase. Power strengthens the rate, it does
  not replace it and does not stack with a second
  phase: a powered workstation regens once per tick,
  at the higher rate, not twice. It produces nothing,
  so no per-machine inventory is justified here.
- Decision, on placement and display: aimed builder
  (`BuildWorkstationAhead`, B adapter key) closes the
  workstation's missing aim rule, at the Slice 1 cost;
  the HUD gains a powered/total workstation count, with
  HV counting as NOT powered, so a disabled workstation
  cannot be displayed as powered.
- Collision, stated openly: while this step was being
  implemented directly, a scheduled run implementing
  the same step in the same shared working copy
  appeared, with the opposite mismatch decision (HV
  destroys the workstation, doubled regen). That run
  stood down and reverted all of its own edits, and
  its design was NOT adopted or merged — the two
  mismatch rules contradict each other, and only one
  may land. This step is the disable design, proved by
  22 new tests; the destruction design is recorded
  here as rejected for this consumer, not lost.
- Deliberately NOT in this step: a new block type, a
  new voltage level, a second source, a per-machine
  inventory, a balance pass, a pre-destruction warning
  for workstations (there is no destruction to warn
  about), mesh tint or refresh, or any claim about
  Windows Unity verification or the pending Slice 1
  director playtest gate.
- Next, in order: Slice 10 Step 2, one distinct power
  source, decided in that step.

## 0.46.0 — Slice 10 Step 2: Alternator, second power source
- Decision, on the source: the Alternator, under the
  blueprint's name, as a native HV generator — a new
  block type, Alternator = 11, appended only. The name
  is kept because the design finally gives it a meaning
  the blueprint never stated: HV is no longer only a
  transformer's flip of Dynamo LV, it has a generator
  of its own, so transformer step-down becomes a
  primary path (Alternator -> transformer -> consumer)
  instead of a round-trip curiosity (Dynamo ->
  transformer -> transformer -> mill). An LV-outputting
  second source was rejected as a reskin risk: at
  Dynamo cost it would have been a Dynamo under a new
  name, and at a merely different cost it would still
  have been proved by the same LV tests a Dynamo passes.
  HV output is proved by tests a Dynamo cannot pass:
  direct feed destroys a mill and disables a
  workstation, while step-down feed powers both.
- Decision, on requirements: always-on while placed,
  like a Dynamo. No fuel, spin-up, terrain, or
  placement condition is invented here. A condition
  would have made every failed delivery ambiguous (no
  fuel, unwired, or conflicted?) and would have been a
  second system smuggled into a source step; the scope
  hypothesis was always-on, and nothing in this step's
  evidence overturned it. The source's distinctness
  therefore rests on output level and cost, both
  tested, not on a condition.
- Decision, on cost: 10 Stone, no EmberOre. This is
  the mirror image of the Dynamo's 5 ore, chosen so
  the two sources compete for different resources: an
  Alternator competes with walls and mills for stone,
  a Dynamo with workstations and squads for ore. It
  also gives the Alternator its one staging advantage:
  it can be placed before the player has any ore.
  Stated openly, as a first-pass value, not a balance
  claim: safe use still needs a transformer's 5 ore,
  so the safe-use ore total ties a Dynamo's 5 ore and
  the Alternator costs 10 stone on top — it is not a
  cheaper Dynamo, and a player who already has ore
  and wants safe LV power has no ore reason to prefer
  it. Its reasons are native HV, stone staging, and
  resource-pool choice, and a balance pass may still
  find those reasons too weak or too strong.
- Decision, on precedence: "direct source" is now
  Dynamo or Alternator together, not Dynamo alone.
  A direct source still beats any converted or cabled
  voltage, whichever source it is, so a mill touching
  a direct Alternator and an LV-outputting transformer
  receives HV and is destroyed — the direct rule is
  source-neutral, not a Dynamo privilege carried over
  by accident. Two distinct direct sources touching
  one cell, one transformer, or one cable network
  resolve to None, not to a Dynamo-beats-Alternator
  rule: a silent Dynamo win would have hidden a wiring
  conflict, and a silent Alternator win would have
  hidden it the other way. None is the existing
  visible-dead outcome for mixed voltages everywhere
  else in this system, and a conflicted consumer is
  never destroyed, because no delivery is not a
  mismatch — the same rule that already protects a
  conflicted mill in Slice 9 Step 2.
- Decision, on proof: both consumers, both delivery
  paths, both failure rules. Face-adjacent step-down
  powers a mill (5 in 5 ticks) and a workstation
  (base + 5 regen); cable step-down powers a mill and
  a workstation the same way; direct HV destroys a
  mill (face-adjacent and through cable) and disables
  a workstation (face-adjacent), with the workstation
  surviving and the mill's pocket surviving. A source
  step proved against the mill alone would not have
  been a widening, and this step's 82 tests are
  counted against that burden, not against the mill.
- Decision, on placement and display: aimed builder
  (`BuildAlternatorAhead`, N adapter key, suppressed
  while the picker is open like every other builder,
  checklist item 18, Windows-only) — N is free, and
  the picker guard is kept uniform even though an
  Alternator spends stone, not ore, so it could not
  break a pending Confirm's affordability: one modal
  rule for all builders, not a per-resource exception.
  No new HUD source count or line, by the Dynamo
  precedent: sources are not counted, consumers are,
  and the existing powered-mill and powered-workstation
  counts already prove the Alternator feeding both.
  Stated openly: a lone Alternator feeding no consumer
  shows nothing, the same named gap as an energised
  cable feeding no mill, carried forward rather than
  fixed by inventing a source indicator in this step.
- Observed, not fixed here: the Step 1 workstation key
  (B) has no playtest-gate checklist item, while every
  other builder key (T, Y, U, I, and now N) does. That
  is a documentation gap in the gate doc, not a core
  rule, and closing it belongs to a step that touches
  the gate doc for its own reason — bundling it here
  would have made this source step also a gate-doc
  repair step, and the gap is recorded here so it is
  not lost.
- Deliberately NOT in this step: a new voltage level,
  a fuel or condition system, a per-hop loss, a cable
  length limit, drag-to-lay, a third consumer, a second
  source, a balance pass, a pre-destruction warning,
  mesh tint or refresh, or any claim about Windows
  Unity verification or the pending Slice 1 director
  playtest gate.
- Next, in order: Slice 10 Step 3, demo scenario +
  documentation-only review, closing Slice 10.

## 0.47.0 — Slice 10 Step 3: demo scenario + review, closing Slice 10
- Decision, on composition: one Simulation, both
  consumers, both delivery paths, both failure rules,
  both baselines — not two demos and not a demo that
  proves the source against the mill alone. The mill
  rows use the cable step-down path for the powered
  mill (Alternator -> cable -> transformer -> cable ->
  mill), because Step 2 proved that path for the mill
  and the face-adjacent step-down path for the
  workstation, and the demo's job is to compose the
  paths, not to re-prove each path in isolation; the
  powered workstation therefore uses face-adjacent
  step-down, so the demo contains one of each path and
  neither path is left as a Step 2 claim the review
  never composes. The direct-HV mill and the direct-HV
  workstation sit in the same run so the two mismatch
  rules — destroy vs disable — are visible side by
  side on the same tick, not in separate runs where a
  reader could mistake one rule for the other.
- Decision, on the bench walk: workstation regen is
  player-centred (Manhattan distance 3), so one player
  position cannot prove three benches at once — a
  powered bench near the player masks an unpowered
  bench near the same player, by the
  `EffectiveWorkstationRegen` rule itself (any powered
  near bench wins). The demo therefore walks the
  player between benches with `MoveTo` (each returning
  True, into Air cells above the y=4 tree layer) and
  reads each bench's regen at its own position, then
  reads the powered bench again after the walk to
  prove the reading follows the bench, not the walk.
  This is recorded as a measurement procedure, not a
  gameplay claim, and the tests assert the same
  readings: no sentence in the demo, the tests, or the
  review claims three benches were proved
  simultaneously at one position. A demo that parked
  the player once and quoted three regen numbers would
  have been quoting one number three times.
- Decision, on staging: the layout is staged directly
  with TrySet, exactly as the Step 1-2 tests stage
  theirs, and the bench walk is a player-position walk,
  not a block-placement walk. Aim placement of a mill,
  a workstation, a transformer, a cable, and an
  Alternator was each proved in its own step's tests;
  walking this layout in by aim would have proved
  walking, not wiring, and the review says so openly
  rather than letting the demo borrow aim-placement
  evidence it does not contain.
- Decision, on the checklist: no new playtest-gate
  checklist item for the demo itself, by the same
  no-new-item rule the Slice 7, 8, and 9 reviews
  applied — the demo exercises no adapter binding the
  checklist does not already cover. The one exception
  is named, not fixed: the B key still has no checklist
  item, while T, Y, U, I, and N each have one, so a
  Windows checker following the gate doc would never
  be asked to press B. That is a gate-doc gap, and a
  review step is the wrong place to repair a gate doc
  it does not otherwise touch; the gap is recorded in
  the review as gap 7, so the gate doc is not quoted
  as complete for Slice 10 while it lacks B, and the
  next step that touches the gate doc for its own
  reason closes it.
- Decision, on the verdict: Slice 10 is closed as
  tested electrical widening plus written adapters, not
  as playtested, balanced, or Unity-verified, and
  Slice 11 is NOT scoped by this review. The scope's
  ranked candidates are carried unchanged (third squad
  type and deferred doctrines first, further electrical
  widening second) because the Step 3 evidence gives no
  reason to reorder them — and scoping Slice 11 now,
  inside a review, would have been guessing with the
  composition evidence in hand rather than deciding
  with it, the same reason the Slice 9 review did not
  scope Slice 10.
- Deliberately NOT in this step: a new core system of
  any kind, a balance pass, a pre-destruction or
  pre-disablement warning, a mesh tint or refresh, a
  display for live networks feeding no consumer, a
  B-key checklist repair, or any claim about Windows
  Unity verification or the pending Slice 1 director
  playtest gate.
- Next, in order: scoping Slice 11 as its own decision
  step, from the ranked candidates carried in
  `docs/slice-10-review.md`.

## 0.48.0 — Slice 11 scope: squad widening, kind then doctrine then review
- Decision, on theme and order: Slice 11 takes the
  Slice 10 review's top-ranked candidate (a third
  squad type and the deferred doctrines) and orders
  it kind first, doctrine second, demo + review
  third. Kind first because its questions are the
  smaller, already-patterned ones — a shape in
  `SquadShapes.For`, a picker key, a price/cap
  answer, a marker answer — proved once by Slice 7
  Step 1, while the doctrine step must answer
  movement and engagement questions that interact
  with every kind it serves. Ordering doctrine
  first would force that step to prove itself on two
  kinds and then re-prove itself on three after the
  kind step; ordering kind first makes the doctrine
  step inherit the full field. The two widenings are
  independent axes (kind = who, doctrine = order),
  so neither step may smuggle the other in as an
  extra.
- Decision, on the kind pool: heavy and flyer only,
  carried from Slice 7's pool with Recon spent, or
  another original role only if the step records why
  neither fits. The scope does not pick between
  heavy and flyer, because that choice is the step's
  evidence question — what tactical choice does it
  add that Vanguard and Recon cannot make — not a
  scope vote. The shared one-voxel-per-tick movement
  rule is a scope hypothesis, not a decision: a
  flyer choice must confront it directly, with tests
  for targeting and occupancy, rather than asserting
  flight as a stat.
- Decision, on the doctrine pool and hypothesis:
  Defend Core and Patrol only, carried from Slice 4's
  deferred pair. Defend Core is the starting
  hypothesis because the core objective already
  exists at (28,5,28) with 100 HP and no squad today
  orders itself to it, while Patrol needs a route
  definition the game does not yet have. The
  hypothesis is explicitly reversible: if Defend Core
  collapses into HoldGround at the core cell, or
  Patrol's route is already answered by the rally
  picker, the step must say so and choose otherwise
  with evidence. The doctrine step must also prove
  itself against more than one kind, because a
  doctrine proved only on Vanguard in a slice that
  just added a third kind leaves the pairing unproved.
- Decision, on the B-key gap: gap 7 from the Slice 10
  review is assigned to Step 1, not left floating and
  not fixed by this scope document itself. The 0.47.0
  rule was that the next step touching the gate doc
  for its own reason closes it; Step 1 adds a
  checklist item for its own kind key, so Step 1 is
  that step. Naming the assignment here, rather than
  in Step 1 alone, keeps the gap from being quoted as
  closed by a scope that adds no checklist item
  itself. The shared-marker gap (gap 8) is likewise
  named as a Step 1 decision, because a third kind
  grows it from two kinds sharing a marker to three.
- Decision, on the binding question: the scope names
  the collision openly instead of leaving it to the
  steps to discover — doctrines occupy Alpha1 /
  Alpha2 and kinds Alpha3 / Alpha4 today, so a third
  kind and a third doctrine cannot both take "the
  next number key" without colliding. Each step must
  name a binding that does not collide while picking;
  Alpha5 is noted as the next free number key for
  Step 1 to confirm, not as a decided binding.
- Deliberately NOT in this scope decision: picking the
  kind, picking the doctrine, changing any existing
  shape, price, or cap, adding a fourth kind or a
  second doctrine, any electrical widening, any
  balance pass, any warning / tint / refresh system,
  any camera, any save-format change, or any claim
  about Windows Unity verification or the pending
  Slice 1 director playtest gate.
- Next, in order: Slice 11 Step 1, exactly one third
  squad kind, decided in that step.

## 0.49.0 — Slice 11 Step 1: third squad kind — Heavy, close-range anchor
- Decision, on the kind: Heavy, from the scope's
  heavy / flyer pool, not a fourth pool name and not
  a second kind as a bonus. What Heavy adds that
  Vanguard and Recon cannot already make is a squad
  that survives contact: 100 total HP against
  Vanguard's 90 and Recon's 40, proved by a
  95-damage hit that kills Vanguard outright and
  leaves Heavy at 5 HP with 1 member. Slice 7's
  heavy rejection is answered, not ignored: then
  the gap was a screen and a tougher anchor was
  Vanguard with more HP; now the gap is the anchor
  itself, and the costs below are what keep Heavy
  from being Vanguard with more HP.
- Decision, on the shape: 2 members, 50 HP each,
  8 damage each, engagement range 2 — full health
  100, full damage 16 per tick. Heavy is better
  than Vanguard in exactly one dimension
  (survivability, total and per-member) and worse
  in two (range 2 against 3 and 5; damage 16
  against 30 and, for a Vanguard comparison, barely
  half), mirroring Recon's proof structure, so it
  is a choice, not a replacement. A Heavy fires
  only at range 2: at range 3 it is silent where a
  Vanguard kills a 30-HP enemy, and its damage
  drops in member steps (100 -> 50 HP is 1 member
  dealing 8), like the other kinds. First-pass
  values for determinism and testability, not final
  feel, exactly as Vanguard's and Recon's numbers
  were — no balance pass rides along.
- Decision, on flyer, rejected again with the
  reason: a flyer is not a stat, it is a movement
  rule (ignoring solids, or moving in the air)
  with targeting and occupancy consequences — a
  flyer inside a wall cell, for example, is a
  position an enemy cannot step into — and proving
  those consequences inside a kind step would make
  the kind untestable in isolation. Flyer remains
  a later-step candidate, not a rejected idea.
- Decision, on movement: the shared
  one-voxel-per-tick rule, unchanged. Heavy marches
  under HoldGround and hunts under Advance exactly
  like the other kinds, with the same step refusals
  (solid and enemy-occupied cells, out of bounds),
  proved by a march test and an Advance hunt test.
  No new movement mode rides along.
- Decision, on shape representation: a per-kind
  shape in `SquadShapes.For`, appended only, not a
  second set of shared constants and not a branch
  in every rule. Vanguard's and Recon's numbers are
  unchanged, proved by shape-table tests. The load
  clamp stays per-kind (Heavy 100, Vanguard 90,
  Recon 40), so a corrupt save cannot create a
  super-squad and a kind is never trimmed to
  another kind's health on load.
- Decision, on price and cap: the settled economy
  holds — 10 EmberOre and a cap of 3 live squads
  in total, the same for Heavy as for Vanguard and
  Recon, so the picker's COST 10 / MAX 3 line stays
  true. A price difference would invent an economy
  distinction no balance evidence supports yet, and
  a per-kind cap would change the cap question
  without a step justifying it; both are refused
  here. A fourth squad across mixed kinds is
  refused at the total cap with no charge, proved
  by test, as before.
- Decision, on the adapter binding: Alpha5 selects
  Heavy while picking, after Alpha3 Vanguard and
  Alpha4 Recon. Alpha5 is confirmed free while
  picking: the picker keys were 1 / 2 / 3 / 4 plus
  Return / Esc / O, and no builder key fires while
  the picker is open, so 5 collides with nothing.
  Doctrine and kind stay independent: changing
  doctrine keeps Heavy selected, an unknown kind
  is rejected with the selection unchanged, and a
  Confirm deploys the shown combination. The third
  doctrine key in Step 2 cannot also take Alpha5;
  that collision is Step 2's binding decision, not
  a problem deferred silently.
- Decision, on display and the shared marker
  (gap 8): no new display line, because the
  headless display already prints kind, doctrine,
  members, health, damage, range, and activity, and
  the snapshot carries Heavy's shape like the other
  kinds'. The scene marker is deliberately the
  shared cyan cube, stated in SquadMarkerDisplay.cs
  and in the new checklist items — not inherited by
  accident. A per-kind marker distinction (tint or
  shape per kind) is a display system, and it stays
  a Slice 12 scoping candidate with the other
  visibility gaps, not a tint or refresh system
  smuggled into a kind step. Until then, kind is
  read from the text display line, not the marker,
  and that limit is stated openly.
- Decision, on the B-key gap (gap 7): closed by
  this step, as the 0.48.0 scope assigned it. Step 1
  touches the gate doc for its own checklist item
  (item 20, Heavy key), so it also adds the missing
  B-key item (item 19, workstation build, like the
  T / Y / U / I / N items), named here so the
  repair is a stated part of this step. No other
  Slice 10 gap is closed here: Unity verification,
  the director playtest, electrical balance,
  destruction / disablement warnings, mesh tint
  and refresh, and live-network display remain open
  and named, not fixed by a squad step.
- Deliberately NOT in this step: a second kind, a
  new doctrine (Step 2 work), any change to
  Vanguard's or Recon's numbers, a price or cap
  change, a movement rule, a targeting rule (no
  taunt, no kind priority — enemy targeting stays
  kind-blind, proved by a nearer-Heavy test), a
  balance pass, a warning / tint / refresh system,
  a camera, a save-format change, or any claim
  about Windows Unity verification or the pending
  Slice 1 director playtest gate.
- Next, in order: Slice 11 Step 2, exactly one
  deferred doctrine, decided in that step (Defend
  Core as the starting hypothesis, Patrol deferred,
  proved against more than one kind).

## 0.50.0 — Slice 11 Step 2: third doctrine — Defend Core, guard the objective core
- Decision, on the doctrine: Defend Core, from the
  scope's deferred pair (Defend Core / Patrol),
  not a third pool name and not a second doctrine
  as a bonus. What Defend Core adds that HoldGround
  and Advance cannot already make is an order to a
  thing, not a place the player picks and not a
  prey: HoldGround holds a rally the player picks,
  Advance hunts the nearest enemy anywhere, and
  neither answers "guard this thing". The
  hypothesis held on examination: Defend Core does
  not collapse into HoldGround at the core cell,
  because its movement is attacker-bounded — it
  leaves the post to chase and comes back, which
  HoldGround at the core never does — and Patrol's
  route question is not already answered by the
  rally picker, because a rally is one point, not
  a route, and inventing waypoints inside a
  doctrine step would make the doctrine untestable
  in isolation. Patrol is deferred, not rejected.
- Decision, on what "core" means: the world
  objective core, WorldZone.CoreX / Y / Z
  (28,5,28, 100 HP), read from the world on every
  tick, not a rally the player happens to pick
  near it and not a stored post. The post is the
  core cell itself: the squad stands on it, which
  the chunk allows because y = 5 is open air above
  the ground (y <= 3 stone, y = 4 sparse trees),
  and a guard whose post cell is ever solid holds
  short of it through the shared step refusals,
  exactly as a HoldGround squad holds short of a
  rally inside a solid block. The doctrine needs
  no new save field because the core coordinates
  are world constants, not per-squad data — the
  saved doctrine value alone re-derives the post
  on load.
- Decision, on the chase: a living enemy within 8
  Manhattan of the core is an attacker; one
  farther away is not, however near it stands to
  the guard. Distance is enemy-to-core, not
  enemy-to-squad, because the doctrine guards the
  core, not the guard's neighbourhood — an enemy
  next to a far-flung guard is not a threat to the
  core until it walks within the radius, and then
  it becomes one on the next tick, which is the
  honest behaviour, not a gap. 8 is a first-pass
  value for determinism and testability, not final
  feel: larger than every kind's engagement range
  (Vanguard 3, Recon 5, Heavy 2) so a guard can
  intercept before an attacker reaches the core,
  and far smaller than the spawn-to-core distance
  (24) so the perimeter is local. Nearest attacker
  wins, ties to the first enemy in list order, the
  same rule as fire, so chase, fire, and display
  never disagree about the attacker.
- Decision, on movement, one decided step per tick:
  with an attacker the guard steps toward the
  attacker; without one it steps toward the core.
  The choice is made once, in MoveForDefendCore,
  and a refused step ends the tick's movement —
  the guard does not then step toward the other
  target instead, because that would execute a
  second, different order in the same tick. The
  step itself is the shared MoveTowardPosition
  rule, so Defend Core cannot drift from HoldGround
  and Advance on axis choice, tie-break, or
  blocking — only the target differs. The rally is
  never a movement target under this doctrine: a
  Defend Core squad's rally is a deployment
  formality the picker still requires, and the
  contrast test (same rally, no enemies) proves
  the three doctrines diverge in one tick.
- Decision, on engagement: Defend Core fires
  whenever an enemy is in its kind's engagement
  range, Advance's rule, not HoldGround's
  hold-only rule. A post-only fire rule was
  considered and refused: it would let an attacker
  walk past a chasing or returning guard unshot,
  and would make the chase phase toothless until
  the guard reached the attacker, which is not a
  guard but a delayed HoldGround. The cost of the
  chosen rule is stated openly: a returning guard
  fires at a non-attacker in range, proved by test
  — but firing is not chasing, and its step stays
  core-ward, so the shot does not drag the guard
  off its post. At the post, an adjacent attacker
  cannot be stepped onto (enemy-occupied refusal),
  so the guard stays on the core cell and fires
  there, proved for Vanguard (kill) and Heavy (16).
- Decision, on kinds served: proved against all
  three kinds, above the scope's minimum (the new
  kind plus one existing kind), because a doctrine
  proved only on Vanguard in the slice that added
  Heavy would leave the Step 1 / Step 2 pairing
  unproved. Vanguard proves chase, kill, and
  return; Heavy proves at-post fire and range-2
  silence at range 3; Recon proves chase from
  radius 8 with silence beyond range 5. Pricing is
  settled, not reopened: Defend Core deploys
  through the paid path at 10 EmberOre under the
  3-live-squad cap, proved by a picker confirm
  test, with no new price invented.
- Decision, on display: Guarding / Hunting /
  Marching, derived in the snapshot from post and
  attacker, not from rally proximity. A Defend Core
  squad at the core with no attacker reads
  Guarding (not Holding, which is a rally word);
  chasing a core attacker reads Hunting with that
  attacker's cell (the same nearest-attacker rule
  as movement); away from the core with no
  attacker reads Marching, the return march. An
  enemy far from the core never makes a guard at
  post read Hunting, so the display cannot claim a
  chase the movement rule is not making. No new
  display line is added: kind, doctrine, members,
  health, damage, range, and activity already print.
- Decision, on the adapter binding: Alpha6 selects
  Defend Core while picking, after Alpha1
  HoldGround and Alpha2 Advance. Alpha6 is
  confirmed free while picking: the picker keys
  were 1 / 2 / 3 / 4 / 5 plus Return / Esc / O, and
  no builder key fires while the picker is open,
  so 6 collides with nothing. Step 2 cannot also
  take Alpha6 — the next doctrine, if any, must
  name its own non-colliding binding in its step.
- Decision, on the shared step fix: the shared
  squad step rule now returns false when already
  at its target instead of reporting a step to its
  own cell. This is a correctness fix to a return
  value, not a behaviour change for existing
  doctrines: HoldGround already returns false
  before calling the rule when at its rally, and
  an Advance squad cannot share a cell with its
  prey (the step onto that cell is refused), so no
  existing movement changes. The fix was found by
  this step's own tests, not asserted in advance:
  the radius-9 and dead-attacker tests first
  failed because a guard at its post reported a
  move, and they pass on the fixed rule with no
  test changed for it.
- Decision, on Patrol, deferred with its open
  questions intact: what the route is and where it
  is stored, what happens at each endpoint, whether
  the squad fires while moving, and what it does
  with no enemy in range. None of those is answered
  here, and Patrol must not be claimed as designed
  by this step.
- Deliberately NOT in this step: a second doctrine
  (Patrol), a fourth kind, any change to
  HoldGround or Advance behaviour, a price or cap
  change, a movement rule beyond the shared step's
  return-value fix, a targeting rule (enemy
  targeting stays kind-blind and doctrine-blind
  except for the squad's own fire rule), a balance
  pass, a warning / tint / refresh system, a
  camera, a save-format change, or any claim about
  Windows Unity verification or the pending Slice 1
  director playtest gate.
- Next, in order: Slice 11 Step 3, demo scenario +
  documentation-only review, composing the third
  kind under the new doctrine beside an existing
  kind under an existing doctrine in one
  Simulation, closing Slice 11.

## 0.51.0 — Slice 11 Step 3: demo scenario + review, closing Slice 11

- Decision, on what Step 3 is: a composition proof
  plus a documentation-only review, not a new system.
  The demo puts the new kind under the new doctrine
  beside an existing kind under an existing doctrine
  in one Simulation — Heavy Defend Core beside
  Vanguard HoldGround — so the reader sees the new
  pairing as a choice among choices, not a
  replacement for them. No fourth kind, no second
  doctrine, no balance pass, and no save-format
  change ride along, because a review step that
  also builds would make the review untestable as
  a review.
- Decision, on the demo's shape: the Heavy marches
  24 ticks to the core (12 east + 12 south from its
  spawn) while the Vanguard kills its lane enemy at
  its rally on tick 4, so the two doctrines run
  simultaneously, not in sequence. The core attacker
  is staged only after the guard is at its post, so
  chase, kill, and return are read cleanly; the
  radius-escape question is deliberately not
  re-proved here, because Step 2's tests already
  prove the 8 / 9 boundary and a demo that staged
  it again would duplicate a test, not add evidence.
- Decision, on the dummy-rally coincidence: the
  Heavy's path crosses its dummy rally cell
  (20,5,20) at tick 8, because that cell lies on the
  diagonal from spawn to core. The demo does not
  dodge the cell or claim the guard never visits it;
  it proves the doctrine, not the rally, decides
  where the squad stops, by the snapshot word:
  Marching at (20,5,20), Guarding only at
  (28,5,28). A review that hid the coincidence
  would leave a reader to find it and doubt the
  doctrine claim; stating it makes the claim
  stronger, not weaker.
- Decision, on the dead squad in the demo: a third
  Vanguard squad, weakened to 10 HP by a direct
  damage call and killed by an adjacent enemy in
  one tick, proves the gate's dead-stays-dead rule
  literally — 2 squads load, not 3, and the cap is
  freed by the death. The weakening is a staging
  device, not a balance claim: no sentence in the
  review claims a 10-HP squad is a fair fight, and
  the device must not be quoted later as evidence
  about squad survivability in play.
- Decision, on the review's verdict and gaps: Slice
  11 is closed as tested squad widening, not as
  playtested, balanced, or Unity-verified work.
  The B-key checklist gap from Slice 10 is recorded
  as closed by Step 1 (gate item 19), not carried
  again, so a later review does not re-carry it by
  accident. The shared marker now covers three
  kinds, not two, and is carried as a grown gap,
  not a new one: Step 1 already decided the shared
  marker, so this review does not re-decide it.
  Patrol stays deferred with its route question
  intact, and Slice 12 is not scoped by this review.
- Deliberately NOT in this step: a fourth kind, a
  second doctrine, a balance pass, a marker
  distinction, a warning / tint / refresh system,
  a camera, a save-format change, a movement or
  targeting rule change, or any claim about Windows
  Unity verification or the pending Slice 1
  director playtest gate.
- Next, in order: Slice 12 scoping, as its own
  decision step, chosen from the Slice 11 review's
  ranked candidates (further electrical widening
  first, adapter visibility and fairness second),
  with Slice 11's composition evidence in hand.

## 0.55.0 — Slice 12 Step 3: composed demo + review, closing Slice 12

- Decision, on composition: one Simulation, not
  three proofs. The Step 3 brief from Step 2 was a
  powered pylon beside a live network feeding no
  consumer, read through both HUD lines, and this
  step composes that literally: the live network
  and the lone source sit on the y=20 plane beside
  an existing LV mill, and the pylon sits on the
  y=5 plane beside an existing powered workstation,
  so the reader sees the widening as a choice among
  choices (a third consumer beside the first two,
  a second source feeding it beside the first) and
  the display as the only readout for the two
  things no older count shows. Splitting the demo
  into a display run and a pylon run would have
  proved each system alone and composition never;
  the scope's Step 3 sentence forbids that split
  by naming one composed run, and this step follows
  it.
- Decision, on the parked player and the bench walk:
  a measurement procedure, stated, not a play
  pattern smuggled in. Workstation regen is
  player-centred (Manhattan distance 3) while pylon
  fire is machine-centred, so one player position
  cannot prove both; the demo therefore parks the
  player at (10,5,10) for the three pylon ticks
  (shields provably flat at 65, no regen confound)
  and then walks to (16,5,28) to read regen 10, the
  same procedure Slice 10 used for its three
  benches. A demo that parked the player at the
  bench for all five ticks would have let regen
  cap the shields before the pylon's ticks finished
  and hidden whether the pylon ticks themselves
  touched the player; a demo that never walked
  would have left the existing consumer unproved
  in composition.
- Decision, on the range-5 honesty: the demo stages
  the enemy at range 5 but does not claim a
  stationary range-5 kill, because enemies move
  before the pylon phase and the first shot lands
  at range 4. The demo's claim is therefore a
  3-tick kill at shrinking range (5 staged, then 4,
  3, 2 at firing time), and the stationary boundary
  — fires at 5, silent at 6 — rests on Step 2's
  tests, which is where it was proved, not on this
  demo. Quoting the demo as a range-5 proof would
  be a category error about tick order, and the
  review pins it so a later sentence cannot make
  it.
- Decision, on staging: TrySet, stated, not aim
  placement re-proved. Aim placement of a cable, a
  mill, a pylon, a workstation, a transformer, a
  Dynamo, and an Alternator was each proved in its
  own step's tests; walking this layout in by aim
  would prove walking, not wiring, and the demo
  claims composition of core rules, not placement
  of this exact layout. The same staging honesty
  Slice 9, Slice 10, and Slice 11 used, applied
  unchanged.
- Decision, on no new checklist item: the review
  adds none, because the demo exercises no adapter
  binding items 22 (live line) and 23 (P key) do not
  already cover. A review that invented a checklist
  item for its own demo would be claiming adapter
  coverage the demo cannot give — the demo is
  headless, and its numbers prove core rules and
  HUD numbers, not key feel or scene visibility.
  The live-in-scene question stays gap 5, not a
  demo claim.
- Decision, on gaps closed and carried: the Slice 11
  gap 6 (a live network feeding no consumer shows
  nothing) is recorded as closed by Step 1, with
  Step 2's extension of the anti-double-count rule
  to the pylon as part of the same closure — the
  pylon's own Alternator counts as neither a live
  network nor a lone source, proved in the demo's
  (1,1) reading. Everything else Slice 11 and Slice
  12 named stays carried, in priority order, with
  the Slice 13 ranking taken unchanged from the
  0.52.0 scope: adapter visibility and fairness
  first, the deferred third source second, Patrol
  third. The Step 3 evidence gives no reason to
  reorder them, and reordering on review instinct
  alone would be a decision without evidence.
- Decision, on not scoping Slice 13 here: the scope
  forbids it ("Slice 13 is scoped afterwards, the
  same way this document follows the Slice 11
  review's candidates rather than preceding them")
  and this review obeys it. Scoping Slice 13 inside
  its own review would let the review choose its
  successor before the composition evidence is
  read, which is exactly the guessing the slice
  order exists to prevent.
- Deliberately not in this step: no new core system;
  no second consumer or source; no warning, tint,
  or mesh refresh; no squad-marker distinction; no
  Patrol, fourth kind, or second doctrine; no
  balance pass on the pylon's range, damage, or
  cost; no save-format change; no delivery,
  transformer, or cable rule change; no claim about
  Windows Unity verification or the pending Slice 1
  director playtest gate.
- Next step, decided in this step: scoping Slice 13
  as its own decision step, chosen from the ranked
  candidates this review carries, with Slice 12's
  composition evidence in hand.

## 0.54.0 — Slice 12 Step 2: third consumer, HV-rated Shock Pylon

- Decision, on the widening itself: the scope's
  starting hypothesis is adopted — a third consumer,
  not a third source. The reason is the one the
  scope gave: both existing consumers are LV-rated,
  so HV has no safe direct use, and a third source
  would widen the side that already has two distinct
  outputs while leaving that asymmetry in place. A
  third source is deferred, not rejected: it is now
  the ranked Slice 13 electrical candidate, behind
  the adapter visibility and fairness work the Slice
  12 scope already ranked first for Slice 13.
- Decision, on what the third consumer does: a
  defensive emitter, not a third gatherer and not a
  second regen bench. A gatherer at HV rating would
  be a mill with its rating swapped — provable, but
  a reskin in the exact sense the Alternator step
  refused for sources — and a regen bench at HV
  would be a workstation with its rating swapped, the
  same objection. A pylon that shoots is the only
  of the three candidate effects that is not one of
  the existing effects wearing a new rating, and its
  proof burden is therefore an effect proof (damage
  per tick, range, targeting, stacking) as well as
  a rating proof (every delivery path, both ways).
- Decision, on rating: HV, as its own PylonRated
  constant, not borrowed from AlternatorOutput and
  not derived from MillRated or WorkstationRated.
  The Alternator step proved a source by feeding
  both consumers; this step proves a consumer the
  same way, in reverse — direct HV, direct LV,
  step-up, step-down, cable HV, cable LV — so the
  rating is demonstrated by delivery, not asserted
  by the constant's name.
- Decision, on mismatch: disable, following the
  workstation, not destroy, following the mill. The
  distinction is principled, not a coin flip: the
  mill's destruction punishes overvoltage (too much
  for the machine), while LV to an HV machine is
  undervoltage (too little), and destroying a
  defensive structure for being fed too little would
  punish the milder mistake harder than the mill's
  rule punishes the worse one. A disabled pylon
  also keeps its counterplay intact: the player can
  rewire and recover, exactly as a mismatched bench
  recovers, instead of losing the defence and the
  wiring in one mistake. None is idle, not a
  mismatch, on the same reasoning as both existing
  consumers.
- Decision, on fire numbers: range 5 and damage 10
  per tick, both first-pass values labelled as such,
  not balance claims. Range 5 is borrowed, openly,
  from the Recon squad's engagement range, so a
  pylon reaches exactly as far as the longest-ranged
  squad and no farther — a range the game already
  teaches. Damage 10 is one squad member's hit, so a
  30-HP enemy dies in 3 pylon ticks alone: a pylon
  holds a lane, it does not one-shot it, and two
  pylons stacking to 20 is the deliberate price of
  building two. No balance pass rides along: these
  numbers are chosen for determinism and testability,
  like every first-pass number before them.
- Decision, on targeting: nearest living enemy, ties
  to the first in list order, one target per pylon
  per tick. Nearest-only (not all enemies in range)
  keeps one pylon from clearing a wave by itself,
  and the list-order tie-break is the same rule the
  squad target search uses, so targeting does not
  fork into a pylon rule and a squad rule that can
  drift apart. Stacking is the consequence, stated
  rather than hidden: two pylons may hit one enemy,
  because each pylon fires at its own nearest.
- Decision, on tick order: the pylon phase runs last,
  after squads, after mismatched destruction, after
  both automation phases. A pylon therefore never
  fires before the squad phase has had its shot, and
  never at an enemy a squad already killed that tick
  (AttackEnemy removes the dead at once, and the
  phase re-scans per pylon, so a later pylon never
  retargets a corpse). A pylon also never damages
  the player, a squad, or the core: it is an
  enemy-only defence, and firing at the objective
  it guards would be a category error, not a feature.
- Decision, on cost: 5 Stone + 10 EmberOre, a
  combination no existing machine uses. The 10 ore
  is exactly a squad's deploy price, deliberately:
  a pylon is bought instead of a squad, a static
  defence against a mobile one, so the ore economy
  forces the choice instead of letting both be bought
  from separate purses. The 5 stone is half a
  workstation's frame, because a pylon is a single
  emitter, not a bench. No Wood, so mills keep their
  own resource; no 10-stone frame, so it does not
  price itself against Alternators at their scale.
- Decision, on display and counting: the pylon joins
  IsConsumer, so the Step 1 anti-double-count rule
  extends to it without a new rule — a network
  feeding a pylon reads (0,0) live, because the
  pylon count already shows that delivery, and a
  lone pylon reads (0,0) because it is a consumer,
  not a source, and a consumer with no power is not
  a live thing. The HUD gains a third consumer line
  (PoweredPylons / TotalPylons, optional
  PylonPowerText), following the workstation line's
  precedent exactly: an LV-fed pylon counts as NOT
  powered, because it is disabled that tick, and
  counting it powered would be the display lie the
  mill and workstation counts were built to avoid.
- Decision, on placement and scene: aimed builder
  plus P key, picker-suppressed like every other
  builder, with P confirmed free (used keys: WASD,
  Space, E, Q, R, O, Return, Esc, 1-6, T, Y, U, I, B,
  N, mouse click). No per-block mesh colour exists
  for any machine in this codebase, so the pylon
  shares the generic scene rendering of the other
  machines; that is recorded as a rendering fact,
  not claimed as a pylon marker, and the three-kind
  shared marker question stays a Slice 13 visibility
  question, not smuggled into this step as a fourth
  marker.
- Deliberately not in this step: no third source;
  no balance pass on range, damage, or cost; no
  pylon-to-pylon chaining, no splash damage, no
  targeting priority (weakest, nearest-to-core), no
  pylon damage to the core or to squads, no warning
  when a pylon is mismatched (gap 4 stays open), no
  mesh tint or mesh refresh for powered pylons
  (gap 5 stays open), no pylon inventory or buffer,
  no change to any existing consumer's rating,
  cost, effect, or phase, and no demo scenario —
  that is Slice 12 Step 3's composition work.
- Next step, decided in this step: Slice 12 Step 3,
  demo scenario + documentation-only review, closing
  Slice 12 — composing the pylon with the Step 1
  display (a powered pylon beside a live network
  feeding no consumer, read through both HUD lines),
  and reviewing Slice 12 against its scope.

## 0.53.0 — Slice 12 Step 1: live-network and lone-source display

- Decision, on what counts as live: the scope's
  hypothesis is adopted, not overturned — a
  network is live exactly when it carries a voltage
  that is not None, and a lone source is live
  because it is already generating with no cable
  and no consumer. The evidence for keeping the
  hypothesis is that every case the step staged
  separates cleanly under it: a sourceless cable
  reads dead, a mixed network reads dead, a lone
  Dynamo and a lone Alternator each read as one
  lone source, and no staged case needed a third
  category. Counting placed blocks regardless of
  voltage was rejected outright: a cable count is
  not a live-network count.
- Decision, on one count or two: two counts, not
  one. A lone source and a cabled network are not
  the same shape of thing — one is a generating
  cell with nowhere to send power, the other is a
  wired structure carrying power — and folding them
  into one number would hide which one the player
  built. The HUD line carries both numbers, so the
  "one number per thing counted" rule holds per
  thing, not per line.
- Decision, on networks, cells, or sources: networks.
  A ten-cable LV run reads as one network, proved
  by test, because the player question is "how many
  live wirings do I have", not "how many cable
  cells did I lay". Sources, by contrast, are
  counted per cell: each Dynamo or Alternator
  generates independently, sources are not
  conductors (a Dynamo does not conduct through
  another Dynamo), so two adjacent Dynamos read as
  two lone sources, not one network. A Dynamo
  beside a wall is still lone — only cable,
  transformer, or consumer adjacency disqualifies,
  because only those give its power somewhere to go.
- Decision, on the anti-double-count rule: a network
  feeding a consumer is not counted, because that
  delivery is already shown by the consumer's
  powered count. Proved three ways: a network
  touching a mill directly, a network touching a
  workstation directly, and a network reaching a
  mill through one transformer hop in either
  direction (Alternator step-down LV and Dynamo
  step-up HV) all read zero live networks while
  the consumer count still reads powered (or, for
  the HV mill, the mismatch the step does not
  hide). A world with one fed network and one free
  network reads exactly one, proving the exclusion
  is per network.
- Decision, on the transformer question: a
  transformer output that is not None counts as a
  live network only when it stands alone — no cable
  neighbour, no consumer neighbour. A connected
  group of such transformers counts as one network,
  not one per stage, by the same networks-not-cells
  reasoning as cables: a two-transformer chain fed
  by one Dynamo reads one. A transformer touching a
  cable is never standalone: its output lives or
  dies with that component. The cost is stated, not
  hidden: a live output driven into a conflicted
  (dead) component shows nothing, proved by a probe
  mill reading None beside a mixed component. That
  is accepted because the component is the network
  and the transformer is a stage of it — counting
  the stage as well would count one wiring twice.
- Decision, on the "feeds" approximation: "feeds" is
  direct adjacency to a consumer, or adjacency to
  a transformer that is itself adjacent to the
  component — one transformer hop. A deeper chain
  (cable, transformer, transformer, mill) still
  reads as a live network, and that is a known
  approximation, recorded here rather than fixed by
  a full delivery trace, because a trace would
  duplicate the delivery walk the step was told to
  reuse, and the one-hop case is the layout every
  earlier step actually builds.
- Decision, on the voltage split: none — LV and HV
  networks count alike in the one number. The count
  answers "how many live wirings have no consumer
  yet", not "what voltage", and a split would double
  the HUD lines for a question the player asks
  while wiring. The danger reading is explicitly
  refused: this count is not a safety indicator, and
  an HV network counted here is live, not safe.
  Gap 4 (no warning) stays open, and Step 2's
  HV-rated consumer, if chosen, will give HV a safe
  use without changing what this count means.
- Decision, on the counting path: the existing walk
  is reused, as the scope required. `LivePowerCounts`
  is a public `WorldZone` method that calls the
  private `CableComponent`, `CableNetworkVoltage`,
  and `TransformerOutput` — no cable is re-walked
  in the HUD, and no delivery rule is touched. The
  count is derived on every snapshot and never
  saved: removing a source drops the count on the
  next snapshot, and a save/load round-trip
  re-derives (1,1) from placement alone. Every
  existing demo save length stands, because a
  display step that changed a save would have
  changed the game, not the display.
- Decision, on the adapter line: a third Text field,
  `LivePowerText`, with its own line, not a fold-in.
  The two existing POWER lines answer "how many
  consumers are powered"; this line answers "what
  live wiring has no consumer yet", and folding it
  into either consumer line would make one line
  answer two questions. Unwired, nothing changes,
  like the other power lines. A HUD line is not a
  mesh tint and not a mesh refresh, and the step
  does not claim a counted network is visible in
  the scene — gap 5 stays open. Playtest-gate
  checklist item 22 (Windows-only) is added, and it
  supersedes item 18's statement that a lone
  Alternator shows nothing: that statement was true
  under the pre-display decision and is now
  superseded by a new decision, not silently edited
  away.
- Test fix during the run, and it was a test fix,
  not a code fix: the dead-component edge test first
  asserted `DeliveredVoltage` on the cable cell
  itself, but a cable cell beside a direct Dynamo
  receives LV by direct precedence, so the component
  was never mixed. The test now stages a genuinely
  mixed component (LV direct plus HV transformer
  output) and proves it with a probe mill beside
  the cable reading None; no core code changed for
  it. A second fix was a name fix, not a code fix:
  four test-local names collided with earlier
  top-level test variables and were prefixed.
- Deliberately NOT in this step: any delivery,
  precedence, conflict, rating, cost, or phase rule
  change; any save-format change; a voltage split;
  a warning, tint, or mesh-refresh system; a
  squad-marker distinction; Patrol; a third
  consumer or source (that is Step 2, decided
  there); a balance pass; a demo scenario (that is
  Step 3, composing this display with Step 2).
- Next step: Slice 12 Step 2, exactly one further
  electrical widening, decided in that step, with a
  third consumer as starting hypothesis.

## 0.52.0 — Slice 12 scope: electrical widening, display first, then one widening

- Decision, on the theme: Slice 12 is further
  electrical widening, because that is the Slice 11
  review's first-ranked candidate and Slice 11's
  composition evidence gives no reason to reorder
  it. The squad side is closed for this slice, so
  a scope that also added a kind, a doctrine, or a
  marker distinction would make the electrical
  work untestable in isolation. The theme is taken
  from the review's candidate list, not invented
  here: "a third consumer, a third source, or a
  display for live networks feeding no consumer".
- Decision, on ordering display first: the scope
  orders that list rather than treating it as a
  menu, because a display step is the smallest step
  that closes a named gap (review gap 6) on its
  own, and because a widening built first would be
  proved only through consumer counts it already
  has, while a display built first gives the
  widening step a second readout for its own
  proof. The cost of this order is stated, not
  hidden: Step 1 adds no new gameplay, only a
  readout, and a reader who wants new power
  gameplay must wait for Step 2.
- Decision, on what "live" means as a hypothesis,
  not a decision: a network is live when it carries
  a voltage that is not None, including a lone
  source generating with no cable and no consumer.
  A cable component with no source and a mixed
  component resolving to None are not live under
  it, however many cable cells they contain. Step
  1 may overturn this hypothesis with evidence —
  for example if counting a lone source as a
  network of one confuses the count — but it must
  say so, not silently count placed blocks.
- Decision, on the Step 2 hypothesis: a third
  consumer first, because both existing consumers
  are rated LV and HV therefore has no safe direct
  use — a mill fed HV is destroyed, a workstation
  fed HV is disabled, and safe HV use always costs
  a transformer step-down. An HV-rated consumer
  would prove itself the way the Alternator did,
  by a test an LV-rated consumer cannot pass. A
  third source on a two-level ladder would need a
  distinctness beyond voltage, which would be a
  second system to invent. The hypothesis is not
  the decision: Step 2 may choose a third source
  if it finds the HV-consumer case collapses into
  a rating constant, with the evidence recorded.
- Decision, on what Step 1 is not: a HUD count is
  not a warning, not a mesh tint, and not a mesh
  refresh. Gap 4 (destruction/disablement warning)
  and gap 5 (tint/refresh) stay open after Step 1
  by decision, and Step 1's review must say so, so
  no later sentence can quote the new HUD line as
  if it warned anyone or made a cable visible in
  the scene. Likewise Step 1 displays power, not
  squads: the three-kind shared marker (gap 7)
  is not fixed, renamed, or quietly decided here.
- Decision, on the counting path: Step 1 must reuse
  the existing network walk (`CableComponent` /
  `CableNetworkVoltage`, private to `WorldZone`
  today) rather than re-walking cables in the HUD,
  or record why a new walk is genuinely needed. A
  duplicated walk would be duplicate logic of the
  kind this project forbids in adapters, moved into
  the HUD instead.
- Decision, on adapter grounding: `HudDisplay`
  renders two POWER lines from two Text fields, so
  Step 1 needs a third field or a stated reason to
  fold into an existing line, plus a Windows-only
  checklist item. The builder keys the scope names
  for Step 2 (T / Y / U / I / N / B) are a memory
  to check against the actual adapter, not a fact
  Step 2 may assume.
- Deliberately NOT in this step: a second consumer
  or source, a warning / tint / refresh system, a
  squad-marker distinction, Patrol, a fourth kind,
  a second doctrine, a balance pass, a camera, a
  save-format change, a delivery / transformer /
  cable rule change, or any claim about Windows
  Unity verification or the pending Slice 1
  director playtest gate.
- Next, in order: Slice 12 Step 1, a display for
  live networks and lone sources, decided in that
  step.

## 0.56.0 — Slice 13 scope: adapter visibility and fairness, split into three ordered steps

- Decision, on the theme: Slice 13 is adapter
  visibility and fairness, because that is the Slice
  12 review's first-ranked candidate and Slice 12's
  composition evidence gives no reason to reorder
  it. The power side is now wide (three consumers,
  two sources, live counts) and the squad side is
  wide (three kinds, three doctrines), but the scene
  still cannot warn of a loss, tell kinds apart, or
  show power state — so the next slice is about
  what the player can see, not about adding a
  fourth thing to see. The theme is taken from the
  review's candidate list, not invented here.
- Decision, on splitting the candidate apart: the
  Slice 12 review left Slice 13 scoping to decide
  the visibility question "as one visibility question
  or split apart there", and this scope splits it
  into three ordered steps, because the three
  sub-questions verify differently: a warning has a
  headless-testable core readout, a marker
  distinction is adapter-only on a snapshot that
  already carries kind, and a mesh tint needs a
  colour channel that does not exist plus a refresh
  event set. One step covering all three would be
  untestable as a unit, and a single "visibility"
  step that claimed all three on one test count
  would be dishonest about which parts any test
  actually proves.
- Decision, on ordering warning first: destruction
  without warning costs a whole mill with no
  salvage, and disablement costs regen or fire with
  no notice, while marker confusion costs only
  recognition and an untinted mesh costs only
  legibility — so the fairness-critical readout
  comes before the recognition and legibility work.
  The cost of this order is stated, not hidden:
  Step 1 adds no scene visibility at all, only a HUD
  readout, and a reader who wants the scene itself
  to change must wait for Steps 2 and 3.
- Decision, on ordering marker second and mesh
  third: the marker step is the smallest adapter-only
  change (snapshot Kind already exists, marker code
  already pools and positions, only the per-kind
  rendering is missing), while the mesh step is the
  largest adapter surface (a new colour channel in
  MeshBuilder plus a refresh event set across build
  keys and destruction ticks), so the small,
  independent step goes before the large one and
  gives the mesh step a settled marker palette to
  avoid colliding with.
- Decision, on what Step 1 is not: a HUD warning
  line is not a mesh tint and not a mesh refresh,
  and a warning is not a grace period — a warning
  that postpones destruction by even one tick would
  be a rule change smuggled in as a display step.
  Gap 5 stays open after Step 1, by decision, and
  Step 1's review must say so, so no later sentence
  can quote the warning line as if the threatened
  machine were visible in the scene.
- Decision, on Step 2's honesty requirement: a
  marker distinction cannot be proved by any test
  count, because the adapter is not compiled
  headlessly, so Step 2's verification is written
  adapter code plus a Windows-only checklist item,
  and existing snapshot-Kind tests are cited, not
  re-counted as marker evidence. The same honesty
  rule applies to Step 3's tint and refresh wiring;
  only Step 3's per-cell power-state query is a
  headless-testable core claim.
- Decision, on adapter grounding: HudDisplay renders
  four POWER lines today, so Step 1 needs a fifth
  Text field or a stated fold-in; builder keys today
  run T / Y / U / I / B / N / P with picker Alpha1-6,
  so no step may assume a free binding for a warning
  or tint control; playtest-gate checklist items run
  through 23, so Slice 13 steps add items 24 and up,
  Windows-only.
- Deliberately NOT in this step: any core system (a
  scope document builds nothing); a third source, a
  third consumer, Patrol, a fourth kind, or a second
  doctrine; a balance pass; a camera or mini-map; a
  new block type, save field, or rule change; or
  any claim about Windows Unity verification or the
  pending Slice 1 director playtest gate.
- Next, in order: Slice 13 Step 1, a
  pre-destruction / pre-disablement warning, decided
  in that step.

## 0.57.0 — Slice 13 Step 1: pre-destruction / pre-disablement warning

- Decision, on coverage: all three mismatch outcomes
  together, as the scope's starting hypothesis
  proposed, because all three already have a mismatch
  predicate in core and a mill-only warning would
  leave the two disablements exactly as silent as
  they are today. The hypothesis survived the step's
  test: a single world holding a mismatched mill, a
  mismatched workstation, and a mismatched pylon
  reads (1, 1, 1) while powered, lone, and conflicted
  machines in the same world read no warning at all.
- Decision, on shape: three counts, not one folded
  number, and not one line per outcome either. One
  number would hide which loss the player built — a
  mill destroyed is a rebuild, a bench disabled is
  lost regen, a pylon disabled is a lane left
  undefended — while three separate adapter lines
  would spend three HUD lines on one question. So
  core carries three fields and the adapter renders
  one line carrying all three numbers, with the line
  empty while all three are zero: a permanent "0
  warned" line would be noise, not a warning, and
  the empty state is itself the all-clear.
- Decision, on None and on unpowered: neither warns,
  following the predicates rather than inventing a
  new rule for the warning. None (no source, a broken
  cable, or a Dynamo-plus-Alternator conflict) is
  neither powered nor mismatched by the slices that
  defined it, and an unpowered machine loses nothing
  on the next tick — warning for either would cry
  wolf every snapshot and teach the player to ignore
  the line before a real loss ever arrives. The
  conflicted-mill test pins this: Dynamo on one side
  and Alternator on the other delivers None and warns
  zero.
- Decision, on timing, and why this is a warning
  rather than a death notice: the readout is derived
  on every snapshot from placement and delivered
  voltage, so a snapshot taken after wiring and
  before the next tick already shows it — proved by
  tests that snapshot, advance, and snapshot again,
  for all three outcomes. It is not a delay, a grace
  period, or a salvage rule, and it must not be
  quoted as one: a warned mill still dies on the
  next tick if it stays wired, and rewiring before
  that tick is what saves it, which the rewiring
  tests prove for the mill as well as for the
  workstation and pylon.
- Decision, on the adapter line: a fifth Text field,
  WarningText, with its own line, not folded into an
  existing POWER line, because folding a warning
  into a powered/total line would make the danger
  disappear into a count the player already reads as
  routine. The line format is fixed in the adapter
  comment ("WARNING: n MILLS WILL BE DESTROYED, n
  WORKSTATIONS WILL BE DISABLED, n PYLONS WILL BE
  DISABLED") in the existing plural style of the
  POWER lines. No new key binding is needed or
  assumed: the warning is a readout, not a control,
  and builder keys still run T / Y / U / I / B / N /
  P with picker Alpha1-6.
- Honesty limit, carried not hidden: in Unity a tick
  runs every frame (FpsInputAdapter calls
  Simulation.Tick every Update, and Tick calls
  World.Advance), so a mill wired to HV in live play
  is expected to show its warning for at most one
  frame before destruction, while a mismatched
  workstation or pylon, which survives disabled,
  warns persistently until rewired. The headless
  proof is therefore a proof of the readout, not a
  proof that the mill warning is usable at 60 fps —
  and this step does not fix that by slowing ticks,
  because slowing ticks would be a rule change
  smuggled in as a display step. Tick pacing for a
  readable mill warning is a question for the Slice
  13 review (or a later slice that owns tick pacing
  explicitly), and playtest-gate item 24 records
  whether the mill flash was actually seen rather
  than claiming it was usable. Until then, the
  workstation and pylon warnings are the usable
  fairness gain of this step, and the mill warning
  is a correct readout with a known visibility
  window.
- Decision, on what stays open: Gap 5 (mesh tint and
  mesh refresh) stays open after this step, by
  decision — a HUD warning line is not a tint and
  not a refresh, and no sentence in this step claims
  the threatened machine is visible in the scene
  because it is warned about in the HUD. Step 3 must
  make its tint agree with this warning: a machine
  warned here must be the machine tinted there, and
  a rewired machine must lose its tint on the next
  refresh, or Step 3 must say why they differ.
- Deliberately NOT in this step: any delivery,
  precedence, conflict, rating, cost, or phase rule
  change; any tick-rate, grace-period, or salvage
  rule; a new block type, save field, or key binding;
  a demo scenario (the warning's proof is the
  snapshot-before/after-tick tests, and a demo
  scenario that printed a warning would not prove a
  visible line); or any claim about Windows Unity
  verification or the pending Slice 1 director
  playtest gate.
- Next, in order: Slice 13 Step 2, a scene-marker
  distinction for the squad kinds, decided in that
  step.

## 0.58.0 — Slice 13 Step 2: scene-marker distinction for the squad kinds

- Decision, on shape versus colour: both, with shape
  primary. A colour-only distinction fails a player who
  cannot tell those colours apart, and the scope named
  that failure explicitly — so the step does not ask
  colour to carry the distinction at all. Cube, capsule,
  and cylinder differ in silhouette at any distance a
  0.6-unit marker reads at all, and colour reinforces
  the shape: cyan for Vanguard, magenta for Recon,
  light grey for Heavy. If Windows testing proves the
  shapes do not read at FPS distance, that is item 25's
  evidence to revisit, not a reason to have shipped
  colour alone and hoped.
- Decision, on varying rather than replacing: Vanguard
  keeps the exact 0.29.0 marker, cyan cube 0.6 units,
  because it is the one marker players already
  recognise, and a replacement of all three would have
  thrown that recognition away for no gain. Recon and
  Heavy vary from it, so the shared-marker history is
  preserved as the anchor rather than erased. The
  Slice 11 Step 1 decision (one shared cube, kind from
  the text line) is superseded, not silently edited:
  item 20's marker statement is superseded by item 25,
  and the adapter header says so.
- Decision, on palette against the untouched markers:
  rally is an orange flat cube and pending is an orange
  sphere, so any kind colour near orange would be
  mistaken for a rally state — the step names its
  colours against those two, not in isolation. Magenta
  is already a neon-sign colour in docs/art-direction.md
  (cyan, magenta, orange neon), so it extends the
  palette rather than inventing one; light grey is the
  dark-metal palette lightened for the anchor kind,
  whose role is metal mass, not energy. Recon's
  capsule is not a pending sphere by another colour:
  it is taller, thinner, and a different primitive,
  and pending exists only while picking, while a
  deployed Recon marker exists only after deploying.
- Decision, on the pool: the scope's budget (at most 3
  squad markers, 3 rally markers, 1 pending marker,
  pooled, not rebuilt per frame) is kept exactly, by
  pooling slots instead of primitives. A squad slot is
  a parent GameObject; its one child primitive is the
  kind's shape. Because a deployed squad's kind never
  changes, the child swap runs only when a slot is
  reused by a later deployment of a different kind
  after the previous squad is gone — a kind-change
  rebuild, not a per-frame rebuild, and the pool never
  grows past 3 squad slots. The alternative, one pool
  per kind, would have tripled the squad pool to 9 and
  broken the budget the scope set; the alternative of
  reusing one primitive and re-scaling it per kind
  would have made shape a scale trick, not a shape.
  An unknown kind falls back to the Vanguard cube:
  disappearing would be worse than a provisional
  marker, and a future kind's distinction is that
  kind's own step to decide.
- Decision, on what does not change: no core file is
  touched, because SquadSnapshot already carries Kind
  and SquadMarkerDisplay already read the snapshot —
  inventing a core mapping to justify a display step
  would have been a rule smuggled in as a marker.
  No kind shape, cost, cap, movement, targeting, or
  doctrine rule moves; OrbitalSquadDisplay is untouched
  and keeps reading kind from the same snapshot, so
  text and markers cannot disagree unless the snapshot
  itself does. Rally and pending markers are untouched
  except by the sentence that says they are.
- Verification, stated without inflation: this step
  adds no test and no demo scenario, by the scope's
  own instruction — a headless test cannot prove a
  colour or a shape reads in the scene, and a demo
  scenario printing a kind would pretend a printed
  kind proves a visible marker. The test count is
  therefore unchanged at 1664, and that number is
  cited only as "no regression", never as marker
  evidence. The cited headless evidence is the
  existing snapshot Kind coverage: deploy, picker,
  save/load, and view tests already pin Kind for all
  three kinds. The marker proof is playtest-gate item
  25 in Unity on Windows, including the slot-reuse
  observation, the rally/pending-unchanged observation,
  and the occlusion observation — and until item 25 is
  observed, the distinction is written adapter code,
  not a verified scene.
- Deliberately NOT in this step: any core rule change;
  any rally or pending marker change; a per-kind pool
  or a shape-by-scale trick; a through-wall marker
  shader; a demo scenario; any claim about Windows
  Unity verification or the pending Slice 1 director
  playtest gate; or closing Slice 13 — Step 3 (mesh
  tint and mesh refresh) and the Slice 13 review are
  still to come.
- Next, in order: Slice 13 Step 3, mesh tint and mesh
  refresh for power state, decided in that step — with
  the Step 1 agreement carried forward: a machine
  warned in Step 1 must be the machine tinted in Step
  3, and a rewired machine must lose its tint on the
  next refresh, or Step 3 must say why they differ.

## 0.59.0 — Slice 13 Step 3: mesh tint and mesh refresh for power state

- Decision, on which cells tint: all three of the scope's
  candidate sets together, and nothing else. Energised
  cables answer "is power reaching this run", powered
  machines answer "is this machine running at rated
  voltage", mismatched machines answer "is this machine
  about to be lost or disabled" — three different player
  questions, and tinting only one set would have left
  the other two invisible in the scene, which is the gap
  Slice 13 exists to close. Tinting everything was
  refused for the scope's own reason: a tint on every
  block would make the tint meaningless, and a tint on a
  source or a transformer would be a fourth, confusing
  answer — a lone Dynamo tinted as live would claim a
  delivery that reaches no consumer, and a transformer
  tinted as energised would hide whether its conversion
  actually reaches anything, since its neighbours already
  show that. Sources and transformers therefore stay
  white, by decision, and that limit is recorded here
  rather than discovered as a surprise: if Windows
  testing shows players read a converting transformer
  as dead because it is untinted, that is item 26's
  evidence to revisit, not a silent gap.
- Decision, on the tint's relationship to the warning:
  agreement, carried forward from Step 2's next-step
  note, by construction. PowerState reuses the exact
  powered and mismatched predicates Step 1's warning
  counts reuse, so there is no second definition that
  could drift from the first: a machine warned in Step 1
  is the machine tinted Mismatched here, a rewired
  machine loses its warning and its tint on the next
  snapshot and the next refresh together, and a
  destroyed mill loses both because it is gone. The one
  asymmetry is honest, not a disagreement: the HUD
  warning counts a machine before its destruction tick,
  while the tint exists only while the machine exists
  to be tinted, so after the tick there is nothing left
  to tint — the warning's one-frame mill limit from
  Step 1 (item 24) applies to the tint as well, and
  item 26 says so.
- Decision, on the colour channel: vertex colours on
  the single MeshData, not a per-machine material split
  and not a second mesh. A material split would have
  multiplied materials by power state and broken the
  "MeshBuilder is the single chunk representation"
  rule the scope set; a second tint mesh would have
  been a second representation by another name. Vertex
  colours keep one mesh, one build, one collider, with
  one more parallel buffer, and white as the untinted
  value means ordinary stone, ore, and wood render
  exactly as before under a vertex-colour material.
  The alternative of tinting by BlockType alone was
  refused because it cannot work: greedy merging merges
  same-type faces, so two adjacent mills of different
  power states would have merged into one quad with one
  colour and no correct answer — hence PowerState
  joining the merge key, which is the step's one
  mesher change, and the merge-separation test that
  proves it (2 top quads tinted, 1 untinted).
- Decision, on the palette against the markers, named
  against both, not in isolation: Powered green is
  never cyan, so it cannot read as a squad marker;
  Mismatched red is never orange, so it cannot read
  as a rally or pending marker; Energised yellow is
  brighter and greener than orange and sits on cable
  cells in the ground, never as a floating marker, so
  it cannot be mistaken for a rally state by shape or
  by location even before hue is considered. The art
  direction's orange-for-warnings rule is deliberately
  NOT followed for the tint, because the scope forbids
  an orange mismatched tint for exactly the rally
  confusion above — the HUD warning line keeps the
  orange-accent warning language in text, while the
  scene tint uses red, and that split is recorded here
  so a later reader does not "fix" the tint to orange
  and reintroduce the confusion. Colour is second to
  location and to the HUD counts, as shape was primary
  for markers: a tinted block is the machine itself at
  its own cell, and the Step 1 counts say the same
  outcome in numbers, so red/green as a first-pass pair
  is not asked to carry the distinction alone.
- Decision, on the refresh event set: a dirty event
  set, not every frame and not a version counter. A
  full 32^3 re-mesh every frame would spend the Slice 1
  budget for no visible change on frames where nothing
  changed, and the scope forbids refreshing blindly;
  a chunk version counter was refused because it would
  have touched every chunk write path (generation,
  gather, build, destruction, load) for a fact the
  adapter already knows — which key succeeded, and
  whether DestroyedMills rose on this tick. Power state
  is placement-derived, so those two facts plus load
  and Start are complete: no tick-time voxel change
  exists except a destruction, and no placement change
  exists except a key, a gather, or a load. A failed
  key refreshes nothing, because a failed key changed
  nothing and a wasted press should not cost a re-mesh;
  reload, fire, and squad deploy refresh nothing,
  because they change no voxel. The one wiring risk is
  stated, not assumed: FpsInputAdapter.MeshRenderer
  must be wired to the same renderer GameManager links,
  and a null field makes refresh a no-op — item 27
  names that wiring as a step, so an unwired scene is
  a checklist failure, not a silent stale mesh.
- Verification, stated without inflation: the query,
  the colour channel (Colours.Count equals Vertices.
  Count), the merge separation, the tint/warning
  agreement, and the save/load re-derivation are
  headless-tested, 44 new tests, 1708 total, and the
  demo saves stand (44,038 and 43,974). The tint and
  the refresh wiring are adapter-only Unity code, not
  compiled headlessly, so no test count proves a tint
  reads or a refresh fires in the scene — the proof is
  playtest-gate items 26 (tint, including the
  vertex-colour material observation and the
  powered-presence item 7 supersession) and 27
  (refresh, including the item-17 destruction-tick
  question and the MeshRenderer wiring), Windows-only.
  No demo scenario is added: a printed power state
  would not prove a visible tint, by the scope's own
  instruction for Step 3's adapter parts.
- Deliberately NOT in this step: any delivery,
  precedence, conflict, rating, cost, or phase rule
  change; a new block type, save field, or key binding;
  a per-machine material split or a second mesh; a
  through-wall tint or marker change; a balance pass
  (the tints are first-pass values, not final feel);
  a demo scenario; any claim about Windows Unity
  verification or the pending Slice 1 director playtest
  gate; or closing Slice 13 — Step 4 (demo scenario +
  review) is still to come.
- Next, in order: Slice 13 Step 4, the composed demo
  scenario and documentation-only review, decided in
  that step — with the Step 1 warning, the Step 2
  snapshot kinds, and this step's power-state query
  proved together in one Simulation, not as separate
  proofs, and with Slice 14 NOT scoped by that review.

## 0.60.0 — Slice 13 Step 4: composed demo + review, closing Slice 13

- Decision, on composition, not three proofs in one file: the
  demo stages every Slice 13 readout in a single Simulation so
  a disagreement between them would show up as a wrong number
  in the same run, not as three green runs that never met. The
  threatened trio is deliberately asymmetric in outcome — the
  mill is destroyed, the workstation and the pylon survive
  disabled — because Step 1's warning already splits those
  outcomes, and a demo that only staged mills would have
  proved a warning, not the warning. The powered counterparts
  are there for the same reason: a Mismatched query with no
  Powered query beside it proves a label, not a distinction,
  and an Energised cable with no live-network count beside it
  proves a colour, not a wiring. The live run (Dynamo -> cable
  -> cable, free end) is kept from Slice 12's pattern, not
  re-staged as a new proof, so the composition reuses a
  counted shape the reader already trusts.
- Decision, on the tick-then-rewire order: the demo ticks
  once while all three machines are threatened, then rewires
  the two survivors, rather than rewiring before any tick.
  Rewiring first would have proved clearing without ever
  proving the warning reads before the loss it warns about,
  and ticking without ever rewiring would have proved loss
  without proving recovery. The chosen order proves both in
  sequence: warning before tick, destruction and disablement
  on tick, clearing on rewiring, recovery fire after
  rewiring. The mill is the exception that proves the rule's
  limit: it cannot be rewired after the tick because it is
  gone, and the demo says so instead of staging a second,
  pre-tick rewiring as if it were the same proof — a mill
  saved by pre-tick rewiring is a Step 1 test's claim, not
  this demo's.
- Decision, on the marching squads, stated, not smoothed
  over: picker squads deploy at the player, and five ticks do
  not carry a squad from (16,5,16) to any of the three
  rallies, so every snapshot in this run reads Marching and
  the loaded snapshot reads Vanguard (13,5,14), Recon
  (12,5,17), Heavy (18,5,13), still en route. Calling them
  "at their rallies" would have been false, and running
  twenty-two idle ticks before staging the electrical layout
  just to photograph Holding squads would have proved
  patience, not composition — the holding proof stays in the
  earlier steps' tests, where it belongs, and this demo's
  squad claim is kinds and doctrines together, nothing more.
  The same honesty applies to the pylon kill: each tick's drop
  is exactly 10, so no squad fired, which is the evidence
  that the kill is the pylon's, not a claim that squads were
  kept out of range by design — the enemy's path
  (11,5,19 -> 12,5,19 -> 12,5,18 -> killed) follows the
  nearest target, squad or player, and the pylon fired while
  the enemy stayed in range, whatever that target was.
- Decision, on the orphaned Alternator: after tick 1 the lone-
  source count reads 1 where it read 0 before the tick,
  because the destroyed mill's Alternator is now adjacent to
  nothing. This is not a bug to fix in the demo and not a
  source to hide: the count is defined per cell on every
  snapshot, so a source orphaned by a destruction must count,
  and a demo that staged the threatened mill's source beside
  a cable just to keep the count at 0 would have hidden a
  real consequence. The review records it as a pinned staging
  fact so a later reader does not mistake it for a new source
  or for a counting regression.
- Decision, on the bench-walk measurement procedure, carried
  from Slices 10 and 12: workstation regen is player-centred
  (Manhattan distance 3) while pylon fire is machine-centred,
  so one player position cannot prove both at once. The demo
  parks at the threatened bench for tick 1 (regen 0), walks
  to the pylon lane for the recovery kill (no bench in range,
  shields flat at 65), and walks back to the rewired bench to
  read regen 10, and states each walk instead of pretending
  otherwise. The loaded phase then applies that regen for
  five bench ticks (45 -> 85, capped), mirroring Slice 12's
  loaded-phase shape, because a reading without a tick
  proves a number, not an effect.
- Decision, on the test-file repair, recorded because a stale
  count is a silent failure mode: the composed tests were
  first appended after the runner's top-level return, so the
  suite printed 1708 as if nothing had been added. The repair
  moves the summary and return to the end of the file, where
  every future append must also go — or, better, where a
  future step should insert its tests before the summary
  rather than appending after it. No test was changed by the
  repair, and the count rose to 1807 with no failures, which
  is the evidence the repair worked, not the repair's claim.
- Verification, stated without inflation: 99 composed-
  integration tests mirror the demo, the demo ran correctly
  on its first execution, and every earlier demo save length
  stands (Slice 11 44,038, Slice 12 43,974, new Slice 13
  44,045). The demo proves core rules and HUD numbers, not
  scene visibility: items 24, 25, 26, and 27 stay Windows-
  only, and this step adds no new checklist item, by the same
  no-new-item rule as the Slice 10, 11, and 12 reviews — a
  demo exercises no adapter binding the checklist does not
  already cover, so inventing an item for it would have been
  checklist inflation.
- Deliberately NOT in this step: any core rule, rating, cost,
  phase, delivery, transformer, or cable change; a new block
  type, save field, or key binding; a balance pass (the
  warning shape, marker shapes and colours, and tint values
  stay first-pass); a claim about Windows Unity verification
  or the pending Slice 1 director playtest gate; scoping
  Slice 14 — the review carries its ranked candidates
  (deferred third source, Patrol, Slice 13 remainders) but
  does not choose among them; or closing any gap beyond
  Slice 13's own, in the sense the review states (tested core
  plus written adapters, not scene-verified proof).
- Next, in order: scoping Slice 14 as its own decision step,
  chosen from the ranked candidates this review carries —
  deferred third source first, Patrol second, Slice 13
  remainders (vertex-colour material, MeshRenderer wiring,
  marker occlusion observations) third — with Slice 13's
  composition evidence in hand, not guessed now.

## 0.61.0 — Slice 14 scope (2026-10-30, silent scheduled run)

- Decision, on taking the top ranked candidate, not
  re-ranking from scratch: the Slice 13 review carried its
  candidates in the order third source, Patrol, Slice 13
  remainders, and this scope keeps that order, because the
  Step 4 composition evidence — warned machines, a rewired
  pylon kill, three kinds in one snapshot — says nothing
  about source distinctness, and re-ranking on evidence
  that does not bear on the choice would have been
  ranking theatre. The scope is therefore the deferred
  third source, and Patrol and the remainders become the
  ranked Slice 15 candidates, not a promise and not a
  rejection.
- Decision, on the slice shape, two steps not three:
  Slice 14 has one substantive system (the source) plus
  demo + review, where Slice 13 needed three substantive
  steps because warning, marker distinction, and mesh
  tint are three different kinds of work with different
  verification. A third source is one system with one
  proof burden — distinctness beyond voltage — so
  splitting it into a display step and a source step, as
  Slice 12 did for its widening, would have invented a
  step the source does not need: the Slice 12 display
  already counts live networks and lone sources, and
  Step 1 must prove the new source is counted by those
  counts, not build a second display first.
- Decision, on the voltage boundary, stated as a scope
  rule, not left for Step 1 to rediscover: the ladder
  stays LV / HV / None, so the third source delivers LV
  or HV. This is a boundary, not a choice smuggled in as
  one: a third level would re-rate every consumer and
  re-prove every transformer, cable, and conflict case,
  which is a ladder slice, not a source step, and a scope
  that left the level open would let Step 1 grow into
  that slice without ever deciding to. Step 1 still
  chooses LV or HV, and still carries that voltage's
  mismatch proofs, but it does not get to choose a third
  level.
- Decision, on the distinctness hypotheses, listed, not
  chosen: condition, fuel, cost shape, delivery shape,
  and pulsed output are named as starting hypotheses
  with the reskin warning attached to cost alone, because
  Slice 10 already taught that a price difference at the
  same voltage with the same behaviour is a reskin, not
  a source. Naming the hypotheses without choosing one
  keeps this scope a scope: the choice, and its test
  that neither Dynamo nor Alternator can pass, belong
  to Step 1, where the code exists to prove or refute
  them, not to a document that cannot run a test.
- Decision, on the Slice 13 remainders as observations,
  not as a step: gap 4's three items are questions about
  a Unity scene (material multiplies vertex colours,
  MeshRenderer wiring, marker occlusion), and no headless
  run can answer them, so recording them as a future
  "step" would have promised a headless proof of a scene
  fact. They are therefore carried as Windows
  observations inside checklist items 25, 26, and 27,
  ranked second for Slice 15 scoping — behind Patrol,
  because Patrol is a buildable system and the
  observations are verification to be done when Windows
  verification runs, not work to be scheduled as if it
  were buildable headlessly.
- Decision, on the generalisation burden: Step 1 must
  generalise DeliveredVoltage, TransformerOutput, and
  CableNetworkVoltage from two named sources to three,
  proving every distinct-source pair conflicts to None,
  not just the pair it stages first. The Slice 10 step
  generalised once, from Dynamo to direct source, and
  this scope names the second generalisation explicitly
  so Step 1 cannot prove only the new source beside a
  Dynamo and leave the Alternator pair unproved — a
  conflict rule proved for one pair and assumed for the
  others would be exactly the silent-winner bug the
  conflict rule exists to prevent.
- Decision, on what is deliberately not in Slice 14: no
  Patrol, fourth kind, or second doctrine (the orbital
  side is closed for this slice); no third consumer or
  fourth source as a bonus (one source, one proof
  burden); no third voltage level (see above); no
  warning, tint, marker, or HUD-count change except
  proving the existing readouts cover the new source;
  no delivery, transformer, or cable rule change beyond
  the source generalisation; no balance pass; no fuel
  economy of its own (a fuelled distinctness is one
  resource, one rule, for this source); no new save
  field unless the distinctness justifies one and proves
  old saves load; and no Windows Unity verification
  claim, which stays blocked on Unity 6000 on Windows.
- Verification, stated without inflation: documentation-
  only step, no new test, no demo scenario, no adapter
  binding, so no new playtest-gate checklist item. The
  full suite was re-run before writing (1807 passed, 0
  failed) and the demo re-run (Slice 12 save 43,974
  chars, Slice 13 save 44,045 chars, all earlier saves
  as in the 0.60.0 entry); those numbers are the
  unchanged baseline, not evidence for a source that
  does not exist yet.
- Next, in order: Slice 14 Step 1, exactly one third
  power source, decided in that step, with its
  distinctness beyond voltage, its LV-or-HV choice, its
  non-output behaviour, its generalisation, its block,
  cost, and adapter line each decided and tested there.

## 0.62.0 — Slice 14 Step 1: third power source, the Conduit (2026-10-30, silent scheduled run)

- Source choice, and why cable-only HV beat the other
  hypotheses: the scope's hypotheses were a condition, a
  fuel, a cost shape, a delivery shape, and a pulsed
  output. A fuel would have needed per-source state and a
  save decision for a first source family member, and a
  pulsed output would have needed DeliveredVoltage to
  depend on tick count, which every snapshot, HUD count,
  and warning predicate also reads — both would have made
  one source carry a second system. A condition tied to
  placement or inventory would have been a second rule
  hiding inside a block. Delivery shape won because it is
  derived from placement alone, like every existing source,
  needs no state, no tick, and no save field, and is still
  a behavioural difference no existing source can pass: a
  Dynamo or Alternator beside a consumer feeds it, a
  Conduit beside a consumer feeds nothing, and only a
  cable changes that. Cost alone was rejected as the
  distinctness, exactly as the scope warned: 5 Stone + 5
  EmberOre is a new combination, but a Conduit priced
  differently and feeding directly would have been a
  Dynamo/Alternator reskin, so the price is recorded as
  pricing, not as the proof.
- Voltage choice, HV not LV: HV gives the Conduit the full
  mismatch proof set the Alternator had — a cable-fed mill
  destroyed, a cable-fed workstation disabled, a cable-fed
  pylon powered — while its direct-adjacency safety is the
  proof that separates it from the Alternator: the same mill
  that a direct Alternator destroys survives a direct
  Conduit untouched. An LV Conduit would have proved only
  safe cable delivery and a disabled pylon, a thinner set,
  and would have duplicated the Dynamo's safe uses through
  cable without ever punishing a wrong wiring. HV also
  keeps the transformer step-down proof meaningful: safe LV
  use of a Conduit still costs a transformer, as it does
  for an Alternator.
- Non-output behaviour, decided before the tests: a Conduit
  with no cable is unwired, not idle-on-a-timer, not
  fuel-starved, and not broken. Consumers beside it read
  None, so a mill falls back to kinetic cadence, a
  workstation runs at base regen, and a pylon idles — the
  same None behaviour every unpowered machine already has,
  so no new idle rule was invented for this source. A
  Conduit touching only another source is likewise unwired:
  sources are not conductors, and the lone-source count
  proves it (two adjacent Conduits count 2 lone sources).
- Generalisation, and the one deliberate asymmetry: the
  source set is three, the direct set is two. DeliveredVoltage
  and TransformerOutput exclude the Conduit from their direct
  branches, while CableNetworkVoltage includes it — that is
  the whole implementation of cable-only delivery, and it is
  why a Dynamo + Conduit mill reads LV (no direct conflict
  exists to resolve) while a Dynamo + Conduit cable reads
  None (a mixed network, like every mixed network). The
  alternative — counting the Conduit as a direct source and
  then suppressing it in consumers — would have put the
  cable-only rule in three predicates instead of two
  branches, and would have made direct conflicts depend on
  which consumer was reading. Two Conduits on one cable
  agree at HV, like two Dynamos agreeing at LV: same level
  is one voltage, not a conflict, whichever source it is.
- Cost rationale, first-pass, not balance: 5 Stone + 5
  EmberOre is half a workstation's frame and half a squad's
  ore price, so the Conduit competes for both resources
  without being the cheapest source to stage in either —
  a source that does nothing until cabled must not also be
  free to scatter. The Alternator's stone-only price still
  stages earlier with no ore at all, and the Dynamo's
  ore-only price still feeds directly with no cable, so
  the Conduit is not cheaper than either in the dimension
  where that source is strong.
- Adapter line: M builds a Conduit by aim, via
  BuildConduitAhead, suppressed while the picker is open
  like every other builder (a Conduit spends ore, so a key
  while picking could break a pending Confirm's
  affordability). M was confirmed free at step time:
  existing keys are WASD / Space / E / Q / R / O / Return /
  Esc / Alpha1-6 / T / Y / U / I / B / N / P / mouse click.
  Playtest-gate checklist item 28 (Windows-only) covers it.
  No mesher change: like the Alternator and the pylon, the
  Conduit renders as an ordinary solid block headlessly,
  and its distinctness is in delivery, not in a mesh.
- Readout agreement, proved: warned counts, PowerState,
  live-network, and lone-source counts all read Conduit
  cable delivery correctly, with no new HUD count — the
  scope's "prove, don't redesign" rule for displays held.
  The one exception is recorded honestly below, not
  papered over with a query change.
- Finding carried, not fixed — single-cable tint: a single
  cable cell touching a Conduit reads None in PowerState,
  because DeliveredVoltage for a cell counts only its
  neighbours, and a Conduit is not a direct source. The
  network is nevertheless live: the mill at the end of
  that single cable is destroyed on tick 1, the live-network
  count reads 1, and a two-cable Conduit run reads
  Energised on both cells. A Dynamo-fed single cable does
  not share the finding, because the direct branch counts
  the Dynamo touching the cable cell. Whether PowerState
  for a cable cell should also count its own component's
  voltage is a display-query question for the Slice 14
  review to carry — changing it inside this source step
  would have moved a display the step was supposed to be
  proved against, and would have changed tint behaviour
  for every existing source's cables as well. Until the
  review decides, a lone Conduit-fed cable cell renders
  untinted while its network is live: recorded here so no
  later run mistakes it for a delivery failure.
- Test notes, stated openly: the first full run after
  implementation failed exactly one test, the single-cable
  Energised assertion — and the failure was the finding
  above, not a code bug, so the test was corrected to
  assert the honest None reading plus a two-cable
  Energised proof, and no core code changed for it. The
  pylon cable test stages its enemy at Manhattan distance
  4 from the pylon, which moves to 3 before the pylon
  phase, so it proves a 10-damage tick, not a range-5
  boundary shot — the range-5 boundary rests on the Slice
  12 Step 2 tests, as in the Slice 12 demo note.
- Deliberately not in this step: a condition, a fuel, a
  pulsed output, or a fourth source; a third voltage level
  or ladder redesign; a delivery, transformer, or cable
  rule change beyond the source generalisation above; a
  warning, tint, marker, or HUD-count change (including
  the single-cable tint finding, carried not fixed); a
  balance pass (cost and HV are first-pass, labelled as
  such); Patrol, a fourth kind, or a second doctrine.
- Next step: Slice 14 Step 2, composed demo scenario plus
  documentation-only review closing Slice 14, carrying the
  single-cable tint finding as a remainder.

## 0.63.0 — Slice 14 Step 2: composed demo + review, closing Slice 14 (2026-10-30, silent scheduled run)

- Composition, decided in this step: one Simulation, not
  separate proofs, because the scope's demo job is to show
  the third source beside the sources already trusted, with
  a mismatch and an idle case beside them. The Dynamo mill
  is the existing-source-at-its-voltage proof (LV direct,
  powered, 5 in 5 ticks); the Conduit cable mill is the
  mismatch proof (HV through cable, destroyed tick 1); the
  direct-Conduit mill is the idle proof (None, no warning,
  kinetic fallback, 1 in 5 ticks); the single-cable pylon
  is the safe-HV proof (powered, three-tick kill); the
  step-down workstation is the safe-LV proof (Conduit
  cable through a transformer, regen 10). No second
  existing-source consumer is staged, because the Dynamo
  mill already carries that role and a second would have
  proved a source Step 1 already proved.
- Staging corrections, stated openly, because the first
  demo execution is part of the evidence: the pylon enemy
  was first staged at (10,5,19), as Slice 13 had staged
  its pylon enemy, and the Conduit-fed pylon fired twice
  before the enemy reached Manhattan distance 6 and
  walked out of range alive at 10 HP. The demo as first
  staged therefore proved a two-hit escape, not a kill,
  so the enemy was re-staged to (10,5,17), where it dies
  on tick 3 (20 after tick 1, 10 after tick 2, dead on
  tick 3, each drop exactly 10, so no other damage source
  fired). The kill is a shrinking-range kill, not a
  range-5 boundary proof: the stationary range-5 boundary
  rests on the Slice 12 Step 2 tests, as in the Slice 12
  and Slice 13 demo notes, and a reader who quotes this
  demo for the boundary is misquoting it.
- Finding found by the demo, not by Step 1 — the
  direct-Conduit count invisibility: the draft demo text
  expected POWER 0 LIVE, 1 LONE before the tick, and the
  run printed 0 LONE. The text was corrected, not the
  count: the Slice 12 lone rule counts a source only when
  no cable, transformer, or consumer is face-adjacent to
  it, and the direct Conduit sits beside a mill, which is
  a consumer, so it counts as neither live nor lone even
  though it delivers nothing. This is the definition
  applied consistently, not a miscount — a Dynamo beside
  a mill is likewise not lone, and it does deliver — but
  it is a genuine visibility question, because the player
  who places a Conduit beside a mill sees nothing in
  either count while the mill reads None and gathers
  kinetically. Whether the counts should distinguish a
  beside-a-consumer source that delivers nothing from one
  that delivers is a Slice 15 scoping question, with its
  own tests; redefining a count inside a demo step would
  have moved the rule the demo was supposed to prove.
- Finding re-proved, not fixed — single-cable tint: the
  demo's single cable cell reads None before the tick, on
  tick 1 (printed, not asserted), and after load, while
  its pylon fires and its sibling two-cable run reads
  Energised on both cells. The step-down cable, by
  contrast, reads Energised even though it is also a
  single cable cell touching a Conduit — because its other
  neighbour is a transformer, whose flipped output counts
  for the cable cell, while the pylon cable's neighbours
  are only a consumer and a Conduit, neither of which
  counts. The finding is therefore narrower than "a single
  cable touching a Conduit reads None": it is "a cable
  cell whose only neighbours are a consumer and a Conduit
  reads None". That precision is recorded here so a later
  query redesign does not over-correct cables that
  already read correctly through a transformer.
- Bench procedure, stated openly: the player never walks
  in this demo. The step-down workstation sits beside the
  spawn (16,5,16) by staging, so regen 10 is a
  spawn-bench reading, not a bench-walk measurement as in
  Slices 10 and 13. The demo also contains no squads, so
  it proves electrical composition only; orbital proof
  stays in the earlier slices' demos and tests. TrySet
  staging is used throughout, exactly as the Step 1 tests
  stage theirs, because aim placement of a Conduit was
  proved in Step 1 and walking this layout in by aim
  would prove walking, not wiring.
- No-new-checklist-item rule, applied: this review adds
  no playtest-gate checklist item, because the demo
  exercises no adapter binding the checklist does not
  already cover. Checklist item 28 already asks the scene
  observer to record the single-cable untinted cell and
  the two-cable yellow run honestly, and the M key is
  already covered there. A new item for the
  direct-Conduit count question would pre-decide a
  display change Slice 15 scoping has not yet weighed, so
  the question is carried in the review and here, not as
  a checklist line.
- Test-file note: the composed tests were inserted before
  the runner's summary and return, per the Slice 13 Step 4
  repair note, and the count rose from 1894 to 1988 on
  the first full run with no failures — no test-file
  repair was needed in this step. The demo's first
  execution needed the two staging corrections above, and
  its second execution ran correctly end to end, so no
  test or demo fix was needed after staging.
- Deliberately not in this step: a new core system of any
  kind; a fix to the single-cable tint or the
  direct-Conduit count; a balance pass (Conduit cost and
  HV stay first-pass); a new checklist item; Slice 15
  scoping (that is the next run's decision step, chosen
  from the review's ranked candidates, not guessed here).
- Next step: scoping Slice 15 as its own decision step,
  chosen from the ranked candidates this review carries
  (Patrol first, Slice 14 remainders second, Slice 13
  scene-proof remainders third).

## 0.64.0 — Slice 15 scope: the deferred doctrine, Patrol (2026-10-30, silent scheduled run)

- Previous state verified from GitHub before work:
  main head 7f580c60509f0f65a31f2d9ae933335227c9289c,
  CHANGELOG 0.63.0 (Slice 14 Step 2, demo + review,
  closing Slice 14) — the run log had no entry for
  0.63.0 yet, but remote head, remote CHANGELOG head,
  and the sync record (65 entries, last = that commit)
  all confirmed it, so no step was repeated: the
  0.63.0 decision named scoping Slice 15 as next,
  chosen from the review's ranked candidates, and this
  run did exactly that. Tests 1988 baseline re-run
  passing; demo Slice 14 save 43,974 chars.
- Scope decision, stated openly: Patrol first, not
  the remainders first. The Slice 14 review ranked
  Patrol first, Slice 14 remainders second, Slice 13
  scene-proof remainders third, and the Step 2
  composition evidence gives no reason to reorder
  them — a Conduit cable mill, a Dynamo mill, and a
  pylon kill say nothing about whether a squad can
  walk a route. The deeper reason is testability in
  isolation: the remainders are display-query
  decisions (a tint query change would move tint for
  every existing source's cables; a count redefinition
  would move a rule two demos were proved against),
  and deciding either inside a doctrine step would
  leave a failing test unable to say whether the
  doctrine or the display failed it. Patrol is the
  only doctrine question left — HoldGround, Advance,
  and Defend Core are the whole doctrine set — so a
  doctrine slice keeps its proof clean, and the
  remainders lose nothing by waiting one slice,
  because checklist item 28 already asks the scene
  observer to record the untinted cell honestly.
- What the scope does NOT decide, stated openly so
  Step 1 is not pre-empted: the route shape (loop,
  back-and-forth, waypoints, or another original
  definition only with a recorded reason), where the
  route is stored (rally + spawn derivation, a second
  picker point, a saved waypoint list, or another
  stated source), the endpoint behaviour, the blocked-
  step answer, the fire rule, the chase-or-pass answer,
  the snapshot activity word, and the adapter key
  (Alpha7 is a starting hypothesis, with Alpha1–Alpha6
  in use, confirmed free at step time — not a decision
  here). What the scope DOES fix is the shape of the
  proof: route repeat or reverse proved, blocked step
  proved, firing while moving proved, return to route
  after any chase proved, idle rule proved (keeps
  walking, not standing), more than one kind proved,
  price 10 EmberOre and cap 3 settled, enum appended
  only unless the route justifies a save field with
  old saves proved loading.
- Deliberately not in this step: a doctrine, a kind,
  an electrical widening, a tint or count redesign, a
  scene-proof observation as a headless step, a
  movement rule change, a balance pass, a save field
  without justification, or Slice 16 scoping (that is
  the Slice 15 review's successor decision, chosen
  from the ranked candidates this scope carries).
- Next step: Slice 15 Step 1, exactly one fourth
  doctrine, decided in that step.

## Open decisions

- Scope slicing order beyond Slice 1 (Slice 1 is locked in
  `docs/slice-01-plan.md`; further slices are decided under delegation
  as Slice 1 evidence arrives).
- Release licence, monetisation, and release date — deferred to
  release stage, logged here when decided.
