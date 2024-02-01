using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class CreateClaimModel
    {
        [Required]
        public required string CoverId { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public ClaimType Type { get; set; }
        [Required]
        public decimal DamageCost { get; set; }
    }
}
