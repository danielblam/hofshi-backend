namespace VacationManager.Models
{
    public class VacationRequest
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public List<VacationDay>? VacationDays { get; set; }
    }
}
