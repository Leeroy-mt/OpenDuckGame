using System;

namespace DuckGame;

/// <summary>
/// Defines an object which contain, or may contain, an object of a specific type
/// </summary>
public interface IContainAThing
{
	Type contains { get; }
}
