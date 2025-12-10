using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

/// <summary>
/// A class for retrieving property bags associated with Types.
/// </summary>
public static class ContentProperties
{
    public class EmptyBag : IReadOnlyPropertyBag
    {
        public IEnumerable<string> Properties => Enumerable.Empty<string>();

        public bool Contains(string property)
        {
            return false;
        }

        public object Get(string property)
        {
            throw new PropertyNotFoundException();
        }

        public T Get<T>(string property)
        {
            throw new PropertyNotFoundException();
        }

        public T GetOrDefault<T>(string property, T defaultValue)
        {
            return defaultValue;
        }

        public bool GetOrDefault(string property, bool defaultValue)
        {
            return defaultValue;
        }

        public bool? IsOfType<T>(string property)
        {
            return null;
        }

        public T? TryGet<T>(string property) where T : struct
        {
            return null;
        }

        public bool TryGet<T>(string property, out T value)
        {
            value = default(T);
            return false;
        }

        public Type TypeOf(string property)
        {
            return null;
        }
    }

    internal static bool EditingAllowed = true;

    private static readonly Dictionary<Type, PropertyBag> _propertyBags = new Dictionary<Type, PropertyBag>();

    private static readonly EmptyBag _emptyBag = new EmptyBag();

    /// <summary>
    /// Initializes the bag of a single type.
    /// </summary>
    /// <param name="type">The type.</param>
    internal static void InitializeBag(Type type)
    {
        PropertyBag bag = new PropertyBag();
        foreach (BaggedPropertyAttribute prop in DG.Reverse((from attrib in type.GetCustomAttributes(typeof(BaggedPropertyAttribute), inherit: true)
                                                             select (BaggedPropertyAttribute)attrib).ToList().ToArray()))
        {
            bag.Set(prop.Property, prop.Value);
        }
        _propertyBags[type] = bag;
    }

    /// <summary>
    /// Initializes the bags of multiple types.
    /// </summary>
    /// <param name="types">The types.</param>
    internal static void InitializeBags(IEnumerable<Type> types)
    {
        foreach (Type type in types)
        {
            InitializeBag(type);
        }
    }

    /// <summary>
    /// Gets a read-only property bag associated with the type.
    /// </summary>
    /// <param name="t">The type to get the bag from.</param>
    /// <returns>The property bag</returns>
    public static IReadOnlyPropertyBag GetBag(Type t)
    {
        if (_propertyBags.TryGetValue(t, out var bag))
        {
            return bag;
        }
        return _emptyBag;
    }

    /// <summary>
    /// Gets a read-only property bag associated with the type.
    /// </summary>
    /// <typeparam name="T">The type to get the bag from</typeparam>
    /// <returns>The property bag</returns>
    public static IReadOnlyPropertyBag GetBag<T>()
    {
        return GetBag(typeof(T));
    }
}
