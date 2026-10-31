# Forgeborne — Slice 19 Review

**Status: REVIEWED HEADLESSLY 2026-10-31, tests 2810/2810.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
16, 17, and 18. This review closes Slice 19 as a reviewed whole; it
does not claim Slice 19 is playtested, Unity-verified, or a
visual result.**

## Review question

Does Slice 19 settle the one display-query remainder Slice 18
deliberately carried — what the HUD counts show for a source
beside wiring that delivers nothing, a Conduit beside a
transformer alone reading POWER 0 LIVE NETWORKS, 0 LONE
SOURCES even though the transformer outputs None and the
Conduit feeds nothing — as its own decision with its own
tests, then proved together with its controls in one demo,
without a delivery, precedence, conflict, rating, cost, or
phase rule change, a material, shader, marker, light, fog,
or view model change, a HUD redesign, a new block, kind,
doctrine, source, or consumer, a balance pass, or a claim
of Unity verification made from a headless run? The answer
headlessly is yes for the remainder as decided, with the
split this review keeps throughout: the count is tested,
the scene reading of the count is still owed on Windows,
and the dead-cable asymmetry is a stated branch distinction,
not a gap the decision overlooked.

## What Slice 19 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.81.0 scope | `docs/slice-19-scope.md`: Slice 19 scoped as the transformer-adjacency count slice, in order — the count query first, then demo + review. Ranked Slice 20 candidates recorded, not promised | Decision document only, no system built by it | Tests 2690/2690 unchanged |
| 0.82.0 Step 1 | Transformer-adjacency count query decided: a Conduit beside a dead standalone transformer counts as lone, folded into the existing POWER line's lone number — no new line, no new word. Lone, restated whole: a source with no cable neighbour, no consumer neighbour that it feeds directly, and no transformer neighbour except a Conduit beside only dead standalone transformers | `WorldZone.LivePowerCounts` dead-transformer exception, branch-limited to Conduit because `TransformerOutput` lists Dynamo and Alternator only; `HudState` documentation updated to match; no delivery, precedence, conflict, rating, cost, or phase rule moved | Tests 2733/2733 (was 2690; +43, including the motivating case, the dead-group-feeding-a-consumer case, both live-transformer no-double-count cases, both live controls, per-source-cell counting, chain and ring, the conflicted transformer both ways, the mill-plus-transformer case, the dead-plus-live and cable-wins cases, both cable-story cases, the dead-cable consistency case, the cable-fed-only-by-a-dead-transformer case, the lone control, and save/load re-derivation); one earlier assertion re-decided with history kept (the Slice 17 Step 2 transformer world, 0 / 0 to 0 / 1); new playtest-gate item 38; demo unchanged, Slice 18 save 44,015 chars, Slice 15 save 44,016 chars, Slice 17 save 43,974 chars |
| 0.83.0 Step 2 | This review, plus a new composed demo scenario staging the decided count together with its controls in one Simulation (see below) | Review document plus CHANGELOG and DECISIONS entries; no core file, no adapter file, no material, marker, light, fog, or view model value, no checklist item added | Tests 2810/2810 (was 2733; +77 composed-integration assertions mirroring the demo); every earlier demo save length unchanged; new Slice 19 scenario save 43,965 chars, quoted as its own entry below, not as a change to any earlier save |

## The Step 2 demo, in numbers

One Simulation, six rows on the y=20 plane, four cells apart
in z so no row can leak voltage into another through a
face-adjacent neighbour, no enemy, no squad, no pocket
seeded — the demo proves counting, and nothing else:

- Row z=10, the Step 1 case: Transformer at (11,20,10),
  Conduit at (12,20,10). The group outputs None, the
  Conduit feeds nothing, and the row contributes 0 LIVE,
  1 LONE.
- Row z=14, the Dynamo live control: Dynamo at (10,20,14),
  Transformer at (11,20,14). The group flips LV to HV,
  feeds no consumer, and contributes 1 LIVE, 0 LONE.
- Row z=18, the Alternator live control: Alternator at
  (10,20,18), Transformer at (11,20,18). The group flips
  HV to LV, feeds no consumer, and contributes 1 LIVE,
  0 LONE.
- Row z=22, the lone control: Conduit at (10,20,22) with
  no neighbour at all, contributing 0 LIVE, 1 LONE.
- Row z=26, the Slice 17 Step 2 control: KineticMill at
  (10,20,26), Conduit at (11,20,26). The mill is delivered
  None, is neither powered nor mismatched, survives every
  tick, and its Conduit contributes 0 LIVE, 1 LONE.
- Row z=30, the dead-cable consistency case: Dynamo at
  (10,20,30), Cable at (11,20,30), Conduit at (12,20,30).
  The mixed LV-plus-HV component carries None, both
  sources are disqualified by cable adjacency, and the row
  contributes 0 LIVE, 0 LONE — deliberately different
  from the Step 1 row, for the branch reason Step 1
  recorded: the Conduit feeds the cable component, so
  cable adjacency is genuine wiring for it, while
  TransformerOutput lists no Conduit branch, so
  transformer adjacency is not wiring for it at all.

Totals, before any tick: 0 of 1 MILLS POWERED, WARNED:
0 MILLS, POWER: 2 LIVE NETWORKS, 3 LONE SOURCES. Every
cell's power state reads None before the tick — the Step 1
Conduit and its transformer, both live-control
transformers, the lone Conduit, the Step 2 mill, and the
dead cable — because sources and transformers are never
tinted by decision, and a dead component carries no
voltage to tint a cable with. The tint and the count
therefore agree in the negative as well as the positive:
nothing here claims a delivery that reaches no cell.

Tick 1 changes nothing, and that stability is the
composition's point, not a missing phase: no mill in
this wiring is mismatched, so none is destroyed; none is
powered, so powered gathered stays 0; the Step 2 mill is
not due a kinetic gather on tick 1, so kinetic gathered
stays 0; stone stays 0; and the POWER line still reads
2 LIVE NETWORKS, 3 LONE SOURCES. A demo that staged a
mismatched mill would prove destruction, not counting,
and Slice 17 already proved destruction beside its own
counts.

Save/load round-trip, under the derived-never-saved rule:

- Every staged cell loads: the Step 1 Conduit and its
  transformer, both live-control transformers, the lone
  Conduit, the Step 2 mill, and the dead cable.
- The counts re-derive as 2 LIVE NETWORKS, 3 LONE
  SOURCES after load, with 0 of 1 mills powered, 0 warned,
  and the Step 1 Conduit, the dead cable, and the Step 2
  mill still reading None in PowerState.
- Session counters reset to 0 on load (powered, kinetic,
  destroyed, stone — all already 0, so the reset is
  proved by the counters' values, not by a change), and
  the player loads at (16,5,16).
- Loaded tick 1 re-proves the same stability: the Step 2
  mill still survives, the counts still read 2 / 3, and
  all three counters still read 0. The loaded field does
  not arrive with a saved count; it computes one.
- The new scenario's save is 43,965 chars. That length is
  quoted here as the Slice 19 entry, not as a change to
  any earlier save, and not as a claim about why it
  differs from any other length: the earlier lengths below
  were read from the same demo run and are all unchanged,
  so no save rule moved in this step.

## What the review verified this run

- Full test suite re-run before writing: **2810 passed, 0
  failed** (was 2733; +77, no failures on the final runs).
  The Step 2 assertions are composed-integration tests
  mirroring the demo layout above, inserted before the test
  runner's summary so they execute — the Slice 13 Step 4
  lesson, that tests appended after the runner's return do
  not run, is observed here, and the count rise from 2733
  to 2810 is the check that they did.
- Headless demo re-run, save lengths as a no-core-change
  check for the earlier scenarios and as the new entry for
  Slice 19: Slice 1 unchanged (save 43,881 chars); Slice 2
  unchanged (save 43,921 chars); Step 1 scenario unchanged
  (save 44,016 chars); Step 3 scenario unchanged (save
  44,103 chars); Slice 4 scenario unchanged (save 44,026
  chars); Slice 5 Step 2 scenario unchanged (save 43,974
  chars); Slice 6 Step 4 scenario unchanged (save 44,024
  chars); Slice 7 Step 2 scenario unchanged (save 44,013
  chars); Slice 8 Step 3 scenario unchanged (save 43,974
  chars); Slice 9 Step 4 scenario unchanged (save 43,975
  chars); Slice 10 Step 3 scenario unchanged (save 43,974
  chars); Slice 11 Step 3 scenario unchanged (save 44,038
  chars); Slice 12 Step 3 scenario unchanged (save 43,974
  chars); Slice 13 Step 4 scenario unchanged (save 44,045
  chars); Slice 14 Step 2 scenario unchanged (save 43,974
  chars); Slice 15 Step 2 scenario unchanged (save 44,016
  chars); Slice 17 Step 3 scenario unchanged (save 43,974
  chars); Slice 18 Step 3 scenario unchanged (save 44,015
  chars); Slice 19 Step 2 scenario new (save 43,965 chars).
  A changed earlier save length would have meant a save
  rule moved; none did. Slice 19 adds no save field — the
  counts are derived on every read — so the unchanged
  earlier lengths are consistent with the decisions, not
  just coincident with them.
- Slice 19 gate, checked against `docs/slice-19-scope.md`:
  every step pushed with passing tests (for Step 1, with
  the suite risen by the proof-set tests, never standing
  still on a query step, and for Step 2, with the suite
  risen by the composed tests); the Step 1 count decision
  is recorded with rationale in DECISIONS.md, with its own
  Windows checklist item (item 38); no delivery,
  precedence, conflict, rating, cost, or phase rule changed
  in any step; no material, marker, light, fog, model, or
  HUD rule changed in any step except the count line's
  decided question itself, folded into the existing POWER
  line's lone number; every earlier demo save length
  unchanged, with the new Slice 19 length quoted as its own
  entry; and this review records the verdict honestly,
  separating headless-tested count claims from the
  Windows-checklist count observation, including that
  Slice 19 is still not playtested and not
  Unity-verified. The Slice 1 director playtest gate is
  still pending throughout and is not closed by Slice 19.
- Checklist relations, stated so a later reader does not
  miscount them: item 38 is the post-decision
  dead-transformer observation (a Conduit beside a
  transformer alone, a Dynamo beside a transformer alone,
  and a lone Conduit, with the POWER line read aloud for
  each). Item 38 extends item 36 (whose direct-Conduit
  mill, workstation, and pylon readings stand) and extends
  item 22 (whose live-network and lone-source display
  stands), and supersedes neither. This review adds no new
  item, because the review exercises no behaviour item 38
  does not already cover. The next free playtest-gate
  number remains 39.
- One claim split stated plainly, so it is not over-claimed
  later: the headless evidence for Slice 19 is count pairs
  for staged layouts, cell power states, delivery and
  powered/mismatched predicates for the one staged mill,
  counter values across ticks, and a save/load round-trip
  that re-derives the counts, plus a demo that prints
  those values for six rows in one field. None of that
  proves a Conduit beside a transformer alone reads
  0 LIVE, 1 LONE on a HUD line in a built scene, or that
  a player understands the lone number as "this placement
  went nowhere" while looking at the wiring. That is a
  Windows observation (item 38), recorded in DECISIONS.md
  when observed, not closed by a test count.
- No Slice 19 step duplicates logic in a Unity adapter.
  The count lives in core `WorldZone.LivePowerCounts` and
  renders through the existing core `HudState` / adapter
  `LivePowerText` line, which already rendered
  LoneSources — Step 1 needed no adapter edit, and Step 2
  edits no adapter file at all. There is no second count
  rule in any adapter that could drift from the core rule.
- No Slice 19 step changed a saved value's meaning: no
  save field was added, no block, kind, doctrine, source,
  or consumer value moved, and every earlier demo save
  length above is unchanged. The one superseded reading in
  this slice's history — the Slice 17 Step 2 transformer
  world reading 0 / 0, now 0 / 1 under Step 1 — was changed
  with history kept in DECISIONS.md, not deleted silently.
- Hard limits held: 3D real-time only, no turn-based
  mechanics, no twin-stick Asteroids loop, no copied assets,
  lore, code, or trademarks. Slice 19 adds no block, kind,
  doctrine, source, or consumer, so there is nothing new
  in it that could copy one.

## Deliberately NOT in Slice 19 (still open, not forgotten)

- A delivery, precedence, conflict, rating, cost, or phase
  rule change. Counting is what the HUD line shows for a
  wiring shape; delivery is what a consumer does on a
  tick, and no Slice 19 step moves it: a Conduit beside a
  transformer still delivers nothing directly, a
  transformer beside a Conduit alone still outputs None,
  and no consumer's powered, mismatched, or warned
  predicate moves. If a future step finds its honest answer
  needs a Conduit transformer branch, that is a delivery
  step with its own decision, deferred here.
- A material, shader, marker, light, fog, or view model
  change. The Slice 16 surface renders whatever Slice 19
  decides; a step that re-paletted a tint or retuned dusk
  to make a count read better would have confused the
  surface with the query.
- A HUD redesign. The count step decided one POWER line
  question (fold the changed lone count into the existing
  line) and nothing else: no new HUD colour, no new HUD
  layout, no quest list, no second POWER line.
- A new block, kind, doctrine, source, or consumer, a
  movement, targeting, or save rule change, a balance pass,
  or a price or cap change. Slice 19 is one display query
  and its review, and the core stands exactly as Slice 18
  left it except for the query itself.
- A patrol recall, re-rally, or player action for a stuck
  patrol, a patrol radius or waypoint change, or any other
  Slice 18 gap 6 decision, as a bonus inside a count step.
  Each is its own decision with its own tests, carried
  after Slice 19, not patched in beside a count.
- A scene-proof observation presented as a headless result.
  Items 25–27 and 30–38 are Windows observations, recorded
  in DECISIONS.md when observed, not closed by a document,
  a script, or a test count.
- A scene file (`.unity`), an imported asset, a texture, or
  a shader file written by a headless run and claimed as a
  built scene. Scene assembly remains Windows checklist
  work.
- A copied asset, model, skyline, HUD layout, or
  trademarked look from any other game.
- Windows Unity verification itself. It remains open for
  Slices 1–19 alike and is blocked on Unity 6000 on Windows,
  not on anything this slice can do headlessly.
- A Slice 20 scope. Slice 20 is scoped after the Slice 19
  review, not inside it.

## Gaps this review names, in priority order

1. **Windows Unity verification, Slices 1–19.** The
   checklists in `docs/slice-01-playtest-gate.md`,
   `docs/orbital-view.md`, `docs/orbital-command.md`, and
   `docs/powered-presence.md` remain the verification plan.
   Slice 19 adds item 38 to that plan, alongside the
   still-open items 25, 26, 27, and 30–37. None of these
   items is claimed verified by any test count in this
   review, and scene assembly is still owed in Unity on
   Windows, not in this repo.
2. **The Slice 1 director playtest gate is still pending.**
   Rob has not yet played Slice 1 in Unity on Windows or
   answered the gate question (fun without extra systems?).
   Slice 19 does not close that gate, and its test count
   must not be quoted as a playtest result.
3. **First-pass graphics values are unproven, not just
   unverified.** Carried from Slice 16: the dusk sun's
   azimuth 235, intensity 1.1, and warm colour; the fog
   density 0.008 and its about-83%-contrast arithmetic at
   the far band; the view model offset 0.22, -0.22, 0.60;
   and the static, weapon-agnostic model itself are
   first-pass values for readability and testability. A
   Windows observation may find the dusk too warm for the
   cyan/green separation, the fog too thick at the far
   band, or the model too large in frame — and that
   observation, not this review, justifies retuning, with
   its own changelog entry. Slice 19 adds a reason this
   matters: the POWER line's lone number must read under
   that dusk light, and only item 38's observation can say
   whether a player reads 0 LIVE, 1 LONE as a placement
   that went nowhere.
4. **The Slice 13 scene-proof observations are still unmade,
   as observations.** Carried from Slice 16: whether the
   chunk material actually multiplies vertex colours (item
   26), whether the adapter refreshes the same renderer
   GameManager links (item 27, with item 31 observing the
   null-fill beside it), whether markers read through
   occlusion (item 25, with item 32 extending it to the
   Step 1 material), and now whether the dusk scene reads
   (item 33), the view model sits correctly in frame (item
   34), the decided tint reads (item 35), the decided
   count reads (item 36), the decided patrol route reads
   (item 37), and the decided dead-transformer count reads
   (item 38), are all still to be observed on Windows. If
   any observation fails, that failure is a fix step,
   recorded as its own decision — not a reason to
   re-scope the proof layer blindly, and not closed by this
   review.
5. **The patrol route's stated limits, as decided, still
   leave open questions a later step may take.** Carried
   from Slice 18 gap 6: a patrol whose loop is permanently
   blocked still cycles its index forever, reading Stuck,
   with no recall, no re-rally, and no player action
   defined for it — those are new decisions, not gaps in
   Slice 18 Step 1's proof, and none is smuggled into this
   review as a promise. The radius (3) and the
   five-waypoint loop are still first-pass values, and if a
   later step finds the radius is the cause of a block,
   that finding is a gap, not a quiet widening.

**Closed by this slice, not carried:** the
transformer-adjacency invisibility in the Slice 18 review's
gap 5 — a Conduit beside a transformer alone reading
0 LIVE, 0 LONE — is now decided, with tests, with a Windows
checklist item for the count observation, and proved
together with its live, lone, and dead-cable controls in
one demo, so the last Slice 17 display-query remainder is
closed in that sense, not left in the numbered gaps above.
The observation in gaps 1 and 4 above is a remainder about
whether the decided count reads correctly in a built
scene, not a reopening of whether the decision exists.

## Slice 19 verdict (headless)

Slice 19 is **reviewed and closed as one display-query
remainder decided with tests and proved together in one
demo (a Conduit beside a dead transformer alone counts
0 LIVE, 1 LONE, folded into the existing POWER line, while
a Dynamo-fed and an Alternator-fed transformer group each
read 1 LIVE, 0 LONE beside it, a lone Conduit and a
Conduit beside a mill each read 0 LIVE, 1 LONE beside
them, and a Dynamo-plus-Conduit mixed-voltage cable reads
0 LIVE, 0 LONE beside all of them — and the whole field,
saved and loaded, re-derives 2 LIVE NETWORKS, 3 LONE
SOURCES, stable across ticks because no mill here is
mismatched)**. It is not playtested, not Unity-verified,
and not a visual result, and its count claims must not be
quoted as if the test count covered the scene reading in
item 38.
**Slice 20 is not scoped by this review** — it is scoped
afterwards, the same way this document follows the Slice 18
review rather than preceding it.

## Ranked candidates carried after Slice 19 (starting point, not promised)

Carried from the 0.81.0 scope, in the same order, with the
Slice 19 work taken:

1. The patrol route's open questions, carried from the
   Slice 18 review's gap 6 — a recall, re-rally, or player
   action for a permanently blocked patrol reading Stuck,
   or a radius / waypoint first-pass value revisited on
   evidence, each as its own decision with its own tests,
   not promised here.
2. Any Slice 13 remainder that Slice 19 does not itself
   settle as an observation (the vertex-colour material
   observation, the MeshRenderer wiring observation, and
   the marker-occlusion observation in gap 4, plus the
   dusk and view model observations in items 33 and 34,
   plus the tint, count, and patrol observations in items
   35, 36, and 37, plus the new item Step 1 adds in item
   38) — if those observations are still unmade when the
   review is written, they remain observations to be made
   when Windows verification runs, not steps to be
   re-scoped blindly, and not promised here.
3. First-pass graphics values retuned only on Windows
   evidence (dusk azimuth, fog contrast, view model offset,
   static weapon-agnostic model) — a retuning step is
   justified by an observation, not by a headless
   re-decision, and is not promised here.
