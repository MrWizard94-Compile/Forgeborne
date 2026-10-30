namespace Forgeborne.Core;

public enum GatherResult { Success, OutOfBounds, Air, NotGatherable }
public enum BuildResult { Success, OutOfBounds, NotAir, InsufficientResources, Floating, InvalidType }

public sealed class Enemy
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int Health { get; private set; } = 30;
    public const int Damage = 10;
    public bool IsDead => Health <= 0;

    public Enemy(int x, int y, int z) { X = x; Y = y; Z = z; }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead) return;
        Health = Math.Max(0, Health - amount);
    }

    /// <summary>One tick: move one voxel toward the player, on the
    /// axis with the largest gap, if that cell is free. No diagonal
    /// moves. Kept for callers that target the player; WorldZone
    /// uses TickTowardPosition with the per-tick target instead.</summary>
    public void TickToward(Player player, VoxelChunk chunk) =>
        TickTowardPosition(player.X, player.Y, player.Z, chunk, null);

    /// <summary>
    /// One tick of the single enemy movement rule, generalised in
    /// Slice 4 Step 1 from "toward the player" to "toward a target
    /// position": move one voxel on the axis with the largest gap
    /// (ties: X, then Y, then Z — the same tie-break as squad
    /// movement, so the path is deterministic). No diagonal moves.
    /// The step is refused if the enemy is already adjacent to its
    /// target (distance 0 or 1: the attack / adjacency-damage
    /// phases handle damage, so the enemy never moves into its
    /// target), if it would leave the chunk, enter a solid block,
    /// or enter a cell in <paramref name="occupied"/> (the player
    /// and every live squad, so an enemy pursuing one target
    /// cannot walk through — and overlap — another target on its
    /// way there). There is one movement rule, not a player rule
    /// and a squad rule that could drift apart.
    /// </summary>
    public void TickTowardPosition(int targetX, int targetY, int targetZ, VoxelChunk chunk,
        IReadOnlyList<(int X, int Y, int Z)>? occupied = null)
    {
        if (IsDead) return;
        int dx = targetX - X, dy = targetY - Y, dz = targetZ - Z;
        if (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) <= 1) return; // adjacent: damage phases handle it, do not move into target
        int nx = X, ny = Y, nz = Z;
        if (Math.Abs(dx) >= Math.Abs(dy) && Math.Abs(dx) >= Math.Abs(dz)) nx += Math.Sign(dx);
        else if (Math.Abs(dy) >= Math.Abs(dz)) ny += Math.Sign(dy);
        else nz += Math.Sign(dz);
        if (!VoxelChunk.InBounds(nx, ny, nz)) return;
        if (BlockInfo.IsSolid(chunk.Get(nx, ny, nz))) return;
        if (occupied != null)
            foreach (var (ox, oy, oz) in occupied)
                if (ox == nx && oy == ny && oz == nz) return;
        X = nx; Y = ny; Z = nz;
    }

    public bool IsAdjacentTo(Player player) =>
        Math.Abs(player.X - X) + Math.Abs(player.Y - Y) + Math.Abs(player.Z - Z) == 1;
}

/// <summary>
/// Slice 1 zone: one chunk, one player, enemies, gather / build rules.
/// All rules are deterministic and synchronous. No time, no Random.
/// </summary>
public sealed class WorldZone
{
    public int Seed { get; }
    public VoxelChunk Chunk { get; }
    public Player Player { get; }
    public List<Enemy> Enemies { get; } = new();
    /// <summary>Orbital Tactical squads deployed in this zone. Empty by default, so zones without squads behave exactly as before. Saved and loaded with the zone since 0.10.0.</summary>
    public List<TacticalSquad> Squads { get; } = new();
    public int Tick { get; private set; }
    /// <summary>Shields restored per tick near a workstation. Base 5; skill WorkstationRegenBonus raises it via Simulation.ApplySkills.</summary>
    public int WorkstationRegenPerTick { get; set; } = 5;
    /// <summary>Blocks gathered by Kinetic Mills this session (not saved: a loaded world starts at 0, like Simulation's session counters).</summary>
    public int KineticBlocksGathered { get; private set; }
    /// <summary>Blocks gathered by powered Kinetic Mills this session (not saved, same session-counter rule as KineticBlocksGathered). A powered mill's output counts here, never in KineticBlocksGathered, so the two rates stay separable in tests.</summary>
    public int PoweredBlocksGathered { get; private set; }
    /// <summary>Kinetic Mills destroyed by mismatched voltage this session (Slice 9 Step 3; not saved, same session-counter rule as the gather counters: a loaded world starts at 0 even though a destroyed mill stays destroyed, because destruction is world state in the chunk bytes, not a counter). No HUD line is added for this counter in this step — the HUD power line already drops, because a destroyed mill is no longer counted in TotalMills — and the scene-visibility question is checklist item 17, not a headless claim.</summary>
    public int DestroyedMills { get; private set; }
    /// <summary>Kinetic automation interval: mills act once every 5 ticks, so automation is real-time pressure relief, not instant or turn-based.</summary>
    public const int KineticIntervalTicks = 5;
    // Slice 1 objective core, from the reference image: DESTROY CORE.
    public int CoreX { get; } = 28;
    public int CoreY { get; } = 5;
    public int CoreZ { get; } = 28;
    public int CoreHealth { get; private set; } = 100;
    public void DamageCore(int amount) { if (amount > 0) CoreHealth = Math.Max(0, CoreHealth - amount); }
    /// <summary>Restore the core's health from a save file, clamped to 0..100. Used only by SaveSystem, so a damaged core stays damaged across a load instead of silently healing to 100.</summary>
    public void RestoreCoreHealth(int health) { CoreHealth = Math.Clamp(health, 0, 100); }
    public void RestoreTick(int tick) { Tick = Math.Max(0, tick); }

    public WorldZone(int seed)
    {
        Seed = seed;
        Chunk = VoxelChunk.Generate(seed);
        // Player starts in open air above the ground, near the centre.
        Player = new Player(VoxelChunk.Size / 2, 5, VoxelChunk.Size / 2);
    }

    public GatherResult Gather(int x, int y, int z)
    {
        if (!VoxelChunk.InBounds(x, y, z)) return GatherResult.OutOfBounds;
        var block = Chunk.Get(x, y, z);
        if (block == BlockType.Air) return GatherResult.Air;
        if (!BlockInfo.IsGatherable(block)) return GatherResult.NotGatherable;
        var overflow = Player.Inventory.Add(BlockInfo.GatherYield(block), 1);
        if (overflow > 0) return GatherResult.NotGatherable; // inventory full: do not consume the block
        Chunk.TrySet(x, y, z, BlockType.Air);
        return GatherResult.Success;
    }

    private static readonly (int, int, int)[] Neighbours =
        { (1, 0, 0), (-1, 0, 0), (0, 1, 0), (0, -1, 0), (0, 0, 1), (0, 0, -1) };

    /// <summary>
    /// Slice 9 Step 2 voltage delivery, derived every tick, never
    /// persisted — the Step 1 rule widened by cable distribution,
    /// not replaced: a cell's delivered voltage is LV when at
    /// least one face-adjacent cell (not diagonal) holds a
    /// Dynamo, and otherwise the single voltage its
    /// face-adjacent neighbours agree on, whether that voltage
    /// arrives from a Transformer output or from a Cable
    /// network. A direct Dynamo beats any converted or cabled
    /// voltage, as in Step 1: a mill touching both a Dynamo and
    /// an HV-outputting transformer (or an HV cable) still
    /// receives LV, so adding a transformer or a cable next to
    /// an existing powered pair never unpowers it. Where no
    /// direct Dynamo decides the cell, distinct voltages
    /// disagreeing at one cell resolve to None, not to the
    /// first in scan order: a mill touching an LV cable and an
    /// HV cable at once receives nothing, so a wiring conflict
    /// is visible as a dead mill, not hidden by an accident of
    /// neighbour order. Voltage is recomputed from the blocks
    /// on every tick, so removing a Dynamo drops a mill to
    /// kinetic cadence on the next tick, and the save format
    /// needs no voltage flag: Dynamo, Transformer, and Cable
    /// blocks are saved as chunk bytes like every block, and an
    /// old save's Dynamo loads at LV by the DynamoOutput rule.
    /// </summary>
    public Voltage DeliveredVoltage(int x, int y, int z)
    {
        foreach (var (dx, dy, dz) in Neighbours)
        {
            int nx = x + dx, ny = y + dy, nz = z + dz;
            if (VoxelChunk.InBounds(nx, ny, nz) && Chunk.Get(nx, ny, nz) == BlockType.Dynamo)
                return VoltageRules.DynamoOutput;
        }
        var candidates = new HashSet<Voltage>();
        foreach (var (dx, dy, dz) in Neighbours)
        {
            int nx = x + dx, ny = y + dy, nz = z + dz;
            if (!VoxelChunk.InBounds(nx, ny, nz)) continue;
            var neighbour = Chunk.Get(nx, ny, nz);
            Voltage v = Voltage.None;
            if (neighbour == BlockType.Transformer)
                v = TransformerOutput(nx, ny, nz, new HashSet<(int, int, int)>(), null);
            else if (neighbour == BlockType.Cable)
                v = CableNetworkVoltage(CableComponent(nx, ny, nz), null);
            if (v != Voltage.None) candidates.Add(v);
        }
        return candidates.Count == 1 ? candidates.First() : Voltage.None;
    }

    /// <summary>
    /// The connected Cable component containing one cable cell:
    /// every Cable cell reachable from it by face-adjacent
    /// steps, face only, no diagonals. Only Cable cells are in
    /// the component — a wall, workstation, mill, Dynamo, or
    /// Transformer is a boundary, never a conductor — so a
    /// non-cable block between two cables breaks the network
    /// rather than passing voltage through silently.
    /// </summary>
    private HashSet<(int, int, int)> CableComponent(int startX, int startY, int startZ)
    {
        var component = new HashSet<(int, int, int)>();
        var stack = new Stack<(int, int, int)>();
        stack.Push((startX, startY, startZ));
        while (stack.Count > 0)
        {
            var (x, y, z) = stack.Pop();
            if (!component.Add((x, y, z))) continue;
            foreach (var (dx, dy, dz) in Neighbours)
            {
                int nx = x + dx, ny = y + dy, nz = z + dz;
                if (VoxelChunk.InBounds(nx, ny, nz) && Chunk.Get(nx, ny, nz) == BlockType.Cable)
                    stack.Push((nx, ny, nz));
            }
        }
        return component;
    }

    /// <summary>
    /// Slice 9 Step 2 network voltage: the single voltage a
    /// connected Cable component carries, derived from the
    /// sources touching any of its cells — a Dynamo touching
    /// any cable in the component (LV), or a Transformer output
    /// touching any cable in it (its flipped input, computed
    /// with this component excluded as that transformer's
    /// input, so a transformer cannot feed the component it
    /// is itself fed by). No loss per hop and no length limit
    /// in this step: a cable ten cells long carries the same
    /// voltage as a cable one cell long, proved by test, so a
    /// failed delivery in this step means unconnected or
    /// conflicted, never "too far" or "too lossy". A component
    /// touched by two distinct voltages carries None — a
    /// mixed-voltage network is dead until the player separates
    /// it, rather than silently carrying the higher voltage or
    /// the first voltage the scan happened to reach. A
    /// component with no source carries None, and a component
    /// whose only source is an unpowered transformer likewise.
    /// <paramref name="excludedTransformer"/> is a transformer
    /// cell whose output must not count as a source for this
    /// component: it is the transformer whose input is being
    /// computed from this component, and counting it would let
    /// a transformer feed itself through the cable.
    /// </summary>
    private Voltage CableNetworkVoltage(HashSet<(int, int, int)> component,
        (int, int, int)? excludedTransformer)
    {
        var candidates = new HashSet<Voltage>();
        foreach (var (cx, cy, cz) in component)
        foreach (var (dx, dy, dz) in Neighbours)
        {
            int nx = cx + dx, ny = cy + dy, nz = cz + dz;
            if (!VoxelChunk.InBounds(nx, ny, nz)) continue;
            var neighbour = Chunk.Get(nx, ny, nz);
            if (neighbour == BlockType.Dynamo) candidates.Add(VoltageRules.DynamoOutput);
            else if (neighbour == BlockType.Transformer && (nx, ny, nz) != excludedTransformer)
            {
                var output = TransformerOutput(nx, ny, nz,
                    new HashSet<(int, int, int)>(), component);
                if (output != Voltage.None) candidates.Add(output);
            }
        }
        return candidates.Count == 1 ? candidates.First() : Voltage.None;
    }

    /// <summary>
    /// A transformer's output voltage: the flipped value of its
    /// input, where the input is LV when a face-adjacent cell
    /// holds a Dynamo — a direct source beats a converted one,
    /// so a transformer touching both a Dynamo and another
    /// transformer converts from the Dynamo — and otherwise
    /// the single voltage its other face-adjacent inputs agree
    /// on: another transformer's output, or a Cable component
    /// touching it (computed with this transformer excluded as
    /// that component's source, so a transformer cannot be fed
    /// by the cable it feeds). Distinct non-Dynamo inputs
    /// resolve to None, not to the first in neighbour order,
    /// for the same reason a mixed cable network resolves to
    /// None: a conflict must be visible, not scan-order luck.
    /// The visited set stops a ring of transformers with no
    /// Dynamo from recursing forever: every transformer in the
    /// ring reports None, because no input ever reaches a
    /// Dynamo. A transformer with no powered neighbour outputs
    /// None, not a guessed level, so an unpowered transformer
    /// converts nothing and powers nothing.
    /// <paramref name="excludedComponent"/> is a Cable component
    /// that must not count as this transformer's input: it is
    /// the component whose voltage is being computed from this
    /// transformer's output, and counting it would let the
    /// transformer be fed by the cable it feeds.
    /// </summary>
    private Voltage TransformerOutput(int x, int y, int z,
        HashSet<(int, int, int)> visited, HashSet<(int, int, int)>? excludedComponent)
    {
        if (!visited.Add((x, y, z))) return Voltage.None;
        foreach (var (dx, dy, dz) in Neighbours)
        {
            int nx = x + dx, ny = y + dy, nz = z + dz;
            if (VoxelChunk.InBounds(nx, ny, nz) && Chunk.Get(nx, ny, nz) == BlockType.Dynamo)
                return VoltageRules.Flipped(VoltageRules.DynamoOutput);
        }
        var candidates = new HashSet<Voltage>();
        foreach (var (dx, dy, dz) in Neighbours)
        {
            int nx = x + dx, ny = y + dy, nz = z + dz;
            if (!VoxelChunk.InBounds(nx, ny, nz)) continue;
            var neighbour = Chunk.Get(nx, ny, nz);
            if (neighbour == BlockType.Transformer)
            {
                var output = TransformerOutput(nx, ny, nz, visited, excludedComponent);
                if (output != Voltage.None) candidates.Add(output);
            }
            else if (neighbour == BlockType.Cable)
            {
                if (excludedComponent != null && excludedComponent.Contains((nx, ny, nz))) continue;
                var v = CableNetworkVoltage(CableComponent(nx, ny, nz), (x, y, z));
                if (v != Voltage.None) candidates.Add(v);
            }
        }
        if (candidates.Count != 1) return Voltage.None;
        return VoltageRules.Flipped(candidates.First());
    }

    /// <summary>
    /// Slice 5 Step 1 power rule, redefined in Slice 9 Step 1
    /// around the ladder: a cell is powered exactly when its
    /// delivered voltage is the mill's rated voltage (LV). A
    /// mill fed HV is therefore NOT powered — and, since Slice 9
    /// Step 3, it does not fall back to kinetic cadence either:
    /// it is destroyed by RunMismatchedDestruction on the tick
    /// the HV is delivered, so "not powered" for an HV mill is
    /// a transient state visible only before that tick's
    /// destruction phase, never a cadence. Step 2 widens
    /// delivery without changing this definition: a mill fed LV
    /// through a cable run is powered, exactly like a mill fed
    /// LV face-adjacent to a Dynamo, and a mill fed nothing
    /// through a broken or conflicted cable run is not — and,
    /// crucially, is not destroyed either, because no voltage
    /// delivered is not a mismatched voltage (see IsMismatched).
    /// The old face-adjacency facts still hold: a mill diagonally
    /// adjacent to a Dynamo is not powered, and a mill two cells
    /// from a Dynamo through another mill is not powered.
    /// </summary>
    public bool IsPowered(int x, int y, int z) => DeliveredVoltage(x, y, z) == VoltageRules.MillRated;

    /// <summary>
    /// Slice 9 Step 3 mismatch rule: a Kinetic Mill is mismatched
    /// exactly when it is delivered a voltage that is neither
    /// nothing nor its rated voltage. On the two-level ladder
    /// that is exactly HV — the overvoltage the blueprint's
    /// failure rule punishes — and it is the only delivered
    /// voltage that destroys. No voltage delivered (None: no
    /// source, a broken cable, or a mixed-voltage conflict)
    /// is not a mismatch: a starved or conflicted mill idles
    /// at kinetic cadence, it does not break, because the
    /// scope's undervoltage hypothesis is that a machine fed
    /// too little is a machine not running, not a machine
    /// breaking — and on this ladder there is no delivered
    /// voltage below LV at all, so "undervoltage" and "no
    /// delivery" are the same observable state, None. A mill
    /// fed LV directly, even while an HV source also touches
    /// it, is not mismatched either: direct-Dynamo precedence
    /// from Step 1 delivers LV, so DeliveredVoltage is LV,
    /// not HV. Non-mill cells are never mismatched, whatever
    /// voltage touches them: cables are voltage-blind carriers
    /// and transformers convert both levels (see DECISIONS.md).
    /// </summary>
    public bool IsMismatched(int x, int y, int z)
    {
        if (Chunk.Get(x, y, z) != BlockType.KineticMill) return false;
        var delivered = DeliveredVoltage(x, y, z);
        return delivered != Voltage.None && delivered != VoltageRules.MillRated;
    }

    /// <summary>Slice 10 Step 1, second powered consumer: a Workstation
    /// is powered exactly when it is delivered its own rated
    /// voltage (LV, VoltageRules.WorkstationRated). This is NOT
    /// IsPowered reused: IsPowered is mill-only by construction,
    /// and a workstation's power must be decided, not inherited.</summary>
    public bool IsWorkstationPowered(int x, int y, int z) =>
        Chunk.Get(x, y, z) == BlockType.Workstation && DeliveredVoltage(x, y, z) == VoltageRules.WorkstationRated;

    /// <summary>Slice 10 Step 1 mismatch, decided here, not inherited
    /// from the mill: a Workstation delivered a voltage that is
    /// neither nothing nor its rating (on this ladder, HV) is
    /// mismatched. Unlike a mill, a mismatched workstation is
    /// NOT destroyed: it is disabled for that tick (see
    /// EffectiveWorkstationRegen), and it works again as soon as
    /// it is rewired to LV or left unpowered. Rationale: a repair
    /// bench tripping offline on overvoltage is a recoverable
    /// wiring mistake; destroying the player's shield bench for
    /// the same mistake that destroys a mill would double-punish
    /// a support structure, and a destroyed bench would also
    /// remove the very counterplay (regen) a player needs to
    /// survive the mistake. No voltage (None) is not a mismatch:
    /// an unpowered workstation runs at its base rate.</summary>
    public bool IsWorkstationMismatched(int x, int y, int z)
    {
        if (Chunk.Get(x, y, z) != BlockType.Workstation) return false;
        var delivered = DeliveredVoltage(x, y, z);
        return delivered != Voltage.None && delivered != VoltageRules.WorkstationRated;
    }

    /// <summary>Slice 10 Step 1 effective regen, decided here: scan
    /// workstations within Manhattan distance 3 of the player.
    /// If any is powered (LV), regen is base + 5 (the powered
    /// bonus, additive with the skill bonus already in
    /// WorkstationRegenPerTick, not replacing it). If none is
    /// powered but any is unpowered, regen is base. If every near
    /// workstation is mismatched (HV), regen is 0 — disabled, not
    /// destroyed. If there is no near workstation, regen is 0.
    /// This runs in the existing workstation phase in Advance
    /// (after the enemy phase, before the squad phase), not in
    /// the powered-automation phase, because it is a player
    /// effect, not an excavation — and it produces nothing, so
    /// no per-machine inventory is needed or built.</summary>
    public int EffectiveWorkstationRegen()
    {
        bool any = false, powered = false, unpowered = false;
        for (int dx = -3; dx <= 3; dx++)
        for (int dy = -3; dy <= 3; dy++)
        for (int dz = -3; dz <= 3; dz++)
        {
            if (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) > 3) continue;
            int x = Player.X + dx, y = Player.Y + dy, z = Player.Z + dz;
            if (!VoxelChunk.InBounds(x, y, z) || Chunk.Get(x, y, z) != BlockType.Workstation) continue;
            any = true;
            if (IsWorkstationPowered(x, y, z)) powered = true;
            else if (DeliveredVoltage(x, y, z) == Voltage.None) unpowered = true;
        }
        if (!any) return 0;
        if (powered) return WorkstationRegenPerTick + 5;
        if (unpowered) return WorkstationRegenPerTick;
        return 0;
    }

    public BuildResult Build(int x, int y, int z, BlockType type)
    {
        if (type is not (BlockType.Wall or BlockType.Workstation or BlockType.KineticMill or BlockType.Dynamo or BlockType.Transformer or BlockType.Cable)) return BuildResult.InvalidType;
        if (!VoxelChunk.InBounds(x, y, z)) return BuildResult.OutOfBounds;
        if (Chunk.Get(x, y, z) != BlockType.Air) return BuildResult.NotAir;
        // Support rule: at least one neighbouring cell must be solid, or y == 0 ground (bedrock layer is solid anyway, but keep the rule explicit).
        bool supported = false;
        foreach (var (dx, dy, dz) in Neighbours)
        {
            int nx = x + dx, ny = y + dy, nz = z + dz;
            if (VoxelChunk.InBounds(nx, ny, nz) && BlockInfo.IsSolid(Chunk.Get(nx, ny, nz))) { supported = true; break; }
        }
        if (!supported) return BuildResult.Floating;
        if (type == BlockType.Wall)
        {
            if (!Player.Inventory.Remove(BlockType.Stone, 2)) return BuildResult.InsufficientResources;
        }
        else if (type == BlockType.KineticMill)
        {
            // Kinetic Mill: 10 Stone (frame) + 5 Wood (crank / gears).
            // Distinct from the workstation's 10 Stone + 5 EmberOre, so
            // the two machines compete for stone but not for each other.
            if (!Player.Inventory.Has(BlockType.Stone, 10) || !Player.Inventory.Has(BlockType.Wood, 5))
                return BuildResult.InsufficientResources;
            Player.Inventory.Remove(BlockType.Stone, 10);
            Player.Inventory.Remove(BlockType.Wood, 5);
        }
        else if (type == BlockType.Dynamo)
        {
            // Slice 5 Step 1 Dynamo: 5 EmberOre only, no Stone or Wood,
            // so it never competes with walls or mills for their frame
            // materials. It does compete, deliberately, with workstations
            // (also 5 EmberOre) and squads (10 EmberOre = two Dynamos):
            // power is a deliberate ore investment, not a free upgrade
            // to a mill the player already owns. The failed-build rule
            // matches the other machines: check before spending, so a
            // refused build spends nothing.
            if (!Player.Inventory.Has(BlockType.EmberOre, 5))
                return BuildResult.InsufficientResources;
            Player.Inventory.Remove(BlockType.EmberOre, 5);
        }
        else if (type == BlockType.Transformer)
        {
            // Slice 9 Step 1 Transformer: 5 EmberOre, the same order
            // as a Dynamo, so electrical builds keep competing with
            // workstations and squads for ore, not with walls and
            // mills for Stone and Wood. The double cost is honest:
            // a transformer on its own powers nothing — it only
            // converts a voltage that already exists — so what the
            // second 5 ore buys is the second voltage itself: the
            // only way to produce HV in this step, which a second
            // Dynamo cannot give, and which Step 2 cables will
            // carry and Step 3 will punish when mismatched. In
            // this step alone a transformer also buys indirect LV:
            // Dynamo -> transformer -> transformer -> mill runs
            // the mill at LV one cell further from its Dynamo.
            // Same check-before-spend rule as the other machines.
            if (!Player.Inventory.Has(BlockType.EmberOre, 5))
                return BuildResult.InsufficientResources;
            Player.Inventory.Remove(BlockType.EmberOre, 5);
        }
        else if (type == BlockType.Cable)
        {
            // Slice 9 Step 2 Cable: 1 EmberOre per cell, so a run
            // is priced by its length and competes with Dynamos,
            // transformers, workstations, and squads for ore, not
            // with walls and mills for Stone and Wood — the same
            // electrical pricing rationale as Step 1, scaled to
            // a per-cell block rather than a single machine. A
            // cable that cost nothing and occupied nothing would
            // not be distribution, it would be a longer
            // face-adjacency rule wearing a name; a cable priced
            // at a full Dynamo's 5 ore per cell would make even
            // a short run cost more than its source, so 1 ore is
            // the first-pass per-cell value, labelled as such,
            // not a balance claim. Same check-before-spend rule
            // as the other machines.
            if (!Player.Inventory.Has(BlockType.EmberOre, 1))
                return BuildResult.InsufficientResources;
            Player.Inventory.Remove(BlockType.EmberOre, 1);
        }
        else // Workstation: 10 Stone + 5 EmberOre
        {
            if (!Player.Inventory.Has(BlockType.Stone, 10) || !Player.Inventory.Has(BlockType.EmberOre, 5))
                return BuildResult.InsufficientResources;
            Player.Inventory.Remove(BlockType.Stone, 10);
            Player.Inventory.Remove(BlockType.EmberOre, 5);
        }
        Chunk.TrySet(x, y, z, type);
        return BuildResult.Success;
    }

    /// <summary>Spawn a pressure wave: N enemies at the zone edge, in open air.</summary>
    public void SpawnWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int x = i % 2 == 0 ? 1 : VoxelChunk.Size - 2;
            Enemies.Add(new Enemy(x, 5, 2 + i * 2 < VoxelChunk.Size ? 2 + i * 2 : VoxelChunk.Size - 2));
        }
    }

    /// <summary>Workstation effect: if a workstation block is within Manhattan distance 3 of the player, restore 5 shields per tick, capped at 85. This is the workstation's real, distinct effect — a wall only blocks movement, a workstation repairs shields, so building one and defending it is a real decision.</summary>
    public bool IsNearWorkstation()
    {
        for (int dx = -3; dx <= 3; dx++)
            for (int dy = -3; dy <= 3; dy++)
                for (int dz = -3; dz <= 3; dz++)
                {
                    if (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) > 3) continue;
                    int x = Player.X + dx, y = Player.Y + dy, z = Player.Z + dz;
                    if (VoxelChunk.InBounds(x, y, z) && Chunk.Get(x, y, z) == BlockType.Workstation) return true;
                }
        return false;
    }

    /// <summary>
    /// Deploy an Orbital Tactical squad: it spawns at the player's
    /// position and follows its doctrine toward the rally point.
    /// The rally must be in bounds; a rally inside a solid block is
    /// accepted (the squad then holds short of it, blocked) rather
    /// than rejected, so a bad rally is visible, not silent. An
    /// unknown doctrine is rejected. Returns null on rejection.
    /// This is the free spawn primitive, for headless tests and
    /// the demo only: it charges nothing and ignores the squad
    /// cap. The play path is TryDeployPaidSquad, which the
    /// picker uses; Unity play must never call this directly.
    /// </summary>
    public TacticalSquad? DeploySquad(int rallyX, int rallyY, int rallyZ,
        TacticalDoctrine doctrine = TacticalDoctrine.HoldGround, SquadKind kind = SquadKind.Vanguard)
    {
        if (!VoxelChunk.InBounds(rallyX, rallyY, rallyZ)) return null;
        if (!Enum.IsDefined(doctrine) || !Enum.IsDefined(kind)) return null;
        var squad = new TacticalSquad(Player.X, Player.Y, Player.Z, rallyX, rallyY, rallyZ, doctrine, kind);
        Squads.Add(squad);
        return squad;
    }

    /// <summary>
    /// The play deployment path (Slice 3 Step 2): deploy one squad
    /// under the squad economy. Checks, in order, rally bounds and
    /// doctrine/kind validity, then the live-squad cap, then
    /// affordability — and charges ONLY after every check passes,
    /// so a refused deploy leaves the inventory untouched and
    /// adds no squad: no partial charge, no half-deployed squad.
    /// A deploy that unexpectedly fails after charging (impossible
    /// for a bounds-checked rally with defined doctrine/kind, but
    /// guarded anyway) refunds the ore in full. Returns null on
    /// any refusal. Dead squads do not count toward the cap.
    /// </summary>
    public TacticalSquad? TryDeployPaidSquad(int rallyX, int rallyY, int rallyZ,
        TacticalDoctrine doctrine = TacticalDoctrine.HoldGround, SquadKind kind = SquadKind.Vanguard)
    {
        if (!VoxelChunk.InBounds(rallyX, rallyY, rallyZ)) return null;
        if (!Enum.IsDefined(doctrine) || !Enum.IsDefined(kind)) return null;
        if (SquadEconomy.LiveSquadCount(this) >= SquadEconomy.MaxConcurrentSquads) return null;
        int price = SquadEconomy.CostForKind(kind);
        if (!Player.Inventory.Has(BlockType.EmberOre, price)) return null;
        if (!Player.Inventory.Remove(BlockType.EmberOre, price)) return null;
        var squad = DeploySquad(rallyX, rallyY, rallyZ, doctrine, kind);
        if (squad == null) Player.Inventory.Add(BlockType.EmberOre, price);
        return squad;
    }

    /// <summary>
    /// Slice 4 Step 1 targeting rule, re-derived every tick, in
    /// WorldZone so the choice is testable through Advance: the
    /// nearest live target wins, by Manhattan distance — the
    /// player or any live squad, moving and holding squads alike
    /// (a marching squad is as valid a target as a holding one,
    /// or the Step 2 moving doctrine would be untargetable while
    /// it moves). Ties go to the player (strictly-less comparison
    /// keeps the player, the starting candidate), and ties between
    /// squads go to the first squad in deploy order. Dead squads
    /// are never targeted. The player therefore remains a valid
    /// target — chosen whenever nearest or tied, and re-chosen as
    /// soon as a nearer squad dies or moves away — rather than
    /// abandoned wholesale: an enemy ignores the player only for
    /// the ticks a squad is strictly nearer, which is exactly
    /// what lets a squad intercept or draw pressure off the
    /// player. No target is persisted: the save format is
    /// unchanged, because positions and health are saved and the
    /// target is recomputed from them on the next tick.
    /// </summary>
    private (int X, int Y, int Z) ChooseTarget(Enemy enemy)
    {
        int best = Math.Abs(Player.X - enemy.X) + Math.Abs(Player.Y - enemy.Y) + Math.Abs(Player.Z - enemy.Z);
        int bx = Player.X, by = Player.Y, bz = Player.Z;
        foreach (var squad in Squads)
        {
            if (squad.IsDead) continue;
            int d = Math.Abs(squad.X - enemy.X) + Math.Abs(squad.Y - enemy.Y) + Math.Abs(squad.Z - enemy.Z);
            if (d < best) { best = d; bx = squad.X; by = squad.Y; bz = squad.Z; }
        }
        return (bx, by, bz);
    }

    /// <summary>One world tick: enemies move toward their per-tick
    /// target (player or nearest live squad), then adjacent enemies
    /// damage the player, then the workstation effect applies, then
    /// squads move / engage / take adjacent-enemy damage (in that
    /// order, so a squad that kills its attacker that tick is not
    /// hit by it), then — every tick, before either automation
    /// phase — mismatched mills are destroyed (Slice 9 Step 3),
    /// then — every 5th tick — Kinetic Mills act, then
    /// — every tick — powered mills act (Slice 5 Step 1). A powered
    /// mill is handled ONLY by the powered phase, never by both:
    /// power replaces the 5-tick cadence, it does not stack with
    /// it, so a powered mill excavates once per tick, not twice
    /// on a 5th tick. Dead enemies and squads are removed.
    /// Damage reconciliation (Slice 4 Step 1): squad damage has
    /// exactly ONE path, the squad phase's "10 per surviving
    /// adjacent enemy, after the squad fires". The enemy phase
    /// adds no squad damage, so an enemy that pursues a squad and
    /// stands next to it is not counted twice for the same hit:
    /// it hits in the squad phase if still adjacent after the
    /// squad moves and fires, and not at all if the squad killed
    /// it first. Player damage stays proximity-based in the enemy
    /// phase (adjacent to the player, whatever the enemy's
    /// movement target was), matching pre-Slice 4 behaviour.</summary>
    public void Advance()
    {
        Tick++;
        var occupied = new List<(int X, int Y, int Z)>(Squads.Count + 1) { (Player.X, Player.Y, Player.Z) };
        foreach (var s in Squads)
            if (!s.IsDead) occupied.Add((s.X, s.Y, s.Z));
        foreach (var e in Enemies)
        {
            var target = ChooseTarget(e);
            e.TickTowardPosition(target.X, target.Y, target.Z, Chunk, occupied);
            if (e.IsAdjacentTo(Player)) Player.TakeDamage(Enemy.Damage);
        }
        Enemies.RemoveAll(e => e.IsDead);
        int workstationRegen = EffectiveWorkstationRegen();
        if (workstationRegen > 0) Player.RestoreShields(workstationRegen);
        RunSquads();
        RunMismatchedDestruction();
        if (Tick % KineticIntervalTicks == 0) RunKineticAutomation();
        RunPoweredAutomation();
    }

    /// <summary>
    /// Orbital squad phase, in squad list order: move by doctrine
    /// (HoldGround toward its rally, Advance toward its nearest
    /// living enemy — no rally fallback when no enemy lives),
    /// then fire at the nearest enemy in range (HoldGround only
    /// once holding; Advance whenever in range), then take 10
    /// damage per enemy still adjacent to the squad. A squad at
    /// 0 health is removed at the end of the phase.
    /// </summary>
    private void RunSquads()
    {
        foreach (var squad in Squads.ToList())
        {
            if (squad.IsDead) continue;
            if (squad.Doctrine == TacticalDoctrine.Advance) squad.MoveTowardNearestEnemy(Chunk, Enemies);
            else squad.MoveTowardRally(Chunk, Enemies);
            var target = squad.FindTarget(Enemies);
            if (target != null) AttackEnemy(target, squad.DamagePerTick);
            int adjacent = 0;
            foreach (var e in Enemies)
                if (!e.IsDead && Math.Abs(e.X - squad.X) + Math.Abs(e.Y - squad.Y) + Math.Abs(e.Z - squad.Z) == 1) adjacent++;
            if (adjacent > 0) squad.TakeDamage(adjacent * Enemy.Damage);
        }
        Squads.RemoveAll(s => s.IsDead);
    }

    /// <summary>
    /// Slice 2 automation tier 1 (Kinetic only): each Kinetic Mill
    /// excavates at most one adjacent gatherable block per activation,
    /// in fixed neighbour order, into the player's inventory. It reuses
    /// <see cref="Gather(int, int, int)"/>, so the same rules apply:
    /// bedrock, walls, workstations, and other mills are never gathered,
    /// and a full inventory consumes nothing. Mills are scanned in
    /// chunk-index order, so two mills reaching for the same block
    /// resolve deterministically — the first in scan order wins.
    /// No separate machine inventory: tier 1 output goes to the
    /// player's inventory, and Slice 5 Step 1 decided the powered
    /// behaviour keeps that rule rather than gaining a buffer.
    /// </summary>
    /// <summary>
    /// Slice 9 Step 3 mismatched-voltage destruction: every tick,
    /// each Kinetic Mill delivered a mismatched voltage (see
    /// IsMismatched — on this ladder, HV) is removed from the
    /// world, its cell becoming Air, in chunk-index scan order
    /// (y, then z, then x, the same order as the automation
    /// phases, so two mismatched mills resolve deterministically).
    /// A destroyed mill excavates nothing on that tick and never
    /// again, and its session counters stop advancing — it is
    /// simply gone before either automation phase runs, so it
    /// cannot gather powered, kinetic, or at all. Salvage is
    /// none: nothing of the mill's 10 Stone + 5 Wood is refunded
    /// to the player inventory, so a wiring mistake costs a whole
    /// mill and a rebuild costs a whole mill again; a refund would
    /// make destruction a tax the player can ignore, and a partial
    /// refund would be a second economy rule smuggled into a
    /// failure rule. Cascade is none: the mill's neighbours —
    /// its cable, its transformer, its Dynamo, and any other mill
    /// — are untouched by this mill's destruction, and each other
    /// mill is judged on its own delivered voltage on the same
    /// tick, so a destroyed branch mill does not take its
    /// surviving branch with it and a failure never eats the
    /// source the player built correctly. Destruction is world
    /// state, not a flag: the cell is Air in the chunk bytes, so
    /// a destroyed mill stays destroyed across save/load, with
    /// no resurrection and no refund on load that was not given
    /// on destruction — only the DestroyedMills session counter
    /// resets on load, like the gather counters. No adapter
    /// binding fires destruction: it is a core rule on a tick,
    /// not a key press.
    /// </summary>
    private void RunMismatchedDestruction()
    {
        for (int y = 0; y < VoxelChunk.Size; y++)
        for (int z = 0; z < VoxelChunk.Size; z++)
        for (int x = 0; x < VoxelChunk.Size; x++)
        {
            if (!IsMismatched(x, y, z)) continue;
            Chunk.TrySet(x, y, z, BlockType.Air);
            DestroyedMills++;
        }
    }

    /// <summary>
    /// Slice 5 Step 1 powered automation: every tick, each powered
    /// Kinetic Mill (receiving its rated voltage, per IsPowered —
    /// LV from a Dynamo or converted back through transformers)
    /// excavates at most one adjacent gatherable block, in the same
    /// fixed neighbour order and through the same reused
    /// <see cref="Gather(int, int, int)"/> as kinetic mills, into the
    /// same player inventory — so there is still one block-gathering
    /// rule, not a kinetic rule and an electrical rule that can
    /// drift apart, and no second inventory system: a full inventory
    /// consumes nothing, and a Dynamo, wall, workstation, or another
    /// mill is never excavated. The powered behaviour is cadence,
    /// not a new excavation: a powered mill runs every tick instead
    /// of every 5th tick, which is exactly what makes power visible
    /// against the proven kinetic baseline. A mill that loses its
    /// Dynamo falls back to the kinetic phase on the next tick,
    /// because power is derived, not stored. Since Slice 9 Step 3
    /// a mismatched (HV-fed) mill never reaches this phase: it is
    /// destroyed by RunMismatchedDestruction first, so the only
    /// mills this phase sees are rated-voltage mills.
    /// </summary>
    private void RunPoweredAutomation()
    {
        for (int y = 0; y < VoxelChunk.Size; y++)
        for (int z = 0; z < VoxelChunk.Size; z++)
        for (int x = 0; x < VoxelChunk.Size; x++)
        {
            if (Chunk.Get(x, y, z) != BlockType.KineticMill) continue;
            if (!IsPowered(x, y, z)) continue;
            foreach (var (dx, dy, dz) in Neighbours)
            {
                if (Gather(x + dx, y + dy, z + dz) == GatherResult.Success)
                {
                    PoweredBlocksGathered++;
                    break;
                }
            }
        }
    }

    private void RunKineticAutomation()
    {
        for (int y = 0; y < VoxelChunk.Size; y++)
        for (int z = 0; z < VoxelChunk.Size; z++)
        for (int x = 0; x < VoxelChunk.Size; x++)
        {
            if (Chunk.Get(x, y, z) != BlockType.KineticMill) continue;
            // Powered mills are handled by RunPoweredAutomation every
            // tick; running them here too would excavate twice on a
            // 5th tick. Power replaces the kinetic cadence.
            // Mismatched mills never reach this check: they were
            // destroyed by RunMismatchedDestruction earlier in
            // the same tick, so this phase only ever sees mills
            // delivered LV, or nothing at all.
            if (IsPowered(x, y, z)) continue;
            foreach (var (dx, dy, dz) in Neighbours)
            {
                if (Gather(x + dx, y + dy, z + dz) == GatherResult.Success)
                {
                    KineticBlocksGathered++;
                    break;
                }
            }
        }
    }

    public int AttackEnemy(Enemy enemy, int damage)
    {
        enemy.TakeDamage(damage);
        if (enemy.IsDead) Enemies.Remove(enemy);
        return enemy.Health;
    }
}
