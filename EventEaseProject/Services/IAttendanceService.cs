using EventEaseProject.Models;

namespace EventEaseProject.Services;

public interface IAttendanceService
{
    event Action<string>? AttendanceChanged;

    Task<AttendanceRecord> CheckInAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<AttendanceRecord> CheckOutAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<AttendanceRecord> MarkNoShowAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceRecord>> GetAttendanceByEventAsync(string eventId, CancellationToken cancellationToken = default);
    Task<AttendanceSummary> GetAttendanceSummaryAsync(string eventId, CancellationToken cancellationToken = default);
}
