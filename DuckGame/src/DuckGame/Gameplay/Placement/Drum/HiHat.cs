using Microsoft.Xna.Framework;

namespace DuckGame;

public class HiHat : Drum
{
    private Sprite _stand;

    public HiHat(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("drumset/hat");
        Center = new Vector2(graphic.w / 2, graphic.h / 2);
        _stand = new Sprite("drumset/hatStand");
        _stand.Center = new Vector2(_stand.w / 2, 0f);
        _sound = "hatClosed";
        _alternateSound = "hatOpen";
    }

    public override void Draw()
    {
        base.Draw();
        _stand.Depth = base.Depth - 1;
        Graphics.Draw(_stand, base.X, base.Y - 4f);
    }
}
