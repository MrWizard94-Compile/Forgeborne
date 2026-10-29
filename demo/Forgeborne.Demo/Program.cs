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

// Slice 3 Step 3 scenario: Slice 3 composed in one run — the Step 1
// persistence (damaged core, skilled build, loot weapon) plus the
// Step 2 economy (paid squads, cap, picker refusal), through a
// save/load round-trip. This is the composition proof the scope
// asked for: persistence and economy working together, not only
// in their own steps' tests.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 3 Step 3 scenario — damaged core, skilled build, paid capped squads, save/load round-trip");
var lootS3 = LootGenerator.Generate(0, 1);
var skillBuildS3 = new SkillBuild();
skillBuildS3.Allocate("S1"); skillBuildS3.Allocate("S2"); skillBuildS3.Allocate("N1");
skillBuildS3.Allocate("S3"); skillBuildS3.Allocate("N2"); skillBuildS3.Allocate("K1");
var simS3 = new Simulation(42, lootS3, skillBuildS3);
simS3.World.Player.Restore(100, 100);
simS3.Fps.Teleport(24, 5, 24);
simS3.FireAtCore();
simS3.FireAtCore();
simS3.World.Player.Inventory.Add(BlockType.EmberOre, 40);
var s3Squad1 = simS3.DeployOrbitalSquadPaid(18, 5, 16);
var s3Squad2 = simS3.DeployOrbitalSquadPaid(19, 5, 16);
var s3Squad3 = simS3.DeployOrbitalSquadPaid(20, 5, 16);
Console.WriteLine($"Before save: core {simS3.World.CoreHealth} HP, weapon {simS3.Weapon.Name} {simS3.Weapon.Mag}/{simS3.Weapon.Reserve}, effective damage {simS3.EffectiveDamage}, shields {simS3.World.Player.Shields}/{simS3.World.Player.MaxShields}");
Console.WriteLine($"Paid squads deployed: {(s3Squad1 == null ? 0 : 1) + (s3Squad2 == null ? 0 : 1) + (s3Squad3 == null ? 0 : 1)} (ore left {simS3.World.Player.Inventory.Get(BlockType.EmberOre)}), squad HP {(simS3.World.Squads.Count == 0 ? 0 : simS3.World.Squads[0].Health)}");
// The 4th deploy goes through the picker, as in play: at the cap
// it is refused, the picker stays open, and the ore is untouched.
simS3.BeginOrbitalPick();
simS3.SetOrbitalRally(21, 5, 16);
var s3Fourth = simS3.ConfirmOrbitalDeploy();
Console.WriteLine($"4th picker deploy: {(s3Fourth == null ? "REFUSED at cap" : "DEPLOYED (unexpected)")}, picker still open: {simS3.OrbitalCommand.IsPicking}, ore left {simS3.World.Player.Inventory.Get(BlockType.EmberOre)}");
var saveS3 = simS3.SaveToJson();
var simS3Loaded = new Simulation(99);
Console.WriteLine($"Load: {simS3Loaded.LoadFromJson(saveS3)}");
var s3Snap = OrbitalView.Snapshot(simS3Loaded.World);
Console.WriteLine($"After load: core {simS3Loaded.World.CoreHealth} HP, weapon {simS3Loaded.Weapon.Name} {simS3Loaded.Weapon.Mag}/{simS3Loaded.Weapon.Reserve}, effective damage {simS3Loaded.EffectiveDamage}, shields {simS3Loaded.World.Player.Shields}/{simS3Loaded.World.Player.MaxShields}, squads loaded {s3Snap.Count} (HP {(s3Snap.Count == 0 ? 0 : s3Snap[0].Health)}, members {(s3Snap.Count == 0 ? 0 : s3Snap[0].AliveMembers)}), ore {simS3Loaded.World.Player.Inventory.Get(BlockType.EmberOre)}, can deploy more: {simS3Loaded.CanDeployOrbitalSquad()}, save JSON length: {saveS3.Length} chars");

// Slice 4 Step 3 scenario: Slice 4 composed in one run — the Step 1
// enemy pursuit and the Step 2 Advance doctrine together, not two
// separate proofs. Both squads deploy through the paid picker
// (10 EmberOre each, as in play), both weakened to 1 member so
// their 10-damage fire does not kill an enemy outright and hide
// the engagement, and the player moves far away so every enemy
// step is a squad pursuit, not player chasing. Enemy A (east)
// pursues the HoldGround squad; enemy B (west) pursues the
// squads from the other side; the Advance squad hunts enemy A,
// then turns to enemy B after A dies. After 4 ticks the game
// saves and loads, and 2 more ticks prove both doctrines keep
// their behaviour from the saved positions and health.
Console.WriteLine();
Console.WriteLine("Forgeborne Slice 4 Step 3 scenario — pursued HoldGround squad, hunting Advance squad, save/load round-trip");
var simS4 = new Simulation(42);
simS4.World.Player.Inventory.Add(BlockType.EmberOre, 20);
simS4.BeginOrbitalPick();
simS4.SetOrbitalRally(20, 5, 16);
var s4Hold = simS4.ConfirmOrbitalDeploy();
simS4.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance);
simS4.BeginOrbitalPick();
simS4.SetOrbitalRally(24, 5, 16);
var s4Adv = simS4.ConfirmOrbitalDeploy();
Console.WriteLine($"Paid squads deployed: HoldGround {(s4Hold == null ? "REJECTED" : "ok")}, Advance {(s4Adv == null ? "REJECTED" : "ok")}, ore left {simS4.World.Player.Inventory.Get(BlockType.EmberOre)}");
s4Hold!.TakeDamage(60); // 90 -> 30, 1 member
s4Adv!.TakeDamage(60); // 90 -> 30, 1 member
simS4.World.Player.X = 28; simS4.World.Player.Z = 28;
simS4.World.Enemies.Add(new Enemy(22, 5, 16));
simS4.World.Enemies.Add(new Enemy(10, 5, 16));
void PrintS4(string label, Simulation s)
{
    var snap = OrbitalView.Snapshot(s.World);
    var enemies = string.Join(", ", s.World.Enemies.Select(e => $"({e.X},5,{e.Z}) {e.Health} HP"));
    Console.WriteLine($"{label}: HoldGround ({(snap.Count > 0 ? snap[0].X : -1)},5,{(snap.Count > 0 ? snap[0].Z : -1)}) {snap[0].Health} HP, Advance ({(snap.Count > 1 ? snap[1].X : -1)},5,{(snap.Count > 1 ? snap[1].Z : -1)}) {snap[1].Health} HP, enemies [{enemies}]");
}
PrintS4("Slice 4 start", simS4);
for (int tick = 1; tick <= 4; tick++)
{
    simS4.World.Advance();
    PrintS4($"Slice 4 tick {tick}", simS4);
}
var saveS4 = simS4.SaveToJson();
var simS4Loaded = new Simulation(99);
Console.WriteLine($"Load: {simS4Loaded.LoadFromJson(saveS4)}");
PrintS4("Slice 4 after load", simS4Loaded);
for (int tick = 5; tick <= 6; tick++)
{
    simS4Loaded.World.Advance();
    PrintS4($"Slice 4 tick {tick} (loaded)", simS4Loaded);
}
var s4LoadedSnap = OrbitalView.Snapshot(simS4Loaded.World);
Console.WriteLine($"Slice 4 done: HoldGround doctrine {s4LoadedSnap[0].Doctrine} at rally: {s4LoadedSnap[0].AtRally}, Advance doctrine {s4LoadedSnap[1].Doctrine}, enemies left {simS4Loaded.World.Enemies.Count}, save JSON length: {saveS4.Length} chars");
