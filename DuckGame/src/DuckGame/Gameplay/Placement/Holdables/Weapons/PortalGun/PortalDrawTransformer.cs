using Microsoft.Xna.Framework;

namespace DuckGame;

public class PortalDrawTransformer : Thing
{
    private Portal _portal;

    private Thing _thing;

    public new Portal portal => _portal;

    public Thing thing => _thing;

    public PortalDrawTransformer(Thing t, Portal p)
    {
        _portal = p;
        _thing = t;
    }

    public override void Draw()
    {
        Vector2 pos = _thing.Position;
        foreach (PortalDoor d in _portal.GetDoors())
        {
            if (Graphics.currentLayer == d.layer)
            {
                if (d.isLeft && _thing.X > d.center.X + 32f)
                {
                    _thing.Position += d.center - _portal.GetOtherDoor(d).center;
                }
                else if (!d.isLeft && _thing.X < d.center.X - 32f)
                {
                    _thing.Position += _portal.GetOtherDoor(d).center - d.center;
                }
                _thing.DoDraw();
                _thing.Position = pos;
            }
        }
    }
}
