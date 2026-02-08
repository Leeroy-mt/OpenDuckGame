using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Stuff|Props")]
[BaggedProperty("isInDemo", true)]
public class Rock : Holdable, IPlatform
{
    public StateBinding _isGoldBinding = new StateBinding(nameof(isGoldRock));

    public EditorProperty<bool> gold = new EditorProperty<bool>(val: false);

    private SpriteMap _sprite;

    public bool isGoldRock;

    private bool _changedCollision;

    private bool _didKill;

    private int _killWait;

    private Sound _winSound;

    public Rock(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("rock01", 16, 16);
        graphic = _sprite;
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-8f, -5f);
        collisionSize = new Vector2(16f, 12f);
        base.Depth = -0.5f;
        thickness = 4f;
        weight = 7f;
        flammable = 0f;
        base.collideSounds.Add("rockHitGround2");
        physicsMaterial = PhysicsMaterial.Metal;
        editorTooltip = "Don’t throw rocks!";
        holsterAngle = 90f;
    }

    public override void Initialize()
    {
        if (gold.value)
        {
            base.material = new MaterialGold(this);
            isGoldRock = true;
        }
        base.Initialize();
    }

    public override void EditorRender()
    {
        if (gold.value)
        {
            if (base.material == null)
            {
                base.material = new MaterialGold(this);
                isGoldRock = true;
            }
        }
        else
        {
            base.material = null;
            isGoldRock = false;
        }
        base.EditorRender();
    }

    [NetworkAction]
    private void StartRockSong()
    {
        if (_winSound != null)
        {
            _winSound.Stop();
        }
        _killWait = 0;
        _didKill = true;
        _winSound = SFX.Play("winzone");
    }

    [NetworkAction]
    private void StopRockSong()
    {
        if (_winSound != null)
        {
            _winSound.Stop();
        }
        _killWait = 0;
        _didKill = false;
        _winSound = null;
    }

    public override void Update()
    {
        if (isGoldRock && !(base.material is MaterialGold))
        {
            base.material = new MaterialGold(this);
        }
        if (base.isServerForObject)
        {
            if (isGoldRock)
            {
                if (base.duck != null && !_didKill)
                {
                    SyncNetworkAction(StartRockSong);
                }
                if (base.duck == null)
                {
                    _didKill = false;
                    _killWait = 0;
                    if (_winSound != null)
                    {
                        SyncNetworkAction(StopRockSong);
                    }
                }
            }
            if (_didKill && base.duck != null)
            {
                _killWait++;
                if (_killWait == 108)
                {
                    foreach (Duck d in Level.current.things[typeof(Duck)])
                    {
                        if (d != null && d.team != base.duck.team)
                        {
                            d.Kill(new DTCrush(this));
                        }
                    }
                }
            }
        }
        if (base.duck == null)
        {
            _didKill = false;
            _killWait = 0;
        }
        if (base.raised)
        {
            if (!_changedCollision)
            {
                collisionSize = new Vector2(collisionSize.Y, collisionSize.X);
                collisionOffset = new Vector2(collisionOffset.Y, collisionOffset.X);
                _changedCollision = true;
            }
        }
        else if (_changedCollision)
        {
            collisionSize = new Vector2(collisionSize.Y, collisionSize.X);
            collisionOffset = new Vector2(collisionOffset.Y, collisionOffset.X);
            _changedCollision = false;
        }
        base.Update();
    }

    public override bool Hit(Bullet bullet, Vector2 hitPos)
    {
        if (bullet.isLocal && owner == null)
        {
            Thing.Fondle(this, DuckNetwork.localConnection);
        }
        if (base.isServerForObject && bullet.isLocal && TeamSelect2.Enabled("EXPLODEYCRATES"))
        {
            if (base.duck != null)
            {
                base.duck.ThrowItem();
            }
            Destroy(new DTShot(bullet));
            Level.Remove(this);
            Level.Add(new GrenadeExplosion(base.X, base.Y));
        }
        return base.Hit(bullet, hitPos);
    }
}
