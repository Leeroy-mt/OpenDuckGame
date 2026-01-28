using System;

namespace DuckGame;

[EditorGroup("Guns|Shotguns")]
public class Shotgun : Gun
{
    public sbyte _loadProgress = 100;

    public float _loadAnimation = 1f;

    public StateBinding _loadProgressBinding = new StateBinding(nameof(_loadProgress));

    protected SpriteMap _loaderSprite;

    public Shotgun(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 2;
        _ammoType = new ATShotgun();
        wideBarrel = true;
        _type = "gun";
        graphic = new Sprite("shotgun");
        Center = new Vec2(16f, 16f);
        collisionOffset = new Vec2(-8f, -3f);
        collisionSize = new Vec2(16f, 8f);
        _barrelOffsetTL = new Vec2(30f, 14f);
        _fireSound = "shotgunFire2";
        _kickForce = 4f;
        _fireRumble = RumbleIntensity.Light;
        _numBulletsPerFire = 6;
        _manualLoad = true;
        _loaderSprite = new SpriteMap("shotgunLoader", 8, 8);
        _loaderSprite.Center = new Vec2(4f, 4f);
        editorTooltip = "It's...a shotgun. I don't really have anything more to say about it.";
    }

    public override void Update()
    {
        base.Update();
        if (_loadAnimation == -1f)
        {
            SFX.Play("shotgunLoad");
            _loadAnimation = 0f;
        }
        if (_loadAnimation >= 0f)
        {
            if (_loadAnimation == 0.5f && ammo != 0)
            {
                PopShell();
            }
            if (_loadAnimation < 1f)
            {
                _loadAnimation += 0.1f;
            }
            else
            {
                _loadAnimation = 1f;
            }
        }
        if (_loadProgress >= 0)
        {
            if (_loadProgress == 50)
            {
                Reload(shell: false);
            }
            if (_loadProgress < 100)
            {
                _loadProgress += 10;
            }
            else
            {
                _loadProgress = 100;
            }
        }
    }

    public override void OnPressAction()
    {
        if (loaded)
        {
            base.OnPressAction();
            _loadProgress = -1;
            _loadAnimation = -0.01f;
        }
        else if (_loadProgress == -1)
        {
            _loadProgress = 0;
            _loadAnimation = -1f;
        }
    }

    public override void Draw()
    {
        base.Draw();
        Vec2 bOffset = new Vec2(13f, -2f);
        float offset = (float)Math.Sin(_loadAnimation * 3.14f) * 3f;
        Draw(_loaderSprite, new Vec2(bOffset.X - 8f - offset, bOffset.Y + 4f));
    }
}
