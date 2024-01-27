using Domain.Enums;

namespace Domain.Entities
{
    public class Cover
    {
        public string Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public CoverType Type { get; set; }
        public decimal Premium { get; set; }
    }
}
