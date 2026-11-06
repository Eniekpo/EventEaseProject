using Bunit;
using EventEaseProject.Components.Pages;
using EventEaseProject.Models;
using EventEaseProject.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventEaseProject.Tests;

public sealed class RegistrationPageTests
{
    private static readonly EventItem TestEvent = new(
        "event-1",
        "Test Event",
        DateTimeOffset.UtcNow.AddDays(10),
        "A test event");

    [Theory]
    [InlineData("Name", "not-an-email", "2")]
    [InlineData("", "guest@example.com", "2")]
    [InlineData("Guest Name", "guest@example.com", "11")]
    public void InvalidFormDoesNotCallRegistrationService(string name, string email, string attendees)
    {
        using var context = CreateContext(out var registrationService);
        var cut = context.RenderComponent<Registration>();

        cut.Find("#full-name").Change(name);
        cut.Find("#email").Change(email);
        cut.Find("#event").Change(TestEvent.Id);
        cut.Find("#attendee-count").Change(attendees);
        cut.Find("form").Submit();

        Assert.Equal(0, registrationService.RegisterCallCount);
        Assert.Contains("validation", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DuplicateRegistrationShowsClearMessage()
    {
        using var context = CreateContext(out var registrationService);
        registrationService.ReturnDuplicate = true;
        var cut = context.RenderComponent<Registration>();
        FillValidForm(cut);
        cut.Find("form").Submit();

        Assert.Contains("already registered", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SuccessfulSubmissionShowsConfirmation()
    {
        using var context = CreateContext(out _);
        var cut = context.RenderComponent<Registration>();
        FillValidForm(cut);
        cut.Find("form").Submit();

        Assert.Contains("registration was successful", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ServiceExceptionShowsFriendlyMessage()
    {
        using var context = CreateContext(out var registrationService);
        registrationService.ThrowOnRegister = true;
        var cut = context.RenderComponent<Registration>();
        FillValidForm(cut);
        cut.Find("form").Submit();

        Assert.Contains("couldn't complete your registration", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Test failure", cut.Markup);
    }

    private static TestContext CreateContext(out FakeRegistrationService registrationService)
    {
        var context = new TestContext();
        registrationService = new FakeRegistrationService();
        context.Services.AddLogging();
        context.Services.AddSingleton<IEventService>(new FakeEventService(TestEvent));
        context.Services.AddSingleton<IRegistrationService>(registrationService);
        context.Services.AddScoped<IProtectedSessionStore, FakeProtectedSessionStore>();
        context.Services.AddScoped<UserSessionService>();
        return context;
    }

    private static void FillValidForm(IRenderedComponent<Registration> cut)
    {
        cut.Find("#full-name").Change("Guest Name");
        cut.Find("#email").Change("guest@example.com");
        cut.Find("#event").Change(TestEvent.Id);
        cut.Find("#attendee-count").Change("2");
    }
}
