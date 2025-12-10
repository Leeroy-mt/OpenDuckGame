using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

/// <summary>
/// Represents a list of mod-managed content of T's
/// </summary>
/// <typeparam name="T">The base type of content to store</typeparam>
public class ManagedContentList<T>
{
    private readonly HashSet<Type> _types = new HashSet<Type>();

    private readonly Dictionary<Type, Type> _redirections = new Dictionary<Type, Type>();

    internal IEnumerable<Type> SortedTypes => _types.OrderBy((Type t) => t.FullName);

    /// <summary>
    /// Gets the registered types.
    /// </summary>
    /// <value>
    /// The types registered.
    /// </value>
    public IEnumerable<Type> Types => _types;

    internal void Add(Type type)
    {
        _types.Add(type);
    }

    /// <summary>
    /// Removes a type from the type pool.
    /// </summary>
    /// <param name="type">The type.</param>
    public void Remove(Type type)
    {
        if (_types.Contains(type) && type.GetCustomAttributes(typeof(LockedContentAttribute), inherit: true).Length == 0)
        {
            _types.Remove(type);
        }
    }

    /// <summary>
    /// Removes a generic type from the type pool.
    /// </summary>
    /// <typeparam name="E">The type to remove</typeparam>
    public void Remove<E>() where E : T
    {
        Remove(typeof(E));
    }

    /// <summary>
    /// Redirects the a type to another type. Attempts to create an Old
    /// will result in a New being created instead.
    /// </summary>
    /// <param name="oldType">Old type, being redirected.</param>
    /// <param name="newType">The new type to redirect to.</param>
    public void Redirect(Type oldType, Type newType)
    {
        if (oldType.GetCustomAttributes(typeof(LockedContentAttribute), inherit: true).Length == 0)
        {
            _redirections[oldType] = newType;
        }
    }

    /// <summary>
    /// Redirects the generic Old type to the New type. Attempts to create an Old
    /// will result in a New being created instead.
    /// </summary>
    /// <typeparam name="Old">Old type, being redirected.</typeparam>
    /// <typeparam name="New">The new type to redirect to.</typeparam>
    public void Redirect<Old, New>() where Old : T where New : Old
    {
        Redirect(typeof(Old), typeof(New));
    }
}
