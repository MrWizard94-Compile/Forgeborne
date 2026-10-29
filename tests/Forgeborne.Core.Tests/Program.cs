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
var gatherResult = sim.GatherAhead();
Check(gatherResult == GatherResult.Success, $"simulation gather ahead succeeds (got {gatherResult})");
Check(sim.BlocksGathered == 1, "simulation blocks gathered counts");
sim.World.Player.Inventory.Add(BlockType.Stone, 10);
var buildResult = sim.BuildWallAhead();
Check(buildResult == BuildResult.Success, $"simulation build wall ahead succeeds (got {buildResult})");
sim.World.SpawnWave(1);
Check(sim.World.Enemies.Count == 1, "simulation wave spawns 1 enemy");
for (int i = 0; i < 3; i++) sim.FireAtNearestEnemy();
Check(sim.EnemiesKilled == 1, "simulation fire kills enemy in 3 shots (30 HP, 10 damage)");
sim.World.DamageCore(100);
Check(sim.IsObjectiveComplete, "simulation objective complete at core 0 HP");
var simHud = sim.Hud();
Eq(100, simHud.ObjectivePercentCompleted, "simulation hud objective 100% after core destroyed");

// ---- Step 4: core attack + mesher benchmark ----
var sim2 = new Simulation(42);
// Move FPS close to the core by teleporting via a fresh controller position is not
// possible (position is private-set in FpsController), so test FireAtCore at spawn
// distance: it should fire but not damage (core is >10m away).
int coreBefore = sim2.World.CoreHealth;
sim2.FireAtCore();
Check(sim2.World.CoreHealth == coreBefore, "fire at core out of range does not damage core");
Check(sim2.Weapon.Mag == 41, "fire at core consumes ammo even out of range");
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
simGen.World.SpawnWave(1);
int enemyHpBefore = simGen.World.Enemies[0].Health;
simGen.FireAtNearestEnemy();
Eq(Math.Max(0, enemyHpBefore - genLoot.Damage), simGen.World.Enemies.Count == 0 ? 0 : simGen.World.Enemies[0].Health, "simulation fire deals generated damage, not fixed 10");
var simEquip = new Simulation(42);
Eq("Pulse Rifle", simEquip.Weapon.Name, "simulation default weapon is still Pulse Rifle");
Eq(10, simEquip.Weapon.Damage, "simulation default weapon damage still 10");
Check(simEquip.Equip(genLoot), "simulation equip generated weapon succeeds");
Eq(genLoot.Name, simEquip.Weapon.Name, "simulation equip changes weapon name");
Eq(genLoot.Damage, simEquip.Weapon.Damage, "simulation equip changes weapon damage");
Eq(genLoot.MagSize, simEquip.Weapon.Mag, "simulation equip gives full generated mag");
int equipMagBefore = simEquip.Weapon.Mag;
simEquip.FireAtNearestEnemy();
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
simSkill.World.SpawnWave(1);
simSkill.FireAtNearestEnemy();
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
// gather in between must not advance the bonus. Target cell is (16,3,17).
var simGather = new Simulation(42, null, fullBuild);
for (int i = 0; i < 4; i++)
{
    simGather.World.Chunk.TrySet(16, 3, 17, BlockType.Stone);
    Check(simGather.GatherAhead() == GatherResult.Success, "skill gather bonus setup gather succeeds");
}
Check(simGather.GatherAhead() == GatherResult.Air, "skill gather bonus failed gather is air");
simGather.World.Chunk.TrySet(16, 3, 17, BlockType.Stone);
Check(simGather.GatherAhead() == GatherResult.Success, "skill gather bonus 5th gather succeeds");
Eq(5, simGather.BlocksGathered, "skill gather counts 5 gathers, not bonus units");
Eq(6, simGather.World.Player.Inventory.Get(BlockType.Stone), "skill gather bonus gives 6 stone from 5 gathers (+20%)");

// ---- Silence instruction (recorded, not a test) ----

Console.WriteLine($"\n{passed} passed, {failed} failed.");
return failed == 0 ? 0 : 1;
