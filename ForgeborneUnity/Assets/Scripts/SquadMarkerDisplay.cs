// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// Renders 3D scene markers from Forgeborne.Core, Slice 6 Step 2.
// No squad logic lives here: it renders OrbitalView.Snapshot only —
// one marker per live squad, one marker per squad rally, and one
// pending-rally marker while the picker is open — and it moves
// nothing. Marker set, decided 0.29.0 (rationale in DECISIONS.md),
// as distinguished by kind in 0.58.0 (Slice 13 Step 2, rationale
// in DECISIONS.md):
//   Vanguard squad marker = cyan cube, 0.6 units, at the squad
//                           cell centre — UNCHANGED from 0.29.0,
//                           because it is the marker players
//                           already recognise
//   Recon squad marker    = magenta capsule, 0.5 wide x 0.8 tall,
//                           at the squad cell centre — tall thin
//                           silhouette for the scout kind
//   Heavy squad marker    = light-grey cylinder, 0.8 wide x 0.7
//                           tall, at the squad cell centre —
//                           squat metal drum for the anchor kind
//   rally marker  = orange flat cube, 0.9 x 0.12 x 0.9, at the
//                   bottom of the rally cell, so it stays visible
//                   under a squad standing on its own rally
//   pending marker = orange sphere, 0.6 units, at the pending
//                   rally cell centre, distinct by shape from a
//                   deployed rally so a pick-in-progress cannot
//                   be mistaken for a deployed squad's rally
// The distinction is shape FIRST and colour second, by decision:
// all three squad shapes differ (cube / capsule / cylinder), so
// the kinds stay apart for a player who cannot tell the colours
// apart, and colour reinforces the shape rather than carrying it
// alone. No kind colour reads as orange: magenta is a neon-sign
// colour in docs/art-direction.md, light grey is the dark-metal
// palette lightened, and cyan is the existing squad colour — so
// no squad marker is mistaken for an orange rally or pending
// marker. Rally and pending markers are UNCHANGED by this step.
// The Slice 11 Step 1 decision (all kinds share one cyan cube,
// kind read from the text line) is SUPERSEDED by this step: kind
// is now read from the marker as well as from the text line, and
// the text display (OrbitalSquadDisplay, which reads kind from
// the same snapshot) must keep agreeing with the markers, word
// for word. No squad rule changes here: no kind shape, cost, cap,
// movement, targeting, or doctrine rule moves because a marker
// changes.
// Markers are original primitives built here (CreatePrimitive),
// no imported assets, no copied art. Colours follow
// docs/art-direction.md: neon cyan for squads (primary energy /
// HUD colour), orange for rallies and objectives (secondary
// accent). Markers do NOT show through walls, by decision:
// they are ordinary opaque primitives with a standard URP Lit
// material (Standard fallback), so the voxel mesh occludes
// them honestly by depth — a through-wall marker would be a
// wallhack claim, not a readability aid. If Windows testing
// proves occlusion makes markers useless at FPS distance, that
// is a later step's evidence to revisit, not a silent shader
// change here.
// Markers are pooled, not rebuilt per frame: the pool grows to
// the largest squad count seen (at most 3 live squads, so at
// most 3 squad markers, 3 rally markers, and 1 pending marker)
// and unused markers are deactivated, so no per-frame allocation
// beyond the snapshot itself. Squad markers are pooled as SLOTS:
// one parent GameObject per live-squad slot, holding one child
// primitive for that slot's current kind. A deployed squad's
// kind never changes, so the child is swapped only when a slot
// is reused by a later deployment of a different kind after the
// previous squad is gone — never every frame, and never by
// growing the pool past 3 squad markers. An unknown kind falls
// back to the Vanguard cube rather than disappearing, so a
// future kind still renders until its own distinction is decided.
// Voxel cells are 1 Unity unit, so a cell (x, y, z) centres at
// (x + 0.5, y + 0.5, z + 0.5).
// Wire MarkerRoot in the Inspector (any empty GameObject in the
// scene; markers are parented under it so the scene stays tidy)
// and drag the scene's GameManager Simulation into Simulation —
// the same Simulation FpsInputAdapter and OrbitalSquadDisplay
// already use. Do not create a second Simulation for markers.
// See docs/orbital-view.md for the Windows checklist.
using System.Collections.Generic;
using UnityEngine;
using Forgeborne.Core;

public class SquadMarkerDisplay : MonoBehaviour
{
    public Simulation? Simulation;
    public Transform? MarkerRoot;

    // Squad marker slots: one parent per live-squad slot (at most 3),
    // each holding one child primitive for its current kind.
    private readonly List<GameObject> _squadSlots = new();
    private readonly List<SquadKind?> _squadSlotKinds = new();
    private readonly List<GameObject?> _squadSlotVisuals = new();
    private readonly List<GameObject> _rallyMarkers = new();
    private GameObject? _pendingMarker;
    private Material? _vanguardMaterial;
    private Material? _reconMaterial;
    private Material? _heavyMaterial;
    private Material? _rallyMaterial;

    private Material LazyMaterial(ref Material? slot, Color color)
    {
        if (slot == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            slot = new Material(shader) { color = color };
        }
        return slot;
    }

    private Material KindMaterial(SquadKind kind) => kind switch
    {
        SquadKind.Recon => LazyMaterial(ref _reconMaterial, Color.magenta),
        SquadKind.Heavy => LazyMaterial(ref _heavyMaterial, new Color(0.85f, 0.85f, 0.85f)),
        _ => LazyMaterial(ref _vanguardMaterial, Color.cyan),
    };

    private PrimitiveType KindShape(SquadKind kind) => kind switch
    {
        SquadKind.Recon => PrimitiveType.Capsule,
        SquadKind.Heavy => PrimitiveType.Cylinder,
        _ => PrimitiveType.Cube,
    };

    private Vector3 KindScale(SquadKind kind) => kind switch
    {
        // Capsule primitive is 2 tall, 1 wide: 0.5 wide, 0.8 tall.
        SquadKind.Recon => new Vector3(0.5f, 0.4f, 0.5f),
        // Cylinder primitive is 2 tall, 1 wide: 0.8 wide, 0.7 tall.
        SquadKind.Heavy => new Vector3(0.8f, 0.35f, 0.8f),
        _ => new Vector3(0.6f, 0.6f, 0.6f),
    };

    private Material RallyMaterial()
    {
        if (_rallyMaterial == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            _rallyMaterial = new Material(shader) { color = new Color(1f, 0.55f, 0f) };
        }
        return _rallyMaterial;
    }

    private GameObject EnsureSquadMarker(int index, SquadKind kind)
    {
        while (_squadSlots.Count <= index)
        {
            var slot = new GameObject("SquadMarker");
            if (MarkerRoot != null) slot.transform.SetParent(MarkerRoot, false);
            _squadSlots.Add(slot);
            _squadSlotKinds.Add(null);
            _squadSlotVisuals.Add(null);
        }
        // Kind-change swap only: a deployed squad's kind never
        // changes, so this runs when a slot is reused by a later
        // deployment of a different kind, never every frame.
        if (_squadSlotKinds[index] != kind)
        {
            if (_squadSlotVisuals[index] != null) Destroy(_squadSlotVisuals[index]);
            var visual = GameObject.CreatePrimitive(KindShape(kind));
            visual.name = kind + "Marker";
            visual.GetComponent<Renderer>().sharedMaterial = KindMaterial(kind);
            // Markers are display only: they must not block the
            // picker raycast or the player's movement, so their
            // colliders are removed on creation.
            Destroy(visual.GetComponent<Collider>());
            visual.transform.SetParent(_squadSlots[index].transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = KindScale(kind);
            _squadSlotVisuals[index] = visual;
            _squadSlotKinds[index] = kind;
        }
        return _squadSlots[index];
    }

    private GameObject EnsureMarker(List<GameObject> pool, int index, PrimitiveType shape, Material material)
    {
        while (pool.Count <= index)
        {
            var marker = GameObject.CreatePrimitive(shape);
            marker.GetComponent<Renderer>().sharedMaterial = material;
            // Markers are display only: they must not block the
            // picker raycast or the player's movement, so their
            // colliders are removed on creation.
            Destroy(marker.GetComponent<Collider>());
            if (MarkerRoot != null) marker.transform.SetParent(MarkerRoot, false);
            pool.Add(marker);
        }
        return pool[index];
    }

    void Update()
    {
        if (Simulation == null) return;
        var squads = OrbitalView.Snapshot(Simulation.World);
        for (int i = 0; i < squads.Count; i++)
        {
            var s = squads[i];
            var squadMarker = EnsureSquadMarker(i, s.Kind);
            squadMarker.transform.position = new Vector3(s.X + 0.5f, s.Y + 0.5f, s.Z + 0.5f);
            squadMarker.SetActive(true);
            var rallyMarker = EnsureMarker(_rallyMarkers, i, PrimitiveType.Cube, RallyMaterial());
            rallyMarker.transform.localScale = new Vector3(0.9f, 0.12f, 0.9f);
            rallyMarker.transform.position = new Vector3(s.RallyX + 0.5f, s.RallyY + 0.1f, s.RallyZ + 0.5f);
            rallyMarker.SetActive(true);
        }
        for (int i = squads.Count; i < _squadSlots.Count; i++) _squadSlots[i].SetActive(false);
        for (int i = squads.Count; i < _rallyMarkers.Count; i++) _rallyMarkers[i].SetActive(false);
        // Pending rally, while picking: a sphere, so it cannot be
        // mistaken for a deployed rally's flat marker.
        var cmd = Simulation.OrbitalCommand;
        if (cmd.IsPicking && cmd.HasRally)
        {
            if (_pendingMarker == null)
            {
                _pendingMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _pendingMarker.GetComponent<Renderer>().sharedMaterial = RallyMaterial();
                Destroy(_pendingMarker.GetComponent<Collider>());
                if (MarkerRoot != null) _pendingMarker.transform.SetParent(MarkerRoot, false);
            }
            _pendingMarker.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            _pendingMarker.transform.position = new Vector3(cmd.RallyX + 0.5f, cmd.RallyY + 0.5f, cmd.RallyZ + 0.5f);
            _pendingMarker.SetActive(true);
        }
        else if (_pendingMarker != null)
        {
            _pendingMarker.SetActive(false);
        }
    }

    void OnDisable()
    {
        foreach (var m in _squadSlots) m.SetActive(false);
        foreach (var m in _rallyMarkers) m.SetActive(false);
        if (_pendingMarker != null) _pendingMarker.SetActive(false);
    }
}
