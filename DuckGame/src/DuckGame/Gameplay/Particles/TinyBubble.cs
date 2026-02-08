using Microsoft.Xna.Framework;

namespace DuckGame;

public class TinyBubble : PhysicsParticle
{
    private SinWaveManualUpdate _wave = new SinWaveManualUpdate(0.1f + Rando.Float(0.1f), Rando.Float(3f));

    private float _minY;

    private float _waveSize = 1f;

    public TinyBubble(float xpos, float ypos, float startHSpeed, float minY, bool blue = false)
        : base(xpos, ypos)
    {
        base.Alpha = 0.7f;
        _minY = minY;
        _gravMult = 0f;
        vSpeed = 0f - Rando.Float(0.5f, 1f);
        hSpeed = startHSpeed;
        base.Depth = 0.3f;
        SpriteMap spr = new SpriteMap("tinyBubbles", 8, 8);
        if (blue)
        {
            spr = new SpriteMap("tinyBlueBubbles", 8, 8);
        }
        spr.frame = Rando.Int(0, 1);
        graphic = spr;
        Center = new Vector2(4f, 4f);
        _waveSize = Rando.Float(0.1f, 0.3f);
        base.ScaleX = (base.ScaleY = 0.1f);
    }

    public override void Update()
    {
        _wave.Update();
        X += _wave.value * _waveSize;
        X += hSpeed;
        Y += vSpeed;
        hSpeed = Lerp.Float(hSpeed, 0f, 0.1f);
        float num = (base.ScaleY = Lerp.Float(base.ScaleX, 1f, 0.1f));
        base.ScaleX = num;
        if (base.Y < _minY - 4f)
        {
            base.Alpha -= 0.025f;
        }
        if (base.Y < _minY - 8f)
        {
            base.Alpha = 0f;
        }
        if (base.Y < _minY)
        {
            base.Alpha -= 0.025f;
            if (base.Alpha < 0f)
            {
                Level.Remove(this);
            }
        }
    }
}
