using System.ComponentModel.DataAnnotations;

namespace Loggy.Core.Models.Events;

public class Event
{
    public required Guid Id { get; set; }
    public required string AccountId { get; set; }
    public required EventType Type { get; set; }
    public required DateTime Timestamp { get; set; }
    public required decimal Amount { get; set; }
    public string Message { get; set; } = string.Empty;
}

public enum EventType
{
    Unknown = 0,
    /// <summary>Unmoved earmarked</summary>
    Reservation,
    /// <summary><see cref="EventType.Reservation"/> no more. Expired, cancelled or consumed.</summary>
    ReservationReleased,
    /// <summary>Actual $$$$ movement</summary>
    Transaction
}

public class EventDto
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(maximumLength: 64, MinimumLength = 1)]
    [RegularExpression(@"^\S.*$|^\S$", ErrorMessage = "AccountId cannot be blank")]
    public required string AccountId { get; set; }

    [Required, AllowedValues(EventType.Reservation, EventType.ReservationReleased, EventType.Transaction)]
    public required EventType Type { get; set; }

    [Required]
    public required decimal Amount { get; set; }

    [StringLength(256)]
    public string Message { get; set; } = string.Empty;
}