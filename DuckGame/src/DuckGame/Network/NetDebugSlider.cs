using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

public class NetDebugSlider : NetDebugElement
{
    private Func<float> _getValue;

    private Action<float> _setValue;

    private Func<float, string> _formatter;

    public NetDebugSlider(NetDebugInterface pInterface, string pName, Func<float> pGet, Action<float> pSet, Func<float, string> pDisplayFormatter)
        : base(pInterface)
    {
        _name = pName;
        _getValue = pGet;
        _setValue = pSet;
        _formatter = pDisplayFormatter;
    }

    public void Update()
    {
    }

    protected override bool Draw(Vector2 position, bool allowInput)
    {
        bool tookInput = !allowInput;
        position.X += indent;
        Graphics.DrawString(_name, position, Color.White, depth + 10);
        float val = _getValue();
        int points = 20;
        int level = (int)Math.Round(val * (float)points);
        Rectangle rect = new Rectangle(position.X + 100f, position.Y, points * 5, 8f);
        float hoverLevel = -1f;
        if (rect.Contains(Mouse.positionConsole) && allowInput)
        {
            hoverLevel = (int)((Mouse.positionConsole.X - rect.Left) / rect.width * (float)points);
            if (Mouse.left == InputState.Down)
            {
                _setValue(hoverLevel / (float)points);
                tookInput = true;
            }
        }
        Vector2 barPos = rect.tl;
        for (int i = 0; i < points; i++)
        {
            Color c = Color.Gray;
            if (hoverLevel >= (float)i && hoverLevel != -1f)
            {
                c = Color.White;
            }
            else if (level >= i)
            {
                c = new Color(200, 200, 200);
            }
            Graphics.DrawRect(barPos, barPos + new Vector2(4f, 8f), c, depth + 5);
            barPos.X += 5f;
        }
        barPos.X += 2f;
        Graphics.DrawString("(" + _formatter(val) + ")", barPos, Color.White, depth + 5);
        return tookInput;
    }
}
