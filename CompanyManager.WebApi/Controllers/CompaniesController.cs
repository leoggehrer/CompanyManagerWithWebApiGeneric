using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace CompanyManager.WebApi.Controllers
{
    using TModel = Models.Company;
    using TEntity = Logic.Entities.Company;

    /// <summary>
    /// Controller for managing companies.
    /// </summary>
    public class CompaniesController : GenericController<TModel, TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompaniesController"/> class.
        /// </summary>
        /// <param name="contextAccessor">The context accessor.</param>
        public CompaniesController(Contracts.IContextAccessor contextAccessor)
            : base(contextAccessor)
        {
        }

        /// <summary>
        /// Converts an entity to a model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The model.</returns>
        protected override TModel ToModel(TEntity entity)
        {
            var result = new TModel();

            result.CopyProperties(entity);
            if (entity.Customers != null)
            {
                result.Customers = entity.Customers.Select(e => Models.Customer.Create(e)).ToArray();
            }
            return result;
        }

        /// <summary>
        /// Converts a model to an entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity.</returns>
        protected override TEntity ToEntity(TModel model, TEntity? entity)
        {
            var result = entity ??= new TEntity();

            result.CopyProperties(model);
            return result;
        }

        /// <summary>
        /// Gets a company by ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>The company model.</returns>
        public override ActionResult<TModel?> Get(int id)
        {
            var dbSet = EntitySet.Include(e => e.Customers);
            var result = dbSet.FirstOrDefault(e => e.Id == id);

            return result == null ? NotFound() : Ok(ToModel(result));
        }
    }
}
