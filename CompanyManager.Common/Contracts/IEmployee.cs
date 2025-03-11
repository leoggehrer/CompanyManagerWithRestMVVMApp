//@CodeCopy
namespace CompanyManager.Common.Contracts
{
    /// <summary>
    /// Represents an employee in the company manager.
    /// </summary>
    public interface IEmployee : IIdentifiable
    {
        #region Properties
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
        #endregion Properties

        #region Methods
        /// <summary>
        /// Copies the properties of the specified employee to this employee.
        /// </summary>
        /// <param name="employee">The employee object that is copied.</param>
        void CopyProperties(IEmployee employee)
        {
            if (employee == null) throw new ArgumentNullException(nameof(employee));

            Id = employee.Id;
            CompanyId = employee.CompanyId;
            FirstName = employee.FirstName;
            LastName = employee.LastName;
            Email = employee.Email;
        }
        #endregion Methods
    }
}
