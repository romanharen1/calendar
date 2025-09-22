using System.ComponentModel.DataAnnotations.Schema;

namespace Art.OnShift.Shared.Models
{
    public class EventAuditModel
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public required string Operation { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime EventDate { get; set; }
        public required string UserId { get; set; }

        // Current/New values
        public string? Title { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? Start { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? End { get; set; }
        public required string Level1Id { get; set; }
        public required string Level2Id { get; set; }
        public required string Level3Id { get; set; }

        // Previous values (for UPDATE operations)
        public string? PreviousTitle { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? PreviousStart { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? PreviousEnd { get; set; }
        public string? PreviousLevel1Id { get; set; }
        public string? PreviousLevel2Id { get; set; }
        public string? PreviousLevel3Id { get; set; }
    }
}
