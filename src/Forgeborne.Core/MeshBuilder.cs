namespace Forgeborne.Core;

/// <summary>
/// Turns greedy MeshQuads into the exact buffers a Unity Mesh needs:
/// positions, normals, UVs, triangle indices, and — since Slice 13
/// Step 3 — vertex colours. Pure C#, no UnityEngine.
/// This is the build-around for "Unity / GPU is not available in the
/// headless dev environment": the mesh data is fully verified here,
/// and the Unity adapter (later) is only: mesh.vertices = positions,
/// mesh.normals = normals, mesh.uv = uvs, mesh.triangles = indices,
/// mesh.colors = colors. No second mesh representation is allowed:
/// the colour channel is one more parallel list on this same
/// MeshData, not a second mesh, and a quad's colour comes from
/// its own PowerState, so no per-machine material split is needed.
/// </summary>
public sealed class MeshData
{
    public List<(float, float, float)> Vertices { get; } = new();
    public List<(float, float, float)> Normals { get; } = new();
    public List<(float, float)> Uvs { get; } = new();
    /// <summary>Slice 13 Step 3: one RGBA colour per vertex, in the
    /// same order as Vertices, so Colors.Count always equals
    /// Vertices.Count. Every vertex of a quad shares that quad's
    /// PowerTint colour, and an untinted quad is white (1, 1, 1,
    /// 1), which leaves a material's own colour unchanged.</summary>
    public List<(float, float, float, float)> Colors { get; } = new();
    public List<int> Indices { get; } = new();
    public int QuadCount { get; set; }
}

public static class MeshBuilder
{
    public static MeshData Build(IReadOnlyList<MeshQuad> quads)
    {
        var data = new MeshData { QuadCount = quads.Count };
        foreach (var q in quads)
        {
            int baseIndex = data.Vertices.Count;
            var corners = q.Corners();
            // Winding: positive faces counter-clockwise when viewed from outside,
            // negative faces reversed, so back-face culling works in Unity.
            int[] order = q.Positive ? new[] { 0, 1, 2, 0, 2, 3 } : new[] { 0, 2, 1, 0, 3, 2 };
            float[] normal = new float[3];
            normal[q.Axis] = q.Positive ? 1f : -1f;
            var normalTuple = (normal[0], normal[1], normal[2]);
            // UVs: u across the quad width, v across the height, in voxel units,
            // so a texture atlas can map per block type later without re-meshing.
            (float, float)[] uvs = { (0f, 0f), (1f, 0f), (1f, 1f), (0f, 1f) };
            // UVs must follow corner order, not a fixed order, when the quad
            // is on a different axis: corners are already ordered by Corners(),
            // so assign UVs by corner index in that order.
            var colour = PowerTint.ColorFor(q.PowerState);
            for (int i = 0; i < 4; i++)
            {
                data.Vertices.Add(corners[i]);
                data.Normals.Add(normalTuple);
                data.Uvs.Add(uvs[i]);
                data.Colors.Add(colour);
            }
            foreach (var idx in order) data.Indices.Add(baseIndex + idx);
        }
        return data;
    }

    public static MeshData BuildFromChunk(VoxelChunk chunk) => Build(Mesher.GreedyMesh(chunk));

    /// <summary>Slice 13 Step 3 tinted build: the same single
    /// mesh, meshed with a per-cell power-state provider
    /// (pass WorldZone.PowerState), so its quads carry their
    /// cells' tints. With no provider this overload is not
    /// used; the parameterless build above stays the untinted
    /// path, and its colours are all white.</summary>
    public static MeshData BuildFromChunk(VoxelChunk chunk,
        Func<int, int, int, CellPowerState> powerState) =>
        Build(Mesher.GreedyMesh(chunk, powerState));
}
