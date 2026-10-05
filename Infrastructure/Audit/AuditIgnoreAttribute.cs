namespace SimpleToDoAPI.Infrastructure.Audit
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class AuditIgnoreAttribute : Attribute
    {
    }
}