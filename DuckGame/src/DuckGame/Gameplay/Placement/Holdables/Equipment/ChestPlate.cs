using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Equipment")]
[BaggedProperty("isInDemo", true)]
public class ChestPlate : Equipment
{
    private SpriteMap _sprite;

    private SpriteMap _spriteOver;

    private Sprite _pickupSprite;

    public ChestPlate(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("chestPlateAnim", 32, 32);
        _spriteOver = new SpriteMap("chestPlateAnimOver", 32, 32);
        _pickupSprite = new Sprite("chestPlatePickup");
        _pickupSprite.CenterOrigin();
        graphic = _pickupSprite;
        collisionOffset = new Vector2(-6f, -4f);
        collisionSize = new Vector2(11f, 8f);
        _equippedCollisionOffset = new Vector2(-7f, -5f);
        _equippedCollisionSize = new Vector2(12f, 11f);
        _hasEquippedCollision = true;
        Center = new Vector2(8f, 8f);
        physicsMaterial = PhysicsMaterial.Metal;
        _equippedDepth = 4;
        _wearOffset = new Vector2(1f, 1f);
        _isArmor = true;
        _equippedThickness = 3f;
        editorTooltip = "Protects against impacts to the chest. Makes you look swole.";
    }

    public override void Update()
    {
        if (_equippedDuck != null && base.duck == null)
        {
            return;
        }
        if (_equippedDuck != null && !destroyed)
        {
            Center = new Vector2(16f, 16f);
            solid = false;
            _sprite.flipH = base.duck._sprite.flipH;
            _spriteOver.flipH = base.duck._sprite.flipH;
            graphic = _sprite;
            if (_equippedDuck.sliding)
            {
                _equippedCollisionOffset = new Vector2(-5f, -5f);
                _equippedCollisionSize = new Vector2(10f, 13f);
            }
            else
            {
                _equippedCollisionOffset = new Vector2(-7f, -5f);
                _equippedCollisionSize = new Vector2(12f, 11f);
            }
        }
        else
        {
            Center = new Vector2(_pickupSprite.w / 2, _pickupSprite.h / 2);
            solid = true;
            _sprite.frame = 0;
            _sprite.flipH = false;
            graphic = _pickupSprite;
        }
        if (destroyed)
        {
            base.Alpha -= 0.05f;
        }
        if (base.Alpha < 0f)
        {
            Level.Remove(this);
        }
        base.Update();
    }

    public override void Draw()
    {
        if (_equippedDuck != null && _equippedDuck._trapped != null)
        {
            base.Depth = _equippedDuck._trapped.Depth + 1;
        }
        base.Draw();
        if ((_equippedDuck == null || base.duck != null) && _equippedDuck != null)
        {
            _spriteOver.flipH = graphic.flipH;
            _spriteOver.Angle = Angle;
            _spriteOver.Alpha = base.Alpha;
            _spriteOver.Scale = base.Scale;
            _spriteOver.Depth = owner.Depth + ((base.duck.holdObject != null) ? 5 : 12);
            _spriteOver.Center = Center;
            Graphics.Draw(_spriteOver, base.X, base.Y);
        }
    }
}
