using EventEaseProject.Models;

namespace EventEaseProject.Services;

public interface IEventService
{
    Task<IReadOnlyList<EventItem>> GetEventsAsync(CancellationToken cancellationToken = default);
}
