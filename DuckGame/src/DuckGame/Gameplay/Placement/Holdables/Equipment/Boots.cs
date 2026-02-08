using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Equipment")]
[BaggedProperty("previewPriority", true)]
public class Boots : Equipment
{
    protected SpriteMap _sprite;

    protected Sprite _pickupSprite;

    public Boots(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _pickupSprite = new Sprite("bootsPickup");
        _sprite = new SpriteMap("boots", 32, 32);
        graphic = _pickupSprite;
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-6f, -6f);
        collisionSize = new Vector2(12f, 13f);
        _equippedDepth = 3;
        editorTooltip = "Keeps feet safe and smashes bugs, among other things.";
        flammable = 0.3f;
        charThreshold = 0.8f;
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        if (type is DTIncinerate)
        {
            return false;
        }
        return true;
    }

    public override void Update()
    {
        if (_equippedDuck != null && !destroyed)
        {
            Center = new Vector2(16f, 12f);
            graphic = _sprite;
            collisionOffset = new Vector2(0f, -9999f);
            collisionSize = new Vector2(0f, 0f);
            solid = false;
            _sprite.frame = _equippedDuck._sprite.imageIndex;
            if (_equippedDuck.ragdoll != null)
            {
                _sprite.frame = 12;
            }
            _sprite.flipH = _equippedDuck._sprite.flipH;
        }
        else
        {
            Center = new Vector2(8f, 8f);
            graphic = _pickupSprite;
            collisionOffset = new Vector2(-6f, -6f);
            collisionSize = new Vector2(12f, 13f);
            solid = true;
            _sprite.frame = 0;
            _sprite.flipH = false;
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
            base.Depth = _equippedDuck._trapped.Depth + 2;
        }
        if (_equippedDuck != null)
        {
            _sprite.frame = _equippedDuck._sprite.imageIndex;
            if (_equippedDuck.ragdoll != null)
            {
                _sprite.frame = 12;
            }
            _sprite.flipH = _equippedDuck._sprite.flipH;
        }
        base.Draw();
    }
}
