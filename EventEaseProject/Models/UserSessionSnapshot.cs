namespace EventEaseProject.Models;

public sealed class UserSessionSnapshot
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset LastActivityAt { get; set; }
    public List<string> PagesVisited { get; set; } = [];
}
