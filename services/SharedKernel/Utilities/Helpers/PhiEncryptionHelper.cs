using Microsoft.Extensions.Options;
using SharedKernel.Settings;
using System.Security.Cryptography;
using System.Text;

namespace SharedKernel.Utilities.Helpers;

public class PhiEncryptionHelper
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _encryptionKey;
    private readonly byte[] _blindIndexKey;

    public PhiEncryptionHelper(IOptions<PhiEncryptionSettings> options)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.EncryptionKey) || string.IsNullOrWhiteSpace(settings.BlindIndexKey))
            throw new InvalidOperationException("PhiEncryption settings are not configured.");

        _encryptionKey = Convert.FromBase64String(settings.EncryptionKey);
        _blindIndexKey = Convert.FromBase64String(settings.BlindIndexKey);

        if (_encryptionKey.Length != 32 || _blindIndexKey.Length != 32)
            throw new InvalidOperationException(
                $"PhiEncryption keys must decode to 32 bytes. Got EncryptionKey={_encryptionKey.Length}, BlindIndexKey={_blindIndexKey.Length}.");
    }

    public byte[] Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            throw new ArgumentException("Plaintext is required.", nameof(plaintext));

        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_encryptionKey, TagSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var blob = new byte[NonceSize + TagSize + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, blob, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, blob, NonceSize, TagSize);
        Buffer.BlockCopy(cipherBytes, 0, blob, NonceSize + TagSize, cipherBytes.Length);
        return blob;
    }

    public string Decrypt(byte[] blob)
    {
        if (blob == null || blob.Length <= NonceSize + TagSize)
            throw new ArgumentException("Invalid ciphertext blob.", nameof(blob));

        var nonce = blob.AsSpan(0, NonceSize);
        var tag = blob.AsSpan(NonceSize, TagSize);
        var cipherBytes = blob.AsSpan(NonceSize + TagSize);
        var plainBytes = new byte[cipherBytes.Length];

        using var aes = new AesGcm(_encryptionKey, TagSize);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }

    public byte[] ComputeBlindIndex(Guid tenantId, string normalizedValue)
    {
        if (string.IsNullOrEmpty(normalizedValue))
            throw new ArgumentException("Normalized value is required.", nameof(normalizedValue));

        var payload = Encoding.UTF8.GetBytes($"{tenantId:N}:{normalizedValue}");
        return HMACSHA256.HashData(_blindIndexKey, payload);
    }

    public static string NormalizePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        return new string(phone.Where(char.IsDigit).ToArray());
    }

    public static string? NormalizeEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
}
