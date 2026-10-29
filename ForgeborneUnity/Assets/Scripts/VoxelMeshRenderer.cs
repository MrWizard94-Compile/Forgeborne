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
    public void RefreshMesh()
    {
        if (Simulation == null || _mesh == null) return;
        var data = MeshBuilder.BuildFromChunk(Simulation.World.Chunk);
        var verts = new Vector3[data.Vertices.Count];
        var norms = new Vector3[data.Normals.Count];
        var uvs = new Vector2[data.Uvs.Count];
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = new Vector3(data.Vertices[i].Item1, data.Vertices[i].Item2, data.Vertices[i].Item3);
            norms[i] = new Vector3(data.Normals[i].Item1, data.Normals[i].Item2, data.Normals[i].Item3);
            uvs[i] = new Vector2(data.Uvs[i].Item1, data.Uvs[i].Item2);
        }
        _mesh.Clear();
        if (_collider != null) _collider.sharedMesh = null;
        _mesh.vertices = verts;
        _mesh.normals = norms;
        _mesh.uv = uvs;
        _mesh.triangles = data.Indices.ToArray();
        _mesh.RecalculateBounds();
        // Re-attach after the clear/rebuild, so the raycast collider
        // matches the freshly built mesh instead of the cleared one.
        if (_collider != null) _collider.sharedMesh = _mesh;
    }
}
