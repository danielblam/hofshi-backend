namespace VacationManager.Models
{
    public class Event
    {
        public int? EventId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? TeamId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsPublic { get; set; }
    }
}
