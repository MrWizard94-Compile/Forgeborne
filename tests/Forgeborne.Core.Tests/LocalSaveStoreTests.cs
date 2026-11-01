using System.Text;
using Forgeborne.Core;

internal static class LocalSaveStoreTests
{
    public static void Run(Action<bool, string> check)
    {
        string root = Path.Combine(Path.GetTempPath(), "forgeborne-save-test-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(root, "slot.json");
        try
        {
            var sim = new Simulation(42);
            string first = sim.SaveToJson();
            LocalSaveStore.Write(path, first);
            check(LocalSaveStore.Read(path) == first, "save store writes and reads exact UTF-8 snapshot");
            sim.World.Player.Inventory.Add(BlockType.Stone, 7);
            string second = sim.SaveToJson();
            LocalSaveStore.Write(path, second);
            check(LocalSaveStore.Read(path) == second, "save store atomically replaces an existing slot");
            check(LocalSaveStore.Read(path + ".backup") == first, "save store preserves the prior slot as a backup");
            bool rejected = false;
            try { LocalSaveStore.Write(path, new string('x', LocalSaveStore.MaximumBytes + 1)); }
            catch (ArgumentException) { rejected = true; }
            check(rejected && LocalSaveStore.Read(path) == second, "save store rejects oversized writes without damaging the slot");
            rejected = false;
            try { LocalSaveStore.Write(path, ""); }
            catch (ArgumentException) { rejected = true; }
            check(rejected && LocalSaveStore.Read(path) == second, "save store rejects empty writes without damaging the slot");
            File.WriteAllBytes(Path.Combine(root, "bad.json"), new byte[] { 0xff, 0xfe, 0xff });
            rejected = false;
            try { LocalSaveStore.Read(Path.Combine(root, "bad.json")); }
            catch (DecoderFallbackException) { rejected = true; }
            check(rejected, "save store rejects malformed UTF-8 instead of silently replacing data");
            byte[] withBom = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(first)).ToArray();
            File.WriteAllBytes(Path.Combine(root, "bom.json"), withBom);
            check(LocalSaveStore.Read(Path.Combine(root, "bom.json")) == first, "save store accepts a UTF-8 BOM from exported older files");
            File.WriteAllBytes(Path.Combine(root, "large.json"), new byte[LocalSaveStore.MaximumBytes + 1]);
            rejected = false;
            try { LocalSaveStore.Read(Path.Combine(root, "large.json")); }
            catch (InvalidDataException) { rejected = true; }
            check(rejected, "save store bounds reads before allocating file content");
            check(Directory.GetFiles(root, "*.pending-*").Length == 0, "save store leaves no pending files after success or rejected input");
            check(sim.LoadFromJson(LocalSaveStore.Read(path + ".backup")) && sim.World.Player.Inventory.Get(BlockType.Stone) == 0,
                "save store backup restores through the production simulation load path");
        }
        finally
        {
            string fullRoot = Path.GetFullPath(root);
            string tempParent = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar);
            if (Path.GetDirectoryName(fullRoot) != tempParent || !Path.GetFileName(fullRoot).StartsWith("forgeborne-save-test-"))
                throw new InvalidOperationException("Refusing cleanup outside the exact test-owned directory.");
            if (Directory.Exists(fullRoot)) Directory.Delete(fullRoot, true);
        }
    }
}
