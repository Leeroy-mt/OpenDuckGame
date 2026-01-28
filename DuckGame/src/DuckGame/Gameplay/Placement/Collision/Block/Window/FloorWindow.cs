using System;

namespace DuckGame;

[EditorGroup("Stuff")]
public class FloorWindow : Window
{
    public FloorWindow(float xpos, float ypos)
        : base(xpos, ypos)
    {
        Angle = -(float)Math.PI / 2f;
        collisionSize = new Vec2(32f, 6f);
        collisionOffset = new Vec2(-16f, -2f);
        _editorName = "Floor Window";
        editorTooltip = "When you really want to see what's underneath your house.";
        _editorIcon = new Sprite("windowIconHorizontal");
        Center = new Vec2(2f, 16f);
        base.editorOffset = new Vec2(8f, -6f);
        floor = true;
        UpdateHeight();
    }
}
