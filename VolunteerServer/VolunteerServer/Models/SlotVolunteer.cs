using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VolunteerServer.Models;

[Table("slot_volunteers")]
public class SlotVolunteer
{
    [Key]
    [Column("record_id")]
    public int RecordId { get; set; }

    [Column("user_id")]
    [Required]
    public int UserId { get; set; }

    [Column("slot_id")]
    [Required]
    public int SlotId { get; set; }

    [Column("registered_at")]
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "registered";

    [Column("attended_at")]
    public DateTime? AttendedAt { get; set; }
}