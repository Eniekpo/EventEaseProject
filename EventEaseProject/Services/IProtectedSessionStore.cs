using EventEaseProject.Models;

namespace EventEaseProject.Services;

public interface IProtectedSessionStore
{
    ValueTask<UserSessionSnapshot?> GetAsync();
    ValueTask SetAsync(UserSessionSnapshot snapshot);
    ValueTask DeleteAsync();
}
