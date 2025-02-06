namespace CompanyManager.WebApi.Models
{
    /// <summary>
    /// Represents a customer in the company manager.
    /// </summary>
    public class Customer : ModelObject, Logic.Contracts.ICustomer
    {
        /// <summary>
        /// Gets or sets the reference to the company.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Gets or sets the name of the customer.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email of the customer.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Copies the properties from another customer object.
        /// </summary>
        /// <param name="other">The other customer object to copy properties from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the other object is null.</exception>
        public virtual void CopyProperties(Logic.Contracts.ICustomer other)
        {
            base.CopyProperties(other);

            CompanyId = other.CompanyId;
            Name = other.Name;
            Email = other.Email;
        }

        /// <summary>
        /// Creates a new customer instance from an existing customer.
        /// </summary>
        /// <param name="customer">The customer instance to copy properties from.</param>
        /// <returns>A new customer instance.</returns>
        public static Customer Create(Logic.Contracts.ICustomer customer)
        {
            var result = new Customer();

            result.CopyProperties(customer);
            return result;
        }

    }
}
