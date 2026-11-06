namespace EventEaseProject.Models;

public sealed record EventItem(
    string Id,
    string Name,
    DateTimeOffset StartsAt,
    string Description);
