namespace DuckGame;

public class WoodDebris : PhysicsParticle
{
    private static int kMaxObjects = 64;

    private static WoodDebris[] _objects = new WoodDebris[kMaxObjects];

    private static int _lastActiveObject = 0;

    private SpriteMap _sprite;

    public static WoodDebris New(float xpos, float ypos)
    {
        WoodDebris obj = null;
        if (_objects[_lastActiveObject] == null)
        {
            obj = new WoodDebris();
            _objects[_lastActiveObject] = obj;
        }
        else
        {
            obj = _objects[_lastActiveObject];
        }
        _lastActiveObject = (_lastActiveObject + 1) % kMaxObjects;
        obj.ResetProperties();
        obj.Init(xpos, ypos);
        obj._sprite.globalIndex = Thing.GetGlobalIndex();
        obj.globalIndex = Thing.GetGlobalIndex();
        return obj;
    }

    public WoodDebris()
        : base(0f, 0f)
    {
        _sprite = new SpriteMap("woodDebris", 8, 8);
        graphic = _sprite;
        center = new Vec2(4f, 4f);
    }

    private void Init(float xpos, float ypos)
    {
        position.x = xpos;
        position.y = ypos;
        hSpeed = -4f - Rando.Float(3f);
        vSpeed = 0f - (Rando.Float(1.5f) + 1f);
        _sprite.frame = Rando.Int(4);
        _bounceEfficiency = 0.3f;
    }

    public override void Update()
    {
        base.Update();
    }
}
