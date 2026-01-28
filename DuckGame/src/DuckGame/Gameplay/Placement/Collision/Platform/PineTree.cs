using System;
using System.Collections.Generic;

namespace DuckGame;

public abstract class PineTree : AutoPlatform
{
    public bool knocked;

    private float shiftTime;

    private int shiftAmount;

    public float _vertPush;

    public bool edge;

    public bool iterated;

    public int orientation;

    public PineTree leftPine;

    public PineTree rightPine;

    public PineTree(float x, float y, string tileset)
        : base(x, y, tileset)
    {
        _sprite = new SpriteMap(tileset, 8, 16);
        graphic = _sprite;
        collisionSize = new Vec2(8f, 16f);
        thickness = 0.2f;
        base.CenterX = 4f;
        base.CenterY = 8f;
        collisionOffset = new Vec2(-4f, -8f);
        base.Depth = -0.12f;
        placementLayerOverride = Layer.Foreground;
        forceEditorGrid = 8;
        treeLike = true;
    }

    public override void InitializeNeighbors()
    {
        if (!_neighborsInitialized)
        {
            _leftBlock = Level.CheckPoint<PineTree>(base.left - 2f, Position.Y, this);
            _rightBlock = Level.CheckPoint<PineTree>(base.right + 2f, Position.Y, this);
            _upBlock = Level.CheckPoint<PineTree>(Position.X, base.top - 2f, this);
            _downBlock = Level.CheckPoint<PineTree>(Position.X, base.bottom + 2f, this);
            _neighborsInitialized = true;
        }
    }

    public virtual void KnockOffSnow(Vec2 dir, bool vertShake)
    {
        knocked = true;
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        shiftTime = 1f;
        shiftAmount = ((bullet.travelDirNormalized.X > 0f) ? 1 : (-1));
        KnockOffSnow(bullet.travelDirNormalized, vertShake: false);
        return false;
    }

    public override bool HasNoCollision()
    {
        return false;
    }

    public override void UpdatePlatform()
    {
        if (needsRefresh)
        {
            PlaceBlock();
            if ((_sprite.frame == 0 || _sprite.frame == 2 || _sprite.frame == 3 || _sprite.frame == 4) && !_init50)
            {
                edge = true;
            }
            solid = false;
            _init50 = true;
            needsRefresh = false;
        }
        if (!_placed)
        {
            PlaceBlock();
            return;
        }
        if ((_sprite.frame == 0 || _sprite.frame == 2 || _sprite.frame == 3 || _sprite.frame == 4) && !_init50)
        {
            edge = true;
        }
        solid = false;
        _init50 = true;
    }

    public override void UpdateCollision()
    {
    }

    public override void UpdateNubbers()
    {
    }

    public override void Draw()
    {
        base.Depth = -0.12f;
        if (_vertPush > 0f)
        {
            base.Depth = -0.11f;
        }
        if (_graphic != null)
        {
            Sprite sprite = _graphic;
            Vec2 vec = Position;
            _ = shiftAmount;
            sprite.Position = vec + new Vec2(0f * shiftTime, _vertPush * 1.5f);
            _graphic.Alpha = base.Alpha;
            _graphic.Angle = Angle;
            _graphic.Depth = base.Depth;
            _graphic.Scale = base.Scale + new Vec2(Math.Abs((float)shiftAmount * 0f) * shiftTime, _vertPush * 0.2f);
            _graphic.Center = Center;
            _graphic.Draw();
        }
        if (shiftTime > 0f)
        {
            _graphic.Position = Position + new Vec2((float)(shiftAmount * 2) * shiftTime, 0f);
            _graphic.Alpha = base.Alpha;
            _graphic.Angle = Angle;
            _graphic.Depth = base.Depth + 10;
            _graphic.Scale = base.Scale + new Vec2(Math.Abs((float)shiftAmount * 0f) * shiftTime, 0f);
            _graphic.Center = Center;
            _graphic.Alpha = 0.6f;
            _graphic.Draw();
        }
        shiftTime = Lerp.FloatSmooth(shiftTime, 0f, 0.1f);
        if (shiftTime < 0.05f)
        {
            shiftTime = 0f;
        }
        _vertPush = Lerp.FloatSmooth(_vertPush, 0f, 0.3f);
        if (_vertPush < 0.05f)
        {
            _vertPush = 0f;
        }
    }

    public void SpecialFindFrame()
    {
        PineTree up = null;
        PineTree down = null;
        PineTree left = leftPine;
        PineTree right = rightPine;
        up = Level.CheckPoint<PineTree>(base.X, base.Y - 16f, this);
        down = Level.CheckPoint<PineTree>(base.X, base.Y + 16f, this);
        if (up != null && up._tileset != _tileset)
        {
            up = null;
        }
        if (down != null && down._tileset != _tileset)
        {
            down = null;
        }
        if (up != null)
        {
            if (right != null)
            {
                if (down != null)
                {
                    if (left != null)
                    {
                        if (orientation == 0)
                        {
                            if (left.leftPine == null && right.rightPine == null)
                            {
                                frame = 1;
                            }
                            else
                            {
                                frame = 10;
                            }
                        }
                        else if (orientation == -1)
                        {
                            if (left.leftPine == null)
                            {
                                frame = 22;
                            }
                            else
                            {
                                frame = 23;
                            }
                        }
                        else if (orientation == 1)
                        {
                            if (right.rightPine == null)
                            {
                                frame = 26;
                            }
                            else
                            {
                                frame = 25;
                            }
                        }
                    }
                    else
                    {
                        frame = 0;
                    }
                }
                else if (left != null)
                {
                    if (orientation == 0)
                    {
                        if (left.leftPine == null && right.rightPine == null)
                        {
                            frame = 1;
                        }
                        else
                        {
                            frame = 10;
                        }
                    }
                    else if (orientation == -1)
                    {
                        if (left.leftPine == null)
                        {
                            frame = 8;
                        }
                        else
                        {
                            frame = 9;
                        }
                    }
                    else if (orientation == 1)
                    {
                        if (right.rightPine == null)
                        {
                            frame = 12;
                        }
                        else
                        {
                            frame = 11;
                        }
                    }
                }
                else
                {
                    frame = 3;
                }
            }
            else if (down != null)
            {
                if (left != null)
                {
                    frame = 2;
                }
                else
                {
                    frame = 5;
                }
            }
            else if (left != null)
            {
                frame = 4;
            }
            else
            {
                frame = 5;
            }
        }
        else if (right != null)
        {
            if (down != null)
            {
                if (left != null)
                {
                    if (orientation == 0)
                    {
                        if (left.leftPine == null && right.rightPine == null)
                        {
                            frame = 1;
                        }
                        else
                        {
                            frame = 31;
                        }
                    }
                    else if (orientation == -1)
                    {
                        if (left.leftPine == null)
                        {
                            frame = 29;
                        }
                        else
                        {
                            frame = 30;
                        }
                    }
                    else if (orientation == 1)
                    {
                        if (right.rightPine == null)
                        {
                            frame = 33;
                        }
                        else
                        {
                            frame = 32;
                        }
                    }
                }
                else
                {
                    frame = 0;
                }
            }
            else if (left != null)
            {
                if (orientation == 0)
                {
                    if (left.leftPine == null && right.rightPine == null)
                    {
                        frame = 1;
                    }
                    else
                    {
                        frame = 17;
                    }
                }
                else if (orientation == -1)
                {
                    if (left.leftPine == null)
                    {
                        frame = 15;
                    }
                    else
                    {
                        frame = 16;
                    }
                }
                else if (orientation == 1)
                {
                    if (right.rightPine == null)
                    {
                        frame = 19;
                    }
                    else
                    {
                        frame = 18;
                    }
                }
            }
            else
            {
                frame = 3;
            }
        }
        else if (down != null)
        {
            if (left != null)
            {
                frame = 2;
            }
            else
            {
                frame = 5;
            }
        }
        else if (left != null)
        {
            frame = 4;
        }
        else
        {
            frame = 5;
        }
    }

    public override void FindFrame()
    {
        PineTree left = null;
        PineTree right = null;
        left = Level.CheckPoint<PineTree>(base.X - 8f, base.Y, this);
        if (left != null && left._tileset != _tileset)
        {
            left = null;
        }
        right = Level.CheckPoint<PineTree>(base.X + 8f, base.Y, this);
        if (right != null && right._tileset != _tileset)
        {
            right = null;
        }
        if (left != null && right != null)
        {
            left.FindFrame();
        }
        else
        {
            if (left != null)
            {
                return;
            }
            List<PineTree> pineList = new List<PineTree>();
            PineTree currentPine = this;
            leftPine = null;
            while (currentPine != null)
            {
                pineList.Add(currentPine);
                currentPine.rightPine = right;
                if (right == null)
                {
                    break;
                }
                right.leftPine = currentPine;
                currentPine = right;
                right = Level.CheckPoint<PineTree>(right.X + 8f, right.Y, right);
            }
            bool even = pineList.Count % 2 == 0;
            foreach (PineTree p in pineList)
            {
                int div = pineList.Count / 2;
                int idx = pineList.IndexOf(p);
                if (even)
                {
                    if (idx < div)
                    {
                        p.orientation = -1;
                    }
                    else
                    {
                        p.orientation = 1;
                    }
                }
                else if (idx == div)
                {
                    p.orientation = 0;
                }
                else if (idx < div)
                {
                    p.orientation = -1;
                }
                else
                {
                    p.orientation = 1;
                }
                p.SpecialFindFrame();
            }
        }
    }

    public override ContextMenu GetContextMenu()
    {
        return null;
    }
}
