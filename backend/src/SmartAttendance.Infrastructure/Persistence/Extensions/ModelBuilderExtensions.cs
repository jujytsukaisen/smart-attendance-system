using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Extensions;

/// <summary>
/// Extension methods for <see cref="ModelBuilder"/> to apply global query filters
/// for soft delete and multi-tenancy across all entity types.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies a global query filter of <c>IsDeleted == false</c> on every entity type
    /// that derives from <see cref="BaseEntity"/> but NOT from <see cref="BaseTenantEntity"/>.
    /// Tenant entities should use <see cref="ApplyTenantFilter"/> instead, which includes
    /// the soft-delete condition in the combined filter expression.
    /// </summary>
    /// <remarks>
    /// EF Core only supports one query filter per entity type. Calling <c>HasQueryFilter</c>
    /// replaces any existing filter. This method must therefore skip tenant entities to avoid
    /// overwriting the combined tenant + soft-delete filter applied by <see cref="ApplyTenantFilter"/>.
    /// </remarks>
    public static void ApplySoftDeleteFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            // Skip owned types — they inherit filters from their owner
            if (entityType.IsOwned())
                continue;

            // Skip tenant entities — they already have a combined filter from ApplyTenantFilter
            if (typeof(BaseTenantEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var condition = Expression.Equal(property, Expression.Constant(false));
            var lambda = Expression.Lambda(condition, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    /// <summary>
    /// Applies a global query filter of <c>IsDeleted == false</c> on ALL entity types
    /// that derive from <see cref="BaseEntity"/>, including <see cref="BaseTenantEntity"/> types.
    /// Use this when no tenant context is available (e.g., system-level operations).
    /// </summary>
    public static void ApplySoftDeleteFilterForAll(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            if (entityType.IsOwned())
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var condition = Expression.Equal(property, Expression.Constant(false));
            var lambda = Expression.Lambda(condition, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    /// <summary>
    /// Applies a global query filter of <c>CompanyId == tenantId</c> on every entity type
    /// that derives from <see cref="BaseTenantEntity"/>. The filter is combined with
    /// the soft-delete filter if both apply to the same entity.
    /// </summary>
    /// <remarks>
    /// Because EF Core only supports one query filter per entity, this method must build
    /// a combined expression that includes both the soft-delete and tenant conditions.
    /// Call this AFTER <see cref="ApplySoftDeleteFilter"/> or combine both in a single pass.
    /// </remarks>
    public static void ApplyTenantFilter(this ModelBuilder modelBuilder, Guid tenantId)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseTenantEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            if (entityType.IsOwned())
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");

            // Soft delete condition: e.IsDeleted == false
            var isDeletedProp = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var softDeleteCondition = Expression.Equal(isDeletedProp, Expression.Constant(false));

            // Tenant condition: e.CompanyId == tenantId
            var companyIdProp = Expression.Property(parameter, nameof(BaseTenantEntity.CompanyId));
            var tenantCondition = Expression.Equal(companyIdProp, Expression.Constant(tenantId));

            // Combined: e.IsDeleted == false && e.CompanyId == tenantId
            var combinedCondition = Expression.AndAlso(softDeleteCondition, tenantCondition);
            var lambda = Expression.Lambda(combinedCondition, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    /// <summary>
    /// Configures all <see cref="BaseEntity"/>-derived entity types to ignore the
    /// <c>DomainEvents</c> navigation property since it is not persisted.
    /// </summary>
    public static void IgnoreDomainEventProperties(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            if (entityType.IsOwned())
                continue;

            modelBuilder.Entity(entityType.ClrType).Ignore(nameof(BaseEntity.DomainEvents));
        }
    }
}
