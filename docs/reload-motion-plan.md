# Reload motion path — 0.133.0

Continue the full six-influence AAA+ game and Rob's arm/hand correction. Previous
turn: verified published progress, clean main/origin at
e7cf355f0c7beb823c4a176f513e33e2a2f30364 (0.132), not art/fun acceptance.

Work on codex/reload-motion-path. The 1.2-second gesture reserves only 120 ms for
the final hand return. Inspect the actual current motion, then author a roughly
two-second cosmetic sequence with wider reach/return intervals, eased acceleration
and an outward clearance arc. Preserve contact with moving assemblies and exact
rest recovery. Use accumulated elapsed time robustly across frame cadences.

Before changing production, record actual baseline kinematics at fixed elapsed
intervals and, if the connected renderer supports it, a normal-frame-backed image
sequence. Compare proposed art budgets of palm speed <=3.5 m/s and acceleration
<=60 m/s^2 against the baseline. These are working animation budgets, not universal
human biomechanics limits or proof of natural motion. Inspect actual motion and
rest/magazine/charging views; technical checks do not accept art or fun.

Core ammo transfer and firing interruption remain unchanged. No timed gameplay
reload, new Simulation, physical catalog/save rule changes or new-only input.
Keep one GameManager Seed-42 Simulation, sole FPS frame driver, Both legacy input
and DLL-only Core delivery. Retain all original art/source/maps, rest rigs, shader/
material settings, nine Plugins DLLs, Core source/tests and current saves.

Verify affected actual rig/contact/pause/reset/cadence/shot consumers, compile,
normal-frame rendering and native Windows controls. Clearly distinguish scripted
render sequences from native input, playback rate from runtime performance and
reference contact from visible contact. Keep new capture paths isolated and
restore camera/view/background flags on completion/failure. Stop source edits
while an owned render job is active.

Rollback is this isolated contribution. Preserve existing native sessions/saves;
no Quit/autosave or old-backup restoration. Verified main updates remain authorized;
release/licence/visibility and unrelated publication are outside scope. Same
Constitution invocation frozen at 6.18.0 / 63d99cdc72a1c18b5aa91b9336d02fd30126407b;
single evolution 922dcbb remains validated and staged, not propagated. Full AAA+
art/actors/environment, native loop/cold recovery, six-influence depth, budgets,
heard mix and director experience/fun gates remain open.
