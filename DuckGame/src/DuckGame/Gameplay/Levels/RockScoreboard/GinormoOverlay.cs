using Microsoft.Xna.Framework.Graphics;
using System;

namespace DuckGame;

public class GinormoOverlay : Thing
{
    private Sprite _targetSprite;

    private Effect _screenMaterial;

#if NO_TEX2D
    Texture2D _overlaySprite;
#else
    private Tex2D _overlaySprite;
#endif

    private bool _smallMode;

    public GinormoOverlay(float xpos, float ypos, bool smallMode)
        : base(xpos, ypos)
    {
        base.Depth = 0.9f;
        graphic = new Sprite("rockThrow/boardOverlay");
        _smallMode = smallMode;
    }

    public override void Initialize()
    {
#if NO_TEX2D
        _overlaySprite = Content.Load<Texture2D>("rockThrow/boardOverlayLarge");
#else
        _overlaySprite = Content.Load<Tex2D>("rockThrow/boardOverlayLarge");
#endif
        _targetSprite = new Sprite(GinormoBoard.boardLayer.target);
        _screenMaterial = Content.Load<Effect>("Shaders/lcdNoBlur");
        _screenMaterial.Parameters["screenWidth"]?.SetValue(GinormoScreen.GetSize(_smallMode).X);
        _screenMaterial.Parameters["screenHeight"]?.SetValue(GinormoScreen.GetSize(_smallMode).Y);
        base.Initialize();
    }

    public override void Draw()
    {
        if (RockScoreboard.drawingNormalTarget || NetworkDebugger.enabled)
        {
            var obj = Graphics.material;
            Graphics.material = _screenMaterial;
            Graphics.device.Textures[1] = (Texture2D)_overlaySprite;
            Graphics.device.SamplerStates[1] = SamplerState.LinearClamp;
            _targetSprite.Depth = 0.9f;
            Graphics.Draw(_targetSprite, base.X - 92f, base.Y - 33f);
            Graphics.material = obj;
        }
    }
}
