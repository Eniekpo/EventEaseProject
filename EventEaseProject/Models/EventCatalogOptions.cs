namespace EventEaseProject.Models;

public sealed class EventCatalogOptions
{
    public List<EventCatalogEntry> Items { get; set; } = [];
}

public sealed class EventCatalogEntry
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset StartsAt { get; set; }
    public string Description { get; set; } = string.Empty;
}
