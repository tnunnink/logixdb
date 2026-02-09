using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace LogixDb.Core;

/// <summary>
/// Provides extension methods for string and byte array manipulation, including compression,
/// decompression, and hashing operations used throughout the LogixDb system.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Compresses a string into a byte array using GZip compression.
    /// The string is first encoded using Unicode encoding before compression.
    /// </summary>
    /// <param name="text">The text string to compress.</param>
    /// <returns>A compressed byte array representation of the input text.</returns>
    internal static byte[] Compress(this string text)
    {
        var bytes = Encoding.Unicode.GetBytes(text);
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
        {
            msi.CopyTo(gs);
        }

        return mso.ToArray();
    }

    /// <summary>
    /// Decompresses a byte array back into a string using GZip decompression.
    /// The decompressed bytes are decoded using Unicode encoding to reconstruct the original string.
    /// </summary>
    /// <param name="bytes">The compressed byte array to decompress.</param>
    /// <returns>The decompressed string representation of the input bytes.</returns>
    internal static string Decompress(this byte[] bytes)
    {
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
        {
            gs.CopyTo(mso);
        }

        return Encoding.Unicode.GetString(mso.ToArray());
    }

    /// <summary>
    /// Generates an MD5 hash of the input string and returns it as a lowercase hexadecimal string.
    /// The input text is encoded using UTF-8 before hashing.
    /// </summary>
    /// <param name="text">The text string to hash.</param>
    /// <returns>A lowercase hexadecimal string representation of the MD5 hash.</returns>
    public static string Hash(this string text)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexStringLower(hash);
    }
}