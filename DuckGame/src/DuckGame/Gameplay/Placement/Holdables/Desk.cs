using System;

namespace DuckGame;

[EditorGroup("Stuff|Props")]
public class Desk : Holdable, IPlatform
{
    public StateBinding _flippedBinding = new StateBinding(nameof(flipped));

    public StateBinding _flipBinding = new StateBinding(nameof(_flip));

    private float damageMultiplier = 1f;

    private SpriteMap _sprite;

    public int flipped;

    public bool landed = true;

    public float _flip;

    private bool firstFrame = true;

    public Desk(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _maxHealth = 15f;
        _hitPoints = 15f;
        _sprite = new SpriteMap("desk", 19, 12);
        graphic = _sprite;
        Center = new Vec2(9f, 6f);
        collisionOffset = new Vec2(-8f, -3f);
        collisionSize = new Vec2(17f, 6f);
        base.Depth = -0.5f;
        _editorName = "Desk";
        thickness = 8f;
        weight = 8f;
        _holdOffset = new Vec2(2f, 2f);
        base.collideSounds.Add("thud");
        physicsMaterial = PhysicsMaterial.Metal;
        editorTooltip = "This is where we get all the important work done.";
        holsterAngle = 90f;
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        _hitPoints = 0f;
        SFX.Play("crateDestroy");
        Level.Remove(this);
        Vec2 flyDir = Vec2.Zero;
        if (type is DTShot)
        {
            flyDir = (type as DTShot).bullet.travelDirNormalized;
        }
        for (int i = 0; i < 6; i++)
        {
            WoodDebris woodDebris = WoodDebris.New(base.X - 8f + Rando.Float(16f), base.Y - 8f + Rando.Float(16f));
            woodDebris.hSpeed = ((Rando.Float(1f) > 0.5f) ? 1f : (-1f)) * Rando.Float(3f) + (float)Math.Sign(flyDir.X) * 0.5f;
            woodDebris.vSpeed = 0f - Rando.Float(1f);
            Level.Add(woodDebris);
        }
        for (int j = 0; j < 5; j++)
        {
            SmallSmoke smallSmoke = SmallSmoke.New(base.X + Rando.Float(-6f, 6f), base.Y + Rando.Float(-6f, 6f));
            smallSmoke.hSpeed += Rando.Float(-0.3f, 0.3f);
            smallSmoke.vSpeed -= Rando.Float(0.1f, 0.2f);
            Level.Add(smallSmoke);
        }
        return true;
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        if (_flip < 0.05f && hitPos.Y > base.top + 4f)
        {
            return false;
        }
        if (_hitPoints <= 0f)
        {
            return base.Hit(bullet, hitPos);
        }
        if (bullet.isLocal && owner == null)
        {
            Thing.Fondle(this, DuckNetwork.localConnection);
        }
        for (int i = 0; (float)i < 1f + damageMultiplier; i++)
        {
            WoodDebris woodDebris = WoodDebris.New(base.X - 8f + Rando.Float(16f), base.Y - 8f + Rando.Float(16f));
            woodDebris.hSpeed = (float)Math.Sign(bullet.travel.X) * Rando.Float(2f) + (float)Math.Sign(bullet.travel.X) * 0.5f;
            woodDebris.vSpeed = 0f - Rando.Float(1f);
            Level.Add(woodDebris);
        }
        SFX.Play("woodHit");
        if (base.isServerForObject && bullet.isLocal)
        {
            _hitPoints -= damageMultiplier;
            damageMultiplier += 2f;
            if (_hitPoints <= 0f)
            {
                Destroy(new DTShot(bullet));
            }
        }
        return base.Hit(bullet, hitPos);
    }

    public override void Update()
    {
        base.Update();
        offDir = 1;
        if (damageMultiplier > 1f)
        {
            damageMultiplier -= 0.2f;
        }
        else
        {
            damageMultiplier = 1f;
        }
        _sprite.frame = (int)Math.Floor((1f - _hitPoints / _maxHealth) * 4f);
        if (_hitPoints <= 0f && !base._destroyed)
        {
            Destroy(new DTImpact(this));
        }
        _flip = MathHelper.Lerp(_flip, (flipped != 0) ? 1.1f : (-0.1f), 0.2f);
        if (_flip > 1f)
        {
            _flip = 1f;
        }
        if (_flip < 0f)
        {
            _flip = 0f;
        }
        if (owner != null && flipped != 0)
        {
            flipped = 0;
        }
        Vec2 prevCol = collisionSize;
        Vec2 prevOff = collisionOffset;
        if (_flip == 0f)
        {
            if (!landed)
            {
                Land();
            }
            collisionOffset = new Vec2(-8f, -6f);
            collisionSize = new Vec2(17f, 11f);
        }
        else if (_flip == 1f)
        {
            if (!landed)
            {
                Land();
            }
            if (flipped > 0)
            {
                collisionOffset = new Vec2(0f, -12f);
                collisionSize = new Vec2(8f, 17f);
            }
            else
            {
                collisionOffset = new Vec2(-10f, -13f);
                collisionSize = new Vec2(8f, 17f);
            }
        }
        else
        {
            landed = false;
            collisionOffset = new Vec2(-2f, 4f);
            collisionSize = new Vec2(4f, 1f);
        }
        if (!firstFrame && (prevOff != collisionOffset || prevCol != collisionSize))
        {
            ReturnItemToWorld(this);
        }
        if (flipped != 0)
        {
            base.CenterX = 9f + 4f * _flip * ((flipped > 0) ? 1f : (-1f));
            base.CenterY = 6f + 4f * _flip;
            Angle = _flip * (1.5f * ((flipped > 0) ? 1f : (-1f)));
        }
        else
        {
            base.CenterX = 9f + 4f * _flip * ((Angle > 0f) ? 1f : (-1f));
            base.CenterY = 6f + 4f * _flip;
            Angle = _flip * (1.5f * ((Angle > 0f) ? 1f : (-1f)));
        }
        firstFrame = false;
    }

    public void Flip(bool left)
    {
        if (owner != null || !base.isServerForObject)
        {
            return;
        }
        SFX.Play("swipe", 0.5f);
        if (base.grounded)
        {
            if (flipped == 0)
            {
                vSpeed -= 1.4f;
            }
            else
            {
                vSpeed -= 1f;
            }
        }
        if (flipped == 0)
        {
            flipped = ((!left) ? 1 : (-1));
        }
        else
        {
            flipped = 0;
        }
    }

    public void Land()
    {
        landed = true;
        if (owner == null)
        {
            SFX.Play("rockHitGround2", 0.7f);
        }
        if (flipped > 0)
        {
            for (int i = 0; i < 2; i++)
            {
                Level.Add(SmallSmoke.New(base.bottomRight.X, base.bottomRight.Y));
            }
        }
        else if (flipped < 0)
        {
            for (int j = 0; j < 2; j++)
            {
                Level.Add(SmallSmoke.New(base.bottomLeft.X, base.bottomLeft.Y));
            }
        }
    }
}
