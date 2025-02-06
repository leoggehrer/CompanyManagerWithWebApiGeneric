namespace CompanyManager.Logic.Contracts
{
    /// <summary>
    /// Represents a customer in the company manager.
    /// </summary>
    public interface ICustomer : IIdentifiable
    {
        /// <summary>
        /// Gets or sets the reference to the company.
        /// </summary>
        int CompanyId { get; set; }
        /// <summary>
        /// Gets or sets the name of the customer.
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Gets or sets email of the customer.
        /// </summary>
        string Email { get; set; }
    }
}
