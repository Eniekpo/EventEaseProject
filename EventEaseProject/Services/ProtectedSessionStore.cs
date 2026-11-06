using EventEaseProject.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace EventEaseProject.Services;

public sealed class ProtectedSessionStore(ProtectedSessionStorage storage) : IProtectedSessionStore
{
    private const string StorageKey = "event-ease-user-session";

    public async ValueTask<UserSessionSnapshot?> GetAsync()
    {
        var result = await storage.GetAsync<UserSessionSnapshot>(StorageKey);
        return result.Success ? result.Value : null;
    }

    public ValueTask SetAsync(UserSessionSnapshot snapshot) =>
        storage.SetAsync(StorageKey, snapshot);

    public ValueTask DeleteAsync() => storage.DeleteAsync(StorageKey);
}
