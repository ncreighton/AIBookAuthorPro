// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

namespace AIBookAuthorPro.Core.Common;

/// <summary>
/// Base class for all domain entities with a unique identifier.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    /// <summary>
    /// Gets or sets the unique identifier for this entity.
    /// </summary>
    public Guid Id { get; protected set; }
    
    /// <summary>
    /// Gets or sets the date and time when this entity was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }
    
    /// <summary>
    /// Gets or sets the date and time when this entity was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the Entity class with a new unique identifier.
    /// </summary>
    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the Entity class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for this entity.</param>
    protected Entity(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the ModifiedAt timestamp to the current UTC time.
    /// </summary>
    protected void MarkAsModified()
    {
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the specified entity is equal to the current entity.
    /// </summary>
    /// <param name="other">The entity to compare with the current entity.</param>
    /// <returns>True if the entities have the same Id; otherwise, false.</returns>
    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>True if the object is an entity with the same Id; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as Entity);

    /// <summary>
    /// Returns a hash code for this entity based on its Id.
    /// </summary>
    /// <returns>A hash code for the current entity.</returns>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Determines whether two entities are equal.
    /// </summary>
    public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);

    /// <summary>
    /// Determines whether two entities are not equal.
    /// </summary>
    public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);
}
