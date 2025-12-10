using System;

namespace DuckGame;

public class UIDivider : UIComponent
{
    private float _splitPercent;

    private int _splitPixels = -1;

    private UIBox _leftBox;

    private UIBox _rightBox;

    private float _seperation = 1f;

    public UIBox leftSection => _leftBox;

    public UIBox rightSection => _rightBox;

    public UIBox topSection => _leftBox;

    public UIBox bottomSection => _rightBox;

    public UIDivider(bool vert, float splitVal, float sep = 1f)
        : base(0f, 0f, 0f, 0f)
    {
        _vertical = vert;
        _splitPercent = splitVal;
        _leftBox = new UIBox(vert: true, isVisible: false);
        _rightBox = new UIBox(vert: true, isVisible: false);
        Add(_leftBox);
        Add(_rightBox);
        _canFit = true;
        _seperation = sep;
    }

    public UIDivider(bool vert, int splitVal, float sep = 1f)
        : base(0f, 0f, 0f, 0f)
    {
        _vertical = vert;
        _leftBox = new UIBox(vert: true, isVisible: false);
        _rightBox = new UIBox(vert: true, isVisible: false);
        Add(_leftBox);
        Add(_rightBox);
        _splitPixels = splitVal;
        _canFit = true;
        _seperation = sep;
    }

    protected override void SizeChildren()
    {
        if (_vertical)
        {
            Vec2 sizes = CalculateSizes();
            _leftBox.collisionSize = new Vec2(sizes.x, collisionSize.y - borderSize.y * 2f);
            _rightBox.collisionSize = new Vec2(sizes.y, collisionSize.y - borderSize.y * 2f);
        }
        else
        {
            Vec2 sizes2 = CalculateSizes();
            _leftBox.collisionSize = new Vec2(collisionSize.x - borderSize.x * 2f, sizes2.x);
            _rightBox.collisionSize = new Vec2(collisionSize.x - borderSize.x * 2f, sizes2.y);
        }
    }

    public Vec2 CalculateSizes()
    {
        Vec2 colSize = collisionSize;
        if (_vertical)
        {
            colSize.x -= _seperation;
            float leftSize = _leftBox.collisionSize.x;
            float rightSize = _rightBox.collisionSize.x;
            if (_splitPercent != 0f)
            {
                leftSize = colSize.x * _splitPercent;
                rightSize = colSize.x * (1f - _splitPercent);
            }
            else if (_splitPixels > 0)
            {
                leftSize = _splitPixels;
                rightSize = colSize.x - (float)_splitPixels;
            }
            return new Vec2(leftSize, rightSize);
        }
        colSize.y -= _seperation;
        float topSize = _leftBox.collisionSize.y;
        float bottomSize = _rightBox.collisionSize.y;
        if (_splitPercent != 0f)
        {
            topSize = colSize.y * _splitPercent;
            bottomSize = colSize.y * (1f - _splitPercent);
        }
        else if (_splitPixels > 0)
        {
            topSize = _splitPixels;
            bottomSize = colSize.y - (float)_splitPixels;
        }
        return new Vec2(topSize, bottomSize);
    }

    protected override void OnResize()
    {
        if (_vertical)
        {
            _collisionSize.y = Math.Max(_leftBox.collisionSize.y, _rightBox.collisionSize.y);
            float minWidth = _leftBox.collisionSize.x + _rightBox.collisionSize.x + _seperation;
            if (_collisionSize.x < minWidth)
            {
                _collisionSize.x = minWidth;
            }
            Vec2 vec = CalculateSizes();
            float leftSize = vec.x;
            float rightSize = vec.y;
            if (leftSize < _leftBox.collisionSize.x)
            {
                leftSize = _leftBox.collisionSize.x;
            }
            if (rightSize < _rightBox.collisionSize.x)
            {
                rightSize = _rightBox.collisionSize.x;
                leftSize = _collisionSize.x - rightSize;
            }
            float minHeight = Math.Max(_leftBox.collisionSize.y, _rightBox.collisionSize.y);
            if (_collisionSize.y < minHeight)
            {
                _collisionSize.y = minHeight;
            }
            _leftBox.anchor.offset.x = 0f - base.halfWidth + leftSize / 2f;
            _leftBox.anchor.offset.y = 0f;
            _rightBox.anchor.offset.x = base.halfWidth - rightSize / 2f;
            _rightBox.anchor.offset.y = 0f;
        }
        else
        {
            _collisionSize.y = Math.Max(_leftBox.collisionSize.y, _splitPixels) + _rightBox.collisionSize.y;
            float minHeight2 = _leftBox.collisionSize.y + _rightBox.collisionSize.y + _seperation;
            if (_collisionSize.y < minHeight2)
            {
                _collisionSize.y = minHeight2;
            }
            Vec2 vec2 = CalculateSizes();
            float topSize = vec2.x;
            float bottomSize = vec2.y;
            if (topSize < _leftBox.collisionSize.y)
            {
                topSize = _leftBox.collisionSize.y;
            }
            if (bottomSize < _rightBox.collisionSize.y)
            {
                bottomSize = _rightBox.collisionSize.y;
                topSize = _collisionSize.y - bottomSize;
            }
            float minWidth2 = Math.Max(_leftBox.collisionSize.x, _rightBox.collisionSize.x);
            if (_collisionSize.x < minWidth2)
            {
                _collisionSize.x = minWidth2;
            }
            _leftBox.anchor.offset.x = 0f;
            _leftBox.anchor.offset.y = 0f - base.halfHeight + topSize / 2f;
            _rightBox.anchor.offset.x = 0f;
            _rightBox.anchor.offset.y = base.halfHeight - bottomSize / 2f;
        }
    }

    public override void Draw()
    {
        if (!_vertical)
        {
            Vec2 tl = _rightBox.position - new Vec2(_rightBox.width / 2f, _rightBox.height / 2f);
            Vec2 tl2 = _leftBox.position - new Vec2(_leftBox.width / 2f, _leftBox.height / 2f);
            if (tl2.x < tl.x)
            {
                tl.x = tl2.x;
            }
            tl.y -= _seperation / 2f;
            Vec2 br = _rightBox.position + new Vec2(_rightBox.width / 2f, _rightBox.height / 2f);
            Vec2 br2 = _leftBox.position + new Vec2(_leftBox.width / 2f, _leftBox.height / 2f);
            if (br2.x > br.x)
            {
                br.x = br2.x;
            }
            if (_splitPixels == 0)
            {
                _ = _splitPercent;
            }
            else
            {
                _ = _splitPixels;
            }
            Graphics.DrawLine(new Vec2(tl.x, tl.y), new Vec2(br.x, tl.y), Color.White, 1f, base.depth + 10);
        }
        _ = debug;
        base.Draw();
    }
}
