using Microsoft.Xna.Framework;

namespace DuckGame;

public class RockWeatherState
{
    public Vector3 add;

    public Vector3 multiply;

    public Vector3 sky;

    public Vector2 sunPos;

    public float lightOpacity;

    public float sunGlow;

    public float sunOpacity = 1f;

    public float rainbowLight;

    public float rainbowLight2;

    public RockWeatherState Copy()
    {
        return new RockWeatherState
        {
            add = add,
            multiply = multiply,
            sky = sky,
            sunPos = sunPos,
            lightOpacity = lightOpacity,
            sunGlow = sunGlow,
            sunOpacity = sunOpacity,
            rainbowLight = rainbowLight,
            rainbowLight2 = rainbowLight2
        };
    }
}
