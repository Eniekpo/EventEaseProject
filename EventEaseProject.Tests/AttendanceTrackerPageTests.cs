using Bunit;
using EventEaseProject.Components.Pages;
using EventEaseProject.Models;
using EventEaseProject.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventEaseProject.Tests;

public sealed class AttendanceTrackerPageTests
{
    [Fact]
    public async Task CheckInUpdatesRegistrantAndSummary()
    {
        using var context = new TestContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddLogging();
        var registrations = new RegistrationService(new FakeEventService(new EventItem(
            "event-1",
            "Test Event",
            DateTimeOffset.UtcNow.AddDays(10),
            "A test event")));
        await registrations.RegisterAsync(new RegistrationModel
        {
            FullName = "Taylor Attendee",
            Email = "taylor@example.com",
            EventId = "event-1",
            AttendeeCount = 2
        });
        var attendance = new AttendanceService(registrations);
        context.Services.AddSingleton<IEventService>(new FakeEventService(new EventItem(
            "event-1",
            "Test Event",
            DateTimeOffset.UtcNow.AddDays(10),
            "A test event")));
        context.Services.AddSingleton<IRegistrationService>(registrations);
        context.Services.AddSingleton<IAttendanceService>(attendance);

        var cut = context.RenderComponent<AttendanceTracker>();

        cut.Find("button").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("CheckedIn", cut.Markup);
            Assert.Contains("<dd class=\"col-sm-3\">100.0%</dd>", cut.Markup);
        });
    }
}
