using EventEaseProject.Models;
using Microsoft.Extensions.Options;

namespace EventEaseProject.Services;

public sealed class EventService(IOptions<EventCatalogOptions> options) : IEventService
{
    private readonly IReadOnlyList<EventItem> _events = options.Value.Items
        .Where(item => !string.IsNullOrWhiteSpace(item.Id) && !string.IsNullOrWhiteSpace(item.Name))
        .Select(item => new EventItem(item.Id, item.Name, item.StartsAt, item.Description))
        .ToArray();

    public Task<IReadOnlyList<EventItem>> GetEventsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_events);
    }
}
