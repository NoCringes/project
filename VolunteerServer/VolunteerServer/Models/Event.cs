using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VolunteerServer.Models;

[Table("events")]
public class Event
{
    [Key]
    [Column("event_id")]
    public int EventId { get; set; }

    [Column("title")]
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "active";

    [Column("location")]
    [Required]
    [MaxLength(300)]
    public string Location { get; set; } = string.Empty;

    [Column("start_datetime")]
    [Required]
    public DateTime StartDateTime { get; set; }

    [Column("end_datetime")]
    [Required]
    public DateTime EndDateTime { get; set; }

    [Column("max_volunteers")]
    [Required]
    public int MaxVolunteers { get; set; }

    [Column("created_by")]
    [Required]
    public int CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}