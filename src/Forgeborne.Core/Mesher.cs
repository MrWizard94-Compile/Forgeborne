namespace Forgeborne.Core;

/// <summary>One merged quad from greedy meshing. Axis: 0=X, 1=Y, 2=Z. Positive = normal in +axis direction.</summary>
public sealed class MeshQuad
{
    public int Axis { get; set; }
    public int Slice { get; set; } // face plane is at Slice+1 on the axis
    public int U0 { get; set; }
    public int V0 { get; set; }
    public int U1 { get; set; } // exclusive
    public int V1 { get; set; } // exclusive
    public BlockType Type { get; set; }
    public bool Positive { get; set; }
    /// <summary>Slice 13 Step 3: the power state of this quad's
    /// source cell, set by GreedyMesh when a provider is given,
    /// None otherwise. MeshBuilder colours every vertex of the
    /// quad from this value, so a quad's colour is uniform by
    /// construction — and, because the merge key below includes
    /// this value, a quad never spans cells of different power
    /// states, so two adjacent mills, one powered and one
    /// mismatched, keep separate top quads in different tints
    /// instead of merging into one ambiguously coloured quad.</summary>
    public CellPowerState PowerState { get; set; } = CellPowerState.None;

    public (float, float, float)[] Corners()
    {
        // Map (axis, u, v) back to (x, y, z). u = (axis+1)%3, v = (axis+2)%3.
        int u = (Axis + 1) % 3, v = (Axis + 2) % 3;
        float[] p0 = new float[3], p1 = new float[3], p2 = new float[3], p3 = new float[3];
        p0[Axis] = p1[Axis] = p2[Axis] = p3[Axis] = Slice + 1;
        p0[u] = U0; p0[v] = V0;
        p1[u] = U1; p1[v] = V0;
        p2[u] = U1; p2[v] = V1;
        p3[u] = U0; p3[v] = V1;
        return new[] { (p0[0], p0[1], p0[2]), (p1[0], p1[1], p1[2]), (p2[0], p2[1], p2[2]), (p3[0], p3[1], p3[2]) };
    }
}

/// <summary>
/// Greedy mesher (standard 0fps-style algorithm, adapted to 32^3 chunks).
/// For each axis and each slice, builds a mask of visible faces, then
/// merges maximal rectangles of the same block type. A single voxel
/// therefore produces 6 quads, not 24 triangles, and a flat 32x32 slab
/// produces 6 quads, not hundreds. This is the CPU-side face count;
/// the Unity step later turns quads into GPU indirect draws.
/// </summary>
public static class Mesher
{
    /// <summary>Greedy mesh, with an optional per-cell power-state
    /// provider (Slice 13 Step 3: pass WorldZone.PowerState).
    /// With no provider every quad's PowerState is None and the
    /// merge behaviour is exactly the pre-Step-3 behaviour, so
    /// every earlier quad-count test stands. With a provider,
    /// the provider is asked only for a face's source cell —
    /// the solid cell the face belongs to, a for a positive
    /// face and b for a negative one — and that state joins
    /// the merge key, so quads never merge across a tint
    /// boundary. The provider is not stored and no second
    /// chunk representation is created: the quads are still
    /// the single chunk representation, now carrying a tint
    /// attribute per quad.</summary>
    public static List<MeshQuad> GreedyMesh(VoxelChunk chunk,
        Func<int, int, int, CellPowerState>? powerState = null)
    {
        var quads = new List<MeshQuad>();
        int s = VoxelChunk.Size;
        for (int axis = 0; axis < 3; axis++)
        {
            int uAxis = (axis + 1) % 3, vAxis = (axis + 2) % 3;
            for (int slice = -1; slice < s; slice++)
            {
                // mask[u, v] = 0 for no face, else (type, positive) encoded as type byte + sign in a parallel array.
                var maskType = new BlockType[s, s];
                var maskPos = new bool[s, s];
                var maskPower = new CellPowerState[s, s];
                var hasFace = new bool[s, s];
                for (int u = 0; u < s; u++)
                for (int v = 0; v < s; v++)
                {
                    int[] a = new int[3], b = new int[3];
                    a[axis] = slice; b[axis] = slice + 1;
                    a[uAxis] = u; b[uAxis] = u;
                    a[vAxis] = v; b[vAxis] = v;
                    var blockA = VoxelChunk.InBounds(a[0], a[1], a[2]) ? chunk.Get(a[0], a[1], a[2]) : BlockType.Air;
                    var blockB = VoxelChunk.InBounds(b[0], b[1], b[2]) ? chunk.Get(b[0], b[1], b[2]) : BlockType.Air;
                    if (BlockInfo.IsSolid(blockA) && !BlockInfo.IsSolid(blockB))
                    {
                        maskType[u, v] = blockA; maskPos[u, v] = true; hasFace[u, v] = true;
                        maskPower[u, v] = powerState == null ? CellPowerState.None : powerState(a[0], a[1], a[2]);
                    }
                    else if (!BlockInfo.IsSolid(blockA) && BlockInfo.IsSolid(blockB))
                    {
                        maskType[u, v] = blockB; maskPos[u, v] = false; hasFace[u, v] = true;
                        maskPower[u, v] = powerState == null ? CellPowerState.None : powerState(b[0], b[1], b[2]);
                    }
                }
                // Greedy rectangle merge.
                for (int v = 0; v < s; v++)
                for (int u = 0; u < s; u++)
                {
                    if (!hasFace[u, v]) continue;
                    var type = maskType[u, v];
                    var pos = maskPos[u, v];
                    var power = maskPower[u, v];
                    int w = 1;
                    while (u + w < s && hasFace[u + w, v] && maskType[u + w, v] == type && maskPos[u + w, v] == pos && maskPower[u + w, v] == power) w++;
                    int h = 1;
                    bool done = false;
                    while (v + h < s && !done)
                    {
                        for (int k = 0; k < w; k++)
                        {
                            if (!hasFace[u + k, v + h] || maskType[u + k, v + h] != type || maskPos[u + k, v + h] != pos || maskPower[u + k, v + h] != power) { done = true; break; }
                        }
                        if (!done) h++;
                    }
                    quads.Add(new MeshQuad { Axis = axis, Slice = slice, U0 = u, V0 = v, U1 = u + w, V1 = v + h, Type = type, Positive = pos, PowerState = power });
                    for (int dv = 0; dv < h; dv++)
                    for (int du = 0; du < w; du++)
                        hasFace[u + du, v + dv] = false;
                }
            }
        }
        return quads;
    }

    /// <summary>Naive face count (one quad per visible face, no merging), for comparison in tests.</summary>
    public static int NaiveFaceCount(VoxelChunk chunk)
    {
        int count = 0;
        int s = VoxelChunk.Size;
        int[][] dirs = { new[] { 1, 0, 0 }, new[] { -1, 0, 0 }, new[] { 0, 1, 0 }, new[] { 0, -1, 0 }, new[] { 0, 0, 1 }, new[] { 0, 0, -1 } };
        for (int x = 0; x < s; x++)
        for (int y = 0; y < s; y++)
        for (int z = 0; z < s; z++)
        {
            if (!BlockInfo.IsSolid(chunk.Get(x, y, z))) continue;
            foreach (var d in dirs)
            {
                int nx = x + d[0], ny = y + d[1], nz = z + d[2];
                var n = VoxelChunk.InBounds(nx, ny, nz) ? chunk.Get(nx, ny, nz) : BlockType.Air;
                if (!BlockInfo.IsSolid(n)) count++;
            }
        }
        return count;
    }
}
