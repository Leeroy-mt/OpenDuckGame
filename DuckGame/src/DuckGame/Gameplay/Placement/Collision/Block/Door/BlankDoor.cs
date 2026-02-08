using Microsoft.Xna.Framework;

namespace DuckGame;

public class BlankDoor : Thing
{
    private BitmapFont _fontSmall;

    public BlankDoor(float pX, float pY)
        : base(pX, pY, new Sprite("blank_door", Vector2.Zero))
    {
        _fontSmall = new BitmapFont("smallBiosFont", 7, 6);
    }

    public override void Draw()
    {
        _fontSmall.DrawOutline("DUCK", new Vector2(base.X + 22f, base.Y + 40f), Color.White, Colors.BlueGray, base.Depth + 10);
        _fontSmall.DrawOutline("GAME", new Vector2(base.X + 90f, base.Y + 40f), Color.White, Colors.BlueGray, base.Depth + 10);
        base.Draw();
    }
}
