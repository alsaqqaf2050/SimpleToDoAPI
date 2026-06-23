namespace SimpleToDoAPI.Constants
{
    public static class Policies
    {
        public const string AdminOnly = "AdminOnly";

        // Todos
        public const string TodoView = Permissions.Todos.View;
        public const string TodoCreate = Permissions.Todos.Create;
        public const string TodoUpdate = Permissions.Todos.Update;
        public const string TodoDelete = Permissions.Todos.Delete;

        // Users
        public const string UserView = Permissions.Users.View;
        public const string UserCreate = Permissions.Users.Create;
        public const string UserUpdate = Permissions.Users.Update;
        public const string UserDelete = Permissions.Users.Delete;

        // Roles
        public const string RoleView = Permissions.Roles.View;
        public const string RoleCreate = Permissions.Roles.Create;
        public const string RoleUpdate = Permissions.Roles.Update;
        public const string RoleDelete = Permissions.Roles.Delete;

        // Audit
        public const string AuditView = Permissions.Audit.View;
    }
}