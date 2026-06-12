namespace SimpleToDoAPI.Constants
{
    public static class Policies
    {
        public const string AdminOnly = "AdminOnly";

        public const string TodoView = Permissions.Todos.View;

        public const string TodoCreate = Permissions.Todos.Create;

        public const string TodoUpdate = Permissions.Todos.Update;

        public const string TodoDelete = Permissions.Todos.Delete;
    }
}