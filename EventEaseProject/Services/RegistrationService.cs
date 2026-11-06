using System.ComponentModel.DataAnnotations;
using EventEaseProject.Models;

namespace EventEaseProject.Services;

public sealed class RegistrationService(IEventService events) : IRegistrationService
{
    private readonly object _sync = new();
    private readonly List<EventRegistration> _registrations = [];

    public async Task<RegistrationResult> RegisterAsync(
        RegistrationModel model,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(model);

        var normalizedModel = new RegistrationModel
        {
            FullName = model.FullName?.Trim() ?? string.Empty,
            Email = model.Email?.Trim() ?? string.Empty,
            Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
            EventId = model.EventId?.Trim() ?? string.Empty,
            AttendeeCount = model.AttendeeCount
        };
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(
                normalizedModel,
                new ValidationContext(normalizedModel),
                validationResults,
                validateAllProperties: true))
        {
            throw new ArgumentException("The registration details are invalid.", nameof(model));
        }

        var availableEvents = await events.GetEventsAsync(cancellationToken);
        if (!availableEvents.Any(eventItem => eventItem.Id == normalizedModel.EventId))
        {
            throw new ArgumentException("The selected event is not available.", nameof(model));
        }

        lock (_sync)
        {
            var duplicate = _registrations.Any(registration =>
                registration.EventId == normalizedModel.EventId &&
                string.Equals(registration.Email, normalizedModel.Email, StringComparison.OrdinalIgnoreCase));

            if (duplicate)
            {
                return new RegistrationResult(false, true, null);
            }

            var registration = new EventRegistration(
                Guid.NewGuid(),
                normalizedModel.EventId,
                normalizedModel.FullName,
                normalizedModel.Email,
                normalizedModel.Phone,
                normalizedModel.AttendeeCount,
                DateTimeOffset.UtcNow);
            _registrations.Add(registration);
            return new RegistrationResult(true, false, registration);
        }
    }

    public Task<IReadOnlyList<EventRegistration>> GetRegistrationsByEventAsync(
        string eventId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            IReadOnlyList<EventRegistration> registrations = _registrations
                .Where(registration => registration.EventId == eventId)
                .OrderBy(registration => registration.FullName)
                .ToArray();
            return Task.FromResult(registrations);
        }
    }

    public Task<EventRegistration?> GetRegistrationAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult(_registrations.FirstOrDefault(registration => registration.Id == registrationId));
        }
    }
}
