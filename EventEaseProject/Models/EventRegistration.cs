namespace EventEaseProject.Models;

public sealed record EventRegistration(
    Guid Id,
    string EventId,
    string FullName,
    string Email,
    string? Phone,
    int AttendeeCount,
    DateTimeOffset RegisteredAt);
