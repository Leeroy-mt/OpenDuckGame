using Microsoft.Xna.Framework;

namespace DuckGame;

public static class Vec3
{
    extension(Vector3 vector3)
    {
        public Color ToColor()
        {
            return new(vector3);
        }
    }
}