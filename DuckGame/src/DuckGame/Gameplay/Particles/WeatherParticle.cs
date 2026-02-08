using Microsoft.Xna.Framework;

namespace DuckGame;

public abstract class WeatherParticle
{
    public Vector2 position;

    public float z;

    public Vector2 velocity;

    public float zSpeed;

    public float alpha = 1f;

    public bool die;

    public WeatherParticle(Vector2 pos)
    {
        position = pos;
    }

    public abstract void Draw();

    public abstract void Update();
}
