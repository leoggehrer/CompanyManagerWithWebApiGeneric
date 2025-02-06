namespace CompanyManager.WebApi.Models
{
    /// <summary>
    /// Represents a company entity.
    /// </summary>
    public class Company : ModelObject, Logic.Contracts.ICompany
    {
        /// <summary>
        /// Gets or sets the name of the company.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of the company.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Gets or sets the description of the company.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the customers of the company.
        /// </summary>
        public Logic.Contracts.ICustomer[] Customers { get; set; } = [];

        /// <summary>
        /// Copies the properties from another company instance.
        /// </summary>
        /// <param name="other">The company instance to copy properties from.</param>
        public virtual void CopyProperties(Logic.Contracts.ICompany other)
        {
            base.CopyProperties(other);

            Name = other.Name;
            Address = other.Address;
            Description = other.Description;
        }

        /// <summary>
        /// Creates a new company instance from an existing company.
        /// </summary>
        /// <param name="company">The company instance to copy properties from.</param>
        /// <returns>A new company instance.</returns>
        public static Company Create(Logic.Contracts.ICompany company)
        {
            var result = new Company();

            result.CopyProperties(company);
            return result;
        }
    }
}
