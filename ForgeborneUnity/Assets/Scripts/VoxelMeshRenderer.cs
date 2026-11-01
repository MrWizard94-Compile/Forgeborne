#nullable enable
// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// This script ONLY assigns MeshBuilder buffers to a Unity Mesh. It contains
// no voxel logic, no gather/build rules, and no second chunk representation.
// Canonical state is Forgeborne.Core.WorldZone, via Simulation.
using UnityEngine;
using Forgeborne.Core;

// MeshCollider added in 0.15.0 so the Orbital rally picker's
// crosshair raycast can hit the voxel mesh: without a collider
// the raycast would pass through the rendered chunk and never
// set a rally. The collider shares the same mesh buffers, so it
// adds no second chunk representation.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VoxelMeshRenderer : MonoBehaviour
{
    public Simulation? Simulation;
    // Slice 16 Step 1: the chunk material, decided 0.68.0
    // (rationale in DECISIONS.md). Inspector-wired when a
    // scene author has built a vertex-colour material in
    // the editor — that material wins, and the runtime
    // fallback below never overwrites it, so a deliberate
    // Shader Graph material is not replaced by a stock
    // shader at Play. Unwired, Awake assigns a runtime
    // material from the URP Lit shader, falling back to
    // Standard — the same shader pair SquadMarkerDisplay
    // already uses, and the project already depends on
    // URP 17.0.0 (Packages/manifest.json), so no new
    // shader asset, texture, or imported material rides
    // along. The material is assigned in Awake, before
    // GameManager.Start calls the first RefreshMesh, so
    // the first mesh never renders one frame under an
    // unwired material. Honest limit, not softened by
    // this step: assigning a material is not proving it
    // multiplies vertex colours in Unity 6000 URP 17.0.0
    // — playtest-gate item 26 remains the observation
    // that settles the tint, and item 30 is the
    // observation that settles which shader was
    // actually assigned and whether untinted stone, ore,
    // and wood still render exactly as before. If
    // neither shader is found, the Inspector material
    // is left untouched and a warning logs, rather than
    // a null material silently rendering pink.
    public Material? ChunkMaterial;
    private Mesh? _mesh;
    private MeshCollider? _collider;
    private Material? _runtimeMaterial;

    void Awake()
    {
        _mesh = new Mesh { name = "ForgeborneChunk" };
        GetComponent<MeshFilter>().mesh = _mesh;
        _collider = GetComponent<MeshCollider>();
        var renderer = GetComponent<MeshRenderer>();
        if (ChunkMaterial != null)
        {
            renderer.sharedMaterial = ChunkMaterial;
        }
        else
        {
            var shader = Shader.Find("Forgeborne/VoxelSurface") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader != null)
            {
                _runtimeMaterial = new Material(shader);
                renderer.sharedMaterial = _runtimeMaterial;
            }
            else Debug.LogWarning("Forgeborne chunk material: neither Universal Render Pipeline/Lit nor Standard shader found; chunk renders with its Inspector material, and vertex tints may not show — see playtest-gate items 26 and 30.");
        }
    }

    void OnDestroy()
    {
        if (_collider != null) _collider.sharedMesh = null;
        if (_mesh != null) Destroy(_mesh);
        if (_runtimeMaterial != null) Destroy(_runtimeMaterial);
    }

    // Call this when the chunk changes (gather / build / destruction).
    // In Slice 1, re-meshing the whole 32^3 chunk is acceptable; chunk
    // dirty-region meshing is a later optimisation, not a Slice 1 gate.
    // Slice 13 Step 3: this refresh is now the tinted refresh, not
    // an untinted one — it builds with WorldZone.PowerState, so
    // every quad carries its cell's power tint, and the mesh's
    // vertex colours carry it to the renderer. No second mesh:
    // the colours are one more buffer on the same MeshData.
    // Honest material limit, not hidden, and not closed by
    // Slice 16 Step 1: mesh.colors only shows if the chunk
    // material multiplies vertex colours. Awake now assigns
    // one (Inspector ChunkMaterial, else runtime URP Lit /
    // Standard fallback), so the scene no longer depends
    // on an unwritten Inspector step — but with a material
    // that ignores vertex colours the mesh still renders
    // untinted, and playtest-gate item 26 is the
    // observation that settles whether the assigned
    // material multiplies them, with item 30 recording
    // which shader was actually assigned. Untinted
    // vertices are white, so a vertex-colour material
    // leaves ordinary stone, ore, and wood exactly as
    // before.
    public void RefreshMesh()
    {
        if (Simulation == null || _mesh == null) return;
        var data = MeshBuilder.BuildFromChunk(Simulation.World.Chunk, Simulation.World.PowerState);
        var verts = new Vector3[data.Vertices.Count];
        var norms = new Vector3[data.Normals.Count];
        var uvs = new Vector2[data.Uvs.Count];
        var cols = new Color[data.Colors.Count];
        var types = new Vector2[data.BlockTypes.Count];
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = new Vector3(data.Vertices[i].Item1, data.Vertices[i].Item2, data.Vertices[i].Item3);
            norms[i] = new Vector3(data.Normals[i].Item1, data.Normals[i].Item2, data.Normals[i].Item3);
            uvs[i] = new Vector2(data.Uvs[i].Item1, data.Uvs[i].Item2);
            var tint = new Color(data.Colors[i].Item1, data.Colors[i].Item2, data.Colors[i].Item3, data.Colors[i].Item4);
            cols[i] = SurfaceColor(data.BlockTypes[i]) * tint;
            types[i] = new Vector2((int)data.BlockTypes[i], 0);
        }
        _mesh.Clear();
        if (_collider != null) _collider.sharedMesh = null;
        _mesh.vertices = verts;
        _mesh.normals = norms;
        _mesh.uv = uvs;
        _mesh.colors = cols;
        _mesh.uv2 = types;
        _mesh.triangles = data.Indices.ToArray();
        _mesh.RecalculateBounds();
        // Re-attach after the clear/rebuild, so the raycast collider
        // matches the freshly built mesh instead of the cleared one.
        if (_collider != null) _collider.sharedMesh = _mesh;
    }

    private static Color SurfaceColor(BlockType block)
    {
        switch (block)
        {
            case BlockType.Stone: return new Color(0.22f, 0.27f, 0.30f);
            case BlockType.EmberOre: return new Color(0.86f, 0.29f, 0.07f);
            case BlockType.Wood: return new Color(0.34f, 0.17f, 0.075f);
            case BlockType.Wall: return new Color(0.10f, 0.16f, 0.20f);
            case BlockType.Workstation: return new Color(0.09f, 0.40f, 0.46f);
            case BlockType.Bedrock: return new Color(0.055f, 0.065f, 0.08f);
            default: return new Color(0.22f, 0.32f, 0.38f);
        }
    }
}
