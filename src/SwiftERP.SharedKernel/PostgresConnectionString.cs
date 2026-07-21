namespace SwiftERP.SharedKernel;

/// <summary>
/// Most managed Postgres providers (Neon, Supabase, Render's own DB, etc.) hand out connection
/// strings as a postgresql:// URI, but Npgsql only accepts its own keyword=value format
/// (Host=...;Username=...). This converts one to the other so a URI can be dropped straight into
/// an env var without anyone hand-translating it — already-keyword-style strings pass through
/// unchanged.
/// </summary>
public static class PostgresConnectionString
{
    public static string Normalize(string connectionString)
    {
        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var database = uri.AbsolutePath.TrimStart('/');
        var port = uri.Port == -1 ? 5432 : uri.Port;

        // sslmode is commonly passed as a query param (?sslmode=require) — Neon requires it.
        // Parsed by hand rather than via System.Web.HttpUtility, which isn't available in a
        // plain class library outside the ASP.NET Core shared framework.
        var sslMode = uri.Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(pair => pair.Split('=', 2))
            .Where(kv => kv[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase))
            .Select(kv => kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : null)
            .FirstOrDefault();

        var builder = $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password}";
        if (!string.IsNullOrEmpty(sslMode))
            builder += $";SSL Mode={sslMode};Trust Server Certificate=true";

        return builder;
    }
}
