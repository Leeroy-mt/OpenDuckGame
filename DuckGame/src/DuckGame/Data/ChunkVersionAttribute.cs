using System;

namespace DuckGame;

/// <summary>
/// Declares a version number to be written for identifying the version of a BinaryClassChunk
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class ChunkVersionAttribute : Attribute
{
    public readonly ushort version;

    public ChunkVersionAttribute(ushort version)
    {
        this.version = version;
    }
}
