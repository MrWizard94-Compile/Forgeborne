using System.Runtime.CompilerServices;

// Slice 18 Step 2: the test assembly is the ONLY friend of the core.
// It needs the internal test seam (WorldZone.SpawnSquad) to isolate
// movement, fire, and targeting tests from the squad economy. No
// other assembly — not the demo, not a Unity adapter assembly — is
// granted this access, so a free spawn from any of them fails to
// compile (observed CS1061 for both the internal seam and the
// removed public free entry points, in the negative-compile proof).
[assembly: InternalsVisibleTo("Forgeborne.Core.Tests")]
