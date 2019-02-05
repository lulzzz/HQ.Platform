namespace HQ.Platform.Api.Models
{
    public class TenantContext<TTenant> where TTenant : class
    {
        public TTenant Tenant { get; set; }
        public string[] Identifiers { get; set; }
    }
}