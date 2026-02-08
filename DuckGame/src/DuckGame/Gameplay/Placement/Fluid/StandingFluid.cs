using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

[EditorGroup("Stuff")]
public class StandingFluid : Thing
{
    public EditorProperty<int> deep = new EditorProperty<int>(1, null, 1f, 100f, 1f);

    public EditorProperty<int> fluidType = new EditorProperty<int>(0, null, 0f, 2f, 1f);

    private Vector2 _prevPos = Vector2.Zero;

    private Vector2 _leftSide;

    private Vector2 _rightSide;

    private float _floor;

    private bool _isValid;

    private bool _filled;

    private int w8;

    public StandingFluid(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _collisionSize = new Vector2(16f, 16f);
        _collisionOffset = new Vector2(-8f, -8f);
        _editorIcon = new Sprite("standingFluidIcon");
        _editorName = "Liquid";
        editorTooltip = "Place a liquid near the floor in a contained space and you've got yourself a pool party.";
        base.hugWalls = WallHug.Floor;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    private FluidData GetFluidType()
    {
        return (int)fluidType switch
        {
            0 => Fluid.Water,
            1 => Fluid.Gas,
            2 => Fluid.Lava,
            _ => Fluid.Water,
        };
    }

    public override void Update()
    {
        w8++;
        if (!_filled && w8 > 2)
        {
            _filled = true;
            Block b = Level.CheckRay<Block>(new Vector2(base.X, base.Y), new Vector2(base.X, base.Y + 64f));
            if (b != null)
            {
                FluidPuddle p = new FluidPuddle(base.X, b.top, b);
                Level.Add(p);
                float lastDeep = 0f;
                while (true)
                {
                    float newDeep = p.CalculateDepth();
                    if (newDeep >= (float)((int)deep * 8))
                    {
                        break;
                    }
                    FluidData feed = GetFluidType();
                    feed.amount = 0.5f;
                    p.Feed(feed);
                    newDeep = p.CalculateDepth();
                    if (Math.Abs(lastDeep - newDeep) < 0.001f)
                    {
                        Level.Remove(this);
                        break;
                    }
                    lastDeep = newDeep;
                }
                p.Update();
                p.PrepareFloaters();
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        if (Level.current is Editor)
        {
            Graphics.Draw(_editorIcon, base.X - 8f, base.Y - 8f);
            if (_prevPos != Position)
            {
                _isValid = false;
                _prevPos = Position;
                if (Level.CheckRay<Block>(Position, Position - new Vector2(1000f, 0f), out var left) != null && Level.CheckRay<Block>(Position, Position + new Vector2(1000f, 0f), out var right) != null && Level.CheckRay<Block>(Position, Position + new Vector2(0f, 64f), out var bottom) != null)
                {
                    _floor = bottom.Y;
                    _leftSide = left;
                    _rightSide = right;
                    _isValid = true;
                }
            }
            if (_isValid)
            {
                Graphics.DrawRect(new Vector2(_leftSide.X, _floor - (float)((int)deep * 8)), new Vector2(_rightSide.X, _floor), new Color(GetFluidType().color) * 0.5f, 0.9f);
            }
        }
        base.Draw();
    }
}
