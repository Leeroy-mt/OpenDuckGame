using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class PointLight : Thing, ILight
{
    public new AutoEffect material;

    private List<LightOccluder> _occluders = new List<LightOccluder>();

    private Color _lightColor;

    private float _range;

#if !MODERN_BATCH
    private GeometryItem _geo;
#else
    ExternalTriangleBatch externalBuffer;
#endif

    private bool _strangeFalloff;

    private Dictionary<Door, bool> _doors = new Dictionary<Door, bool>();

    private Dictionary<VerticalDoor, bool> _verticalDoors = new Dictionary<VerticalDoor, bool>();

    private List<Door> _doorList = new List<Door>();

    private List<VerticalDoor> _verticalDoorList = new List<VerticalDoor>();

    private new bool _initialized;

    private List<Block> _objectsInRange;

    private int fullRefreshCountdown;

    public PointLight(float xpos, float ypos, Color c, float range, List<LightOccluder> occluders = null, bool strangeFalloff = false)
        : base(xpos, ypos)
    {
        base.layer = Layer.Lighting;
        _occluders = occluders;
        _lightColor = c;
        if (_occluders == null)
        {
            _occluders = new List<LightOccluder>();
        }
        _range = range;
        _strangeFalloff = strangeFalloff;
    }

    public override void Initialize()
    {
        if (!NetworkDebugger.enabled)
        {
            Layer.lighting = true;
        }
    }

    public override void Update()
    {
        if (NetworkDebugger.enabled)
        {
            return;
        }
        Layer.lighting = true;
        if (!_initialized)
        {
            DrawLightNew();
            foreach (Door d in Level.CheckCircleAll<Door>(Position, _range * 0.8f))
            {
                if (Level.CheckLine<Block>(Position, d.Position, d) == null || Level.CheckLine<Block>(Position, d.topLeft, d) == null || Level.CheckLine<Block>(Position, d.bottomRight, d) == null)
                {
                    _doors[d] = false;
                    _doorList.Add(d);
                }
            }
            foreach (VerticalDoor d2 in Level.CheckCircleAll<VerticalDoor>(Position, _range * 0.8f))
            {
                if (Level.CheckLine<Block>(Position, d2.Position, d2) == null || Level.CheckLine<Block>(Position, d2.topLeft, d2) == null || Level.CheckLine<Block>(Position, d2.bottomRight, d2) == null)
                {
                    _verticalDoors[d2] = false;
                    _verticalDoorList.Add(d2);
                }
            }
            _initialized = true;
        }
        bool refresh = false;
        foreach (Door door in _doorList)
        {
            if (!_doors[door] && Math.Abs(door._open) > 0.8f)
            {
                _doors[door] = true;
                refresh = true;
            }
            else if (_doors[door] && Math.Abs(door._open) < 0.2f)
            {
                _doors[door] = false;
                refresh = true;
            }
        }
        foreach (VerticalDoor door2 in _verticalDoorList)
        {
            if (!_verticalDoors[door2] && Math.Abs(door2._open) > 0.8f)
            {
                _verticalDoors[door2] = true;
                refresh = true;
            }
            else if (_verticalDoors[door2] && Math.Abs(door2._open) < 0.2f)
            {
                _verticalDoors[door2] = false;
                refresh = true;
            }
        }
        if (fullRefreshCountdown > 0)
        {
            if (fullRefreshCountdown == 1)
            {
                _objectsInRange = null;
                DrawLightNew();
            }
            fullRefreshCountdown--;
        }
#if !MODERN_BATCH
        else if (refresh || _geo == null)
#else
        else if (refresh || externalBuffer == null)
#endif
        {
            DrawLightNew();
        }
    }

    private void DrawLightNew()
    {
        if (NetworkDebugger.enabled)
        {
            return;
        }
#if !MODERN_BATCH
        _geo = MTSpriteBatch.CreateGeometryItem();
#else
        externalBuffer ??= new(256);
        externalBuffer.Clear();
#endif
        Vector2 prevPos = Vector2.Zero;
        Color farColPrev = Color.White;
        bool hasPrev = false;
        if (_objectsInRange == null)
        {
            _objectsInRange = Level.CheckCircleAll<Block>(Position, _range).ToList();
        }
        int loops = 64;
        for (int i = 0; i <= loops; i++)
        {
            Color farColor = Color.Black;
            float a = i / (float)loops * 360f;
            Vector2 dir = new Vector2((float)Math.Cos((double)Maths.DegToRad(a)), -(float)Math.Sin((double)Maths.DegToRad(a)));
            Vector2 rayPos = Vector2.Zero;
            Vector2 castTo = Position + dir * _range;
            if (_strangeFalloff)
            {
                rayPos = castTo;
            }
            else
            {
                rayPos = new Vector2(999999f, 999999f);
                float nearestRay = 9999999f;
                for (int iBlock = 0; iBlock < _objectsInRange.Count; iBlock++)
                {
                    if (!(_objectsInRange[iBlock] is Window) && _objectsInRange[iBlock].solid && Collision.Line(Position, castTo, _objectsInRange[iBlock]))
                    {
                        Vector2 point = Collision.LinePoint(Position, castTo, _objectsInRange[iBlock]);
                        if (point != Vector2.Zero)
                        {
                            float len = (point - Position).LengthSquared();
                            if (len < nearestRay)
                            {
                                rayPos = point;
                                nearestRay = len;
                            }
                        }
                    }
                }
                if (nearestRay > 99999f)
                {
                    rayPos = castTo;
                }
            }
            Color nearColor = _lightColor;
            float lightLength = (rayPos - Position).Length();
            if (_strangeFalloff)
            {
                lightLength += 30f;
            }
            float fade = 0f;
            if (_strangeFalloff)
            {
                float val = Math.Max(lightLength - 30f, 0f) / _range;
                fade = 1f - val;
                fade *= fade;
            }
            else
            {
                fade = 1f - lightLength / _range;
            }
            bool dark = false;
            Color darkOccluder = Color.White;
            foreach (LightOccluder occluder in _occluders)
            {
                if (Collision.LineIntersect(occluder.p1, occluder.p2, Position, rayPos) && (!hasPrev || Collision.LineIntersect(occluder.p1, occluder.p2, Position, prevPos)))
                {
                    Vector3 nc = (nearColor * 0.5f).ToVector3();
                    darkOccluder = occluder.color;
                    nearColor = new Color(nc * occluder.color.ToVector3());
                    dark = true;
                    break;
                }
            }
            farColor = _lightColor * fade;
            if (dark)
            {
                Vector3 nc2 = (farColor * 0.5f).ToVector3();
                farColor = new Color(nc2 * darkOccluder.ToVector3());
            }
            farColor.A = 0;
            nearColor.A = 0;
            if (hasPrev)
            {
                if (!Layer.lightingTwoPointOh)
                {
                    rayPos.X = (float)Math.Round(rayPos.X);
                    rayPos.Y = (float)Math.Round(rayPos.Y);
                }
#if !MODERN_BATCH
                _geo.AddTriangle(Position, rayPos, prevPos, nearColor, farColor, farColPrev);
#else
                externalBuffer.SetTriangle(Position, rayPos, prevPos, nearColor, farColor, farColPrev, 0);
#endif
            }
            hasPrev = true;
            prevPos = rayPos;
            farColPrev = farColor;
        }
    }

    public override void Draw()
    {
#if !MODERN_BATCH
        if (_geo != null)
            Graphics.screen.SubmitGeometry(_geo);
#else
        if (externalBuffer != null)
            Graphics.screen.SubmitExternalBatch(externalBuffer);
#endif
    }

    public void Refresh()
    {
        fullRefreshCountdown = 3;
    }
}