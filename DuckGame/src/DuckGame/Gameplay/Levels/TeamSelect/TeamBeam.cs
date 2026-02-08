using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class TeamBeam : MaterialThing
{
    private Sprite _selectBeam;

    private float _spawnWait;

    private SinWave _wave = 0.016f;

    private SinWave _wave2 = 0.02f;

    private List<BeamDuck> _ducks = new List<BeamDuck>();

    private List<Thing> _guns = new List<Thing>();

    private float _beamHeight = 180f;

    private float _flash;

    private int waitFrames;

    private List<Duck> _networkDucks = new List<Duck>();

    public TeamBeam(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _selectBeam = new Sprite("selectBeam");
        _selectBeam.Alpha = 0.9f;
        _selectBeam.Depth = -0.8f;
        _selectBeam.Center = new Vector2(_selectBeam.w / 2, 0f);
        base.Depth = 0f;
        _collisionOffset = new Vector2(0f - (float)(_selectBeam.w / 2) * 0.8f, 0f);
        _collisionSize = new Vector2((float)_selectBeam.w * 0.8f, 180f);
        Center = new Vector2(_selectBeam.w / 2);
        thickness = 10f;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public void TakeDuck(Duck d)
    {
        if (!_ducks.Any((BeamDuck t) => t.duck == d))
        {
            float entry = 0f;
            entry = ((d.Y < 100f) ? 40f : ((!(d.Y < 150f)) ? 220f : 130f));
            SFX.Play("stepInBeam");
            d.beammode = true;
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
            d.offDir = 1;
            _ducks.Add(new BeamDuck
            {
                duck = d,
                entryHeight = entry,
                leaving = false,
                entryDir = ((!(d.X < base.X)) ? 1 : (-1)),
                sin = new SinWave(0.1f),
                sin2 = new SinWave(0.05f)
            });
            if (_ducks.Count > 0)
            {
                _ = NetworkDebugger.currentIndex;
                _ = 1;
            }
        }
    }

    public void ClearBeam()
    {
        foreach (BeamDuck duck in _ducks)
        {
            duck.leaving = true;
        }
    }

    public void RemoveDuck(Duck duck)
    {
        foreach (BeamDuck d in _ducks)
        {
            if (d.duck == duck)
            {
                d.leaving = true;
            }
        }
    }

    public override void Update()
    {
        if (TeamSelect2.zoomedOut)
        {
            _beamHeight = 270f;
            _collisionSize = new Vector2((float)_selectBeam.w * 0.8f, 270f);
        }
        else
        {
            _beamHeight = 180f;
            _collisionSize = new Vector2((float)_selectBeam.w * 0.8f, 180f);
        }
        _selectBeam.color = new Color(0.3f, 0.3f + _wave2.normalized * 0.2f, 0.5f + _wave.normalized * 0.3f) * (1f + _flash);
        _flash = Maths.CountDown(_flash, 0.1f);
        _spawnWait -= 0.1f;
        if (_spawnWait < 0f)
        {
            Level.Add(new BeamParticle(base.X, base.Y + 290f, -0.8f - _wave.normalized, inverse: false, Color.Cyan * 0.8f));
            Level.Add(new BeamParticle(base.X, base.Y + 290f, -0.8f - _wave2.normalized, inverse: true, Color.LightBlue * 0.8f));
            _spawnWait = 1f;
        }
        waitFrames++;
        if (waitFrames > 5)
        {
            foreach (Duck d in Level.CheckRectAll<Duck>(base.topLeft, base.bottomRight))
            {
                if (d.isServerForObject)
                {
                    TakeDuck(d);
                }
            }
        }
        foreach (Holdable t in Level.CheckRectAll<Holdable>(base.topLeft, base.bottomRight))
        {
            if (!t.isServerForObject)
            {
                continue;
            }
            if (t is RagdollPart)
            {
                Duck d2 = (t as RagdollPart).doll.captureDuck;
                if (d2 != null)
                {
                    (t as RagdollPart).doll.Unragdoll();
                    TakeDuck(d2);
                }
            }
            else if (t.owner == null && !_guns.Contains(t))
            {
                _guns.Add(t);
            }
        }
        int numDucks = _ducks.Count;
        if (Network.isActive)
        {
            foreach (BeamDuck duck in _ducks)
            {
                duck.floatOrder = 0;
            }
            if (Level.current is TeamSelect2 t2)
            {
                int num = 0;
                foreach (ProfileBox2 b in t2._profiles)
                {
                    if (!b.ready || b.duck == null || !(Math.Abs(b.duck.X - base.X) < 16f))
                    {
                        continue;
                    }
                    if (b.duck != null && b.duck.profile != null)
                    {
                        foreach (BeamDuck d3 in _ducks)
                        {
                            if (b.duck.profile.networkIndex < d3.duck.profile.networkIndex)
                            {
                                d3.floatOrder++;
                            }
                        }
                    }
                    num++;
                }
                numDucks = Math.Max(num, numDucks);
            }
        }
        int duckNum = 0;
        float topOffset = _beamHeight / numDucks / 2 + 20 * (float)((numDucks > 1) ? 1 : 0);
        float span = (_beamHeight - topOffset * 2) / ((numDucks <= 1) ? 1 : (numDucks - 1));
        for (int i = 0; i < _ducks.Count; i++)
        {
            BeamDuck d4 = _ducks[i];
            if (d4.duck == null || d4.duck.removeFromLevel || !d4.duck.beammode)
            {
                _ducks.RemoveAt(i);
                i--;
                continue;
            }
            if (d4.leaving)
            {
                d4.duck.solid = true;
                d4.duck.Y = MathHelper.Lerp(d4.duck.Y, d4.entryHeight, 0.35f);
                d4.duck.vSpeed = 0f;
                if (Math.Abs(d4.duck.Y - d4.entryHeight) < 4)
                {
                    d4.duck.Y = d4.entryHeight;
                    d4.duck.hSpeed = d4.entryDir * 3F;
                    d4.duck.vSpeed = 0;
                }
                if (Math.Abs(d4.duck.X - X) > 24)
                {
                    d4.duck.gravMultiplier = 1;
                    d4.duck.immobilized = false;
                    d4.duck.beammode = false;
                    _ducks.RemoveAt(i);
                    i--;
                    continue;
                }
            }
            else
            {
                if (Math.Abs(d4.duck.Position.X - base.X) <= 24f)
                {
                    d4.duck.beammode = true;
                }
                int floatIndex = i;
                if (Network.isActive && d4.duck.profile != null)
                {
                    floatIndex = d4.floatOrder;
                }
                d4.duck.X = Lerp.FloatSmooth(d4.duck.Position.X, Position.X + (float)d4.sin2 * 1f, 0.4f);
                d4.duck.Y = Lerp.FloatSmooth(d4.duck.Position.Y, topOffset + span * (float)floatIndex + (float)d4.sin * 2f, 0.1f);
                d4.duck.vSpeed = 0f;
                d4.duck.hSpeed = 0f;
                d4.duck.gravMultiplier = 0f;
            }
            if (d4.duck.inputProfile != null && d4.duck.inputProfile.Pressed("CANCEL") && Math.Abs(d4.duck.Position.X - base.X) < 2f)
            {
                d4.leaving = true;
            }
            if (d4.duck.profile == null)
            {
                d4.leaving = true;
            }
            if (Network.isActive && d4.duck.profile != null && (d4.duck.profile.connection == null || d4.duck.profile.connection.status == ConnectionStatus.Disconnected))
            {
                d4.leaving = true;
            }
            duckNum++;
        }
        for (int j = 0; j < _guns.Count; j++)
        {
            Thing g = _guns[j];
            g.vSpeed = 0f;
            g.hSpeed = 0f;
            if (Math.Abs(Position.X - g.Position.X) < 6f)
            {
                g.Position = Vector2.Lerp(g.Position, new Vector2(Position.X, g.Position.Y - 3f), 0.1f);
                g.Alpha = Maths.LerpTowards(g.Alpha, 0f, 0.1f);
                if (g.Alpha <= 0f)
                {
                    g.Y = -200f;
                    _guns.RemoveAt(j);
                    j--;
                }
            }
            else
            {
                g.Position = Vector2.Lerp(g.Position, new Vector2(Position.X, g.Position.Y), 0.2f);
            }
        }
        base.Update();
    }

    public override bool Hit(Bullet bullet, Vector2 hitPos)
    {
        for (int i = 0; i < 6; i++)
        {
            Level.Add(new GlassParticle(hitPos.X, hitPos.Y, new Vector2(Rando.Float(-1f, 1f), Rando.Float(-1f, 1f))));
        }
        _flash = 1f;
        if (bullet != null)
        {
            bullet.hitArmor = true;
        }
        return true;
    }

    public override void Draw()
    {
        base.Draw();
        for (int i = 0; i < 10; i++)
        {
            Graphics.Draw(_selectBeam, base.X, base.Y - 32f + (float)(i * 32));
        }
    }
}
