using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class SpaceDonut : Thing
{
    #region Private Fields

    float sinInc;

    SpriteMap _donuroid;

    List<Donuroid> _roids = [];

    #endregion

    public SpaceDonut(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("background/donut")
        {
            Depth = -0.9f
        };
        _donuroid = new SpriteMap("background/donuroids", 32, 32);
        _donuroid.CenterOrigin();
        Random generator = new(4562280);
        Random old = Rando.generator;
        Rando.generator = generator;
        Vector2 launch = new(-22, -14);
        Vector2 start = new(130, 120);
        for (int i = 0; i < 20; i++)
        {
            _roids.Add(new Donuroid(start.X + Rando.Float(-6, 6), start.Y + Rando.Float(-18, 18), _donuroid, Rando.Int(0, 7), 1, 1));
            _roids.Add(new Donuroid(start.X + Rando.Float(-6, -1), start.Y + Rando.Float(-10, 0) - 10f, _donuroid, Rando.Int(0, 7), Depth - 20, .5f));
            _roids.Add(new Donuroid(start.X + Rando.Float(6, 1), start.Y + Rando.Float(10, 0) - 10, _donuroid, Rando.Int(0, 7), Depth - 20, .5f));
            _roids.Add(new Donuroid(start.X + Rando.Float(-6, -1), start.Y + Rando.Float(-10, 0) - 20, _donuroid, Rando.Int(0, 7), Depth - 30, .25f));
            _roids.Add(new Donuroid(start.X + Rando.Float(6, 1), start.Y + Rando.Float(10, 0) - 20, _donuroid, Rando.Int(0, 7), Depth - 30, .25f));
            start += launch;
            launch.Y += 1.4f;
        }
        Rando.generator = old;
    }

    public override void Draw()
    {
        sinInc += .02f;
        Graphics.Draw(graphic, X, Y + float.Sin(sinInc) * 2, .9f);
        foreach (Donuroid roid in _roids)
            roid.Draw(Position);
    }
}
