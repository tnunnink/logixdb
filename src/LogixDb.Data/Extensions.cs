using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace LogixDb.Data;

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
    /// Computes the MD5 hash of the input text.
    /// The input string is encoded using UTF-8 before generating the hash.
    /// </summary>
    /// <param name="text">The input string to hash.</param>
    /// <returns>A byte array representing the computed MD5 hash of the input text.</returns>
    public static byte[] Hash(this string text)
    {
        return MD5.HashData(Encoding.UTF8.GetBytes(text));
    }

    /// <summary>
    /// Converts a byte array to its lowercase hexadecimal string representation.
    /// </summary>
    /// <param name="binary">The byte array to be converted.</param>
    /// <returns>A string containing the lowercase hexadecimal representation of the input byte array.</returns>
    public static string ToHexString(this byte[] binary)
    {
        return Convert.ToHexStringLower(binary);
    }

    /// <summary>
    /// Serializes a key-value pair into a string by concatenating the key and the formatted value
    /// with specific control characters for delimitation.
    /// </summary>
    /// <param name="field">The key-value pair to serialize, where the key is a string and the value can be any object.</param>
    /// <returns>A serialized string representation of the key-value pair, including control characters as delimiters.</returns>
    public static string SerializeField(this KeyValuePair<string, object?> field)
    {
        return '\u001E' + field.Key + '\u001F' + FormatValue(field.Value);

        static string FormatValue(object? value)
        {
            return value switch
            {
                null => "\u2400",
                string s => s.Replace("\r\n", "\n"),
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString() ?? string.Empty
            };
        }
    }
}