namespace DuckGame;

internal class CrashLogRoom : Level
{
    private FancyBitmapFont _font = new FancyBitmapFont("smallFont");

    private string _error;

    public CrashLogRoom(string error)
    {
        _error = error;
        _centeredView = true;
    }

    public override void Initialize()
    {
        _startCalled = true;
        base.Initialize();
    }

    public override void Draw()
    {
        _font.Scale = new Vec2(0.5f, 0.5f);
        _font.Draw(_error, new Vec2(30f, 30f), Color.White);
    }
}
