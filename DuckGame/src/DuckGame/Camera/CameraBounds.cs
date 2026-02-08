using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Special", EditorItemType.Arcade)]
public class CameraBounds : Thing
{
    public EditorProperty<int> wide;

    public EditorProperty<int> high;

    public CameraBounds()
    {
        graphic = new Sprite("swirl");
        Center = new Vector2(8f, 8f);
        collisionSize = new Vector2(16f, 16f);
        collisionOffset = new Vector2(-8f, -8f);
        _canFlip = false;
        _visibleInGame = false;
        wide = new EditorProperty<int>(320, this, 60f, 1920f, 1f);
        high = new EditorProperty<int>(320, this, 60f, 1920f, 1f);
    }

    public override void Draw()
    {
        base.Draw();
        float wid = wide.value;
        float hig = high.value;
        Graphics.DrawRect(Position + new Vector2((0f - wid) / 2f, (0f - hig) / 2f), Position + new Vector2(wid / 2f, hig / 2f), Color.Blue * 0.5f, 1f, filled: false);
    }
}
