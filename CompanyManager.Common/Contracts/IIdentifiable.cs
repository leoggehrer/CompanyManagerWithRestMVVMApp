//@CodeCopy
namespace CompanyManager.Common.Contracts
{
    /// <summary>
    /// Represents an identifiable in the company manager.
    /// </summary>
    public interface IIdentifiable
    {
        #region Properties
        /// <summary>
        /// Gets the unique identifier for the entity.
        /// </summary>
        int Id { get; protected set; }
        #endregion Properties
    }
}
