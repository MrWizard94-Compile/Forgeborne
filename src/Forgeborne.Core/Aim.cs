namespace Forgeborne.Core;

/// <summary>What an aim ray first reaches: a solid block, a live
/// enemy, the objective core, or nothing within reach.</summary>
public enum AimTargetKind { None, Solid, Enemy, Core }

/// <summary>
/// One aim-ray result. <see cref="Cell"/> is the hit cell (solid,
/// enemy, or core cell); <see cref="PrevCell"/> is the last air
/// cell the ray was in before entering it, which is the
/// face-adjacent build cell for a solid hit. <see cref="LeftBounds"/>
/// is true only when the ray left the chunk before reaching
/// anything, so callers can tell "nothing aimed" (reach ran out
/// in open air) from "aimed out of the world".
/// </summary>
public readonly record struct AimHit(
    AimTargetKind Kind,
    int X, int Y, int Z,
    int PrevX, int PrevY, int PrevZ,
    Enemy? Enemy,
    bool LeftBounds);

/// <summary>
/// Slice 8 Step 2 — the one aim ray, shared by gather, build, and
/// fire, so the three actions cannot drift into three different
/// aim rules. It walks from the eye point along the core Look
/// forward vector in 0.05 m steps (small enough that a cell
/// change is always face-by-face, never diagonal, so the previous
/// cell is always face-adjacent to the hit solid), up to the
/// caller's reach. Actors (enemies, the core) are air cells, so
/// when <paramref name="includeActors"/> is false the ray passes
/// through them to the first solid — a gather or build is not
/// blocked by standing in front of an enemy — and when true the
/// first actor cell wins over any solid behind it, because the
/// walk reaches it first. A solid always stops the ray: gather
/// cannot strip a block through a wall, and fire cannot hit an
/// enemy behind one. There is no fallback anywhere in this
/// class: no solid, no actor, and no bounds within reach is
/// Kind None, and callers must fail from that, never quietly
/// act on the old fixed +Z cell.
/// </summary>
public static class AimRay
{
    public const float StepMetres = 0.05f;

    public static AimHit Trace(
        VoxelChunk chunk,
        IReadOnlyList<Enemy> enemies,
        int coreX, int coreY, int coreZ,
        float originX, float originY, float originZ,
        (float X, float Y, float Z) forward,
        float reach,
        bool includeActors)
    {
        int prevX = (int)Math.Floor(originX);
        int prevY = (int)Math.Floor(originY);
        int prevZ = (int)Math.Floor(originZ);
        if (reach <= 0) return new AimHit(AimTargetKind.None, prevX, prevY, prevZ, prevX, prevY, prevZ, null, false);
        for (float t = StepMetres; t <= reach; t += StepMetres)
        {
            float px = originX + forward.X * t;
            float py = originY + forward.Y * t;
            float pz = originZ + forward.Z * t;
            int x = (int)Math.Floor(px);
            int y = (int)Math.Floor(py);
            int z = (int)Math.Floor(pz);
            if (x == prevX && y == prevY && z == prevZ) continue;
            if (!VoxelChunk.InBounds(x, y, z))
                return new AimHit(AimTargetKind.None, x, y, z, prevX, prevY, prevZ, null, true);
            if (includeActors)
            {
                foreach (var e in enemies)
                {
                    if (!e.IsDead && e.X == x && e.Y == y && e.Z == z)
                        return new AimHit(AimTargetKind.Enemy, x, y, z, prevX, prevY, prevZ, e, false);
                }
                if (x == coreX && y == coreY && z == coreZ)
                    return new AimHit(AimTargetKind.Core, x, y, z, prevX, prevY, prevZ, null, false);
            }
            if (BlockInfo.IsSolid(chunk.Get(x, y, z)))
                return new AimHit(AimTargetKind.Solid, x, y, z, prevX, prevY, prevZ, null, false);
            prevX = x; prevY = y; prevZ = z;
        }
        return new AimHit(AimTargetKind.None, prevX, prevY, prevZ, prevX, prevY, prevZ, null, false);
    }
}
