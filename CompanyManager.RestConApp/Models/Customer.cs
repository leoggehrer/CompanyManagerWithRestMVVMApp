namespace CompanyManager.RestConApp.Models
{
    /// <summary>
    /// Represents a customer in the company manager.
    /// </summary>
    public class Customer : ModelObject, Common.Contracts.ICustomer
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

        #region methods
        /// <summary>
        /// Returns a string representation of the customer.
        /// </summary>
        /// <returns>A string representation of the customer.</returns>
        public override string ToString()
        {
            return $"Customer: {Name}";
        }
        #endregion methods
    }
}
