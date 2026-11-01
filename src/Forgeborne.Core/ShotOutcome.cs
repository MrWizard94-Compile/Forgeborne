namespace Forgeborne.Core;

/// <summary>Resolved shot facts for presentation. Damage is the actual clamped health loss, not nominal weapon damage.</summary>
public readonly record struct ShotOutcome(
    bool Fired, AimTargetKind Target, int X, int Y, int Z, int DamageApplied, bool Destroyed);
