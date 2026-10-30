// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// Slice 16 Step 3: the scene's dusk light, sky, and fog, decided 0.70.0
// (rationale in DECISIONS.md), to docs/art-direction.md rules 1 and 6:
// dark greys and blacks for voxel stone and metal, a storm blue-grey
// sky at dusk, neon cyan primary and orange secondary, readable in
// daylight dusk, not clean, not cartoon, not horror-dark, with smoke
// and fire atmosphere never hiding targets or voxel geometry.
// Form, decided in this step: a STATED COMBINATION, not a pure
// script and not pure Inspector notes. Light, fog, and ambient are
// set here in code, because they are fully code-settable values
// with no scene file needed; the sky is an Inspector-wired
// material, because a gradient skybox needs an editor-built
// material and a headless run writes no shader file and no scene
// file and claims no built scene (Slice 16 scope, deliberately-not
// list). The sky values below are therefore a BUILD SPEC for the
// scene author, recorded here, in docs/art-direction.md, and in
// DECISIONS.md — not values this script applies at runtime.
//   Sky gradient spec (editor-built material, original only):
//     zenith  storm blue-grey (0.20, 0.26, 0.34)
//     horizon storm blue-grey lightened (0.52, 0.54, 0.58)
//     no planet, no skyline, no copied art — the reference
//     image's planet and city remain style only, never assets.
//   If SkyboxMaterial is unwired, this script does NOT invent a
//   skybox: it logs a warning and leaves the scene's existing
//   skybox untouched, so a deliberate sky is never replaced by
//   a runtime guess, and playtest-gate item 33 observes the
//   sky as actually built, not as specified here.
// Light values are public fields, not hidden constants, so the
// scene author tunes through the Inspector without editing code:
// the field values ARE the decided first-pass values, labelled
// first-pass because they are chosen for readability and
// testability, not final feel (Slice 16 scope, doctrine-balance
// precedent). A deliberate scene light is expressed by changing
// these fields, not by a second light the script would fight.
//   Directional dusk sun: warm (1.00, 0.82, 0.60), intensity 1.1,
//     low sun about 15 degrees above the horizon (rotation
//     X 15), azimuth 235 first-pass, soft shadows so voxel
//     edges read by shading as well as by geometry.
//   Flat ambient: storm blue-grey (0.32, 0.38, 0.46), so shadow
//     sides read blue-grey instead of falling to black — the
//     "not horror-dark" half of rule 6, in one value.
//   Fog: exponential, density 0.008, storm blue-grey
//     (0.45, 0.52, 0.60). At the far observation band of about
//     24 cells, exp(-0.008 x 24) is about 0.83, so fog gives
//     depth beyond the fight without hiding far markers;
//     at 100 cells it is about 0.45, skyline depth only.
//     If item 33 finds fog hiding a far marker, that is a
//     fog finding to record and a light/fog value to change
//     — never a reason to re-palette a marker or a tint.
// This script touches NOTHING else: no HUD layout or colour,
// no marker colour or shape, no tint value, no core rule, no
// camera (FpsInputAdapter owns camera movement), and no
// refresh event. If this light makes a colour unread, the
// step changes the light or records the colour as evidence
// to revisit — it does not quietly re-palette the game to
// fit the light, because the palette is art-direction law
// and the light is the new variable. The Step 2 combined
// scene (playtest-gate item 32) is re-observed under this
// light by item 33, which is what settles the separations
// below — this script predicts them, it does not prove them:
// warm light multiplies cyan (0, 1, 1) toward (0, 0.82, 0.60)
// while Powered green (0.25, 0.9, 0.25) goes toward
// (0.25, 0.74, 0.15), so the two stay apart on the blue
// channel; Mismatched red (1, 0.15, 0.15) stays far from
// orange rally (1, 0.55, 0) on the green channel; Energised
// yellow (1, 0.85, 0.1) stays on ground cable cells, never
// floating. Item 33 observes each separation in the scene,
// it does not take this arithmetic as the observation.
// Wire DuskLight in the Inspector to use an existing scene
// light, or leave it unwired and this script creates one
// named ForgeborneDuskLight at Awake. Wire SkyboxMaterial
// to the editor-built gradient material, or leave it unwired
// and see the warning above. Add this script to any
// GameObject in the scene; no GameManager field is needed.
using UnityEngine;

public class DuskLighting : MonoBehaviour
{
    public Light? DuskLight;
    public Material? SkyboxMaterial;

    // First-pass dusk values, decided 0.70.0 — see header.
    public Color SunColor = new Color(1f, 0.82f, 0.60f);
    public float SunIntensity = 1.1f;
    public Vector3 SunRotation = new Vector3(15f, 235f, 0f);
    public Color AmbientColor = new Color(0.32f, 0.38f, 0.46f);
    public Color FogColor = new Color(0.45f, 0.52f, 0.60f);
    public float FogDensity = 0.008f;

    void Awake()
    {
        if (DuskLight == null)
        {
            var sunObject = new GameObject("ForgeborneDuskLight");
            DuskLight = sunObject.AddComponent<Light>();
            DuskLight.type = LightType.Directional;
        }
        DuskLight.color = SunColor;
        DuskLight.intensity = SunIntensity;
        DuskLight.shadows = LightShadows.Soft;
        DuskLight.transform.rotation = Quaternion.Euler(SunRotation);

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = AmbientColor;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = FogDensity;
        RenderSettings.fogColor = FogColor;

        if (SkyboxMaterial != null)
        {
            RenderSettings.skybox = SkyboxMaterial;
        }
        else
        {
            Debug.LogWarning("Forgeborne dusk light: no SkyboxMaterial wired — the scene keeps its existing skybox, and the dusk gradient (zenith 0.20/0.26/0.34, horizon 0.52/0.54/0.58, no planet or skyline) still needs building in the editor. See playtest-gate item 33.");
        }
    }
}
