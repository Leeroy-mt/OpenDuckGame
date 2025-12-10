using System;

namespace DuckGame;

public class KillEvent : Event
{
    private Type _weapon;

    public Type weapon => _weapon;

    public KillEvent(Profile killerVal, Profile killedVal, Type weapon)
        : base(killerVal, killedVal)
    {
        _weapon = weapon;
    }
}
