using EventEaseProject.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EventEaseProject.Tests;

public sealed class UserSessionServiceTests
{
    [Fact]
    public async Task SessionRestoresIdentityAndPageHistoryFromProtectedStore()
    {
        var store = new FakeProtectedSessionStore();
        var firstSession = new UserSessionService(store, NullLogger<UserSessionService>.Instance);
        await firstSession.InitializeAsync();
        await firstSession.SetRegisteredUserAsync("Avery Guest", "avery@example.com");
        await firstSession.RecordPageVisitAsync("/registration");
        var userId = firstSession.Snapshot.UserId;

        var restoredSession = new UserSessionService(store, NullLogger<UserSessionService>.Instance);
        await restoredSession.InitializeAsync();

        Assert.Equal(userId, restoredSession.Snapshot.UserId);
        Assert.Equal("Avery Guest", restoredSession.Snapshot.Name);
        Assert.Equal("avery@example.com", restoredSession.Snapshot.Email);
        Assert.Contains("/registration", restoredSession.Snapshot.PagesVisited);
    }

    [Fact]
    public async Task ClearRemovesStoredDetailsAndStartsFreshSession()
    {
        var store = new FakeProtectedSessionStore();
        var session = new UserSessionService(store, NullLogger<UserSessionService>.Instance);
        await session.InitializeAsync();
        await session.SetRegisteredUserAsync("Avery Guest", "avery@example.com");
        var originalUserId = session.Snapshot.UserId;

        await session.ClearAsync();

        Assert.Null(store.Value);
        Assert.Null(session.Snapshot.Name);
        Assert.Null(session.Snapshot.Email);
        Assert.NotEqual(originalUserId, session.Snapshot.UserId);
        Assert.Empty(session.Snapshot.PagesVisited);
    }

    [Fact]
    public async Task SessionChangedIsRaisedForUpdatesAndClear()
    {
        var session = new UserSessionService(
            new FakeProtectedSessionStore(),
            NullLogger<UserSessionService>.Instance);
        var notifications = 0;
        session.SessionChanged += () => notifications++;

        await session.InitializeAsync();
        await session.RecordPageVisitAsync("/attendance");
        await session.ClearAsync();

        Assert.Equal(3, notifications);
    }
}
