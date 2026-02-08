using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

[EditorGroup("Guns|Machine Guns")]
[BaggedProperty("isSuperWeapon", true)]
public class Chaingun : Gun
{
    public StateBinding _fireWaitBinding = new StateBinding(nameof(_fireWait));

    public StateBinding _spinBinding = new StateBinding(nameof(_spin));

    public StateBinding _spinningBinding = new StateBinding(nameof(_spinning));

    private SpriteMap _tip;

    private SpriteMap _sprite;

    public float _spin;

    private ChaingunBullet _bullets;

    private ChaingunBullet _topBullet;

    private Sound _spinUp;

    private Sound _spinDown;

    private int bulletsTillRemove = 10;

    private int numHanging = 10;

    private bool _spinning;

    private float spinAmount;

    public Chaingun(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 100;
        _ammoType = new AT9mm();
        _ammoType.range = 170f;
        _ammoType.accuracy = 0.5f;
        wideBarrel = true;
        barrelInsertOffset = new Vector2(0f, 0f);
        _type = "gun";
        _sprite = new SpriteMap("chaingun", 42, 28);
        graphic = _sprite;
        Center = new Vector2(14f, 14f);
        collisionOffset = new Vector2(-8f, -3f);
        collisionSize = new Vector2(24f, 10f);
        _tip = new SpriteMap("chaingunTip", 42, 28);
        _barrelOffsetTL = new Vector2(39f, 14f);
        _fireSound = "pistolFire";
        _fullAuto = true;
        _fireWait = 0.7f;
        _kickForce = 1f;
        _fireRumble = RumbleIntensity.Kick;
        weight = 8f;
        _spinUp = SFX.Get("chaingunSpinUp");
        _spinDown = SFX.Get("chaingunSpinDown");
        _holdOffset = new Vector2(0f, 2f);
        editorTooltip = "Like a chaingun, but for adults. Fires mean pointy metal things.";
    }

    public override void Initialize()
    {
        base.Initialize();
        _bullets = new ChaingunBullet(base.X, base.Y);
        _bullets.parentThing = this;
        _topBullet = _bullets;
        float add = 0.1f;
        ChaingunBullet lastBullet = null;
        for (int i = 0; i < 9; i++)
        {
            ChaingunBullet b = new ChaingunBullet(base.X, base.Y);
            b.parentThing = _bullets;
            _bullets = b;
            b.waveAdd = add;
            add += 0.4f;
            if (i == 0)
            {
                _topBullet.childThing = b;
            }
            else
            {
                lastBullet.childThing = b;
            }
            lastBullet = b;
        }
    }

    public override void Terminate()
    {
    }

    public override void OnHoldAction()
    {
        if (!_spinning)
        {
            _spinning = true;
            _spinDown.Volume = 0f;
            _spinDown.Stop();
            _spinUp.Volume = 1f;
            _spinUp.Play();
        }
        if (_spin < 1f)
        {
            _spin += 0.04f;
            return;
        }
        _spin = 1f;
        base.OnHoldAction();
    }

    public override void OnReleaseAction()
    {
        if (_spinning)
        {
            _spinning = false;
            _spinUp.Volume = 0f;
            _spinUp.Stop();
            if (_spin > 0.9f)
            {
                _spinDown.Volume = 1f;
                _spinDown.Play();
            }
        }
    }

    public override void Update()
    {
        if (_topBullet != null)
        {
            _topBullet.DoUpdate();
            int newHanging = (int)((float)ammo / (float)bulletsTillRemove);
            if (newHanging < numHanging)
            {
                _topBullet = _topBullet.childThing as ChaingunBullet;
                if (_topBullet != null)
                {
                    _topBullet.parentThing = this;
                }
            }
            numHanging = newHanging;
        }
        _fireWait = 0.7f + Maths.NormalizeSection(_barrelHeat, 5f, 9f) * 5f;
        if (_barrelHeat > 11f)
        {
            _barrelHeat = 11f;
        }
        _barrelHeat -= 0.005f;
        if (_barrelHeat < 0f)
        {
            _barrelHeat = 0f;
        }
        _sprite.speed = _spin;
        _tip.speed = _spin;
        spinAmount += _spin;
        barrelInsertOffset = new Vector2(0f, 2f + (float)Math.Sin(spinAmount / 9f * 3.14f) * 2f);
        if (_spin > 0f)
        {
            _spin -= 0.01f;
        }
        else
        {
            _spin = 0f;
        }
        base.Update();
        if (_topBullet != null)
        {
            if (!graphic.flipH)
            {
                _topBullet.chainOffset = new Vector2(1f, 5f);
            }
            else
            {
                _topBullet.chainOffset = new Vector2(-1f, 5f);
            }
        }
    }

    public override void Draw()
    {
        Material obj = Graphics.material;
        base.Draw();
        Graphics.material = base.material;
        _tip.flipH = graphic.flipH;
        _tip.Center = graphic.Center;
        _tip.Depth = base.Depth + 1;
        _tip.Alpha = Math.Min(_barrelHeat * 1.5f / 10f, 1f);
        _tip.Angle = Angle;
        Graphics.Draw(_tip, base.X, base.Y);
        if (_topBullet != null)
        {
            _topBullet.material = base.material;
            _topBullet.DoDraw();
        }
        Graphics.material = obj;
    }
}
