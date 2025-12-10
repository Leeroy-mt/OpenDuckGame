using System;
using System.Collections.Generic;

namespace DuckGame;

/// <summary>
/// Defines an object which contains, or may contain, these type of objects.
/// </summary>
public interface IContainThings
{
    IEnumerable<Type> contains { get; }
}
