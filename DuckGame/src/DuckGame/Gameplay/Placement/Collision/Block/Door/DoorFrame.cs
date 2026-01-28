namespace DuckGame;

public class DoorFrame : Thing
{
    public DoorFrame(float xpos, float ypos, bool secondaryFrame)
        : base(xpos, ypos)
    {
        graphic = new Sprite(secondaryFrame ? "pyramidDoorFrame" : "doorFrame");
        Center = new Vec2(5f, 26f);
        base.Depth = -0.95f;
        _editorCanModify = false;
    }
}
