using Microsoft.Xna.Framework;

namespace DuckGame;

public class DoorOffHinges : PhysicsObject
{
    public StateBinding _throwSpinBinding = new StateBinding(nameof(_throwSpin));

    public StateBinding _secondaryBinding = new StateBinding(nameof(_secondaryFrame));

    public bool _secondaryFrame;

    public bool _wasSecondaryFrame;

    public float _throwSpin;

    private bool sounded;

    public DoorOffHinges(float xpos, float ypos, bool secondaryFrame)
        : base(xpos, ypos)
    {
        _secondaryFrame = secondaryFrame;
        _collisionSize = new Vector2(8f, 8f);
        _collisionOffset = new Vector2(-4f, -6f);
        Center = new Vector2(16f, 16f);
        base.collideSounds.Add("rockHitGround");
        weight = 2f;
    }

    public override void Initialize()
    {
        RefreshSprite();
        base.Initialize();
    }

    public void MakeEffects()
    {
        if (!sounded)
        {
            Level.Add(SmallSmoke.New(base.X, base.Y + 2f));
            Level.Add(SmallSmoke.New(base.X, base.Y - 16f));
            SFX.Play("doorBreak");
            for (int i = 0; i < 8; i++)
            {
                WoodDebris woodDebris = WoodDebris.New(base.X - 8f + Rando.Float(16f), base.Y - 8f + Rando.Float(16f));
                woodDebris.hSpeed = ((Rando.Float(1f) > 0.5f) ? 1f : (-1f)) * Rando.Float(3f);
                woodDebris.vSpeed = 0f - Rando.Float(1f);
                Level.Add(woodDebris);
            }
            sounded = true;
        }
    }

    private void RefreshSprite()
    {
        graphic = new SpriteMap(_secondaryFrame ? "flimsyDoorDamaged" : "doorFucked", 32, 32);
        _wasSecondaryFrame = _secondaryFrame;
    }

    public override void Update()
    {
        if (_secondaryFrame != _wasSecondaryFrame)
        {
            RefreshSprite();
        }
        if (Network.isActive && !sounded && visible)
        {
            MakeEffects();
        }
        base.AngleDegrees = _throwSpin;
        Center = new Vector2(16f, 16f);
        _throwSpin %= 360f;
        if (offDir > 0)
        {
            _throwSpin = Lerp.Float(_throwSpin, 90f, 12f);
        }
        else
        {
            _throwSpin = Lerp.Float(_throwSpin, -90f, 12f);
        }
        base.Update();
    }
}
