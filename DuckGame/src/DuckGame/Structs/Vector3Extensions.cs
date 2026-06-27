using Microsoft.Xna.Framework;

namespace DuckGame;

/// <summary>
/// Extends <see cref="Vector3"/>.
/// </summary>
public static class Vector3Extensions
{
    extension(Vector3 vector3)
    {
        public Color ToColor()
        {
            return new(vector3);
        }
    }
}