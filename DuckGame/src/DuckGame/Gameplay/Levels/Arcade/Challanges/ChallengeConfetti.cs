using Microsoft.Xna.Framework;

namespace DuckGame;

public class ChallengeConfetti : Thing
{
    private float _fallSpeed;

    private float _horSpeed;

    public ChallengeConfetti(float xpos, float ypos)
        : base(xpos, ypos)
    {
        SpriteMap spr = new SpriteMap("arcade/confetti", 8, 8);
        spr.AddAnimation("idle", 0.1f, true, 0, 1);
        spr.SetAnimation("idle");
        graphic = spr;
        _fallSpeed = Rando.Float(0.5f, 1.2f);
        base.layer = Layer.HUD;
        int num = Rando.ChooseInt(0, 1, 2, 3);
        if (num == 0)
        {
            spr.color = Color.Violet;
        }
        if (num == 1)
        {
            spr.color = Color.SkyBlue;
        }
        if (num == 2)
        {
            spr.color = Color.Wheat;
        }
        if (num == 4)
        {
            spr.color = Color.GreenYellow;
        }
        base.Depth = 1f;
    }

    public override void Update()
    {
        base.Alpha = ArcadeHUD.alphaVal + Chancy.alpha;
        base.Y += _fallSpeed;
        _horSpeed += Rando.Float(-0.1f, 0.1f);
        if (_horSpeed < -0.3f)
        {
            _horSpeed = -0.3f;
        }
        else if (_horSpeed > 0.3f)
        {
            _horSpeed = 0.3f;
        }
        base.X += _horSpeed;
        if (base.Y > 200f)
        {
            Level.Remove(this);
        }
    }
}
