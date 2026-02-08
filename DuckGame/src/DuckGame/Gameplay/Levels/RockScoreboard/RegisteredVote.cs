using Microsoft.Xna.Framework;

namespace DuckGame;

public class RegisteredVote
{
    public Profile who;

    public VoteType vote = VoteType.None;

    public float slide;

    public bool open = true;

    public bool doClose;

    public float wobble;

    public float wobbleInc;

    public Vector2 leftStick;

    public Vector2 rightStick;
}
