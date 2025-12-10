using System;

namespace DuckGame;

/// <summary>
/// Declares a magic number to be written for identifying a BinaryClassChunk
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class MagicNumberAttribute : Attribute
{
    public readonly long magicNumber;

    public MagicNumberAttribute(long number)
    {
        magicNumber = number;
    }
}
