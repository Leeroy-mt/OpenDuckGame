using Microsoft.Xna.Framework;

namespace DuckGame;

public class TapedSword : Sword
{
    public TapedSword(float xval, float yval)
        : base(xval, yval)
    {
        graphic = new Sprite("tapedSword");
        Center = new Vector2(4f, 44f);
        collisionOffset = new Vector2(-2f, -16f);
        collisionSize = new Vector2(4f, 18f);
        centerHeld = new Vector2(4f, 44f);
        centerUnheld = new Vector2(4f, 22f);
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
