namespace Application.Models
{
    public class CoverAuditModel
    {
        public string? CoverId { get; set; }

        public DateTime Created { get; set; }

        public string? HttpRequestType { get; set; }
    }
}
