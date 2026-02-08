using Microsoft.Xna.Framework;
using System.Linq;

namespace DuckGame;

public class NMSkySay : NMEvent
{
    public string _text;

    public Vector2 _spawn;

    public bool _flyLeft;

    public NMSkySay(string pText, Vector2 pSpawn, bool pFlyLeft)
    {
        _text = pText;
        _spawn = pSpawn;
        _flyLeft = pFlyLeft;
    }

    public NMSkySay()
    {
    }

    public override void Activate()
    {
        if (Level.current != null && Level.current.things[typeof(CityBackground)].FirstOrDefault() is CityBackground b)
        {
            b.SkySay(_text, _spawn, _flyLeft);
        }
    }
}
