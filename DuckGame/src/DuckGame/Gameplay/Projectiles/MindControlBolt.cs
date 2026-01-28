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
        Center = new Vec2(8f, 8f);
        base.Scale = new Vec2(0.1f, 0.1f);
        base.Alpha = 0f;
    }

    public override void Update()
    {
        Vec2 pos = _controlledDuck.Position;
        if (_controlledDuck.ragdoll != null)
        {
            pos = _controlledDuck.ragdoll.part3.Position;
        }
        Vec2 travel = pos - Position;
        float length = travel.Length();
        travel.Normalize();
        base.AngleDegrees = 0f - Maths.PointDirection(Position, pos) + 90f;
        Position += travel * 4f;
        float num = (base.ScaleY = Lerp.Float(base.ScaleX, 1f, 0.05f));
        base.ScaleX = num;
        if (length < 48f || _controlledDuck.mindControl == null)
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
