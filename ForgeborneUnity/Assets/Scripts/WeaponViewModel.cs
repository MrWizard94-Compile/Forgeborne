// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// Slice 16 Step 4: the first FPS weapon view model, decided 0.71.0
// (rationale in DECISIONS.md), to docs/art-direction.md rule 3:
// the weapon occupies the lower-right frame, with a readable
// digital sight in spirit; energy weapons glow, kinetic weapons
// read worn and industrial, not pristine. This model is the
// energy side of that rule: worn dark metal with a cyan
// energy-glow accent and one small orange accent panel, original
// primitives only — no imported art, no copied rifle, no
// pixel-for-pixel copy of the reference sight layout (the
// reference sight's AMMO / TARGET readout is NOT reproduced
// here; numeric ammo lives in one place only, see below).
// Form, decided in this step: a SCRIPT-BUILT model, following
// the SquadMarkerDisplay pattern of original primitives built
// in code, NOT Inspector-wired parts. The scope offered both,
// and script-built wins for the same reason the markers are
// script-built: a headless run writes no scene file and no
// part assets, so Inspector-wired parts would be a wiring
// note with no model behind it, while a script builds the
// same model in any scene that wires the one anchor field.
// The script builds once, at Start, and never rebuilds:
// a second build would duplicate the model in the frame,
// and a per-frame build would allocate like the pre-pool
// marker pattern this project deliberately left behind.
// Adapter line, decided in this step: the model is parented
// to the camera the adapter already moves. Wire CameraAnchor
// in the Inspector to the GameObject carrying FpsInputAdapter
// (the camera object itself). FpsInputAdapter sets that
// transform every Update from the core Look forward vector
// and the FPS controller position, so a child model inherits
// aim and movement with NO duplicated camera math, NO
// FpsInputAdapter edit in this step, and NO GameManager
// field — a view model needs no Simulation, by the
// DuskLighting precedent that a script needing no core
// state takes no GameManager ceremony. If CameraAnchor is
// unwired, this script builds NOTHING and logs a warning
// naming that the model will not appear: an unwired
// anchor must be a visible absence, never a rifle
// floating at the world origin pretending to be a view
// model. The model root sits at local (0.22, -0.22, 0.60)
// on the camera: right, down, and in front, so the
// crosshair at frame centre stays clear, and every part
// stays in front of the default near plane (rearmost
// part at about 0.39 in front of the camera).
// Geometry, first-pass and deliberately simple (all sizes
// in Unity units, +Z forward, camera-local):
//   receiver  dark cube, 0.09 x 0.11 x 0.40, at the root
//   barrel    dark cylinder, 0.04 wide x 0.28 long,
//             forward of the receiver
//   stock     dark cube, 0.08 x 0.10 x 0.14, behind
//   grip      dark cube, 0.07 x 0.12 x 0.06, below and
//             slightly tilted, so the shape reads as
//             a held weapon rather than a bare box
//   energy strip  bright cyan thin cube along the top
//             of the receiver — the energy glow, as
//             colour, not bloom: no post-processing
//             effect is claimed here
//   sight housing  dark cube on top of the receiver,
//             with one small bright cyan dot at its
//             front — a readable sight in the
//             art-direction's spirit, a shape and a
//             glow point, not a copied digital layout
//   accent panel  small orange cube on the right side
//             of the receiver — the secondary accent
//             from the palette, kept small so orange
//             stays an accent and never competes with
//             the orange objective / rally markers
//   "worn" here means dark, matte, industrial metal,
//   not a bright pristine finish; no texture, no
//   decal, and no weathering pass is claimed —
//   weathering is a later art step, not a colour
//   value in this one.
// Data, decided in this step: this model renders NO
// state of its own and carries NO number. Numeric ammo
// — name, mag, reserve — stays exactly where it
// already is, in HudDisplay's AmmoText, read from
// HudState the way HudDisplay already reads it, so
// the model and the HUD ammo line can never
// contradict each other: there is no second count
// to disagree. That is why this script takes NO
// Simulation field, deliberately: a field read
// every frame and discarded would be fake
// core-reading, a field driving a made-up glow
// rule would be a second weapon rule, and a
// HudState poll every frame would double the
// 32^3 snapshot scan HudDisplay already pays.
// The model's geometry is WEAPON-AGNOSTIC, stated
// plainly: one original shape for the Pulse Rifle
// and for any GeneratedWeapon the player equips.
// A generated weapon differs in core stats (name,
// damage, mag size), not in this model's shape, so
// equipping one changes the HUD ammo line and
// nothing about the model — a per-weapon model is
// a later slice, not a promise smuggled into this
// step, and the first model must never be read as
// "the Pulse Rifle's own shape" that other weapons
// are expected to share by identity.
// Motion, deferred openly: this is a STATIC model.
// No sway, no bob, no recoil is included, because
// each is a motion rule that needs tuning against
// the camera motion FpsInputAdapter already owns
// and a Windows observation of its own; including
// any of them unobserved would be polish claimed
// without evidence. The static first-pass proves
// the frame placement, the sight readability, and
// the non-obstruction that item 34 observes, and
// motion is the next view-model decision, not a
// hidden part of this one.
// Save / load, decided in this step: the model
// holds no state, so a save/load round-trip
// cannot duplicate it and cannot reset ammo
// through it. GameManager.LoadFromJson swaps the
// world and weapon inside the same Simulation and
// never recreates this script's GameObject, so
// the model stays one model in the frame and the
// HUD ammo line re-reads from the restored weapon.
// Item 34 observes exactly that: model still
// present once after load, HUD ammo matching the
// save, not refilled and not duplicated.
// Known limit, stated not hidden: a camera-child
// model intersects world geometry when the player
// stands closer to a wall than the model's depth
// (about 0.6 in front of the camera). No viewmodel
// layer and no second depth camera is added here:
// that is a camera-system decision for a later
// step, and item 34 observes the clipping honestly
// rather than this step pretending a static parent
// cannot clip. Item 34 also observes the HUD
// overlap question the static placement raises:
// the model sits lower-right, where the AmmoText
// line also lives, so the observation must confirm
// the model does not cover the ammo line, the
// picker, or the objective readout — placement
// arithmetic is not that observation.
// This step touches NOTHING else: no core file, no
// MeshBuilder colour, no PowerTint value, no
// PowerState query, no refresh event set, no HUD
// layout or colour, no marker colour, shape, size,
// or position, no light, sky, or fog value, no
// scene file, no shader file, no imported asset,
// and no camera change — the camera stays
// FpsInputAdapter's, and this script only rides it.
using UnityEngine;

public class WeaponViewModel : MonoBehaviour
{
    // Wire this to the GameObject carrying FpsInputAdapter
    // (the camera object). Unwired, no model is built and
    // a warning logs — see the header.
    public Transform? CameraAnchor;

    // First-pass frame placement, decided 0.71.0 — see header.
    // Right, down, in front of the camera; crosshair clear.
    public Vector3 ViewOffset = new Vector3(0.22f, -0.22f, 0.60f);

    private bool _built;
    private Material? _darkMetalMaterial;
    private Material? _energyMaterial;
    private Material? _accentMaterial;

    private Material LazyMaterial(ref Material? slot, Color color)
    {
        if (slot == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            slot = new Material(shader) { color = color };
        }
        return slot;
    }

    private Material DarkMetal() => LazyMaterial(ref _darkMetalMaterial, new Color(0.15f, 0.15f, 0.17f));
    private Material EnergyGlow() => LazyMaterial(ref _energyMaterial, Color.cyan);
    private Material Accent() => LazyMaterial(ref _accentMaterial, new Color(1f, 0.55f, 0f));

    private GameObject AddPart(GameObject root, string name, PrimitiveType shape, Material material, Vector3 localPosition, Vector3 localScale, Vector3? localEuler = null)
    {
        var part = GameObject.CreatePrimitive(shape);
        part.name = name;
        part.GetComponent<Renderer>().sharedMaterial = material;
        // Display only: a view-model part must never block
        // the picker raycast or the player's movement, by
        // the SquadMarkerDisplay precedent — colliders are
        // removed on creation, so the centre-screen ray
        // the picker casts from the camera passes the
        // model instead of hitting it.
        Destroy(part.GetComponent<Collider>());
        part.transform.SetParent(root.transform, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        if (localEuler.HasValue) part.transform.localRotation = Quaternion.Euler(localEuler.Value);
        return part;
    }

    void Start()
    {
        if (_built) return;
        if (CameraAnchor == null)
        {
            Debug.LogWarning("Forgeborne weapon view model: no CameraAnchor wired — no model is built, so the frame shows no weapon rather than a rifle floating at the world origin. Wire CameraAnchor to the GameObject carrying FpsInputAdapter — see playtest-gate item 34.");
            return;
        }
        var root = new GameObject("ForgeborneWeaponViewModel");
        root.transform.SetParent(CameraAnchor, false);
        root.transform.localPosition = ViewOffset;
        root.transform.localRotation = Quaternion.identity;

        // Receiver, barrel, stock, grip: worn dark metal.
        AddPart(root, "Receiver", PrimitiveType.Cube, DarkMetal(), new Vector3(0f, 0f, 0.05f), new Vector3(0.09f, 0.11f, 0.40f));
        // Cylinder axis is Y; rotating X 90 degrees lays
        // the barrel along +Z, forward of the receiver.
        AddPart(root, "Barrel", PrimitiveType.Cylinder, DarkMetal(), new Vector3(0f, 0.01f, 0.32f), new Vector3(0.04f, 0.28f, 0.04f), new Vector3(90f, 0f, 0f));
        AddPart(root, "Stock", PrimitiveType.Cube, DarkMetal(), new Vector3(0f, -0.01f, -0.20f), new Vector3(0.08f, 0.10f, 0.14f));
        AddPart(root, "Grip", PrimitiveType.Cube, DarkMetal(), new Vector3(0f, -0.10f, -0.08f), new Vector3(0.07f, 0.12f, 0.06f), new Vector3(20f, 0f, 0f));
        // Cyan energy strip along the top of the receiver,
        // and the sight: a dark housing with one small
        // bright cyan dot at its front.
        AddPart(root, "EnergyStrip", PrimitiveType.Cube, EnergyGlow(), new Vector3(0f, 0.065f, 0.05f), new Vector3(0.02f, 0.015f, 0.30f));
        AddPart(root, "SightHousing", PrimitiveType.Cube, DarkMetal(), new Vector3(0f, 0.085f, 0.08f), new Vector3(0.05f, 0.04f, 0.10f));
        AddPart(root, "SightDot", PrimitiveType.Cube, EnergyGlow(), new Vector3(0f, 0.09f, 0.16f), new Vector3(0.015f, 0.015f, 0.015f));
        // One small orange accent panel on the receiver's
        // right side: secondary accent, kept small.
        AddPart(root, "AccentPanel", PrimitiveType.Cube, Accent(), new Vector3(0.05f, 0f, 0.02f), new Vector3(0.01f, 0.04f, 0.14f));

        _built = true;
    }
}
