namespace DuckGame;

public class SuperTestRoom : Level
{
    private Sprite _sprite;

    public override void Initialize()
    {
        _sprite = new Sprite("happyman");
        base.Initialize();
    }

    public override void Update()
    {
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.Game)
        {
            Graphics.Draw(_sprite, 10f, 10f);
        }
    }
}
