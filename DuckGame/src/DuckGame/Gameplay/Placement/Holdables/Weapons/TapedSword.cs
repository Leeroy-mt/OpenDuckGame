namespace DuckGame;

public class TapedSword : Sword
{
    public TapedSword(float xval, float yval)
        : base(xval, yval)
    {
        graphic = new Sprite("tapedSword");
        Center = new Vec2(4f, 44f);
        collisionOffset = new Vec2(-2f, -16f);
        collisionSize = new Vec2(4f, 18f);
        centerHeld = new Vec2(4f, 44f);
        centerUnheld = new Vec2(4f, 22f);
    }

    public override bool CanTapeTo(Thing pThing)
    {
        if (pThing is Sword || pThing is EnergyScimitar)
        {
            return false;
        }
        return true;
    }
}
