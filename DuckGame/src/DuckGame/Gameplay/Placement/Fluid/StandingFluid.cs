using System;

namespace DuckGame;

[EditorGroup("Stuff")]
public class StandingFluid : Thing
{
    public EditorProperty<int> deep = new EditorProperty<int>(1, null, 1f, 100f, 1f);

    public EditorProperty<int> fluidType = new EditorProperty<int>(0, null, 0f, 2f, 1f);

    private Vec2 _prevPos = Vec2.Zero;

    private Vec2 _leftSide;

    private Vec2 _rightSide;

    private float _floor;

    private bool _isValid;

    private bool _filled;

    private int w8;

    public StandingFluid(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _collisionSize = new Vec2(16f, 16f);
        _collisionOffset = new Vec2(-8f, -8f);
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
            Block b = Level.CheckRay<Block>(new Vec2(base.x, base.y), new Vec2(base.x, base.y + 64f));
            if (b != null)
            {
                FluidPuddle p = new FluidPuddle(base.x, b.top, b);
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
            Graphics.Draw(_editorIcon, base.x - 8f, base.y - 8f);
            if (_prevPos != position)
            {
                _isValid = false;
                _prevPos = position;
                if (Level.CheckRay<Block>(position, position - new Vec2(1000f, 0f), out var left) != null && Level.CheckRay<Block>(position, position + new Vec2(1000f, 0f), out var right) != null && Level.CheckRay<Block>(position, position + new Vec2(0f, 64f), out var bottom) != null)
                {
                    _floor = bottom.y;
                    _leftSide = left;
                    _rightSide = right;
                    _isValid = true;
                }
            }
            if (_isValid)
            {
                Graphics.DrawRect(new Vec2(_leftSide.x, _floor - (float)((int)deep * 8)), new Vec2(_rightSide.x, _floor), new Color(GetFluidType().color) * 0.5f, 0.9f);
            }
        }
        base.Draw();
    }
}
