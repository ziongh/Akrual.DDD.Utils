using System;
using Akrual.DDD.Utils.Domain.Exceptions;

namespace Akrual.DDD.Utils.Domain.Entities
{
    /// <summary>
    ///     Base class for any Entity Type (i.e. the 'Entity Object' of DDD).
    ///     <remarks><c>All properties of this child classes should have the setter as private!</c></remarks>
    /// </summary>
    /// <typeparam name="T">Domain type to be 'turned' into a Entity Type.</typeparam>
    public abstract class Entity<T> : IEntity
    {
        /// <exception cref="ShouldNeverCallIsValidOnTheEntityException">Should never call It!!</exception>
        public bool IsValid => throw new ShouldNeverCallIsValidOnTheEntityException();

        public BaseDomainStatus Status { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Entity"/> class.
        ///     Every constructor have to satisfy:
        ///     <remarks><c>If the entity is not an Aggregate Root, then it cannot be created without passing the owner of this entity (Some entity inside the Aggregate, maybe the Aggregate root)!</c></remarks>
        ///     <remarks><c>GUID. This must be unique inside the Aggegate.!</c></remarks>
        ///     <remarks><c>You should probably extend this constructor to include every property that makes this entity unique.</c></remarks>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="owner"></param>
        protected Entity(Guid id, IEntity owner)
        {
            if (id == default(Guid))
            {
                throw new ArgumentException("Id must be defined.", "id");
            }

            Id = id;
        }

        /// <summary>
        /// It should never be possible to Create an Entity without and ID and an Owner.
        /// </summary>
        private Entity()
        {
        }

        public Guid Id { get; protected set; }

        public override bool Equals(object obj)
        {
            var compareTo = obj as Entity<T>;

            if (ReferenceEquals(this, compareTo)) return true;
            if (ReferenceEquals(null, compareTo)) return false;

            return Id.Equals(compareTo.Id);
        }

        public static bool operator ==(Entity<T> a, Entity<T> b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity<T> a, Entity<T> b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return (GetType().GetHashCode() * 907) + Id.GetHashCode();
        }

        public override string ToString()
        {
            return GetType().Name + " [Id=" + Id + "]";
        }
    }
}
