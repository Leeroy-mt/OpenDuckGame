using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class SpriteMap : Sprite, ICloneable
{
    private int _globalIndex = Thing.GetGlobalIndex();

    private int _width;

    private int _height;

    public float _speed = 1f;

    private bool _finished;

    private List<Animation> _animations = new List<Animation>();

    private Animation? _currentAnimation;

    private bool _hasAnimation;

    public int _frame;

    private int _lastFrame = -1;

    public int _imageIndex;

    private int _lastImageIndex = -1;

    private Rectangle _spriteBox;

    public float _frameInc;

    private static Dictionary<string, List<bool>> _transparency = new Dictionary<string, List<bool>>();

    private int _cutWidth;

    private bool _flipFlop = true;

    private MTSpriteBatchItem _batchItem;

    private int _waitFrames;

    public new int globalIndex
    {
        get => _globalIndex;
        set => _globalIndex = value;
    }

    public override int width => _width;

    public override int height => _height;

    public float speed
    {
        get => _speed;
        set => _speed = value;
    }

    public bool finished
    {
        get => _finished;
        set => _finished = value;
    }

    public int frame
    {
        get => _frame;
        set
        {
            SetFrameWithoutReset(value);
            _frameInc = 0;
            _finished = false;
        }
    }

    public int imageIndex
    {
        get => _imageIndex;
        set => _imageIndex = value;
    }

    public int animationIndex
    {
        get
        {
            if (_currentAnimation.HasValue && _currentAnimation.HasValue && _animations.Contains(_currentAnimation.Value))
                return _animations.IndexOf(_currentAnimation.Value);
            return 0;
        }
        set
        {
            if (_animations != null)
                SetAnimation(_animations[value].name);
        }
    }

    private bool valid
    {
        get
        {
            if (_texture == null || _texture.w <= 0 || w <= 0)
                return false;
            return true;
        }
    }

    public string currentAnimation
    {
        get
        {
            if (!_currentAnimation.HasValue)
                return string.Empty;
            return _currentAnimation.Value.name;
        }
        set => SetAnimation(value);
    }

    public int cutWidth
    {
        get => _cutWidth;
        set
        {
            _cutWidth = value;
            UpdateSpriteBox();
        }
    }

    public void SetFrameWithoutReset(int frame)
    {
        _frame = frame;
        if (_currentAnimation.HasValue && valid)
        {
            if (_frame >= _currentAnimation.Value.frames.Length)
                _frame = _currentAnimation.Value.frames.Length - 1;
            _frame = int.Max(_frame, 0);
            _imageIndex = _currentAnimation.Value.frames[_frame];
        }
        else
            _imageIndex = _frame;
    }

    public SpriteMap(Tex2D tex, int frameWidth, int frameHeight)
    {
        _texture = tex;
        frameWidth = Math.Min(_texture.width, frameWidth);
        frameHeight = Math.Min(_texture.height, frameHeight);
        tex.frameWidth = frameWidth;
        tex.frameHeight = frameHeight;
        Position = new Vec2(X, Y);
        _width = frameWidth;
        _height = frameHeight;
        AddDefaultAnimation();
    }

    public SpriteMap(string tex, int frameWidth, int frameHeight, int pFrame)
        : this(tex, frameWidth, frameHeight)
    {
        frame = pFrame;
    }

    public SpriteMap(string tex, int frameWidth, int frameHeight, bool calculateTransparency = false)
    {
        _texture = Content.Load<Tex2D>(tex);
        frameWidth = Math.Min(_texture.width, frameWidth);
        frameHeight = Math.Min(_texture.height, frameHeight);
        _texture.frameWidth = frameWidth;
        _texture.frameHeight = frameHeight;
        Position = new Vec2(base.X, base.Y);
        _width = frameWidth;
        _height = frameHeight;
        AddDefaultAnimation();
    }

    public bool CurrentFrameIsOpaque()
    {
        return false;
    }

    private void AddDefaultAnimation()
    {
        int num = 1;
        if (_width > 0)
        {
            num = _texture.width / _width * (_texture.height / _height);
        }
        int[] frames = new int[num];
        for (int i = 0; i < num; i++)
        {
            frames[i] = i;
        }
        _animations.Add(new Animation("default", 1f, loopVal: true, frames));
        SetAnimation("default");
        _speed = 0f;
    }

    public void AddAnimation(string name, float speed, bool looping, params int[] frames)
    {
        if (!_hasAnimation)
        {
            ClearAnimations();
            _speed = 1f;
        }
        _hasAnimation = true;
        _animations.Add(new Animation(name, speed, looping, frames));
    }

    public void SetAnimation(string name)
    {
        if (_currentAnimation.HasValue && _currentAnimation.Value.name == name)
        {
            return;
        }
        _finished = false;
        foreach (Animation anim in _animations)
        {
            if (anim.name == name)
            {
                _currentAnimation = anim;
                _frameInc = 0f;
                frame = 0;
                return;
            }
        }
        _currentAnimation = null;
    }

    public void ClearAnimations()
    {
        _animations.Clear();
        _currentAnimation = null;
    }

    public void CloneAnimations(SpriteMap into)
    {
        into._animations = new List<Animation>(_animations);
    }

    public Rectangle GetSpriteBox() =>
        _spriteBox; //new

    public void UpdateSpriteBox()
    {
        if (valid)
        {
            int framesPerRow = _texture.width / w;
            int currentRow = _imageIndex / framesPerRow;
            int currentColumn = _imageIndex - currentRow * framesPerRow;
            _spriteBox = new Rectangle(currentColumn * w, currentRow * h, w - cutWidth, h);
            _lastImageIndex = _imageIndex;
        }
    }

    public bool UpdateFrame(bool ignoreFlipFlop = false)
    {
        if (!valid)
        {
            return false;
        }
        if (_currentAnimation.HasValue && (ignoreFlipFlop || _flipFlop != Graphics.frameFlipFlop) && !VirtualTransition.doingVirtualTransition)
        {
            _frameInc += _currentAnimation.Value.speed * _speed;
            if (_frameInc >= 1f)
            {
                _frameInc = 0f;
                _frame++;
            }
            if (_lastFrame != _frame)
            {
                if (_frame >= _currentAnimation.Value.frames.Length)
                {
                    if (_currentAnimation.Value.looping)
                    {
                        frame = 0;
                    }
                    else
                    {
                        frame = _currentAnimation.Value.frames.Length - 1;
                        finished = true;
                    }
                }
                _imageIndex = _currentAnimation.Value.frames[_frame];
                _lastFrame = _frame;
            }
            _flipFlop = !_flipFlop;
        }
        if (_lastImageIndex != _imageIndex)
        {
            UpdateSpriteBox();
        }
        return true;
    }

    public void UpdateFrameSpecial()
    {
        if (!valid)
        {
            return;
        }
        if (_currentAnimation.HasValue && !VirtualTransition.doingVirtualTransition)
        {
            _frameInc += _currentAnimation.Value.speed * _speed;
            if (_frameInc >= 1f)
            {
                _frameInc = 0f;
                _frame++;
            }
            if (_frame >= _currentAnimation.Value.frames.Length)
            {
                if (_currentAnimation.Value.looping)
                {
                    frame = 0;
                }
                else
                {
                    frame = _currentAnimation.Value.frames.Length - 1;
                    finished = true;
                }
            }
            _imageIndex = _currentAnimation.Value.frames[_frame];
        }
        UpdateSpriteBox();
    }

    public override void Draw()
    {
        if (UpdateFrame())
        {
            _texture.currentObjectIndex = _globalIndex;
            if (w > 0)
            {
                Graphics.Draw(_texture, Position, _spriteBox, _color * base.Alpha, Angle, Center, base.Scale, base.flipH ? SpriteEffects.FlipHorizontally : (base.flipV ? SpriteEffects.FlipVertically : SpriteEffects.None), base.Depth);
            }
        }
    }

    public override void Draw(Rectangle r)
    {
        if (UpdateFrame())
        {
            r.x += _spriteBox.x;
            r.y += _spriteBox.y;
            _texture.currentObjectIndex = _globalIndex;
            Graphics.Draw(_texture, Position, r, _color * base.Alpha, Angle, Center, base.Scale, _flipH ? SpriteEffects.FlipHorizontally : (_flipV ? SpriteEffects.FlipVertically : SpriteEffects.None), base.Depth);
        }
    }

    public void DrawWithoutUpdate()
    {
        if (valid)
        {
            _texture.currentObjectIndex = _globalIndex;
            if (w > 0)
            {
                Graphics.Draw(_texture, Position, _spriteBox, _color * base.Alpha, Angle, Center, base.Scale, base.flipH ? SpriteEffects.FlipHorizontally : (base.flipV ? SpriteEffects.FlipVertically : SpriteEffects.None), base.Depth);
            }
        }
    }

    public override void CheapDraw(bool flipH = false)
    {
        if (valid)
        {
            _texture.currentObjectIndex = _globalIndex;
            Graphics.Draw(_texture, Position, _spriteBox, _color, Angle, Center, base.Scale, flipH ? SpriteEffects.FlipHorizontally : SpriteEffects.None, base.Depth);
        }
    }

    public void ClearCache()
    {
        _batchItem = null;
    }

    public override void UltraCheapStaticDraw(bool flipH = false)
    {
        if (_batchItem == null)
        {
            if (!valid)
            {
                return;
            }
            UpdateFrame();
            Graphics.recordMetadata = true;
            _texture.currentObjectIndex = _globalIndex;
            Graphics.Draw(_texture, Position, _spriteBox, _color, Angle, Center, base.Scale, flipH ? SpriteEffects.FlipHorizontally : SpriteEffects.None, base.Depth);
            if (_waitFrames == 1)
            {
                _batchItem = Graphics.screen.StealLastSpriteBatchItem();
                if (_batchItem.MetaData == null)
                {
                    _batchItem = null;
                }
            }
            _waitFrames++;
            Graphics.recordMetadata = false;
        }
        else
        {
            _texture.currentObjectIndex = _globalIndex;
            Graphics.Draw(_batchItem);
        }
    }

    public override Sprite Clone()
    {
        SpriteMap map = new SpriteMap(_texture, _width, _height);
        CloneAnimations(map);
        map.Center = Center;
        map.imageIndex = imageIndex;
        map.frame = frame;
        map._globalIndex = _globalIndex;
        return map;
    }

    public SpriteMap CloneMap()
    {
        return (SpriteMap)Clone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
}
