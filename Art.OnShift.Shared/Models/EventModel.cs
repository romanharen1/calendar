using System.ComponentModel.DataAnnotations.Schema;

namespace Art.OnShift.Shared.Models
{
    public class EventModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? Start { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? End { get; set; }
        public string Level1Id { get; set; }
        public UserModel? Level1 { get; set; }
        public string Level2Id { get; set; }
        public UserModel? Level2 { get; set; }
        public string Level3Id { get; set; }
        public UserModel? Level3 { get; set; }
    }
}
