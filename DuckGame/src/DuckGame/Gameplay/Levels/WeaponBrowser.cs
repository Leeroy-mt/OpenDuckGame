using Microsoft.Xna.Framework;

namespace DuckGame;

public class WeaponBrowser : Level
{
    private BitmapFont _font;

    public override void Initialize()
    {
        Layer.Add(new GridBackground("GRID", 99999));
        _font = new BitmapFont("duckFont", 8);
        _font.Scale = new Vector2(2f, 2f);
        Gun m = new Saxaphone(0f, 0f);
        m.Scale = new Vector2(2f, 2f);
        UIMenu multiplayerMenu = new UIMenu(m.editorName, Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 220f);
        UIBox div = new UIBox(vert: false, isVisible: false);
        UIImage image = new UIImage(m.GetEditorImage(64, 64, transparentBack: true));
        image.collisionSize = new Vector2(64f, 32f);
        div.Add(image);
        UIBox infoBox = new UIBox(vert: true, isVisible: false);
        infoBox.Add(new UIText("AMMO: " + ((m.ammo > 900) ? "INFINITE" : m.ammo.ToString()), Color.White, UIAlign.Left));
        string range = "SHORT";
        if (m.ammoType.range > 150f)
        {
            range = "MEDIUM";
        }
        if (m.ammoType.range > 300f)
        {
            range = "LONG";
        }
        if (m.ammoType.range > 600f)
        {
            range = "EXTREME";
        }
        infoBox.Add(new UIText("RANGE: " + range, Color.White, UIAlign.Left));
        if (m.ammoType.penetration > 0f)
        {
            infoBox.Add(new UIText("PENETRATION: " + m.ammoType.penetration, Color.White, UIAlign.Left));
        }
        else
        {
            infoBox.Add(new UIText("SPECIAL AMMO", Color.White, UIAlign.Left));
        }
        div.Add(infoBox);
        multiplayerMenu.Add(div);
        UIBox descBox = new UIBox(vert: true, isVisible: false);
        descBox.Add(new UIText("---------------------", Color.White));
        float width = 190f;
        string text = m.bio;
        string curText = "";
        string nextWord = "";
        while (true)
        {
            if (text.Length > 0 && text[0] != ' ')
            {
                nextWord += text[0];
            }
            else
            {
                if ((float)((curText.Length + nextWord.Length) * 8) > width)
                {
                    descBox.Add(new UIText(curText, Color.White));
                    curText = "";
                }
                if (curText.Length > 0)
                {
                    curText += " ";
                }
                curText += nextWord;
                nextWord = "";
            }
            if (text.Length == 0)
            {
                break;
            }
            text = text.Remove(0, 1);
        }
        if (nextWord.Length > 0)
        {
            if (curText.Length > 0)
            {
                curText += " ";
            }
            curText += nextWord;
        }
        if (curText.Length > 0)
        {
            descBox.Add(new UIText(curText, Color.White));
            curText = "";
        }
        multiplayerMenu.Add(descBox);
        Level.Add(multiplayerMenu);
    }

    public override void Update()
    {
    }

    public override void Draw()
    {
    }

    public override void PostDrawLayer(Layer layer)
    {
    }
}
