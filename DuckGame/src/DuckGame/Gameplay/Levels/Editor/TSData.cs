using Microsoft.Xna.Framework;

namespace DuckGame;

public class TSData
{
    public int fingerId;

    public Vector2 touchXY;

    public int diameterX;

    public int diameterY;

    public int rotationAngle;

    public long msTimeElapsed;

    public TSData(int initValue)
    {
        fingerId = initValue;
        touchXY.X = (touchXY.Y = initValue);
        diameterX = initValue;
        diameterY = initValue;
        rotationAngle = initValue;
        msTimeElapsed = initValue;
    }
}
