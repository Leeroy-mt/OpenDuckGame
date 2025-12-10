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
        _collisionOffset.y = -4f;
        _collisionSize.y = 1f;
        _block = b;
        base.depth = 0.3f;
        flammable = 0.9f;
        base.alpha = 0f;
        List<BlockCorner> groupCorners = b.GetGroupCorners();
        _coll = new List<PhysicsObject>();
        _leftCorner = null;
        _rightCorner = null;
        foreach (BlockCorner corner in groupCorners)
        {
            if (!(Math.Abs(ypos - corner.corner.y) < 4f))
            {
                continue;
            }
            if (corner.corner.x > xpos)
            {
                if (_rightCorner == null)
                {
                    _rightCorner = corner;
                }
                else if (corner.corner.x < _rightCorner.corner.x)
                {
                    _rightCorner = corner;
                }
            }
            else if (corner.corner.x < xpos)
            {
                if (_leftCorner == null)
                {
                    _leftCorner = corner;
                }
                else if (corner.corner.x > _leftCorner.corner.x)
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
            base.alpha = 1f;
        }
        return true;
    }

    public override void AddFire()
    {
        SpriteMap surfaceFire = new SpriteMap("surfaceFire", 16, 10);
        surfaceFire.AddAnimation("idle", 0.2f, true, 0, 1, 2, 3);
        surfaceFire.SetAnimation("idle");
        surfaceFire.center = new Vec2(8f, 10f);
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
            base.y = _leftCorner.corner.y;
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
            _lava.center = new Vec2(8f, 10f);
            _lavaAlternate = new SpriteMap(dat.sprite, 16, 16);
            _lavaAlternate.AddAnimation("idle", 0.1f, true, 2, 3, 0, 1);
            _lavaAlternate.SetAnimation("idle");
            _lavaAlternate.center = new Vec2(8f, 10f);
        }
        if (_lightRect == null && Layer.lighting)
        {
            _lightRect = new WhiteRectangle(base.x, base.y, base.width, base.height, !(dat.heat > 0f));
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
        _collisionOffset.x = 0f - size / 2f;
        _collisionSize.x = size;
        FeedEdges();
        if (_leftCorner != null && _rightCorner != null && _wide > _rightCorner.corner.x - _leftCorner.corner.x)
        {
            _wide = _rightCorner.corner.x - _leftCorner.corner.x;
            base.x = _leftCorner.corner.x + (_rightCorner.corner.x - _leftCorner.corner.x) / 2f;
        }
        size = _wide + 4f;
        _collisionOffset.x = 0f - size / 2f;
        _collisionSize.x = size;
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
            float topY = _topLeftCorner.corner.y + 8f;
            if (_topRightCorner.corner.y > topY)
            {
                topY = _topRightCorner.corner.y + 8f;
            }
            return DistanceToFeedAmount((_leftCorner.corner.y - topY) * _collisionSize.x);
        }
        return 999999f;
    }

    public void FeedEdges()
    {
        if (_rightCorner != null && base.right > _rightCorner.corner.x && _rightCorner.wallCorner)
        {
            base.x -= base.right - _rightCorner.corner.x;
        }
        if (_leftCorner != null && base.left < _leftCorner.corner.x && _leftCorner.wallCorner)
        {
            base.x += _leftCorner.corner.x - base.left;
        }
        if (_rightCorner != null && base.right > _rightCorner.corner.x && !_rightCorner.wallCorner)
        {
            float feedVal = DistanceToFeedAmount(base.right - _rightCorner.corner.x);
            base.x -= (base.right - _rightCorner.corner.x) / 2f;
            if (_rightStream == null)
            {
                _rightStream = new FluidStream(_rightCorner.corner.x - 2f, base.y, new Vec2(1f, 0f), 1f);
            }
            _rightStream.position.y = base.y - _collisionOffset.y;
            _rightStream.position.x = _rightCorner.corner.x + 2f;
            _rightStream.Feed(data.Take(feedVal));
        }
        _wide = FeedAmountToDistance(data.amount);
        float size = _wide + 4f;
        _collisionOffset.x = 0f - size / 2f;
        _collisionSize.x = size;
        if (_leftCorner != null && base.left < _leftCorner.corner.x && !_leftCorner.wallCorner)
        {
            float feedVal2 = DistanceToFeedAmount(_leftCorner.corner.x - base.left);
            base.x += (_leftCorner.corner.x - base.left) / 2f;
            if (_leftStream == null)
            {
                _leftStream = new FluidStream(_leftCorner.corner.x - 2f, base.y, new Vec2(-1f, 0f), 1f);
            }
            _leftStream.position.y = base.y - _collisionOffset.y;
            _leftStream.position.x = _leftCorner.corner.x - 2f;
            _leftStream.Feed(data.Take(feedVal2));
        }
        _wide = FeedAmountToDistance(data.amount);
        size = _wide + 4f;
        _collisionOffset.x = 0f - size / 2f;
        _collisionSize.x = size;
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
        if (!(collisionSize.y > 10f))
        {
            return;
        }
        foreach (PhysicsObject item in Level.CheckLineAll<PhysicsObject>(base.topLeft + new Vec2(0f, -8f), base.topRight + new Vec2(0f, -8f)))
        {
            item.position.y = base.top;
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
        if (collisionSize.y > 10f)
        {
            bubbleWait++;
            if (bubbleWait > Rando.Int(15, 25))
            {
                for (int i = 0; i < (int)Math.Floor(collisionSize.x / 16f); i++)
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
        FluidPuddle puddle = Level.CheckLine<FluidPuddle>(new Vec2(base.left, base.y), new Vec2(base.right, base.y), this);
        if (puddle != null && puddle.data.amount < data.amount)
        {
            puddle.active = false;
            float leftMost = Math.Min(puddle.left, base.left);
            float rightMost = Math.Max(puddle.right, base.right);
            base.x = leftMost + (rightMost - leftMost) / 2f;
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
                    base.alpha = Lerp.Float(base.alpha, 0f, 0.04f);
                }
                else
                {
                    base.alpha = Lerp.Float(base.alpha, 1f, 0.04f);
                }
                if (base.alpha <= 0f)
                {
                    Level.Remove(this);
                }
            }
        }
        else
        {
            base.alpha = Lerp.Float(base.alpha, 1f, 0.04f);
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
                if (_leftCorner != null && b.corner.x == _leftCorner.corner.x && b.corner.y < _leftCorner.corner.y)
                {
                    if (_topLeftCorner == null)
                    {
                        _topLeftCorner = b;
                    }
                    else if (b.corner.y > _topLeftCorner.corner.y)
                    {
                        _topLeftCorner = b;
                    }
                }
                else if (_rightCorner != null && b.corner.x == _rightCorner.corner.x && b.corner.y < _rightCorner.corner.y)
                {
                    if (_topRightCorner == null)
                    {
                        _topRightCorner = b;
                    }
                    else if (b.corner.y > _topRightCorner.corner.y)
                    {
                        _topRightCorner = b;
                    }
                }
            }
        }
        if (_leftStream != null)
        {
            _leftStream.position.y = base.y - _collisionOffset.y;
        }
        if (_rightStream != null)
        {
            _rightStream.position.y = base.y - _collisionOffset.y;
        }
        _collisionOffset.y = 0f - thick - 1f;
        _collisionSize.y = thick;
    }

    public override void Draw()
    {
        Graphics.DrawLine(position + new Vec2(0f - _collisionOffset.x, collisionOffset.y / 2f + 0.5f), position + new Vec2(_collisionOffset.x, collisionOffset.y / 2f + 0.5f), new Color(data.color) * data.transparent, _collisionSize.y, 0.9f);
        Graphics.DrawLine(position + new Vec2(0f - _collisionOffset.x, collisionOffset.y / 2f + 0.5f), position + new Vec2(_collisionOffset.x, collisionOffset.y / 2f + 0.5f), new Color(data.color), _collisionSize.y, -0.99f);
        if (_lightRect != null)
        {
            _lightRect.position = base.topLeft;
            _lightRect.size = new Vec2(base.width, base.height);
        }
        int val = (int)Math.Ceiling(_collisionSize.x / 16f);
        float inc = _collisionSize.x / (float)val;
        if (_onFire)
        {
            while (_surfaceFire.Count < val)
            {
                AddFire();
            }
            float minus = 0f;
            if (_collisionSize.y > 2f)
            {
                minus = 2f;
            }
            for (int i = 0; i < val; i++)
            {
                _surfaceFire[i].alpha = base.alpha;
                _surfaceFire[i].yscale = _fireRise;
                _surfaceFire[i].depth = base.depth + 1;
                Graphics.Draw(_surfaceFire[i], base.left + 8f + (float)i * inc, base.y + _collisionOffset.y + 1f - minus);
            }
        }
        if (_lava != null && collisionSize.y > 2f)
        {
            bool alternate = false;
            for (int j = 0; j < val; j++)
            {
                SpriteMap draw = _lava;
                if (alternate)
                {
                    draw = _lavaAlternate;
                }
                draw.depth = -0.7f;
                draw.depth += j;
                draw.alpha = 1f;
                Graphics.DrawWithoutUpdate(draw, (float)Math.Round(base.left + 8f + (float)j * inc), base.y + _collisionOffset.y - 4.5f);
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
