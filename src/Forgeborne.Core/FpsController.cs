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

    internal void RestoreRuntime(float x, float y, float z, float velocity, bool grounded)
    {
        X = x; Y = y; Z = z;
        VelocityY = velocity;
        IsGrounded = grounded;
    }

    private static (int X0, int X1, int Y0, int Y1, int Z0, int Z1) BodyCells(float px, float py, float pz) =>
        ((int)Math.Floor(px - HalfWidth), (int)Math.Floor(px + HalfWidth),
         (int)Math.Floor(py), (int)Math.Floor(py + Height - 0.001f),
         (int)Math.Floor(pz - HalfWidth), (int)Math.Floor(pz + HalfWidth));

    /// <summary>True for a voxel touched by the same body footprint used by movement collision.</summary>
    public bool OverlapsCell(int x, int y, int z)
    {
        var cells = BodyCells(X, Y, Z);
        return x >= cells.X0 && x <= cells.X1 && y >= cells.Y0 && y <= cells.Y1 && z >= cells.Z0 && z <= cells.Z1;
    }

    private bool Collides(VoxelChunk chunk, float px, float py, float pz)
    {
        var cells = BodyCells(px, py, pz);
        for (int x = cells.X0; x <= cells.X1; x++)
        for (int y = cells.Y0; y <= cells.Y1; y++)
        for (int z = cells.Z0; z <= cells.Z1; z++)
        {
            if (!VoxelChunk.InBounds(x, y, z)) continue;
            if (BlockInfo.IsSolid(chunk.Get(x, y, z))) return true;
        }
        return false;
    }

    /// <summary>
    /// World-aligned move: forward is the +Z component, strafe the
    /// +X component. This is the Slice 1 entry point, kept so direct
    /// FpsController tests and callers that have no Look keep exactly
    /// the old behaviour. It delegates to UpdateWorld, so there is
    /// still one movement implementation, not two.
    /// </summary>
    public void Update(float dt, float forward, float strafe, bool jump, VoxelChunk chunk) =>
        UpdateWorld(dt, strafe, forward, jump, chunk);

    /// <summary>
    /// World-space move (Slice 8 Step 1): dx is the +X component and
    /// dz the +Z component of the intended walk, already rotated by
    /// the caller (Simulation.Tick rotates by Look yaw; this class
    /// holds no Look and applies no rotation of its own). The two
    /// components are applied at full speed each, exactly as the
    /// Slice 1 forward/strafe pair was, so a diagonal keeps its
    /// Slice 1 speed quirk — see DECISIONS.md 0.36.0.
    /// </summary>
    public void UpdateWorld(float dt, float dx, float dz, bool jump, VoxelChunk chunk)
    {
        if(chunk.SurfaceProfile!=SurfaceProfile.LegacyBlocks)
        {UpdateContinuous(dt,dx,dz,jump,chunk.Geometry);return;}
        // Horizontal X, then Z, separately, so sliding along a wall
        // still works: a blocked X component does not cancel Z.
        float nx = X + dx * EffectiveSpeed * dt;
        if (!Collides(chunk, nx, Y, Z)) X = nx;
        float nz = Z + dz * EffectiveSpeed * dt;
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

    private void UpdateContinuous(float dt,float dx,float dz,bool jump,TerrainGeometry geometry)
    {
        var position=new System.Numerics.Vector3(X,Y,Z);
        const float stepHeight=0.35f;
        const float walkableNormalY=0.642788f;
        void Horizontal(System.Numerics.Vector3 movement)
        {
            if(movement.LengthSquared()<1e-16f)return;
            var sweep=geometry.SweepBody(position,movement);
            if(sweep.Hit && IsGrounded)
            {
                var up=new System.Numerics.Vector3(0,stepHeight,0);
                var lift=geometry.SweepBody(position,up);
                var across=geometry.SweepBody(position+up,movement);
                var down=new System.Numerics.Vector3(0,-stepHeight-0.02f,0);
                var landing=geometry.SweepBody(position+up+movement,down);
                if(!lift.Hit && !across.Hit && landing.Hit && landing.Normal.Y>=walkableNormalY)
                {position+=up+movement+down*landing.Fraction;return;}
            }
            position+=movement*sweep.Fraction;
        }
        Horizontal(new System.Numerics.Vector3(dx*EffectiveSpeed*dt,0,0));
        Horizontal(new System.Numerics.Vector3(0,0,dz*EffectiveSpeed*dt));
        if(jump && IsGrounded){VelocityY=JumpVelocity;IsGrounded=false;}
        VelocityY+=Gravity*dt;
        var vertical=new System.Numerics.Vector3(0,VelocityY*dt,0);
        var contact=geometry.SweepBody(position,vertical);
        position+=vertical*contact.Fraction;
        IsGrounded=contact.Hit && VelocityY<=0 && contact.Normal.Y>=walkableNormalY;
        if(contact.Hit)VelocityY=0;
        if(!IsGrounded && VelocityY<=0)
        {
            var probe=geometry.SweepBody(position,new System.Numerics.Vector3(0,-0.002f,0));
            IsGrounded=probe.Hit && probe.Normal.Y>=walkableNormalY;
        }
        X=position.X;Y=position.Y;Z=position.Z;
    }
}
