using System;
using System.Collections.Generic;

namespace DuckGame;

public class FluidPuddle : MaterialThing
{
    private WhiteRectangle _lightRect;

    public FluidData data;

    public float _wide;

    public float fluidWave;

    private FluidStream _leftStream;

    private FluidStream _rightStream;

    private BlockCorner _leftCorner;

    private BlockCorner _rightCorner;

    private BlockCorner _topLeftCorner;

    private BlockCorner _topRightCorner;

    private bool _initializedUpperCorners;

    private List<SpriteMap> _surfaceFire = new List<SpriteMap>();

    private Block _block;

    private SpriteMap _lava;

    private SpriteMap _lavaAlternate;

    private int _framesSinceFeed;

    private float _fireID;

    private List<PhysicsObject> _coll;

    private float _fireRise;

    private int bubbleWait;

    public float fireID => _fireID;

    public FluidPuddle(float xpos, float ypos, Block b)
        : base(xpos, ypos)
    {
        _collisionOffset.Y = -4f;
        _collisionSize.Y = 1f;
        _block = b;
        base.Depth = 0.3f;
        flammable = 0.9f;
        base.Alpha = 0f;
        List<BlockCorner> groupCorners = b.GetGroupCorners();
        _coll = new List<PhysicsObject>();
        _leftCorner = null;
        _rightCorner = null;
        foreach (BlockCorner corner in groupCorners)
        {
            if (!(Math.Abs(ypos - corner.corner.Y) < 4f))
            {
                continue;
            }
            if (corner.corner.X > xpos)
            {
                if (_rightCorner == null)
                {
                    _rightCorner = corner;
                }
                else if (corner.corner.X < _rightCorner.corner.X)
                {
                    _rightCorner = corner;
                }
            }
            else if (corner.corner.X < xpos)
            {
                if (_leftCorner == null)
                {
                    _leftCorner = corner;
                }
                else if (corner.corner.X > _leftCorner.corner.X)
                {
                    _leftCorner = corner;
                }
            }
        }
    }

    protected override bool OnBurn(Vec2 firePosition, Thing litBy)
    {
        if (!_onFire && data.flammable > 0.5f)
        {
            _fireID = FireManager.GetFireID();
            SFX.Play("ignite", 1f, -0.3f + Rando.Float(0.3f));
            _onFire = true;
            base.Alpha = 1f;
        }
        return true;
    }

    public override void AddFire()
    {
        SpriteMap surfaceFire = new SpriteMap("surfaceFire", 16, 10);
        surfaceFire.AddAnimation("idle", 0.2f, true, 0, 1, 2, 3);
        surfaceFire.SetAnimation("idle");
        surfaceFire.Center = new Vec2(8f, 10f);
        surfaceFire.frame = Rando.Int(3);
        _surfaceFire.Add(surfaceFire);
    }

    public override void Initialize()
    {
        if (_leftCorner == null || _rightCorner == null)
        {
            Level.Remove(this);
        }
        else
        {
            base.Y = _leftCorner.corner.Y;
        }
    }

    public void Feed(FluidData dat)
    {
        if (_lava == null && dat.sprite != "" && dat.sprite != null)
        {
            if (data.sprite == null)
            {
                data.sprite = dat.sprite;
            }
            _lava = new SpriteMap(dat.sprite, 16, 16);
            _lava.AddAnimation("idle", 0.1f, true, 0, 1, 2, 3);
            _lava.SetAnimation("idle");
            _lava.Center = new Vec2(8f, 10f);
            _lavaAlternate = new SpriteMap(dat.sprite, 16, 16);
            _lavaAlternate.AddAnimation("idle", 0.1f, true, 2, 3, 0, 1);
            _lavaAlternate.SetAnimation("idle");
            _lavaAlternate.Center = new Vec2(8f, 10f);
        }
        if (_lightRect == null && Layer.lighting)
        {
            _lightRect = new WhiteRectangle(base.X, base.Y, base.width, base.height, !(dat.heat > 0f));
            Level.Add(_lightRect);
        }
        if (dat.amount > 0f)
        {
            _framesSinceFeed = 0;
        }
        data.Mix(dat);
        data.amount = Maths.Clamp(data.amount, 0f, MaxFluidFill());
        _wide = FeedAmountToDistance(data.amount);
        float size = _wide + 4f;
        _collisionOffset.X = 0f - size / 2f;
        _collisionSize.X = size;
        FeedEdges();
        if (_leftCorner != null && _rightCorner != null && _wide > _rightCorner.corner.X - _leftCorner.corner.X)
        {
            _wide = _rightCorner.corner.X - _leftCorner.corner.X;
            base.X = _leftCorner.corner.X + (_rightCorner.corner.X - _leftCorner.corner.X) / 2f;
        }
        size = _wide + 4f;
        _collisionOffset.X = 0f - size / 2f;
        _collisionSize.X = size;
        if (!(data.sprite == "water") || _leftCorner == null)
        {
            return;
        }
        for (Block b = _leftCorner.block; b != null; b = b.rightBlock)
        {
            if (b is SnowTileset)
            {
                if (b.left + 2f > base.left && b.right - 2f < base.right)
                {
                    (b as SnowTileset).Freeze();
                }
            }
            else if (b is SnowIceTileset && b.left + 2f > base.left && b.right - 2f < base.right)
            {
                (b as SnowIceTileset).Freeze();
            }
        }
    }

    public float DistanceToFeedAmount(float distance)
    {
        return distance / 600f;
    }

    public float FeedAmountToDistance(float feed)
    {
        return feed * 600f;
    }

    public float MaxFluidFill()
    {
        if (_topLeftCorner != null && _topRightCorner != null)
        {
            float topY = _topLeftCorner.corner.Y + 8f;
            if (_topRightCorner.corner.Y > topY)
            {
                topY = _topRightCorner.corner.Y + 8f;
            }
            return DistanceToFeedAmount((_leftCorner.corner.Y - topY) * _collisionSize.X);
        }
        return 999999f;
    }

    public void FeedEdges()
    {
        if (_rightCorner != null && base.right > _rightCorner.corner.X && _rightCorner.wallCorner)
        {
            base.X -= base.right - _rightCorner.corner.X;
        }
        if (_leftCorner != null && base.left < _leftCorner.corner.X && _leftCorner.wallCorner)
        {
            base.X += _leftCorner.corner.X - base.left;
        }
        if (_rightCorner != null && base.right > _rightCorner.corner.X && !_rightCorner.wallCorner)
        {
            float feedVal = DistanceToFeedAmount(base.right - _rightCorner.corner.X);
            base.X -= (base.right - _rightCorner.corner.X) / 2f;
            if (_rightStream == null)
            {
                _rightStream = new FluidStream(_rightCorner.corner.X - 2f, base.Y, new Vec2(1f, 0f), 1f);
            }
            _rightStream.Y = base.Y - _collisionOffset.Y;
            _rightStream.X = _rightCorner.corner.X + 2f;
            _rightStream.Feed(data.Take(feedVal));
        }
        _wide = FeedAmountToDistance(data.amount);
        float size = _wide + 4f;
        _collisionOffset.X = 0f - size / 2f;
        _collisionSize.X = size;
        if (_leftCorner != null && base.left < _leftCorner.corner.X && !_leftCorner.wallCorner)
        {
            float feedVal2 = DistanceToFeedAmount(_leftCorner.corner.X - base.left);
            base.X += (_leftCorner.corner.X - base.left) / 2f;
            if (_leftStream == null)
            {
                _leftStream = new FluidStream(_leftCorner.corner.X - 2f, base.Y, new Vec2(-1f, 0f), 1f);
            }
            _leftStream.Y = base.Y - _collisionOffset.Y;
            _leftStream.X = _leftCorner.corner.X - 2f;
            _leftStream.Feed(data.Take(feedVal2));
        }
        _wide = FeedAmountToDistance(data.amount);
        size = _wide + 4f;
        _collisionOffset.X = 0f - size / 2f;
        _collisionSize.X = size;
    }

    public float CalculateDepth()
    {
        float num = FeedAmountToDistance(data.amount);
        if (_wide == 0f)
        {
            _wide = 0.001f;
        }
        return Maths.Clamp(num / _wide, 1f, 99999f);
    }

    public void PrepareFloaters()
    {
        if (!(collisionSize.Y > 10f))
        {
            return;
        }
        foreach (PhysicsObject item in Level.CheckLineAll<PhysicsObject>(base.topLeft + new Vec2(0f, -8f), base.topRight + new Vec2(0f, -8f)))
        {
            item.Y = base.top;
            item.DoFloat();
        }
    }

    public override void Update()
    {
        _framesSinceFeed++;
        fluidWave += 0.1f;
        if (data.amount < 0.0001f)
        {
            Level.Remove(this);
        }
        if (collisionSize.Y > 10f)
        {
            bubbleWait++;
            if (bubbleWait > Rando.Int(15, 25))
            {
                for (int i = 0; i < (int)Math.Floor(collisionSize.X / 16f); i++)
                {
                    if (Rando.Float(1f) > 0.85f)
                    {
                        Level.Add(new TinyBubble(base.left + (float)(i * 16) + Rando.Float(-4f, 4f), base.bottom + Rando.Float(-4f), 0f, base.top + 10f));
                    }
                }
                bubbleWait = 0;
            }
            _coll.Clear();
            Level.CheckRectAll(base.topLeft, base.bottomRight, _coll);
            foreach (PhysicsObject item in _coll)
            {
                item.sleeping = false;
            }
        }
        FluidPuddle puddle = Level.CheckLine<FluidPuddle>(new Vec2(base.left, base.Y), new Vec2(base.right, base.Y), this);
        if (puddle != null && puddle.data.amount < data.amount)
        {
            puddle.active = false;
            float leftMost = Math.Min(puddle.left, base.left);
            float rightMost = Math.Max(puddle.right, base.right);
            base.X = leftMost + (rightMost - leftMost) / 2f;
            Feed(puddle.data);
            Level.Remove(puddle);
        }
        if (_leftStream != null)
        {
            _leftStream.Update();
            _leftStream.onFire = base.onFire;
        }
        if (_rightStream != null)
        {
            _rightStream.Update();
            _rightStream.onFire = base.onFire;
        }
        float num = FeedAmountToDistance(data.amount);
        if (_wide == 0f)
        {
            _wide = 0.001f;
        }
        float thick = Maths.Clamp(num / _wide, 1f, 99999f);
        if (base.onFire)
        {
            _fireRise = Lerp.FloatSmooth(_fireRise, 1f, 0.1f, 1.2f);
            if (_framesSinceFeed > 10)
            {
                FluidData dat = data;
                dat.amount = -0.001f;
                Feed(dat);
                if (data.amount <= 0f)
                {
                    data.amount = 0f;
                    base.Alpha = Lerp.Float(base.Alpha, 0f, 0.04f);
                }
                else
                {
                    base.Alpha = Lerp.Float(base.Alpha, 1f, 0.04f);
                }
                if (base.Alpha <= 0f)
                {
                    Level.Remove(this);
                }
            }
        }
        else
        {
            base.Alpha = Lerp.Float(base.Alpha, 1f, 0.04f);
            if (thick < 3f)
            {
                FluidData dat2 = data;
                dat2.amount = -0.0001f;
                Feed(dat2);
            }
        }
        thick = CalculateDepth();
        if (thick > 4f && !_initializedUpperCorners)
        {
            _initializedUpperCorners = true;
            foreach (BlockCorner b in _block.GetGroupCorners())
            {
                if (_leftCorner != null && b.corner.X == _leftCorner.corner.X && b.corner.Y < _leftCorner.corner.Y)
                {
                    if (_topLeftCorner == null)
                    {
                        _topLeftCorner = b;
                    }
                    else if (b.corner.Y > _topLeftCorner.corner.Y)
                    {
                        _topLeftCorner = b;
                    }
                }
                else if (_rightCorner != null && b.corner.X == _rightCorner.corner.X && b.corner.Y < _rightCorner.corner.Y)
                {
                    if (_topRightCorner == null)
                    {
                        _topRightCorner = b;
                    }
                    else if (b.corner.Y > _topRightCorner.corner.Y)
                    {
                        _topRightCorner = b;
                    }
                }
            }
        }
        if (_leftStream != null)
        {
            _leftStream.Y = base.Y - _collisionOffset.Y;
        }
        if (_rightStream != null)
        {
            _rightStream.Y = base.Y - _collisionOffset.Y;
        }
        _collisionOffset.Y = 0f - thick - 1f;
        _collisionSize.Y = thick;
    }

    public override void Draw()
    {
        Graphics.DrawLine(Position + new Vec2(0f - _collisionOffset.X, collisionOffset.Y / 2f + 0.5f), Position + new Vec2(_collisionOffset.X, collisionOffset.Y / 2f + 0.5f), new Color(data.color) * data.transparent, _collisionSize.Y, 0.9f);
        Graphics.DrawLine(Position + new Vec2(0f - _collisionOffset.X, collisionOffset.Y / 2f + 0.5f), Position + new Vec2(_collisionOffset.X, collisionOffset.Y / 2f + 0.5f), new Color(data.color), _collisionSize.Y, -0.99f);
        if (_lightRect != null)
        {
            _lightRect.Position = base.topLeft;
            _lightRect.size = new Vec2(base.width, base.height);
        }
        int val = (int)Math.Ceiling(_collisionSize.X / 16f);
        float inc = _collisionSize.X / (float)val;
        if (_onFire)
        {
            while (_surfaceFire.Count < val)
            {
                AddFire();
            }
            float minus = 0f;
            if (_collisionSize.Y > 2f)
            {
                minus = 2f;
            }
            for (int i = 0; i < val; i++)
            {
                _surfaceFire[i].Alpha = base.Alpha;
                _surfaceFire[i].ScaleY = _fireRise;
                _surfaceFire[i].Depth = base.Depth + 1;
                Graphics.Draw(_surfaceFire[i], base.left + 8f + (float)i * inc, base.Y + _collisionOffset.Y + 1f - minus);
            }
        }
        if (_lava != null && collisionSize.Y > 2f)
        {
            bool alternate = false;
            for (int j = 0; j < val; j++)
            {
                SpriteMap draw = _lava;
                if (alternate)
                {
                    draw = _lavaAlternate;
                }
                draw.Depth = -0.7f;
                draw.Depth += j;
                draw.Alpha = 1f;
                Graphics.DrawWithoutUpdate(draw, (float)Math.Round(base.left + 8f + (float)j * inc), base.Y + _collisionOffset.Y - 4.5f);
                alternate = !alternate;
            }
            _lava.UpdateFrame();
            _lavaAlternate.UpdateFrame();
        }
        base.Draw();
    }

    public override void Terminate()
    {
        if (_lightRect != null)
        {
            Level.Remove(_lightRect);
        }
        base.Terminate();
    }
}
