using CompanyManager.Logic.Contracts;

namespace CompanyManager.Logic.Entities
{
    /// <summary>
    /// Represents an abstract base class for entities with an identifier.
    /// </summary>
    public abstract class EntityObject : IIdentifiable
    {
        /// <summary>
        /// Gets or sets the identifier of the entity.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }

        /// <summary>
        /// Copies the properties from another identifiable entity.
        /// </summary>
        /// <param name="other">The other identifiable entity to copy properties from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the other entity is null.</exception>
        public virtual void CopyProperties(IIdentifiable other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            Id = other.Id;
        }
    }
}
