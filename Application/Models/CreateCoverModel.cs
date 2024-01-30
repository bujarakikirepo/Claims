using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class CreateCoverModel
    {
        [Required]
        public DateOnly StartDate { get; set; }
        [Required]
        public DateOnly EndDate { get; set; }
        [Required]
        public CoverType Type { get; set; }
    }
}
