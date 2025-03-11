namespace CompanyManager.RestConApp.Models
{
    /// <summary>
    /// Represents a company entity.
    /// </summary>
    public class Company : ModelObject, Common.Contracts.ICompany
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
        public Common.Contracts.ICustomer[] Customers { get; set; } = [];
        #region methods
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
