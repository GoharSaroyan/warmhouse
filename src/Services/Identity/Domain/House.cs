namespace Identity.Api.Domain;

/// <summary>
/// A home belonging to a user. One user can have many houses; each house
/// belongs to exactly one user. See docs/erd/erd.puml.
/// </summary>
public class House
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Address { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
