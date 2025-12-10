namespace DuckGame;

[EditorGroup("Stuff|Pyramid", EditorItemType.Pyramid)]
public class Altar : Platform
{
    public EditorProperty<int> wide;

    public bool kill;

    public Platform leftPlat;

    public Platform rightPlat;

    public override void EditorPropertyChanged(object property)
    {
        (graphic as SpriteMap).frame = wide.value - 1;
        UpdateSize();
    }

    public void UpdateSize()
    {
        if ((graphic as SpriteMap).frame == 0)
        {
            center = new(8, 8);
            collisionSize = new(12, 13);
            collisionOffset = new(-6, -5);
        }
        else if ((graphic as SpriteMap).frame == 1)
        {
            center = new(16, 8);
            collisionSize = new(28, 13);
            collisionOffset = new(-14, -5);
        }
        else if ((graphic as SpriteMap).frame == 2)
        {
            center = new(24, 8);
            collisionSize = new(44, 13);
            collisionOffset = new(-22, -5);
        }
    }

    public Altar(float xpos, float ypos, int dir)
        : base(xpos, ypos)
    {
        wide = new(1, this, 1, 3, 1);
        graphic = new SpriteMap("altar", 48, 16);
        hugWalls = WallHug.Floor;
        UpdateSize();
        thickness = 0;
        depth = -0.05f;
        placementLayerOverride = Layer.Blocks;
    }

    public override void Draw()
    {
        flipHorizontal = false;
        base.Draw();
    }

    public override void Terminate()
    {
        if (leftPlat != null)
            Level.Remove(leftPlat);
        if (rightPlat != null)
            Level.Remove(rightPlat);

        base.Terminate();
    }
}