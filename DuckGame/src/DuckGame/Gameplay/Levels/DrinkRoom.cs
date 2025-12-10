namespace DuckGame;

public class DrinkRoom : Level, IHaveAVirtualTransition
{
    private Level _next;

    private bool _fade;

    public DrinkRoom(Level next)
    {
        transitionSpeedMultiplier = 4f;
        _centeredView = true;
        _next = next;
    }

    public override void Initialize()
    {
        HUD.AddCornerMessage(HUDCorner.BottomRight, "@MENU2@CONTINUE");
        base.Initialize();
    }

    public override void Update()
    {
        if (Input.Pressed("MENU2"))
        {
            _fade = true;
        }
        Graphics.fade = Lerp.Float(Graphics.fade, _fade ? 0f : 1f, 0.1f);
        if (_fade && Graphics.fade < 0.01f)
        {
            Level.current = _next;
        }
        base.Update();
    }

    public override void Draw()
    {
        string text = "";
        float ypos = 12f;
        foreach (Profile p in Profiles.active)
        {
            bool did = false;
            int drinks = Party.GetDrinks(p);
            if (drinks > 0)
            {
                text = p.name + " |WHITE|drinks |RED|" + drinks;
                Graphics.DrawString(text, new Vec2(Layer.HUD.camera.width / 2f - Graphics.GetStringWidth(text) / 2f, ypos), p.persona.colorUsable);
                ypos += 9f;
                did = true;
            }
            foreach (PartyPerks perk in Party.GetPerks(p))
            {
                text = p.name + " |WHITE|gets |GREEN|" + perk;
                Graphics.DrawString(text, new Vec2(Layer.HUD.camera.width / 2f - Graphics.GetStringWidth(text) / 2f, ypos), p.persona.colorUsable);
                ypos += 9f;
                did = true;
            }
            if (did)
            {
                ypos += 9f;
            }
        }
    }
}
