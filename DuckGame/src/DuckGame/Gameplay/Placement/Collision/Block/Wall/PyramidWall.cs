namespace DuckGame;

public class PyramidWall : Block, IBigStupidWall
{
    private Sprite _corner;

    private Sprite _corner2;

    private Vec2 levelCenter = new Vec2(242f, 100f);

    public bool hasLeft;

    public bool hasRight;

    public bool hasUp;

    public bool hasDown;

    public PyramidWall(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("pyramidEdge");
        collisionSize = new Vec2(200f, 153f);
        collisionOffset = new Vec2(-4f, -4f);
        _corner = new Sprite("pyWallCorner");
        _corner2 = new Sprite("pyWallCorner2");
        physicsMaterial = PhysicsMaterial.Metal;
        base.Depth = -0.9f;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public void AddExtraWalls()
    {
        if (hasUp)
        {
            for (int i = -1; i < 13; i++)
            {
                Vec2 check = new Vec2(base.left + (float)(i * 16) + 12f, base.top - 4f);
                Thing t = Level.current.CollisionPoint<Thing>(check, this);
                if (t == null || t is AutoPlatform || t is BackgroundTile)
                {
                    Level.Add(new PyramidTileset(check.X, check.Y));
                }
            }
        }
        if (hasDown)
        {
            for (int j = -1; j < 13; j++)
            {
                Vec2 check2 = new Vec2(base.left + (float)(j * 16) + 12f, base.bottom + 4f);
                Thing t2 = Level.current.CollisionPoint<Thing>(check2, this);
                if (t2 == null || t2 is AutoPlatform || t2 is BackgroundTile)
                {
                    Level.Add(new PyramidTileset(check2.X, check2.Y - 1f));
                }
            }
        }
        if (hasLeft)
        {
            for (int k = 0; k < 9; k++)
            {
                Vec2 check3 = new Vec2(base.left - 4f, base.top + (float)(k * 16) + 12f);
                Thing t3 = Level.current.CollisionPoint<Thing>(check3, this);
                if (t3 == null || t3 is AutoPlatform || t3 is BackgroundTile)
                {
                    Level.Add(new PyramidTileset(check3.X, check3.Y));
                }
            }
        }
        if (!hasRight)
        {
            return;
        }
        for (int l = 0; l < 9; l++)
        {
            Vec2 check4 = new Vec2(base.right + 4f, base.top + (float)(l * 16) + 12f);
            Thing t4 = Level.current.CollisionPoint<Thing>(check4, this);
            if (t4 == null || t4 is AutoPlatform || t4 is BackgroundTile)
            {
                Level.Add(new PyramidTileset(check4.X, check4.Y));
            }
        }
    }

    public override void Draw()
    {
        graphic.Depth = -0.8f;
        Graphics.Draw(graphic, base.X - 8f, base.Y - 8f, new Rectangle(0f, 0f, 208f, 8f));
        graphic.Depth = -0.85f;
        Graphics.Draw(graphic, base.X, base.Y + 144f, new Rectangle(8f, 152f, 192f, 8f));
        graphic.Depth = -0.86f;
        Graphics.Draw(graphic, base.X + 192f, base.Y, new Rectangle(200f, 8f, 8f, 144f));
        Graphics.Draw(graphic, base.X - 8f, base.Y - 8f, new Rectangle(0f, 0f, 8f, 152f));
        _corner.Depth = -0.9f;
        Graphics.Draw(_corner, base.X - 8f, base.Y + 144f);
        _corner2.Depth = -0.9f;
        Graphics.Draw(_corner2, base.X + 192f, base.Y + 144f);
        graphic.Depth = -0.7f;
        Graphics.Draw(graphic, base.X, base.Y, new Rectangle(8f, 8f, 192f, 144f));
        if (!DevConsole.showCollision)
        {
            return;
        }
        Graphics.DrawRect(base.rectangle, Color.Red, 1f, filled: false);
        if (hasUp)
        {
            for (int i = 0; i < 12; i++)
            {
                Vec2 check = new Vec2(base.left + (float)(i * 16) + 12f, base.top - 2f);
                Graphics.DrawRect(check + new Vec2(-2f, -2f), check + new Vec2(2f, 2f), Color.Orange, 1f);
            }
        }
        if (hasDown)
        {
            for (int j = -1; j < 13; j++)
            {
                Vec2 check2 = new Vec2(base.left + (float)(j * 16) + 12f, base.bottom + 2f);
                Graphics.DrawRect(check2 + new Vec2(-2f, -2f), check2 + new Vec2(2f, 2f), Color.Orange, 1f);
            }
        }
        if (hasLeft)
        {
            for (int k = 0; k < 9; k++)
            {
                Vec2 check3 = new Vec2(base.left - 2f, base.top + (float)(k * 16) + 12f);
                Graphics.DrawRect(check3 + new Vec2(-2f, -2f), check3 + new Vec2(2f, 2f), Color.Orange, 1f);
            }
        }
        if (hasRight)
        {
            for (int l = 0; l < 9; l++)
            {
                Vec2 check4 = new Vec2(base.right + 2f, base.top + (float)(l * 16) + 12f);
                Graphics.DrawRect(check4 + new Vec2(-2f, -2f), check4 + new Vec2(2f, 2f), Color.Orange, 1f);
            }
        }
    }
}
