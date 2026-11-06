namespace EventEaseProject.Models;

public sealed record AttendanceSummary(
    int TotalRegistered,
    int CheckedIn,
    int NoShows,
    double AttendanceRate);
