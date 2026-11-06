namespace EventEaseProject.Models;

public enum AttendanceStatus
{
    Registered,
    CheckedIn,
    CheckedOut,
    NoShow
}

public sealed record AttendanceRecord(
    Guid RegistrationId,
    string EventId,
    DateTimeOffset? CheckInTime,
    DateTimeOffset? CheckOutTime,
    AttendanceStatus Status);
