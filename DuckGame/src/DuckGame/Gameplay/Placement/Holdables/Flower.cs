using System;

namespace DuckGame;

[EditorGroup("Stuff|Props")]
[BaggedProperty("isInDemo", true)]
public class Flower : Holdable
{
    private Sprite _burnt;

    public bool _picked;

    private int framesSinceThrown = 1000;

    public Gun _stuck;

    public float _prevBarrelHeat;

    public Flower(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("flower");
        _burnt = new Sprite("flower_burned");
        Center = new Vec2(8f, 12f);
        collisionOffset = new Vec2(-3f, -12f);
        collisionSize = new Vec2(6f, 14f);
        _holdOffset = new Vec2(-2f, 2f);
        base.Depth = -0.5f;
        weight = 1f;
        flammable = 0.3f;
        base.hugWalls = WallHug.Floor;
        editorTooltip = "It's beautiful.";
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        return false;
    }

    public static void PoofEffect(Vec2 pPosition)
    {
        for (int i = 0; i < 4; i++)
        {
            ConfettiParticle confettiParticle = new ConfettiParticle();
            confettiParticle.Init(pPosition.X + Rando.Float(-4f, 0f), pPosition.Y + Rando.Float(-4f, 6f), new Vec2(Rando.Float(-1f, 0f), Rando.Float(-1f, 1f)));
            confettiParticle._color = new Color(49, 163, 242);
            Level.Add(confettiParticle);
        }
        for (int j = 0; j < 2; j++)
        {
            ConfettiParticle confettiParticle2 = new ConfettiParticle();
            confettiParticle2.Init(pPosition.X + Rando.Float(-4f, 0f), pPosition.Y + Rando.Float(-4f, 6f), new Vec2(Rando.Float(-1f, 0f), Rando.Float(-1f, 1f)));
            confettiParticle2._color = new Color(163, 206, 39);
            Level.Add(confettiParticle2);
        }
    }

    public override void Update()
    {
        if (burnt >= 1f)
        {
            if (graphic != _burnt)
            {
                SFX.Play("flameExplode");
                Level.Add(SmallFire.New(base.X + Rando.Float(-2f, 2f), base.Y + Rando.Float(-2f, 2f), -1f + Rando.Float(2f), -1f + Rando.Float(2f), shortLife: false, null, canMultiply: true, this));
                Level.Add(SmallFire.New(base.X + Rando.Float(-2f, 2f), base.Y + Rando.Float(-2f, 2f), -1f + Rando.Float(2f), -1f + Rando.Float(2f), shortLife: false, null, canMultiply: true, this));
                for (int i = 0; i < 3; i++)
                {
                    Level.Add(SmallSmoke.New(base.X + Rando.Float(-2f, 2f), base.Y + Rando.Float(-2f, 2f)));
                }
            }
            graphic = _burnt;
        }
        if (_stuck != null)
        {
            if (base.held || graphic == _burnt)
            {
                _stuck.plugged = false;
                _stuck = null;
            }
            else
            {
                _stuck.plugged = true;
                if (Network.isActive && _stuck.isServerForObject)
                {
                    _stuck.Fondle(this);
                }
                if (!_stuck.removeFromLevel || !base.isServerForObject)
                {
                    Position = _stuck.Offset(_stuck.barrelOffset + _stuck.barrelInsertOffset + new Vec2(1f, 1f));
                    offDir = _stuck.offDir;
                    base.AngleDegrees = _stuck.AngleDegrees + (float)(90 * offDir);
                    base.Depth = _stuck.Depth - 4;
                    base.velocity = Vec2.Zero;
                    if (_stuck._barrelHeat < _prevBarrelHeat)
                    {
                        _prevBarrelHeat = _stuck._barrelHeat;
                    }
                    if (base.isServerForObject && _stuck._barrelHeat > _prevBarrelHeat + 0.01f)
                    {
                        PoofEffect(Position);
                        if (Network.isActive)
                        {
                            Send.Message(new NMFlowerPoof(Position));
                        }
                        Level.Remove(this);
                    }
                    return;
                }
                if (_stuck is DuelingPistol)
                {
                    vSpeed -= 2f;
                }
                _stuck = null;
            }
        }
        if (Math.Abs(hSpeed) > 0.2f || (!_picked && owner != null))
        {
            _picked = true;
        }
        if (_picked)
        {
            if (owner != null)
            {
                framesSinceThrown = 0;
                Center = new Vec2(8f, 12f);
                collisionOffset = new Vec2(-3f, -12f);
                collisionSize = new Vec2(6f, 14f);
                base.AngleDegrees = 0f;
                graphic.flipH = offDir < 0;
            }
            else
            {
                base.Depth = -0.5f;
                if (framesSinceThrown < 15)
                {
                    Gun g = Level.current.NearestThing<Gun>(Position);
                    if (g != null && (g.barrelPosition - Position).Length() < 4f && g.held && g.wideBarrel && ((g.offDir > 0 && hSpeed < 0f) || (g.offDir < 0 && hSpeed > 0f)))
                    {
                        _stuck = g;
                        _prevBarrelHeat = _stuck._barrelHeat;
                        SFX.PlaySynchronized("pipeOut", 1f, 0.2f);
                    }
                }
                framesSinceThrown++;
                Center = new Vec2(8f, 8f);
                collisionOffset = new Vec2(-7f, -5f);
                collisionSize = new Vec2(14f, 6f);
                base.AngleDegrees = 90f;
                graphic.flipH = true;
                base.Depth = 0.4f;
            }
        }
        base.Update();
    }

    public override void OnPressAction()
    {
        if (graphic == _burnt)
        {
            return;
        }
        if (Network.isActive)
        {
            if (base.isServerForObject)
            {
                NetSoundEffect.Play("flowerHappyQuack");
            }
        }
        else
        {
            SFX.Play("happyQuack01", 1f, Rando.Float(-0.1f, 0.1f));
        }
        if (base.duck != null)
        {
            base.duck.quack = 20;
            return;
        }
        Level.Remove(this);
        SFX.Play("flameExplode");
        for (int i = 0; i < 8; i++)
        {
            Level.Add(SmallFire.New(base.X + Rando.Float(-8f, 8f), base.Y + Rando.Float(-8f, 8f), -3f + Rando.Float(6f), -3f + Rando.Float(6f), shortLife: false, null, canMultiply: true, this));
        }
    }

    public override void OnReleaseAction()
    {
        if (base.duck != null && graphic != _burnt)
        {
            base.duck.quack = 0;
        }
    }

    public override void Draw()
    {
        if (_stuck != null)
        {
            Position = _stuck.Offset(_stuck.barrelOffset + _stuck.barrelInsertOffset + new Vec2(1f, 1f));
            offDir = _stuck.offDir;
            base.AngleDegrees = _stuck.AngleDegrees + (float)(90 * offDir);
            base.Depth = _stuck.Depth - 4;
            base.velocity = Vec2.Zero;
        }
        base.Draw();
    }
}
