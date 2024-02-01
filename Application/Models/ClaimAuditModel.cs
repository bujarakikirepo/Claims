namespace Domain.Models
{
    public class ClaimAuditModel
    {
        public required string ClaimId { get; set; }

        public DateTime Created { get; set; }

        public required string HttpRequestType { get; set; }
    }
}
