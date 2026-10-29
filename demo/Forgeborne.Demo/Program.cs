using System.Diagnostics;
using Forgeborne.Core;

// Headless demo: runs the full Slice 1 loop without Unity.
// Usage: dotnet run --project demo/Forgeborne.Demo
var sim = new Simulation(42);
sim.World.SpawnWave(3);
Console.WriteLine("Forgeborne Slice 1 headless demo — gather, build, fight, objective");
for (int tick = 0; tick < 60; tick++)
{
    sim.Tick(0.1f, tick < 20 ? 1f : 0f, 0f, false);
    if (tick == 5) Console.WriteLine($"Tick {tick}: gather -> {sim.GatherAhead()} (gathered {sim.BlocksGathered})");
    if (tick == 10) { sim.World.Player.Inventory.Add(BlockType.Stone, 10); Console.WriteLine($"Tick {tick}: build -> {sim.BuildWallAhead()} (built {sim.BlocksBuilt})"); }
    if (tick >= 20 && tick % 5 == 0) sim.FireAtNearestEnemy();
    if (tick % 20 == 0)
    {
        var hud = sim.Hud();
        Console.WriteLine($"Tick {tick}: HP {hud.Health} Shields {hud.Shields} Ammo {hud.AmmoMag}/{hud.AmmoReserve} Enemies {hud.EnemyCount} Objective {hud.ObjectivePercentCompleted}% Dist {hud.ObjectiveDistanceMetres}m");
    }
}
var sw = Stopwatch.StartNew();
var mesh = MeshBuilder.BuildFromChunk(sim.World.Chunk);
sw.Stop();
Console.WriteLine($"Mesher: {mesh.QuadCount} quads, {mesh.Vertices.Count} vertices in {sw.ElapsedMilliseconds} ms");
Console.WriteLine($"Done: gathered {sim.BlocksGathered}, built {sim.BlocksBuilt}, killed {sim.EnemiesKilled}, player dead: {sim.IsPlayerDead}, objective complete: {sim.IsObjectiveComplete}");
Console.WriteLine($"Save JSON length: {SaveSystem.SaveZone(sim.World).Length} chars");

// Slice 2 scenario: the same Simulation composed with every Slice 2
// system at once — generated loot equipped, full Forge Survivor
// skill cluster applied, a Kinetic Mill built, an Orbital squad
// deployed — so the demo proves composition, not just that each
// system works in isolation. Numbers below are the evidence.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 2 scenario — loot, skills, Kinetic mill, Orbital squad, save with squads");
var loot = LootGenerator.Generate(0, 1);
var skillBuild = new SkillBuild();
skillBuild.Allocate("S1"); skillBuild.Allocate("S2"); skillBuild.Allocate("N1");
skillBuild.Allocate("S3"); skillBuild.Allocate("N2"); skillBuild.Allocate("K1");
var sim2 = new Simulation(42, loot, skillBuild);
Console.WriteLine($"Loot: {loot.Name} ({loot.Rarity}, {loot.Damage} dmg, {loot.MagSize} mag)");
Console.WriteLine($"Skills: full Forge Survivor cluster — effective damage {sim2.EffectiveDamage}, core damage {sim2.EffectiveCoreDamage}, shields cap {sim2.World.Player.MaxShields}, speed {sim2.Fps.EffectiveSpeed} m/s");
sim2.World.SpawnWave(3);
// Slice 3 Step 2: the demo squad deploys through the paid path
// (10 EmberOre), like a picker Confirm — the free direct path is
// headless-only and no longer represents a deploy in play.
sim2.World.Player.Inventory.Add(BlockType.EmberOre, 10);
var squad = sim2.DeployOrbitalSquadPaid(sim2.World.Player.X, sim2.World.Player.Y, sim2.World.Player.Z);
Console.WriteLine($"Orbital squad deployed: {(squad == null ? "REJECTED" : "Vanguard HoldGround at player position")}");
sim2.World.Player.Inventory.Add(BlockType.Stone, 10);
sim2.World.Player.Inventory.Add(BlockType.Wood, 5);
Console.WriteLine($"Kinetic mill build -> {sim2.BuildKineticMillAhead()} (built {sim2.BlocksBuilt})");
for (int tick = 0; tick < 20; tick++)
{
    sim2.Tick(0.1f, 0f, 0f, false);
    if (tick % 5 == 0) sim2.FireAtNearestEnemy();
    if (tick % 5 == 4)
    {
        var snap = OrbitalView.Snapshot(sim2.World);
        Console.WriteLine($"Slice 2 tick {tick}: enemies {sim2.World.Enemies.Count}, squad members {(snap.Count == 0 ? 0 : snap[0].AliveMembers)}, squad HP {(snap.Count == 0 ? 0 : snap[0].Health)}, kinetic gathered {sim2.World.KineticBlocksGathered}");
    }
}
var save2 = SaveSystem.SaveZone(sim2.World);
var loaded2 = SaveSystem.LoadZone(save2);
Console.WriteLine($"Slice 2 done: gathered {sim2.BlocksGathered}, built {sim2.BlocksBuilt}, killed {sim2.EnemiesKilled}, kinetic gathered {sim2.World.KineticBlocksGathered}, player dead: {sim2.IsPlayerDead}, squads saved+loaded: {loaded2.Squads.Count}, save JSON length: {save2.Length} chars");

// Slice 3 Step 1 scenario: save-format completion. A skilled,
// loot-equipped Simulation damages its core, then saves with the
// full format and loads into a fresh Simulation — proving the
// core health, the skill build, the shields above the base cap,
// and the equipped weapon (with its in-progress mag) all survive.
// The FPS controller is teleported near the core as the headless
// stand-in for walking there; FireAtCore then does real damage.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 3 Step 1 scenario — core health, skill build, and weapon survive a save/load round-trip");
var loot3 = LootGenerator.Generate(0, 1);
var skillBuild3 = new SkillBuild();
skillBuild3.Allocate("S1"); skillBuild3.Allocate("S2"); skillBuild3.Allocate("N1");
skillBuild3.Allocate("S3"); skillBuild3.Allocate("N2"); skillBuild3.Allocate("K1");
var sim3 = new Simulation(42, loot3, skillBuild3);
sim3.World.Player.Restore(100, 100);
sim3.Fps.Teleport(24, 5, 24);
sim3.FireAtCore();
sim3.FireAtCore();
Console.WriteLine($"Before save: core {sim3.World.CoreHealth} HP, weapon {sim3.Weapon.Name} {sim3.Weapon.Mag}/{sim3.Weapon.Reserve}, effective damage {sim3.EffectiveDamage}, shields {sim3.World.Player.Shields}/{sim3.World.Player.MaxShields}");
var save3 = sim3.SaveToJson();
var sim4 = new Simulation(99);
Console.WriteLine($"Load: {sim4.LoadFromJson(save3)}");
Console.WriteLine($"After load: core {sim4.World.CoreHealth} HP, weapon {sim4.Weapon.Name} {sim4.Weapon.Mag}/{sim4.Weapon.Reserve}, effective damage {sim4.EffectiveDamage}, shields {sim4.World.Player.Shields}/{sim4.World.Player.MaxShields}, save JSON length: {save3.Length} chars");
