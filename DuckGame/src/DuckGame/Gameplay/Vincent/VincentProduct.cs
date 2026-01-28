namespace DuckGame;

public class VincentProduct
{
    public VPType type;

    public int cost;

    public int originalCost;

    public int rarity;

    public int count;

    public Furniture furnitureData;

    public Team teamData;

    public bool sold;

    public Sprite sprite
    {
        get
        {
            if (furnitureData != null)
            {
                return furnitureData.sprite;
            }
            if (teamData != null)
            {
                return teamData.hat;
            }
            return null;
        }
    }

    public Color color
    {
        get
        {
            if (furnitureData != null)
            {
                return furnitureData.group.color;
            }
            return Color.White;
        }
    }

    public string name
    {
        get
        {
            if (furnitureData != null)
            {
                return furnitureData.name;
            }
            if (teamData != null)
            {
                return teamData.name + " HAT";
            }
            return "Something";
        }
    }

    public string group
    {
        get
        {
            if (furnitureData != null)
            {
                return furnitureData.group.name;
            }
            return "HATS";
        }
    }

    public string description
    {
        get
        {
            if (furnitureData != null)
            {
                return furnitureData.description;
            }
            if (teamData != null)
            {
                return teamData.description;
            }
            return "What a fine piece of furniture.";
        }
    }

    public void Draw(Vec2 pos, float alpha, float deep)
    {
        if (furnitureData != null)
        {
            SpriteMap spr = furnitureData.sprite;
            if (furnitureData.icon != null)
            {
                spr = furnitureData.icon;
            }
            if (furnitureData.font != null && furnitureData.sprite == null)
            {
                furnitureData.font.Scale = new Vec2(1f, 1f);
                furnitureData.font.Draw("F", pos + new Vec2(-3.5f, -3f), Color.Black, deep + 0.005f);
            }
            spr.Depth = deep;
            spr.frame = 0;
            spr.Alpha = alpha;
            Graphics.Draw(spr, pos.X, pos.Y);
            spr.Alpha = 1f;
        }
        if (teamData != null)
        {
            SpriteMap hat = teamData.hat;
            hat.Depth = deep;
            hat.frame = 0;
            hat.Alpha = alpha;
            hat.Center = new Vec2(16f, 16f) + teamData.hatOffset;
            Graphics.Draw(hat, pos.X, pos.Y);
            hat.Alpha = 1f;
        }
    }
}
