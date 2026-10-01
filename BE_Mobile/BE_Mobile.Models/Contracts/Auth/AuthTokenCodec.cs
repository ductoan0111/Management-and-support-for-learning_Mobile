using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BE_Mobile.Contracts.Auth;

public sealed record AuthTokenPayload(long UserId, long ExpiresAtUnixSeconds);

public sealed record AuthTokenUserDto(
    long UserId,
    byte RoleId,
    string RoleCode,
    string RoleName,
    long? StudentId,
    long? TeacherId,
    string Identifier,
    string FullName,
    string Email,
    string? Phone,
    string? AvatarUrl,
    string PasswordHash,
    bool IsActive);

public static class AuthTokenCodec
{
    private const string Purpose = "BE_Mobile.App.AccessToken.v1";

    public static string Issue(long userId, string passwordHash, DateTimeOffset expiresAt)
    {
        var payload = new AuthTokenPayload(userId, expiresAt.ToUnixTimeSeconds());
        var payloadSegment = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var signatureSegment = Sign(payloadSegment, passwordHash);
        return $"{payloadSegment}.{signatureSegment}";
    }

    public static bool TryReadPayload(string token, out AuthTokenPayload payload)
    {
        payload = default!;
        var separator = token.IndexOf('.', StringComparison.Ordinal);
        if (separator <= 0 || separator == token.Length - 1)
            return false;

        var payloadSegment = token[..separator];
        if (!TryBase64UrlDecode(payloadSegment, out var payloadBytes))
            return false;

        try
        {
            var parsed = JsonSerializer.Deserialize<AuthTokenPayload>(payloadBytes);
            if (parsed is null || parsed.UserId <= 0)
                return false;

            payload = parsed;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static bool Validate(string token, string passwordHash, DateTimeOffset now, out AuthTokenPayload payload)
    {
        payload = default!;
        var separator = token.IndexOf('.', StringComparison.Ordinal);
        if (separator <= 0 || separator == token.Length - 1)
            return false;

        var payloadSegment = token[..separator];
        var signatureSegment = token[(separator + 1)..];
        if (!TryReadPayload(token, out var parsedPayload))
            return false;

        if (parsedPayload.ExpiresAtUnixSeconds <= now.ToUnixTimeSeconds())
            return false;

        var expectedSignature = Sign(payloadSegment, passwordHash);
        if (!FixedTimeEquals(signatureSegment, expectedSignature))
            return false;

        payload = parsedPayload;
        return true;
    }

    private static string Sign(string payloadSegment, string passwordHash)
    {
        var key = SHA256.HashData(Encoding.UTF8.GetBytes($"{Purpose}|{passwordHash}"));
        using var hmac = new HMACSHA256(key);
        var signature = hmac.ComputeHash(Encoding.ASCII.GetBytes(payloadSegment));
        return Base64UrlEncode(signature);
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        if (!TryBase64UrlDecode(left, out var leftBytes) || !TryBase64UrlDecode(right, out var rightBytes))
            return false;

        return leftBytes.Length == rightBytes.Length
            && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static bool TryBase64UrlDecode(string value, out byte[] bytes)
    {
        bytes = [];
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var padded = value.Replace('-', '+').Replace('_', '/');
        padded += (padded.Length % 4) switch
        {
            0 => string.Empty,
            2 => "==",
            3 => "=",
            _ => "?"
        };

        try
        {
            bytes = Convert.FromBase64String(padded);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
