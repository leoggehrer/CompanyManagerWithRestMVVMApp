//@CodeCopy
using System.Net;

namespace CompanyManager.Common.Contracts
{
    /// <summary>
    /// Represents a customer in the company manager.
    /// </summary>
    public interface ICustomer : IIdentifiable
    {
        #region Properties
        /// <summary>
        /// Gets or sets the reference to the company.
        /// </summary>
        int CompanyId { get; set; }
        /// <summary>
        /// Gets or sets the name of the customer.
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Gets or sets email of the customer.
        /// </summary>
        string Email { get; set; }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Copies the properties of the specified customer to this customer.
        /// </summary>
        /// <param name="customer">The customer object that is copied.</param>
        void CopyProperties(ICustomer customer)
        {
            if (customer == null) throw new ArgumentNullException(nameof(customer));

            Id = customer.Id;
            CompanyId = customer.CompanyId;
            Name = customer.Name;
            Email = customer.Email;
        }
        #endregion Methods
    }
}
