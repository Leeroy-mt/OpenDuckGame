using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class LauncherGrenade : PhysicsObject
{
    private List<Vector2> _trail = new List<Vector2>();

    private float _isVolatile = 1f;

    private Vector2 _prevPosition;

    private bool _fade;

    private int _numTrail;

    private float _fadeVal = 1f;

    private bool _blowUp;

    private float _startWait = 0.2f;

    public LauncherGrenade(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("launcherGrenade");
        Center = new Vector2(8f, 8f);
        collisionSize = new Vector2(8f, 6f);
        collisionOffset = new Vector2(-4f, -3f);
        for (int i = 0; i < 17; i++)
        {
            _trail.Add(new Vector2(0f, 0f));
        }
        _prevPosition = Position;
        base.bouncy = 1f;
        friction = 0f;
        _dontCrush = true;
    }

    public override void Initialize()
    {
        if (Level.CheckPoint<Block>(Position) != null)
        {
            _blowUp = true;
        }
        base.Initialize();
    }

    public override void Update()
    {
        if (_fade)
        {
            enablePhysics = false;
        }
        base.Update();
        _startWait -= 0.1f;
        Angle = 0f - Maths.DegToRad(Maths.PointDirection(base.X, base.Y, _prevPosition.X, _prevPosition.Y));
        _isVolatile -= 0.06f;
        for (int i = 15; i >= 0; i--)
        {
            _trail[i + 1] = new Vector2(_trail[i].X, _trail[i].Y);
        }
        if (!_fade)
        {
            _trail[0] = new Vector2(base.X, base.Y);
            _numTrail++;
        }
        else
        {
            _numTrail--;
            _fadeVal -= 0.1f;
            if (_fadeVal <= 0f)
            {
                Level.Remove(this);
            }
        }
        _prevPosition.X = Position.X;
        _prevPosition.Y = Position.Y;
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (_fade || with is Gun || ((with is AutoPlatform || with is Nubber) && vSpeed <= 0f))
        {
            return;
        }
        if (with is PhysicsObject)
        {
            _isVolatile = -1f;
        }
        if (_startWait <= 0f && !_fade && ((base.totalImpactPower > 2f && (_isVolatile <= 0f || !(with is Block))) || _blowUp))
        {
            int i = 0;
            for (int repeat = 0; repeat < 1; repeat++)
            {
                ExplosionPart explosionPart = new ExplosionPart(base.X - 8f + Rando.Float(16f), base.Y - 8f + Rando.Float(16f));
                explosionPart.ScaleX *= 0.7f;
                explosionPart.ScaleY *= 0.7f;
                Level.Add(explosionPart);
                i++;
            }
            SFX.Play("explode");
            RumbleManager.AddRumbleEvent(Position, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium));
            for (i = 0; i < 12; i++)
            {
                float dir = (float)i * 30f - 10f + Rando.Float(20f);
                ATShrapnel shrap = new ATShrapnel();
                shrap.range = 25f + Rando.Float(10f);
                Level.Add(new Bullet(base.X + (float)(Math.Cos(Maths.DegToRad(dir)) * 8.0), base.Y - (float)(Math.Sin(Maths.DegToRad(dir)) * 8.0), shrap, dir)
                {
                    firedFrom = this
                });
            }
            _fade = true;
            base.Y += 10000f;
        }
        else if (!(with is IPlatform))
        {
            if (from == ImpactedFrom.Left || from == ImpactedFrom.Right)
            {
                BounceH();
            }
            if (from == ImpactedFrom.Top || from == ImpactedFrom.Bottom)
            {
                BounceV();
            }
        }
    }

    public override void Draw()
    {
        if (!_fade)
        {
            base.Draw();
        }
        for (int i = 1; i < 16; i++)
        {
            if (i < _numTrail)
            {
                float al = (1f - (float)i / 16f) * _fadeVal * 0.8f;
                Graphics.DrawLine(new Vector2(_trail[i - 1].X, _trail[i - 1].Y), new Vector2(_trail[i].X, _trail[i].Y), Color.White * al);
            }
        }
    }
}
