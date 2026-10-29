// Forgeborne Unity skeleton — NOT compiled in the Linux headless environment.
// This file documents the intended Unity integration. The authoritative,
// tested logic lives in src/Forgeborne.Core (pure C#, no UnityEngine).
//
// In Unity 6000 on Windows:
//  - Open ForgeborneUnity/ as a Unity project.
//  - Add the Entities / DOTS packages from Packages/manifest.json.
//  - Drive VoxelChunk / WorldZone from a MonoBehaviour / System:
//    WorldZone is the canonical state; Unity rendering, input, and UI
//    must read from it and send commands to it, never maintain a
//    second copy of chunk / inventory / health state.
//  - Voxel meshing: implement GPU greedy meshing in a later Slice 1
//    step, reading VoxelChunk.ToBytes() and building a mesh per chunk.
//    Do not add a second voxel representation.
//
// Slice 1 Step 1 status: core logic only. No Unity scene, no meshing,
// no input, no UI yet. Those are Slice 1 Steps 2-4, in that order.
namespace Forgeborne.Unity
{
    public static class UnityIntegrationNote
    {
        public const string Engine = "Unity 6000 + Entities / DOTS, URP, C#";
        public const string CanonicalState = "Forgeborne.Core.WorldZone";
    }
}
