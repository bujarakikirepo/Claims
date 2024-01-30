using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class CreateClaimModel
    {
        [Required]
        public string CoverId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public ClaimType Type { get; set; }
        [Required]
        public decimal DamageCost { get; set; }
    }
}
