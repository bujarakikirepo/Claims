namespace Domain.Interfaces
{
    public interface IAuditingService
    {
        Task AuditClaim(string id, string httpRequestType);
        Task AuditCover(string id, string httpRequestType);
    }
}
