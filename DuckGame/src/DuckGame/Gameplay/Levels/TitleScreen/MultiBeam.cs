using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class MultiBeam : MaterialThing
{
    private Sprite _selectBeam;

    private float _spawnWait;

    private SinWave _wave = 0.016f;

    private SinWave _wave2 = 0.02f;

    private List<BeamDuck> _ducks = new List<BeamDuck>();

    private List<Thing> _guns = new List<Thing>();

    private float _beamHeight = 180f;

    private float _flash;

    private bool _leaveLeft;

    public bool entered;

    public MultiBeam(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _selectBeam = new Sprite("selectBeam");
        _selectBeam.Alpha = 0.9f;
        _selectBeam.Depth = -0.8f;
        _selectBeam.Center = new Vec2(_selectBeam.w / 2, 0f);
        base.Depth = 0.5f;
        _collisionOffset = new Vec2(0f - (float)(_selectBeam.w / 2) * 0.8f, 0f);
        _collisionSize = new Vec2((float)_selectBeam.w * 0.8f, 180f);
        Center = new Vec2(_selectBeam.w / 2);
        base.layer = Layer.Background;
        thickness = 10f;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update()
    {
        _selectBeam.color = new Color(0.3f, 0.3f + _wave2.normalized * 0.2f, 0.5f + _wave.normalized * 0.3f) * (1f + _flash);
        _flash = Maths.CountDown(_flash, 0.1f);
        _spawnWait -= 0.025f;
        if (_spawnWait < 0f)
        {
            Level.Add(new MultiBeamParticle(base.X, base.Y + 190f, -0.8f - _wave.normalized, inverse: false, Color.Cyan * 0.8f));
            Level.Add(new MultiBeamParticle(base.X, base.Y + 190f, -0.8f - _wave2.normalized, inverse: true, Color.LightBlue * 0.8f));
            _spawnWait = 1f;
        }
        foreach (Duck d in Level.CheckRectAll<Duck>(Position - Center, Position - Center + new Vec2(_collisionSize.X, _collisionSize.Y)))
        {
            if (!_ducks.Any((BeamDuck beamDuck) => beamDuck.duck == d))
            {
                float entry = 0f;
                entry = ((!(d.Y < 100f)) ? 130f : 40f);
                SFX.Play("stepInBeam");
                d.immobilized = true;
                d.crouch = false;
                d.sliding = false;
                if (d.holdObject != null)
                {
                    _guns.Add(d.holdObject);
                }
                d.ThrowItem();
                d.solid = false;
                d.grounded = false;
                _ducks.Add(new BeamDuck
                {
                    duck = d,
                    entryHeight = entry,
                    leaving = false,
                    entryDir = ((!(d.X < base.X)) ? 1 : (-1)),
                    sin = new SinWave(0.1f),
                    sin2 = new SinWave(0.05f)
                });
                entered = true;
            }
        }
        foreach (Holdable t in Level.CheckRectAll<Holdable>(Position - Center, Position - Center + new Vec2(_collisionSize.X, _collisionSize.Y)))
        {
            if (t.owner == null && !_guns.Contains(t))
            {
                _guns.Add(t);
            }
        }
        int numDucks = _ducks.Count;
        int duckNum = 0;
        float topOffset = _beamHeight / numDucks / 2 + 20F * ((numDucks > 1) ? 1 : 0);
        float span = (_beamHeight - topOffset * 2) / ((numDucks <= 1) ? 1 : (numDucks - 1));
        for (int i = 0; i < _ducks.Count; i++)
        {
            BeamDuck d2 = _ducks[i];
            if (d2.leaving)
            {
                d2.duck.solid = true;
                d2.duck.hSpeed = (_leaveLeft ? (-4) : 4);
                d2.duck.vSpeed = 0f;
                if (Math.Abs(d2.duck.X - X) > 24)
                {
                    d2.duck.immobilized = false;
                    _ducks.RemoveAt(i);
                    i--;
                    continue;
                }
            }
            else
            {
                d2.duck.X = Lerp.FloatSmooth(d2.duck.X, X + (float)d2.sin2 * 1, 0.2f);
                d2.duck.Y = Lerp.FloatSmooth(d2.duck.Y, topOffset + span * i + (float)d2.sin * 2, 0.08f);
                d2.duck.vSpeed = 0;
                d2.duck.hSpeed = 0;
            }
            if (d2.duck.inputProfile != null)
            {
                if (!TitleScreen.hasMenusOpen && d2.duck.inputProfile.Pressed("LEFT"))
                {
                    d2.leaving = true;
                    _leaveLeft = true;
                    d2.duck.offDir = -1;
                    entered = false;
                }
                else if (!TitleScreen.hasMenusOpen && d2.duck.inputProfile.Pressed("RIGHT"))
                {
                    d2.leaving = true;
                    _leaveLeft = false;
                    d2.duck.offDir = 1;
                    entered = false;
                }
            }
            duckNum++;
        }
        for (int i2 = 0; i2 < _guns.Count; i2++)
        {
            Thing g = _guns[i2];
            g.vSpeed = 0;
            g.hSpeed = 0;
            if (Math.Abs(Position.X - g.Position.X) < 6f)
            {
                g.Position = Vec2.Lerp(g.Position, new Vec2(X, g.Y - 3), 0.1f);
                g.Alpha = Maths.LerpTowards(g.Alpha, 0, 0.1f);
                if (g.Alpha <= 0)
                {
                    g.Y = -200;
                    _guns.RemoveAt(i2);
                    i2--;
                }
            }
            else
            {
                g.Position = Vec2.Lerp(g.Position, new Vec2(X, g.Y), 0.2f);
            }
        }
        base.Update();
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        for (int i = 0; i < 6; i++)
        {
            Level.Add(new GlassParticle(hitPos.X, hitPos.Y, new Vec2(Rando.Float(-1f, 1f), Rando.Float(-1f, 1f))));
        }
        _flash = 1f;
        return true;
    }

    public override void Draw()
    {
        base.Draw();
        _selectBeam.Depth = base.Depth;
        for (int i = 0; i < 6; i++)
        {
            Graphics.Draw(_selectBeam, base.X, base.Y + (float)(i * 32));
        }
    }
}
