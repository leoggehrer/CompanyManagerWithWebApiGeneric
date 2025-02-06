using CompanyManager.WebApi.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CompanyManager.WebApi.Controllers
{
    /// <summary>
    /// Provides access to the database context and its DbSets.
    /// </summary>
    public sealed class ContextAccessor : IContextAccessor
    {
        #region fields
        private Logic.Contracts.IContext? context = null;
        #endregion fields

        /// <summary>
        /// Gets the current context or creates a new one if it doesn't exist.
        /// </summary>
        /// <returns>The current context.</returns>
        public Logic.Contracts.IContext GetContext() => context ??= Logic.DataContext.Factory.CreateContext();

        /// <summary>
        /// Gets the DbSet for the specified entity type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>The DbSet for the specified entity type, or null if the entity type is not recognized.</returns>
        public DbSet<TEntity>? GetDbSet<TEntity>() where TEntity : class
        {
            DbSet<TEntity>? result = default;

            if (typeof(TEntity) == typeof(Logic.Entities.Company))
            {
                result = GetContext().CompanySet as DbSet<TEntity>;
            }
            else if (typeof(TEntity) == typeof(Logic.Entities.Customer))
            {
                result = GetContext().CustomerSet as DbSet<TEntity>;
            }
            else if (typeof(TEntity) == typeof(Logic.Entities.Employee))
            {
                result = GetContext().EmployeeSet as DbSet<TEntity>;
            }
            return result;
        }

        /// <summary>
        /// Disposes the current context.
        /// </summary>
        public void Dispose()
        {
            context?.Dispose();
            context = null;
        }
    }
}
