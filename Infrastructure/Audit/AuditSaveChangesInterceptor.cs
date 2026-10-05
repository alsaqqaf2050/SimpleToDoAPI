using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SimpleToDoAPI.Data;
using SimpleToDoAPI.Services.Interfaces;
using SimpleToDoAPI.Services.Common;
using System.Text.Json;
using SimpleToDoAPI.Models;
using SimpleToDoAPI.Configurations;

namespace SimpleToDoAPI.Infrastructure.Audit
{

    /// <summary>
    /// وهذه النقلة هي التي تحول النظام من Audit يدوي إلى Audit احترافي بالكامل
    /// ها ستزيل كمية كبيرة من الكود المكرر في المشروع، وتجعل أي SaveChanges() في النظام يُسجل تلقائياً في سجل التدقيق.
    /// الآن وصلنا إلى أهم نقطة في مشروعنا كله. بعد هذه المرحلة لن نكتب:
    /// await _auditService.LogAsync(...); في أي Service مرة أخرى
    /// أي شخص يكتب: await _context.SaveChangesAsync(); سيتم تسجيل العملية تلقائياً.
    /// </summary>
    public class AuditSaveChangesInterceptor
        : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUser;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditSaveChangesInterceptor(
            ICurrentUserService currentUser,
            IHttpContextAccessor httpContextAccessor)
        {
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
        }


        /// <summary>
        /// دالة تسجيل العمليات AuditLog
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="result"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;

            if (context == null)
                return await base.SavingChangesAsync(eventData, result, cancellationToken);

            //var ignoredProperties = new HashSet<string>
            //        {
            //            "ConcurrencyStamp",
            //            "SecurityStamp",
            //            "PasswordHash",
            //            "NormalizedEmail",
            //            "NormalizedUserName",
            //            "NormalizedName",
            //            "AccessFailedCount",
            //            "LockoutEnd",
            //            "TwoFactorEnabled"
            //        };

            var audits = new List<AuditLog>();

            var entries = context.ChangeTracker
                .Entries()
                .Where(x =>
                    x.State == EntityState.Added ||
                    x.State == EntityState.Modified ||
                    x.State == EntityState.Deleted);

            foreach (var entry in entries)
            {
                var entityType = entry.Entity.GetType();

                // تجاهل الكيانات المستثناة
                if (Attribute.IsDefined(entityType, typeof(AuditIgnoreAttribute)))
                    continue;

                var action = entry.State switch
                {
                    EntityState.Added => "Create",
                    EntityState.Modified => "Update",
                    EntityState.Deleted => "Delete",
                    _ => ""
                };

                var primaryKey = entry.Metadata.FindPrimaryKey();

                var entityId = string.Empty;

                if (primaryKey != null)
                {
                    entityId = string.Join(",",
                        primaryKey.Properties
                            .Select(p => entry.Property(p.Name).CurrentValue?.ToString()));
                }

                var oldValues = new Dictionary<string, object?>();

                var newValues = new Dictionary<string, object?>();

                //foreach (var property in entry.Properties)
                //{
                //    if (property.Metadata.IsPrimaryKey())
                //        continue;

                //    if (ignoredProperties.Contains(property.Metadata.Name))
                //        continue;

                //    switch (entry.State)
                //    {
                //        case EntityState.Added:

                //            newValues[property.Metadata.Name] =
                //                property.CurrentValue;

                //            break;

                //        case EntityState.Modified:

                //            if (!property.IsModified)
                //                continue;

                //            oldValues[property.Metadata.Name] =
                //                property.OriginalValue;

                //            newValues[property.Metadata.Name] =
                //                property.CurrentValue;

                //            break;

                //        case EntityState.Deleted:

                //            oldValues[property.Metadata.Name] =
                //                property.OriginalValue;

                //            break;
                //    }
                //}


                foreach (var property in entry.Properties)
                {
                    // تجاهل المفتاح الأساسي
                    if (property.Metadata.IsPrimaryKey())
                        continue;

                    //// تجاهل الخصائص التي عليها [AuditIgnore]
                    //if (property.Metadata.PropertyInfo?
                    //        .IsDefined(typeof(AuditIgnoreAttribute), true) == true)
                    //{
                    //    continue;
                    //}

                    if (AuditConfiguration.IsIgnored(entityType,property.Metadata.Name))
                    {
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:

                            newValues[property.Metadata.Name] =
                                property.CurrentValue;

                            break;

                        case EntityState.Modified:

                            if (!property.IsModified)
                                continue;

                            oldValues[property.Metadata.Name] =
                                property.OriginalValue;

                            newValues[property.Metadata.Name] =
                                property.CurrentValue;

                            break;

                        case EntityState.Deleted:

                            oldValues[property.Metadata.Name] =
                                property.OriginalValue;

                            break;
                    }
                }



                audits.Add(new AuditLog
                {
                    UserId = _currentUser.UserId,

                    UserName =
                        _httpContextAccessor.HttpContext?
                        .User?
                        .Identity?
                        .Name ?? "",

                    Action = action,

                    EntityName = entityType.Name,

                    EntityId = entityId,

                    OldValues = oldValues.Count == 0
                        ? null
                        : JsonSerializer.Serialize(oldValues),

                    NewValues = newValues.Count == 0
                        ? null
                        : JsonSerializer.Serialize(newValues),

                    IpAddress =
                        _httpContextAccessor.HttpContext?
                        .Connection?
                        .RemoteIpAddress?
                        .ToString(),

                    CreatedOn = DateTime.UtcNow
                });
            }

            if (audits.Any())
            {
                context.Set<AuditLog>().AddRange(audits);
            }

            return await base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);
        }



        //public override async ValueTask<
        //    InterceptionResult<int>>
        //    SavingChangesAsync(
        //        DbContextEventData eventData,
        //        InterceptionResult<int> result,
        //        CancellationToken cancellationToken = default)
        //{
        //    return await base.SavingChangesAsync(
        //        eventData,
        //        result,
        //        cancellationToken);
        //}


        //public override async ValueTask<InterceptionResult<int>>
        //SavingChangesAsync(DbContextEventData eventData,InterceptionResult<int> result, CancellationToken cancellationToken = default)

        //    {
        //        var context = eventData.Context;

        //        if (context == null)
        //        {
        //            return await base.SavingChangesAsync(
        //                eventData,
        //                result,
        //                cancellationToken);
        //        }

        //        var entries = context.ChangeTracker
        //            .Entries()
        //            .Where(x =>
        //                x.State == EntityState.Added ||
        //                x.State == EntityState.Modified ||
        //                x.State == EntityState.Deleted)
        //            .ToList();

        //        foreach (var entry in entries)
        //        {
        //            var entityType = entry.Entity.GetType();

        //            //if (Attribute.IsDefined(entityType, typeof(AuditIgnoreAttribute)))
        //            if (entityType.GetCustomAttributes(typeof(AuditIgnoreAttribute), true).Any())
        //                continue;



        //            var primaryKey = entry.Metadata.FindPrimaryKey();

        //            var entityId = string.Empty;

        //            if (primaryKey != null)
        //            {
        //                entityId = string.Join(",",
        //                    primaryKey.Properties
        //                        .Select(p => entry.Property(p.Name).CurrentValue?.ToString()));
        //            }


        //            var oldValues = new Dictionary<string, object?>();

        //            var newValues = new Dictionary<string, object?>();


        //            var ignoredProperties = new HashSet<string>
        //                    {
        //                        "ConcurrencyStamp",
        //                        "SecurityStamp",
        //                        "PasswordHash",
        //                        "NormalizedName",
        //                        "NormalizedEmail",
        //                        "NormalizedUserName",
        //                        "AccessFailedCount",
        //                        "LockoutEnd",
        //                        "TwoFactorEnabled"
        //                    };


        //            foreach (var property in entry.Properties)
        //            {
        //                // تجاهل الـ Navigation Properties
        //                if (property.Metadata.IsPrimaryKey())
        //                    continue;

        //                if (ignoredProperties.Contains(property.Metadata.Name))
        //                    continue;

        //                switch (entry.State)
        //                {
        //                    case EntityState.Added:

        //                        newValues[property.Metadata.Name] =
        //                            property.CurrentValue;

        //                        break;

        //                    case EntityState.Modified:

        //                        //وهذه نقطة احترافية جداً، لأنها تمنع تسجيل الحقول التي لم تتغير
        //                        if (!property.IsModified)
        //                            continue;

        //                        oldValues[property.Metadata.Name] =
        //                            property.OriginalValue;

        //                        newValues[property.Metadata.Name] =
        //                            property.CurrentValue;

        //                        break;

        //                    case EntityState.Deleted:

        //                        oldValues[property.Metadata.Name] =
        //                            property.OriginalValue;

        //                        break;
        //                }
        //            }

        //            System.Diagnostics.Debug.WriteLine("Old Values");

        //            foreach (var item in oldValues)
        //            {
        //                System.Diagnostics.Debug.WriteLine(
        //                    $"{item.Key} = {item.Value}");
        //            }

        //            System.Diagnostics.Debug.WriteLine("New Values");

        //            foreach (var item in newValues)
        //            {
        //                System.Diagnostics.Debug.WriteLine(
        //                    $"{item.Key} = {item.Value}");
        //            }

        //            var action = entry.State switch
        //            {
        //                EntityState.Added => "Create",
        //                EntityState.Modified => "Update",
        //                EntityState.Deleted => "Delete",
        //                _ => ""
        //            };





        //            var audit = new AuditLog
        //            {
        //                UserId = _currentUser.UserId,

        //                UserName =
        //                    _httpContextAccessor
        //                        .HttpContext?
        //                        .User?
        //                        .Identity ?

        //                        ?.Name ?? "",

        //                Action = action,

        //                EntityName = entityType.Name,

        //                EntityId = entityId,

        //                OldValues =
        //                    oldValues.Count == 0
        //                        ? null
        //                        : JsonSerializer.Serialize(oldValues),

        //                NewValues =
        //                    newValues.Count == 0
        //                        ? null
        //                        : JsonSerializer.Serialize(newValues),

        //                IpAddress =
        //                    _httpContextAccessor
        //                        .HttpContext?
        //                        .Connection?
        //                        .RemoteIpAddress?
        //                        .ToString(),

        //                CreatedOn = DateTime.UtcNow
        //            };



        //            System.Diagnostics.Debug.WriteLine("--------------------------------");
        //            System.Diagnostics.Debug.WriteLine($"Entity   : {entityType.Name}");
        //            System.Diagnostics.Debug.WriteLine($"Action   : {action}");
        //            System.Diagnostics.Debug.WriteLine($"EntityId : {entityId}");
        //            System.Diagnostics.Debug.WriteLine("--------------------------------");
        //        }

        //        return await base.SavingChangesAsync(
        //            eventData,
        //            result,
        //            cancellationToken);
        //    }



    }
}