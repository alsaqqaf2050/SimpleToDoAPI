namespace SimpleToDoAPI.Configurations
{
    public static class AuditConfiguration
    {
        private static readonly Dictionary<Type, HashSet<string>> _ignoredProperties = new();

        public static void IgnoreProperty<TEntity>(string propertyName)
        {
            var type = typeof(TEntity);

            if (!_ignoredProperties.ContainsKey(type))
            {
                _ignoredProperties[type] = new HashSet<string>();
            }

            _ignoredProperties[type].Add(propertyName);
        }

        public static bool IsIgnored(Type entityType, string propertyName)
        {
            return _ignoredProperties.TryGetValue(entityType, out var properties)
                   && properties.Contains(propertyName);
        }
    }
}