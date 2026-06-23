namespace SimpleToDoAPI.Constants
{
    public static class AuditActions
    {
        public const string CreateUser = "Create User";
        public const string UpdateUser = "Update User";
        public const string DeleteUser = "Delete User";
        public const string RestoreUser = "Restore User";

        public const string CreateRole = "Create Role";
        public const string UpdateRole = "Update Role";
        public const string DeleteRole = "Delete Role";

        public const string AssignRole = "Assign Role";
        public const string RemoveRole = "Remove Role";

        public const string AssignPermission = "Assign Permission";
        public const string RemovePermission = "Remove Permission";

        public const string Login = "Login";
        public const string Logout = "Logout";
        public const string RefreshToken = "Refresh Token";
    }
}