# Forgeborne — Art Direction

## Director refinement — 2026-11-02

Rob said the hall is beginning to look good and called it real progress, while
stressing how much remains. He then specified that the meshes must become more
realistic and organic. Retain the emerging hall, palette and atmosphere; improve
shape quality, natural asymmetry, weathered fracture forms, believable material
transitions, weapon/hand anatomy and actor construction. Smooth shading alone
does not meet this request. Inspect actual 3D silhouettes and close-up surfaces.
This accepts a direction, not the finished meshes or the full AAA+ experience.

## Current director gate — 2026-11-01

**Director correction: no blocky world aesthetic.** Rob explicitly challenged
the visible block forms: "In what reality is Bioshock blocky?" He clarified:
"We use modded Minecrafts 'Gather, build, survive, combat, concept'." Its
influence concerns that gameplay loop and systemic freedom. It
does not prescribe cubic terrain, one-metre art modules or a voxel visual style.
Believable architecture, sculpted natural surfaces and detailed original models
are the required presentation. Any historical voxel-readability rule below is
superseded by this correction. Internal spatial data is an implementation detail;
it must not dictate the look of the game or justify visibly wrong collision/aim.
Remove the block presentation itself before seeking another visual acceptance.

**AAA+ is the finished-game target. Current presentation is rejected.** Rob
compared the actual 0.120 player unfavorably to vanilla Minecraft and reiterated
BioShock's atmosphere. The old minimal-art scope below is historical; it does
not authorize primitive placeholder presentation as an accepted milestone.

The next review must use the actual Unity game: cohesive original architecture,
environmental storytelling, authored materials with wear and surface response,
layered practical lighting/depth, properly modeled weapon/actors, animation,
effects and audio. Preserve gameplay readability, canonical collision/aim and
measured PC performance. A dark filter, bloom, decorative clutter or a concept
image alone cannot pass this gate. The entire playable encounter needs cohesion.
Use the established industrial sci-fi palette/setting; do not copy BioShock art,
Rapture/Columbia, names, architecture, trademarks or assets.

## BioShock visual research

Rob requested direct visual references on 2026-11-02 after acknowledging the
texture improvement. The remotely viewed publisher/platform images and concrete
original-art consequences are in [the visual study](bioshock-visual-study.md).
Apply its construction, material, composition and environmental-history lessons
through actual original meshes. Preserve all six equally important influences.

## Reference

Director-provided reference image, 2026-10-29 (Gemini-generated,
supplied by Rob for style only). The image file itself is kept in the
director's media library, not in this repository, and must not be
shipped as a game asset.

## What the reference shows

- **View:** First-person, right hand holding a large sci-fi rifle in
  the foreground, looking over a battlefield toward a futuristic city.
- **Weapon:** "PULSE RIFLE", ammo 42 / 120, with a digital sight
  showing AMMO 42 / 120 and TARGET 450m. Blue energy glow along the
  weapon body, orange accent panels, worn dark metal.
- **HUD (top left):** HEALTH bar, 100 HP, SHIELDS: 85, in cyan / blue.
- **HUD (top right):** OBJECTIVE — "DESTROY CORE - 450m",
  "45% COMPLETED", with an orange progress bar.
- **HUD (bottom left):** MINI-MAP, dark, with red markers and a green
  player arrow.
- **HUD (bottom right):** WEAPON label, pulse rifle icon, 42 / 120.
- **World:** Dark voxel city / ruins, blocky voxel destruction with
  debris cubes flying on the left, smoke and fire, neon signs (cyan,
  magenta, orange) on towers, a large planet / moon in a stormy dusk
  sky, aircraft and tracers overhead.
- **Battle:** A squad of armoured soldiers and several tanks advancing
  through a ruined street toward a fortified gate / core structure.
  This is FPS + RTS in one frame: you are in the fight, and an army
  is fighting with you.

## Style rules derived from it (original art only)

1. **Palette:** Dark greys and blacks for voxel stone / metal, storm
   blue-grey sky at dusk, neon cyan as the primary energy / HUD colour,
   orange as the secondary accent for objectives, warnings, and fire.
   Magenta sparingly, for city signage, not for gameplay-critical UI.
2. **World readability:** Believable continuous architecture and sculpted terrain
   must read clearly at FPS distance. Minecraft-like blocks are rejected by the
   director. Resource seams, machinery, cover and destructible surfaces must be
   identifiable through shape/material/function. Destruction needs convincing
   fracture, debris and material response rather than disappearing cubes.
3. **FPS weapon feel:** Weapon occupies the lower-right frame, with
   a readable digital sight. Energy weapons glow; kinetic weapons
   should read as worn, functional, industrial — not pristine.
4. **HUD discipline:** Health, shields, objective distance and
   completion, mini-map, and ammo — no more. Objective is always
   visible: what to destroy, how far, how complete. No cluttered
   quest lists on screen.
5. **Scale contrast:** Player-scale FPS foreground vs city-scale
   skyline and planet in the sky vs squad / armour scale in the
   mid-ground. The world must feel bigger than the player, and the
   army must feel like it matters, not decoration.
6. **Mood:** Gritty, war-torn, industrial sci-fi — not clean, not
   cartoon, not horror-dark. Readable in daylight dusk, not only in
   darkness. Smoke and fire for atmosphere, but never so much that
   targets and voxel geometry become unreadable.
7. **What not to copy:** Do not copy the exact rifle design, HUD
   layout pixel-for-pixel, city skyline, or any trademarked look
   from the reference or from other games. Use it as a mood and
   composition target, then design original Forgeborne art to that
   target.

## Slice 1 art scope

Slice 1 needs only: one voxel material set (stone / metal / terrain),
one FPS weapon view model (simple, original), one enemy silhouette,
and a minimal HUD (health, shields, objective, ammo, mini-map
placeholder). Full city, planet, squad, and tank art are later slices,
not Slice 1. Art that is not in Slice 1 must not block Slice 1's
gameplay gate.

## Slice 16 Step 3 — first-pass dusk light (0.70.0)

Rules 1–7 above are UNCHANGED by this note; these are the
first-pass values that implement rules 1 and 6 in the scene,
decided in Slice 16 Step 3 (rationale in DECISIONS.md) and
observed on Windows by playtest-gate item 33, not by this note:

- Directional dusk sun, warm (1.00, 0.82, 0.60), intensity 1.1,
  low sun about 15 degrees above the horizon, azimuth 235
  first-pass, soft shadows — set in code by
  `ForgeborneUnity/Assets/Scripts/DuskLighting.cs`, whose
  public fields are the tuning surface.
- Flat ambient, storm blue-grey (0.32, 0.38, 0.46), so shadow
  sides read blue-grey instead of falling to black.
- Fog, exponential, density 0.008, storm blue-grey
  (0.45, 0.52, 0.60) — depth beyond the fight, not a veil
  over the far observation band (about 24 cells).
- Sky gradient, editor-built material (no shader file from a
  headless run, no planet, no skyline, original only):
  zenith storm blue-grey (0.20, 0.26, 0.34), horizon
  lightened storm blue-grey (0.52, 0.54, 0.58). If no
  gradient material is wired, the scene keeps its existing
  skybox and item 33 observes that sky, not this spec.

All values are first-pass, chosen for readability and
testability, not final feel. If the light makes a marker,
tint, or HUD colour unread, the light changes — the palette
does not move to fit the light.

## Slice 16 Step 4 — first FPS weapon view model (0.71.0)

Rule 3 above is UNCHANGED by this note; this is the
first-pass model that implements it in the scene,
decided in Slice 16 Step 4 (rationale in DECISIONS.md)
and observed on Windows by playtest-gate item 34, not
by this note:

- Form: script-built from original primitives at Start
  by `ForgeborneUnity/Assets/Scripts/WeaponViewModel.cs`,
  parented to the camera object carrying
  `FpsInputAdapter` (CameraAnchor field); no imported
  art, no copied rifle, no scene file from a headless
  run. If the anchor is unwired, no model is built
  and a warning logs — a visible absence, never a
  rifle floating at the world origin.
- Placement: camera-local (0.22, -0.22, 0.60) —
  lower-right frame, crosshair clear, rearmost part
  about 0.39 in front of the camera.
- Shape: dark worn-metal receiver, barrel, stock, and
  grip; a bright cyan energy strip along the receiver
  and a small cyan sight dot in a dark sight housing;
  one small orange accent panel on the receiver's
  right side. Colour only for the glow — no bloom
  or post-processing effect is claimed.
- Scope: static and weapon-agnostic. No sway, bob, or
  recoil yet, and one shape for every equipped weapon;
  numeric ammo stays in the HUD ammo line only, so
  the model can never contradict it. A known limit:
  the camera-child model clips into a wall closer
  than its own depth — observed by item 34, and a
  later step's camera-system question, not hidden.
