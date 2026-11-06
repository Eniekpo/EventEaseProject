using System.Security.Cryptography;
using System.Text.Json;
using EventEaseProject.Models;
using Microsoft.JSInterop;

namespace EventEaseProject.Services;

public sealed class UserSessionService(
    IProtectedSessionStore storage,
    ILogger<UserSessionService> logger)
{
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private readonly object _sync = new();
    private UserSessionSnapshot _snapshot = CreateNewSnapshot();
    private bool _initialized;
    private bool _storageAvailable = true;

    public event Action? SessionChanged;

    public UserSessionSnapshot Snapshot
    {
        get
        {
            lock (_sync)
            {
                return Clone(_snapshot);
            }
        }
    }

    public async Task InitializeAsync()
    {
        await _initializationLock.WaitAsync();
        try
        {
            if (_initialized)
            {
                return;
            }

            try
            {
                var stored = await storage.GetAsync();
                if (stored is not null)
                {
                    _snapshot = Normalize(stored);
                }
            }
            catch (Exception exception) when (IsBrowserStorageFailure(exception))
            {
                _storageAvailable = false;
                logger.LogWarning("Browser session storage is unavailable; this session will not survive a refresh.");
            }

            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }

        SessionChanged?.Invoke();
    }

    public async Task SetRegisteredUserAsync(string name, string email)
    {
        await EnsureInitializedAsync();
        lock (_sync)
        {
            _snapshot.Name = name.Trim();
            _snapshot.Email = email.Trim();
            _snapshot.LastActivityAt = DateTimeOffset.UtcNow;
        }

        await PersistAsync();
        SessionChanged?.Invoke();
    }

    public async Task RecordPageVisitAsync(string page)
    {
        await EnsureInitializedAsync();
        var normalizedPage = string.IsNullOrWhiteSpace(page) ? "/" : page;
        lock (_sync)
        {
            if (!_snapshot.PagesVisited.Contains(normalizedPage, StringComparer.OrdinalIgnoreCase))
            {
                if (_snapshot.PagesVisited.Count == 100)
                {
                    _snapshot.PagesVisited.RemoveAt(0);
                }

                _snapshot.PagesVisited.Add(normalizedPage);
            }

            _snapshot.LastActivityAt = DateTimeOffset.UtcNow;
        }

        await PersistAsync();
        SessionChanged?.Invoke();
    }

    public async Task ClearAsync()
    {
        await EnsureInitializedAsync();
        lock (_sync)
        {
            _snapshot = CreateNewSnapshot();
        }

        if (_storageAvailable)
        {
            try
            {
                await storage.DeleteAsync();
            }
            catch (Exception exception) when (IsBrowserStorageFailure(exception))
            {
                _storageAvailable = false;
                logger.LogWarning("Browser session storage could not be cleared; the in-memory session was reset.");
            }
        }

        SessionChanged?.Invoke();
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_initialized)
        {
            await InitializeAsync();
        }
    }

    private async Task PersistAsync()
    {
        if (!_storageAvailable)
        {
            return;
        }

        try
        {
            await storage.SetAsync(Snapshot);
        }
        catch (Exception exception) when (IsBrowserStorageFailure(exception))
        {
            _storageAvailable = false;
            logger.LogWarning("Browser session storage is unavailable; changes will remain in memory only.");
        }
    }

    private static bool IsBrowserStorageFailure(Exception exception) =>
        exception is JSException or InvalidOperationException or CryptographicException or JsonException;

    private static UserSessionSnapshot CreateNewSnapshot()
    {
        var now = DateTimeOffset.UtcNow;
        return new UserSessionSnapshot
        {
            UserId = Guid.NewGuid(),
            StartedAt = now,
            LastActivityAt = now
        };
    }

    private static UserSessionSnapshot Normalize(UserSessionSnapshot snapshot)
    {
        var now = DateTimeOffset.UtcNow;
        var pages = snapshot.PagesVisited ?? [];
        return new UserSessionSnapshot
        {
            UserId = snapshot.UserId == Guid.Empty ? Guid.NewGuid() : snapshot.UserId,
            Name = snapshot.Name,
            Email = snapshot.Email,
            StartedAt = snapshot.StartedAt == default ? now : snapshot.StartedAt,
            LastActivityAt = snapshot.LastActivityAt == default ? now : snapshot.LastActivityAt,
            PagesVisited = pages.Where(page => !string.IsNullOrWhiteSpace(page)).TakeLast(100).ToList()
        };
    }

    private static UserSessionSnapshot Clone(UserSessionSnapshot snapshot) => new()
    {
        UserId = snapshot.UserId,
        Name = snapshot.Name,
        Email = snapshot.Email,
        StartedAt = snapshot.StartedAt,
        LastActivityAt = snapshot.LastActivityAt,
        PagesVisited = [.. snapshot.PagesVisited]
    };
}
