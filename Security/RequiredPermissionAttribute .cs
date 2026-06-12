namespace SimpleToDoAPI.Security
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequiredPermissionAttribute : Attribute
    {
        public string Permission { get; }

        public RequiredPermissionAttribute(string permission)
        {
            Permission = permission;
        }
    }
}