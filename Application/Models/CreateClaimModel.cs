using Domain.Enums;

namespace Application.Models
{
    public class CreateClaimModel
    {
        public string Id { get; set; }
        public string CoverId { get; set; }
        public string Name { get; set; }
        public ClaimType Type { get; set; }
        public decimal DamageCost { get; set; }
    }
}
