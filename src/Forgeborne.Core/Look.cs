namespace Forgeborne.Core;

/// <summary>
/// FPS view direction, in pure C# core (Slice 6 Step 1). This is the
/// answer to the scope's "where does pitch live" question: pitch
/// lives here, in core, together with yaw — not as an adapter-only
/// camera angle — because the direction vector is what the picker
/// raycast (and any future aim rule) actually consumes. A pitch
/// that existed only as a camera angle would be a rendering fact,
/// not an aim fact, and no headless test could touch it.
///
/// Yaw and pitch are in degrees. Yaw 0 faces +Z (the same "front"
/// as GatherAhead / BuildDynamoAhead), yaw increases toward +X, and
/// pitch is positive looking up. The adapter feeds already-scaled
/// mouse deltas (mouse delta times its existing sensitivity) into
/// Add, so there is no second sensitivity value in core: the
/// adapter's MouseSensitivity stays the one sensitivity, exactly
/// as the scope's hypothesis asked.
///
/// Yaw is NOT moved into FpsController, and movement is NOT
/// rotated by it here: FpsController stays world-aligned in this
/// step, as the scope requires. Moving yaw into Look (rather than
/// adding a second yaw beside the adapter's) is what avoids
/// duplication: the adapter no longer stores its own _yaw, it
/// reads Simulation.Look.Yaw / Forward for the camera. Look is
/// session input state, like the picker selection: it is not
/// saved, and an in-place RestoreWorld does not reset it — a
/// fresh Simulation starts at yaw 0, pitch 0, which is what a
/// new Unity scene's adapter sees. A load into an existing
/// Simulation therefore keeps the player's current view instead
/// of snapping the camera to +Z, for the same identity reason
/// RestoreWorld keeps the same Fps object.
/// </summary>
public sealed class Look
{
    /// <summary>Pitch clamp, in degrees: looking straight up or down
    /// would make the horizontal forward component zero and the
    /// camera flip at the pole, so pitch stops 1 degree short of
    /// vertical. A first-pass value for testability, not a feel
    /// claim — feel is a playtest question.</summary>
    public const float MaxPitch = 89f;
    public const float MinPitch = -89f;

    /// <summary>Horizontal view angle, degrees. Unbounded: it wraps
    /// naturally in the forward-vector trigonometry, so no
    /// normalisation rule is needed and large accumulated mouse
    /// movement cannot overflow a wrap check.</summary>
    public float Yaw { get; private set; }

    /// <summary>Vertical view angle, degrees, clamped to
    /// [-89, 89]. Positive is looking up.</summary>
    public float Pitch { get; private set; }

    /// <summary>
    /// Apply already-scaled mouse deltas. Positive yawDelta turns
    /// toward +X; positive pitchDelta looks up. Pitch clamps;
    /// yaw does not.
    /// </summary>
    public void Add(float yawDeltaDegrees, float pitchDeltaDegrees)
    {
        Yaw += yawDeltaDegrees;
        Pitch = Math.Clamp(Pitch + pitchDeltaDegrees, MinPitch, MaxPitch);
    }

    /// <summary>Unit forward vector derived from yaw and pitch.</summary>
    public (float X, float Y, float Z) Forward()
    {
        float yawRad = Yaw * (float)Math.PI / 180f;
        float pitchRad = Pitch * (float)Math.PI / 180f;
        float horizontal = (float)Math.Cos(pitchRad);
        return (
            horizontal * (float)Math.Sin(yawRad),
            (float)Math.Sin(pitchRad),
            horizontal * (float)Math.Cos(yawRad));
    }

    public float ForwardX => Forward().X;
    public float ForwardY => Forward().Y;
    public float ForwardZ => Forward().Z;
}
