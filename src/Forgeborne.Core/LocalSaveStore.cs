using System.Text;

namespace Forgeborne.Core;

/// <summary>Bounded UTF-8 save files, atomic same-volume replacement and one recoverable prior slot.</summary>
public static class LocalSaveStore
{
    public const int MaximumBytes = 2 * 1024 * 1024;
    private static readonly UTF8Encoding Utf8 = new(false, true);

    public static void Write(string path, string json)
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Save content is empty.", nameof(json));
        if (Utf8.GetByteCount(json) > MaximumBytes) throw new ArgumentException("Save exceeds the 2 MiB limit.", nameof(json));
        string destination = Path.GetFullPath(path);
        string directory = Path.GetDirectoryName(destination) ?? throw new ArgumentException("Save has no parent directory.", nameof(path));
        Directory.CreateDirectory(directory);
        string pending = destination + ".pending-" + Guid.NewGuid().ToString("N");
        try
        {
            using (var stream = new FileStream(pending, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                byte[] bytes = Utf8.GetBytes(json);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            if (File.Exists(destination)) File.Replace(pending, destination, destination + ".backup");
            else File.Move(pending, destination);
        }
        finally
        {
            if (File.Exists(pending)) File.Delete(pending);
        }
    }

    public static string Read(string path)
    {
        using var stream = new FileStream(Path.GetFullPath(path), FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length == 0 || stream.Length > MaximumBytes) throw new InvalidDataException("Save must contain at most 2 MiB of UTF-8 JSON.");
        byte[] bytes = new byte[(int)stream.Length];
        int offset = 0;
        while (offset < bytes.Length)
        {
            int read = stream.Read(bytes, offset, bytes.Length - offset);
            if (read == 0) throw new EndOfStreamException("Save changed or ended while reading.");
            offset += read;
        }
        if (stream.ReadByte() != -1) throw new InvalidDataException("Save changed while reading.");
        int bom = bytes.Length >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf ? 3 : 0;
        return Utf8.GetString(bytes, bom, bytes.Length - bom);
    }
}
