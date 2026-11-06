using EventEaseProject.Models;

namespace EventEaseProject.Services;

public sealed class AttendanceService(IRegistrationService registrations) : IAttendanceService
{
    private readonly object _sync = new();
    private readonly Dictionary<Guid, AttendanceRecord> _records = [];

    public event Action<string>? AttendanceChanged;

    public async Task<AttendanceRecord> CheckInAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureRecordAsync(registrationId, cancellationToken);
        return UpdateStatus(registrationId, record =>
        {
            if (record.Status != AttendanceStatus.Registered)
            {
                throw new InvalidOperationException("Only registered attendees can be checked in.");
            }

            return record with { CheckInTime = DateTimeOffset.UtcNow, Status = AttendanceStatus.CheckedIn };
        });
    }

    public async Task<AttendanceRecord> CheckOutAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureRecordAsync(registrationId, cancellationToken);
        return UpdateStatus(registrationId, record =>
        {
            if (record.Status != AttendanceStatus.CheckedIn)
            {
                throw new InvalidOperationException("An attendee must be checked in before checking out.");
            }

            return record with { CheckOutTime = DateTimeOffset.UtcNow, Status = AttendanceStatus.CheckedOut };
        });
    }

    public async Task<AttendanceRecord> MarkNoShowAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureRecordAsync(registrationId, cancellationToken);
        return UpdateStatus(registrationId, record =>
        {
            if (record.Status != AttendanceStatus.Registered)
            {
                throw new InvalidOperationException("Only registered attendees can be marked as a no-show.");
            }

            return record with { Status = AttendanceStatus.NoShow };
        });
    }

    public async Task<IReadOnlyList<AttendanceRecord>> GetAttendanceByEventAsync(
        string eventId,
        CancellationToken cancellationToken = default)
    {
        var eventRegistrations = await registrations.GetRegistrationsByEventAsync(eventId, cancellationToken);
        lock (_sync)
        {
            foreach (var registration in eventRegistrations)
            {
                _records.TryAdd(registration.Id, new AttendanceRecord(
                    registration.Id,
                    registration.EventId,
                    null,
                    null,
                    AttendanceStatus.Registered));
            }

            IReadOnlyList<AttendanceRecord> records = _records.Values
                .Where(record => record.EventId == eventId)
                .OrderBy(record => record.RegistrationId)
                .ToArray();
            return records;
        }
    }

    public async Task<AttendanceSummary> GetAttendanceSummaryAsync(
        string eventId,
        CancellationToken cancellationToken = default)
    {
        var records = await GetAttendanceByEventAsync(eventId, cancellationToken);
        var checkedIn = records.Count(record =>
            record.Status is AttendanceStatus.CheckedIn or AttendanceStatus.CheckedOut);
        var noShows = records.Count(record => record.Status == AttendanceStatus.NoShow);
        var attendanceRate = records.Count == 0 ? 0 : checkedIn * 100d / records.Count;
        return new AttendanceSummary(records.Count, checkedIn, noShows, attendanceRate);
    }

    private async Task EnsureRecordAsync(Guid registrationId, CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            if (_records.ContainsKey(registrationId))
            {
                return;
            }
        }

        var registration = await registrations.GetRegistrationAsync(registrationId, cancellationToken);
        if (registration is null)
        {
            throw new KeyNotFoundException("The registration could not be found.");
        }

        lock (_sync)
        {
            _records.TryAdd(registrationId, new AttendanceRecord(
                registrationId,
                registration.EventId,
                null,
                null,
                AttendanceStatus.Registered));
        }
    }

    private AttendanceRecord UpdateStatus(
        Guid registrationId,
        Func<AttendanceRecord, AttendanceRecord> update)
    {
        AttendanceRecord updated;
        lock (_sync)
        {
            if (!_records.TryGetValue(registrationId, out var current))
            {
                throw new KeyNotFoundException("The registration could not be found.");
            }

            updated = update(current);
            _records[registrationId] = updated;
        }

        AttendanceChanged?.Invoke(updated.EventId);
        return updated;
    }
}
