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
    private Mesh? _mesh;
    private MeshCollider? _collider;

    void Awake()
    {
        _mesh = new Mesh { name = "ForgeborneChunk" };
        GetComponent<MeshFilter>().mesh = _mesh;
        _collider = GetComponent<MeshCollider>();
    }

    // Call this when the chunk changes (gather / build / destruction).
    // In Slice 1, re-meshing the whole 32^3 chunk is acceptable; chunk
    // dirty-region meshing is a later optimisation, not a Slice 1 gate.
    // Slice 13 Step 3: this refresh is now the tinted refresh, not
    // an untinted one — it builds with WorldZone.PowerState, so
    // every quad carries its cell's power tint, and the mesh's
    // vertex colours carry it to the renderer. No second mesh:
    // the colours are one more buffer on the same MeshData.
    // Honest material limit, not hidden: mesh.colors only shows
    // if the chunk material multiplies vertex colours. Wire a
    // vertex-colour material in the scene; with a material that
    // ignores vertex colours the mesh renders untinted, and
    // playtest-gate item 26 is the observation that settles
    // which material the scene actually uses. Untinted vertices
    // are white, so a vertex-colour material leaves ordinary
    // stone, ore, and wood exactly as before.
    public void RefreshMesh()
    {
        if (Simulation == null || _mesh == null) return;
        var data = MeshBuilder.BuildFromChunk(Simulation.World.Chunk, Simulation.World.PowerState);
        var verts = new Vector3[data.Vertices.Count];
        var norms = new Vector3[data.Normals.Count];
        var uvs = new Vector2[data.Uvs.Count];
        var cols = new Color[data.Colors.Count];
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = new Vector3(data.Vertices[i].Item1, data.Vertices[i].Item2, data.Vertices[i].Item3);
            norms[i] = new Vector3(data.Normals[i].Item1, data.Normals[i].Item2, data.Normals[i].Item3);
            uvs[i] = new Vector2(data.Uvs[i].Item1, data.Uvs[i].Item2);
            cols[i] = new Color(data.Colors[i].Item1, data.Colors[i].Item2, data.Colors[i].Item3, data.Colors[i].Item4);
        }
        _mesh.Clear();
        if (_collider != null) _collider.sharedMesh = null;
        _mesh.vertices = verts;
        _mesh.normals = norms;
        _mesh.uv = uvs;
        _mesh.colors = cols;
        _mesh.triangles = data.Indices.ToArray();
        _mesh.RecalculateBounds();
        // Re-attach after the clear/rebuild, so the raycast collider
        // matches the freshly built mesh instead of the cleared one.
        if (_collider != null) _collider.sharedMesh = _mesh;
    }
}
