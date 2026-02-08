using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class SunLight : Thing, ILight
{
    public class Section
    {
        private List<Thing> affectors = new List<Thing>();

        private Dictionary<Door, bool> _doors = new Dictionary<Door, bool>();

        private List<Door> _doorList = new List<Door>();

        private Dictionary<VerticalDoor, bool> _verticalDoors = new Dictionary<VerticalDoor, bool>();

        private List<VerticalDoor> _verticalDoorList = new List<VerticalDoor>();

        public Vector2 start;

        public GeometryItem geo;

        public Color lightColor;

        public void RefreshDoors()
        {
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
            if (refresh)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            affectors.Clear();
            geo = MTSpriteBatch.CreateGeometryItem();
            lightColor.a = 0;
            Vector2 tl = start;
            float dis = 0.25f;
            Vector2 rayOffset = new Vector2(3000f, 5000f);
            for (int i = 0; i < 8; i++)
            {
                Vector2 castPos = tl + new Vector2(32f * dis, 0f);
                tl += new Vector2(8f, 0f);
                Vector2 rayPos = Vector2.Zero;
                Block hit = Level.CheckRay<Block>(castPos, castPos + rayOffset, out rayPos);
                if (hit is Window)
                {
                    hit = null;
                }
                if (hit == null)
                {
                    geo.AddTriangle(castPos, castPos + new Vector2(9f, 0f), castPos + rayOffset, lightColor, lightColor, lightColor);
                    geo.AddTriangle(castPos, castPos + rayOffset, castPos + new Vector2(9f, 0f) + rayOffset, lightColor, lightColor, lightColor);
                    continue;
                }
                if (Level.CheckPoint<Block>(rayPos + new Vector2(0f, -9f) + new Vector2(1f, 0f)) != null)
                {
                    geo.AddTriangle(castPos, castPos + new Vector2(8f, 0f), rayPos + new Vector2(0f, -18f), lightColor, lightColor, lightColor);
                    geo.AddTriangle(castPos, rayPos, rayPos + new Vector2(0f, -18f), lightColor, lightColor, lightColor);
                }
                else
                {
                    geo.AddTriangle(castPos, castPos + new Vector2(12f, 0f), rayPos + new Vector2(8f, 0f), lightColor, lightColor, lightColor);
                    geo.AddTriangle(castPos, rayPos, rayPos + new Vector2(12f, 0f), lightColor, lightColor, lightColor);
                }
                affectors.Add(hit);
                if (hit is Door)
                {
                    _doorList.Add(hit as Door);
                    _doors[hit as Door] = false;
                }
                if (hit is VerticalDoor)
                {
                    _verticalDoorList.Add(hit as VerticalDoor);
                    _verticalDoors[hit as VerticalDoor] = false;
                }
            }
        }

        public bool NeedsRefresh()
        {
            foreach (Thing t in affectors)
            {
                if (t.removeFromLevel || t.level == null)
                {
                    return true;
                }
            }
            return false;
        }
    }

    private Color _lightColor;

    private float _range;

    private bool _strangeFalloff;

    private bool _vertical;

    private Dictionary<Door, bool> _doors = new Dictionary<Door, bool>();

    private List<Door> _doorList = new List<Door>();

    private new bool _initialized;

    private int needsRefresh;

    private List<Section> _sections = new List<Section>();

    public SunLight(float xpos, float ypos, Color c, float range, List<LightOccluder> occluders = null, bool strangeFalloff = false, bool vertical = false)
        : base(xpos, ypos)
    {
        base.layer = Layer.Lighting;
        _lightColor = c;
        _range = range;
        _vertical = vertical;
        _strangeFalloff = strangeFalloff;
    }

    public override void Initialize()
    {
        Layer.lighting = true;
    }

    public override void Update()
    {
        Layer.lighting = true;
        if (!_initialized)
        {
            DrawLight();
            _initialized = true;
        }
        foreach (Section section in _sections)
        {
            section.RefreshDoors();
        }
        if (needsRefresh <= 0)
        {
            return;
        }
        if (needsRefresh == 1)
        {
            foreach (Section s in _sections)
            {
                if (s.NeedsRefresh())
                {
                    s.Refresh();
                }
            }
        }
        needsRefresh--;
    }

    public void Refresh()
    {
        needsRefresh = 3;
    }

    private void DrawLight()
    {
        Vector2 tl = Maths.Snap(Level.current.topLeft, 16f, 16f) + new Vector2(-1024f, -256f);
        for (int i = 0; i < 42; i++)
        {
            Section s = new Section
            {
                start = tl,
                lightColor = _lightColor
            };
            s.Refresh();
            _sections.Add(s);
            tl.X += 64f;
        }
    }

    public override void Draw()
    {
        foreach (Section s in _sections)
        {
            Graphics.screen.SubmitGeometry(s.geo);
        }
    }
}
