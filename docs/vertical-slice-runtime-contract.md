# Vertical-slice runtime contract — candidate 2026-10-31

This is an implementation contract for the next playable review, not a director
fun/quality acceptance. It follows the studio-production brief, which was read
in full, including the expanded goal-objective attachment. Unity remains the
locked engine; Unreal/C++ examples in that reference are conceptual examples.

The integration rollback point is local commit `b65233e` on
`codex/unity-main-scene`. No remote push or release is authorized by this work.

## Time and ownership

GameManager creates one Simulation. FpsInputAdapter calls its AdvanceFrame once
per rendered frame; neither displays nor the overhead camera may tick a second
copy or advance that Simulation again. Movement/collision uses 120 fixed steps
per second. The initial review candidate advances the world at **two ticks per
second**: enemy movement/damage, regeneration, squads and automation retain their
existing per-tick rules but gain an explicit real-time pace. This candidate rate
is chosen so enemies advance 2 cells/second against a 6 m/second player and an
idle player has time to observe/respond. It is a tuning choice for live review,
not an assertion of final balance.

AdvanceFrame retains fractional time and buffers a jump edge until a movement
step occurs. It rejects negative/non-finite time and caps any hitch contribution
at 250 ms, bounding catch-up work/damage. Legacy Tick remains the explicit
one-world-step API for headless callers. Death/victory stops encounter progression.
Loading a legacy save resets the fractional clock, retains saved world ticks and
keeps all adapters on the same Simulation. Exact sub-tick/continuous-pose save
restoration is still required in the persistence stage before final acceptance.

Equal-time tests at 30/60/144/240 callbacks per second must produce the same
world ticks, movement and damage. Additional checks cover hitches, invalid time,
jump buffering, legacy saves and terminal states. Core checks cannot pass the
live pacing/feel gate; observe the shipped DLL in Unity and the native build.

## Player loop and presentation

Use the canonical gather/build/aim/economy rules and existing original palette.
The first encounter needs visible threats and objective, clear targets/resource
materials, pointer capture/release, useful action feedback, death/restart, save
recovery and an actual 3D overhead view. It must allow an unassisted player to
understand gather → build → survive, then reach/destroy the core.

WorldActorDisplay reads the live world through its linked GameManager; rendered
enemy/core geometry is a view, never a second damage/collision authority. The
initial drone silhouettes occupy their canonical target cells so aim does not
silently diverge from displayed targets. Their current stepped movement is a
known presentation issue to resolve in the encounter/art work, not final animation.
Original primitive construction is an authoring method, not an automatic quality
pass. Inspect silhouettes, lighting, materials, effects and audio together.

## Evidence and gates

Core source, published netstandard2.1 DLLs, Unity import/compile, Play behavior,
packaged Windows player, sustained hardware measurements and Rob's review remain
separate gates. Preserve legacy input/Both, stable block IDs, old-save loadability
and the no-core-source-in-Assets guard. Rebuild and install the DLL after core
changes; a green source suite with an old DLL is not an integrated fix.
