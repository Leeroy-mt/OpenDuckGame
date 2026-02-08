using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

public class ExtinguisherSmoke : PhysicsParticle, ITeleport
{
    private SpriteMap _sprite;

    private SinWaveManualUpdate _moveWave = new SinWaveManualUpdate(Rando.Float(0.15f, 0.17f), Rando.Float(6.28f));

    private SinWaveManualUpdate _moveWave2 = new SinWaveManualUpdate(Rando.Float(0.15f, 0.17f), Rando.Float(6.28f));

    private float _fullScale = Rando.Float(1.1f, 1.5f);

    private int _smokeID;

    private float _orbitInc = Rando.Float(5f);

    private SpriteMap _sprite2;

    private SpriteMap _orbiter;

    private float _rotSpeed = Rando.Float(0.01f, 0.02f);

    private float _distPulseSpeed = Rando.Float(0.01f, 0.02f);

    private float _distPulse = Rando.Float(5f);

    private float s1 = 1f;

    private float s2 = 1f;

    private bool didRemove;

    private float _groundedTime;

    private float lifeTake = 0.05f;

    public int smokeID => _smokeID;

    public ExtinguisherSmoke(float xpos, float ypos, bool network = false)
        : base(xpos, ypos)
    {
        Center = new Vector2(8f, 8f);
        hSpeed = Rando.Float(-0.2f, 0.2f);
        vSpeed = Rando.Float(-0.2f, 0.2f);
        _life += Rando.Float(0.2f);
        base.AngleDegrees = Rando.Float(360f);
        _gravMult = 0.8f;
        _sticky = 0.2f;
        _life = 3f;
        _bounceEfficiency = 0.2f;
        base.ScaleX = (base.ScaleY = Rando.Float(0.4f, 0.5f));
        _smokeID = FireManager.GetFireID();
        _collisionSize = new Vector2(4f, 4f);
        _collisionOffset = new Vector2(-2f, -2f);
        needsSynchronization = true;
        _sprite = new SpriteMap("tinySmokeTestFront", 16, 16);
        int off = Rando.Int(3) * 4;
        _sprite.AddAnimation("idle", 0.1f, true, off);
        _sprite.AddAnimation("puff", Rando.Float(0.15f, 0.25f), false, off, 1 + off, 2 + off, 3 + off);
        _orbiter = new SpriteMap("tinySmokeTestFront", 16, 16);
        off = Rando.Int(3) * 4;
        _orbiter.AddAnimation("idle", 0.1f, true, off);
        _orbiter.AddAnimation("puff", Rando.Float(0.15f, 0.25f), false, off, 1 + off, 2 + off, 3 + off);
        _sprite2 = new SpriteMap("tinySmokeTestBack", 16, 16);
        graphic = _sprite;
        Center = new Vector2(8f, 8f);
        if (Network.isActive && !network)
        {
            GhostManager.context.particleManager.AddLocalParticle(this);
        }
        isLocal = !network;
        _orbitInc += 0.2f;
        _sprite.SetAnimation("idle");
        _sprite.AngleDegrees = Rando.Float(360f);
        _orbiter.AngleDegrees = Rando.Float(360f);
        s1 = Rando.Float(0.8f, 1.1f);
        s2 = Rando.Float(0.8f, 1.1f);
        float lightness = 0.6f - Rando.Float(0.2f);
        lightness = 1f;
        _sprite.color = new Color(lightness, lightness, lightness);
        base.Depth = 0.8f;
        base.Alpha = 1f;
        base.layer = Layer.Game;
        s1 = base.ScaleX;
        s2 = base.ScaleX;
    }

    public override void Removed()
    {
        if (Network.isActive && !didRemove && isLocal && GhostManager.context != null)
        {
            didRemove = true;
            GhostManager.context.particleManager.RemoveParticle(this);
        }
        base.Removed();
    }

    public override void Initialize()
    {
    }

    public override void Update()
    {
        _moveWave.Update();
        _moveWave2.Update();
        _orbitInc += _rotSpeed;
        _distPulse += _distPulseSpeed;
        if (_life < 0.3f)
        {
            float num = (base.ScaleY = Maths.LerpTowards(base.ScaleX, 0.1f, 0.015f));
            base.ScaleX = num;
        }
        else if (_grounded)
        {
            float num = (base.ScaleY = Maths.LerpTowards(base.ScaleX, _fullScale, 0.01f));
            base.ScaleX = num;
        }
        else
        {
            float num = (base.ScaleY = Maths.LerpTowards(base.ScaleX, _fullScale * 0.8f, 0.04f));
            base.ScaleX = num;
        }
        s1 = base.ScaleX;
        s2 = base.ScaleX;
        if (!isLocal)
        {
            base.Update();
            return;
        }
        if (_grounded)
        {
            _groundedTime += 0.01f;
            ExtinguisherSmoke e = Level.CheckCircle<ExtinguisherSmoke>(new Vector2(base.X, base.Y + 4f), 6f);
            if (e != null && _groundedTime < e._groundedTime - 0.1f)
            {
                e.Y -= 0.1f;
            }
        }
        if (_life < 0f && _sprite.currentAnimation != "puff")
        {
            _sprite.SetAnimation("puff");
        }
        if (_sprite.currentAnimation == "puff" && _sprite.finished)
        {
            Level.Remove(this);
        }
        base.Update();
    }

    public override void Draw()
    {
        float distPulse = (float)Math.Sin(_distPulse);
        float xOff = (0f - (float)Math.Sin(_orbitInc) * distPulse) * s1;
        float yOff = (float)Math.Cos(_orbitInc) * distPulse * s1;
        _sprite.imageIndex = _sprite.imageIndex;
        _sprite.Depth = base.Depth;
        _sprite.Scale = new Vector2(s1);
        _sprite.Center = Center;
        Graphics.Draw(_sprite, base.X + xOff, base.Y + yOff);
        _sprite2.imageIndex = _sprite.imageIndex;
        _sprite2.Angle = _sprite.Angle;
        _sprite2.Depth = -0.5f;
        _sprite2.Scale = _sprite.Scale;
        _sprite2.Center = Center;
        float lightness = 0.6f - Rando.Float(0.2f);
        lightness = 0.4f;
        _sprite2.color = new Color(lightness, lightness, lightness);
        Graphics.Draw(_sprite2, base.X + xOff, base.Y + yOff);
        _orbiter.imageIndex = _sprite.imageIndex;
        _orbiter.color = _sprite.color;
        _orbiter.Depth = base.Depth;
        _orbiter.Scale = new Vector2(s2);
        _orbiter.Center = Center;
        Graphics.Draw(_orbiter, base.X - xOff, base.Y - yOff);
        _sprite2.imageIndex = _orbiter.imageIndex;
        _sprite2.Angle = _orbiter.Angle;
        _sprite2.Depth = -0.5f;
        _sprite2.Scale = _orbiter.Scale;
        _sprite2.Center = Center;
        _sprite2.color = new Color(lightness, lightness, lightness);
        Graphics.Draw(_sprite2, base.X - xOff, base.Y - yOff);
    }
}
