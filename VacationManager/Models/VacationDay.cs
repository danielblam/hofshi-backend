namespace VacationManager.Models
{
    public class VacationDay
    {
        public int? VacationDayId { get; set; }
        public int? VacationId { get; set; }
        public int DayType { get; set; }
        public DateOnly Date { get; set; }
        public short? Status { get; set; }
    }
}
