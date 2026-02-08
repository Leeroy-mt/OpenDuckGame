using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class MainOptions : Thing
{
    private List<string> _options;

    private BitmapFont _font = new BitmapFont("biosFont", 8);

    private float _menuWidth;

    public MainOptions(float xpos, float ypos)
        : base(xpos, ypos)
    {
        base.layer = Layer.HUD;
        _font.Scale = new Vector2(4f, 4f);
        _options = new List<string> { "MULTIPLAYER", "OPTIONS", "QUIT" };
        float longest = 0f;
        foreach (string option in _options)
        {
            float size = _font.GetWidth(option);
            if (size > longest)
            {
                longest = size;
            }
        }
        _menuWidth = longest + 80f;
    }

    public override void Draw()
    {
        Graphics.DrawRect(new Vector2((float)Graphics.width / 2f - _menuWidth / 2f, base.Y), new Vector2((float)Graphics.width / 2f + _menuWidth / 2f, base.Y + 250f), Color.Black, 0.9f);
        int index = 0;
        foreach (string option in _options)
        {
            float size = _font.GetWidth(option);
            _font.Draw(option, (float)Graphics.width / 2f - size / 2f, base.Y + 30f + (float)(index * 60), Color.White);
            index++;
        }
    }
}
