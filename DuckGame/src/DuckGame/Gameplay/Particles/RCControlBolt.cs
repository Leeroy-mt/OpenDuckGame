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
        center = new Vec2(8f, 8f);
        base.scale = new Vec2(0.3f, 0.3f);
        base.alpha = 1f;
    }

    public override void Update()
    {
        Vec2 travel = _control.position - position;
        float length = travel.length;
        travel.Normalize();
        base.angleDegrees = 0f - Maths.PointDirection(position, _control.position) + 90f;
        position += travel * 8f;
        float num = (base.yscale = Lerp.Float(base.xscale, 1f, 0.1f));
        base.xscale = num;
        if (length < 48f || _control.destroyed || !_control.receivingSignal)
        {
            _fade = true;
        }
        base.alpha = Lerp.Float(base.alpha, _fade ? 0f : 1f, 0.1f);
        if (base.alpha < 0.01f && _fade)
        {
            Level.Remove(this);
        }
        base.Update();
    }
}
