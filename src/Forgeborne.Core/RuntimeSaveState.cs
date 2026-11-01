namespace Forgeborne.Core;

/// <summary>Versioned interactive-runtime snapshot. Nullable members make a partial record fail closed.</summary>
public sealed class RuntimeSaveState
{
    public int? Version { get; set; }
    public float? X { get; set; }
    public float? Y { get; set; }
    public float? Z { get; set; }
    public float? VelocityY { get; set; }
    public bool? Grounded { get; set; }
    public float? Yaw { get; set; }
    public float? Pitch { get; set; }
    public double? FrameRemainder { get; set; }
    public int? MovementPhase { get; set; }
    public bool? QueuedJump { get; set; }
    public double? GatherBonusCarry { get; set; }
    public int? BlocksGathered { get; set; }
    public int? BlocksBuilt { get; set; }
    public int? EnemiesKilled { get; set; }

    internal bool Matches(WorldZone world)
    {
        if (Version != 1 || X is not float x || Y is not float y || Z is not float z
            || VelocityY is not float velocity || Grounded is null || Yaw is not float yaw
            || Pitch is not float pitch || FrameRemainder is not double remainder
            || MovementPhase is not int phase || QueuedJump is null || GatherBonusCarry is not double gather
            || BlocksGathered is not int gathered || BlocksBuilt is not int built || EnemiesKilled is not int killed)
            return false;
        bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        return Finite(x) && Finite(y) && Finite(z) && Finite(velocity) && Finite(yaw) && Finite(pitch)
            && Math.Abs(x) <= 1_000_000 && Math.Abs(y) <= 1_000_000 && Math.Abs(z) <= 1_000_000
            && Math.Abs(velocity) <= 10_000 && pitch >= Look.MinPitch && pitch <= Look.MaxPitch
            && !double.IsNaN(remainder) && !double.IsInfinity(remainder) && remainder >= 0 && remainder < Simulation.FrameStepSeconds
            && phase >= 0 && phase < Simulation.MovementStepsPerWorldTick
            && !double.IsNaN(gather) && !double.IsInfinity(gather) && gather >= 0
            && gathered >= 0 && built >= 0 && killed >= 0
            && Math.Floor(x) == world.Player.X && Math.Floor(y) == world.Player.Y && Math.Floor(z) == world.Player.Z;
    }
}
