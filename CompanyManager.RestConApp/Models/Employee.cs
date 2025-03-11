namespace CompanyManager.RestConApp.Models
{
    /// <summary>
    /// Represents an employee in the company manager.
    /// </summary>
    public class Employee : ModelObject, Common.Contracts.IEmployee
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

        #region methods
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
