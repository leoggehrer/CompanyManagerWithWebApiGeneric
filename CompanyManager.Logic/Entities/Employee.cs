using CompanyManager.Logic.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyManager.Logic.Entities
{
    /// <summary>
    /// Represents an employee entity.
    /// </summary>
    public class Employee : EntityObject, IEmployee
    {
        #region properties
        /// <summary>
        /// Gets or sets the company ID.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Gets or sets the first name of the employee.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the employee.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email of the employee.
        /// </summary>
        public string Email { get; set; } = string.Empty;
        #endregion properties

        #region navigation properties
        /// <summary>
        /// Gets or sets the associated company.
        /// </summary>
        public Company? Company { get; set; }
        #endregion navigation properties

        #region methods
        /// <summary>
        /// Copies properties from another employee.
        /// </summary>
        /// <param name="employee">The employee to copy properties from.</param>
        public virtual void CopyProperties(IEmployee employee)
        {
            base.CopyProperties(employee);

            CompanyId = employee.CompanyId;
            FirstName = employee.FirstName;
            LastName = employee.LastName;
            Email = employee.Email;
        }

        /// <summary>
        /// Returns a string representation of the employee.
        /// </summary>
        /// <returns>A string representation of the employee.</returns>
        public override string ToString()
        {
            return $"{FirstName} {LastName}";
        }
        #endregion methods
    }
}
