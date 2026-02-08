using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Guns|Pistols")]
[BaggedProperty("isInDemo", true)]
public class MagBlaster : Gun
{
    private SpriteMap _sprite;

    public MagBlaster(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 12;
        _ammoType = new ATMag();
        _ammoType.penetration = 0.4f;
        wideBarrel = true;
        barrelInsertOffset = new Vector2(3f, 1f);
        _type = "gun";
        _sprite = new SpriteMap("magBlaster", 25, 19);
        _sprite.AddAnimation("idle", 1f, true, default(int));
        _sprite.AddAnimation("fire", 0.8f, false, 1, 1, 2, 2, 3, 3);
        _sprite.AddAnimation("empty", 1f, true, 4);
        graphic = _sprite;
        Center = new Vector2(12f, 8f);
        collisionOffset = new Vector2(-8f, -7f);
        collisionSize = new Vector2(16f, 14f);
        _barrelOffsetTL = new Vector2(20f, 5f);
        _fireSound = "magShot";
        _kickForce = 5f;
        _fireRumble = RumbleIntensity.Kick;
        _holdOffset = new Vector2(1f, 0f);
        loseAccuracy = 0.1f;
        maxAccuracyLost = 0.6f;
        _bio = "Old faithful, the 9MM pistol.";
        _editorName = "Mag Blaster";
        editorTooltip = "The preferred gun for enacting justice in a post-apocalyptic megacity.";
        physicsMaterial = PhysicsMaterial.Metal;
    }

    public override void Update()
    {
        if (_sprite.currentAnimation == "fire" && _sprite.finished)
        {
            _sprite.SetAnimation("idle");
        }
        base.Update();
    }

    public override void OnPressAction()
    {
        if (ammo > 0)
        {
            _sprite.SetAnimation("fire");
            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = Offset(new Vector2(-9f, 0f));
                Vector2 rot = base.barrelVector.Rotate(Rando.Float(1f), Vector2.Zero);
                Level.Add(Spark.New(pos.X, pos.Y, rot, 0.1f));
            }
        }
        else
        {
            _sprite.SetAnimation("empty");
        }
        base.Fire();
    }
}
