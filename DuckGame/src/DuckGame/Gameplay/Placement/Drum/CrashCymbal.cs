namespace DuckGame;

public class CrashCymbal : Drum
{
    private Sprite _stand;

    public CrashCymbal(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new("drumset/crashCymbal");
        center = new(graphic.w / 2, graphic.h / 2);
        _stand = new("drumset/crashStand");
        _stand.center = new(_stand.w / 2, 0);
        _sound = "crash";
    }

    public override void Draw()
    {
        base.Draw();
        _stand.depth = depth - 1;
        Graphics.Draw(_stand, x, y + 1);
    }
}
