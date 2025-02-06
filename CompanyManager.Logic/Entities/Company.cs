using CompanyManager.Logic.Contracts;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyManager.Logic.Entities
{
    /// <summary>
    /// Represents a company entity.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.Table("Companies")]
    [Index(nameof(Name), IsUnique = true)]
    public class Company : EntityObject, ICompany
    {
        #region properties
        /// <summary>
        /// Gets or sets the name of the company.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of the company.
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(1024)]
        public string? Address { get; set; }

        /// <summary>
        /// Gets or sets the description of the company.
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(2048)]
        public string? Description { get; set; }
        #endregion properties

        #region navigation properties
        /// <summary>
        /// Gets or sets the list of customers associated with the company.
        /// </summary>
        public List<Customer> Customers { get; set; } = [];
        /// <summary>
        /// Gets or sets the list of employees associated with the company.
        /// </summary>
        public List<Employee> Employees { get; set; } = [];
        #endregion navigation properties

        #region methods
        /// <summary>
        /// Copies the properties from another company instance.
        /// </summary>
        /// <param name="company">The company instance to copy properties from.</param>
        public virtual void CopyProperties(ICompany company)
        {
            base.CopyProperties(company);

            Name = company.Name;
            Address = company.Address;
            Description = company.Description;
        }

        /// <summary>
        /// Returns a string representation of the company.
        /// </summary>
        /// <returns>A string that represents the company.</returns>
        public override string ToString()
        {
            return $"Company: {Name}";
        }
        #endregion methods
    }
}
