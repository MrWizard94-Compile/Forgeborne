namespace Forgeborne.Core;

/// <summary>
/// Slice 9 Step 1 voltage ladder: exactly two levels, because two
/// suffice and more would be unused names. LV is the Dynamo's
/// output and the Kinetic Mill's rating; HV exists only because
/// a transformer can produce it, so that a cable (Step 2) has a
/// second voltage to carry and a mismatch rule (Step 3) has a
/// voltage to punish. None means no voltage is delivered, not a
/// third level: a cell with no powered neighbour carries nothing.
/// </summary>
public enum Voltage
{
    None = 0,
    LV = 1,
    HV = 2,
}

public static class VoltageRules
{
    /// <summary>The existing Dynamo's output level, stated so an
    /// old world does not go dark on load: a Dynamo in an old save
    /// carries no voltage field, and it loads at LV, exactly the
    /// level that powered its neighbouring mill before this step.</summary>
    public const Voltage DynamoOutput = Voltage.LV;

    /// <summary>The Kinetic Mill's rated voltage: the only voltage
    /// that runs it at the every-tick powered cadence. Any other
    /// delivered voltage is unmatched — since Slice 9 Step 3,
    /// overvoltage (HV) destroys the mill on its delivery tick
    /// instead of falling back to kinetic cadence, and no
    /// delivered voltage (None) still falls back to kinetic
    /// cadence, because no delivery is not a mismatch.</summary>
    public const Voltage MillRated = Voltage.LV;

    /// <summary>Slice 10 Step 1: the Workstation's rated voltage.
    /// Same value as MillRated (LV) by decision, not by inheritance:
    /// a separate constant so a future rating change for one
    /// consumer cannot silently change the other, and so
    /// IsWorkstationPowered / IsWorkstationMismatched state their
    /// own rating instead of borrowing the mill's.</summary>
    public const Voltage WorkstationRated = Voltage.LV;

    /// <summary>Transformer conversion on a two-level ladder: each
    /// level converts to the other, so there is no "nowhere higher
    /// / nowhere lower" edge left undefined — LV in gives HV out
    /// (step-up) and HV in gives LV out (step-down). None in gives
    /// None out: an unpowered transformer converts nothing.</summary>
    public static Voltage Flipped(Voltage voltage) => voltage switch
    {
        Voltage.LV => Voltage.HV,
        Voltage.HV => Voltage.LV,
        _ => Voltage.None,
    };
}
