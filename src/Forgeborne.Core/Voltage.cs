namespace Forgeborne.Core;

/// <summary>
/// Slice 9 Step 1 voltage ladder: exactly two levels, because two
/// suffice and more would be unused names. LV is the Dynamo's
/// output and the Kinetic Mill's rating; HV exists only because
/// a transformer can produce it, so that a cable (Step 2) has a
/// second voltage to carry and a mismatch rule (Step 3) has a
/// voltage to punish. Since Slice 10 Step 2 HV also has a native
/// generator, the Alternator (see AlternatorOutput): HV is no
/// longer only a transformer's flip of LV. None means no voltage
/// is delivered, not a third level: a cell with no powered
/// neighbour carries nothing.
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

    /// <summary>Slice 10 Step 2: the Alternator's output level.
    /// HV, not LV — that is the whole of the source's
    /// distinctness at the voltage level: a native HV
    /// generator, where the Dynamo is a native LV generator
    /// and a transformer only flips a voltage it is fed.
    /// An LV-rated consumer fed directly by an Alternator
    /// is therefore mismatched (a mill is destroyed, a
    /// workstation is disabled), and safe use runs through
    /// one transformer step-down, which is the step's
    /// proof burden, not an accident of placement.</summary>
    public const Voltage AlternatorOutput = Voltage.HV;

    /// <summary>Slice 14 Step 1: the Conduit's output level, HV —
    /// deliberately the Alternator's level, not a new level,
    /// because the ladder stays LV / HV / None in this slice.
    /// The Conduit's distinctness is therefore NOT its voltage:
    /// it is its delivery shape. A Conduit feeds only through a
    /// Cable network (see CableNetworkVoltage); face-adjacency
    /// to a consumer or a transformer delivers nothing from it,
    /// so a mill standing beside a Conduit survives a tick an
    /// Alternator-fed mill does not, while a mill at the end of
    /// a Conduit-fed cable is destroyed exactly like an
    /// Alternator-fed one. Its cost (5 Stone + 5 EmberOre) is
    /// a new combination, but cost alone is not claimed as the
    /// distinctness — the cable-only delivery is, and it is
    /// proved by tests no existing source can pass.</summary>
    public const Voltage ConduitOutput = Voltage.HV;

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

    /// <summary>Slice 12 Step 2: the Shock Pylon's rated voltage.
    /// HV, by decision, not by inheritance: this is the step's
    /// whole point — the first consumer that actually wants HV,
    /// so the Alternator gains a safe direct use and HV stops
    /// being a voltage that only ever punishes LV consumers.
    /// A separate constant, like WorkstationRated, so a future
    /// rating change for one consumer cannot silently change
    /// another. LV delivered to a pylon is therefore a mismatch
    /// (disabled, not destroyed — see IsPylonMismatched), and
    /// None is idle, exactly the workstation's mismatch shape
    /// with the levels swapped.</summary>
    public const Voltage PylonRated = Voltage.HV;

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
