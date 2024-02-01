namespace Application.Models
{
    public class CoverAuditModel
    {
        public required string CoverId { get; set; }

        public DateTime Created { get; set; }

        public required string HttpRequestType { get; set; }
    }
}
