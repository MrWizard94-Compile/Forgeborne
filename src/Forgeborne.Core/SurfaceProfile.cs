namespace Forgeborne.Core;

/// <summary>Serialized geometric contract. A missing profile in old saves is LegacyBlocks.</summary>
public enum SurfaceProfile { LegacyBlocks = 0, ContinuousTerrainV1 = 1, AuthoredFoundryV1 = 2 }
