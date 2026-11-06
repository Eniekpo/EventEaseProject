using System.ComponentModel.DataAnnotations;

namespace EventEaseProject.Models;

public sealed class RegistrationModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^\+?[0-9\s().-]{7,25}$", ErrorMessage = "Enter a valid phone number.")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Please select an event.")]
    public string EventId { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "Number of attendees must be between 1 and 10.")]
    public int AttendeeCount { get; set; } = 1;
}
