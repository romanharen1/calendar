namespace Art.OnShift.Shared.Models
{
    public class EventAuditViewModel : EventAuditModel
    {
        // Resolved names from database
        public string? Level1Name { get; set; }
        public string? Level2Name { get; set; }
        public string? Level3Name { get; set; }

        // Previous resolved names for UPDATE operations
        public string? PreviousLevel1Name { get; set; }
        public string? PreviousLevel2Name { get; set; }
        public string? PreviousLevel3Name { get; set; }
    }

}
