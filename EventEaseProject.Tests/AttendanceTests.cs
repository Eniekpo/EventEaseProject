using EventEaseProject.Models;
using EventEaseProject.Services;
using Xunit;

namespace EventEaseProject.Tests;

public sealed class AttendanceTests
{
    [Fact]
    public async Task CheckInAndCheckOutFollowValidTransitions()
    {
        var (attendance, registration) = await CreateRegistrationAsync("first-event");

        var checkedIn = await attendance.CheckInAsync(registration.Id);
        var checkedOut = await attendance.CheckOutAsync(registration.Id);

        Assert.Equal(AttendanceStatus.CheckedIn, checkedIn.Status);
        Assert.NotNull(checkedIn.CheckInTime);
        Assert.Equal(AttendanceStatus.CheckedOut, checkedOut.Status);
        Assert.NotNull(checkedOut.CheckOutTime);
    }

    [Fact]
    public async Task InvalidTransitionIsRejected()
    {
        var (attendance, registration) = await CreateRegistrationAsync("first-event");

        await Assert.ThrowsAsync<InvalidOperationException>(() => attendance.CheckOutAsync(registration.Id));
        await attendance.MarkNoShowAsync(registration.Id);
        await Assert.ThrowsAsync<InvalidOperationException>(() => attendance.CheckInAsync(registration.Id));
    }

    [Fact]
    public async Task SummaryCountsCheckedOutAndNoShowsAndCalculatesRate()
    {
        var registrations = NewRegistrationService("summary-event");
        var checkedIn = await registrations.RegisterAsync(NewModel("summary-event", "a@example.com"));
        var noShow = await registrations.RegisterAsync(NewModel("summary-event", "b@example.com"));
        await registrations.RegisterAsync(NewModel("summary-event", "c@example.com"));
        var attendance = new AttendanceService(registrations);

        await attendance.CheckInAsync(checkedIn.Registration!.Id);
        await attendance.CheckOutAsync(checkedIn.Registration.Id);
        await attendance.MarkNoShowAsync(noShow.Registration!.Id);

        var summary = await attendance.GetAttendanceSummaryAsync("summary-event");

        Assert.Equal(3, summary.TotalRegistered);
        Assert.Equal(1, summary.CheckedIn);
        Assert.Equal(1, summary.NoShows);
        Assert.Equal(100d / 3, summary.AttendanceRate, 5);
    }

    private static async Task<(AttendanceService Attendance, EventRegistration Registration)> CreateRegistrationAsync(
        string eventId)
    {
        var registrations = NewRegistrationService(eventId);
        var result = await registrations.RegisterAsync(NewModel(eventId, "guest@example.com"));
        return (new AttendanceService(registrations), result.Registration!);
    }

    private static RegistrationModel NewModel(string eventId, string email) => new()
    {
        FullName = "Guest Attendee",
        Email = email,
        EventId = eventId,
        AttendeeCount = 1
    };

    private static RegistrationService NewRegistrationService(params string[] eventIds) =>
        new(new FakeEventService(eventIds
            .Select(eventId => new EventItem(eventId, eventId, DateTimeOffset.UtcNow.AddDays(1), string.Empty))
            .ToArray()));
}
