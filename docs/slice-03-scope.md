# Forgeborne — Slice 3 Scope

**Status: SCOPED 2026-10-29, under delegation. No Slice 3 system is
built by this document. Each step below is built one at a time,
with tests, in this order, and Slice 3 is closed by its own
review — it is not closed by finishing step 2 alone.**

## Scope question

Slice 2 added depth (loot, skills, automation, Orbital squads)
but left two kinds of debt: things the game promises and then
loses on load, and things the game lets the player do without
limit because nothing prices them. Slice 3 pays that debt
before any new system widens the game further. Its theme is
therefore **economy and persistence**: make what exists
survive a save/load round-trip, and give the one unlimited
action (squad deployment) a real cost and a real cap.

## Inputs this scope is decided from

- Slice 2 review (`docs/slice-02-review.md`) deliberately-not-done
  list: deployment cost / cap, skill builds not saved, and the
  full 1,500-node graph, tiers 2-4, extra doctrines/kinds, and
  enemy AI targeting squads all still open.
- 0.14.0 follow-up: the core objective's health is not in the
  save format, so a loaded world starts with a fresh 100-HP
  core. That is a save-format step, and it belongs here.
- 0.15.0 decision: deployment cost/cap deferred to a Slice 3
  economy step, because inventory holds blocks, not a currency,
  so a price invented in the picker step would have been a
  balance guess with no economy to price against.
- 0.7.0 gap, repeated in the review: a loaded world starts at
  base skill values until skills are applied again. The 0.14.0
  restore re-applies a build the Simulation already holds, but
  a fresh Simulation loading a save has no build to re-apply —
  the build itself is not saved.

## Slice 3, in order

### Step 1 — Save-format completion (persistence first)

Persist, in the existing save format, what a load currently
loses or resets, without changing the meaning of any value an
old save already carries:

- Core objective health (so a damaged core stays damaged
  across a load, instead of silently healing to 100 HP).
- The applied skill build (so a fresh Simulation loading a
  save restores the build, instead of starting at base values
  until someone re-applies it).
- The equipped weapon is a candidate for the same step, not a
  promise: the step must decide, and record in DECISIONS.md,
  whether the weapon joins the format or stays session-only
  with the 0.14.0 rationale (kept on in-place restore, dropped
  on a fresh load). If it joins, old saves without it must
  still load with the Pulse Rifle default.

Rules for the step: old saves load (missing fields default,
never fail the load); malformed new fields are skipped, never
fatal, matching the 0.10.0 squad rule; the demo gains a
save/load round-trip that proves core HP and the build
survive; no balance change and no new system rides along.

Why first: an economy priced against a build that vanishes on
load, or a core that heals on load, would be pricing fiction.
Persistence is the precondition, not a nice-to-have.

### Step 2 — Squad economy (cost and cap)

Give Orbital deployment a cost and a cap, decided in that
step — not in this scope document — with the reasoning
recorded in DECISIONS.md:

- The step must choose the pricing basis and justify it: pay
  in gathered resources (Stone / Wood / EmberOre, which the
  inventory already holds) or introduce a new currency with
  sources and sinks. The scope's starting hypothesis is
  resource pricing, because it needs no new system, but a
  hypothesis is not a decision: if the step finds resource
  pricing distorts gathering (for example, squads competing
  with walls and mills for the same Stone), it must say so
  and choose otherwise, with the evidence.
- The cap must be a gameplay rule, not an arbitrary number:
  what it limits (concurrent live squads, total deployed,
  or something else), and what happens at the cap (deploy
  refused, oldest squad recalled, or another rule), are part
  of the step's decision.
- The picker (`OrbitalCommand`) must enforce both: a deploy
  the player cannot afford, or that exceeds the cap, fails
  closed like a failed Confirm in 0.15.0 — picker stays open,
  no squad, no partial charge.
- No new doctrine or squad type rides along. HoldGround /
  Vanguard is the priced unit; pricing a second unit before
  the first is priced would repeat the 0.15.0 mistake of
  inventing balance without an economy.

### Step 3 — Slice 3 demo scenario and review

As with 0.13.0 and 0.12.0: extend the demo with one composed
Slice 3 run (a damaged core, a skilled build, and a paid,
capped squad deployment, through a save/load round-trip),
then write `docs/slice-03-review.md` as a documentation-only
step. The review closes Slice 3 as tested, integrated
persistence + economy — not as playtested, balanced, or
Unity-verified — and names the next candidates in priority
order. Slice 4 is not scoped by that review; it is scoped
afterwards, the same way this document follows the Slice 2
review's follow-ups rather than preceding them.

## Deliberately NOT in Slice 3

- The full 1,500-node skill graph (the 6-node Forge Survivor
  cluster is the persisted unit; growing the graph is a later
  slice, not a persistence fix).
- Automation tiers 2-4, crafting, fuel/power, per-mill
  inventory (tier 1 stays excavation-only into the player
  inventory).
- Additional doctrines (advance, defend core, patrol), squad
  types (recon, heavy, flyer), enemy AI targeting squads,
  3D squad markers, or an Orbital camera. Those are the
  ranked candidates for Slice 4 scoping, not Slice 3 scope
  creep — with one exception noted below.
- A balance pass. Step 2's prices are first-pass values chosen
  for determinism and testability, exactly as Slice 2's
  numbers were, and must be labelled as such, not quoted as
  final balance.
- FPS pitch aiming, licence, monetisation, release status, or
  any change to repo visibility. Those need their own
  decision steps (and, for licence/monetisation/release, Rob).
- Windows Unity verification. It remains open for Slices 1-3
  alike and is blocked on Unity 6000 on Windows, not on
  anything this slice can do headlessly. The checklists in
  `docs/slice-01-playtest-gate.md`, `docs/orbital-view.md`,
  and `docs/orbital-command.md` remain the verification plan.

## Ranked candidates for Slice 4 scoping (not promised)

These are recorded here so the Slice 3 review starts from a
list, not from scratch. Order is provisional and the review
may reorder it on the evidence of Steps 1-2:

1. Enemy AI targeting squads + a second doctrine (the two
   belong together: a doctrine that moves squads into danger
   is only fair once enemies can answer them).
2. Automation tier 2 (electrical), with whatever inventory
   question tier 1 deferred, decided then.
3. FPS pitch aiming + 3D squad/rally markers (the two
   presentation gaps that most limit the picker in Unity).
4. A second squad type, priced under the Slice 3 economy.

## Slice 3 gate (what "done" means)

Slice 3 is done when: every step above is pushed with passing
tests; old saves still load; a damaged core, a skill build,
and (if Step 1 decides so) a weapon survive a save/load
round-trip in tests and in the demo; squad deployment refuses
unaffordable and over-cap deploys without charging or
half-deploying; and `docs/slice-03-review.md` records the
verdict honestly, including that Slice 3 is still not
playtested, balanced, or Unity-verified. The Slice 1 director
playtest gate is still pending throughout and is not closed
by Slice 3.
