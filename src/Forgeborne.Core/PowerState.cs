namespace Forgeborne.Core;

/// <summary>
/// Slice 13 Step 3: the per-cell power state a mesh tint shows.
/// This is a readout of the existing delivery rules, never a new
/// rule: Powered is a consumer delivered its rated voltage,
/// Mismatched is a consumer delivered a non-None voltage that is
/// not its rating (exactly the Step 1 warning predicates, so the
/// tint and the HUD warning agree cell for cell), Energised is
/// a Cable cell in a component carrying a non-None voltage
/// (Slice 17 Step 1: World.PowerState now reads the component
/// voltage directly, so this documented meaning is the
/// implemented meaning even for a single-cable component), and
/// None is everything else — unpowered, conflicted (None
/// delivery), a source, a transformer, a non-electrical block,
/// or air. Sources and transformers are deliberately None, not
/// Energised: a lone Dynamo is live but feeds nothing, and
/// tinting it would claim a delivery that reaches no cell, and
/// a transformer is a converter, not a consumer or a cable —
/// its conversion is already shown by what its neighbours
/// become, not by tinting the converter itself.
/// </summary>
public enum CellPowerState
{
    None = 0,
    Energised = 1,
    Powered = 2,
    Mismatched = 3,
}

/// <summary>
/// Slice 13 Step 3 tint colours, first-pass values for
/// determinism and testability, not final feel. None is white:
/// a Unity vertex colour of white leaves the material's own
/// colour unchanged, so an untinted block renders exactly as
/// before. The three tints are named against the marker
/// palette, not in isolation: Energised yellow (1, 0.85, 0.1)
/// is brighter and greener than the orange rally / pending
/// markers (1, 0.55, 0), and it sits on cable cells in the
/// ground, never as a floating marker; Powered green is never
/// cyan, so it cannot read as a squad marker; Mismatched red
/// is never orange, so it cannot read as a rally marker.
/// Colour is second to location here, as shape was second to
/// nothing for markers: a tinted block is the machine itself,
/// at its own cell, and the HUD warning counts (Step 1) say
/// the same outcome in numbers, so a player who cannot tell
/// red from green still has the machine's cell and the HUD
/// line to go on. If Windows testing proves a tint does not
/// read, that is playtest-gate items 26-27's evidence to
/// revisit, not a reason to have shipped cyan or orange.
/// </summary>
public static class PowerTint
{
    public static (float R, float G, float B, float A) ColorFor(CellPowerState state) => state switch
    {
        CellPowerState.Energised => (1f, 0.85f, 0.1f, 1f),
        CellPowerState.Powered => (0.25f, 0.9f, 0.25f, 1f),
        CellPowerState.Mismatched => (1f, 0.15f, 0.15f, 1f),
        _ => (1f, 1f, 1f, 1f),
    };
}
