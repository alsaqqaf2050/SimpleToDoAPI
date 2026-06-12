//using SimpleToDoAPI.Constants;

//namespace SimpleToDoAPI.Authorization
//{
//    public static class PermissionStore
//    {
//        public static List<string> GetPermissions(string role)
//        {
//            return role switch
//            {
//                "Admin" => new List<string>
//                {
//                    Permissions.Todos.View,
//                    Permissions.Todos.Create,
//                    Permissions.Todos.Update,
//                    Permissions.Todos.Delete,

//                    Permissions.Users.View,
//                    Permissions.Users.Create,
//                    Permissions.Users.Update,
//                    Permissions.Users.Delete
//                },

//                "User" => new List<string>
//                {
//                    Permissions.Todos.View,
//                    Permissions.Todos.Create,
//                    Permissions.Todos.Update,
//                    Permissions.Todos.Delete,
//                },

//                _ => new List<string>()
//            };
//        }
//    }
//}