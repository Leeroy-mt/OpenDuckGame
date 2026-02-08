using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details")]
public class WaterCooler : MaterialThing, IPlatform
{
    private SpriteMap _sprite;

    private SpriteMap _jugLine;

    private Sprite _bottom;

    private SinWave _colorFlux = 0.1f;

    protected float _fluidLevel = 1f;

    protected int _alternate;

    private List<FluidStream> _holes = new List<FluidStream>();

    protected FluidData _fluid;

    public float _shakeMult;

    private float _shakeInc;

    public WaterCooler(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("waterCoolerJug", 16, 16);
        graphic = _sprite;
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-5f, -5f);
        collisionSize = new Vector2(10f, 10f);
        base.Depth = -0.5f;
        _editorName = "Water Cooler";
        editorTooltip = "Looking for all the latest hot gossip? This is the place to hang.";
        thickness = 2f;
        weight = 5f;
        _jugLine = new SpriteMap("waterCoolerJugLine", 16, 16);
        _jugLine.CenterOrigin();
        flammable = 0.3f;
        _bottom = new Sprite("waterCoolerBottom");
        _bottom.CenterOrigin();
        base.editorOffset = new Vector2(0f, -8f);
        _fluid = Fluid.Water;
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        return true;
    }

    public override bool Hit(Bullet bullet, Vector2 hitPos)
    {
        hitPos += bullet.travelDirNormalized * 2f;
        if (1f - (hitPos.Y - base.top) / (base.bottom - base.top) < _fluidLevel)
        {
            thickness = 2f;
            Vector2 offset = hitPos - Position;
            bool found = false;
            foreach (FluidStream hole in _holes)
            {
                if ((hole.offset - offset).Length() < 2f)
                {
                    hole.offset = offset;
                    hole.holeThickness += 0.5f;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Vector2 holeVec = (-bullet.travelDirNormalized).Rotate(Rando.Float(-0.2f, 0.2f), Vector2.Zero);
                FluidStream newHole = new FluidStream(0f, 0f, holeVec, 1f, offset);
                _holes.Add(newHole);
                newHole.streamSpeedMultiplier = 2f;
            }
            _shakeMult = 1f;
            SFX.Play("bulletHitWater", 1f, Rando.Float(-0.2f, 0.2f));
            return base.Hit(bullet, hitPos);
        }
        thickness = 1f;
        return base.Hit(bullet, hitPos);
    }

    public override void ExitHit(Bullet bullet, Vector2 exitPos)
    {
        exitPos -= bullet.travelDirNormalized * 2f;
        Vector2 offset = exitPos - Position;
        bool found = false;
        foreach (FluidStream hole in _holes)
        {
            if ((hole.offset - offset).Length() < 2f)
            {
                hole.offset = offset;
                hole.holeThickness += 0.5f;
                found = true;
                break;
            }
        }
        if (!found)
        {
            Vector2 holeVec = bullet.travelDirNormalized;
            holeVec = holeVec.Rotate(Rando.Float(-0.2f, 0.2f), Vector2.Zero);
            _holes.Add(new FluidStream(0f, 0f, holeVec, 1f, offset));
        }
    }

    public override void Update()
    {
        base.Update();
        _shakeInc += 0.8f;
        _shakeMult = Lerp.Float(_shakeMult, 0f, 0.05f);
        if (_alternate == 0)
        {
            foreach (FluidStream hole in _holes)
            {
                hole.onFire = base.onFire;
                hole.hSpeed = hSpeed;
                hole.vSpeed = vSpeed;
                hole.DoUpdate();
                hole.Position = Offset(hole.offset);
                hole.sprayAngle = OffsetLocal(hole.startSprayAngle);
                float level = 1f - (hole.offset.Y - base.topLocal) / (base.bottomLocal - base.topLocal);
                if (hole.X > base.left - 2f && hole.X < base.right + 2f && level < _fluidLevel)
                {
                    level = Maths.Clamp(_fluidLevel - level, 0.1f, 1f);
                    float loss = level * 0.0012f * hole.holeThickness;
                    FluidData f = _fluid;
                    f.amount = loss;
                    hole.Feed(f);
                    _fluidLevel -= loss;
                }
            }
        }
        weight = _fluidLevel * 10f;
        _alternate++;
        if (_alternate > 4)
        {
            _alternate = 0;
        }
    }

    public override void Draw()
    {
        _sprite.frame = (int)((1f - _fluidLevel) * 10f);
        Vector2 pos = Position;
        float shakeOffset = (float)Math.Sin(_shakeInc) * _shakeMult * 1f;
        X += shakeOffset;
        base.Draw();
        Position = pos;
        _bottom.Depth = base.Depth + 1;
        Graphics.Draw(_bottom, base.X, base.Y + 9f);
        _jugLine.Depth = base.Depth + 1;
        _jugLine.imageIndex = _sprite.imageIndex;
        _jugLine.Alpha = _fluidLevel * 10f % 1f;
        Graphics.Draw(_jugLine, base.X + shakeOffset, base.Y);
    }
}
