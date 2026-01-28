using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class GinormoOverlay : Thing
{
    private Sprite _targetSprite;

    private Material _screenMaterial;

    private Tex2D _overlaySprite;

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
        _overlaySprite = Content.Load<Tex2D>("rockThrow/boardOverlayLarge");
        _targetSprite = new Sprite(GinormoBoard.boardLayer.target);
        _screenMaterial = new Material("Shaders/lcdNoBlur");
        _screenMaterial.SetValue("screenWidth", GinormoScreen.GetSize(_smallMode).X);
        _screenMaterial.SetValue("screenHeight", GinormoScreen.GetSize(_smallMode).Y);
        base.Initialize();
    }

    public override void Draw()
    {
        if (RockScoreboard.drawingNormalTarget || NetworkDebugger.enabled)
        {
            Material obj = Graphics.material;
            Graphics.material = _screenMaterial;
            Graphics.device.Textures[1] = (Texture2D)_overlaySprite;
            Graphics.device.SamplerStates[1] = SamplerState.LinearClamp;
            _targetSprite.Depth = 0.9f;
            Graphics.Draw(_targetSprite, base.X - 92f, base.Y - 33f);
            Graphics.material = obj;
        }
    }
}
