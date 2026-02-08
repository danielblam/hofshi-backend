namespace VacationManager.Models
{
    public class User
    {
        public int? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? Role { get; set; }
        public int? TeamId { get; set; }
        public bool? IsActive { get; set; }
    }
}
