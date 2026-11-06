using EventEaseProject.Models;

namespace EventEaseProject.Services;

public interface IRegistrationService
{
    Task<RegistrationResult> RegisterAsync(RegistrationModel model, CancellationToken cancellationToken = default);
    Task<EventRegistration?> GetRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventRegistration>> GetRegistrationsByEventAsync(string eventId, CancellationToken cancellationToken = default);
}

public sealed record RegistrationResult(bool Succeeded, bool IsDuplicate, EventRegistration? Registration);
