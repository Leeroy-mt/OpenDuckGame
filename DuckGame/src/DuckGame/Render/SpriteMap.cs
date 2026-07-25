using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class SpriteMap : Sprite, ICloneable
{
#if NO_TEX2D
    static Dictionary<Texture2D, Vector2> textureFramesizes = [];
#endif

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

    private RectangleF _spriteBox;

    public float _frameInc;

    private int _cutWidth;

    private bool _flipFlop = true;

#if !MODERN_BATCH
    private MTSpriteBatchItem _batchItem;
#else
    TriangleBatch.TriangleInfo? triangle0, triangle1;
#endif

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
#if NO_TEX2D
            if (_texture == null || _texture.Width <= 0 || w <= 0)
#else
            if (_texture == null || _texture.width <= 0 || w <= 0)
#endif
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

#if NO_TEX2D
    public SpriteMap(Texture2D tex, int frameWidth, int frameHeight)
#else
    public SpriteMap(Tex2D tex, int frameWidth, int frameHeight)
#endif
    {
        _texture = tex;
#if NO_TEX2D
        frameWidth = Math.Min(_texture.Width, frameWidth);
        frameHeight = Math.Min(_texture.Height, frameHeight);
        SetFrameSize(tex, new(frameWidth, frameHeight));
#else
        frameWidth = Math.Min(_texture.width, frameWidth);
        frameHeight = Math.Min(_texture.height, frameHeight);
        tex.frameWidth = frameWidth;
        tex.frameHeight = frameHeight;
#endif
        Position = new Vector2(X, Y);
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
#if NO_TEX2D
        _texture = Content.Load<Texture2D>(tex);
        frameWidth = Math.Min(_texture.Width, frameWidth);
        frameHeight = Math.Min(_texture.Height, frameHeight);
        SetFrameSize(_texture, new(frameWidth, frameHeight));
#else
        _texture = Content.Load<Tex2D>(tex);
        frameWidth = Math.Min(_texture.width, frameWidth);
        frameHeight = Math.Min(_texture.height, frameHeight);
        _texture.frameWidth = frameWidth;
        _texture.frameHeight = frameHeight;
#endif
        Position = new Vector2(base.X, base.Y);
        _width = frameWidth;
        _height = frameHeight;
        AddDefaultAnimation();
    }

#if NO_TEX2D
    public static Vector2 GetFrameSize(Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture, nameof(texture));
        if (textureFramesizes.TryGetValue(texture, out var vec))
            return vec;
        return new(texture.Width, texture.Height);
    }

    public static void SetFrameSize(Texture2D texture, Vector2 frameSize)
    {
        ArgumentNullException.ThrowIfNull(texture, nameof(texture));
        if (textureFramesizes.TryAdd(texture, frameSize))
            return;
        textureFramesizes[texture] = frameSize;
    }
#endif

    public bool CurrentFrameIsOpaque()
    {
        return false;
    }

    private void AddDefaultAnimation()
    {
        int num = 1;
        if (_width > 0)
        {
#if NO_TEX2D
            num = _texture.Width / _width * (_texture.Height / _height);
#else
            num = _texture.width / _width * (_texture.height / _height);
#endif
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

    public RectangleF GetSpriteBox() =>
        _spriteBox; //new

    public void UpdateSpriteBox()
    {
        if (valid)
        {
#if NO_TEX2D
            int framesPerRow = _texture.Width / w;
#else
            int framesPerRow = _texture.width / w;
#endif
            int currentRow = _imageIndex / framesPerRow;
            int currentColumn = _imageIndex - currentRow * framesPerRow;
            _spriteBox = new RectangleF(currentColumn * w, currentRow * h, w - cutWidth, h);
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
#if !NO_TEX2D
            _texture.currentObjectIndex = _globalIndex;
#endif
            if (w > 0)
            {
                Graphics.Draw(_texture, Position, _spriteBox, _color * base.Alpha, Angle, Center, base.Scale, base.flipH ? SpriteEffects.FlipHorizontally : (base.flipV ? SpriteEffects.FlipVertically : SpriteEffects.None), base.Depth);
            }
        }
    }

    public override void Draw(RectangleF r)
    {
        if (UpdateFrame())
        {
            r.X += _spriteBox.X;
            r.Y += _spriteBox.Y;
#if !NO_TEX2D
            _texture.currentObjectIndex = _globalIndex;
#endif
            Graphics.Draw(_texture, Position, r, _color * base.Alpha, Angle, Center, base.Scale, _flipH ? SpriteEffects.FlipHorizontally : (_flipV ? SpriteEffects.FlipVertically : SpriteEffects.None), base.Depth);
        }
    }

    public void DrawWithoutUpdate()
    {
        if (valid)
        {
#if !NO_TEX2D
            _texture.currentObjectIndex = _globalIndex;
#endif
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
#if !NO_TEX2D
            _texture.currentObjectIndex = _globalIndex;
#endif
            Graphics.Draw(_texture, Position, _spriteBox, _color, Angle, Center, base.Scale, flipH ? SpriteEffects.FlipHorizontally : SpriteEffects.None, base.Depth);
        }
    }

    public void ClearCache()
    {
#if !MODERN_BATCH
        _batchItem = null;
#else
        triangle0 = triangle1 = null;
#endif
    }

    public override void UltraCheapStaticDraw(bool flipH = false)
    {
#if !MODERN_BATCH
        bool cached = _batchItem != null;
#else
        bool cached = triangle0 != null && triangle1 != null;
#endif

        if (!cached)
        {
            if (!valid)
            {
                return;
            }
            UpdateFrame();
            Graphics.recordMetadata = true;
#if !NO_TEX2D
            _texture.currentObjectIndex = _globalIndex;
#endif
            Graphics.Draw(_texture, Position, _spriteBox, _color, Angle, Center, base.Scale, flipH ? SpriteEffects.FlipHorizontally : SpriteEffects.None, base.Depth);
            if (_waitFrames == 1)
            {
#if !MODERN_BATCH
                _batchItem = Graphics.screen.StealLastSpriteBatchItem();
                if (_batchItem.MetaData == null)
                {
                    _batchItem = null;
                }
#else
                triangle0 = Graphics.screen.StealLastTriangle();
                triangle1 = Graphics.screen.StealLastTriangle();
#endif
            }
            _waitFrames++;
            Graphics.recordMetadata = false;
        }
        else
        {
#if !NO_TEX2D
            _texture.currentObjectIndex = _globalIndex;
#endif
#if !MODERN_BATCH
            Graphics.Draw(_batchItem);
#else
            Graphics.Draw(triangle0.Value);
            Graphics.Draw(triangle1.Value);

            if (Recorder.currentRecording != null)
            {
                var t0 = triangle0.Value;
                var t1 = triangle1.Value;

                Recorder.currentRecording.LogDraw(
                    Content.GetTextureIndex(t0.Texture),
                    new Vector2(t0.V0.Position.X, t0.V0.Position.Y),
                    new Vector2(t1.V1.Position.X, t1.V1.Position.Y),
                    Angle,
                    color,
                    (short)_spriteBox.X,
                    (short)_spriteBox.Y,
                    (short)(_spriteBox.Width * (flipH ? -1 : 1)),
                    (short)(_spriteBox.Height * (flipV ? 1 : -1)),
                    Graphics.AdjustDepth(Depth)
                    );
            }
#endif
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
