using System.Collections.Generic;

namespace DuckGame;

public class CookedDuck : Holdable, IPlatform
{
    private List<SpriteMap> _flavourLines = new List<SpriteMap>();

    private int _timeHot = 3600;

    private float _hotAlpha = 1f;

    public override bool visible
    {
        get
        {
            return base.visible;
        }
        set
        {
            base.visible = value;
        }
    }

    public CookedDuck(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("cookedDuck");
        center = new Vec2(8f, 8f);
        collisionOffset = new Vec2(-6f, -4f);
        collisionSize = new Vec2(12f, 11f);
        base.depth = -0.5f;
        thickness = 0.5f;
        weight = 5f;
        base.collideSounds.Add("rockHitGround2", ImpactedFrom.Bottom);
        base.collideSounds.Add("smallSplatLouder");
        for (int i = 0; i < 3; i++)
        {
            SpriteMap m = new SpriteMap("barrelSmoke", 8, 8);
            m.AddAnimation("idle", 0.12f, true, 3, 4, 5, 6, 7, 8);
            m.SetAnimation("idle");
            m.frame = Rando.Int(5);
            m.center = new Vec2(1f, 8f);
            m.alpha = 0.5f;
            _flavourLines.Add(m);
        }
        holsterAngle = -90f;
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        return false;
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        if (bullet.isLocal)
        {
            OnDestroy(new DTShot(bullet));
            base.velocity += bullet.travelDirNormalized * 0.7f;
            vSpeed -= 0.5f;
        }
        SFX.Play("smallSplat", Rando.Float(0.8f, 1f), Rando.Float(-0.2f, 0.2f));
        Level.Add(new WetEnterEffect(hitPos.x, hitPos.y, -bullet.travelDirNormalized, this));
        return base.Hit(bullet, hitPos);
    }

    public override void ExitHit(Bullet bullet, Vec2 exitPos)
    {
        Level.Add(new WetPierceEffect(exitPos.x, exitPos.y, bullet.travelDirNormalized, this));
    }

    public override void Update()
    {
        _timeHot--;
        base.Update();
    }

    public override void Draw()
    {
        base.Draw();
        if (_hotAlpha > 0f)
        {
            if (_timeHot <= 0)
            {
                _hotAlpha -= 0.01f;
            }
            float xOff = 0f;
            if (offDir < 0)
            {
                xOff = -2f;
            }
            for (int i = 0; i < 3; i++)
            {
                _flavourLines[i].depth = base.depth;
                _flavourLines[i].color = Color.White * _hotAlpha;
                Graphics.Draw(_flavourLines[i], base.x - 4f + (float)(i * 4) + xOff, base.y - 3f);
            }
        }
    }
}
