namespace DuckGame;

[EditorGroup("Special", EditorItemType.Arcade)]
public class CustomCamera : Thing
{
    public EditorProperty<int> wide;

    public CustomCamera()
    {
        graphic = new Sprite("swirl");
        Center = new Vec2(8f, 8f);
        collisionSize = new Vec2(16f, 16f);
        collisionOffset = new Vec2(-8f, -8f);
        _canFlip = false;
        wide = new EditorProperty<int>(320, this, 60f, 1920f, 1f);
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            base.Alpha = 0f;
        }
        base.Initialize();
    }

    public override void Draw()
    {
        base.Draw();
        if (!Editor.editorDraw && Level.current is Editor)
        {
            float wid = wide.value;
            float hig = wid * 0.5625f;
            Graphics.DrawRect(Position + new Vec2((0f - wid) / 2f, (0f - hig) / 2f), Position + new Vec2(wid / 2f, hig / 2f), Color.Blue * 0.5f, 1f, filled: false);
        }
    }
}
