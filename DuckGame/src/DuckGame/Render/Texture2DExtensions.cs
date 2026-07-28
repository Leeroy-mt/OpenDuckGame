using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public static class Texture2DExtensions
{
    extension(Texture2D texture2D)
    {
        public short GetTextureIndex()
        {
            return (short)Content.textureList.IndexOf(texture2D);
        }

        public Color[] GetData()
        {
            var data = new Color[texture2D.Width * texture2D.Height];
            texture2D.GetData(data);
            return data;
        }

        public static Texture2D GetTex2DLike(Texture2D tex, string name)
        {
            tex.Name = name;
            return tex;
        }

        public static Texture2D GetTex2DLike(int width, int height)
        {
            Texture2D tex = new(Graphics.device, width, height, mipMap: false, SurfaceFormat.Color)
            {
                Name = "__internal"
            };
            Content.AssignTextureIndex(tex);
            return tex;
        }
    }
}