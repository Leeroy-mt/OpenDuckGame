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
        Vec2 pos = _thing.position;
        foreach (PortalDoor d in _portal.GetDoors())
        {
            if (Graphics.currentLayer == d.layer)
            {
                if (d.isLeft && _thing.x > d.center.x + 32f)
                {
                    _thing.position += d.center - _portal.GetOtherDoor(d).center;
                }
                else if (!d.isLeft && _thing.x < d.center.x - 32f)
                {
                    _thing.position += _portal.GetOtherDoor(d).center - d.center;
                }
                _thing.DoDraw();
                _thing.position = pos;
            }
        }
    }
}
