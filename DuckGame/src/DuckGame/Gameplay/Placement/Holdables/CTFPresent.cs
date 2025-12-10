namespace DuckGame;

public class CTFPresent : Present
{
    private SpriteMap _sprite;

    public CTFPresent(float xpos, float ypos, bool team)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("ctf/present", 18, 17);
        _sprite.frame = ((!team) ? 1 : 0);
        graphic = _sprite;
        center = new Vec2(9f, 8f);
        collisionOffset = new Vec2(-9f, -6f);
        collisionSize = new Vec2(18f, 14f);
        weight = 7f;
        flammable = 0.8f;
    }

    public override void OnPressAction()
    {
        if (base.duck != null && base.duck.ctfTeamIndex != _sprite.frame)
        {
            base.OnPressAction();
        }
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        if (type is DTIncinerate)
        {
            Level.Remove(this);
            Level.Add(SmallSmoke.New(base.x, base.y));
        }
        return false;
    }
}
