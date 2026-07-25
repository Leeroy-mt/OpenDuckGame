using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public static class EffectExtensions
{
    extension(Effect effect)
    {
        public short GetEffectIndex()
        {
            return (short)Content.effectList.IndexOf(effect);
        }
    }
}