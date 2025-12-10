namespace DuckGame;

public class MindControlBolt : Thing
{
    private bool _fade;

    private Duck _controlledDuck;

    public Duck controlledDuck => _controlledDuck;

    public MindControlBolt(float xval, float yval, Duck control)
        : base(xval, yval)
    {
        _controlledDuck = control;
        graphic = new Sprite("mindBolt");
        center = new Vec2(8f, 8f);
        base.scale = new Vec2(0.1f, 0.1f);
        base.alpha = 0f;
    }

    public override void Update()
    {
        Vec2 pos = _controlledDuck.position;
        if (_controlledDuck.ragdoll != null)
        {
            pos = _controlledDuck.ragdoll.part3.position;
        }
        Vec2 travel = pos - position;
        float length = travel.length;
        travel.Normalize();
        base.angleDegrees = 0f - Maths.PointDirection(position, pos) + 90f;
        position += travel * 4f;
        float num = (base.yscale = Lerp.Float(base.xscale, 1f, 0.05f));
        base.xscale = num;
        if (length < 48f || _controlledDuck.mindControl == null)
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
