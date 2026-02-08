using Microsoft.Xna.Framework;

namespace DuckGame;

public class Feather : Thing
{
    private static int kMaxObjects = 64;

    private static Feather[] _objects = new Feather[kMaxObjects];

    private static int _lastActiveObject = 0;

    private SpriteMap _sprite;

    private bool _rested;

    public static Feather New(float xpos, float ypos, DuckPersona who)
    {
        if (who == null)
        {
            who = Persona.Duck1;
        }
        Feather obj = null;
        if (NetworkDebugger.enabled)
        {
            obj = new Feather();
        }
        else if (_objects[_lastActiveObject] == null)
        {
            obj = new Feather();
            _objects[_lastActiveObject] = obj;
        }
        else
        {
            obj = _objects[_lastActiveObject];
        }
        Level.Remove(obj);
        _lastActiveObject = (_lastActiveObject + 1) % kMaxObjects;
        obj.Init(xpos, ypos, who);
        obj.ResetProperties();
        obj._sprite.globalIndex = Thing.GetGlobalIndex();
        obj.globalIndex = Thing.GetGlobalIndex();
        return obj;
    }

    private Feather()
    {
        _sprite = new SpriteMap("feather", 12, 4)
        {
            speed = 0.3f
        };
        _sprite.AddAnimation("feather", 1, true, 0, 1, 2, 3);
        graphic = _sprite;
        Center = new Vector2(6, 1);
    }

    private void Init(float xpos, float ypos, DuckPersona who)
    {
        X = xpos;
        Y = ypos;
        Alpha = 1f;
        hSpeed = -3 + Rando.Float(6);
        vSpeed = -1 + (-1 + Rando.Float(2));
        _sprite = who.featherSprite.CloneMap();
        _sprite.SetAnimation("feather");
        _sprite.frame = Rando.Int(3);
        _sprite.flipH = Rando.Double() > 0.5;
        graphic = _sprite;
        _rested = false;
    }

    public override void Update()
    {
        if (_rested)
            return;

        if (hSpeed > 0)
            hSpeed -= 0.1f;
        if (hSpeed < 0)
            hSpeed += 0.1f;
        if ((double)hSpeed < 0.1 && hSpeed > -0.1)
            hSpeed = 0;
        if (vSpeed < 1)
            vSpeed += 0.06f;
        if (vSpeed < 0)
        {
            _sprite.speed = 0;
            if (Level.CheckPoint<Block>(X, Y - 7) != null)
                vSpeed = 0;
        }
        else if (Level.CheckPoint<IPlatform>(X, Y + 3) is Thing col)
        {
            vSpeed = 0;
            _sprite.speed = 0;
            if (col is Block)
                _rested = true;
        }
        else
            _sprite.speed = 0.3f;
        X += hSpeed;
        Y += vSpeed;
    }
}
