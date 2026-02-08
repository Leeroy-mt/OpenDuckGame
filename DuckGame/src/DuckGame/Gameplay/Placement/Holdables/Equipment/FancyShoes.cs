using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Equipment", EditorItemType.PowerUser)]
[BaggedProperty("previewPriority", false)]
[BaggedProperty("canSpawn", false)]
public class FancyShoes : Boots
{
    public FancyShoes(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _pickupSprite = new Sprite("walljumpBootsPickup");
        _sprite = new SpriteMap("walljumpBoots", 32, 32);
        graphic = _pickupSprite;
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-6f, -6f);
        collisionSize = new Vector2(12f, 13f);
        _equippedDepth = 3;
    }
}
