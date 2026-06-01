using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VolunteerServer.Models;

[Table("event_slots")]
public class EventSlot
{
    [Key]
    [Column("slot_id")]
    public int SlotId { get; set; }

    [Column("event_id")]
    [Required]
    public int EventId { get; set; }

    [Column("title")]
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("slots_available")]
    [Required]
    public int SlotsAvailable { get; set; }
}