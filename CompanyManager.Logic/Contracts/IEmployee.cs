namespace CompanyManager.Logic.Contracts
{
    /// <summary>
    /// Represents an employee in the company manager.
    /// </summary>
    public interface IEmployee : IIdentifiable
    {
        /// <summary>
        /// Gets or sets the reference to the company.
        /// </summary>
        int CompanyId { get; set; }
        /// <summary>
        /// Gets or sets the first name of the employee.
        /// </summary>
        string FirstName { get; set; }
        /// <summary>
        /// Gets or sets the last name of the employee.
        /// </summary>
        string LastName { get; set; }
        /// <summary>
        /// Gets or sets email of the employee.
        /// </summary>
        string Email { get; set; }
    }
}
