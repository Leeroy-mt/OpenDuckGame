using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Guns|Pistols")]
public class OldPistol : Gun
{
    public StateBinding _loadStateBinding = new StateBinding(nameof(_loadState));

    public int _loadState = -1;

    public float _angleOffset;

    private SpriteMap _sprite;

    public OldPistol(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 2;
        _ammoType = new ATOldPistol();
        _type = "gun";
        _sprite = new SpriteMap("oldPistol", 32, 32);
        graphic = _sprite;
        Center = new Vector2(16f, 17f);
        collisionOffset = new Vector2(-8f, -4f);
        collisionSize = new Vector2(16f, 8f);
        _barrelOffsetTL = new Vector2(24f, 16f);
        _fireSound = "shotgun";
        _kickForce = 2f;
        _fireRumble = RumbleIntensity.Kick;
        _manualLoad = true;
        _holdOffset = new Vector2(2f, 0f);
        editorTooltip = "A pain in the tailfeathers to reload, but it'll get the job done.";
    }

    public override void Update()
    {
        base.Update();
        if (ammo > 1)
        {
            _sprite.frame = 0;
        }
        else
        {
            _sprite.frame = 1;
        }
        if (infinite.value)
        {
            UpdateLoadState();
            UpdateLoadState();
        }
        else
        {
            UpdateLoadState();
        }
    }

    private void UpdateLoadState()
    {
        if (_loadState <= -1)
        {
            return;
        }
        if (owner == null)
        {
            if (_loadState == 3)
            {
                loaded = true;
            }
            _loadState = -1;
            _angleOffset = 0f;
            handOffset = Vector2.Zero;
        }
        if (_loadState == 0)
        {
            if (Network.isActive)
            {
                if (base.isServerForObject)
                {
                    NetSoundEffect.Play("oldPistolSwipe");
                }
            }
            else
            {
                SFX.Play("swipe", 0.6f, -0.3f);
            }
            _loadState++;
        }
        else if (_loadState == 1)
        {
            if (_angleOffset < 0.16f)
            {
                _angleOffset = MathHelper.Lerp(_angleOffset, 0.2f, 0.08f);
            }
            else
            {
                _loadState++;
            }
        }
        else if (_loadState == 2)
        {
            handOffset.Y -= 0.28f;
            if (!(handOffset.Y < -4f))
            {
                return;
            }
            _loadState++;
            ammo = 2;
            loaded = false;
            if (Network.isActive)
            {
                if (base.isServerForObject)
                {
                    NetSoundEffect.Play("oldPistolLoad");
                }
            }
            else
            {
                SFX.Play("shotgunLoad");
            }
        }
        else if (_loadState == 3)
        {
            handOffset.Y += 0.15f;
            if (!(handOffset.Y >= 0f))
            {
                return;
            }
            _loadState++;
            handOffset.Y = 0f;
            if (Network.isActive)
            {
                if (base.isServerForObject)
                {
                    NetSoundEffect.Play("oldPistolSwipe2");
                }
            }
            else
            {
                SFX.Play("swipe", 0.7f);
            }
        }
        else
        {
            if (_loadState != 4)
            {
                return;
            }
            if (_angleOffset > 0.04f)
            {
                _angleOffset = MathHelper.Lerp(_angleOffset, 0f, 0.08f);
                return;
            }
            _loadState = -1;
            loaded = true;
            _angleOffset = 0f;
            if (base.isServerForObject && base.duck != null && base.duck.profile != null)
            {
                RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(RumbleIntensity.Kick, RumbleDuration.Pulse, RumbleFalloff.None));
            }
            if (Network.isActive)
            {
                if (base.isServerForObject)
                {
                    SFX.PlaySynchronized("click", 1f, 0.5f, 0f, looped: false, louderForMe: true);
                }
            }
            else
            {
                SFX.Play("click", 1f, 0.5f);
            }
        }
    }

    public override void OnPressAction()
    {
        if (loaded && ammo > 1)
        {
            base.OnPressAction();
            for (int i = 0; i < 4; i++)
            {
                Level.Add(Spark.New((offDir > 0) ? (base.X - 9f) : (base.X + 9f), base.Y - 6f, new Vector2(Rando.Float(-1f, 1f), -0.5f), 0.05f));
            }
            for (int j = 0; j < 4; j++)
            {
                Level.Add(SmallSmoke.New(base.barrelPosition.X + (float)offDir * 4f, base.barrelPosition.Y));
            }
            ammo = 1;
        }
        else if (_loadState == -1)
        {
            _loadState = 0;
        }
    }

    public override void Draw()
    {
        float ang = Angle;
        if (offDir > 0)
        {
            Angle -= _angleOffset;
        }
        else
        {
            Angle += _angleOffset;
        }
        base.Draw();
        Angle = ang;
    }
}
