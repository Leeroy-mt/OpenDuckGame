namespace DuckGame;

public class DistanceMarker : Thing
{
    private BitmapFont _font;

    private Sprite _distanceSign;

    private int _distance;

    public DistanceMarker(float xpos, float ypos, int dist)
        : base(xpos, ypos)
    {
        _distanceSign = new("distanceSign");
        _distanceSign.CenterOrigin();
        _distance = dist;
        _font = new("biosFont", 8);
        _distanceSign.CenterOrigin();
        collisionOffset = new(-8, -6);
        collisionSize = new(16, 13);
        center = new(_distanceSign.w / 2, _distanceSign.h / 2);
    }

    public override void Draw()
    {
        _distanceSign.depth = depth;
        Graphics.Draw(_distanceSign, x, y);
        string text = Change.ToString(_distance);
        _font.Draw(text, x - _font.GetWidth(text) / 2f, y - _font.height / 2f + 1f, Color.DarkSlateGray, depth + 1);
    }
}
