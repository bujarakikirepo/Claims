using Domain.Enums;

namespace Application.Models
{
    public class CreateCoverModel
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public CoverType Type { get; set; }
        public decimal Premium { get; set; }
    }
}
