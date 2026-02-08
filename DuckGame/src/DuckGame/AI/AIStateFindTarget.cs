using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class AIStateFindTarget : AIState
{
    private Duck _target;

    private int _refresh;

    private float _targetWait = 1f;

    private float _scatterWait = 1f;

    public override AIState Update(Duck duck, DuckAI ai)
    {
        if (duck.holdObject is Gun g)
        {
            if (g.ammo <= 0)
            {
                duck.ThrowItem();
                return new AIStateWait(1f);
            }
            _refresh++;
            if (_refresh > 10 && ai.canRefresh)
            {
                _refresh = 0;
                _target = null;
                ai.canRefresh = false;
            }
            if (_target == null)
            {
                List<Thing> ducks = Level.current.things[typeof(Duck)].Where((Thing x) => x != duck && !(x as Duck).dead).ToList();
                if (!(AI.Nearest(duck.Position, ducks) is Duck nearest))
                {
                    return new AIStateWait(Rando.Float(0.8f, 1f));
                }
                _target = nearest;
                ai.SetTarget(nearest.Position);
            }
            else
            {
                if ((duck.Position - _target.Position).Length() < 10f)
                {
                    _scatterWait -= 0.01f;
                    if (_scatterWait < 0f)
                    {
                        List<Thing> _nodes = Level.current.things[typeof(PathNode)].ToList();
                        ai.SetTarget(_nodes[Rando.Int(_nodes.Count - 1)].Position);
                        _state.Push(new AIStateWait(1f + Rando.Float(1f)));
                        _scatterWait = 1f;
                    }
                }
                if (Math.Abs(duck.Y - _target.Y) < 16f && Math.Abs(duck.X - _target.X) < 150f && Level.CheckRay<Duck>(duck.Position + new Vector2(duck.offDir * 10, 0f), _target.Position) == _target)
                {
                    if (Level.CheckLine<Block>(duck.Position, _target.Position) == null)
                    {
                        _targetWait -= 0.2f;
                        if (_targetWait <= 0f && Rando.Float(1f) > 0.6f)
                        {
                            ai.Press("SHOOT");
                            _state.Push(new AIStateWait(Rando.Float(0.2f, 0.3f)));
                            return this;
                        }
                    }
                }
                else
                {
                    _targetWait = 1f;
                }
            }
            return this;
        }
        return null;
    }
}
