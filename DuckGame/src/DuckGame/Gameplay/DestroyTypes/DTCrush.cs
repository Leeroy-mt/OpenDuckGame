namespace DuckGame;

public class DTCrush : DestroyType
{
    private PhysicsObject _thing;

    public new PhysicsObject thing => _thing;

    public DTCrush(PhysicsObject t)
        : base(t)
    {
        _thing = t;
    }
}
