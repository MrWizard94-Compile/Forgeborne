# Forgeborne — Manifest

## Mission

Build a studio-grade, production-grade, PC-native FPS-RTS voxel ARPG
hybrid that is fun because gather → build → survive works as a loop,
not because it has many systems.

## Scope (in scope, in order)

1. Voxel world that can be gathered, terraformed, and built in.
2. FPS gather / build / combat, with Orbital Tactical command.
3. Survival pressure: swarms, elites, bosses, and world events that test
   what the player gathered and built.
4. Loot and builds: procedural weapons, affixes, synergies, and a deep
   passive graph — added only after Slice 1 proves the core loop.
5. Automation tiers: Kinetic, Electrical, Digital, Arcane — added one
   tier at a time, only after the previous tier is complete and tested.
6. A world with atmosphere and story told through place, sound, and
   discovery, not a blank arena.

## Non-goals (explicitly out, unless the director changes them)

- 2D, in any form. 2.5D / flat-sprite workarounds do not satisfy this.
- Turn-based mechanics, deckbuilders, and paused build-phase tower
  defense.
- Twin-stick Asteroids-style arena shooting as the core loop.
- Ember Forge code, balance, or enemy roster. Ember Forge is a closed
  learning experience; nothing is carried forward except lessons.
- Multiplayer / client-server networking in the foundation. Deferred.
- Neural world models as canonical runtime state. See `DECISIONS.md`.
- Copied assets, lore, code, or trademarks from Minecraft, StarCraft,
  Diablo, Path of Exile, BioShock, or Borderlands.

## Quality gate (requirements, not suggestions)

A feature ships only when it is:

- Complete end-to-end and playable, not a demo or placeholder.
- Tested: automated tests where logic is testable, plus manual playtest
  evidence. Edge cases named.
- Documented: acceptance criteria, CHANGELOG entry, and updated docs.
- Maintainable and secure: clean structure, no secrets in code, no
  copied material, deterministic logic separated from rendering where
  it matters.
- Performant: measured, not hoped. 60 FPS target on desktop, with
  quality settings and FPS-adaptive fallback. Graceful degradation on
  lower-end hardware.
- Reliable: no silent save loss. Local saves with export / import.
- Approved by the director at the milestone gate. A passing check does
  not prove usefulness or fun.

## Definition of done (per milestone)

Clear acceptance criteria, tests, playtest evidence, edge cases,
CHANGELOG entry, and director approval. Any missing item is a named
acceptance gap, never a shortcut to Done.
