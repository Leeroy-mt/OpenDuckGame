namespace DuckGame;

public class Confetti : Thing
{
    private Color _color;

    private SinWaveManualUpdate _sin;

    private float _size;

    private float _speed;

    public Confetti(float xpos, float ypos)
        : base(xpos, ypos)
    {
        int num = Rando.ChooseInt(0, 1, 2, 3);
        if (num == 0)
        {
            _color = Color.Violet;
        }
        if (num == 1)
        {
            _color = Color.SkyBlue;
        }
        if (num == 2)
        {
            _color = Color.Wheat;
        }
        if (num == 4)
        {
            _color = Color.GreenYellow;
        }
        _sin = new SinWaveManualUpdate(0.01f + Rando.Float(0.03f), Rando.Float(7f));
        _size = 10f + Rando.Float(60f);
        _speed = 0.8f + Rando.Float(1.4f);
        base.depth = 0.95f;
    }

    public override void Update()
    {
        _sin.Update();
        base.y += _speed;
    }

    public override void Draw()
    {
        Vec2 pos = position;
        pos.x += _sin.value * _size;
        Graphics.DrawRect(pos, pos + new Vec2(2f, 2f), _color, base.depth);
    }
}
