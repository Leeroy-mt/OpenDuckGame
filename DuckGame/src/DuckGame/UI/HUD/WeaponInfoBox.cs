using Microsoft.Xna.Framework;

namespace DuckGame;

public class WeaponInfoBox : Thing
{
    private BitmapFont _font;

    private Sprite _image;

    public WeaponInfoBox(float xpos, float ypos, Gun gun)
        : base(xpos, ypos)
    {
        _font = new BitmapFont("duckFont", 8);
        _font.Scale = new Vector2(0.5f, 0.5f);
        base.layer = Layer.HUD;
        _image = gun.GetEditorImage(0, 0, transparentBack: true);
        _image.Depth = 1f;
        _image.Scale = new Vector2(4f, 4f);
    }

    public override void Update()
    {
    }

    public override void Draw()
    {
        Graphics.DrawRect(Position, Position + new Vector2(100f, 100f), new Color(100, 100, 100), 0.8f);
        Graphics.Draw(_image, Position.X, Position.Y);
    }
}
