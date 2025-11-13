using System;

namespace DuckGame;

/// <summary>
/// Declares which group this Thing is in the editor
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class NetworkAction : Attribute
{
}
