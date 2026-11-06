using EventEaseProject.Models;
using EventEaseProject.Services;
using Xunit;

namespace EventEaseProject.Tests;

public sealed class RegistrationServiceTests
{
    [Fact]
    public async Task DuplicateEmailIsRejectedForSameEventButAllowedForAnotherEvent()
    {
        var service = new RegistrationService(new FakeEventService(
            new EventItem("event-1", "Event One", DateTimeOffset.UtcNow.AddDays(1), string.Empty),
            new EventItem("event-2", "Event Two", DateTimeOffset.UtcNow.AddDays(2), string.Empty)));
        var first = await service.RegisterAsync(NewModel("event-1", "guest@example.com"));
        var duplicate = await service.RegisterAsync(NewModel("event-1", "GUEST@example.com"));
        var otherEvent = await service.RegisterAsync(NewModel("event-2", "guest@example.com"));

        Assert.True(first.Succeeded);
        Assert.True(duplicate.IsDuplicate);
        Assert.False(duplicate.Succeeded);
        Assert.True(otherEvent.Succeeded);
    }

    [Fact]
    public async Task RegistrationRejectsEventsOutsideTheConfiguredCatalog()
    {
        var service = new RegistrationService(new FakeEventService(
            new EventItem("event-1", "Event One", DateTimeOffset.UtcNow.AddDays(1), string.Empty)));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RegisterAsync(NewModel("unconfigured-event", "guest@example.com")));
    }

    private static RegistrationModel NewModel(string eventId, string email) => new()
    {
        FullName = "Guest Attendee",
        Email = email,
        EventId = eventId,
        AttendeeCount = 1
    };
}
