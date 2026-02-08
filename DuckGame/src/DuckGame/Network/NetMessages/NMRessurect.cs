using Microsoft.Xna.Framework;

namespace DuckGame;

public class NMRessurect : NMEvent
{
    public Vector2 position;

    public Duck duck;

    public byte lifeIndex;

    public NMRessurect()
    {
    }

    public NMRessurect(Vector2 pPosition, Duck pDuck, byte pLifeChangeIndex)
    {
        position = pPosition;
        duck = pDuck;
        lifeIndex = pLifeChangeIndex;
    }

    public override void Activate()
    {
        if (duck != null && duck.profile != null && duck.WillAcceptLifeChange(lifeIndex))
        {
            Duck.ResurrectEffect(position);
            duck.ResetNonServerDeathState();
        }
        base.Activate();
    }
}
