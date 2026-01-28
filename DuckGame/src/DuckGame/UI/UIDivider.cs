using System;

namespace DuckGame;

public class UIDivider : UIComponent
{
    #region Private Fields

    int _splitPixels = -1;

    float _splitPercent;

    float _seperation = 1;

    UIBox _leftBox;

    UIBox _rightBox;

    #endregion

    #region Public Properties

    public UIBox leftSection => _leftBox;

    public UIBox rightSection => _rightBox;

    public UIBox topSection => _leftBox;

    public UIBox bottomSection => _rightBox;

    #endregion

    #region Public Constructors

    public UIDivider(bool vert, float splitVal, float sep = 1)
        : base(0, 0, 0, 0)
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

    public UIDivider(bool vert, int splitVal, float sep = 1)
        : base(0, 0, 0, 0)
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

    #endregion

    #region Public Methods

    public Vec2 CalculateSizes()
    {
        Vec2 colSize = collisionSize;
        if (_vertical)
        {
            colSize.X -= _seperation;
            float leftSize = _leftBox.collisionSize.X;
            float rightSize = _rightBox.collisionSize.X;
            if (_splitPercent != 0f)
            {
                leftSize = colSize.X * _splitPercent;
                rightSize = colSize.X * (1f - _splitPercent);
            }
            else if (_splitPixels > 0)
            {
                leftSize = _splitPixels;
                rightSize = colSize.X - (float)_splitPixels;
            }
            return new Vec2(leftSize, rightSize);
        }
        colSize.Y -= _seperation;
        float topSize = _leftBox.collisionSize.Y;
        float bottomSize = _rightBox.collisionSize.Y;
        if (_splitPercent != 0f)
        {
            topSize = colSize.Y * _splitPercent;
            bottomSize = colSize.Y * (1f - _splitPercent);
        }
        else if (_splitPixels > 0)
        {
            topSize = _splitPixels;
            bottomSize = colSize.Y - (float)_splitPixels;
        }
        return new Vec2(topSize, bottomSize);
    }

    public override void Draw()
    {
        if (!_vertical)
        {
            Vec2 tl = _rightBox.Position - new Vec2(_rightBox.width / 2, _rightBox.height / 2);
            Vec2 tl2 = _leftBox.Position - new Vec2(_leftBox.width / 2, _leftBox.height / 2);
            if (tl2.X < tl.X)
                tl.X = tl2.X;
            tl.Y -= _seperation / 2;
            Vec2 br = _rightBox.Position + new Vec2(_rightBox.width / 2, _rightBox.height / 2);
            Vec2 br2 = _leftBox.Position + new Vec2(_leftBox.width / 2, _leftBox.height / 2);
            if (br2.X > br.X)
                br.X = br2.X;
            if (_splitPixels == 0)
                _ = _splitPercent;
            else
                _ = _splitPixels;
            Graphics.DrawLine(new Vec2(tl.X, tl.Y), new Vec2(br.X, tl.Y), Color.White, 1, Depth + 10);
        }
        _ = debug;
        base.Draw();
    }

    #endregion

    #region Protected Methods

    protected override void OnResize()
    {
        if (_vertical)
        {
            _collisionSize.Y = Math.Max(_leftBox.collisionSize.Y, _rightBox.collisionSize.Y);
            float minWidth = _leftBox.collisionSize.X + _rightBox.collisionSize.X + _seperation;
            if (_collisionSize.X < minWidth)
                _collisionSize.X = minWidth;
            Vec2 vec = CalculateSizes();
            float leftSize = vec.X;
            float rightSize = vec.Y;
            if (leftSize < _leftBox.collisionSize.X)
                leftSize = _leftBox.collisionSize.X;
            if (rightSize < _rightBox.collisionSize.X)
            {
                rightSize = _rightBox.collisionSize.X;
                leftSize = _collisionSize.X - rightSize;
            }
            float minHeight = Math.Max(_leftBox.collisionSize.Y, _rightBox.collisionSize.Y);
            if (_collisionSize.Y < minHeight)
                _collisionSize.Y = minHeight;
            _leftBox.anchor.offset.X = 0f - halfWidth + leftSize / 2;
            _leftBox.anchor.offset.Y = 0f;
            _rightBox.anchor.offset.X = halfWidth - rightSize / 2;
            _rightBox.anchor.offset.Y = 0f;
        }
        else
        {
            _collisionSize.Y = Math.Max(_leftBox.collisionSize.Y, _splitPixels) + _rightBox.collisionSize.Y;
            float minHeight2 = _leftBox.collisionSize.Y + _rightBox.collisionSize.Y + _seperation;
            if (_collisionSize.Y < minHeight2)
                _collisionSize.Y = minHeight2;
            Vec2 vec2 = CalculateSizes();
            float topSize = vec2.X;
            float bottomSize = vec2.Y;
            if (topSize < _leftBox.collisionSize.Y)
                topSize = _leftBox.collisionSize.Y;
            if (bottomSize < _rightBox.collisionSize.Y)
            {
                bottomSize = _rightBox.collisionSize.Y;
                topSize = _collisionSize.Y - bottomSize;
            }
            float minWidth2 = Math.Max(_leftBox.collisionSize.X, _rightBox.collisionSize.X);
            if (_collisionSize.X < minWidth2)
                _collisionSize.X = minWidth2;
            _leftBox.anchor.offset.X = 0;
            _leftBox.anchor.offset.Y = 0 - halfHeight + topSize / 2;
            _rightBox.anchor.offset.X = 0;
            _rightBox.anchor.offset.Y = halfHeight - bottomSize / 2;
        }
    }

    protected override void SizeChildren()
    {
        if (_vertical)
        {
            Vec2 sizes = CalculateSizes();
            _leftBox.collisionSize = new Vec2(sizes.X, collisionSize.Y - borderSize.Y * 2);
            _rightBox.collisionSize = new Vec2(sizes.Y, collisionSize.Y - borderSize.Y * 2);
        }
        else
        {
            Vec2 sizes2 = CalculateSizes();
            _leftBox.collisionSize = new Vec2(collisionSize.X - borderSize.X * 2, sizes2.X);
            _rightBox.collisionSize = new Vec2(collisionSize.X - borderSize.X * 2, sizes2.Y);
        }
    }

    #endregion
}
