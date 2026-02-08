using Microsoft.Xna.Framework;

namespace DuckGame;

public class MediumTom : Drum
{
    private Sprite _stand;

    public MediumTom(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("drumset/mediumTom");
        Center = new Vector2(graphic.w / 2, graphic.h / 2);
        _stand = new Sprite("drumset/highTomStand");
        _stand.Center = new Vector2(_stand.w / 2, 0f);
        _sound = "medTom";
    }

    public override void Draw()
    {
        base.Draw();
        _stand.Depth = base.Depth - 1;
        Graphics.Draw(_stand, base.X + 7f, base.Y);
    }
}
