using Microsoft.Xna.Framework;

namespace DuckGame;

public class ChainLink : PhysicsObject
{
    public ChainLink(float xpos, float ypos)
    {
        graphic = new Sprite("chainLink");
        Center = new Vector2(3f, 2f);
        _collisionOffset = new Vector2(-2f, -2f);
        _collisionSize = new Vector2(4f, 4f);
        _skipPlatforms = true;
        weight = 0.1f;
        _impactThreshold = 999f;
    }
}
