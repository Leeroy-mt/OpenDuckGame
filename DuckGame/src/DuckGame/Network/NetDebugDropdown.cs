using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class NetDebugDropdown : NetDebugElement
{
    public class Element
    {
        public string name;

        public object value;
    }

    private Func<List<Element>> _elements;

    public Element selected;

    private Sprite _downArrow;

    private bool _dropped;

    public NetDebugDropdown(NetDebugInterface pInterface, string pName, Func<List<Element>> pElements)
        : base(pInterface)
    {
        _name = pName;
        _elements = pElements;
        selected = _elements().FirstOrDefault();
        _downArrow = new Sprite("cloudDown");
        _downArrow.CenterOrigin();
    }

    protected override bool Draw(Vec2 position, bool allowInput)
    {
        bool tookInput = !allowInput;
        position.X += indent;
        Vec2 size = new Vec2(160f, 12f);
        Graphics.DrawString(_name, position, Color.White, depth + 10);
        position.X += 100f;
        position.Y -= 2f;
        width = 280f;
        List<Element> elements = _elements();
        Rectangle dropButton = new Rectangle(position.X + (size.X - size.Y), position.Y, size.Y, size.Y);
        Rectangle fullRect = new Rectangle(position.X, position.Y, size.X, size.Y);
        if (_dropped)
        {
            tookInput = true;
            Rectangle dropList = new Rectangle(position.X, position.Y + size.Y + 4f, size.X, size.Y * (float)elements.Count);
            Graphics.DrawRect(dropList, Color.White, depth + 2, filled: false);
            Graphics.DrawRect(dropList, Color.Black * 0.8f, depth + 1);
            foreach (Element e in elements)
            {
                Rectangle elementRect = new Rectangle(dropList.x, dropList.y, size.X, size.Y);
                if (elementRect.Contains(Mouse.positionConsole))
                {
                    Graphics.DrawRect(elementRect, Color.White * 0.5f, depth + 3);
                    if (Mouse.left == InputState.Pressed)
                    {
                        _dropped = false;
                        selected = e;
                    }
                }
                Graphics.DrawString(e.name, dropList.tl + new Vec2(2f, 2f), Color.White, depth + 5);
                dropList.y += size.Y;
            }
            if (Mouse.right == InputState.Pressed || (Mouse.left == InputState.Pressed && !dropList.Contains(Mouse.positionConsole)))
            {
                _dropped = false;
            }
        }
        bool hoverDropButton = fullRect.Contains(Mouse.positionConsole);
        if (hoverDropButton && Mouse.left == InputState.Pressed && allowInput)
        {
            _dropped = true;
            tookInput = true;
        }
        Graphics.DrawRect(position, position + size, Color.White, depth + 2, filled: false);
        Graphics.DrawRect(position, position + size, Color.Black * 0.8f, depth + 1);
        Graphics.DrawRect(dropButton, Color.White, depth + 6, filled: false);
        Graphics.DrawRect(dropButton, hoverDropButton ? (Color.White * 0.6f) : (Color.Gray * 0.5f), depth + 5);
        Graphics.Draw(_downArrow, dropButton.Center.X, dropButton.Center.Y, depth + 8);
        string selectedName = "-";
        if (selected != null)
        {
            selectedName = selected.name;
        }
        Graphics.DrawString(selectedName, position + new Vec2(4f, 2f), Color.White, depth + 10);
        return tookInput;
    }
}
