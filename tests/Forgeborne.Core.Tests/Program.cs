// Slice 18 Step 2: this file is the test assembly, the core's only
// friend assembly (see src/Forgeborne.Core/AssemblyInfo.cs), so it
// alone may call the internal test seam WorldZone.SpawnSquad. The
// former public free entry points (WorldZone.DeploySquad,
// Simulation.DeployOrbitalSquad) were removed in that step: any
// other assembly calling them fails to compile, and any non-friend
// caller of the seam fails to compile (observed CS1061 in the negative-compile proof). Play deploys go through the
// paid path, never through this seam.
using Forgeborne.Core;

// Minimal test runner, no external packages: each Check must pass or the
// process exits non-zero with the failing test name. Run: dotnet run
int passed = 0, failed = 0;
void Check(bool condition, string name)
{
    if (condition) { passed++; Console.WriteLine($"PASS {name}"); }
    else { failed++; Console.WriteLine($"FAIL {name}"); }
}
void Eq<T>(T expected, T actual, string name) => Check(EqualityComparer<T>.Default.Equals(expected, actual), $"{name} (expected {expected}, got {actual})");

// Chunk
var chunk = new VoxelChunk();
Check(chunk.TrySet(1, 1, 1, BlockType.Stone), "chunk set in bounds");
Eq(BlockType.Stone, chunk.Get(1, 1, 1), "chunk get");
Check(!chunk.TrySet(-1, 0, 0, BlockType.Stone), "chunk set out of bounds fails");
Eq(BlockType.Air, chunk.Get(99, 99, 99), "chunk get out of bounds is air");
Eq(VoxelChunk.Size * VoxelChunk.Size * VoxelChunk.Size, VoxelChunk.Volume, "chunk volume 32^3");

// Deterministic generation
var a = VoxelChunk.Generate(42);
var b = VoxelChunk.Generate(42);
var c = VoxelChunk.Generate(43);
Check(a.ToBytes().SequenceEqual(b.ToBytes()), "generation same seed equal");
Check(!a.ToBytes().SequenceEqual(c.ToBytes()), "generation different seed differs");
Eq(BlockType.Bedrock, a.Get(5, 0, 5), "generation bedrock layer");
Check(a.CountSolid() > 3000, "generation has solid ground");

// World: gather
var world = new WorldZone(42);
// Find a stone block in the generated ground.
int sx = -1, sy = -1, sz = -1;
for (int x = 0; x < 32 && sx < 0; x++)
for (int y = 1; y <= 3 && sx < 0; y++)
for (int z = 0; z < 32 && sx < 0; z++)
    if (world.Chunk.Get(x, y, z) == BlockType.Stone) { sx = x; sy = y; sz = z; }
Check(sx >= 0, "found stone to gather");
Eq(GatherResult.Success, world.Gather(sx, sy, sz), "gather stone succeeds");
Eq(BlockType.Air, world.Chunk.Get(sx, sy, sz), "gathered block becomes air");
Eq(1, world.Player.Inventory.Get(BlockType.Stone), "gather gives 1 stone");
Eq(GatherResult.Air, world.Gather(sx, sy, sz), "gather air fails");
Eq(GatherResult.OutOfBounds, world.Gather(-1, 0, 0), "gather out of bounds fails");
Eq(GatherResult.NotGatherable, world.Gather(5, 0, 5), "gather bedrock fails");

// Build: wall needs 2 stone, adjacent support, air target
world.Player.Inventory.Add(BlockType.Stone, 10);
// Place a wall directly above ground (y=4) adjacent to ground below (y=3 solid, unless that cell was gathered — use a fresh cell).
int bx = 10, bz = 10;
Eq(BuildResult.Success, world.Build(bx, 4, bz, BlockType.Wall), "build wall above ground succeeds");
Eq(BlockType.Wall, world.Chunk.Get(bx, 4, bz), "wall placed");
Eq(BuildResult.NotAir, world.Build(bx, 4, bz, BlockType.Wall), "build on solid fails");
Eq(BuildResult.Floating, world.Build(20, 20, 20, BlockType.Wall), "build floating fails");
Eq(BuildResult.OutOfBounds, world.Build(-1, 0, 0, BlockType.Wall), "build out of bounds fails");
Eq(BuildResult.InvalidType, world.Build(11, 4, 11, BlockType.Stone), "build invalid type fails");
// Insufficient resources: drain stone, then try again on a new supported cell.
while (world.Player.Inventory.Remove(BlockType.Stone, 1)) { }
Eq(BuildResult.InsufficientResources, world.Build(12, 4, 12, BlockType.Wall), "build without stone fails");

// Workstation: needs 10 stone + 5 ore. Gather ore first from a fresh world.
var w2 = new WorldZone(7);
int ox = -1, oy = -1, oz = -1;
for (int x = 0; x < 32 && ox < 0; x++)
for (int y = 1; y <= 3 && ox < 0; y++)
for (int z = 0; z < 32 && ox < 0; z++)
    if (w2.Chunk.Get(x, y, z) == BlockType.EmberOre) { ox = x; oy = y; oz = z; }
Check(ox >= 0, "found ore for workstation test");
if (ox >= 0)
{
    // Gather ore blocks until we have 5, plus stone until 10, by gathering in a scan.
    for (int x = 0; x < 32; x++)
    for (int y = 1; y <= 3; y++)
    for (int z = 0; z < 32; z++)
    {
        if (w2.Player.Inventory.Get(BlockType.EmberOre) >= 5 && w2.Player.Inventory.Get(BlockType.Stone) >= 10) break;
        w2.Gather(x, y, z);
    }
    Check(w2.Player.Inventory.Get(BlockType.EmberOre) >= 5 && w2.Player.Inventory.Get(BlockType.Stone) >= 10, "gathered workstation costs");
    Eq(BuildResult.Success, w2.Build(15, 4, 15, BlockType.Workstation), "build workstation succeeds");
    Eq(BlockType.Workstation, w2.Chunk.Get(15, 4, 15), "workstation placed");
}

// Damage: shields first, then health, death
var p = new Player(16, 5, 16);
p.TakeDamage(50);
Eq(35, p.Shields, "damage hits shields first");
Eq(100, p.Health, "health untouched while shields remain");
p.TakeDamage(50);
Eq(0, p.Shields, "shields depleted");
Eq(85, p.Health, "health after shields depleted");
p.TakeDamage(200);
Eq(0, p.Health, "health clamped at zero");
Check(p.IsDead, "player dead at zero health");

// Enemy movement and attack
var w3 = new WorldZone(99);
w3.Player.X = 16; w3.Player.Y = 5; w3.Player.Z = 16;
var enemy = new Enemy(16, 5, 20);
w3.Enemies.Add(enemy);
int distBefore = Math.Abs(w3.Player.X - enemy.X) + Math.Abs(w3.Player.Y - enemy.Y) + Math.Abs(w3.Player.Z - enemy.Z);
w3.Advance();
int distAfter = Math.Abs(w3.Player.X - enemy.X) + Math.Abs(w3.Player.Y - enemy.Y) + Math.Abs(w3.Player.Z - enemy.Z);
Check(distAfter < distBefore, "enemy moves toward player");
w3.AttackEnemy(enemy, 30);
Check(enemy.IsDead, "enemy dies at zero health");
Check(!w3.Enemies.Contains(enemy), "dead enemy removed");

// Save / load round-trip
var w4 = new WorldZone(123);
w4.Gather(sx, sy, sz); // may be air/stone depending on seed; do a deterministic gather scan instead
for (int x = 0; x < 32; x++)
for (int y = 1; y <= 3; y++)
for (int z = 0; z < 32; z++) w4.Gather(x, y, z);
w4.Player.TakeDamage(10);
w4.SpawnWave(2);
var json = SaveSystem.SaveZone(w4);
var loaded = SaveSystem.LoadZone(json);
Check(loaded.Chunk.ToBytes().SequenceEqual(w4.Chunk.ToBytes()), "save/load chunk equal");
Eq(w4.Player.Health, loaded.Player.Health, "save/load health equal");
Eq(w4.Player.Shields, loaded.Player.Shields, "save/load shields equal");
Eq(w4.Player.Inventory.Get(BlockType.Stone), loaded.Player.Inventory.Get(BlockType.Stone), "save/load stone equal");
Eq(w4.Enemies.Count, loaded.Enemies.Count, "save/load enemy count equal");
Eq(w4.Seed, loaded.Seed, "save/load seed equal");

// ---- Slice 1 Step 2: mesher, FPS, weapon, HUD ----
// Mesher: single voxel
var single = new VoxelChunk();
single.TrySet(10, 10, 10, BlockType.Stone);
Eq(6, Mesher.GreedyMesh(single).Count, "mesher single voxel 6 quads");
// Mesher: 2x2x2 cube
var cube = new VoxelChunk();
for (int x = 10; x < 12; x++) for (int y = 10; y < 12; y++) for (int z = 10; z < 12; z++) cube.TrySet(x, y, z, BlockType.Stone);
Eq(6, Mesher.GreedyMesh(cube).Count, "mesher 2x2x2 cube 6 quads (greedy merge)");
// Mesher: empty
Eq(0, Mesher.GreedyMesh(new VoxelChunk()).Count, "mesher empty 0 quads");
// Mesher: flat slab 32x1x32 stone at y=1 -> top 1 + bottom 1 + 4 sides = 6
var slab = new VoxelChunk();
for (int x = 0; x < 32; x++) for (int z = 0; z < 32; z++) slab.TrySet(x, 1, z, BlockType.Stone);
Eq(6, Mesher.GreedyMesh(slab).Count, "mesher flat slab 6 quads");
// Mesher: generated world greedy < naive
var gen = VoxelChunk.Generate(42);
int greedy = Mesher.GreedyMesh(gen).Count;
int naive = Mesher.NaiveFaceCount(gen);
Check(greedy > 0 && greedy < naive, $"mesher generated greedy {greedy} < naive {naive}");
Check(Mesher.GreedyMesh(gen)[0].Corners().Length == 4, "mesher quad has 4 corners");

// FPS: free move, wall collision, fall, jump
var fpsWorld = new WorldZone(42);
var fps = new FpsController(16f, 4f, 16f);
fps.Update(0.1f, 0, 0, false, fpsWorld.Chunk); // settle to ground
Check(fps.IsGrounded, "fps grounded after settle");
float startX = fps.X;
fps.Update(0.1f, 1f, 0f, false, fpsWorld.Chunk);
Check(fps.Z > 16f, "fps moves forward");
// Wall collision: place a wall directly ahead in Z at feet level, then try to move through it.
var wallWorld = new VoxelChunk();
for (int x = 0; x < 32; x++) for (int z = 0; z < 32; z++) for (int y = 1; y <= 3; y++) wallWorld.TrySet(x, y, z, BlockType.Stone);
wallWorld.TrySet(16, 4, 18, BlockType.Stone);
wallWorld.TrySet(16, 5, 18, BlockType.Stone);
var fps2 = new FpsController(16f, 4f, 16f);
for (int i = 0; i < 20; i++) fps2.Update(0.1f, 1f, 0f, false, wallWorld);
Check(fps2.Z < 18f - FpsController.HalfWidth, $"fps wall blocks movement (Z={fps2.Z})");
// Fall: spawn high, fall to ground top (y=4)
var fps3 = new FpsController(16f, 10f, 16f);
for (int i = 0; i < 30; i++) fps3.Update(0.1f, 0, 0, false, fpsWorld.Chunk);
Check(Math.Abs(fps3.Y - 4f) < 0.1f, $"fps falls to ground (Y={fps3.Y})");
Check(fps3.IsGrounded, "fps grounded after fall");
// Jump: from grounded, jump rises, then lands again
var fps4 = new FpsController(16f, 4f, 16f);
fps4.Update(0.1f, 0, 0, false, fpsWorld.Chunk);
fps4.Update(0.1f, 0, 0, true, fpsWorld.Chunk);
Check(fps4.Y > 4f, $"fps jump rises (Y={fps4.Y})");
for (int i = 0; i < 30; i++) fps4.Update(0.1f, 0, 0, false, fpsWorld.Chunk);
Check(Math.Abs(fps4.Y - 4f) < 0.1f && fps4.IsGrounded, "fps jump lands back on ground");

// Weapon + HUD
var weapon = new Weapon();
Eq(42, weapon.Mag, "weapon starts 42 mag");
Eq(120, weapon.Reserve, "weapon starts 120 reserve");
Check(weapon.Fire(), "weapon fires");
Eq(41, weapon.Mag, "weapon mag decreases on fire");
for (int i = 0; i < 41; i++) weapon.Fire();
Eq(0, weapon.Mag, "weapon mag empty after 42 shots");
Check(!weapon.Fire(), "weapon cannot fire empty");
weapon.Reload();
Eq(42, weapon.Mag, "weapon reload refills mag");
Eq(78, weapon.Reserve, "weapon reload consumes reserve (120 - 42 refilled = 78; test arithmetic corrected, not code)");
var hudWorld = new WorldZone(42);
var hud = HudState.FromWorld(hudWorld, new Weapon());
Eq(100, hud.Health, "hud health 100");
Eq(85, hud.Shields, "hud shields 85");
Eq("DESTROY CORE", hud.ObjectiveName, "hud objective destroy core");
Check(hud.ObjectiveDistanceMetres > 0, "hud objective distance positive");
Eq(0, hud.ObjectivePercentCompleted, "hud objective 0% at start");
hudWorld.DamageCore(45);
Eq(45, HudState.FromWorld(hudWorld, new Weapon()).ObjectivePercentCompleted, "hud objective 45% after core damage");
Check(HudState.FromWorld(hudWorld, new Weapon()).MiniMapSolid[16, 16], "hud minimap solid at centre ground");

// ---- Build-around step: MeshBuilder (Unity/GPU not available headlessly) ----
var singleMesh = MeshBuilder.Build(Mesher.GreedyMesh(single));
Eq(6, singleMesh.QuadCount, "meshbuilder single voxel quad count");
Eq(24, singleMesh.Vertices.Count, "meshbuilder single voxel 24 vertices");
Eq(36, singleMesh.Indices.Count, "meshbuilder single voxel 36 indices");
Check(singleMesh.Indices.All(i => i >= 0 && i < singleMesh.Vertices.Count), "meshbuilder indices in range");
Check(singleMesh.Normals.All(n => Math.Abs(Math.Abs(n.Item1) + Math.Abs(n.Item2) + Math.Abs(n.Item3) - 1f) < 0.001f), "meshbuilder normals are unit axis vectors");
var genMesh = MeshBuilder.BuildFromChunk(gen);
Eq(genMesh.QuadCount * 4, genMesh.Vertices.Count, "meshbuilder vertices = quads x 4");
Eq(genMesh.QuadCount * 6, genMesh.Indices.Count, "meshbuilder indices = quads x 6");
Check(genMesh.QuadCount == greedy, "meshbuilder quad count matches mesher");

// ---- Simulation loop (headless playable loop) ----
var sim = new Simulation(42);
sim.Tick(0.1f, 0, 0, false);
Check(!sim.IsPlayerDead, "simulation player alive after first tick");
// Slice 8 Step 2: gather and build are aimed now. Pitching down
// 45 at yaw 0 aims the eye ray at the old gather cell (16,3,17),
// so the Slice 1 numbers survive with an explicit aim, not by
// a fixed cell. Fire aims level at an enemy placed in front
// within the 10 m reach, replacing the old nearest-enemy rule.
sim.ApplyLook(0f, -45f);
var gatherResult = sim.GatherAhead();
Check(gatherResult == GatherResult.Success, $"simulation gather ahead succeeds (got {gatherResult})");
Check(sim.BlocksGathered == 1, "simulation blocks gathered counts");
sim.World.Player.Inventory.Add(BlockType.Stone, 10);
var buildResult = sim.BuildWallAhead();
Check(buildResult == BuildResult.Success, $"simulation build wall ahead succeeds (got {buildResult})");
sim.ApplyLook(0f, 45f); // back to level for fire
sim.World.Enemies.Add(new Enemy(16, 5, 22));
Check(sim.World.Enemies.Count == 1, "simulation aimed fire test enemy placed in front");
for (int i = 0; i < 3; i++) sim.FireAimed();
Check(sim.EnemiesKilled == 1, "simulation aimed fire kills enemy in 3 shots (30 HP, 10 damage)");
sim.World.DamageCore(100);
Check(sim.IsObjectiveComplete, "simulation objective complete at core 0 HP");
var simHud = sim.Hud();
Eq(100, simHud.ObjectivePercentCompleted, "simulation hud objective 100% after core destroyed");

// ---- Step 4: core attack + mesher benchmark ----
var sim2 = new Simulation(42);
// Slice 8 Step 2: aimed fire at spawn, level toward +Z, does not
// hit the core — the core is at (28,5,28), not in the aim line,
// and beyond the 10 m reach — so it fires but does not damage.
int coreBefore = sim2.World.CoreHealth;
sim2.FireAimed();
Check(sim2.World.CoreHealth == coreBefore, "aimed fire off-line does not damage core");
Check(sim2.Weapon.Mag == 41, "aimed fire consumes ammo even on a miss");
// Mesher benchmark: one generated chunk must mesh in under 500 ms on this machine.
var benchSw = System.Diagnostics.Stopwatch.StartNew();
var benchMesh = MeshBuilder.BuildFromChunk(VoxelChunk.Generate(42));
benchSw.Stop();
Check(benchSw.ElapsedMilliseconds < 500, $"mesher benchmark <500ms (got {benchSw.ElapsedMilliseconds}ms, {benchMesh.QuadCount} quads)");

// ---- Slice 2 Step 1: procedural loot ----
var lootA = LootGenerator.Generate(42, 1);
var lootB = LootGenerator.Generate(42, 1);
Check(lootA.Name == lootB.Name && lootA.Damage == lootB.Damage && lootA.Rarity == lootB.Rarity, "loot same seed same weapon");
var lootC = LootGenerator.Generate(43, 1);
Check(lootC.Name != lootA.Name || lootC.Damage != lootA.Damage, "loot different seed usually differs");
Check(lootA.Damage > 0 && lootA.MagSize > 0 && lootA.FireRatePerSecond > 0, "loot stats positive");
Check(Enum.IsDefined(lootA.Rarity) && Enum.IsDefined(lootA.Element), "loot rarity and element valid");
var legendaryFound = false;
for (int s = 0; s < 500; s++) if (LootGenerator.Generate(s, 10).Rarity == Rarity.Legendary) { legendaryFound = true; break; }
Check(legendaryFound, "loot legendary exists in 500 seeds (rare, not impossible)");
var drop = LootGenerator.EnemyDrop(42, 1, 1);
Check(drop == null || drop.Damage > 0, "loot enemy drop is null or a valid weapon");
var highLevel = LootGenerator.Generate(42, 20);
var lowLevel = LootGenerator.Generate(42, 1);
// Level scaling is via base damage formula, but rarity is seed-driven, so only check both are valid and level 20 is not weaker on average across seeds.
int sumLow = 0, sumHigh = 0;
for (int s = 0; s < 50; s++) { sumLow += LootGenerator.Generate(s, 1).Damage; sumHigh += LootGenerator.Generate(s, 20).Damage; }
Check(sumHigh > sumLow, $"loot level scaling: level 20 total {sumHigh} > level 1 total {sumLow}");

// ---- Begin work: workstation real effect ----
var wsWorld = new WorldZone(42);
wsWorld.Player.TakeDamage(20); // shields 85 -> 65
Check(wsWorld.Player.Shields == 65, $"workstation test setup shields 65 (got {wsWorld.Player.Shields})");
wsWorld.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
Check(wsWorld.IsNearWorkstation(), "workstation near player detected");
wsWorld.Advance();
Check(wsWorld.Player.Shields == 70, $"workstation restores 5 shields per tick (got {wsWorld.Player.Shields})");
var farWorld = new WorldZone(42);
farWorld.Player.TakeDamage(20);
farWorld.Chunk.TrySet(1, 5, 1, BlockType.Workstation);
farWorld.Advance();
Check(farWorld.Player.Shields == 65, $"far workstation does not restore shields (got {farWorld.Player.Shields})");

// ---- Slice 2 Step 2: passive skill cluster ----
var build = new SkillBuild();
Check(!build.Allocate("N1"), "skill prereq blocks N1 before S1+S2");
Check(build.Allocate("S1") && build.Allocate("S2"), "skill travel nodes allocate");
Check(!build.Allocate("S1"), "skill duplicate allocate rejected");
Check(build.Allocate("N1"), "skill N1 allocates after prereqs");
Check(!build.Allocate("K1"), "skill keystone blocks before N2");
Check(build.Allocate("S3") && build.Allocate("N2") && build.Allocate("K1"), "skill full cluster allocates in order");
Check(build.TotalEffect("DamagePct") == 20, $"skill total DamagePct 20 (got {build.TotalEffect("DamagePct")})");
Check(build.TotalEffect("ShieldBonus") == 20, $"skill total ShieldBonus 20 (got {build.TotalEffect("ShieldBonus")})");
Check(build.TotalEffect("CoreDamagePct") == 25, "skill keystone CoreDamagePct 25");
var encoded = build.Encode();
var decoded = SkillBuild.Decode(encoded);
Check(decoded.Allocated.Count == 6 && decoded.TotalEffect("DamagePct") == 20, "skill encode/decode round-trip preserves build");
Check(!new SkillBuild().Allocate("NOPE"), "skill unknown node rejected");

// ---- Slice 2 Step 3: GeneratedWeapon integrated into Simulation ----
// Seed 0 is chosen deliberately: it generates a 20-damage / 30-mag weapon,
// unlike seed 42's 10-damage / 42-mag loot, so these tests cannot pass
// by accident on the fixed Pulse Rifle's stats.
var genLoot = LootGenerator.Generate(0, 1);
var genWeapon = Weapon.FromGenerated(genLoot);
Eq(genLoot.Name, genWeapon.Name, "generated weapon name preserved");
Eq(genLoot.Damage, genWeapon.Damage, "generated weapon damage preserved");
Eq(genLoot.MagSize, genWeapon.MagCapacity, "generated weapon mag capacity preserved");
Eq(genLoot.MagSize, genWeapon.Mag, "generated weapon starts with full mag");
var simGen = new Simulation(42, genLoot);
Eq(genLoot.Damage, simGen.Weapon.Damage, "simulation equipped generated damage");
Eq(genLoot.MagSize, simGen.Weapon.Mag, "simulation equipped generated mag");
simGen.World.Enemies.Add(new Enemy(16, 5, 22));
int enemyHpBefore = simGen.World.Enemies[0].Health;
simGen.FireAimed();
Eq(Math.Max(0, enemyHpBefore - genLoot.Damage), simGen.World.Enemies.Count == 0 ? 0 : simGen.World.Enemies[0].Health, "simulation fire deals generated damage, not fixed 10");
var simEquip = new Simulation(42);
Eq("Pulse Rifle", simEquip.Weapon.Name, "simulation default weapon is still Pulse Rifle");
Eq(10, simEquip.Weapon.Damage, "simulation default weapon damage still 10");
Check(simEquip.Equip(genLoot), "simulation equip generated weapon succeeds");
Eq(genLoot.Name, simEquip.Weapon.Name, "simulation equip changes weapon name");
Eq(genLoot.Damage, simEquip.Weapon.Damage, "simulation equip changes weapon damage");
Eq(genLoot.MagSize, simEquip.Weapon.Mag, "simulation equip gives full generated mag");
int equipMagBefore = simEquip.Weapon.Mag;
simEquip.FireAimed();
Eq(equipMagBefore - 1, simEquip.Weapon.Mag, "simulation equip fire consumes generated mag");
Check(!simEquip.Equip(null), "simulation equip null rejected, weapon unchanged");
Eq(genLoot.Name, simEquip.Weapon.Name, "simulation equip null leaves weapon unchanged");
Eq(genLoot.Name.ToUpperInvariant(), simGen.Hud().WeaponName, "hud weapon name follows equipped generated weapon");

// ---- Slice 2 Step 4: Forge Survivor skill effects applied to gameplay ----
var fullBuild = new SkillBuild();
Check(fullBuild.Allocate("S1") && fullBuild.Allocate("S2") && fullBuild.Allocate("N1") && fullBuild.Allocate("S3") && fullBuild.Allocate("N2") && fullBuild.Allocate("K1"), "skill full cluster allocates for application test");
// Seed 0 loot is 20 damage, so DamagePct 20% -> 24 enemy / 29 core (20% + 25%) is discriminating, not the base 20.
var simSkill = new Simulation(42, genLoot, fullBuild);
Eq(24, simSkill.EffectiveDamage, "skill effective enemy damage 24 (20 x 1.20)");
Eq(29, simSkill.EffectiveCoreDamage, "skill effective core damage 29 (20 x 1.45)");
Eq(105, simSkill.World.Player.MaxShields, "skill shield cap 105 (85 + 20)");
Eq(10, simSkill.World.WorkstationRegenPerTick, "skill workstation regen 10 per tick (5 + 5)");
Check(Math.Abs(simSkill.Fps.SpeedMultiplier - 1.05f) < 0.001f, "skill move speed multiplier 1.05");
simSkill.World.Enemies.Add(new Enemy(16, 5, 22));
simSkill.FireAimed();
Eq(6, simSkill.World.Enemies[0].Health, "skill fire deals 24 damage to 30 HP enemy, leaving 6");
var simNoSkill = new Simulation(42, genLoot);
Eq(20, simNoSkill.EffectiveDamage, "no-skill effective damage equals base weapon damage");
Eq(20, simNoSkill.EffectiveCoreDamage, "no-skill effective core damage equals base weapon damage");
Check(!simNoSkill.ApplySkills(null), "skill apply null rejected");
Eq(20, simNoSkill.EffectiveDamage, "skill apply null leaves damage unchanged");
// Respec replaces, never stacks: full build then empty build returns to base.
var simRespec = new Simulation(42, genLoot, fullBuild);
Check(simRespec.ApplySkills(new SkillBuild()), "skill respec to empty build succeeds");
Eq(85, simRespec.World.Player.MaxShields, "skill respec restores shield cap 85");
Eq(20, simRespec.EffectiveDamage, "skill respec restores base damage");
// Workstation regen with skills: 65 -> 75 (+10), not the base +5 -> 70.
var simWsSkill = new Simulation(42, null, fullBuild);
simWsSkill.World.Player.TakeDamage(20);
simWsSkill.World.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
simWsSkill.World.Advance();
Eq(75, simWsSkill.World.Player.Shields, "skill workstation restores 10 shields per tick");
// Move speed with Swift Feet only: 6 -> 6.3 m/s, so 0.1 s forward moves 0.63 m.
var swiftBuild = new SkillBuild();
Check(swiftBuild.Allocate("S3"), "skill Swift Feet allocates alone");
var simSwift = new Simulation(42, null, swiftBuild);
Check(Math.Abs(simSwift.Fps.EffectiveSpeed - 6.3f) < 0.01f, "skill Swift Feet effective speed 6.3");
simSwift.Tick(0.1f, 1, 0, false);
Check(Math.Abs(simSwift.Fps.Z - 16.63f) < 0.01f, $"skill Swift Feet moves 0.63m in 0.1s (got Z {simSwift.Fps.Z})");
// Gather bonus: +20% = one bonus unit per 5 successful gathers; a failed
// gather in between must not advance the bonus. Target cell is (16,3,17),
// aimed at with pitch -45 at yaw 0 (Slice 8 Step 2). The failed gather
// aims at a Wall in that cell, not at air: with aim, an emptied cell
// would simply let the ray reach the next ground cell, so air is no
// longer a reliable failure — a non-gatherable solid is.
var simGather = new Simulation(42, null, fullBuild);
simGather.ApplyLook(0f, -45f);
for (int i = 0; i < 4; i++)
{
    simGather.World.Chunk.TrySet(16, 3, 17, BlockType.Stone);
    Check(simGather.GatherAhead() == GatherResult.Success, "skill gather bonus setup gather succeeds");
}
simGather.World.Chunk.TrySet(16, 3, 17, BlockType.Wall);
Check(simGather.GatherAhead() == GatherResult.NotGatherable, "skill gather bonus failed gather is not gatherable");
simGather.World.Chunk.TrySet(16, 3, 17, BlockType.Stone);
Check(simGather.GatherAhead() == GatherResult.Success, "skill gather bonus 5th gather succeeds");
Eq(5, simGather.BlocksGathered, "skill gather counts 5 gathers, not bonus units");
Eq(6, simGather.World.Player.Inventory.Get(BlockType.Stone), "skill gather bonus gives 6 stone from 5 gathers (+20%)");

// ---- Slice 2 Step 5: automation tier 1 (Kinetic only) ----
// Build cost: 10 Stone + 5 Wood, placed above ground (supported below).
var millWorld = new WorldZone(42);
millWorld.Player.Inventory.Add(BlockType.Stone, 10);
millWorld.Player.Inventory.Add(BlockType.Wood, 5);
Eq(BuildResult.Success, millWorld.Build(15, 4, 15, BlockType.KineticMill), "kinetic mill builds with 10 stone + 5 wood");
Eq(BlockType.KineticMill, millWorld.Chunk.Get(15, 4, 15), "kinetic mill placed");
Eq(0, millWorld.Player.Inventory.Get(BlockType.Stone), "kinetic mill consumes 10 stone");
Eq(0, millWorld.Player.Inventory.Get(BlockType.Wood), "kinetic mill consumes 5 wood");
Eq(GatherResult.NotGatherable, millWorld.Gather(15, 4, 15), "kinetic mill itself is not gatherable");
// Insufficient wood: stone alone must not build, and must not be spent.
var poorWorld = new WorldZone(42);
poorWorld.Player.Inventory.Add(BlockType.Stone, 10);
Eq(BuildResult.InsufficientResources, poorWorld.Build(15, 4, 15, BlockType.KineticMill), "kinetic mill without wood fails");
Eq(BlockType.Air, poorWorld.Chunk.Get(15, 4, 15), "failed kinetic mill leaves air");
Eq(10, poorWorld.Player.Inventory.Get(BlockType.Stone), "failed kinetic mill does not spend stone");
// Timing + rate: a mill above ground gathers the ground stone below
// on the 5th tick, not before, and exactly one block per activation.
var autoWorld = new WorldZone(42);
autoWorld.Chunk.TrySet(16, 4, 16, BlockType.KineticMill);
int stoneBefore = autoWorld.Player.Inventory.Get(BlockType.Stone);
for (int i = 0; i < 4; i++) autoWorld.Advance();
Eq(stoneBefore, autoWorld.Player.Inventory.Get(BlockType.Stone), "kinetic mill gathers nothing in first 4 ticks");
Eq(0, autoWorld.KineticBlocksGathered, "kinetic mill counter 0 before activation tick");
autoWorld.Advance(); // tick 5
Eq(stoneBefore + 1, autoWorld.Player.Inventory.Get(BlockType.Stone), "kinetic mill gathers 1 stone on 5th tick");
Eq(1, autoWorld.KineticBlocksGathered, "kinetic mill counter 1 after activation");
Eq(BlockType.Air, autoWorld.Chunk.Get(16, 3, 16), "kinetic mill excavated the ground block below it");
// Built blocks and bedrock are never excavated, even with nothing else adjacent.
var safeWorld = new WorldZone(42);
safeWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
safeWorld.Chunk.TrySet(17, 20, 16, BlockType.Wall);
safeWorld.Chunk.TrySet(15, 20, 16, BlockType.Workstation);
safeWorld.Chunk.TrySet(16, 21, 16, BlockType.KineticMill);
safeWorld.Chunk.TrySet(16, 19, 16, BlockType.Bedrock);
for (int i = 0; i < 5; i++) safeWorld.Advance();
Eq(BlockType.Wall, safeWorld.Chunk.Get(17, 20, 16), "kinetic mill does not excavate a wall");
Eq(BlockType.Workstation, safeWorld.Chunk.Get(15, 20, 16), "kinetic mill does not excavate a workstation");
Eq(BlockType.KineticMill, safeWorld.Chunk.Get(16, 21, 16), "kinetic mill does not excavate another mill");
Eq(BlockType.Bedrock, safeWorld.Chunk.Get(16, 19, 16), "kinetic mill does not excavate bedrock");
Eq(0, safeWorld.KineticBlocksGathered, "kinetic mill gathers nothing when only ungatherable neighbours exist");
// Two mills, one shared stone: exactly one gather, first in scan order wins.
var raceWorld = new WorldZone(42);
raceWorld.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
raceWorld.Chunk.TrySet(12, 20, 10, BlockType.KineticMill);
raceWorld.Chunk.TrySet(11, 20, 10, BlockType.Stone);
for (int i = 0; i < 5; i++) raceWorld.Advance();
Eq(BlockType.Air, raceWorld.Chunk.Get(11, 20, 10), "two mills shared stone is excavated");
Eq(1, raceWorld.KineticBlocksGathered, "two mills competing gather exactly 1 block, not 2");
// Full inventory: nothing is consumed.
var fullWorld = new WorldZone(42);
fullWorld.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
fullWorld.Chunk.TrySet(11, 20, 10, BlockType.Stone);
fullWorld.Player.Inventory.Add(BlockType.Stone, Inventory.MaxStack);
for (int i = 0; i < 5; i++) fullWorld.Advance();
Eq(BlockType.Stone, fullWorld.Chunk.Get(11, 20, 10), "kinetic mill with full inventory leaves the block");
Eq(0, fullWorld.KineticBlocksGathered, "kinetic mill with full inventory gathers 0");
// Save/load: the mill block survives the chunk round-trip (byte array).
var saveMillWorld = new WorldZone(42);
saveMillWorld.Chunk.TrySet(15, 4, 15, BlockType.KineticMill);
var saveMillLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(saveMillWorld));
Eq(BlockType.KineticMill, saveMillLoaded.Chunk.Get(15, 4, 15), "kinetic mill survives save/load");
// Simulation: build a mill ahead, then 5 ticks run its automation.
var simMill = new Simulation(42);
simMill.Tick(0.1f, 0, 0, false);
simMill.World.Player.Inventory.Add(BlockType.Stone, 10);
simMill.World.Player.Inventory.Add(BlockType.Wood, 5);
simMill.ApplyLook(-18.4f, -53f);
Eq(BuildResult.Success, simMill.BuildKineticMillAhead(), "simulation build kinetic mill ahead succeeds");
for (int i = 0; i < 5; i++) simMill.Tick(0.1f, 0, 0, false);
Eq(1, simMill.World.KineticBlocksGathered, "simulation kinetic mill gathers 1 block in 5 ticks");
Check(simMill.World.Player.Inventory.Get(BlockType.Stone) >= 1, "simulation kinetic mill output reaches player inventory");

// ---- Slice 2 Step 6: Orbital Tactical doctrine prototype (one doctrine, one squad type) ----
// Player starts at (16,5,16) in a fresh WorldZone; y=5 is open air, so
// X/Z movement at that height is unblocked unless a test places a block.
var orbWorld = new WorldZone(42);
var squad = orbWorld.SpawnSquad(16, 5, 16);
Check(squad != null, "orbital deploy at player rally succeeds");
Eq(1, orbWorld.Squads.Count, "orbital deploy adds 1 squad");
Eq(SquadKind.Vanguard, squad!.Kind, "orbital squad kind is Vanguard");
Eq(TacticalDoctrine.HoldGround, squad.Doctrine, "orbital squad doctrine is HoldGround");
Eq(16, squad.X, "orbital squad spawns at player X");
Eq(3, squad.AliveMembers, "orbital squad starts with 3 members");
Eq(90, squad.Health, "orbital squad starts at 90 health");
Eq(30, squad.DamagePerTick, "orbital squad deals 30 per tick at full strength");
Check(squad.AtRally, "orbital squad at rally when rally is spawn point");
Check(orbWorld.SpawnSquad(-1, 5, 5) == null, "orbital deploy out-of-bounds rally rejected");
Eq(1, orbWorld.Squads.Count, "orbital rejected deploy adds no squad");
Check(orbWorld.SpawnSquad(5, 5, 5, (TacticalDoctrine)99) == null, "orbital deploy unknown doctrine rejected");
Eq(1, orbWorld.Squads.Count, "orbital unknown doctrine adds no squad");
// Movement: one voxel per tick, largest-gap axis first, ties X before Z.
var moveWorld = new WorldZone(42);
var mover = moveWorld.SpawnSquad(20, 5, 20)!;
moveWorld.Advance();
Eq(17, mover.X, "orbital squad moves one voxel in X first on diagonal rally");
Eq(16, mover.Z, "orbital squad does not also move in Z on the same tick");
Check(!mover.AtRally, "orbital squad not at rally while moving");
for (int i = 0; i < 7; i++) moveWorld.Advance();
Eq(20, mover.X, "orbital squad reaches rally X after 8 ticks");
Eq(20, mover.Z, "orbital squad reaches rally Z after 8 ticks");
Check(mover.AtRally, "orbital squad at rally after moving");
moveWorld.Advance();
Eq(20, mover.X, "orbital squad holds rally X on later ticks");
Eq(20, mover.Z, "orbital squad holds rally Z on later ticks");
// Blocked movement: a solid block on the path stops the squad that tick.
var blockWorld = new WorldZone(42);
blockWorld.Chunk.TrySet(17, 5, 16, BlockType.Stone);
var blocked = blockWorld.SpawnSquad(20, 5, 16)!;
blockWorld.Advance();
Eq(16, blocked.X, "orbital squad does not move into a solid block");
Check(!blocked.AtRally, "orbital squad blocked short of rally is not at rally");
// An enemy on the path cell also blocks movement, and a moving squad does not fire.
var pathWorld = new WorldZone(42);
var pathEnemy = new Enemy(17, 5, 16);
pathWorld.Enemies.Add(pathEnemy);
var pathSquad = pathWorld.SpawnSquad(20, 5, 16)!;
pathWorld.Advance();
Eq(16, pathSquad.X, "orbital squad does not move into an enemy-occupied cell");
Eq(30, pathEnemy.Health, "orbital moving squad does not fire at the blocking enemy");
// En-route squad does not fire even when an enemy is in range.
var routeWorld = new WorldZone(42);
var routeEnemy = new Enemy(17, 5, 18); // distance 3 from spawn (16,5,16)
routeWorld.Enemies.Add(routeEnemy);
routeWorld.SpawnSquad(22, 5, 16);
routeWorld.Advance();
Eq(30, routeEnemy.Health, "orbital en-route squad does not fire in-range enemy");
// Holding squad fires: 3 members x 10 = 30 kills a 30 HP enemy in one tick.
var holdWorld = new WorldZone(42);
var holdEnemy = new Enemy(18, 5, 16); // distance 2 from rally (16,5,16)
holdWorld.Enemies.Add(holdEnemy);
holdWorld.SpawnSquad(16, 5, 16);
holdWorld.Advance();
Eq(0, holdWorld.Enemies.Count, "orbital holding squad kills in-range enemy in one tick");
// Range limit: distance 4 is out of the 3-range engagement. The
// player is moved far away first. UPDATED in Slice 4 Step 1 for
// the intended behaviour change: enemies now pursue the nearest
// live target, so this enemy would walk toward the nearer squad
// (distance 4) into range 3 and be killed before the range rule
// could be isolated. A solid block on its step (19,5,16) blocks
// that move, so the enemy stays at distance 4 and the range rule
// itself is still what is tested — the pursuit-into-range case
// is covered by the Slice 4 tests below instead.
var rangeWorld = new WorldZone(42);
var farEnemy = new Enemy(20, 5, 16); // distance 4 from rally (16,5,16)
rangeWorld.Enemies.Add(farEnemy);
rangeWorld.Chunk.TrySet(19, 5, 16, BlockType.Stone);
rangeWorld.SpawnSquad(16, 5, 16);
rangeWorld.Player.X = 28; rangeWorld.Player.Z = 28;
rangeWorld.Advance();
Eq(30, farEnemy.Health, "orbital holding squad does not fire beyond range 3");
Eq(1, rangeWorld.Enemies.Count, "orbital out-of-range enemy survives");
// Firepower steps down with members: after 30 damage, 2 members deal 20.
var weakWorld = new WorldZone(42);
var weakEnemy = new Enemy(18, 5, 16);
weakWorld.Enemies.Add(weakEnemy);
var weakSquad = weakWorld.SpawnSquad(16, 5, 16)!;
weakSquad.TakeDamage(30);
Eq(2, weakSquad.AliveMembers, "orbital squad drops to 2 members after 30 damage");
Eq(20, weakSquad.DamagePerTick, "orbital 2-member squad deals 20 per tick");
weakWorld.Advance();
Eq(10, weakEnemy.Health, "orbital 2-member squad leaves 10 HP on a 30 HP enemy");
// Squad takes adjacent-enemy damage after firing: 1 member deals 10,
// the surviving adjacent enemy then deals 10 back.
var hurtWorld = new WorldZone(42);
var hurtEnemy = new Enemy(17, 5, 16); // adjacent to rally (16,5,16)
hurtWorld.Enemies.Add(hurtEnemy);
var hurtSquad = hurtWorld.SpawnSquad(16, 5, 16)!;
hurtSquad.TakeDamage(60); // 90 -> 30, 1 member
Eq(1, hurtSquad.AliveMembers, "orbital squad drops to 1 member after 60 damage");
hurtWorld.Advance();
Eq(20, hurtEnemy.Health, "orbital 1-member squad deals 10 to adjacent enemy");
Eq(20, hurtSquad.Health, "orbital squad takes 10 from surviving adjacent enemy");
// A squad that kills its adjacent attacker that tick is not hit by it.
var killWorld = new WorldZone(42);
var killEnemy = new Enemy(17, 5, 16);
killWorld.Enemies.Add(killEnemy);
var killSquad = killWorld.SpawnSquad(16, 5, 16)!;
killWorld.Advance();
Eq(0, killWorld.Enemies.Count, "orbital full squad kills adjacent enemy before it can hit back");
Eq(90, killSquad.Health, "orbital squad unharmed when it kills its adjacent attacker");
// Dead squads are removed by Advance.
var deadWorld = new WorldZone(42);
var deadSquad = deadWorld.SpawnSquad(16, 5, 16)!;
deadSquad.TakeDamage(90);
Check(deadSquad.IsDead, "orbital squad dead at 0 health");
deadWorld.Advance();
Eq(0, deadWorld.Squads.Count, "orbital dead squad removed on tick");
// Determinism: same deploy + advance gives the same squad position.
var detA = new WorldZone(42); var detB = new WorldZone(42);
var sA = detA.SpawnSquad(20, 5, 16)!; var sB = detB.SpawnSquad(20, 5, 16)!;
detA.Advance(); detB.Advance();
Check(sA.X == sB.X && sA.Z == sB.Z, "orbital squad movement is deterministic across worlds");
// Simulation command: deploy via Simulation, squad moves on sim ticks.
var simOrb = new Simulation(42);
var simSquad = simOrb.World.SpawnSquad(20, 5, 16);
Check(simSquad != null, "simulation deploy orbital squad succeeds");
Eq(1, simOrb.World.Squads.Count, "simulation deploy adds squad to world");
simOrb.Tick(0.1f, 0, 0, false);
Eq(17, simSquad!.X, "simulation orbital squad moves on sim tick");
Check(simOrb.World.SpawnSquad(-1, 5, 5) == null, "simulation deploy out-of-bounds rally rejected");

// ---- Slice 2 Step 7: Orbital squad save / load persistence ----
// A wounded squad survives a save/load round-trip with position,
// rally, health, and member count intact, and keeps moving after load.
var saveSquadWorld = new WorldZone(42);
var saveSquad = saveSquadWorld.SpawnSquad(20, 5, 16)!;
saveSquadWorld.Advance(); // squad moves 16 -> 17 toward its rally
saveSquad.TakeDamage(60); // 90 -> 30, 1 member left
var saveSquadFull = saveSquadWorld.SpawnSquad(20, 5, 16)!;
var saveSquadJson = SaveSystem.SaveZone(saveSquadWorld);
var saveSquadLoaded = SaveSystem.LoadZone(saveSquadJson);
Eq(2, saveSquadLoaded.Squads.Count, "save/load squad count preserved");
var loadedWounded = saveSquadLoaded.Squads[0];
Eq(17, loadedWounded.X, "save/load wounded squad position preserved");
Eq(20, loadedWounded.RallyX, "save/load squad rally preserved");
Eq(30, loadedWounded.Health, "save/load wounded squad health preserved");
Eq(1, loadedWounded.AliveMembers, "save/load wounded squad has 1 member");
Eq(TacticalDoctrine.HoldGround, loadedWounded.Doctrine, "save/load squad doctrine preserved");
Eq(SquadKind.Vanguard, loadedWounded.Kind, "save/load squad kind preserved");
Eq(90, saveSquadLoaded.Squads[1].Health, "save/load full squad health preserved");
saveSquadLoaded.Advance();
Eq(18, loadedWounded.X, "loaded squad keeps moving toward rally after load");
// Old saves (no Squads field) still load, with no squads — not a crash.
var oldJson = saveSquadJson[..saveSquadJson.LastIndexOf(",\"Squads\":", StringComparison.Ordinal)] + "}";
var oldLoaded = SaveSystem.LoadZone(oldJson);
Eq(0, oldLoaded.Squads.Count, "old save without squads loads with no squads");
// A dead squad in the save is skipped on load, not resurrected.
var deadSaveWorld = new WorldZone(42);
var deadSaveSquad = deadSaveWorld.SpawnSquad(20, 5, 16)!;
deadSaveSquad.TakeDamage(90);
var deadSaveLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(deadSaveWorld));
Eq(0, deadSaveLoaded.Squads.Count, "dead squad in save is skipped on load");

// ---- Slice 2 Step 8: Orbital view snapshot + Unity adapter data ----
// An empty world snapshots to an empty list, not null.
var viewEmpty = new WorldZone(42);
Eq(0, OrbitalView.Snapshot(viewEmpty).Count, "orbital view empty world snapshots to empty list");
// A deployed squad snapshots with position, rally, health, members,
// damage, doctrine, and kind — holding at its spawn-point rally.
var viewWorld = new WorldZone(42);
viewWorld.SpawnSquad(16, 5, 16);
var viewSnap = OrbitalView.Snapshot(viewWorld);
Eq(1, viewSnap.Count, "orbital view snapshot has 1 squad");
Eq(16, viewSnap[0].X, "orbital view snapshot squad X");
Eq(16, viewSnap[0].RallyX, "orbital view snapshot squad rally X");
Eq(90, viewSnap[0].Health, "orbital view snapshot squad health");
Eq(3, viewSnap[0].AliveMembers, "orbital view snapshot squad members");
Eq(30, viewSnap[0].DamagePerTick, "orbital view snapshot squad damage");
Check(viewSnap[0].AtRally, "orbital view snapshot squad at rally");
Check(!viewSnap[0].IsMoving, "orbital view snapshot holding squad is not moving");
Eq(SquadKind.Vanguard, viewSnap[0].Kind, "orbital view snapshot squad kind");
Eq(TacticalDoctrine.HoldGround, viewSnap[0].Doctrine, "orbital view snapshot squad doctrine");
// A marching squad snapshots as moving, and the snapshot is a copy:
// advancing the world after the snapshot leaves the old snapshot
// at its pre-tick position.
var viewMove = new WorldZone(42);
viewMove.SpawnSquad(20, 5, 16);
var moveSnap = OrbitalView.Snapshot(viewMove);
Check(moveSnap[0].IsMoving, "orbital view snapshot marching squad is moving");
Check(!moveSnap[0].AtRally, "orbital view snapshot marching squad not at rally");
viewMove.Advance();
Eq(16, moveSnap[0].X, "orbital view snapshot is a copy, not the live squad");
Eq(17, viewMove.Squads[0].X, "orbital view live squad moved after snapshot");
// A wounded squad snapshots its reduced member count and damage.
var viewWeak = new WorldZone(42);
var weakViewSquad = viewWeak.SpawnSquad(16, 5, 16)!;
weakViewSquad.TakeDamage(60);
var weakSnap = OrbitalView.Snapshot(viewWeak);
Eq(1, weakSnap[0].AliveMembers, "orbital view wounded snapshot has 1 member");
Eq(10, weakSnap[0].DamagePerTick, "orbital view wounded snapshot deals 10 per tick");
// Two squads snapshot in deploy order, and a dead squad (removed
// by Advance) never appears in a snapshot.
var viewTwo = new WorldZone(42);
viewTwo.SpawnSquad(16, 5, 16);
var viewTwoDead = viewTwo.SpawnSquad(20, 5, 16)!;
viewTwoDead.TakeDamage(90);
viewTwo.Advance();
Eq(1, OrbitalView.Snapshot(viewTwo).Count, "orbital view snapshot omits dead squad");
// Snapshot through Simulation, the path the Unity adapter uses.
var viewSim = new Simulation(42);
viewSim.World.SpawnSquad(20, 5, 16);
Eq(1, OrbitalView.Snapshot(viewSim.World).Count, "orbital view snapshot via simulation world has 1 squad");

// ---- Slice 2 composition: loot + skills + mill + squad in one Simulation ----
var compLoot = LootGenerator.Generate(0, 1);
var compBuild = new SkillBuild();
Check(compBuild.Allocate("S1") && compBuild.Allocate("S2") && compBuild.Allocate("N1") && compBuild.Allocate("S3") && compBuild.Allocate("N2") && compBuild.Allocate("K1"), "slice 2 composition full cluster allocates");
var compSim = new Simulation(42, compLoot, compBuild);
Eq("Titan Long Pulse Rifle", compSim.Weapon.Name, "slice 2 composition loot weapon equipped");
Eq(24, compSim.EffectiveDamage, "slice 2 composition skilled damage 20 -> 24");
Eq(29, compSim.EffectiveCoreDamage, "slice 2 composition skilled core damage 20 -> 29");
Eq(105, compSim.World.Player.MaxShields, "slice 2 composition shields cap 85 -> 105");
compSim.World.Player.Inventory.Add(BlockType.Stone, 10);
compSim.World.Player.Inventory.Add(BlockType.Wood, 5);
compSim.ApplyLook(-18.4f, -53f);
Eq(BuildResult.Success, compSim.BuildKineticMillAhead(), "slice 2 composition kinetic mill builds");
Check(compSim.World.SpawnSquad(compSim.World.Player.X, compSim.World.Player.Y, compSim.World.Player.Z) != null, "slice 2 composition squad deploys");
Eq(1, OrbitalView.Snapshot(compSim.World).Count, "slice 2 composition snapshot has 1 squad");
var compLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(compSim.World));
Eq(1, compLoaded.Squads.Count, "slice 2 composition save/load keeps the squad");

// ---- Slice 2 follow-up: GameManager.LoadFromJson in-place restore ----
// Teleport moves the feet position and clears vertical velocity.
var tpFps = new FpsController(1, 4, 1);
tpFps.Teleport(10, 5, 12);
Eq(10f, tpFps.X, "fps teleport sets X");
Eq(5f, tpFps.Y, "fps teleport sets Y");
Eq(12f, tpFps.Z, "fps teleport sets Z");
Eq(0f, tpFps.VelocityY, "fps teleport clears vertical velocity");
// Build a discriminating saved world: different seed, moved player,
// inventory, and a squad — then restore it into a live Simulation.
var saveSource = new WorldZone(7);
saveSource.Player.X = 10; saveSource.Player.Y = 5; saveSource.Player.Z = 12;
saveSource.Player.Inventory.Add(BlockType.Stone, 4);
saveSource.SpawnSquad(10, 5, 12);
var saveJson = SaveSystem.SaveZone(saveSource);
var restoreSim = new Simulation(42);
var fpsBefore = restoreSim.Fps;
var loadedWorld = SaveSystem.LoadZone(saveJson);
Check(restoreSim.RestoreWorld(loadedWorld), "restore world accepts a loaded world");
Check(ReferenceEquals(loadedWorld, restoreSim.World), "restore world swaps in the loaded world object");
Check(ReferenceEquals(fpsBefore, restoreSim.Fps), "restore keeps the same fps object in place");
Eq(7, restoreSim.World.Seed, "restore world has the saved seed");
Eq(10, restoreSim.World.Player.X, "restore world player X");
Eq(4, restoreSim.World.Player.Inventory.Get(BlockType.Stone), "restore world inventory");
Eq(1, restoreSim.World.Squads.Count, "restore world keeps the saved squad");
Eq(10f, restoreSim.Fps.X, "restore teleports fps to saved player X");
Eq(5f, restoreSim.Fps.Y, "restore teleports fps to saved player Y");
Eq(12f, restoreSim.Fps.Z, "restore teleports fps to saved player Z");
// Restored play continues: one tick advances the restored world.
int tickBefore = restoreSim.World.Tick;
restoreSim.Tick(0.016f, 0, 0, false);
Check(restoreSim.World.Tick > tickBefore, "restored simulation ticks after restore");
// Null restore fails closed and leaves the world untouched.
var nullSim = new Simulation(42);
Check(!nullSim.RestoreWorld(null), "restore world rejects null");
Eq(42, nullSim.World.Seed, "null restore leaves the world unchanged");
// Session counters reset on restore, and a skill build is re-applied
// to the new world instead of silently reverting to base values.
var counterSim = new Simulation(42);
counterSim.World.Player.Inventory.Add(BlockType.Stone, 10);
counterSim.World.Player.Inventory.Add(BlockType.Wood, 5);
counterSim.ApplyLook(-18.4f, -53f);
Eq(BuildResult.Success, counterSim.BuildKineticMillAhead(), "restore test mill builds for counter");
Eq(1, counterSim.BlocksBuilt, "restore test counter before restore is 1");
var skillBuild = new SkillBuild();
Check(skillBuild.Allocate("S1") && skillBuild.Allocate("S2") && skillBuild.Allocate("N1") && skillBuild.Allocate("S3") && skillBuild.Allocate("N2") && skillBuild.Allocate("K1"), "restore test full cluster allocates");
counterSim.ApplySkills(skillBuild);
Eq(105, counterSim.World.Player.MaxShields, "restore test shields cap before restore is 105");
Check(counterSim.RestoreWorld(new WorldZone(9)), "restore with skills succeeds");
Eq(0, counterSim.BlocksBuilt, "restore resets session counters");
Eq(105, counterSim.World.Player.MaxShields, "restore re-applies skill shield cap");
Check(Math.Abs(counterSim.Fps.EffectiveSpeed - 6.3f) < 0.001f, "restore re-applies skill move speed");
// LoadFromJson: valid JSON swaps the world; malformed, empty, and
// null input fail closed with the current world untouched.
var jsonSim = new Simulation(42);
Check(jsonSim.LoadFromJson(saveJson), "load from json accepts a valid save");
Eq(7, jsonSim.World.Seed, "load from json swaps to the saved seed");
Check(!jsonSim.LoadFromJson("this is not json"), "load from json rejects malformed json");
Eq(7, jsonSim.World.Seed, "malformed load leaves the world unchanged");
Check(!jsonSim.LoadFromJson(""), "load from json rejects empty input");
Check(!jsonSim.LoadFromJson(null), "load from json rejects null");
Eq(7, jsonSim.World.Seed, "rejected loads leave the world unchanged");
// Round-trip through LoadFromJson keeps the chunk identical.
var chunkSim = new Simulation(42);
var chunkJson = SaveSystem.SaveZone(chunkSim.World);
var chunkSim2 = new Simulation(43);
Check(chunkSim2.LoadFromJson(chunkJson), "load from json round-trip succeeds");
Check(chunkSim.World.Chunk.ToBytes().SequenceEqual(chunkSim2.World.Chunk.ToBytes()), "load from json round-trip keeps the chunk");

// ---- Slice 2 follow-up: doctrine menu / rally picker (0.15.0) ----
// Defaults: the only current doctrine/kind pair, no pick in progress.
var pickSim = new Simulation(42);
Eq(TacticalDoctrine.HoldGround, pickSim.OrbitalCommand.Doctrine, "orbital picker defaults to HoldGround");
Eq(SquadKind.Vanguard, pickSim.OrbitalCommand.Kind, "orbital picker defaults to Vanguard");
Check(!pickSim.OrbitalCommand.IsPicking, "orbital picker starts idle");
Check(!pickSim.OrbitalCommand.HasRally, "orbital picker starts with no rally");
// Confirm with no pick deploys nothing and changes nothing.
Check(pickSim.ConfirmOrbitalDeploy() == null, "orbital picker confirm with no pick deploys nothing");
Eq(0, pickSim.World.Squads.Count, "orbital picker confirm with no pick adds no squad");
// SetRally outside picking fails closed.
Check(!pickSim.SetOrbitalRally(20, 5, 16), "orbital picker rally outside picking rejected");
Check(!pickSim.OrbitalCommand.HasRally, "orbital picker rally outside picking leaves no rally");
// Pick, rally, confirm: one squad, selected doctrine/kind, at the rally.
// Since 0.18.0 the picker confirm is paid, so this confirm is funded
// with 10 EmberOre first; the unfunded-confirm refusal is tested in
// the Slice 3 Step 2 section below.
pickSim.World.Player.Inventory.Add(BlockType.EmberOre, 10);
pickSim.BeginOrbitalPick();
Check(pickSim.OrbitalCommand.IsPicking, "orbital picker begin opens picking");
Check(pickSim.SetOrbitalRally(20, 5, 16), "orbital picker rally while picking accepted");
Check(pickSim.OrbitalCommand.HasRally, "orbital picker has rally after set");
var picked = pickSim.ConfirmOrbitalDeploy();
Check(picked != null, "orbital picker confirm deploys a squad");
Eq(1, pickSim.World.Squads.Count, "orbital picker confirm adds exactly one squad");
Eq(20, picked!.RallyX, "orbital picker squad rally X");
Eq(TacticalDoctrine.HoldGround, picked.Doctrine, "orbital picker squad doctrine");
Eq(SquadKind.Vanguard, picked.Kind, "orbital picker squad kind");
Check(!pickSim.OrbitalCommand.IsPicking, "orbital picker closes after confirm");
Check(!pickSim.OrbitalCommand.HasRally, "orbital picker clears rally after confirm");
// A second confirm without a new pick deploys nothing.
Check(pickSim.ConfirmOrbitalDeploy() == null, "orbital picker second confirm without pick deploys nothing");
Eq(1, pickSim.World.Squads.Count, "orbital picker second confirm adds no squad");
// Out-of-bounds rally rejected; the picker stays open for a re-pick.
var oobSim = new Simulation(42);
oobSim.BeginOrbitalPick();
Check(!oobSim.SetOrbitalRally(99, 5, 16), "orbital picker out-of-bounds rally rejected");
Check(!oobSim.OrbitalCommand.HasRally, "orbital picker rejected rally leaves no rally");
Check(oobSim.ConfirmOrbitalDeploy() == null, "orbital picker confirm with no rally deploys nothing");
Check(oobSim.OrbitalCommand.IsPicking, "orbital picker stays open after failed confirm");
Eq(0, oobSim.World.Squads.Count, "orbital picker failed confirm adds no squad");
// Re-picking a rally overwrites the earlier pending point.
Check(oobSim.SetOrbitalRally(18, 5, 16), "orbital picker first rally accepted");
Check(oobSim.SetOrbitalRally(22, 5, 16), "orbital picker re-pick accepted");
oobSim.World.Player.Inventory.Add(BlockType.EmberOre, 10); // picker confirm is paid since 0.18.0
var repicked = oobSim.ConfirmOrbitalDeploy();
Eq(22, repicked!.RallyX, "orbital picker confirm uses the latest rally");
// BeginPick clears a stale pending rally, so Enter cannot deploy it.
var staleSim = new Simulation(42);
staleSim.BeginOrbitalPick();
Check(staleSim.SetOrbitalRally(20, 5, 16), "orbital picker stale rally set");
staleSim.BeginOrbitalPick();
Check(!staleSim.OrbitalCommand.HasRally, "orbital picker begin clears stale rally");
Check(staleSim.ConfirmOrbitalDeploy() == null, "orbital picker confirm after stale clear deploys nothing");
// Cancel abandons the pick with no squad.
var cancelSim = new Simulation(42);
cancelSim.BeginOrbitalPick();
Check(cancelSim.SetOrbitalRally(20, 5, 16), "orbital picker cancel-test rally set");
cancelSim.CancelOrbitalPick();
Check(!cancelSim.OrbitalCommand.IsPicking, "orbital picker cancel closes picking");
Check(!cancelSim.OrbitalCommand.HasRally, "orbital picker cancel drops rally");
Check(cancelSim.ConfirmOrbitalDeploy() == null, "orbital picker confirm after cancel deploys nothing");
Eq(0, cancelSim.World.Squads.Count, "orbital picker cancel adds no squad");
// Unknown doctrine/kind selections are rejected, selection unchanged.
var selSim = new Simulation(42);
Check(!selSim.OrbitalCommand.SelectDoctrine((TacticalDoctrine)99), "orbital picker unknown doctrine rejected");
Eq(TacticalDoctrine.HoldGround, selSim.OrbitalCommand.Doctrine, "orbital picker doctrine unchanged after rejection");
Check(!selSim.OrbitalCommand.SelectKind((SquadKind)99), "orbital picker unknown kind rejected");
Eq(SquadKind.Vanguard, selSim.OrbitalCommand.Kind, "orbital picker kind unchanged after rejection");
Check(selSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround), "orbital picker valid doctrine accepted");
Check(selSim.OrbitalCommand.SelectKind(SquadKind.Vanguard), "orbital picker valid kind accepted");
// A picked squad actually marches to its rally on later ticks.
var marchSim = new Simulation(42);
marchSim.World.Player.Inventory.Add(BlockType.EmberOre, 10); // picker confirm is paid since 0.18.0
marchSim.BeginOrbitalPick();
Check(marchSim.SetOrbitalRally(20, 5, 16), "orbital picker march rally set");
var marchSquad = marchSim.ConfirmOrbitalDeploy();
Eq(16, marchSquad!.X, "orbital picker squad spawns at player X");
marchSim.World.Advance();
Eq(17, marchSquad.X, "orbital picker squad marches toward rally on tick");

// ---- Slice 3 Step 1: save-format completion (0.17.0) ----
// Core health survives a save/load round-trip, including destroyed.
var coreWorld = new WorldZone(42);
coreWorld.DamageCore(40);
Eq(60, SaveSystem.LoadZone(SaveSystem.SaveZone(coreWorld)).CoreHealth, "save/load core health survives");
var deadCoreWorld = new WorldZone(42);
deadCoreWorld.DamageCore(100);
Eq(0, SaveSystem.LoadZone(SaveSystem.SaveZone(deadCoreWorld)).CoreHealth, "save/load destroyed core stays destroyed, not healed");
// An old save (no CoreHealth field) still loads, with a fresh 100 core.
var coreJson = SaveSystem.SaveZone(coreWorld);
var coreOldJson = coreJson[..coreJson.LastIndexOf(",\"CoreHealth\":", StringComparison.Ordinal)] + "}";
Eq(100, SaveSystem.LoadZone(coreOldJson).CoreHealth, "old save without core health loads fresh 100 core");
// Out-of-range core values in a save are clamped, never fatal.
Eq(100, SaveSystem.LoadZone(coreJson.Replace("\"CoreHealth\":60", "\"CoreHealth\":999")).CoreHealth, "save/load core health above 100 clamps to 100");
Eq(0, SaveSystem.LoadZone(coreJson.Replace("\"CoreHealth\":60", "\"CoreHealth\":-20")).CoreHealth, "save/load core health below 0 clamps to 0");
// World-level skill effects restore through the SaveZone overload:
// the shield cap and workstation regen arrive with the world, so a
// skilled save's shields are not clamped to the base cap on load.
var worldSkillBuild = new SkillBuild();
Check(worldSkillBuild.Allocate("S1") && worldSkillBuild.Allocate("S2") && worldSkillBuild.Allocate("N1") && worldSkillBuild.Allocate("S3") && worldSkillBuild.Allocate("N2") && worldSkillBuild.Allocate("K1"), "save-format test full cluster allocates");
var worldSkillLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(new WorldZone(42), worldSkillBuild, null));
Eq(105, worldSkillLoaded.Player.MaxShields, "save/load world-level skill shield cap restored");
Eq(10, worldSkillLoaded.WorkstationRegenPerTick, "save/load world-level skill workstation regen restored");
// Full Simulation round-trip: loot weapon (one shot fired, so the
// mag is 29 not 30), full skill build, shields above the base cap,
// and a damaged core all survive into a fresh Simulation.
var saveLoot = LootGenerator.Generate(0, 1);
var saveBuild = new SkillBuild();
Check(saveBuild.Allocate("S1") && saveBuild.Allocate("S2") && saveBuild.Allocate("N1") && saveBuild.Allocate("S3") && saveBuild.Allocate("N2") && saveBuild.Allocate("K1"), "save-format simulation full cluster allocates");
var saveSim = new Simulation(42, saveLoot, saveBuild);
saveSim.World.Player.Restore(100, 100);
saveSim.World.DamageCore(35);
Check(saveSim.Weapon.Fire(), "save-format test fires one shot before saving");
var fullJson = saveSim.SaveToJson();
var loadSim = new Simulation(99);
Check(loadSim.LoadFromJson(fullJson), "simulation save/load round-trip succeeds");
Eq("Titan Long Pulse Rifle", loadSim.Weapon.Name, "save/load weapon name survives");
Eq(20, loadSim.Weapon.Damage, "save/load weapon damage survives");
Eq(30, loadSim.Weapon.MagCapacity, "save/load weapon mag capacity survives");
Eq(29, loadSim.Weapon.Mag, "save/load weapon mag survives with no free refill");
Eq(120, loadSim.Weapon.Reserve, "save/load weapon reserve survives");
Eq(24, loadSim.EffectiveDamage, "save/load skill build restores damage");
Eq(105, loadSim.World.Player.MaxShields, "save/load skill build restores shield cap");
Eq(100, loadSim.World.Player.Shields, "save/load shields above base cap survive (cap raised before restore)");
Check(Math.Abs(loadSim.Fps.EffectiveSpeed - 6.3f) < 0.001f, "save/load skill build restores move speed");
Eq(65, loadSim.World.CoreHealth, "save/load core health survives via simulation save");
// An empty build (a respec) persists as an empty build, not as
// "no build": loading it returns to base values, not to the
// previous build.
var respecSim = new Simulation(42, saveLoot, saveBuild);
respecSim.ApplySkills(new SkillBuild());
var respecLoadedSim = new Simulation(99);
Check(respecLoadedSim.LoadFromJson(respecSim.SaveToJson()), "save/load respec round-trip succeeds");
Eq(85, respecLoadedSim.World.Player.MaxShields, "save/load empty build respecs to base cap");
Eq(20, respecLoadedSim.EffectiveDamage, "save/load empty build restores base damage");
Check(respecLoadedSim.AppliedSkills != null && respecLoadedSim.AppliedSkills.Allocated.Count == 0, "save/load empty build is present but empty");
// The default Pulse Rifle persists too, not just loot weapons.
var defaultLoadedSim = new Simulation(99);
Check(defaultLoadedSim.LoadFromJson(new Simulation(42).SaveToJson()), "save/load default weapon round-trip succeeds");
Eq("Pulse Rifle", defaultLoadedSim.Weapon.Name, "save/load default weapon name survives");
Eq(42, defaultLoadedSim.Weapon.MagCapacity, "save/load default weapon mag capacity survives");
// An old save (core, build, and weapon fields stripped) loaded
// into a skilled, loot-equipped Simulation keeps the current
// weapon and build — in-place semantics — while its core, having
// no saved value, loads fresh at 100, as before.
var keepSim = new Simulation(42, saveLoot, saveBuild);
keepSim.World.DamageCore(50);
var oldFullJson = fullJson[..fullJson.LastIndexOf(",\"CoreHealth\":", StringComparison.Ordinal)] + "}";
Check(keepSim.LoadFromJson(oldFullJson), "old save loads into skilled simulation");
Eq("Titan Long Pulse Rifle", keepSim.Weapon.Name, "old save keeps current weapon");
Eq(24, keepSim.EffectiveDamage, "old save keeps current skill build");
Eq(100, keepSim.World.CoreHealth, "old save loads fresh 100 core");
// A malformed skill string in an otherwise valid save is skipped,
// never fatal: the load succeeds, the weapon and core still
// restore, and no build is applied.
int badSkillStart = fullJson.IndexOf("\"SkillBuild\":\"", StringComparison.Ordinal) + "\"SkillBuild\":\"".Length;
int badSkillEnd = fullJson.IndexOf("\"", badSkillStart);
var badSkillJson = fullJson[..badSkillStart] + "!!!not-base64!!!" + fullJson[badSkillEnd..];
var badSkillSim = new Simulation(99);
Check(badSkillSim.LoadFromJson(badSkillJson), "malformed skill string still loads");
Eq("Titan Long Pulse Rifle", badSkillSim.Weapon.Name, "malformed skill string keeps weapon restore");
Check(badSkillSim.AppliedSkills == null, "malformed skill string restores no build");
Eq(65, badSkillSim.World.CoreHealth, "malformed skill string keeps core restore");
// A partial weapon (mag and reserve stripped) is skipped, never
// fatal, while the skill build in the same save still applies.
var partialWeaponJson = fullJson[..fullJson.LastIndexOf(",\"WeaponMag\":", StringComparison.Ordinal)] + "}";
var partialSim = new Simulation(99);
Check(partialSim.LoadFromJson(partialWeaponJson), "partial weapon still loads");
Eq("Pulse Rifle", partialSim.Weapon.Name, "partial weapon keeps current weapon");
// The build still applies to the kept weapon: Pulse Rifle 10 damage
// with the cluster's +20% is 12, which proves the skill restore
// happened even though the weapon restore was skipped.
Eq(12, partialSim.EffectiveDamage, "partial weapon keeps skill restore");

// ---- Slice 3 Step 2: squad economy (cost and cap) ----
// Constants: one Vanguard squad costs 10 EmberOre; at most 3 live
// squads may be fielded at once. Both are first-pass values.
Eq(3, SquadEconomy.MaxConcurrentSquads, "squad economy cap is 3 live squads");
Eq(10, SquadEconomy.EmberOreCost, "squad economy cost is 10 EmberOre");
// Paid deploy with exactly the price: succeeds, charges all 10 ore.
var paidWorld = new WorldZone(42);
paidWorld.Player.Inventory.Add(BlockType.EmberOre, 10);
Check(SquadEconomy.CanDeploy(paidWorld), "squad economy affordable and under cap can deploy");
var paidSquad = paidWorld.TryDeployPaidSquad(16, 5, 16);
Check(paidSquad != null, "squad economy paid deploy succeeds with exactly 10 ore");
Eq(1, paidWorld.Squads.Count, "squad economy paid deploy adds exactly one squad");
Eq(0, paidWorld.Player.Inventory.Get(BlockType.EmberOre), "squad economy paid deploy charges all 10 ore");
Eq(1, SquadEconomy.LiveSquadCount(paidWorld), "squad economy live count is 1 after paid deploy");
// Unaffordable by 1 ore: refused, no squad, no partial charge.
var brokeWorld = new WorldZone(42);
brokeWorld.Player.Inventory.Add(BlockType.EmberOre, 9);
Check(!SquadEconomy.CanDeploy(brokeWorld), "squad economy unaffordable cannot deploy");
Check(brokeWorld.TryDeployPaidSquad(16, 5, 16) == null, "squad economy 9 ore deploy refused");
Eq(0, brokeWorld.Squads.Count, "squad economy refused deploy adds no squad");
Eq(9, brokeWorld.Player.Inventory.Get(BlockType.EmberOre), "squad economy refused deploy charges nothing");
// Zero ore: same fail-closed behaviour.
var emptyWorld = new WorldZone(42);
Check(emptyWorld.TryDeployPaidSquad(16, 5, 16) == null, "squad economy zero ore deploy refused");
Eq(0, emptyWorld.Squads.Count, "squad economy zero ore deploy adds no squad");
// Cap: 3 paid squads field, the 4th is refused even with ore in hand,
// and the refused attempt charges nothing.
var capWorld = new WorldZone(42);
capWorld.Player.Inventory.Add(BlockType.EmberOre, 40);
Check(capWorld.TryDeployPaidSquad(16, 5, 16) != null, "squad economy cap deploy 1 succeeds");
Check(capWorld.TryDeployPaidSquad(17, 5, 16) != null, "squad economy cap deploy 2 succeeds");
Check(capWorld.TryDeployPaidSquad(18, 5, 16) != null, "squad economy cap deploy 3 succeeds");
Eq(3, capWorld.Squads.Count, "squad economy 3 squads fielded");
Eq(10, capWorld.Player.Inventory.Get(BlockType.EmberOre), "squad economy 10 ore left after 3 deploys");
Check(!SquadEconomy.CanDeploy(capWorld), "squad economy at cap cannot deploy");
Check(capWorld.TryDeployPaidSquad(19, 5, 16) == null, "squad economy 4th deploy refused at cap");
Eq(3, capWorld.Squads.Count, "squad economy refused 4th deploy adds no squad");
Eq(10, capWorld.Player.Inventory.Get(BlockType.EmberOre), "squad economy refused 4th deploy charges nothing");
// A dead squad frees its cap slot without waiting for Advance to
// remove it: live count excludes the dead, so a replacement deploys.
var deadCapWorld = new WorldZone(42);
deadCapWorld.Player.Inventory.Add(BlockType.EmberOre, 40);
deadCapWorld.TryDeployPaidSquad(16, 5, 16);
deadCapWorld.TryDeployPaidSquad(17, 5, 16);
var doomed = deadCapWorld.TryDeployPaidSquad(18, 5, 16)!;
doomed.TakeDamage(90);
Check(doomed.IsDead, "squad economy cap test squad is dead");
Eq(2, SquadEconomy.LiveSquadCount(deadCapWorld), "squad economy dead squad does not count toward cap");
Check(deadCapWorld.TryDeployPaidSquad(19, 5, 16) != null, "squad economy replacement deploys after a squad dies");
Eq(3, SquadEconomy.LiveSquadCount(deadCapWorld), "squad economy live count back to 3 after replacement");
// Picker enforces the economy: a funded confirm deploys and closes;
// an unfunded confirm fails closed — picker stays open, no squad,
// no charge — so the player can gather ore and re-confirm.
var econPickSim = new Simulation(42);
econPickSim.World.Player.Inventory.Add(BlockType.EmberOre, 10);
econPickSim.BeginOrbitalPick();
Check(econPickSim.SetOrbitalRally(20, 5, 16), "squad economy picker rally set");
Check(econPickSim.ConfirmOrbitalDeploy() != null, "squad economy picker funded confirm deploys");
Eq(1, econPickSim.World.Squads.Count, "squad economy picker funded confirm adds one squad");
Eq(0, econPickSim.World.Player.Inventory.Get(BlockType.EmberOre), "squad economy picker funded confirm charges 10 ore");
Check(!econPickSim.OrbitalCommand.IsPicking, "squad economy picker closes after funded confirm");
econPickSim.BeginOrbitalPick();
Check(econPickSim.SetOrbitalRally(22, 5, 16), "squad economy picker second rally set");
Check(econPickSim.ConfirmOrbitalDeploy() == null, "squad economy picker unfunded confirm refused");
Eq(1, econPickSim.World.Squads.Count, "squad economy picker refused confirm adds no squad");
Eq(0, econPickSim.World.Player.Inventory.Get(BlockType.EmberOre), "squad economy picker refused confirm charges nothing");
Check(econPickSim.OrbitalCommand.IsPicking, "squad economy picker stays open after refused confirm");
// Picker enforces the cap too: 3 funded picker deploys, then a 4th
// funded confirm is refused and stays open with ore untouched.
var econCapSim = new Simulation(42);
econCapSim.World.Player.Inventory.Add(BlockType.EmberOre, 40);
for (int i = 0; i < 3; i++)
{
    econCapSim.BeginOrbitalPick();
    Check(econCapSim.SetOrbitalRally(20, 5, 16), "squad economy picker cap rally set");
    Check(econCapSim.ConfirmOrbitalDeploy() != null, "squad economy picker cap deploy succeeds");
}
Eq(3, econCapSim.World.Squads.Count, "squad economy picker 3 squads fielded");
econCapSim.BeginOrbitalPick();
Check(econCapSim.SetOrbitalRally(22, 5, 16), "squad economy picker over-cap rally set");
Check(econCapSim.ConfirmOrbitalDeploy() == null, "squad economy picker over-cap confirm refused");
Eq(3, econCapSim.World.Squads.Count, "squad economy picker over-cap confirm adds no squad");
Eq(10, econCapSim.World.Player.Inventory.Get(BlockType.EmberOre), "squad economy picker over-cap confirm charges nothing");
Check(econCapSim.OrbitalCommand.IsPicking, "squad economy picker stays open after over-cap refusal");
// Simulation paid path mirrors the picker: affordable succeeds,
// unaffordable fails closed, and the CanDeploy query tracks both.
var econSim = new Simulation(42);
Check(!econSim.CanDeployOrbitalSquad(), "squad economy simulation broke cannot deploy");
Check(econSim.DeployOrbitalSquadPaid(20, 5, 16) == null, "squad economy simulation paid deploy broke refused");
econSim.World.Player.Inventory.Add(BlockType.EmberOre, 10);
Check(econSim.CanDeployOrbitalSquad(), "squad economy simulation funded can deploy");
Check(econSim.DeployOrbitalSquadPaid(20, 5, 16) != null, "squad economy simulation paid deploy funded succeeds");
Eq(0, econSim.World.Player.Inventory.Get(BlockType.EmberOre), "squad economy simulation paid deploy charges 10 ore");
// The free direct path is deliberately kept for headless test use
// only, through the internal seam (World.SpawnSquad, friend assembly
// only, since Slice 18 Step 2): it still deploys with no ore. This
// is the documented distinction, not a bypass in play — Unity play
// goes through the picker, which is paid (see the picker tests
// above).
var freeSim = new Simulation(42);
Check(freeSim.World.SpawnSquad(20, 5, 16) != null, "squad economy free direct path still deploys without ore (headless only)");
Eq(1, freeSim.World.Squads.Count, "squad economy free direct path adds one squad");
// Paid squads persist through a save/load round-trip like any squad.
var paidSaveWorld = new WorldZone(42);
paidSaveWorld.Player.Inventory.Add(BlockType.EmberOre, 10);
paidSaveWorld.TryDeployPaidSquad(20, 5, 16);
var paidLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(paidSaveWorld));
Eq(1, paidLoaded.Squads.Count, "squad economy paid squad survives save/load");

// ---- Slice 3 Step 3: demo scenario + review (0.19.0) ----
// One Simulation composing Slice 3 end to end, mirroring the demo:
// a damaged core, a skilled build, a loot weapon with an
// in-progress mag, three paid squads, a picker refusal at the
// cap, and a save/load round-trip that keeps all of it.
var s3Loot = LootGenerator.Generate(0, 1);
var s3Build = new SkillBuild();
Check(s3Build.Allocate("S1") && s3Build.Allocate("S2") && s3Build.Allocate("N1") && s3Build.Allocate("S3") && s3Build.Allocate("N2") && s3Build.Allocate("K1"), "slice 3 demo full cluster allocates");
var s3Sim = new Simulation(42, s3Loot, s3Build);
s3Sim.World.Player.Restore(100, 100);
s3Sim.Fps.Teleport(24, 5, 24);
s3Sim.ApplyLook(45f, -10f);
Check(s3Sim.FireAimed() && s3Sim.FireAimed(), "slice 3 demo two aimed core shots fired");
Eq(42, s3Sim.World.CoreHealth, "slice 3 demo core damaged to 42 before save");
Eq(28, s3Sim.Weapon.Mag, "slice 3 demo weapon mag is 28 after two core shots");
s3Sim.World.Player.Inventory.Add(BlockType.EmberOre, 40);
Check(s3Sim.DeployOrbitalSquadPaid(18, 5, 16) != null, "slice 3 demo paid squad 1 deploys");
Check(s3Sim.DeployOrbitalSquadPaid(19, 5, 16) != null, "slice 3 demo paid squad 2 deploys");
Check(s3Sim.DeployOrbitalSquadPaid(20, 5, 16) != null, "slice 3 demo paid squad 3 deploys");
Eq(3, s3Sim.World.Squads.Count, "slice 3 demo 3 paid squads fielded");
Eq(10, s3Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice 3 demo 10 ore left after 3 paid squads");
Eq(90, s3Sim.World.Squads[0].Health, "slice 3 demo squad at full 90 HP before save");
s3Sim.BeginOrbitalPick();
Check(s3Sim.SetOrbitalRally(21, 5, 16), "slice 3 demo picker 4th rally set");
Check(s3Sim.ConfirmOrbitalDeploy() == null, "slice 3 demo picker 4th deploy refused at cap");
Eq(3, s3Sim.World.Squads.Count, "slice 3 demo refused 4th deploy adds no squad");
Eq(10, s3Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice 3 demo refused 4th deploy charges nothing");
Check(s3Sim.OrbitalCommand.IsPicking, "slice 3 demo picker stays open after cap refusal");
var s3Json = s3Sim.SaveToJson();
var s3Loaded = new Simulation(99);
Check(s3Loaded.LoadFromJson(s3Json), "slice 3 demo save/load round-trip succeeds");
Eq(42, s3Loaded.World.CoreHealth, "slice 3 demo core health survives round-trip");
Eq("Titan Long Pulse Rifle", s3Loaded.Weapon.Name, "slice 3 demo weapon name survives round-trip");
Eq(28, s3Loaded.Weapon.Mag, "slice 3 demo weapon mag survives round-trip");
Eq(120, s3Loaded.Weapon.Reserve, "slice 3 demo weapon reserve survives round-trip");
Eq(24, s3Loaded.EffectiveDamage, "slice 3 demo skill build survives round-trip");
Eq(105, s3Loaded.World.Player.MaxShields, "slice 3 demo shield cap survives round-trip");
Eq(100, s3Loaded.World.Player.Shields, "slice 3 demo shields above base cap survive round-trip");
Eq(3, s3Loaded.World.Squads.Count, "slice 3 demo 3 squads survive round-trip");
Eq(90, s3Loaded.World.Squads[0].Health, "slice 3 demo squad health survives round-trip");
Eq(10, s3Loaded.World.Player.Inventory.Get(BlockType.EmberOre), "slice 3 demo leftover ore survives round-trip");
Check(!s3Loaded.CanDeployOrbitalSquad(), "slice 3 demo loaded game still at cap, cannot deploy more");
var s3Snap = OrbitalView.Snapshot(s3Loaded.World);
Eq(3, s3Snap.Count, "slice 3 demo snapshot shows 3 loaded squads");
Eq(3, s3Snap[0].AliveMembers, "slice 3 demo loaded squad has 3 living members");

// ---- Slice 4 Step 1: enemy AI targeting squads ----
// Targeting rule (decided here, rationale in DECISIONS.md):
// nearest live target by Manhattan distance, player wins ties,
// squads in deploy order win squad-squad ties, moving and holding
// squads alike, dead squads never targeted, re-derived every tick.
// Damage reconciliation: squad damage stays ONLY in the squad
// phase (10 per surviving adjacent enemy, after squad fire) —
// the enemy phase adds no squad damage, so a pursued squad is
// never counted twice for the same hit.
// Enemy pursues the nearer squad: player far away, squad holding
// at its spawn rally, enemy 4 away. The squad is weakened to 1
// member so it deals 10 (not 30) and the enemy survives to show
// both the move and the fire.
var pursueWorld = new WorldZone(42);
var pursueSquad = pursueWorld.SpawnSquad(16, 5, 16)!;
pursueSquad.TakeDamage(60); // 90 -> 30, 1 member
pursueWorld.Player.X = 28; pursueWorld.Player.Z = 28;
var pursueEnemy = new Enemy(20, 5, 16);
pursueWorld.Enemies.Add(pursueEnemy);
pursueWorld.Advance();
Eq(19, pursueEnemy.X, "slice4 enemy moves toward nearer squad, not far player");
Eq(20, pursueEnemy.Health, "slice4 1-member squad deals 10 to pursued enemy");
Eq(30, pursueSquad.Health, "slice4 pursued squad unharmed at distance 3");
// Player nearer wins: squad marches west, player stays at spawn,
// enemy starts equidistant at tick start (both at 16) — tie goes
// to the player, so the enemy steps east toward the player.
var nearPlayerWorld = new WorldZone(42);
var nearSquad = nearPlayerWorld.SpawnSquad(10, 5, 16)!;
var nearEnemy = new Enemy(20, 5, 16);
nearPlayerWorld.Enemies.Add(nearEnemy);
nearPlayerWorld.Advance();
Eq(19, nearEnemy.X, "slice4 enemy moves toward nearer player, not farther squad");
Eq(15, nearSquad.X, "slice4 squad still marches west while enemy goes for player");
// Tie, distinct positions: squad holding at 16, player moved to
// 24, enemy at 20 — distance 4 to both. Player wins the tie, so
// the enemy steps east (21), not west toward the squad (19).
var tieWorld = new WorldZone(42);
tieWorld.SpawnSquad(16, 5, 16);
tieWorld.Player.X = 24;
var tieEnemy = new Enemy(20, 5, 16);
tieWorld.Enemies.Add(tieEnemy);
tieWorld.Advance();
Eq(21, tieEnemy.X, "slice4 tie goes to player, enemy moves toward player not squad");
// Dead squad never targeted: dead squad at 16 is nearer than the
// player at 28, but the enemy still steps toward the player.
var deadTargetWorld = new WorldZone(42);
var deadTargetSquad = deadTargetWorld.SpawnSquad(16, 5, 16)!;
deadTargetSquad.TakeDamage(90);
deadTargetWorld.Player.X = 28;
var deadTargetEnemy = new Enemy(20, 5, 16);
deadTargetWorld.Enemies.Add(deadTargetEnemy);
deadTargetWorld.Advance();
Eq(21, deadTargetEnemy.X, "slice4 dead squad never targeted, enemy moves toward player");
Eq(0, deadTargetWorld.Squads.Count, "slice4 dead squad removed on tick");
// Moving squad targeted alike: squad marches east from 16, player
// far away, enemy at 22 follows the squad's position tick by tick.
var movingTargetWorld = new WorldZone(42);
var movingTargetSquad = movingTargetWorld.SpawnSquad(20, 5, 16)!;
movingTargetWorld.Player.X = 28; movingTargetWorld.Player.Z = 28;
var movingTargetEnemy = new Enemy(22, 5, 16);
movingTargetWorld.Enemies.Add(movingTargetEnemy);
movingTargetWorld.Advance();
Eq(21, movingTargetEnemy.X, "slice4 enemy follows moving squad tick 1");
Eq(17, movingTargetSquad.X, "slice4 moving squad marches while pursued tick 1");
movingTargetWorld.Advance();
Eq(20, movingTargetEnemy.X, "slice4 enemy follows moving squad tick 2");
Eq(18, movingTargetSquad.X, "slice4 moving squad marches while pursued tick 2");
// No double-count, non-killing case: 1-member squad adjacent to
// an enemy, player far. Squad deals 10, surviving enemy deals 10
// back in the squad phase — squad 30 -> 20, not 10 (double) and
// not 30 (enemy phase hitting first would still leave 20 here,
// so the killing case below is the sharper proof).
var onceWorld = new WorldZone(42);
var onceSquad = onceWorld.SpawnSquad(16, 5, 16)!;
onceSquad.TakeDamage(60); // 90 -> 30, 1 member
onceWorld.Player.X = 28; onceWorld.Player.Z = 28;
var onceEnemy = new Enemy(17, 5, 16);
onceWorld.Enemies.Add(onceEnemy);
onceWorld.Advance();
Eq(20, onceEnemy.Health, "slice4 adjacent squad deals 10, enemy survives");
Eq(20, onceSquad.Health, "slice4 squad takes adjacent damage exactly once");
// No double-count, killing case: full squad adjacent to an enemy,
// player far. If the enemy phase hit the squad before the squad
// fired, the squad would be 80 even though it killed its attacker;
// it stays 90, proving the enemy phase adds no squad damage.
var killTargetWorld = new WorldZone(42);
var killTargetSquad = killTargetWorld.SpawnSquad(16, 5, 16)!;
killTargetWorld.Player.X = 28; killTargetWorld.Player.Z = 28;
var killTargetEnemy = new Enemy(17, 5, 16);
killTargetWorld.Enemies.Add(killTargetEnemy);
killTargetWorld.Advance();
Eq(0, killTargetWorld.Enemies.Count, "slice4 full squad kills adjacent pursued enemy");
Eq(90, killTargetSquad.Health, "slice4 squad unharmed when it kills its adjacent attacker");
// Retargeting, derived each tick: enemy pursues the squad on tick
// 1, the squad is then killed outright, and on tick 2 the enemy
// turns toward the player (largest gap is Z, so it steps in Z).
var retargetWorld = new WorldZone(42);
var retargetSquad = retargetWorld.SpawnSquad(16, 5, 16)!;
retargetSquad.TakeDamage(60); // 1 member, so tick 1 does not kill the enemy
retargetWorld.Player.X = 28; retargetWorld.Player.Z = 28;
var retargetEnemy = new Enemy(20, 5, 16);
retargetWorld.Enemies.Add(retargetEnemy);
retargetWorld.Advance();
Eq(19, retargetEnemy.X, "slice4 retarget setup enemy pursues squad first");
retargetSquad.TakeDamage(90); // killed outright between ticks
retargetWorld.Advance();
Eq(17, retargetEnemy.Z, "slice4 enemy retargets player after squad dies");
Eq(19, retargetEnemy.X, "slice4 retargeted enemy does not also step in X that tick");
Eq(0, retargetWorld.Squads.Count, "slice4 retargeted dead squad removed");
// Movement generalisation, direct: one rule for any target —
// largest-gap axis, X tie-break, adjacent stop, solid block, and
// an occupied cell all refuse the step.
var genWorld = new WorldZone(42);
var genEnemy = new Enemy(16, 5, 20);
genEnemy.TickTowardPosition(20, 5, 20, genWorld.Chunk);
Eq(17, genEnemy.X, "slice4 generalised movement steps in X on diagonal target");
Eq(20, genEnemy.Z, "slice4 generalised movement does not also step in Z");
var genTie = new Enemy(16, 5, 16);
genTie.TickTowardPosition(20, 5, 20, genWorld.Chunk);
Eq(17, genTie.X, "slice4 generalised movement tie goes to X first");
Eq(16, genTie.Z, "slice4 generalised movement tie does not step in Z");
var genAdjacent = new Enemy(16, 5, 17);
genAdjacent.TickTowardPosition(16, 5, 16, genWorld.Chunk);
Eq(17, genAdjacent.Z, "slice4 generalised movement stops when adjacent to target");
var genBlockedWorld = new WorldZone(42);
genBlockedWorld.Chunk.TrySet(17, 5, 16, BlockType.Stone);
var genBlocked = new Enemy(16, 5, 16);
genBlocked.TickTowardPosition(20, 5, 16, genBlockedWorld.Chunk);
Eq(16, genBlocked.X, "slice4 generalised movement blocked by solid block");
var genOccupied = new Enemy(16, 5, 16);
genOccupied.TickTowardPosition(20, 5, 16, genWorld.Chunk, new[] { (17, 5, 16) });
Eq(16, genOccupied.X, "slice4 generalised movement blocked by occupied cell");
// Targeting needs no new save field: after a save/load round-trip
// the enemy still pursues the nearer squad, because the target is
// recomputed from saved positions and health, not read from a field.
var saveTargetWorld = new WorldZone(42);
var saveTargetSquad = saveTargetWorld.SpawnSquad(16, 5, 16)!;
saveTargetSquad.TakeDamage(60);
saveTargetWorld.Player.X = 28; saveTargetWorld.Player.Z = 28;
saveTargetWorld.Enemies.Add(new Enemy(20, 5, 16));
var saveTargetLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(saveTargetWorld));
saveTargetLoaded.Advance();
Eq(19, saveTargetLoaded.Enemies[0].X, "slice4 targeting derived after load, no new save field");
Eq(20, saveTargetLoaded.Enemies[0].Health, "slice4 loaded squad fires at pursued enemy");

// ---- Slice 4 Step 2: second doctrine — Advance (chosen from Advance / Defend Core / Patrol) ----
// Advance is a hunting doctrine, not HoldGround at a moving rally:
// it seeks the nearest living enemy every tick, fires in range
// whether or not it is at its rally, and stands still when no
// enemy lives — it never falls back to marching to its rally.
// Defend Core and Patrol are deferred, not rejected (see DECISIONS).
// Deploy accepts Advance; HoldGround behaviour is unchanged elsewhere.
var advWorld = new WorldZone(42);
var advSquad = advWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.Advance)!;
Eq(TacticalDoctrine.Advance, advSquad.Doctrine, "slice4 advance deploy doctrine is Advance");
Eq(SquadKind.Vanguard, advSquad.Kind, "slice4 advance deploy kind stays Vanguard");
Check(advWorld.SpawnSquad(5, 5, 5, (TacticalDoctrine)99) == null, "slice4 advance unknown doctrine still rejected");
// No living enemy: an Advance squad stands still, it does NOT march to its rally.
var advIdleWorld = new WorldZone(42);
var advIdle = advIdleWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.Advance)!;
for (int i = 0; i < 3; i++) advIdleWorld.Advance();
Eq(16, advIdle.X, "slice4 advance with no enemy does not march to rally X");
Eq(16, advIdle.Z, "slice4 advance with no enemy does not march to rally Z");
// Advance moves toward its enemy, not its rally: rally east, enemy
// west. Weakened to 1 member so its fire (10) does not kill the
// enemy outright and hide the movement. Enemy phase first: the
// enemy is tied with the player at 4, so the player wins the tie
// and the enemy steps east toward the player, then the squad
// steps west toward the enemy and fires.
var advWestWorld = new WorldZone(42);
var advWest = advWestWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.Advance)!;
advWest.TakeDamage(60); // 90 -> 30, 1 member
advWestWorld.Enemies.Add(new Enemy(12, 5, 16));
advWestWorld.Advance();
Eq(15, advWest.X, "slice4 advance moves toward enemy, not rally");
Eq(13, advWestWorld.Enemies[0].X, "slice4 advance enemy steps toward player on tie before squad moves");
Eq(20, advWestWorld.Enemies[0].Health, "slice4 advance weakened squad deals 10 to enemy it moved toward");
// Advance fires while NOT at its rally: adjacent enemy, rally far
// east. The step onto the enemy's cell is refused, so the squad
// stays put and fires — HoldGround would stay silent while moving.
var advFireWorld = new WorldZone(42);
var advFire = advFireWorld.SpawnSquad(24, 5, 16, TacticalDoctrine.Advance)!;
advFire.TakeDamage(60); // 1 member, 10 damage
advFireWorld.Player.X = 28; advFireWorld.Player.Z = 28;
advFireWorld.Enemies.Add(new Enemy(17, 5, 16));
advFireWorld.Advance();
Eq(16, advFire.X, "slice4 advance does not step onto adjacent enemy cell");
Check(!advFire.AtRally, "slice4 advance firing squad is not at its rally");
Eq(20, advFireWorld.Enemies[0].Health, "slice4 advance fires at adjacent enemy despite not at rally");
Eq(20, advFire.Health, "slice4 advance takes single adjacent damage after firing");
// Advance moves, then fires in the same tick on entering range:
// enemy 4 away, player far so the enemy pursues the squad. Enemy
// steps 20 -> 19, squad steps 16 -> 17 (distance 2), fires 10.
var advRangeWorld = new WorldZone(42);
var advRange = advRangeWorld.SpawnSquad(24, 5, 16, TacticalDoctrine.Advance)!;
advRange.TakeDamage(60);
advRangeWorld.Player.X = 28; advRangeWorld.Player.Z = 28;
advRangeWorld.Enemies.Add(new Enemy(20, 5, 16));
advRangeWorld.Advance();
Eq(17, advRange.X, "slice4 advance moves into range in one tick");
Eq(19, advRangeWorld.Enemies[0].X, "slice4 advance enemy pursues advancing squad");
Eq(20, advRangeWorld.Enemies[0].Health, "slice4 advance fires in the same tick it enters range");
// Advance picks the nearest enemy, ties to the first in list order:
// enemies west and east both 4 away at tick start; the player at
// the spawn is tied with both, so both enemies step toward the
// player first, leaving a 3-3 tie the squad breaks west (first).
var advTieWorld = new WorldZone(42);
var advTie = advTieWorld.SpawnSquad(24, 5, 16, TacticalDoctrine.Advance)!;
advTie.TakeDamage(60);
advTieWorld.Enemies.Add(new Enemy(12, 5, 16));
advTieWorld.Enemies.Add(new Enemy(20, 5, 16));
advTieWorld.Advance();
Eq(15, advTie.X, "slice4 advance tie goes to first enemy in list order");
// Advance blocked by a solid block holds its position that tick
// but still fires at the in-range enemy it cannot reach.
var advBlockWorld = new WorldZone(42);
advBlockWorld.Chunk.TrySet(17, 5, 16, BlockType.Stone);
var advBlock = advBlockWorld.SpawnSquad(24, 5, 16, TacticalDoctrine.Advance)!;
advBlock.TakeDamage(60);
advBlockWorld.Enemies.Add(new Enemy(20, 5, 16));
advBlockWorld.Advance();
Eq(16, advBlock.X, "slice4 advance blocked by solid block stays put");
Eq(20, advBlockWorld.Enemies[0].Health, "slice4 advance blocked squad still fires in range");
// Advance stops hunting when its prey dies: full squad kills an
// enemy on tick 1, then on tick 2 it stands still instead of
// marching to the rally it was deployed with.
var advStopWorld = new WorldZone(42);
var advStop = advStopWorld.SpawnSquad(24, 5, 16, TacticalDoctrine.Advance)!;
advStopWorld.Player.X = 28; advStopWorld.Player.Z = 28;
advStopWorld.Enemies.Add(new Enemy(19, 5, 16));
advStopWorld.Advance();
Eq(0, advStopWorld.Enemies.Count, "slice4 advance full squad kills enemy in range");
Eq(17, advStop.X, "slice4 advance position after killing its enemy");
advStopWorld.Advance();
Eq(17, advStop.X, "slice4 advance stands still after prey dies, no rally fallback");
// Advance persists through save/load and keeps hunting afterwards:
// loaded squad doctrine is Advance and it moves toward its enemy,
// not toward the rally stored in the save.
var advSaveWorld = new WorldZone(42);
var advSave = advSaveWorld.SpawnSquad(24, 5, 16, TacticalDoctrine.Advance)!;
advSave.TakeDamage(60);
advSaveWorld.Player.X = 28; advSaveWorld.Player.Z = 28;
advSaveWorld.Enemies.Add(new Enemy(20, 5, 16));
var advSaveLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(advSaveWorld));
Eq(TacticalDoctrine.Advance, advSaveLoaded.Squads[0].Doctrine, "slice4 advance doctrine survives save/load");
Eq(24, advSaveLoaded.Squads[0].RallyX, "slice4 advance rally survives save/load");
advSaveLoaded.Advance();
Eq(17, advSaveLoaded.Squads[0].X, "slice4 advance loaded squad moves toward enemy, not rally");
Eq(20, advSaveLoaded.Enemies[0].Health, "slice4 advance loaded squad fires after moving into range");
// Picker: Advance is selectable, and a picker confirm deploys it
// through the paid path — 10 EmberOre charged, cap unchanged —
// with no new price invented for the doctrine.
var advPickSim = new Simulation(42);
Check(advPickSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance), "slice4 advance picker doctrine selectable");
Eq(TacticalDoctrine.Advance, advPickSim.OrbitalCommand.Doctrine, "slice4 advance picker selection sticks");
advPickSim.World.Player.Inventory.Add(BlockType.EmberOre, 10);
advPickSim.BeginOrbitalPick();
Check(advPickSim.SetOrbitalRally(20, 5, 16), "slice4 advance picker rally set");
var advPicked = advPickSim.ConfirmOrbitalDeploy();
Check(advPicked != null, "slice4 advance picker confirm deploys");
Eq(TacticalDoctrine.Advance, advPicked!.Doctrine, "slice4 advance picker squad doctrine");
Eq(0, advPickSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice4 advance picker deploy charges the settled 10 ore");
Check(!advPickSim.OrbitalCommand.IsPicking, "slice4 advance picker closes after confirm");
// Snapshot carries the new doctrine, so the Unity display can show it.
var advViewWorld = new WorldZone(42);
advViewWorld.SpawnSquad(24, 5, 16, TacticalDoctrine.Advance);
Eq(TacticalDoctrine.Advance, OrbitalView.Snapshot(advViewWorld)[0].Doctrine, "slice4 advance snapshot doctrine");

// ---- Slice 4 Step 3: demo scenario + review (0.23.0) ----
// One Simulation composing Slice 4 end to end, mirroring the demo:
// a HoldGround squad pursued by an enemy, an Advance squad
// hunting in the same world, both deployed through the paid
// picker, both weakened to 1 member so their 10-damage fire does
// not kill an enemy outright and hide the engagement, and the
// player moved far away so every enemy step is a squad pursuit.
// Enemy A starts east, enemy B west; the Advance squad hunts A,
// then turns to B after A dies. A save/load round-trip after
// tick 4 proves both doctrines keep their behaviour from the
// saved positions and health — the loaded Advance squad moves
// WEST toward B, not east toward its stored rally.
var s4Sim = new Simulation(42);
s4Sim.World.Player.Inventory.Add(BlockType.EmberOre, 20);
s4Sim.BeginOrbitalPick();
Check(s4Sim.SetOrbitalRally(20, 5, 16), "slice 4 demo picker HoldGround rally set");
var s4Hold = s4Sim.ConfirmOrbitalDeploy();
Check(s4Hold != null, "slice 4 demo paid HoldGround squad deploys");
Check(s4Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance), "slice 4 demo picker Advance doctrine selectable");
s4Sim.BeginOrbitalPick();
Check(s4Sim.SetOrbitalRally(24, 5, 16), "slice 4 demo picker Advance rally set");
var s4Adv = s4Sim.ConfirmOrbitalDeploy();
Check(s4Adv != null, "slice 4 demo paid Advance squad deploys");
Eq(TacticalDoctrine.HoldGround, s4Hold!.Doctrine, "slice 4 demo HoldGround squad doctrine");
Eq(TacticalDoctrine.Advance, s4Adv!.Doctrine, "slice 4 demo Advance squad doctrine");
Eq(0, s4Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice 4 demo two paid squads cost all 20 ore");
s4Hold.TakeDamage(60); // 90 -> 30, 1 member
s4Adv.TakeDamage(60); // 90 -> 30, 1 member
s4Sim.World.Player.X = 28; s4Sim.World.Player.Z = 28;
s4Sim.World.Enemies.Add(new Enemy(22, 5, 16));
s4Sim.World.Enemies.Add(new Enemy(10, 5, 16));
// Tick 1: both enemies step toward the squads, both squads march
// east, nobody in range yet, nobody hurt.
s4Sim.World.Advance();
Eq(17, s4Hold.X, "slice 4 demo tick 1 HoldGround marches east");
Eq(17, s4Adv.X, "slice 4 demo tick 1 Advance marches east with HoldGround");
Eq(21, s4Sim.World.Enemies[0].X, "slice 4 demo tick 1 east enemy pursues squads");
Eq(11, s4Sim.World.Enemies[1].X, "slice 4 demo tick 1 west enemy pursues squads");
Eq(30, s4Hold.Health, "slice 4 demo tick 1 squads unharmed out of range");
Eq(30, s4Adv.Health, "slice 4 demo tick 1 Advance unharmed out of range");
// Tick 2: Advance moves into range of enemy A and fires 10;
// HoldGround is still marching, so it does not fire.
s4Sim.World.Advance();
Eq(18, s4Hold.X, "slice 4 demo tick 2 HoldGround marches east");
Eq(18, s4Adv.X, "slice 4 demo tick 2 Advance marches east");
Eq(20, s4Sim.World.Enemies[0].Health, "slice 4 demo tick 2 Advance fires 10 at east enemy");
Eq(30, s4Sim.World.Enemies[1].Health, "slice 4 demo tick 2 west enemy untouched");
Eq(30, s4Hold.Health, "slice 4 demo tick 2 squads still unharmed");
// Tick 3: enemy A reaches adjacency; both squads take exactly
// 10 each (single adjacent damage, not double), and Advance
// fires A down to 10 while HoldGround, still moving, stays silent.
s4Sim.World.Advance();
Eq(18, s4Hold.X, "slice 4 demo tick 3 HoldGround blocked by adjacent enemy");
Eq(18, s4Adv.X, "slice 4 demo tick 3 Advance blocked by adjacent enemy");
Eq(10, s4Sim.World.Enemies[0].Health, "slice 4 demo tick 3 Advance fires adjacent enemy to 10");
Eq(20, s4Hold.Health, "slice 4 demo tick 3 HoldGround takes single adjacent damage");
Eq(20, s4Adv.Health, "slice 4 demo tick 3 Advance takes single adjacent damage");
// Tick 4: HoldGround takes a second 10 from the surviving enemy,
// while Advance kills that enemy and takes nothing from it —
// the no-double-count killing case, inside the composed run.
s4Sim.World.Advance();
Eq(10, s4Hold.Health, "slice 4 demo tick 4 HoldGround takes second adjacent damage");
Eq(20, s4Adv.Health, "slice 4 demo tick 4 Advance unharmed when it kills its attacker");
Eq(1, s4Sim.World.Enemies.Count, "slice 4 demo tick 4 east enemy dead, west enemy remains");
Eq(14, s4Sim.World.Enemies[0].X, "slice 4 demo tick 4 west enemy pursued squads");
Eq(30, s4Sim.World.Enemies[0].Health, "slice 4 demo tick 4 west enemy unharmed");
// Save/load after tick 4: positions, health, doctrines, rallies,
// and the spent ore all survive; the loaded game cannot deploy
// more (no ore left), even though it is under the squad cap.
var s4Json = s4Sim.SaveToJson();
var s4Loaded = new Simulation(99);
Check(s4Loaded.LoadFromJson(s4Json), "slice 4 demo save/load round-trip succeeds");
Eq(2, s4Loaded.World.Squads.Count, "slice 4 demo 2 squads survive round-trip");
Eq(TacticalDoctrine.HoldGround, s4Loaded.World.Squads[0].Doctrine, "slice 4 demo HoldGround doctrine survives round-trip");
Eq(TacticalDoctrine.Advance, s4Loaded.World.Squads[1].Doctrine, "slice 4 demo Advance doctrine survives round-trip");
Eq(20, s4Loaded.World.Squads[0].RallyX, "slice 4 demo HoldGround rally survives round-trip");
Eq(24, s4Loaded.World.Squads[1].RallyX, "slice 4 demo Advance rally survives round-trip");
Eq(18, s4Loaded.World.Squads[0].X, "slice 4 demo HoldGround position survives round-trip");
Eq(10, s4Loaded.World.Squads[0].Health, "slice 4 demo HoldGround health survives round-trip");
Eq(20, s4Loaded.World.Squads[1].Health, "slice 4 demo Advance health survives round-trip");
Eq(0, s4Loaded.World.Player.Inventory.Get(BlockType.EmberOre), "slice 4 demo spent ore survives round-trip");
Check(!s4Loaded.CanDeployOrbitalSquad(), "slice 4 demo loaded game cannot deploy without ore");
// Tick 5 (loaded): HoldGround marches on toward its rally, but
// the loaded Advance squad moves WEST toward enemy B and fires
// it — hunting from the saved position, not marching east to
// its stored rally, which is what proves the doctrine survived.
s4Loaded.World.Advance();
Eq(19, s4Loaded.World.Squads[0].X, "slice 4 demo tick 5 loaded HoldGround marches toward rally");
Eq(17, s4Loaded.World.Squads[1].X, "slice 4 demo tick 5 loaded Advance moves west toward enemy, not rally");
Eq(20, s4Loaded.World.Enemies[0].Health, "slice 4 demo tick 5 loaded Advance fires west enemy to 20");
// Tick 6 (loaded): HoldGround reaches its rally; enemy B, now
// nearer the Advance squad than HoldGround, retargets to it and
// steps adjacent; Advance fires it to 10 and takes exactly one
// adjacent 10, while HoldGround at its rally is untouched and
// out of range, so it does not fire.
s4Loaded.World.Advance();
Eq(20, s4Loaded.World.Squads[0].X, "slice 4 demo tick 6 loaded HoldGround reaches rally");
Check(s4Loaded.World.Squads[0].AtRally, "slice 4 demo tick 6 loaded HoldGround is at rally");
Eq(17, s4Loaded.World.Squads[1].X, "slice 4 demo tick 6 loaded Advance holds against adjacent enemy");
Eq(16, s4Loaded.World.Enemies[0].X, "slice 4 demo tick 6 enemy retargets to nearer Advance squad");
Eq(10, s4Loaded.World.Enemies[0].Health, "slice 4 demo tick 6 loaded Advance fires enemy to 10");
Eq(10, s4Loaded.World.Squads[0].Health, "slice 4 demo tick 6 HoldGround at rally unharmed");
Eq(10, s4Loaded.World.Squads[1].Health, "slice 4 demo tick 6 Advance takes single adjacent damage");
var s4LoadedSnap = OrbitalView.Snapshot(s4Loaded.World);
Eq(TacticalDoctrine.Advance, s4LoadedSnap[1].Doctrine, "slice 4 demo snapshot carries Advance doctrine after load");

// ---- Slice 5 Step 1: automation tier 2, first electrical loop ----
// Source, distribution, and powered behaviour are each exactly one
// choice: one Dynamo source, face-adjacency distribution, and one
// powered behaviour (a powered Kinetic Mill runs every tick instead
// of every 5th). Cost: 5 EmberOre only.
var dynWorld = new WorldZone(42);
dynWorld.Player.Inventory.Add(BlockType.EmberOre, 5);
Eq(BuildResult.Success, dynWorld.Build(15, 4, 15, BlockType.Dynamo), "dynamo builds with 5 ember ore");
Eq(BlockType.Dynamo, dynWorld.Chunk.Get(15, 4, 15), "dynamo placed");
Eq(0, dynWorld.Player.Inventory.Get(BlockType.EmberOre), "dynamo consumes 5 ember ore");
Eq(GatherResult.NotGatherable, dynWorld.Gather(15, 4, 15), "dynamo itself is not gatherable");
var poorDynWorld = new WorldZone(42);
poorDynWorld.Player.Inventory.Add(BlockType.EmberOre, 4);
Eq(BuildResult.InsufficientResources, poorDynWorld.Build(15, 4, 15, BlockType.Dynamo), "dynamo without 5 ore fails");
Eq(BlockType.Air, poorDynWorld.Chunk.Get(15, 4, 15), "failed dynamo leaves air");
Eq(4, poorDynWorld.Player.Inventory.Get(BlockType.EmberOre), "failed dynamo does not spend ore");
var floatDynWorld = new WorldZone(42);
floatDynWorld.Player.Inventory.Add(BlockType.EmberOre, 5);
Eq(BuildResult.Floating, floatDynWorld.Build(16, 20, 16, BlockType.Dynamo), "dynamo floating in air fails like other machines");
// Distribution: face-adjacent only, no diagonals, no chaining.
var powWorld = new WorldZone(42);
powWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
powWorld.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
Check(powWorld.IsPowered(16, 20, 16), "mill face-adjacent to dynamo is powered");
var diagWorld = new WorldZone(42);
diagWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
diagWorld.Chunk.TrySet(17, 21, 16, BlockType.Dynamo);
Check(!diagWorld.IsPowered(16, 20, 16), "mill diagonally adjacent to dynamo is not powered");
var chainWorld = new WorldZone(42);
chainWorld.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
chainWorld.Chunk.TrySet(11, 20, 10, BlockType.KineticMill);
chainWorld.Chunk.TrySet(12, 20, 10, BlockType.Dynamo);
Check(!chainWorld.IsPowered(10, 20, 10), "mill two cells from dynamo is not powered through another mill");
Check(chainWorld.IsPowered(11, 20, 10), "mill face-adjacent to dynamo is powered in chain setup");
// Rate proof, controlled pocket: a powered mill with 5 stone
// neighbours excavates one per tick (5 in 5 ticks); the same pocket
// without a Dynamo excavates one per 5 ticks (1 in 5 ticks).
var poweredWorld = new WorldZone(42);
poweredWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
poweredWorld.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
poweredWorld.Chunk.TrySet(15, 20, 16, BlockType.Stone);
poweredWorld.Chunk.TrySet(16, 21, 16, BlockType.Stone);
poweredWorld.Chunk.TrySet(16, 19, 16, BlockType.Stone);
poweredWorld.Chunk.TrySet(16, 20, 17, BlockType.Stone);
poweredWorld.Chunk.TrySet(16, 20, 15, BlockType.Stone);
int powStoneBefore = poweredWorld.Player.Inventory.Get(BlockType.Stone);
for (int i = 0; i < 5; i++) poweredWorld.Advance();
Eq(5, poweredWorld.PoweredBlocksGathered, "powered mill gathers 5 blocks in 5 ticks");
Eq(0, poweredWorld.KineticBlocksGathered, "powered mill counts nothing in kinetic counter (no double activation on tick 5)");
Eq(powStoneBefore + 5, poweredWorld.Player.Inventory.Get(BlockType.Stone), "powered mill output reaches player inventory, no buffer");
Eq(BlockType.Dynamo, poweredWorld.Chunk.Get(17, 20, 16), "powered mill never excavates its dynamo");
var unpoweredWorld = new WorldZone(42);
unpoweredWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
unpoweredWorld.Chunk.TrySet(15, 20, 16, BlockType.Stone);
unpoweredWorld.Chunk.TrySet(16, 21, 16, BlockType.Stone);
unpoweredWorld.Chunk.TrySet(16, 19, 16, BlockType.Stone);
unpoweredWorld.Chunk.TrySet(16, 20, 17, BlockType.Stone);
unpoweredWorld.Chunk.TrySet(16, 20, 15, BlockType.Stone);
for (int i = 0; i < 5; i++) unpoweredWorld.Advance();
Eq(1, unpoweredWorld.KineticBlocksGathered, "unpowered mill gathers 1 block in 5 ticks");
Eq(0, unpoweredWorld.PoweredBlocksGathered, "unpowered mill counts nothing powered");
// Power is derived, not stored: removing the Dynamo drops the mill
// back to kinetic cadence on the next tick.
var lossWorld = new WorldZone(42);
lossWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
lossWorld.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
lossWorld.Chunk.TrySet(15, 20, 16, BlockType.Stone);
lossWorld.Chunk.TrySet(16, 21, 16, BlockType.Stone);
lossWorld.Chunk.TrySet(16, 19, 16, BlockType.Stone);
lossWorld.Chunk.TrySet(16, 20, 17, BlockType.Stone);
lossWorld.Chunk.TrySet(16, 20, 15, BlockType.Stone);
lossWorld.Advance(); lossWorld.Advance();
Eq(2, lossWorld.PoweredBlocksGathered, "powered mill gathers 2 in 2 ticks before dynamo removal");
lossWorld.Chunk.TrySet(17, 20, 16, BlockType.Air);
Check(!lossWorld.IsPowered(16, 20, 16), "mill is unpowered after dynamo removal");
for (int i = 0; i < 3; i++) lossWorld.Advance();
Eq(2, lossWorld.PoweredBlocksGathered, "no powered gathers after dynamo removal");
Eq(1, lossWorld.KineticBlocksGathered, "mill falls back to kinetic cadence, gathering once on tick 5");
// Full inventory: a powered mill consumes nothing, like a kinetic one.
var fullPowWorld = new WorldZone(42);
fullPowWorld.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
fullPowWorld.Chunk.TrySet(11, 20, 10, BlockType.Dynamo);
fullPowWorld.Chunk.TrySet(9, 20, 10, BlockType.Stone);
fullPowWorld.Player.Inventory.Add(BlockType.Stone, Inventory.MaxStack);
for (int i = 0; i < 5; i++) fullPowWorld.Advance();
Eq(BlockType.Stone, fullPowWorld.Chunk.Get(9, 20, 10), "powered mill with full inventory leaves the block");
Eq(0, fullPowWorld.PoweredBlocksGathered, "powered mill with full inventory gathers 0");
// Save/load: the Dynamo block survives as a chunk byte (no new save
// field), and the loaded mill is powered again from its placement.
var savePowWorld = new WorldZone(42);
savePowWorld.Chunk.TrySet(15, 4, 15, BlockType.KineticMill);
savePowWorld.Chunk.TrySet(16, 4, 15, BlockType.Dynamo);
savePowWorld.Chunk.TrySet(14, 4, 15, BlockType.Stone);
var savePowLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(savePowWorld));
Eq(BlockType.KineticMill, savePowLoaded.Chunk.Get(15, 4, 15), "mill survives save/load");
Eq(BlockType.Dynamo, savePowLoaded.Chunk.Get(16, 4, 15), "dynamo survives save/load");
Check(savePowLoaded.IsPowered(15, 4, 15), "loaded mill is powered from placement, no flag needed");
savePowLoaded.Advance();
Eq(1, savePowLoaded.PoweredBlocksGathered, "loaded powered mill gathers on its first tick");
// Simulation helper: a Dynamo builds ahead, front-centre.
var simDyn = new Simulation(42);
simDyn.Tick(0.1f, 0, 0, false);
simDyn.World.Player.Inventory.Add(BlockType.EmberOre, 5);
simDyn.ApplyLook(0f, -45f);
Eq(BuildResult.Success, simDyn.BuildDynamoAhead(), "simulation build dynamo ahead succeeds");
Eq(BlockType.Dynamo, simDyn.World.Chunk.Get(16, 4, 17), "simulation dynamo placed front-centre");
Eq(0, simDyn.World.Player.Inventory.Get(BlockType.EmberOre), "simulation dynamo spends its ore");

// ---- Slice 5 Step 2: demo scenario + review (composition only) ----
// Mirrors the Slice 5 demo scenario step for step: a powered pair
// built through the Simulation builders, a seeded stone pocket, an
// unpowered mill pocket as the kinetic baseline, 5 ticks, a
// save/load round-trip, a freshly seeded pocket in the loaded
// game, and 5 more ticks. No new core system rides along.
var simS5 = new Simulation(42);
simS5.Tick(0.1f, 0, 0, false);
simS5.World.Player.Inventory.Add(BlockType.Stone, 10);
simS5.World.Player.Inventory.Add(BlockType.Wood, 5);
simS5.World.Player.Inventory.Add(BlockType.EmberOre, 5);
simS5.ApplyLook(-18.4f, -53f);
Eq(BuildResult.Success, simS5.BuildKineticMillAhead(), "slice 5 demo mill builds through simulation builder");
simS5.ApplyLook(18.4f, 8f);
Eq(BuildResult.Success, simS5.BuildDynamoAhead(), "slice 5 demo dynamo builds through simulation builder");
Eq(BlockType.KineticMill, simS5.World.Chunk.Get(15, 4, 17), "slice 5 demo mill placed front-left");
Eq(BlockType.Dynamo, simS5.World.Chunk.Get(16, 4, 17), "slice 5 demo dynamo placed front-centre");
Eq(0, simS5.World.Player.Inventory.Get(BlockType.EmberOre), "slice 5 demo dynamo spends its ore");
Check(simS5.World.IsPowered(15, 4, 17), "slice 5 demo builder pair is powered with no move between builds");
foreach (var (x, y, z) in new[] { (14, 4, 17), (15, 5, 17), (15, 3, 17), (15, 4, 16), (15, 4, 18) })
    simS5.World.Chunk.TrySet(x, y, z, BlockType.Stone);
simS5.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
foreach (var (x, y, z) in new[] { (9, 20, 10), (10, 21, 10), (10, 19, 10), (10, 20, 11), (10, 20, 9) })
    simS5.World.Chunk.TrySet(x, y, z, BlockType.Stone);
Check(!simS5.World.IsPowered(10, 20, 10), "slice 5 demo unpowered pocket mill is not powered");
Eq(0, simS5.World.Player.Inventory.Get(BlockType.Stone), "slice 5 demo stone spent on mill, none banked before ticks");
for (int i = 0; i < 5; i++) simS5.World.Advance();
Eq(5, simS5.World.PoweredBlocksGathered, "slice 5 demo powered mill gathers 5 in 5 ticks");
Eq(1, simS5.World.KineticBlocksGathered, "slice 5 demo kinetic mill gathers 1 in 5 ticks alongside");
Eq(6, simS5.World.Player.Inventory.Get(BlockType.Stone), "slice 5 demo powered + kinetic output both reach player inventory");
Eq(BlockType.Dynamo, simS5.World.Chunk.Get(16, 4, 17), "slice 5 demo powered mill never excavates its dynamo");
var saveS5 = simS5.SaveToJson();
var simS5Loaded = new Simulation(99);
Check(simS5Loaded.LoadFromJson(saveS5), "slice 5 demo save loads");
Eq(BlockType.KineticMill, simS5Loaded.World.Chunk.Get(15, 4, 17), "slice 5 demo mill survives save/load");
Eq(BlockType.Dynamo, simS5Loaded.World.Chunk.Get(16, 4, 17), "slice 5 demo dynamo survives save/load");
Check(simS5Loaded.World.IsPowered(15, 4, 17), "slice 5 demo loaded mill is powered from placement, no flag needed");
Eq(0, simS5Loaded.World.PoweredBlocksGathered, "slice 5 demo session powered counter resets on load");
Eq(0, simS5Loaded.World.KineticBlocksGathered, "slice 5 demo session kinetic counter resets on load");
Eq(6, simS5Loaded.World.Player.Inventory.Get(BlockType.Stone), "slice 5 demo gathered stone survives save/load");
foreach (var (x, y, z) in new[] { (14, 4, 17), (15, 5, 17), (15, 3, 17), (15, 4, 16), (15, 4, 18) })
    simS5Loaded.World.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) simS5Loaded.World.Advance();
Eq(5, simS5Loaded.World.PoweredBlocksGathered, "slice 5 demo loaded powered mill gathers 5 in 5 ticks");
Eq(1, simS5Loaded.World.KineticBlocksGathered, "slice 5 demo loaded kinetic mill gathers 1 in 5 ticks alongside");
Eq(12, simS5Loaded.World.Player.Inventory.Get(BlockType.Stone), "slice 5 demo loaded stone total is pre-load 6 plus loaded 6");
Eq(BlockType.Dynamo, simS5Loaded.World.Chunk.Get(16, 4, 17), "slice 5 demo loaded powered mill never excavates its dynamo");

// ---- Slice 6 Step 1: FPS pitch aiming (core Look) ----
// Decides the scope's open questions in core: pitch lives in a
// testable core Look (yaw + clamped pitch + forward vector), the
// adapter feeds it scaled mouse deltas and renders from it, and
// camera + picker are the only consumers in this step — gather,
// build, fire, and movement keep their Slice 1 rules.
var look0 = new Look();
Eq(0f, look0.Yaw, "look starts at yaw 0");
Eq(0f, look0.Pitch, "look starts at pitch 0");
var f0 = look0.Forward();
Check(Math.Abs(f0.X) < 0.001f && Math.Abs(f0.Y) < 0.001f && Math.Abs(f0.Z - 1f) < 0.001f, $"look forward at 0,0 is +Z (got {f0.X},{f0.Y},{f0.Z})");
var lookYaw = new Look();
lookYaw.Add(90f, 0f);
var fYaw = lookYaw.Forward();
Check(Math.Abs(fYaw.X - 1f) < 0.001f && Math.Abs(fYaw.Y) < 0.001f && Math.Abs(fYaw.Z) < 0.001f, $"look yaw 90 faces +X (got {fYaw.X},{fYaw.Y},{fYaw.Z})");
var lookBack = new Look();
lookBack.Add(180f, 0f);
var fBack = lookBack.Forward();
Check(Math.Abs(fBack.X) < 0.001f && Math.Abs(fBack.Y) < 0.001f && Math.Abs(fBack.Z + 1f) < 0.001f, $"look yaw 180 faces -Z (got {fBack.X},{fBack.Y},{fBack.Z})");
var lookLeft = new Look();
lookLeft.Add(-90f, 0f);
var fLeft = lookLeft.Forward();
Check(Math.Abs(fLeft.X + 1f) < 0.001f && Math.Abs(fLeft.Z) < 0.001f, $"look yaw -90 faces -X (got {fLeft.X},{fLeft.Y},{fLeft.Z})");
var lookUp = new Look();
lookUp.Add(0f, 45f);
var fUp = lookUp.Forward();
Check(Math.Abs(fUp.X) < 0.001f && Math.Abs(fUp.Y - 0.7071f) < 0.001f && Math.Abs(fUp.Z - 0.7071f) < 0.001f, $"look pitch 45 lifts forward equally up and ahead (got {fUp.X},{fUp.Y},{fUp.Z})");
var lookDown = new Look();
lookDown.Add(0f, -45f);
var fDown = lookDown.Forward();
Check(Math.Abs(fDown.Y + 0.7071f) < 0.001f && Math.Abs(fDown.Z - 0.7071f) < 0.001f, $"look pitch -45 drops forward equally down and ahead (got {fDown.X},{fDown.Y},{fDown.Z})");
var lookCombo = new Look();
lookCombo.Add(90f, 45f);
var fCombo = lookCombo.Forward();
Check(Math.Abs(fCombo.X - 0.7071f) < 0.001f && Math.Abs(fCombo.Y - 0.7071f) < 0.001f && Math.Abs(fCombo.Z) < 0.001f, $"look yaw 90 pitch 45 faces +X and up, no +Z (got {fCombo.X},{fCombo.Y},{fCombo.Z})");
var lookClampUp = new Look();
lookClampUp.Add(0f, 200f);
Eq(89f, lookClampUp.Pitch, "look pitch clamps at +89, never past vertical");
Check(lookClampUp.ForwardY > 0.999f && Math.Abs(lookClampUp.ForwardZ) < 0.02f, $"look clamped-up forward is almost straight up (got Y {lookClampUp.ForwardY}, Z {lookClampUp.ForwardZ})");
var lookClampDown = new Look();
lookClampDown.Add(0f, -200f);
Eq(-89f, lookClampDown.Pitch, "look pitch clamps at -89, never past vertical");
var lookClampStep = new Look();
lookClampStep.Add(0f, 80f);
lookClampStep.Add(0f, 20f);
Eq(89f, lookClampStep.Pitch, "look pitch clamps in steps too, 80 + 20 stops at 89 not 100");
var lookWrap = new Look();
lookWrap.Add(360f, 0f);
Eq(360f, lookWrap.Yaw, "look yaw accumulates unbounded, no hidden normalisation");
var fWrap = lookWrap.Forward();
Check(Math.Abs(fWrap.X) < 0.001f && Math.Abs(fWrap.Z - 1f) < 0.001f, $"look yaw 360 faces +Z again (got {fWrap.X},{fWrap.Y},{fWrap.Z})");
var lookUnit = new Look();
lookUnit.Add(30f, -20f);
var fUnit = lookUnit.Forward();
double unitLen = Math.Sqrt(fUnit.X * fUnit.X + fUnit.Y * fUnit.Y + fUnit.Z * fUnit.Z);
Check(Math.Abs(unitLen - 1.0) < 0.001, $"look forward vector is unit length (got {unitLen})");
// Simulation owns one Look; the adapter feeds it, it holds no yaw of its own.
var simLook = new Simulation(42);
Eq(0f, simLook.Look.Yaw, "simulation look starts at yaw 0");
Eq(0f, simLook.Look.Pitch, "simulation look starts at pitch 0");
simLook.ApplyLook(90f, 45f);
Eq(90f, simLook.Look.Yaw, "simulation apply look sets yaw");
Eq(45f, simLook.Look.Pitch, "simulation apply look sets pitch");
Check(Math.Abs(simLook.Look.ForwardX - 0.7071f) < 0.001f, $"simulation look forward X follows yaw+pitch (got {simLook.Look.ForwardX})");
// Look is session state, not saved: a load into a fresh Simulation
// starts looking forward, whatever the saving Simulation looked at.
var simLookSave = new Simulation(42);
simLookSave.ApplyLook(30f, -20f);
var lookSaveJson = simLookSave.SaveToJson();
var simLookLoaded = new Simulation(99);
Check(simLookLoaded.LoadFromJson(lookSaveJson), "look save/load round-trip succeeds");
Eq(0f, simLookLoaded.Look.Yaw, "look yaw is not saved: loaded simulation starts at yaw 0");
Eq(0f, simLookLoaded.Look.Pitch, "look pitch is not saved: loaded simulation starts at pitch 0");
// …and an in-place restore keeps the current session's view, so a
// Unity load into the same Simulation does not snap the camera.
var simLookRestore = new Simulation(42);
simLookRestore.ApplyLook(30f, -20f);
Check(simLookRestore.RestoreWorld(new WorldZone(9)), "look restore world succeeds");
Eq(30f, simLookRestore.Look.Yaw, "look yaw survives in-place restore");
Eq(-20f, simLookRestore.Look.Pitch, "look pitch survives in-place restore");
// Slice 8 Step 2 replaces this 0.28.0 non-change test, not by
// accident: pitched up 45 degrees, gather / build / fire now
// follow the aim ray into the sky, where there is nothing to
// gather, no surface to build on, and no enemy in the line.
// Keeping the old test would have asserted the aim-blind bug
// Step 2 closes. Movement is NOT part of that list any more
// either: Slice 8 Step 1 rotates it by yaw (see the Slice 8
// section at the end of this file).
var simPitch = new Simulation(42);
simPitch.Tick(0.1f, 0, 0, false);
simPitch.ApplyLook(0f, 45f);
Check(simPitch.GatherAhead() == GatherResult.Air, "pitched up 45 gather aims at sky, nothing gathered");
simPitch.World.Player.Inventory.Add(BlockType.Stone, 10);
Check(simPitch.BuildWallAhead() == BuildResult.Floating, "pitched up 45 build has no aimed surface");
Eq(10, simPitch.World.Player.Inventory.Get(BlockType.Stone), "pitched up 45 failed build spends no stone");
simPitch.World.SpawnWave(1);
for (int i = 0; i < 3; i++) simPitch.FireAimed();
Eq(0, simPitch.EnemiesKilled, "pitched up 45 aimed fire misses the unaimed enemy");
Eq(39, simPitch.Weapon.Mag, "pitched up 45 missed shots still consume ammo");

// ---- Slice 6 Step 2: squad activity word + hunting target (0.29.0) ----
// HoldGround reads by its rally: marching while away, holding at it, no prey ever.
var actMarchWorld = new WorldZone(42);
actMarchWorld.SpawnSquad(20, 5, 16);
var actMarch = OrbitalView.Snapshot(actMarchWorld)[0];
Eq(SquadActivity.Marching, actMarch.Activity, "slice6 HoldGround marching squad reads MARCHING");
Check(!actMarch.HasHuntingTarget, "slice6 marching squad has no hunting target");
Check(actMarch.HuntingTargetX == null, "slice6 marching squad hunting target cell is null");
var actHoldWorld = new WorldZone(42);
actHoldWorld.SpawnSquad(16, 5, 16);
var actHold = OrbitalView.Snapshot(actHoldWorld)[0];
Eq(SquadActivity.Holding, actHold.Activity, "slice6 HoldGround squad at rally reads HOLDING");
// Advance with a living enemy reads HUNTING even beyond engagement
// range (4 > 3): hunting is not firing, and the old rally-relative
// word would have called this same squad MOVING toward a rally it
// ignores. The target is the prey cell, not the rally.
var actHuntWorld = new WorldZone(42);
actHuntWorld.SpawnSquad(24, 5, 16, TacticalDoctrine.Advance);
actHuntWorld.Enemies.Add(new Enemy(20, 5, 16));
var actHunt = OrbitalView.Snapshot(actHuntWorld)[0];
Eq(SquadActivity.Hunting, actHunt.Activity, "slice6 Advance squad beyond range reads HUNTING not MOVING");
Check(actHunt.HasHuntingTarget, "slice6 hunting squad has a hunting target");
Eq(20, actHunt.HuntingTargetX, "slice6 hunting target X is the prey cell");
Eq(5, actHunt.HuntingTargetY, "slice6 hunting target Y is the prey cell");
Eq(16, actHunt.HuntingTargetZ, "slice6 hunting target Z is the prey cell");
// Advance adjacent to its prey also reads HUNTING: the old word
// would have read HOLDING (at its rally) or MOVING (away from it)
// for a squad that is, in fact, standing and firing.
var actAdjWorld = new WorldZone(42);
actAdjWorld.SpawnSquad(16, 5, 16, TacticalDoctrine.Advance);
actAdjWorld.Enemies.Add(new Enemy(17, 5, 16));
Eq(SquadActivity.Hunting, OrbitalView.Snapshot(actAdjWorld)[0].Activity, "slice6 Advance adjacent to prey reads HUNTING");
// Advance with no living enemy reads IDLE and stands still —
// documented behaviour, not a bug to hide — and a dead enemy
// alone does not make it hunt.
var actIdleWorld = new WorldZone(42);
actIdleWorld.SpawnSquad(24, 5, 16, TacticalDoctrine.Advance);
Eq(SquadActivity.Idle, OrbitalView.Snapshot(actIdleWorld)[0].Activity, "slice6 Advance with no enemy reads IDLE");
var actDeadWorld = new WorldZone(42);
actDeadWorld.SpawnSquad(24, 5, 16, TacticalDoctrine.Advance);
var actDeadEnemy = new Enemy(20, 5, 16);
actDeadWorld.Enemies.Add(actDeadEnemy);
actDeadWorld.AttackEnemy(actDeadEnemy, 30);
Eq(SquadActivity.Idle, OrbitalView.Snapshot(actDeadWorld)[0].Activity, "slice6 Advance with only a dead enemy reads IDLE");
// Hunting target ties go to the first enemy in list order, the
// same rule movement and fire use, so display cannot disagree
// with either about the prey.
var actTieWorld = new WorldZone(42);
actTieWorld.SpawnSquad(24, 5, 16, TacticalDoctrine.Advance);
actTieWorld.Enemies.Add(new Enemy(12, 5, 16));
actTieWorld.Enemies.Add(new Enemy(20, 5, 16));
var actTie = OrbitalView.Snapshot(actTieWorld)[0];
Eq(12, actTie.HuntingTargetX, "slice6 hunting tie goes to first enemy in list order");
// The target is re-derived on every snapshot, not stored: kill
// the first-listed prey (both prey are 4 away, so the first
// wins the tie) and the next snapshot hunts the other one,
// while the old snapshot still shows the old prey (copy, as
// with position since 0.11.0).
var actReWorld = new WorldZone(42);
actReWorld.SpawnSquad(24, 5, 16, TacticalDoctrine.Advance);
var actPreyNear = new Enemy(20, 5, 16);
var actPreyFar = new Enemy(12, 5, 16);
actReWorld.Enemies.Add(actPreyNear);
actReWorld.Enemies.Add(actPreyFar);
var actBefore = OrbitalView.Snapshot(actReWorld)[0];
Eq(20, actBefore.HuntingTargetX, "slice6 hunting target before kill is the nearest prey");
actReWorld.AttackEnemy(actPreyNear, 30);
var actAfter = OrbitalView.Snapshot(actReWorld)[0];
Eq(12, actAfter.HuntingTargetX, "slice6 hunting target re-derived after prey dies");
Eq(20, actBefore.HuntingTargetX, "slice6 old snapshot keeps its old hunting target");
Eq(SquadActivity.Hunting, actAfter.Activity, "slice6 squad still hunting after retargeting");
// Activity is derived, not saved: an Advance squad round-trips
// through save/load and still reads HUNTING at the same prey,
// with no new save field created for it.
var actSaveWorld = new WorldZone(42);
actSaveWorld.SpawnSquad(24, 5, 16, TacticalDoctrine.Advance);
actSaveWorld.Enemies.Add(new Enemy(20, 5, 16));
var actSaveLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(actSaveWorld));
var actSaveSnap = OrbitalView.Snapshot(actSaveLoaded)[0];
Eq(SquadActivity.Hunting, actSaveSnap.Activity, "slice6 hunting activity survives save/load by derivation");
Eq(20, actSaveSnap.HuntingTargetX, "slice6 hunting target survives save/load by derivation");
// The old rally-relative words are gone from the core display
// contract: AtRally / IsMoving remain as position facts (kept
// for existing tests), but Activity is the display word, and a
// hunting squad's Activity contradicts neither — it is simply
// not a function of AtRally at all.
Check(actHunt.AtRally == false && actHunt.Activity == SquadActivity.Hunting, "slice6 hunting word is independent of rally position");

// ---- Slice 6 Step 3: powered Unity presence (core indicator only) ----
// The indicator is a HudState count derived from placement through
// WorldZone.IsPowered, not a new core rule and not a new save field.
// The adapter keys (T mill, Y Dynamo) and the HUD text line cannot
// be compiled headlessly, so these tests prove the numbers the
// adapter will render — the same core/adapter split as Steps 1-2.
var powHudFresh = HudState.FromWorld(new WorldZone(42), new Weapon());
Eq(0, powHudFresh.TotalMills, "slice6 fresh world has no mills for the power indicator");
Eq(0, powHudFresh.PoweredMills, "slice6 fresh world has no powered mills");
var powHudUn = new WorldZone(42);
powHudUn.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
var powHudUnSnap = HudState.FromWorld(powHudUn, new Weapon());
Eq(1, powHudUnSnap.TotalMills, "slice6 unpowered mill counts as one mill");
Eq(0, powHudUnSnap.PoweredMills, "slice6 unpowered mill counts as zero powered");
var powHudPair = new WorldZone(42);
powHudPair.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
powHudPair.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
var powHudPairSnap = HudState.FromWorld(powHudPair, new Weapon());
Eq(1, powHudPairSnap.TotalMills, "slice6 powered pair counts as one mill");
Eq(1, powHudPairSnap.PoweredMills, "slice6 powered pair counts as one powered mill");
// Two mills, one powered: the count is per mill, not per Dynamo,
// and the chained mill (two cells from the Dynamo, through the
// powered mill) stays unpowered, per the Slice 5 distribution rule.
var powHudTwo = new WorldZone(42);
powHudTwo.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
powHudTwo.Chunk.TrySet(11, 20, 10, BlockType.KineticMill);
powHudTwo.Chunk.TrySet(12, 20, 10, BlockType.Dynamo);
var powHudTwoSnap = HudState.FromWorld(powHudTwo, new Weapon());
Eq(2, powHudTwoSnap.TotalMills, "slice6 two mills count as two");
Eq(1, powHudTwoSnap.PoweredMills, "slice6 only the face-adjacent mill of two counts powered");
// A diagonal Dynamo powers nothing, and neither a Dynamo alone
// nor a wall next to a Dynamo counts as a mill at all.
var powHudDiag = new WorldZone(42);
powHudDiag.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
powHudDiag.Chunk.TrySet(17, 21, 16, BlockType.Dynamo);
Eq(0, HudState.FromWorld(powHudDiag, new Weapon()).PoweredMills, "slice6 diagonal dynamo powers no mill");
var powHudDynOnly = new WorldZone(42);
powHudDynOnly.Chunk.TrySet(16, 20, 16, BlockType.Dynamo);
var powHudDynOnlySnap = HudState.FromWorld(powHudDynOnly, new Weapon());
Eq(0, powHudDynOnlySnap.TotalMills, "slice6 a dynamo alone is not a mill");
Eq(0, powHudDynOnlySnap.PoweredMills, "slice6 a dynamo alone is not a powered mill");
var powHudWall = new WorldZone(42);
powHudWall.Chunk.TrySet(16, 20, 16, BlockType.Wall);
powHudWall.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
Eq(0, HudState.FromWorld(powHudWall, new Weapon()).TotalMills, "slice6 a wall next to a dynamo is not a mill");
// Derived, not stored: removing the Dynamo drops the count on
// the next snapshot, with no save or flag involved.
var powHudLoss = new WorldZone(42);
powHudLoss.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
powHudLoss.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
Eq(1, HudState.FromWorld(powHudLoss, new Weapon()).PoweredMills, "slice6 powered count before dynamo removal is one");
powHudLoss.Chunk.TrySet(17, 20, 16, BlockType.Air);
Eq(0, HudState.FromWorld(powHudLoss, new Weapon()).PoweredMills, "slice6 powered count after dynamo removal is zero");
Eq(1, HudState.FromWorld(powHudLoss, new Weapon()).TotalMills, "slice6 mill itself survives dynamo removal in the count");
// Derived across save/load too: the pair survives as chunk bytes
// (no new save field), and the loaded snapshot counts it again.
var powHudSave = new WorldZone(42);
powHudSave.Chunk.TrySet(15, 4, 15, BlockType.KineticMill);
powHudSave.Chunk.TrySet(16, 4, 15, BlockType.Dynamo);
var powHudSaveSnap = HudState.FromWorld(SaveSystem.LoadZone(SaveSystem.SaveZone(powHudSave)), new Weapon());
Eq(1, powHudSaveSnap.TotalMills, "slice6 loaded powered pair counts as one mill");
Eq(1, powHudSaveSnap.PoweredMills, "slice6 loaded powered pair counts as one powered mill");
// Through the real Simulation builders, with no move between
// the two builds: mill front-left, Dynamo front-centre, and the
// two cells are face-adjacent, so Simulation.Hud() — the exact
// call HudDisplay renders — reads 1/1. A mill built alone reads
// 0/1, so the indicator distinguishes the pair from the mill.
var powSim = new Simulation(42);
powSim.Tick(0.1f, 0, 0, false);
powSim.World.Player.Inventory.Add(BlockType.Stone, 10);
powSim.World.Player.Inventory.Add(BlockType.Wood, 5);
powSim.World.Player.Inventory.Add(BlockType.EmberOre, 5);
powSim.ApplyLook(-18.4f, -53f);
Eq(BuildResult.Success, powSim.BuildKineticMillAhead(), "slice6 simulation mill ahead succeeds for the indicator pair");
powSim.ApplyLook(18.4f, 8f);
Eq(BuildResult.Success, powSim.BuildDynamoAhead(), "slice6 simulation dynamo ahead succeeds for the indicator pair");
Eq(1, powSim.Hud().TotalMills, "slice6 simulation builder pair counts as one mill in Hud()");
Eq(1, powSim.Hud().PoweredMills, "slice6 simulation builder pair counts as one powered mill in Hud()");
var powSimSolo = new Simulation(42);
powSimSolo.Tick(0.1f, 0, 0, false);
powSimSolo.World.Player.Inventory.Add(BlockType.Stone, 10);
powSimSolo.World.Player.Inventory.Add(BlockType.Wood, 5);
powSimSolo.ApplyLook(-18.4f, -53f);
Eq(BuildResult.Success, powSimSolo.BuildKineticMillAhead(), "slice6 simulation mill alone succeeds for the indicator");
Eq(1, powSimSolo.Hud().TotalMills, "slice6 simulation mill alone counts as one mill in Hud()");
Eq(0, powSimSolo.Hud().PoweredMills, "slice6 simulation mill alone counts as zero powered in Hud()");

// ---- Slice 6 Step 4: demo scenario + review (composition only) ----
// Mirrors the Slice 6 demo scenario step for step: a clamped Look
// and its forward vector; a HoldGround squad reading Marching
// then Holding; an Advance squad reading Hunting then Idle as
// its prey lives and dies; and a powered pair built through the
// Simulation builders, through a save/load round-trip, with
// the loaded loop running again at the powered rate. No new
// core system rides along — every piece below was proved in
// steps 1-3, and this section proves only that they compose.
var simS6 = new Simulation(42);
var s6f0 = simS6.Look.Forward();
Check(Math.Abs(s6f0.X) < 0.001f && Math.Abs(s6f0.Y) < 0.001f && Math.Abs(s6f0.Z - 1f) < 0.001f, "slice 6 demo look starts facing +Z");
simS6.ApplyLook(90f, 45f);
var s6f1 = simS6.Look.Forward();
Check(Math.Abs(s6f1.X - 0.7071f) < 0.001f && Math.Abs(s6f1.Y - 0.7071f) < 0.001f && Math.Abs(s6f1.Z) < 0.001f, "slice 6 demo look at 90 yaw / 45 pitch faces diagonally up");
simS6.ApplyLook(0f, 100f);
Eq(89f, simS6.Look.Pitch, "slice 6 demo look pitch clamps at 89 after +100 more");
var s6f2 = simS6.Look.Forward();
Check(s6f2.Y > 0.999f && Math.Abs(s6f2.X - 0.0175f) < 0.001f, "slice 6 demo clamped look faces almost straight up");
simS6.World.Player.Inventory.Add(BlockType.EmberOre, 20);
simS6.BeginOrbitalPick();
simS6.SetOrbitalRally(20, 5, 16);
var s6Hold = simS6.ConfirmOrbitalDeploy();
Check(s6Hold != null, "slice 6 demo HoldGround squad deploys through the paid picker");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(simS6.World)[0].Activity, "slice 6 demo HoldGround squad reads MARCHING before marching");
for (int i = 0; i < 4; i++) simS6.World.Advance();
var s6HoldSnap = OrbitalView.Snapshot(simS6.World)[0];
Eq(SquadActivity.Holding, s6HoldSnap.Activity, "slice 6 demo HoldGround squad reads HOLDING at its rally after 4 ticks");
Eq(20, s6HoldSnap.X, "slice 6 demo HoldGround squad reaches its rally x after 4 ticks");
Eq(90, s6HoldSnap.Health, "slice 6 demo HoldGround squad takes no damage marching with no enemy");
simS6.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance);
simS6.BeginOrbitalPick();
simS6.SetOrbitalRally(24, 5, 16);
var s6Adv = simS6.ConfirmOrbitalDeploy();
Check(s6Adv != null, "slice 6 demo Advance squad deploys through the paid picker");
Eq(0, simS6.World.Player.Inventory.Get(BlockType.EmberOre), "slice 6 demo two paid squads spend both 10-ore deploys");
simS6.World.Enemies.Add(new Enemy(16, 5, 20));
var s6AdvSnap = OrbitalView.Snapshot(simS6.World)[1];
Eq(SquadActivity.Hunting, s6AdvSnap.Activity, "slice 6 demo Advance squad reads HUNTING with a live prey");
Eq(20, s6AdvSnap.HuntingTargetZ, "slice 6 demo Advance hunting target is the live prey cell");
simS6.World.Advance();
Eq(0, simS6.World.Enemies.Count, "slice 6 demo full-strength Advance squad kills its 30-HP prey in one engagement tick");
var s6AdvAfter = OrbitalView.Snapshot(simS6.World)[1];
Eq(SquadActivity.Idle, s6AdvAfter.Activity, "slice 6 demo Advance squad reads IDLE once its prey is dead");
Eq(16, s6AdvAfter.X, "slice 6 demo Advance squad does not march to its ignored rally after the kill");
Check(s6AdvAfter.Z != 16 || s6AdvAfter.X != 16, "slice 6 demo Advance squad moved toward its prey before killing it");
Eq(90, s6AdvAfter.Health, "slice 6 demo Advance squad takes no damage: its prey dies from squad fire before adjacent damage");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(simS6.World)[0].Activity, "slice 6 demo HoldGround squad still reads HOLDING after the Advance engagement");
simS6.World.Player.Inventory.Add(BlockType.Stone, 10);
simS6.World.Player.Inventory.Add(BlockType.Wood, 5);
simS6.World.Player.Inventory.Add(BlockType.EmberOre, 5);
simS6.ApplyLook(-108.4f, -142f);
Eq(BuildResult.Success, simS6.BuildKineticMillAhead(), "slice 6 demo mill builds through simulation builder");
simS6.ApplyLook(18.4f, 8f);
Eq(BuildResult.Success, simS6.BuildDynamoAhead(), "slice 6 demo dynamo builds through simulation builder");
var s6Hud = simS6.Hud();
Eq(1, s6Hud.TotalMills, "slice 6 demo power indicator counts one mill");
Eq(1, s6Hud.PoweredMills, "slice 6 demo power indicator counts one powered mill");
foreach (var (x, y, z) in new[] { (14, 4, 17), (15, 5, 17), (15, 3, 17), (15, 4, 16), (15, 4, 18) })
    simS6.World.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) simS6.World.Advance();
Eq(5, simS6.World.PoweredBlocksGathered, "slice 6 demo powered mill gathers 5 in 5 ticks");
Eq(5, simS6.World.Player.Inventory.Get(BlockType.Stone), "slice 6 demo powered output reaches player inventory");
var saveS6 = simS6.SaveToJson();
var simS6Loaded = new Simulation(99);
Check(simS6Loaded.LoadFromJson(saveS6), "slice 6 demo save loads");
Eq(BlockType.KineticMill, simS6Loaded.World.Chunk.Get(15, 4, 17), "slice 6 demo mill survives save/load");
Eq(BlockType.Dynamo, simS6Loaded.World.Chunk.Get(16, 4, 17), "slice 6 demo dynamo survives save/load");
Eq(1, simS6Loaded.Hud().PoweredMills, "slice 6 demo loaded power indicator still counts one powered mill");
Eq(0, simS6Loaded.World.PoweredBlocksGathered, "slice 6 demo session powered counter resets on load");
Eq(2, OrbitalView.Snapshot(simS6Loaded.World).Count, "slice 6 demo both squads survive save/load");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(simS6Loaded.World)[0].Activity, "slice 6 demo loaded HoldGround squad still reads HOLDING by derivation");
Eq(SquadActivity.Idle, OrbitalView.Snapshot(simS6Loaded.World)[1].Activity, "slice 6 demo loaded Advance squad still reads IDLE by derivation");
Eq(0f, simS6Loaded.Look.Pitch, "slice 6 demo Look is not saved: a fresh loaded simulation starts at pitch 0");
var s6LoadedF = simS6Loaded.Look.Forward();
Check(Math.Abs(s6LoadedF.Z - 1f) < 0.001f, "slice 6 demo loaded Look faces +Z, not the pre-save view");
foreach (var (x, y, z) in new[] { (14, 4, 17), (15, 5, 17), (15, 3, 17), (15, 4, 16), (15, 4, 18) })
    simS6Loaded.World.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) simS6Loaded.World.Advance();
Eq(5, simS6Loaded.World.PoweredBlocksGathered, "slice 6 demo loaded powered mill gathers 5 in 5 ticks");
// The demo's own save length, as a composition fingerprint: the
// Slice 6 save carries two squads, a mill, and a Dynamo.
Eq(44024, saveS6.Length, "slice 6 demo save JSON length matches the demo run");

// ---- Slice 7 Step 1: second squad type — Recon (chosen from recon / heavy / flyer) ----
// Recon is the long-range screen Vanguard cannot be: it engages
// from range 5, but pays with fragility (2 x 20 HP = 40, against
// Vanguard's 3 x 30 = 90) and low firepower (2 x 6 = 12 per tick,
// against Vanguard's 30). It is better than Vanguard in exactly
// one dimension (range) and worse in the other two, so it is a
// choice, not a replacement. Heavy was not chosen: a tougher
// anchor under HoldGround is Vanguard with more HP, not a new
// tactical choice. Flyer was not chosen: it would need a new
// movement rule (ignoring solids / air) with targeting and
// occupancy consequences, which would make the kind untestable
// in isolation — the scope's own warning. Movement stays the
// shared one-voxel rule for both kinds.
// Per-kind shape table: Vanguard's numbers are unchanged since 0.9.0.
Eq(3, SquadShapes.For(SquadKind.Vanguard).Members, "slice7 vanguard shape members unchanged");
Eq(30, SquadShapes.For(SquadKind.Vanguard).MemberHealth, "slice7 vanguard shape member health unchanged");
Eq(10, SquadShapes.For(SquadKind.Vanguard).MemberDamage, "slice7 vanguard shape member damage unchanged");
Eq(3, SquadShapes.For(SquadKind.Vanguard).EngagementRange, "slice7 vanguard shape range unchanged");
Eq(2, SquadShapes.For(SquadKind.Recon).Members, "slice7 recon shape members");
Eq(20, SquadShapes.For(SquadKind.Recon).MemberHealth, "slice7 recon shape member health");
Eq(6, SquadShapes.For(SquadKind.Recon).MemberDamage, "slice7 recon shape member damage");
Eq(5, SquadShapes.For(SquadKind.Recon).EngagementRange, "slice7 recon shape range");
bool shapeThrows = false;
try { SquadShapes.For((SquadKind)99); } catch (ArgumentOutOfRangeException) { shapeThrows = true; }
Check(shapeThrows, "slice7 unknown kind has no shape, throws rather than falling back to Vanguard");
// A deployed Recon squad reads its own shape, not Vanguard's.
var reconWorld = new WorldZone(42);
var recon = reconWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon)!;
Eq(SquadKind.Recon, recon.Kind, "slice7 recon deploy kind");
Eq(40, recon.Health, "slice7 recon full health is 40, not 90");
Eq(40, recon.MaxHealth, "slice7 recon max health is 40");
Eq(2, recon.AliveMembers, "slice7 recon full members");
Eq(12, recon.DamagePerTick, "slice7 recon full damage per tick");
var vanWorld = new WorldZone(42);
var van = vanWorld.SpawnSquad(20, 5, 16)!;
Eq(90, van.Health, "slice7 vanguard full health still 90");
Eq(3, van.AliveMembers, "slice7 vanguard full members still 3");
Eq(30, van.DamagePerTick, "slice7 vanguard full damage still 30");
// Firepower drops in member steps, per kind: Recon 40 -> 20 HP
// is 1 member dealing 6, and 40 damage kills it outright.
recon.TakeDamage(20);
Eq(20, recon.Health, "slice7 recon wounded health");
Eq(1, recon.AliveMembers, "slice7 recon wounded has 1 member");
Eq(6, recon.DamagePerTick, "slice7 recon wounded damage per tick");
recon.TakeDamage(20);
Check(recon.IsDead, "slice7 recon at 0 health is dead");
Eq(0, recon.DamagePerTick, "slice7 dead recon deals nothing");
// Recon fires at range 5: enemy starts 6 away, steps to 5 in the
// enemy phase (player far, so the squad is the nearer target),
// then the holding Recon fires 12. Vanguard at the same geometry
// stays silent, and no kind fires at range 6.
var reconRangeWorld = new WorldZone(42);
var reconRange = reconRangeWorld.SpawnSquad(16, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon)!;
reconRangeWorld.Player.X = 28; reconRangeWorld.Player.Z = 28;
reconRangeWorld.Enemies.Add(new Enemy(22, 5, 16));
reconRangeWorld.Advance();
Eq(21, reconRangeWorld.Enemies[0].X, "slice7 recon range enemy steps to distance 5");
Eq(18, reconRangeWorld.Enemies[0].Health, "slice7 recon fires at range 5 for 12");
var reconFarWorld = new WorldZone(42);
reconFarWorld.SpawnSquad(16, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon);
reconFarWorld.Player.X = 28; reconFarWorld.Player.Z = 28;
reconFarWorld.Enemies.Add(new Enemy(23, 5, 16));
reconFarWorld.Advance();
Eq(22, reconFarWorld.Enemies[0].X, "slice7 recon range-6 enemy steps to distance 6");
Eq(30, reconFarWorld.Enemies[0].Health, "slice7 recon does not fire at range 6");
var vanRangeWorld = new WorldZone(42);
vanRangeWorld.SpawnSquad(16, 5, 16);
vanRangeWorld.Player.X = 28; vanRangeWorld.Player.Z = 28;
vanRangeWorld.Enemies.Add(new Enemy(21, 5, 16));
vanRangeWorld.Advance();
Eq(20, vanRangeWorld.Enemies[0].X, "slice7 vanguard range enemy steps to distance 4");
Eq(30, vanRangeWorld.Enemies[0].Health, "slice7 vanguard does not fire at range 4, range still 3");
// Movement is the shared rule: a Recon HoldGround squad marches
// one voxel per tick toward its rally, exactly like Vanguard.
var reconMarchWorld = new WorldZone(42);
var reconMarch = reconMarchWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon)!;
reconMarchWorld.Advance();
Eq(17, reconMarch.X, "slice7 recon marches one voxel toward rally");
// Both doctrines work for Recon: an Advance Recon hunts west
// toward its enemy, not east toward its rally, and fires.
var reconAdvWorld = new WorldZone(42);
var reconAdv = reconAdvWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.Advance, SquadKind.Recon)!;
reconAdvWorld.Player.X = 28; reconAdvWorld.Player.Z = 28;
reconAdvWorld.Enemies.Add(new Enemy(12, 5, 16));
reconAdvWorld.Advance();
Eq(15, reconAdv.X, "slice7 advance recon moves toward enemy, not rally");
Eq(13, reconAdvWorld.Enemies[0].X, "slice7 advance recon enemy pursues squad");
Eq(18, reconAdvWorld.Enemies[0].Health, "slice7 advance recon fires 12 in range");
// Economy: one price, total cap. CostForKind is 10 for both
// kinds, so the prompt's COST 10 line stays true; the cap is
// 3 live squads in total, not 3 per kind.
Eq(10, SquadEconomy.CostForKind(SquadKind.Vanguard), "slice7 vanguard price is 10");
Eq(10, SquadEconomy.CostForKind(SquadKind.Recon), "slice7 recon price is 10");
var econWorld = new WorldZone(42);
econWorld.Player.Inventory.Add(BlockType.EmberOre, 40);
var econRecon = econWorld.TryDeployPaidSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon)!;
Eq(SquadKind.Recon, econRecon.Kind, "slice7 paid recon deploy kind");
Eq(40, econRecon.Health, "slice7 paid recon deploy health");
Eq(30, econWorld.Player.Inventory.Get(BlockType.EmberOre), "slice7 paid recon deploy charges exactly 10");
econWorld.TryDeployPaidSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Vanguard);
econWorld.TryDeployPaidSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon);
Eq(3, SquadEconomy.LiveSquadCount(econWorld), "slice7 mixed kinds count toward one total cap");
Check(econWorld.TryDeployPaidSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon) == null, "slice7 fourth squad refused at total cap, not per-kind cap");
Eq(10, econWorld.Player.Inventory.Get(BlockType.EmberOre), "slice7 refused over-cap deploy charges nothing");
var econPoorWorld = new WorldZone(42);
Check(econPoorWorld.TryDeployPaidSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon) == null, "slice7 unfunded recon deploy refused");
Eq(0, econPoorWorld.Squads.Count, "slice7 unfunded recon deploy adds no squad");
// Picker: doctrine and kind are independent choices now. A kind
// selection survives a doctrine change, an unknown kind is
// rejected, and a Confirm deploys the selected combination.
var reconPickSim = new Simulation(42);
reconPickSim.World.Player.Inventory.Add(BlockType.EmberOre, 10);
reconPickSim.BeginOrbitalPick();
Check(reconPickSim.OrbitalCommand.SelectKind(SquadKind.Recon), "slice7 picker selects recon kind");
Check(reconPickSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround), "slice7 picker selects doctrine after kind");
Eq(SquadKind.Recon, reconPickSim.OrbitalCommand.Kind, "slice7 doctrine change keeps selected kind");
Check(!reconPickSim.OrbitalCommand.SelectKind((SquadKind)99), "slice7 picker rejects unknown kind");
Eq(SquadKind.Recon, reconPickSim.OrbitalCommand.Kind, "slice7 rejected kind leaves selection unchanged");
reconPickSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance);
reconPickSim.SetOrbitalRally(20, 5, 16);
var reconPicked = reconPickSim.ConfirmOrbitalDeploy()!;
Eq(SquadKind.Recon, reconPicked.Kind, "slice7 picker confirm deploys recon");
Eq(TacticalDoctrine.Advance, reconPicked.Doctrine, "slice7 picker confirm deploys advance");
Eq(0, reconPickSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice7 picker recon deploy charges 10");
// Save/load: the load clamp is per-kind, not Vanguard-shaped.
// A 999-HP Recon loads at 40, a 999-HP Vanguard at 90, and a
// mixed save round-trips both kinds with their own health.
var clampRecon = new TacticalSquad(16, 5, 16, 20, 5, 16, 999, TacticalDoctrine.HoldGround, SquadKind.Recon);
Eq(40, clampRecon.Health, "slice7 recon load clamp is 40, not vanguard 90");
var clampVan = new TacticalSquad(16, 5, 16, 20, 5, 16, 999, TacticalDoctrine.HoldGround, SquadKind.Vanguard);
Eq(90, clampVan.Health, "slice7 vanguard load clamp still 90");
var saveMixedWorld = new WorldZone(42);
saveMixedWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Vanguard);
var saveRecon = saveMixedWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon)!;
saveRecon.TakeDamage(5); // 40 -> 35, still 2 members
var saveMixedLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(saveMixedWorld));
Eq(2, saveMixedLoaded.Squads.Count, "slice7 mixed save loads both squads");
Eq(SquadKind.Vanguard, saveMixedLoaded.Squads[0].Kind, "slice7 mixed save vanguard kind preserved");
Eq(90, saveMixedLoaded.Squads[0].Health, "slice7 mixed save vanguard health preserved");
Eq(SquadKind.Recon, saveMixedLoaded.Squads[1].Kind, "slice7 mixed save recon kind preserved");
Eq(35, saveMixedLoaded.Squads[1].Health, "slice7 mixed save recon health preserved");
Eq(2, saveMixedLoaded.Squads[1].AliveMembers, "slice7 mixed save recon members preserved");
// Targeting stays kind-blind: an enemy pursues the nearer Recon
// squad, not the farther player, with no kind priority in the rule.
var targetWorld = new WorldZone(42);
targetWorld.SpawnSquad(16, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon);
targetWorld.Player.X = 28; targetWorld.Player.Z = 28;
targetWorld.Enemies.Add(new Enemy(20, 5, 16));
targetWorld.Advance();
Eq(19, targetWorld.Enemies[0].X, "slice7 enemy pursues nearer recon squad over farther player");
// Snapshot carries the kind and its shape: a holding Recon
// reads Holding with 2 members, 40 HP, 12 damage.
var snapWorld = new WorldZone(42);
snapWorld.SpawnSquad(16, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon);
var snapRecon = OrbitalView.Snapshot(snapWorld)[0];
Eq(SquadKind.Recon, snapRecon.Kind, "slice7 snapshot recon kind");
Eq(2, snapRecon.AliveMembers, "slice7 snapshot recon members");
Eq(40, snapRecon.Health, "slice7 snapshot recon health");
Eq(12, snapRecon.DamagePerTick, "slice7 snapshot recon damage");
Eq(SquadActivity.Holding, snapRecon.Activity, "slice7 snapshot recon at rally is holding");

// ---- Slice 7 Step 2: demo scenario, mirrored as composed tests ----
// The same run the demo prints: Vanguard HoldGround on z=16 and
// Recon Advance on z=20, both through the paid picker, one enemy
// per lane at (27,5,z), player parked far at (28,5,4) so every
// enemy step pursues a squad. The Recon spawns on its own lane
// only because the player walks to z=20 between deploys —
// deploying both from one spot would stack them, as a first
// draft of this scenario did, and the Recon would then hunt
// the Vanguard-lane enemy instead of its own.
var demoS7 = new Simulation(42);
demoS7.World.Player.Inventory.Add(BlockType.EmberOre, 20);
demoS7.BeginOrbitalPick();
demoS7.SetOrbitalRally(20, 5, 16);
var demoS7Van = demoS7.ConfirmOrbitalDeploy()!;
Eq(SquadKind.Vanguard, demoS7Van.Kind, "slice7 demo vanguard kind");
Eq(TacticalDoctrine.HoldGround, demoS7Van.Doctrine, "slice7 demo vanguard doctrine");
Eq(3, demoS7Van.AliveMembers, "slice7 demo vanguard members");
Eq(90, demoS7Van.Health, "slice7 demo vanguard health");
Eq(30, demoS7Van.DamagePerTick, "slice7 demo vanguard damage");
Eq(3, demoS7Van.EngagementRange, "slice7 demo vanguard range");
demoS7.World.Player.Z = 20;
demoS7.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance);
demoS7.OrbitalCommand.SelectKind(SquadKind.Recon);
demoS7.BeginOrbitalPick();
demoS7.SetOrbitalRally(24, 5, 20);
var demoS7Rec = demoS7.ConfirmOrbitalDeploy()!;
Eq(SquadKind.Recon, demoS7Rec.Kind, "slice7 demo recon kind");
Eq(TacticalDoctrine.Advance, demoS7Rec.Doctrine, "slice7 demo recon doctrine");
Eq(2, demoS7Rec.AliveMembers, "slice7 demo recon members");
Eq(40, demoS7Rec.Health, "slice7 demo recon health");
Eq(12, demoS7Rec.DamagePerTick, "slice7 demo recon damage");
Eq(5, demoS7Rec.EngagementRange, "slice7 demo recon range");
Eq(0, demoS7.World.Player.Inventory.Get(BlockType.EmberOre), "slice7 demo two deploys spend all 20 ore");
Eq(16, demoS7Van.X, "slice7 demo vanguard spawns at player lane");
Eq(20, demoS7Rec.Z, "slice7 demo recon spawns on second lane after player move");
demoS7.BeginOrbitalPick();
demoS7.SetOrbitalRally(20, 5, 20);
Check(demoS7.ConfirmOrbitalDeploy() == null, "slice7 demo third picker deploy refused unaffordable");
Check(demoS7.OrbitalCommand.IsPicking, "slice7 demo failed confirm keeps picker open");
Eq(0, demoS7.World.Player.Inventory.Get(BlockType.EmberOre), "slice7 demo refused deploy charges nothing");
demoS7.OrbitalCommand.CancelPick();
demoS7.World.Player.X = 28; demoS7.World.Player.Z = 4;
demoS7.World.Enemies.Add(new Enemy(27, 5, 16));
demoS7.World.Enemies.Add(new Enemy(27, 5, 20));
demoS7.World.Advance();
Eq(17, demoS7Van.X, "slice7 demo tick 1 vanguard marches");
Eq(17, demoS7Rec.X, "slice7 demo tick 1 recon hunts");
Eq(26, demoS7.World.Enemies[0].X, "slice7 demo tick 1 vanguard-lane enemy steps west");
Eq(26, demoS7.World.Enemies[1].X, "slice7 demo tick 1 recon-lane enemy steps west");
demoS7.World.Advance();
Eq(18, demoS7Van.X, "slice7 demo tick 2 vanguard marches");
Eq(18, demoS7Rec.X, "slice7 demo tick 2 recon hunts");
Eq(30, demoS7.World.Enemies[1].Health, "slice7 demo tick 2 recon still out of range");
demoS7.World.Advance();
Eq(19, demoS7Van.X, "slice7 demo tick 3 vanguard still marching");
Eq(30, demoS7.World.Enemies[0].Health, "slice7 demo tick 3 marching vanguard does not fire");
Eq(18, demoS7.World.Enemies[1].Health, "slice7 demo tick 3 recon fires at range 5 for 12");
demoS7.World.Advance();
Eq(20, demoS7Van.X, "slice7 demo tick 4 vanguard reaches rally");
Check(demoS7Van.AtRally, "slice7 demo tick 4 vanguard at rally");
Eq(1, demoS7.World.Enemies.Count, "slice7 demo tick 4 vanguard kills its lane enemy at range 3");
Eq(6, demoS7.World.Enemies[0].Health, "slice7 demo tick 4 recon fires its lane enemy to 6");
demoS7.World.Advance();
Eq(0, demoS7.World.Enemies.Count, "slice7 demo tick 5 recon kills its lane enemy");
Eq(90, demoS7Van.Health, "slice7 demo both squads finish intact vanguard");
Eq(40, demoS7Rec.Health, "slice7 demo both squads finish intact recon");
var demoS7Snap = OrbitalView.Snapshot(demoS7.World);
Eq(SquadActivity.Holding, demoS7Snap[0].Activity, "slice7 demo vanguard reads holding");
Eq(SquadActivity.Idle, demoS7Snap[1].Activity, "slice7 demo recon reads idle with no prey");
var demoS7Save = demoS7.SaveToJson();
var demoS7Loaded = new Simulation(99);
Check(demoS7Loaded.LoadFromJson(demoS7Save), "slice7 demo save loads");
Eq(2, demoS7Loaded.World.Squads.Count, "slice7 demo loaded squad count");
Eq(SquadKind.Vanguard, demoS7Loaded.World.Squads[0].Kind, "slice7 demo loaded vanguard kind");
Eq(TacticalDoctrine.HoldGround, demoS7Loaded.World.Squads[0].Doctrine, "slice7 demo loaded vanguard doctrine");
Eq(20, demoS7Loaded.World.Squads[0].X, "slice7 demo loaded vanguard position");
Eq(90, demoS7Loaded.World.Squads[0].Health, "slice7 demo loaded vanguard health");
Eq(SquadKind.Recon, demoS7Loaded.World.Squads[1].Kind, "slice7 demo loaded recon kind");
Eq(TacticalDoctrine.Advance, demoS7Loaded.World.Squads[1].Doctrine, "slice7 demo loaded recon doctrine");
Eq(21, demoS7Loaded.World.Squads[1].X, "slice7 demo loaded recon position");
Eq(40, demoS7Loaded.World.Squads[1].Health, "slice7 demo loaded recon health");
Eq(0, demoS7Loaded.World.Player.Inventory.Get(BlockType.EmberOre), "slice7 demo loaded ore still spent");
Check(!demoS7Loaded.CanDeployOrbitalSquad(), "slice7 demo loaded sim cannot deploy unfunded");
demoS7Loaded.World.Advance();
var demoS7LoadedSnap = OrbitalView.Snapshot(demoS7Loaded.World);
Eq(SquadActivity.Holding, demoS7LoadedSnap[0].Activity, "slice7 demo loaded vanguard still holding");
Eq(SquadActivity.Idle, demoS7LoadedSnap[1].Activity, "slice7 demo loaded recon still idle");
var demoS7WorldSave = SaveSystem.SaveZone(demoS7.World);
var demoS7OldJson = demoS7WorldSave[..demoS7WorldSave.LastIndexOf(",\"Squads\":", StringComparison.Ordinal)] + "}";
Eq(0, SaveSystem.LoadZone(demoS7OldJson).Squads.Count, "slice7 demo old save without squads loads with no squads");

// ---- Slice 8 Step 1: yaw-rotated movement (0.36.0) ----
// Rotation lives in Simulation.Tick, yaw only, horizontal only.
// Helper: settle a fresh sim, yaw it, walk one tick, return deltas.
(float dX, float dZ, float dY) WalkYaw(float yaw, float pitch, float forward, float strafe)
{
    var s = new Simulation(42);
    s.Tick(0.1f, 0, 0, false);
    float sx = s.Fps.X, sz = s.Fps.Z, sy = s.Fps.Y;
    s.ApplyLook(yaw, pitch);
    s.Tick(0.1f, forward, strafe, false);
    return (s.Fps.X - sx, s.Fps.Z - sz, s.Fps.Y - sy);
}
bool Near(float got, float want) => Math.Abs(got - want) < 0.02f;
var w0 = WalkYaw(0f, 0f, 1f, 0f);
Check(Near(w0.dX, 0f) && Near(w0.dZ, 0.6f), $"slice8 yaw 0 W walks +Z (dX={w0.dX}, dZ={w0.dZ})");
var d0 = WalkYaw(0f, 0f, 0f, 1f);
Check(Near(d0.dX, 0.6f) && Near(d0.dZ, 0f), $"slice8 yaw 0 D walks +X (dX={d0.dX}, dZ={d0.dZ})");
var s0 = WalkYaw(0f, 0f, -1f, 0f);
Check(Near(s0.dZ, -0.6f), $"slice8 yaw 0 S walks -Z (dZ={s0.dZ})");
var a0 = WalkYaw(0f, 0f, 0f, -1f);
Check(Near(a0.dX, -0.6f), $"slice8 yaw 0 A walks -X (dX={a0.dX})");
// Yaw 90: W walks +X. Strafe sign, decided and pinned here: D walks
// -Z, the player's right when facing +X (right = (cos yaw, -sin yaw)
// in XZ), and A walks +Z, the mirror of it — not the other way.
var w90 = WalkYaw(90f, 0f, 1f, 0f);
Check(Near(w90.dX, 0.6f) && Near(w90.dZ, 0f), $"slice8 yaw 90 W walks +X (dX={w90.dX}, dZ={w90.dZ})");
var d90 = WalkYaw(90f, 0f, 0f, 1f);
Check(Near(d90.dX, 0f) && Near(d90.dZ, -0.6f), $"slice8 yaw 90 D walks -Z, right of +X facing (dX={d90.dX}, dZ={d90.dZ})");
var a90 = WalkYaw(90f, 0f, 0f, -1f);
Check(Near(a90.dZ, 0.6f), $"slice8 yaw 90 A walks +Z (dZ={a90.dZ})");
var s90 = WalkYaw(90f, 0f, -1f, 0f);
Check(Near(s90.dX, -0.6f), $"slice8 yaw 90 S walks -X (dX={s90.dX})");
var w180 = WalkYaw(180f, 0f, 1f, 0f);
Check(Near(w180.dZ, -0.6f), $"slice8 yaw 180 W walks -Z (dZ={w180.dZ})");
var d180 = WalkYaw(180f, 0f, 0f, 1f);
Check(Near(d180.dX, -0.6f), $"slice8 yaw 180 D walks -X (dX={d180.dX})");
var wNeg90 = WalkYaw(-90f, 0f, 1f, 0f);
Check(Near(wNeg90.dX, -0.6f), $"slice8 yaw -90 W walks -X (dX={wNeg90.dX})");
var w45 = WalkYaw(45f, 0f, 1f, 0f);
Check(Near(w45.dX, 0.4243f) && Near(w45.dZ, 0.4243f), $"slice8 yaw 45 W walks diagonal +X+Z (dX={w45.dX}, dZ={w45.dZ})");
var w360 = WalkYaw(360f, 0f, 1f, 0f);
Check(Near(w360.dZ, 0.6f), $"slice8 yaw 360 W walks +Z again (dZ={w360.dZ})");
// Pitch must NOT move the player vertically, at yaw 0 or yaw 90.
var p0 = WalkYaw(0f, 45f, 1f, 0f);
Check(Near(p0.dZ, 0.6f) && Near(p0.dY, 0f), $"slice8 pitch 45 yaw 0 W still walks level +Z (dZ={p0.dZ}, dY={p0.dY})");
var p90 = WalkYaw(90f, 45f, 1f, 0f);
Check(Near(p90.dX, 0.6f) && Near(p90.dY, 0f), $"slice8 pitch 45 yaw 90 W still walks level +X (dX={p90.dX}, dY={p90.dY})");
var pDown = WalkYaw(90f, -45f, 1f, 0f);
Check(Near(pDown.dX, 0.6f) && Near(pDown.dY, 0f), $"slice8 pitch -45 yaw 90 W does not dive (dX={pDown.dX}, dY={pDown.dY})");
// Diagonal speed: PRESERVED, not normalised — decided in this step
// so a direction fix does not smuggle in a speed change. W+D at
// yaw 0 is (0.6, 0.6), magnitude 0.849, faster than a straight 0.6;
// at yaw 90 the same pair is (0.6, -0.6), same magnitude, rotated;
// at yaw 45 it collapses to (0.849, 0), same magnitude again.
var diag0 = WalkYaw(0f, 0f, 1f, 1f);
Check(Near(diag0.dX, 0.6f) && Near(diag0.dZ, 0.6f), $"slice8 diagonal yaw 0 preserved (dX={diag0.dX}, dZ={diag0.dZ})");
var diag90 = WalkYaw(90f, 0f, 1f, 1f);
Check(Near(diag90.dX, 0.6f) && Near(diag90.dZ, -0.6f), $"slice8 diagonal yaw 90 preserved and rotated (dX={diag90.dX}, dZ={diag90.dZ})");
var diag45 = WalkYaw(45f, 0f, 1f, 1f);
Check(Near(diag45.dX, 0.8485f) && Near(diag45.dZ, 0f), $"slice8 diagonal yaw 45 preserved (dX={diag45.dX}, dZ={diag45.dZ})");
// Skill speed still scales rotated movement: Swift Feet (+5%)
// at yaw 90 walks 0.63 in +X, not the base 0.6.
var s8SwiftSim = new Simulation(42);
s8SwiftSim.Tick(0.1f, 0, 0, false);
var s8SwiftBuild = new SkillBuild();
Check(s8SwiftBuild.Allocate("S3"), "slice8 swift feet allocates");
Check(s8SwiftSim.ApplySkills(s8SwiftBuild), "slice8 swift feet applies");
float s8SwiftSX = s8SwiftSim.Fps.X;
s8SwiftSim.ApplyLook(90f, 0f);
s8SwiftSim.Tick(0.1f, 1f, 0f, false);
Check(Near(s8SwiftSim.Fps.X - s8SwiftSX, 0.63f), $"slice8 skill speed scales yaw-rotated movement (dX={s8SwiftSim.Fps.X - s8SwiftSX})");
// Axis-separated sliding in world space, through the new entry
// point the rotation uses: X blocked by a wall, Z still moves.
var slideChunk = new VoxelChunk();
for (int x = 0; x < 32; x++) for (int z = 0; z < 32; z++) for (int y = 1; y <= 3; y++) slideChunk.TrySet(x, y, z, BlockType.Stone);
// A full wall plane at x=18 (feet and head height, all z), not a
// single cell: a diagonal walk past a single cell would legitimately
// slide around its corner, which is not a blocked-X proof.
for (int z = 0; z < 32; z++) { slideChunk.TrySet(18, 4, z, BlockType.Stone); slideChunk.TrySet(18, 5, z, BlockType.Stone); }
var slideFpsX = new FpsController(16f, 4f, 16f);
slideFpsX.UpdateWorld(0.1f, 0, 0, false, slideChunk);
for (int i = 0; i < 20; i++) slideFpsX.UpdateWorld(0.1f, 1f, 0f, false, slideChunk);
Check(slideFpsX.X < 18f - FpsController.HalfWidth, $"slice8 world-space wall plane blocks X (X={slideFpsX.X})");
var slideFps = new FpsController(16f, 4f, 16f);
slideFps.UpdateWorld(0.1f, 0, 0, false, slideChunk);
for (int i = 0; i < 20; i++) slideFps.UpdateWorld(0.1f, 1f, 1f, false, slideChunk);
Check(slideFps.X < 18f - FpsController.HalfWidth, $"slice8 world-space wall blocks X on diagonal (X={slideFps.X})");
Check(slideFps.Z > 20f, $"slice8 world-space sliding still moves Z past blocked X (Z={slideFps.Z})");
// ---- Slice 8 Step 2: aim-based gather / build / fire ----
// The Step 1 asymmetry is closed here: at yaw 90, gather,
// build, and fire follow the aim ray (+X, down for ground),
// not the old fixed +Z cells or the nearest-enemy rule.
// Neutral-aim compatibility, stated openly: at yaw 0,
// pitch -45, the eye ray reaches the old gather cell
// (16,3,17) — pitch, not yaw, is what moved gather off
// the old feet-level rule, because the eye is 1.6 m above
// the feet and a level ray passes over the ground. At
// yaw 0, pitch 0, gather and build therefore do NOT
// reproduce the Slice 1 cells (the level ray finds only
// air at eye height), and the Slice 1 demo is updated
// for that in this step instead of silently keeping
// numbers the new rule no longer produces.
var aimNeutral = new Simulation(42);
aimNeutral.Tick(0.1f, 0, 0, false);
aimNeutral.ApplyLook(0f, -45f);
Check(aimNeutral.GatherAhead() == GatherResult.Success, "slice8 step 2 pitch -45 yaw 0 gathers the old ground cell (16,3,17)");
Eq(BlockType.Air, aimNeutral.World.Chunk.Get(16, 3, 17), "slice8 step 2 old gather cell is empty after aimed gather");
var aimLevel = new Simulation(42);
aimLevel.Tick(0.1f, 0, 0, false);
Check(aimLevel.GatherAhead() == GatherResult.Air, "slice8 step 2 level aim gathers nothing: the eye ray passes over the ground");
var asymSim = new Simulation(42);
asymSim.Tick(0.1f, 0, 0, false);
asymSim.ApplyLook(90f, -45f);
Check(asymSim.GatherAhead() == GatherResult.Success, "slice8 step 2 yaw 90 pitch -45 gathers the +X ground cell (17,3,16)");
Eq(BlockType.Air, asymSim.World.Chunk.Get(17, 3, 16), "slice8 step 2 yaw 90 gather cell is empty after aimed gather");
var aimBuild = new Simulation(42);
aimBuild.Tick(0.1f, 0, 0, false);
aimBuild.ApplyLook(90f, -45f);
aimBuild.World.Player.Inventory.Add(BlockType.Stone, 10);
Check(aimBuild.BuildWallAhead() == BuildResult.Success, "slice8 step 2 yaw 90 pitch -45 builds a wall");
Eq(BlockType.Wall, aimBuild.World.Chunk.Get(17, 4, 16), "slice8 step 2 yaw 90 wall lands in the air cell before the +X ground");
var aimFire = new Simulation(42);
aimFire.Tick(0.1f, 0, 0, false);
aimFire.ApplyLook(90f, 0f);
aimFire.World.Enemies.Add(new Enemy(22, 5, 16));
for (int i = 0; i < 3; i++) aimFire.FireAimed();
Eq(1, aimFire.EnemiesKilled, "slice8 step 2 yaw 90 aimed fire kills the +X enemy in 3 shots");
// Fire is blocked by a solid: a wall between the player and
// an enemy stops the shot, costs the round, and leaves the
// enemy untouched — fire cannot hit through a wall.
var aimBlocked = new Simulation(42);
aimBlocked.Tick(0.1f, 0, 0, false);
aimBlocked.World.Chunk.TrySet(16, 5, 19, BlockType.Wall);
aimBlocked.World.Enemies.Add(new Enemy(16, 5, 22));
Check(aimBlocked.FireAimed(), "slice8 step 2 blocked shot still fires");
Eq(30, aimBlocked.World.Enemies[0].Health, "slice8 step 2 wall blocks aimed fire: enemy untouched");
Eq(41, aimBlocked.Weapon.Mag, "slice8 step 2 blocked shot still consumes ammo");
// Fire reaches the core by aim: from (24,5,24), yaw 45 and
// pitch -10 put the core cell (28,5,28) in the ray within
// the 10 m reach, replacing the old 10 m bubble.
var aimCore = new Simulation(42);
aimCore.Fps.Teleport(24, 5, 24);
aimCore.ApplyLook(45f, -10f);
Check(aimCore.FireAimed(), "slice8 step 2 aimed core shot fires");
Eq(90, aimCore.World.CoreHealth, "slice8 step 2 aimed core shot deals 10 damage");
// Gather refuses a non-gatherable solid instead of falling
// through it: a wall in the aimed cell is NotGatherable,
// not a gather of the ground behind it.
var aimWall = new Simulation(42);
aimWall.Tick(0.1f, 0, 0, false);
aimWall.World.Chunk.TrySet(16, 3, 17, BlockType.Wall);
aimWall.ApplyLook(0f, -45f);
Check(aimWall.GatherAhead() == GatherResult.NotGatherable, "slice8 step 2 gather at a wall refuses, does not gather through it");
// No aimed surface, no build, no spend: pitching up with
// stone in hand returns Floating and leaves the stone.
var aimSky = new Simulation(42);
aimSky.Tick(0.1f, 0, 0, false);
aimSky.ApplyLook(0f, 45f);
aimSky.World.Player.Inventory.Add(BlockType.Stone, 10);
Check(aimSky.BuildWallAhead() == BuildResult.Floating, "slice8 step 2 skyward build has no aimed surface");
Eq(10, aimSky.World.Player.Inventory.Get(BlockType.Stone), "slice8 step 2 skyward failed build spends no stone");
// A ray that leaves the chunk is OutOfBounds, not Air.
var aimEdge = new Simulation(42);
aimEdge.Fps.Teleport(31, 4, 31);
Check(aimEdge.GatherAhead() == GatherResult.OutOfBounds, "slice8 step 2 gather out of the chunk is out of bounds");
// Reach is bounded: an enemy 12 m ahead, past the 10 m
// reach, is not hit, and the shot still costs a round.
var aimFar = new Simulation(42);
aimFar.Tick(0.1f, 0, 0, false);
aimFar.World.Enemies.Add(new Enemy(16, 5, 28));
Check(aimFar.FireAimed(), "slice8 step 2 out-of-reach shot still fires");
Eq(30, aimFar.World.Enemies[0].Health, "slice8 step 2 enemy past 10 m reach is not hit");
Eq(41, aimFar.Weapon.Mag, "slice8 step 2 out-of-reach shot still consumes ammo");
// Direct FpsController.Update keeps its Slice 1 meaning (forward
// is +Z, strafe is +X) even now that Simulation rotates above it:
// the rotation is a Simulation-Tick rule, not a second rule inside
// the controller, and a controller with no Look cannot rotate.
var directFps = new FpsController(16f, 4f, 16f);
directFps.Update(0.1f, 0, 0, false, new WorldZone(42).Chunk);
directFps.Update(0.1f, 1f, 0f, false, new WorldZone(42).Chunk);
Check(directFps.Z > 16f && Math.Abs(directFps.X - 16f) < 0.001f, "slice8 direct fps update stays world-aligned forward +Z");

// ---- Slice 8 Step 3: composed demo scenario (0.38.0) ----
// Mirrors the Slice 8 demo scenario step for step: one
// Simulation faces +X throughout — yaw 90 walk, pitch -45
// gather, same-aim build, level fire — then a save/load
// round-trip. The gather cell is (18,3,16), not the
// feet-level yaw-90 cell (17,3,16): after the 0.6 m walk
// the eye is 0.6 m east, and the descending ray strikes
// the ground one cell further east, which is why the
// demo prints the cell instead of assuming it.
var s8Demo = new Simulation(42);
s8Demo.Tick(0.1f, 0, 0, false);
Check(Near(s8Demo.Fps.X, 16f) && Near(s8Demo.Fps.Z, 16f) && Near(s8Demo.Fps.Y, 4f), $"slice8 step 3 demo starts at feet (16,4,16) (got {s8Demo.Fps.X},{s8Demo.Fps.Y},{s8Demo.Fps.Z})");
s8Demo.ApplyLook(90f, 0f);
s8Demo.Tick(0.1f, 1f, 0f, false);
Check(Near(s8Demo.Fps.X, 16.6f) && Near(s8Demo.Fps.Z, 16f), $"slice8 step 3 yaw 90 walk moves +X only (got {s8Demo.Fps.X},{s8Demo.Fps.Z})");
s8Demo.ApplyLook(0f, -45f);
Check(s8Demo.GatherAhead() == GatherResult.Success, "slice8 step 3 composed gather succeeds at yaw 90 pitch -45");
Eq(BlockType.Air, s8Demo.World.Chunk.Get(18, 3, 16), "slice8 step 3 composed gather empties (18,3,16)");
Eq(1, s8Demo.BlocksGathered, "slice8 step 3 composed gather counts 1");
s8Demo.World.Player.Inventory.Add(BlockType.Stone, 10);
Check(s8Demo.BuildWallAhead() == BuildResult.Success, "slice8 step 3 composed build succeeds at the same aim");
Eq(BlockType.Wall, s8Demo.World.Chunk.Get(18, 3, 16), "slice8 step 3 composed build fills (18,3,16) with a wall");
Eq(1, s8Demo.BlocksBuilt, "slice8 step 3 composed build counts 1");
Eq(9, s8Demo.World.Player.Inventory.Get(BlockType.Stone), "slice8 step 3 stone: 10 funded + 1 gathered - 2 wall = 9");
s8Demo.World.Enemies.Add(new Enemy(22, 5, 16));
s8Demo.ApplyLook(0f, 45f);
for (int i = 0; i < 3; i++) s8Demo.FireAimed();
Eq(1, s8Demo.EnemiesKilled, "slice8 step 3 composed fire kills the +X enemy in 3 shots");
Eq(0, s8Demo.World.Enemies.Count, "slice8 step 3 composed fire leaves no enemies");
Eq(39, s8Demo.Weapon.Mag, "slice8 step 3 composed fire leaves 39 rounds in the mag");
var s8DemoSave = s8Demo.SaveToJson();
var s8DemoLoaded = new Simulation(99);
Check(s8DemoLoaded.LoadFromJson(s8DemoSave), "slice8 step 3 composed save loads");
Eq(BlockType.Wall, s8DemoLoaded.World.Chunk.Get(18, 3, 16), "slice8 step 3 wall survives save/load");
Eq(0, s8DemoLoaded.World.Enemies.Count, "slice8 step 3 kill survives save/load as an empty enemy list");
Eq(16f, s8DemoLoaded.Fps.X, "slice8 step 3 fractional walk is not saved: loaded feet restart at the cell corner");
Eq(0f, s8DemoLoaded.Look.Yaw, "slice8 step 3 loaded view is fresh yaw 0, not the pre-save yaw 90");
Eq(0f, s8DemoLoaded.Look.Pitch, "slice8 step 3 loaded view is fresh pitch 0");
Eq(0, s8DemoLoaded.BlocksGathered, "slice8 step 3 session gather counter resets on load");
Eq(0, s8DemoLoaded.EnemiesKilled, "slice8 step 3 session kill counter resets on load");
s8DemoLoaded.Tick(0.1f, 1f, 0f, false);
Check(Near(s8DemoLoaded.Fps.X, 16f) && Near(s8DemoLoaded.Fps.Z, 16.6f), $"slice8 step 3 loaded fresh-view walk goes +Z (got {s8DemoLoaded.Fps.X},{s8DemoLoaded.Fps.Z})");

// ---- Slice 9 Step 1: voltage ladder and transformer ----
// Decides the scope's open questions in core: two levels (LV,
// HV) because two suffice; the existing Dynamo outputs LV so
// old worlds keep working; the transformer is one new block
// type that flips its input voltage, face-adjacency only;
// a mill is powered only at its rated LV. In this step an
// HV-fed mill fell back to kinetic cadence as an interim rule,
// and Step 3 replaced that fallback with destruction as a
// recorded behaviour change: the HV delivery assertions below
// still stand, while the cadence assertions for HV mills were
// rewritten in Step 3 and are named there.
Eq(Voltage.LV, VoltageRules.DynamoOutput, "slice9 dynamo outputs LV");
Eq(Voltage.LV, VoltageRules.MillRated, "slice9 mill is rated LV");
Eq(Voltage.HV, VoltageRules.Flipped(Voltage.LV), "slice9 transformer steps LV up to HV");
Eq(Voltage.LV, VoltageRules.Flipped(Voltage.HV), "slice9 transformer steps HV down to LV");
Eq(Voltage.None, VoltageRules.Flipped(Voltage.None), "slice9 unpowered transformer converts nothing");
Eq(9, (byte)BlockType.Transformer, "slice9 transformer block type appended as 9");
Eq(8, (byte)BlockType.Dynamo, "slice9 dynamo value unchanged at 8");
Eq(7, (byte)BlockType.KineticMill, "slice9 mill value unchanged at 7");
// Build: 5 EmberOre, check-before-spend, support rule, not gatherable.
var trWorld = new WorldZone(42);
trWorld.Player.Inventory.Add(BlockType.EmberOre, 5);
Eq(BuildResult.Success, trWorld.Build(15, 4, 15, BlockType.Transformer), "slice9 transformer builds with 5 ember ore");
Eq(BlockType.Transformer, trWorld.Chunk.Get(15, 4, 15), "slice9 transformer placed");
Eq(0, trWorld.Player.Inventory.Get(BlockType.EmberOre), "slice9 transformer consumes 5 ember ore");
Eq(GatherResult.NotGatherable, trWorld.Gather(15, 4, 15), "slice9 transformer itself is not gatherable");
var poorTrWorld = new WorldZone(42);
poorTrWorld.Player.Inventory.Add(BlockType.EmberOre, 4);
Eq(BuildResult.InsufficientResources, poorTrWorld.Build(15, 4, 15, BlockType.Transformer), "slice9 transformer without 5 ore fails");
Eq(BlockType.Air, poorTrWorld.Chunk.Get(15, 4, 15), "slice9 failed transformer leaves air");
Eq(4, poorTrWorld.Player.Inventory.Get(BlockType.EmberOre), "slice9 failed transformer does not spend ore");
var floatTrWorld = new WorldZone(42);
floatTrWorld.Player.Inventory.Add(BlockType.EmberOre, 5);
Eq(BuildResult.Floating, floatTrWorld.Build(16, 20, 16, BlockType.Transformer), "slice9 transformer floating in air fails like other machines");
// Delivery: Dynamo face-adjacent gives LV; diagonal gives nothing.
var lvWorld = new WorldZone(42);
lvWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
lvWorld.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
Eq(Voltage.LV, lvWorld.DeliveredVoltage(16, 20, 16), "slice9 dynamo-adjacent mill receives LV");
Check(lvWorld.IsPowered(16, 20, 16), "slice9 dynamo-adjacent mill is powered at rated voltage");
var lvDiagWorld = new WorldZone(42);
lvDiagWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
lvDiagWorld.Chunk.TrySet(17, 21, 16, BlockType.Dynamo);
Eq(Voltage.None, lvDiagWorld.DeliveredVoltage(16, 20, 16), "slice9 diagonally adjacent dynamo delivers nothing");
// One transformer step: Dynamo -> transformer -> mill delivers HV,
// and the mill is NOT powered. Step 3 changed what that means:
// the Step 1 interim kinetic fallback is replaced here by
// destruction, so this test now pins destruction on the first
// tick instead of 1 kinetic gather in 5 ticks.
var hvWorld = new WorldZone(42);
hvWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
hvWorld.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
hvWorld.Chunk.TrySet(18, 20, 16, BlockType.Dynamo);
Eq(Voltage.HV, hvWorld.DeliveredVoltage(16, 20, 16), "slice9 transformer steps dynamo LV up to HV for its mill");
Check(!hvWorld.IsPowered(16, 20, 16), "slice9 HV-fed mill is not powered at rated voltage");
Check(hvWorld.IsMismatched(16, 20, 16), "slice9 step3 HV-fed mill is mismatched before its destruction tick");
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    hvWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
hvWorld.Advance();
Eq(BlockType.Air, hvWorld.Chunk.Get(16, 20, 16), "slice9 step3 HV-fed mill is destroyed on its first tick");
Eq(1, hvWorld.DestroyedMills, "slice9 step3 one mill destroyed");
Eq(0, hvWorld.KineticBlocksGathered, "slice9 step3 destroyed mill gathers nothing kinetic");
Eq(0, hvWorld.PoweredBlocksGathered, "slice9 step3 destroyed mill counts nothing powered");
Eq(BlockType.Stone, hvWorld.Chunk.Get(15, 20, 16), "slice9 step3 destroyed mill excavates nothing on its destruction tick");
Eq(BlockType.Transformer, hvWorld.Chunk.Get(17, 20, 16), "slice9 step3 destruction never excavates the transformer");
// Two transformer steps: Dynamo -> T -> T -> mill delivers LV again,
// so indirect power through a conversion round-trip runs every tick.
var roundWorld = new WorldZone(42);
roundWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
roundWorld.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
roundWorld.Chunk.TrySet(18, 20, 16, BlockType.Transformer);
roundWorld.Chunk.TrySet(19, 20, 16, BlockType.Dynamo);
Eq(Voltage.LV, roundWorld.DeliveredVoltage(16, 20, 16), "slice9 two transformers convert LV back to LV for the mill");
Check(roundWorld.IsPowered(16, 20, 16), "slice9 round-trip mill is powered at rated voltage");
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    roundWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) roundWorld.Advance();
Eq(5, roundWorld.PoweredBlocksGathered, "slice9 round-trip mill gathers 5 in 5 ticks at powered cadence");
Eq(0, roundWorld.KineticBlocksGathered, "slice9 round-trip mill counts nothing kinetic");
// Precedence: a direct Dynamo beats a converted voltage, both for
// the mill and for a transformer fed by both a Dynamo and a
// transformer output.
var precWorld = new WorldZone(42);
precWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
precWorld.Chunk.TrySet(15, 20, 16, BlockType.Dynamo);
precWorld.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
precWorld.Chunk.TrySet(18, 20, 16, BlockType.Dynamo);
Eq(Voltage.LV, precWorld.DeliveredVoltage(16, 20, 16), "slice9 direct dynamo beats converted HV for the mill");
Check(precWorld.IsPowered(16, 20, 16), "slice9 precedence mill stays powered when a transformer is added");
var precTrWorld = new WorldZone(42);
precTrWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
precTrWorld.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
precTrWorld.Chunk.TrySet(18, 20, 16, BlockType.Dynamo);
precTrWorld.Chunk.TrySet(17, 21, 16, BlockType.Transformer);
precTrWorld.Chunk.TrySet(17, 22, 16, BlockType.Dynamo);
Eq(Voltage.HV, precTrWorld.DeliveredVoltage(16, 20, 16), "slice9 transformer fed by a dynamo converts from the dynamo, not from a neighbouring transformer");
// Unpowered transformer converts nothing; a ring with no Dynamo
// reports None instead of recursing forever.
var deadTrWorld = new WorldZone(42);
deadTrWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
deadTrWorld.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
Eq(Voltage.None, deadTrWorld.DeliveredVoltage(16, 20, 16), "slice9 unpowered transformer delivers nothing");
Check(!deadTrWorld.IsPowered(16, 20, 16), "slice9 unpowered transformer powers nothing");
var ringWorld = new WorldZone(42);
ringWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
ringWorld.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
ringWorld.Chunk.TrySet(18, 20, 16, BlockType.Transformer);
ringWorld.Chunk.TrySet(18, 20, 17, BlockType.Transformer);
ringWorld.Chunk.TrySet(17, 20, 17, BlockType.Transformer);
Eq(Voltage.None, ringWorld.DeliveredVoltage(16, 20, 16), "slice9 transformer ring with no dynamo delivers nothing");
// Derived, not stored: removing the Dynamo drops the converted
// voltage on the next tick, and a loaded transformer chain
// re-derives its voltage from placement alone.
var dropTrWorld = new WorldZone(42);
dropTrWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
dropTrWorld.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
dropTrWorld.Chunk.TrySet(18, 20, 16, BlockType.Dynamo);
Eq(Voltage.HV, dropTrWorld.DeliveredVoltage(16, 20, 16), "slice9 converted HV present before dynamo removal");
dropTrWorld.Chunk.TrySet(18, 20, 16, BlockType.Air);
Eq(Voltage.None, dropTrWorld.DeliveredVoltage(16, 20, 16), "slice9 converted voltage drops when its dynamo is removed");
var saveTrWorld = new WorldZone(42);
saveTrWorld.Chunk.TrySet(15, 4, 15, BlockType.KineticMill);
saveTrWorld.Chunk.TrySet(16, 4, 15, BlockType.Transformer);
saveTrWorld.Chunk.TrySet(17, 4, 15, BlockType.Dynamo);
saveTrWorld.Chunk.TrySet(10, 4, 10, BlockType.KineticMill);
saveTrWorld.Chunk.TrySet(11, 4, 10, BlockType.Transformer);
saveTrWorld.Chunk.TrySet(12, 4, 10, BlockType.Transformer);
saveTrWorld.Chunk.TrySet(13, 4, 10, BlockType.Dynamo);
var saveTrLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(saveTrWorld));
Eq(BlockType.Transformer, saveTrLoaded.Chunk.Get(16, 4, 15), "slice9 transformer survives save/load as a chunk byte");
Eq(Voltage.HV, saveTrLoaded.DeliveredVoltage(15, 4, 15), "slice9 loaded HV mill re-derives HV from placement, no flag needed");
Eq(Voltage.LV, saveTrLoaded.DeliveredVoltage(10, 4, 10), "slice9 loaded round-trip mill re-derives LV from placement");
Check(!saveTrLoaded.IsPowered(15, 4, 15), "slice9 loaded HV mill is not powered");
Check(saveTrLoaded.IsPowered(10, 4, 10), "slice9 loaded round-trip mill is powered");
// HUD: the power count is rated-voltage only — one LV mill and one
// HV mill count as two mills, one powered.
var trHudWorld = new WorldZone(42);
trHudWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
trHudWorld.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
trHudWorld.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
trHudWorld.Chunk.TrySet(11, 20, 10, BlockType.Transformer);
trHudWorld.Chunk.TrySet(12, 20, 10, BlockType.Dynamo);
var trHud = HudState.FromWorld(trHudWorld, new Weapon());
Eq(2, trHud.TotalMills, "slice9 HUD counts both LV and HV mills");
Eq(1, trHud.PoweredMills, "slice9 HUD counts only the LV mill as powered, HV mill is not powered (step3: it would be destroyed on a tick, not left kinetic)");
// Simulation aimed builder: same aim as the Dynamo test, same cell.
var simTr = new Simulation(42);
simTr.Tick(0.1f, 0, 0, false);
simTr.World.Player.Inventory.Add(BlockType.EmberOre, 5);
simTr.ApplyLook(0f, -45f);
Eq(BuildResult.Success, simTr.BuildTransformerAhead(), "slice9 simulation build transformer ahead succeeds");
Eq(BlockType.Transformer, simTr.World.Chunk.Get(16, 4, 17), "slice9 simulation transformer placed by aim");
Eq(0, simTr.World.Player.Inventory.Get(BlockType.EmberOre), "slice9 simulation transformer spends its ore");

// Slice 9 Step 2 — cable distribution. Cable carries a voltage
// beyond face-adjacency, face only, with no loss and no length
// limit in this step; only Cable cells conduct, and a
// mixed-voltage network (or a mill touching two voltages)
// carries nothing, rather than resolving by scan order.
// The interim unmatched-voltage fallback from Step 1 is
// unchanged here: an HV-fed mill falls back to kinetic
// cadence, and destruction is still Step 3's decision.
// (Step 3 later replaced that HV fallback with destruction;
// the Step 2 assertions it changed are rewritten there.)
Eq(10, (byte)BlockType.Cable, "slice9 step2 cable block type appended as 10");
Eq(9, (byte)BlockType.Transformer, "slice9 step2 transformer value unchanged at 9");
Eq(8, (byte)BlockType.Dynamo, "slice9 step2 dynamo value unchanged at 8");
// Build: 1 EmberOre per cell, check-before-spend, support
// rule, not gatherable.
var cableWorld = new WorldZone(42);
cableWorld.Player.Inventory.Add(BlockType.EmberOre, 1);
Eq(BuildResult.Success, cableWorld.Build(15, 4, 15, BlockType.Cable), "slice9 step2 cable builds with 1 ember ore");
Eq(BlockType.Cable, cableWorld.Chunk.Get(15, 4, 15), "slice9 step2 cable placed");
Eq(0, cableWorld.Player.Inventory.Get(BlockType.EmberOre), "slice9 step2 cable consumes 1 ember ore");
Eq(GatherResult.NotGatherable, cableWorld.Gather(15, 4, 15), "slice9 step2 cable itself is not gatherable");
var poorCableWorld = new WorldZone(42);
Eq(BuildResult.InsufficientResources, poorCableWorld.Build(15, 4, 15, BlockType.Cable), "slice9 step2 cable without ore fails");
Eq(BlockType.Air, poorCableWorld.Chunk.Get(15, 4, 15), "slice9 step2 failed cable leaves air");
var floatCableWorld = new WorldZone(42);
floatCableWorld.Player.Inventory.Add(BlockType.EmberOre, 1);
Eq(BuildResult.Floating, floatCableWorld.Build(16, 20, 16, BlockType.Cable), "slice9 step2 cable floating in air fails like other machines");
// LV cable run: Dynamo -> cable -> cable -> mill runs at
// the every-tick powered cadence, and no mill excavates
// its own wiring.
var lvCableWorld = new WorldZone(42);
lvCableWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
lvCableWorld.Chunk.TrySet(17, 20, 16, BlockType.Cable);
lvCableWorld.Chunk.TrySet(18, 20, 16, BlockType.Cable);
lvCableWorld.Chunk.TrySet(19, 20, 16, BlockType.Dynamo);
Eq(Voltage.LV, lvCableWorld.DeliveredVoltage(16, 20, 16), "slice9 step2 cable run delivers LV to its mill");
Check(lvCableWorld.IsPowered(16, 20, 16), "slice9 step2 cable-fed mill is powered at rated voltage");
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    lvCableWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) lvCableWorld.Advance();
Eq(5, lvCableWorld.PoweredBlocksGathered, "slice9 step2 cable-fed mill gathers 5 in 5 ticks at powered cadence");
Eq(0, lvCableWorld.KineticBlocksGathered, "slice9 step2 cable-fed mill counts nothing kinetic");
Eq(BlockType.Cable, lvCableWorld.Chunk.Get(17, 20, 16), "slice9 step2 powered mill never excavates its cable");
// No length limit in this step: a ten-cable run carries LV.
var longCableWorld = new WorldZone(42);
longCableWorld.Chunk.TrySet(17, 20, 16, BlockType.KineticMill);
for (int x = 18; x <= 27; x++) longCableWorld.Chunk.TrySet(x, 20, 16, BlockType.Cable);
longCableWorld.Chunk.TrySet(28, 20, 16, BlockType.Dynamo);
Eq(Voltage.LV, longCableWorld.DeliveredVoltage(17, 20, 16), "slice9 step2 ten-cable run still delivers LV, no length limit");
Check(longCableWorld.IsPowered(17, 20, 16), "slice9 step2 ten-cable mill is powered");
// Branch: one cable network feeds two mills at the same voltage.
var branchWorld = new WorldZone(42);
branchWorld.Chunk.TrySet(17, 20, 16, BlockType.KineticMill);
branchWorld.Chunk.TrySet(18, 20, 18, BlockType.KineticMill);
branchWorld.Chunk.TrySet(19, 20, 16, BlockType.Cable);
branchWorld.Chunk.TrySet(18, 20, 16, BlockType.Cable);
branchWorld.Chunk.TrySet(18, 20, 17, BlockType.Cable);
branchWorld.Chunk.TrySet(20, 20, 16, BlockType.Dynamo);
Eq(Voltage.LV, branchWorld.DeliveredVoltage(17, 20, 16), "slice9 step2 branch trunk mill receives LV");
Eq(Voltage.LV, branchWorld.DeliveredVoltage(18, 20, 18), "slice9 step2 branch leaf mill receives LV");
foreach (var (x, y, z) in new[] { (16, 20, 16), (17, 21, 16), (17, 19, 16), (17, 20, 17), (17, 20, 15),
    (17, 20, 18), (19, 20, 18), (18, 21, 18), (18, 19, 18), (18, 20, 19) })
    branchWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) branchWorld.Advance();
Eq(10, branchWorld.PoweredBlocksGathered, "slice9 step2 branched mills gather 10 in 5 ticks, 5 each");
Eq(0, branchWorld.KineticBlocksGathered, "slice9 step2 branched mills count nothing kinetic");
// A non-conducting block breaks the network: a wall between
// cables leaves the far cable unpowered, and the mill falls
// back to kinetic cadence.
var breakWorld = new WorldZone(42);
breakWorld.Chunk.TrySet(15, 20, 16, BlockType.KineticMill);
breakWorld.Chunk.TrySet(16, 20, 16, BlockType.Cable);
breakWorld.Chunk.TrySet(17, 20, 16, BlockType.Wall);
breakWorld.Chunk.TrySet(18, 20, 16, BlockType.Cable);
breakWorld.Chunk.TrySet(19, 20, 16, BlockType.Dynamo);
Eq(Voltage.None, breakWorld.DeliveredVoltage(15, 20, 16), "slice9 step2 wall between cables delivers nothing");
Check(!breakWorld.IsPowered(15, 20, 16), "slice9 step2 wall-broken mill is not powered");
foreach (var (x, y, z) in new[] { (14, 20, 16), (15, 21, 16), (15, 19, 16), (15, 20, 17), (15, 20, 15) })
    breakWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) breakWorld.Advance();
Eq(1, breakWorld.KineticBlocksGathered, "slice9 step2 wall-broken mill falls back to kinetic cadence, 1 in 5 ticks");
Eq(0, breakWorld.PoweredBlocksGathered, "slice9 step2 wall-broken mill counts nothing powered");
Eq(BlockType.Wall, breakWorld.Chunk.Get(17, 20, 16), "slice9 step2 wall still stands between the cables");
// Diagonal adjacency does not conduct, for cables as for Dynamos.
var diagCableWorld = new WorldZone(42);
diagCableWorld.Chunk.TrySet(17, 20, 16, BlockType.KineticMill);
diagCableWorld.Chunk.TrySet(18, 20, 16, BlockType.Cable);
diagCableWorld.Chunk.TrySet(19, 21, 16, BlockType.Dynamo);
Eq(Voltage.None, diagCableWorld.DeliveredVoltage(17, 20, 16), "slice9 step2 diagonally adjacent dynamo delivers nothing through a cable");
// Transformer in a cable run: the source-side mill still
// runs at LV, while the mill beyond the transformer runs
// at the converted HV. Step 3 changed that mill's fate:
// it no longer falls back to kinetic cadence, it is
// destroyed on its first tick, while the source-side mill
// keeps its powered cadence untouched.
var runTrWorld = new WorldZone(42);
runTrWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
runTrWorld.Chunk.TrySet(17, 20, 16, BlockType.Cable);
runTrWorld.Chunk.TrySet(18, 20, 16, BlockType.Transformer);
runTrWorld.Chunk.TrySet(19, 20, 16, BlockType.Cable);
runTrWorld.Chunk.TrySet(20, 20, 16, BlockType.Dynamo);
runTrWorld.Chunk.TrySet(19, 21, 16, BlockType.KineticMill);
Eq(Voltage.HV, runTrWorld.DeliveredVoltage(16, 20, 16), "slice9 step2 mill beyond a transformer in a cable run receives HV");
Check(!runTrWorld.IsPowered(16, 20, 16), "slice9 step2 converted cable mill is not powered at rated voltage");
Eq(Voltage.LV, runTrWorld.DeliveredVoltage(19, 21, 16), "slice9 step2 source-side mill still receives LV");
Check(runTrWorld.IsPowered(19, 21, 16), "slice9 step2 source-side mill is powered, transformer does not overwrite its side");
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15),
    (18, 21, 16), (20, 21, 16), (19, 22, 16), (19, 21, 17), (19, 21, 15) })
    runTrWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) runTrWorld.Advance();
Eq(5, runTrWorld.PoweredBlocksGathered, "slice9 step2 source-side mill gathers 5 in 5 ticks at powered cadence");
Eq(0, runTrWorld.KineticBlocksGathered, "slice9 step3 converted cable mill gathers nothing kinetic, it is destroyed first");
Eq(BlockType.Air, runTrWorld.Chunk.Get(16, 20, 16), "slice9 step3 converted cable mill is destroyed, not left at kinetic cadence");
Eq(1, runTrWorld.DestroyedMills, "slice9 step3 one converted cable mill destroyed");
Eq(BlockType.KineticMill, runTrWorld.Chunk.Get(19, 21, 16), "slice9 step3 source-side mill survives while the converted mill is destroyed");
// Mixed-voltage network: a cable touched by both a Dynamo
// (LV) and a transformer output (HV) carries nothing, and
// removing one source resolves it to LV on the next check.
var mixedWorld = new WorldZone(42);
mixedWorld.Chunk.TrySet(18, 21, 16, BlockType.KineticMill);
mixedWorld.Chunk.TrySet(18, 20, 16, BlockType.Cable);
mixedWorld.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
mixedWorld.Chunk.TrySet(16, 20, 16, BlockType.Dynamo);
mixedWorld.Chunk.TrySet(19, 20, 16, BlockType.Dynamo);
Eq(Voltage.None, mixedWorld.DeliveredVoltage(18, 21, 16), "slice9 step2 mixed-voltage cable network delivers nothing");
Check(!mixedWorld.IsPowered(18, 21, 16), "slice9 step2 mixed-voltage mill is not powered");
mixedWorld.Chunk.TrySet(16, 20, 16, BlockType.Air);
Eq(Voltage.LV, mixedWorld.DeliveredVoltage(18, 21, 16), "slice9 step2 mixed network resolves to LV when one source is removed");
// Mill-level conflict: a mill touching an LV cable and an
// HV transformer output at once receives nothing, not the
// first voltage the neighbour scan reaches.
var millMixedWorld = new WorldZone(42);
millMixedWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
millMixedWorld.Chunk.TrySet(15, 20, 16, BlockType.Cable);
millMixedWorld.Chunk.TrySet(14, 20, 16, BlockType.Dynamo);
millMixedWorld.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
millMixedWorld.Chunk.TrySet(18, 20, 16, BlockType.Dynamo);
Eq(Voltage.None, millMixedWorld.DeliveredVoltage(16, 20, 16), "slice9 step2 mill touching LV cable and HV transformer receives nothing");
Check(!millMixedWorld.IsPowered(16, 20, 16), "slice9 step2 mill-level conflict mill is not powered");
// Derived, not stored: a cable run survives save/load as
// chunk bytes and re-derives LV and HV on load.
var saveCableWorld = new WorldZone(42);
saveCableWorld.Chunk.TrySet(15, 4, 15, BlockType.KineticMill);
saveCableWorld.Chunk.TrySet(16, 4, 15, BlockType.Cable);
saveCableWorld.Chunk.TrySet(17, 4, 15, BlockType.Dynamo);
saveCableWorld.Chunk.TrySet(10, 4, 10, BlockType.KineticMill);
saveCableWorld.Chunk.TrySet(11, 4, 10, BlockType.Cable);
saveCableWorld.Chunk.TrySet(12, 4, 10, BlockType.Transformer);
saveCableWorld.Chunk.TrySet(13, 4, 10, BlockType.Dynamo);
var saveCableLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(saveCableWorld));
Eq(BlockType.Cable, saveCableLoaded.Chunk.Get(16, 4, 15), "slice9 step2 cable survives save/load as a chunk byte");
Eq(Voltage.LV, saveCableLoaded.DeliveredVoltage(15, 4, 15), "slice9 step2 loaded LV cable mill re-derives LV from placement");
Eq(Voltage.HV, saveCableLoaded.DeliveredVoltage(10, 4, 10), "slice9 step2 loaded HV cable mill re-derives HV from placement");
Check(saveCableLoaded.IsPowered(15, 4, 15), "slice9 step2 loaded LV cable mill is powered");
Check(!saveCableLoaded.IsPowered(10, 4, 10), "slice9 step2 loaded HV cable mill is not powered");
// HUD: the power count is still rated-voltage only — a
// cable-fed LV mill counts as powered, a cable-fed HV mill
// counts as not, exactly as face-adjacent mills did.
var cableHudWorld = new WorldZone(42);
cableHudWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
cableHudWorld.Chunk.TrySet(17, 20, 16, BlockType.Cable);
cableHudWorld.Chunk.TrySet(18, 20, 16, BlockType.Dynamo);
cableHudWorld.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
cableHudWorld.Chunk.TrySet(11, 20, 10, BlockType.Cable);
cableHudWorld.Chunk.TrySet(12, 20, 10, BlockType.Transformer);
cableHudWorld.Chunk.TrySet(13, 20, 10, BlockType.Dynamo);
var cableHud = HudState.FromWorld(cableHudWorld, new Weapon());
Eq(2, cableHud.TotalMills, "slice9 step2 HUD counts both cable-fed mills");
Eq(1, cableHud.PoweredMills, "slice9 step2 HUD counts only the LV cable mill as powered");
// Simulation aimed builder: same aim as the transformer test.
var simCable = new Simulation(42);
simCable.Tick(0.1f, 0, 0, false);
simCable.World.Player.Inventory.Add(BlockType.EmberOre, 1);
simCable.ApplyLook(0f, -45f);
Eq(BuildResult.Success, simCable.BuildCableAhead(), "slice9 step2 simulation build cable ahead succeeds");
Eq(BlockType.Cable, simCable.World.Chunk.Get(16, 4, 17), "slice9 step2 simulation cable placed by aim");
Eq(0, simCable.World.Player.Inventory.Get(BlockType.EmberOre), "slice9 step2 simulation cable spends its ore");

// ---- Slice 9 Step 3: mismatched-voltage destruction ----
// Decides the scope's open questions in core: the mill is
// rated LV only, so HV delivered through a transformer or a
// cable is a mismatch and destroys the mill on the tick it
// is delivered, in scan order, before either automation
// phase; no voltage delivered (None) is NOT a mismatch, so
// a starved or conflicted mill idles at kinetic cadence
// instead of breaking — on this two-level ladder there is
// no delivered voltage below LV, so undervoltage and no
// delivery are the same observable state, None. Salvage is
// none and cascade is none: the destroyed cell becomes Air,
// nothing is refunded, and its cable, transformer, Dynamo,
// and neighbouring mills survive, each judged on its own
// voltage. Transformers are not destroyed by HV input —
// they convert both levels — and cables are voltage-blind
// carriers that never break. No new adapter binding: this
// is a core rule on a tick, not a key press.
// Cable-run destruction: Dynamo -> transformer -> cable
// -> mill, with a stone pocket the mill would have
// gathered from under the old fallback, and an inventory
// snapshot proving no salvage.
var hvCableWorld = new WorldZone(42);
hvCableWorld.Player.Inventory.Add(BlockType.Stone, 3);
hvCableWorld.Player.Inventory.Add(BlockType.Wood, 2);
hvCableWorld.Player.Inventory.Add(BlockType.EmberOre, 7);
hvCableWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
hvCableWorld.Chunk.TrySet(17, 20, 16, BlockType.Cable);
hvCableWorld.Chunk.TrySet(18, 20, 16, BlockType.Transformer);
hvCableWorld.Chunk.TrySet(19, 20, 16, BlockType.Dynamo);
Eq(Voltage.HV, hvCableWorld.DeliveredVoltage(16, 20, 16), "slice9 step3 cable-run mill receives HV");
Check(hvCableWorld.IsMismatched(16, 20, 16), "slice9 step3 cable-run mill is mismatched");
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    hvCableWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
hvCableWorld.Advance();
Eq(BlockType.Air, hvCableWorld.Chunk.Get(16, 20, 16), "slice9 step3 cable-run mill is destroyed on its first tick");
Eq(1, hvCableWorld.DestroyedMills, "slice9 step3 one cable-run mill destroyed");
Eq(0, hvCableWorld.KineticBlocksGathered, "slice9 step3 cable-run mill gathers nothing before destruction");
Eq(0, hvCableWorld.PoweredBlocksGathered, "slice9 step3 cable-run mill counts nothing powered");
Eq(BlockType.Stone, hvCableWorld.Chunk.Get(15, 20, 16), "slice9 step3 cable-run mill excavates nothing on its destruction tick");
Eq(3, hvCableWorld.Player.Inventory.Get(BlockType.Stone), "slice9 step3 destruction refunds no stone");
Eq(2, hvCableWorld.Player.Inventory.Get(BlockType.Wood), "slice9 step3 destruction refunds no wood");
Eq(7, hvCableWorld.Player.Inventory.Get(BlockType.EmberOre), "slice9 step3 destruction refunds no ore");
Eq(BlockType.Cable, hvCableWorld.Chunk.Get(17, 20, 16), "slice9 step3 cable survives its mill's destruction");
Eq(BlockType.Transformer, hvCableWorld.Chunk.Get(18, 20, 16), "slice9 step3 transformer survives its mill's destruction");
Eq(BlockType.Dynamo, hvCableWorld.Chunk.Get(19, 20, 16), "slice9 step3 dynamo survives its mill's destruction");
// Destruction on a 5th tick: destruction runs before the
// kinetic phase, so a mill that would have gathered
// kinetically on tick 5 is destroyed instead, gathering
// nothing. RestoreTick puts the world at tick 4, so the
// next Advance is tick 5.
var fifthWorld = new WorldZone(42);
fifthWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
fifthWorld.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
fifthWorld.Chunk.TrySet(18, 20, 16, BlockType.Dynamo);
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    fifthWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
fifthWorld.RestoreTick(4);
fifthWorld.Advance();
Eq(5, fifthWorld.Tick, "slice9 step3 fifth-tick world is on tick 5");
Eq(BlockType.Air, fifthWorld.Chunk.Get(16, 20, 16), "slice9 step3 mill is destroyed on the 5th tick, not gathered kinetically");
Eq(0, fifthWorld.KineticBlocksGathered, "slice9 step3 5th-tick destruction gathers nothing kinetic");
// An LV mill beside an HV mill in the same world: the LV
// mill keeps its powered cadence for the full 5 ticks while
// the HV mill is gone after tick 1.
var sideWorld = new WorldZone(42);
sideWorld.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
sideWorld.Chunk.TrySet(11, 20, 10, BlockType.Dynamo);
sideWorld.Chunk.TrySet(20, 20, 20, BlockType.KineticMill);
sideWorld.Chunk.TrySet(21, 20, 20, BlockType.Transformer);
sideWorld.Chunk.TrySet(22, 20, 20, BlockType.Dynamo);
foreach (var (x, y, z) in new[] { (9, 20, 10), (10, 21, 10), (10, 19, 10), (10, 20, 11), (10, 20, 9),
    (19, 20, 20), (20, 21, 20), (20, 19, 20), (20, 20, 21), (20, 20, 19) })
    sideWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) sideWorld.Advance();
Eq(BlockType.KineticMill, sideWorld.Chunk.Get(10, 20, 10), "slice9 step3 LV mill survives beside a destroyed HV mill");
Eq(BlockType.Air, sideWorld.Chunk.Get(20, 20, 20), "slice9 step3 HV mill is destroyed beside a surviving LV mill");
Eq(5, sideWorld.PoweredBlocksGathered, "slice9 step3 LV mill gathers 5 in 5 ticks beside a destroyed mill");
Eq(0, sideWorld.KineticBlocksGathered, "slice9 step3 no kinetic gathers beside the destroyed mill");
Eq(1, sideWorld.DestroyedMills, "slice9 step3 exactly one mill destroyed beside the LV mill");
// Branch, no cascade: one Dynamo feeds a west LV branch
// and, through a transformer, an east HV branch. The east
// mill is destroyed, the west mill keeps gathering, and
// every cable, the transformer, and the Dynamo survive.
var branch3World = new WorldZone(42);
branch3World.Chunk.TrySet(18, 20, 16, BlockType.KineticMill);
branch3World.Chunk.TrySet(19, 20, 16, BlockType.Cable);
branch3World.Chunk.TrySet(20, 20, 16, BlockType.Dynamo);
branch3World.Chunk.TrySet(21, 20, 16, BlockType.Cable);
branch3World.Chunk.TrySet(22, 20, 16, BlockType.Transformer);
branch3World.Chunk.TrySet(23, 20, 16, BlockType.Cable);
branch3World.Chunk.TrySet(24, 20, 16, BlockType.KineticMill);
Eq(Voltage.LV, branch3World.DeliveredVoltage(18, 20, 16), "slice9 step3 branch west mill receives LV");
Eq(Voltage.HV, branch3World.DeliveredVoltage(24, 20, 16), "slice9 step3 branch east mill receives HV");
foreach (var (x, y, z) in new[] { (17, 20, 16), (18, 21, 16), (18, 19, 16), (18, 20, 17), (18, 20, 15),
    (25, 20, 16), (24, 21, 16), (24, 19, 16), (24, 20, 17), (24, 20, 15) })
    branch3World.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) branch3World.Advance();
Eq(BlockType.KineticMill, branch3World.Chunk.Get(18, 20, 16), "slice9 step3 branch west mill survives");
Eq(BlockType.Air, branch3World.Chunk.Get(24, 20, 16), "slice9 step3 branch east mill is destroyed");
Eq(5, branch3World.PoweredBlocksGathered, "slice9 step3 branch west mill gathers 5 in 5 ticks after its branch is destroyed");
Eq(1, branch3World.DestroyedMills, "slice9 step3 branch destruction counts exactly one mill");
Eq(BlockType.Cable, branch3World.Chunk.Get(19, 20, 16), "slice9 step3 branch west cable survives");
Eq(BlockType.Cable, branch3World.Chunk.Get(21, 20, 16), "slice9 step3 branch east cable survives");
Eq(BlockType.Cable, branch3World.Chunk.Get(23, 20, 16), "slice9 step3 branch beyond-transformer cable survives");
Eq(BlockType.Transformer, branch3World.Chunk.Get(22, 20, 16), "slice9 step3 branch transformer survives");
Eq(BlockType.Dynamo, branch3World.Chunk.Get(20, 20, 16), "slice9 step3 branch dynamo survives");
// None is not a mismatch: a conflicted mill and an
// unpowered mill both idle at kinetic cadence and are
// never destroyed, proved over the same 5 ticks.
var noneWorld = new WorldZone(42);
noneWorld.Chunk.TrySet(18, 21, 16, BlockType.KineticMill);
noneWorld.Chunk.TrySet(18, 20, 16, BlockType.Cable);
noneWorld.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
noneWorld.Chunk.TrySet(16, 20, 16, BlockType.Dynamo);
noneWorld.Chunk.TrySet(19, 20, 16, BlockType.Dynamo);
Eq(Voltage.None, noneWorld.DeliveredVoltage(18, 21, 16), "slice9 step3 conflicted mill receives no voltage");
Check(!noneWorld.IsMismatched(18, 21, 16), "slice9 step3 conflicted mill is not mismatched");
foreach (var (x, y, z) in new[] { (17, 21, 16), (19, 21, 16), (18, 22, 16), (18, 21, 17), (18, 21, 15) })
    noneWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) noneWorld.Advance();
Eq(BlockType.KineticMill, noneWorld.Chunk.Get(18, 21, 16), "slice9 step3 conflicted mill survives 5 ticks");
Eq(1, noneWorld.KineticBlocksGathered, "slice9 step3 conflicted mill gathers 1 in 5 ticks at kinetic cadence");
Eq(0, noneWorld.DestroyedMills, "slice9 step3 conflicted mill is never destroyed");
var idleWorld = new WorldZone(42);
idleWorld.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
Check(!idleWorld.IsMismatched(10, 20, 10), "slice9 step3 unpowered mill is not mismatched");
foreach (var (x, y, z) in new[] { (9, 20, 10), (10, 21, 10), (10, 19, 10), (10, 20, 11), (10, 20, 9) })
    idleWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) idleWorld.Advance();
Eq(BlockType.KineticMill, idleWorld.Chunk.Get(10, 20, 10), "slice9 step3 unpowered mill survives 5 ticks");
Eq(1, idleWorld.KineticBlocksGathered, "slice9 step3 unpowered mill gathers 1 in 5 ticks at kinetic cadence");
Eq(0, idleWorld.DestroyedMills, "slice9 step3 unpowered mill is never destroyed");
// Direct-Dynamo precedence still saves a mill: a mill
// touching both a Dynamo (LV) and an HV transformer
// receives LV, is powered, and is never destroyed.
var prec3World = new WorldZone(42);
prec3World.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
prec3World.Chunk.TrySet(15, 20, 16, BlockType.Dynamo);
prec3World.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
prec3World.Chunk.TrySet(18, 20, 16, BlockType.Dynamo);
Eq(Voltage.LV, prec3World.DeliveredVoltage(16, 20, 16), "slice9 step3 direct dynamo still beats HV for its mill");
Check(!prec3World.IsMismatched(16, 20, 16), "slice9 step3 direct-dynamo mill is not mismatched");
foreach (var (x, y, z) in new[] { (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    prec3World.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) prec3World.Advance();
Eq(BlockType.KineticMill, prec3World.Chunk.Get(16, 20, 16), "slice9 step3 direct-dynamo mill survives 5 ticks");
Eq(4, prec3World.PoweredBlocksGathered, "slice9 step3 direct-dynamo mill gathers its four stone neighbours at powered cadence, nothing on tick 5 (its other two faces are the dynamo and transformer)");
Eq(0, prec3World.DestroyedMills, "slice9 step3 direct-dynamo mill is never destroyed");
// A transformer fed HV is not destroyed: in a round-trip
// Dynamo -> T -> T -> mill, the second transformer is fed
// HV, converts it back to LV, and survives with its mill.
var trSurviveWorld = new WorldZone(42);
trSurviveWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
trSurviveWorld.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
trSurviveWorld.Chunk.TrySet(18, 20, 16, BlockType.Transformer);
trSurviveWorld.Chunk.TrySet(19, 20, 16, BlockType.Dynamo);
Check(!trSurviveWorld.IsMismatched(17, 20, 16), "slice9 step3 a transformer cell is never mismatched");
Check(!trSurviveWorld.IsMismatched(18, 20, 16), "slice9 step3 the HV-fed transformer is never mismatched");
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    trSurviveWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
for (int i = 0; i < 5; i++) trSurviveWorld.Advance();
Eq(BlockType.KineticMill, trSurviveWorld.Chunk.Get(16, 20, 16), "slice9 step3 round-trip mill survives");
Eq(BlockType.Transformer, trSurviveWorld.Chunk.Get(17, 20, 16), "slice9 step3 first transformer survives");
Eq(BlockType.Transformer, trSurviveWorld.Chunk.Get(18, 20, 16), "slice9 step3 HV-fed transformer survives");
Eq(5, trSurviveWorld.PoweredBlocksGathered, "slice9 step3 round-trip mill gathers 5 in 5 ticks");
Eq(0, trSurviveWorld.DestroyedMills, "slice9 step3 round-trip destroys nothing");
// Save/load: a destroyed mill stays destroyed, with no
// resurrection and no refund on load, while the surviving
// LV mill and all wiring survive; only the session
// counter resets, like the gather counters.
var save3World = new WorldZone(42);
save3World.Player.Inventory.Add(BlockType.Stone, 3);
save3World.Player.Inventory.Add(BlockType.Wood, 2);
save3World.Player.Inventory.Add(BlockType.EmberOre, 7);
save3World.Chunk.TrySet(15, 4, 15, BlockType.KineticMill);
save3World.Chunk.TrySet(16, 4, 15, BlockType.Cable);
save3World.Chunk.TrySet(17, 4, 15, BlockType.Transformer);
save3World.Chunk.TrySet(18, 4, 15, BlockType.Dynamo);
save3World.Chunk.TrySet(10, 4, 10, BlockType.KineticMill);
save3World.Chunk.TrySet(11, 4, 10, BlockType.Cable);
save3World.Chunk.TrySet(12, 4, 10, BlockType.Dynamo);
// Clear the surviving mill's other neighbours to Air, so its
// powered tick before the save gathers nothing and the stone
// count below proves no refund, not a gather minus a refund.
foreach (var (x, y, z) in new[] { (9, 4, 10), (10, 5, 10), (10, 3, 10), (10, 4, 11), (10, 4, 9) })
    save3World.Chunk.TrySet(x, y, z, BlockType.Air);
save3World.Advance();
Eq(1, save3World.DestroyedMills, "slice9 step3 mill destroyed before save");
var save3Loaded = SaveSystem.LoadZone(SaveSystem.SaveZone(save3World));
Eq(BlockType.Air, save3Loaded.Chunk.Get(15, 4, 15), "slice9 step3 destroyed mill stays destroyed after load");
Eq(BlockType.KineticMill, save3Loaded.Chunk.Get(10, 4, 10), "slice9 step3 surviving LV mill survives load");
Eq(BlockType.Cable, save3Loaded.Chunk.Get(16, 4, 15), "slice9 step3 destroyed mill's cable survives load");
Eq(BlockType.Transformer, save3Loaded.Chunk.Get(17, 4, 15), "slice9 step3 destroyed mill's transformer survives load");
Eq(BlockType.Dynamo, save3Loaded.Chunk.Get(18, 4, 15), "slice9 step3 destroyed mill's dynamo survives load");
Eq(3, save3Loaded.Player.Inventory.Get(BlockType.Stone), "slice9 step3 load gives no stone refund for the destroyed mill");
Eq(2, save3Loaded.Player.Inventory.Get(BlockType.Wood), "slice9 step3 load gives no wood refund for the destroyed mill");
Eq(7, save3Loaded.Player.Inventory.Get(BlockType.EmberOre), "slice9 step3 load gives no ore refund for the destroyed mill");
Eq(0, save3Loaded.DestroyedMills, "slice9 step3 destruction counter resets on load like the gather counters");
Eq(Voltage.LV, save3Loaded.DeliveredVoltage(10, 4, 10), "slice9 step3 surviving LV mill still receives LV after load");
Check(save3Loaded.IsPowered(10, 4, 10), "slice9 step3 surviving LV mill is still powered after load");
// HUD after destruction: TotalMills drops with the mill,
// so the power line already reflects the loss without a
// new HUD line in this step.
var hud3World = new WorldZone(42);
hud3World.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
hud3World.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
hud3World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
hud3World.Chunk.TrySet(11, 20, 10, BlockType.Transformer);
hud3World.Chunk.TrySet(12, 20, 10, BlockType.Dynamo);
var hud3Before = HudState.FromWorld(hud3World, new Weapon());
Eq(2, hud3Before.TotalMills, "slice9 step3 HUD counts both mills before destruction");
Eq(1, hud3Before.PoweredMills, "slice9 step3 HUD counts only the LV mill as powered before destruction");
hud3World.Advance();
var hud3After = HudState.FromWorld(hud3World, new Weapon());
Eq(1, hud3After.TotalMills, "slice9 step3 HUD total drops to one mill after destruction");
Eq(1, hud3After.PoweredMills, "slice9 step3 HUD powered count is unchanged after destruction");

// ---- Slice 9 Step 4: composed demo scenario (0.43.0) ----
// Mirrors the Slice 9 demo scenario step for step: one
// Simulation with four mills on the y=20 plane — an LV
// cable mill, a round-trip LV mill (two transformers),
// an HV cable mill destroyed on tick 1, and a kinetic
// mill — then a save/load round-trip and a loaded
// 5-tick phase. The scope's Step 4 wording said the
// second mill "runs at the converted voltage"; after
// Step 3 that cannot mean surviving on HV, so the demo
// proves converted voltage two honest ways: row B
// survives on HV converted back to LV, and row C is
// destroyed by HV itself. The layout is staged with
// TrySet exactly as the Step 1-3 tests stage theirs;
// aim placement of a transformer and of a cable was
// each proved in its own step's tests.
var s9Demo = new Simulation(42);
void SeedPocketS9Test(WorldZone w, int mx, int mz)
{
    foreach (var (x, y, z) in new[] { (mx - 1, 20, mz), (mx, 21, mz), (mx, 19, mz), (mx, 20, mz - 1), (mx, 20, mz + 1) })
        w.Chunk.TrySet(x, y, z, BlockType.Stone);
}
s9Demo.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
s9Demo.World.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s9Demo.World.Chunk.TrySet(12, 20, 10, BlockType.Cable);
s9Demo.World.Chunk.TrySet(13, 20, 10, BlockType.Dynamo);
s9Demo.World.Chunk.TrySet(10, 20, 14, BlockType.KineticMill);
s9Demo.World.Chunk.TrySet(11, 20, 14, BlockType.Transformer);
s9Demo.World.Chunk.TrySet(12, 20, 14, BlockType.Transformer);
s9Demo.World.Chunk.TrySet(13, 20, 14, BlockType.Dynamo);
s9Demo.World.Chunk.TrySet(10, 20, 18, BlockType.KineticMill);
s9Demo.World.Chunk.TrySet(11, 20, 18, BlockType.Cable);
s9Demo.World.Chunk.TrySet(12, 20, 18, BlockType.Transformer);
s9Demo.World.Chunk.TrySet(13, 20, 18, BlockType.Dynamo);
s9Demo.World.Chunk.TrySet(10, 20, 22, BlockType.KineticMill);
SeedPocketS9Test(s9Demo.World, 10, 10);
SeedPocketS9Test(s9Demo.World, 10, 14);
SeedPocketS9Test(s9Demo.World, 10, 18);
SeedPocketS9Test(s9Demo.World, 10, 22);
Eq(Voltage.LV, s9Demo.World.DeliveredVoltage(10, 20, 10), "slice9 step4 composed cable mill receives LV");
Eq(Voltage.LV, s9Demo.World.DeliveredVoltage(10, 20, 14), "slice9 step4 composed round-trip mill receives LV");
Eq(Voltage.HV, s9Demo.World.DeliveredVoltage(10, 20, 18), "slice9 step4 composed HV cable mill receives HV");
Eq(Voltage.None, s9Demo.World.DeliveredVoltage(10, 20, 22), "slice9 step4 composed kinetic mill receives nothing");
var s9DemoHudBefore = s9Demo.Hud();
Eq(4, s9DemoHudBefore.TotalMills, "slice9 step4 composed HUD counts 4 mills before destruction");
Eq(2, s9DemoHudBefore.PoweredMills, "slice9 step4 composed HUD counts only the 2 LV mills as powered");
s9Demo.World.Advance();
Eq(BlockType.Air, s9Demo.World.Chunk.Get(10, 20, 18), "slice9 step4 composed HV mill destroyed on tick 1");
Eq(1, s9Demo.World.DestroyedMills, "slice9 step4 composed destruction counter is 1 after tick 1");
Eq(2, s9Demo.World.PoweredBlocksGathered, "slice9 step4 composed powered mills gather 2 on tick 1");
Eq(BlockType.Stone, s9Demo.World.Chunk.Get(9, 20, 18), "slice9 step4 composed destroyed mill excavates nothing: its pocket stone survives");
Eq(BlockType.Cable, s9Demo.World.Chunk.Get(11, 20, 18), "slice9 step4 composed destroyed mill's cable survives");
Eq(BlockType.Transformer, s9Demo.World.Chunk.Get(12, 20, 18), "slice9 step4 composed destroyed mill's transformer survives");
Eq(BlockType.Dynamo, s9Demo.World.Chunk.Get(13, 20, 18), "slice9 step4 composed destroyed mill's Dynamo survives");
var s9DemoHudAfter1 = s9Demo.Hud();
Eq(3, s9DemoHudAfter1.TotalMills, "slice9 step4 composed HUD total drops to 3 after destruction");
Eq(2, s9DemoHudAfter1.PoweredMills, "slice9 step4 composed HUD powered count stays 2 after destruction");
for (int i = 0; i < 4; i++) s9Demo.World.Advance();
Eq(10, s9Demo.World.PoweredBlocksGathered, "slice9 step4 composed powered mills gather 5 + 5 in 5 ticks");
Eq(1, s9Demo.World.KineticBlocksGathered, "slice9 step4 composed kinetic mill gathers 1 in 5 ticks");
Eq(1, s9Demo.World.DestroyedMills, "slice9 step4 composed destruction counter stays 1 through 5 ticks");
Eq(11, s9Demo.World.Player.Inventory.Get(BlockType.Stone), "slice9 step4 composed stone is 5 + 5 + 1, no refund for the destroyed mill");
var s9DemoSave = s9Demo.SaveToJson();
var s9DemoLoaded = new Simulation(99);
Check(s9DemoLoaded.LoadFromJson(s9DemoSave), "slice9 step4 composed save loads");
Eq(BlockType.KineticMill, s9DemoLoaded.World.Chunk.Get(10, 20, 10), "slice9 step4 composed cable mill survives load");
Eq(BlockType.KineticMill, s9DemoLoaded.World.Chunk.Get(10, 20, 14), "slice9 step4 composed round-trip mill survives load");
Eq(BlockType.Air, s9DemoLoaded.World.Chunk.Get(10, 20, 18), "slice9 step4 composed destroyed mill stays destroyed after load");
Eq(BlockType.KineticMill, s9DemoLoaded.World.Chunk.Get(10, 20, 22), "slice9 step4 composed kinetic mill survives load");
Eq(Voltage.LV, s9DemoLoaded.World.DeliveredVoltage(10, 20, 10), "slice9 step4 composed cable mill re-derives LV after load");
Eq(Voltage.LV, s9DemoLoaded.World.DeliveredVoltage(10, 20, 14), "slice9 step4 composed round-trip mill re-derives LV after load");
Eq(0, s9DemoLoaded.World.PoweredBlocksGathered, "slice9 step4 composed powered counter resets on load");
Eq(0, s9DemoLoaded.World.KineticBlocksGathered, "slice9 step4 composed kinetic counter resets on load");
Eq(0, s9DemoLoaded.World.DestroyedMills, "slice9 step4 composed destruction counter resets on load");
Eq(11, s9DemoLoaded.World.Player.Inventory.Get(BlockType.Stone), "slice9 step4 composed load gives no stone refund for the destroyed mill");
var s9DemoLoadedHud = s9DemoLoaded.Hud();
Eq(3, s9DemoLoadedHud.TotalMills, "slice9 step4 composed HUD counts 3 mills after load");
Eq(2, s9DemoLoadedHud.PoweredMills, "slice9 step4 composed HUD counts 2 powered mills after load");
SeedPocketS9Test(s9DemoLoaded.World, 10, 10);
SeedPocketS9Test(s9DemoLoaded.World, 10, 14);
for (int i = 0; i < 5; i++) s9DemoLoaded.World.Advance();
Eq(10, s9DemoLoaded.World.PoweredBlocksGathered, "slice9 step4 composed loaded mills gather 5 + 5 again in 5 ticks");
Eq(1, s9DemoLoaded.World.KineticBlocksGathered, "slice9 step4 composed loaded kinetic mill gathers 1 again in 5 ticks");
Eq(0, s9DemoLoaded.World.DestroyedMills, "slice9 step4 composed loaded world destroys nothing new");
Eq(22, s9DemoLoaded.World.Player.Inventory.Get(BlockType.Stone), "slice9 step4 composed loaded stone reaches 11 + 11");

// ---- Slice 10 Step 1: powered Workstation, second powered consumer ----
// Rating decided, not inherited: LV via a face-adjacent Dynamo powers
// a workstation; the same workstation with no source is unpowered.
var pwWorld = new WorldZone(42);
pwWorld.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
pwWorld.Chunk.TrySet(18, 5, 16, BlockType.Dynamo);
Check(pwWorld.IsWorkstationPowered(17, 5, 16), "slice10 LV-fed workstation is powered at its own rating");
Check(!pwWorld.IsWorkstationMismatched(17, 5, 16), "slice10 LV-fed workstation is not mismatched");
var unpowWorld = new WorldZone(42);
unpowWorld.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
Check(!unpowWorld.IsWorkstationPowered(17, 5, 16), "slice10 unpowered workstation is not powered");
Check(!unpowWorld.IsWorkstationMismatched(17, 5, 16), "slice10 unpowered workstation is not mismatched");
// HV via one transformer step: Dynamo -> transformer -> workstation
// delivers HV, so it is mismatched, not powered — and, unlike a mill,
// it is NOT destroyed on its tick, it is disabled instead.
var hvWsWorld = new WorldZone(42);
hvWsWorld.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
hvWsWorld.Chunk.TrySet(18, 5, 16, BlockType.Transformer);
hvWsWorld.Chunk.TrySet(19, 5, 16, BlockType.Dynamo);
Eq(Voltage.HV, hvWsWorld.DeliveredVoltage(17, 5, 16), "slice10 transformer steps dynamo LV up to HV for its workstation");
Check(!hvWsWorld.IsWorkstationPowered(17, 5, 16), "slice10 HV-fed workstation is not powered");
Check(hvWsWorld.IsWorkstationMismatched(17, 5, 16), "slice10 HV-fed workstation is mismatched before its tick");
hvWsWorld.Player.TakeDamage(20);
hvWsWorld.Advance();
Eq(BlockType.Workstation, hvWsWorld.Chunk.Get(17, 5, 16), "slice10 HV-fed workstation survives its tick, unlike a mill");
Eq(65, hvWsWorld.Player.Shields, "slice10 HV-fed workstation restores nothing: disabled, not boosted, not destroyed");
// Regen: unpowered = base 5 (existing rule, unchanged); powered =
// base + 5 powered bonus; powered + skill bonus stacks additively.
var pwRegenWorld = new WorldZone(42);
pwRegenWorld.Player.TakeDamage(20);
pwRegenWorld.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
pwRegenWorld.Chunk.TrySet(18, 5, 16, BlockType.Dynamo);
Eq(10, pwRegenWorld.EffectiveWorkstationRegen(), "slice10 powered workstation effective regen is base 5 + powered 5");
pwRegenWorld.Advance();
Eq(75, pwRegenWorld.Player.Shields, "slice10 powered workstation restores 10 shields per tick");
var pwSkillWorld = new WorldZone(42);
pwSkillWorld.WorkstationRegenPerTick = 10; // skill bonus, as ApplySkills sets it
pwSkillWorld.Player.TakeDamage(30);
pwSkillWorld.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
pwSkillWorld.Chunk.TrySet(18, 5, 16, BlockType.Dynamo);
Eq(15, pwSkillWorld.EffectiveWorkstationRegen(), "slice10 powered + skill regen stacks: 10 skill rate + 5 powered");
pwSkillWorld.Advance();
Eq(70, pwSkillWorld.Player.Shields, "slice10 powered + skill workstation restores 15 shields per tick");
// Powered workstation recovers when rewired: HV (disabled) -> LV
// (powered) on the same cell, with no rebuild and no salvage.
hvWsWorld.Chunk.TrySet(18, 5, 16, BlockType.Dynamo);
hvWsWorld.Chunk.TrySet(19, 5, 16, BlockType.Air);
Check(hvWsWorld.IsWorkstationPowered(17, 5, 16), "slice10 rewired workstation is powered again after HV source removed");
hvWsWorld.Advance();
Eq(75, hvWsWorld.Player.Shields, "slice10 rewired workstation restores 10 shields per tick again");
// Aimed builder: same aim as the Slice 5 demo mill build, with
// workstation inventory (10 Stone + 5 EmberOre), places a workstation.
var simWs = new Simulation(42);
simWs.Tick(0.1f, 0, 0, false);
simWs.World.Player.Inventory.Add(BlockType.Stone, 10);
simWs.World.Player.Inventory.Add(BlockType.EmberOre, 5);
simWs.ApplyLook(-18.4f, -53f);
Eq(BuildResult.Success, simWs.BuildWorkstationAhead(), "slice10 simulation build workstation ahead succeeds");
Eq(BlockType.Workstation, simWs.World.Chunk.Get(15, 4, 17), "slice10 simulation workstation placed by aim");
Eq(0, simWs.World.Player.Inventory.Get(BlockType.EmberOre), "slice10 simulation workstation spends its ore");
// HUD second-consumer count: powered = LV only; HV counts as not
// powered; unpowered counts as not powered.
var hudPw = new Simulation(42);
hudPw.World.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
hudPw.World.Chunk.TrySet(18, 5, 16, BlockType.Dynamo);
var hudPwState = hudPw.Hud();
Eq(1, hudPwState.TotalWorkstations, "slice10 HUD counts 1 workstation");
Eq(1, hudPwState.PoweredWorkstations, "slice10 HUD counts LV workstation as powered");
var hudHv = new Simulation(42);
hudHv.World.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
hudHv.World.Chunk.TrySet(18, 5, 16, BlockType.Transformer);
hudHv.World.Chunk.TrySet(19, 5, 16, BlockType.Dynamo);
var hudHvState = hudHv.Hud();
Eq(1, hudHvState.TotalWorkstations, "slice10 HUD counts HV workstation in total");
Eq(0, hudHvState.PoweredWorkstations, "slice10 HUD counts HV workstation as NOT powered");

// ---- Slice 10 Step 2: Alternator, second power source ----
// The source decided in this step: a native HV generator, not
// a Dynamo reskin. Its output level, its stone-only price, its
// place in DeliveredVoltage precedence, and its proof against
// BOTH consumers (mill and workstation) through face-adjacency
// and through cable are all decided here, with tests.
Eq(11, (byte)BlockType.Alternator, "slice10 step2 alternator block type appended as 11");
Eq(8, (byte)BlockType.Dynamo, "slice10 step2 dynamo value unchanged at 8");
Eq(9, (byte)BlockType.Transformer, "slice10 step2 transformer value unchanged at 9");
Eq(10, (byte)BlockType.Cable, "slice10 step2 cable value unchanged at 10");
Eq(Voltage.HV, VoltageRules.AlternatorOutput, "slice10 step2 alternator outputs HV, not the dynamo's LV");
// Build: 10 Stone, no EmberOre, check-before-spend, support
// rule, not gatherable.
var altWorld = new WorldZone(42);
altWorld.Player.Inventory.Add(BlockType.Stone, 10);
Eq(BuildResult.Success, altWorld.Build(15, 4, 15, BlockType.Alternator), "slice10 step2 alternator builds with 10 stone");
Eq(BlockType.Alternator, altWorld.Chunk.Get(15, 4, 15), "slice10 step2 alternator placed");
Eq(0, altWorld.Player.Inventory.Get(BlockType.Stone), "slice10 step2 alternator consumes its stone");
Eq(0, altWorld.Player.Inventory.Get(BlockType.EmberOre), "slice10 step2 alternator spends no ore");
Eq(GatherResult.NotGatherable, altWorld.Gather(15, 4, 15), "slice10 step2 alternator itself is not gatherable");
var poorAltWorld = new WorldZone(42);
Eq(BuildResult.InsufficientResources, poorAltWorld.Build(15, 4, 15, BlockType.Alternator), "slice10 step2 alternator without stone fails");
Eq(BlockType.Air, poorAltWorld.Chunk.Get(15, 4, 15), "slice10 step2 failed alternator leaves air");
var floatAltWorld = new WorldZone(42);
floatAltWorld.Player.Inventory.Add(BlockType.Stone, 10);
Eq(BuildResult.Floating, floatAltWorld.Build(16, 20, 16, BlockType.Alternator), "slice10 step2 alternator floating in air fails like other machines");
// Direct HV destroys a mill: the native-HV proof, and the
// reason safe use needs a transformer. The mill excavates
// nothing on its destruction tick, and its pocket survives.
var altDirectWorld = new WorldZone(42);
altDirectWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
altDirectWorld.Chunk.TrySet(17, 20, 16, BlockType.Alternator);
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    altDirectWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
Eq(Voltage.HV, altDirectWorld.DeliveredVoltage(16, 20, 16), "slice10 step2 direct alternator delivers HV to its mill");
Check(altDirectWorld.IsMismatched(16, 20, 16), "slice10 step2 direct alternator mill is mismatched");
Check(!altDirectWorld.IsPowered(16, 20, 16), "slice10 step2 direct alternator mill is not powered");
altDirectWorld.Advance();
Eq(BlockType.Air, altDirectWorld.Chunk.Get(16, 20, 16), "slice10 step2 direct alternator mill destroyed on tick 1");
Eq(1, altDirectWorld.DestroyedMills, "slice10 step2 direct alternator destruction counter is 1");
Eq(0, altDirectWorld.PoweredBlocksGathered, "slice10 step2 destroyed mill gathers nothing powered");
Eq(0, altDirectWorld.KineticBlocksGathered, "slice10 step2 destroyed mill gathers nothing kinetic");
Eq(BlockType.Stone, altDirectWorld.Chunk.Get(15, 20, 16), "slice10 step2 destroyed mill excavates nothing: its pocket stone survives");
Eq(BlockType.Alternator, altDirectWorld.Chunk.Get(17, 20, 16), "slice10 step2 destroying a mill does not destroy its alternator");
// Face-adjacent step-down, mill: Alternator -> transformer ->
// mill runs at LV, every tick, like a Dynamo-fed mill.
var altStepMillWorld = new WorldZone(42);
altStepMillWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
altStepMillWorld.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
altStepMillWorld.Chunk.TrySet(18, 20, 16, BlockType.Alternator);
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    altStepMillWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
Eq(Voltage.LV, altStepMillWorld.DeliveredVoltage(16, 20, 16), "slice10 step2 alternator stepped down through a transformer delivers LV to its mill");
Check(altStepMillWorld.IsPowered(16, 20, 16), "slice10 step2 stepped-down alternator mill is powered");
for (int i = 0; i < 5; i++) altStepMillWorld.Advance();
Eq(5, altStepMillWorld.PoweredBlocksGathered, "slice10 step2 stepped-down mill gathers 5 in 5 ticks at powered cadence");
Eq(0, altStepMillWorld.KineticBlocksGathered, "slice10 step2 stepped-down mill counts nothing kinetic");
Eq(BlockType.Alternator, altStepMillWorld.Chunk.Get(18, 20, 16), "slice10 step2 powered mill never excavates its alternator");
Eq(BlockType.Transformer, altStepMillWorld.Chunk.Get(17, 20, 16), "slice10 step2 powered mill never excavates its transformer");
// Face-adjacent step-down, workstation: the second consumer
// proved against the new source, not just the mill.
var altStepWsWorld = new WorldZone(42);
altStepWsWorld.Player.TakeDamage(20);
altStepWsWorld.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
altStepWsWorld.Chunk.TrySet(18, 5, 16, BlockType.Transformer);
altStepWsWorld.Chunk.TrySet(19, 5, 16, BlockType.Alternator);
Eq(Voltage.LV, altStepWsWorld.DeliveredVoltage(17, 5, 16), "slice10 step2 alternator stepped down delivers LV to its workstation");
Check(altStepWsWorld.IsWorkstationPowered(17, 5, 16), "slice10 step2 stepped-down alternator workstation is powered");
Check(!altStepWsWorld.IsWorkstationMismatched(17, 5, 16), "slice10 step2 stepped-down alternator workstation is not mismatched");
Eq(10, altStepWsWorld.EffectiveWorkstationRegen(), "slice10 step2 stepped-down workstation effective regen is base 5 + powered 5");
altStepWsWorld.Advance();
Eq(75, altStepWsWorld.Player.Shields, "slice10 step2 stepped-down workstation restores 10 shields per tick");
// Direct HV disables a workstation, exactly as Slice 10 Step 1
// decided: it survives, it restores nothing, it is not powered.
var altDirectWsWorld = new WorldZone(42);
altDirectWsWorld.Player.TakeDamage(20);
altDirectWsWorld.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
altDirectWsWorld.Chunk.TrySet(18, 5, 16, BlockType.Alternator);
Eq(Voltage.HV, altDirectWsWorld.DeliveredVoltage(17, 5, 16), "slice10 step2 direct alternator delivers HV to its workstation");
Check(!altDirectWsWorld.IsWorkstationPowered(17, 5, 16), "slice10 step2 direct alternator workstation is not powered");
Check(altDirectWsWorld.IsWorkstationMismatched(17, 5, 16), "slice10 step2 direct alternator workstation is mismatched");
Eq(0, altDirectWsWorld.EffectiveWorkstationRegen(), "slice10 step2 direct alternator workstation effective regen is 0: disabled");
altDirectWsWorld.Advance();
Eq(BlockType.Workstation, altDirectWsWorld.Chunk.Get(17, 5, 16), "slice10 step2 direct alternator workstation survives its tick, unlike a mill");
Eq(65, altDirectWsWorld.Player.Shields, "slice10 step2 direct alternator workstation restores nothing");
// Cable step-down, mill: Alternator -> cable -> transformer ->
// cable -> mill runs at LV through a cable run, so the source
// is proved through distribution, not only face-adjacency.
var altCableMillWorld = new WorldZone(42);
altCableMillWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
altCableMillWorld.Chunk.TrySet(17, 20, 16, BlockType.Cable);
altCableMillWorld.Chunk.TrySet(18, 20, 16, BlockType.Transformer);
altCableMillWorld.Chunk.TrySet(19, 20, 16, BlockType.Cable);
altCableMillWorld.Chunk.TrySet(20, 20, 16, BlockType.Alternator);
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    altCableMillWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
Eq(Voltage.LV, altCableMillWorld.DeliveredVoltage(16, 20, 16), "slice10 step2 alternator through cable and transformer delivers LV to its mill");
Check(altCableMillWorld.IsPowered(16, 20, 16), "slice10 step2 cable stepped-down alternator mill is powered");
for (int i = 0; i < 5; i++) altCableMillWorld.Advance();
Eq(5, altCableMillWorld.PoweredBlocksGathered, "slice10 step2 cable stepped-down mill gathers 5 in 5 ticks");
Eq(BlockType.Cable, altCableMillWorld.Chunk.Get(17, 20, 16), "slice10 step2 cable mill never excavates its cable");
Eq(BlockType.Alternator, altCableMillWorld.Chunk.Get(20, 20, 16), "slice10 step2 cable mill never excavates its alternator");
// Cable direct HV destroys a mill, as face-adjacent HV does.
var altCableHvWorld = new WorldZone(42);
altCableHvWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
altCableHvWorld.Chunk.TrySet(17, 20, 16, BlockType.Cable);
altCableHvWorld.Chunk.TrySet(18, 20, 16, BlockType.Cable);
altCableHvWorld.Chunk.TrySet(19, 20, 16, BlockType.Alternator);
Eq(Voltage.HV, altCableHvWorld.DeliveredVoltage(16, 20, 16), "slice10 step2 alternator through cable delivers HV to its mill");
altCableHvWorld.Advance();
Eq(BlockType.Air, altCableHvWorld.Chunk.Get(16, 20, 16), "slice10 step2 cable HV mill destroyed on tick 1");
Eq(1, altCableHvWorld.DestroyedMills, "slice10 step2 cable HV destruction counter is 1");
// Cable step-down, workstation: the second consumer proved
// through cable as well as face-adjacency.
var altCableWsWorld = new WorldZone(42);
altCableWsWorld.Player.TakeDamage(20);
altCableWsWorld.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
altCableWsWorld.Chunk.TrySet(18, 5, 16, BlockType.Cable);
altCableWsWorld.Chunk.TrySet(19, 5, 16, BlockType.Transformer);
altCableWsWorld.Chunk.TrySet(20, 5, 16, BlockType.Cable);
altCableWsWorld.Chunk.TrySet(21, 5, 16, BlockType.Alternator);
Eq(Voltage.LV, altCableWsWorld.DeliveredVoltage(17, 5, 16), "slice10 step2 alternator through cable and transformer delivers LV to its workstation");
Check(altCableWsWorld.IsWorkstationPowered(17, 5, 16), "slice10 step2 cable stepped-down alternator workstation is powered");
altCableWsWorld.Advance();
Eq(75, altCableWsWorld.Player.Shields, "slice10 step2 cable stepped-down workstation restores 10 shields per tick");
// Precedence, direct conflict: a mill touching a Dynamo and
// an Alternator receives None — a visible dead mill, not a
// silent Dynamo win and not a destruction.
var altConflictWorld = new WorldZone(42);
altConflictWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
altConflictWorld.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
altConflictWorld.Chunk.TrySet(15, 20, 16, BlockType.Alternator);
foreach (var (x, y, z) in new[] { (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    altConflictWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
Eq(Voltage.None, altConflictWorld.DeliveredVoltage(16, 20, 16), "slice10 step2 dynamo plus alternator on one mill delivers nothing");
Check(!altConflictWorld.IsMismatched(16, 20, 16), "slice10 step2 direct-conflict mill is not mismatched");
Check(!altConflictWorld.IsPowered(16, 20, 16), "slice10 step2 direct-conflict mill is not powered");
for (int i = 0; i < 5; i++) altConflictWorld.Advance();
Eq(BlockType.KineticMill, altConflictWorld.Chunk.Get(16, 20, 16), "slice10 step2 direct-conflict mill survives 5 ticks");
Eq(1, altConflictWorld.KineticBlocksGathered, "slice10 step2 direct-conflict mill falls back to kinetic cadence, 1 in 5 ticks");
Eq(0, altConflictWorld.PoweredBlocksGathered, "slice10 step2 direct-conflict mill counts nothing powered");
// Precedence, direct beats converted: a mill touching a direct
// Alternator (HV) and a transformer outputting LV receives HV,
// so the direct source wins over the conversion, and the mill
// is destroyed — the direct rule is source-neutral, not a
// Dynamo-only privilege.
var altDirectBeatsWorld = new WorldZone(42);
altDirectBeatsWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
altDirectBeatsWorld.Chunk.TrySet(17, 20, 16, BlockType.Alternator);
altDirectBeatsWorld.Chunk.TrySet(18, 20, 16, BlockType.Transformer);
altDirectBeatsWorld.Chunk.TrySet(19, 20, 16, BlockType.Alternator);
Eq(Voltage.HV, altDirectBeatsWorld.DeliveredVoltage(16, 20, 16), "slice10 step2 direct alternator beats converted LV for its mill");
altDirectBeatsWorld.Advance();
Eq(BlockType.Air, altDirectBeatsWorld.Chunk.Get(16, 20, 16), "slice10 step2 direct-beats-converted mill destroyed on tick 1");
Eq(BlockType.Transformer, altDirectBeatsWorld.Chunk.Get(18, 20, 16), "slice10 step2 direct-beats-converted transformer survives");
// Precedence, transformer conflict: a transformer touching a
// Dynamo and an Alternator converts nothing, so the mill it
// feeds idles at kinetic cadence instead of guessing a level.
var altTrConflictWorld = new WorldZone(42);
altTrConflictWorld.Chunk.TrySet(18, 21, 16, BlockType.KineticMill);
altTrConflictWorld.Chunk.TrySet(18, 20, 16, BlockType.Transformer);
altTrConflictWorld.Chunk.TrySet(19, 20, 16, BlockType.Dynamo);
altTrConflictWorld.Chunk.TrySet(17, 20, 16, BlockType.Alternator);
foreach (var (x, y, z) in new[] { (17, 21, 16), (19, 21, 16), (18, 22, 16), (18, 21, 17), (18, 21, 15) })
    altTrConflictWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
Eq(Voltage.None, altTrConflictWorld.DeliveredVoltage(18, 21, 16), "slice10 step2 dynamo plus alternator transformer outputs nothing");
Check(!altTrConflictWorld.IsPowered(18, 21, 16), "slice10 step2 transformer-conflict mill is not powered");
for (int i = 0; i < 5; i++) altTrConflictWorld.Advance();
Eq(BlockType.KineticMill, altTrConflictWorld.Chunk.Get(18, 21, 16), "slice10 step2 transformer-conflict mill survives 5 ticks");
Eq(1, altTrConflictWorld.KineticBlocksGathered, "slice10 step2 transformer-conflict mill falls back to kinetic cadence, 1 in 5 ticks");
// Precedence, cable conflict: one network touched by a Dynamo
// and an Alternator carries nothing, like any mixed network.
var altCableConflictWorld = new WorldZone(42);
altCableConflictWorld.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
altCableConflictWorld.Chunk.TrySet(17, 20, 16, BlockType.Cable);
altCableConflictWorld.Chunk.TrySet(18, 20, 16, BlockType.Cable);
altCableConflictWorld.Chunk.TrySet(17, 21, 16, BlockType.Dynamo);
altCableConflictWorld.Chunk.TrySet(18, 21, 16, BlockType.Alternator);
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    altCableConflictWorld.Chunk.TrySet(x, y, z, BlockType.Stone);
Eq(Voltage.None, altCableConflictWorld.DeliveredVoltage(16, 20, 16), "slice10 step2 dynamo plus alternator cable network delivers nothing");
for (int i = 0; i < 5; i++) altCableConflictWorld.Advance();
Eq(BlockType.KineticMill, altCableConflictWorld.Chunk.Get(16, 20, 16), "slice10 step2 cable-conflict mill survives 5 ticks");
Eq(1, altCableConflictWorld.KineticBlocksGathered, "slice10 step2 cable-conflict mill falls back to kinetic cadence, 1 in 5 ticks");
// Aimed builder: same aim as the Slice 10 Step 1 workstation
// build, with alternator inventory (10 Stone, no ore).
var simAlt = new Simulation(42);
simAlt.Tick(0.1f, 0, 0, false);
simAlt.World.Player.Inventory.Add(BlockType.Stone, 10);
simAlt.ApplyLook(-18.4f, -53f);
Eq(BuildResult.Success, simAlt.BuildAlternatorAhead(), "slice10 step2 simulation build alternator ahead succeeds");
Eq(BlockType.Alternator, simAlt.World.Chunk.Get(15, 4, 17), "slice10 step2 simulation alternator placed by aim");
Eq(0, simAlt.World.Player.Inventory.Get(BlockType.Stone), "slice10 step2 simulation alternator spends its stone");
// Derived, not stored: an Alternator and its step-down chains
// survive save/load as chunk bytes and re-derive LV on load,
// with no stored voltage or source flag.
var altSaveWorld = new WorldZone(42);
altSaveWorld.Chunk.TrySet(15, 4, 15, BlockType.KineticMill);
altSaveWorld.Chunk.TrySet(16, 4, 15, BlockType.Transformer);
altSaveWorld.Chunk.TrySet(17, 4, 15, BlockType.Alternator);
altSaveWorld.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
altSaveWorld.Chunk.TrySet(18, 5, 16, BlockType.Transformer);
altSaveWorld.Chunk.TrySet(19, 5, 16, BlockType.Alternator);
var altSaveLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(altSaveWorld));
Check(altSaveLoaded.Chunk.ToBytes().SequenceEqual(altSaveWorld.Chunk.ToBytes()), "slice10 step2 save/load chunk equal with alternators");
Eq(BlockType.Alternator, altSaveLoaded.Chunk.Get(17, 4, 15), "slice10 step2 alternator survives save/load as a chunk byte");
Eq(BlockType.Alternator, altSaveLoaded.Chunk.Get(19, 5, 16), "slice10 step2 second alternator survives save/load as a chunk byte");
Eq(Voltage.LV, altSaveLoaded.DeliveredVoltage(15, 4, 15), "slice10 step2 loaded stepped-down mill re-derives LV from placement");
Eq(Voltage.LV, altSaveLoaded.DeliveredVoltage(17, 5, 16), "slice10 step2 loaded stepped-down workstation re-derives LV from placement");
Check(altSaveLoaded.IsPowered(15, 4, 15), "slice10 step2 loaded stepped-down mill is powered");
Check(altSaveLoaded.IsWorkstationPowered(17, 5, 16), "slice10 step2 loaded stepped-down workstation is powered");
// HUD display decision: no new source count — sources are not
// counted, consumers are, as with the Dynamo. The existing
// powered-consumer counts are the proof a stepped-down
// Alternator is feeding both consumers, and a lone Alternator
// counts as neither, openly: a live source feeding no
// consumer shows nothing, the same gap as an energised cable
// feeding no mill.
var altHud = new Simulation(42);
altHud.World.Chunk.TrySet(15, 4, 15, BlockType.KineticMill);
altHud.World.Chunk.TrySet(16, 4, 15, BlockType.Transformer);
altHud.World.Chunk.TrySet(17, 4, 15, BlockType.Alternator);
altHud.World.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
altHud.World.Chunk.TrySet(18, 5, 16, BlockType.Transformer);
altHud.World.Chunk.TrySet(19, 5, 16, BlockType.Alternator);
var altHudState = altHud.Hud();
Eq(1, altHudState.TotalMills, "slice10 step2 HUD counts 1 alternator-fed mill");
Eq(1, altHudState.PoweredMills, "slice10 step2 HUD counts stepped-down alternator mill as powered");
Eq(1, altHudState.TotalWorkstations, "slice10 step2 HUD counts 1 alternator-fed workstation");
Eq(1, altHudState.PoweredWorkstations, "slice10 step2 HUD counts stepped-down alternator workstation as powered");
var altLoneHud = new Simulation(42);
altLoneHud.World.Chunk.TrySet(17, 5, 16, BlockType.Alternator);
var altLoneHudState = altLoneHud.Hud();
Eq(0, altLoneHudState.TotalMills, "slice10 step2 HUD counts a lone alternator as no mill");
Eq(0, altLoneHudState.TotalWorkstations, "slice10 step2 HUD counts a lone alternator as no workstation");

// ---- Slice 10 Step 3: composed demo scenario (0.47.0) ----
// Mirrors the Slice 10 demo scenario step for step: one
// Simulation with a cable step-down Alternator mill, a
// direct-HV Alternator mill, and a kinetic mill on the
// y=20 plane, plus a step-down Alternator workstation,
// an unpowered workstation, and a direct-HV workstation
// on the y=5 plane — then the player walking between
// the three benches (regen is player-centred, so one
// position cannot prove all three benches at once),
// then a save/load round-trip and a loaded 5-tick
// phase. The layout is staged with TrySet exactly as
// the Step 1-2 tests stage theirs; aim placement of
// each block type was proved in its own step's tests.
var s10Demo = new Simulation(42);
void SeedPocketS10Test(WorldZone w, int mx, int mz)
{
    foreach (var (x, y, z) in new[] { (mx - 1, 20, mz), (mx, 21, mz), (mx, 19, mz), (mx, 20, mz - 1), (mx, 20, mz + 1) })
        w.Chunk.TrySet(x, y, z, BlockType.Stone);
}
s10Demo.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
s10Demo.World.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s10Demo.World.Chunk.TrySet(12, 20, 10, BlockType.Transformer);
s10Demo.World.Chunk.TrySet(13, 20, 10, BlockType.Cable);
s10Demo.World.Chunk.TrySet(14, 20, 10, BlockType.Alternator);
s10Demo.World.Chunk.TrySet(10, 20, 14, BlockType.KineticMill);
s10Demo.World.Chunk.TrySet(11, 20, 14, BlockType.Alternator);
s10Demo.World.Chunk.TrySet(10, 20, 18, BlockType.KineticMill);
SeedPocketS10Test(s10Demo.World, 10, 10);
SeedPocketS10Test(s10Demo.World, 10, 14);
SeedPocketS10Test(s10Demo.World, 10, 18);
s10Demo.World.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
s10Demo.World.Chunk.TrySet(18, 5, 16, BlockType.Transformer);
s10Demo.World.Chunk.TrySet(19, 5, 16, BlockType.Alternator);
s10Demo.World.Chunk.TrySet(17, 5, 22, BlockType.Workstation);
s10Demo.World.Chunk.TrySet(17, 5, 28, BlockType.Workstation);
s10Demo.World.Chunk.TrySet(18, 5, 28, BlockType.Alternator);
s10Demo.World.Player.TakeDamage(20);
Eq(Voltage.LV, s10Demo.World.DeliveredVoltage(10, 20, 10), "slice10 step3 composed cable step-down mill receives LV");
Eq(Voltage.HV, s10Demo.World.DeliveredVoltage(10, 20, 14), "slice10 step3 composed direct alternator mill receives HV");
Eq(Voltage.None, s10Demo.World.DeliveredVoltage(10, 20, 18), "slice10 step3 composed kinetic mill receives nothing");
Eq(Voltage.LV, s10Demo.World.DeliveredVoltage(17, 5, 16), "slice10 step3 composed step-down workstation receives LV");
Eq(Voltage.None, s10Demo.World.DeliveredVoltage(17, 5, 22), "slice10 step3 composed unpowered workstation receives nothing");
Eq(Voltage.HV, s10Demo.World.DeliveredVoltage(17, 5, 28), "slice10 step3 composed direct alternator workstation receives HV");
var s10DemoHudBefore = s10Demo.Hud();
Eq(3, s10DemoHudBefore.TotalMills, "slice10 step3 composed HUD counts 3 mills before destruction");
Eq(1, s10DemoHudBefore.PoweredMills, "slice10 step3 composed HUD counts only the LV mill as powered");
Eq(3, s10DemoHudBefore.TotalWorkstations, "slice10 step3 composed HUD counts 3 workstations");
Eq(1, s10DemoHudBefore.PoweredWorkstations, "slice10 step3 composed HUD counts only the LV workstation as powered");
Eq(10, s10Demo.World.EffectiveWorkstationRegen(), "slice10 step3 composed powered bench regen is base 5 + powered 5 at spawn");
s10Demo.World.Advance();
Eq(BlockType.Air, s10Demo.World.Chunk.Get(10, 20, 14), "slice10 step3 composed HV mill destroyed on tick 1");
Eq(1, s10Demo.World.DestroyedMills, "slice10 step3 composed destruction counter is 1 after tick 1");
Eq(1, s10Demo.World.PoweredBlocksGathered, "slice10 step3 composed powered mill gathers 1 on tick 1");
Eq(0, s10Demo.World.KineticBlocksGathered, "slice10 step3 composed kinetic mill gathers 0 on tick 1");
Eq(BlockType.Stone, s10Demo.World.Chunk.Get(9, 20, 14), "slice10 step3 composed destroyed mill excavates nothing: its pocket stone survives");
Eq(BlockType.Alternator, s10Demo.World.Chunk.Get(11, 20, 14), "slice10 step3 composed destroyed mill's alternator survives");
Eq(BlockType.Workstation, s10Demo.World.Chunk.Get(17, 5, 28), "slice10 step3 composed HV workstation survives its tick, unlike a mill");
Eq(75, s10Demo.World.Player.Shields, "slice10 step3 composed powered bench restores 10 shields on tick 1");
var s10DemoHudAfter1 = s10Demo.Hud();
Eq(2, s10DemoHudAfter1.TotalMills, "slice10 step3 composed HUD mill total drops to 2 after destruction");
Eq(1, s10DemoHudAfter1.PoweredMills, "slice10 step3 composed HUD powered mill count stays 1 after destruction");
Eq(3, s10DemoHudAfter1.TotalWorkstations, "slice10 step3 composed HUD workstation total stays 3 after tick 1");
Eq(1, s10DemoHudAfter1.PoweredWorkstations, "slice10 step3 composed HUD powered workstation count stays 1 after tick 1");
Check(s10Demo.World.Player.MoveTo(16, 5, 22, s10Demo.World.Chunk), "slice10 step3 composed player walks to the unpowered bench");
Eq(5, s10Demo.World.EffectiveWorkstationRegen(), "slice10 step3 composed unpowered bench regen is base 5");
Check(s10Demo.World.Player.MoveTo(16, 5, 28, s10Demo.World.Chunk), "slice10 step3 composed player walks to the HV bench");
Eq(0, s10Demo.World.EffectiveWorkstationRegen(), "slice10 step3 composed HV bench regen is 0: disabled");
Check(s10Demo.World.Player.MoveTo(16, 5, 16, s10Demo.World.Chunk), "slice10 step3 composed player walks back to the powered bench");
Eq(10, s10Demo.World.EffectiveWorkstationRegen(), "slice10 step3 composed powered bench regen is 10 again after the walk");
for (int i = 0; i < 4; i++) s10Demo.World.Advance();
Eq(5, s10Demo.World.PoweredBlocksGathered, "slice10 step3 composed powered mill gathers 5 in 5 ticks");
Eq(1, s10Demo.World.KineticBlocksGathered, "slice10 step3 composed kinetic mill gathers 1 in 5 ticks");
Eq(1, s10Demo.World.DestroyedMills, "slice10 step3 composed destruction counter stays 1 through 5 ticks");
Eq(6, s10Demo.World.Player.Inventory.Get(BlockType.Stone), "slice10 step3 composed stone is 5 + 1, no refund for the destroyed mill");
Eq(85, s10Demo.World.Player.Shields, "slice10 step3 composed shields cap at 85 after powered ticks");
var s10DemoSave = s10Demo.SaveToJson();
var s10DemoLoaded = new Simulation(99);
Check(s10DemoLoaded.LoadFromJson(s10DemoSave), "slice10 step3 composed save loads");
Eq(BlockType.KineticMill, s10DemoLoaded.World.Chunk.Get(10, 20, 10), "slice10 step3 composed cable step-down mill survives load");
Eq(BlockType.Air, s10DemoLoaded.World.Chunk.Get(10, 20, 14), "slice10 step3 composed destroyed mill stays destroyed after load");
Eq(BlockType.KineticMill, s10DemoLoaded.World.Chunk.Get(10, 20, 18), "slice10 step3 composed kinetic mill survives load");
Eq(BlockType.Workstation, s10DemoLoaded.World.Chunk.Get(17, 5, 16), "slice10 step3 composed powered workstation survives load");
Eq(BlockType.Workstation, s10DemoLoaded.World.Chunk.Get(17, 5, 22), "slice10 step3 composed unpowered workstation survives load");
Eq(BlockType.Workstation, s10DemoLoaded.World.Chunk.Get(17, 5, 28), "slice10 step3 composed HV workstation survives load");
Eq(Voltage.LV, s10DemoLoaded.World.DeliveredVoltage(10, 20, 10), "slice10 step3 composed mill re-derives LV after load");
Eq(Voltage.LV, s10DemoLoaded.World.DeliveredVoltage(17, 5, 16), "slice10 step3 composed powered workstation re-derives LV after load");
Eq(Voltage.HV, s10DemoLoaded.World.DeliveredVoltage(17, 5, 28), "slice10 step3 composed HV workstation re-derives HV after load");
Eq(0, s10DemoLoaded.World.PoweredBlocksGathered, "slice10 step3 composed powered counter resets on load");
Eq(0, s10DemoLoaded.World.KineticBlocksGathered, "slice10 step3 composed kinetic counter resets on load");
Eq(0, s10DemoLoaded.World.DestroyedMills, "slice10 step3 composed destruction counter resets on load");
Eq(6, s10DemoLoaded.World.Player.Inventory.Get(BlockType.Stone), "slice10 step3 composed load gives no stone refund for the destroyed mill");
Eq(16, s10DemoLoaded.World.Player.X, "slice10 step3 composed loaded player returns to the powered bench X");
Eq(5, s10DemoLoaded.World.Player.Y, "slice10 step3 composed loaded player returns to the powered bench Y");
Eq(16, s10DemoLoaded.World.Player.Z, "slice10 step3 composed loaded player returns to the powered bench Z");
Eq(10, s10DemoLoaded.World.EffectiveWorkstationRegen(), "slice10 step3 composed loaded powered bench regen is 10");
var s10DemoLoadedHud = s10DemoLoaded.Hud();
Eq(2, s10DemoLoadedHud.TotalMills, "slice10 step3 composed HUD counts 2 mills after load");
Eq(1, s10DemoLoadedHud.PoweredMills, "slice10 step3 composed HUD counts 1 powered mill after load");
Eq(3, s10DemoLoadedHud.TotalWorkstations, "slice10 step3 composed HUD counts 3 workstations after load");
Eq(1, s10DemoLoadedHud.PoweredWorkstations, "slice10 step3 composed HUD counts 1 powered workstation after load");
s10DemoLoaded.World.Player.TakeDamage(20);
SeedPocketS10Test(s10DemoLoaded.World, 10, 10);
for (int i = 0; i < 5; i++) s10DemoLoaded.World.Advance();
Eq(5, s10DemoLoaded.World.PoweredBlocksGathered, "slice10 step3 composed loaded mill gathers 5 again in 5 ticks");
Eq(1, s10DemoLoaded.World.KineticBlocksGathered, "slice10 step3 composed loaded kinetic mill gathers 1 again in 5 ticks");
Eq(0, s10DemoLoaded.World.DestroyedMills, "slice10 step3 composed loaded world destroys nothing new");
Eq(12, s10DemoLoaded.World.Player.Inventory.Get(BlockType.Stone), "slice10 step3 composed loaded stone reaches 6 + 6");
Eq(85, s10DemoLoaded.World.Player.Shields, "slice10 step3 composed loaded powered bench caps shields at 85");

// ---- Slice 11 Step 1: third squad type — Heavy (chosen from heavy / flyer) ----
// Heavy is the close-range anchor the other kinds cannot be: it
// survives a hit that kills Vanguard outright (2 x 50 HP = 100,
// against Vanguard's 90), but it pays in exactly the two
// dimensions Recon's proof used — range and firepower. It fires
// only at range 2 (Vanguard 3, Recon 5) and deals 2 x 8 = 16 per
// tick (Vanguard 30, Recon 12), so it is better than Vanguard in
// exactly one dimension (survivability) and worse in two, and it
// is a choice, not a replacement. Flyer was not chosen: it would
// need a movement rule (ignoring solids / air) with targeting
// and occupancy consequences, which would make the kind
// untestable in isolation — the scope's own warning, carried
// from Slice 7. Movement stays the shared one-voxel rule for
// all three kinds. Slice 7's heavy rejection is answered, not
// ignored: then the gap was a screen and a tougher anchor was
// Vanguard with more HP; now the gap is the anchor itself, and
// the range-2 / 16-DPS costs are what keep Heavy from being
// Vanguard with more HP — a Vanguard fires at range 3 where a
// Heavy is silent, and a Heavy deals barely half a Vanguard's
// damage where it does fire.
// Per-kind shape table: Vanguard's and Recon's numbers are unchanged.
Eq(3, SquadShapes.For(SquadKind.Vanguard).Members, "slice11 vanguard shape members unchanged");
Eq(30, SquadShapes.For(SquadKind.Vanguard).MemberHealth, "slice11 vanguard shape member health unchanged");
Eq(10, SquadShapes.For(SquadKind.Vanguard).MemberDamage, "slice11 vanguard shape member damage unchanged");
Eq(3, SquadShapes.For(SquadKind.Vanguard).EngagementRange, "slice11 vanguard shape range unchanged");
Eq(2, SquadShapes.For(SquadKind.Recon).Members, "slice11 recon shape members unchanged");
Eq(20, SquadShapes.For(SquadKind.Recon).MemberHealth, "slice11 recon shape member health unchanged");
Eq(6, SquadShapes.For(SquadKind.Recon).MemberDamage, "slice11 recon shape member damage unchanged");
Eq(5, SquadShapes.For(SquadKind.Recon).EngagementRange, "slice11 recon shape range unchanged");
Eq(2, SquadShapes.For(SquadKind.Heavy).Members, "slice11 heavy shape members");
Eq(50, SquadShapes.For(SquadKind.Heavy).MemberHealth, "slice11 heavy shape member health");
Eq(8, SquadShapes.For(SquadKind.Heavy).MemberDamage, "slice11 heavy shape member damage");
Eq(2, SquadShapes.For(SquadKind.Heavy).EngagementRange, "slice11 heavy shape range");
bool heavyShapeThrows = false;
try { SquadShapes.For((SquadKind)99); } catch (ArgumentOutOfRangeException) { heavyShapeThrows = true; }
Check(heavyShapeThrows, "slice11 unknown kind still has no shape, throws rather than falling back to Vanguard");
// A deployed Heavy squad reads its own shape, not Vanguard's.
var heavyWorld = new WorldZone(42);
var heavy = heavyWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Heavy)!;
Eq(SquadKind.Heavy, heavy.Kind, "slice11 heavy deploy kind");
Eq(100, heavy.Health, "slice11 heavy full health is 100, not 90");
Eq(100, heavy.MaxHealth, "slice11 heavy max health is 100");
Eq(2, heavy.AliveMembers, "slice11 heavy full members");
Eq(16, heavy.DamagePerTick, "slice11 heavy full damage per tick is 16, below vanguard 30");
// Firepower drops in member steps, per kind: Heavy 100 -> 50 HP
// is 1 member dealing 8, and 100 damage kills it outright.
heavy.TakeDamage(50);
Eq(50, heavy.Health, "slice11 heavy wounded health");
Eq(1, heavy.AliveMembers, "slice11 heavy wounded has 1 member");
Eq(8, heavy.DamagePerTick, "slice11 heavy wounded damage per tick");
heavy.TakeDamage(50);
Check(heavy.IsDead, "slice11 heavy at 0 health is dead");
Eq(0, heavy.DamagePerTick, "slice11 dead heavy deals nothing");
// The anchor proof, stated exactly: 95 damage kills a Vanguard
// (90 HP) outright and leaves a Heavy at 5 HP with 1 member.
var anchorVanWorld = new WorldZone(42);
var anchorVan = anchorVanWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
anchorVan.TakeDamage(95);
Check(anchorVan.IsDead, "slice11 anchor hit kills vanguard outright");
var anchorHeavyWorld = new WorldZone(42);
var anchorHeavy = anchorHeavyWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Heavy)!;
anchorHeavy.TakeDamage(95);
Eq(5, anchorHeavy.Health, "slice11 anchor hit leaves heavy at 5 hp");
Eq(1, anchorHeavy.AliveMembers, "slice11 anchor hit leaves heavy with 1 member");
Check(!anchorHeavy.IsDead, "slice11 anchor hit does not kill heavy");
// Heavy fires at range 2, not at range 3: enemy starts 4 away,
// steps to 3 in the enemy phase, and the holding Heavy stays
// silent; a Vanguard at the same geometry fires for 30.
var heavyFarWorld = new WorldZone(42);
heavyFarWorld.SpawnSquad(16, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Heavy);
heavyFarWorld.Player.X = 28; heavyFarWorld.Player.Z = 28;
heavyFarWorld.Enemies.Add(new Enemy(20, 5, 16));
heavyFarWorld.Advance();
Eq(19, heavyFarWorld.Enemies[0].X, "slice11 heavy range-3 enemy steps to distance 3");
Eq(30, heavyFarWorld.Enemies[0].Health, "slice11 heavy does not fire at range 3");
var vanFarWorld = new WorldZone(42);
vanFarWorld.SpawnSquad(16, 5, 16);
vanFarWorld.Player.X = 28; vanFarWorld.Player.Z = 28;
vanFarWorld.Enemies.Add(new Enemy(20, 5, 16));
vanFarWorld.Advance();
Eq(0, vanFarWorld.Enemies.Count, "slice11 vanguard at same geometry fires for 30 at range 3 and kills the 30 hp enemy");
// Heavy fires at range 2 for 16: enemy starts 3 away, steps to 2,
// then the holding Heavy fires.
var heavyRangeWorld = new WorldZone(42);
heavyRangeWorld.SpawnSquad(16, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Heavy);
heavyRangeWorld.Player.X = 28; heavyRangeWorld.Player.Z = 28;
heavyRangeWorld.Enemies.Add(new Enemy(19, 5, 16));
heavyRangeWorld.Advance();
Eq(18, heavyRangeWorld.Enemies[0].X, "slice11 heavy range enemy steps to distance 2");
Eq(14, heavyRangeWorld.Enemies[0].Health, "slice11 heavy fires at range 2 for 16");
// Movement is the shared rule: a Heavy HoldGround squad marches
// one voxel per tick toward its rally, exactly like Vanguard.
var heavyMarchWorld = new WorldZone(42);
var heavyMarch = heavyMarchWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Heavy)!;
heavyMarchWorld.Advance();
Eq(17, heavyMarch.X, "slice11 heavy marches one voxel toward rally");
// Both doctrines work for Heavy: an Advance Heavy hunts west
// toward its enemy, not east toward its rally, and fires 16
// once in range 2.
var heavyAdvWorld = new WorldZone(42);
var heavyAdv = heavyAdvWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.Advance, SquadKind.Heavy)!;
heavyAdvWorld.Player.X = 28; heavyAdvWorld.Player.Z = 28;
heavyAdvWorld.Enemies.Add(new Enemy(12, 5, 16));
heavyAdvWorld.Advance();
Eq(15, heavyAdv.X, "slice11 advance heavy moves toward enemy, not rally");
Eq(13, heavyAdvWorld.Enemies[0].X, "slice11 advance heavy enemy pursues squad");
Eq(14, heavyAdvWorld.Enemies[0].Health, "slice11 advance heavy fires 16 in range");
// Economy: one price, total cap. CostForKind is 10 for all three
// kinds, so the prompt's COST 10 line stays true; the cap is
// 3 live squads in total, not 3 per kind.
Eq(10, SquadEconomy.CostForKind(SquadKind.Vanguard), "slice11 vanguard price still 10");
Eq(10, SquadEconomy.CostForKind(SquadKind.Recon), "slice11 recon price still 10");
Eq(10, SquadEconomy.CostForKind(SquadKind.Heavy), "slice11 heavy price is 10");
var econHeavyWorld = new WorldZone(42);
econHeavyWorld.Player.Inventory.Add(BlockType.EmberOre, 40);
var econHeavy = econHeavyWorld.TryDeployPaidSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Heavy)!;
Eq(SquadKind.Heavy, econHeavy.Kind, "slice11 paid heavy deploy kind");
Eq(100, econHeavy.Health, "slice11 paid heavy deploy health");
Eq(30, econHeavyWorld.Player.Inventory.Get(BlockType.EmberOre), "slice11 paid heavy deploy charges exactly 10");
econHeavyWorld.TryDeployPaidSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Vanguard);
econHeavyWorld.TryDeployPaidSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon);
Eq(3, SquadEconomy.LiveSquadCount(econHeavyWorld), "slice11 mixed kinds count toward one total cap");
Check(econHeavyWorld.TryDeployPaidSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Heavy) == null, "slice11 fourth squad refused at total cap, not per-kind cap");
Eq(10, econHeavyWorld.Player.Inventory.Get(BlockType.EmberOre), "slice11 refused over-cap deploy charges nothing");
var econPoorHeavyWorld = new WorldZone(42);
Check(econPoorHeavyWorld.TryDeployPaidSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Heavy) == null, "slice11 unfunded heavy deploy refused");
Eq(0, econPoorHeavyWorld.Squads.Count, "slice11 unfunded heavy deploy adds no squad");
// Picker: doctrine and kind are independent choices. A Heavy
// selection survives a doctrine change, an unknown kind is
// rejected, and a Confirm deploys the selected combination.
var heavyPickSim = new Simulation(42);
heavyPickSim.World.Player.Inventory.Add(BlockType.EmberOre, 10);
heavyPickSim.BeginOrbitalPick();
Check(heavyPickSim.OrbitalCommand.SelectKind(SquadKind.Heavy), "slice11 picker selects heavy kind");
Check(heavyPickSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround), "slice11 picker selects doctrine after kind");
Eq(SquadKind.Heavy, heavyPickSim.OrbitalCommand.Kind, "slice11 doctrine change keeps selected kind");
Check(!heavyPickSim.OrbitalCommand.SelectKind((SquadKind)99), "slice11 picker rejects unknown kind");
Eq(SquadKind.Heavy, heavyPickSim.OrbitalCommand.Kind, "slice11 rejected kind leaves selection unchanged");
heavyPickSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance);
heavyPickSim.SetOrbitalRally(20, 5, 16);
var heavyPicked = heavyPickSim.ConfirmOrbitalDeploy()!;
Eq(SquadKind.Heavy, heavyPicked.Kind, "slice11 picker confirm deploys heavy");
Eq(TacticalDoctrine.Advance, heavyPicked.Doctrine, "slice11 picker confirm deploys advance");
Eq(0, heavyPickSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice11 picker heavy deploy charges 10");
// Save/load: the load clamp is per-kind, not Vanguard-shaped.
// A 999-HP Heavy loads at 100, a 999-HP Vanguard at 90, and a
// mixed save round-trips all three kinds with their own health.
var clampHeavy = new TacticalSquad(16, 5, 16, 20, 5, 16, 999, TacticalDoctrine.HoldGround, SquadKind.Heavy);
Eq(100, clampHeavy.Health, "slice11 heavy load clamp is 100, not vanguard 90");
var clampVan2 = new TacticalSquad(16, 5, 16, 20, 5, 16, 999, TacticalDoctrine.HoldGround, SquadKind.Vanguard);
Eq(90, clampVan2.Health, "slice11 vanguard load clamp still 90");
var saveMixed3World = new WorldZone(42);
saveMixed3World.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Vanguard);
saveMixed3World.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon);
var saveHeavy = saveMixed3World.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Heavy)!;
saveHeavy.TakeDamage(10); // 100 -> 90, still 2 members
var saveMixed3Loaded = SaveSystem.LoadZone(SaveSystem.SaveZone(saveMixed3World));
Eq(3, saveMixed3Loaded.Squads.Count, "slice11 mixed save loads all three squads");
Eq(SquadKind.Vanguard, saveMixed3Loaded.Squads[0].Kind, "slice11 mixed save vanguard kind preserved");
Eq(90, saveMixed3Loaded.Squads[0].Health, "slice11 mixed save vanguard health preserved");
Eq(SquadKind.Recon, saveMixed3Loaded.Squads[1].Kind, "slice11 mixed save recon kind preserved");
Eq(SquadKind.Heavy, saveMixed3Loaded.Squads[2].Kind, "slice11 mixed save heavy kind preserved");
Eq(90, saveMixed3Loaded.Squads[2].Health, "slice11 mixed save heavy health preserved");
Eq(2, saveMixed3Loaded.Squads[2].AliveMembers, "slice11 mixed save heavy members preserved");
// Targeting stays kind-blind: an enemy pursues the nearer Heavy
// squad, not the farther player, with no kind priority in the rule.
var heavyTargetWorld = new WorldZone(42);
heavyTargetWorld.SpawnSquad(16, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Heavy);
heavyTargetWorld.Player.X = 28; heavyTargetWorld.Player.Z = 28;
heavyTargetWorld.Enemies.Add(new Enemy(20, 5, 16));
heavyTargetWorld.Advance();
Eq(19, heavyTargetWorld.Enemies[0].X, "slice11 enemy pursues nearer heavy squad over farther player");
// Snapshot carries the kind and its shape: a holding Heavy
// reads Holding with 2 members, 100 HP, 16 damage.
var heavySnapWorld = new WorldZone(42);
heavySnapWorld.SpawnSquad(16, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Heavy);
var snapHeavy = OrbitalView.Snapshot(heavySnapWorld)[0];
Eq(SquadKind.Heavy, snapHeavy.Kind, "slice11 snapshot heavy kind");
Eq(2, snapHeavy.AliveMembers, "slice11 snapshot heavy members");
Eq(100, snapHeavy.Health, "slice11 snapshot heavy health");
Eq(16, snapHeavy.DamagePerTick, "slice11 snapshot heavy damage");
Eq(SquadActivity.Holding, snapHeavy.Activity, "slice11 snapshot heavy at rally is holding");

// ---- Slice 11 Step 2: third doctrine — Defend Core (chosen from Defend Core / Patrol) ----
// Defend Core is a guard doctrine, not HoldGround at the core
// and not Advance with a starting point: its post is the world
// objective core (28,5,28), not the rally the picker still
// requires; it chases a living enemy within 8 Manhattan of the
// core, ignores one farther away, and otherwise returns to the
// core one voxel per tick. It fires whenever an enemy is in its
// kind's engagement range, like Advance — so a chasing or
// returning guard shoots what it passes, but it never chases a
// target that is not a core attacker. Patrol is deferred, not
// rejected (see DECISIONS.md): it still needs a route the core
// does not yet define.
// Enum order: appended only, so old doctrines keep their values.
Eq(0, (int)TacticalDoctrine.HoldGround, "slice11 defendcore holdground value unchanged");
Eq(1, (int)TacticalDoctrine.Advance, "slice11 defendcore advance value unchanged");
Eq(2, (int)TacticalDoctrine.DefendCore, "slice11 defendcore appended as 2");
Eq(8, TacticalSquad.DefendCoreChaseRadius, "slice11 defendcore chase radius is 8");
var dcUnknownWorld = new WorldZone(42);
Check(dcUnknownWorld.SpawnSquad(20, 5, 16, (TacticalDoctrine)99) == null, "slice11 defendcore unknown doctrine still rejected");
// March to the core, rally ignored: rally west, core east, and
// the squad walks east one voxel per tick on the shared
// largest-gap rule (X on the 12/12 tie, then Z), reaching the
// core in 24 ticks and reading Guarding there, not Holding —
// its rally (5,5,5) was never its destination.
var dcMarchWorld = new WorldZone(42);
var dcMarch = dcMarchWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore)!;
dcMarchWorld.Advance();
Eq(17, dcMarch.X, "slice11 defendcore march tick 1 steps east toward core");
Eq(16, dcMarch.Z, "slice11 defendcore march tick 1 does not step toward rally");
dcMarchWorld.Advance();
Eq(17, dcMarch.X, "slice11 defendcore march tick 2 steps north toward core");
Eq(17, dcMarch.Z, "slice11 defendcore march tick 2 reaches diagonal");
for (int i = 0; i < 22; i++) dcMarchWorld.Advance();
Eq(28, dcMarch.X, "slice11 defendcore march reaches core X after 24 ticks");
Eq(28, dcMarch.Z, "slice11 defendcore march reaches core Z after 24 ticks");
Eq(SquadActivity.Guarding, OrbitalView.Snapshot(dcMarchWorld)[0].Activity, "slice11 defendcore at core reads guarding not holding");
Check(!dcMarch.AtRally, "slice11 defendcore guarding is not rally holding");
// One-tick doctrine contrast, same rally, no enemies:
// HoldGround walks west to its rally, Advance stands still
// with no prey, Defend Core walks east to the core.
var dcContrastHoldWorld = new WorldZone(42);
var dcContrastHold = dcContrastHoldWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.HoldGround)!;
dcContrastHoldWorld.Advance();
Eq(15, dcContrastHold.X, "slice11 defendcore contrast holdground walks to rally");
var dcContrastAdvWorld = new WorldZone(42);
var dcContrastAdv = dcContrastAdvWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.Advance)!;
dcContrastAdvWorld.Advance();
Eq(16, dcContrastAdv.X, "slice11 defendcore contrast advance stands with no enemy");
Eq(16, dcContrastAdv.Z, "slice11 defendcore contrast advance does not march to rally");
var dcContrastCoreWorld = new WorldZone(42);
var dcContrastCore = dcContrastCoreWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore)!;
dcContrastCoreWorld.Advance();
Eq(17, dcContrastCore.X, "slice11 defendcore contrast defendcore walks to core not rally");
// Chase radius, direct (no enemy phase to move the target):
// an attacker 8 from the core is chased, one 9 away is ignored,
// a dead attacker is ignored, ties go to the first in list order,
// and a blocked chase step does not fall through to the core.
var dcBoundWorld = new WorldZone(42);
dcBoundWorld.Player.X = 28; dcBoundWorld.Player.Z = 28;
var dcBound = dcBoundWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore)!;
var dcBoundAttacker = new Enemy(20, 5, 28);
Check(dcBound.MoveForDefendCore(dcBoundWorld.Chunk, new[] { dcBoundAttacker }, 28, 5, 28), "slice11 defendcore attacker at radius 8 is chased");
Eq(27, dcBound.X, "slice11 defendcore chase step moves toward attacker");
var dcFarDirectWorld = new WorldZone(42);
dcFarDirectWorld.Player.X = 28; dcFarDirectWorld.Player.Z = 28;
var dcFarDirect = dcFarDirectWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore)!;
Check(!dcFarDirect.MoveForDefendCore(dcFarDirectWorld.Chunk, new[] { new Enemy(19, 5, 28) }, 28, 5, 28), "slice11 defendcore enemy at radius 9 is not chased");
Eq(28, dcFarDirect.X, "slice11 defendcore ignored enemy leaves guard at post");
var dcDeadAttackerWorld = new WorldZone(42);
dcDeadAttackerWorld.Player.X = 28; dcDeadAttackerWorld.Player.Z = 28;
var dcDeadAttacker = dcDeadAttackerWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore)!;
var dcDeadEnemy = new Enemy(20, 5, 28);
dcDeadEnemy.TakeDamage(30);
Check(!dcDeadAttacker.MoveForDefendCore(dcDeadAttackerWorld.Chunk, new[] { dcDeadEnemy }, 28, 5, 28), "slice11 defendcore dead attacker is not chased");
var dcTieWorld = new WorldZone(42);
dcTieWorld.Player.X = 28; dcTieWorld.Player.Z = 28;
var dcTie = dcTieWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore)!;
dcTie.MoveForDefendCore(dcTieWorld.Chunk, new[] { new Enemy(24, 5, 28), new Enemy(28, 5, 24) }, 28, 5, 28);
Eq(27, dcTie.X, "slice11 defendcore attacker tie goes to first enemy in list");
Eq(28, dcTie.Z, "slice11 defendcore attacker tie does not step toward second enemy");
var dcBlockedWorld = new WorldZone(42);
dcBlockedWorld.Player.X = 27; dcBlockedWorld.Player.Z = 28;
var dcBlocked = dcBlockedWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore)!;
dcBlockedWorld.Chunk.TrySet(26, 5, 28, BlockType.Stone);
Check(!dcBlocked.MoveForDefendCore(dcBlockedWorld.Chunk, new[] { new Enemy(25, 5, 28) }, 28, 5, 28), "slice11 defendcore blocked chase step is refused");
Eq(27, dcBlocked.X, "slice11 defendcore blocked chase does not fall through to core in same tick");
// Chase, fire, return through WorldZone.Advance: Vanguard guard
// at the core chases an attacker 6 from the core, kills it in
// range, then steps back toward the core on the next ticks.
var dcChaseWorld = new WorldZone(42);
dcChaseWorld.Player.X = 28; dcChaseWorld.Player.Z = 28;
var dcChase = dcChaseWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore)!;
dcChaseWorld.Player.X = 0; dcChaseWorld.Player.Z = 0;
dcChaseWorld.Enemies.Add(new Enemy(28, 5, 22));
dcChaseWorld.Advance();
Eq(27, dcChase.Z, "slice11 defendcore chase tick 1 steps toward attacker");
Eq(23, dcChaseWorld.Enemies[0].Z, "slice11 defendcore chase tick 1 attacker steps toward guard");
Eq(30, dcChaseWorld.Enemies[0].Health, "slice11 defendcore chase tick 1 out of range does not fire");
dcChaseWorld.Advance();
Eq(26, dcChase.Z, "slice11 defendcore chase tick 2 closes to range");
Eq(0, dcChaseWorld.Enemies.Count, "slice11 defendcore chase tick 2 kills attacker in range");
dcChaseWorld.Advance();
Eq(27, dcChase.Z, "slice11 defendcore return tick steps back toward core after kill");
dcChaseWorld.Advance();
Eq(28, dcChase.Z, "slice11 defendcore return reaches core post");
Eq(SquadActivity.Guarding, OrbitalView.Snapshot(dcChaseWorld)[0].Activity, "slice11 defendcore returned guard reads guarding");
// Far enemy, one tick, against Advance: the enemy starts 11
// from the core and steps to 10, still outside the radius, so
// the guard stays at its post while an Advance squad at the
// same post hunts west toward that same enemy.
var dcFarWorld = new WorldZone(42);
dcFarWorld.Player.X = 28; dcFarWorld.Player.Z = 28;
var dcFar = dcFarWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore)!;
dcFarWorld.Player.X = 0; dcFarWorld.Player.Z = 0;
dcFarWorld.Enemies.Add(new Enemy(17, 5, 28));
dcFarWorld.Advance();
Eq(28, dcFar.X, "slice11 defendcore far enemy leaves guard at post");
Eq(18, dcFarWorld.Enemies[0].X, "slice11 defendcore far enemy steps closer but stays outside radius");
Eq(SquadActivity.Guarding, OrbitalView.Snapshot(dcFarWorld)[0].Activity, "slice11 defendcore far enemy is not a hunting target");
var dcFarAdvWorld = new WorldZone(42);
dcFarAdvWorld.Player.X = 28; dcFarAdvWorld.Player.Z = 28;
var dcFarAdv = dcFarAdvWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.Advance)!;
dcFarAdvWorld.Player.X = 0; dcFarAdvWorld.Player.Z = 0;
dcFarAdvWorld.Enemies.Add(new Enemy(17, 5, 28));
dcFarAdvWorld.Advance();
Eq(27, dcFarAdv.X, "slice11 defendcore contrast advance hunts the far enemy defendcore ignores");
// Returning fire: a guard marching back to the core fires at
// an enemy in range that is NOT a core attacker (21 from the
// core), but its step is still core-ward, not toward the enemy.
var dcRetFireWorld = new WorldZone(42);
var dcRetFire = dcRetFireWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore)!;
dcRetFire.TakeDamage(60); // 1 member, 10 damage, so the shot wounds instead of killing
dcRetFireWorld.Enemies.Add(new Enemy(16, 5, 19));
dcRetFireWorld.Advance();
Eq(17, dcRetFire.X, "slice11 defendcore returning guard steps toward core not enemy");
Eq(16, dcRetFire.Z, "slice11 defendcore returning guard does not chase its target");
Eq(20, dcRetFireWorld.Enemies[0].Health, "slice11 defendcore returning guard fires at non-attacker in range");
// At-post fire, Vanguard: an adjacent attacker cannot be
// stepped onto, so the guard stays on the core cell and fires.
var dcPostWorld = new WorldZone(42);
dcPostWorld.Player.X = 28; dcPostWorld.Player.Z = 28;
var dcPost = dcPostWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore)!;
dcPostWorld.Player.X = 0; dcPostWorld.Player.Z = 0;
dcPostWorld.Enemies.Add(new Enemy(28, 5, 29));
dcPostWorld.Advance();
Eq(28, dcPost.X, "slice11 defendcore at-post guard stays on core cell");
Eq(28, dcPost.Z, "slice11 defendcore at-post guard does not overlap attacker");
Eq(0, dcPostWorld.Enemies.Count, "slice11 defendcore at-post guard kills adjacent attacker");
// At-post fire, Heavy (the new kind under the new doctrine):
// an attacker stepping adjacent is shot for 16, not 30.
var dcHeavyPostWorld = new WorldZone(42);
dcHeavyPostWorld.Player.X = 28; dcHeavyPostWorld.Player.Z = 28;
var dcHeavyPost = dcHeavyPostWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore, SquadKind.Heavy)!;
dcHeavyPostWorld.Player.X = 0; dcHeavyPostWorld.Player.Z = 0;
dcHeavyPostWorld.Enemies.Add(new Enemy(28, 5, 30));
dcHeavyPostWorld.Advance();
Eq(28, dcHeavyPost.X, "slice11 defendcore heavy guard stays on core cell");
Eq(14, dcHeavyPostWorld.Enemies[0].Health, "slice11 defendcore heavy guard fires 16 at post");
// Engagement ranges, direct: Heavy is silent at range 3,
// Vanguard fires at range 3, Recon fires at range 5, and a
// Defend Core squad away from its post still fires in range
// where a HoldGround squad still marching does not.
var dcFindHeavyWorld = new WorldZone(42);
var dcFindHeavy = dcFindHeavyWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.DefendCore, SquadKind.Heavy)!;
Check(dcFindHeavy.FindTarget(new[] { new Enemy(19, 5, 16) }) == null, "slice11 defendcore heavy does not fire at range 3");
var dcFindVanWorld = new WorldZone(42);
var dcFindVan = dcFindVanWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.DefendCore, SquadKind.Vanguard)!;
Check(dcFindVan.FindTarget(new[] { new Enemy(19, 5, 16) }) != null, "slice11 defendcore vanguard fires at range 3");
var dcFindReconWorld = new WorldZone(42);
var dcFindRecon = dcFindReconWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.DefendCore, SquadKind.Recon)!;
Check(dcFindRecon.FindTarget(new[] { new Enemy(21, 5, 16) }) != null, "slice11 defendcore recon fires at range 5");
var dcFindHoldWorld = new WorldZone(42);
var dcFindHold = dcFindHoldWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround)!;
Check(dcFindHold.FindTarget(new[] { new Enemy(17, 5, 16) }) == null, "slice11 defendcore contrast holdground marching does not fire");
// Recon under Defend Core (the third kind proved): a Recon
// guard chases an attacker 8 from the core and stays silent
// at range 6, beyond its range 5.
var dcReconWorld = new WorldZone(42);
dcReconWorld.Player.X = 28; dcReconWorld.Player.Z = 28;
var dcRecon = dcReconWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore, SquadKind.Recon)!;
dcReconWorld.Player.X = 0; dcReconWorld.Player.Z = 0;
dcReconWorld.Enemies.Add(new Enemy(28, 5, 20));
dcReconWorld.Advance();
Eq(27, dcRecon.Z, "slice11 defendcore recon guard chases attacker");
Eq(21, dcReconWorld.Enemies[0].Z, "slice11 defendcore recon attacker steps toward guard");
Eq(30, dcReconWorld.Enemies[0].Health, "slice11 defendcore recon guard silent beyond range 5");
// Picker and economy: Defend Core + Heavy deploy through the
// paid path for 10 EmberOre, and an unknown doctrine is
// rejected without changing the Defend Core selection.
var dcPickSim = new Simulation(42);
dcPickSim.World.Player.Inventory.Add(BlockType.EmberOre, 10);
Check(dcPickSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.DefendCore), "slice11 defendcore picker doctrine accepted");
Check(dcPickSim.OrbitalCommand.SelectKind(SquadKind.Heavy), "slice11 defendcore picker heavy kind accepted");
Check(!dcPickSim.OrbitalCommand.SelectDoctrine((TacticalDoctrine)99), "slice11 defendcore picker unknown doctrine rejected");
Eq(TacticalDoctrine.DefendCore, dcPickSim.OrbitalCommand.Doctrine, "slice11 defendcore picker doctrine unchanged after rejection");
dcPickSim.BeginOrbitalPick();
Check(dcPickSim.SetOrbitalRally(28, 5, 28), "slice11 defendcore picker rally set");
var dcPicked = dcPickSim.ConfirmOrbitalDeploy();
Check(dcPicked != null, "slice11 defendcore picker confirm deploys");
Eq(TacticalDoctrine.DefendCore, dcPicked!.Doctrine, "slice11 defendcore picker deploy doctrine");
Eq(SquadKind.Heavy, dcPicked.Kind, "slice11 defendcore picker deploy kind");
Check(!dcPickSim.World.Player.Inventory.Has(BlockType.EmberOre, 1), "slice11 defendcore picker deploy charges 10 ore");
// Save rule, proved, not changed: doctrine is stored as an int
// and the loader already rejects undefined doctrines, so no new
// save field is invented. A Defend Core Heavy at the core and a
// Defend Core Vanguard marching both load, and the loaded
// Vanguard marches toward the core, not its rally.
var dcSaveWorld = new WorldZone(42);
dcSaveWorld.Player.X = 28; dcSaveWorld.Player.Z = 28;
dcSaveWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore, SquadKind.Heavy);
dcSaveWorld.Player.X = 16; dcSaveWorld.Player.Z = 16;
dcSaveWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore, SquadKind.Vanguard);
var dcSaveLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(dcSaveWorld));
Eq(2, dcSaveLoaded.Squads.Count, "slice11 defendcore save loads both squads");
Eq(TacticalDoctrine.DefendCore, dcSaveLoaded.Squads[0].Doctrine, "slice11 defendcore save heavy doctrine preserved");
Eq(SquadKind.Heavy, dcSaveLoaded.Squads[0].Kind, "slice11 defendcore save heavy kind preserved");
Eq(100, dcSaveLoaded.Squads[0].Health, "slice11 defendcore save heavy health preserved");
Eq(TacticalDoctrine.DefendCore, dcSaveLoaded.Squads[1].Doctrine, "slice11 defendcore save vanguard doctrine preserved");
dcSaveLoaded.Advance();
Eq(17, dcSaveLoaded.Squads[1].X, "slice11 defendcore loaded vanguard marches toward core not rally");
Eq(28, dcSaveLoaded.Squads[0].X, "slice11 defendcore loaded heavy stays at core");
// Snapshot activities: Guarding at post with no attacker,
// Hunting a core attacker with its cell, Marching on the
// return with no attacker, and Guarding — not Hunting — at
// post when the only enemy is far from the core.
var dcSnapGuardWorld = new WorldZone(42);
dcSnapGuardWorld.Player.X = 28; dcSnapGuardWorld.Player.Z = 28;
dcSnapGuardWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore);
var dcSnapGuard = OrbitalView.Snapshot(dcSnapGuardWorld)[0];
Eq(SquadActivity.Guarding, dcSnapGuard.Activity, "slice11 defendcore snapshot at post reads guarding");
Check(!dcSnapGuard.HasHuntingTarget, "slice11 defendcore snapshot guarding has no hunting target");
var dcSnapHuntWorld = new WorldZone(42);
dcSnapHuntWorld.Player.X = 28; dcSnapHuntWorld.Player.Z = 28;
dcSnapHuntWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore);
dcSnapHuntWorld.Enemies.Add(new Enemy(28, 5, 24));
var dcSnapHunt = OrbitalView.Snapshot(dcSnapHuntWorld)[0];
Eq(SquadActivity.Hunting, dcSnapHunt.Activity, "slice11 defendcore snapshot chasing attacker reads hunting");
Check(dcSnapHunt.HasHuntingTarget, "slice11 defendcore snapshot hunting has a target");
Eq(24, dcSnapHunt.HuntingTargetZ, "slice11 defendcore snapshot hunting target is the attacker cell");
var dcSnapMarchWorld = new WorldZone(42);
dcSnapMarchWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore);
Eq(SquadActivity.Marching, OrbitalView.Snapshot(dcSnapMarchWorld)[0].Activity, "slice11 defendcore snapshot returning reads marching");
var dcSnapFarWorld = new WorldZone(42);
dcSnapFarWorld.Player.X = 28; dcSnapFarWorld.Player.Z = 28;
dcSnapFarWorld.SpawnSquad(5, 5, 5, TacticalDoctrine.DefendCore);
dcSnapFarWorld.Enemies.Add(new Enemy(17, 5, 28));
var dcSnapFar = OrbitalView.Snapshot(dcSnapFarWorld)[0];
Eq(SquadActivity.Guarding, dcSnapFar.Activity, "slice11 defendcore snapshot far enemy does not turn guarding into hunting");
Check(!dcSnapFar.HasHuntingTarget, "slice11 defendcore snapshot far enemy is not a hunting target");

// ---- Slice 11 Step 3: composed demo scenario (0.51.0) ----
// Mirrors the Slice 11 demo scenario step for step: a Heavy
// Defend Core squad beside a Vanguard HoldGround squad, both
// through the paid picker, a lane enemy killed at the rally,
// the Heavy marching to the core and guarding it, a staged
// core attacker killed at the post, a weakened third squad
// dying in one tick, and a save/load round-trip in which
// kind and doctrine survive and the dead squad does not.
var s11Sim = new Simulation(42);
s11Sim.World.Player.Inventory.Add(BlockType.EmberOre, 30);
s11Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.DefendCore);
s11Sim.OrbitalCommand.SelectKind(SquadKind.Heavy);
s11Sim.BeginOrbitalPick();
s11Sim.SetOrbitalRally(20, 5, 20);
var s11Heavy = s11Sim.ConfirmOrbitalDeploy();
s11Sim.World.Player.X = 16; s11Sim.World.Player.Z = 8;
s11Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround);
s11Sim.OrbitalCommand.SelectKind(SquadKind.Vanguard);
s11Sim.BeginOrbitalPick();
s11Sim.SetOrbitalRally(20, 5, 8);
var s11Van = s11Sim.ConfirmOrbitalDeploy();
Check(s11Heavy != null, "slice11 step3 heavy defendcore deploys");
Check(s11Van != null, "slice11 step3 vanguard holdground deploys");
Eq(10, s11Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice11 step3 two paid squads leave 10 ore");
s11Sim.World.Player.X = 4; s11Sim.World.Player.Z = 4;
s11Sim.World.Enemies.Add(new Enemy(27, 5, 8));
for (int i = 0; i < 4; i++) s11Sim.World.Advance();
Eq(20, s11Van!.X, "slice11 step3 vanguard reaches rally in 4 ticks");
Eq(8, s11Van.Z, "slice11 step3 vanguard rally lane");
Check(s11Van.AtRally, "slice11 step3 vanguard at rally");
Eq(0, s11Sim.World.Enemies.Count, "slice11 step3 lane enemy killed at range 3 on tick 4");
Eq(18, s11Heavy!.X, "slice11 step3 heavy marches core-ward in 4 ticks");
Eq(18, s11Heavy.Z, "slice11 step3 heavy marches diagonally to core");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s11Sim.World)[0].Activity, "slice11 step3 heavy reads marching en route");
for (int i = 0; i < 20; i++) s11Sim.World.Advance();
Eq(28, s11Heavy.X, "slice11 step3 heavy reaches core in 24 ticks");
Eq(28, s11Heavy.Z, "slice11 step3 heavy post is the core cell");
Eq(SquadActivity.Guarding, OrbitalView.Snapshot(s11Sim.World)[0].Activity, "slice11 step3 heavy reads guarding at post");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s11Sim.World)[1].Activity, "slice11 step3 vanguard still holding at rally");
s11Sim.World.Enemies.Add(new Enemy(26, 5, 28));
var s11Hunt = OrbitalView.Snapshot(s11Sim.World)[0];
Eq(SquadActivity.Hunting, s11Hunt.Activity, "slice11 step3 staged core attacker reads hunting");
Eq(26, s11Hunt.HuntingTargetX, "slice11 step3 hunting target is the attacker cell");
s11Sim.World.Advance();
Eq(14, s11Sim.World.Enemies[0].Health, "slice11 step3 guard fires 16 at range 2 on first attacker tick");
Eq(90, s11Heavy.Health, "slice11 step3 guard takes one adjacent hit");
s11Sim.World.Advance();
Eq(0, s11Sim.World.Enemies.Count, "slice11 step3 core attacker killed in two ticks");
Eq(SquadActivity.Guarding, OrbitalView.Snapshot(s11Sim.World)[0].Activity, "slice11 step3 guard reads guarding again after the kill");
s11Sim.World.Player.X = 10; s11Sim.World.Player.Z = 10;
s11Sim.World.Player.Inventory.Add(BlockType.EmberOre, 10);
s11Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround);
s11Sim.OrbitalCommand.SelectKind(SquadKind.Vanguard);
s11Sim.BeginOrbitalPick();
s11Sim.SetOrbitalRally(10, 5, 10);
var s11Doomed = s11Sim.ConfirmOrbitalDeploy();
Check(s11Doomed != null, "slice11 step3 doomed squad deploys");
s11Doomed!.TakeDamage(80);
s11Sim.World.Enemies.Add(new Enemy(11, 5, 10));
s11Sim.World.Advance();
Eq(2, s11Sim.World.Squads.Count, "slice11 step3 dead squad removed in its tick");
Eq(20, s11Sim.World.Enemies[0].Health, "slice11 step3 doomed squad fires once for 10 before dying");
var s11Save = s11Sim.SaveToJson();
var s11Loaded = new Simulation(99);
Check(s11Loaded.LoadFromJson(s11Save), "slice11 step3 save loads");
Eq(2, s11Loaded.World.Squads.Count, "slice11 step3 dead squad stays dead after load");
Eq(SquadKind.Heavy, s11Loaded.World.Squads[0].Kind, "slice11 step3 loaded heavy kind preserved");
Eq(TacticalDoctrine.DefendCore, s11Loaded.World.Squads[0].Doctrine, "slice11 step3 loaded heavy doctrine preserved");
Eq(28, s11Loaded.World.Squads[0].X, "slice11 step3 loaded heavy still at core");
Eq(90, s11Loaded.World.Squads[0].Health, "slice11 step3 loaded heavy health preserved");
Eq(SquadKind.Vanguard, s11Loaded.World.Squads[1].Kind, "slice11 step3 loaded vanguard kind preserved");
Eq(TacticalDoctrine.HoldGround, s11Loaded.World.Squads[1].Doctrine, "slice11 step3 loaded vanguard doctrine preserved");
Eq(20, s11Loaded.World.Squads[1].X, "slice11 step3 loaded vanguard still at rally");
Eq(1, s11Loaded.World.Enemies.Count, "slice11 step3 surviving enemy loads");
Eq(20, s11Loaded.World.Enemies[0].Health, "slice11 step3 surviving enemy health preserved");
Check(s11Loaded.CanDeployOrbitalSquad(), "slice11 step3 cap freed by the death after load");
s11Loaded.World.Advance();
Eq(2, s11Loaded.World.Squads.Count, "slice11 step3 loaded tick keeps two squads");
Eq(SquadActivity.Guarding, OrbitalView.Snapshot(s11Loaded.World)[0].Activity, "slice11 step3 loaded heavy still guarding");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s11Loaded.World)[1].Activity, "slice11 step3 loaded vanguard still holding");

// ---- Slice 12 Step 1: live-network and lone-source display ----
// Staged on the y=20 plane (open air above generated terrain) via
// Chunk.TrySet, exactly as earlier electrical tests stage theirs.
var s12LiveWorld = new WorldZone(42);
Eq((0, 0), s12LiveWorld.LivePowerCounts(), "slice12 step1 empty world counts nothing");
s12LiveWorld.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
Eq((0, 1), s12LiveWorld.LivePowerCounts(), "slice12 step1 lone dynamo is one lone source");
s12LiveWorld.Chunk.TrySet(10, 20, 10, BlockType.Air);
s12LiveWorld.Chunk.TrySet(10, 20, 10, BlockType.Alternator);
Eq((0, 1), s12LiveWorld.LivePowerCounts(), "slice12 step1 lone alternator is one lone source");
s12LiveWorld.Chunk.TrySet(11, 20, 10, BlockType.Dynamo);
Eq((0, 2), s12LiveWorld.LivePowerCounts(), "slice12 step1 adjacent dynamos are two lone sources not one network");
s12LiveWorld.Chunk.TrySet(10, 20, 10, BlockType.Air);
s12LiveWorld.Chunk.TrySet(11, 20, 10, BlockType.Air);
s12LiveWorld.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
s12LiveWorld.Chunk.TrySet(11, 20, 10, BlockType.Wall);
Eq((0, 1), s12LiveWorld.LivePowerCounts(), "slice12 step1 dynamo beside a wall is still lone");

var s12CableWorld = new WorldZone(42);
s12CableWorld.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
s12CableWorld.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s12CableWorld.Chunk.TrySet(12, 20, 10, BlockType.Cable);
Eq((1, 0), s12CableWorld.LivePowerCounts(), "slice12 step1 live cable network counts one not cells");
var s12TenWorld = new WorldZone(42);
s12TenWorld.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
for (int i = 1; i <= 10; i++) s12TenWorld.Chunk.TrySet(10 + i, 20, 10, BlockType.Cable);
Eq((1, 0), s12TenWorld.LivePowerCounts(), "slice12 step1 ten-cable LV run is one network");
var s12DeadCableWorld = new WorldZone(42);
s12DeadCableWorld.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s12DeadCableWorld.Chunk.TrySet(12, 20, 10, BlockType.Cable);
Eq((0, 0), s12DeadCableWorld.LivePowerCounts(), "slice12 step1 cable with no source is not live");
var s12MixedWorld = new WorldZone(42);
s12MixedWorld.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
s12MixedWorld.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s12MixedWorld.Chunk.TrySet(12, 20, 10, BlockType.Alternator);
Eq((0, 0), s12MixedWorld.LivePowerCounts(), "slice12 step1 mixed-voltage cable network is dead not live");
var s12HvWorld = new WorldZone(42);
s12HvWorld.Chunk.TrySet(10, 20, 10, BlockType.Alternator);
s12HvWorld.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s12HvWorld.Chunk.TrySet(12, 20, 10, BlockType.Cable);
Eq((1, 0), s12HvWorld.LivePowerCounts(), "slice12 step1 HV network counts alike no voltage split");

var s12FedWorld = new WorldZone(42);
s12FedWorld.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
s12FedWorld.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s12FedWorld.Chunk.TrySet(12, 20, 10, BlockType.KineticMill);
Eq((0, 0), s12FedWorld.LivePowerCounts(), "slice12 step1 network feeding a mill is not counted twice");
Eq(1, HudState.FromWorld(s12FedWorld, new Weapon()).PoweredMills, "slice12 step1 fed mill still shows in the consumer count");
var s12HopWorld = new WorldZone(42);
s12HopWorld.Chunk.TrySet(10, 20, 10, BlockType.Alternator);
s12HopWorld.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s12HopWorld.Chunk.TrySet(12, 20, 10, BlockType.Transformer);
s12HopWorld.Chunk.TrySet(13, 20, 10, BlockType.KineticMill);
Eq((0, 0), s12HopWorld.LivePowerCounts(), "slice12 step1 network feeding a mill through one transformer is not counted twice");
Eq(Voltage.LV, s12HopWorld.DeliveredVoltage(13, 20, 10), "slice12 step1 step-down mill receives LV");
Eq(1, HudState.FromWorld(s12HopWorld, new Weapon()).PoweredMills, "slice12 step1 step-down mill shows powered");
var s12HopHvWorld = new WorldZone(42);
s12HopHvWorld.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
s12HopHvWorld.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s12HopHvWorld.Chunk.TrySet(12, 20, 10, BlockType.Transformer);
s12HopHvWorld.Chunk.TrySet(13, 20, 10, BlockType.KineticMill);
Eq((0, 0), s12HopHvWorld.LivePowerCounts(), "slice12 step1 network feeding a mill through a step-up transformer is not counted twice");
Eq(Voltage.HV, s12HopHvWorld.DeliveredVoltage(13, 20, 10), "slice12 step1 step-up mill receives HV");
var s12BenchWorld = new WorldZone(42);
s12BenchWorld.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
s12BenchWorld.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s12BenchWorld.Chunk.TrySet(12, 20, 10, BlockType.Workstation);
Eq((0, 0), s12BenchWorld.LivePowerCounts(), "slice12 step1 network feeding a workstation is not counted twice");
Eq(1, HudState.FromWorld(s12BenchWorld, new Weapon()).PoweredWorkstations, "slice12 step1 fed workstation shows powered");
var s12TwoNetWorld = new WorldZone(42);
s12TwoNetWorld.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
s12TwoNetWorld.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s12TwoNetWorld.Chunk.TrySet(12, 20, 10, BlockType.KineticMill);
s12TwoNetWorld.Chunk.TrySet(20, 20, 10, BlockType.Alternator);
s12TwoNetWorld.Chunk.TrySet(21, 20, 10, BlockType.Cable);
s12TwoNetWorld.Chunk.TrySet(22, 20, 10, BlockType.Cable);
Eq((1, 0), s12TwoNetWorld.LivePowerCounts(), "slice12 step1 exclusion is per network one fed one free");

var s12TxWorld = new WorldZone(42);
s12TxWorld.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
s12TxWorld.Chunk.TrySet(11, 20, 10, BlockType.Transformer);
Eq((1, 0), s12TxWorld.LivePowerCounts(), "slice12 step1 standalone transformer output is one live network");
var s12ChainWorld = new WorldZone(42);
s12ChainWorld.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
s12ChainWorld.Chunk.TrySet(11, 20, 10, BlockType.Transformer);
s12ChainWorld.Chunk.TrySet(12, 20, 10, BlockType.Transformer);
Eq((1, 0), s12ChainWorld.LivePowerCounts(), "slice12 step1 transformer chain is one network not one per stage");
var s12DeadTxWorld = new WorldZone(42);
s12DeadTxWorld.Chunk.TrySet(11, 20, 10, BlockType.Transformer);
Eq((0, 0), s12DeadTxWorld.LivePowerCounts(), "slice12 step1 unpowered transformer is not live");
var s12RingWorld = new WorldZone(42);
s12RingWorld.Chunk.TrySet(10, 20, 10, BlockType.Transformer);
s12RingWorld.Chunk.TrySet(11, 20, 10, BlockType.Transformer);
s12RingWorld.Chunk.TrySet(11, 20, 11, BlockType.Transformer);
s12RingWorld.Chunk.TrySet(10, 20, 11, BlockType.Transformer);
Eq((0, 0), s12RingWorld.LivePowerCounts(), "slice12 step1 sourceless transformer ring is not live");
var s12TxFedWorld = new WorldZone(42);
s12TxFedWorld.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
s12TxFedWorld.Chunk.TrySet(11, 20, 10, BlockType.Transformer);
s12TxFedWorld.Chunk.TrySet(12, 20, 10, BlockType.KineticMill);
Eq((0, 0), s12TxFedWorld.LivePowerCounts(), "slice12 step1 transformer feeding a mill is not counted twice");
Eq(Voltage.HV, s12TxFedWorld.DeliveredVoltage(12, 20, 10), "slice12 step1 direct transformer mill receives HV");
// Decided edge: a transformer touching a cable is never standalone,
// so its live output into a conflicted (dead) component shows nothing.
// The component is genuinely mixed: a Dynamo touches the cable (LV)
// while a second Dynamo drives the neighbouring transformer, whose
// flipped output is HV. A probe mill beside the cable proves the
// component itself resolves to None (a direct Dynamo neighbour
// would decide a probed cable cell to LV by precedence, so the
// probe must sit beside the cable, not on it).
var s12DeadEdgeWorld = new WorldZone(42);
s12DeadEdgeWorld.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
s12DeadEdgeWorld.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s12DeadEdgeWorld.Chunk.TrySet(12, 20, 10, BlockType.Transformer);
s12DeadEdgeWorld.Chunk.TrySet(13, 20, 10, BlockType.Dynamo);
s12DeadEdgeWorld.Chunk.TrySet(11, 20, 11, BlockType.KineticMill);
Eq(Voltage.None, s12DeadEdgeWorld.DeliveredVoltage(11, 20, 11), "slice12 step1 mixed component resolves to none");
Eq((0, 0), s12DeadEdgeWorld.LivePowerCounts(), "slice12 step1 live transformer output into a dead component shows nothing");

var s12DropWorld = new WorldZone(42);
s12DropWorld.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
s12DropWorld.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s12DropWorld.Chunk.TrySet(12, 20, 10, BlockType.Cable);
Eq((1, 0), s12DropWorld.LivePowerCounts(), "slice12 step1 live network counted before removal");
s12DropWorld.Chunk.TrySet(10, 20, 10, BlockType.Air);
Eq((0, 0), s12DropWorld.LivePowerCounts(), "slice12 step1 removing the source drops the count next snapshot");
var s12LiveSim = new Simulation(42);
s12LiveSim.World.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
s12LiveSim.World.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s12LiveSim.World.Chunk.TrySet(20, 20, 10, BlockType.Alternator);
var s12LiveSave = s12LiveSim.SaveToJson();
var s12LiveLoaded = new Simulation(99);
Check(s12LiveLoaded.LoadFromJson(s12LiveSave), "slice12 step1 live-power save loads");
Eq((1, 1), s12LiveLoaded.World.LivePowerCounts(), "slice12 step1 counts re-derive after load not saved");
var s12LiveHud = HudState.FromWorld(s12LiveLoaded.World, new Weapon());
Eq(1, s12LiveHud.LiveNetworks, "slice12 step1 hud carries live networks");
Eq(1, s12LiveHud.LoneSources, "slice12 step1 hud carries lone sources");

// ---- Slice 12 Step 2: third consumer, HV-rated Shock Pylon ----
// Rating, decided in this step: HV is the first consumer rating
// that is not LV, so the Alternator gains a safe direct use.
Eq(Voltage.HV, VoltageRules.PylonRated, "slice12 step2 pylon rated HV not LV");
var s12pDirectHv = new WorldZone(42);
s12pDirectHv.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
s12pDirectHv.Chunk.TrySet(15, 20, 16, BlockType.Alternator);
Eq(Voltage.HV, s12pDirectHv.DeliveredVoltage(16, 20, 16), "slice12 step2 direct alternator delivers HV to pylon");
Check(s12pDirectHv.IsPylonPowered(16, 20, 16), "slice12 step2 direct alternator pylon is powered");
Check(!s12pDirectHv.IsPylonMismatched(16, 20, 16), "slice12 step2 HV pylon is not mismatched");
var s12pDirectLv = new WorldZone(42);
s12pDirectLv.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
s12pDirectLv.Chunk.TrySet(15, 20, 16, BlockType.Dynamo);
Eq(Voltage.LV, s12pDirectLv.DeliveredVoltage(16, 20, 16), "slice12 step2 direct dynamo delivers LV to pylon");
Check(!s12pDirectLv.IsPylonPowered(16, 20, 16), "slice12 step2 LV pylon is not powered");
Check(s12pDirectLv.IsPylonMismatched(16, 20, 16), "slice12 step2 LV pylon is mismatched");
var s12pNone = new WorldZone(42);
s12pNone.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
Check(!s12pNone.IsPylonPowered(16, 20, 16), "slice12 step2 lone pylon is not powered");
Check(!s12pNone.IsPylonMismatched(16, 20, 16), "slice12 step2 lone pylon is not mismatched");
var s12pNonPylon = new WorldZone(42);
s12pNonPylon.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s12pNonPylon.Chunk.TrySet(17, 20, 16, BlockType.Workstation);
Check(!s12pNonPylon.IsPylonPowered(16, 20, 16), "slice12 step2 mill cell is not a powered pylon");
Check(!s12pNonPylon.IsPylonMismatched(17, 20, 16), "slice12 step2 workstation cell is not a mismatched pylon");
// Conversion both ways: step-up Dynamo -> transformer -> pylon
// is HV and powered; step-down Alternator -> transformer ->
// pylon is LV and mismatched, the mirror of the mill proofs.
var s12pStepUp = new WorldZone(42);
s12pStepUp.Chunk.TrySet(14, 20, 16, BlockType.Dynamo);
s12pStepUp.Chunk.TrySet(15, 20, 16, BlockType.Transformer);
s12pStepUp.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
Check(s12pStepUp.IsPylonPowered(16, 20, 16), "slice12 step2 stepped-up dynamo powers a pylon");
var s12pStepDown = new WorldZone(42);
s12pStepDown.Chunk.TrySet(14, 20, 16, BlockType.Alternator);
s12pStepDown.Chunk.TrySet(15, 20, 16, BlockType.Transformer);
s12pStepDown.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
Check(!s12pStepDown.IsPylonPowered(16, 20, 16), "slice12 step2 stepped-down alternator does not power a pylon");
Check(s12pStepDown.IsPylonMismatched(16, 20, 16), "slice12 step2 stepped-down alternator mismatches a pylon");
// Cable delivery both ways, like the conversion proofs.
var s12pCableHv = new WorldZone(42);
s12pCableHv.Chunk.TrySet(14, 20, 16, BlockType.Alternator);
s12pCableHv.Chunk.TrySet(15, 20, 16, BlockType.Cable);
s12pCableHv.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
Check(s12pCableHv.IsPylonPowered(16, 20, 16), "slice12 step2 cable-fed HV powers a pylon");
var s12pCableLv = new WorldZone(42);
s12pCableLv.Chunk.TrySet(14, 20, 16, BlockType.Dynamo);
s12pCableLv.Chunk.TrySet(15, 20, 16, BlockType.Cable);
s12pCableLv.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
Check(!s12pCableLv.IsPylonPowered(16, 20, 16), "slice12 step2 cable-fed LV does not power a pylon");
Check(s12pCableLv.IsPylonMismatched(16, 20, 16), "slice12 step2 cable-fed LV mismatches a pylon");
// Conflict: two distinct direct sources resolve to None, so the
// pylon is neither powered nor mismatched, and it survives.
var s12pConflict = new WorldZone(42);
s12pConflict.Chunk.TrySet(15, 20, 16, BlockType.Dynamo);
s12pConflict.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
s12pConflict.Chunk.TrySet(17, 20, 16, BlockType.Alternator);
Eq(Voltage.None, s12pConflict.DeliveredVoltage(16, 20, 16), "slice12 step2 conflicting direct sources deliver nothing to pylon");
Check(!s12pConflict.IsPylonPowered(16, 20, 16), "slice12 step2 conflicted pylon is not powered");
Check(!s12pConflict.IsPylonMismatched(16, 20, 16), "slice12 step2 conflicted pylon is not mismatched");
// Fire phase: a powered pylon deals 10 per tick to one enemy.
// Enemies are pocketed by a Wall under their move cell, so the
// enemy phase cannot move them out of range before the pylon
// phase fires: the player stands far below at (16,5,16), so an
// unblocked enemy would step down in Y first.
var s12pFire = new WorldZone(42);
s12pFire.Chunk.TrySet(15, 20, 16, BlockType.Alternator);
s12pFire.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
s12pFire.Chunk.TrySet(19, 19, 16, BlockType.Wall);
s12pFire.Enemies.Add(new Enemy(19, 20, 16));
s12pFire.Advance();
Eq(20, s12pFire.Enemies[0].Health, "slice12 step2 powered pylon deals 10 in one tick");
s12pFire.Advance();
Eq(10, s12pFire.Enemies[0].Health, "slice12 step2 powered pylon deals 10 again on tick two");
s12pFire.Advance();
Eq(0, s12pFire.Enemies.Count, "slice12 step2 pylon kills a 30-HP enemy in 3 ticks and removes it");
Eq(BlockType.ShockPylon, s12pFire.Chunk.Get(16, 20, 16), "slice12 step2 pylon survives its own kills");
// Range boundary: Manhattan 5 fires, Manhattan 6 does not.
var s12pRange5 = new WorldZone(42);
s12pRange5.Chunk.TrySet(15, 20, 16, BlockType.Alternator);
s12pRange5.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
s12pRange5.Chunk.TrySet(21, 19, 16, BlockType.Wall);
s12pRange5.Enemies.Add(new Enemy(21, 20, 16));
s12pRange5.Advance();
Eq(20, s12pRange5.Enemies[0].Health, "slice12 step2 enemy at range 5 takes pylon damage");
var s12pRange6 = new WorldZone(42);
s12pRange6.Chunk.TrySet(15, 20, 16, BlockType.Alternator);
s12pRange6.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
s12pRange6.Chunk.TrySet(22, 19, 16, BlockType.Wall);
s12pRange6.Enemies.Add(new Enemy(22, 20, 16));
s12pRange6.Advance();
Eq(30, s12pRange6.Enemies[0].Health, "slice12 step2 enemy at range 6 takes no pylon damage");
// Nearest only, and ties to the first in list order.
var s12pNearest = new WorldZone(42);
s12pNearest.Chunk.TrySet(15, 20, 16, BlockType.Alternator);
s12pNearest.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
s12pNearest.Chunk.TrySet(19, 19, 16, BlockType.Wall);
s12pNearest.Chunk.TrySet(21, 19, 16, BlockType.Wall);
s12pNearest.Enemies.Add(new Enemy(19, 20, 16));
s12pNearest.Enemies.Add(new Enemy(21, 20, 16));
s12pNearest.Advance();
Eq(20, s12pNearest.Enemies[0].Health, "slice12 step2 pylon hits only the nearest enemy");
Eq(30, s12pNearest.Enemies[1].Health, "slice12 step2 farther enemy in range is untouched when a nearer one exists");
var s12pTie = new WorldZone(42);
s12pTie.Chunk.TrySet(15, 20, 16, BlockType.Alternator);
s12pTie.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
s12pTie.Chunk.TrySet(16, 19, 19, BlockType.Wall);
s12pTie.Chunk.TrySet(19, 19, 16, BlockType.Wall);
s12pTie.Enemies.Add(new Enemy(16, 20, 19));
s12pTie.Enemies.Add(new Enemy(19, 20, 16));
s12pTie.Advance();
Eq(20, s12pTie.Enemies[0].Health, "slice12 step2 tied enemies resolve to the first in list order");
Eq(30, s12pTie.Enemies[1].Health, "slice12 step2 tied second enemy is untouched");
// Two pylons stack on one enemy: 20 in a tick.
var s12pStack = new WorldZone(42);
s12pStack.Chunk.TrySet(15, 20, 16, BlockType.Alternator);
s12pStack.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
s12pStack.Chunk.TrySet(16, 20, 19, BlockType.Alternator);
s12pStack.Chunk.TrySet(16, 20, 18, BlockType.ShockPylon);
s12pStack.Chunk.TrySet(19, 19, 17, BlockType.Wall);
s12pStack.Enemies.Add(new Enemy(19, 20, 17));
s12pStack.Advance();
Eq(10, s12pStack.Enemies[0].Health, "slice12 step2 two pylons stack 20 damage in one tick");
// Mismatch disables, never destroys, and rewiring recovers:
// LV-fed pylon leaves its enemy untouched and survives, then
// the same pylon fed HV fires on the next tick.
var s12pRewire = new WorldZone(42);
s12pRewire.Chunk.TrySet(15, 20, 16, BlockType.Dynamo);
s12pRewire.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
s12pRewire.Chunk.TrySet(19, 19, 16, BlockType.Wall);
s12pRewire.Enemies.Add(new Enemy(19, 20, 16));
s12pRewire.Advance();
Eq(30, s12pRewire.Enemies[0].Health, "slice12 step2 mismatched pylon fires nothing");
Eq(BlockType.ShockPylon, s12pRewire.Chunk.Get(16, 20, 16), "slice12 step2 mismatched pylon survives its disabled tick");
s12pRewire.Chunk.TrySet(15, 20, 16, BlockType.Air);
s12pRewire.Chunk.TrySet(15, 20, 16, BlockType.Alternator);
Check(s12pRewire.IsPylonPowered(16, 20, 16), "slice12 step2 rewired pylon is powered before its next tick");
s12pRewire.Advance();
Eq(20, s12pRewire.Enemies[0].Health, "slice12 step2 rewired pylon fires on the next tick");
// Unpowered pylon idles and survives, like an unpowered bench.
var s12pIdle = new WorldZone(42);
s12pIdle.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
s12pIdle.Chunk.TrySet(19, 19, 16, BlockType.Wall);
s12pIdle.Enemies.Add(new Enemy(19, 20, 16));
s12pIdle.Advance();
Eq(30, s12pIdle.Enemies[0].Health, "slice12 step2 unpowered pylon fires nothing");
Eq(BlockType.ShockPylon, s12pIdle.Chunk.Get(16, 20, 16), "slice12 step2 unpowered pylon survives");
// Build cost: 5 Stone + 10 EmberOre, check before spend.
var s12pBuild = new WorldZone(42);
s12pBuild.Chunk.TrySet(16, 19, 16, BlockType.Stone);
s12pBuild.Player.Inventory.Add(BlockType.Stone, 5);
s12pBuild.Player.Inventory.Add(BlockType.EmberOre, 10);
Eq(BuildResult.Success, s12pBuild.Build(16, 20, 16, BlockType.ShockPylon), "slice12 step2 pylon builds at its cost");
Eq(BlockType.ShockPylon, s12pBuild.Chunk.Get(16, 20, 16), "slice12 step2 pylon placed by build");
Eq(0, s12pBuild.Player.Inventory.Get(BlockType.Stone), "slice12 step2 pylon spends its stone");
Eq(0, s12pBuild.Player.Inventory.Get(BlockType.EmberOre), "slice12 step2 pylon spends its ore");
var s12pPoor = new WorldZone(42);
s12pPoor.Chunk.TrySet(16, 19, 16, BlockType.Stone);
s12pPoor.Player.Inventory.Add(BlockType.Stone, 5);
s12pPoor.Player.Inventory.Add(BlockType.EmberOre, 9);
Eq(BuildResult.InsufficientResources, s12pPoor.Build(16, 20, 16, BlockType.ShockPylon), "slice12 step2 pylon refused one ore short");
Eq(BlockType.Air, s12pPoor.Chunk.Get(16, 20, 16), "slice12 step2 refused pylon places nothing");
Eq(9, s12pPoor.Player.Inventory.Get(BlockType.EmberOre), "slice12 step2 refused pylon spends nothing");
// Aimed builder: same aim as the Slice 10 Step 2 alternator build.
var s12pSim = new Simulation(42);
s12pSim.Tick(0.1f, 0, 0, false);
s12pSim.World.Player.Inventory.Add(BlockType.Stone, 5);
s12pSim.World.Player.Inventory.Add(BlockType.EmberOre, 10);
s12pSim.ApplyLook(-18.4f, -53f);
Eq(BuildResult.Success, s12pSim.BuildShockPylonAhead(), "slice12 step2 simulation build pylon ahead succeeds");
Eq(BlockType.ShockPylon, s12pSim.World.Chunk.Get(15, 4, 17), "slice12 step2 simulation pylon placed by aim");
Eq(0, s12pSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice12 step2 simulation pylon spends its ore");
// A pylon is not gatherable: a gather attempt refuses it.
var s12pGather = new WorldZone(42);
s12pGather.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
Eq(GatherResult.NotGatherable, s12pGather.Gather(16, 20, 16), "slice12 step2 pylon is not gatherable");
// HUD counts: one powered pylon beside one lone pylon.
var s12pHudWorld = new WorldZone(42);
s12pHudWorld.Chunk.TrySet(15, 20, 16, BlockType.Alternator);
s12pHudWorld.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
s12pHudWorld.Chunk.TrySet(20, 20, 16, BlockType.ShockPylon);
var s12pHud = HudState.FromWorld(s12pHudWorld, new Weapon());
Eq(2, s12pHud.TotalPylons, "slice12 step2 hud counts both pylons");
Eq(1, s12pHud.PoweredPylons, "slice12 step2 hud counts only the HV-fed pylon as powered");
// Live-power anti-double-count, Step 1 rule extended: a network
// feeding a pylon is shown by the pylon count, not counted again;
// a lone pylon is not a source and counts as nothing live.
var s12pLiveFed = new WorldZone(42);
s12pLiveFed.Chunk.TrySet(14, 20, 16, BlockType.Alternator);
s12pLiveFed.Chunk.TrySet(15, 20, 16, BlockType.Cable);
s12pLiveFed.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
Eq((0, 0), s12pLiveFed.LivePowerCounts(), "slice12 step2 network feeding a pylon is not counted twice");
Eq((0, 0), s12pNone.LivePowerCounts(), "slice12 step2 lone pylon counts as no live network and no lone source");
var s12pLiveMixed = new WorldZone(42);
s12pLiveMixed.Chunk.TrySet(14, 20, 16, BlockType.Alternator);
s12pLiveMixed.Chunk.TrySet(15, 20, 16, BlockType.Cable);
s12pLiveMixed.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
s12pLiveMixed.Chunk.TrySet(20, 20, 16, BlockType.Alternator);
s12pLiveMixed.Chunk.TrySet(21, 20, 16, BlockType.Cable);
Eq((1, 0), s12pLiveMixed.LivePowerCounts(), "slice12 step2 one fed plus one free network reads exactly one live network");
// Save/load: chunk bytes carry the pylon, power re-derives,
// and the loaded pylon fires on its first tick.
var s12pSaveWorld = new WorldZone(42);
s12pSaveWorld.Chunk.TrySet(15, 20, 16, BlockType.Alternator);
s12pSaveWorld.Chunk.TrySet(16, 20, 16, BlockType.ShockPylon);
var s12pSaveLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(s12pSaveWorld));
Check(s12pSaveLoaded.Chunk.ToBytes().SequenceEqual(s12pSaveWorld.Chunk.ToBytes()), "slice12 step2 save/load chunk equal with pylon");
Check(s12pSaveLoaded.IsPylonPowered(16, 20, 16), "slice12 step2 loaded pylon is powered from placement");
s12pSaveLoaded.Chunk.TrySet(19, 19, 16, BlockType.Wall);
s12pSaveLoaded.Enemies.Add(new Enemy(19, 20, 16));
s12pSaveLoaded.Advance();
Eq(20, s12pSaveLoaded.Enemies[0].Health, "slice12 step2 loaded pylon fires on its first tick");

// ---- Slice 12 Step 3: composed demo scenario (0.55.0) ----
// Mirrors the Slice 12 demo scenario step for step: a live
// network feeding no consumer and a lone source read only
// through the Step 1 counts, beside an HV pylon, an LV mill,
// and a powered workstation, through a save/load round-trip.
// Staged with TrySet, exactly as the Step 1-2 tests stage
// theirs; aim placement of each block was proved in its own
// step's tests and is not re-proved here.
var s12Sim = new Simulation(42);
void SeedPocketS12Test(WorldZone w, int mx, int mz)
{
    foreach (var (x, y, z) in new[] { (mx - 1, 20, mz), (mx, 21, mz), (mx, 19, mz), (mx, 20, mz - 1), (mx, 20, mz + 1) })
        w.Chunk.TrySet(x, y, z, BlockType.Stone);
}
s12Sim.World.Chunk.TrySet(14, 20, 10, BlockType.Dynamo);
s12Sim.World.Chunk.TrySet(13, 20, 10, BlockType.Cable);
s12Sim.World.Chunk.TrySet(12, 20, 10, BlockType.Cable);
s12Sim.World.Chunk.TrySet(10, 20, 14, BlockType.KineticMill);
s12Sim.World.Chunk.TrySet(11, 20, 14, BlockType.Dynamo);
SeedPocketS12Test(s12Sim.World, 10, 14);
s12Sim.World.Chunk.TrySet(10, 20, 18, BlockType.Alternator);
s12Sim.World.Chunk.TrySet(17, 5, 16, BlockType.ShockPylon);
s12Sim.World.Chunk.TrySet(18, 5, 16, BlockType.Alternator);
s12Sim.World.Chunk.TrySet(17, 5, 28, BlockType.Workstation);
s12Sim.World.Chunk.TrySet(18, 5, 28, BlockType.Transformer);
s12Sim.World.Chunk.TrySet(19, 5, 28, BlockType.Alternator);
Eq(Voltage.LV, s12Sim.World.DeliveredVoltage(10, 20, 14), "slice12 step3 composed LV mill voltage");
Eq(Voltage.HV, s12Sim.World.DeliveredVoltage(17, 5, 16), "slice12 step3 composed HV pylon voltage");
Eq(Voltage.LV, s12Sim.World.DeliveredVoltage(17, 5, 28), "slice12 step3 composed LV workstation voltage");
var s12HudBefore = s12Sim.Hud();
Eq(1, s12HudBefore.PoweredMills, "slice12 step3 composed one mill powered");
Eq(1, s12HudBefore.TotalMills, "slice12 step3 composed one mill total");
Eq(1, s12HudBefore.PoweredWorkstations, "slice12 step3 composed one workstation powered");
Eq(1, s12HudBefore.TotalWorkstations, "slice12 step3 composed one workstation total");
Eq(1, s12HudBefore.PoweredPylons, "slice12 step3 composed one pylon powered");
Eq(1, s12HudBefore.TotalPylons, "slice12 step3 composed one pylon total");
Eq(1, s12HudBefore.LiveNetworks, "slice12 step3 composed one live network feeding no consumer");
Eq(1, s12HudBefore.LoneSources, "slice12 step3 composed one lone source");
Check(s12Sim.World.IsPylonPowered(17, 5, 16), "slice12 step3 composed pylon is powered, not just placed");
Check(!s12Sim.World.IsPylonMismatched(17, 5, 16), "slice12 step3 composed pylon is not mismatched");
s12Sim.World.Player.TakeDamage(20);
Eq(65, s12Sim.World.Player.Shields, "slice12 step3 composed shields 65 after 20 damage");
s12Sim.World.Player.X = 10; s12Sim.World.Player.Z = 10;
s12Sim.World.Enemies.Add(new Enemy(17, 5, 21));
s12Sim.World.Advance();
Eq(20, s12Sim.World.Enemies[0].Health, "slice12 step3 composed pylon fires 10 on tick 1 at range 4 after the enemy step");
Eq(65, s12Sim.World.Player.Shields, "slice12 step3 composed no bench regen while parked far from the workstation");
s12Sim.World.Advance();
Eq(10, s12Sim.World.Enemies[0].Health, "slice12 step3 composed pylon fires 10 on tick 2");
s12Sim.World.Advance();
Eq(0, s12Sim.World.Enemies.Count, "slice12 step3 composed pylon kills the 30-HP enemy in 3 ticks");
Eq(3, s12Sim.World.PoweredBlocksGathered, "slice12 step3 composed LV mill gathers 3 in 3 ticks");
Check(s12Sim.World.Player.MoveTo(16, 5, 28, s12Sim.World.Chunk), "slice12 step3 composed player walks to the powered bench");
Eq(10, s12Sim.World.EffectiveWorkstationRegen(), "slice12 step3 composed powered bench regen is base 5 plus powered 5");
s12Sim.World.Advance();
s12Sim.World.Advance();
Eq(5, s12Sim.World.PoweredBlocksGathered, "slice12 step3 composed LV mill gathers 5 in 5 ticks, pocket exhausted");
Eq(0, s12Sim.World.DestroyedMills, "slice12 step3 composed no mill destroyed by the HV pylon wiring");
Eq(5, s12Sim.World.Player.Inventory.Get(BlockType.Stone), "slice12 step3 composed stone is the mill pocket only, no refund or extra");
Eq(85, s12Sim.World.Player.Shields, "slice12 step3 composed bench regen caps shields at 85 after two bench ticks");
var s12Save = s12Sim.SaveToJson();
var s12Loaded = new Simulation(99);
Check(s12Loaded.LoadFromJson(s12Save), "slice12 step3 composed save loads");
Eq(BlockType.Cable, s12Loaded.World.Chunk.Get(12, 20, 10), "slice12 step3 composed live network cable loads");
Eq(BlockType.Alternator, s12Loaded.World.Chunk.Get(10, 20, 18), "slice12 step3 composed lone Alternator loads");
Eq(BlockType.KineticMill, s12Loaded.World.Chunk.Get(10, 20, 14), "slice12 step3 composed LV mill loads");
Eq(BlockType.ShockPylon, s12Loaded.World.Chunk.Get(17, 5, 16), "slice12 step3 composed pylon loads");
Eq(BlockType.Workstation, s12Loaded.World.Chunk.Get(17, 5, 28), "slice12 step3 composed workstation loads");
var s12LoadedHud = s12Loaded.Hud();
Eq(1, s12LoadedHud.PoweredMills, "slice12 step3 composed loaded mill still powered");
Eq(1, s12LoadedHud.PoweredWorkstations, "slice12 step3 composed loaded workstation still powered");
Eq(1, s12LoadedHud.PoweredPylons, "slice12 step3 composed loaded pylon still powered");
Eq(1, s12LoadedHud.LiveNetworks, "slice12 step3 composed live network count re-derives after load");
Eq(1, s12LoadedHud.LoneSources, "slice12 step3 composed lone source count re-derives after load");
Eq(0, s12Loaded.World.PoweredBlocksGathered, "slice12 step3 composed session gather counter resets after load");
Eq(0, s12Loaded.World.DestroyedMills, "slice12 step3 composed session destruction counter resets after load");
Eq(16, s12Loaded.World.Player.X, "slice12 step3 composed loaded player back at the bench");
Eq(28, s12Loaded.World.Player.Z, "slice12 step3 composed loaded player bench lane");
Eq(10, s12Loaded.World.EffectiveWorkstationRegen(), "slice12 step3 composed loaded bench regen re-derives");
s12Loaded.World.Player.TakeDamage(20);
Eq(65, s12Loaded.World.Player.Shields, "slice12 step3 composed loaded shields 65 after 20 damage");
SeedPocketS12Test(s12Loaded.World, 10, 14);
for (int i = 0; i < 5; i++) s12Loaded.World.Advance();
Eq(5, s12Loaded.World.PoweredBlocksGathered, "slice12 step3 composed loaded mill gathers 5 again in 5 ticks");
Eq(0, s12Loaded.World.DestroyedMills, "slice12 step3 composed loaded phase destroys nothing");
Eq(10, s12Loaded.World.Player.Inventory.Get(BlockType.Stone), "slice12 step3 composed loaded stone is 5 plus 5, no refund on load");
Eq(85, s12Loaded.World.Player.Shields, "slice12 step3 composed loaded bench regen caps shields at 85");
Eq(0, s12Loaded.World.Enemies.Count, "slice12 step3 composed pylon kill stays dead after load");

// ---- Slice 13 Step 1: pre-destruction / pre-disablement warning (0.57.0) ----
// The warning is a derived HudState readout of the three existing
// mismatch predicates, taken on a snapshot between placement and
// tick — never a delay, grace period, or salvage rule. Staged
// with TrySet on the y=20 plane, as the Slice 12 tests stage
// theirs; aim placement was proved in its own step's tests.
var s13Fresh = new Simulation(42);
var s13FreshHud = s13Fresh.Hud();
Eq(0, s13FreshHud.WarnedMills, "slice13 step1 fresh world warns no mills");
Eq(0, s13FreshHud.WarnedWorkstations, "slice13 step1 fresh world warns no workstations");
Eq(0, s13FreshHud.WarnedPylons, "slice13 step1 fresh world warns no pylons");
// Mill: HV warns before the tick, destruction removes the warning.
var s13Mill = new Simulation(42);
s13Mill.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
s13Mill.World.Chunk.TrySet(11, 20, 10, BlockType.Alternator);
Check(s13Mill.World.IsMismatched(10, 20, 10), "slice13 step1 HV mill is mismatched before the tick");
var s13MillHud = s13Mill.Hud();
Eq(1, s13MillHud.WarnedMills, "slice13 step1 HV mill warns one mill before the tick");
Eq(0, s13MillHud.WarnedWorkstations, "slice13 step1 HV mill warns no workstation");
Eq(0, s13MillHud.WarnedPylons, "slice13 step1 HV mill warns no pylon");
Eq(0, s13MillHud.PoweredMills, "slice13 step1 HV mill is not counted powered");
s13Mill.World.Advance();
Eq(BlockType.Air, s13Mill.World.Chunk.Get(10, 20, 10), "slice13 step1 warned mill is destroyed on the tick, warning postpones nothing");
Eq(1, s13Mill.World.DestroyedMills, "slice13 step1 warned mill destruction counter advances");
Eq(0, s13Mill.Hud().WarnedMills, "slice13 step1 destroyed mill warns no more");
// Workstation: HV warns before the tick, survives disabled, clears on rewiring.
var s13Bench = new Simulation(42);
s13Bench.World.Chunk.TrySet(10, 20, 10, BlockType.Workstation);
s13Bench.World.Chunk.TrySet(11, 20, 10, BlockType.Alternator);
Eq(1, s13Bench.Hud().WarnedWorkstations, "slice13 step1 HV workstation warns one workstation before the tick");
s13Bench.World.Advance();
Eq(BlockType.Workstation, s13Bench.World.Chunk.Get(10, 20, 10), "slice13 step1 warned workstation survives its disablement tick");
Eq(1, s13Bench.Hud().WarnedWorkstations, "slice13 step1 warned workstation still warns while it stays mismatched");
s13Bench.World.Chunk.TrySet(11, 20, 10, BlockType.Air);
s13Bench.World.Chunk.TrySet(11, 20, 10, BlockType.Dynamo);
Eq(0, s13Bench.Hud().WarnedWorkstations, "slice13 step1 rewired workstation warning clears on the next snapshot");
Eq(1, s13Bench.Hud().PoweredWorkstations, "slice13 step1 rewired workstation counts powered again");
// Pylon: LV warns before the tick, survives disabled, clears on rewiring.
var s13Pylon = new Simulation(42);
s13Pylon.World.Chunk.TrySet(10, 20, 10, BlockType.ShockPylon);
s13Pylon.World.Chunk.TrySet(11, 20, 10, BlockType.Dynamo);
Eq(1, s13Pylon.Hud().WarnedPylons, "slice13 step1 LV pylon warns one pylon before the tick");
s13Pylon.World.Advance();
Eq(BlockType.ShockPylon, s13Pylon.World.Chunk.Get(10, 20, 10), "slice13 step1 warned pylon survives its disablement tick");
Eq(1, s13Pylon.Hud().WarnedPylons, "slice13 step1 warned pylon still warns while it stays mismatched");
s13Pylon.World.Chunk.TrySet(11, 20, 10, BlockType.Air);
s13Pylon.World.Chunk.TrySet(11, 20, 10, BlockType.Alternator);
Eq(0, s13Pylon.Hud().WarnedPylons, "slice13 step1 rewired pylon warning clears on the next snapshot");
Eq(1, s13Pylon.Hud().PoweredPylons, "slice13 step1 rewired pylon counts powered again");
// Rewiring a mill before the tick saves it: the warning is a
// readout of current wiring, not a sentence already passed.
var s13Save = new Simulation(42);
s13Save.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
s13Save.World.Chunk.TrySet(11, 20, 10, BlockType.Alternator);
Eq(1, s13Save.Hud().WarnedMills, "slice13 step1 mill warns before rewiring");
s13Save.World.Chunk.TrySet(11, 20, 10, BlockType.Air);
Eq(0, s13Save.Hud().WarnedMills, "slice13 step1 mill warning clears when its HV source is removed");
s13Save.World.Advance();
Eq(BlockType.KineticMill, s13Save.World.Chunk.Get(10, 20, 10), "slice13 step1 rewired mill survives the tick");
Eq(0, s13Save.World.DestroyedMills, "slice13 step1 rewired mill destroys nothing");
// All three outcomes together, beside powered, lone, and
// conflicted machines that must not warn: the split counts,
// not one folded number, are what the player reads.
var s13All = new Simulation(42);
s13All.World.Chunk.TrySet(6, 20, 10, BlockType.KineticMill);
s13All.World.Chunk.TrySet(7, 20, 10, BlockType.Alternator);
s13All.World.Chunk.TrySet(6, 20, 16, BlockType.Workstation);
s13All.World.Chunk.TrySet(7, 20, 16, BlockType.Alternator);
s13All.World.Chunk.TrySet(6, 20, 22, BlockType.ShockPylon);
s13All.World.Chunk.TrySet(7, 20, 22, BlockType.Dynamo);
s13All.World.Chunk.TrySet(14, 20, 10, BlockType.KineticMill);
s13All.World.Chunk.TrySet(15, 20, 10, BlockType.Dynamo);
s13All.World.Chunk.TrySet(14, 20, 16, BlockType.Workstation);
s13All.World.Chunk.TrySet(15, 20, 16, BlockType.Dynamo);
s13All.World.Chunk.TrySet(14, 20, 22, BlockType.ShockPylon);
s13All.World.Chunk.TrySet(15, 20, 22, BlockType.Alternator);
s13All.World.Chunk.TrySet(22, 20, 10, BlockType.KineticMill);
s13All.World.Chunk.TrySet(22, 20, 16, BlockType.Workstation);
s13All.World.Chunk.TrySet(22, 20, 22, BlockType.ShockPylon);
s13All.World.Chunk.TrySet(28, 20, 10, BlockType.KineticMill);
s13All.World.Chunk.TrySet(27, 20, 10, BlockType.Dynamo);
s13All.World.Chunk.TrySet(29, 20, 10, BlockType.Alternator);
Eq(Voltage.None, s13All.World.DeliveredVoltage(28, 20, 10), "slice13 step1 Dynamo-plus-Alternator mill delivers None, conflicted not mismatched");
var s13AllHud = s13All.Hud();
Eq(1, s13AllHud.WarnedMills, "slice13 step1 mixed world warns exactly one mill");
Eq(1, s13AllHud.WarnedWorkstations, "slice13 step1 mixed world warns exactly one workstation");
Eq(1, s13AllHud.WarnedPylons, "slice13 step1 mixed world warns exactly one pylon");
Eq(4, s13AllHud.TotalMills, "slice13 step1 mixed world totals four mills");
Eq(1, s13AllHud.PoweredMills, "slice13 step1 mixed world powers exactly one mill");
Eq(3, s13AllHud.TotalWorkstations, "slice13 step1 mixed world totals three workstations");
Eq(1, s13AllHud.PoweredWorkstations, "slice13 step1 mixed world powers exactly one workstation");
Eq(3, s13AllHud.TotalPylons, "slice13 step1 mixed world totals three pylons");
Eq(1, s13AllHud.PoweredPylons, "slice13 step1 mixed world powers exactly one pylon");
// Cable-fed mismatch warns too: the readout follows
// DeliveredVoltage, not just face-adjacent sources.
var s13Cable = new Simulation(42);
s13Cable.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
s13Cable.World.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s13Cable.World.Chunk.TrySet(12, 20, 10, BlockType.Alternator);
Eq(Voltage.HV, s13Cable.World.DeliveredVoltage(10, 20, 10), "slice13 step1 cable-fed mill delivers HV");
Eq(1, s13Cable.Hud().WarnedMills, "slice13 step1 cable-fed HV mill warns");
// Non-consumers never warn, however live their wiring looks.
var s13None = new Simulation(42);
s13None.World.Chunk.TrySet(10, 20, 10, BlockType.Dynamo);
s13None.World.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s13None.World.Chunk.TrySet(12, 20, 10, BlockType.Transformer);
s13None.World.Chunk.TrySet(20, 20, 20, BlockType.Alternator);
var s13NoneHud = s13None.Hud();
Eq(0, s13NoneHud.WarnedMills, "slice13 step1 source and cable world warns no mills");
Eq(0, s13NoneHud.WarnedWorkstations, "slice13 step1 source and cable world warns no workstations");
Eq(0, s13NoneHud.WarnedPylons, "slice13 step1 source and cable world warns no pylons");
// Warning re-derives after save/load: it is never saved.
var s13Load = new Simulation(42);
s13Load.World.Chunk.TrySet(10, 20, 10, BlockType.Workstation);
s13Load.World.Chunk.TrySet(11, 20, 10, BlockType.Alternator);
var s13LoadJson = s13Load.SaveToJson();
var s13Loaded = new Simulation(99);
Check(s13Loaded.LoadFromJson(s13LoadJson), "slice13 step1 warned world loads");
Eq(1, s13Loaded.Hud().WarnedWorkstations, "slice13 step1 warning re-derives after load");

// ---- Slice 13 Step 3: mesh tint and mesh refresh for power state (0.59.0) ----
// The query is tested; the tint and the refresh wiring are adapter
// code, so their proof is playtest-gate items 26-27, not these
// tests — but the query, the tint channel, and the tint/warning
// agreement are all headless claims, and they are proved here.
var s13Tint = new Simulation(42);
s13Tint.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
s13Tint.World.Chunk.TrySet(11, 20, 10, BlockType.Dynamo);
s13Tint.World.Chunk.TrySet(10, 20, 16, BlockType.KineticMill);
s13Tint.World.Chunk.TrySet(11, 20, 16, BlockType.Alternator);
s13Tint.World.Chunk.TrySet(10, 20, 22, BlockType.KineticMill);
s13Tint.World.Chunk.TrySet(16, 20, 10, BlockType.KineticMill);
s13Tint.World.Chunk.TrySet(15, 20, 10, BlockType.Dynamo);
s13Tint.World.Chunk.TrySet(17, 20, 10, BlockType.Alternator);
s13Tint.World.Chunk.TrySet(20, 20, 10, BlockType.Workstation);
s13Tint.World.Chunk.TrySet(21, 20, 10, BlockType.Dynamo);
s13Tint.World.Chunk.TrySet(20, 20, 16, BlockType.Workstation);
s13Tint.World.Chunk.TrySet(21, 20, 16, BlockType.Alternator);
s13Tint.World.Chunk.TrySet(20, 20, 22, BlockType.ShockPylon);
s13Tint.World.Chunk.TrySet(21, 20, 22, BlockType.Alternator);
s13Tint.World.Chunk.TrySet(26, 20, 10, BlockType.ShockPylon);
s13Tint.World.Chunk.TrySet(27, 20, 10, BlockType.Dynamo);
s13Tint.World.Chunk.TrySet(26, 20, 16, BlockType.Cable);
s13Tint.World.Chunk.TrySet(27, 20, 16, BlockType.Dynamo);
s13Tint.World.Chunk.TrySet(26, 20, 22, BlockType.Cable);
s13Tint.World.Chunk.TrySet(16, 20, 22, BlockType.Cable);
s13Tint.World.Chunk.TrySet(15, 20, 22, BlockType.Dynamo);
s13Tint.World.Chunk.TrySet(17, 20, 22, BlockType.Alternator);
s13Tint.World.Chunk.TrySet(6, 20, 10, BlockType.Transformer);
s13Tint.World.Chunk.TrySet(5, 20, 10, BlockType.Dynamo);
s13Tint.World.Chunk.TrySet(6, 20, 16, BlockType.Dynamo);
s13Tint.World.Chunk.TrySet(6, 20, 22, BlockType.Stone);
Eq(CellPowerState.Powered, s13Tint.World.PowerState(10, 20, 10), "slice13 step3 LV mill is Powered");
Eq(CellPowerState.Mismatched, s13Tint.World.PowerState(10, 20, 16), "slice13 step3 HV mill is Mismatched");
Eq(CellPowerState.None, s13Tint.World.PowerState(10, 20, 22), "slice13 step3 unpowered mill is None");
Eq(CellPowerState.None, s13Tint.World.PowerState(16, 20, 10), "slice13 step3 conflicted mill is None, not Mismatched");
Eq(CellPowerState.Powered, s13Tint.World.PowerState(20, 20, 10), "slice13 step3 LV workstation is Powered");
Eq(CellPowerState.Mismatched, s13Tint.World.PowerState(20, 20, 16), "slice13 step3 HV workstation is Mismatched");
Eq(CellPowerState.Powered, s13Tint.World.PowerState(20, 20, 22), "slice13 step3 HV pylon is Powered");
Eq(CellPowerState.Mismatched, s13Tint.World.PowerState(26, 20, 10), "slice13 step3 LV pylon is Mismatched");
Eq(CellPowerState.Energised, s13Tint.World.PowerState(26, 20, 16), "slice13 step3 Dynamo-fed cable is Energised");
Eq(CellPowerState.None, s13Tint.World.PowerState(26, 20, 22), "slice13 step3 dead cable is None");
Eq(CellPowerState.None, s13Tint.World.PowerState(16, 20, 22), "slice13 step3 conflicted cable is None");
Eq(CellPowerState.None, s13Tint.World.PowerState(6, 20, 10), "slice13 step3 converting transformer is None, not Energised");
Eq(CellPowerState.None, s13Tint.World.PowerState(6, 20, 16), "slice13 step3 lone source is None, not Energised");
Eq(CellPowerState.None, s13Tint.World.PowerState(6, 20, 22), "slice13 step3 stone is None");
Eq(CellPowerState.None, s13Tint.World.PowerState(0, 20, 0), "slice13 step3 air is None");
// Tint and warning agree, because both read the same predicates:
// exactly the one mismatched machine of each kind warns.
var s13TintHud = s13Tint.Hud();
Eq(1, s13TintHud.WarnedMills, "slice13 step3 tint world warns exactly one mill");
Eq(1, s13TintHud.WarnedWorkstations, "slice13 step3 tint world warns exactly one workstation");
Eq(1, s13TintHud.WarnedPylons, "slice13 step3 tint world warns exactly one pylon");
// Rewiring clears the tint state on the next query, exactly as
// it clears the warning on the next snapshot — tint is derived,
// never saved, and never a grace period.
var s13Rewire = new Simulation(42);
s13Rewire.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
s13Rewire.World.Chunk.TrySet(11, 20, 10, BlockType.Alternator);
Eq(CellPowerState.Mismatched, s13Rewire.World.PowerState(10, 20, 10), "slice13 step3 HV mill tinted Mismatched before rewiring");
Eq(1, s13Rewire.Hud().WarnedMills, "slice13 step3 HV mill warns before rewiring");
s13Rewire.World.Chunk.TrySet(11, 20, 10, BlockType.Air);
Eq(CellPowerState.None, s13Rewire.World.PowerState(10, 20, 10), "slice13 step3 mill tint clears when its HV source is removed");
Eq(0, s13Rewire.Hud().WarnedMills, "slice13 step3 mill warning clears with its tint");
// Colour channel: one colour per vertex, tint values as decided,
// untinted builds all white, and the tinted build carries all
// three tints for the mixed world above.
Eq((0.25f, 0.9f, 0.25f, 1f), PowerTint.ColorFor(CellPowerState.Powered), "slice13 step3 Powered tint is green, not cyan");
Eq((1f, 0.15f, 0.15f, 1f), PowerTint.ColorFor(CellPowerState.Mismatched), "slice13 step3 Mismatched tint is red, not orange");
Eq((1f, 0.85f, 0.1f, 1f), PowerTint.ColorFor(CellPowerState.Energised), "slice13 step3 Energised tint is yellow");
Eq((1f, 1f, 1f, 1f), PowerTint.ColorFor(CellPowerState.None), "slice13 step3 None tint is white");
var s13Quads = Mesher.GreedyMesh(s13Tint.World.Chunk, s13Tint.World.PowerState);
var s13Mesh = MeshBuilder.Build(s13Quads);
Eq(s13Mesh.Vertices.Count, s13Mesh.Colors.Count, "slice13 step3 colours count equals vertices count");
Check(s13Quads.Any(q => q.Type == BlockType.KineticMill && q.PowerState == CellPowerState.Powered), "slice13 step3 tinted quads carry a Powered mill");
Check(s13Quads.Any(q => q.Type == BlockType.KineticMill && q.PowerState == CellPowerState.Mismatched), "slice13 step3 tinted quads carry a Mismatched mill");
Check(s13Quads.Any(q => q.Type == BlockType.Workstation && q.PowerState == CellPowerState.Powered), "slice13 step3 tinted quads carry a Powered workstation");
Check(s13Quads.Any(q => q.Type == BlockType.ShockPylon && q.PowerState == CellPowerState.Mismatched), "slice13 step3 tinted quads carry a Mismatched pylon");
Check(s13Quads.Any(q => q.Type == BlockType.Cable && q.PowerState == CellPowerState.Energised), "slice13 step3 tinted quads carry an Energised cable");
var s13Untinted = MeshBuilder.BuildFromChunk(s13Tint.World.Chunk);
Check(s13Untinted.Colors.All(c => c == (1f, 1f, 1f, 1f)), "slice13 step3 untinted build is all white");
var s13TintedMesh = MeshBuilder.BuildFromChunk(s13Tint.World.Chunk, s13Tint.World.PowerState);
Eq(s13Quads.Count, s13TintedMesh.QuadCount, "slice13 step3 tinted BuildFromChunk quad count matches tinted mesher");
Check(s13TintedMesh.Colors.Contains((0.25f, 0.9f, 0.25f, 1f)), "slice13 step3 tinted mesh contains green");
Check(s13TintedMesh.Colors.Contains((1f, 0.15f, 0.15f, 1f)), "slice13 step3 tinted mesh contains red");
Check(s13TintedMesh.Colors.Contains((1f, 0.85f, 0.1f, 1f)), "slice13 step3 tinted mesh contains yellow");
// Merge separation: two adjacent mills, one powered and one
// mismatched, must NOT merge their top faces into one quad.
var s13Merge = new Simulation(42);
s13Merge.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
s13Merge.World.Chunk.TrySet(9, 20, 10, BlockType.Dynamo);
s13Merge.World.Chunk.TrySet(11, 20, 10, BlockType.KineticMill);
s13Merge.World.Chunk.TrySet(12, 20, 10, BlockType.Alternator);
var s13MergeTinted = Mesher.GreedyMesh(s13Merge.World.Chunk, s13Merge.World.PowerState)
    .Where(q => q.Type == BlockType.KineticMill && q.Axis == 1 && q.Slice == 20 && q.Positive).ToList();
var s13MergeUntinted = Mesher.GreedyMesh(s13Merge.World.Chunk)
    .Where(q => q.Type == BlockType.KineticMill && q.Axis == 1 && q.Slice == 20 && q.Positive).ToList();
Eq(2, s13MergeTinted.Count, "slice13 step3 adjacent powered and mismatched mills keep separate top quads");
Eq(1, s13MergeUntinted.Count, "slice13 step3 same mills merge to one top quad without a power provider");
Check(s13MergeTinted.Any(q => q.PowerState == CellPowerState.Powered), "slice13 step3 separated top quads include Powered");
Check(s13MergeTinted.Any(q => q.PowerState == CellPowerState.Mismatched), "slice13 step3 separated top quads include Mismatched");
// Power state re-derives after save/load, like the warning.
var s13TintLoad = new Simulation(42);
s13TintLoad.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
s13TintLoad.World.Chunk.TrySet(11, 20, 10, BlockType.Dynamo);
s13TintLoad.World.Chunk.TrySet(20, 20, 10, BlockType.ShockPylon);
s13TintLoad.World.Chunk.TrySet(21, 20, 10, BlockType.Dynamo);
var s13TintLoadJson = s13TintLoad.SaveToJson();
var s13TintLoaded = new Simulation(99);
Check(s13TintLoaded.LoadFromJson(s13TintLoadJson), "slice13 step3 tinted world loads");
Eq(CellPowerState.Powered, s13TintLoaded.World.PowerState(10, 20, 10), "slice13 step3 Powered re-derives after load");
Eq(CellPowerState.Mismatched, s13TintLoaded.World.PowerState(20, 20, 10), "slice13 step3 Mismatched re-derives after load");

// ---- Silence instruction (recorded, not a test) ----


// ---- Slice 13 Step 4: composed demo scenario (0.60.0) ----
// Mirrors the Slice 13 demo scenario step for step: a warning
// reading a threatened mill, a threatened workstation, and a
// threatened pylon in one Simulation, beside the Step 2
// snapshot kinds and the Step 3 power-state query reading the
// same cells the warning reads, through a save/load
// round-trip. Staged with TrySet, exactly as the Step 1-3
// tests stage theirs; aim placement of each block was proved
// in its own step's tests and is not re-proved here. Two
// composition facts are pinned so they are not over-claimed
// later: squads deploy at the player and march to their
// rallies, so the snapshot reads Marching, not Holding, at
// every point in this run; and the destroyed mill's
// Alternator becomes a lone source after the tick, so the
// lone-source count reads 1 after load where it read 0
// before the tick — an orphaned source, not a new source.
var s13fSim = new Simulation(42);
s13fSim.World.Player.Inventory.Add(BlockType.EmberOre, 30);
s13fSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround);
s13fSim.OrbitalCommand.SelectKind(SquadKind.Vanguard);
s13fSim.BeginOrbitalPick();
s13fSim.SetOrbitalRally(5, 5, 5);
Check(s13fSim.ConfirmOrbitalDeploy() != null, "slice13 step4 composed Vanguard deploys through the paid picker");
s13fSim.OrbitalCommand.SelectKind(SquadKind.Recon);
s13fSim.BeginOrbitalPick();
s13fSim.SetOrbitalRally(5, 5, 25);
Check(s13fSim.ConfirmOrbitalDeploy() != null, "slice13 step4 composed Recon deploys through the paid picker");
s13fSim.OrbitalCommand.SelectKind(SquadKind.Heavy);
s13fSim.BeginOrbitalPick();
s13fSim.SetOrbitalRally(25, 5, 5);
Check(s13fSim.ConfirmOrbitalDeploy() != null, "slice13 step4 composed Heavy deploys through the paid picker");
Eq(0, s13fSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice13 step4 composed three paid squads cost all 30 ore");
void SeedPocketS13fTest(WorldZone w, int mx, int mz)
{
    foreach (var (x, y, z) in new[] { (mx - 1, 20, mz), (mx, 21, mz), (mx, 19, mz), (mx, 20, mz - 1), (mx, 20, mz + 1) })
        w.Chunk.TrySet(x, y, z, BlockType.Stone);
}
s13fSim.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
s13fSim.World.Chunk.TrySet(11, 20, 10, BlockType.Alternator);
s13fSim.World.Chunk.TrySet(10, 20, 14, BlockType.KineticMill);
s13fSim.World.Chunk.TrySet(11, 20, 14, BlockType.Dynamo);
SeedPocketS13fTest(s13fSim.World, 10, 14);
s13fSim.World.Chunk.TrySet(14, 20, 18, BlockType.Dynamo);
s13fSim.World.Chunk.TrySet(13, 20, 18, BlockType.Cable);
s13fSim.World.Chunk.TrySet(12, 20, 18, BlockType.Cable);
s13fSim.World.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
s13fSim.World.Chunk.TrySet(18, 5, 16, BlockType.Alternator);
s13fSim.World.Chunk.TrySet(17, 5, 28, BlockType.Workstation);
s13fSim.World.Chunk.TrySet(18, 5, 28, BlockType.Transformer);
s13fSim.World.Chunk.TrySet(19, 5, 28, BlockType.Alternator);
s13fSim.World.Chunk.TrySet(10, 5, 16, BlockType.ShockPylon);
s13fSim.World.Chunk.TrySet(11, 5, 16, BlockType.Dynamo);
Eq(Voltage.HV, s13fSim.World.DeliveredVoltage(10, 20, 10), "slice13 step4 composed threatened mill voltage HV");
Eq(Voltage.LV, s13fSim.World.DeliveredVoltage(10, 20, 14), "slice13 step4 composed powered mill voltage LV");
Eq(Voltage.LV, s13fSim.World.DeliveredVoltage(12, 20, 18), "slice13 step4 composed cable voltage LV");
Eq(Voltage.HV, s13fSim.World.DeliveredVoltage(17, 5, 16), "slice13 step4 composed threatened workstation voltage HV");
Eq(Voltage.LV, s13fSim.World.DeliveredVoltage(17, 5, 28), "slice13 step4 composed powered workstation voltage LV");
Eq(Voltage.LV, s13fSim.World.DeliveredVoltage(10, 5, 16), "slice13 step4 composed threatened pylon voltage LV");
var s13fHudBefore = s13fSim.Hud();
Eq(1, s13fHudBefore.PoweredMills, "slice13 step4 composed one mill powered before the tick");
Eq(2, s13fHudBefore.TotalMills, "slice13 step4 composed two mills total before the tick");
Eq(1, s13fHudBefore.PoweredWorkstations, "slice13 step4 composed one workstation powered before the tick");
Eq(2, s13fHudBefore.TotalWorkstations, "slice13 step4 composed two workstations total before the tick");
Eq(0, s13fHudBefore.PoweredPylons, "slice13 step4 composed no pylon powered before the tick");
Eq(1, s13fHudBefore.TotalPylons, "slice13 step4 composed one pylon total before the tick");
Eq(1, s13fHudBefore.WarnedMills, "slice13 step4 composed one mill warned before the tick");
Eq(1, s13fHudBefore.WarnedWorkstations, "slice13 step4 composed one workstation warned before the tick");
Eq(1, s13fHudBefore.WarnedPylons, "slice13 step4 composed one pylon warned before the tick");
Eq(1, s13fHudBefore.LiveNetworks, "slice13 step4 composed one live network before the tick");
Eq(0, s13fHudBefore.LoneSources, "slice13 step4 composed no lone source before the tick");
Eq(CellPowerState.Mismatched, s13fSim.World.PowerState(10, 20, 10), "slice13 step4 composed threatened mill query Mismatched");
Eq(CellPowerState.Powered, s13fSim.World.PowerState(10, 20, 14), "slice13 step4 composed powered mill query Powered");
Eq(CellPowerState.Energised, s13fSim.World.PowerState(12, 20, 18), "slice13 step4 composed cable query Energised");
Eq(CellPowerState.Mismatched, s13fSim.World.PowerState(17, 5, 16), "slice13 step4 composed threatened workstation query Mismatched");
Eq(CellPowerState.Powered, s13fSim.World.PowerState(17, 5, 28), "slice13 step4 composed powered workstation query Powered");
Eq(CellPowerState.Mismatched, s13fSim.World.PowerState(10, 5, 16), "slice13 step4 composed threatened pylon query Mismatched");
Eq(CellPowerState.None, s13fSim.World.PowerState(11, 20, 14), "slice13 step4 composed Dynamo query None");
Eq(CellPowerState.None, s13fSim.World.PowerState(18, 5, 28), "slice13 step4 composed transformer query None");
var s13fSnapBefore = OrbitalView.Snapshot(s13fSim.World);
Eq(3, s13fSnapBefore.Count, "slice13 step4 composed snapshot carries three squads");
Eq(SquadKind.Vanguard, s13fSnapBefore[0].Kind, "slice13 step4 composed snapshot first kind Vanguard");
Eq(SquadKind.Recon, s13fSnapBefore[1].Kind, "slice13 step4 composed snapshot second kind Recon");
Eq(SquadKind.Heavy, s13fSnapBefore[2].Kind, "slice13 step4 composed snapshot third kind Heavy");
Eq(SquadActivity.Marching, s13fSnapBefore[0].Activity, "slice13 step4 composed Vanguard marches to its rally, not holding yet");
Eq(SquadActivity.Marching, s13fSnapBefore[1].Activity, "slice13 step4 composed Recon marches to its rally, not holding yet");
Eq(SquadActivity.Marching, s13fSnapBefore[2].Activity, "slice13 step4 composed Heavy marches to its rally, not holding yet");
s13fSim.World.Player.TakeDamage(20);
Eq(65, s13fSim.World.Player.Shields, "slice13 step4 composed shields 65 after 20 damage");
s13fSim.World.Enemies.Add(new Enemy(10, 5, 19));
s13fSim.World.Advance();
Eq(BlockType.Air, s13fSim.World.Chunk.Get(10, 20, 10), "slice13 step4 composed warned mill destroyed on tick 1");
Eq(1, s13fSim.World.DestroyedMills, "slice13 step4 composed one mill destroyed");
Eq(1, s13fSim.World.PoweredBlocksGathered, "slice13 step4 composed powered mill gathers 1 on tick 1");
Eq(65, s13fSim.World.Player.Shields, "slice13 step4 composed disabled bench regens 0 on tick 1");
Eq(1, s13fSim.World.Enemies.Count, "slice13 step4 composed enemy survives the disabled pylon tick");
Eq(30, s13fSim.World.Enemies[0].Health, "slice13 step4 composed disabled pylon fires nothing on tick 1");
Eq(11, s13fSim.World.Enemies[0].X, "slice13 step4 composed enemy steps toward the player on tick 1");
var s13fHudAfter1 = s13fSim.Hud();
Eq(0, s13fHudAfter1.WarnedMills, "slice13 step4 composed destroyed mill warns no more");
Eq(1, s13fHudAfter1.WarnedWorkstations, "slice13 step4 composed warned workstation still warns after tick 1");
Eq(1, s13fHudAfter1.WarnedPylons, "slice13 step4 composed warned pylon still warns after tick 1");
Eq(1, s13fHudAfter1.LoneSources, "slice13 step4 composed destroyed mill orphans its Alternator into a lone source");
Check(s13fSim.World.Chunk.TrySet(18, 5, 16, BlockType.Dynamo), "slice13 step4 composed workstation rewired Alternator to Dynamo");
Check(s13fSim.World.Chunk.TrySet(11, 5, 16, BlockType.Alternator), "slice13 step4 composed pylon rewired Dynamo to Alternator");
var s13fHudRewired = s13fSim.Hud();
Eq(0, s13fHudRewired.WarnedMills, "slice13 step4 composed rewiring leaves no mill warned");
Eq(0, s13fHudRewired.WarnedWorkstations, "slice13 step4 composed rewired workstation warning clears");
Eq(0, s13fHudRewired.WarnedPylons, "slice13 step4 composed rewired pylon warning clears");
Eq(2, s13fHudRewired.PoweredWorkstations, "slice13 step4 composed rewired workstation counts powered");
Eq(1, s13fHudRewired.PoweredPylons, "slice13 step4 composed rewired pylon counts powered");
Eq(CellPowerState.Powered, s13fSim.World.PowerState(17, 5, 16), "slice13 step4 composed rewired workstation query Powered");
Eq(CellPowerState.Powered, s13fSim.World.PowerState(10, 5, 16), "slice13 step4 composed rewired pylon query Powered");
Check(s13fSim.World.Player.MoveTo(10, 5, 10, s13fSim.World.Chunk), "slice13 step4 composed player walks to the pylon lane");
s13fSim.World.Advance();
Eq(20, s13fSim.World.Enemies[0].Health, "slice13 step4 composed rewired pylon fires 10 on tick 2");
s13fSim.World.Advance();
Eq(10, s13fSim.World.Enemies[0].Health, "slice13 step4 composed rewired pylon fires 10 on tick 3");
s13fSim.World.Advance();
Eq(0, s13fSim.World.Enemies.Count, "slice13 step4 composed rewired pylon kills the enemy on tick 4");
s13fSim.World.Advance();
Eq(5, s13fSim.World.PoweredBlocksGathered, "slice13 step4 composed powered mill gathers 5 in 5 ticks, pocket exhausted");
Eq(5, s13fSim.World.Player.Inventory.Get(BlockType.Stone), "slice13 step4 composed stone is the mill pocket only");
Eq(65, s13fSim.World.Player.Shields, "slice13 step4 composed no bench regen on the pylon lane");
Check(s13fSim.World.Player.MoveTo(16, 5, 16, s13fSim.World.Chunk), "slice13 step4 composed player walks back to the rewired bench");
Eq(10, s13fSim.World.EffectiveWorkstationRegen(), "slice13 step4 composed rewired bench regen is base 5 plus powered 5");
var s13fSave = s13fSim.SaveToJson();
var s13fLoaded = new Simulation(99);
Check(s13fLoaded.LoadFromJson(s13fSave), "slice13 step4 composed save loads");
Eq(BlockType.Air, s13fLoaded.World.Chunk.Get(10, 20, 10), "slice13 step4 composed destroyed mill stays gone after load");
Eq(BlockType.KineticMill, s13fLoaded.World.Chunk.Get(10, 20, 14), "slice13 step4 composed powered mill loads");
Eq(BlockType.Cable, s13fLoaded.World.Chunk.Get(12, 20, 18), "slice13 step4 composed energised cable loads");
Eq(BlockType.Workstation, s13fLoaded.World.Chunk.Get(17, 5, 16), "slice13 step4 composed rewired workstation loads");
Eq(BlockType.ShockPylon, s13fLoaded.World.Chunk.Get(10, 5, 16), "slice13 step4 composed rewired pylon loads");
var s13fLoadedHud = s13fLoaded.Hud();
Eq(1, s13fLoadedHud.PoweredMills, "slice13 step4 composed loaded mill still powered");
Eq(1, s13fLoadedHud.TotalMills, "slice13 step4 composed loaded mill total drops with the destroyed mill");
Eq(2, s13fLoadedHud.PoweredWorkstations, "slice13 step4 composed loaded workstations still powered");
Eq(1, s13fLoadedHud.PoweredPylons, "slice13 step4 composed loaded pylon still powered");
Eq(0, s13fLoadedHud.WarnedMills, "slice13 step4 composed loaded warnings all clear, mills");
Eq(0, s13fLoadedHud.WarnedWorkstations, "slice13 step4 composed loaded warnings all clear, workstations");
Eq(0, s13fLoadedHud.WarnedPylons, "slice13 step4 composed loaded warnings all clear, pylons");
Eq(1, s13fLoadedHud.LiveNetworks, "slice13 step4 composed live network count re-derives after load");
Eq(1, s13fLoadedHud.LoneSources, "slice13 step4 composed orphaned lone source count re-derives after load");
Eq(CellPowerState.Powered, s13fLoaded.World.PowerState(10, 20, 14), "slice13 step4 composed loaded mill query Powered");
Eq(CellPowerState.Powered, s13fLoaded.World.PowerState(17, 5, 16), "slice13 step4 composed loaded workstation query Powered");
Eq(CellPowerState.Powered, s13fLoaded.World.PowerState(10, 5, 16), "slice13 step4 composed loaded pylon query Powered");
Eq(CellPowerState.Energised, s13fLoaded.World.PowerState(12, 20, 18), "slice13 step4 composed loaded cable query Energised");
Eq(0, s13fLoaded.World.PoweredBlocksGathered, "slice13 step4 composed session gather counter resets after load");
Eq(0, s13fLoaded.World.DestroyedMills, "slice13 step4 composed session destruction counter resets after load");
Eq(16, s13fLoaded.World.Player.X, "slice13 step4 composed loaded player back at the bench");
Eq(10, s13fLoaded.World.EffectiveWorkstationRegen(), "slice13 step4 composed loaded bench regen re-derives");
var s13fSnapLoaded = OrbitalView.Snapshot(s13fLoaded.World);
Eq(3, s13fSnapLoaded.Count, "slice13 step4 composed snapshot still carries three squads after load");
Eq(SquadKind.Vanguard, s13fSnapLoaded[0].Kind, "slice13 step4 composed loaded snapshot first kind Vanguard");
Eq(SquadKind.Recon, s13fSnapLoaded[1].Kind, "slice13 step4 composed loaded snapshot second kind Recon");
Eq(SquadKind.Heavy, s13fSnapLoaded[2].Kind, "slice13 step4 composed loaded snapshot third kind Heavy");
s13fLoaded.World.Player.TakeDamage(20);
Eq(45, s13fLoaded.World.Player.Shields, "slice13 step4 composed loaded shields 45 after 20 damage");
SeedPocketS13fTest(s13fLoaded.World, 10, 14);
for (int i = 0; i < 5; i++) s13fLoaded.World.Advance();
Eq(5, s13fLoaded.World.PoweredBlocksGathered, "slice13 step4 composed loaded mill gathers 5 again");
Eq(0, s13fLoaded.World.DestroyedMills, "slice13 step4 composed loaded phase destroys no mill");
Eq(10, s13fLoaded.World.Player.Inventory.Get(BlockType.Stone), "slice13 step4 composed loaded stone is 5 plus 5, no refund on load");
Eq(85, s13fLoaded.World.Player.Shields, "slice13 step4 composed loaded bench regen caps shields at 85");
Eq(0, s13fLoaded.World.Enemies.Count, "slice13 step4 composed pylon kill stays dead after load");

// ---- Slice 14 Step 1: Conduit, third power source, cable-only HV ----
// The source decided in this step: HV like the Alternator, but
// cable-only — face-adjacency delivers nothing from it, and a
// Cable network touching it carries HV. That delivery shape is
// the distinctness, not the price (5 Stone + 5 EmberOre, a new
// combination, but cost alone would be the reskin Slice 10
// rejected). No new voltage level, no per-source state, no new
// save field: output is derived from placement on every call,
// exactly like the Dynamo's and Alternator's.
Eq(13, (byte)BlockType.Conduit, "slice14 step1 conduit block type appended as 13");
Eq(12, (byte)BlockType.ShockPylon, "slice14 step1 shock pylon value unchanged at 12");
Eq(11, (byte)BlockType.Alternator, "slice14 step1 alternator value unchanged at 11");
Eq(Voltage.HV, VoltageRules.ConduitOutput, "slice14 step1 conduit outputs HV, the alternator's level, not a new level");
// Build: 5 Stone + 5 EmberOre, check-before-spend, support
// rule, not gatherable.
var condWorld = new WorldZone(42);
condWorld.Player.Inventory.Add(BlockType.Stone, 5);
condWorld.Player.Inventory.Add(BlockType.EmberOre, 5);
Eq(BuildResult.Success, condWorld.Build(15, 4, 15, BlockType.Conduit), "slice14 step1 conduit builds with 5 stone and 5 ore");
Eq(BlockType.Conduit, condWorld.Chunk.Get(15, 4, 15), "slice14 step1 conduit placed");
Eq(0, condWorld.Player.Inventory.Get(BlockType.Stone), "slice14 step1 conduit consumes its stone");
Eq(0, condWorld.Player.Inventory.Get(BlockType.EmberOre), "slice14 step1 conduit consumes its ore");
Eq(GatherResult.NotGatherable, condWorld.Gather(15, 4, 15), "slice14 step1 conduit itself is not gatherable");
var condPoorOre = new WorldZone(42);
condPoorOre.Player.Inventory.Add(BlockType.Stone, 5);
condPoorOre.Player.Inventory.Add(BlockType.EmberOre, 4);
Eq(BuildResult.InsufficientResources, condPoorOre.Build(15, 4, 15, BlockType.Conduit), "slice14 step1 conduit one ore short fails");
Eq(BlockType.Air, condPoorOre.Chunk.Get(15, 4, 15), "slice14 step1 refused conduit places nothing");
Eq(4, condPoorOre.Player.Inventory.Get(BlockType.EmberOre), "slice14 step1 refused conduit spends no ore");
var condPoorStone = new WorldZone(42);
condPoorStone.Player.Inventory.Add(BlockType.Stone, 4);
condPoorStone.Player.Inventory.Add(BlockType.EmberOre, 5);
Eq(BuildResult.InsufficientResources, condPoorStone.Build(15, 4, 15, BlockType.Conduit), "slice14 step1 conduit one stone short fails");
var condFloat = new WorldZone(42);
condFloat.Player.Inventory.Add(BlockType.Stone, 5);
condFloat.Player.Inventory.Add(BlockType.EmberOre, 5);
Eq(BuildResult.Floating, condFloat.Build(16, 20, 16, BlockType.Conduit), "slice14 step1 conduit floating in air fails like other machines");
// Direct face-adjacency delivers NOTHING: the proof no
// existing source can pass. A mill beside a Conduit is not
// powered, not mismatched, not warned, and survives its tick
// — an Alternator beside the same mill destroys it.
var condDirectMill = new WorldZone(42);
condDirectMill.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
condDirectMill.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    condDirectMill.Chunk.TrySet(x, y, z, BlockType.Stone);
Eq(Voltage.None, condDirectMill.DeliveredVoltage(16, 20, 16), "slice14 step1 direct conduit delivers nothing to its mill");
Check(!condDirectMill.IsPowered(16, 20, 16), "slice14 step1 direct conduit mill is not powered");
Check(!condDirectMill.IsMismatched(16, 20, 16), "slice14 step1 direct conduit mill is not mismatched");
Eq(CellPowerState.None, condDirectMill.PowerState(16, 20, 16), "slice14 step1 direct conduit mill power state is None");
Eq(0, HudState.FromWorld(condDirectMill, new Weapon()).WarnedMills, "slice14 step1 direct conduit mill warns nothing");
condDirectMill.Advance();
Eq(BlockType.KineticMill, condDirectMill.Chunk.Get(16, 20, 16), "slice14 step1 direct conduit mill survives its tick");
Eq(0, condDirectMill.DestroyedMills, "slice14 step1 direct conduit mill destruction counter is 0");
// Direct Conduit beside a workstation: nothing delivered, so
// the bench is unpowered at base regen, not disabled.
var condDirectWs = new WorldZone(42);
condDirectWs.Player.TakeDamage(20);
condDirectWs.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
condDirectWs.Chunk.TrySet(18, 5, 16, BlockType.Conduit);
Eq(Voltage.None, condDirectWs.DeliveredVoltage(17, 5, 16), "slice14 step1 direct conduit delivers nothing to its workstation");
Check(!condDirectWs.IsWorkstationPowered(17, 5, 16), "slice14 step1 direct conduit workstation is not powered");
Check(!condDirectWs.IsWorkstationMismatched(17, 5, 16), "slice14 step1 direct conduit workstation is not mismatched");
Eq(5, condDirectWs.EffectiveWorkstationRegen(), "slice14 step1 direct conduit workstation effective regen is base 5, not disabled 0");
// Direct Conduit beside a pylon: nothing delivered, so the
// pylon idles — not powered, not mismatched.
var condDirectPylon = new WorldZone(42);
condDirectPylon.Chunk.TrySet(17, 5, 16, BlockType.ShockPylon);
condDirectPylon.Chunk.TrySet(18, 5, 16, BlockType.Conduit);
Eq(Voltage.None, condDirectPylon.DeliveredVoltage(17, 5, 16), "slice14 step1 direct conduit delivers nothing to its pylon");
Check(!condDirectPylon.IsPylonPowered(17, 5, 16), "slice14 step1 direct conduit pylon is not powered");
Check(!condDirectPylon.IsPylonMismatched(17, 5, 16), "slice14 step1 direct conduit pylon is not mismatched");
Eq(CellPowerState.None, condDirectPylon.PowerState(17, 5, 16), "slice14 step1 direct conduit pylon power state is None");
// Direct Conduit beside a transformer: the transformer gets
// no direct input from it, so a mill beside that transformer
// reads nothing — the Conduit must reach it through cable.
var condDirectTrans = new WorldZone(42);
condDirectTrans.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
condDirectTrans.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
condDirectTrans.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
Eq(Voltage.None, condDirectTrans.DeliveredVoltage(16, 20, 16), "slice14 step1 transformer beside a conduit directly delivers nothing to its mill");
Check(!condDirectTrans.IsPowered(16, 20, 16), "slice14 step1 direct-conduit transformer mill is not powered");
// No direct conflict with a Conduit: a Dynamo beside the
// same mill decides it, because the Conduit contributes
// nothing directly — unlike Dynamo + Alternator, which is
// a conflict. An Alternator beside the same mill decides
// HV instead, and destroys it on the tick.
var condDynDirect = new WorldZone(42);
condDynDirect.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
condDynDirect.Chunk.TrySet(15, 20, 16, BlockType.Dynamo);
condDynDirect.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
Eq(Voltage.LV, condDynDirect.DeliveredVoltage(16, 20, 16), "slice14 step1 dynamo plus direct conduit delivers LV, not a conflict");
Check(condDynDirect.IsPowered(16, 20, 16), "slice14 step1 dynamo plus direct conduit mill is powered");
var condAltDirect = new WorldZone(42);
condAltDirect.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
condAltDirect.Chunk.TrySet(15, 20, 16, BlockType.Alternator);
condAltDirect.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
foreach (var (x, y, z) in new[] { (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    condAltDirect.Chunk.TrySet(x, y, z, BlockType.Stone);
Eq(Voltage.HV, condAltDirect.DeliveredVoltage(16, 20, 16), "slice14 step1 alternator plus direct conduit delivers HV");
Check(condAltDirect.IsMismatched(16, 20, 16), "slice14 step1 alternator plus direct conduit mill is mismatched");
condAltDirect.Advance();
Eq(BlockType.Air, condAltDirect.Chunk.Get(16, 20, 16), "slice14 step1 alternator plus direct conduit mill destroyed on tick 1");
// Cable delivery, mill: Conduit -> cable -> mill destroys
// the mill exactly like a direct Alternator would, and the
// cable reads Energised and the mill Mismatched first.
var condCableMill = new WorldZone(42);
condCableMill.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
condCableMill.Chunk.TrySet(17, 20, 16, BlockType.Cable);
condCableMill.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    condCableMill.Chunk.TrySet(x, y, z, BlockType.Stone);
Eq(Voltage.HV, condCableMill.DeliveredVoltage(16, 20, 16), "slice14 step1 conduit through cable delivers HV to its mill");
Check(condCableMill.IsMismatched(16, 20, 16), "slice14 step1 conduit cable mill is mismatched");
Eq(CellPowerState.Mismatched, condCableMill.PowerState(16, 20, 16), "slice14 step1 conduit cable mill power state is Mismatched");
// Slice 17 Step 1 supersedes the Slice 14 Step 1 finding recorded
// here: a single cable cell touching a Conduit now reads
// Energised, because PowerState reads its component's voltage
// (HV from the Conduit) rather than the cell's own delivered
// voltage — DeliveredVoltage for a cell counts only neighbours,
// and a Conduit is deliberately not a direct source, so the old
// reading left a live network's only cable untinted while a
// two-cable run read Energised on both cells. The Slice 14
// finding is kept in this comment as history; the assertion
// below is the decided reading, proved again in the Slice 17
// Step 1 block with the full proof set.
Eq(CellPowerState.Energised, condCableMill.PowerState(17, 20, 16), "slice17 step1 single conduit cable cell power state is Energised, superseding the slice14 finding");
// Two-cable run: every cable cell after the first reads its
// neighbour cable's component, so both read Energised, and the
// mill at the end is still destroyed — the network is live, and
// the tint agrees wherever DeliveredVoltage can see the run.
var condCableTwo = new WorldZone(42);
condCableTwo.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
condCableTwo.Chunk.TrySet(17, 20, 16, BlockType.Cable);
condCableTwo.Chunk.TrySet(18, 20, 16, BlockType.Cable);
condCableTwo.Chunk.TrySet(19, 20, 16, BlockType.Conduit);
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    condCableTwo.Chunk.TrySet(x, y, z, BlockType.Stone);
Eq(CellPowerState.Energised, condCableTwo.PowerState(17, 20, 16), "slice14 step1 two-cable conduit run first cable power state is Energised");
Eq(CellPowerState.Energised, condCableTwo.PowerState(18, 20, 16), "slice14 step1 two-cable conduit run second cable power state is Energised");
Eq(Voltage.HV, condCableTwo.DeliveredVoltage(16, 20, 16), "slice14 step1 two-cable conduit run delivers HV to its mill");
Eq(1, HudState.FromWorld(condCableMill, new Weapon()).WarnedMills, "slice14 step1 conduit cable mill warns one mill");
condCableMill.Advance();
Eq(BlockType.Air, condCableMill.Chunk.Get(16, 20, 16), "slice14 step1 conduit cable mill destroyed on tick 1");
Eq(1, condCableMill.DestroyedMills, "slice14 step1 conduit cable mill destruction counter is 1");
Eq(BlockType.Stone, condCableMill.Chunk.Get(15, 20, 16), "slice14 step1 conduit cable destroyed mill excavates nothing");
Eq(BlockType.Conduit, condCableMill.Chunk.Get(18, 20, 16), "slice14 step1 destroying a mill does not destroy its conduit");
// Cable delivery, pylon: Conduit -> cable -> pylon is the
// Conduit's safe direct use — powered, firing 10 per tick.
var condCablePylon = new WorldZone(42);
condCablePylon.Chunk.TrySet(17, 5, 16, BlockType.ShockPylon);
condCablePylon.Chunk.TrySet(18, 5, 16, BlockType.Cable);
condCablePylon.Chunk.TrySet(19, 5, 16, BlockType.Conduit);
condCablePylon.Enemies.Add(new Enemy(17, 5, 20));
Eq(Voltage.HV, condCablePylon.DeliveredVoltage(17, 5, 16), "slice14 step1 conduit through cable delivers HV to its pylon");
Check(condCablePylon.IsPylonPowered(17, 5, 16), "slice14 step1 conduit cable pylon is powered");
Check(!condCablePylon.IsPylonMismatched(17, 5, 16), "slice14 step1 conduit cable pylon is not mismatched");
Eq(CellPowerState.Powered, condCablePylon.PowerState(17, 5, 16), "slice14 step1 conduit cable pylon power state is Powered");
condCablePylon.Advance();
Eq(20, condCablePylon.Enemies[0].Health, "slice14 step1 conduit cable pylon fires 10 on its first tick");
// Cable delivery, workstation: Conduit -> cable -> workstation
// is mismatched HV — disabled, surviving, restoring nothing.
var condCableWs = new WorldZone(42);
condCableWs.Player.TakeDamage(20);
condCableWs.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
condCableWs.Chunk.TrySet(18, 5, 16, BlockType.Cable);
condCableWs.Chunk.TrySet(19, 5, 16, BlockType.Conduit);
Eq(Voltage.HV, condCableWs.DeliveredVoltage(17, 5, 16), "slice14 step1 conduit through cable delivers HV to its workstation");
Check(!condCableWs.IsWorkstationPowered(17, 5, 16), "slice14 step1 conduit cable workstation is not powered");
Check(condCableWs.IsWorkstationMismatched(17, 5, 16), "slice14 step1 conduit cable workstation is mismatched");
Eq(CellPowerState.Mismatched, condCableWs.PowerState(17, 5, 16), "slice14 step1 conduit cable workstation power state is Mismatched");
Eq(0, condCableWs.EffectiveWorkstationRegen(), "slice14 step1 conduit cable workstation effective regen is 0: disabled");
condCableWs.Advance();
Eq(BlockType.Workstation, condCableWs.Chunk.Get(17, 5, 16), "slice14 step1 conduit cable workstation survives its tick");
Eq(65, condCableWs.Player.Shields, "slice14 step1 conduit cable workstation restores nothing");
// Cable step-down, mill: Conduit -> cable -> transformer ->
// cable -> mill runs at LV, every tick, like an Alternator
// stepped down — safe use needs the transformer, as before.
var condStepMill = new WorldZone(42);
condStepMill.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
condStepMill.Chunk.TrySet(17, 20, 16, BlockType.Cable);
condStepMill.Chunk.TrySet(18, 20, 16, BlockType.Transformer);
condStepMill.Chunk.TrySet(19, 20, 16, BlockType.Cable);
condStepMill.Chunk.TrySet(20, 20, 16, BlockType.Conduit);
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    condStepMill.Chunk.TrySet(x, y, z, BlockType.Stone);
Eq(Voltage.LV, condStepMill.DeliveredVoltage(16, 20, 16), "slice14 step1 conduit stepped down through cable and transformer delivers LV to its mill");
Check(condStepMill.IsPowered(16, 20, 16), "slice14 step1 conduit stepped-down mill is powered");
for (int i = 0; i < 5; i++) condStepMill.Advance();
Eq(5, condStepMill.PoweredBlocksGathered, "slice14 step1 conduit stepped-down mill gathers 5 in 5 ticks");
Eq(BlockType.Conduit, condStepMill.Chunk.Get(20, 20, 16), "slice14 step1 stepped-down mill never excavates its conduit");
// Cable step-down, workstation: Conduit -> cable ->
// transformer -> workstation runs at LV, regen 10 per tick.
var condStepWs = new WorldZone(42);
condStepWs.Player.TakeDamage(20);
condStepWs.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
condStepWs.Chunk.TrySet(18, 5, 16, BlockType.Transformer);
condStepWs.Chunk.TrySet(19, 5, 16, BlockType.Cable);
condStepWs.Chunk.TrySet(20, 5, 16, BlockType.Conduit);
Eq(Voltage.LV, condStepWs.DeliveredVoltage(17, 5, 16), "slice14 step1 conduit stepped down through cable and transformer delivers LV to its workstation");
Check(condStepWs.IsWorkstationPowered(17, 5, 16), "slice14 step1 conduit stepped-down workstation is powered");
Eq(10, condStepWs.EffectiveWorkstationRegen(), "slice14 step1 conduit stepped-down workstation effective regen is base 5 + powered 5");
condStepWs.Advance();
Eq(75, condStepWs.Player.Shields, "slice14 step1 conduit stepped-down workstation restores 10 shields per tick");
// Cable conflicts, the Conduit way: Conduit + Dynamo on one
// cable component resolve to None (mixed network), so the
// mill at its end idles at kinetic cadence and survives;
// Conduit + Alternator on one component agree at HV, so
// the mill at its end is destroyed; two Conduits agree at
// HV as well.
var condDynCable = new WorldZone(42);
condDynCable.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
condDynCable.Chunk.TrySet(17, 20, 16, BlockType.Cable);
condDynCable.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
condDynCable.Chunk.TrySet(17, 20, 15, BlockType.Dynamo);
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    condDynCable.Chunk.TrySet(x, y, z, BlockType.Stone);
Eq(Voltage.None, condDynCable.DeliveredVoltage(16, 20, 16), "slice14 step1 conduit plus dynamo on one cable delivers nothing to its mill");
Check(!condDynCable.IsMismatched(16, 20, 16), "slice14 step1 conduit dynamo cable mill is not mismatched");
for (int i = 0; i < 5; i++) condDynCable.Advance();
Eq(BlockType.KineticMill, condDynCable.Chunk.Get(16, 20, 16), "slice14 step1 conduit dynamo cable mill survives 5 ticks");
Eq(1, condDynCable.KineticBlocksGathered, "slice14 step1 conduit dynamo cable mill falls back to kinetic cadence, 1 in 5 ticks");
var condAltCable = new WorldZone(42);
condAltCable.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
condAltCable.Chunk.TrySet(17, 20, 16, BlockType.Cable);
condAltCable.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
condAltCable.Chunk.TrySet(17, 20, 15, BlockType.Alternator);
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    condAltCable.Chunk.TrySet(x, y, z, BlockType.Stone);
Eq(Voltage.HV, condAltCable.DeliveredVoltage(16, 20, 16), "slice14 step1 conduit plus alternator on one cable delivers HV to its mill");
condAltCable.Advance();
Eq(BlockType.Air, condAltCable.Chunk.Get(16, 20, 16), "slice14 step1 conduit alternator cable mill destroyed on tick 1");
var condTwoCable = new WorldZone(42);
condTwoCable.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
condTwoCable.Chunk.TrySet(17, 20, 16, BlockType.Cable);
condTwoCable.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
condTwoCable.Chunk.TrySet(17, 20, 15, BlockType.Conduit);
Eq(Voltage.HV, condTwoCable.DeliveredVoltage(16, 20, 16), "slice14 step1 two conduits on one cable agree at HV to their mill");
// Live-network / lone-source counts cover the Conduit with
// no new HUD count: a lone Conduit counts, two adjacent
// Conduits count as two (sources are not conductors), a
// Conduit cabled to nothing counts as one live network,
// and a Conduit cabled to a mill counts as neither again.
var condLone = new WorldZone(42);
condLone.Chunk.TrySet(16, 20, 16, BlockType.Conduit);
Eq((0, 1), condLone.LivePowerCounts(), "slice14 step1 lone conduit counts 0 live networks, 1 lone source");
var condLoneTwo = new WorldZone(42);
condLoneTwo.Chunk.TrySet(16, 20, 16, BlockType.Conduit);
condLoneTwo.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
Eq((0, 2), condLoneTwo.LivePowerCounts(), "slice14 step1 two adjacent conduits count 2 lone sources");
var condLive = new WorldZone(42);
condLive.Chunk.TrySet(17, 20, 16, BlockType.Cable);
condLive.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
Eq((1, 0), condLive.LivePowerCounts(), "slice14 step1 conduit cabled to nothing counts 1 live network, 0 lone sources");
var condFed = new WorldZone(42);
condFed.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
condFed.Chunk.TrySet(17, 20, 16, BlockType.Cable);
condFed.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
Eq((0, 0), condFed.LivePowerCounts(), "slice14 step1 conduit cabled to a mill counts neither live nor lone");
// Aimed builder: same aim as the Slice 10 Step 2 alternator
// build, with conduit inventory (5 Stone + 5 EmberOre).
var simCond = new Simulation(42);
simCond.Tick(0.1f, 0, 0, false);
simCond.World.Player.Inventory.Add(BlockType.Stone, 5);
simCond.World.Player.Inventory.Add(BlockType.EmberOre, 5);
simCond.ApplyLook(-18.4f, -53f);
Eq(BuildResult.Success, simCond.BuildConduitAhead(), "slice14 step1 simulation build conduit ahead succeeds");
Eq(BlockType.Conduit, simCond.World.Chunk.Get(15, 4, 17), "slice14 step1 simulation conduit placed by aim");
Eq(0, simCond.World.Player.Inventory.Get(BlockType.Stone), "slice14 step1 simulation conduit spends its stone");
Eq(0, simCond.World.Player.Inventory.Get(BlockType.EmberOre), "slice14 step1 simulation conduit spends its ore");
// Derived, not stored: a Conduit and its cable chain survive
// save/load as chunk bytes and re-derive HV on load, with no
// stored voltage or source flag, and old block values load.
var condSave = new WorldZone(42);
condSave.Chunk.TrySet(17, 5, 16, BlockType.ShockPylon);
condSave.Chunk.TrySet(18, 5, 16, BlockType.Cable);
condSave.Chunk.TrySet(19, 5, 16, BlockType.Conduit);
var condLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(condSave));
Eq(BlockType.Conduit, condLoaded.Chunk.Get(19, 5, 16), "slice14 step1 conduit survives save/load as a chunk byte");
Eq(Voltage.HV, condLoaded.DeliveredVoltage(17, 5, 16), "slice14 step1 conduit cable pylon re-derives HV after load");
Check(condLoaded.IsPylonPowered(17, 5, 16), "slice14 step1 conduit cable pylon is powered after load");
Eq(CellPowerState.Powered, condLoaded.PowerState(17, 5, 16), "slice14 step1 conduit cable pylon power state is Powered after load");

// Slice 14 Step 2 composed integration: the demo layout as
// tests, so the demo numbers are proved, not just printed.
// One Simulation carries the Conduit cable-fed mill, the
// Dynamo mill, the direct-Conduit mill, the Conduit cable
// pylon, and the Conduit step-down workstation together
// through ticks and a save/load round-trip. One finding from
// Step 1 was asserted here as a composition fact, not smoothed
// over, until Slice 17 Step 2 (0.75.0) superseded it: a Conduit
// standing directly beside a mill then counted as neither a
// live network nor a lone source, because the Slice 12 lone
// rule disqualified any source beside a consumer — under the
// decided Step 2 rule it counts as one lone source, and the
// assertions below were updated to the decided reading rather
// than left asserting the superseded one. The other Step 1 finding — a single cable cell
// touching a Conduit reading None in PowerState while its
// pylon fires — was superseded by Slice 17 Step 1 (0.74.0):
// that cable now reads Energised, and the assertions below
// were updated to the decided reading rather than left
// asserting the superseded one.
var s14Sim = new Simulation(42);
void SeedPocketS14Test(WorldZone w, int mx, int mz)
{
    foreach (var (x, y, z) in new[] { (mx - 1, 20, mz), (mx, 21, mz), (mx, 19, mz), (mx, 20, mz - 1), (mx, 20, mz + 1) })
        w.Chunk.TrySet(x, y, z, BlockType.Stone);
}
s14Sim.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
s14Sim.World.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s14Sim.World.Chunk.TrySet(12, 20, 10, BlockType.Cable);
s14Sim.World.Chunk.TrySet(13, 20, 10, BlockType.Conduit);
s14Sim.World.Chunk.TrySet(10, 20, 14, BlockType.KineticMill);
s14Sim.World.Chunk.TrySet(11, 20, 14, BlockType.Dynamo);
SeedPocketS14Test(s14Sim.World, 10, 14);
s14Sim.World.Chunk.TrySet(10, 20, 18, BlockType.KineticMill);
s14Sim.World.Chunk.TrySet(11, 20, 18, BlockType.Conduit);
SeedPocketS14Test(s14Sim.World, 10, 18);
s14Sim.World.Chunk.TrySet(10, 5, 16, BlockType.ShockPylon);
s14Sim.World.Chunk.TrySet(11, 5, 16, BlockType.Cable);
s14Sim.World.Chunk.TrySet(12, 5, 16, BlockType.Conduit);
s14Sim.World.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
s14Sim.World.Chunk.TrySet(18, 5, 16, BlockType.Transformer);
s14Sim.World.Chunk.TrySet(19, 5, 16, BlockType.Cable);
s14Sim.World.Chunk.TrySet(20, 5, 16, BlockType.Conduit);
s14Sim.World.Player.TakeDamage(20);
s14Sim.World.Enemies.Add(new Enemy(10, 5, 17));
Eq(Voltage.HV, s14Sim.World.DeliveredVoltage(10, 20, 10), "slice14 step2 composed conduit cable mill voltage HV");
Eq(Voltage.LV, s14Sim.World.DeliveredVoltage(10, 20, 14), "slice14 step2 composed dynamo mill voltage LV");
Eq(Voltage.None, s14Sim.World.DeliveredVoltage(10, 20, 18), "slice14 step2 composed direct conduit mill voltage None");
Eq(Voltage.HV, s14Sim.World.DeliveredVoltage(10, 5, 16), "slice14 step2 composed conduit cable pylon voltage HV");
Eq(Voltage.LV, s14Sim.World.DeliveredVoltage(17, 5, 16), "slice14 step2 composed conduit step-down workstation voltage LV");
Check(s14Sim.World.IsMismatched(10, 20, 10), "slice14 step2 composed conduit cable mill is mismatched");
Check(s14Sim.World.IsPowered(10, 20, 14), "slice14 step2 composed dynamo mill is powered");
Check(!s14Sim.World.IsPowered(10, 20, 18), "slice14 step2 composed direct conduit mill is not powered");
Check(!s14Sim.World.IsMismatched(10, 20, 18), "slice14 step2 composed direct conduit mill is not mismatched");
Check(s14Sim.World.IsPylonPowered(10, 5, 16), "slice14 step2 composed conduit cable pylon is powered");
Check(!s14Sim.World.IsPylonMismatched(10, 5, 16), "slice14 step2 composed conduit cable pylon is not mismatched");
Check(s14Sim.World.IsWorkstationPowered(17, 5, 16), "slice14 step2 composed step-down workstation is powered");
Check(!s14Sim.World.IsWorkstationMismatched(17, 5, 16), "slice14 step2 composed step-down workstation is not mismatched");
Eq(CellPowerState.Mismatched, s14Sim.World.PowerState(10, 20, 10), "slice14 step2 composed conduit cable mill state Mismatched");
Eq(CellPowerState.Energised, s14Sim.World.PowerState(11, 20, 10), "slice14 step2 composed two-cable run first cable Energised");
Eq(CellPowerState.Energised, s14Sim.World.PowerState(12, 20, 10), "slice14 step2 composed two-cable run second cable Energised");
Eq(CellPowerState.Powered, s14Sim.World.PowerState(10, 20, 14), "slice14 step2 composed dynamo mill state Powered");
Eq(CellPowerState.None, s14Sim.World.PowerState(10, 20, 18), "slice14 step2 composed direct conduit mill state None");
Eq(CellPowerState.Powered, s14Sim.World.PowerState(10, 5, 16), "slice14 step2 composed conduit cable pylon state Powered");
Eq(CellPowerState.Energised, s14Sim.World.PowerState(11, 5, 16), "slice17 step1 composed single conduit cable state Energised while its pylon fires");
Eq(CellPowerState.Powered, s14Sim.World.PowerState(17, 5, 16), "slice14 step2 composed step-down workstation state Powered");
Eq(CellPowerState.Energised, s14Sim.World.PowerState(19, 5, 16), "slice14 step2 composed step-down cable state Energised via its transformer");
Eq(CellPowerState.None, s14Sim.World.PowerState(18, 5, 16), "slice14 step2 composed transformer state None by decision");
Eq(CellPowerState.None, s14Sim.World.PowerState(13, 20, 10), "slice14 step2 composed cable-run conduit state None by decision");
Eq(CellPowerState.None, s14Sim.World.PowerState(11, 20, 18), "slice14 step2 composed direct conduit state None by decision");
var s14HudBefore = s14Sim.Hud();
Eq(1, s14HudBefore.PoweredMills, "slice14 step2 composed one mill powered before the tick");
Eq(3, s14HudBefore.TotalMills, "slice14 step2 composed three mills total before the tick");
Eq(1, s14HudBefore.PoweredWorkstations, "slice14 step2 composed one workstation powered before the tick");
Eq(1, s14HudBefore.TotalWorkstations, "slice14 step2 composed one workstation total before the tick");
Eq(1, s14HudBefore.PoweredPylons, "slice14 step2 composed one pylon powered before the tick");
Eq(1, s14HudBefore.TotalPylons, "slice14 step2 composed one pylon total before the tick");
Eq(1, s14HudBefore.WarnedMills, "slice14 step2 composed one mill warned before the tick");
Eq(0, s14HudBefore.WarnedWorkstations, "slice14 step2 composed no workstation warned before the tick");
Eq(0, s14HudBefore.WarnedPylons, "slice14 step2 composed no pylon warned before the tick");
Eq(0, s14HudBefore.LiveNetworks, "slice14 step2 composed no live network before the tick");
Eq(1, s14HudBefore.LoneSources, "slice17 step2 composed one lone source before the tick: the direct conduit beside a mill now counts as lone, superseding the slice14 step2 reading that it counted neither");
Eq(65, s14Sim.World.Player.Shields, "slice14 step2 composed shields 65 after 20 damage");
Eq(10, s14Sim.World.EffectiveWorkstationRegen(), "slice14 step2 composed step-down bench regen 10 at spawn");
Eq(0, s14Sim.World.Player.Inventory.Get(BlockType.Stone), "slice14 step2 composed stone starts at 0");
Eq(30, s14Sim.World.Enemies[0].Health, "slice14 step2 composed staged enemy 30 HP");
s14Sim.World.Advance();
Eq(BlockType.Air, s14Sim.World.Chunk.Get(10, 20, 10), "slice14 step2 composed conduit cable mill destroyed on tick 1");
Eq(1, s14Sim.World.DestroyedMills, "slice14 step2 composed one mill destroyed after tick 1");
Eq(1, s14Sim.World.PoweredBlocksGathered, "slice14 step2 composed powered gathered 1 after tick 1");
Eq(0, s14Sim.World.KineticBlocksGathered, "slice14 step2 composed kinetic gathered 0 after tick 1");
Eq(BlockType.KineticMill, s14Sim.World.Chunk.Get(10, 20, 18), "slice14 step2 composed direct conduit mill survives tick 1");
var s14HudAfter1 = s14Sim.Hud();
Eq(0, s14HudAfter1.WarnedMills, "slice14 step2 composed no mill warned after tick 1");
Eq(1, s14HudAfter1.LiveNetworks, "slice14 step2 composed destroyed mill cable run counts live after tick 1");
Eq(1, s14HudAfter1.LoneSources, "slice17 step2 composed still one lone source after tick 1: the direct conduit survives its mill's neighbour tick");
Eq(75, s14Sim.World.Player.Shields, "slice14 step2 composed shields 75 after tick 1 regen");
Eq(20, s14Sim.World.Enemies[0].Health, "slice14 step2 composed pylon fires 10 on tick 1");
Eq(11, s14Sim.World.Enemies[0].X, "slice14 step2 composed enemy steps east on tick 1");
Eq(CellPowerState.Energised, s14Sim.World.PowerState(11, 5, 16), "slice17 step1 composed single conduit cable still Energised after tick 1");
s14Sim.World.Advance();
Eq(10, s14Sim.World.Enemies[0].Health, "slice14 step2 composed pylon fires 10 on tick 2");
s14Sim.World.Advance();
Eq(0, s14Sim.World.Enemies.Count, "slice14 step2 composed pylon kills the enemy on tick 3");
s14Sim.World.Advance();
s14Sim.World.Advance();
Eq(5, s14Sim.World.PoweredBlocksGathered, "slice14 step2 composed powered gathered 5 after 5 ticks");
Eq(1, s14Sim.World.KineticBlocksGathered, "slice14 step2 composed kinetic gathered 1 after 5 ticks");
Eq(1, s14Sim.World.DestroyedMills, "slice14 step2 composed destroyed stays 1 after 5 ticks");
Eq(6, s14Sim.World.Player.Inventory.Get(BlockType.Stone), "slice14 step2 composed stone 6 after 5 ticks");
Eq(85, s14Sim.World.Player.Shields, "slice14 step2 composed shields capped at 85 after 5 ticks");
Eq(0, s14Sim.World.Enemies.Count, "slice14 step2 composed no enemies after 5 ticks");
var s14Save = s14Sim.SaveToJson();
var s14Loaded = new Simulation(99);
Check(s14Loaded.LoadFromJson(s14Save), "slice14 step2 composed save loads");
Eq(BlockType.Air, s14Loaded.World.Chunk.Get(10, 20, 10), "slice14 step2 composed destroyed mill stays gone after load");
Eq(BlockType.KineticMill, s14Loaded.World.Chunk.Get(10, 20, 14), "slice14 step2 composed dynamo mill loads");
Eq(BlockType.KineticMill, s14Loaded.World.Chunk.Get(10, 20, 18), "slice14 step2 composed direct conduit mill loads");
Eq(BlockType.ShockPylon, s14Loaded.World.Chunk.Get(10, 5, 16), "slice14 step2 composed pylon loads");
Eq(BlockType.Workstation, s14Loaded.World.Chunk.Get(17, 5, 16), "slice14 step2 composed workstation loads");
Eq(BlockType.Conduit, s14Loaded.World.Chunk.Get(13, 20, 10), "slice14 step2 composed cable-run conduit loads");
Eq(BlockType.Conduit, s14Loaded.World.Chunk.Get(11, 20, 18), "slice14 step2 composed direct conduit loads");
Eq(BlockType.Conduit, s14Loaded.World.Chunk.Get(12, 5, 16), "slice14 step2 composed pylon conduit loads");
Eq(BlockType.Conduit, s14Loaded.World.Chunk.Get(20, 5, 16), "slice14 step2 composed step-down conduit loads");
var s14LoadedHud = s14Loaded.Hud();
Eq(1, s14LoadedHud.PoweredMills, "slice14 step2 composed one mill powered after load");
Eq(2, s14LoadedHud.TotalMills, "slice14 step2 composed two mills total after load");
Eq(1, s14LoadedHud.PoweredWorkstations, "slice14 step2 composed one workstation powered after load");
Eq(1, s14LoadedHud.PoweredPylons, "slice14 step2 composed one pylon powered after load");
Eq(0, s14LoadedHud.WarnedMills, "slice14 step2 composed no mill warned after load");
Eq(1, s14LoadedHud.LiveNetworks, "slice14 step2 composed one live network after load");
Eq(1, s14LoadedHud.LoneSources, "slice17 step2 composed one lone source after load: the direct conduit count re-derives");
Eq(CellPowerState.Powered, s14Loaded.World.PowerState(10, 20, 14), "slice14 step2 composed dynamo mill state Powered after load");
Eq(CellPowerState.None, s14Loaded.World.PowerState(10, 20, 18), "slice14 step2 composed direct conduit mill state None after load");
Eq(CellPowerState.Powered, s14Loaded.World.PowerState(10, 5, 16), "slice14 step2 composed pylon state Powered after load");
Eq(CellPowerState.Energised, s14Loaded.World.PowerState(11, 5, 16), "slice17 step1 composed single cable state Energised after load");
Eq(CellPowerState.Powered, s14Loaded.World.PowerState(17, 5, 16), "slice14 step2 composed workstation state Powered after load");
Eq(0, s14Loaded.World.PoweredBlocksGathered, "slice14 step2 composed powered counter resets on load");
Eq(0, s14Loaded.World.KineticBlocksGathered, "slice14 step2 composed kinetic counter resets on load");
Eq(0, s14Loaded.World.DestroyedMills, "slice14 step2 composed destroyed counter resets on load");
Eq(6, s14Loaded.World.Player.Inventory.Get(BlockType.Stone), "slice14 step2 composed stone stays 6 on load");
Eq(16, s14Loaded.World.Player.X, "slice14 step2 composed player back at bench X after load");
Eq(10, s14Loaded.World.EffectiveWorkstationRegen(), "slice14 step2 composed bench regen 10 after load");
s14Loaded.World.Player.TakeDamage(20);
SeedPocketS14Test(s14Loaded.World, 10, 14);
SeedPocketS14Test(s14Loaded.World, 10, 18);
for (int s14Tick = 0; s14Tick < 5; s14Tick++) s14Loaded.World.Advance();
Eq(5, s14Loaded.World.PoweredBlocksGathered, "slice14 step2 composed loaded powered gathered 5 again");
Eq(1, s14Loaded.World.KineticBlocksGathered, "slice14 step2 composed loaded kinetic gathered 1 again");
Eq(0, s14Loaded.World.DestroyedMills, "slice14 step2 composed loaded destroyed stays 0");
Eq(12, s14Loaded.World.Player.Inventory.Get(BlockType.Stone), "slice14 step2 composed loaded stone 12");
Eq(85, s14Loaded.World.Player.Shields, "slice14 step2 composed loaded shields capped at 85");
Eq(0, s14Loaded.World.Enemies.Count, "slice14 step2 composed pylon kill stays dead after load");

// ---- Slice 15 Step 1: fourth doctrine — Patrol (the deferred doctrine, from Slice 4 / Slice 11) ----
// Patrol is a route doctrine, not HoldGround at a moving rally
// and not Advance with a starting point: it walks a closed loop
// derived from the rally alone — rally, then 3 east, south, west,
// and north of it, then back to the rally, forever — one voxel
// per tick through the shared step rule, firing in passing at any
// enemy in its kind's range without ever chasing one. No new save
// field exists: the waypoint index is live route progress that
// starts at 0 on deployment and resets to 0 on load, so a loaded
// patrol marches to its rally first and then resumes the loop.
// Enum order: appended only, so old doctrines keep their values.
Eq(0, (int)TacticalDoctrine.HoldGround, "slice15 patrol holdground value unchanged");
Eq(1, (int)TacticalDoctrine.Advance, "slice15 patrol advance value unchanged");
Eq(2, (int)TacticalDoctrine.DefendCore, "slice15 patrol defendcore value unchanged");
Eq(3, (int)TacticalDoctrine.Patrol, "slice15 patrol appended as 3");
Eq(3, TacticalSquad.PatrolLoopRadius, "slice15 patrol loop radius is 3");
Eq(5, TacticalSquad.PatrolWaypointCount, "slice15 patrol waypoint count is 5");
var p15UnknownWorld = new WorldZone(42);
Check(p15UnknownWorld.SpawnSquad(20, 5, 20, (TacticalDoctrine)99) == null, "slice15 patrol unknown doctrine 99 still rejected");
Check(p15UnknownWorld.SpawnSquad(20, 5, 20, (TacticalDoctrine)4) == null, "slice15 patrol next undefined doctrine 4 still rejected");
// Route derivation: deployed AT its rally, the squad's first
// waypoint is the rally itself, and its first step is east —
// it does not hold at the rally the way HoldGround would.
var p15RouteWorld = new WorldZone(42);
p15RouteWorld.Player.X = 20; p15RouteWorld.Player.Z = 20;
var p15Route = p15RouteWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Eq(0, p15Route.PatrolWaypointIndex, "slice15 patrol starts at waypoint 0");
Eq(20, p15Route.PatrolTarget.X, "slice15 patrol waypoint 0 is the rally X");
Eq(20, p15Route.PatrolTarget.Z, "slice15 patrol waypoint 0 is the rally Z");
Check(p15Route.AtPatrolTarget, "slice15 patrol deployed at rally is on its waypoint");
Check(p15Route.MoveForPatrol(p15RouteWorld.Chunk, Array.Empty<Enemy>()), "slice15 patrol at rally steps instead of holding");
Eq(1, p15Route.PatrolWaypointIndex, "slice15 patrol at rally advances to waypoint 1");
Eq(23, p15Route.PatrolTarget.X, "slice15 patrol waypoint 1 is rally plus radius east");
Eq(21, p15Route.X, "slice15 patrol first step is east from the rally");
Eq(20, p15Route.Z, "slice15 patrol first step does not drift south");
// Full loop, call by call: east at call 3, south at call 9,
// west at call 15, north at call 21, rally again at call 24 —
// 3 + 6 + 6 + 6 + 3 steps, no dwell tick at any waypoint,
// because arriving at a waypoint advances the index and steps
// onward in the same call. Call 25 then steps east again,
// proving the loop repeats rather than holding at the rally.
for (int p15Call = 2; p15Call <= 24; p15Call++)
{
    p15Route.MoveForPatrol(p15RouteWorld.Chunk, Array.Empty<Enemy>());
    if (p15Call == 3) { Eq(23, p15Route.X, "slice15 patrol loop east waypoint reached at call 3"); Eq(20, p15Route.Z, "slice15 patrol loop east waypoint Z"); }
    if (p15Call == 9) { Eq(20, p15Route.X, "slice15 patrol loop south waypoint reached at call 9"); Eq(23, p15Route.Z, "slice15 patrol loop south waypoint Z"); }
    if (p15Call == 15) { Eq(17, p15Route.X, "slice15 patrol loop west waypoint reached at call 15"); Eq(20, p15Route.Z, "slice15 patrol loop west waypoint Z"); }
    if (p15Call == 21) { Eq(20, p15Route.X, "slice15 patrol loop north waypoint reached at call 21"); Eq(17, p15Route.Z, "slice15 patrol loop north waypoint Z"); }
}
Eq(20, p15Route.X, "slice15 patrol loop returns to rally X at call 24");
Eq(20, p15Route.Z, "slice15 patrol loop returns to rally Z at call 24");
Eq(0, p15Route.PatrolWaypointIndex, "slice15 patrol loop index wraps to 0 at the rally");
Check(p15Route.MoveForPatrol(p15RouteWorld.Chunk, Array.Empty<Enemy>()), "slice15 patrol repeats after the rally instead of holding");
Eq(21, p15Route.X, "slice15 patrol repeat step is east again");
// Contrast, same rally: HoldGround at its rally does not move.
var p15HoldWorld = new WorldZone(42);
p15HoldWorld.Player.X = 20; p15HoldWorld.Player.Z = 20;
var p15Hold = p15HoldWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.HoldGround)!;
Check(!p15Hold.MoveTowardRally(p15HoldWorld.Chunk, Array.Empty<Enemy>()), "slice15 patrol contrast holdground at rally does not move");
// No chase: an enemy off the route does not divert the step.
// The squad heads east toward its waypoint while the enemy
// stands south of the route, and the step is east, not south.
var p15NoChaseWorld = new WorldZone(42);
p15NoChaseWorld.Player.X = 20; p15NoChaseWorld.Player.Z = 20;
var p15NoChase = p15NoChaseWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
p15NoChase.MoveForPatrol(p15NoChaseWorld.Chunk, new[] { new Enemy(20, 5, 23) });
Eq(21, p15NoChase.X, "slice15 patrol does not chase an off-route enemy");
Eq(20, p15NoChase.Z, "slice15 patrol step stays on route with an enemy nearby");
// Blocked path cell: the first east step is a solid block, so
// the squad skips the east waypoint for this circuit (index 2,
// no move that tick) and then walks south on the next call —
// it does not march forever at the wall.
var p15BlockedWorld = new WorldZone(42);
p15BlockedWorld.Player.X = 20; p15BlockedWorld.Player.Z = 20;
var p15Blocked = p15BlockedWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(p15BlockedWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice15 patrol blocked test stages a solid step cell");
Check(!p15Blocked.MoveForPatrol(p15BlockedWorld.Chunk, Array.Empty<Enemy>()), "slice15 patrol blocked step does not move");
Eq(2, p15Blocked.PatrolWaypointIndex, "slice15 patrol blocked step skips to the south waypoint");
Eq(20, p15Blocked.X, "slice15 patrol blocked step holds its cell that tick");
Check(p15Blocked.MoveForPatrol(p15BlockedWorld.Chunk, Array.Empty<Enemy>()), "slice15 patrol after a skip walks south");
Eq(21, p15Blocked.Z, "slice15 patrol after a skip steps toward the south waypoint");
// Blocked waypoint cell: the rally itself is solid, so a squad
// stepping toward it skips it and heads east instead.
var p15SolidWpWorld = new WorldZone(42);
p15SolidWpWorld.Player.X = 22; p15SolidWpWorld.Player.Z = 20;
var p15SolidWp = p15SolidWpWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(p15SolidWpWorld.Chunk.TrySet(20, 5, 20, BlockType.Stone), "slice15 patrol blocked test stages a solid rally cell");
Check(p15SolidWp.MoveForPatrol(p15SolidWpWorld.Chunk, Array.Empty<Enemy>()), "slice15 patrol steps west toward a solid rally first");
Eq(21, p15SolidWp.X, "slice15 patrol solid-rally approach steps west once");
Check(!p15SolidWp.MoveForPatrol(p15SolidWpWorld.Chunk, Array.Empty<Enemy>()), "slice15 patrol step into a solid waypoint is refused");
Eq(1, p15SolidWp.PatrolWaypointIndex, "slice15 patrol solid waypoint is skipped to east");
Eq(21, p15SolidWp.X, "slice15 patrol solid waypoint skip holds its cell that tick");
// Firing in passing, through WorldZone.Advance: a Vanguard
// patrol steps east, its on-route enemy closes and dies to the
// 30-damage passing shot on tick 1, and on tick 2 the squad
// still steps east along its route — it neither chased the
// enemy (it was already on the route) nor stops after the kill.
var p15FireWorld = new WorldZone(42);
p15FireWorld.Player.X = 20; p15FireWorld.Player.Z = 20;
var p15Fire = p15FireWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
p15FireWorld.Player.X = 0; p15FireWorld.Player.Z = 0;
p15FireWorld.Enemies.Add(new Enemy(24, 5, 20));
p15FireWorld.Advance();
Eq(0, p15FireWorld.Enemies.Count, "slice15 patrol firing in passing kills the on-route enemy on tick 1");
Eq(21, p15Fire.X, "slice15 patrol firing tick still steps east");
p15FireWorld.Advance();
Eq(22, p15Fire.X, "slice15 patrol route continues east after the kill");
// Fire rule, direct: a patrol NOT at its rally still fires,
// where HoldGround moving to its rally does not.
var p15FireRuleWorld = new WorldZone(42);
p15FireRuleWorld.Player.X = 20; p15FireRuleWorld.Player.Z = 20;
var p15FireRule = p15FireRuleWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
p15FireRule.MoveForPatrol(p15FireRuleWorld.Chunk, Array.Empty<Enemy>());
Check(p15FireRule.FindTarget(new[] { new Enemy(21, 5, 23) }) != null, "slice15 patrol mid-route fires at an enemy in range");
var p15HoldFireWorld = new WorldZone(42);
p15HoldFireWorld.Player.X = 16; p15HoldFireWorld.Player.Z = 16;
var p15HoldFire = p15HoldFireWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.HoldGround)!;
Check(p15HoldFire.FindTarget(new[] { new Enemy(16, 5, 19) }) == null, "slice15 patrol contrast holdground moving does not fire");
// Kinds: all three patrol, with their own shapes — Recon
// (2 x 20 HP, 6 damage, range 5) and Heavy (2 x 50 HP, 8
// damage, range 2) step east from the rally like Vanguard.
var p15ReconWorld = new WorldZone(42);
p15ReconWorld.Player.X = 20; p15ReconWorld.Player.Z = 20;
var p15Recon = p15ReconWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Recon)!;
Check(p15Recon.MoveForPatrol(p15ReconWorld.Chunk, Array.Empty<Enemy>()), "slice15 patrol recon steps east from the rally");
Eq(12, p15Recon.DamagePerTick, "slice15 patrol recon damage");
Eq(5, p15Recon.EngagementRange, "slice15 patrol recon range");
var p15HeavyWorld = new WorldZone(42);
p15HeavyWorld.Player.X = 20; p15HeavyWorld.Player.Z = 20;
var p15Heavy = p15HeavyWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Heavy)!;
Check(p15Heavy.MoveForPatrol(p15HeavyWorld.Chunk, Array.Empty<Enemy>()), "slice15 patrol heavy steps east from the rally");
Eq(16, p15Heavy.DamagePerTick, "slice15 patrol heavy damage");
Eq(2, p15Heavy.EngagementRange, "slice15 patrol heavy range");
// Picker and economy, unchanged: Patrol deploys through the
// paid path (10 EmberOre, cap 3 live squads), and a fourth
// deploy at the cap is refused with no charge.
var p15PickWorld = new WorldZone(42);
p15PickWorld.Player.Inventory.Add(BlockType.EmberOre, 40);
var p15Cmd = new OrbitalCommand();
Check(p15Cmd.SelectDoctrine(TacticalDoctrine.Patrol), "slice15 patrol picker selects patrol");
Check(!p15Cmd.SelectDoctrine((TacticalDoctrine)99), "slice15 patrol picker rejects an unknown doctrine");
Eq(TacticalDoctrine.Patrol, p15Cmd.Doctrine, "slice15 patrol picker rejection leaves patrol selected");
p15Cmd.BeginPick();
Check(p15Cmd.SetRally(20, 5, 20), "slice15 patrol picker rally set");
p15Cmd.SelectKind(SquadKind.Heavy);
var p15Paid = p15Cmd.Confirm(p15PickWorld);
Check(p15Paid != null, "slice15 patrol paid deploy succeeds");
Eq(TacticalDoctrine.Patrol, p15Paid!.Doctrine, "slice15 patrol paid deploy doctrine");
Eq(SquadKind.Heavy, p15Paid.Kind, "slice15 patrol paid deploy kind");
Eq(30, p15PickWorld.Player.Inventory.Get(BlockType.EmberOre), "slice15 patrol paid deploy charges 10 ore");
Check(p15PickWorld.TryDeployPaidSquad(20, 5, 20, TacticalDoctrine.Patrol) != null, "slice15 patrol second paid deploy succeeds");
Check(p15PickWorld.TryDeployPaidSquad(20, 5, 20, TacticalDoctrine.Patrol) != null, "slice15 patrol third paid deploy succeeds");
Check(p15PickWorld.TryDeployPaidSquad(20, 5, 20, TacticalDoctrine.Patrol) == null, "slice15 patrol fourth deploy refused at cap");
Eq(10, p15PickWorld.Player.Inventory.Get(BlockType.EmberOre), "slice15 patrol cap refusal charges nothing");
// Snapshot: a patrolling squad reads Patrolling — not
// Marching, not Hunting — with no hunting target, even with
// an enemy in range, because it chases nothing; and it still
// reads Patrolling mid-route.
var p15SnapWorld = new WorldZone(42);
p15SnapWorld.Player.X = 20; p15SnapWorld.Player.Z = 20;
var p15Snap = p15SnapWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
p15SnapWorld.Enemies.Add(new Enemy(21, 5, 20));
var p15Snap0 = OrbitalView.Snapshot(p15SnapWorld)[0];
Eq(SquadActivity.Patrolling, p15Snap0.Activity, "slice15 patrol snapshot at rally reads patrolling");
Check(!p15Snap0.HasHuntingTarget, "slice15 patrol snapshot has no hunting target with an enemy in range");
Eq(TacticalDoctrine.Patrol, p15Snap0.Doctrine, "slice15 patrol snapshot doctrine");
p15SnapWorld.Enemies.Clear();
p15Snap.MoveForPatrol(p15SnapWorld.Chunk, Array.Empty<Enemy>());
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(p15SnapWorld)[0].Activity, "slice15 patrol snapshot mid-route reads patrolling");
// Save / load: doctrine, kind, health, position, and rally
// survive, but the waypoint index resets to 0 — the loaded
// patrol marches to its rally first, then resumes the loop.
// No save field was added, so old saves load unchanged.
var p15SaveWorld = new WorldZone(42);
p15SaveWorld.Player.X = 20; p15SaveWorld.Player.Z = 20;
var p15Save = p15SaveWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Recon)!;
p15Save.MoveForPatrol(p15SaveWorld.Chunk, Array.Empty<Enemy>());
p15Save.MoveForPatrol(p15SaveWorld.Chunk, Array.Empty<Enemy>());
p15Save.MoveForPatrol(p15SaveWorld.Chunk, Array.Empty<Enemy>());
Eq(1, p15Save.PatrolWaypointIndex, "slice15 patrol save source is mid-route at waypoint 1");
var p15SaveLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(p15SaveWorld));
Eq(1, p15SaveLoaded.Squads.Count, "slice15 patrol save/load keeps the squad");
var p15Loaded = p15SaveLoaded.Squads[0];
Eq(TacticalDoctrine.Patrol, p15Loaded.Doctrine, "slice15 patrol save/load doctrine preserved");
Eq(SquadKind.Recon, p15Loaded.Kind, "slice15 patrol save/load kind preserved");
Eq(23, p15Loaded.X, "slice15 patrol save/load position preserved");
Eq(20, p15Loaded.RallyX, "slice15 patrol save/load rally preserved");
Eq(0, p15Loaded.PatrolWaypointIndex, "slice15 patrol save/load waypoint index resets to 0");
Check(p15Loaded.MoveForPatrol(p15SaveLoaded.Chunk, Array.Empty<Enemy>()), "slice15 patrol loaded squad steps toward its rally");
Eq(22, p15Loaded.X, "slice15 patrol loaded squad steps west to its rally first");


// Slice 15 Step 2 composed integration: the demo layout as
// tests, so the demo numbers are proved, not just printed.
// One Simulation carries a Recon Patrol on its rally loop
// beside a Heavy HoldGround marching to its own rally, with
// one enemy meeting the patrol on the route, through ticks
// and a save/load round-trip. The Recon kind is deliberate:
// 12 damage per tick means a 30-HP enemy takes three hits,
// so the demo proves firing while still walking — a Vanguard
// kill in a single tick would not show the patrol walking on
// after a hit. The Heavy is the contrast: it marches, then
// holds and reads Holding, and never fires while moving.
var s15Sim = new Simulation(42);
s15Sim.World.Player.Inventory.Add(BlockType.EmberOre, 20);
s15Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol);
s15Sim.OrbitalCommand.SelectKind(SquadKind.Recon);
s15Sim.BeginOrbitalPick();
s15Sim.SetOrbitalRally(20, 5, 20);
var s15Patrol = s15Sim.ConfirmOrbitalDeploy();
s15Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround);
s15Sim.OrbitalCommand.SelectKind(SquadKind.Heavy);
s15Sim.BeginOrbitalPick();
s15Sim.SetOrbitalRally(10, 5, 10);
var s15Heavy = s15Sim.ConfirmOrbitalDeploy();
s15Sim.World.Enemies.Add(new Enemy(26, 5, 20));
Check(s15Patrol != null, "slice15 step2 composed patrol deploys paid through the picker");
Check(s15Heavy != null, "slice15 step2 composed heavy deploys paid through the picker");
Eq(0, s15Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice15 step2 composed two paid deploys spend all 20 ore");
Eq(TacticalDoctrine.Patrol, s15Sim.World.Squads[0].Doctrine, "slice15 step2 composed first squad doctrine patrol");
Eq(SquadKind.Recon, s15Sim.World.Squads[0].Kind, "slice15 step2 composed first squad kind recon");
Eq(TacticalDoctrine.HoldGround, s15Sim.World.Squads[1].Doctrine, "slice15 step2 composed second squad doctrine holdground");
Eq(SquadKind.Heavy, s15Sim.World.Squads[1].Kind, "slice15 step2 composed second squad kind heavy");
var s15SnapBefore = OrbitalView.Snapshot(s15Sim.World);
Eq(SquadActivity.Patrolling, s15SnapBefore[0].Activity, "slice15 step2 composed patrol reads patrolling before any tick");
Eq(SquadActivity.Marching, s15SnapBefore[1].Activity, "slice15 step2 composed heavy reads marching before any tick");
Eq(30, s15Sim.World.Enemies[0].Health, "slice15 step2 composed staged enemy 30 HP");
for (int s15Tick = 0; s15Tick < 5; s15Tick++) s15Sim.World.Advance();
Eq(19, s15Sim.World.Squads[0].X, "slice15 step2 composed patrol X after 5 ticks");
Eq(18, s15Sim.World.Squads[0].Z, "slice15 step2 composed patrol Z after 5 ticks");
Eq(0, s15Sim.World.Squads[0].PatrolWaypointIndex, "slice15 step2 composed patrol still on waypoint 0 after 5 ticks");
Eq(18, s15Sim.World.Enemies[0].Health, "slice15 step2 composed enemy 18 HP after 5 ticks: one 12-damage passing shot");
Eq(21, s15Sim.World.Enemies[0].X, "slice15 step2 composed enemy steps toward the patrol");
var s15Snap5 = OrbitalView.Snapshot(s15Sim.World)[0];
Eq(SquadActivity.Patrolling, s15Snap5.Activity, "slice15 step2 composed patrol still reads patrolling while firing in passing");
Check(!s15Snap5.HasHuntingTarget, "slice15 step2 composed firing patrol has no hunting target");
s15Sim.World.Advance();
Eq(19, s15Sim.World.Squads[0].X, "slice15 step2 composed patrol X after 6 ticks");
Eq(19, s15Sim.World.Squads[0].Z, "slice15 step2 composed patrol Z after 6 ticks");
Eq(6, s15Sim.World.Enemies[0].Health, "slice15 step2 composed enemy 6 HP after 6 ticks: second passing shot");
Eq(20, s15Sim.World.Enemies[0].X, "slice15 step2 composed enemy reaches the rally cell on tick 6");
Eq(20, s15Sim.World.Enemies[0].Z, "slice15 step2 composed enemy Z on the rally cell on tick 6");
s15Sim.World.Advance();
Eq(0, s15Sim.World.Enemies.Count, "slice15 step2 composed enemy dead on tick 7: third passing shot, no chase");
Eq(20, s15Sim.World.Squads[0].X, "slice15 step2 composed patrol X after the kill tick");
Eq(19, s15Sim.World.Squads[0].Z, "slice15 step2 composed patrol Z after the kill tick: still on route, not on the enemy cell");
s15Sim.World.Advance();
Eq(20, s15Sim.World.Squads[0].X, "slice15 step2 composed patrol reaches its rally on tick 8");
Eq(20, s15Sim.World.Squads[0].Z, "slice15 step2 composed patrol rally Z on tick 8");
Eq(0, s15Sim.World.Squads[0].PatrolWaypointIndex, "slice15 step2 composed patrol at rally waypoint on tick 8");
for (int s15Tick = 0; s15Tick < 3; s15Tick++) s15Sim.World.Advance();
Eq(23, s15Sim.World.Squads[0].X, "slice15 step2 composed patrol reaches the east waypoint on tick 11");
Eq(20, s15Sim.World.Squads[0].Z, "slice15 step2 composed patrol east waypoint Z on tick 11");
Eq(1, s15Sim.World.Squads[0].PatrolWaypointIndex, "slice15 step2 composed patrol east waypoint index on tick 11");
s15Sim.World.Advance();
Eq(22, s15Sim.World.Squads[0].X, "slice15 step2 composed patrol turns at the east waypoint without dwelling on tick 12");
Eq(2, s15Sim.World.Squads[0].PatrolWaypointIndex, "slice15 step2 composed patrol south waypoint index on tick 12");
Eq(10, s15Sim.World.Squads[1].X, "slice15 step2 composed heavy reaches its rally X on tick 12");
Eq(10, s15Sim.World.Squads[1].Z, "slice15 step2 composed heavy reaches its rally Z on tick 12");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s15Sim.World)[1].Activity, "slice15 step2 composed heavy reads holding on tick 12");
for (int s15Tick = 0; s15Tick < 5; s15Tick++) s15Sim.World.Advance();
Eq(20, s15Sim.World.Squads[0].X, "slice15 step2 composed patrol reaches the south waypoint on tick 17");
Eq(23, s15Sim.World.Squads[0].Z, "slice15 step2 composed patrol south waypoint Z on tick 17");
for (int s15Tick = 0; s15Tick < 6; s15Tick++) s15Sim.World.Advance();
Eq(17, s15Sim.World.Squads[0].X, "slice15 step2 composed patrol reaches the west waypoint on tick 23");
Eq(20, s15Sim.World.Squads[0].Z, "slice15 step2 composed patrol west waypoint Z on tick 23");
for (int s15Tick = 0; s15Tick < 6; s15Tick++) s15Sim.World.Advance();
Eq(20, s15Sim.World.Squads[0].X, "slice15 step2 composed patrol reaches the north waypoint on tick 29");
Eq(17, s15Sim.World.Squads[0].Z, "slice15 step2 composed patrol north waypoint Z on tick 29");
for (int s15Tick = 0; s15Tick < 3; s15Tick++) s15Sim.World.Advance();
Eq(20, s15Sim.World.Squads[0].X, "slice15 step2 composed patrol returns to its rally on tick 32: the loop repeats");
Eq(20, s15Sim.World.Squads[0].Z, "slice15 step2 composed patrol rally Z again on tick 32");
Eq(0, s15Sim.World.Squads[0].PatrolWaypointIndex, "slice15 step2 composed patrol rally waypoint index again on tick 32");
var s15Snap32 = OrbitalView.Snapshot(s15Sim.World);
Eq(SquadActivity.Patrolling, s15Snap32[0].Activity, "slice15 step2 composed patrol reads patrolling after a full circuit");
Eq(SquadActivity.Holding, s15Snap32[1].Activity, "slice15 step2 composed heavy still reads holding after 32 ticks");
Eq(100, s15Snap32[1].Health, "slice15 step2 composed heavy takes no damage on its march and hold");
Eq(0, s15Sim.World.Enemies.Count, "slice15 step2 composed no enemies after 32 ticks");
var s15Save = s15Sim.SaveToJson();
var s15Loaded = new Simulation(99);
Check(s15Loaded.LoadFromJson(s15Save), "slice15 step2 composed save loads");
Eq(20, s15Loaded.World.Squads[0].X, "slice15 step2 composed loaded patrol X at its rally");
Eq(20, s15Loaded.World.Squads[0].Z, "slice15 step2 composed loaded patrol Z at its rally");
Eq(0, s15Loaded.World.Squads[0].PatrolWaypointIndex, "slice15 step2 composed loaded patrol waypoint index resets to 0");
Eq(10, s15Loaded.World.Squads[1].X, "slice15 step2 composed loaded heavy X at its rally");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s15Loaded.World)[1].Activity, "slice15 step2 composed loaded heavy reads holding");
Eq(0, s15Loaded.World.Enemies.Count, "slice15 step2 composed kill stays dead after load");
s15Loaded.World.Advance();
Eq(21, s15Loaded.World.Squads[0].X, "slice15 step2 composed loaded patrol turns east on tick 1: reset proved, a persisted waypoint 4 would step north");
Eq(1, s15Loaded.World.Squads[0].PatrolWaypointIndex, "slice15 step2 composed loaded patrol east waypoint index on tick 1");
for (int s15Tick = 0; s15Tick < 2; s15Tick++) s15Loaded.World.Advance();
Eq(23, s15Loaded.World.Squads[0].X, "slice15 step2 composed loaded patrol reaches the east waypoint again");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s15Loaded.World)[0].Activity, "slice15 step2 composed loaded patrol reads patrolling on its resumed loop");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s15Loaded.World)[1].Activity, "slice15 step2 composed loaded heavy still reads holding");

// ---- Slice 17 Step 1: single-cable tint query (0.74.0) ----
// Decision, in one sentence a later reader can quote: for a
// cable cell, Energised now means its cable component carries
// a non-None voltage, not that voltage is delivered to that
// cell itself. The proof set below covers every case the
// Slice 17 scope requires, plus the delivery-unchanged proofs
// that separate a PowerState change from a DeliveredVoltage
// change. Each layout is its own WorldZone, so no case can
// leak voltage into another through a shared chunk.
// Motivating case: single cable, Conduit -> cable -> mill.
var s17Mill = new WorldZone(42);
s17Mill.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s17Mill.Chunk.TrySet(17, 20, 16, BlockType.Cable);
s17Mill.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
foreach (var (x, y, z) in new[] { (15, 20, 16), (16, 21, 16), (16, 19, 16), (16, 20, 17), (16, 20, 15) })
    s17Mill.Chunk.TrySet(x, y, z, BlockType.Stone);
Eq(CellPowerState.Energised, s17Mill.PowerState(17, 20, 16), "slice17 step1 single conduit cable to a mill is Energised, not None");
Eq(CellPowerState.Mismatched, s17Mill.PowerState(16, 20, 16), "slice17 step1 conduit cable mill is Mismatched beside its Energised cable");
Eq(Voltage.HV, s17Mill.DeliveredVoltage(16, 20, 16), "slice17 step1 conduit cable mill delivery is still HV");
Eq(1, HudState.FromWorld(s17Mill, new Weapon()).WarnedMills, "slice17 step1 conduit cable mill still warns one mill");
Check(Mesher.GreedyMesh(s17Mill.Chunk, s17Mill.PowerState).Any(q => q.Type == BlockType.Cable && q.PowerState == CellPowerState.Energised), "slice17 step1 single conduit cable carries an Energised mesh quad");
s17Mill.Advance();
Eq(BlockType.Air, s17Mill.Chunk.Get(16, 20, 16), "slice17 step1 conduit cable mill is still destroyed on tick 1");
Eq(CellPowerState.Energised, s17Mill.PowerState(17, 20, 16), "slice17 step1 cable stays Energised after its mill is destroyed: the component is still live");
// Single cable, Conduit and air: live network, no consumer.
var s17Air = new WorldZone(42);
s17Air.Chunk.TrySet(17, 20, 16, BlockType.Cable);
s17Air.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
Eq(CellPowerState.Energised, s17Air.PowerState(17, 20, 16), "slice17 step1 single conduit cable to air is Energised");
Eq((1, 0), s17Air.LivePowerCounts(), "slice17 step1 single conduit cable to air counts one live network, no lone source");
// Two Conduits on one single cable: one component, one voltage.
var s17TwoCond = new WorldZone(42);
s17TwoCond.Chunk.TrySet(16, 20, 16, BlockType.Conduit);
s17TwoCond.Chunk.TrySet(17, 20, 16, BlockType.Cable);
s17TwoCond.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
Eq(CellPowerState.Energised, s17TwoCond.PowerState(17, 20, 16), "slice17 step1 single cable between two Conduits is Energised");
// Two-cable Conduit run: already Energised, stays so.
var s17TwoCable = new WorldZone(42);
s17TwoCable.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s17TwoCable.Chunk.TrySet(17, 20, 16, BlockType.Cable);
s17TwoCable.Chunk.TrySet(18, 20, 16, BlockType.Cable);
s17TwoCable.Chunk.TrySet(19, 20, 16, BlockType.Conduit);
Eq(CellPowerState.Energised, s17TwoCable.PowerState(17, 20, 16), "slice17 step1 two-cable conduit run first cable stays Energised");
Eq(CellPowerState.Energised, s17TwoCable.PowerState(18, 20, 16), "slice17 step1 two-cable conduit run second cable stays Energised");
// Dynamo-fed and Alternator-fed single cables: already
// Energised by direct delivery, stay so under the component
// reading (LV and HV respectively, no voltage split in tint).
var s17Dyn = new WorldZone(42);
s17Dyn.Chunk.TrySet(16, 20, 16, BlockType.Dynamo);
s17Dyn.Chunk.TrySet(17, 20, 16, BlockType.Cable);
Eq(CellPowerState.Energised, s17Dyn.PowerState(17, 20, 16), "slice17 step1 Dynamo-fed single cable stays Energised");
Eq(Voltage.LV, s17Dyn.DeliveredVoltage(17, 20, 16), "slice17 step1 Dynamo-fed single cable delivery is still LV");
var s17Alt = new WorldZone(42);
s17Alt.Chunk.TrySet(16, 20, 16, BlockType.Alternator);
s17Alt.Chunk.TrySet(17, 20, 16, BlockType.Cable);
Eq(CellPowerState.Energised, s17Alt.PowerState(17, 20, 16), "slice17 step1 Alternator-fed single cable stays Energised");
// Transformer-fed single cables: already Energised through
// the second branch, stay so (Alternator flips to LV, Dynamo
// flips to HV — the cable reads Energised either way).
var s17TransAlt = new WorldZone(42);
s17TransAlt.Chunk.TrySet(15, 20, 16, BlockType.Alternator);
s17TransAlt.Chunk.TrySet(16, 20, 16, BlockType.Transformer);
s17TransAlt.Chunk.TrySet(17, 20, 16, BlockType.Cable);
Eq(CellPowerState.Energised, s17TransAlt.PowerState(17, 20, 16), "slice17 step1 Alternator-transformer-fed single cable stays Energised");
var s17TransDyn = new WorldZone(42);
s17TransDyn.Chunk.TrySet(15, 20, 16, BlockType.Dynamo);
s17TransDyn.Chunk.TrySet(16, 20, 16, BlockType.Transformer);
s17TransDyn.Chunk.TrySet(17, 20, 16, BlockType.Cable);
Eq(CellPowerState.Energised, s17TransDyn.PowerState(17, 20, 16), "slice17 step1 Dynamo-transformer-fed single cable stays Energised");
// Mixed-voltage components: the component reading must NOT
// resurrect a dead network. Dynamo + Conduit, Dynamo +
// Alternator, and a two-cable Dynamo + Conduit run all
// resolve to None, so every cable stays untinted.
var s17MixedSingle = new WorldZone(42);
s17MixedSingle.Chunk.TrySet(16, 20, 16, BlockType.Dynamo);
s17MixedSingle.Chunk.TrySet(17, 20, 16, BlockType.Cable);
s17MixedSingle.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
Eq(CellPowerState.None, s17MixedSingle.PowerState(17, 20, 16), "slice17 step1 mixed Dynamo-Conduit single cable stays None");
var s17MixedAlt = new WorldZone(42);
s17MixedAlt.Chunk.TrySet(16, 20, 16, BlockType.Dynamo);
s17MixedAlt.Chunk.TrySet(17, 20, 16, BlockType.Cable);
s17MixedAlt.Chunk.TrySet(18, 20, 16, BlockType.Alternator);
Eq(CellPowerState.None, s17MixedAlt.PowerState(17, 20, 16), "slice17 step1 mixed Dynamo-Alternator single cable stays None");
var s17MixedTwo = new WorldZone(42);
s17MixedTwo.Chunk.TrySet(16, 20, 16, BlockType.Dynamo);
s17MixedTwo.Chunk.TrySet(17, 20, 16, BlockType.Cable);
s17MixedTwo.Chunk.TrySet(18, 20, 16, BlockType.Cable);
s17MixedTwo.Chunk.TrySet(19, 20, 16, BlockType.Conduit);
Eq(CellPowerState.None, s17MixedTwo.PowerState(17, 20, 16), "slice17 step1 mixed two-cable run first cable stays None");
Eq(CellPowerState.None, s17MixedTwo.PowerState(18, 20, 16), "slice17 step1 mixed two-cable run second cable stays None");
// Unpowered components: None under either reading.
var s17Dead = new WorldZone(42);
s17Dead.Chunk.TrySet(17, 20, 16, BlockType.Cable);
Eq(CellPowerState.None, s17Dead.PowerState(17, 20, 16), "slice17 step1 unpowered single cable is None");
var s17DeadStone = new WorldZone(42);
s17DeadStone.Chunk.TrySet(17, 20, 16, BlockType.Cable);
s17DeadStone.Chunk.TrySet(18, 20, 16, BlockType.Stone);
Eq(CellPowerState.None, s17DeadStone.PowerState(17, 20, 16), "slice17 step1 cable beside stone only is None");
// Delivery unchanged, pylon proof: the single cable's tint
// changes, the pylon's fire does not — 10 per tick, as in
// Slice 14 Step 1, with the cable Energised before the tick.
var s17Pylon = new WorldZone(42);
s17Pylon.Chunk.TrySet(17, 5, 16, BlockType.ShockPylon);
s17Pylon.Chunk.TrySet(18, 5, 16, BlockType.Cable);
s17Pylon.Chunk.TrySet(19, 5, 16, BlockType.Conduit);
s17Pylon.Enemies.Add(new Enemy(17, 5, 20));
Eq(CellPowerState.Energised, s17Pylon.PowerState(18, 5, 16), "slice17 step1 conduit cable pylon single cable is Energised before firing");
Eq(Voltage.HV, s17Pylon.DeliveredVoltage(17, 5, 16), "slice17 step1 conduit cable pylon delivery is still HV");
Check(s17Pylon.IsPylonPowered(17, 5, 16), "slice17 step1 conduit cable pylon is still powered");
s17Pylon.Advance();
Eq(20, s17Pylon.Enemies[0].Health, "slice17 step1 conduit cable pylon still fires 10 on its first tick");
// Delivery unchanged, direct and workstation proofs: a
// Conduit beside a mill still delivers nothing and the mill
// survives, and a cable-fed workstation is still mismatched
// HV with regen 0 — the tint change touches neither.
var s17Direct = new WorldZone(42);
s17Direct.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s17Direct.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
Eq(Voltage.None, s17Direct.DeliveredVoltage(16, 20, 16), "slice17 step1 direct Conduit still delivers nothing to its mill");
Eq(CellPowerState.None, s17Direct.PowerState(16, 20, 16), "slice17 step1 direct Conduit mill power state is still None");
s17Direct.Advance();
Eq(BlockType.KineticMill, s17Direct.Chunk.Get(16, 20, 16), "slice17 step1 direct Conduit mill still survives its tick");
var s17Ws = new WorldZone(42);
s17Ws.Player.TakeDamage(20);
s17Ws.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
s17Ws.Chunk.TrySet(18, 5, 16, BlockType.Cable);
s17Ws.Chunk.TrySet(19, 5, 16, BlockType.Conduit);
Eq(Voltage.HV, s17Ws.DeliveredVoltage(17, 5, 16), "slice17 step1 conduit cable workstation delivery is still HV");
Check(s17Ws.IsWorkstationMismatched(17, 5, 16), "slice17 step1 conduit cable workstation is still mismatched");
Eq(0, s17Ws.EffectiveWorkstationRegen(), "slice17 step1 conduit cable workstation regen is still 0: disabled");
// Rewiring at query level, both placement orders: a cable
// alone reads None, gains Energised when a Conduit is placed
// beside it, and loses it again when that Conduit is removed
// — the query is derived on every call, never a saved tint.
var s17Rewire = new WorldZone(42);
s17Rewire.Chunk.TrySet(17, 20, 16, BlockType.Cable);
Eq(CellPowerState.None, s17Rewire.PowerState(17, 20, 16), "slice17 step1 cable alone reads None before its source is placed");
s17Rewire.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
Eq(CellPowerState.Energised, s17Rewire.PowerState(17, 20, 16), "slice17 step1 cable gains Energised when a Conduit is placed beside it");
s17Rewire.Chunk.TrySet(18, 20, 16, BlockType.Air);
Eq(CellPowerState.None, s17Rewire.PowerState(17, 20, 16), "slice17 step1 cable loses Energised when its Conduit is removed");
// Save/load re-derivation: the decided reading is derived,
// never saved, so a loaded single-cable run reads Energised.
var s17Load = new Simulation(42);
s17Load.World.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s17Load.World.Chunk.TrySet(17, 20, 16, BlockType.Cable);
s17Load.World.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
var s17LoadJson = s17Load.SaveToJson();
var s17Loaded = new Simulation(99);
Check(s17Loaded.LoadFromJson(s17LoadJson), "slice17 step1 single-cable world loads");
Eq(CellPowerState.Energised, s17Loaded.World.PowerState(17, 20, 16), "slice17 step1 single cable reads Energised after load");
Eq(CellPowerState.Mismatched, s17Loaded.World.PowerState(16, 20, 16), "slice17 step1 conduit cable mill reads Mismatched after load");

// ---- Slice 17 Step 2: direct-Conduit count query ----
// Decision, decided in this step (rationale in DECISIONS.md):
// a Conduit beside a consumer counts as lone, folded into the
// existing POWER line's lone number — no new line, no new word.
// Lone now means: a source with no cable or transformer neighbour,
// and no consumer neighbour that it feeds directly. Dynamo and
// Alternator feed any adjacent consumer directly (rated or
// mismatched alike), so a consumer neighbour still disqualifies
// them; a Conduit feeds no adjacent consumer, so it does not.
// A cable or transformer neighbour still disqualifies every
// source, Conduit included, even when that wiring delivers
// nothing — a transformer is wiring, not a consumer endpoint,
// and the dead-cable precedent says adjacency to wiring
// disqualifies even when the wiring is dead. The count is per
// source cell, not per consumer or per pair. Step 1 independence,
// stated explicitly: Step 1 decides cable tint (component
// liveness), and these cases have no cable, so Step 1's answer
// changes no Step 2 case; the counts keep the Slice 12 meaning
// of "live" (carrying a non-None voltage, feeding no consumer)
// and do not adopt Step 1's tint vocabulary for counts.
// Motivating case: a Conduit directly beside a mill.
var s17cMill = new WorldZone(42);
s17cMill.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s17cMill.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
Eq((0, 1), s17cMill.LivePowerCounts(), "slice17 step2 conduit beside a mill counts 0 live networks, 1 lone source");
Eq(Voltage.None, s17cMill.DeliveredVoltage(16, 20, 16), "slice17 step2 direct conduit mill delivery is still None");
Eq(CellPowerState.None, s17cMill.PowerState(16, 20, 16), "slice17 step2 direct conduit mill power state is still None");
Eq(0, HudState.FromWorld(s17cMill, new Weapon()).PoweredMills, "slice17 step2 direct conduit mill is not powered in the consumer count");
Eq(1, HudState.FromWorld(s17cMill, new Weapon()).TotalMills, "slice17 step2 direct conduit mill still totals one mill");
Eq(0, HudState.FromWorld(s17cMill, new Weapon()).WarnedMills, "slice17 step2 direct conduit mill is not warned in the consumer count");
Eq(1, HudState.FromWorld(s17cMill, new Weapon()).LoneSources, "slice17 step2 hud carries the direct conduit as one lone source");
s17cMill.Advance();
Eq(BlockType.KineticMill, s17cMill.Chunk.Get(16, 20, 16), "slice17 step2 direct conduit mill still survives its tick");
Eq((0, 1), s17cMill.LivePowerCounts(), "slice17 step2 direct conduit still counts 1 lone source after its mill's tick");
// One rule for all three consumers: workstation, then pylon.
var s17cWs = new WorldZone(42);
s17cWs.Chunk.TrySet(17, 5, 16, BlockType.Workstation);
s17cWs.Chunk.TrySet(18, 5, 16, BlockType.Conduit);
Eq((0, 1), s17cWs.LivePowerCounts(), "slice17 step2 conduit beside a workstation counts 0 live networks, 1 lone source");
Eq(Voltage.None, s17cWs.DeliveredVoltage(17, 5, 16), "slice17 step2 direct conduit workstation delivery is still None");
Check(!s17cWs.IsWorkstationPowered(17, 5, 16), "slice17 step2 direct conduit workstation is not powered");
Check(!s17cWs.IsWorkstationMismatched(17, 5, 16), "slice17 step2 direct conduit workstation is not mismatched");
Eq(0, HudState.FromWorld(s17cWs, new Weapon()).PoweredWorkstations, "slice17 step2 direct conduit workstation is not powered in the consumer count");
Eq(1, HudState.FromWorld(s17cWs, new Weapon()).TotalWorkstations, "slice17 step2 direct conduit workstation still totals one workstation");
var s17cPylon = new WorldZone(42);
s17cPylon.Chunk.TrySet(17, 5, 16, BlockType.ShockPylon);
s17cPylon.Chunk.TrySet(18, 5, 16, BlockType.Conduit);
Eq((0, 1), s17cPylon.LivePowerCounts(), "slice17 step2 conduit beside a pylon counts 0 live networks, 1 lone source");
Eq(Voltage.None, s17cPylon.DeliveredVoltage(17, 5, 16), "slice17 step2 direct conduit pylon delivery is still None");
Check(!s17cPylon.IsPylonPowered(17, 5, 16), "slice17 step2 direct conduit pylon is not powered");
Check(!s17cPylon.IsPylonMismatched(17, 5, 16), "slice17 step2 direct conduit pylon is not mismatched");
Eq(0, HudState.FromWorld(s17cPylon, new Weapon()).PoweredPylons, "slice17 step2 direct conduit pylon is not powered in the consumer count");
Eq(1, HudState.FromWorld(s17cPylon, new Weapon()).TotalPylons, "slice17 step2 direct conduit pylon still totals one pylon");
// Feeding cases: neither a Dynamo nor an Alternator beside a
// mill becomes lone under the new rule, whatever it delivers.
var s17cDyn = new WorldZone(42);
s17cDyn.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s17cDyn.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
Eq((0, 0), s17cDyn.LivePowerCounts(), "slice17 step2 dynamo beside a mill counts neither live nor lone");
Eq(Voltage.LV, s17cDyn.DeliveredVoltage(16, 20, 16), "slice17 step2 dynamo beside a mill still delivers LV");
Check(s17cDyn.IsPowered(16, 20, 16), "slice17 step2 dynamo beside a mill is still powered");
var s17cAlt = new WorldZone(42);
s17cAlt.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s17cAlt.Chunk.TrySet(17, 20, 16, BlockType.Alternator);
Eq((0, 0), s17cAlt.LivePowerCounts(), "slice17 step2 alternator beside a mill counts neither live nor lone");
Eq(Voltage.HV, s17cAlt.DeliveredVoltage(16, 20, 16), "slice17 step2 alternator beside a mill still delivers HV");
Check(s17cAlt.IsMismatched(16, 20, 16), "slice17 step2 alternator beside a mill is still mismatched");
Eq(1, HudState.FromWorld(s17cAlt, new Weapon()).WarnedMills, "slice17 step2 alternator beside a mill still warns one mill");
var s17cDynPylon = new WorldZone(42);
s17cDynPylon.Chunk.TrySet(17, 5, 16, BlockType.ShockPylon);
s17cDynPylon.Chunk.TrySet(18, 5, 16, BlockType.Dynamo);
Eq((0, 0), s17cDynPylon.LivePowerCounts(), "slice17 step2 dynamo beside a pylon counts neither live nor lone, mismatched LV included");
Check(s17cDynPylon.IsPylonMismatched(17, 5, 16), "slice17 step2 dynamo beside a pylon is still mismatched LV");
var s17cAltPylon = new WorldZone(42);
s17cAltPylon.Chunk.TrySet(17, 5, 16, BlockType.ShockPylon);
s17cAltPylon.Chunk.TrySet(18, 5, 16, BlockType.Alternator);
Eq((0, 0), s17cAltPylon.LivePowerCounts(), "slice17 step2 alternator beside a pylon counts neither live nor lone");
Check(s17cAltPylon.IsPylonPowered(17, 5, 16), "slice17 step2 alternator beside a pylon is still powered HV");
// Conflict is still feeding, not lone: a Dynamo and an
// Alternator beside one mill deliver None collectively, and
// neither source counts as lone — counting either would hide
// the conflict behind a lone number.
var s17cConflict = new WorldZone(42);
s17cConflict.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s17cConflict.Chunk.TrySet(17, 20, 16, BlockType.Dynamo);
s17cConflict.Chunk.TrySet(15, 20, 16, BlockType.Alternator);
Eq((0, 0), s17cConflict.LivePowerCounts(), "slice17 step2 dynamo plus alternator beside one mill count neither live nor lone");
Eq(Voltage.None, s17cConflict.DeliveredVoltage(16, 20, 16), "slice17 step2 direct-source conflict still delivers None");
Check(!s17cConflict.IsPowered(16, 20, 16), "slice17 step2 conflicted mill is not powered");
Check(!s17cConflict.IsMismatched(16, 20, 16), "slice17 step2 conflicted mill is not mismatched, None is neither");
// A Conduit beside an already-fed mill is still lone: it feeds
// nothing itself, even though its neighbour mill is powered
// by another source, or mismatched by one.
var s17cDynPlus = new WorldZone(42);
s17cDynPlus.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s17cDynPlus.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
s17cDynPlus.Chunk.TrySet(15, 20, 16, BlockType.Dynamo);
Eq((0, 1), s17cDynPlus.LivePowerCounts(), "slice17 step2 conduit beside a dynamo-fed mill counts 1 lone source, dynamo counts neither");
Eq(Voltage.LV, s17cDynPlus.DeliveredVoltage(16, 20, 16), "slice17 step2 dynamo still decides a mill beside a conduit at LV");
var s17cAltPlus = new WorldZone(42);
s17cAltPlus.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s17cAltPlus.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
s17cAltPlus.Chunk.TrySet(15, 20, 16, BlockType.Alternator);
Eq((0, 1), s17cAltPlus.LivePowerCounts(), "slice17 step2 conduit beside an alternator-fed mill counts 1 lone source, alternator counts neither");
Check(s17cAltPlus.IsMismatched(16, 20, 16), "slice17 step2 alternator still mismatches a mill beside a conduit");
// A Conduit beside a mill that also touches a cable is not
// double-counted: the fed network counts as fed (0 live, 0
// lone), and the free network counts as live (1 live, 0 lone).
var s17cFedCable = new WorldZone(42);
s17cFedCable.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s17cFedCable.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
s17cFedCable.Chunk.TrySet(17, 20, 17, BlockType.Cable);
s17cFedCable.Chunk.TrySet(16, 20, 17, BlockType.Cable);
Eq((0, 0), s17cFedCable.LivePowerCounts(), "slice17 step2 conduit beside a mill and touching its feeding cable counts neither live nor lone");
Eq(Voltage.HV, s17cFedCable.DeliveredVoltage(16, 20, 16), "slice17 step2 conduit cable mill still delivers HV through the cable");
var s17cFreeCable = new WorldZone(42);
s17cFreeCable.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s17cFreeCable.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
s17cFreeCable.Chunk.TrySet(18, 20, 16, BlockType.Cable);
Eq((1, 0), s17cFreeCable.LivePowerCounts(), "slice17 step2 conduit beside a mill and touching a free cable counts 1 live network, 0 lone sources");
// Per source cell, not per consumer or per pair.
var s17cTwoMill = new WorldZone(42);
s17cTwoMill.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s17cTwoMill.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
s17cTwoMill.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
Eq((0, 2), s17cTwoMill.LivePowerCounts(), "slice17 step2 two conduits beside one mill count 2 lone sources");
var s17cTwoConsumer = new WorldZone(42);
s17cTwoConsumer.Chunk.TrySet(15, 20, 16, BlockType.KineticMill);
s17cTwoConsumer.Chunk.TrySet(16, 20, 16, BlockType.Conduit);
s17cTwoConsumer.Chunk.TrySet(17, 20, 16, BlockType.KineticMill);
Eq((0, 1), s17cTwoConsumer.LivePowerCounts(), "slice17 step2 one conduit beside two mills counts 1 lone source, not one per pair");
Eq(Voltage.None, s17cTwoConsumer.DeliveredVoltage(15, 20, 16), "slice17 step2 one conduit beside two mills delivers nothing to the first mill");
Eq(Voltage.None, s17cTwoConsumer.DeliveredVoltage(17, 20, 16), "slice17 step2 one conduit beside two mills delivers nothing to the second mill");
// Transformer adjacency stayed disqualifying in Slice 17 Step 2
// on its own grounds: a transformer is wiring, not a consumer
// endpoint, so a Conduit beside one counted neither, even
// though the transformer outputs None with no other input.
// Slice 19 Step 1 (0.82.0) supersedes that reading for a dead
// standalone transformer only: a Conduit is absent from the
// transformer's direct branch, so a dead group is not wiring
// for it, and the Conduit below now counts as one lone source
// — the history is kept, the assertion is not weakened, it is
// re-decided. A Conduit beside a live transformer still counts
// neither, as the next world proves.
var s17cTx = new WorldZone(42);
s17cTx.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
s17cTx.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
s17cTx.Chunk.TrySet(17, 20, 15, BlockType.KineticMill);
Eq((0, 1), s17cTx.LivePowerCounts(), "slice19 step1 conduit beside a dead transformer counts 0 live networks, 1 lone source, superseding slice17 step2");
Eq(Voltage.None, s17cTx.DeliveredVoltage(17, 20, 15), "slice17 step2 transformer beside a conduit alone outputs None to its probe mill");
var s17cTxLive = new WorldZone(42);
s17cTxLive.Chunk.TrySet(16, 20, 16, BlockType.Dynamo);
s17cTxLive.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
s17cTxLive.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
s17cTxLive.Chunk.TrySet(17, 20, 15, BlockType.KineticMill);
Eq((0, 0), s17cTxLive.LivePowerCounts(), "slice17 step2 conduit beside a live transformer feeding a mill counts neither live nor lone");
Eq(Voltage.HV, s17cTxLive.DeliveredVoltage(17, 20, 15), "slice17 step2 dynamo-fed transformer still outputs stepped-up HV to its probe mill");
// Control: a lone Conduit with no neighbour at all still counts.
var s17cLone = new WorldZone(42);
s17cLone.Chunk.TrySet(16, 20, 16, BlockType.Conduit);
Eq((0, 1), s17cLone.LivePowerCounts(), "slice17 step2 lone conduit with no neighbour still counts 1 lone source");
// Save/load re-derivation: the decided count is derived, never
// saved, so a loaded direct-Conduit world reads it again.
var s17cSim = new Simulation(42);
s17cSim.World.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s17cSim.World.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
Eq((0, 1), s17cSim.World.LivePowerCounts(), "slice17 step2 direct conduit counts 1 lone source before save");
var s17cSave = s17cSim.SaveToJson();
var s17cLoaded = new Simulation(99);
Check(s17cLoaded.LoadFromJson(s17cSave), "slice17 step2 direct conduit world loads");
Eq((0, 1), s17cLoaded.World.LivePowerCounts(), "slice17 step2 direct conduit counts 1 lone source after load");
Eq(1, s17cLoaded.Hud().LoneSources, "slice17 step2 hud carries the direct conduit lone count after load");
Eq(0, s17cLoaded.Hud().PoweredMills, "slice17 step2 direct conduit mill is still not powered after load");
Eq(1, s17cLoaded.Hud().TotalMills, "slice17 step2 direct conduit mill still totals one mill after load");

// ---- Slice 17 Step 3 composed integration: the demo layout as
// tests, so the demo numbers are proved, not just printed.
// One Simulation carries the Step 1 single-cable run, the
// two-cable control, the mixed-voltage control, the Step 2
// direct-Conduit mill, the Dynamo-fed control mill, and the
// lone Conduit together through ticks and a save/load
// round-trip. Rows are four cells apart in z, so no row can
// leak voltage into another through a face-adjacent
// neighbour, and every aggregate below is a total over all
// six rows, not a per-row reading — the composed claim is
// exactly that the two decided readings agree in one wiring.
var s17Sim = new Simulation(42);
void SeedPocketS17Test(WorldZone w, int mx, int mz)
{
    foreach (var (x, y, z) in new[] { (mx - 1, 20, mz), (mx, 21, mz), (mx, 19, mz), (mx, 20, mz - 1), (mx, 20, mz + 1) })
        w.Chunk.TrySet(x, y, z, BlockType.Stone);
}
s17Sim.World.Chunk.TrySet(10, 20, 10, BlockType.KineticMill);
s17Sim.World.Chunk.TrySet(11, 20, 10, BlockType.Cable);
s17Sim.World.Chunk.TrySet(12, 20, 10, BlockType.Conduit);
SeedPocketS17Test(s17Sim.World, 10, 10);
s17Sim.World.Chunk.TrySet(10, 20, 14, BlockType.KineticMill);
s17Sim.World.Chunk.TrySet(11, 20, 14, BlockType.Cable);
s17Sim.World.Chunk.TrySet(12, 20, 14, BlockType.Cable);
s17Sim.World.Chunk.TrySet(13, 20, 14, BlockType.Conduit);
SeedPocketS17Test(s17Sim.World, 10, 14);
s17Sim.World.Chunk.TrySet(10, 20, 18, BlockType.Dynamo);
s17Sim.World.Chunk.TrySet(11, 20, 18, BlockType.Cable);
s17Sim.World.Chunk.TrySet(12, 20, 18, BlockType.Conduit);
s17Sim.World.Chunk.TrySet(10, 20, 22, BlockType.KineticMill);
s17Sim.World.Chunk.TrySet(11, 20, 22, BlockType.Conduit);
SeedPocketS17Test(s17Sim.World, 10, 22);
s17Sim.World.Chunk.TrySet(10, 20, 26, BlockType.KineticMill);
s17Sim.World.Chunk.TrySet(11, 20, 26, BlockType.Dynamo);
SeedPocketS17Test(s17Sim.World, 10, 26);
s17Sim.World.Chunk.TrySet(10, 20, 30, BlockType.Conduit);
Eq(Voltage.HV, s17Sim.World.DeliveredVoltage(10, 20, 10), "slice17 step3 composed single-cable mill voltage HV");
Eq(Voltage.HV, s17Sim.World.DeliveredVoltage(10, 20, 14), "slice17 step3 composed two-cable mill voltage HV");
Eq(Voltage.None, s17Sim.World.DeliveredVoltage(10, 20, 22), "slice17 step3 composed direct-conduit mill voltage None");
Eq(Voltage.LV, s17Sim.World.DeliveredVoltage(10, 20, 26), "slice17 step3 composed dynamo mill voltage LV");
Check(s17Sim.World.IsMismatched(10, 20, 10), "slice17 step3 composed single-cable mill is mismatched");
Check(s17Sim.World.IsMismatched(10, 20, 14), "slice17 step3 composed two-cable mill is mismatched");
Check(!s17Sim.World.IsPowered(10, 20, 22), "slice17 step3 composed direct-conduit mill is not powered");
Check(!s17Sim.World.IsMismatched(10, 20, 22), "slice17 step3 composed direct-conduit mill is not mismatched");
Check(s17Sim.World.IsPowered(10, 20, 26), "slice17 step3 composed dynamo mill is powered");
Eq(CellPowerState.Mismatched, s17Sim.World.PowerState(10, 20, 10), "slice17 step3 composed single-cable mill state Mismatched");
Eq(CellPowerState.Energised, s17Sim.World.PowerState(11, 20, 10), "slice17 step3 composed single cable state Energised");
Eq(CellPowerState.Mismatched, s17Sim.World.PowerState(10, 20, 14), "slice17 step3 composed two-cable mill state Mismatched");
Eq(CellPowerState.Energised, s17Sim.World.PowerState(11, 20, 14), "slice17 step3 composed two-cable run first cable Energised");
Eq(CellPowerState.Energised, s17Sim.World.PowerState(12, 20, 14), "slice17 step3 composed two-cable run second cable Energised");
Eq(CellPowerState.None, s17Sim.World.PowerState(11, 20, 18), "slice17 step3 composed mixed cable state None");
Eq(CellPowerState.None, s17Sim.World.PowerState(10, 20, 22), "slice17 step3 composed direct-conduit mill state None");
Eq(CellPowerState.Powered, s17Sim.World.PowerState(10, 20, 26), "slice17 step3 composed dynamo mill state Powered");
Eq(CellPowerState.None, s17Sim.World.PowerState(12, 20, 10), "slice17 step3 composed single-run conduit state None by decision");
Eq(CellPowerState.None, s17Sim.World.PowerState(11, 20, 22), "slice17 step3 composed direct conduit state None by decision");
Eq(CellPowerState.None, s17Sim.World.PowerState(10, 20, 30), "slice17 step3 composed lone conduit state None by decision");
Eq((0, 2), s17Sim.World.LivePowerCounts(), "slice17 step3 composed counts before the tick are 0 live, 2 lone");
var s17HudBefore = s17Sim.Hud();
Eq(1, s17HudBefore.PoweredMills, "slice17 step3 composed one mill powered before the tick");
Eq(4, s17HudBefore.TotalMills, "slice17 step3 composed four mills total before the tick");
Eq(2, s17HudBefore.WarnedMills, "slice17 step3 composed two mills warned before the tick");
Eq(0, s17HudBefore.LiveNetworks, "slice17 step3 composed no live network before the tick: both cable runs feed a mill");
Eq(2, s17HudBefore.LoneSources, "slice17 step3 composed two lone sources before the tick: direct conduit and lone conduit");
Eq(0, s17Sim.World.Player.Inventory.Get(BlockType.Stone), "slice17 step3 composed stone starts at 0");
s17Sim.World.Advance();
Eq(BlockType.Air, s17Sim.World.Chunk.Get(10, 20, 10), "slice17 step3 composed single-cable mill destroyed on tick 1");
Eq(BlockType.Air, s17Sim.World.Chunk.Get(10, 20, 14), "slice17 step3 composed two-cable mill destroyed on tick 1");
Eq(2, s17Sim.World.DestroyedMills, "slice17 step3 composed two mills destroyed after tick 1");
Eq(1, s17Sim.World.PoweredBlocksGathered, "slice17 step3 composed powered gathered 1 after tick 1");
Eq(0, s17Sim.World.KineticBlocksGathered, "slice17 step3 composed kinetic gathered 0 after tick 1");
Eq(BlockType.KineticMill, s17Sim.World.Chunk.Get(10, 20, 22), "slice17 step3 composed direct-conduit mill survives tick 1");
Eq(CellPowerState.Energised, s17Sim.World.PowerState(11, 20, 10), "slice17 step3 composed single cable still Energised after tick 1");
Eq(CellPowerState.Energised, s17Sim.World.PowerState(11, 20, 14), "slice17 step3 composed two-cable run first cable still Energised after tick 1");
Eq(CellPowerState.Energised, s17Sim.World.PowerState(12, 20, 14), "slice17 step3 composed two-cable run second cable still Energised after tick 1");
Eq(CellPowerState.None, s17Sim.World.PowerState(11, 20, 18), "slice17 step3 composed mixed cable still None after tick 1");
var s17HudAfter1 = s17Sim.Hud();
Eq(1, s17HudAfter1.PoweredMills, "slice17 step3 composed one mill powered after tick 1");
Eq(2, s17HudAfter1.TotalMills, "slice17 step3 composed two mills total after tick 1");
Eq(0, s17HudAfter1.WarnedMills, "slice17 step3 composed no mill warned after tick 1");
Eq(2, s17HudAfter1.LiveNetworks, "slice17 step3 composed two live networks after tick 1: both destroyed mills' runs now feed nothing");
Eq(2, s17HudAfter1.LoneSources, "slice17 step3 composed still two lone sources after tick 1: the direct conduit survives its mill's neighbour tick");
for (int s17Tick = 0; s17Tick < 4; s17Tick++) s17Sim.World.Advance();
Eq(5, s17Sim.World.PoweredBlocksGathered, "slice17 step3 composed powered gathered 5 after 5 ticks");
Eq(1, s17Sim.World.KineticBlocksGathered, "slice17 step3 composed kinetic gathered 1 after 5 ticks");
Eq(2, s17Sim.World.DestroyedMills, "slice17 step3 composed destroyed stays 2 after 5 ticks");
Eq(6, s17Sim.World.Player.Inventory.Get(BlockType.Stone), "slice17 step3 composed stone 6 after 5 ticks");
var s17Save = s17Sim.SaveToJson();
var s17Step3Loaded = new Simulation(99);
Check(s17Step3Loaded.LoadFromJson(s17Save), "slice17 step3 composed save loads");
Eq(BlockType.Air, s17Step3Loaded.World.Chunk.Get(10, 20, 10), "slice17 step3 composed single-cable mill stays gone after load");
Eq(BlockType.Air, s17Step3Loaded.World.Chunk.Get(10, 20, 14), "slice17 step3 composed two-cable mill stays gone after load");
Eq(BlockType.KineticMill, s17Step3Loaded.World.Chunk.Get(10, 20, 22), "slice17 step3 composed direct-conduit mill loads");
Eq(BlockType.KineticMill, s17Step3Loaded.World.Chunk.Get(10, 20, 26), "slice17 step3 composed dynamo mill loads");
Eq(BlockType.Conduit, s17Step3Loaded.World.Chunk.Get(12, 20, 10), "slice17 step3 composed single-run conduit loads");
Eq(BlockType.Conduit, s17Step3Loaded.World.Chunk.Get(11, 20, 22), "slice17 step3 composed direct conduit loads");
Eq(BlockType.Conduit, s17Step3Loaded.World.Chunk.Get(10, 20, 30), "slice17 step3 composed lone conduit loads");
var s17Step3LoadedHud = s17Step3Loaded.Hud();
Eq(1, s17Step3LoadedHud.PoweredMills, "slice17 step3 composed one mill powered after load");
Eq(2, s17Step3LoadedHud.TotalMills, "slice17 step3 composed two mills total after load");
Eq(0, s17Step3LoadedHud.WarnedMills, "slice17 step3 composed no mill warned after load");
Eq(2, s17Step3LoadedHud.LiveNetworks, "slice17 step3 composed two live networks after load");
Eq(2, s17Step3LoadedHud.LoneSources, "slice17 step3 composed two lone sources after load: both readings re-derive");
Eq(CellPowerState.Energised, s17Step3Loaded.World.PowerState(11, 20, 10), "slice17 step3 composed single cable state Energised after load");
Eq(CellPowerState.Energised, s17Step3Loaded.World.PowerState(11, 20, 14), "slice17 step3 composed two-cable run first cable Energised after load");
Eq(CellPowerState.None, s17Step3Loaded.World.PowerState(11, 20, 18), "slice17 step3 composed mixed cable state None after load");
Eq(CellPowerState.None, s17Step3Loaded.World.PowerState(10, 20, 22), "slice17 step3 composed direct-conduit mill state None after load");
Eq(CellPowerState.Powered, s17Step3Loaded.World.PowerState(10, 20, 26), "slice17 step3 composed dynamo mill state Powered after load");
Eq(0, s17Step3Loaded.World.PoweredBlocksGathered, "slice17 step3 composed powered counter resets on load");
Eq(0, s17Step3Loaded.World.KineticBlocksGathered, "slice17 step3 composed kinetic counter resets on load");
Eq(0, s17Step3Loaded.World.DestroyedMills, "slice17 step3 composed destroyed counter resets on load");
Eq(6, s17Step3Loaded.World.Player.Inventory.Get(BlockType.Stone), "slice17 step3 composed stone stays 6 on load");
Eq(16, s17Step3Loaded.World.Player.X, "slice17 step3 composed player X after load");
Eq(16, s17Step3Loaded.World.Player.Z, "slice17 step3 composed player Z after load");
SeedPocketS17Test(s17Step3Loaded.World, 10, 26);
for (int s17Tick = 0; s17Tick < 5; s17Tick++) s17Step3Loaded.World.Advance();
Eq(5, s17Step3Loaded.World.PoweredBlocksGathered, "slice17 step3 composed loaded powered gathered 5 again");
Eq(1, s17Step3Loaded.World.KineticBlocksGathered, "slice17 step3 composed loaded kinetic gathered 1 again");
Eq(0, s17Step3Loaded.World.DestroyedMills, "slice17 step3 composed loaded destroyed stays 0");
Eq(12, s17Step3Loaded.World.Player.Inventory.Get(BlockType.Stone), "slice17 step3 composed loaded stone 12");

// ---- Slice 18 Step 1: patrol route limits, decided here ----
// Decision (rationale in DECISIONS.md): a refused direct
// step first tries a distance-reducing sidestep on a
// remaining gap axis, in MoveForPatrol only; only if
// every gap axis is refused does the step skip that
// waypoint for this circuit, as Slice 15 decided; and
// after a full circuit of consecutive refusals (5, the
// waypoint count) the patrol reads Stuck, not
// Patrolling. Rejected here, with reasons in
// DECISIONS.md: hold (freezes the whole loop on one
// block and still misreads while stuck), keep-skip
// unchanged (leaves the enclosed-patrol misread),
// and a pathfinder (a new system, not a sidestep).
// No price, cap, radius, waypoint count, fire rule,
// or engagement range moves in this step.
Eq(6, (int)SquadActivity.Stuck, "slice18 patrol stuck appended as 6");
Eq(3, (int)TacticalDoctrine.Patrol, "slice18 patrol doctrine value unchanged");
// Single-axis blocked, unchanged from Slice 15, plus
// the refusal counter: the east step is solid, there
// is no second gap axis, so the step skips (index 2,
// no move, 1 refusal, still Patrolling), and the next
// tick walks south and resets the counter.
var s18SingleWorld = new WorldZone(42);
s18SingleWorld.Player.X = 20; s18SingleWorld.Player.Z = 20;
var s18Single = s18SingleWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s18SingleWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice18 patrol single-axis test stages a solid step cell");
Check(!s18Single.MoveForPatrol(s18SingleWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol single-axis blocked step does not move");
Eq(2, s18Single.PatrolWaypointIndex, "slice18 patrol single-axis blocked step skips to south");
Eq(1, s18Single.PatrolConsecutiveRefusals, "slice18 patrol single-axis blocked step counts one refusal");
Check(!s18Single.IsPatrolStuck, "slice18 patrol one refusal is not stuck");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18SingleWorld)[0].Activity, "slice18 patrol one refusal still reads patrolling");
Check(s18Single.MoveForPatrol(s18SingleWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol after a single-axis skip walks south");
Eq(0, s18Single.PatrolConsecutiveRefusals, "slice18 patrol a move resets the refusal counter");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18SingleWorld)[0].Activity, "slice18 patrol after moving reads patrolling again");
// Diagonal sidestep, solid block: the squad stands
// diagonal to its rally waypoint, the primary west
// step is solid, the south step is free and reduces
// the Manhattan distance from 2 to 1, so it moves
// south, keeps the rally waypoint (index 0), and
// counts no refusal. The next tick steps west onto
// the rally itself, proving the waypoint is resumed,
// not skipped, after a sidestep.
var s18SideWorld = new WorldZone(42);
s18SideWorld.Player.X = 21; s18SideWorld.Player.Z = 21;
var s18Side = s18SideWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s18SideWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice18 patrol sidestep test stages a solid primary step cell");
Eq(2, Math.Abs(20 - s18Side.X) + Math.Abs(20 - s18Side.Z), "slice18 patrol sidestep distance before is 2");
Check(s18Side.MoveForPatrol(s18SideWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol diagonal blocked step sidesteps instead of skipping");
Eq(21, s18Side.X, "slice18 patrol sidestep keeps its X");
Eq(20, s18Side.Z, "slice18 patrol sidestep steps south onto the gap axis");
Eq(1, Math.Abs(20 - s18Side.X) + Math.Abs(20 - s18Side.Z), "slice18 patrol sidestep distance after is 1");
Eq(0, s18Side.PatrolWaypointIndex, "slice18 patrol sidestep keeps the same waypoint");
Eq(0, s18Side.PatrolConsecutiveRefusals, "slice18 patrol sidestep counts no refusal");
Check(s18Side.MoveForPatrol(s18SideWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol after a sidestep steps onto its waypoint");
Eq(20, s18Side.X, "slice18 patrol after a sidestep reaches the rally X");
Eq(0, s18Side.PatrolWaypointIndex, "slice18 patrol waypoint index after reaching the rally");
// Diagonal sidestep, all three kinds: movement is
// kind-independent in code, but each kind is proved
// to sidestep, not only Vanguard.
var s18SideReconWorld = new WorldZone(42);
s18SideReconWorld.Player.X = 21; s18SideReconWorld.Player.Z = 21;
var s18SideRecon = s18SideReconWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Recon)!;
Check(s18SideReconWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice18 patrol recon sidestep test stages a solid primary step cell");
Check(s18SideRecon.MoveForPatrol(s18SideReconWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol recon diagonal blocked step sidesteps");
Eq(20, s18SideRecon.Z, "slice18 patrol recon sidestep steps south");
var s18SideHeavyWorld = new WorldZone(42);
s18SideHeavyWorld.Player.X = 21; s18SideHeavyWorld.Player.Z = 21;
var s18SideHeavy = s18SideHeavyWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Heavy)!;
Check(s18SideHeavyWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice18 patrol heavy sidestep test stages a solid primary step cell");
Check(s18SideHeavy.MoveForPatrol(s18SideHeavyWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol heavy diagonal blocked step sidesteps");
Eq(20, s18SideHeavy.Z, "slice18 patrol heavy sidestep steps south");
// Diagonal, both gap axes blocked: no sidestep is
// available, so the step falls back to the Slice 15
// skip (index 1, no move, 1 refusal), not to a hold
// — and the next tick walks east toward the next
// waypoint, resetting the counter.
var s18BothWorld = new WorldZone(42);
s18BothWorld.Player.X = 21; s18BothWorld.Player.Z = 21;
var s18Both = s18BothWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s18BothWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice18 patrol both-axes test stages a solid primary step cell");
Check(s18BothWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice18 patrol both-axes test stages a solid sidestep cell");
Check(!s18Both.MoveForPatrol(s18BothWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol both gap axes blocked does not move");
Eq(1, s18Both.PatrolWaypointIndex, "slice18 patrol both gap axes blocked skips to east");
Eq(1, s18Both.PatrolConsecutiveRefusals, "slice18 patrol both gap axes blocked counts one refusal");
Check(s18Both.MoveForPatrol(s18BothWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol after a both-axes skip walks east");
Eq(22, s18Both.X, "slice18 patrol after a both-axes skip steps east");
Eq(0, s18Both.PatrolConsecutiveRefusals, "slice18 patrol a move after a both-axes skip resets the counter");
// Enemy-blocked step, single axis: the enemy blocks
// the east step, so the patrol skips to south — and
// when the enemy leaves before the next tick, the
// patrol still walks south, not east. Skip and hold
// differ observably here, and this is the skip.
var s18EnemyWorld = new WorldZone(42);
s18EnemyWorld.Player.X = 20; s18EnemyWorld.Player.Z = 20;
var s18Enemy = s18EnemyWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
var s18Blocker = new Enemy(21, 5, 20);
Check(!s18Enemy.MoveForPatrol(s18EnemyWorld.Chunk, new[] { s18Blocker }), "slice18 patrol enemy-blocked step does not move");
Eq(2, s18Enemy.PatrolWaypointIndex, "slice18 patrol enemy-blocked step skips to south");
Check(s18Enemy.MoveForPatrol(s18EnemyWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol after an enemy-blocked skip walks south even though the enemy left");
Eq(21, s18Enemy.Z, "slice18 patrol after an enemy-blocked skip steps south, not east");
// Enemy-blocked step, diagonal: the enemy blocks the
// primary west step, the sidestep south is free, so
// the patrol sidesteps; once the enemy is gone the
// next tick steps west onto the rally.
var s18EnemySideWorld = new WorldZone(42);
s18EnemySideWorld.Player.X = 21; s18EnemySideWorld.Player.Z = 21;
var s18EnemySide = s18EnemySideWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s18EnemySide.MoveForPatrol(s18EnemySideWorld.Chunk, new[] { new Enemy(20, 5, 21) }), "slice18 patrol enemy-blocked diagonal step sidesteps");
Eq(20, s18EnemySide.Z, "slice18 patrol enemy-blocked diagonal sidestep steps south");
Check(s18EnemySide.MoveForPatrol(s18EnemySideWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol after an enemy-blocked sidestep steps onto its waypoint");
Eq(20, s18EnemySide.X, "slice18 patrol after an enemy-blocked sidestep reaches the rally");
// Next-circuit retry, then resumption after the
// block clears: the east step stays solid, so the
// patrol walks its whole remaining loop, returns to
// the rally at index 0, and on the next circuit
// tries east again and skips again — the skip is
// per-circuit, not permanent. Clearing the solid
// cell and walking the loop again makes the same
// retry step east successfully.
var s18LoopWorld = new WorldZone(42);
s18LoopWorld.Player.X = 20; s18LoopWorld.Player.Z = 20;
var s18Loop = s18LoopWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s18LoopWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice18 patrol loop test stages a solid east step cell");
Check(!s18Loop.MoveForPatrol(s18LoopWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol loop first east step is refused");
Eq(2, s18Loop.PatrolWaypointIndex, "slice18 patrol loop first refusal skips to south");
int s18LoopCalls = 0;
while (s18LoopCalls < 40 && !(s18Loop.X == 20 && s18Loop.Z == 20 && s18Loop.PatrolWaypointIndex == 0))
{
    s18Loop.MoveForPatrol(s18LoopWorld.Chunk, Array.Empty<Enemy>());
    s18LoopCalls++;
}
Eq(20, s18Loop.X, "slice18 patrol loop returns to the rally X");
Eq(20, s18Loop.Z, "slice18 patrol loop returns to the rally Z");
Eq(0, s18Loop.PatrolWaypointIndex, "slice18 patrol loop returns to the rally at index 0");
Check(!s18Loop.MoveForPatrol(s18LoopWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol next-circuit east step is tried again and refused");
Eq(2, s18Loop.PatrolWaypointIndex, "slice18 patrol next-circuit east refusal skips to south again");
Check(s18LoopWorld.Chunk.TrySet(21, 5, 20, BlockType.Air), "slice18 patrol loop test clears the east step cell");
s18LoopCalls = 0;
while (s18LoopCalls < 40 && !(s18Loop.X == 20 && s18Loop.Z == 20 && s18Loop.PatrolWaypointIndex == 0))
{
    s18Loop.MoveForPatrol(s18LoopWorld.Chunk, Array.Empty<Enemy>());
    s18LoopCalls++;
}
Eq(0, s18Loop.PatrolWaypointIndex, "slice18 patrol cleared loop returns to the rally at index 0");
Check(s18Loop.MoveForPatrol(s18LoopWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol cleared east step moves east");
Eq(21, s18Loop.X, "slice18 patrol cleared east step reaches the step cell");
Eq(1, s18Loop.PatrolWaypointIndex, "slice18 patrol cleared east step keeps the east waypoint");
Eq(0, s18Loop.PatrolConsecutiveRefusals, "slice18 patrol cleared east step resets refusals");
// Fully enclosed patrol, threshold honesty: four
// consecutive refusals still read Patrolling, the
// fifth — a full circuit — reads Stuck, and further
// refusals keep it Stuck, with no movement and no
// crash over ten ticks.
var s18StuckWorld = new WorldZone(42);
s18StuckWorld.Player.X = 20; s18StuckWorld.Player.Z = 20;
var s18Stuck = s18StuckWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s18StuckWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice18 patrol stuck test stages an east wall");
Check(s18StuckWorld.Chunk.TrySet(19, 5, 20, BlockType.Stone), "slice18 patrol stuck test stages a west wall");
Check(s18StuckWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice18 patrol stuck test stages a south wall");
Check(s18StuckWorld.Chunk.TrySet(20, 5, 19, BlockType.Stone), "slice18 patrol stuck test stages a north wall");
for (int s18Tick = 0; s18Tick < 4; s18Tick++) s18Stuck.MoveForPatrol(s18StuckWorld.Chunk, Array.Empty<Enemy>());
Eq(4, s18Stuck.PatrolConsecutiveRefusals, "slice18 patrol four refusals counted");
Check(!s18Stuck.IsPatrolStuck, "slice18 patrol four refusals is not yet stuck");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18StuckWorld)[0].Activity, "slice18 patrol four refusals still reads patrolling");
Check(!s18Stuck.MoveForPatrol(s18StuckWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol fifth refusal does not move");
Eq(5, s18Stuck.PatrolConsecutiveRefusals, "slice18 patrol five refusals counted");
Check(s18Stuck.IsPatrolStuck, "slice18 patrol five refusals is stuck");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s18StuckWorld)[0].Activity, "slice18 patrol five refusals reads stuck");
Eq(20, s18Stuck.X, "slice18 patrol stuck squad holds its X");
Eq(20, s18Stuck.Z, "slice18 patrol stuck squad holds its Z");
for (int s18Tick = 0; s18Tick < 5; s18Tick++) s18Stuck.MoveForPatrol(s18StuckWorld.Chunk, Array.Empty<Enemy>());
Eq(20, s18Stuck.X, "slice18 patrol stuck squad still holds its X after ten ticks");
Eq(20, s18Stuck.Z, "slice18 patrol stuck squad still holds its Z after ten ticks");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s18StuckWorld)[0].Activity, "slice18 patrol still reads stuck after ten ticks");
// Stuck, then unblocked: removing one wall lets the
// patrol move again, resets the counter, and returns
// the reading to Patrolling — Stuck is a live
// reading, not a latch. After ten refusals the squad
// is on the west waypoint (index 3), so the west and
// north steps are still refused before the cleared
// east step is tried and moves.
Check(s18StuckWorld.Chunk.TrySet(21, 5, 20, BlockType.Air), "slice18 patrol unstuck test clears the east wall");
Check(!s18Stuck.MoveForPatrol(s18StuckWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol unstuck west step is still refused");
Check(!s18Stuck.MoveForPatrol(s18StuckWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol unstuck north step is still refused");
Check(s18Stuck.MoveForPatrol(s18StuckWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol unstuck east step moves");
Eq(21, s18Stuck.X, "slice18 patrol unstuck east step reaches the cleared cell");
Eq(0, s18Stuck.PatrolConsecutiveRefusals, "slice18 patrol unblocked step resets refusals");
Check(!s18Stuck.IsPatrolStuck, "slice18 patrol unblocked squad is not stuck");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18StuckWorld)[0].Activity, "slice18 patrol unblocked squad reads patrolling again");
// Save/load: a stuck patrol loads unstuck, at
// index 0, and becomes stuck again after a fresh
// full circuit of refusals in the loaded world —
// the counter is live progress, like the index.
var s18SaveWorld = new WorldZone(42);
s18SaveWorld.Player.X = 20; s18SaveWorld.Player.Z = 20;
var s18Save = s18SaveWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s18SaveWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice18 patrol save test stages an east wall");
Check(s18SaveWorld.Chunk.TrySet(19, 5, 20, BlockType.Stone), "slice18 patrol save test stages a west wall");
Check(s18SaveWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice18 patrol save test stages a south wall");
Check(s18SaveWorld.Chunk.TrySet(20, 5, 19, BlockType.Stone), "slice18 patrol save test stages a north wall");
for (int s18Tick = 0; s18Tick < 5; s18Tick++) s18Save.MoveForPatrol(s18SaveWorld.Chunk, Array.Empty<Enemy>());
Check(s18Save.IsPatrolStuck, "slice18 patrol save source is stuck before the save");
var s18SaveLoaded = SaveSystem.LoadZone(SaveSystem.SaveZone(s18SaveWorld));
Eq(1, s18SaveLoaded.Squads.Count, "slice18 patrol save/load keeps the stuck squad");
var s18Loaded = s18SaveLoaded.Squads[0];
Eq(20, s18Loaded.X, "slice18 patrol save/load position preserved");
Eq(0, s18Loaded.PatrolWaypointIndex, "slice18 patrol save/load waypoint index resets to 0");
Eq(0, s18Loaded.PatrolConsecutiveRefusals, "slice18 patrol save/load refusal counter resets to 0");
Check(!s18Loaded.IsPatrolStuck, "slice18 patrol save/load loaded squad is not stuck");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18SaveLoaded)[0].Activity, "slice18 patrol save/load loaded squad reads patrolling");
for (int s18Tick = 0; s18Tick < 5; s18Tick++) s18Loaded.MoveForPatrol(s18SaveLoaded.Chunk, Array.Empty<Enemy>());
Check(s18Loaded.IsPatrolStuck, "slice18 patrol save/load loaded squad is stuck again after a full circuit");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s18SaveLoaded)[0].Activity, "slice18 patrol save/load loaded squad reads stuck again");
// Fire while stuck, through WorldZone.Advance, all
// three kinds: a stuck patrol fires exactly as a
// moving patrol fires, and the stuck reading does
// not stop, start, or chase the fire. Recon (range
// 5, 12 damage) kills its 30-HP enemy over three
// ticks while the squad never moves; Vanguard
// (range 3, 30 damage) kills in one tick; Heavy
// (range 2, 16 damage) takes two ticks against an
// enemy held at range 2 by its own wall.
var s18FireReconWorld = new WorldZone(42);
s18FireReconWorld.Player.X = 20; s18FireReconWorld.Player.Z = 20;
var s18FireRecon = s18FireReconWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Recon)!;
Check(s18FireReconWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice18 patrol recon fire test stages an east wall");
Check(s18FireReconWorld.Chunk.TrySet(19, 5, 20, BlockType.Stone), "slice18 patrol recon fire test stages a west wall");
Check(s18FireReconWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice18 patrol recon fire test stages a south wall");
Check(s18FireReconWorld.Chunk.TrySet(20, 5, 19, BlockType.Stone), "slice18 patrol recon fire test stages a north wall");
s18FireReconWorld.Enemies.Add(new Enemy(20, 5, 25));
s18FireReconWorld.Advance();
Eq(18, s18FireReconWorld.Enemies[0].Health, "slice18 patrol recon stuck fires 12 on tick 1");
Eq(20, s18FireRecon.X, "slice18 patrol recon stuck squad does not move on tick 1");
s18FireReconWorld.Advance();
Eq(6, s18FireReconWorld.Enemies[0].Health, "slice18 patrol recon stuck fires 12 on tick 2");
s18FireReconWorld.Advance();
Eq(0, s18FireReconWorld.Enemies.Count, "slice18 patrol recon stuck kills the enemy on tick 3");
s18FireReconWorld.Advance();
s18FireReconWorld.Advance();
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s18FireReconWorld)[0].Activity, "slice18 patrol recon stuck reads stuck after the kill");
Eq(20, s18FireRecon.Z, "slice18 patrol recon stuck squad never moved");
var s18FireVangWorld = new WorldZone(42);
s18FireVangWorld.Player.X = 20; s18FireVangWorld.Player.Z = 20;
var s18FireVang = s18FireVangWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(s18FireVangWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice18 patrol vanguard fire test stages an east wall");
Check(s18FireVangWorld.Chunk.TrySet(19, 5, 20, BlockType.Stone), "slice18 patrol vanguard fire test stages a west wall");
Check(s18FireVangWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice18 patrol vanguard fire test stages a south wall");
Check(s18FireVangWorld.Chunk.TrySet(20, 5, 19, BlockType.Stone), "slice18 patrol vanguard fire test stages a north wall");
s18FireVangWorld.Enemies.Add(new Enemy(20, 5, 23));
s18FireVangWorld.Advance();
Eq(0, s18FireVangWorld.Enemies.Count, "slice18 patrol vanguard stuck kills the enemy on tick 1");
for (int s18Tick = 0; s18Tick < 4; s18Tick++) s18FireVangWorld.Advance();
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s18FireVangWorld)[0].Activity, "slice18 patrol vanguard stuck reads stuck");
Eq(20, s18FireVang.X, "slice18 patrol vanguard stuck squad never moved");
var s18FireHeavyWorld = new WorldZone(42);
s18FireHeavyWorld.Player.X = 20; s18FireHeavyWorld.Player.Z = 20;
var s18FireHeavy = s18FireHeavyWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Heavy)!;
Check(s18FireHeavyWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice18 patrol heavy fire test stages an east wall");
Check(s18FireHeavyWorld.Chunk.TrySet(19, 5, 20, BlockType.Stone), "slice18 patrol heavy fire test stages a west wall");
Check(s18FireHeavyWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice18 patrol heavy fire test stages a south wall");
Check(s18FireHeavyWorld.Chunk.TrySet(20, 5, 19, BlockType.Stone), "slice18 patrol heavy fire test stages a north wall");
s18FireHeavyWorld.Enemies.Add(new Enemy(20, 5, 22));
s18FireHeavyWorld.Advance();
Eq(14, s18FireHeavyWorld.Enemies[0].Health, "slice18 patrol heavy stuck fires 16 on tick 1");
s18FireHeavyWorld.Advance();
Eq(0, s18FireHeavyWorld.Enemies.Count, "slice18 patrol heavy stuck kills the enemy on tick 2");
for (int s18Tick = 0; s18Tick < 3; s18Tick++) s18FireHeavyWorld.Advance();
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s18FireHeavyWorld)[0].Activity, "slice18 patrol heavy stuck reads stuck");
Eq(20, s18FireHeavy.Z, "slice18 patrol heavy stuck squad never moved");
// Other doctrines unchanged — the sidestep lives in
// MoveForPatrol only, never in the shared rule.
// HoldGround diagonal to its rally, primary step
// solid, sidestep cell free: it does not sidestep.
var s18HoldWorld = new WorldZone(42);
s18HoldWorld.Player.X = 21; s18HoldWorld.Player.Z = 21;
var s18Hold = s18HoldWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.HoldGround)!;
Check(s18HoldWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice18 patrol contrast holdground test stages a solid primary step cell");
Check(!s18Hold.MoveTowardRally(s18HoldWorld.Chunk, Array.Empty<Enemy>()), "slice18 patrol contrast holdground diagonal blocked step does not sidestep");
Eq(21, s18Hold.X, "slice18 patrol contrast holdground holds its X");
Eq(21, s18Hold.Z, "slice18 patrol contrast holdground holds its Z");
// Advance diagonal to its prey, primary step solid,
// sidestep cell free: it does not sidestep either.
var s18AdvWorld = new WorldZone(42);
s18AdvWorld.Player.X = 21; s18AdvWorld.Player.Z = 21;
var s18Adv = s18AdvWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Advance)!;
Check(s18AdvWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice18 patrol contrast advance test stages a solid primary step cell");
Check(!s18Adv.MoveTowardNearestEnemy(s18AdvWorld.Chunk, new[] { new Enemy(20, 5, 20) }), "slice18 patrol contrast advance diagonal blocked step does not sidestep");
Eq(21, s18Adv.X, "slice18 patrol contrast advance holds its X");
Eq(21, s18Adv.Z, "slice18 patrol contrast advance holds its Z");
// Defend Core diagonal to its post, primary step
// solid, sidestep cell free: it does not sidestep.
var s18DefWorld = new WorldZone(42);
s18DefWorld.Player.X = 29; s18DefWorld.Player.Z = 29;
var s18Def = s18DefWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.DefendCore)!;
Check(s18DefWorld.Chunk.TrySet(28, 5, 29, BlockType.Stone), "slice18 patrol contrast defendcore test stages a solid primary step cell");
Check(!s18Def.MoveForDefendCore(s18DefWorld.Chunk, Array.Empty<Enemy>(), s18DefWorld.CoreX, s18DefWorld.CoreY, s18DefWorld.CoreZ), "slice18 patrol contrast defendcore diagonal blocked step does not sidestep");
Eq(29, s18Def.X, "slice18 patrol contrast defendcore holds its X");
Eq(29, s18Def.Z, "slice18 patrol contrast defendcore holds its Z");

// ---- Slice 18 Step 2: free deploy primitive enforcement (0.79.0) ----
// Decision, decided in this step (rationale in DECISIONS.md): the
// public free primitives are REMOVED and headless tests use the
// internal seam WorldZone.SpawnSquad (friend assembly only), so a
// play-path call fails to compile; the paid path is unchanged. No
// price, cap, doctrine, kind, or movement rule moves here, and no
// checklist item is taken: a compile error is not a scene
// behaviour to observe, so the next free gate number remains 38
// after Step 1's item 37. Note the corrected history: this step
// first tried Obsolete with error: true plus a test-file pragma,
// and the build proved a pragma cannot suppress that error, so the
// mechanism was changed to removal plus the internal seam rather
// than weakened to a warning (see DECISIONS.md).
// Paid-path matrix, all four doctrines x all three kinds: funded
// deploys succeed and charge exactly 10, unfunded deploys refuse
// with no squad and no charge. Each combination gets a fresh world,
// so no combination's squad or ore leaks into another's proof.
var s18Doctrines = new[] { TacticalDoctrine.HoldGround, TacticalDoctrine.Advance, TacticalDoctrine.DefendCore, TacticalDoctrine.Patrol };
var s18Kinds = new[] { SquadKind.Vanguard, SquadKind.Recon, SquadKind.Heavy };
foreach (var s18Doctrine in s18Doctrines)
foreach (var s18Kind in s18Kinds)
{
    var s18PaidWorld = new WorldZone(42);
    s18PaidWorld.Player.Inventory.Add(BlockType.EmberOre, 10);
    var s18Paid = s18PaidWorld.TryDeployPaidSquad(20, 5, 20, s18Doctrine, s18Kind);
    Check(s18Paid != null, $"slice18 deploy paid {s18Doctrine}/{s18Kind} funded succeeds");
    Eq(s18Doctrine, s18Paid!.Doctrine, $"slice18 deploy paid {s18Doctrine}/{s18Kind} keeps its doctrine");
    Eq(s18Kind, s18Paid.Kind, $"slice18 deploy paid {s18Doctrine}/{s18Kind} keeps its kind");
    Eq(0, s18PaidWorld.Player.Inventory.Get(BlockType.EmberOre), $"slice18 deploy paid {s18Doctrine}/{s18Kind} charges exactly 10 ore");
    var s18PoorWorld = new WorldZone(42);
    Check(s18PoorWorld.TryDeployPaidSquad(20, 5, 20, s18Doctrine, s18Kind) == null, $"slice18 deploy paid {s18Doctrine}/{s18Kind} unfunded refused");
    Eq(0, s18PoorWorld.Squads.Count, $"slice18 deploy paid {s18Doctrine}/{s18Kind} unfunded adds no squad");
    Eq(0, s18PoorWorld.Player.Inventory.Get(BlockType.EmberOre), $"slice18 deploy paid {s18Doctrine}/{s18Kind} unfunded charges nothing");
}
// Cap through the paid path for the two doctrines whose cap was not
// already proved per-doctrine (HoldGround cap rests on Slice 3/7/11
// tests, Patrol cap on Slice 15 tests): three field, the fourth is
// refused with ore untouched.
foreach (var s18CapDoctrine in new[] { TacticalDoctrine.Advance, TacticalDoctrine.DefendCore })
{
    var s18CapWorld = new WorldZone(42);
    s18CapWorld.Player.Inventory.Add(BlockType.EmberOre, 40);
    Check(s18CapWorld.TryDeployPaidSquad(16, 5, 16, s18CapDoctrine) != null, $"slice18 deploy paid {s18CapDoctrine} cap deploy 1 succeeds");
    Check(s18CapWorld.TryDeployPaidSquad(17, 5, 16, s18CapDoctrine) != null, $"slice18 deploy paid {s18CapDoctrine} cap deploy 2 succeeds");
    Check(s18CapWorld.TryDeployPaidSquad(18, 5, 16, s18CapDoctrine) != null, $"slice18 deploy paid {s18CapDoctrine} cap deploy 3 succeeds");
    Check(s18CapWorld.TryDeployPaidSquad(19, 5, 16, s18CapDoctrine) == null, $"slice18 deploy paid {s18CapDoctrine} 4th deploy refused at cap");
    Eq(3, s18CapWorld.Squads.Count, $"slice18 deploy paid {s18CapDoctrine} refused 4th adds no squad");
    Eq(10, s18CapWorld.Player.Inventory.Get(BlockType.EmberOre), $"slice18 deploy paid {s18CapDoctrine} refused 4th charges nothing");
}
// The mechanism itself, proved by reflection from inside the friend
// assembly (a test in this file cannot prove a compile error by
// calling a removed method — there is nothing to call): the public
// free entry points are gone, and the seam is internal, not public.
// A non-friend caller is proved to fail separately, outside this
// suite, by a throwaway project built against the core in this run
// (see DECISIONS.md): calling the old names fails as no-such-member
// and calling the seam fails to compile (observed CS1061 in the negative-compile proof). What this suite CAN prove
// is that the seam is internal and that the old names are absent.
Check(typeof(WorldZone).GetMethod("SpawnSquad") == null, "slice18 deploy seam spawn is not public on world");
Check(typeof(WorldZone).GetMethod("DeploySquad") == null, "slice18 deploy old world free name is gone");
Check(typeof(Simulation).GetMethod("DeployOrbitalSquad") == null, "slice18 deploy old simulation free name is gone");
Check(typeof(WorldZone).GetMethod("SpawnSquad", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null, "slice18 deploy seam spawn exists as an internal instance method");
// Paid-path patrol identity: a Patrol deployed through the picker
// keeps its doctrine and kind and pays, exactly as Step 1's patrol
// behaves — the deploy step does not change a doctrine's movement.
var s18PickPatrolSim = new Simulation(42);
s18PickPatrolSim.World.Player.Inventory.Add(BlockType.EmberOre, 10);
s18PickPatrolSim.BeginOrbitalPick();
Check(s18PickPatrolSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol), "slice18 deploy picker patrol doctrine selected");
Check(s18PickPatrolSim.OrbitalCommand.SelectKind(SquadKind.Recon), "slice18 deploy picker patrol kind selected");
Check(s18PickPatrolSim.SetOrbitalRally(20, 5, 20), "slice18 deploy picker patrol rally set");
var s18PickPatrol = s18PickPatrolSim.ConfirmOrbitalDeploy();
Check(s18PickPatrol != null, "slice18 deploy picker patrol confirm deploys funded");
Eq(TacticalDoctrine.Patrol, s18PickPatrol!.Doctrine, "slice18 deploy picker patrol keeps patrol doctrine");
Eq(SquadKind.Recon, s18PickPatrol.Kind, "slice18 deploy picker patrol keeps recon kind");
Eq(0, s18PickPatrolSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice18 deploy picker patrol charges 10 ore");
Check(!s18PickPatrolSim.OrbitalCommand.IsPicking, "slice18 deploy picker patrol closes after confirm");
// Paid-path patrol, Step 1 behaviour: the same fully enclosed setup
// as Step 1's stuck test, but deployed through the paid path — four
// refusals still read Patrolling, the fifth reads Stuck, unmoved.
var s18PaidStuckWorld = new WorldZone(42);
s18PaidStuckWorld.Player.X = 20; s18PaidStuckWorld.Player.Z = 20;
s18PaidStuckWorld.Player.Inventory.Add(BlockType.EmberOre, 10);
var s18PaidStuck = s18PaidStuckWorld.TryDeployPaidSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s18PaidStuck != null, "slice18 deploy paid patrol enclosed deploys funded");
Eq(0, s18PaidStuckWorld.Player.Inventory.Get(BlockType.EmberOre), "slice18 deploy paid patrol enclosed charges 10 ore");
Check(s18PaidStuckWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice18 deploy paid patrol enclosed test stages an east wall");
Check(s18PaidStuckWorld.Chunk.TrySet(19, 5, 20, BlockType.Stone), "slice18 deploy paid patrol enclosed test stages a west wall");
Check(s18PaidStuckWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice18 deploy paid patrol enclosed test stages a south wall");
Check(s18PaidStuckWorld.Chunk.TrySet(20, 5, 19, BlockType.Stone), "slice18 deploy paid patrol enclosed test stages a north wall");
for (int s18Tick = 0; s18Tick < 4; s18Tick++) s18PaidStuck.MoveForPatrol(s18PaidStuckWorld.Chunk, Array.Empty<Enemy>());
Eq(4, s18PaidStuck.PatrolConsecutiveRefusals, "slice18 deploy paid patrol four refusals counted");
Check(!s18PaidStuck.IsPatrolStuck, "slice18 deploy paid patrol four refusals is not yet stuck");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18PaidStuckWorld)[0].Activity, "slice18 deploy paid patrol four refusals still reads patrolling");
Check(!s18PaidStuck.MoveForPatrol(s18PaidStuckWorld.Chunk, Array.Empty<Enemy>()), "slice18 deploy paid patrol fifth refusal does not move");
Eq(5, s18PaidStuck.PatrolConsecutiveRefusals, "slice18 deploy paid patrol five refusals counted");
Check(s18PaidStuck.IsPatrolStuck, "slice18 deploy paid patrol five refusals is stuck");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s18PaidStuckWorld)[0].Activity, "slice18 deploy paid patrol five refusals reads stuck");
Eq(20, s18PaidStuck.X, "slice18 deploy paid patrol stuck squad holds its X");
Eq(20, s18PaidStuck.Z, "slice18 deploy paid patrol stuck squad holds its Z");
// Post-charge refund guard, stated honestly: after the paid path's
// checks pass, its spawn cannot fail through the public API (bounds
// and validity were already checked), so no black-box test can force
// the refund branch — the guard in TryDeployPaidSquad (refund in
// full if SpawnSquad ever returned null after charging) is a code
// reading, not a test claim, and this step does not pretend
// otherwise. What is testable — refused-before-charge charges
// nothing — is proved by the matrix and cap tests above.

// ---- Slice 18 Step 3: composed demo + review, closing Slice 18 (0.80.0) ----
// One Simulation composing both Slice 18 decisions through the
// paid path only, mirroring the demo scenario exactly: a Recon
// Patrol that sidesteps a diagonal march block, skips a
// single-axis loop block, and resets its refusal counter on
// moving again; then a Vanguard Patrol deployed later, after
// the first has marched away, whose rally is the player cell
// enclosed on all four sides after it deploys, so it cycles
// through five consecutive refusals and reads Stuck, unmoved,
// while the first patrol keeps walking; then a save/load
// round-trip under Step 1's load rule (index and counter
// reset to 0, so the enclosed patrol reads Patrolling on
// arrival and reaches Stuck again only after a fresh full
// circuit). No free deploy path is used here — Step 2
// removed it — and no enemy is staged, so no fire, chase,
// or damage claim is smuggled into a route composition.
var s18Step3Sim = new Simulation(42);
s18Step3Sim.World.Player.Inventory.Add(BlockType.EmberOre, 20);
Check(s18Step3Sim.World.Chunk.TrySet(18, 5, 17, BlockType.Stone), "slice18 step3 composed test stages the diagonal march block");
Check(s18Step3Sim.World.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice18 step3 composed test stages the loop east block");
s18Step3Sim.BeginOrbitalPick();
Check(s18Step3Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol), "slice18 step3 composed patrol doctrine selected");
Check(s18Step3Sim.OrbitalCommand.SelectKind(SquadKind.Recon), "slice18 step3 composed patrol kind selected");
Check(s18Step3Sim.SetOrbitalRally(20, 5, 20), "slice18 step3 composed patrol rally set");
var s18Step3Patrol = s18Step3Sim.ConfirmOrbitalDeploy();
Check(s18Step3Patrol != null, "slice18 step3 composed patrol deploys paid");
Eq(TacticalDoctrine.Patrol, s18Step3Patrol!.Doctrine, "slice18 step3 composed patrol keeps patrol doctrine");
Eq(SquadKind.Recon, s18Step3Patrol.Kind, "slice18 step3 composed patrol keeps recon kind");
Eq(10, s18Step3Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice18 step3 composed patrol charges 10 ore");
Eq(16, s18Step3Patrol.X, "slice18 step3 composed patrol deploys at the player X");
Eq(16, s18Step3Patrol.Z, "slice18 step3 composed patrol deploys at the player Z");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18Step3Sim.World)[0].Activity, "slice18 step3 composed patrol reads patrolling before any tick");
s18Step3Sim.World.Advance();
Eq(17, s18Step3Patrol.X, "slice18 step3 composed tick 1 patrol X");
Eq(16, s18Step3Patrol.Z, "slice18 step3 composed tick 1 patrol Z");
Eq(0, s18Step3Patrol.PatrolWaypointIndex, "slice18 step3 composed tick 1 patrol waypoint");
s18Step3Sim.World.Advance();
Eq(17, s18Step3Patrol.X, "slice18 step3 composed tick 2 patrol X");
Eq(17, s18Step3Patrol.Z, "slice18 step3 composed tick 2 patrol Z");
Eq(0, s18Step3Patrol.PatrolWaypointIndex, "slice18 step3 composed tick 2 patrol waypoint");
s18Step3Sim.World.Advance();
Eq(17, s18Step3Patrol.X, "slice18 step3 composed tick 3 sidestep patrol X");
Eq(18, s18Step3Patrol.Z, "slice18 step3 composed tick 3 sidestep patrol Z");
Eq(0, s18Step3Patrol.PatrolWaypointIndex, "slice18 step3 composed tick 3 sidestep keeps the waypoint");
Eq(0, s18Step3Patrol.PatrolConsecutiveRefusals, "slice18 step3 composed tick 3 sidestep counts no refusal");
Eq(5, Math.Abs(20 - s18Step3Patrol.X) + Math.Abs(20 - s18Step3Patrol.Z), "slice18 step3 composed tick 3 sidestep distance is 5, down from 6");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18Step3Sim.World)[0].Activity, "slice18 step3 composed tick 3 sidestep still reads patrolling");
s18Step3Sim.World.Advance();
Eq(18, s18Step3Patrol.X, "slice18 step3 composed tick 4 patrol X");
Eq(18, s18Step3Patrol.Z, "slice18 step3 composed tick 4 patrol Z");
s18Step3Sim.World.Advance();
Eq(19, s18Step3Patrol.X, "slice18 step3 composed tick 5 patrol X");
Eq(18, s18Step3Patrol.Z, "slice18 step3 composed tick 5 patrol Z");
s18Step3Sim.World.Advance();
Eq(19, s18Step3Patrol.X, "slice18 step3 composed tick 6 patrol X");
Eq(19, s18Step3Patrol.Z, "slice18 step3 composed tick 6 patrol Z");
s18Step3Sim.World.Advance();
Eq(20, s18Step3Patrol.X, "slice18 step3 composed tick 7 patrol X");
Eq(19, s18Step3Patrol.Z, "slice18 step3 composed tick 7 patrol Z");
s18Step3Sim.World.Advance();
Eq(20, s18Step3Patrol.X, "slice18 step3 composed tick 8 patrol reaches the rally X");
Eq(20, s18Step3Patrol.Z, "slice18 step3 composed tick 8 patrol reaches the rally Z");
Eq(0, s18Step3Patrol.PatrolWaypointIndex, "slice18 step3 composed tick 8 patrol waypoint at the rally");
s18Step3Sim.World.Advance();
Eq(20, s18Step3Patrol.X, "slice18 step3 composed tick 9 skipped east leg patrol X");
Eq(20, s18Step3Patrol.Z, "slice18 step3 composed tick 9 skipped east leg patrol Z");
Eq(2, s18Step3Patrol.PatrolWaypointIndex, "slice18 step3 composed tick 9 skipped east leg waypoint is south");
Eq(1, s18Step3Patrol.PatrolConsecutiveRefusals, "slice18 step3 composed tick 9 skipped east leg counts one refusal");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18Step3Sim.World)[0].Activity, "slice18 step3 composed tick 9 one refusal still reads patrolling");
s18Step3Sim.World.Advance();
Eq(20, s18Step3Patrol.X, "slice18 step3 composed tick 10 patrol X");
Eq(21, s18Step3Patrol.Z, "slice18 step3 composed tick 10 patrol walks south");
Eq(2, s18Step3Patrol.PatrolWaypointIndex, "slice18 step3 composed tick 10 patrol waypoint stays south");
Eq(0, s18Step3Patrol.PatrolConsecutiveRefusals, "slice18 step3 composed tick 10 a move resets the refusal counter");
// Second patrol, deployed only after the first has marched
// away, with its rally on the player cell — and the
// enclosure staged only after this deploy, because staging
// it first would trap the first patrol at its own spawn.
s18Step3Sim.BeginOrbitalPick();
Check(s18Step3Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol), "slice18 step3 composed enclosed doctrine selected");
Check(s18Step3Sim.OrbitalCommand.SelectKind(SquadKind.Vanguard), "slice18 step3 composed enclosed kind selected");
Check(s18Step3Sim.SetOrbitalRally(16, 5, 16), "slice18 step3 composed enclosed rally set");
var s18Step3Enclosed = s18Step3Sim.ConfirmOrbitalDeploy();
Check(s18Step3Enclosed != null, "slice18 step3 composed enclosed deploys paid");
Eq(TacticalDoctrine.Patrol, s18Step3Enclosed!.Doctrine, "slice18 step3 composed enclosed keeps patrol doctrine");
Eq(SquadKind.Vanguard, s18Step3Enclosed.Kind, "slice18 step3 composed enclosed keeps vanguard kind");
Eq(0, s18Step3Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice18 step3 composed enclosed charges the last 10 ore");
Eq(2, s18Step3Sim.World.Squads.Count, "slice18 step3 composed two paid patrols fielded");
Check(s18Step3Sim.World.Chunk.TrySet(17, 5, 16, BlockType.Stone), "slice18 step3 composed test stages the east enclosure wall");
Check(s18Step3Sim.World.Chunk.TrySet(15, 5, 16, BlockType.Stone), "slice18 step3 composed test stages the west enclosure wall");
Check(s18Step3Sim.World.Chunk.TrySet(16, 5, 17, BlockType.Stone), "slice18 step3 composed test stages the south enclosure wall");
Check(s18Step3Sim.World.Chunk.TrySet(16, 5, 15, BlockType.Stone), "slice18 step3 composed test stages the north enclosure wall");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18Step3Sim.World)[1].Activity, "slice18 step3 composed enclosed reads patrolling before its circuit");
s18Step3Sim.World.Advance();
Eq(20, s18Step3Patrol.X, "slice18 step3 composed enclosed tick 1 patrol X");
Eq(22, s18Step3Patrol.Z, "slice18 step3 composed enclosed tick 1 patrol Z");
Eq(16, s18Step3Enclosed.X, "slice18 step3 composed enclosed tick 1 enclosed X");
Eq(16, s18Step3Enclosed.Z, "slice18 step3 composed enclosed tick 1 enclosed Z");
Eq(2, s18Step3Enclosed.PatrolWaypointIndex, "slice18 step3 composed enclosed tick 1 enclosed waypoint is south");
Eq(1, s18Step3Enclosed.PatrolConsecutiveRefusals, "slice18 step3 composed enclosed tick 1 one refusal counted");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18Step3Sim.World)[1].Activity, "slice18 step3 composed enclosed tick 1 one refusal still reads patrolling");
s18Step3Sim.World.Advance();
Eq(20, s18Step3Patrol.X, "slice18 step3 composed enclosed tick 2 patrol X");
Eq(23, s18Step3Patrol.Z, "slice18 step3 composed enclosed tick 2 patrol reaches the south waypoint");
Eq(16, s18Step3Enclosed.X, "slice18 step3 composed enclosed tick 2 enclosed X");
Eq(16, s18Step3Enclosed.Z, "slice18 step3 composed enclosed tick 2 enclosed Z");
Eq(3, s18Step3Enclosed.PatrolWaypointIndex, "slice18 step3 composed enclosed tick 2 enclosed waypoint is west");
Eq(2, s18Step3Enclosed.PatrolConsecutiveRefusals, "slice18 step3 composed enclosed tick 2 two refusals counted");
s18Step3Sim.World.Advance();
Eq(19, s18Step3Patrol.X, "slice18 step3 composed enclosed tick 3 patrol turns west X");
Eq(23, s18Step3Patrol.Z, "slice18 step3 composed enclosed tick 3 patrol Z");
Eq(16, s18Step3Enclosed.X, "slice18 step3 composed enclosed tick 3 enclosed X");
Eq(16, s18Step3Enclosed.Z, "slice18 step3 composed enclosed tick 3 enclosed Z");
Eq(4, s18Step3Enclosed.PatrolWaypointIndex, "slice18 step3 composed enclosed tick 3 enclosed waypoint is north");
Eq(3, s18Step3Enclosed.PatrolConsecutiveRefusals, "slice18 step3 composed enclosed tick 3 three refusals counted");
s18Step3Sim.World.Advance();
Eq(19, s18Step3Patrol.X, "slice18 step3 composed enclosed tick 4 patrol X");
Eq(22, s18Step3Patrol.Z, "slice18 step3 composed enclosed tick 4 patrol steps north");
Eq(16, s18Step3Enclosed.X, "slice18 step3 composed enclosed tick 4 enclosed X");
Eq(16, s18Step3Enclosed.Z, "slice18 step3 composed enclosed tick 4 enclosed Z");
Eq(0, s18Step3Enclosed.PatrolWaypointIndex, "slice18 step3 composed enclosed tick 4 enclosed waypoint is the rally");
Eq(4, s18Step3Enclosed.PatrolConsecutiveRefusals, "slice18 step3 composed enclosed tick 4 four refusals counted");
Check(!s18Step3Enclosed.IsPatrolStuck, "slice18 step3 composed enclosed tick 4 four refusals is not yet stuck");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18Step3Sim.World)[1].Activity, "slice18 step3 composed enclosed tick 4 four refusals still reads patrolling");
s18Step3Sim.World.Advance();
Eq(18, s18Step3Patrol.X, "slice18 step3 composed enclosed tick 5 patrol steps west X");
Eq(22, s18Step3Patrol.Z, "slice18 step3 composed enclosed tick 5 patrol Z");
Eq(16, s18Step3Enclosed.X, "slice18 step3 composed enclosed tick 5 enclosed X");
Eq(16, s18Step3Enclosed.Z, "slice18 step3 composed enclosed tick 5 enclosed Z");
Eq(2, s18Step3Enclosed.PatrolWaypointIndex, "slice18 step3 composed enclosed tick 5 enclosed waypoint is south again");
Eq(5, s18Step3Enclosed.PatrolConsecutiveRefusals, "slice18 step3 composed enclosed tick 5 five refusals counted");
Check(s18Step3Enclosed.IsPatrolStuck, "slice18 step3 composed enclosed tick 5 five refusals is stuck");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s18Step3Sim.World)[1].Activity, "slice18 step3 composed enclosed tick 5 five refusals reads stuck");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18Step3Sim.World)[0].Activity, "slice18 step3 composed enclosed tick 5 walking patrol still reads patrolling beside it");
// Save/load round-trip under Step 1's load rule: both
// indices and both counters reset, so the enclosed patrol
// reads Patrolling on arrival and reaches Stuck again only
// after a fresh full circuit, while the walking patrol
// marches to its rally first and re-proves the blocked
// east leg once it stands on its rally.
var s18Step3Save = s18Step3Sim.SaveToJson();
Eq(44015, s18Step3Save.Length, "slice18 step3 composed save JSON length matches the demo run");
var s18Step3Loaded = new Simulation(99);
Check(s18Step3Loaded.LoadFromJson(s18Step3Save), "slice18 step3 composed save loads");
Eq(2, s18Step3Loaded.World.Squads.Count, "slice18 step3 composed loaded squads count");
var s18Step3LoadedPatrol = s18Step3Loaded.World.Squads[0];
var s18Step3LoadedEnclosed = s18Step3Loaded.World.Squads[1];
Eq(18, s18Step3LoadedPatrol.X, "slice18 step3 composed loaded patrol X");
Eq(22, s18Step3LoadedPatrol.Z, "slice18 step3 composed loaded patrol Z");
Eq(0, s18Step3LoadedPatrol.PatrolWaypointIndex, "slice18 step3 composed loaded patrol waypoint resets to 0");
Eq(0, s18Step3LoadedPatrol.PatrolConsecutiveRefusals, "slice18 step3 composed loaded patrol counter resets to 0");
Eq(16, s18Step3LoadedEnclosed.X, "slice18 step3 composed loaded enclosed X");
Eq(16, s18Step3LoadedEnclosed.Z, "slice18 step3 composed loaded enclosed Z");
Eq(0, s18Step3LoadedEnclosed.PatrolWaypointIndex, "slice18 step3 composed loaded enclosed waypoint resets to 0");
Eq(0, s18Step3LoadedEnclosed.PatrolConsecutiveRefusals, "slice18 step3 composed loaded enclosed counter resets to 0");
Check(!s18Step3LoadedEnclosed.IsPatrolStuck, "slice18 step3 composed loaded enclosed is not stuck on arrival");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18Step3Loaded.World)[1].Activity, "slice18 step3 composed loaded enclosed reads patrolling on arrival");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18Step3Loaded.World)[0].Activity, "slice18 step3 composed loaded patrol reads patrolling on arrival");
s18Step3Loaded.World.Advance();
Eq(19, s18Step3LoadedPatrol.X, "slice18 step3 composed loaded tick 1 patrol steps east X");
Eq(22, s18Step3LoadedPatrol.Z, "slice18 step3 composed loaded tick 1 patrol Z");
Eq(2, s18Step3LoadedEnclosed.PatrolWaypointIndex, "slice18 step3 composed loaded tick 1 enclosed waypoint is south");
Eq(1, s18Step3LoadedEnclosed.PatrolConsecutiveRefusals, "slice18 step3 composed loaded tick 1 enclosed one refusal counted");
s18Step3Loaded.World.Advance();
Eq(19, s18Step3LoadedPatrol.X, "slice18 step3 composed loaded tick 2 patrol X");
Eq(21, s18Step3LoadedPatrol.Z, "slice18 step3 composed loaded tick 2 patrol steps north");
Eq(3, s18Step3LoadedEnclosed.PatrolWaypointIndex, "slice18 step3 composed loaded tick 2 enclosed waypoint is west");
Eq(2, s18Step3LoadedEnclosed.PatrolConsecutiveRefusals, "slice18 step3 composed loaded tick 2 enclosed two refusals counted");
s18Step3Loaded.World.Advance();
Eq(20, s18Step3LoadedPatrol.X, "slice18 step3 composed loaded tick 3 patrol steps east X");
Eq(21, s18Step3LoadedPatrol.Z, "slice18 step3 composed loaded tick 3 patrol Z");
Eq(4, s18Step3LoadedEnclosed.PatrolWaypointIndex, "slice18 step3 composed loaded tick 3 enclosed waypoint is north");
Eq(3, s18Step3LoadedEnclosed.PatrolConsecutiveRefusals, "slice18 step3 composed loaded tick 3 enclosed three refusals counted");
s18Step3Loaded.World.Advance();
Eq(20, s18Step3LoadedPatrol.X, "slice18 step3 composed loaded tick 4 patrol reaches the rally X");
Eq(20, s18Step3LoadedPatrol.Z, "slice18 step3 composed loaded tick 4 patrol reaches the rally Z");
Eq(0, s18Step3LoadedEnclosed.PatrolWaypointIndex, "slice18 step3 composed loaded tick 4 enclosed waypoint is the rally");
Eq(4, s18Step3LoadedEnclosed.PatrolConsecutiveRefusals, "slice18 step3 composed loaded tick 4 enclosed four refusals counted");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s18Step3Loaded.World)[1].Activity, "slice18 step3 composed loaded tick 4 enclosed four refusals still reads patrolling");
s18Step3Loaded.World.Advance();
Eq(20, s18Step3LoadedPatrol.X, "slice18 step3 composed loaded tick 5 patrol X");
Eq(20, s18Step3LoadedPatrol.Z, "slice18 step3 composed loaded tick 5 patrol Z");
Eq(2, s18Step3LoadedPatrol.PatrolWaypointIndex, "slice18 step3 composed loaded tick 5 patrol skips the blocked east leg again");
Eq(1, s18Step3LoadedPatrol.PatrolConsecutiveRefusals, "slice18 step3 composed loaded tick 5 patrol counts one refusal again");
Eq(16, s18Step3LoadedEnclosed.X, "slice18 step3 composed loaded tick 5 enclosed X");
Eq(16, s18Step3LoadedEnclosed.Z, "slice18 step3 composed loaded tick 5 enclosed Z");
Eq(2, s18Step3LoadedEnclosed.PatrolWaypointIndex, "slice18 step3 composed loaded tick 5 enclosed waypoint is south again");
Eq(5, s18Step3LoadedEnclosed.PatrolConsecutiveRefusals, "slice18 step3 composed loaded tick 5 enclosed five refusals counted again");
Check(s18Step3LoadedEnclosed.IsPatrolStuck, "slice18 step3 composed loaded tick 5 enclosed is stuck again");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s18Step3Loaded.World)[1].Activity, "slice18 step3 composed loaded tick 5 enclosed reads stuck again");

// ---- Slice 19 Step 1: transformer-adjacency count query ----
// Decision, decided in this step (rationale in DECISIONS.md):
// a Conduit beside a dead standalone transformer counts as
// lone, folded into the existing POWER line's lone number —
// no new line, no new word. A Conduit is absent from
// TransformerOutput's direct branch, so a transformer is not
// wiring for a Conduit the way it is for a Dynamo or
// Alternator; the exception holds only while the group is
// dead (every cell outputs None) and standalone (no cable
// neighbour). The four hypotheses, weighed: keep unchanged
// (rejected — the motivating case then stays invisible on
// every count, and the player's placement went nowhere with
// no readout anywhere); count as lone (adopted, in the
// branch-limited form above); a distinct readout (rejected
// — HUD discipline, art-direction rule 4, and the lone
// number already means a placed source delivering nothing
// since Slice 17 Step 2, so a third number would split one
// question across two numbers); redefine live to include
// dead wiring (rejected — live has meant carrying a
// non-None voltage since Slice 12, and a dead group counted
// as live would make the live number lie about the very
// thing it exists to report). No delivery rule moves: a
// Conduit beside a transformer still delivers nothing
// directly, a transformer beside a Conduit alone still
// outputs None, and no consumer predicate moves.
// Motivating case: a Conduit beside a transformer alone.
var s19Tx = new WorldZone(42);
s19Tx.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
s19Tx.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
Eq((0, 1), s19Tx.LivePowerCounts(), "slice19 step1 conduit beside a transformer alone counts 0 live networks, 1 lone source");
Eq(0, HudState.FromWorld(s19Tx, new Weapon()).LiveNetworks, "slice19 step1 hud carries no live network for the dead transformer case");
Eq(1, HudState.FromWorld(s19Tx, new Weapon()).LoneSources, "slice19 step1 hud carries the dead-transformer conduit as one lone source");
// Dead group feeding a consumer: the consumer's presence does
// not block the lone count — the group's deadness decides,
// and the mill's own counts (total, unpowered) do not
// double-count the source.
var s19TxMill = new WorldZone(42);
s19TxMill.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
s19TxMill.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
s19TxMill.Chunk.TrySet(17, 20, 15, BlockType.KineticMill);
Eq((0, 1), s19TxMill.LivePowerCounts(), "slice19 step1 conduit beside a dead transformer beside a mill counts 0 live networks, 1 lone source");
Eq(Voltage.None, s19TxMill.DeliveredVoltage(17, 20, 15), "slice19 step1 dead transformer mill delivery is still None");
Check(!s19TxMill.IsPowered(17, 20, 15), "slice19 step1 dead transformer mill is not powered");
Check(!s19TxMill.IsMismatched(17, 20, 15), "slice19 step1 dead transformer mill is not mismatched");
Eq(1, HudState.FromWorld(s19TxMill, new Weapon()).TotalMills, "slice19 step1 dead transformer mill still totals one mill");
Eq(0, HudState.FromWorld(s19TxMill, new Weapon()).PoweredMills, "slice19 step1 dead transformer mill is not powered in the consumer count");
Eq(0, HudState.FromWorld(s19TxMill, new Weapon()).WarnedMills, "slice19 step1 dead transformer mill is not warned in the consumer count");
// No double count: a Conduit beside a live transformer gains
// no lone count, whether the group feeds a consumer or not.
var s19TxLive = new WorldZone(42);
s19TxLive.Chunk.TrySet(16, 20, 16, BlockType.Dynamo);
s19TxLive.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
s19TxLive.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
Eq((1, 0), s19TxLive.LivePowerCounts(), "slice19 step1 conduit beside a live transformer counts 1 live network, 0 lone sources, no double count");
Eq(1, HudState.FromWorld(s19TxLive, new Weapon()).LiveNetworks, "slice19 step1 hud carries the live transformer group as one live network");
var s19TxLiveFed = new WorldZone(42);
s19TxLiveFed.Chunk.TrySet(16, 20, 16, BlockType.Dynamo);
s19TxLiveFed.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
s19TxLiveFed.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
s19TxLiveFed.Chunk.TrySet(17, 20, 15, BlockType.KineticMill);
Eq((0, 0), s19TxLiveFed.LivePowerCounts(), "slice19 step1 conduit beside a live transformer feeding a mill counts neither live nor lone");
Eq(Voltage.HV, s19TxLiveFed.DeliveredVoltage(17, 20, 15), "slice19 step1 dynamo-fed transformer still delivers stepped-up HV to its mill");
Check(s19TxLiveFed.IsMismatched(17, 20, 15), "slice19 step1 dynamo-fed transformer mill is still mismatched, delivery unchanged");
// Live controls: a Dynamo or Alternator beside a transformer
// alone still reads live, and neither becomes lone.
var s19DynTx = new WorldZone(42);
s19DynTx.Chunk.TrySet(16, 20, 16, BlockType.Dynamo);
s19DynTx.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
Eq((1, 0), s19DynTx.LivePowerCounts(), "slice19 step1 dynamo beside a transformer alone counts 1 live network, 0 lone sources");
var s19AltTx = new WorldZone(42);
s19AltTx.Chunk.TrySet(16, 20, 16, BlockType.Alternator);
s19AltTx.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
Eq((1, 0), s19AltTx.LivePowerCounts(), "slice19 step1 alternator beside a transformer alone counts 1 live network, 0 lone sources");
// Per source cell, not per transformer or per pair.
var s19TwoCondOneTx = new WorldZone(42);
s19TwoCondOneTx.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
s19TwoCondOneTx.Chunk.TrySet(16, 20, 16, BlockType.Conduit);
s19TwoCondOneTx.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
Eq((0, 2), s19TwoCondOneTx.LivePowerCounts(), "slice19 step1 two conduits beside one dead transformer count 2 lone sources");
var s19OneCondTwoTx = new WorldZone(42);
s19OneCondTwoTx.Chunk.TrySet(16, 20, 16, BlockType.Transformer);
s19OneCondTwoTx.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
s19OneCondTwoTx.Chunk.TrySet(18, 20, 16, BlockType.Transformer);
Eq((0, 1), s19OneCondTwoTx.LivePowerCounts(), "slice19 step1 one conduit beside two dead transformers counts 1 lone source, not one per pair");
// Chain and ring with no source: the group is counted once
// in the sense that matters here — one idle source beside it
// counts once, however many dead cells the group has.
var s19Chain = new WorldZone(42);
s19Chain.Chunk.TrySet(16, 20, 16, BlockType.Transformer);
s19Chain.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
s19Chain.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
Eq((0, 1), s19Chain.LivePowerCounts(), "slice19 step1 conduit beside a dead transformer chain counts 1 lone source");
var s19Ring = new WorldZone(42);
s19Ring.Chunk.TrySet(16, 20, 16, BlockType.Transformer);
s19Ring.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
s19Ring.Chunk.TrySet(17, 20, 17, BlockType.Transformer);
s19Ring.Chunk.TrySet(16, 20, 17, BlockType.Transformer);
s19Ring.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
Eq((0, 1), s19Ring.LivePowerCounts(), "slice19 step1 conduit beside a sourceless transformer ring counts 1 lone source");
// Conflicted transformer: Dynamo plus Alternator, output
// None, two placed sources at 0 / 0 — the feeding kinds do
// not become lone, so the conflict is not hidden behind a
// lone number. A Conduit beside that same conflicted group
// does count, because it feeds nothing into it.
var s19Conflict = new WorldZone(42);
s19Conflict.Chunk.TrySet(16, 20, 16, BlockType.Dynamo);
s19Conflict.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
s19Conflict.Chunk.TrySet(18, 20, 16, BlockType.Alternator);
s19Conflict.Chunk.TrySet(17, 20, 15, BlockType.KineticMill);
Eq((0, 0), s19Conflict.LivePowerCounts(), "slice19 step1 dynamo plus alternator beside one conflicted transformer count neither live nor lone");
Eq(Voltage.None, s19Conflict.DeliveredVoltage(17, 20, 15), "slice19 step1 conflicted transformer mill delivery is still None");
Check(!s19Conflict.IsPowered(17, 20, 15), "slice19 step1 conflicted transformer mill is not powered");
Check(!s19Conflict.IsMismatched(17, 20, 15), "slice19 step1 conflicted transformer mill is not mismatched, None is neither");
var s19ConflictCond = new WorldZone(42);
s19ConflictCond.Chunk.TrySet(16, 20, 16, BlockType.Dynamo);
s19ConflictCond.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
s19ConflictCond.Chunk.TrySet(18, 20, 16, BlockType.Alternator);
s19ConflictCond.Chunk.TrySet(17, 20, 15, BlockType.Conduit);
Eq((0, 1), s19ConflictCond.LivePowerCounts(), "slice19 step1 conduit beside a conflicted transformer counts 1 lone source, its feeding neighbours count neither");
// Which neighbour wins: a Conduit beside both a mill and a
// dead transformer counts lone under both rules at once —
// neither neighbour disqualifies, because the mill alone
// already counted it lone in Slice 17 Step 2 and the dead
// transformer adds nothing delivering.
var s19MillDeadTx = new WorldZone(42);
s19MillDeadTx.Chunk.TrySet(16, 20, 16, BlockType.KineticMill);
s19MillDeadTx.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
s19MillDeadTx.Chunk.TrySet(18, 20, 16, BlockType.Transformer);
Eq((0, 1), s19MillDeadTx.LivePowerCounts(), "slice19 step1 conduit beside both a mill and a dead transformer counts 1 lone source");
Eq(Voltage.None, s19MillDeadTx.DeliveredVoltage(16, 20, 16), "slice19 step1 mill beside a dead-transformer conduit still receives None");
Check(!s19MillDeadTx.IsPowered(16, 20, 16), "slice19 step1 mill beside a dead-transformer conduit is not powered");
Check(!s19MillDeadTx.IsMismatched(16, 20, 16), "slice19 step1 mill beside a dead-transformer conduit is not mismatched");
// Only dead counts: a Conduit beside one dead and one live
// transformer is not lone — the live wiring disqualifies —
// and a cable neighbour disqualifies even beside a dead
// transformer, because a Conduit feeds a cable directly.
var s19MixedTx = new WorldZone(42);
s19MixedTx.Chunk.TrySet(16, 20, 16, BlockType.Transformer);
s19MixedTx.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
s19MixedTx.Chunk.TrySet(18, 20, 16, BlockType.Transformer);
s19MixedTx.Chunk.TrySet(19, 20, 16, BlockType.Dynamo);
Eq((1, 0), s19MixedTx.LivePowerCounts(), "slice19 step1 conduit beside one dead and one live transformer counts 1 live network, 0 lone sources");
var s19CableWins = new WorldZone(42);
s19CableWins.Chunk.TrySet(16, 20, 16, BlockType.Transformer);
s19CableWins.Chunk.TrySet(17, 20, 16, BlockType.Conduit);
s19CableWins.Chunk.TrySet(17, 20, 17, BlockType.Cable);
Eq((1, 0), s19CableWins.LivePowerCounts(), "slice19 step1 conduit beside a dead transformer and touching a cable counts 1 live network, 0 lone sources");
// Cable story: a transformer beside a cable belongs to that
// component, not to a standalone group — for a live cable
// and for a dead, mixed-voltage cable alike, the Conduit
// beside it stays cable-story, not lone.
var s19CableStoryLive = new WorldZone(42);
s19CableStoryLive.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
s19CableStoryLive.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
s19CableStoryLive.Chunk.TrySet(17, 20, 17, BlockType.Cable);
s19CableStoryLive.Chunk.TrySet(16, 20, 17, BlockType.Dynamo);
Eq((1, 0), s19CableStoryLive.LivePowerCounts(), "slice19 step1 conduit beside a transformer beside a live cable counts 1 live network, 0 lone sources");
var s19CableStoryDead = new WorldZone(42);
s19CableStoryDead.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
s19CableStoryDead.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
s19CableStoryDead.Chunk.TrySet(17, 20, 17, BlockType.Cable);
s19CableStoryDead.Chunk.TrySet(16, 20, 17, BlockType.Dynamo);
s19CableStoryDead.Chunk.TrySet(17, 20, 18, BlockType.Alternator);
Eq((0, 0), s19CableStoryDead.LivePowerCounts(), "slice19 step1 conduit beside a transformer beside a dead mixed-voltage cable counts neither live nor lone");
// Dead-cable consistency, met in the open: a Dynamo and a
// Conduit touching one mixed-voltage component read 0 / 0
// today, and stay 0 / 0 — the distinction is the branch, not
// a preference. A Conduit is a full cable source (HV into
// the component), so cable adjacency is genuine wiring for
// it, while transformer adjacency is not; a rule that
// counted cable-adjacent dead sources as lone would count
// sources that feed the very component that died. A cable
// component fed only by a dead transformer is a cable case,
// not a transformer case: the component's voltage is a cable
// computation, its transformer is cable-story, and it reads
// 0 / 0 with no source to count.
var s19DeadCable = new WorldZone(42);
s19DeadCable.Chunk.TrySet(17, 20, 16, BlockType.Cable);
s19DeadCable.Chunk.TrySet(16, 20, 16, BlockType.Dynamo);
s19DeadCable.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
Eq((0, 0), s19DeadCable.LivePowerCounts(), "slice19 step1 dynamo plus conduit beside one dead mixed-voltage cable count neither live nor lone");
Eq(0, HudState.FromWorld(s19DeadCable, new Weapon()).LoneSources, "slice19 step1 hud carries no lone source for the dead cable case");
var s19DeadTxCable = new WorldZone(42);
s19DeadTxCable.Chunk.TrySet(17, 20, 16, BlockType.Cable);
s19DeadTxCable.Chunk.TrySet(16, 20, 16, BlockType.Transformer);
Eq((0, 0), s19DeadTxCable.LivePowerCounts(), "slice19 step1 cable fed only by a dead transformer counts neither live nor lone, a cable case");
// Control: a lone Conduit with no neighbour at all still
// counts, exactly as before.
var s19Lone = new WorldZone(42);
s19Lone.Chunk.TrySet(16, 20, 16, BlockType.Conduit);
Eq((0, 1), s19Lone.LivePowerCounts(), "slice19 step1 lone conduit with no neighbour still counts 1 lone source");
// Save/load re-derivation: the decided count is derived,
// never saved, so a loaded dead-transformer world reads it
// again.
var s19Sim = new Simulation(42);
s19Sim.World.Chunk.TrySet(17, 20, 16, BlockType.Transformer);
s19Sim.World.Chunk.TrySet(18, 20, 16, BlockType.Conduit);
Eq((0, 1), s19Sim.World.LivePowerCounts(), "slice19 step1 dead transformer conduit counts 1 lone source before save");
var s19Save = s19Sim.SaveToJson();
var s19Loaded = new Simulation(99);
Check(s19Loaded.LoadFromJson(s19Save), "slice19 step1 dead transformer world loads");
Eq((0, 1), s19Loaded.World.LivePowerCounts(), "slice19 step1 dead transformer conduit counts 1 lone source after load");
Eq(1, s19Loaded.Hud().LoneSources, "slice19 step1 hud carries the dead transformer conduit lone count after load");
Eq(0, s19Loaded.Hud().LiveNetworks, "slice19 step1 hud carries no live network for the dead transformer case after load");

// ---- Slice 19 Step 2: composed demo + review, closing Slice 19 (0.83.0) ----
// One Simulation composing the Step 1 count with its
// controls, mirroring the demo scenario exactly: a Conduit
// beside a transformer alone (the decided case), a Dynamo
// and an Alternator each beside a transformer alone (the
// live controls), a lone Conduit (the lone control), a mill
// beside a Conduit (the Slice 17 Step 2 control), and a
// Dynamo-plus-Conduit mixed-voltage cable component (the
// dead-cable consistency case, reading differently from the
// decided case by the branch distinction Step 1 recorded).
// Rows are four cells apart in z, so no row can leak
// voltage into another, and every aggregate below is a
// total over all six rows — the composed claim is exactly
// that the decided reading agrees with the live, lone, and
// dead-cable controls in one wiring, stays stable across a
// tick (no mill here is mismatched, so nothing is
// destroyed and nothing starts feeding), and re-derives on
// load. No new core system, no adapter edit, no checklist
// item: the review exercises no behaviour item 38 does not
// already cover.
var s19Step3Sim = new Simulation(42);
Check(s19Step3Sim.World.Chunk.TrySet(11, 20, 10, BlockType.Transformer), "slice19 step3 composed test stages the step1 transformer");
Check(s19Step3Sim.World.Chunk.TrySet(12, 20, 10, BlockType.Conduit), "slice19 step3 composed test stages the step1 conduit");
Check(s19Step3Sim.World.Chunk.TrySet(10, 20, 14, BlockType.Dynamo), "slice19 step3 composed test stages the dynamo live control source");
Check(s19Step3Sim.World.Chunk.TrySet(11, 20, 14, BlockType.Transformer), "slice19 step3 composed test stages the dynamo live control transformer");
Check(s19Step3Sim.World.Chunk.TrySet(10, 20, 18, BlockType.Alternator), "slice19 step3 composed test stages the alternator live control source");
Check(s19Step3Sim.World.Chunk.TrySet(11, 20, 18, BlockType.Transformer), "slice19 step3 composed test stages the alternator live control transformer");
Check(s19Step3Sim.World.Chunk.TrySet(10, 20, 22, BlockType.Conduit), "slice19 step3 composed test stages the lone conduit control");
Check(s19Step3Sim.World.Chunk.TrySet(10, 20, 26, BlockType.KineticMill), "slice19 step3 composed test stages the step2 control mill");
Check(s19Step3Sim.World.Chunk.TrySet(11, 20, 26, BlockType.Conduit), "slice19 step3 composed test stages the step2 control conduit");
Check(s19Step3Sim.World.Chunk.TrySet(10, 20, 30, BlockType.Dynamo), "slice19 step3 composed test stages the dead cable dynamo");
Check(s19Step3Sim.World.Chunk.TrySet(11, 20, 30, BlockType.Cable), "slice19 step3 composed test stages the dead cable cell");
Check(s19Step3Sim.World.Chunk.TrySet(12, 20, 30, BlockType.Conduit), "slice19 step3 composed test stages the dead cable conduit");
Eq(BlockType.Transformer, s19Step3Sim.World.Chunk.Get(11, 20, 10), "slice19 step3 composed step1 transformer cell");
Eq(BlockType.Conduit, s19Step3Sim.World.Chunk.Get(12, 20, 10), "slice19 step3 composed step1 conduit cell");
Eq(BlockType.Dynamo, s19Step3Sim.World.Chunk.Get(10, 20, 14), "slice19 step3 composed dynamo live control cell");
Eq(BlockType.Transformer, s19Step3Sim.World.Chunk.Get(11, 20, 14), "slice19 step3 composed dynamo transformer cell");
Eq(BlockType.Alternator, s19Step3Sim.World.Chunk.Get(10, 20, 18), "slice19 step3 composed alternator live control cell");
Eq(BlockType.Transformer, s19Step3Sim.World.Chunk.Get(11, 20, 18), "slice19 step3 composed alternator transformer cell");
Eq(BlockType.Conduit, s19Step3Sim.World.Chunk.Get(10, 20, 22), "slice19 step3 composed lone conduit cell");
Eq(BlockType.KineticMill, s19Step3Sim.World.Chunk.Get(10, 20, 26), "slice19 step3 composed step2 mill cell");
Eq(BlockType.Conduit, s19Step3Sim.World.Chunk.Get(11, 20, 26), "slice19 step3 composed step2 conduit cell");
Eq(BlockType.Dynamo, s19Step3Sim.World.Chunk.Get(10, 20, 30), "slice19 step3 composed dead cable dynamo cell");
Eq(BlockType.Cable, s19Step3Sim.World.Chunk.Get(11, 20, 30), "slice19 step3 composed dead cable cell");
Eq(BlockType.Conduit, s19Step3Sim.World.Chunk.Get(12, 20, 30), "slice19 step3 composed dead cable conduit cell");
Eq((2, 3), s19Step3Sim.World.LivePowerCounts(), "slice19 step3 composed counts before the tick are 2 live, 3 lone");
var s19Step3HudBefore = s19Step3Sim.Hud();
Eq(0, s19Step3HudBefore.PoweredMills, "slice19 step3 composed no mill powered before the tick");
Eq(1, s19Step3HudBefore.TotalMills, "slice19 step3 composed one mill total before the tick");
Eq(0, s19Step3HudBefore.WarnedMills, "slice19 step3 composed no mill warned before the tick");
Eq(2, s19Step3HudBefore.LiveNetworks, "slice19 step3 composed two live networks before the tick: the two fed transformer groups");
Eq(3, s19Step3HudBefore.LoneSources, "slice19 step3 composed three lone sources before the tick: step1 conduit, lone conduit, step2 conduit");
Eq(Voltage.None, s19Step3Sim.World.DeliveredVoltage(10, 20, 26), "slice19 step3 composed step2 mill delivery None");
Check(!s19Step3Sim.World.IsPowered(10, 20, 26), "slice19 step3 composed step2 mill is not powered");
Check(!s19Step3Sim.World.IsMismatched(10, 20, 26), "slice19 step3 composed step2 mill is not mismatched");
Eq(CellPowerState.None, s19Step3Sim.World.PowerState(12, 20, 10), "slice19 step3 composed step1 conduit state None by decision");
Eq(CellPowerState.None, s19Step3Sim.World.PowerState(11, 20, 10), "slice19 step3 composed step1 transformer state None by decision");
Eq(CellPowerState.None, s19Step3Sim.World.PowerState(11, 20, 14), "slice19 step3 composed dynamo transformer state None by decision");
Eq(CellPowerState.None, s19Step3Sim.World.PowerState(11, 20, 18), "slice19 step3 composed alternator transformer state None by decision");
Eq(CellPowerState.None, s19Step3Sim.World.PowerState(10, 20, 22), "slice19 step3 composed lone conduit state None by decision");
Eq(CellPowerState.None, s19Step3Sim.World.PowerState(10, 20, 26), "slice19 step3 composed step2 mill state None");
Eq(CellPowerState.None, s19Step3Sim.World.PowerState(11, 20, 30), "slice19 step3 composed dead cable state None: a mixed component carries no voltage");
s19Step3Sim.World.Advance();
Eq(BlockType.KineticMill, s19Step3Sim.World.Chunk.Get(10, 20, 26), "slice19 step3 composed step2 mill survives tick 1");
Eq(0, s19Step3Sim.World.PoweredBlocksGathered, "slice19 step3 composed powered gathered 0 after tick 1");
Eq(0, s19Step3Sim.World.KineticBlocksGathered, "slice19 step3 composed kinetic gathered 0 after tick 1");
Eq(0, s19Step3Sim.World.DestroyedMills, "slice19 step3 composed destroyed 0 after tick 1");
Eq(0, s19Step3Sim.World.Player.Inventory.Get(BlockType.Stone), "slice19 step3 composed stone 0 after tick 1");
Eq((2, 3), s19Step3Sim.World.LivePowerCounts(), "slice19 step3 composed counts still 2 live, 3 lone after tick 1");
var s19Step3HudAfter1 = s19Step3Sim.Hud();
Eq(2, s19Step3HudAfter1.LiveNetworks, "slice19 step3 composed still two live networks after tick 1");
Eq(3, s19Step3HudAfter1.LoneSources, "slice19 step3 composed still three lone sources after tick 1");
// Save/load round-trip: the counts are derived, never
// saved, so the loaded field must read them again, and the
// loaded tick must re-prove the same stability rather than
// arriving with a saved count.
var s19Step3Save = s19Step3Sim.SaveToJson();
Eq(43965, s19Step3Save.Length, "slice19 step3 composed save JSON length matches the demo run");
var s19Step3Loaded = new Simulation(99);
Check(s19Step3Loaded.LoadFromJson(s19Step3Save), "slice19 step3 composed save loads");
Eq(BlockType.Transformer, s19Step3Loaded.World.Chunk.Get(11, 20, 10), "slice19 step3 composed step1 transformer loads");
Eq(BlockType.Conduit, s19Step3Loaded.World.Chunk.Get(12, 20, 10), "slice19 step3 composed step1 conduit loads");
Eq(BlockType.Transformer, s19Step3Loaded.World.Chunk.Get(11, 20, 14), "slice19 step3 composed dynamo transformer loads");
Eq(BlockType.Transformer, s19Step3Loaded.World.Chunk.Get(11, 20, 18), "slice19 step3 composed alternator transformer loads");
Eq(BlockType.Conduit, s19Step3Loaded.World.Chunk.Get(10, 20, 22), "slice19 step3 composed lone conduit loads");
Eq(BlockType.KineticMill, s19Step3Loaded.World.Chunk.Get(10, 20, 26), "slice19 step3 composed step2 mill loads");
Eq(BlockType.Cable, s19Step3Loaded.World.Chunk.Get(11, 20, 30), "slice19 step3 composed dead cable loads");
Eq((2, 3), s19Step3Loaded.World.LivePowerCounts(), "slice19 step3 composed counts re-derive as 2 live, 3 lone after load");
var s19Step3LoadedHud = s19Step3Loaded.Hud();
Eq(0, s19Step3LoadedHud.PoweredMills, "slice19 step3 composed no mill powered after load");
Eq(1, s19Step3LoadedHud.TotalMills, "slice19 step3 composed one mill total after load");
Eq(0, s19Step3LoadedHud.WarnedMills, "slice19 step3 composed no mill warned after load");
Eq(2, s19Step3LoadedHud.LiveNetworks, "slice19 step3 composed two live networks after load");
Eq(3, s19Step3LoadedHud.LoneSources, "slice19 step3 composed three lone sources after load");
Eq(CellPowerState.None, s19Step3Loaded.World.PowerState(12, 20, 10), "slice19 step3 composed step1 conduit state None after load");
Eq(CellPowerState.None, s19Step3Loaded.World.PowerState(11, 20, 30), "slice19 step3 composed dead cable state None after load");
Eq(CellPowerState.None, s19Step3Loaded.World.PowerState(10, 20, 26), "slice19 step3 composed step2 mill state None after load");
Eq(0, s19Step3Loaded.World.PoweredBlocksGathered, "slice19 step3 composed powered counter resets on load");
Eq(0, s19Step3Loaded.World.KineticBlocksGathered, "slice19 step3 composed kinetic counter resets on load");
Eq(0, s19Step3Loaded.World.DestroyedMills, "slice19 step3 composed destroyed counter resets on load");
Eq(0, s19Step3Loaded.World.Player.Inventory.Get(BlockType.Stone), "slice19 step3 composed stone stays 0 on load");
Eq(16, s19Step3Loaded.World.Player.X, "slice19 step3 composed player X after load");
Eq(16, s19Step3Loaded.World.Player.Z, "slice19 step3 composed player Z after load");
s19Step3Loaded.World.Advance();
Eq(BlockType.KineticMill, s19Step3Loaded.World.Chunk.Get(10, 20, 26), "slice19 step3 composed step2 mill survives loaded tick 1");
Eq((2, 3), s19Step3Loaded.World.LivePowerCounts(), "slice19 step3 composed counts still 2 live, 3 lone after loaded tick 1");
Eq(0, s19Step3Loaded.World.PoweredBlocksGathered, "slice19 step3 composed powered gathered 0 after loaded tick 1");
Eq(0, s19Step3Loaded.World.KineticBlocksGathered, "slice19 step3 composed kinetic gathered 0 after loaded tick 1");
Eq(0, s19Step3Loaded.World.DestroyedMills, "slice19 step3 composed destroyed 0 after loaded tick 1");

// ---- Slice 20 Step 1: stuck-patrol player action, decided here (0.85.0) ----
// Decision, in one sentence: a FREE re-rally, for a stuck
// Patrol only — not a recall, not both, not neither (see
// DECISIONS.md for the rejected hypotheses and their costs).
// The staging below is deliberately split in two, because
// the two stuck shapes behave differently and conflating
// them would hide a limit: (a) a route-blocked patrol
// whose east neighbour is open — its loop is blocked at
// every waypoint, yet it can still step east, so a
// re-rally to an eastern rally makes it march at once;
// (b) a fully cell-enclosed patrol (the Slice 18 shape) —
// a re-rally changes its loop, but it still cannot step
// out, so it re-sticks on the new loop instead of marching
// through a wall. Both are proved, and neither is claimed
// to be the other.
var s20World = new WorldZone(42);
s20World.Player.X = 24; s20World.Player.Y = 5; s20World.Player.Z = 20;
var s20Patrol = s20World.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(s20World.Chunk.TrySet(23, 5, 20, BlockType.Stone), "slice20 re-rally test stages a west wall");
Check(s20World.Chunk.TrySet(24, 5, 21, BlockType.Stone), "slice20 re-rally test stages a south wall");
Check(s20World.Chunk.TrySet(24, 5, 19, BlockType.Stone), "slice20 re-rally test stages a north wall");
Eq(BlockType.Air, s20World.Chunk.Get(25, 5, 20), "slice20 re-rally test east step cell stays open");
for (int s20Tick = 0; s20Tick < 5; s20Tick++) s20Patrol.MoveForPatrol(s20World.Chunk, Array.Empty<Enemy>());
Eq(5, s20Patrol.PatrolConsecutiveRefusals, "slice20 re-rally route-blocked patrol counts five refusals");
Eq(0, s20Patrol.PatrolWaypointIndex, "slice20 re-rally route-blocked patrol index cycles back to 0");
Check(s20Patrol.IsPatrolStuck, "slice20 re-rally route-blocked patrol is stuck before the action");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s20World)[0].Activity, "slice20 re-rally route-blocked patrol reads stuck before the action");
Eq(24, s20Patrol.X, "slice20 re-rally route-blocked patrol holds its X before the action");
Eq(3, TacticalSquad.PatrolLoopRadius, "slice20 re-rally loop radius is still 3, not retuned");
Eq(5, TacticalSquad.PatrolWaypointCount, "slice20 re-rally loop waypoint count is still 5, not retuned");
Check(s20World.TryReRallyStuckPatrol(0, 30, 5, 20), "slice20 re-rally stuck patrol accepts a free re-rally");
Eq(30, s20Patrol.RallyX, "slice20 re-rally new rally X");
Eq(20, s20Patrol.RallyZ, "slice20 re-rally new rally Z");
Eq(0, s20Patrol.PatrolWaypointIndex, "slice20 re-rally waypoint index resets to 0");
Eq(0, s20Patrol.PatrolConsecutiveRefusals, "slice20 re-rally refusal counter resets to 0");
Check(!s20Patrol.IsPatrolStuck, "slice20 re-rally patrol is not stuck immediately after the action");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s20World)[0].Activity, "slice20 re-rally patrol reads patrolling immediately after the action");
Eq(24, s20Patrol.X, "slice20 re-rally does not teleport the squad to the new rally");
Eq((30, 5, 20), s20Patrol.PatrolTarget, "slice20 re-rally loop re-derives from the new rally, waypoint 0");
for (int s20Tick = 0; s20Tick < 6; s20Tick++) s20Patrol.MoveForPatrol(s20World.Chunk, Array.Empty<Enemy>());
Eq(30, s20Patrol.X, "slice20 re-rally patrol marches to the new rally in six steps");
Eq(0, s20Patrol.PatrolWaypointIndex, "slice20 re-rally patrol index is still 0 on arrival at the new rally");
Check(s20Patrol.MoveForPatrol(s20World.Chunk, Array.Empty<Enemy>()), "slice20 re-rally patrol turns onto the new loop");
Eq(31, s20Patrol.X, "slice20 re-rally patrol steps east onto the new loop");
Eq(1, s20Patrol.PatrolWaypointIndex, "slice20 re-rally patrol index advances to the east waypoint");
Eq((33, 5, 20), s20Patrol.PatrolTarget, "slice20 re-rally east waypoint is the new rally plus radius 3");
// Recon and Heavy: the action is doctrine, index, and
// counter logic, not a kind stat — but kinds differ in
// health, damage, and range, so all three are proved for
// the core effect and for fire in passing, not just one.
var s20ReconWorld = new WorldZone(42);
s20ReconWorld.Player.X = 24; s20ReconWorld.Player.Y = 5; s20ReconWorld.Player.Z = 20;
var s20Recon = s20ReconWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Recon)!;
Check(s20ReconWorld.Chunk.TrySet(23, 5, 20, BlockType.Stone), "slice20 re-rally recon test stages a west wall");
Check(s20ReconWorld.Chunk.TrySet(24, 5, 21, BlockType.Stone), "slice20 re-rally recon test stages a south wall");
Check(s20ReconWorld.Chunk.TrySet(24, 5, 19, BlockType.Stone), "slice20 re-rally recon test stages a north wall");
for (int s20Tick = 0; s20Tick < 5; s20Tick++) s20Recon.MoveForPatrol(s20ReconWorld.Chunk, Array.Empty<Enemy>());
Check(s20Recon.IsPatrolStuck, "slice20 re-rally recon patrol is stuck before the action");
Check(s20ReconWorld.TryReRallyStuckPatrol(0, 30, 5, 20), "slice20 re-rally recon patrol accepts a re-rally");
Eq(30, s20Recon.RallyX, "slice20 re-rally recon new rally X");
Eq(0, s20Recon.PatrolConsecutiveRefusals, "slice20 re-rally recon refusal counter resets");
Check(s20Recon.MoveForPatrol(s20ReconWorld.Chunk, Array.Empty<Enemy>()), "slice20 re-rally recon patrol marches east on the new loop");
Eq(25, s20Recon.X, "slice20 re-rally recon patrol steps east to 25");
Eq(12, s20Recon.DamagePerTick, "slice20 re-rally recon damage per tick unchanged");
Check(s20Recon.FindTarget(new[] { new Enemy(28, 5, 20) }) != null, "slice20 re-rally recon patrol still fires in passing at range 3 after marching");
var s20HeavyWorld = new WorldZone(42);
s20HeavyWorld.Player.X = 24; s20HeavyWorld.Player.Y = 5; s20HeavyWorld.Player.Z = 20;
var s20Heavy = s20HeavyWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Heavy)!;
Check(s20HeavyWorld.Chunk.TrySet(23, 5, 20, BlockType.Stone), "slice20 re-rally heavy test stages a west wall");
Check(s20HeavyWorld.Chunk.TrySet(24, 5, 21, BlockType.Stone), "slice20 re-rally heavy test stages a south wall");
Check(s20HeavyWorld.Chunk.TrySet(24, 5, 19, BlockType.Stone), "slice20 re-rally heavy test stages a north wall");
for (int s20Tick = 0; s20Tick < 5; s20Tick++) s20Heavy.MoveForPatrol(s20HeavyWorld.Chunk, Array.Empty<Enemy>());
Check(s20Heavy.IsPatrolStuck, "slice20 re-rally heavy patrol is stuck before the action");
Check(s20HeavyWorld.TryReRallyStuckPatrol(0, 30, 5, 20), "slice20 re-rally heavy patrol accepts a re-rally");
Eq(0, s20Heavy.PatrolConsecutiveRefusals, "slice20 re-rally heavy refusal counter resets");
Check(s20Heavy.MoveForPatrol(s20HeavyWorld.Chunk, Array.Empty<Enemy>()), "slice20 re-rally heavy patrol marches east on the new loop");
Eq(25, s20Heavy.X, "slice20 re-rally heavy patrol steps east to 25");
Eq(16, s20Heavy.DamagePerTick, "slice20 re-rally heavy damage per tick unchanged");
Check(s20Heavy.FindTarget(new[] { new Enemy(27, 5, 20) }) != null, "slice20 re-rally heavy patrol still fires in passing at range 2 after marching");
// Enclosed shape, honestly: a re-rally that cannot fail
// again would hide the block, not answer it. Here the
// loop changes and the squad still cannot step out, so
// it reads Stuck again after a full circuit on the new
// loop, at the same cell, with the new rally kept.
var s20EnclosedWorld = new WorldZone(42);
s20EnclosedWorld.Player.X = 20; s20EnclosedWorld.Player.Y = 5; s20EnclosedWorld.Player.Z = 20;
var s20Enclosed = s20EnclosedWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s20EnclosedWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice20 re-rally enclosed test stages an east wall");
Check(s20EnclosedWorld.Chunk.TrySet(19, 5, 20, BlockType.Stone), "slice20 re-rally enclosed test stages a west wall");
Check(s20EnclosedWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice20 re-rally enclosed test stages a south wall");
Check(s20EnclosedWorld.Chunk.TrySet(20, 5, 19, BlockType.Stone), "slice20 re-rally enclosed test stages a north wall");
for (int s20Tick = 0; s20Tick < 5; s20Tick++) s20Enclosed.MoveForPatrol(s20EnclosedWorld.Chunk, Array.Empty<Enemy>());
Check(s20Enclosed.IsPatrolStuck, "slice20 re-rally enclosed patrol is stuck before the action");
Check(s20EnclosedWorld.TryReRallyStuckPatrol(0, 26, 5, 20), "slice20 re-rally enclosed patrol accepts a re-rally to an outside rally");
Eq(26, s20Enclosed.RallyX, "slice20 re-rally enclosed patrol keeps the new rally");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s20EnclosedWorld)[0].Activity, "slice20 re-rally enclosed patrol reads patrolling at once on the new loop");
for (int s20Tick = 0; s20Tick < 5; s20Tick++) s20Enclosed.MoveForPatrol(s20EnclosedWorld.Chunk, Array.Empty<Enemy>());
Check(s20Enclosed.IsPatrolStuck, "slice20 re-rally enclosed patrol reads stuck again after a full circuit on the new loop");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s20EnclosedWorld)[0].Activity, "slice20 re-rally enclosed patrol snapshot reads stuck again");
Eq(20, s20Enclosed.X, "slice20 re-rally enclosed patrol never moved through a wall");
Eq(20, s20Enclosed.Z, "slice20 re-rally enclosed patrol never moved through a wall in Z");
// Fail-closed refusals: every gate failure returns false
// and changes nothing — the action is stuck-Patrol-only,
// and a stale or wrong target must never re-rally the
// wrong squad.
var s20MovingWorld = new WorldZone(42);
s20MovingWorld.Player.X = 20; s20MovingWorld.Player.Y = 5; s20MovingWorld.Player.Z = 20;
var s20Moving = s20MovingWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s20Moving.MoveForPatrol(s20MovingWorld.Chunk, Array.Empty<Enemy>()), "slice20 re-rally moving test patrol walks east in open ground");
Check(!s20Moving.IsPatrolStuck, "slice20 re-rally moving patrol is not stuck");
Check(!s20MovingWorld.TryReRallyStuckPatrol(0, 26, 5, 20), "slice20 re-rally non-stuck patrol is refused");
Eq(20, s20Moving.RallyX, "slice20 re-rally refused non-stuck patrol keeps its rally");
Eq(1, s20Moving.PatrolWaypointIndex, "slice20 re-rally refused non-stuck patrol keeps its index");
Eq(21, s20Moving.X, "slice20 re-rally refused non-stuck patrol keeps its position");
Check(!s20MovingWorld.TryReRallyStuckPatrol(-1, 26, 5, 20), "slice20 re-rally negative index is refused");
Check(!s20MovingWorld.TryReRallyStuckPatrol(1, 26, 5, 20), "slice20 re-rally out-of-range index is refused");
Eq(20, s20Moving.RallyX, "slice20 re-rally refused index cases keep the rally");
var s20DoctrinesWorld = new WorldZone(42);
s20DoctrinesWorld.Player.X = 20; s20DoctrinesWorld.Player.Y = 5; s20DoctrinesWorld.Player.Z = 20;
var s20DocPatrol = s20DoctrinesWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
var s20DocHold = s20DoctrinesWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.HoldGround)!;
var s20DocAdvance = s20DoctrinesWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.Advance)!;
var s20DocDefend = s20DoctrinesWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.DefendCore)!;
Check(s20DoctrinesWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice20 re-rally doctrine test stages an east wall");
Check(s20DoctrinesWorld.Chunk.TrySet(19, 5, 20, BlockType.Stone), "slice20 re-rally doctrine test stages a west wall");
Check(s20DoctrinesWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice20 re-rally doctrine test stages a south wall");
Check(s20DoctrinesWorld.Chunk.TrySet(20, 5, 19, BlockType.Stone), "slice20 re-rally doctrine test stages a north wall");
for (int s20Tick = 0; s20Tick < 5; s20Tick++) s20DocPatrol.MoveForPatrol(s20DoctrinesWorld.Chunk, Array.Empty<Enemy>());
Check(s20DocPatrol.IsPatrolStuck, "slice20 re-rally doctrine test patrol is stuck");
Check(!s20DoctrinesWorld.TryReRallyStuckPatrol(1, 26, 5, 20), "slice20 re-rally HoldGround squad is refused");
Check(!s20DoctrinesWorld.TryReRallyStuckPatrol(2, 26, 5, 20), "slice20 re-rally Advance squad is refused");
Check(!s20DoctrinesWorld.TryReRallyStuckPatrol(3, 26, 5, 20), "slice20 re-rally DefendCore squad is refused");
Eq(10, s20DocHold.RallyX, "slice20 re-rally refused HoldGround squad keeps its rally");
Eq(10, s20DocAdvance.RallyX, "slice20 re-rally refused Advance squad keeps its rally");
Eq(10, s20DocDefend.RallyX, "slice20 re-rally refused DefendCore squad keeps its rally");
Eq(20, s20DocHold.X, "slice20 re-rally refused squads keep their position");
Check(s20DoctrinesWorld.TryReRallyStuckPatrol(0, 26, 5, 20), "slice20 re-rally stuck patrol still succeeds after the doctrine refusals");
Eq(26, s20DocPatrol.RallyX, "slice20 re-rally stuck patrol takes the new rally after the refusals");
Eq(10, s20DocHold.RallyX, "slice20 re-rally other squads are untouched by a patrol re-rally");
var s20DeadWorld = new WorldZone(42);
s20DeadWorld.Player.X = 20; s20DeadWorld.Player.Y = 5; s20DeadWorld.Player.Z = 20;
var s20Dead = s20DeadWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
s20Dead.TakeDamage(1000);
Check(s20Dead.IsDead, "slice20 re-rally dead test squad is dead before the action");
Check(!s20DeadWorld.TryReRallyStuckPatrol(0, 26, 5, 20), "slice20 re-rally dead squad is refused");
Eq(20, s20Dead.RallyX, "slice20 re-rally refused dead squad keeps its rally");
var s20BoundsWorld = new WorldZone(42);
s20BoundsWorld.Player.X = 20; s20BoundsWorld.Player.Y = 5; s20BoundsWorld.Player.Z = 20;
var s20Bounds = s20BoundsWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s20BoundsWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice20 re-rally bounds test stages an east wall");
Check(s20BoundsWorld.Chunk.TrySet(19, 5, 20, BlockType.Stone), "slice20 re-rally bounds test stages a west wall");
Check(s20BoundsWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice20 re-rally bounds test stages a south wall");
Check(s20BoundsWorld.Chunk.TrySet(20, 5, 19, BlockType.Stone), "slice20 re-rally bounds test stages a north wall");
for (int s20Tick = 0; s20Tick < 5; s20Tick++) s20Bounds.MoveForPatrol(s20BoundsWorld.Chunk, Array.Empty<Enemy>());
Check(s20Bounds.IsPatrolStuck, "slice20 re-rally bounds test patrol is stuck");
Check(!s20BoundsWorld.TryReRallyStuckPatrol(0, 99, 5, 20), "slice20 re-rally out-of-bounds rally is refused");
Check(!s20BoundsWorld.TryReRallyStuckPatrol(0, -1, 5, 20), "slice20 re-rally negative rally is refused");
Check(!s20BoundsWorld.TryReRallyStuckPatrol(0, 20, 5, 20), "slice20 re-rally to the same rally is refused");
Eq(20, s20Bounds.RallyX, "slice20 re-rally refused rally cases keep the old rally");
Eq(5, s20Bounds.PatrolConsecutiveRefusals, "slice20 re-rally refused rally cases keep the refusal counter");
Check(s20Bounds.IsPatrolStuck, "slice20 re-rally refused rally cases keep the patrol stuck");
// Solid rally, deploy-consistent: accepted, never
// teleported into. The squad steps to the open cell
// before it, then holds short of the solid rally cell.
var s20SolidWorld = new WorldZone(42);
s20SolidWorld.Player.X = 24; s20SolidWorld.Player.Y = 5; s20SolidWorld.Player.Z = 20;
var s20Solid = s20SolidWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s20SolidWorld.Chunk.TrySet(23, 5, 20, BlockType.Stone), "slice20 re-rally solid test stages a west wall");
Check(s20SolidWorld.Chunk.TrySet(24, 5, 21, BlockType.Stone), "slice20 re-rally solid test stages a south wall");
Check(s20SolidWorld.Chunk.TrySet(24, 5, 19, BlockType.Stone), "slice20 re-rally solid test stages a north wall");
for (int s20Tick = 0; s20Tick < 5; s20Tick++) s20Solid.MoveForPatrol(s20SolidWorld.Chunk, Array.Empty<Enemy>());
Check(s20Solid.IsPatrolStuck, "slice20 re-rally solid test patrol is stuck");
Check(s20SolidWorld.Chunk.TrySet(26, 5, 20, BlockType.Stone), "slice20 re-rally solid test stages a solid new rally cell");
Check(s20SolidWorld.TryReRallyStuckPatrol(0, 26, 5, 20), "slice20 re-rally to a solid rally cell is accepted, as deploy accepts one");
Eq(26, s20Solid.RallyX, "slice20 re-rally solid new rally X");
Check(s20Solid.MoveForPatrol(s20SolidWorld.Chunk, Array.Empty<Enemy>()), "slice20 re-rally solid patrol steps east toward the solid rally");
Eq(25, s20Solid.X, "slice20 re-rally solid patrol reaches the open cell before the rally");
Check(!s20Solid.MoveForPatrol(s20SolidWorld.Chunk, Array.Empty<Enemy>()), "slice20 re-rally solid patrol step into the solid rally is refused");
Eq(25, s20Solid.X, "slice20 re-rally solid patrol holds short of the solid rally, never inside it");
Eq(BlockType.Stone, s20SolidWorld.Chunk.Get(26, 5, 20), "slice20 re-rally solid rally cell stays stone");
// Fire through a world tick after a re-rally: the
// re-rallied Vanguard walks east and kills in passing,
// exactly as a moving patrol fires — the action changes
// no fire rule.
var s20FireWorld = new WorldZone(42);
s20FireWorld.Player.X = 24; s20FireWorld.Player.Y = 5; s20FireWorld.Player.Z = 20;
var s20Fire = s20FireWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s20FireWorld.Chunk.TrySet(23, 5, 20, BlockType.Stone), "slice20 re-rally fire test stages a west wall");
Check(s20FireWorld.Chunk.TrySet(24, 5, 21, BlockType.Stone), "slice20 re-rally fire test stages a south wall");
Check(s20FireWorld.Chunk.TrySet(24, 5, 19, BlockType.Stone), "slice20 re-rally fire test stages a north wall");
for (int s20Tick = 0; s20Tick < 5; s20Tick++) s20Fire.MoveForPatrol(s20FireWorld.Chunk, Array.Empty<Enemy>());
Check(s20Fire.IsPatrolStuck, "slice20 re-rally fire test patrol is stuck");
Check(s20FireWorld.TryReRallyStuckPatrol(0, 30, 5, 20), "slice20 re-rally fire test patrol accepts a re-rally");
s20FireWorld.Enemies.Add(new Enemy(27, 5, 20));
s20FireWorld.Advance();
Eq(25, s20Fire.X, "slice20 re-rally fire test patrol walks east on the tick after the action");
Eq(0, s20FireWorld.Enemies.Count, "slice20 re-rally fire test patrol kills in passing on that tick");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s20FireWorld)[0].Activity, "slice20 re-rally fire test patrol reads patrolling after the tick");
// Save/load: a re-rallied patrol loads with its new
// rally and marches from index 0, through the thin
// Simulation passthrough; and a stuck-before-save patrol
// still loads Patrolling at index 0 under the existing
// rule, which this step does not break.
var s20Sim = new Simulation(42);
s20Sim.World.Player.X = 24; s20Sim.World.Player.Y = 5; s20Sim.World.Player.Z = 20;
var s20SimPatrol = s20Sim.World.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s20Sim.World.Chunk.TrySet(23, 5, 20, BlockType.Stone), "slice20 re-rally save test stages a west wall");
Check(s20Sim.World.Chunk.TrySet(24, 5, 21, BlockType.Stone), "slice20 re-rally save test stages a south wall");
Check(s20Sim.World.Chunk.TrySet(24, 5, 19, BlockType.Stone), "slice20 re-rally save test stages a north wall");
for (int s20Tick = 0; s20Tick < 5; s20Tick++) s20SimPatrol.MoveForPatrol(s20Sim.World.Chunk, Array.Empty<Enemy>());
Check(s20SimPatrol.IsPatrolStuck, "slice20 re-rally save test patrol is stuck before the action");
Eq(0, s20Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice20 re-rally save test starts with no ore");
Check(s20Sim.TryReRallyStuckPatrol(0, 30, 5, 20), "slice20 re-rally save test accepts a re-rally through the Simulation passthrough");
Eq(0, s20Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice20 re-rally charges no ore");
var s20Save = s20Sim.SaveToJson();
var s20LoadedSim = new Simulation(99);
Check(s20LoadedSim.LoadFromJson(s20Save), "slice20 re-rally save test loads");
Eq(1, s20LoadedSim.World.Squads.Count, "slice20 re-rally save test keeps one squad after load");
var s20Loaded = s20LoadedSim.World.Squads[0];
Eq(24, s20Loaded.X, "slice20 re-rally save test loaded position preserved");
Eq(30, s20Loaded.RallyX, "slice20 re-rally save test loaded rally is the new rally");
Eq(0, s20Loaded.PatrolWaypointIndex, "slice20 re-rally save test loaded index is 0");
Eq(0, s20Loaded.PatrolConsecutiveRefusals, "slice20 re-rally save test loaded counter is 0");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s20LoadedSim.World)[0].Activity, "slice20 re-rally save test loaded squad reads patrolling");
Check(s20Loaded.MoveForPatrol(s20LoadedSim.World.Chunk, Array.Empty<Enemy>()), "slice20 re-rally save test loaded patrol marches east");
Eq(25, s20Loaded.X, "slice20 re-rally save test loaded patrol steps east to 25");
var s20StuckSaveSim = new Simulation(42);
s20StuckSaveSim.World.Player.X = 20; s20StuckSaveSim.World.Player.Y = 5; s20StuckSaveSim.World.Player.Z = 20;
var s20StuckSave = s20StuckSaveSim.World.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s20StuckSaveSim.World.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice20 re-rally stuck-save test stages an east wall");
Check(s20StuckSaveSim.World.Chunk.TrySet(19, 5, 20, BlockType.Stone), "slice20 re-rally stuck-save test stages a west wall");
Check(s20StuckSaveSim.World.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice20 re-rally stuck-save test stages a south wall");
Check(s20StuckSaveSim.World.Chunk.TrySet(20, 5, 19, BlockType.Stone), "slice20 re-rally stuck-save test stages a north wall");
for (int s20Tick = 0; s20Tick < 5; s20Tick++) s20StuckSave.MoveForPatrol(s20StuckSaveSim.World.Chunk, Array.Empty<Enemy>());
Check(s20StuckSave.IsPatrolStuck, "slice20 re-rally stuck-save test patrol is stuck before the save");
var s20StuckSaveJson = s20StuckSaveSim.SaveToJson();
var s20StuckLoadedSim = new Simulation(99);
Check(s20StuckLoadedSim.LoadFromJson(s20StuckSaveJson), "slice20 re-rally stuck-save test loads");
var s20StuckLoaded = s20StuckLoadedSim.World.Squads[0];
Check(!s20StuckLoaded.IsPatrolStuck, "slice20 re-rally stuck-before-save patrol loads not stuck, as before");
Eq(0, s20StuckLoaded.PatrolWaypointIndex, "slice20 re-rally stuck-before-save patrol loads at index 0, as before");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s20StuckLoadedSim.World)[0].Activity, "slice20 re-rally stuck-before-save patrol loads reading patrolling, as before");
for (int s20Tick = 0; s20Tick < 5; s20Tick++) s20StuckLoaded.MoveForPatrol(s20StuckLoadedSim.World.Chunk, Array.Empty<Enemy>());
Check(s20StuckLoaded.IsPatrolStuck, "slice20 re-rally stuck-before-save patrol is stuck again after a full circuit");
// Paid path unchanged, and a re-rally frees no cap
// slot: three paid squads, one stuck patrol among them,
// re-rallied free, and a fourth paid deploy with ore in
// hand is still refused at the cap, with ore untouched.
var s20PaidWorld = new WorldZone(42);
s20PaidWorld.Player.X = 24; s20PaidWorld.Player.Y = 5; s20PaidWorld.Player.Z = 20;
s20PaidWorld.Player.Inventory.Add(BlockType.EmberOre, 40);
var s20PaidPatrol = s20PaidWorld.TryDeployPaidSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Eq(30, s20PaidWorld.Player.Inventory.Get(BlockType.EmberOre), "slice20 re-rally paid test first deploy charges 10");
Check(s20PaidWorld.Chunk.TrySet(23, 5, 20, BlockType.Stone), "slice20 re-rally paid test stages a west wall");
Check(s20PaidWorld.Chunk.TrySet(24, 5, 21, BlockType.Stone), "slice20 re-rally paid test stages a south wall");
Check(s20PaidWorld.Chunk.TrySet(24, 5, 19, BlockType.Stone), "slice20 re-rally paid test stages a north wall");
for (int s20Tick = 0; s20Tick < 5; s20Tick++) s20PaidPatrol.MoveForPatrol(s20PaidWorld.Chunk, Array.Empty<Enemy>());
Check(s20PaidPatrol.IsPatrolStuck, "slice20 re-rally paid test patrol is stuck");
Check(s20PaidWorld.TryDeployPaidSquad(10, 5, 10, TacticalDoctrine.HoldGround, SquadKind.Vanguard) != null, "slice20 re-rally paid test second deploy succeeds");
Check(s20PaidWorld.TryDeployPaidSquad(10, 5, 10, TacticalDoctrine.Advance, SquadKind.Recon) != null, "slice20 re-rally paid test third deploy succeeds at the cap");
Eq(10, s20PaidWorld.Player.Inventory.Get(BlockType.EmberOre), "slice20 re-rally paid test three deploys leave 10 ore");
Eq(3, SquadEconomy.LiveSquadCount(s20PaidWorld), "slice20 re-rally paid test cap is full at 3 live squads");
Check(s20PaidWorld.TryReRallyStuckPatrol(0, 30, 5, 20), "slice20 re-rally paid test stuck patrol accepts a free re-rally");
Eq(10, s20PaidWorld.Player.Inventory.Get(BlockType.EmberOre), "slice20 re-rally paid test re-rally charges nothing and refunds nothing");
Eq(3, SquadEconomy.LiveSquadCount(s20PaidWorld), "slice20 re-rally paid test re-rally frees no cap slot");
Check(s20PaidWorld.TryDeployPaidSquad(10, 5, 10, TacticalDoctrine.DefendCore, SquadKind.Heavy) == null, "slice20 re-rally paid test fourth deploy is still refused at the cap");
Eq(10, s20PaidWorld.Player.Inventory.Get(BlockType.EmberOre), "slice20 re-rally paid test refused fourth deploy charges nothing");
Eq(30, s20PaidPatrol.RallyX, "slice20 re-rally paid test patrol keeps its new rally after the cap refusal");

// ---- Slice 20 Step 2: composed demo + review, closing Slice 20 (0.86.0) ----
// One Simulation composing the Step 1 decision with its
// controls through the paid path only, mirroring the demo
// scenario exactly: a Heavy HoldGround marched away first,
// a Recon Patrol marched to its rally and walking its loop,
// then a Vanguard Patrol deployed at the player cell in the
// route-blocked shape (west, south, and north walls, east
// open) that counts five refusals and reads Stuck, unmoved,
// while the other two keep their readings; then a
// stuck-before-save load copy, the two fail-closed
// refusals, the free re-rally, a refused fourth deploy at
// the full cap, the six-tick march to the new rally, the
// turn onto the new loop, and a save/load round-trip that
// loads the new rally. No new core system, no adapter
// edit, and no checklist item is added by this step: item
// 39 already observes the action, and this step only
// proves it together with its controls in one field.
var s20Step2Sim = new Simulation(42);
s20Step2Sim.World.Player.Inventory.Add(BlockType.EmberOre, 30);
s20Step2Sim.BeginOrbitalPick();
Check(s20Step2Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround), "slice20 step2 composed hold doctrine selected");
Check(s20Step2Sim.OrbitalCommand.SelectKind(SquadKind.Heavy), "slice20 step2 composed hold kind selected");
Check(s20Step2Sim.SetOrbitalRally(10, 5, 10), "slice20 step2 composed hold rally set");
var s20Step2Hold = s20Step2Sim.ConfirmOrbitalDeploy();
Check(s20Step2Hold != null, "slice20 step2 composed hold deploys paid");
Eq(TacticalDoctrine.HoldGround, s20Step2Hold!.Doctrine, "slice20 step2 composed hold keeps holdground doctrine");
Eq(SquadKind.Heavy, s20Step2Hold.Kind, "slice20 step2 composed hold keeps heavy kind");
Eq(20, s20Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice20 step2 composed hold charges 10 ore");
Eq(16, s20Step2Hold.X, "slice20 step2 composed hold deploys at the player X");
Eq(16, s20Step2Hold.Z, "slice20 step2 composed hold deploys at the player Z");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s20Step2Sim.World)[0].Activity, "slice20 step2 composed hold reads marching before any tick");
s20Step2Sim.World.Advance();
Eq(15, s20Step2Hold.X, "slice20 step2 composed hold march tick 1 X");
Eq(16, s20Step2Hold.Z, "slice20 step2 composed hold march tick 1 Z");
s20Step2Sim.World.Advance();
Eq(15, s20Step2Hold.X, "slice20 step2 composed hold march tick 2 X");
Eq(15, s20Step2Hold.Z, "slice20 step2 composed hold march tick 2 Z");
s20Step2Sim.World.Advance();
Eq(14, s20Step2Hold.X, "slice20 step2 composed hold march tick 3 X");
Eq(15, s20Step2Hold.Z, "slice20 step2 composed hold march tick 3 Z");
s20Step2Sim.World.Advance();
Eq(14, s20Step2Hold.X, "slice20 step2 composed hold march tick 4 X");
Eq(14, s20Step2Hold.Z, "slice20 step2 composed hold march tick 4 Z");
s20Step2Sim.World.Advance();
Eq(13, s20Step2Hold.X, "slice20 step2 composed hold march tick 5 X");
Eq(14, s20Step2Hold.Z, "slice20 step2 composed hold march tick 5 Z");
s20Step2Sim.World.Advance();
Eq(13, s20Step2Hold.X, "slice20 step2 composed hold march tick 6 X");
Eq(13, s20Step2Hold.Z, "slice20 step2 composed hold march tick 6 Z");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s20Step2Sim.World)[0].Activity, "slice20 step2 composed hold still reads marching after 6 ticks");
s20Step2Sim.BeginOrbitalPick();
Check(s20Step2Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol), "slice20 step2 composed patrol doctrine selected");
Check(s20Step2Sim.OrbitalCommand.SelectKind(SquadKind.Recon), "slice20 step2 composed patrol kind selected");
Check(s20Step2Sim.SetOrbitalRally(20, 5, 20), "slice20 step2 composed patrol rally set");
var s20Step2Patrol = s20Step2Sim.ConfirmOrbitalDeploy();
Check(s20Step2Patrol != null, "slice20 step2 composed patrol deploys paid");
Eq(TacticalDoctrine.Patrol, s20Step2Patrol!.Doctrine, "slice20 step2 composed patrol keeps patrol doctrine");
Eq(SquadKind.Recon, s20Step2Patrol.Kind, "slice20 step2 composed patrol keeps recon kind");
Eq(10, s20Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice20 step2 composed patrol charges 10 ore");
Eq(16, s20Step2Patrol.X, "slice20 step2 composed patrol deploys at the player X");
Eq(16, s20Step2Patrol.Z, "slice20 step2 composed patrol deploys at the player Z");
s20Step2Sim.World.Advance();
Eq(12, s20Step2Hold.X, "slice20 step2 composed patrol march tick 1 hold X");
Eq(13, s20Step2Hold.Z, "slice20 step2 composed patrol march tick 1 hold Z");
Eq(17, s20Step2Patrol.X, "slice20 step2 composed patrol march tick 1 patrol X");
Eq(16, s20Step2Patrol.Z, "slice20 step2 composed patrol march tick 1 patrol Z");
Eq(0, s20Step2Patrol.PatrolWaypointIndex, "slice20 step2 composed patrol march tick 1 patrol waypoint");
s20Step2Sim.World.Advance();
Eq(12, s20Step2Hold.X, "slice20 step2 composed patrol march tick 2 hold X");
Eq(12, s20Step2Hold.Z, "slice20 step2 composed patrol march tick 2 hold Z");
Eq(17, s20Step2Patrol.X, "slice20 step2 composed patrol march tick 2 patrol X");
Eq(17, s20Step2Patrol.Z, "slice20 step2 composed patrol march tick 2 patrol Z");
s20Step2Sim.World.Advance();
Eq(11, s20Step2Hold.X, "slice20 step2 composed patrol march tick 3 hold X");
Eq(12, s20Step2Hold.Z, "slice20 step2 composed patrol march tick 3 hold Z");
Eq(18, s20Step2Patrol.X, "slice20 step2 composed patrol march tick 3 patrol X");
Eq(17, s20Step2Patrol.Z, "slice20 step2 composed patrol march tick 3 patrol Z");
s20Step2Sim.World.Advance();
Eq(11, s20Step2Hold.X, "slice20 step2 composed patrol march tick 4 hold X");
Eq(11, s20Step2Hold.Z, "slice20 step2 composed patrol march tick 4 hold Z");
Eq(18, s20Step2Patrol.X, "slice20 step2 composed patrol march tick 4 patrol X");
Eq(18, s20Step2Patrol.Z, "slice20 step2 composed patrol march tick 4 patrol Z");
s20Step2Sim.World.Advance();
Eq(10, s20Step2Hold.X, "slice20 step2 composed patrol march tick 5 hold X");
Eq(11, s20Step2Hold.Z, "slice20 step2 composed patrol march tick 5 hold Z");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s20Step2Sim.World)[0].Activity, "slice20 step2 composed patrol march tick 5 hold still reads marching");
Eq(19, s20Step2Patrol.X, "slice20 step2 composed patrol march tick 5 patrol X");
Eq(18, s20Step2Patrol.Z, "slice20 step2 composed patrol march tick 5 patrol Z");
s20Step2Sim.World.Advance();
Eq(10, s20Step2Hold.X, "slice20 step2 composed patrol march tick 6 hold X");
Eq(10, s20Step2Hold.Z, "slice20 step2 composed patrol march tick 6 hold Z");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s20Step2Sim.World)[0].Activity, "slice20 step2 composed patrol march tick 6 hold reads holding at its rally");
Eq(19, s20Step2Patrol.X, "slice20 step2 composed patrol march tick 6 patrol X");
Eq(19, s20Step2Patrol.Z, "slice20 step2 composed patrol march tick 6 patrol Z");
s20Step2Sim.World.Advance();
Eq(20, s20Step2Patrol.X, "slice20 step2 composed patrol march tick 7 patrol X");
Eq(19, s20Step2Patrol.Z, "slice20 step2 composed patrol march tick 7 patrol Z");
s20Step2Sim.World.Advance();
Eq(20, s20Step2Patrol.X, "slice20 step2 composed patrol march tick 8 patrol reaches its rally X");
Eq(20, s20Step2Patrol.Z, "slice20 step2 composed patrol march tick 8 patrol reaches its rally Z");
Eq(0, s20Step2Patrol.PatrolWaypointIndex, "slice20 step2 composed patrol march tick 8 patrol waypoint at the rally");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s20Step2Sim.World)[1].Activity, "slice20 step2 composed patrol march tick 8 patrol reads patrolling at its rally");
s20Step2Sim.BeginOrbitalPick();
Check(s20Step2Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol), "slice20 step2 composed stuck doctrine selected");
Check(s20Step2Sim.OrbitalCommand.SelectKind(SquadKind.Vanguard), "slice20 step2 composed stuck kind selected");
Check(s20Step2Sim.SetOrbitalRally(12, 5, 16), "slice20 step2 composed stuck rally set");
var s20Step2Stuck = s20Step2Sim.ConfirmOrbitalDeploy();
Check(s20Step2Stuck != null, "slice20 step2 composed stuck deploys paid");
Eq(TacticalDoctrine.Patrol, s20Step2Stuck!.Doctrine, "slice20 step2 composed stuck keeps patrol doctrine");
Eq(SquadKind.Vanguard, s20Step2Stuck.Kind, "slice20 step2 composed stuck keeps vanguard kind");
Eq(0, s20Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice20 step2 composed stuck charges the last 10 ore");
Eq(3, s20Step2Sim.World.Squads.Count, "slice20 step2 composed three paid squads fielded, cap full");
Eq(16, s20Step2Stuck.X, "slice20 step2 composed stuck deploys at the player X");
Eq(16, s20Step2Stuck.Z, "slice20 step2 composed stuck deploys at the player Z");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s20Step2Sim.World)[2].Activity, "slice20 step2 composed stuck reads patrolling before its circuit");
Check(s20Step2Sim.World.Chunk.TrySet(15, 5, 16, BlockType.Stone), "slice20 step2 composed test stages the west enclosure wall");
Check(s20Step2Sim.World.Chunk.TrySet(16, 5, 17, BlockType.Stone), "slice20 step2 composed test stages the south enclosure wall");
Check(s20Step2Sim.World.Chunk.TrySet(16, 5, 15, BlockType.Stone), "slice20 step2 composed test stages the north enclosure wall");
Eq(BlockType.Air, s20Step2Sim.World.Chunk.Get(17, 5, 16), "slice20 step2 composed test east step cell stays open");
s20Step2Sim.World.Advance();
Eq(21, s20Step2Patrol.X, "slice20 step2 composed stuck tick 1 patrol X");
Eq(20, s20Step2Patrol.Z, "slice20 step2 composed stuck tick 1 patrol Z");
Eq(1, s20Step2Patrol.PatrolWaypointIndex, "slice20 step2 composed stuck tick 1 patrol waypoint is east");
Eq(16, s20Step2Stuck.X, "slice20 step2 composed stuck tick 1 stuck X");
Eq(16, s20Step2Stuck.Z, "slice20 step2 composed stuck tick 1 stuck Z");
Eq(1, s20Step2Stuck.PatrolWaypointIndex, "slice20 step2 composed stuck tick 1 stuck waypoint");
Eq(1, s20Step2Stuck.PatrolConsecutiveRefusals, "slice20 step2 composed stuck tick 1 one refusal counted");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s20Step2Sim.World)[2].Activity, "slice20 step2 composed stuck tick 1 one refusal still reads patrolling");
s20Step2Sim.World.Advance();
Eq(22, s20Step2Patrol.X, "slice20 step2 composed stuck tick 2 patrol X");
Eq(2, s20Step2Stuck.PatrolWaypointIndex, "slice20 step2 composed stuck tick 2 stuck waypoint");
Eq(2, s20Step2Stuck.PatrolConsecutiveRefusals, "slice20 step2 composed stuck tick 2 two refusals counted");
s20Step2Sim.World.Advance();
Eq(23, s20Step2Patrol.X, "slice20 step2 composed stuck tick 3 patrol reaches the east waypoint X");
Eq(3, s20Step2Stuck.PatrolWaypointIndex, "slice20 step2 composed stuck tick 3 stuck waypoint");
Eq(3, s20Step2Stuck.PatrolConsecutiveRefusals, "slice20 step2 composed stuck tick 3 three refusals counted");
s20Step2Sim.World.Advance();
Eq(22, s20Step2Patrol.X, "slice20 step2 composed stuck tick 4 patrol turns south X");
Eq(2, s20Step2Patrol.PatrolWaypointIndex, "slice20 step2 composed stuck tick 4 patrol waypoint is south");
Eq(4, s20Step2Stuck.PatrolWaypointIndex, "slice20 step2 composed stuck tick 4 stuck waypoint");
Eq(4, s20Step2Stuck.PatrolConsecutiveRefusals, "slice20 step2 composed stuck tick 4 four refusals counted");
Check(!s20Step2Stuck.IsPatrolStuck, "slice20 step2 composed stuck tick 4 four refusals is not yet stuck");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s20Step2Sim.World)[2].Activity, "slice20 step2 composed stuck tick 4 four refusals still reads patrolling");
s20Step2Sim.World.Advance();
Eq(22, s20Step2Patrol.X, "slice20 step2 composed stuck tick 5 patrol X");
Eq(21, s20Step2Patrol.Z, "slice20 step2 composed stuck tick 5 patrol Z");
Eq(16, s20Step2Stuck.X, "slice20 step2 composed stuck tick 5 stuck X");
Eq(16, s20Step2Stuck.Z, "slice20 step2 composed stuck tick 5 stuck Z");
Eq(0, s20Step2Stuck.PatrolWaypointIndex, "slice20 step2 composed stuck tick 5 stuck waypoint cycles back to 0");
Eq(5, s20Step2Stuck.PatrolConsecutiveRefusals, "slice20 step2 composed stuck tick 5 five refusals counted");
Check(s20Step2Stuck.IsPatrolStuck, "slice20 step2 composed stuck tick 5 five refusals is stuck");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s20Step2Sim.World)[2].Activity, "slice20 step2 composed stuck tick 5 five refusals reads stuck");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s20Step2Sim.World)[0].Activity, "slice20 step2 composed stuck tick 5 hold still reads holding beside it");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s20Step2Sim.World)[1].Activity, "slice20 step2 composed stuck tick 5 moving patrol still reads patrolling beside it");
// Stuck-before-save load copy, mirroring the demo's
// load copy: the stuck patrol reads Patrolling at index
// 0 on arrival and re-sticks after a fresh circuit,
// while the other two keep their readings.
var s20Step2StuckSave = s20Step2Sim.SaveToJson();
var s20Step2StuckLoadedSim = new Simulation(99);
Check(s20Step2StuckLoadedSim.LoadFromJson(s20Step2StuckSave), "slice20 step2 composed stuck-before-save copy loads");
Eq(3, s20Step2StuckLoadedSim.World.Squads.Count, "slice20 step2 composed stuck-before-save copy keeps three squads");
var s20Step2StuckLoaded = s20Step2StuckLoadedSim.World.Squads[2];
Eq(16, s20Step2StuckLoaded.X, "slice20 step2 composed stuck-before-save loaded position preserved X");
Eq(12, s20Step2StuckLoaded.RallyX, "slice20 step2 composed stuck-before-save loaded rally is the old rally");
Eq(0, s20Step2StuckLoaded.PatrolWaypointIndex, "slice20 step2 composed stuck-before-save loaded index is 0");
Eq(0, s20Step2StuckLoaded.PatrolConsecutiveRefusals, "slice20 step2 composed stuck-before-save loaded counter is 0");
Check(!s20Step2StuckLoaded.IsPatrolStuck, "slice20 step2 composed stuck-before-save loaded patrol is not stuck on arrival");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s20Step2StuckLoadedSim.World)[2].Activity, "slice20 step2 composed stuck-before-save loaded patrol reads patrolling");
for (int s20Step2Tick = 0; s20Step2Tick < 5; s20Step2Tick++) s20Step2StuckLoadedSim.World.Advance();
Eq(16, s20Step2StuckLoaded.X, "slice20 step2 composed stuck-before-save loaded patrol stays unmoved after a fresh circuit X");
Eq(16, s20Step2StuckLoaded.Z, "slice20 step2 composed stuck-before-save loaded patrol stays unmoved after a fresh circuit Z");
Check(s20Step2StuckLoaded.IsPatrolStuck, "slice20 step2 composed stuck-before-save loaded patrol is stuck again after a full circuit");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s20Step2StuckLoadedSim.World)[2].Activity, "slice20 step2 composed stuck-before-save loaded patrol reads stuck again");
// Refusals on the main field, then the decided action.
Check(!s20Step2Sim.TryReRallyStuckPatrol(0, 22, 5, 16), "slice20 step2 composed holdground re-rally is refused");
Eq(10, s20Step2Hold.RallyX, "slice20 step2 composed refused holdground keeps its rally X");
Eq(10, s20Step2Hold.RallyZ, "slice20 step2 composed refused holdground keeps its rally Z");
Check(!s20Step2Sim.TryReRallyStuckPatrol(1, 22, 5, 16), "slice20 step2 composed moving patrol re-rally is refused");
Eq(20, s20Step2Patrol.RallyX, "slice20 step2 composed refused moving patrol keeps its rally X");
Eq(20, s20Step2Patrol.RallyZ, "slice20 step2 composed refused moving patrol keeps its rally Z");
Eq(22, s20Step2Patrol.X, "slice20 step2 composed refused moving patrol keeps its position X");
Eq(21, s20Step2Patrol.Z, "slice20 step2 composed refused moving patrol keeps its position Z");
Eq(12, s20Step2Stuck.RallyX, "slice20 step2 composed stuck keeps its old rally before the action");
Check(s20Step2Sim.TryReRallyStuckPatrol(2, 22, 5, 16), "slice20 step2 composed stuck patrol accepts a free re-rally");
Eq(22, s20Step2Stuck.RallyX, "slice20 step2 composed re-rally new rally X");
Eq(16, s20Step2Stuck.RallyZ, "slice20 step2 composed re-rally new rally Z");
Eq(0, s20Step2Stuck.PatrolWaypointIndex, "slice20 step2 composed re-rally waypoint index resets to 0");
Eq(0, s20Step2Stuck.PatrolConsecutiveRefusals, "slice20 step2 composed re-rally refusal counter resets to 0");
Check(!s20Step2Stuck.IsPatrolStuck, "slice20 step2 composed re-rallied patrol is not stuck immediately after the action");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s20Step2Sim.World)[2].Activity, "slice20 step2 composed re-rallied patrol reads patrolling immediately after the action");
Eq(16, s20Step2Stuck.X, "slice20 step2 composed re-rally does not teleport the squad X");
Eq(16, s20Step2Stuck.Z, "slice20 step2 composed re-rally does not teleport the squad Z");
Eq(0, s20Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice20 step2 composed re-rally charges no ore");
Eq(3, s20Step2Sim.World.Squads.Count, "slice20 step2 composed re-rally frees no cap slot");
// Fourth paid deploy at the full cap, still refused.
s20Step2Sim.BeginOrbitalPick();
Check(s20Step2Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.DefendCore), "slice20 step2 composed fourth doctrine selected");
Check(s20Step2Sim.OrbitalCommand.SelectKind(SquadKind.Heavy), "slice20 step2 composed fourth kind selected");
Check(s20Step2Sim.SetOrbitalRally(10, 5, 10), "slice20 step2 composed fourth rally set");
Eq(null, s20Step2Sim.ConfirmOrbitalDeploy(), "slice20 step2 composed fourth deploy is refused at the cap");
s20Step2Sim.CancelOrbitalPick();
Check(!s20Step2Sim.OrbitalCommand.IsPicking, "slice20 step2 composed fourth deploy leaves the picker closed after cancel");
Eq(0, s20Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice20 step2 composed refused fourth deploy charges nothing");
Eq(3, s20Step2Sim.World.Squads.Count, "slice20 step2 composed refused fourth deploy adds no squad");
Eq(22, s20Step2Stuck.RallyX, "slice20 step2 composed re-rallied patrol keeps its new rally after the cap refusal");
// Six-tick march to the new rally, then the turn onto
// the new loop, mirroring the demo tick by tick.
s20Step2Sim.World.Advance();
Eq(17, s20Step2Stuck.X, "slice20 step2 composed re-rally march tick 1 stuck X");
Eq(0, s20Step2Stuck.PatrolWaypointIndex, "slice20 step2 composed re-rally march tick 1 stuck waypoint");
Eq((22, 5, 16), s20Step2Stuck.PatrolTarget, "slice20 step2 composed re-rally march tick 1 stuck target is the new rally");
Eq(21, s20Step2Patrol.X, "slice20 step2 composed re-rally march tick 1 patrol X");
Eq(21, s20Step2Patrol.Z, "slice20 step2 composed re-rally march tick 1 patrol Z");
s20Step2Sim.World.Advance();
Eq(18, s20Step2Stuck.X, "slice20 step2 composed re-rally march tick 2 stuck X");
Eq(21, s20Step2Patrol.X, "slice20 step2 composed re-rally march tick 2 patrol X");
Eq(22, s20Step2Patrol.Z, "slice20 step2 composed re-rally march tick 2 patrol Z");
s20Step2Sim.World.Advance();
Eq(19, s20Step2Stuck.X, "slice20 step2 composed re-rally march tick 3 stuck X");
Eq(20, s20Step2Patrol.X, "slice20 step2 composed re-rally march tick 3 patrol X");
Eq(22, s20Step2Patrol.Z, "slice20 step2 composed re-rally march tick 3 patrol Z");
s20Step2Sim.World.Advance();
Eq(20, s20Step2Stuck.X, "slice20 step2 composed re-rally march tick 4 stuck X");
Eq(20, s20Step2Patrol.X, "slice20 step2 composed re-rally march tick 4 patrol X");
Eq(23, s20Step2Patrol.Z, "slice20 step2 composed re-rally march tick 4 patrol Z");
s20Step2Sim.World.Advance();
Eq(21, s20Step2Stuck.X, "slice20 step2 composed re-rally march tick 5 stuck X");
Eq(19, s20Step2Patrol.X, "slice20 step2 composed re-rally march tick 5 patrol X");
Eq(23, s20Step2Patrol.Z, "slice20 step2 composed re-rally march tick 5 patrol Z");
Eq(3, s20Step2Patrol.PatrolWaypointIndex, "slice20 step2 composed re-rally march tick 5 patrol waypoint is west");
s20Step2Sim.World.Advance();
Eq(22, s20Step2Stuck.X, "slice20 step2 composed re-rally march tick 6 stuck reaches the new rally X");
Eq(16, s20Step2Stuck.Z, "slice20 step2 composed re-rally march tick 6 stuck reaches the new rally Z");
Eq(0, s20Step2Stuck.PatrolWaypointIndex, "slice20 step2 composed re-rally march tick 6 stuck waypoint at the new rally");
Eq(19, s20Step2Patrol.X, "slice20 step2 composed re-rally march tick 6 patrol X");
Eq(22, s20Step2Patrol.Z, "slice20 step2 composed re-rally march tick 6 patrol Z");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s20Step2Sim.World)[0].Activity, "slice20 step2 composed re-rally march tick 6 hold still reads holding");
s20Step2Sim.World.Advance();
Eq(23, s20Step2Stuck.X, "slice20 step2 composed re-rally loop tick stuck steps east onto the new loop");
Eq(1, s20Step2Stuck.PatrolWaypointIndex, "slice20 step2 composed re-rally loop tick stuck waypoint is east");
Eq((25, 5, 16), s20Step2Stuck.PatrolTarget, "slice20 step2 composed re-rally east waypoint is the new rally plus radius 3");
Eq(3, TacticalSquad.PatrolLoopRadius, "slice20 step2 composed loop radius is still 3, not retuned");
Eq(5, TacticalSquad.PatrolWaypointCount, "slice20 step2 composed loop waypoint count is still 5, not retuned");
// Save/load round-trip after the action: the new rally
// loads, indices reset to 0, and the loaded re-rallied
// patrol steps back toward its new rally on tick 1,
// because index 0 targets the rally itself.
var s20Step2Save = s20Step2Sim.SaveToJson();
var s20Step2LoadedSim = new Simulation(99);
Check(s20Step2LoadedSim.LoadFromJson(s20Step2Save), "slice20 step2 composed save loads");
Eq(3, s20Step2LoadedSim.World.Squads.Count, "slice20 step2 composed save keeps three squads");
var s20Step2Loaded = s20Step2LoadedSim.World.Squads[2];
Eq(23, s20Step2Loaded.X, "slice20 step2 composed loaded re-rallied position preserved X");
Eq(22, s20Step2Loaded.RallyX, "slice20 step2 composed loaded rally is the new rally, not the old one");
Eq(16, s20Step2Loaded.RallyZ, "slice20 step2 composed loaded rally Z");
Eq(0, s20Step2Loaded.PatrolWaypointIndex, "slice20 step2 composed loaded index is 0");
Eq(0, s20Step2Loaded.PatrolConsecutiveRefusals, "slice20 step2 composed loaded counter is 0");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s20Step2LoadedSim.World)[2].Activity, "slice20 step2 composed loaded re-rallied patrol reads patrolling");
Eq(10, s20Step2LoadedSim.World.Squads[0].X, "slice20 step2 composed loaded hold position preserved X");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s20Step2LoadedSim.World)[0].Activity, "slice20 step2 composed loaded hold reads holding");
Eq(18, s20Step2LoadedSim.World.Squads[1].X, "slice20 step2 composed loaded patrol position preserved X");
Eq(22, s20Step2LoadedSim.World.Squads[1].Z, "slice20 step2 composed loaded patrol position preserved Z");
s20Step2LoadedSim.World.Advance();
Eq(22, s20Step2Loaded.X, "slice20 step2 composed loaded tick 1 re-rallied steps back to its new rally X");
Eq(0, s20Step2Loaded.PatrolWaypointIndex, "slice20 step2 composed loaded tick 1 re-rallied waypoint is the rally");
Eq(19, s20Step2LoadedSim.World.Squads[1].X, "slice20 step2 composed loaded tick 1 patrol X");
Eq(22, s20Step2LoadedSim.World.Squads[1].Z, "slice20 step2 composed loaded tick 1 patrol Z");
Eq(0, s20Step2LoadedSim.World.Squads[1].PatrolWaypointIndex, "slice20 step2 composed loaded tick 1 patrol waypoint is its rally");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s20Step2LoadedSim.World)[0].Activity, "slice20 step2 composed loaded tick 1 hold still reads holding");

// ---- Slice 21 Step 1: general re-rally decision, decided here (0.88.0) ----
// Decision, in one sentence: a FREE general re-rally
// for any live HoldGround or Patrol squad, stuck or
// not — and a fail-closed refusal for Advance and
// Defend Core, whose rally moves nothing (see
// DECISIONS.md for the rejected hypotheses: a priced
// re-rally, an all-doctrine scope with a field-only
// change for the inert doctrines, and merging the two
// gates). A stuck Patrol succeeds under both gates
// with the identical outcome, so Slice 20 is subsumed,
// not repriced and not revised, and its proof set
// above still passes unchanged.
// HoldGround, Vanguard, in full: a marching squad
// re-rallied elsewhere marches to the new rally and
// reads Holding on arrival, with no teleport.
var s21HoldWorld = new WorldZone(42);
s21HoldWorld.Player.X = 16; s21HoldWorld.Player.Y = 5; s21HoldWorld.Player.Z = 16;
var s21Hold = s21HoldWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s21HoldWorld)[0].Activity, "slice21 general holdground reads marching before the action");
Check(s21HoldWorld.TryReRallySquad(0, 20, 5, 20), "slice21 general holdground accepts a re-rally");
Eq(20, s21Hold.RallyX, "slice21 general holdground new rally X");
Eq(20, s21Hold.RallyZ, "slice21 general holdground new rally Z");
Eq(16, s21Hold.X, "slice21 general holdground does not teleport to the new rally X");
Eq(16, s21Hold.Z, "slice21 general holdground does not teleport to the new rally Z");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s21HoldWorld)[0].Activity, "slice21 general holdground reads marching immediately after the action");
Check(s21Hold.MoveTowardRally(s21HoldWorld.Chunk, Array.Empty<Enemy>()), "slice21 general holdground first march step moves");
Eq(17, s21Hold.X, "slice21 general holdground first march step is east on the X tie-break");
Eq(16, s21Hold.Z, "slice21 general holdground first march step keeps Z");
for (int s21Tick = 0; s21Tick < 30 && !s21Hold.AtRally; s21Tick++) s21Hold.MoveTowardRally(s21HoldWorld.Chunk, Array.Empty<Enemy>());
Check(s21Hold.AtRally, "slice21 general holdground reaches its new rally");
Eq(20, s21Hold.X, "slice21 general holdground arrives at the new rally X");
Eq(20, s21Hold.Z, "slice21 general holdground arrives at the new rally Z");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s21HoldWorld)[0].Activity, "slice21 general holdground reads holding at its new rally");
Eq(3, TacticalSquad.PatrolLoopRadius, "slice21 general loop radius is still 3, not retuned");
Eq(5, TacticalSquad.PatrolWaypointCount, "slice21 general loop waypoint count is still 5, not retuned");
// HoldGround fire, suspended while marching and
// resuming once holding, under the existing rule
// (FindTarget returns null for a marching HoldGround
// squad, whatever is in range): an enemy in range
// while marching is not shot, and an enemy in range
// once holding is.
var s21HoldFireWorld = new WorldZone(42);
s21HoldFireWorld.Player.X = 16; s21HoldFireWorld.Player.Y = 5; s21HoldFireWorld.Player.Z = 16;
var s21HoldFire = s21HoldFireWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
var s21HoldFireNear = new Enemy(17, 5, 17);
var s21HoldFireFar = new Enemy(22, 5, 18);
s21HoldFireWorld.Enemies.Add(s21HoldFireNear);
s21HoldFireWorld.Enemies.Add(s21HoldFireFar);
Check(s21HoldFireWorld.TryReRallySquad(0, 22, 5, 16), "slice21 general holdground fire test accepts a re-rally");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s21HoldFireWorld)[0].Activity, "slice21 general holdground fire test reads marching after the action");
Check(s21HoldFire.FindTarget(s21HoldFireWorld.Enemies) == null, "slice21 general holdground marching squad does not fire at an in-range enemy");
for (int s21Tick = 0; s21Tick < 30 && !s21HoldFire.AtRally; s21Tick++) s21HoldFire.MoveTowardRally(s21HoldFireWorld.Chunk, Array.Empty<Enemy>());
Check(s21HoldFire.AtRally, "slice21 general holdground fire test reaches its new rally");
Eq(22, s21HoldFire.X, "slice21 general holdground fire test arrives at the new rally X");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s21HoldFireWorld)[0].Activity, "slice21 general holdground fire test reads holding at its new rally");
Check(s21HoldFire.FindTarget(s21HoldFireWorld.Enemies) == s21HoldFireFar, "slice21 general holdground holding squad fires at the in-range enemy by its new rally");
Eq(30, s21HoldFire.DamagePerTick, "slice21 general holdground vanguard damage per tick unchanged");
// HoldGround re-rallied to the cell it already
// stands on reads Holding at once, with no march.
var s21HoldOwnWorld = new WorldZone(42);
s21HoldOwnWorld.Player.X = 16; s21HoldOwnWorld.Player.Y = 5; s21HoldOwnWorld.Player.Z = 16;
var s21HoldOwn = s21HoldOwnWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
Check(s21HoldOwnWorld.TryReRallySquad(0, 16, 5, 16), "slice21 general holdground accepts a re-rally to its own cell");
Check(s21HoldOwn.AtRally, "slice21 general holdground re-rallied to its own cell is at its rally");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s21HoldOwnWorld)[0].Activity, "slice21 general holdground re-rallied to its own cell reads holding at once");
Eq(16, s21HoldOwn.X, "slice21 general holdground re-rallied to its own cell does not move");
// HoldGround Recon and Heavy: the action is doctrine
// logic, not a kind stat — but kinds differ in damage
// and range, so all three are proved for the core march.
var s21HoldReconWorld = new WorldZone(42);
s21HoldReconWorld.Player.X = 16; s21HoldReconWorld.Player.Y = 5; s21HoldReconWorld.Player.Z = 16;
var s21HoldRecon = s21HoldReconWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.HoldGround, SquadKind.Recon)!;
Check(s21HoldReconWorld.TryReRallySquad(0, 20, 5, 20), "slice21 general holdground recon accepts a re-rally");
for (int s21Tick = 0; s21Tick < 30 && !s21HoldRecon.AtRally; s21Tick++) s21HoldRecon.MoveTowardRally(s21HoldReconWorld.Chunk, Array.Empty<Enemy>());
Check(s21HoldRecon.AtRally, "slice21 general holdground recon reaches its new rally");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s21HoldReconWorld)[0].Activity, "slice21 general holdground recon reads holding at its new rally");
Eq(12, s21HoldRecon.DamagePerTick, "slice21 general holdground recon damage per tick unchanged");
var s21HoldHeavyWorld = new WorldZone(42);
s21HoldHeavyWorld.Player.X = 16; s21HoldHeavyWorld.Player.Y = 5; s21HoldHeavyWorld.Player.Z = 16;
var s21HoldHeavy = s21HoldHeavyWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.HoldGround, SquadKind.Heavy)!;
Check(s21HoldHeavyWorld.TryReRallySquad(0, 20, 5, 20), "slice21 general holdground heavy accepts a re-rally");
for (int s21Tick = 0; s21Tick < 30 && !s21HoldHeavy.AtRally; s21Tick++) s21HoldHeavy.MoveTowardRally(s21HoldHeavyWorld.Chunk, Array.Empty<Enemy>());
Check(s21HoldHeavy.AtRally, "slice21 general holdground heavy reaches its new rally");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s21HoldHeavyWorld)[0].Activity, "slice21 general holdground heavy reads holding at its new rally");
Eq(16, s21HoldHeavy.DamagePerTick, "slice21 general holdground heavy damage per tick unchanged");
// HoldGround re-rallied to a solid rally cell is
// accepted, as deploy accepts one, and holds short
// of it instead of stepping inside it.
var s21HoldSolidWorld = new WorldZone(42);
s21HoldSolidWorld.Player.X = 16; s21HoldSolidWorld.Player.Y = 5; s21HoldSolidWorld.Player.Z = 16;
var s21HoldSolid = s21HoldSolidWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
Check(s21HoldSolidWorld.Chunk.TrySet(20, 5, 16, BlockType.Stone), "slice21 general holdground solid test stages a solid new rally cell");
Check(s21HoldSolidWorld.TryReRallySquad(0, 20, 5, 16), "slice21 general holdground to a solid rally cell is accepted, as deploy accepts one");
Eq(20, s21HoldSolid.RallyX, "slice21 general holdground solid new rally X");
for (int s21Tick = 0; s21Tick < 4; s21Tick++) s21HoldSolid.MoveTowardRally(s21HoldSolidWorld.Chunk, Array.Empty<Enemy>());
Eq(19, s21HoldSolid.X, "slice21 general holdground solid patrol reaches the open cell before the rally");
Check(!s21HoldSolid.MoveTowardRally(s21HoldSolidWorld.Chunk, Array.Empty<Enemy>()), "slice21 general holdground solid step into the rally is refused");
Eq(19, s21HoldSolid.X, "slice21 general holdground solid squad holds short of the solid rally, never inside it");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s21HoldSolidWorld)[0].Activity, "slice21 general holdground solid squad still reads marching short of its rally");
Eq(BlockType.Stone, s21HoldSolidWorld.Chunk.Get(20, 5, 16), "slice21 general holdground solid rally cell stays stone");
// Patrol, Vanguard, mid-loop: a non-stuck patrol
// re-rallied mid-circuit moves its loop, marches to
// the new rally, and turns onto the new loop at
// radius 3 with five waypoints.
var s21PatrolWorld = new WorldZone(42);
s21PatrolWorld.Player.X = 16; s21PatrolWorld.Player.Y = 5; s21PatrolWorld.Player.Z = 16;
var s21Patrol = s21PatrolWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(s21Patrol.MoveForPatrol(s21PatrolWorld.Chunk, Array.Empty<Enemy>()), "slice21 general patrol mid-loop first step moves");
Check(s21Patrol.MoveForPatrol(s21PatrolWorld.Chunk, Array.Empty<Enemy>()), "slice21 general patrol mid-loop second step moves");
Eq(17, s21Patrol.X, "slice21 general patrol mid-loop position X before the action");
Eq(17, s21Patrol.Z, "slice21 general patrol mid-loop position Z before the action");
Eq(0, s21Patrol.PatrolWaypointIndex, "slice21 general patrol mid-loop index is still 0 before the action");
Eq(0, s21Patrol.PatrolConsecutiveRefusals, "slice21 general patrol mid-loop counter is 0 before the action");
Check(s21PatrolWorld.TryReRallySquad(0, 26, 5, 20), "slice21 general non-stuck patrol accepts a re-rally");
Eq(26, s21Patrol.RallyX, "slice21 general patrol new rally X");
Eq(0, s21Patrol.PatrolWaypointIndex, "slice21 general patrol waypoint index resets to 0");
Eq(0, s21Patrol.PatrolConsecutiveRefusals, "slice21 general patrol refusal counter resets to 0");
Eq((26, 5, 20), s21Patrol.PatrolTarget, "slice21 general patrol loop re-derives from the new rally, waypoint 0");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s21PatrolWorld)[0].Activity, "slice21 general patrol reads patrolling immediately after the action");
Eq(17, s21Patrol.X, "slice21 general patrol does not teleport to the new rally");
for (int s21Tick = 0; s21Tick < 40 && !(s21Patrol.X == 26 && s21Patrol.Z == 20); s21Tick++) s21Patrol.MoveForPatrol(s21PatrolWorld.Chunk, Array.Empty<Enemy>());
Eq(26, s21Patrol.X, "slice21 general patrol marches to its new rally X");
Eq(20, s21Patrol.Z, "slice21 general patrol marches to its new rally Z");
Eq(0, s21Patrol.PatrolWaypointIndex, "slice21 general patrol index is still 0 on arrival at the new rally");
Check(s21Patrol.MoveForPatrol(s21PatrolWorld.Chunk, Array.Empty<Enemy>()), "slice21 general patrol turns onto the new loop");
Eq(27, s21Patrol.X, "slice21 general patrol steps east onto the new loop");
Eq(1, s21Patrol.PatrolWaypointIndex, "slice21 general patrol index advances to the east waypoint");
Eq((29, 5, 20), s21Patrol.PatrolTarget, "slice21 general patrol east waypoint is the new rally plus radius 3");
// Patrol with a counter above 0 but below 5 — the
// case that separates reset from carry-over: two
// refused waypoints, then a re-rally, must clear the
// counter (the loop is new) rather than carry two
// refusals onto a loop that refused nothing. The
// same-rally refusal first proves a refusal changes
// nothing, including the counter.
var s21CounterWorld = new WorldZone(42);
s21CounterWorld.Player.X = 20; s21CounterWorld.Player.Y = 5; s21CounterWorld.Player.Z = 20;
var s21Counter = s21CounterWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(s21CounterWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice21 general counter test stages an east wall");
Check(s21CounterWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice21 general counter test stages a south wall");
Check(!s21Counter.MoveForPatrol(s21CounterWorld.Chunk, Array.Empty<Enemy>()), "slice21 general counter test first refused waypoint does not move");
Eq(1, s21Counter.PatrolConsecutiveRefusals, "slice21 general counter test one refusal counted");
Eq(2, s21Counter.PatrolWaypointIndex, "slice21 general counter test index advances past the refused east waypoint");
Check(!s21Counter.MoveForPatrol(s21CounterWorld.Chunk, Array.Empty<Enemy>()), "slice21 general counter test second refused waypoint does not move");
Eq(2, s21Counter.PatrolConsecutiveRefusals, "slice21 general counter test two refusals counted");
Eq(3, s21Counter.PatrolWaypointIndex, "slice21 general counter test index advances past the refused south waypoint");
Check(!s21Counter.IsPatrolStuck, "slice21 general counter test two refusals is not stuck");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s21CounterWorld)[0].Activity, "slice21 general counter test two refusals still reads patrolling");
Check(!s21CounterWorld.TryReRallySquad(0, 20, 5, 20), "slice21 general counter test same-rally re-rally is refused");
Eq(2, s21Counter.PatrolConsecutiveRefusals, "slice21 general counter test refused same-rally keeps the counter at 2");
Eq(3, s21Counter.PatrolWaypointIndex, "slice21 general counter test refused same-rally keeps the index at 3");
Check(s21CounterWorld.TryReRallySquad(0, 14, 5, 20), "slice21 general counter test accepts a re-rally west");
Eq(14, s21Counter.RallyX, "slice21 general counter test new rally X");
Eq(0, s21Counter.PatrolWaypointIndex, "slice21 general counter test re-rally resets the index to 0");
Eq(0, s21Counter.PatrolConsecutiveRefusals, "slice21 general counter test re-rally resets a counter above 0 to 0, not carried over");
Eq((14, 5, 20), s21Counter.PatrolTarget, "slice21 general counter test loop re-derives from the new rally, waypoint 0");
for (int s21Tick = 0; s21Tick < 40 && !(s21Counter.X == 14 && s21Counter.Z == 20); s21Tick++) s21Counter.MoveForPatrol(s21CounterWorld.Chunk, Array.Empty<Enemy>());
Eq(14, s21Counter.X, "slice21 general counter test patrol marches west to its new rally");
Eq(0, s21Counter.PatrolWaypointIndex, "slice21 general counter test index is still 0 on arrival at the new rally");
Eq(0, s21Counter.PatrolConsecutiveRefusals, "slice21 general counter test counter stays 0 through the march");
Check(s21Counter.MoveForPatrol(s21CounterWorld.Chunk, Array.Empty<Enemy>()), "slice21 general counter test patrol turns onto the new loop");
Eq(15, s21Counter.X, "slice21 general counter test patrol steps east onto the new loop");
Eq((17, 5, 20), s21Counter.PatrolTarget, "slice21 general counter test east waypoint is the new rally plus radius 3");
// Patrol Recon and Heavy: core march plus fire in
// passing, as in Slice 20, for the general action.
var s21PatrolReconWorld = new WorldZone(42);
s21PatrolReconWorld.Player.X = 16; s21PatrolReconWorld.Player.Y = 5; s21PatrolReconWorld.Player.Z = 16;
var s21PatrolRecon = s21PatrolReconWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Recon)!;
Check(s21PatrolReconWorld.TryReRallySquad(0, 26, 5, 20), "slice21 general patrol recon accepts a re-rally");
for (int s21Tick = 0; s21Tick < 40 && !(s21PatrolRecon.X == 26 && s21PatrolRecon.Z == 20); s21Tick++) s21PatrolRecon.MoveForPatrol(s21PatrolReconWorld.Chunk, Array.Empty<Enemy>());
Eq(26, s21PatrolRecon.X, "slice21 general patrol recon marches to its new rally");
Eq(0, s21PatrolRecon.PatrolWaypointIndex, "slice21 general patrol recon index is 0 on arrival");
Check(s21PatrolRecon.MoveForPatrol(s21PatrolReconWorld.Chunk, Array.Empty<Enemy>()), "slice21 general patrol recon turns onto the new loop");
Eq((29, 5, 20), s21PatrolRecon.PatrolTarget, "slice21 general patrol recon east waypoint is the new rally plus radius 3");
Eq(12, s21PatrolRecon.DamagePerTick, "slice21 general patrol recon damage per tick unchanged");
Check(s21PatrolRecon.FindTarget(new[] { new Enemy(29, 5, 20) }) != null, "slice21 general patrol recon still fires in passing after the action");
var s21PatrolHeavyWorld = new WorldZone(42);
s21PatrolHeavyWorld.Player.X = 16; s21PatrolHeavyWorld.Player.Y = 5; s21PatrolHeavyWorld.Player.Z = 16;
var s21PatrolHeavy = s21PatrolHeavyWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Heavy)!;
Check(s21PatrolHeavyWorld.TryReRallySquad(0, 26, 5, 20), "slice21 general patrol heavy accepts a re-rally");
for (int s21Tick = 0; s21Tick < 40 && !(s21PatrolHeavy.X == 26 && s21PatrolHeavy.Z == 20); s21Tick++) s21PatrolHeavy.MoveForPatrol(s21PatrolHeavyWorld.Chunk, Array.Empty<Enemy>());
Eq(26, s21PatrolHeavy.X, "slice21 general patrol heavy marches to its new rally");
Check(s21PatrolHeavy.MoveForPatrol(s21PatrolHeavyWorld.Chunk, Array.Empty<Enemy>()), "slice21 general patrol heavy turns onto the new loop");
Eq(16, s21PatrolHeavy.DamagePerTick, "slice21 general patrol heavy damage per tick unchanged");
Check(s21PatrolHeavy.FindTarget(new[] { new Enemy(28, 5, 20) }) != null, "slice21 general patrol heavy still fires in passing after the action");
// Patrol re-rallied to a solid rally cell is accepted,
// holds short of it, and skips that waypoint on the
// new loop instead of stepping inside the rally cell.
var s21PatrolSolidWorld = new WorldZone(42);
s21PatrolSolidWorld.Player.X = 16; s21PatrolSolidWorld.Player.Y = 5; s21PatrolSolidWorld.Player.Z = 16;
var s21PatrolSolid = s21PatrolSolidWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(s21PatrolSolidWorld.Chunk.TrySet(20, 5, 16, BlockType.Stone), "slice21 general patrol solid test stages a solid new rally cell");
Check(s21PatrolSolidWorld.TryReRallySquad(0, 20, 5, 16), "slice21 general patrol to a solid rally cell is accepted, as deploy accepts one");
Eq(20, s21PatrolSolid.RallyX, "slice21 general patrol solid new rally X");
for (int s21Tick = 0; s21Tick < 3; s21Tick++) s21PatrolSolid.MoveForPatrol(s21PatrolSolidWorld.Chunk, Array.Empty<Enemy>());
Eq(19, s21PatrolSolid.X, "slice21 general patrol solid squad reaches the open cell before the rally");
Check(!s21PatrolSolid.MoveForPatrol(s21PatrolSolidWorld.Chunk, Array.Empty<Enemy>()), "slice21 general patrol solid step into the rally is refused");
Eq(19, s21PatrolSolid.X, "slice21 general patrol solid squad holds short of the solid rally, never inside it");
Eq(1, s21PatrolSolid.PatrolConsecutiveRefusals, "slice21 general patrol solid refusal counts once");
Eq(1, s21PatrolSolid.PatrolWaypointIndex, "slice21 general patrol solid squad skips to the east waypoint");
Eq((23, 5, 16), s21PatrolSolid.PatrolTarget, "slice21 general patrol solid east waypoint is the new rally plus radius 3");
Eq(BlockType.Stone, s21PatrolSolidWorld.Chunk.Get(20, 5, 16), "slice21 general patrol solid rally cell stays stone");
// A non-stuck patrol re-rallied to a loop it cannot
// walk reads Stuck again after a full circuit on the
// new loop, unmoved, with the new rally kept — the
// general action corrects the loop, it does not march
// a squad through a wall, exactly as Slice 20 proved
// for the stuck case.
var s21RestickWorld = new WorldZone(42);
s21RestickWorld.Player.X = 20; s21RestickWorld.Player.Y = 5; s21RestickWorld.Player.Z = 20;
var s21Restick = s21RestickWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(s21RestickWorld.TryReRallySquad(0, 26, 5, 20), "slice21 general restick test non-stuck patrol accepts a re-rally");
Check(s21RestickWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice21 general restick test stages an east wall");
Check(s21RestickWorld.Chunk.TrySet(19, 5, 20, BlockType.Stone), "slice21 general restick test stages a west wall");
Check(s21RestickWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice21 general restick test stages a south wall");
Check(s21RestickWorld.Chunk.TrySet(20, 5, 19, BlockType.Stone), "slice21 general restick test stages a north wall");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s21RestickWorld)[0].Activity, "slice21 general restick test reads patrolling at once on the new loop");
for (int s21Tick = 0; s21Tick < 5; s21Tick++) s21Restick.MoveForPatrol(s21RestickWorld.Chunk, Array.Empty<Enemy>());
Check(s21Restick.IsPatrolStuck, "slice21 general restick test reads stuck again after a full circuit on the new loop");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s21RestickWorld)[0].Activity, "slice21 general restick test snapshot reads stuck again");
Eq(20, s21Restick.X, "slice21 general restick test patrol never moved through a wall");
Eq(26, s21Restick.RallyX, "slice21 general restick test patrol keeps its new rally while stuck again");
// The inert doctrines, all three kinds each: every
// refusal returns false and changes nothing — rally,
// position, and health all stand.
var s21InertWorld = new WorldZone(42);
s21InertWorld.Player.X = 16; s21InertWorld.Player.Y = 5; s21InertWorld.Player.Z = 16;
var s21InertAdvVan = s21InertWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.Advance, SquadKind.Vanguard)!;
var s21InertAdvRec = s21InertWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.Advance, SquadKind.Recon)!;
var s21InertAdvHea = s21InertWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.Advance, SquadKind.Heavy)!;
var s21InertDefVan = s21InertWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.DefendCore, SquadKind.Vanguard)!;
var s21InertDefRec = s21InertWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.DefendCore, SquadKind.Recon)!;
var s21InertDefHea = s21InertWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.DefendCore, SquadKind.Heavy)!;
Check(!s21InertWorld.TryReRallySquad(0, 20, 5, 20), "slice21 general advance vanguard is refused");
Eq(10, s21InertAdvVan.RallyX, "slice21 general refused advance vanguard keeps its rally");
Eq(16, s21InertAdvVan.X, "slice21 general refused advance vanguard keeps its position");
Eq(90, s21InertAdvVan.Health, "slice21 general refused advance vanguard keeps its health");
Check(!s21InertWorld.TryReRallySquad(1, 20, 5, 20), "slice21 general advance recon is refused");
Eq(10, s21InertAdvRec.RallyX, "slice21 general refused advance recon keeps its rally");
Eq(40, s21InertAdvRec.Health, "slice21 general refused advance recon keeps its health");
Check(!s21InertWorld.TryReRallySquad(2, 20, 5, 20), "slice21 general advance heavy is refused");
Eq(10, s21InertAdvHea.RallyX, "slice21 general refused advance heavy keeps its rally");
Eq(100, s21InertAdvHea.Health, "slice21 general refused advance heavy keeps its health");
Check(!s21InertWorld.TryReRallySquad(3, 20, 5, 20), "slice21 general defendcore vanguard is refused");
Eq(10, s21InertDefVan.RallyX, "slice21 general refused defendcore vanguard keeps its rally");
Eq(90, s21InertDefVan.Health, "slice21 general refused defendcore vanguard keeps its health");
Check(!s21InertWorld.TryReRallySquad(4, 20, 5, 20), "slice21 general defendcore recon is refused");
Eq(10, s21InertDefRec.RallyX, "slice21 general refused defendcore recon keeps its rally");
Eq(40, s21InertDefRec.Health, "slice21 general refused defendcore recon keeps its health");
Check(!s21InertWorld.TryReRallySquad(5, 20, 5, 20), "slice21 general defendcore heavy is refused");
Eq(10, s21InertDefHea.RallyX, "slice21 general refused defendcore heavy keeps its rally");
Eq(100, s21InertDefHea.Health, "slice21 general refused defendcore heavy keeps its health");
// A refused Advance squad still chases the same enemy,
// and a refused Advance squad with no enemy still
// reads Idle and does not move — the refusal changes
// no movement, chase, or fire behaviour.
var s21AdvWorld = new WorldZone(42);
s21AdvWorld.Player.X = 16; s21AdvWorld.Player.Y = 5; s21AdvWorld.Player.Z = 16;
var s21Adv = s21AdvWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.Advance, SquadKind.Vanguard)!;
var s21AdvEnemy = new Enemy(20, 5, 16);
s21AdvWorld.Enemies.Add(s21AdvEnemy);
Eq(SquadActivity.Hunting, OrbitalView.Snapshot(s21AdvWorld)[0].Activity, "slice21 general advance with an enemy reads hunting before the action");
Check(OrbitalView.Snapshot(s21AdvWorld)[0].HasHuntingTarget, "slice21 general advance has a hunting target before the action");
Eq(20, OrbitalView.Snapshot(s21AdvWorld)[0].HuntingTargetX, "slice21 general advance hunting target X before the action");
Check(!s21AdvWorld.TryReRallySquad(0, 22, 5, 16), "slice21 general advance with an enemy is refused");
Eq(10, s21Adv.RallyX, "slice21 general refused advance keeps its rally");
Eq(SquadActivity.Hunting, OrbitalView.Snapshot(s21AdvWorld)[0].Activity, "slice21 general refused advance still reads hunting");
Eq(20, OrbitalView.Snapshot(s21AdvWorld)[0].HuntingTargetX, "slice21 general refused advance keeps its hunting target");
Check(s21Adv.MoveTowardNearestEnemy(s21AdvWorld.Chunk, s21AdvWorld.Enemies), "slice21 general refused advance still chases its enemy");
Eq(17, s21Adv.X, "slice21 general refused advance steps toward its enemy");
Eq(10, s21Adv.RallyX, "slice21 general refused advance keeps its rally after chasing");
var s21AdvIdleWorld = new WorldZone(42);
s21AdvIdleWorld.Player.X = 16; s21AdvIdleWorld.Player.Y = 5; s21AdvIdleWorld.Player.Z = 16;
var s21AdvIdle = s21AdvIdleWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.Advance, SquadKind.Recon)!;
Eq(SquadActivity.Idle, OrbitalView.Snapshot(s21AdvIdleWorld)[0].Activity, "slice21 general advance with no enemy reads idle before the action");
Check(!s21AdvIdleWorld.TryReRallySquad(0, 22, 5, 16), "slice21 general advance with no enemy is refused");
Eq(SquadActivity.Idle, OrbitalView.Snapshot(s21AdvIdleWorld)[0].Activity, "slice21 general refused advance with no enemy still reads idle");
Check(!s21AdvIdle.MoveTowardNearestEnemy(s21AdvIdleWorld.Chunk, s21AdvIdleWorld.Enemies), "slice21 general refused advance with no enemy still does not move");
Eq(16, s21AdvIdle.X, "slice21 general refused advance with no enemy keeps its position");
// A refused Defend Core squad still chases the same
// core attacker, and a refused guard at its post with
// no attacker still reads Guarding and does not move.
var s21DefWorld = new WorldZone(42);
s21DefWorld.Player.X = 26; s21DefWorld.Player.Y = 5; s21DefWorld.Player.Z = 28;
var s21Def = s21DefWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.DefendCore, SquadKind.Vanguard)!;
var s21DefAttacker = new Enemy(24, 5, 28);
s21DefWorld.Enemies.Add(s21DefAttacker);
Eq(SquadActivity.Hunting, OrbitalView.Snapshot(s21DefWorld)[0].Activity, "slice21 general defendcore with an attacker reads hunting before the action");
Check(!s21DefWorld.TryReRallySquad(0, 20, 5, 28), "slice21 general defendcore with an attacker is refused");
Eq(10, s21Def.RallyX, "slice21 general refused defendcore keeps its rally");
Eq(SquadActivity.Hunting, OrbitalView.Snapshot(s21DefWorld)[0].Activity, "slice21 general refused defendcore still reads hunting");
Check(s21Def.MoveForDefendCore(s21DefWorld.Chunk, s21DefWorld.Enemies, 28, 5, 28), "slice21 general refused defendcore still chases its attacker");
Eq(25, s21Def.X, "slice21 general refused defendcore steps toward its attacker");
Eq(10, s21Def.RallyX, "slice21 general refused defendcore keeps its rally after chasing");
var s21DefPostWorld = new WorldZone(42);
s21DefPostWorld.Player.X = 28; s21DefPostWorld.Player.Y = 5; s21DefPostWorld.Player.Z = 28;
var s21DefPost = s21DefPostWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.DefendCore, SquadKind.Heavy)!;
Eq(SquadActivity.Guarding, OrbitalView.Snapshot(s21DefPostWorld)[0].Activity, "slice21 general defendcore at its post reads guarding before the action");
Check(!s21DefPostWorld.TryReRallySquad(0, 20, 5, 28), "slice21 general defendcore at its post is refused");
Eq(SquadActivity.Guarding, OrbitalView.Snapshot(s21DefPostWorld)[0].Activity, "slice21 general refused defendcore at its post still reads guarding");
Check(!s21DefPost.MoveForDefendCore(s21DefPostWorld.Chunk, s21DefPostWorld.Enemies, 28, 5, 28), "slice21 general refused defendcore at its post still does not move");
Eq(28, s21DefPost.X, "slice21 general refused defendcore at its post keeps its position");
// The two gates agree about identity: the same index
// names the same list entry under both gates, in a
// world where two squads share one cell (the deploy-
// at-the-player's-feet ambiguity). Eligibility differs
// by design — the stuck gate refuses the HoldGround
// squad, the general gate accepts it — but neither
// gate ever names the wrong squad, and a stuck patrol
// succeeds under the stuck gate and then again under
// the general gate, now as an ordinary Patrol.
var s21GatesWorld = new WorldZone(42);
s21GatesWorld.Player.X = 24; s21GatesWorld.Player.Y = 5; s21GatesWorld.Player.Z = 20;
var s21GatesPatrol = s21GatesWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
var s21GatesHold = s21GatesWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
Check(s21GatesWorld.Chunk.TrySet(23, 5, 20, BlockType.Stone), "slice21 general gates test stages a west wall");
Check(s21GatesWorld.Chunk.TrySet(24, 5, 21, BlockType.Stone), "slice21 general gates test stages a south wall");
Check(s21GatesWorld.Chunk.TrySet(24, 5, 19, BlockType.Stone), "slice21 general gates test stages a north wall");
for (int s21Tick = 0; s21Tick < 5; s21Tick++) s21GatesPatrol.MoveForPatrol(s21GatesWorld.Chunk, Array.Empty<Enemy>());
Check(s21GatesPatrol.IsPatrolStuck, "slice21 general gates test patrol is stuck before the actions");
Check(!s21GatesWorld.TryReRallyStuckPatrol(1, 26, 5, 20), "slice21 general gates test stuck gate refuses the holdground squad at index 1");
Eq(10, s21GatesHold.RallyX, "slice21 general gates test stuck-gate refusal keeps the holdground rally");
Check(s21GatesWorld.TryReRallySquad(1, 26, 5, 20), "slice21 general gates test general gate accepts the same holdground squad at index 1");
Eq(26, s21GatesHold.RallyX, "slice21 general gates test general gate moves the holdground rally at index 1");
Eq(20, s21GatesPatrol.RallyX, "slice21 general gates test holdground re-rally leaves the patrol rally at index 0 untouched");
Check(s21GatesWorld.TryReRallyStuckPatrol(0, 30, 5, 20), "slice21 general gates test stuck gate accepts the stuck patrol at index 0");
Eq(30, s21GatesPatrol.RallyX, "slice21 general gates test stuck gate moves the patrol rally at index 0");
Check(s21GatesWorld.TryReRallySquad(0, 28, 5, 20), "slice21 general gates test general gate accepts the same patrol at index 0 again, now as an ordinary patrol");
Eq(28, s21GatesPatrol.RallyX, "slice21 general gates test general gate moves the patrol rally a second time");
Eq(0, s21GatesPatrol.PatrolWaypointIndex, "slice21 general gates test second re-rally resets the patrol index");
Eq(24, s21GatesPatrol.X, "slice21 general gates test neither re-rally teleports the patrol");
Eq(24, s21GatesHold.X, "slice21 general gates test neither action teleports the holdground squad");
Eq(26, s21GatesHold.RallyX, "slice21 general gates test holdground rally stands after the patrol actions");
// Fail-closed general refusals: every gate failure
// returns false and changes nothing — dead squad,
// out-of-range and negative indices, out-of-bounds
// rallies, and the same rally again, for both covered
// doctrines.
var s21RefuseWorld = new WorldZone(42);
s21RefuseWorld.Player.X = 20; s21RefuseWorld.Player.Y = 5; s21RefuseWorld.Player.Z = 20;
var s21RefuseHold = s21RefuseWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
var s21RefusePatrol = s21RefuseWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(!s21RefuseWorld.TryReRallySquad(-1, 26, 5, 20), "slice21 general negative index is refused");
Check(!s21RefuseWorld.TryReRallySquad(2, 26, 5, 20), "slice21 general out-of-range index is refused");
Check(!s21RefuseWorld.TryReRallySquad(0, 99, 5, 20), "slice21 general out-of-bounds rally is refused for holdground");
Check(!s21RefuseWorld.TryReRallySquad(1, -1, 5, 20), "slice21 general negative rally is refused for patrol");
Check(!s21RefuseWorld.TryReRallySquad(0, 10, 5, 10), "slice21 general same-rally re-rally is refused for holdground");
Check(!s21RefuseWorld.TryReRallySquad(1, 20, 5, 20), "slice21 general same-rally re-rally is refused for patrol");
Eq(10, s21RefuseHold.RallyX, "slice21 general refused holdground keeps its rally");
Eq(20, s21RefusePatrol.RallyX, "slice21 general refused patrol keeps its rally");
Eq(20, s21RefuseHold.X, "slice21 general refused holdground keeps its position");
Eq(20, s21RefusePatrol.X, "slice21 general refused patrol keeps its position");
Eq(0, s21RefusePatrol.PatrolWaypointIndex, "slice21 general refused patrol keeps its index");
Eq(0, s21RefusePatrol.PatrolConsecutiveRefusals, "slice21 general refused patrol keeps its counter");
var s21DeadWorld = new WorldZone(42);
s21DeadWorld.Player.X = 20; s21DeadWorld.Player.Y = 5; s21DeadWorld.Player.Z = 20;
var s21Dead = s21DeadWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
s21Dead.TakeDamage(1000);
Check(s21Dead.IsDead, "slice21 general dead test squad is dead before the action");
Check(!s21DeadWorld.TryReRallySquad(0, 26, 5, 20), "slice21 general dead squad is refused");
Eq(10, s21Dead.RallyX, "slice21 general refused dead squad keeps its rally");
// Save/load, Simulation passthrough, and the paid
// path unchanged: three paid squads, a free general
// re-rally for the HoldGround squad and for the
// Patrol through the passthrough, a refused inert
// re-rally, and a fourth deploy still refused at the
// full cap with ore untouched; then a save/load
// round-trip in which the re-rallied squads load
// with their new rallies and march on the loaded tick.
var s21Sim = new Simulation(42);
s21Sim.World.Player.X = 16; s21Sim.World.Player.Y = 5; s21Sim.World.Player.Z = 16;
s21Sim.World.Player.Inventory.Add(BlockType.EmberOre, 40);
var s21SimHold = s21Sim.World.TryDeployPaidSquad(10, 5, 10, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
Eq(30, s21Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 general paid test first deploy charges 10");
var s21SimPatrol = s21Sim.World.TryDeployPaidSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Recon)!;
Eq(20, s21Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 general paid test second deploy charges 10");
var s21SimAdv = s21Sim.World.TryDeployPaidSquad(10, 5, 10, TacticalDoctrine.Advance, SquadKind.Heavy)!;
Eq(10, s21Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 general paid test third deploy charges 10");
Eq(3, SquadEconomy.LiveSquadCount(s21Sim.World), "slice21 general paid test cap is full at 3 live squads");
Check(s21Sim.TryReRallySquad(0, 22, 5, 16), "slice21 general paid test holdground accepts a free re-rally through the passthrough");
Eq(22, s21SimHold.RallyX, "slice21 general paid test holdground takes its new rally");
Eq(10, s21Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 general paid test holdground re-rally charges nothing");
Eq(3, SquadEconomy.LiveSquadCount(s21Sim.World), "slice21 general paid test holdground re-rally frees no cap slot");
Check(!s21Sim.TryReRallySquad(2, 22, 5, 16), "slice21 general paid test advance re-rally is refused through the passthrough");
Eq(10, s21SimAdv.RallyX, "slice21 general paid test refused advance keeps its rally");
Check(s21Sim.TryReRallySquad(1, 26, 5, 20), "slice21 general paid test patrol accepts a free re-rally through the passthrough");
Eq(26, s21SimPatrol.RallyX, "slice21 general paid test patrol takes its new rally");
Eq(10, s21Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 general paid test patrol re-rally charges nothing");
Check(s21Sim.World.TryDeployPaidSquad(10, 5, 10, TacticalDoctrine.DefendCore, SquadKind.Heavy) == null, "slice21 general paid test fourth deploy is still refused at the cap");
Eq(10, s21Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 general paid test refused fourth deploy charges nothing");
Eq(22, s21SimHold.RallyX, "slice21 general paid test holdground keeps its new rally after the cap refusal");
Eq(26, s21SimPatrol.RallyX, "slice21 general paid test patrol keeps its new rally after the cap refusal");
var s21Save = s21Sim.SaveToJson();
var s21LoadedSim = new Simulation(99);
Check(s21LoadedSim.LoadFromJson(s21Save), "slice21 general paid test save loads");
Eq(3, s21LoadedSim.World.Squads.Count, "slice21 general paid test save keeps three squads");
var s21LoadedHold = s21LoadedSim.World.Squads[0];
var s21LoadedPatrol = s21LoadedSim.World.Squads[1];
Eq(16, s21LoadedHold.X, "slice21 general paid test loaded holdground position preserved");
Eq(22, s21LoadedHold.RallyX, "slice21 general paid test loaded holdground rally is the new rally, not the old one");
Eq(16, s21LoadedPatrol.X, "slice21 general paid test loaded patrol position preserved");
Eq(26, s21LoadedPatrol.RallyX, "slice21 general paid test loaded patrol rally is the new rally, not the old one");
Eq(0, s21LoadedPatrol.PatrolWaypointIndex, "slice21 general paid test loaded patrol index is 0");
Eq(0, s21LoadedPatrol.PatrolConsecutiveRefusals, "slice21 general paid test loaded patrol counter is 0");
s21LoadedSim.World.Advance();
Eq(17, s21LoadedHold.X, "slice21 general paid test loaded tick 1 holdground steps toward its new rally");
Eq(17, s21LoadedPatrol.X, "slice21 general paid test loaded tick 1 patrol steps toward its new rally");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s21LoadedSim.World)[0].Activity, "slice21 general paid test loaded tick 1 holdground reads marching");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s21LoadedSim.World)[1].Activity, "slice21 general paid test loaded tick 1 patrol reads patrolling");
// A stuck-before-save patrol still loads Patrolling
// at index 0 under the existing rule, which this step
// does not break, and re-sticks after a fresh circuit.
var s21StuckSaveSim = new Simulation(42);
s21StuckSaveSim.World.Player.X = 20; s21StuckSaveSim.World.Player.Y = 5; s21StuckSaveSim.World.Player.Z = 20;
var s21StuckSave = s21StuckSaveSim.World.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s21StuckSaveSim.World.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice21 general stuck-save test stages an east wall");
Check(s21StuckSaveSim.World.Chunk.TrySet(19, 5, 20, BlockType.Stone), "slice21 general stuck-save test stages a west wall");
Check(s21StuckSaveSim.World.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice21 general stuck-save test stages a south wall");
Check(s21StuckSaveSim.World.Chunk.TrySet(20, 5, 19, BlockType.Stone), "slice21 general stuck-save test stages a north wall");
for (int s21Tick = 0; s21Tick < 5; s21Tick++) s21StuckSave.MoveForPatrol(s21StuckSaveSim.World.Chunk, Array.Empty<Enemy>());
Check(s21StuckSave.IsPatrolStuck, "slice21 general stuck-save test patrol is stuck before the save");
var s21StuckSaveJson = s21StuckSaveSim.SaveToJson();
var s21StuckLoadedSim = new Simulation(99);
Check(s21StuckLoadedSim.LoadFromJson(s21StuckSaveJson), "slice21 general stuck-save test loads");
var s21StuckLoaded = s21StuckLoadedSim.World.Squads[0];
Check(!s21StuckLoaded.IsPatrolStuck, "slice21 general stuck-before-save patrol loads not stuck, as before");
Eq(0, s21StuckLoaded.PatrolWaypointIndex, "slice21 general stuck-before-save patrol loads at index 0, as before");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s21StuckLoadedSim.World)[0].Activity, "slice21 general stuck-before-save patrol loads reading patrolling, as before");
for (int s21Tick = 0; s21Tick < 5; s21Tick++) s21StuckLoaded.MoveForPatrol(s21StuckLoadedSim.World.Chunk, Array.Empty<Enemy>());
Check(s21StuckLoaded.IsPatrolStuck, "slice21 general stuck-before-save patrol is stuck again after a full circuit");

// ---- Slice 21 Step 2: composed demo + review, closing Slice 21 (0.89.0) ----
// One Simulation composing the Step 1 decision with its
// controls through the paid path only, mirroring the demo
// scenario exactly: an Advance and a Defend Core squad
// whose general re-rallies are refused fail-closed, then
// removed by staged lethal damage and one tick so the cap
// of 3 can hold the three squads that follow (a death
// freeing a cap slot, as Slice 11 proved, NOT a recall);
// a Vanguard HoldGround marched to a first rally,
// generally re-rallied, and marched to its new rally;
// a Vanguard Patrol marched to its rally, walked onto
// its loop, generally re-rallied mid-loop, marched to
// its new rally, and turned onto its new loop; then a
// Vanguard Patrol deployed at the player cell in the
// route-blocked shape (west, south, and north walls,
// east open) that counts five refusals and reads Stuck,
// unmoved, while the other two keep their readings; then
// a stuck-before-save load copy, the free general
// re-rally of the stuck patrol, a refused fourth deploy
// at the full cap, the six-tick march to the new rally,
// the turn onto the new loop, and a save/load round-trip
// that loads the new rallies. No new core system, no
// adapter edit, and no checklist item is added by this
// step: item 40 already observes the general action, and
// this step only proves it together with its controls in
// one field.
var s21Step2Sim = new Simulation(42);
s21Step2Sim.World.Player.Inventory.Add(BlockType.EmberOre, 50);
s21Step2Sim.BeginOrbitalPick();
Check(s21Step2Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance), "slice21 step2 composed advance doctrine selected");
Check(s21Step2Sim.OrbitalCommand.SelectKind(SquadKind.Vanguard), "slice21 step2 composed advance kind selected");
Check(s21Step2Sim.SetOrbitalRally(10, 5, 10), "slice21 step2 composed advance rally set");
var s21Step2Adv = s21Step2Sim.ConfirmOrbitalDeploy();
Check(s21Step2Adv != null, "slice21 step2 composed advance deploys paid");
Eq(TacticalDoctrine.Advance, s21Step2Adv!.Doctrine, "slice21 step2 composed advance keeps advance doctrine");
Eq(SquadKind.Vanguard, s21Step2Adv.Kind, "slice21 step2 composed advance keeps vanguard kind");
Eq(40, s21Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 step2 composed advance charges 10 ore");
Eq(16, s21Step2Adv.X, "slice21 step2 composed advance deploys at the player X");
Eq(16, s21Step2Adv.Z, "slice21 step2 composed advance deploys at the player Z");
Eq(SquadActivity.Idle, OrbitalView.Snapshot(s21Step2Sim.World)[0].Activity, "slice21 step2 composed advance reads idle with no enemy");
Eq(90, s21Step2Adv.Health, "slice21 step2 composed advance health before the refusal");
s21Step2Sim.BeginOrbitalPick();
Check(s21Step2Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.DefendCore), "slice21 step2 composed defendcore doctrine selected");
Check(s21Step2Sim.OrbitalCommand.SelectKind(SquadKind.Vanguard), "slice21 step2 composed defendcore kind selected");
Check(s21Step2Sim.SetOrbitalRally(10, 5, 10), "slice21 step2 composed defendcore rally set");
var s21Step2Def = s21Step2Sim.ConfirmOrbitalDeploy();
Check(s21Step2Def != null, "slice21 step2 composed defendcore deploys paid");
Eq(TacticalDoctrine.DefendCore, s21Step2Def!.Doctrine, "slice21 step2 composed defendcore keeps defendcore doctrine");
Eq(30, s21Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 step2 composed defendcore charges 10 ore");
Eq(16, s21Step2Def.X, "slice21 step2 composed defendcore deploys at the player X");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s21Step2Sim.World)[1].Activity, "slice21 step2 composed defendcore reads marching away from the core with no attacker");
Eq(90, s21Step2Def.Health, "slice21 step2 composed defendcore health before the refusal");
Check(!s21Step2Sim.TryReRallySquad(0, 20, 5, 20), "slice21 step2 composed advance re-rally is refused");
Check(!s21Step2Sim.TryReRallySquad(1, 20, 5, 20), "slice21 step2 composed defendcore re-rally is refused");
Eq(10, s21Step2Adv.RallyX, "slice21 step2 composed refused advance keeps its rally X");
Eq(10, s21Step2Def.RallyX, "slice21 step2 composed refused defendcore keeps its rally X");
Eq(16, s21Step2Adv.X, "slice21 step2 composed refused advance keeps its position X");
Eq(16, s21Step2Def.X, "slice21 step2 composed refused defendcore keeps its position X");
Eq(90, s21Step2Adv.Health, "slice21 step2 composed refused advance keeps its health");
Eq(90, s21Step2Def.Health, "slice21 step2 composed refused defendcore keeps its health");
Eq(SquadActivity.Idle, OrbitalView.Snapshot(s21Step2Sim.World)[0].Activity, "slice21 step2 composed refused advance still reads idle");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s21Step2Sim.World)[1].Activity, "slice21 step2 composed refused defendcore still reads marching");
Eq(30, s21Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 step2 composed refusals charge nothing");
s21Step2Adv.TakeDamage(90);
s21Step2Def.TakeDamage(90);
s21Step2Sim.World.Advance();
Eq(0, s21Step2Sim.World.Squads.Count, "slice21 step2 composed staged deaths free both cap slots");
Eq(30, s21Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 step2 composed staged deaths refund nothing");
s21Step2Sim.BeginOrbitalPick();
Check(s21Step2Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround), "slice21 step2 composed hold doctrine selected");
Check(s21Step2Sim.OrbitalCommand.SelectKind(SquadKind.Vanguard), "slice21 step2 composed hold kind selected");
Check(s21Step2Sim.SetOrbitalRally(12, 5, 16), "slice21 step2 composed hold rally set");
var s21Step2Hold = s21Step2Sim.ConfirmOrbitalDeploy();
Check(s21Step2Hold != null, "slice21 step2 composed hold deploys paid");
Eq(20, s21Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 step2 composed hold charges 10 ore");
Eq(16, s21Step2Hold!.X, "slice21 step2 composed hold deploys at the player X");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s21Step2Sim.World)[0].Activity, "slice21 step2 composed hold reads marching before any tick");
s21Step2Sim.World.Advance();
Eq(15, s21Step2Hold.X, "slice21 step2 composed hold march tick 1 X");
Eq(16, s21Step2Hold.Z, "slice21 step2 composed hold march tick 1 Z");
s21Step2Sim.World.Advance();
Eq(14, s21Step2Hold.X, "slice21 step2 composed hold march tick 2 X");
s21Step2Sim.World.Advance();
Eq(13, s21Step2Hold.X, "slice21 step2 composed hold march tick 3 X");
s21Step2Sim.World.Advance();
Eq(12, s21Step2Hold.X, "slice21 step2 composed hold march tick 4 X");
Eq(16, s21Step2Hold.Z, "slice21 step2 composed hold march tick 4 Z");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s21Step2Sim.World)[0].Activity, "slice21 step2 composed hold reads holding at its first rally");
Check(s21Step2Sim.TryReRallySquad(0, 20, 5, 16), "slice21 step2 composed hold accepts a free general re-rally");
Eq(20, s21Step2Hold.RallyX, "slice21 step2 composed hold new rally X");
Eq(12, s21Step2Hold.X, "slice21 step2 composed hold does not teleport to the new rally");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s21Step2Sim.World)[0].Activity, "slice21 step2 composed hold reads marching immediately after the action");
Eq(20, s21Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 step2 composed hold re-rally charges nothing");
Eq(1, s21Step2Sim.World.Squads.Count, "slice21 step2 composed hold re-rally takes no cap slot");
Eq(3, TacticalSquad.PatrolLoopRadius, "slice21 step2 composed loop radius is still 3, not retuned");
Eq(5, TacticalSquad.PatrolWaypointCount, "slice21 step2 composed loop waypoint count is still 5, not retuned");
s21Step2Sim.World.Advance();
Eq(13, s21Step2Hold.X, "slice21 step2 composed hold re-rally march tick 1 X");
s21Step2Sim.World.Advance();
Eq(14, s21Step2Hold.X, "slice21 step2 composed hold re-rally march tick 2 X");
s21Step2Sim.World.Advance();
Eq(15, s21Step2Hold.X, "slice21 step2 composed hold re-rally march tick 3 X");
s21Step2Sim.World.Advance();
Eq(16, s21Step2Hold.X, "slice21 step2 composed hold re-rally march tick 4 X");
s21Step2Sim.World.Advance();
Eq(17, s21Step2Hold.X, "slice21 step2 composed hold re-rally march tick 5 X");
s21Step2Sim.World.Advance();
Eq(18, s21Step2Hold.X, "slice21 step2 composed hold re-rally march tick 6 X");
s21Step2Sim.World.Advance();
Eq(19, s21Step2Hold.X, "slice21 step2 composed hold re-rally march tick 7 X");
s21Step2Sim.World.Advance();
Eq(20, s21Step2Hold.X, "slice21 step2 composed hold re-rally march tick 8 X");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s21Step2Sim.World)[0].Activity, "slice21 step2 composed hold reads holding at its new rally");
s21Step2Sim.BeginOrbitalPick();
Check(s21Step2Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol), "slice21 step2 composed patrol doctrine selected");
Check(s21Step2Sim.OrbitalCommand.SelectKind(SquadKind.Vanguard), "slice21 step2 composed patrol kind selected");
Check(s21Step2Sim.SetOrbitalRally(20, 5, 20), "slice21 step2 composed patrol rally set");
var s21Step2Patrol = s21Step2Sim.ConfirmOrbitalDeploy();
Check(s21Step2Patrol != null, "slice21 step2 composed patrol deploys paid");
Eq(10, s21Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 step2 composed patrol charges 10 ore");
Eq(16, s21Step2Patrol!.X, "slice21 step2 composed patrol deploys at the player X");
s21Step2Sim.World.Advance();
Eq(20, s21Step2Hold.X, "slice21 step2 composed patrol march tick 1 hold X");
Eq(17, s21Step2Patrol.X, "slice21 step2 composed patrol march tick 1 patrol X");
Eq(16, s21Step2Patrol.Z, "slice21 step2 composed patrol march tick 1 patrol Z");
s21Step2Sim.World.Advance();
Eq(17, s21Step2Patrol.X, "slice21 step2 composed patrol march tick 2 patrol X");
Eq(17, s21Step2Patrol.Z, "slice21 step2 composed patrol march tick 2 patrol Z");
s21Step2Sim.World.Advance();
Eq(18, s21Step2Patrol.X, "slice21 step2 composed patrol march tick 3 patrol X");
Eq(17, s21Step2Patrol.Z, "slice21 step2 composed patrol march tick 3 patrol Z");
s21Step2Sim.World.Advance();
Eq(18, s21Step2Patrol.X, "slice21 step2 composed patrol march tick 4 patrol X");
Eq(18, s21Step2Patrol.Z, "slice21 step2 composed patrol march tick 4 patrol Z");
s21Step2Sim.World.Advance();
Eq(19, s21Step2Patrol.X, "slice21 step2 composed patrol march tick 5 patrol X");
Eq(18, s21Step2Patrol.Z, "slice21 step2 composed patrol march tick 5 patrol Z");
s21Step2Sim.World.Advance();
Eq(19, s21Step2Patrol.X, "slice21 step2 composed patrol march tick 6 patrol X");
Eq(19, s21Step2Patrol.Z, "slice21 step2 composed patrol march tick 6 patrol Z");
s21Step2Sim.World.Advance();
Eq(20, s21Step2Patrol.X, "slice21 step2 composed patrol march tick 7 patrol X");
Eq(19, s21Step2Patrol.Z, "slice21 step2 composed patrol march tick 7 patrol Z");
s21Step2Sim.World.Advance();
Eq(20, s21Step2Patrol.X, "slice21 step2 composed patrol march tick 8 patrol reaches its rally X");
Eq(20, s21Step2Patrol.Z, "slice21 step2 composed patrol march tick 8 patrol reaches its rally Z");
Eq(0, s21Step2Patrol.PatrolWaypointIndex, "slice21 step2 composed patrol march tick 8 patrol waypoint at the rally");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s21Step2Sim.World)[1].Activity, "slice21 step2 composed patrol march tick 8 patrol reads patrolling at its rally");
s21Step2Sim.World.Advance();
Eq(21, s21Step2Patrol.X, "slice21 step2 composed patrol loop tick X");
Eq(1, s21Step2Patrol.PatrolWaypointIndex, "slice21 step2 composed patrol loop tick waypoint");
Eq((23, 5, 20), s21Step2Patrol.PatrolTarget, "slice21 step2 composed patrol loop tick east waypoint is the rally plus radius 3");
Check(s21Step2Sim.TryReRallySquad(1, 26, 5, 20), "slice21 step2 composed non-stuck patrol accepts a free general re-rally mid-loop");
Eq(26, s21Step2Patrol.RallyX, "slice21 step2 composed patrol new rally X");
Eq(0, s21Step2Patrol.PatrolWaypointIndex, "slice21 step2 composed patrol waypoint index resets to 0");
Eq(0, s21Step2Patrol.PatrolConsecutiveRefusals, "slice21 step2 composed patrol refusal counter resets to 0");
Eq(21, s21Step2Patrol.X, "slice21 step2 composed patrol does not teleport to the new rally");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s21Step2Sim.World)[1].Activity, "slice21 step2 composed patrol reads patrolling immediately after the action");
Eq(10, s21Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 step2 composed patrol re-rally charges nothing");
s21Step2Sim.World.Advance();
Eq(22, s21Step2Patrol.X, "slice21 step2 composed patrol re-rally march tick 1 X");
s21Step2Sim.World.Advance();
Eq(23, s21Step2Patrol.X, "slice21 step2 composed patrol re-rally march tick 2 X");
s21Step2Sim.World.Advance();
Eq(24, s21Step2Patrol.X, "slice21 step2 composed patrol re-rally march tick 3 X");
s21Step2Sim.World.Advance();
Eq(25, s21Step2Patrol.X, "slice21 step2 composed patrol re-rally march tick 4 X");
s21Step2Sim.World.Advance();
Eq(26, s21Step2Patrol.X, "slice21 step2 composed patrol re-rally march tick 5 X");
s21Step2Sim.World.Advance();
Eq(27, s21Step2Patrol.X, "slice21 step2 composed patrol new loop tick X");
Eq(1, s21Step2Patrol.PatrolWaypointIndex, "slice21 step2 composed patrol new loop tick waypoint");
Eq((29, 5, 20), s21Step2Patrol.PatrolTarget, "slice21 step2 composed patrol new loop east waypoint is the new rally plus radius 3");
s21Step2Sim.BeginOrbitalPick();
Check(s21Step2Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol), "slice21 step2 composed stuck doctrine selected");
Check(s21Step2Sim.OrbitalCommand.SelectKind(SquadKind.Vanguard), "slice21 step2 composed stuck kind selected");
Check(s21Step2Sim.SetOrbitalRally(12, 5, 16), "slice21 step2 composed stuck rally set");
var s21Step2Stuck = s21Step2Sim.ConfirmOrbitalDeploy();
Check(s21Step2Stuck != null, "slice21 step2 composed stuck deploys paid");
Eq(0, s21Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 step2 composed stuck charges the last 10 ore");
Eq(3, s21Step2Sim.World.Squads.Count, "slice21 step2 composed three paid squads fielded, cap full");
Eq(16, s21Step2Stuck!.X, "slice21 step2 composed stuck deploys at the player X");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s21Step2Sim.World)[2].Activity, "slice21 step2 composed stuck reads patrolling before its circuit");
Check(s21Step2Sim.World.Chunk.TrySet(15, 5, 16, BlockType.Stone), "slice21 step2 composed test stages the west enclosure wall");
Check(s21Step2Sim.World.Chunk.TrySet(16, 5, 17, BlockType.Stone), "slice21 step2 composed test stages the south enclosure wall");
Check(s21Step2Sim.World.Chunk.TrySet(16, 5, 15, BlockType.Stone), "slice21 step2 composed test stages the north enclosure wall");
Eq(BlockType.Air, s21Step2Sim.World.Chunk.Get(17, 5, 16), "slice21 step2 composed test east step cell stays open");
s21Step2Sim.World.Advance();
Eq(28, s21Step2Patrol.X, "slice21 step2 composed stuck tick 1 patrol X");
Eq(1, s21Step2Patrol.PatrolWaypointIndex, "slice21 step2 composed stuck tick 1 patrol waypoint");
Eq(1, s21Step2Stuck.PatrolConsecutiveRefusals, "slice21 step2 composed stuck tick 1 refusals");
Eq(1, s21Step2Stuck.PatrolWaypointIndex, "slice21 step2 composed stuck tick 1 waypoint");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s21Step2Sim.World)[2].Activity, "slice21 step2 composed stuck tick 1 reads patrolling");
s21Step2Sim.World.Advance();
Eq(29, s21Step2Patrol.X, "slice21 step2 composed stuck tick 2 patrol X");
Eq(2, s21Step2Stuck.PatrolConsecutiveRefusals, "slice21 step2 composed stuck tick 2 refusals");
s21Step2Sim.World.Advance();
Eq(28, s21Step2Patrol.X, "slice21 step2 composed stuck tick 3 patrol X");
Eq(2, s21Step2Patrol.PatrolWaypointIndex, "slice21 step2 composed stuck tick 3 patrol waypoint");
Eq(3, s21Step2Stuck.PatrolConsecutiveRefusals, "slice21 step2 composed stuck tick 3 refusals");
s21Step2Sim.World.Advance();
Eq(28, s21Step2Patrol.X, "slice21 step2 composed stuck tick 4 patrol X");
Eq(21, s21Step2Patrol.Z, "slice21 step2 composed stuck tick 4 patrol Z");
Eq(4, s21Step2Stuck.PatrolConsecutiveRefusals, "slice21 step2 composed stuck tick 4 refusals");
s21Step2Sim.World.Advance();
Eq(27, s21Step2Patrol.X, "slice21 step2 composed stuck tick 5 patrol X");
Eq(21, s21Step2Patrol.Z, "slice21 step2 composed stuck tick 5 patrol Z");
Eq(16, s21Step2Stuck.X, "slice21 step2 composed stuck stays unmoved X");
Eq(16, s21Step2Stuck.Z, "slice21 step2 composed stuck stays unmoved Z");
Eq(5, s21Step2Stuck.PatrolConsecutiveRefusals, "slice21 step2 composed stuck tick 5 refusals");
Eq(0, s21Step2Stuck.PatrolWaypointIndex, "slice21 step2 composed stuck tick 5 waypoint cycles back to 0");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s21Step2Sim.World)[2].Activity, "slice21 step2 composed stuck reads stuck at five refusals");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s21Step2Sim.World)[0].Activity, "slice21 step2 composed stuck phase hold still reads holding");
var s21Step2StuckSave = s21Step2Sim.SaveToJson();
var s21Step2StuckLoadedSim = new Simulation(99);
Check(s21Step2StuckLoadedSim.LoadFromJson(s21Step2StuckSave), "slice21 step2 composed stuck-before-save copy loads");
Eq(3, s21Step2StuckLoadedSim.World.Squads.Count, "slice21 step2 composed stuck-before-save copy keeps three squads");
Eq(0, s21Step2StuckLoadedSim.World.Squads[2].PatrolWaypointIndex, "slice21 step2 composed stuck-before-save copy stuck index is 0");
Eq(0, s21Step2StuckLoadedSim.World.Squads[2].PatrolConsecutiveRefusals, "slice21 step2 composed stuck-before-save copy stuck counter is 0");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s21Step2StuckLoadedSim.World)[2].Activity, "slice21 step2 composed stuck-before-save copy stuck reads patrolling");
Eq(12, s21Step2StuckLoadedSim.World.Squads[2].RallyX, "slice21 step2 composed stuck-before-save copy stuck keeps its old rally");
for (int s21Step2Tick = 0; s21Step2Tick < 5; s21Step2Tick++) s21Step2StuckLoadedSim.World.Advance();
var s21Step2StuckLoaded = s21Step2StuckLoadedSim.World.Squads[2];
Check(s21Step2StuckLoaded.IsPatrolStuck, "slice21 step2 composed stuck-before-save copy re-sticks after a fresh circuit");
Eq(16, s21Step2StuckLoaded.X, "slice21 step2 composed stuck-before-save copy stays unmoved X");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s21Step2StuckLoadedSim.World)[2].Activity, "slice21 step2 composed stuck-before-save copy reads stuck again");
Check(s21Step2Sim.TryReRallySquad(2, 22, 5, 16), "slice21 step2 composed stuck patrol accepts a free general re-rally");
Eq(22, s21Step2Stuck.RallyX, "slice21 step2 composed stuck new rally X");
Eq(0, s21Step2Stuck.PatrolWaypointIndex, "slice21 step2 composed stuck waypoint index resets to 0");
Eq(0, s21Step2Stuck.PatrolConsecutiveRefusals, "slice21 step2 composed stuck refusal counter resets to 0");
Eq(16, s21Step2Stuck.X, "slice21 step2 composed stuck does not teleport to the new rally");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s21Step2Sim.World)[2].Activity, "slice21 step2 composed stuck reads patrolling immediately after the action");
Eq(0, s21Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 step2 composed stuck re-rally charges nothing, as under the stuck gate");
Eq(3, s21Step2Sim.World.Squads.Count, "slice21 step2 composed stuck re-rally frees no cap slot");
s21Step2Sim.BeginOrbitalPick();
Check(s21Step2Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.DefendCore), "slice21 step2 composed fourth doctrine selected");
Check(s21Step2Sim.OrbitalCommand.SelectKind(SquadKind.Heavy), "slice21 step2 composed fourth kind selected");
Check(s21Step2Sim.SetOrbitalRally(10, 5, 10), "slice21 step2 composed fourth rally set");
Check(s21Step2Sim.ConfirmOrbitalDeploy() == null, "slice21 step2 composed fourth paid deploy is refused at the full cap");
s21Step2Sim.CancelOrbitalPick();
Eq(0, s21Step2Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 step2 composed refused fourth deploy charges nothing");
Eq(22, s21Step2Stuck.RallyX, "slice21 step2 composed stuck keeps its new rally after the cap refusal");
s21Step2Sim.World.Advance();
Eq(27, s21Step2Patrol.X, "slice21 step2 composed re-rally march tick 1 patrol X");
Eq(22, s21Step2Patrol.Z, "slice21 step2 composed re-rally march tick 1 patrol Z");
Eq(17, s21Step2Stuck.X, "slice21 step2 composed re-rally march tick 1 stuck X");
Eq(0, s21Step2Stuck.PatrolWaypointIndex, "slice21 step2 composed re-rally march tick 1 stuck waypoint");
s21Step2Sim.World.Advance();
Eq(26, s21Step2Patrol.X, "slice21 step2 composed re-rally march tick 2 patrol X");
Eq(18, s21Step2Stuck.X, "slice21 step2 composed re-rally march tick 2 stuck X");
s21Step2Sim.World.Advance();
Eq(26, s21Step2Patrol.X, "slice21 step2 composed re-rally march tick 3 patrol X");
Eq(23, s21Step2Patrol.Z, "slice21 step2 composed re-rally march tick 3 patrol Z");
Eq(19, s21Step2Stuck.X, "slice21 step2 composed re-rally march tick 3 stuck X");
s21Step2Sim.World.Advance();
Eq(25, s21Step2Patrol.X, "slice21 step2 composed re-rally march tick 4 patrol X");
Eq(3, s21Step2Patrol.PatrolWaypointIndex, "slice21 step2 composed re-rally march tick 4 patrol waypoint");
Eq(20, s21Step2Stuck.X, "slice21 step2 composed re-rally march tick 4 stuck X");
s21Step2Sim.World.Advance();
Eq(25, s21Step2Patrol.X, "slice21 step2 composed re-rally march tick 5 patrol X");
Eq(22, s21Step2Patrol.Z, "slice21 step2 composed re-rally march tick 5 patrol Z");
Eq(21, s21Step2Stuck.X, "slice21 step2 composed re-rally march tick 5 stuck X");
s21Step2Sim.World.Advance();
Eq(24, s21Step2Patrol.X, "slice21 step2 composed re-rally march tick 6 patrol X");
Eq(22, s21Step2Stuck.X, "slice21 step2 composed re-rally march tick 6 stuck reaches its new rally X");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s21Step2Sim.World)[2].Activity, "slice21 step2 composed re-rally march tick 6 stuck reads patrolling at its new rally");
s21Step2Sim.World.Advance();
Eq(23, s21Step2Stuck.X, "slice21 step2 composed re-rally loop tick stuck X");
Eq(1, s21Step2Stuck.PatrolWaypointIndex, "slice21 step2 composed re-rally loop tick stuck waypoint");
Eq((25, 5, 16), s21Step2Stuck.PatrolTarget, "slice21 step2 composed re-rally loop tick east waypoint is the new rally plus radius 3");
Eq(24, s21Step2Patrol.X, "slice21 step2 composed re-rally loop tick patrol X");
Eq(21, s21Step2Patrol.Z, "slice21 step2 composed re-rally loop tick patrol Z");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s21Step2Sim.World)[0].Activity, "slice21 step2 composed re-rally loop tick hold still reads holding");
var s21Step2Save = s21Step2Sim.SaveToJson();
var s21Step2LoadedSim = new Simulation(99);
Check(s21Step2LoadedSim.LoadFromJson(s21Step2Save), "slice21 step2 composed save loads");
Eq(3, s21Step2LoadedSim.World.Squads.Count, "slice21 step2 composed save keeps three squads");
Eq(20, s21Step2LoadedSim.World.Squads[0].RallyX, "slice21 step2 composed loaded hold rally is the new rally");
Eq(26, s21Step2LoadedSim.World.Squads[1].RallyX, "slice21 step2 composed loaded patrol rally is the new rally");
Eq(22, s21Step2LoadedSim.World.Squads[2].RallyX, "slice21 step2 composed loaded re-rallied patrol rally is the new rally, not the old one");
Eq(0, s21Step2LoadedSim.World.Squads[0].PatrolWaypointIndex, "slice21 step2 composed loaded hold index is 0");
Eq(0, s21Step2LoadedSim.World.Squads[1].PatrolWaypointIndex, "slice21 step2 composed loaded patrol index is 0");
Eq(0, s21Step2LoadedSim.World.Squads[2].PatrolWaypointIndex, "slice21 step2 composed loaded re-rallied index is 0");
Eq(20, s21Step2LoadedSim.World.Squads[0].X, "slice21 step2 composed loaded hold position preserved");
Eq(24, s21Step2LoadedSim.World.Squads[1].X, "slice21 step2 composed loaded patrol position preserved");
Eq(23, s21Step2LoadedSim.World.Squads[2].X, "slice21 step2 composed loaded re-rallied position preserved");
Eq(0, s21Step2LoadedSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice21 step2 composed loaded ore is still 0");
s21Step2LoadedSim.World.Advance();
Eq(20, s21Step2LoadedSim.World.Squads[0].X, "slice21 step2 composed loaded tick 1 hold stays at its rally");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s21Step2LoadedSim.World)[0].Activity, "slice21 step2 composed loaded tick 1 hold reads holding");
Eq(25, s21Step2LoadedSim.World.Squads[1].X, "slice21 step2 composed loaded tick 1 patrol steps toward its new rally");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s21Step2LoadedSim.World)[1].Activity, "slice21 step2 composed loaded tick 1 patrol reads patrolling");
Eq(22, s21Step2LoadedSim.World.Squads[2].X, "slice21 step2 composed loaded tick 1 re-rallied steps back to its new rally");
Eq(0, s21Step2LoadedSim.World.Squads[2].PatrolWaypointIndex, "slice21 step2 composed loaded tick 1 re-rallied waypoint is 0 at its rally");

// ---- Slice 22 Step 1: dismiss decision, decided here (0.91.0) ----
// Decision, in one sentence: DISMISS (not recall) — any
// live squad of any doctrine and any kind leaves the field
// at once, by squad list index, for refund NONE and one
// freed cap slot, with confirmation in the adapter only,
// by proposal (see DECISIONS.md for the rejected
// hypotheses: recall, both actions, neither, a full or
// partial refund, a core pending-confirmation state, a
// different targeting rule, next-tick removal, and a
// narrower doctrine scope). A dismissed squad is gone,
// not stored: there is no reserve, bench, or
// redeploy-from-reserve rule behind the name.
// Paid chain first: three paid deploys at a full cap,
// a dismiss in the middle, then a paid redeploy through
// the picker — ore and cap shown at every step, so the
// two economy answers (refund none, slot freed) are
// proved together, not asserted separately.
var s22Sim = new Simulation(42);
s22Sim.World.Player.Inventory.Add(BlockType.EmberOre, 40);
// The headless paid helper deploys only the default
// HoldGround Vanguard, so this chain deploys through
// the picker — the play path — to get three doctrines.
s22Sim.BeginOrbitalPick();
Check(s22Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround), "slice22 dismiss paid chain hold doctrine selected");
Check(s22Sim.OrbitalCommand.SelectKind(SquadKind.Vanguard), "slice22 dismiss paid chain hold kind selected");
Check(s22Sim.SetOrbitalRally(20, 5, 16), "slice22 dismiss paid chain hold rally set");
Check(s22Sim.ConfirmOrbitalDeploy() != null, "slice22 dismiss paid chain hold deploys");
s22Sim.BeginOrbitalPick();
Check(s22Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol), "slice22 dismiss paid chain patrol doctrine selected");
Check(s22Sim.OrbitalCommand.SelectKind(SquadKind.Recon), "slice22 dismiss paid chain patrol kind selected");
Check(s22Sim.SetOrbitalRally(20, 5, 20), "slice22 dismiss paid chain patrol rally set");
Check(s22Sim.ConfirmOrbitalDeploy() != null, "slice22 dismiss paid chain patrol deploys");
s22Sim.BeginOrbitalPick();
Check(s22Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance), "slice22 dismiss paid chain advance doctrine selected");
Check(s22Sim.OrbitalCommand.SelectKind(SquadKind.Heavy), "slice22 dismiss paid chain advance kind selected");
Check(s22Sim.SetOrbitalRally(10, 5, 10), "slice22 dismiss paid chain advance rally set");
Check(s22Sim.ConfirmOrbitalDeploy() != null, "slice22 dismiss paid chain advance deploys");
Eq(10, s22Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 dismiss paid chain ore after three deploys is 10");
Eq(3, s22Sim.World.Squads.Count, "slice22 dismiss paid chain cap is full at 3");
Eq(3, SquadEconomy.LiveSquadCount(s22Sim.World), "slice22 dismiss paid chain live count is 3");
Eq(3, OrbitalView.Snapshot(s22Sim.World).Count, "slice22 dismiss paid chain snapshot lists 3");
var s22PaidHold = s22Sim.World.Squads[0];
var s22PaidAdv = s22Sim.World.Squads[2];
Check(s22Sim.TryDismissSquad(1), "slice22 dismiss paid chain middle patrol dismisses");
Eq(2, s22Sim.World.Squads.Count, "slice22 dismiss paid chain count drops to 2");
Eq(2, SquadEconomy.LiveSquadCount(s22Sim.World), "slice22 dismiss paid chain live count drops to 2");
Eq(10, s22Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 dismiss paid chain refund is none, ore still 10");
Eq(2, OrbitalView.Snapshot(s22Sim.World).Count, "slice22 dismiss paid chain snapshot lists 2 after the dismiss");
Eq(TacticalDoctrine.HoldGround, s22Sim.World.Squads[0].Doctrine, "slice22 dismiss paid chain remaining first squad is still holdground");
Eq(TacticalDoctrine.Advance, s22Sim.World.Squads[1].Doctrine, "slice22 dismiss paid chain remaining second squad is still advance");
Eq(90, s22PaidHold.Health, "slice22 dismiss paid chain remaining hold health unchanged");
Eq(100, s22PaidAdv.Health, "slice22 dismiss paid chain remaining advance health unchanged");
s22Sim.BeginOrbitalPick();
Check(s22Sim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.DefendCore), "slice22 dismiss paid chain redeploy doctrine selected");
Check(s22Sim.OrbitalCommand.SelectKind(SquadKind.Heavy), "slice22 dismiss paid chain redeploy kind selected");
Check(s22Sim.SetOrbitalRally(10, 5, 20), "slice22 dismiss paid chain redeploy rally set");
Check(s22Sim.ConfirmOrbitalDeploy() != null, "slice22 dismiss paid chain redeploy at the freed slot succeeds through the picker");
Eq(0, s22Sim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 dismiss paid chain redeploy costs a fresh 10, ore now 0");
Eq(3, s22Sim.World.Squads.Count, "slice22 dismiss paid chain cap is full again at 3");
Eq(TacticalDoctrine.DefendCore, s22Sim.World.Squads[2].Doctrine, "slice22 dismiss paid chain redeployed squad is defendcore");
Eq(SquadKind.Heavy, s22Sim.World.Squads[2].Kind, "slice22 dismiss paid chain redeployed squad is heavy");
// Every doctrine x every kind: twelve seam squads in one
// world, dismissed from index 0 in turn, so each combo is
// proved by its own removal, not by one representative.
var s22MatrixWorld = new WorldZone(42);
s22MatrixWorld.Player.X = 16; s22MatrixWorld.Player.Y = 5; s22MatrixWorld.Player.Z = 16;
var s22MatrixDoctrines = new[] { TacticalDoctrine.HoldGround, TacticalDoctrine.Patrol, TacticalDoctrine.Advance, TacticalDoctrine.DefendCore };
var s22MatrixKinds = new[] { SquadKind.Vanguard, SquadKind.Recon, SquadKind.Heavy };
var s22MatrixHealths = new[] { 90, 40, 100 };
foreach (var s22Doc in s22MatrixDoctrines)
foreach (var s22Kind in s22MatrixKinds)
    Check(s22MatrixWorld.SpawnSquad(20, 5, 20, s22Doc, s22Kind) != null, $"slice22 dismiss matrix spawns {s22Doc} {s22Kind}");
Eq(12, s22MatrixWorld.Squads.Count, "slice22 dismiss matrix holds all twelve doctrine-kind combos");
for (int s22Mi = 0; s22Mi < 12; s22Mi++)
{
    var s22Doc = s22MatrixDoctrines[s22Mi / 3];
    var s22Kind = s22MatrixKinds[s22Mi % 3];
    Eq(s22Doc, s22MatrixWorld.Squads[0].Doctrine, $"slice22 dismiss matrix combo {s22Mi} doctrine is {s22Doc}");
    Eq(s22Kind, s22MatrixWorld.Squads[0].Kind, $"slice22 dismiss matrix combo {s22Mi} kind is {s22Kind}");
    Eq(s22MatrixHealths[s22Mi % 3], s22MatrixWorld.Squads[0].Health, $"slice22 dismiss matrix combo {s22Mi} health is the kind full health");
    Check(s22MatrixWorld.TryDismissSquad(0), $"slice22 dismiss matrix combo {s22Mi} dismisses");
    Eq(11 - s22Mi, s22MatrixWorld.Squads.Count, $"slice22 dismiss matrix count after combo {s22Mi}");
}
Eq(0, OrbitalView.Snapshot(s22MatrixWorld).Count, "slice22 dismiss matrix snapshot is empty after all twelve");
// The named cases, one world each, so the scope's
// holding / marching / chasing / returning / patrolling
// list is proved by readings, not folded into "all".
var s22HoldWorld = new WorldZone(42);
s22HoldWorld.Player.X = 16; s22HoldWorld.Player.Y = 5; s22HoldWorld.Player.Z = 16;
var s22Hold = s22HoldWorld.SpawnSquad(16, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s22HoldWorld)[0].Activity, "slice22 dismiss holding holdground reads holding before the dismiss");
Check(s22HoldWorld.TryDismissSquad(0), "slice22 dismiss holding holdground dismisses");
Eq(0, s22HoldWorld.Squads.Count, "slice22 dismiss holding holdground leaves no squad");
var s22MarchWorld = new WorldZone(42);
s22MarchWorld.Player.X = 16; s22MarchWorld.Player.Y = 5; s22MarchWorld.Player.Z = 16;
var s22March = s22MarchWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Recon)!;
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s22MarchWorld)[0].Activity, "slice22 dismiss marching holdground reads marching before the dismiss");
Check(s22MarchWorld.TryDismissSquad(0), "slice22 dismiss marching holdground dismisses");
Eq(40, s22March.Health, "slice22 dismiss marching recon health is untouched by its own removal, it is simply gone from the list");
var s22PatrolCaseWorld = new WorldZone(42);
s22PatrolCaseWorld.Player.X = 16; s22PatrolCaseWorld.Player.Y = 5; s22PatrolCaseWorld.Player.Z = 16;
var s22PatrolCase = s22PatrolCaseWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Heavy)!;
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s22PatrolCaseWorld)[0].Activity, "slice22 dismiss patrolling patrol reads patrolling before the dismiss");
Check(s22PatrolCaseWorld.TryDismissSquad(0), "slice22 dismiss patrolling patrol dismisses");
var s22ChaseWorld = new WorldZone(42);
s22ChaseWorld.Player.X = 16; s22ChaseWorld.Player.Y = 5; s22ChaseWorld.Player.Z = 16;
var s22Chase = s22ChaseWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.Advance, SquadKind.Vanguard)!;
s22ChaseWorld.Enemies.Add(new Enemy(20, 5, 16));
Eq(SquadActivity.Hunting, OrbitalView.Snapshot(s22ChaseWorld)[0].Activity, "slice22 dismiss chasing advance reads hunting before the dismiss");
Check(s22ChaseWorld.TryDismissSquad(0), "slice22 dismiss chasing advance dismisses");
var s22ReturnWorld = new WorldZone(42);
s22ReturnWorld.Player.X = 16; s22ReturnWorld.Player.Y = 5; s22ReturnWorld.Player.Z = 16;
var s22Return = s22ReturnWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.DefendCore, SquadKind.Recon)!;
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s22ReturnWorld)[0].Activity, "slice22 dismiss returning defendcore reads marching before the dismiss");
Check(s22ReturnWorld.TryDismissSquad(0), "slice22 dismiss returning defendcore dismisses");
var s22GuardWorld = new WorldZone(42);
s22GuardWorld.Player.X = 28; s22GuardWorld.Player.Y = 5; s22GuardWorld.Player.Z = 28;
var s22Guard = s22GuardWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.DefendCore, SquadKind.Heavy)!;
Eq(SquadActivity.Guarding, OrbitalView.Snapshot(s22GuardWorld)[0].Activity, "slice22 dismiss guarding defendcore reads guarding before the dismiss");
Check(s22GuardWorld.TryDismissSquad(0), "slice22 dismiss guarding defendcore dismisses");
// A damaged squad dismisses for refund none too: health
// is not a refund basis, so a wounded squad is worth
// the same 0 as a full one, and its neighbours are
// untouched by its removal.
var s22WoundWorld = new WorldZone(42);
s22WoundWorld.Player.X = 16; s22WoundWorld.Player.Y = 5; s22WoundWorld.Player.Z = 16;
var s22Wound = s22WoundWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
var s22WoundOther = s22WoundWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.HoldGround, SquadKind.Heavy)!;
s22Wound.TakeDamage(30);
Eq(60, s22Wound.Health, "slice22 dismiss wounded squad health is 60 before the dismiss");
Check(s22WoundWorld.TryDismissSquad(0), "slice22 dismiss wounded squad dismisses");
Eq(0, s22WoundWorld.Player.Inventory.Get(BlockType.EmberOre), "slice22 dismiss wounded squad refunds nothing");
Eq(100, s22WoundOther.Health, "slice22 dismiss wounded squad leaves its neighbour at full health");
Eq(TacticalDoctrine.HoldGround, s22WoundWorld.Squads[0].Doctrine, "slice22 dismiss wounded squad leaves only the neighbour");
// Stuck patrol: dismiss is available beside the free
// rescue, not instead of it. The same staging as Slice
// 21's gates test (player at (24,5,20), walls west,
// south, and north) makes the patrol stuck in five
// patrol steps; one world dismisses it, a second
// re-rallies it free, and a third dismisses a different
// squad and still re-rallies the stuck one.
var s22StuckDismissWorld = new WorldZone(42);
s22StuckDismissWorld.Player.X = 24; s22StuckDismissWorld.Player.Y = 5; s22StuckDismissWorld.Player.Z = 20;
var s22StuckDismiss = s22StuckDismissWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(s22StuckDismissWorld.Chunk.TrySet(23, 5, 20, BlockType.Stone), "slice22 dismiss stuck test stages a west wall");
Check(s22StuckDismissWorld.Chunk.TrySet(24, 5, 21, BlockType.Stone), "slice22 dismiss stuck test stages a south wall");
Check(s22StuckDismissWorld.Chunk.TrySet(24, 5, 19, BlockType.Stone), "slice22 dismiss stuck test stages a north wall");
for (int s22Tick = 0; s22Tick < 5; s22Tick++) s22StuckDismiss.MoveForPatrol(s22StuckDismissWorld.Chunk, Array.Empty<Enemy>());
Check(s22StuckDismiss.IsPatrolStuck, "slice22 dismiss stuck test patrol is stuck before the choice");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s22StuckDismissWorld)[0].Activity, "slice22 dismiss stuck test reads stuck before the choice");
Check(s22StuckDismissWorld.TryDismissSquad(0), "slice22 dismiss stuck patrol dismisses as a distinct choice");
Eq(0, s22StuckDismissWorld.Squads.Count, "slice22 dismiss stuck patrol leaves no squad");
var s22StuckRescueWorld = new WorldZone(42);
s22StuckRescueWorld.Player.X = 24; s22StuckRescueWorld.Player.Y = 5; s22StuckRescueWorld.Player.Z = 20;
var s22StuckRescue = s22StuckRescueWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(s22StuckRescueWorld.Chunk.TrySet(23, 5, 20, BlockType.Stone), "slice22 dismiss rescue test stages a west wall");
Check(s22StuckRescueWorld.Chunk.TrySet(24, 5, 21, BlockType.Stone), "slice22 dismiss rescue test stages a south wall");
Check(s22StuckRescueWorld.Chunk.TrySet(24, 5, 19, BlockType.Stone), "slice22 dismiss rescue test stages a north wall");
for (int s22Tick = 0; s22Tick < 5; s22Tick++) s22StuckRescue.MoveForPatrol(s22StuckRescueWorld.Chunk, Array.Empty<Enemy>());
Check(s22StuckRescue.IsPatrolStuck, "slice22 dismiss rescue test patrol is stuck before the choice");
Check(s22StuckRescueWorld.TryReRallySquad(0, 26, 5, 20), "slice22 dismiss rescue test stuck patrol still accepts a free re-rally");
Eq(26, s22StuckRescue.RallyX, "slice22 dismiss rescue test re-rally moves the rally");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s22StuckRescueWorld)[0].Activity, "slice22 dismiss rescue test reads patrolling after the free rescue");
Eq(1, s22StuckRescueWorld.Squads.Count, "slice22 dismiss rescue test keeps the rescued squad on the field");
var s22StuckBesideWorld = new WorldZone(42);
s22StuckBesideWorld.Player.X = 24; s22StuckBesideWorld.Player.Y = 5; s22StuckBesideWorld.Player.Z = 20;
var s22StuckBesideHold = s22StuckBesideWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
var s22StuckBeside = s22StuckBesideWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(s22StuckBesideWorld.Chunk.TrySet(23, 5, 20, BlockType.Stone), "slice22 dismiss beside test stages a west wall");
Check(s22StuckBesideWorld.Chunk.TrySet(24, 5, 21, BlockType.Stone), "slice22 dismiss beside test stages a south wall");
Check(s22StuckBesideWorld.Chunk.TrySet(24, 5, 19, BlockType.Stone), "slice22 dismiss beside test stages a north wall");
for (int s22Tick = 0; s22Tick < 5; s22Tick++) s22StuckBeside.MoveForPatrol(s22StuckBesideWorld.Chunk, Array.Empty<Enemy>());
Check(s22StuckBeside.IsPatrolStuck, "slice22 dismiss beside test patrol is stuck before the removal");
Check(s22StuckBesideWorld.TryDismissSquad(0), "slice22 dismiss beside test dismisses the other squad");
Eq(1, s22StuckBesideWorld.Squads.Count, "slice22 dismiss beside test leaves only the stuck patrol");
Check(s22StuckBesideWorld.TryReRallySquad(0, 26, 5, 20), "slice22 dismiss beside test stuck patrol still accepts a free re-rally after a removal in the same world");
Eq(26, s22StuckBeside.RallyX, "slice22 dismiss beside test re-rally moves the stuck rally after the removal");
// Refusals, each with nothing changed: empty list,
// negative and out-of-range indices, and a dead squad
// that is still in the list until Advance sweeps it.
var s22EmptyWorld = new WorldZone(42);
Check(!s22EmptyWorld.TryDismissSquad(0), "slice22 dismiss empty list is refused");
Eq(0, s22EmptyWorld.Squads.Count, "slice22 dismiss empty list stays empty");
var s22RefuseWorld = new WorldZone(42);
s22RefuseWorld.Player.X = 16; s22RefuseWorld.Player.Y = 5; s22RefuseWorld.Player.Z = 16;
var s22RefuseHold = s22RefuseWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
Check(!s22RefuseWorld.TryDismissSquad(-1), "slice22 dismiss negative index is refused");
Check(!s22RefuseWorld.TryDismissSquad(1), "slice22 dismiss out-of-range index is refused");
Eq(1, s22RefuseWorld.Squads.Count, "slice22 dismiss refused indices leave the squad");
Eq(20, s22RefuseHold.RallyX, "slice22 dismiss refused indices keep the rally");
Eq(90, s22RefuseHold.Health, "slice22 dismiss refused indices keep the health");
var s22DeadWorld = new WorldZone(42);
s22DeadWorld.Player.X = 16; s22DeadWorld.Player.Y = 5; s22DeadWorld.Player.Z = 16;
var s22Dead = s22DeadWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
s22Dead.TakeDamage(90);
Check(s22Dead.IsDead, "slice22 dismiss dead test squad is dead before the call");
Eq(0, SquadEconomy.LiveSquadCount(s22DeadWorld), "slice22 dismiss dead test live count is already 0");
Check(!s22DeadWorld.TryDismissSquad(0), "slice22 dismiss dead squad is refused");
Eq(1, s22DeadWorld.Squads.Count, "slice22 dismiss dead refusal leaves the dead entry in the list");
s22DeadWorld.Advance();
Eq(0, s22DeadWorld.Squads.Count, "slice22 dismiss dead entry is swept by the next tick, by death, not by dismiss");
// List shift, stated as player responsibility: after a
// dismiss, later indices shift down by one, and the
// gates prove it — the old index 2 is out of range,
// the old index 1's squad is now at index 1's target
// for the re-rally gates, and a stale index is never
// remapped to the squad it used to name.
var s22ShiftWorld = new WorldZone(42);
s22ShiftWorld.Player.X = 16; s22ShiftWorld.Player.Y = 5; s22ShiftWorld.Player.Z = 16;
var s22ShiftHold = s22ShiftWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
var s22ShiftPatrol = s22ShiftWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Recon)!;
var s22ShiftHold2 = s22ShiftWorld.SpawnSquad(22, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Heavy)!;
Check(s22ShiftWorld.TryDismissSquad(1), "slice22 dismiss shift test dismisses the middle patrol");
Eq(2, s22ShiftWorld.Squads.Count, "slice22 dismiss shift test count drops to 2");
Eq(TacticalDoctrine.HoldGround, s22ShiftWorld.Squads[1].Doctrine, "slice22 dismiss shift test index 1 now names the old index 2 squad");
Eq(SquadKind.Heavy, s22ShiftWorld.Squads[1].Kind, "slice22 dismiss shift test index 1 now names the heavy squad");
Check(!s22ShiftWorld.TryReRallySquad(2, 26, 5, 16), "slice22 dismiss shift test old index 2 is out of range after the shift");
Check(s22ShiftWorld.TryReRallySquad(1, 24, 5, 16), "slice22 dismiss shift test re-rally at index 1 now moves the shifted squad");
Eq(24, s22ShiftHold2.RallyX, "slice22 dismiss shift test shifted squad rally moves");
Eq(20, s22ShiftHold.RallyX, "slice22 dismiss shift test first squad rally untouched by the shift");
// Re-rally gates unchanged after a removal: a HoldGround
// squad still re-rallies free, an Advance squad is still
// refused for doctrine, and the shifted index is the
// one that is refused — the gates agree with the new
// list, not with the old one.
var s22GatesWorld = new WorldZone(42);
s22GatesWorld.Player.X = 16; s22GatesWorld.Player.Y = 5; s22GatesWorld.Player.Z = 16;
var s22GatesHold = s22GatesWorld.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
var s22GatesPatrol = s22GatesWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
var s22GatesAdv = s22GatesWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.Advance, SquadKind.Vanguard)!;
Check(s22GatesWorld.TryDismissSquad(1), "slice22 dismiss gates test dismisses the patrol");
Check(s22GatesWorld.TryReRallySquad(0, 21, 5, 16), "slice22 dismiss gates test remaining holdground still re-rallies free");
Eq(21, s22GatesHold.RallyX, "slice22 dismiss gates test holdground rally moves after the removal");
Check(!s22GatesWorld.TryReRallySquad(1, 22, 5, 16), "slice22 dismiss gates test shifted advance is still refused for doctrine");
Eq(10, s22GatesAdv.RallyX, "slice22 dismiss gates test refused advance keeps its rally after the removal");
// Timing: immediate, not next-tick. A dismissed squad
// that would have fired fires nothing, takes no hit,
// and leaves no half-tick: with only that squad and
// one enemy, the tick ends with the enemy at full
// health, stepped toward the player, and the player
// unhurt — while a remaining holding squad in a second
// world still fires exactly as before, so the tick is
// clean for both the removed and the remaining.
var s22TimingWorld = new WorldZone(42);
s22TimingWorld.Player.X = 16; s22TimingWorld.Player.Y = 5; s22TimingWorld.Player.Z = 16;
var s22TimingAdv = s22TimingWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.Advance, SquadKind.Vanguard)!;
var s22TimingEnemy = new Enemy(19, 5, 16);
s22TimingWorld.Enemies.Add(s22TimingEnemy);
Check(s22TimingWorld.TryDismissSquad(0), "slice22 dismiss timing test dismisses before the tick");
s22TimingWorld.Advance();
Eq(30, s22TimingEnemy.Health, "slice22 dismiss timing test dismissed squad fires nothing, enemy at full health");
Eq(18, s22TimingEnemy.X, "slice22 dismiss timing test enemy retargets cleanly to the player");
Eq(100, s22TimingWorld.Player.Health, "slice22 dismiss timing test player takes no hit from the retargeted enemy");
Eq(0, s22TimingWorld.Squads.Count, "slice22 dismiss timing test no squad remains to take a hit");
var s22RemainWorld = new WorldZone(42);
s22RemainWorld.Player.X = 16; s22RemainWorld.Player.Y = 5; s22RemainWorld.Player.Z = 16;
var s22RemainHold = s22RemainWorld.SpawnSquad(16, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
var s22RemainAdv = s22RemainWorld.SpawnSquad(10, 5, 10, TacticalDoctrine.Advance, SquadKind.Vanguard)!;
var s22RemainEnemy = new Enemy(18, 5, 16);
s22RemainWorld.Enemies.Add(s22RemainEnemy);
Check(s22RemainWorld.TryDismissSquad(1), "slice22 dismiss remain test dismisses the advance squad");
s22RemainWorld.Advance();
Eq(0, s22RemainWorld.Enemies.Count, "slice22 dismiss remain test remaining holding squad still kills its enemy on the tick");
Eq(75, s22RemainWorld.Player.Shields, "slice22 dismiss remain test player shields show the enemy's adjacent hit before the squad kill");
Eq(90, s22RemainHold.Health, "slice22 dismiss remain test remaining holding squad takes no squad damage");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s22RemainWorld)[0].Activity, "slice22 dismiss remain test remaining squad still reads holding");
// Picker independence and the death-counter check: a
// dismiss while the picker is open succeeds and leaves
// the picker — its selection and its pending rally —
// exactly as it was, and no kill counter moves, because
// no squad-death counter exists in core (the only kill
// counter counts enemies killed by player fire).
var s22PickSim = new Simulation(42);
var s22PickSquad = s22PickSim.World.SpawnSquad(20, 5, 16, TacticalDoctrine.HoldGround, SquadKind.Vanguard)!;
Eq(0, s22PickSim.EnemiesKilled, "slice22 dismiss picker test no enemy killed before the dismiss");
s22PickSim.BeginOrbitalPick();
Check(s22PickSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.DefendCore), "slice22 dismiss picker test doctrine selected while picking");
Check(s22PickSim.OrbitalCommand.SelectKind(SquadKind.Heavy), "slice22 dismiss picker test kind selected while picking");
Check(s22PickSim.SetOrbitalRally(10, 5, 20), "slice22 dismiss picker test rally set while picking");
Check(s22PickSim.TryDismissSquad(0), "slice22 dismiss picker test dismiss succeeds while the picker is open");
Check(s22PickSim.OrbitalCommand.IsPicking, "slice22 dismiss picker test picker stays open after the dismiss");
Check(s22PickSim.OrbitalCommand.HasRally, "slice22 dismiss picker test pending rally survives the dismiss");
Eq(TacticalDoctrine.DefendCore, s22PickSim.OrbitalCommand.Doctrine, "slice22 dismiss picker test doctrine selection survives the dismiss");
Eq(SquadKind.Heavy, s22PickSim.OrbitalCommand.Kind, "slice22 dismiss picker test kind selection survives the dismiss");
Eq(0, s22PickSim.EnemiesKilled, "slice22 dismiss picker test no kill counter moves on a dismiss");
s22PickSim.CancelOrbitalPick();
Check(!s22PickSim.OrbitalCommand.IsPicking, "slice22 dismiss picker test picker cancels cleanly after the dismiss");
// Save/load: a post-dismiss save has no entry for the
// removed squad, and a pre-dismiss save still loads
// with the squad present — no new save field, so old
// saves load unchanged.
var s22SaveSim = new Simulation(42);
s22SaveSim.World.Player.Inventory.Add(BlockType.EmberOre, 30);
// Picker deploys here too, for the same reason as the
// paid chain above: the paid helper is default-only.
s22SaveSim.BeginOrbitalPick();
Check(s22SaveSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround), "slice22 dismiss save test hold doctrine selected");
Check(s22SaveSim.OrbitalCommand.SelectKind(SquadKind.Vanguard), "slice22 dismiss save test hold kind selected");
Check(s22SaveSim.SetOrbitalRally(20, 5, 16), "slice22 dismiss save test hold rally set");
Check(s22SaveSim.ConfirmOrbitalDeploy() != null, "slice22 dismiss save test hold deploys");
s22SaveSim.BeginOrbitalPick();
Check(s22SaveSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol), "slice22 dismiss save test patrol doctrine selected");
Check(s22SaveSim.OrbitalCommand.SelectKind(SquadKind.Recon), "slice22 dismiss save test patrol kind selected");
Check(s22SaveSim.SetOrbitalRally(20, 5, 20), "slice22 dismiss save test patrol rally set");
Check(s22SaveSim.ConfirmOrbitalDeploy() != null, "slice22 dismiss save test patrol deploys");
s22SaveSim.BeginOrbitalPick();
Check(s22SaveSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance), "slice22 dismiss save test advance doctrine selected");
Check(s22SaveSim.OrbitalCommand.SelectKind(SquadKind.Heavy), "slice22 dismiss save test advance kind selected");
Check(s22SaveSim.SetOrbitalRally(10, 5, 10), "slice22 dismiss save test advance rally set");
Check(s22SaveSim.ConfirmOrbitalDeploy() != null, "slice22 dismiss save test advance deploys");
var s22PreDismissSave = s22SaveSim.SaveToJson();
var s22PreDismissLoaded = new Simulation(99);
Check(s22PreDismissLoaded.LoadFromJson(s22PreDismissSave), "slice22 dismiss save test pre-dismiss save loads");
Eq(3, s22PreDismissLoaded.World.Squads.Count, "slice22 dismiss save test pre-dismiss save keeps three squads");
Eq(TacticalDoctrine.Patrol, s22PreDismissLoaded.World.Squads[1].Doctrine, "slice22 dismiss save test pre-dismiss save keeps the patrol");
Check(s22SaveSim.TryDismissSquad(1), "slice22 dismiss save test dismisses the patrol");
Eq(0, s22SaveSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 dismiss save test ore is still 0 after the dismiss");
var s22PostDismissSave = s22SaveSim.SaveToJson();
var s22PostDismissLoaded = new Simulation(99);
Check(s22PostDismissLoaded.LoadFromJson(s22PostDismissSave), "slice22 dismiss save test post-dismiss save loads");
Eq(2, s22PostDismissLoaded.World.Squads.Count, "slice22 dismiss save test post-dismiss save has no entry for the removed squad");
Eq(TacticalDoctrine.HoldGround, s22PostDismissLoaded.World.Squads[0].Doctrine, "slice22 dismiss save test post-dismiss save keeps the holdground");
Eq(TacticalDoctrine.Advance, s22PostDismissLoaded.World.Squads[1].Doctrine, "slice22 dismiss save test post-dismiss save keeps the advance");
Eq(20, s22PostDismissLoaded.World.Squads[0].RallyX, "slice22 dismiss save test loaded hold rally preserved");
Eq(10, s22PostDismissLoaded.World.Squads[1].RallyX, "slice22 dismiss save test loaded advance rally preserved");
Eq(0, s22PostDismissLoaded.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 dismiss save test loaded ore is still 0");
// Radius and waypoint count carried, not re-decided:
// a dismiss moves no loop, so the first-pass values
// stand exactly as Slices 15, 20, and 21 left them.
Eq(3, TacticalSquad.PatrolLoopRadius, "slice22 dismiss loop radius is still 3, not retuned");
Eq(5, TacticalSquad.PatrolWaypointCount, "slice22 dismiss loop waypoint count is still 5, not retuned");

// ---- Slice 22 Step 2: composed demo + review, closing Slice 22 (0.92.0) ----
// One Simulation mirroring the demo, tick by tick: a
// full cap marched to its readings, out-of-range and
// negative refusals, an Advance dismiss, a paid
// Defend Core redeploy at the freed slot, a HoldGround
// dismiss, a route-blocked stuck Patrol, a pre-removal
// save, a free stuck re-rally beside a Defend Core
// dismiss, a post-removal save, a rescued march and
// loop turn, a moving Patrol dismiss, a sacrificial
// dead-squad refusal and sweep, an empty-field dismiss
// and empty refusal, and a final paid rebuild — with
// ore shown at every step, so the composed field
// proves the removal, its refusals, its economy, its
// rescue distinction, and its movement agree.
var s22cSim = new Simulation(42);
s22cSim.World.Player.Inventory.Add(BlockType.EmberOre, 70);
s22cSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround);
s22cSim.OrbitalCommand.SelectKind(SquadKind.Vanguard);
s22cSim.BeginOrbitalPick();
Check(s22cSim.SetOrbitalRally(12, 5, 16), "slice22 composed hold rally set");
Check(s22cSim.ConfirmOrbitalDeploy() != null, "slice22 composed hold deploys paid");
Eq(60, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed ore after hold deploy is 60");
s22cSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol);
s22cSim.OrbitalCommand.SelectKind(SquadKind.Recon);
s22cSim.BeginOrbitalPick();
Check(s22cSim.SetOrbitalRally(20, 5, 20), "slice22 composed patrol rally set");
Check(s22cSim.ConfirmOrbitalDeploy() != null, "slice22 composed patrol deploys paid");
Eq(50, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed ore after patrol deploy is 50");
s22cSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Advance);
s22cSim.OrbitalCommand.SelectKind(SquadKind.Heavy);
s22cSim.BeginOrbitalPick();
Check(s22cSim.SetOrbitalRally(10, 5, 10), "slice22 composed advance rally set");
Check(s22cSim.ConfirmOrbitalDeploy() != null, "slice22 composed advance deploys paid");
Eq(40, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed ore after advance deploy is 40");
Eq(3, s22cSim.World.Squads.Count, "slice22 composed cap is full at 3");
Eq(3, OrbitalView.Snapshot(s22cSim.World).Count, "slice22 composed snapshot lists 3");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s22cSim.World)[0].Activity, "slice22 composed hold reads marching before the march");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s22cSim.World)[1].Activity, "slice22 composed patrol reads patrolling before the march");
Eq(SquadActivity.Idle, OrbitalView.Snapshot(s22cSim.World)[2].Activity, "slice22 composed advance reads idle before the march");
Eq(90, s22cSim.World.Squads[0].Health, "slice22 composed hold health is 90");
Eq(40, s22cSim.World.Squads[1].Health, "slice22 composed patrol health is 40");
Eq(100, s22cSim.World.Squads[2].Health, "slice22 composed advance health is 100");
int[] s22cHoldX = { 15, 14, 13, 12, 12, 12, 12, 12, 12 };
int[] s22cPatrolX = { 17, 17, 18, 18, 19, 19, 20, 20, 21 };
int[] s22cPatrolZ = { 16, 17, 17, 18, 18, 19, 19, 20, 20 };
for (int s22cTick = 0; s22cTick < 9; s22cTick++)
{
    s22cSim.World.Advance();
    Eq(s22cHoldX[s22cTick], s22cSim.World.Squads[0].X, $"slice22 composed march tick {s22cTick + 1} hold x");
    Eq(16, s22cSim.World.Squads[0].Z, $"slice22 composed march tick {s22cTick + 1} hold z");
    Eq(s22cPatrolX[s22cTick], s22cSim.World.Squads[1].X, $"slice22 composed march tick {s22cTick + 1} patrol x");
    Eq(s22cPatrolZ[s22cTick], s22cSim.World.Squads[1].Z, $"slice22 composed march tick {s22cTick + 1} patrol z");
    Eq(16, s22cSim.World.Squads[2].X, $"slice22 composed march tick {s22cTick + 1} advance x stays");
    Eq(16, s22cSim.World.Squads[2].Z, $"slice22 composed march tick {s22cTick + 1} advance z stays");
    Eq(SquadActivity.Idle, OrbitalView.Snapshot(s22cSim.World)[2].Activity, $"slice22 composed march tick {s22cTick + 1} advance still idle");
}
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s22cSim.World)[0].Activity, "slice22 composed hold reads holding at its rally");
Eq(1, s22cSim.World.Squads[1].PatrolWaypointIndex, "slice22 composed patrol turns onto its loop at waypoint 1");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s22cSim.World)[1].Activity, "slice22 composed patrol reads patrolling on its loop");
Check(!s22cSim.TryDismissSquad(3), "slice22 composed out-of-range dismiss refused");
Check(!s22cSim.TryDismissSquad(-1), "slice22 composed negative dismiss refused");
Eq(40, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed refusals charge nothing");
Eq(3, s22cSim.World.Squads.Count, "slice22 composed refusals remove nothing");
Eq(12, s22cSim.World.Squads[0].X, "slice22 composed refusals leave the hold position");
Eq(21, s22cSim.World.Squads[1].X, "slice22 composed refusals leave the patrol position");
Eq(16, s22cSim.World.Squads[2].X, "slice22 composed refusals leave the advance position");
Check(s22cSim.TryDismissSquad(2), "slice22 composed advance dismisses");
Eq(2, s22cSim.World.Squads.Count, "slice22 composed count drops to 2 after the advance dismiss");
Eq(40, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed advance dismiss refunds none");
Eq(TacticalDoctrine.HoldGround, s22cSim.World.Squads[0].Doctrine, "slice22 composed hold remains first after the advance dismiss");
Eq(TacticalDoctrine.Patrol, s22cSim.World.Squads[1].Doctrine, "slice22 composed patrol remains second after the advance dismiss");
Eq(90, s22cSim.World.Squads[0].Health, "slice22 composed hold health unchanged by the advance dismiss");
Eq(40, s22cSim.World.Squads[1].Health, "slice22 composed patrol health unchanged by the advance dismiss");
s22cSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.DefendCore);
s22cSim.OrbitalCommand.SelectKind(SquadKind.Heavy);
s22cSim.BeginOrbitalPick();
Check(s22cSim.SetOrbitalRally(10, 5, 20), "slice22 composed defendcore rally set");
Check(s22cSim.ConfirmOrbitalDeploy() != null, "slice22 composed defendcore redeploys paid at the freed slot");
Eq(30, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed redeploy costs a fresh 10, ore now 30");
Eq(3, s22cSim.World.Squads.Count, "slice22 composed cap is full again at 3");
Eq(TacticalDoctrine.DefendCore, s22cSim.World.Squads[2].Doctrine, "slice22 composed redeployed squad is defendcore");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s22cSim.World)[2].Activity, "slice22 composed defendcore reads marching before any tick");
Check(s22cSim.TryDismissSquad(0), "slice22 composed holdground dismisses");
Eq(2, s22cSim.World.Squads.Count, "slice22 composed count drops to 2 after the hold dismiss");
Eq(30, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed hold dismiss refunds none");
Eq(TacticalDoctrine.Patrol, s22cSim.World.Squads[0].Doctrine, "slice22 composed patrol remains first after the hold dismiss");
Eq(TacticalDoctrine.DefendCore, s22cSim.World.Squads[1].Doctrine, "slice22 composed defendcore remains second after the hold dismiss");
Eq(21, s22cSim.World.Squads[0].X, "slice22 composed patrol position unchanged by the hold dismiss");
Eq(16, s22cSim.World.Squads[1].X, "slice22 composed defendcore position unchanged by the hold dismiss");
s22cSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol);
s22cSim.OrbitalCommand.SelectKind(SquadKind.Vanguard);
s22cSim.BeginOrbitalPick();
Check(s22cSim.SetOrbitalRally(12, 5, 16), "slice22 composed stuck patrol rally set");
Check(s22cSim.ConfirmOrbitalDeploy() != null, "slice22 composed stuck patrol deploys paid");
Eq(20, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed ore after stuck deploy is 20");
Eq(3, s22cSim.World.Squads.Count, "slice22 composed cap is full again at 3 with the stuck patrol");
s22cSim.World.Chunk.TrySet(15, 5, 16, BlockType.Stone);
s22cSim.World.Chunk.TrySet(16, 5, 17, BlockType.Stone);
s22cSim.World.Chunk.TrySet(16, 5, 15, BlockType.Stone);
int[] s22cStuckPatrolX = { 22, 23, 22, 22, 21 };
int[] s22cStuckPatrolZ = { 20, 20, 20, 21, 21 };
int[] s22cDefX = { 17, 17, 18, 18, 19 };
int[] s22cDefZ = { 16, 17, 17, 18, 18 };
int[] s22cStuckRefusals = { 1, 2, 3, 4, 5 };
int[] s22cStuckWaypoint = { 1, 2, 3, 4, 0 };
for (int s22cTick = 0; s22cTick < 5; s22cTick++)
{
    s22cSim.World.Advance();
    Eq(s22cStuckPatrolX[s22cTick], s22cSim.World.Squads[0].X, $"slice22 composed stuck tick {s22cTick + 1} patrol x");
    Eq(s22cStuckPatrolZ[s22cTick], s22cSim.World.Squads[0].Z, $"slice22 composed stuck tick {s22cTick + 1} patrol z");
    Eq(s22cDefX[s22cTick], s22cSim.World.Squads[1].X, $"slice22 composed stuck tick {s22cTick + 1} defendcore x");
    Eq(s22cDefZ[s22cTick], s22cSim.World.Squads[1].Z, $"slice22 composed stuck tick {s22cTick + 1} defendcore z");
    Eq(16, s22cSim.World.Squads[2].X, $"slice22 composed stuck tick {s22cTick + 1} stuck x unmoved");
    Eq(16, s22cSim.World.Squads[2].Z, $"slice22 composed stuck tick {s22cTick + 1} stuck z unmoved");
    Eq(s22cStuckRefusals[s22cTick], s22cSim.World.Squads[2].PatrolConsecutiveRefusals, $"slice22 composed stuck tick {s22cTick + 1} refusals");
    Eq(s22cStuckWaypoint[s22cTick], s22cSim.World.Squads[2].PatrolWaypointIndex, $"slice22 composed stuck tick {s22cTick + 1} waypoint");
}
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s22cSim.World)[2].Activity, "slice22 composed stuck patrol reads stuck after five refusals");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s22cSim.World)[0].Activity, "slice22 composed moving patrol still reads patrolling beside the stuck patrol");
Eq(SquadActivity.Marching, OrbitalView.Snapshot(s22cSim.World)[1].Activity, "slice22 composed defendcore still reads marching beside the stuck patrol");
var s22cPreSave = s22cSim.SaveToJson();
var s22cPreLoaded = new Simulation(99);
Check(s22cPreLoaded.LoadFromJson(s22cPreSave), "slice22 composed pre-removal save loads");
Eq(3, s22cPreLoaded.World.Squads.Count, "slice22 composed pre-removal save keeps three squads");
Eq(TacticalDoctrine.DefendCore, s22cPreLoaded.World.Squads[1].Doctrine, "slice22 composed pre-removal save keeps the defendcore");
Eq(19, s22cPreLoaded.World.Squads[1].X, "slice22 composed pre-removal defendcore position kept");
Eq(18, s22cPreLoaded.World.Squads[1].Z, "slice22 composed pre-removal defendcore z kept");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s22cPreLoaded.World)[2].Activity, "slice22 composed pre-removal stuck patrol reads patrolling on load, not stuck");
Eq(0, s22cPreLoaded.World.Squads[2].PatrolWaypointIndex, "slice22 composed pre-removal stuck patrol index resets to 0 on load");
Eq(0, s22cPreLoaded.World.Squads[2].PatrolConsecutiveRefusals, "slice22 composed pre-removal stuck patrol counter resets to 0 on load");
Eq(12, s22cPreLoaded.World.Squads[2].RallyX, "slice22 composed pre-removal stuck patrol keeps its old rally on load");
Check(s22cSim.TryReRallyStuckPatrol(2, 22, 5, 16), "slice22 composed stuck patrol re-rallies free");
Eq(22, s22cSim.World.Squads[2].RallyX, "slice22 composed rescued rally is now 22");
Eq(0, s22cSim.World.Squads[2].PatrolWaypointIndex, "slice22 composed rescued waypoint resets to 0");
Eq(0, s22cSim.World.Squads[2].PatrolConsecutiveRefusals, "slice22 composed rescued counter resets to 0");
Eq(16, s22cSim.World.Squads[2].X, "slice22 composed rescued position unchanged, no teleport");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s22cSim.World)[2].Activity, "slice22 composed rescued patrol reads patrolling at once");
Eq(20, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed rescue charges nothing");
Eq(3, s22cSim.World.Squads.Count, "slice22 composed rescue frees no cap slot");
Check(s22cSim.TryDismissSquad(1), "slice22 composed defendcore dismisses beside the rescue");
Eq(2, s22cSim.World.Squads.Count, "slice22 composed count drops to 2 after the defendcore dismiss");
Eq(20, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed defendcore dismiss refunds none");
Eq(TacticalDoctrine.Patrol, s22cSim.World.Squads[0].Doctrine, "slice22 composed patrol remains first after the defendcore dismiss");
Eq(TacticalDoctrine.Patrol, s22cSim.World.Squads[1].Doctrine, "slice22 composed rescued patrol remains second after the defendcore dismiss");
Eq(21, s22cSim.World.Squads[0].X, "slice22 composed patrol position unchanged by the defendcore dismiss");
Eq(16, s22cSim.World.Squads[1].X, "slice22 composed rescued position unchanged by the defendcore dismiss");
Eq(22, s22cSim.World.Squads[1].RallyX, "slice22 composed rescued rally unchanged by the defendcore dismiss");
Eq(40, s22cSim.World.Squads[0].Health, "slice22 composed patrol health unchanged by the defendcore dismiss");
Eq(90, s22cSim.World.Squads[1].Health, "slice22 composed rescued health unchanged by the defendcore dismiss");
var s22cPostSave = s22cSim.SaveToJson();
var s22cPostLoaded = new Simulation(99);
Check(s22cPostLoaded.LoadFromJson(s22cPostSave), "slice22 composed post-removal save loads");
Eq(2, s22cPostLoaded.World.Squads.Count, "slice22 composed post-removal save has no defendcore entry");
Eq(TacticalDoctrine.Patrol, s22cPostLoaded.World.Squads[0].Doctrine, "slice22 composed post-removal save keeps the patrol");
Eq(TacticalDoctrine.Patrol, s22cPostLoaded.World.Squads[1].Doctrine, "slice22 composed post-removal save keeps the rescued patrol");
Eq(20, s22cPostLoaded.World.Squads[0].RallyX, "slice22 composed post-removal patrol rally kept");
Eq(22, s22cPostLoaded.World.Squads[1].RallyX, "slice22 composed post-removal rescued rally kept");
Eq(20, s22cPostLoaded.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed post-removal loaded ore is still 20");
int[] s22cRescuedX = { 17, 18, 19, 20, 21, 22 };
int[] s22cLoopPatrolX = { 21, 20, 20, 19, 19, 18 };
int[] s22cLoopPatrolZ = { 22, 22, 23, 23, 22, 22 };
for (int s22cTick = 0; s22cTick < 6; s22cTick++)
{
    s22cSim.World.Advance();
    Eq(s22cLoopPatrolX[s22cTick], s22cSim.World.Squads[0].X, $"slice22 composed rescued march tick {s22cTick + 1} patrol x");
    Eq(s22cLoopPatrolZ[s22cTick], s22cSim.World.Squads[0].Z, $"slice22 composed rescued march tick {s22cTick + 1} patrol z");
    Eq(s22cRescuedX[s22cTick], s22cSim.World.Squads[1].X, $"slice22 composed rescued march tick {s22cTick + 1} rescued x");
    Eq(16, s22cSim.World.Squads[1].Z, $"slice22 composed rescued march tick {s22cTick + 1} rescued z");
    Eq(0, s22cSim.World.Squads[1].PatrolWaypointIndex, $"slice22 composed rescued march tick {s22cTick + 1} rescued waypoint stays 0 toward its rally");
    Eq(22, s22cSim.World.Squads[1].PatrolTarget.X, $"slice22 composed rescued march tick {s22cTick + 1} rescued target is its new rally");
}
s22cSim.World.Advance();
Eq(23, s22cSim.World.Squads[1].X, "slice22 composed rescued loop tick steps east onto its new loop");
Eq(1, s22cSim.World.Squads[1].PatrolWaypointIndex, "slice22 composed rescued loop tick is waypoint 1");
Eq(25, s22cSim.World.Squads[1].PatrolTarget.X, "slice22 composed rescued loop target is the new rally plus radius 3");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s22cSim.World)[1].Activity, "slice22 composed rescued patrol reads patrolling on its new loop");
Check(s22cSim.TryDismissSquad(0), "slice22 composed moving patrol dismisses");
Eq(1, s22cSim.World.Squads.Count, "slice22 composed count drops to 1 after the moving patrol dismiss");
Eq(20, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed moving patrol dismiss refunds none");
Eq(23, s22cSim.World.Squads[0].X, "slice22 composed rescued position unchanged by the moving patrol dismiss");
Eq(22, s22cSim.World.Squads[0].RallyX, "slice22 composed rescued rally unchanged by the moving patrol dismiss");
Eq(1, s22cSim.World.Squads[0].PatrolWaypointIndex, "slice22 composed rescued waypoint unchanged by the moving patrol dismiss");
s22cSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround);
s22cSim.OrbitalCommand.SelectKind(SquadKind.Vanguard);
s22cSim.BeginOrbitalPick();
Check(s22cSim.SetOrbitalRally(20, 5, 16), "slice22 composed sacrificial rally set");
Check(s22cSim.ConfirmOrbitalDeploy() != null, "slice22 composed sacrificial hold deploys paid");
Eq(10, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed ore after sacrificial deploy is 10");
Eq(2, s22cSim.World.Squads.Count, "slice22 composed cap is 2 with the sacrificial squad");
var s22cSac = s22cSim.World.Squads[1];
s22cSac.TakeDamage(90);
Eq(0, s22cSac.Health, "slice22 composed sacrificial squad is dead at 0 HP");
Check(!s22cSim.TryDismissSquad(1), "slice22 composed dead squad dismiss refused");
Eq(2, s22cSim.World.Squads.Count, "slice22 composed dead refusal leaves the list at 2");
Eq(10, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed dead refusal changes no ore");
Eq(23, s22cSim.World.Squads[0].X, "slice22 composed dead refusal leaves the rescued patrol");
s22cSim.World.Advance();
Eq(1, s22cSim.World.Squads.Count, "slice22 composed sweep tick removes the dead squad from the list");
Eq(24, s22cSim.World.Squads[0].X, "slice22 composed sweep tick steps the rescued patrol to 24");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s22cSim.World)[0].Activity, "slice22 composed rescued patrol still patrolling after the sweep");
Check(s22cSim.TryDismissSquad(0), "slice22 composed rescued patrol dismisses, emptying the field");
Eq(0, s22cSim.World.Squads.Count, "slice22 composed field is empty after the rescued dismiss");
Eq(0, OrbitalView.Snapshot(s22cSim.World).Count, "slice22 composed snapshot is empty after the rescued dismiss");
Eq(10, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed rescued dismiss refunds none");
Check(!s22cSim.TryDismissSquad(0), "slice22 composed empty-list dismiss refused");
Eq(0, s22cSim.World.Squads.Count, "slice22 composed empty refusal leaves the field empty");
Eq(10, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed empty refusal changes no ore");
s22cSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround);
s22cSim.OrbitalCommand.SelectKind(SquadKind.Vanguard);
s22cSim.BeginOrbitalPick();
Check(s22cSim.SetOrbitalRally(20, 5, 16), "slice22 composed final rally set");
Check(s22cSim.ConfirmOrbitalDeploy() != null, "slice22 composed final hold deploys paid with the last ore");
Eq(0, s22cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed ore is 0 after the final deploy");
Eq(1, s22cSim.World.Squads.Count, "slice22 composed cap is 1 with the rebuilt hold");
int[] s22cFinalX = { 17, 18, 19, 20 };
for (int s22cTick = 0; s22cTick < 4; s22cTick++)
{
    s22cSim.World.Advance();
    Eq(s22cFinalX[s22cTick], s22cSim.World.Squads[0].X, $"slice22 composed final march tick {s22cTick + 1} final hold x");
    Eq(16, s22cSim.World.Squads[0].Z, $"slice22 composed final march tick {s22cTick + 1} final hold z");
}
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s22cSim.World)[0].Activity, "slice22 composed final hold reads holding at its rally");
Eq(20, s22cSim.World.Squads[0].RallyX, "slice22 composed final hold rally is 20");
var s22cFinalSave = s22cSim.SaveToJson();
var s22cFinalLoaded = new Simulation(99);
Check(s22cFinalLoaded.LoadFromJson(s22cFinalSave), "slice22 composed final save loads");
Eq(1, s22cFinalLoaded.World.Squads.Count, "slice22 composed final save keeps one squad");
Eq(TacticalDoctrine.HoldGround, s22cFinalLoaded.World.Squads[0].Doctrine, "slice22 composed final save keeps the holdground");
Eq(20, s22cFinalLoaded.World.Squads[0].X, "slice22 composed final loaded hold position kept");
Eq(20, s22cFinalLoaded.World.Squads[0].RallyX, "slice22 composed final loaded hold rally kept");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s22cFinalLoaded.World)[0].Activity, "slice22 composed final loaded hold reads holding");
Eq(0, s22cFinalLoaded.World.Player.Inventory.Get(BlockType.EmberOre), "slice22 composed final loaded ore is 0");
s22cFinalLoaded.World.Advance();
Eq(20, s22cFinalLoaded.World.Squads[0].X, "slice22 composed final loaded tick 1 hold stays at its rally");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s22cFinalLoaded.World)[0].Activity, "slice22 composed final loaded tick 1 hold still holding");
Eq(3, TacticalSquad.PatrolLoopRadius, "slice22 composed loop radius is still 3, not retuned");
Eq(5, TacticalSquad.PatrolWaypointCount, "slice22 composed loop waypoint count is still 5, not retuned");

// ---- Slice 23 Step 1: the radius and waypoint decision, decided here (0.94.0) ----
// Decision, in one sentence: KEEP radius 3 and waypoint count 5 as one
// pair — no change, no split, no per-kind radius — because the audit
// below finds no radius-caused or loop-shape-caused refusal anywhere,
// and every proved stuck geometry traces to blocked step cells that,
// once cleared, let the same squad walk the same loop at the same
// values. The full rationale, rejected hypotheses, coupling answers,
// Windows reopen condition, and deliberately-not list are in
// DECISIONS.md; this section is the audit's proof set, not a re-telling
// of earlier slices: earlier stuck proofs are re-staged only where the
// audit needs their classification or their removal proof, and the
// range, circuit, and attempted radius-only cases are new here.
// Constants and the corrected range relation: the old radius comment
// said 3 is larger than every kind's range except Recon's 5. The table
// says otherwise for Vanguard, whose range is exactly 3 — so the
// comment is corrected in Orbital.cs in this step, and the relation is
// proved from the table here, not from the comment.
Eq(3, TacticalSquad.PatrolLoopRadius, "slice23 audit loop radius is 3");
Eq(5, TacticalSquad.PatrolWaypointCount, "slice23 audit loop waypoint count is 5");
Eq(3, SquadShapes.For(SquadKind.Vanguard).EngagementRange, "slice23 audit vanguard range is 3, equal to the radius, not below it");
Eq(5, SquadShapes.For(SquadKind.Recon).EngagementRange, "slice23 audit recon range is 5, above the radius");
Eq(2, SquadShapes.For(SquadKind.Heavy).EngagementRange, "slice23 audit heavy range is 2, below the radius");
// Fire relation from the rally, enemy on the east waypoint at distance 3:
// Vanguard engages at exactly its range, Recon within its range, Heavy
// does not — and no kind engages the opposite waypoint at distance 6
// from a waypoint, so "shoot across the loop" holds only in the
// rally-to-waypoint sense, not across the diameter.
var s23RangeWorld = new WorldZone(42);
s23RangeWorld.Player.X = 20; s23RangeWorld.Player.Z = 20;
var s23RangeVan = s23RangeWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
var s23RangeRec = s23RangeWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Recon)!;
var s23RangeHea = s23RangeWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Heavy)!;
Check(s23RangeVan.FindTarget(new[] { new Enemy(23, 5, 20) }) != null, "slice23 audit vanguard on its rally engages the east waypoint enemy at exactly range 3");
Check(s23RangeRec.FindTarget(new[] { new Enemy(23, 5, 20) }) != null, "slice23 audit recon on its rally engages the east waypoint enemy within range 5");
Check(s23RangeHea.FindTarget(new[] { new Enemy(23, 5, 20) }) == null, "slice23 audit heavy on its rally does not engage the east waypoint enemy beyond range 2");
var s23AcrossWorld = new WorldZone(42);
s23AcrossWorld.Player.X = 23; s23AcrossWorld.Player.Z = 20;
var s23Across = s23AcrossWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Recon)!;
Check(s23Across.FindTarget(new[] { new Enemy(17, 5, 20) }) == null, "slice23 audit even recon on the east waypoint does not engage the opposite waypoint at distance 6");
// Circuit length, walked in open ground: rally to east 3, east to
// south 6, south to west 6, west to north 6, north to rally 3 — 24
// steps at radius 3, and 8 steps per added radius unit on this cross
// shape (8r), so a larger radius is never free.
var s23CircuitWorld = new WorldZone(42);
s23CircuitWorld.Player.X = 20; s23CircuitWorld.Player.Z = 20;
var s23Circuit = s23CircuitWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
for (int s23Tick = 0; s23Tick < 3; s23Tick++) s23Circuit.MoveForPatrol(s23CircuitWorld.Chunk, Array.Empty<Enemy>());
Eq(23, s23Circuit.X, "slice23 audit open circuit reaches the east waypoint in 3 steps");
Eq(20, s23Circuit.Z, "slice23 audit open circuit east waypoint Z");
for (int s23Tick = 0; s23Tick < 6; s23Tick++) s23Circuit.MoveForPatrol(s23CircuitWorld.Chunk, Array.Empty<Enemy>());
Eq(20, s23Circuit.X, "slice23 audit open circuit reaches the south waypoint 6 steps later");
Eq(23, s23Circuit.Z, "slice23 audit open circuit south waypoint Z");
for (int s23Tick = 0; s23Tick < 6; s23Tick++) s23Circuit.MoveForPatrol(s23CircuitWorld.Chunk, Array.Empty<Enemy>());
Eq(17, s23Circuit.X, "slice23 audit open circuit reaches the west waypoint 6 steps later");
Eq(20, s23Circuit.Z, "slice23 audit open circuit west waypoint Z");
for (int s23Tick = 0; s23Tick < 6; s23Tick++) s23Circuit.MoveForPatrol(s23CircuitWorld.Chunk, Array.Empty<Enemy>());
Eq(20, s23Circuit.X, "slice23 audit open circuit reaches the north waypoint 6 steps later");
Eq(17, s23Circuit.Z, "slice23 audit open circuit north waypoint Z");
for (int s23Tick = 0; s23Tick < 3; s23Tick++) s23Circuit.MoveForPatrol(s23CircuitWorld.Chunk, Array.Empty<Enemy>());
Eq(20, s23Circuit.X, "slice23 audit open circuit returns to the rally in 24 steps total");
Eq(20, s23Circuit.Z, "slice23 audit open circuit returns to the rally Z");
Eq(0, s23Circuit.PatrolWaypointIndex, "slice23 audit open circuit index is back to the rally waypoint");
Eq(0, s23Circuit.PatrolConsecutiveRefusals, "slice23 audit open circuit counts no refusals");
Check(!s23Circuit.IsPatrolStuck, "slice23 audit open circuit squad is not stuck");
// The loop is shared, not per-kind: Recon and Heavy walk the identical
// 24-step circuit from the same rally, so a per-kind radius would be a
// new derivation, not a reading of the code — PatrolTarget does not
// branch on kind, and these walks prove the movement does not either.
foreach (var s23Kind in new[] { SquadKind.Recon, SquadKind.Heavy })
{
    var s23KindWorld = new WorldZone(42);
    s23KindWorld.Player.X = 20; s23KindWorld.Player.Z = 20;
    var s23KindSquad = s23KindWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, s23Kind)!;
    for (int s23Tick = 0; s23Tick < 3; s23Tick++) s23KindSquad.MoveForPatrol(s23KindWorld.Chunk, Array.Empty<Enemy>());
    Eq(23, s23KindSquad.X, $"slice23 audit shared loop {s23Kind} reaches the east waypoint in 3 steps");
    for (int s23Tick = 0; s23Tick < 21; s23Tick++) s23KindSquad.MoveForPatrol(s23KindWorld.Chunk, Array.Empty<Enemy>());
    Eq(20, s23KindSquad.X, $"slice23 audit shared loop {s23Kind} returns to the rally in 24 steps");
    Eq(20, s23KindSquad.Z, $"slice23 audit shared loop {s23Kind} returns to the rally Z");
    Check(!s23KindSquad.IsPatrolStuck, $"slice23 audit shared loop {s23Kind} is not stuck");
}
// Audit A — route-blocked shape (Slice 20 shape, classified tick by
// tick): the squad starts at the player cell (24,20), east of its rally
// (20,20), with stone west, south, and north of it and the east step
// cell open. Every refusal's attempted step cell is stone; no refusal
// occurs with an open step cell, so none is loop-shape-caused. The one
// cell that is both a step cell and a waypoint — (23,20), the east
// waypoint — is counted once, as a blocked cell, not as a shape: at
// tick 1 it blocks the step toward the rally, and at tick 2 it is the
// target waypoint itself, still blocked by the same stone.
var s23RouteWorld = new WorldZone(42);
s23RouteWorld.Player.X = 24; s23RouteWorld.Player.Z = 20;
var s23Route = s23RouteWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(s23RouteWorld.Chunk.TrySet(23, 5, 20, BlockType.Stone), "slice23 audit route-blocked stages a west wall");
Check(s23RouteWorld.Chunk.TrySet(24, 5, 21, BlockType.Stone), "slice23 audit route-blocked stages a south wall");
Check(s23RouteWorld.Chunk.TrySet(24, 5, 19, BlockType.Stone), "slice23 audit route-blocked stages a north wall");
Eq((20, 5, 20), s23Route.PatrolTarget, "slice23 audit route-blocked tick 1 target is the rally");
Eq(BlockType.Stone, s23RouteWorld.Chunk.Get(23, 5, 20), "slice23 audit route-blocked tick 1 direct step cell is stone");
Check(!s23Route.MoveForPatrol(s23RouteWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit route-blocked tick 1 does not move");
Eq(1, s23Route.PatrolConsecutiveRefusals, "slice23 audit route-blocked tick 1 counts one refusal");
Eq(1, s23Route.PatrolWaypointIndex, "slice23 audit route-blocked tick 1 skips to the east waypoint");
Eq((23, 5, 20), s23Route.PatrolTarget, "slice23 audit route-blocked tick 2 target is the east waypoint");
Eq(BlockType.Stone, s23RouteWorld.Chunk.Get(23, 5, 20), "slice23 audit route-blocked tick 2 waypoint cell itself is stone");
Check(!s23Route.MoveForPatrol(s23RouteWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit route-blocked tick 2 does not move");
Eq(2, s23Route.PatrolConsecutiveRefusals, "slice23 audit route-blocked tick 2 counts two refusals");
Eq(2, s23Route.PatrolWaypointIndex, "slice23 audit route-blocked tick 2 skips to the south waypoint");
Eq((20, 5, 23), s23Route.PatrolTarget, "slice23 audit route-blocked tick 3 target is the south waypoint");
Eq(BlockType.Stone, s23RouteWorld.Chunk.Get(23, 5, 20), "slice23 audit route-blocked tick 3 direct step cell is stone");
Eq(BlockType.Stone, s23RouteWorld.Chunk.Get(24, 5, 21), "slice23 audit route-blocked tick 3 sidestep cell is stone");
Check(!s23Route.MoveForPatrol(s23RouteWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit route-blocked tick 3 does not move");
Eq(3, s23Route.PatrolConsecutiveRefusals, "slice23 audit route-blocked tick 3 counts three refusals");
Eq(3, s23Route.PatrolWaypointIndex, "slice23 audit route-blocked tick 3 skips to the west waypoint");
Eq((17, 5, 20), s23Route.PatrolTarget, "slice23 audit route-blocked tick 4 target is the west waypoint");
Eq(BlockType.Stone, s23RouteWorld.Chunk.Get(23, 5, 20), "slice23 audit route-blocked tick 4 direct step cell is stone");
Check(!s23Route.MoveForPatrol(s23RouteWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit route-blocked tick 4 does not move");
Eq(4, s23Route.PatrolConsecutiveRefusals, "slice23 audit route-blocked tick 4 counts four refusals");
Check(!s23Route.IsPatrolStuck, "slice23 audit route-blocked four refusals is not yet stuck, so the threshold is the count itself");
Eq(4, s23Route.PatrolWaypointIndex, "slice23 audit route-blocked tick 4 skips to the north waypoint");
Eq((20, 5, 17), s23Route.PatrolTarget, "slice23 audit route-blocked tick 5 target is the north waypoint");
Eq(BlockType.Stone, s23RouteWorld.Chunk.Get(23, 5, 20), "slice23 audit route-blocked tick 5 direct step cell is stone");
Eq(BlockType.Stone, s23RouteWorld.Chunk.Get(24, 5, 19), "slice23 audit route-blocked tick 5 sidestep cell is stone");
Check(!s23Route.MoveForPatrol(s23RouteWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit route-blocked tick 5 does not move");
Eq(5, s23Route.PatrolConsecutiveRefusals, "slice23 audit route-blocked tick 5 counts five refusals");
Eq(0, s23Route.PatrolWaypointIndex, "slice23 audit route-blocked tick 5 index cycles back to the rally waypoint");
Check(s23Route.IsPatrolStuck, "slice23 audit route-blocked five refusals is stuck");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s23RouteWorld)[0].Activity, "slice23 audit route-blocked reads stuck");
Eq(24, s23Route.X, "slice23 audit route-blocked squad holds its X");
Eq(20, s23Route.Z, "slice23 audit route-blocked squad holds its Z");
// Removal proof, same squad, same loop, same values: clearing only the
// three staged stones lets this squad march west to its rally and then
// walk the full loop back to it — 4 steps to the rally plus the 24-step
// circuit — with no re-rally, no new value, and no teleport.
Check(s23RouteWorld.Chunk.TrySet(23, 5, 20, BlockType.Air), "slice23 audit route-blocked removal clears the west wall");
Check(s23RouteWorld.Chunk.TrySet(24, 5, 21, BlockType.Air), "slice23 audit route-blocked removal clears the south wall");
Check(s23RouteWorld.Chunk.TrySet(24, 5, 19, BlockType.Air), "slice23 audit route-blocked removal clears the north wall");
Check(s23Route.MoveForPatrol(s23RouteWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit route-blocked cleared first step moves west");
Eq(23, s23Route.X, "slice23 audit route-blocked cleared first step reaches the cleared cell");
Eq(0, s23Route.PatrolConsecutiveRefusals, "slice23 audit route-blocked cleared step resets refusals");
Check(!s23Route.IsPatrolStuck, "slice23 audit route-blocked cleared squad is not stuck");
for (int s23Tick = 0; s23Tick < 27; s23Tick++) s23Route.MoveForPatrol(s23RouteWorld.Chunk, Array.Empty<Enemy>());
Eq(20, s23Route.X, "slice23 audit route-blocked cleared squad returns to its rally after the march plus the loop");
Eq(20, s23Route.Z, "slice23 audit route-blocked cleared squad returns to its rally Z");
Eq(0, s23Route.PatrolWaypointIndex, "slice23 audit route-blocked cleared loop index is back to the rally waypoint");
Eq(3, TacticalSquad.PatrolLoopRadius, "slice23 audit route-blocked cleared loop radius is still 3");
Eq(5, TacticalSquad.PatrolWaypointCount, "slice23 audit route-blocked cleared loop waypoint count is still 5");
// Audit B — fully enclosed shape (Slice 18 shape, classified): every
// refusal's direct step cell is stone while the target waypoint cell
// itself is air, so these are step-cell refusals, not waypoint
// refusals and not shape refusals — the loop's waypoints are all
// walkable, the squad simply cannot take a first step toward any of
// them. The threshold coupling is proved in the same run: four
// refusals read Patrolling, the fifth reads Stuck, because the
// threshold is PatrolWaypointCount itself.
var s23EnclosedWorld = new WorldZone(42);
s23EnclosedWorld.Player.X = 20; s23EnclosedWorld.Player.Z = 20;
var s23Enclosed = s23EnclosedWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s23EnclosedWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice23 audit enclosed stages an east wall");
Check(s23EnclosedWorld.Chunk.TrySet(19, 5, 20, BlockType.Stone), "slice23 audit enclosed stages a west wall");
Check(s23EnclosedWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice23 audit enclosed stages a south wall");
Check(s23EnclosedWorld.Chunk.TrySet(20, 5, 19, BlockType.Stone), "slice23 audit enclosed stages a north wall");
Eq((20, 5, 20), s23Enclosed.PatrolTarget, "slice23 audit enclosed target before the first tick is the rally itself, turning to east in that tick");
Eq(BlockType.Air, s23EnclosedWorld.Chunk.Get(23, 5, 20), "slice23 audit enclosed east waypoint cell itself is air");
Eq(BlockType.Stone, s23EnclosedWorld.Chunk.Get(21, 5, 20), "slice23 audit enclosed tick 1 direct step cell is stone");
Check(!s23Enclosed.MoveForPatrol(s23EnclosedWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit enclosed tick 1 does not move");
Eq(BlockType.Stone, s23EnclosedWorld.Chunk.Get(20, 5, 21), "slice23 audit enclosed tick 2 direct step cell is stone");
Eq(BlockType.Air, s23EnclosedWorld.Chunk.Get(20, 5, 23), "slice23 audit enclosed south waypoint cell itself is air");
Check(!s23Enclosed.MoveForPatrol(s23EnclosedWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit enclosed tick 2 does not move");
Eq(BlockType.Stone, s23EnclosedWorld.Chunk.Get(19, 5, 20), "slice23 audit enclosed tick 3 direct step cell is stone");
Eq(BlockType.Air, s23EnclosedWorld.Chunk.Get(17, 5, 20), "slice23 audit enclosed west waypoint cell itself is air");
Check(!s23Enclosed.MoveForPatrol(s23EnclosedWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit enclosed tick 3 does not move");
Eq(BlockType.Stone, s23EnclosedWorld.Chunk.Get(20, 5, 19), "slice23 audit enclosed tick 4 direct step cell is stone");
Eq(BlockType.Air, s23EnclosedWorld.Chunk.Get(20, 5, 17), "slice23 audit enclosed north waypoint cell itself is air");
Check(!s23Enclosed.MoveForPatrol(s23EnclosedWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit enclosed tick 4 does not move");
Eq(4, s23Enclosed.PatrolConsecutiveRefusals, "slice23 audit enclosed four refusals counted");
Check(!s23Enclosed.IsPatrolStuck, "slice23 audit enclosed four refusals is not stuck");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s23EnclosedWorld)[0].Activity, "slice23 audit enclosed four refusals still reads patrolling");
Check(!s23Enclosed.MoveForPatrol(s23EnclosedWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit enclosed tick 5 does not move");
Eq(5, s23Enclosed.PatrolConsecutiveRefusals, "slice23 audit enclosed five refusals counted");
Check(s23Enclosed.IsPatrolStuck, "slice23 audit enclosed five refusals is stuck, so keeping the count keeps the threshold");
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s23EnclosedWorld)[0].Activity, "slice23 audit enclosed five refusals reads stuck");
// Removal proof, same squad, same loop: clearing the four walls lets
// this squad walk on from its stuck index (south) and reach its rally
// in 18 steps via the remaining legs — south 3, west 6, north 6,
// rally 3 — with the counter reset on the first cleared step.
Check(s23EnclosedWorld.Chunk.TrySet(21, 5, 20, BlockType.Air), "slice23 audit enclosed removal clears the east wall");
Check(s23EnclosedWorld.Chunk.TrySet(19, 5, 20, BlockType.Air), "slice23 audit enclosed removal clears the west wall");
Check(s23EnclosedWorld.Chunk.TrySet(20, 5, 21, BlockType.Air), "slice23 audit enclosed removal clears the south wall");
Check(s23EnclosedWorld.Chunk.TrySet(20, 5, 19, BlockType.Air), "slice23 audit enclosed removal clears the north wall");
Check(s23Enclosed.MoveForPatrol(s23EnclosedWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit enclosed cleared first step moves south");
Eq(0, s23Enclosed.PatrolConsecutiveRefusals, "slice23 audit enclosed cleared step resets refusals");
for (int s23Tick = 0; s23Tick < 17; s23Tick++) s23Enclosed.MoveForPatrol(s23EnclosedWorld.Chunk, Array.Empty<Enemy>());
Eq(20, s23Enclosed.X, "slice23 audit enclosed cleared squad reaches its rally via the remaining loop legs");
Eq(20, s23Enclosed.Z, "slice23 audit enclosed cleared squad reaches its rally Z");
Check(!s23Enclosed.IsPatrolStuck, "slice23 audit enclosed cleared squad is not stuck");
// Audit C — blocked new loop (Slice 21 restick shape): a non-stuck
// patrol re-rallied to (26,20) and then enclosed at its old cell
// re-sticks on the new loop, and the new loop's waypoint cells are
// all air — so the second stuck reading is again step-cell-caused,
// and moving the loop's centre did not change the cause, only the
// loop that fails.
var s23RestickWorld = new WorldZone(42);
s23RestickWorld.Player.X = 20; s23RestickWorld.Player.Z = 20;
var s23Restick = s23RestickWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s23RestickWorld.TryReRallySquad(0, 26, 5, 20), "slice23 audit blocked-loop patrol accepts a re-rally to the new loop");
Check(s23RestickWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice23 audit blocked-loop stages an east wall");
Check(s23RestickWorld.Chunk.TrySet(19, 5, 20, BlockType.Stone), "slice23 audit blocked-loop stages a west wall");
Check(s23RestickWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice23 audit blocked-loop stages a south wall");
Check(s23RestickWorld.Chunk.TrySet(20, 5, 19, BlockType.Stone), "slice23 audit blocked-loop stages a north wall");
Eq(BlockType.Air, s23RestickWorld.Chunk.Get(29, 5, 20), "slice23 audit blocked-loop new east waypoint cell is air");
Eq(BlockType.Air, s23RestickWorld.Chunk.Get(26, 5, 23), "slice23 audit blocked-loop new south waypoint cell is air");
Eq(BlockType.Air, s23RestickWorld.Chunk.Get(23, 5, 20), "slice23 audit blocked-loop new west waypoint cell is air");
Eq(BlockType.Air, s23RestickWorld.Chunk.Get(26, 5, 17), "slice23 audit blocked-loop new north waypoint cell is air");
for (int s23Tick = 0; s23Tick < 5; s23Tick++) s23Restick.MoveForPatrol(s23RestickWorld.Chunk, Array.Empty<Enemy>());
Check(s23Restick.IsPatrolStuck, "slice23 audit blocked-loop patrol reads stuck again after a full circuit on the new loop");
Eq(20, s23Restick.X, "slice23 audit blocked-loop patrol never moved through a wall");
Eq(26, s23Restick.RallyX, "slice23 audit blocked-loop patrol keeps its new rally while stuck again");
// Removal proof, same squad, new loop: clearing the walls lets it
// march 6 steps to the new rally and then walk the new loop back to
// it in 24 more steps, at the unchanged radius and count.
Check(s23RestickWorld.Chunk.TrySet(21, 5, 20, BlockType.Air), "slice23 audit blocked-loop removal clears the east wall");
Check(s23RestickWorld.Chunk.TrySet(19, 5, 20, BlockType.Air), "slice23 audit blocked-loop removal clears the west wall");
Check(s23RestickWorld.Chunk.TrySet(20, 5, 21, BlockType.Air), "slice23 audit blocked-loop removal clears the south wall");
Check(s23RestickWorld.Chunk.TrySet(20, 5, 19, BlockType.Air), "slice23 audit blocked-loop removal clears the north wall");
for (int s23Tick = 0; s23Tick < 30; s23Tick++) s23Restick.MoveForPatrol(s23RestickWorld.Chunk, Array.Empty<Enemy>());
Eq(26, s23Restick.X, "slice23 audit blocked-loop cleared squad returns to its new rally after the march plus the loop");
Eq(20, s23Restick.Z, "slice23 audit blocked-loop cleared squad returns to its new rally Z");
Check(!s23Restick.IsPatrolStuck, "slice23 audit blocked-loop cleared squad is not stuck");
// Audit D — enemy ring, a case earlier slices staged only as single
// skips and fire proofs, now proved as a stuck cause: four living
// enemies on the four adjacent cells refuse every step exactly as
// stone does, so stuck is a blocked-cell reading, not a
// stone-specific one — and removing the enemies (an empty list, the
// same enemies having left) lets the same squad walk on.
var s23EnemyWorld = new WorldZone(42);
s23EnemyWorld.Player.X = 20; s23EnemyWorld.Player.Z = 20;
var s23EnemySquad = s23EnemyWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
var s23Ring = new[] { new Enemy(21, 5, 20), new Enemy(19, 5, 20), new Enemy(20, 5, 21), new Enemy(20, 5, 19) };
for (int s23Tick = 0; s23Tick < 5; s23Tick++) s23EnemySquad.MoveForPatrol(s23EnemyWorld.Chunk, s23Ring);
Check(s23EnemySquad.IsPatrolStuck, "slice23 audit enemy ring patrol is stuck after a full circuit of enemy-blocked steps");
Eq(20, s23EnemySquad.X, "slice23 audit enemy ring squad holds its X");
Eq(20, s23EnemySquad.Z, "slice23 audit enemy ring squad holds its Z");
Check(s23EnemySquad.MoveForPatrol(s23EnemyWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit enemy ring cleared first step moves south once the enemies leave");
Eq(0, s23EnemySquad.PatrolConsecutiveRefusals, "slice23 audit enemy ring cleared step resets refusals");
for (int s23Tick = 0; s23Tick < 17; s23Tick++) s23EnemySquad.MoveForPatrol(s23EnemyWorld.Chunk, Array.Empty<Enemy>());
Eq(20, s23EnemySquad.X, "slice23 audit enemy ring cleared squad reaches its rally via the remaining loop legs");
Eq(20, s23EnemySquad.Z, "slice23 audit enemy ring cleared squad reaches its rally Z");
Check(!s23EnemySquad.IsPatrolStuck, "slice23 audit enemy ring cleared squad is not stuck");
// Waypoint-caused is not stuck: a solid waypoint alone, and an
// out-of-bounds waypoint alone, each cost one skip and the loop still
// completes — the skip rule, not the radius, is what saves them.
// Solid waypoint: only the east waypoint (23,20) is stone.
var s23SolidWorld = new WorldZone(42);
s23SolidWorld.Player.X = 20; s23SolidWorld.Player.Z = 20;
var s23Solid = s23SolidWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
Check(s23SolidWorld.Chunk.TrySet(23, 5, 20, BlockType.Stone), "slice23 audit solid waypoint stages stone on the east waypoint only");
Check(s23Solid.MoveForPatrol(s23SolidWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit solid waypoint first step east moves");
Check(s23Solid.MoveForPatrol(s23SolidWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit solid waypoint second step east moves");
Check(!s23Solid.MoveForPatrol(s23SolidWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit solid waypoint step into the waypoint is refused");
Eq(1, s23Solid.PatrolConsecutiveRefusals, "slice23 audit solid waypoint refusal counts once");
Eq(2, s23Solid.PatrolWaypointIndex, "slice23 audit solid waypoint refusal skips to the south waypoint");
Check(!s23Solid.IsPatrolStuck, "slice23 audit solid waypoint single refusal is not stuck");
int s23SolidCalls = 0;
while (s23SolidCalls < 40 && !(s23Solid.X == 20 && s23Solid.Z == 20 && s23Solid.PatrolWaypointIndex == 0))
{
    s23Solid.MoveForPatrol(s23SolidWorld.Chunk, Array.Empty<Enemy>());
    s23SolidCalls++;
}
Eq(20, s23Solid.X, "slice23 audit solid waypoint loop returns to the rally X despite the solid waypoint");
Eq(20, s23Solid.Z, "slice23 audit solid waypoint loop returns to the rally Z");
Eq(0, s23Solid.PatrolWaypointIndex, "slice23 audit solid waypoint loop returns to the rally at index 0");
Eq(0, s23Solid.PatrolConsecutiveRefusals, "slice23 audit solid waypoint loop resets refusals on its first successful step after the skip");
Check(!s23Solid.IsPatrolStuck, "slice23 audit solid waypoint loop is not stuck");
Eq(BlockType.Stone, s23SolidWorld.Chunk.Get(23, 5, 20), "slice23 audit solid waypoint cell is still stone after the loop completes");
// Out-of-bounds waypoint, the radius-only attempt: rally (29,20) puts
// the east waypoint at (32,20), outside the 32-wide chunk, yet the
// loop still walks — shrinking the radius would move that waypoint
// inside, but it would not change stuck to walking, because the skip
// already handles the bad waypoint at radius 3. No radius-only case
// could be staged: a bad waypoint, at any radius, is a skip, and a
// stuck reading needs every waypoint refused, which needs blocked
// cells, not a radius.
var s23EdgeWorld = new WorldZone(42);
s23EdgeWorld.Player.X = 29; s23EdgeWorld.Player.Z = 20;
var s23Edge = s23EdgeWorld.SpawnSquad(29, 5, 20, TacticalDoctrine.Patrol)!;
Check(s23Edge.MoveForPatrol(s23EdgeWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit edge loop first step east moves");
Check(s23Edge.MoveForPatrol(s23EdgeWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit edge loop second step east moves");
Eq(31, s23Edge.X, "slice23 audit edge loop stands one cell inside the chunk edge");
Check(!s23Edge.MoveForPatrol(s23EdgeWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit edge loop step outside the chunk is refused");
Eq(1, s23Edge.PatrolConsecutiveRefusals, "slice23 audit edge loop out-of-bounds refusal counts once");
Eq(2, s23Edge.PatrolWaypointIndex, "slice23 audit edge loop out-of-bounds refusal skips to the south waypoint");
Check(!s23Edge.IsPatrolStuck, "slice23 audit edge loop single out-of-bounds refusal is not stuck");
int s23EdgeCalls = 0;
while (s23EdgeCalls < 40 && !(s23Edge.X == 29 && s23Edge.Z == 20 && s23Edge.PatrolWaypointIndex == 0))
{
    s23Edge.MoveForPatrol(s23EdgeWorld.Chunk, Array.Empty<Enemy>());
    s23EdgeCalls++;
}
Eq(29, s23Edge.X, "slice23 audit edge loop returns to its rally X despite the out-of-bounds waypoint");
Eq(20, s23Edge.Z, "slice23 audit edge loop returns to its rally Z");
Eq(0, s23Edge.PatrolWaypointIndex, "slice23 audit edge loop returns to its rally at index 0");
Check(!s23Edge.IsPatrolStuck, "slice23 audit edge loop is not stuck");
// Wall across the loop, the shape attempt: a stone wall at x=22
// spanning the loop's east leg does not stick the patrol either — the
// east leg is skipped and the remaining legs walk, at radius 3. At a
// smaller radius the east waypoint would sit on the wall cell itself,
// which is again a skip, not a stick, so the wall's position relative
// to the radius is not a walking-versus-sticking difference in either
// direction; the only difference a radius change would make here is
// which leg is skipped.
var s23WallWorld = new WorldZone(42);
s23WallWorld.Player.X = 20; s23WallWorld.Player.Z = 20;
var s23Wall = s23WallWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol)!;
for (int s23WallZ = 17; s23WallZ <= 23; s23WallZ++)
    Check(s23WallWorld.Chunk.TrySet(22, 5, s23WallZ, BlockType.Stone), $"slice23 audit wall across loop stages stone at x=22 z={s23WallZ}");
Check(s23Wall.MoveForPatrol(s23WallWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit wall across loop first step east moves");
Check(!s23Wall.MoveForPatrol(s23WallWorld.Chunk, Array.Empty<Enemy>()), "slice23 audit wall across loop step into the wall is refused");
Eq(1, s23Wall.PatrolConsecutiveRefusals, "slice23 audit wall across loop refusal counts once");
Eq(2, s23Wall.PatrolWaypointIndex, "slice23 audit wall across loop refusal skips to the south waypoint");
Check(!s23Wall.IsPatrolStuck, "slice23 audit wall across loop single refusal is not stuck");
int s23WallCalls = 0;
while (s23WallCalls < 40 && !(s23Wall.X == 20 && s23Wall.Z == 20 && s23Wall.PatrolWaypointIndex == 0))
{
    s23Wall.MoveForPatrol(s23WallWorld.Chunk, Array.Empty<Enemy>());
    s23WallCalls++;
}
Eq(20, s23Wall.X, "slice23 audit wall across loop returns to the rally X");
Eq(20, s23Wall.Z, "slice23 audit wall across loop returns to the rally Z");
Check(!s23Wall.IsPatrolStuck, "slice23 audit wall across loop is not stuck");
Eq(3, TacticalSquad.PatrolLoopRadius, "slice23 audit wall across loop radius is still 3, not retuned");
Eq(5, TacticalSquad.PatrolWaypointCount, "slice23 audit wall across loop waypoint count is still 5, not retuned");

// ---- Slice 23 Step 2: composed demo + review, closing Slice 23 (0.95.0) ----
// One Simulation mirroring the demo, tick by tick: a Heavy
// HoldGround marched away first, a Recon Patrol marched to its
// rally and walking one full 24-step loop at radius 3 and count
// 5, a Vanguard Patrol staged in the audited route-blocked
// shape beside it counting five step-cell refusals and reading
// Stuck, the same free re-rally Slice 20 decided, a fourth paid
// deploy refused at the full cap, the rescued march and loop
// turn, and a save/load round-trip preserving both loop
// centres. No enemy is staged, so no fire claim rides along.
var s23cSim = new Simulation(42);
s23cSim.World.Player.Inventory.Add(BlockType.EmberOre, 30);
s23cSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.HoldGround);
s23cSim.OrbitalCommand.SelectKind(SquadKind.Heavy);
s23cSim.BeginOrbitalPick();
Check(s23cSim.SetOrbitalRally(10, 5, 10), "slice23 composed hold rally set");
Check(s23cSim.ConfirmOrbitalDeploy() != null, "slice23 composed hold deploys paid");
Eq(20, s23cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice23 composed ore after hold deploy is 20");
Eq(100, s23cSim.World.Squads[0].Health, "slice23 composed hold health is 100");
int[] s23cHoldX = { 15, 15, 14, 14, 13, 13 };
int[] s23cHoldZ = { 16, 15, 15, 14, 14, 13 };
for (int s23cTick = 0; s23cTick < 6; s23cTick++)
{
    s23cSim.World.Advance();
    Eq(s23cHoldX[s23cTick], s23cSim.World.Squads[0].X, $"slice23 composed hold march tick {s23cTick + 1} hold x");
    Eq(s23cHoldZ[s23cTick], s23cSim.World.Squads[0].Z, $"slice23 composed hold march tick {s23cTick + 1} hold z");
    Eq(SquadActivity.Marching, OrbitalView.Snapshot(s23cSim.World)[0].Activity, $"slice23 composed hold march tick {s23cTick + 1} hold reads marching");
}
s23cSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol);
s23cSim.OrbitalCommand.SelectKind(SquadKind.Recon);
s23cSim.BeginOrbitalPick();
Check(s23cSim.SetOrbitalRally(20, 5, 20), "slice23 composed patrol rally set");
Check(s23cSim.ConfirmOrbitalDeploy() != null, "slice23 composed patrol deploys paid");
Eq(10, s23cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice23 composed ore after patrol deploy is 10");
Eq(40, s23cSim.World.Squads[1].Health, "slice23 composed patrol health is 40");
int[] s23cMarchHoldX = { 12, 12, 11, 11, 10, 10, 10, 10 };
int[] s23cMarchHoldZ = { 13, 12, 12, 11, 11, 10, 10, 10 };
int[] s23cMarchPatrolX = { 17, 17, 18, 18, 19, 19, 20, 20 };
int[] s23cMarchPatrolZ = { 16, 17, 17, 18, 18, 19, 19, 20 };
for (int s23cTick = 0; s23cTick < 8; s23cTick++)
{
    s23cSim.World.Advance();
    Eq(s23cMarchHoldX[s23cTick], s23cSim.World.Squads[0].X, $"slice23 composed patrol march tick {s23cTick + 1} hold x");
    Eq(s23cMarchHoldZ[s23cTick], s23cSim.World.Squads[0].Z, $"slice23 composed patrol march tick {s23cTick + 1} hold z");
    Eq(s23cMarchPatrolX[s23cTick], s23cSim.World.Squads[1].X, $"slice23 composed patrol march tick {s23cTick + 1} patrol x");
    Eq(s23cMarchPatrolZ[s23cTick], s23cSim.World.Squads[1].Z, $"slice23 composed patrol march tick {s23cTick + 1} patrol z");
    Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s23cSim.World)[1].Activity, $"slice23 composed patrol march tick {s23cTick + 1} patrol reads patrolling");
}
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s23cSim.World)[0].Activity, "slice23 composed hold reads holding at its rally after the patrol march");
Eq(0, s23cSim.World.Squads[1].PatrolWaypointIndex, "slice23 composed patrol is at its rally waypoint after the march");
Eq((20, 5, 20), s23cSim.World.Squads[1].PatrolTarget, "slice23 composed patrol target after the march is its rally");
int[] s23cLoopX = { 21, 22, 23, 22, 22, 21, 21, 20, 20, 19, 19, 18, 18, 17, 17, 18, 18, 19, 19, 20, 20, 20, 20, 20 };
int[] s23cLoopZ = { 20, 20, 20, 20, 21, 21, 22, 22, 23, 23, 22, 22, 21, 21, 20, 20, 19, 19, 18, 18, 17, 18, 19, 20 };
int[] s23cLoopIdx = { 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 0, 0, 0 };
for (int s23cTick = 0; s23cTick < 24; s23cTick++)
{
    s23cSim.World.Advance();
    Eq(s23cLoopX[s23cTick], s23cSim.World.Squads[1].X, $"slice23 composed loop tick {s23cTick + 1} patrol x");
    Eq(s23cLoopZ[s23cTick], s23cSim.World.Squads[1].Z, $"slice23 composed loop tick {s23cTick + 1} patrol z");
    Eq(s23cLoopIdx[s23cTick], s23cSim.World.Squads[1].PatrolWaypointIndex, $"slice23 composed loop tick {s23cTick + 1} patrol waypoint");
}
Eq(20, s23cSim.World.Squads[1].X, "slice23 composed loop returns to the rally X in 24 steps");
Eq(20, s23cSim.World.Squads[1].Z, "slice23 composed loop returns to the rally Z in 24 steps");
Eq(0, s23cSim.World.Squads[1].PatrolWaypointIndex, "slice23 composed loop index is back to the rally waypoint");
Eq(0, s23cSim.World.Squads[1].PatrolConsecutiveRefusals, "slice23 composed loop counts no refusals");
Check(!s23cSim.World.Squads[1].IsPatrolStuck, "slice23 composed loop patrol is not stuck");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s23cSim.World)[1].Activity, "slice23 composed loop patrol reads patrolling at its rally");
Eq(10, s23cSim.World.Squads[0].X, "slice23 composed loop hold stays at its rally X");
Eq(10, s23cSim.World.Squads[0].Z, "slice23 composed loop hold stays at its rally Z");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s23cSim.World)[0].Activity, "slice23 composed loop hold still reads holding");
s23cSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.Patrol);
s23cSim.OrbitalCommand.SelectKind(SquadKind.Vanguard);
s23cSim.BeginOrbitalPick();
Check(s23cSim.SetOrbitalRally(12, 5, 16), "slice23 composed stuck patrol rally set");
Check(s23cSim.ConfirmOrbitalDeploy() != null, "slice23 composed stuck patrol deploys paid");
Eq(0, s23cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice23 composed ore after stuck deploy is 0");
Eq(3, s23cSim.World.Squads.Count, "slice23 composed cap is full at 3 with the stuck patrol");
Eq((12, 5, 16), s23cSim.World.Squads[2].PatrolTarget, "slice23 composed stuck patrol target before staging is its rally");
Check(s23cSim.World.Chunk.TrySet(15, 5, 16, BlockType.Stone), "slice23 composed stuck stages a west wall");
Check(s23cSim.World.Chunk.TrySet(16, 5, 17, BlockType.Stone), "slice23 composed stuck stages a south wall");
Check(s23cSim.World.Chunk.TrySet(16, 5, 15, BlockType.Stone), "slice23 composed stuck stages a north wall");
Eq(BlockType.Stone, s23cSim.World.Chunk.Get(15, 5, 16), "slice23 composed stuck east waypoint cell is stone, the blocked cell counted once");
int[] s23cStuckWalkX = { 21, 22, 23, 22, 22 };
int[] s23cStuckWalkZ = { 20, 20, 20, 20, 21 };
int[] s23cStuckRefusals = { 1, 2, 3, 4, 5 };
int[] s23cStuckWaypoint = { 1, 2, 3, 4, 0 };
for (int s23cTick = 0; s23cTick < 5; s23cTick++)
{
    s23cSim.World.Advance();
    Eq(s23cStuckWalkX[s23cTick], s23cSim.World.Squads[1].X, $"slice23 composed stuck tick {s23cTick + 1} walking patrol x");
    Eq(s23cStuckWalkZ[s23cTick], s23cSim.World.Squads[1].Z, $"slice23 composed stuck tick {s23cTick + 1} walking patrol z");
    Eq(16, s23cSim.World.Squads[2].X, $"slice23 composed stuck tick {s23cTick + 1} stuck x unmoved");
    Eq(16, s23cSim.World.Squads[2].Z, $"slice23 composed stuck tick {s23cTick + 1} stuck z unmoved");
    Eq(s23cStuckRefusals[s23cTick], s23cSim.World.Squads[2].PatrolConsecutiveRefusals, $"slice23 composed stuck tick {s23cTick + 1} refusals");
    Eq(s23cStuckWaypoint[s23cTick], s23cSim.World.Squads[2].PatrolWaypointIndex, $"slice23 composed stuck tick {s23cTick + 1} waypoint");
    Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s23cSim.World)[1].Activity, $"slice23 composed stuck tick {s23cTick + 1} walking patrol still reads patrolling");
}
Eq(SquadActivity.Stuck, OrbitalView.Snapshot(s23cSim.World)[2].Activity, "slice23 composed stuck patrol reads stuck after five refusals");
Check(s23cSim.World.Squads[2].IsPatrolStuck, "slice23 composed stuck patrol is stuck");
Eq(22, s23cSim.World.Squads[1].X, "slice23 composed stuck phase walking patrol ends east on its loop");
Eq(21, s23cSim.World.Squads[1].Z, "slice23 composed stuck phase walking patrol ends south on its loop");
Eq(2, s23cSim.World.Squads[1].PatrolWaypointIndex, "slice23 composed stuck phase walking patrol waypoint is south");
Check(s23cSim.TryReRallyStuckPatrol(2, 22, 5, 16), "slice23 composed stuck patrol re-rallies free");
Eq(22, s23cSim.World.Squads[2].RallyX, "slice23 composed rescued rally is now 22");
Eq(0, s23cSim.World.Squads[2].PatrolWaypointIndex, "slice23 composed rescued waypoint resets to 0");
Eq(0, s23cSim.World.Squads[2].PatrolConsecutiveRefusals, "slice23 composed rescued counter resets to 0");
Eq(16, s23cSim.World.Squads[2].X, "slice23 composed rescued position unchanged, no teleport");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s23cSim.World)[2].Activity, "slice23 composed rescued patrol reads patrolling at once");
Eq(0, s23cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice23 composed rescue charges nothing");
Eq(3, s23cSim.World.Squads.Count, "slice23 composed rescue frees no cap slot");
s23cSim.OrbitalCommand.SelectDoctrine(TacticalDoctrine.DefendCore);
s23cSim.OrbitalCommand.SelectKind(SquadKind.Heavy);
s23cSim.BeginOrbitalPick();
Check(s23cSim.SetOrbitalRally(10, 5, 10), "slice23 composed fourth rally set");
Check(s23cSim.ConfirmOrbitalDeploy() == null, "slice23 composed fourth paid deploy at the full cap is refused");
s23cSim.CancelOrbitalPick();
Eq(0, s23cSim.World.Player.Inventory.Get(BlockType.EmberOre), "slice23 composed fourth refusal charges nothing");
Eq(3, s23cSim.World.Squads.Count, "slice23 composed fourth refusal adds no squad");
Eq(22, s23cSim.World.Squads[2].RallyX, "slice23 composed fourth refusal leaves the rescued rally");
int[] s23cRescuedX = { 17, 18, 19, 20, 21, 22 };
int[] s23cRescueWalkX = { 21, 21, 20, 20, 19, 19 };
int[] s23cRescueWalkZ = { 21, 22, 22, 23, 23, 22 };
int[] s23cRescueWalkIdx = { 2, 2, 2, 2, 3, 3 };
for (int s23cTick = 0; s23cTick < 6; s23cTick++)
{
    s23cSim.World.Advance();
    Eq(s23cRescueWalkX[s23cTick], s23cSim.World.Squads[1].X, $"slice23 composed rescued march tick {s23cTick + 1} walking patrol x");
    Eq(s23cRescueWalkZ[s23cTick], s23cSim.World.Squads[1].Z, $"slice23 composed rescued march tick {s23cTick + 1} walking patrol z");
    Eq(s23cRescueWalkIdx[s23cTick], s23cSim.World.Squads[1].PatrolWaypointIndex, $"slice23 composed rescued march tick {s23cTick + 1} walking patrol waypoint");
    Eq(s23cRescuedX[s23cTick], s23cSim.World.Squads[2].X, $"slice23 composed rescued march tick {s23cTick + 1} rescued x");
    Eq(16, s23cSim.World.Squads[2].Z, $"slice23 composed rescued march tick {s23cTick + 1} rescued z");
    Eq(0, s23cSim.World.Squads[2].PatrolWaypointIndex, $"slice23 composed rescued march tick {s23cTick + 1} rescued waypoint stays 0 toward its rally");
    Eq(22, s23cSim.World.Squads[2].PatrolTarget.X, $"slice23 composed rescued march tick {s23cTick + 1} rescued target is its new rally");
}
Eq(22, s23cSim.World.Squads[2].X, "slice23 composed rescued march reaches its new rally X");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s23cSim.World)[2].Activity, "slice23 composed rescued patrol reads patrolling at its new rally");
s23cSim.World.Advance();
Eq(23, s23cSim.World.Squads[2].X, "slice23 composed rescued loop tick steps east onto its new loop");
Eq(1, s23cSim.World.Squads[2].PatrolWaypointIndex, "slice23 composed rescued loop tick is waypoint 1");
Eq(25, s23cSim.World.Squads[2].PatrolTarget.X, "slice23 composed rescued loop target is the new rally plus radius 3");
Eq(18, s23cSim.World.Squads[1].X, "slice23 composed rescued loop tick walking patrol steps west on its loop");
Eq(22, s23cSim.World.Squads[1].Z, "slice23 composed rescued loop tick walking patrol z");
Eq(3, s23cSim.World.Squads[1].PatrolWaypointIndex, "slice23 composed rescued loop tick walking patrol waypoint is west");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s23cSim.World)[2].Activity, "slice23 composed rescued patrol reads patrolling on its new loop");
var s23cSave = s23cSim.SaveToJson();
var s23cLoaded = new Simulation(99);
Check(s23cLoaded.LoadFromJson(s23cSave), "slice23 composed save loads");
Eq(3, s23cLoaded.World.Squads.Count, "slice23 composed save keeps three squads");
Eq(10, s23cLoaded.World.Squads[0].RallyX, "slice23 composed loaded hold rally kept");
Eq(20, s23cLoaded.World.Squads[1].RallyX, "slice23 composed loaded walking patrol rally kept");
Eq(22, s23cLoaded.World.Squads[2].RallyX, "slice23 composed loaded rescued rally kept, not the old one");
Eq(10, s23cLoaded.World.Squads[0].X, "slice23 composed loaded hold position kept");
Eq(18, s23cLoaded.World.Squads[1].X, "slice23 composed loaded walking patrol position kept");
Eq(23, s23cLoaded.World.Squads[2].X, "slice23 composed loaded rescued position kept");
Eq(0, s23cLoaded.World.Squads[0].PatrolWaypointIndex, "slice23 composed loaded hold index is 0");
Eq(0, s23cLoaded.World.Squads[1].PatrolWaypointIndex, "slice23 composed loaded walking patrol index is 0");
Eq(0, s23cLoaded.World.Squads[2].PatrolWaypointIndex, "slice23 composed loaded rescued index is 0");
Eq(0, s23cLoaded.World.Player.Inventory.Get(BlockType.EmberOre), "slice23 composed loaded ore is still 0");
Eq(100, s23cLoaded.World.Squads[0].Health, "slice23 composed loaded hold health kept");
Eq(40, s23cLoaded.World.Squads[1].Health, "slice23 composed loaded walking patrol health kept");
Eq(90, s23cLoaded.World.Squads[2].Health, "slice23 composed loaded rescued health kept");
s23cLoaded.World.Advance();
Eq(10, s23cLoaded.World.Squads[0].X, "slice23 composed loaded tick 1 hold stays at its rally");
Eq(SquadActivity.Holding, OrbitalView.Snapshot(s23cLoaded.World)[0].Activity, "slice23 composed loaded tick 1 hold reads holding");
Eq(19, s23cLoaded.World.Squads[1].X, "slice23 composed loaded tick 1 walking patrol steps toward its rally");
Eq(22, s23cLoaded.World.Squads[1].Z, "slice23 composed loaded tick 1 walking patrol z");
Eq(SquadActivity.Patrolling, OrbitalView.Snapshot(s23cLoaded.World)[1].Activity, "slice23 composed loaded tick 1 walking patrol reads patrolling");
Eq(22, s23cLoaded.World.Squads[2].X, "slice23 composed loaded tick 1 rescued steps back to its new rally");
Eq(0, s23cLoaded.World.Squads[2].PatrolWaypointIndex, "slice23 composed loaded tick 1 rescued waypoint is 0 at its rally");
Eq(3, TacticalSquad.PatrolLoopRadius, "slice23 composed loop radius is still 3, not retuned");
Eq(5, TacticalSquad.PatrolWaypointCount, "slice23 composed loop waypoint count is still 5, not retuned");
Eq(3, SquadShapes.For(SquadKind.Vanguard).EngagementRange, "slice23 composed vanguard range is 3, equal to the radius");
Eq(5, SquadShapes.For(SquadKind.Recon).EngagementRange, "slice23 composed recon range is 5, above the radius");
Eq(2, SquadShapes.For(SquadKind.Heavy).EngagementRange, "slice23 composed heavy range is 2, below the radius");

// ---- Slice 25 Step 1: the loop-shape decision, decided here (0.101.0) ----
// Decision, in one sentence: KEEP the flat cardinal cross loop —
// rally, then east, south, west, and north at radius 3, then back
// to the rally, all on the rally's Y plane — because the audit
// below finds no shape-caused failure anywhere: a diagonal-leg
// block sidesteps where a cardinal-leg block skips, but neither
// sticks a squad, stuck stays a blocked-cell reading, the fire
// split follows from radius and Manhattan distance rather than
// from shape alone, and the unwalked corners are named and
// judged not to matter for a patrol route. The full rationale,
// coupling answers, rejected alternatives with mechanisms,
// Windows reopen condition, and checklist relation are in
// DECISIONS.md; this section is the audit's proof set.
// Audit A — coverage, walked cell by cell: the loop the code
// actually walks is a diamond perimeter, not the full square
// of side 6 around the rally and not the disc of radius 3.
// 24 steps visit 22 distinct cells (the start rally, the
// (22,20) stair-step cell, and the (20,18) rally-leg cell
// are each visited twice), every step stays on Y 5, and
// the four corner cells of the square — rally plus or
// minus 3 in both X and Z — are never visited, nor are
// the non-cardinal square-edge cells beside them. A
// patrol that never visits a corner cannot be said to
// patrol the square, and this audit says so plainly;
// the judgement that it does not matter is in
// DECISIONS.md, not hidden by omitting the corners.
var s25WalkWorld = new WorldZone(42);
s25WalkWorld.Player.X = 20; s25WalkWorld.Player.Z = 20;
var s25Walk = s25WalkWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
var s25Visited = new HashSet<(int X, int Z)> { (20, 20) };
for (int s25Tick = 0; s25Tick < 24; s25Tick++)
{
    s25Walk.MoveForPatrol(s25WalkWorld.Chunk, Array.Empty<Enemy>());
    s25Visited.Add((s25Walk.X, s25Walk.Z));
    Eq(5, s25Walk.Y, $"slice25 audit coverage tick {s25Tick + 1} stays on the rally Y plane");
}
Eq(20, s25Walk.X, "slice25 audit coverage circuit returns to the rally X in 24 steps");
Eq(20, s25Walk.Z, "slice25 audit coverage circuit returns to the rally Z in 24 steps");
Eq(0, s25Walk.PatrolWaypointIndex, "slice25 audit coverage circuit index is back to the rally waypoint");
Eq(0, s25Walk.PatrolConsecutiveRefusals, "slice25 audit coverage circuit counts no refusals");
Check(!s25Walk.IsPatrolStuck, "slice25 audit coverage circuit squad is not stuck");
Eq(22, s25Visited.Count, "slice25 audit coverage circuit visits 22 distinct cells in 24 steps");
Check(s25Visited.Contains((23, 20)), "slice25 audit coverage visits the east cardinal point");
Check(s25Visited.Contains((20, 23)), "slice25 audit coverage visits the south cardinal point");
Check(s25Visited.Contains((17, 20)), "slice25 audit coverage visits the west cardinal point");
Check(s25Visited.Contains((20, 17)), "slice25 audit coverage visits the north cardinal point");
Check(!s25Visited.Contains((23, 23)), "slice25 audit coverage never visits the south-east corner cell");
Check(!s25Visited.Contains((17, 23)), "slice25 audit coverage never visits the south-west corner cell");
Check(!s25Visited.Contains((23, 17)), "slice25 audit coverage never visits the north-east corner cell");
Check(!s25Visited.Contains((17, 17)), "slice25 audit coverage never visits the north-west corner cell");
Check(!s25Visited.Contains((23, 18)), "slice25 audit coverage never visits a non-cardinal east-edge cell");
Check(!s25Visited.Contains((23, 22)), "slice25 audit coverage never visits a non-cardinal east-edge cell, south side");
Check(!s25Visited.Contains((18, 23)), "slice25 audit coverage never visits a non-cardinal south-edge cell");
Check(!s25Visited.Contains((22, 23)), "slice25 audit coverage never visits a non-cardinal south-edge cell, east side");
// The same circuit, leg by leg, with the waypoint target
// at each arrival: the target is the waypoint just
// reached until the next tick turns onto the next leg.
var s25LegWorld = new WorldZone(42);
s25LegWorld.Player.X = 20; s25LegWorld.Player.Z = 20;
var s25Leg = s25LegWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Eq((20, 5, 20), s25Leg.PatrolTarget, "slice25 audit legs start target is the rally");
for (int s25Tick = 0; s25Tick < 3; s25Tick++) s25Leg.MoveForPatrol(s25LegWorld.Chunk, Array.Empty<Enemy>());
Eq(23, s25Leg.X, "slice25 audit legs reach the east waypoint in 3 steps");
Eq((23, 5, 20), s25Leg.PatrolTarget, "slice25 audit legs east arrival target is still the east waypoint");
for (int s25Tick = 0; s25Tick < 6; s25Tick++) s25Leg.MoveForPatrol(s25LegWorld.Chunk, Array.Empty<Enemy>());
Eq(20, s25Leg.X, "slice25 audit legs reach the south waypoint 6 steps later");
Eq(23, s25Leg.Z, "slice25 audit legs reach the south waypoint Z");
Eq((20, 5, 23), s25Leg.PatrolTarget, "slice25 audit legs south arrival target is still the south waypoint");
for (int s25Tick = 0; s25Tick < 6; s25Tick++) s25Leg.MoveForPatrol(s25LegWorld.Chunk, Array.Empty<Enemy>());
Eq(17, s25Leg.X, "slice25 audit legs reach the west waypoint 6 steps later");
Eq((17, 5, 20), s25Leg.PatrolTarget, "slice25 audit legs west arrival target is still the west waypoint");
for (int s25Tick = 0; s25Tick < 6; s25Tick++) s25Leg.MoveForPatrol(s25LegWorld.Chunk, Array.Empty<Enemy>());
Eq(20, s25Leg.X, "slice25 audit legs reach the north waypoint 6 steps later");
Eq(17, s25Leg.Z, "slice25 audit legs reach the north waypoint Z");
Eq((20, 5, 17), s25Leg.PatrolTarget, "slice25 audit legs north arrival target is still the north waypoint");
for (int s25Tick = 0; s25Tick < 3; s25Tick++) s25Leg.MoveForPatrol(s25LegWorld.Chunk, Array.Empty<Enemy>());
Eq(20, s25Leg.X, "slice25 audit legs return to the rally in 24 steps total");
Eq((20, 5, 20), s25Leg.PatrolTarget, "slice25 audit legs rally arrival target is the rally");
// Audit B1 — one block on a diagonal stair-step cell
// that is not itself a waypoint: the east-to-south leg
// stair-steps (22,20), (22,21), (21,21), (21,22),
// (20,22), (20,23). Blocking (22,21) does not skip
// the south waypoint and does not stick the squad:
// the direct step is refused, the sidestep onto the
// remaining gap axis (21,20) moves, refusals stay 0,
// the waypoint index stays 2, and the loop completes
// at the same radius and count. The shape's diagonal
// leg is, on this cell, more forgiving than a
// cardinal leg, not less — the opposite of a
// shape-caused stuck mechanism.
var s25DiagWorld = new WorldZone(42);
s25DiagWorld.Player.X = 20; s25DiagWorld.Player.Z = 20;
var s25Diag = s25DiagWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(s25DiagWorld.Chunk.TrySet(22, 5, 21, BlockType.Stone), "slice25 audit diagonal stages a stone on a stair-step cell, not a waypoint");
for (int s25Tick = 0; s25Tick < 3; s25Tick++) s25Diag.MoveForPatrol(s25DiagWorld.Chunk, Array.Empty<Enemy>());
Eq(23, s25Diag.X, "slice25 audit diagonal reaches the east waypoint before the blocked leg");
Check(s25Diag.MoveForPatrol(s25DiagWorld.Chunk, Array.Empty<Enemy>()), "slice25 audit diagonal first stair-step moves");
Eq(22, s25Diag.X, "slice25 audit diagonal first stair-step X");
Eq(BlockType.Stone, s25DiagWorld.Chunk.Get(22, 5, 21), "slice25 audit diagonal blocked cell is the next direct step cell");
Check(s25Diag.MoveForPatrol(s25DiagWorld.Chunk, Array.Empty<Enemy>()), "slice25 audit diagonal blocked stair-step still moves, by sidestep");
Eq(21, s25Diag.X, "slice25 audit diagonal sidestep lands on the remaining gap axis");
Eq(20, s25Diag.Z, "slice25 audit diagonal sidestep Z is unchanged");
Eq(0, s25Diag.PatrolConsecutiveRefusals, "slice25 audit diagonal sidestep counts no refusal");
Eq(2, s25Diag.PatrolWaypointIndex, "slice25 audit diagonal sidestep keeps the south waypoint");
Eq((20, 5, 23), s25Diag.PatrolTarget, "slice25 audit diagonal sidestep target is still the south waypoint");
for (int s25Tick = 0; s25Tick < 4; s25Tick++) s25Diag.MoveForPatrol(s25DiagWorld.Chunk, Array.Empty<Enemy>());
Eq(20, s25Diag.X, "slice25 audit diagonal sidestepped squad reaches the south waypoint");
Eq(23, s25Diag.Z, "slice25 audit diagonal sidestepped squad reaches the south waypoint Z");
for (int s25Tick = 0; s25Tick < 15; s25Tick++) s25Diag.MoveForPatrol(s25DiagWorld.Chunk, Array.Empty<Enemy>());
Eq(20, s25Diag.X, "slice25 audit diagonal sidestepped squad returns to the rally");
Eq(0, s25Diag.PatrolConsecutiveRefusals, "slice25 audit diagonal sidestepped squad counts no refusals on the full circuit");
Check(!s25Diag.IsPatrolStuck, "slice25 audit diagonal single stair-step block does not stick the squad");
// Audit B2 — the same single block on a cardinal leg,
// for contrast: blocking the middle rally-to-east
// step cell (22,20) skips the east waypoint, because
// a cardinal leg has no remaining gap axis to
// sidestep onto. The shape difference is real —
// skip versus sidestep — but it is a one-waypoint
// skip, not a stuck squad: the next leg moves at
// once, refusals reset, and the loop completes.
var s25CardWorld = new WorldZone(42);
s25CardWorld.Player.X = 20; s25CardWorld.Player.Z = 20;
var s25Card = s25CardWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(s25CardWorld.Chunk.TrySet(22, 5, 20, BlockType.Stone), "slice25 audit cardinal stages a stone on a cardinal step cell, not a waypoint");
Check(s25Card.MoveForPatrol(s25CardWorld.Chunk, Array.Empty<Enemy>()), "slice25 audit cardinal first step moves");
Eq(21, s25Card.X, "slice25 audit cardinal first step X");
Check(!s25Card.MoveForPatrol(s25CardWorld.Chunk, Array.Empty<Enemy>()), "slice25 audit cardinal blocked step does not move");
Eq(1, s25Card.PatrolConsecutiveRefusals, "slice25 audit cardinal blocked step counts one refusal");
Eq(2, s25Card.PatrolWaypointIndex, "slice25 audit cardinal blocked step skips to the south waypoint");
Eq(21, s25Card.X, "slice25 audit cardinal skipped squad holds its X");
Check(s25Card.MoveForPatrol(s25CardWorld.Chunk, Array.Empty<Enemy>()), "slice25 audit cardinal next leg moves at once");
Eq(21, s25Card.Z, "slice25 audit cardinal next leg steps south");
Eq(0, s25Card.PatrolConsecutiveRefusals, "slice25 audit cardinal next leg resets refusals");
for (int s25Tick = 0; s25Tick < 18; s25Tick++) s25Card.MoveForPatrol(s25CardWorld.Chunk, Array.Empty<Enemy>());
Eq(20, s25Card.X, "slice25 audit cardinal skipped squad returns to the rally");
Eq(20, s25Card.Z, "slice25 audit cardinal skipped squad returns to the rally Z");
Check(!s25Card.IsPatrolStuck, "slice25 audit cardinal single step-cell block does not stick the squad");
// Audit B3 — both a diagonal stair-step cell and
// its sidestep cell blocked: from (22,21) toward
// the south waypoint, the direct cell (21,21) and
// the sidestep cell (22,22) are both stone, so the
// south waypoint is skipped — exactly one refusal,
// exactly as a cardinal block skips one waypoint.
// The next leg then sidesteps around the staged
// stone (21,21) itself, refusals reset, and the
// loop completes: two blocked cells cost one skip,
// not a stuck squad, and no shape property beyond
// the blocked cells explains any refusal here.
var s25BothWorld = new WorldZone(42);
s25BothWorld.Player.X = 20; s25BothWorld.Player.Z = 20;
var s25Both = s25BothWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(s25BothWorld.Chunk.TrySet(21, 5, 21, BlockType.Stone), "slice25 audit both-blocked stages the direct stair-step cell");
Check(s25BothWorld.Chunk.TrySet(22, 5, 22, BlockType.Stone), "slice25 audit both-blocked stages the sidestep cell");
for (int s25Tick = 0; s25Tick < 5; s25Tick++) s25Both.MoveForPatrol(s25BothWorld.Chunk, Array.Empty<Enemy>());
Eq(22, s25Both.X, "slice25 audit both-blocked reaches the second stair-step cell");
Eq(21, s25Both.Z, "slice25 audit both-blocked reaches the second stair-step cell Z");
Check(!s25Both.MoveForPatrol(s25BothWorld.Chunk, Array.Empty<Enemy>()), "slice25 audit both-blocked step does not move");
Eq(1, s25Both.PatrolConsecutiveRefusals, "slice25 audit both-blocked step counts one refusal");
Eq(3, s25Both.PatrolWaypointIndex, "slice25 audit both-blocked step skips to the west waypoint");
Eq(22, s25Both.X, "slice25 audit both-blocked skipped squad holds its X");
Check(s25Both.MoveForPatrol(s25BothWorld.Chunk, Array.Empty<Enemy>()), "slice25 audit both-blocked next leg sidesteps around the staged stone");
Eq(22, s25Both.X, "slice25 audit both-blocked sidestep X");
Eq(20, s25Both.Z, "slice25 audit both-blocked sidestep Z");
Eq(0, s25Both.PatrolConsecutiveRefusals, "slice25 audit both-blocked sidestep resets refusals");
for (int s25Tick = 0; s25Tick < 14; s25Tick++) s25Both.MoveForPatrol(s25BothWorld.Chunk, Array.Empty<Enemy>());
Eq(20, s25Both.X, "slice25 audit both-blocked squad returns to the rally");
Eq(20, s25Both.Z, "slice25 audit both-blocked squad returns to the rally Z");
Check(!s25Both.IsPatrolStuck, "slice25 audit both-blocked pair costs one skip, not a stuck squad");
// Audit B4 — enclosed control: with every first
// step cell stone, no leg shape is ever exercised,
// yet the squad still reads Stuck after five
// refusals. Stuck is therefore a blocked-cell
// reading that holds with no shape in play at all,
// which is the shape-independence control for B1
// through B3, not a new stuck geometry.
var s25EncWorld = new WorldZone(42);
s25EncWorld.Player.X = 20; s25EncWorld.Player.Z = 20;
var s25Enc = s25EncWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(s25EncWorld.Chunk.TrySet(21, 5, 20, BlockType.Stone), "slice25 audit enclosed stages an east wall");
Check(s25EncWorld.Chunk.TrySet(19, 5, 20, BlockType.Stone), "slice25 audit enclosed stages a west wall");
Check(s25EncWorld.Chunk.TrySet(20, 5, 21, BlockType.Stone), "slice25 audit enclosed stages a south wall");
Check(s25EncWorld.Chunk.TrySet(20, 5, 19, BlockType.Stone), "slice25 audit enclosed stages a north wall");
for (int s25Tick = 0; s25Tick < 5; s25Tick++) Check(!s25Enc.MoveForPatrol(s25EncWorld.Chunk, Array.Empty<Enemy>()), $"slice25 audit enclosed tick {s25Tick + 1} does not move");
Eq(5, s25Enc.PatrolConsecutiveRefusals, "slice25 audit enclosed counts five refusals");
Check(s25Enc.IsPatrolStuck, "slice25 audit enclosed five refusals is stuck with no leg shape exercised");
Eq(20, s25Enc.X, "slice25 audit enclosed squad holds its X");
Eq(20, s25Enc.Z, "slice25 audit enclosed squad holds its Z");
// Audit C — fire asymmetry, computed in Manhattan
// distance, not asserted from the loop's looks.
// Current shape: rally to any waypoint is 3, every
// waypoint to every other waypoint is 6. So from
// the rally Vanguard engages a waypoint at exactly
// 3, Recon within 5, Heavy not beyond 2; from a
// waypoint no kind engages another waypoint, and
// the same split holds waypoint-to-rally, because
// Manhattan distance is symmetric. The split is
// therefore a radius property (3 against ranges
// 3, 5, 2) plus the waypoint separation (6), not
// a shape virtue: a different shape at the same
// radius that kept every waypoint at distance 3
// from the rally and 6 from every other waypoint
// would keep exactly this split.
Eq(3, TacticalSquad.PatrolLoopRadius, "slice25 audit fire loop radius is still 3");
Eq(5, TacticalSquad.PatrolWaypointCount, "slice25 audit fire loop waypoint count is still 5");
Eq(3, SquadShapes.For(SquadKind.Vanguard).EngagementRange, "slice25 audit fire vanguard range is 3");
Eq(5, SquadShapes.For(SquadKind.Recon).EngagementRange, "slice25 audit fire recon range is 5");
Eq(2, SquadShapes.For(SquadKind.Heavy).EngagementRange, "slice25 audit fire heavy range is 2");
var s25FireWorld = new WorldZone(42);
s25FireWorld.Player.X = 20; s25FireWorld.Player.Z = 20;
var s25FireVan = s25FireWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
var s25FireRec = s25FireWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Recon)!;
var s25FireHea = s25FireWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Heavy)!;
Check(s25FireVan.FindTarget(new[] { new Enemy(23, 5, 20) }) != null, "slice25 audit fire vanguard on its rally engages the east waypoint at exactly range 3");
Check(s25FireRec.FindTarget(new[] { new Enemy(23, 5, 20) }) != null, "slice25 audit fire recon on its rally engages the east waypoint within range 5");
Check(s25FireHea.FindTarget(new[] { new Enemy(23, 5, 20) }) == null, "slice25 audit fire heavy on its rally does not engage the east waypoint beyond range 2");
var s25EastWorld = new WorldZone(42);
s25EastWorld.Player.X = 23; s25EastWorld.Player.Z = 20;
var s25EastRec = s25EastWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Recon)!;
var s25EastVan = s25EastWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
Check(s25EastRec.FindTarget(new[] { new Enemy(20, 5, 23) }) == null, "slice25 audit fire recon on the east waypoint does not engage the south waypoint at distance 6");
Check(s25EastRec.FindTarget(new[] { new Enemy(17, 5, 20) }) == null, "slice25 audit fire recon on the east waypoint does not engage the west waypoint at distance 6");
Check(s25EastVan.FindTarget(new[] { new Enemy(20, 5, 23) }) == null, "slice25 audit fire vanguard on the east waypoint does not engage the south waypoint at distance 6");
Check(s25EastRec.FindTarget(new[] { new Enemy(20, 5, 20) }) != null, "slice25 audit fire recon on the east waypoint engages its rally at distance 3");
Check(s25EastVan.FindTarget(new[] { new Enemy(20, 5, 20) }) != null, "slice25 audit fire vanguard on the east waypoint engages its rally at exactly range 3");
// What a square perimeter would change, computed
// for a hypothetical corner waypoint (23,23) —
// this cell is not a waypoint in the code, and no
// shape is changed here; the distances are the
// mechanism for rejecting the square on fire
// grounds. From the east waypoint the corner is
// distance 3, so Vanguard and Recon would engage
// a corner waypoint from another waypoint, which
// the current shape never allows; from the rally
// the corner is distance 6, so no kind would
// engage it from the rally, which the current
// cardinal waypoints all allow for Vanguard and
// Recon. A square therefore inverts both halves
// of the current split at once.
Check(s25EastVan.FindTarget(new[] { new Enemy(23, 5, 23) }) != null, "slice25 audit fire hypothetical corner at distance 3 from the east waypoint is engaged by vanguard");
Check(s25EastRec.FindTarget(new[] { new Enemy(23, 5, 23) }) != null, "slice25 audit fire hypothetical corner at distance 3 from the east waypoint is engaged by recon");
Check(s25FireVan.FindTarget(new[] { new Enemy(23, 5, 23) }) == null, "slice25 audit fire hypothetical corner at distance 6 from the rally is engaged by no kind, vanguard");
Check(s25FireRec.FindTarget(new[] { new Enemy(23, 5, 23) }) == null, "slice25 audit fire hypothetical corner at distance 6 from the rally is engaged by no kind, recon");
// What a climbing loop would change, computed for
// a hypothetical raised east waypoint (23,6,20):
// one Y of climb turns Vanguard's exact-range 3
// into distance 4, so the kind that engages a
// flat waypoint at exactly its range would stop
// engaging it — the flat-Y invariant is load
// bearing for the fire split, not decorative.
Check(s25FireVan.FindTarget(new[] { new Enemy(23, 6, 20) }) == null, "slice25 audit fire hypothetical raised waypoint at distance 4 is beyond vanguard range 3");
Check(s25FireRec.FindTarget(new[] { new Enemy(23, 6, 20) }) != null, "slice25 audit fire hypothetical raised waypoint at distance 4 is still within recon range 5");
// Audit D — coupling: a general re-rally moves
// the loop's centre and re-derives the same
// shape from the new rally, changing nothing
// else. Radius, count, position, and the stuck
// threshold's coupling (threshold equals count,
// proved in B4 at five) all stand, and the new
// loop's east waypoint is the new rally plus
// radius 3, proved by walking onto it.
var s25RallyWorld = new WorldZone(42);
s25RallyWorld.Player.X = 20; s25RallyWorld.Player.Z = 20;
var s25Rally = s25RallyWorld.SpawnSquad(20, 5, 20, TacticalDoctrine.Patrol, SquadKind.Vanguard)!;
for (int s25Tick = 0; s25Tick < 3; s25Tick++) s25Rally.MoveForPatrol(s25RallyWorld.Chunk, Array.Empty<Enemy>());
Eq(23, s25Rally.X, "slice25 audit rerally squad reaches the east waypoint before the re-rally");
Check(s25RallyWorld.TryReRallySquad(0, 26, 5, 20), "slice25 audit rerally general re-rally is accepted");
Eq(26, s25Rally.RallyX, "slice25 audit rerally rally moves to the new rally X");
Eq(20, s25Rally.RallyZ, "slice25 audit rerally rally Z is unchanged");
Eq(23, s25Rally.X, "slice25 audit rerally squad does not teleport");
Eq(0, s25Rally.PatrolWaypointIndex, "slice25 audit rerally waypoint index resets to 0");
Eq((26, 5, 20), s25Rally.PatrolTarget, "slice25 audit rerally target is the new rally");
Eq(3, TacticalSquad.PatrolLoopRadius, "slice25 audit rerally radius is still 3 after the re-rally");
Eq(5, TacticalSquad.PatrolWaypointCount, "slice25 audit rerally waypoint count is still 5 after the re-rally");
for (int s25Tick = 0; s25Tick < 3; s25Tick++) s25Rally.MoveForPatrol(s25RallyWorld.Chunk, Array.Empty<Enemy>());
Eq(26, s25Rally.X, "slice25 audit rerally squad marches to its new rally");
Check(s25Rally.MoveForPatrol(s25RallyWorld.Chunk, Array.Empty<Enemy>()), "slice25 audit rerally loop tick moves onto the new loop");
Eq(27, s25Rally.X, "slice25 audit rerally new loop east step is the new rally plus one");
Eq((29, 5, 20), s25Rally.PatrolTarget, "slice25 audit rerally new loop east waypoint is the new rally plus radius 3");
Check(!s25Rally.IsPatrolStuck, "slice25 audit rerally squad on the new loop is not stuck");

Console.WriteLine($"\n{passed} passed, {failed} failed.");
return failed == 0 ? 0 : 1;
