namespace Domain.Models
{
    public class ClaimAuditModel
    {
        public string? ClaimId { get; set; }

        public DateTime Created { get; set; }

        public string? HttpRequestType { get; set; }
    }
}
