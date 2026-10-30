// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// Renders 3D scene markers from Forgeborne.Core, Slice 6 Step 2.
// No squad logic lives here: it renders OrbitalView.Snapshot only —
// one marker per live squad, one marker per squad rally, and one
// pending-rally marker while the picker is open — and it moves
// nothing. Marker set, decided 0.29.0 (rationale in DECISIONS.md):
//   squad marker  = cyan cube, 0.6 units, at the squad cell centre
//   rally marker  = orange flat cube, 0.9 x 0.12 x 0.9, at the
//                   bottom of the rally cell, so it stays visible
//                   under a squad standing on its own rally
//   pending marker = orange sphere, 0.6 units, at the pending
//                   rally cell centre, distinct by shape from a
//                   deployed rally so a pick-in-progress cannot
//                   be mistaken for a deployed squad's rally
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
// beyond the snapshot itself. Voxel cells are 1 Unity unit, so a
// cell (x, y, z) centres at (x + 0.5, y + 0.5, z + 0.5).
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

    private readonly List<GameObject> _squadMarkers = new();
    private readonly List<GameObject> _rallyMarkers = new();
    private GameObject? _pendingMarker;
    private Material? _squadMaterial;
    private Material? _rallyMaterial;

    private Material SquadMaterial()
    {
        if (_squadMaterial == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            _squadMaterial = new Material(shader) { color = Color.cyan };
        }
        return _squadMaterial;
    }

    private Material RallyMaterial()
    {
        if (_rallyMaterial == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            _rallyMaterial = new Material(shader) { color = new Color(1f, 0.55f, 0f) };
        }
        return _rallyMaterial;
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
            var squadMarker = EnsureMarker(_squadMarkers, i, PrimitiveType.Cube, SquadMaterial());
            squadMarker.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            squadMarker.transform.position = new Vector3(s.X + 0.5f, s.Y + 0.5f, s.Z + 0.5f);
            squadMarker.SetActive(true);
            var rallyMarker = EnsureMarker(_rallyMarkers, i, PrimitiveType.Cube, RallyMaterial());
            rallyMarker.transform.localScale = new Vector3(0.9f, 0.12f, 0.9f);
            rallyMarker.transform.position = new Vector3(s.RallyX + 0.5f, s.RallyY + 0.1f, s.RallyZ + 0.5f);
            rallyMarker.SetActive(true);
        }
        for (int i = squads.Count; i < _squadMarkers.Count; i++) _squadMarkers[i].SetActive(false);
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
        foreach (var m in _squadMarkers) m.SetActive(false);
        foreach (var m in _rallyMarkers) m.SetActive(false);
        if (_pendingMarker != null) _pendingMarker.SetActive(false);
    }
}
