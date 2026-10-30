# Forgeborne — Slice 6 Review

**Status: REVIEWED HEADLESSLY 2026-10-30, tests 746/746.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, and 5. This review closes Slice 6 as a reviewed
whole; it does not claim Slice 6 is playtested, balanced, or
finished in every direction it could grow.**

## Review question

Does Slice 6 close the presentation gap Slice 5 left open — a
player who could not look up or down, squads and rallies
existing in Unity only as text lines, an Advance squad
described by a rally it ignores, and a powered loop with no
hand or indicator in Unity at all — with exactly the three
presentation steps its scope ordered (pitch, markers, powered
presence), plus a composed demo, without inventing a new core
system, a new unit, a new power tier, or a balance claim, and
without breaking the gather -> build -> survive loop, squad
price and cap, Dynamo cost and cadence, loot, skills, core
health, or old saves? The answer headlessly is yes for the
systems below, with the gaps and next candidates listed
honestly at the end — and with one claim split the review
must keep split: the core presentation pieces are tested,
the adapter pieces are written and checklist-listed, and
neither claim borrows the other's evidence.

## What Slice 6 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.27.0 scope | `docs/slice-06-scope.md`: Slice 6 scoped as FPS presentation, in order — pitch, markers + Advance display fix, powered presence, then demo + review. Ranked Slice 7 candidates recorded, not promised | Decision document only, no system built by it | Tests 633/633 unchanged |
| 0.28.0 FPS pitch aiming | Core `Look` (yaw + pitch clamped [-89, 89], forward vector from both, yaw 0 faces +Z, positive pitch up); `Simulation.Look` + `ApplyLook` feeding already-scaled mouse deltas, no second sensitivity; adapter camera rendered from the core forward vector, picker raycast following it. Camera + picker only, by design | Core owns the view direction; the adapter no longer stores its own yaw and holds no duplicate aim math | Tests 665/665; pitched gather / build / fire keep Slice 1 rules, yaw-90 movement stays world-aligned, Look not saved but kept across an in-place restore |
| 0.29.0 3D squad / rally markers + Advance display fix | Core `SquadActivity` (Marching / Holding / Hunting / Idle) derived in `OrbitalView.Snapshot`, with a hunting target re-derived each snapshot from the same nearest-enemy rule movement and fire use, never saved; adapter `SquadMarkerDisplay` (pooled original primitives: cyan squad cubes, orange rally slabs, orange pending-rally sphere, colliders removed, ordinary opaque material so markers do not show through walls); text display renders the activity word verbatim | Snapshot is the single source for text and markers alike, so the two displays cannot disagree; adapters render the snapshot only | Tests 685/685; Advance beyond range, adjacent, and with no living enemy read Hunting / Hunting / Idle; activity survives save/load by derivation |
| 0.30.0 powered Unity presence | Adapter keys T (Kinetic Mill, front-left) and Y (Dynamo, front-centre) wiring the existing builders, both suppressed while the rally picker is open; core `HudState` power counts (`PoweredMills` / `TotalMills`) derived per snapshot from mill cells and `WorldZone.IsPowered`, no new save field; `HudDisplay` renders `POWER: x/y MILLS POWERED` | Builders, cells, costs, and support rules unchanged — this step wires keys to proved builders and counts proved placement | Tests 709/709; counts cover fresh 0/0, unpowered 0/1, powered 1/1, two mills 1/2, diagonal 0 powered, Dynamo-alone and wall-next-to-Dynamo as no mill, count dropping on Dynamo removal, counts surviving save/load by derivation |
| 0.31.0 demo scenario + review | This review, plus a composed Slice 6 demo scenario: one Simulation in which pitch, both squad activity words, and the powered indicator run together, through a save/load round-trip. No new core system rides along with this step | Demo and composed-integration tests mirror each other; adapter rendering is explicitly out of headless reach and stays in its checklists | Tests 746/746; demo Step 4 scenario below |

## What the review verified this run

- Full test suite re-run before writing: **746 passed, 0 failed**
  (was 709; +37 composed-integration tests for this step,
  with no test failures during the run).
- Headless demo re-run: Slice 1 unchanged (gathered 1, built 1,
  killed 2, save 43,881 chars); Slice 2 unchanged (built 1,
  killed 2, kinetic gathered 1, squad intact 3 members / 90 HP,
  squads saved+loaded 1, save 43,908 chars); Step 1 scenario
  unchanged (core 42 HP, weapon 28/120, shields 100/105, save
  44,016 chars); Step 3 scenario unchanged (core 42 HP, 3 paid
  squads 90 HP, 10 ore left, 4th picker deploy REFUSED at cap,
  save 44,103 chars); Slice 4 scenario unchanged (HoldGround
  at rally, Advance hunting, save 44,026 chars); Slice 5 Step 2
  scenario unchanged (powered 5 vs kinetic 1 in 5 ticks, stone
  0 -> 6 then 12, save 43,974 chars). New Step 4 scenario, in
  full: one Simulation, Look starting at yaw 0 / pitch 0 with
  forward (0.000, 0.000, 1.000), then 90 yaw / 45 pitch giving
  forward (0.707, 0.707, -0.000), then +100 pitch more clamping
  at pitch 89 with forward (0.017, 1.000, -0.000) — the clamp
  is proved, not just the direction. A HoldGround squad
  deployed through the paid picker reads Marching at spawn
  (16,5,16) and, after exactly 4 ticks, Holding at its rally
  (20,5,16) with full 90 HP — no enemy existed during its
  march, so the march is a clean reading, not a combat
  accident. An Advance squad deployed through the paid picker
  (second 10-ore deploy, 0 ore left) reads Hunting with prey
  (16,5,20); after one tick the full-strength squad has
  killed its 30-HP prey in one engagement tick and reads
  Idle at (16,5,17), still 90 HP — its prey died from squad
  fire before any adjacent damage, and the squad has not
  marched toward its ignored rally (24,5,16), which is the
  documented Advance behaviour, not a failure to move. A
  Kinetic Mill built front-left (15,4,17) and a Dynamo built
  front-centre (16,4,17) through the real Simulation builders
  read 1/1 on the power indicator; with five stone neighbours
  seeded, 5 ticks give powered gathered 5 and stone 5. Save /
  load: the mill, the Dynamo, both squads, and the gathered
  stone all survive; the loaded power indicator still reads
  1/1, the session powered counter resets to 0, the loaded
  squads still read Holding and Idle by derivation, and the
  loaded Look is a fresh view (yaw 0, pitch 0, forward
  (0.000, 0.000, 1.000)) because Look is session input state
  and is not saved — a fresh Simulation starts facing +Z,
  while an in-place restore into an existing Simulation keeps
  the current view, per the 0.28.0 decision. Five fresh stone
  neighbours seeded around the loaded powered mill give
  loaded powered gathered 5 again. Save JSON 44,024 chars.
- Slice 6 gate, checked against `docs/slice-06-scope.md`:
  every step pushed with passing tests; pitch is clamped and
  its direction vector is proved in core tests, and the
  picker raycast follows the pitched camera in the adapter
  code (not headlessly proved, and not claimed as such);
  squads and rallies have scene markers rendered from the
  snapshot only in adapter code (not headlessly proved, and
  not claimed as such), and an Advance squad reads hunting
  or idle by its prey, never by a rally it ignores — proved
  headlessly, including the idle-after-kill case; a mill and
  a Dynamo are buildable by key in the adapter code (not
  headlessly proved, and not claimed as such), with a power
  indicator derived from the existing `IsPowered` rule —
  proved headlessly, including through save/load; the
  composed demo proves the core presentation pieces through
  a save/load round-trip; old saves still load (no save
  field was added in any Slice 6 step, and every earlier
  demo save length above is unchanged); squad price, cap,
  and behaviour, Dynamo cost, distribution, and cadence,
  loot, skills, and core health are unchanged; and this
  review records honestly that Slice 6 is still not
  playtested, balanced, or Unity-verified.
- No Slice 6 step duplicates logic in a Unity adapter. Core
  owns Look, the activity word, and the power counts;
  adapters feed mouse deltas, render the camera and markers
  from the snapshot, and bind keys to existing builders.
  The one place the split matters most — markers and keys
  cannot be compiled headlessly — is stated in every step's
  changelog entry and again here, so the test count is
  never read as adapter coverage.
- No Slice 6 step changed a saved value's meaning: the only
  saved things that move in this slice are blocks and squads
  that earlier slices already saved, and Look and activity
  are both derived-or-session values with no field at all.
- Hard limits held: 3D real-time only, no turn-based mechanics
  (squads and mills act once per world tick, with no planning
  pauses), no twin-stick Asteroids loop, no copied assets,
  lore, code, or trademarks. Marker shapes are original
  primitives and their colours follow `docs/art-direction.md`
  (cyan squads, orange rallies); the free deploy primitive
  is headless-only by rule, not by enforcement, and is
  untouched by this slice.

## Deliberately NOT in Slice 6 (still open, not forgotten)

- A second squad type, priced under the Slice 3 economy.
  First ranked Slice 7 candidate, not Slice 6 scope creep:
  pricing a second unit before the picker that deploys it
  could be aimed and read in the scene would have repeated
  the 0.15.0 mistake — Slice 6 removes that reason, so the
  candidate now stands on its own.
- Aim-based gather / build / fire, and yaw-rotated movement.
  Named follow-ups of step 1, carried forward as one Slice 7
  candidate because they are one question (does the player's
  aim, not a fixed +Z cell, decide what the player touches?),
  not two unrelated systems. The demo proves pitch does not
  change the Slice 1 interaction cells; it does not prove
  aim-based interaction would work.
- The rest of automation tier 2: a voltage ladder (LV to EV),
  transformers, mismatched-voltage destruction, and
  additional powered consumers or power sources. Deferred,
  not rejected, and ranked behind the two candidates above
  by the scope — the Slice 6 evidence gives no reason to
  reorder it first, since power at one voltage is now
  presented, not just proved.
- Defend Core and Patrol doctrines, an Orbital camera, or a
  mini-map beyond the Slice 1 placeholder. Slice 6 markers
  are scene markers in the FPS view, not a second camera.
- A balance pass. Pitch clamp (89 degrees), marker colours
  and shapes, and the T / Y keys are first-pass values
  chosen for determinism and testability, exactly as Slice 3's
  prices and Slice 4's combat numbers were, and must be
  labelled as such, not quoted as final feel.
- Enforcement of the free-deploy rule beyond its existing
  documentation. The free `DeploySquad` primitive remains a
  headless-only rule, re-named in the Slice 3 review's gaps,
  not quietly fixed here.
- A mesh tint for Dynamo / powered mills, and a mesh refresh
  on any key build. Both were named as gaps in 0.30.0, not
  fixed here: the review step adds no adapter system, so a
  tint or a refresh hook would have been a second system in
  a composition step. They remain checklist questions in
  `docs/powered-presence.md` and item 12 of
  `docs/slice-01-playtest-gate.md`.
- FPS pitch beyond step 1, licence, monetisation, release
  status, or any change to repo visibility. Those need their
  own decision steps (and, for licence / monetisation /
  release, Rob).
- Windows Unity verification itself. It remains open for
  Slices 1–6 alike and is blocked on Unity 6000 on Windows,
  not on anything this slice can do headlessly. The
  checklists in `docs/slice-01-playtest-gate.md`,
  `docs/orbital-view.md`, `docs/orbital-command.md`, and
  `docs/powered-presence.md` remain the verification plan,
  and this review adds no new checklist item because its
  adapter claims are already covered by gate items 11–12
  and the orbital / powered checklists.

## Gaps this review names, in priority order

1. **No Unity verification at all yet** — for Slices 1–6.
   Every adapter is explicitly uncompiled headlessly and
   needs Unity 6000 on Windows. Slice 6 is the slice most
   exposed to this gap by design: its main value is adapter
   code (camera from Look, markers, T / Y keys, power text),
   and the headless proof covers only the core numbers
   those adapters render. Until the checklists run, Slice 6
   is written presentation, not seen presentation.
2. **No director playtest yet.** The Slice 1 playtest gate
   (`docs/slice-01-playtest-gate.md`) is still pending
   throughout Slice 6 and is not closed by it: no one has
   played gather -> build -> survive in Unity and answered
   whether it is fun, and Slice 6 adds no playtest evidence
   of its own beyond headless composition — a pitched camera
   and markers are feel questions, and feel is not proved
   by a forward vector.
3. **No balance pass.** Pitch clamp, marker colours and
   shapes, T / Y keys, squad price and cap, mill cost and
   cadence, Dynamo cost, distribution, and cadence, loot
   drop rate, and skill numbers are all first-pass. Slice 6's
   presentation is proven to describe the game deterministically,
   not proven to feel right — description and feel are
   different claims, and only the first is made here.
4. **Mesh tint and mesh refresh remain named gaps.**
   A Dynamo is still not rendered distinctly from stone or
   mills, and no adapter refreshes the voxel mesh on any
   key build, so a mill or Dynamo built by key may be
   counted by the HUD line before it is visible in the scene.
   Both were named in 0.30.0 with their scope discipline
   (a tint needs per-block UV / material work; a refresh
   for T / Y alone would split the builders' behaviour),
   and neither is closed by a review step.
5. **The free deploy primitive remains a documented gap.**
   It is headless-only by rule, not by enforcement: nothing
   in code stops a future play path from calling it. If a
   future step finds a play path reaching it, that is a bug
   in that path, per the 0.18.0 decision — but the review
   notes it again here so it is not forgotten as slices grow.

## Slice 6 verdict (headless)

Slice 6 is **reviewed and closed as tested presentation core
plus written adapters — a clamped pitch with a proved
direction vector, squad and rally markers rendered from a
snapshot whose activity word finally describes Advance
squads by their prey, and a powered loop buildable by key
with a derived power indicator, all composed through a
save/load round-trip in one demo run**. It is not playtested,
not balanced, and not Unity-verified, and its adapter claims
must not be quoted as if the test count covered them.
**Slice 7 is not scoped by this review** — its scope is the
decision for the next run, chosen from the ranked candidates
below, so it is chosen with Slice 6's composition evidence
in hand rather than guessed now.

## Ranked candidates for Slice 7 scoping (starting point, not promised)

Carried from the 0.27.0 scope, in the same order, because
the Step 4 evidence gives no reason to reorder them:

1. A second squad type, priced under the Slice 3 economy.
2. Aim-based gather / build / fire, and yaw-rotated
   movement — the interaction follow-ups step 1 carries
   forward, as one candidate because they are one question
   (does the player's aim, not a fixed +Z cell, decide what
   the player touches?), not two unrelated systems.
3. The rest of automation tier 2 (voltage ladder,
   transformers, mismatch destruction, additional consumers
   and sources), deferred, not rejected.
