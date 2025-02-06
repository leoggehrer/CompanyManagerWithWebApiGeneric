namespace CompanyManager.WebApi.Models
{
    /// <summary>
    /// Represents an employee in the company manager.
    /// </summary>
    public class Employee : ModelObject, Logic.Contracts.IEmployee
    {
        /// <summary>
        /// Gets or sets the reference to the company.
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

        /// <summary>
        /// Copies the properties from another employee object.
        /// </summary>
        /// <param name="other">The other employee object to copy properties from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the other object is null.</exception>
        public virtual void CopyProperties(Logic.Contracts.IEmployee other)
        {
            base.CopyProperties(other);

            CompanyId = other.CompanyId;
            FirstName = other.FirstName;
            LastName = other.LastName;
            Email = other.Email;
        }
    }
}
