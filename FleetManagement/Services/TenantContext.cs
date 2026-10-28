using FleetManagement.Core.Models;
using Microsoft.AspNetCore.Http;

namespace FleetManagement.Services
{
    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? TenantId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                {
                    return null;
                }

                var tenantClaim = user.FindFirst("tenant_id") ?? user.FindFirst("tenantId");
                if (tenantClaim != null && int.TryParse(tenantClaim.Value, out var parsedTenantId))
                {
                    return parsedTenantId;
                }

                return null;
            }
        }

        public bool IsAdmin => _httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;

        public bool IsOwner => _httpContextAccessor.HttpContext?.User.IsInRole("Owner") ?? false;

        public string? TenantDomain
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null)
                {
                    return null;
                }

                var tenantClaim = user.FindFirst("tenant_domain") ?? user.FindFirst("tenantDomain");
                return tenantClaim?.Value;
            }
        }
    }
}
