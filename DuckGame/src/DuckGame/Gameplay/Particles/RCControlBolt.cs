using Microsoft.Xna.Framework;

namespace DuckGame;

public class RCControlBolt : Thing
{
    private bool _fade;

    private RCCar _control;

    public RCCar control => _control;

    public RCControlBolt(float xval, float yval, RCCar c)
        : base(xval, yval)
    {
        _control = c;
        graphic = new Sprite("rcBolt");
        Center = new Vector2(8f, 8f);
        base.Scale = new Vector2(0.3f, 0.3f);
        base.Alpha = 1f;
    }

    public override void Update()
    {
        Vector2 travel = _control.Position - Position;
        float length = travel.Length();
        travel.Normalize();
        base.AngleDegrees = 0f - Maths.PointDirection(Position, _control.Position) + 90f;
        Position += travel * 8f;
        float num = (base.ScaleY = Lerp.Float(base.ScaleX, 1f, 0.1f));
        base.ScaleX = num;
        if (length < 48f || _control.destroyed || !_control.receivingSignal)
        {
            _fade = true;
        }
        base.Alpha = Lerp.Float(base.Alpha, _fade ? 0f : 1f, 0.1f);
        if (base.Alpha < 0.01f && _fade)
        {
            Level.Remove(this);
        }
        base.Update();
    }
}
