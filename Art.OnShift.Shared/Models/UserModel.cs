using System.ComponentModel.DataAnnotations;

namespace Art.OnShift.Shared.Models
{
    public class UserModel
    {
        [Key]
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }
}
