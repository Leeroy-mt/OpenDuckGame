using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class DamageHit
{
    public Thing thing;

    public List<Vector2> points = new List<Vector2>();

    public List<DamageType> types = new List<DamageType>();
}
