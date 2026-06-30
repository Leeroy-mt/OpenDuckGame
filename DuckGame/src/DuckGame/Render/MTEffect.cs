using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MTEffect
{
    #region Private Fields

    string _effectName;
    Effect _base;

    #endregion

    #region Public Properties

    public short EffectIndex { get; set; }

    public string effectName => _effectName;

    public Effect effect => _base;

    #endregion

    public MTEffect(Effect tex, string cureffectName, short cureffectIndex = 0)
    {
        _base = tex;
        _effectName = cureffectName;
        EffectIndex = cureffectIndex;
    }

    public static implicit operator Effect(MTEffect tex)
    {
        return tex?._base;
    }

    public static implicit operator MTEffect(Effect tex)
    {
        return tex is null ? null : Content.GetMTEffect(tex);
    }
}
