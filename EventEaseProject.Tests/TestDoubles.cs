using EventEaseProject.Models;
using EventEaseProject.Services;

namespace EventEaseProject.Tests;

internal sealed class FakeProtectedSessionStore : IProtectedSessionStore
{
    public UserSessionSnapshot? Value { get; private set; }

    public ValueTask<UserSessionSnapshot?> GetAsync() => ValueTask.FromResult(Value);
    public ValueTask SetAsync(UserSessionSnapshot snapshot)
    {
        Value = new UserSessionSnapshot
        {
            UserId = snapshot.UserId,
            Name = snapshot.Name,
            Email = snapshot.Email,
            StartedAt = snapshot.StartedAt,
            LastActivityAt = snapshot.LastActivityAt,
            PagesVisited = [.. snapshot.PagesVisited]
        };
        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteAsync()
    {
        Value = null;
        return ValueTask.CompletedTask;
    }
}

internal sealed class FakeEventService(params EventItem[] events) : IEventService
{
    public Task<IReadOnlyList<EventItem>> GetEventsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<EventItem>>(events);
    }
}

internal sealed class FakeRegistrationService : IRegistrationService
{
    private readonly List<EventRegistration> _registrations = [];
    public int RegisterCallCount { get; private set; }
    public bool ReturnDuplicate { get; set; }
    public bool ThrowOnRegister { get; set; }

    public Task<RegistrationResult> RegisterAsync(RegistrationModel model, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RegisterCallCount++;
        if (ThrowOnRegister)
        {
            throw new InvalidOperationException("Test failure");
        }

        if (ReturnDuplicate)
        {
            return Task.FromResult(new RegistrationResult(false, true, null));
        }

        var registration = new EventRegistration(
            Guid.NewGuid(),
            model.EventId,
            model.FullName.Trim(),
            model.Email.Trim(),
            model.Phone,
            model.AttendeeCount,
            DateTimeOffset.UtcNow);
        _registrations.Add(registration);
        return Task.FromResult(new RegistrationResult(true, false, registration));
    }

    public Task<EventRegistration?> GetRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_registrations.FirstOrDefault(registration => registration.Id == registrationId));
    }

    public Task<IReadOnlyList<EventRegistration>> GetRegistrationsByEventAsync(string eventId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<EventRegistration>>(
            _registrations.Where(registration => registration.EventId == eventId).ToArray());
    }
}
