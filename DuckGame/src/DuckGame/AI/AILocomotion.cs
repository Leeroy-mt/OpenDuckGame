using System;

namespace DuckGame;

public class AILocomotion
{
    private AIPathFinder _path = new AIPathFinder();

    private DuckAI _ai;

    private float _jumpFrames;

    private float _quackFrames;

    private float _slideFrames;

    private int _tryJump = -1;

    public AIPathFinder pathFinder => _path;

    public Vec2 target
    {
        get
        {
            if (_path.target == null)
            {
                return Vec2.Zero;
            }
            return _path.target.link.Position;
        }
        set
        {
            _path.SetTarget(value);
        }
    }

    public void RunLeft()
    {
        _ai.Release("RIGHT");
        _ai.Press("LEFT");
    }

    public void RunRight()
    {
        _ai.Press("RIGHT");
        _ai.Release("LEFT");
    }

    public void Jump(int frames)
    {
        _jumpFrames = frames;
        _ai.Press("JUMP");
    }

    public void Quack(int frames)
    {
        _quackFrames = frames;
        _ai.Press("QUACK");
    }

    public void Slide(int frames)
    {
        _slideFrames = frames;
        _ai.Press("DOWN");
    }

    public void TrimLastTarget()
    {
        if (_path.path != null && _path.path.Count > 0)
        {
            _path.path.RemoveAt(_path.path.Count - 1);
        }
    }

    public void Update(DuckAI ai, Duck duck)
    {
        if (Mouse.right == InputState.Pressed)
        {
            _path.followObject = duck;
            _path.SetTarget(Mouse.positionScreen);
            _path.Refresh();
        }
        _ai = ai;
        _path.followObject = duck;
        if (_jumpFrames == 1f)
        {
            _jumpFrames = 0f;
            _ai.Release("JUMP");
        }
        else if (_jumpFrames > 0f)
        {
            _jumpFrames -= 1f;
        }
        if (_slideFrames == 1f)
        {
            _slideFrames = 0f;
            _ai.Release("DOWN");
        }
        else if (_slideFrames > 0f)
        {
            _slideFrames -= 1f;
        }
        if (_quackFrames == 1f)
        {
            _quackFrames = 0f;
            _ai.Release("QUACK");
        }
        else if (_quackFrames > 0f)
        {
            _quackFrames -= 1f;
        }
        ai.Release("LEFT");
        ai.Release("RIGHT");
        if (_path.path == null || _path.path.Count == 0)
        {
            return;
        }
        Vec2 nextPoint = target;
        Vec2 dist = new Vec2(nextPoint.X - duck.X, nextPoint.Y - duck.Y);
        if (!PathNode.LineIsClear(duck.Position, nextPoint))
        {
            _path.Refresh();
            if (_path.path == null)
            {
                return;
            }
            nextPoint = target;
            dist = new Vec2(nextPoint.X - duck.X, nextPoint.Y - duck.Y);
        }
        if (_path.path == null)
        {
            return;
        }
        if (dist.Y < duck.Y && Math.Abs(dist.Y) > 64f && _path.path.Count > 1)
        {
            _path.Refresh();
            if (_path.path == null)
            {
                return;
            }
            nextPoint = target;
            dist = new Vec2(nextPoint.X - duck.X, nextPoint.Y - duck.Y);
        }
        if (!PathNode.LineIsClear(duck.Position, nextPoint))
        {
            _path.Refresh();
            if (_path.path == null)
            {
                return;
            }
            nextPoint = target;
            dist = new Vec2(nextPoint.X - duck.X, nextPoint.Y - duck.Y);
        }
        if (_path.path == null)
        {
            return;
        }
        bool quickTapJump = false;
        if (_tryJump > 0)
        {
            _tryJump--;
        }
        if (_tryJump == 0 && duck.grounded)
        {
            _path.Refresh();
            if (_path.path == null)
            {
                return;
            }
            nextPoint = target;
            dist = new Vec2(nextPoint.X - duck.X, nextPoint.Y - duck.Y);
            _tryJump = -1;
        }
        if (_path.path == null)
        {
            return;
        }
        float speedMul = 1f;
        if (_path.target.position.Y == target.Y)
        {
            speedMul = 0f;
        }
        if (dist.X < (duck.hSpeed * 3f - 2f) * speedMul)
        {
            if (duck.grounded && Level.CheckLine<Window>(duck.Position, duck.Position + new Vec2(-32f, 0f)) != null)
            {
                Slide(30);
            }
            RunLeft();
        }
        else if (dist.X > (duck.hSpeed * 3f + 2f) * speedMul)
        {
            if (duck.grounded && Level.CheckLine<Window>(duck.Position, duck.Position + new Vec2(32f, 0f)) != null)
            {
                Slide(30);
            }
            RunRight();
        }
        if (_path.peek.gap && duck.grounded)
        {
            Jump((int)(Maths.Clamp(Math.Abs(dist.X), 0f, 48f) / 48f * 16f));
            _tryJump = 5;
        }
        if (dist.Y <= -4f && duck.grounded)
        {
            Jump((int)(Maths.Clamp(Math.Abs(dist.Y), 0f, 48f) / 48f * 16f));
            _tryJump = 5;
        }
        if (quickTapJump)
        {
            ai.Release("JUMP");
        }
        float minYdist = 8f;
        if (Math.Abs(_path.peek.owner.Y - nextPoint.Y) < 8f)
        {
            minYdist = 200f;
        }
        if (Math.Abs(dist.X) < 4f && Math.Abs(dist.Y) < minYdist && PathNode.LineIsClear(duck.Position - new Vec2(0f, 8f), nextPoint) && (!(_path.peek.link.Position.Y < duck.Y - 8f) || duck.grounded) && duck.grounded)
        {
            _path.AtTarget();
            _ai.canRefresh = true;
            _tryJump = -1;
        }
    }
}
