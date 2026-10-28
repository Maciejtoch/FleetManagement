namespace FleetManagement.Services
{
    public interface ITenantContext
    {
        int? TenantId { get; }
        bool IsAdmin { get; }
        bool IsOwner { get; }
        string? TenantDomain { get; }
    }
}
