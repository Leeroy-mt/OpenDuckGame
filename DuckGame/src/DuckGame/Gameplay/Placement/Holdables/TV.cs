using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Stuff|Props")]
public class TV : Holdable, IPlatform
{
    private SpriteMap _sprite;

    private Sprite _frame;

    private Sprite _damaged;

    public StateBinding _ruinedBinding = new StateBinding(nameof(_ruined));

    public StateBinding _channelBinding = new StateBinding(nameof(channel));

    private float _ghostWait = 1f;

    private bool _madeGhost;

    public bool channel;

    private int _switchFrames;

    private Sprite _rainbow;

    private Cape _cape;

    public bool jumpReady;

    private List<Vector2> trail = new List<Vector2>();

    private SpriteMap _channels;

    private SpriteMap _tvNoise;

    private int wait;

    private bool fakeGrounded;

    private float prevVSpeed;

    public TV(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("plasma2", 16, 16);
        _sprite.speed = 0.2f;
        graphic = _sprite;
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-8f, -7f);
        collisionSize = new Vector2(16f, 14f);
        base.Depth = -0.5f;
        _editorName = "TV";
        thickness = 2f;
        weight = 5f;
        holsterAngle = 0f;
        flammable = 0.3f;
        _frame = new Sprite("tv2");
        _frame.CenterOrigin();
        _damaged = new Sprite("tvBroken");
        _damaged.CenterOrigin();
        _holdOffset = new Vector2(2f, 0f);
        _breakForce = 4f;
        base.collideSounds.Add("landTV");
        physicsMaterial = PhysicsMaterial.Metal;
        _channels = new SpriteMap("channels", 8, 6);
        _channels.Depth = base.Depth + 5;
        _tvNoise = new SpriteMap("tvnoise", 8, 6);
        _tvNoise.AddAnimation("noise", 0.6f, true, 0, 1, 2);
        _tvNoise.currentAnimation = "noise";
        _rainbow = new Sprite("rainbowGradient");
        editorTooltip = "Your source for breaking Duck Channel news.";
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            _cape = new Cape(base.X, base.Y, this, trail: true);
            _cape.metadata.CapeIsTrail.value = true;
            _cape._capeTexture = new Sprite("rainbowCarp").texture;
            Level.Add(_cape);
        }
        base.Initialize();
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        if (!base.isServerForObject || (type.thing != null && type.thing is Duck))
        {
            return false;
        }
        if (!_ruined)
        {
            _ruined = true;
            graphic = _damaged;
            SFX.Play("breakTV");
            for (int i = 0; i < 8; i++)
            {
                Level.Add(new GlassParticle(base.X + Rando.Float(-8f, 8f), base.Y + Rando.Float(-8f, 8f), new Vector2(Rando.Float(-1f, 1f), Rando.Float(-1f, 1f))));
            }
            base.collideSounds.Clear();
            base.collideSounds.Add("deadTVLand");
            _sendDestroyMessage = true;
        }
        return false;
    }

    public override bool Hit(Bullet bullet, Vector2 hitPos)
    {
        if (bullet.isLocal && owner == null)
        {
            Thing.Fondle(this, DuckNetwork.localConnection);
        }
        if (base.isServerForObject && !_ruined && bullet.isLocal)
        {
            OnDestroy(new DTShot(bullet));
            return base.Hit(bullet, hitPos);
        }
        return base.Hit(bullet, hitPos);
    }

    public override void Update()
    {
        if (_switchFrames > 0)
        {
            _switchFrames--;
        }
        if (_ruined)
        {
            if (_cape != null)
            {
                Level.Remove(_cape);
                _cape = null;
            }
            graphic = _damaged;
            if (_ghostWait > 0f)
            {
                _ghostWait -= 0.4f;
            }
            else
            {
                if (!_madeGhost)
                {
                    Level.Add(new EscapingGhost(base.X, base.Y - 6f));
                    for (int i = 0; i < 8; i++)
                    {
                        Level.Add(Spark.New(base.X + Rando.Float(-8f, 8f), base.Y + Rando.Float(-8f, 8f), new Vector2(Rando.Float(-1f, 1f), Rando.Float(-1f, 1f))));
                    }
                }
                _madeGhost = true;
            }
        }
        Duck d = base.duck;
        if (_owner is SpikeHelm && _owner.owner is Duck)
        {
            d = _owner.owner as Duck;
        }
        if (d != null)
        {
            if (d.vSpeed < -1f && prevVSpeed > 0f && !d.tvJumped)
            {
                fakeGrounded = true;
            }
            jumpReady = jumpReady || d.grounded || fakeGrounded || d._vine != null;
            prevVSpeed = d.vSpeed;
        }
        fakeGrounded = false;
        base.Update();
    }

    public override void OnTeleport()
    {
        fakeGrounded = true;
        base.OnTeleport();
    }

    [NetworkAction]
    public void SwitchChannelEffect()
    {
        _switchFrames = 8;
        SFX.Play("switchchannel", 0.7f, 0.5f);
    }

    public override void OnPressAction()
    {
        if (!_ruined)
        {
            channel = !channel;
            SyncNetworkAction(SwitchChannelEffect);
        }
        base.OnPressAction();
    }

    public override void Draw()
    {
        base.Draw();
        if (_ruined)
        {
            return;
        }
        Sprite sprite = _frame;
        SpriteMap channels = _channels;
        float num = (_tvNoise.Angle = Angle);
        float num3 = (channels.Angle = num);
        sprite.Angle = num3;
        Sprite sprite2 = _frame;
        SpriteMap channels2 = _channels;
        bool flag = (_tvNoise.flipH = offDir < 0);
        bool flipH = (channels2.flipH = flag);
        sprite2.flipH = flipH;
        _frame.Depth = base.Depth + 1;
        Graphics.Draw(_frame, base.X, base.Y);
        _channels.Alpha = Lerp.Float(_channels.Alpha, (owner != null) ? 1f : 0f, 0.1f);
        _channels.Depth = base.Depth + 4;
        _channels.frame = (channel ? (jumpReady ? 1 : 2) : 0);
        Vector2 channelPos = Offset(new Vector2(-4f, -4f));
        Graphics.Draw(_channels, channelPos.X, channelPos.Y);
        if (owner != null)
        {
            Vector2 prev = Vector2.Zero;
            bool hasPrev = false;
            foreach (Vector2 v in trail)
            {
                if (!hasPrev)
                {
                    hasPrev = true;
                }
                else
                {
                    Graphics.DrawTexturedLine(_rainbow.texture, prev, v, Color.White, 1f, base.Depth - 10);
                }
                prev = v;
            }
        }
        if (_switchFrames > 0)
        {
            _tvNoise.Alpha = 1f;
        }
        else
        {
            _tvNoise.Alpha = 0.2f;
        }
        _tvNoise.Depth = base.Depth + 8;
        Graphics.Draw(_tvNoise, channelPos.X, channelPos.Y);
    }
}
