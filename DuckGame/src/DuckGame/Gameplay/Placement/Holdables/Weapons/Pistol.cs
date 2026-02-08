using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Guns|Pistols")]
[BaggedProperty("isInDemo", true)]
[BaggedProperty("previewPriority", true)]
public class Pistol : Gun
{
    private SpriteMap _sprite;

    public Pistol(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 9;
        _ammoType = new AT9mm();
        wideBarrel = true;
        barrelInsertOffset = new Vector2(0f, -1f);
        _type = "gun";
        _sprite = new SpriteMap("pistol", 18, 10);
        _sprite.AddAnimation("idle", 1f, true, default(int));
        _sprite.AddAnimation("fire", 0.8f, false, 1, 2, 2, 3, 3);
        _sprite.AddAnimation("empty", 1f, true, 2);
        graphic = _sprite;
        Center = new Vector2(10f, 3f);
        collisionOffset = new Vector2(-8f, -3f);
        collisionSize = new Vector2(16f, 9f);
        _barrelOffsetTL = new Vector2(18f, 2f);
        _fireSound = "pistolFire";
        _kickForce = 3f;
        _fireRumble = RumbleIntensity.Kick;
        _holdOffset = new Vector2(0f, 0f);
        loseAccuracy = 0.1f;
        maxAccuracyLost = 0.6f;
        _bio = "Old faithful, the 9MM pistol.";
        _editorName = "Pistol";
        editorTooltip = "Your average everyday pistol. Just workin' to keep its kids fed, never bothered nobody.";
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
