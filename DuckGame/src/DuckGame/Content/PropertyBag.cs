using System;
using System.Collections.Generic;

namespace DuckGame;

/// <summary>
/// Implementation of property bag
/// </summary>
public class PropertyBag : IPropertyBag, IReadOnlyPropertyBag
{
    private Dictionary<string, object> _dictionary = new Dictionary<string, object>();

    /// <summary>
    /// An enumerator to iterate over property keys.
    /// </summary>
    public IEnumerable<string> Properties => _dictionary.Keys;

    internal PropertyBag()
    {
    }

    /// <summary>
    /// Check if a property is in this bag.
    /// </summary>
    /// <param name="property">The property key.</param>
    /// <returns>
    /// true if the property is in this bag; false if not
    /// </returns>
    public bool Contains(string property)
    {
        return _dictionary.ContainsKey(property);
    }

    /// <summary>
    /// Get an untyped property value from the bag.
    /// </summary>
    /// <param name="property">The property name.</param>
    /// <returns>
    /// The object, if it is in the bag
    /// </returns>
    /// <exception cref="T:DuckGame.PropertyNotFoundException">The property key is not in this property bag.</exception>
    public object Get(string property)
    {
        if (!_dictionary.TryGetValue(property, out var value))
        {
            throw new PropertyNotFoundException("key " + property + " not found in bag");
        }
        return value;
    }

    /// <summary>
    /// Get a property value from the bag.
    /// </summary>
    /// <typeparam name="T">The type to unbox to.</typeparam>
    /// <param name="property">The property name.</param>
    /// <returns>
    /// The object casted to T if it is in the bag and can be converted to the requested type
    /// </returns>
    /// <exception cref="T:DuckGame.PropertyNotFoundException">The property key is not in this property bag.</exception>
    public T Get<T>(string property)
    {
        return (T)Get(property);
    }

    /// <summary>
    /// Check if a property in this bag is of, or assignable to, a certain type.
    /// </summary>
    /// <typeparam name="T">The type to check for.</typeparam>
    /// <param name="property">The property key.</param>
    /// <returns>
    /// true if the property is this type or is assignable to this type; false if not; null if the key does not exist or if the property value is null
    /// </returns>
    public bool? IsOfType<T>(string property)
    {
        if (!_dictionary.TryGetValue(property, out var value) || value == null)
        {
            return null;
        }
        return value.GetType().IsAssignableFrom(typeof(T));
    }

    /// <summary>
    /// Get the type of a property contained in the bag.
    /// </summary>
    /// <param name="property">The property key.</param>
    /// <returns>
    /// The type of this property.
    /// </returns>
    public Type TypeOf(string property)
    {
        if (!_dictionary.TryGetValue(property, out var value) || value == null)
        {
            return null;
        }
        return value.GetType();
    }

    /// <summary>
    /// Set a property's value in the bag
    /// </summary>
    /// <typeparam name="T">The type of value to set.</typeparam>
    /// <param name="property">The property key</param>
    /// <param name="value">The value</param>
    public void Set<T>(string property, T value)
    {
        _dictionary[property] = value;
    }

    /// <summary>
    /// Remove a property value from the bag.
    /// </summary>
    /// <param name="property">The value to remove.</param>
    public void Remove(string property)
    {
        _dictionary.Remove(property);
    }

    /// <summary>
    /// Set multiple property values in the bag at once.
    /// </summary>
    /// <param name="properties">Enumerable set of properties</param>
    public void Set(IDictionary<string, object> properties)
    {
        foreach (KeyValuePair<string, object> prop in properties)
        {
            Set(prop.Key, prop.Value);
        }
    }

    /// <summary>
    /// Tries to get a property value from a bag; does not throw, but returns null if the property is not there.
    /// </summary>
    /// <typeparam name="T">The type to unbox to. Must be a value type.</typeparam>
    /// <param name="property">The property name.</param>
    /// <returns>null if it does not exist, otherwise the property value</returns>
    public T? TryGet<T>(string property) where T : struct
    {
        if (!_dictionary.TryGetValue(property, out var v))
        {
            return null;
        }
        return (T)v;
    }

    /// <summary>
    /// Tries to get a property value from a bag; does not throw.
    /// </summary>
    /// <typeparam name="T">The type to unbox to.</typeparam>
    /// <param name="property">The property name.</param>
    /// <param name="value">The output value.</param>
    /// <returns>true if it was in the property bag</returns>
    public bool TryGet<T>(string property, out T value)
    {
        if (!_dictionary.TryGetValue(property, out var v))
        {
            value = default(T);
            return false;
        }
        value = (T)v;
        return true;
    }

    /// <summary>
    /// Get a property value from the bag. Does not throw, returns defaultValue if the key does not exist.
    /// </summary>
    /// <typeparam name="T">The type to unbox to.</typeparam>
    /// <param name="property">The property name.</param>
    /// <param name="defaultValue">The default value to use if the key is not in the bag.</param>
    /// <returns>The object casted to T if it is in the bag and can be converted to the requested type</returns>
    public T GetOrDefault<T>(string property, T defaultValue)
    {
        if (!_dictionary.TryGetValue(property, out var v))
        {
            return defaultValue;
        }
        return (T)v;
    }

    public bool GetOrDefault(string property, bool defaultValue)
    {
        if (!_dictionary.TryGetValue(property, out var v))
        {
            return defaultValue;
        }
        return (bool)v;
    }
}
