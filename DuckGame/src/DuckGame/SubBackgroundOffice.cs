namespace DuckGame;

public class SubBackgroundOffice : SubBackgroundTile
{
    public SubBackgroundOffice(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("officeSubBackground", 32, 32, calculateTransparency: true);
        _opacityFromGraphic = true;
        Center = new Vec2(24, 16);
        collisionSize = new Vec2(32);
        collisionOffset = new Vec2(-16);
    }
}
