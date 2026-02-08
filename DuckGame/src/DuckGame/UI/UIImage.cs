using Microsoft.Xna.Framework;

namespace DuckGame;

public class UIImage : UIComponent
{
    public Sprite _image;

    private float yOffset;

    public UIImage(string imageVal, UIAlign al = UIAlign.Left)
        : base(0f, 0f, -1f, -1f)
    {
        _image = new Sprite(imageVal);
        _collisionSize = new Vector2(_image.w, _image.h);
        _image.CenterOrigin();
        base.align = al;
    }

    public UIImage(Sprite imageVal, UIAlign al = UIAlign.Left)
        : base(0f, 0f, -1f, -1f)
    {
        _image = imageVal;
        _collisionSize = new Vector2(_image.w, _image.h);
        _image.CenterOrigin();
        base.align = al;
    }

    public UIImage(Sprite imageVal, UIAlign al, float s = 1f, float yOff = 0f)
        : base(0f, 0f, -1f, -1f)
    {
        _image = imageVal;
        _collisionSize = new Vector2((float)_image.w * s, (float)_image.h * s);
        _image.CenterOrigin();
        base.Scale = new Vector2(s);
        base.align = al;
        yOffset = yOff;
    }

    public override void Draw()
    {
        _image.Scale = base.Scale;
        _image.Alpha = base.Alpha;
        _image.Depth = base.Depth;
        Graphics.Draw(_image, base.X, base.Y + yOffset);
        base.Draw();
    }
}
