namespace DuckGame;

public class Ember : PhysicsParticle
{
    private SinWaveManualUpdate _wave = new SinWaveManualUpdate(0.1f + Rando.Float(0.1f));

    private Color _col;

    private float _initialLife = 1f;

    public Ember(float xpos, float ypos)
        : base(xpos, ypos)
    {
        vSpeed = 0f - (0.2f + Rando.Float(0.7f));
        hSpeed = -0.2f + Rando.Float(0.4f);
        if (Rando.Float(1f) < 0.4f)
        {
            _col = Color.Yellow;
        }
        else if (Rando.Float(1f) < 0.4f)
        {
            _col = Color.Orange;
        }
        else
        {
            _col = Color.Gray;
        }
        if (Rando.Float(1f) < 0.2f)
        {
            _initialLife += Rando.Float(10f);
        }
        base.alpha = 0.7f;
    }

    public override void Update()
    {
        _wave.Update();
        position.x += _wave.value * 0.2f;
        position.x += hSpeed;
        position.y += vSpeed;
        _initialLife -= 0.1f;
        if (_initialLife < 0f)
        {
            base.alpha -= 0.025f;
            if (base.alpha < 0f)
            {
                Level.Remove(this);
            }
        }
    }

    public override void Draw()
    {
        Graphics.DrawRect(position, position + new Vec2(1f, 1f), _col * base.alpha, base.depth);
    }
}
