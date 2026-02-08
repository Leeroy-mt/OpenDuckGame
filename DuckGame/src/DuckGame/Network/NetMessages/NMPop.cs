using Microsoft.Xna.Framework;

namespace DuckGame;

public class NMPop : NMEvent
{
    public Vector2 position;

    public NMPop()
    {
    }

    public NMPop(Vector2 pPosition)
    {
        position = pPosition;
    }

    public override void Activate()
    {
        AmazingDisappearingParticles(position);
        base.Activate();
    }

    public static void AmazingDisappearingParticles(Vector2 pPos)
    {
        Level.Add(new PopEffect(pPos.X, pPos.Y));
        for (int i = 0; i < 6; i++)
        {
            Level.Add(ConfettiParticle.New(pPos.X + Rando.Float(-6f, 0f), pPos.Y + Rando.Float(-6f, 6f), new Vector2(Rando.Float(-1f, 0f), Rando.Float(-1f, 1f))));
            Level.Add(ConfettiParticle.New(pPos.X + Rando.Float(0f, 6f), pPos.Y + Rando.Float(-6f, 6f), new Vector2(Rando.Float(0f, 1f), Rando.Float(-1f, 1f))));
            Level.Add(ConfettiParticle.New(pPos.X + Rando.Float(-6f, 0f), pPos.Y + Rando.Float(-6f, 6f), new Vector2(Rando.Float(-1f, 0f), Rando.Float(-1f, 1f)), 0.02f, lineType: true));
            Level.Add(ConfettiParticle.New(pPos.X + Rando.Float(0f, 6f), pPos.Y + Rando.Float(-6f, 6f), new Vector2(Rando.Float(0f, 1f), Rando.Float(-1f, 1f)), 0.02f, lineType: true));
        }
        SFX.Play("balloonPop", 1f, Rando.Float(-0.15f, 0.15f));
    }

    public static void AmazingDisappearingParticles(Vector2 pPos, Layer pLayer)
    {
        Level.Add(new PopEffect(pPos.X, pPos.Y)
        {
            layer = pLayer
        });
        for (int i = 0; i < 6; i++)
        {
            ConfettiParticle confettiParticle = ConfettiParticle.New(pPos.X + Rando.Float(-6f, 0f), pPos.Y + Rando.Float(-6f, 6f), new Vector2(Rando.Float(-1f, 0f), Rando.Float(-1f, 1f)));
            confettiParticle.layer = pLayer;
            Level.Add(confettiParticle);
            ConfettiParticle confettiParticle2 = ConfettiParticle.New(pPos.X + Rando.Float(0f, 6f), pPos.Y + Rando.Float(-6f, 6f), new Vector2(Rando.Float(0f, 1f), Rando.Float(-1f, 1f)));
            confettiParticle2.layer = pLayer;
            Level.Add(confettiParticle2);
            ConfettiParticle confettiParticle3 = ConfettiParticle.New(pPos.X + Rando.Float(-6f, 0f), pPos.Y + Rando.Float(-6f, 6f), new Vector2(Rando.Float(-1f, 0f), Rando.Float(-1f, 1f)), 0.02f, lineType: true);
            confettiParticle3.layer = pLayer;
            Level.Add(confettiParticle3);
            ConfettiParticle confettiParticle4 = ConfettiParticle.New(pPos.X + Rando.Float(0f, 6f), pPos.Y + Rando.Float(-6f, 6f), new Vector2(Rando.Float(0f, 1f), Rando.Float(-1f, 1f)), 0.02f, lineType: true);
            confettiParticle4.layer = pLayer;
            Level.Add(confettiParticle4);
        }
        SFX.Play("balloonPop", 1f, Rando.Float(-0.15f, 0.15f));
    }
}
