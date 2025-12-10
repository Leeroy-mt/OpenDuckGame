using System;

namespace DuckGame;

public class NetDebugButton : NetDebugElement
{
    private Action _pressAction;

    private Action _holdAction;

    private bool pressing;

    public NetDebugButton(NetDebugInterface pInterface, string pName, Action pPress, Action pHold)
        : base(pInterface)
    {
        _name = pName;
        _pressAction = pPress;
        _holdAction = pHold;
    }

    protected override bool Draw(Vec2 position, bool allowInput)
    {
        bool tookInput = !allowInput;
        position.y -= 2f;
        Vec2 size = new Vec2(100f, 12f);
        width = 100f;
        Rectangle elementRect = new Rectangle(position.x, position.y, size.x, size.y);
        if ((!tookInput && elementRect.Contains(Mouse.positionConsole)) || pressing)
        {
            Graphics.DrawRect(position, position + size, Color.White, depth + 2, filled: false);
            Graphics.DrawRect(position, position + size, Color.White * 0.3f, depth + 1);
            Graphics.DrawString(_name, position + new Vec2(size.x / 2f - Graphics.GetStringWidth(_name) / 2f, 2f), Color.White * 1f, depth + 10);
            if (Mouse.left == InputState.Pressed)
            {
                if (_pressAction != null)
                {
                    _pressAction();
                }
                pressing = true;
                tookInput = true;
            }
            else if (pressing)
            {
                if (_holdAction != null)
                {
                    _holdAction();
                }
                tookInput = true;
            }
        }
        else
        {
            Graphics.DrawRect(position, position + size, Color.White, depth + 2, filled: false);
            Graphics.DrawRect(position, position + size, Color.Black * 0.8f, depth + 1);
            Graphics.DrawString(_name, position + new Vec2(size.x / 2f - Graphics.GetStringWidth(_name) / 2f, 2f), Color.White * 0.8f, depth + 10);
        }
        if (Mouse.left == InputState.Released)
        {
            pressing = false;
        }
        return tookInput;
    }
}
