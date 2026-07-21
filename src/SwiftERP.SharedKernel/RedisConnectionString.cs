namespace SwiftERP.SharedKernel;

/// <summary>
/// Same problem as PostgresConnectionString, different service: managed Redis providers (Upstash
/// included) hand out a redis:// or rediss:// URI, but StackExchange.Redis only accepts its own
/// "host:port,password=...,ssl=True" format. Converts one to the other; already-native strings
/// pass through unchanged.
/// </summary>
public static class RedisConnectionString
{
    public static string Normalize(string connectionString)
    {
        var isTls = connectionString.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase);
        if (!isTls && !connectionString.StartsWith("redis://", StringComparison.OrdinalIgnoreCase))
            return connectionString;

        var uri = new Uri(connectionString);
        var password = uri.UserInfo.Contains(':')
            ? Uri.UnescapeDataString(uri.UserInfo.Split(':', 2)[1])
            : Uri.UnescapeDataString(uri.UserInfo);
        var port = uri.Port == -1 ? 6379 : uri.Port;

        var result = $"{uri.Host}:{port},abortConnect=False";
        if (!string.IsNullOrEmpty(password))
            result += $",password={password}";
        if (isTls)
            result += ",ssl=True";

        return result;
    }
}
