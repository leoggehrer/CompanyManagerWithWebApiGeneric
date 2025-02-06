namespace CompanyManager.WebApi.Models
{
    /// <summary>
    /// Represents an abstract base class for model objects that are identifiable.
    /// </summary>
    public abstract class ModelObject : Logic.Contracts.IIdentifiable
    {
        /// <summary>
        /// Gets or sets the unique identifier for the model object.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Copies the properties from another identifiable object.
        /// </summary>
        /// <param name="other">The other identifiable object to copy properties from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the other object is null.</exception>
        public virtual void CopyProperties(Logic.Contracts.IIdentifiable other)
        {
            ArgumentNullException.ThrowIfNull(other);

            Id = other.Id;
        }
    }
}
