namespace VacationManager.Models
{
    public class Vacation
    {
        public int VacationId { get; set; }
        public int UserId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public short Status { get; set; }
        public int? ResolvedBy { get; set; }
    }
}
