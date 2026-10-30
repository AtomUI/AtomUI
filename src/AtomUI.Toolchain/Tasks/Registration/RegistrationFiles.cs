using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AtomUI.Build.Tasks;

internal static class RegistrationFiles
{
    internal static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    internal static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexStringLower(SHA256.HashData(stream));
    }

    // Native linked proofs are immutable; ordinary phase receipts may replace their predecessor.
    internal static void CreateJson(string path, object value) => Write(path, Json(value), overwrite: false);
    internal static void ReplaceJson(string path, object value) => Write(path, Json(value), overwrite: true);
    internal static void ReplaceText(string path, string text) => Write(path, text, overwrite: true);

    private static string Json(object value) => JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });

    private static void Write(string path, string text, bool overwrite)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(Encoding.UTF8.GetBytes(text));
            }
            File.Move(temporary, path, overwrite);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }
}
