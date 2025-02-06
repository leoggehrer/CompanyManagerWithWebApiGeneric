namespace CompanyManager.WebApi.Controllers
{
    using TModel = Models.Customer;
    using TEntity = Logic.Entities.Customer;

    /// <summary>
    /// Controller for handling customer-related operations.
    /// </summary>
    public class CustomersController : GenericController<TModel, TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomersController"/> class.
        /// </summary>
        /// <param name="contextAccessor">The context accessor.</param>
        public CustomersController(Contracts.IContextAccessor contextAccessor)
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
    }
}
