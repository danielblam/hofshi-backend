namespace VacationManager.Models
{
    public class Session
    {
        public int UserID { get; set; }
        public string? Token { get; set; }
        public long LoginTime { get; set; }
    }
}
