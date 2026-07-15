namespace SwiftERP.Identity.Domain.Roles;

/// <summary>Ordered so a numeric comparison (level >= required) is a valid "at least this much
/// access" check — None &lt; View &lt; Edit &lt; Full.</summary>
public enum AccessLevel
{
    None = 0,
    View = 1,
    Edit = 2,
    Full = 3
}
