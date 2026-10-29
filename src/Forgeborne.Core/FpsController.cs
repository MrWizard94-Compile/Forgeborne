namespace Forgeborne.Core;

/// <summary>
/// FPS movement in voxel space, pure logic, no Unity input.
/// Position is feet position. AABB: half-width 0.3, height 1.8.
/// Axes are resolved separately so sliding along a wall works.
/// Gravity -20, jump velocity 8, move speed 6 (metres / second, Slice 1 values).
/// </summary>
public sealed class FpsController
{
    public float X { get; private set; }
    public float Y { get; private set; }
    public float Z { get; private set; }
    public float VelocityY { get; private set; }
    public bool IsGrounded { get; private set; }

    public const float Speed = 6f;
    /// <summary>Move-speed multiplier. Base 1; skill MoveSpeedPct raises it via Simulation.ApplySkills.</summary>
    public float SpeedMultiplier { get; set; } = 1f;
    public float EffectiveSpeed => Speed * SpeedMultiplier;
    public const float JumpVelocity = 8f;
    public const float Gravity = -20f;
    public const float HalfWidth = 0.3f;
    public const float Height = 1.8f;

    public FpsController(float x, float y, float z) { X = x; Y = y; Z = z; }

    /// <summary>
    /// Move the feet position instantly, e.g. to a loaded save's player
    /// position. Vertical velocity is cleared and grounded is reset so
    /// the next Update re-derives them from the new cell; the caller
    /// (Simulation.RestoreWorld) is responsible for the world swap.
    /// </summary>
    public void Teleport(float x, float y, float z)
    {
        X = x; Y = y; Z = z;
        VelocityY = 0;
        IsGrounded = false;
    }

    private bool Collides(VoxelChunk chunk, float px, float py, float pz)
    {
        int x0 = (int)Math.Floor(px - HalfWidth), x1 = (int)Math.Floor(px + HalfWidth);
        int y0 = (int)Math.Floor(py), y1 = (int)Math.Floor(py + Height - 0.001f);
        int z0 = (int)Math.Floor(pz - HalfWidth), z1 = (int)Math.Floor(pz + HalfWidth);
        for (int x = x0; x <= x1; x++)
        for (int y = y0; y <= y1; y++)
        for (int z = z0; z <= z1; z++)
        {
            if (!VoxelChunk.InBounds(x, y, z)) continue;
            if (BlockInfo.IsSolid(chunk.Get(x, y, z))) return true;
        }
        return false;
    }

    public void Update(float dt, float forward, float strafe, bool jump, VoxelChunk chunk)
    {
        // Horizontal X, then Z, separately.
        float nx = X + strafe * EffectiveSpeed * dt;
        if (!Collides(chunk, nx, Y, Z)) X = nx;
        float nz = Z + forward * EffectiveSpeed * dt;
        if (!Collides(chunk, X, Y, nz)) Z = nz;

        // Vertical.
        if (jump && IsGrounded) { VelocityY = JumpVelocity; IsGrounded = false; }
        VelocityY += Gravity * dt;
        float prevY = Y;
        float ny = Y + VelocityY * dt;
        if (Collides(chunk, X, ny, Z))
        {
            if (VelocityY < 0)
            {
                // Land on top of the highest solid block at or below the
                // previous feet position. This prevents tunnelling when a
                // large dt step jumps past the top surface (e.g. 4.4 -> 2.8,
                // where floor(ny) would wrongly snap to 3 instead of 4).
                int topSolid = -1;
                for (int y = (int)Math.Floor(prevY); y >= 0; y--)
                {
                    if (VoxelChunk.InBounds((int)Math.Floor(X), y, (int)Math.Floor(Z)) &&
                        BlockInfo.IsSolid(chunk.Get((int)Math.Floor(X), y, (int)Math.Floor(Z))))
                    { topSolid = y; break; }
                }
                Y = topSolid >= 0 ? topSolid + 1 : prevY;
                IsGrounded = true;
            }
            else
            {
                // Hit ceiling: stop upward motion, stay at current Y.
                Y = Y;
            }
            VelocityY = 0;
        }
        else
        {
            Y = ny;
            IsGrounded = false;
        }
        // Ground check: if there is solid directly below the feet, we are grounded (for jump eligibility).
        if (!IsGrounded)
        {
            int below = (int)Math.Floor(Y - 0.01f);
            if (VoxelChunk.InBounds((int)Math.Floor(X), below, (int)Math.Floor(Z)) &&
                BlockInfo.IsSolid(chunk.Get((int)Math.Floor(X), below, (int)Math.Floor(Z))) &&
                Math.Abs(Y - (below + 1)) < 0.01f)
                IsGrounded = true;
        }
    }
}
