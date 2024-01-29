using Domain.Entities;
using Domain.Interfaces;

namespace Infrastructure.Auditing
{
    public class AuditingService : IAuditingService
    {
        private readonly AuditContext _auditContext;

        public AuditingService(AuditContext auditContext)
        {
            _auditContext = auditContext;
        }

        public async Task AuditClaim(string id, string httpRequestType)
        {
            var claimAudit = new ClaimAudit()
            {
                Created = DateTime.Now,
                HttpRequestType = httpRequestType,
                ClaimId = id
            };

            _auditContext.Add(claimAudit);
            await _auditContext.SaveChangesAsync();
        }

        public async Task AuditCover(string id, string httpRequestType)
        {
            var coverAudit = new CoverAudit()
            {
                Created = DateTime.Now,
                HttpRequestType = httpRequestType,
                CoverId = id
            };

            _auditContext.Add(coverAudit);
            await _auditContext.SaveChangesAsync();
        }
    }
}
