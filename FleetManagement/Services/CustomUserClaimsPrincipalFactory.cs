using System.Security.Claims;
using FleetManagement.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FleetManagement.Services
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, IdentityRole>
    {
        public CustomUserClaimsPrincipalFactory(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            if (user.TenantId.HasValue)
            {
                identity.AddClaim(new Claim("tenant_id", user.TenantId.Value.ToString()));
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var tenantDomain = user.Email.Split('@').LastOrDefault();
                if (!string.IsNullOrWhiteSpace(tenantDomain))
                {
                    identity.AddClaim(new Claim("tenant_domain", tenantDomain));
                }
            }

            return identity;
        }
    }
}
