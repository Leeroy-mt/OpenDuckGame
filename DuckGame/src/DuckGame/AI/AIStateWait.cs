namespace DuckGame;

public class AIStateWait : AIState
{
    private float _wait;

    public AIStateWait(float wait)
    {
        _wait = wait;
    }

    public override AIState Update(Duck duck, DuckAI ai)
    {
        _wait -= 0.016f;
        if (_wait <= 0f)
        {
            return null;
        }
        return this;
    }
}
