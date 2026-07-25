using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using XnaRenderTarget2D = Microsoft.Xna.Framework.Graphics.RenderTarget2D;

namespace DuckGame;

public class Sprite : Transform, ICloneable
{
    private int _globalIndex = Thing.GetGlobalIndex();

#if NO_TEX2D
    protected Texture2D _texture;
#else
    protected Tex2D _texture;
#endif

#if NO_TEX2D
    protected XnaRenderTarget2D _renderTexture;
#else
    protected RenderTarget2D _renderTexture;
#endif

    protected bool _flipH;

    protected bool _flipV;

    public bool moji;

    protected Color _color = Color.White;

    public int globalIndex => _globalIndex;

#if NO_TEX2D
    public Texture2D texture
#else
    public Tex2D texture
#endif
    {
        get
        {
            return _texture;
        }
        set
        {
            _texture = value;
        }
    }

#if NO_TEX2D
    public XnaRenderTarget2D renderTexture
#else
    public RenderTarget2D renderTexture
#endif
    {
        get
        {
            return _renderTexture;
        }
        set
        {
            _renderTexture = value;
        }
    }

#if NO_TEX2D
    public virtual int width => _texture.Width;
#else
    public virtual int width => _texture.width;
#endif

    public virtual int w => width;

#if NO_TEX2D
    public virtual int height => _texture.Height;
#else
    public virtual int height => _texture.height;
#endif

    public virtual int h => height;

    public bool flipH
    {
        get
        {
            return _flipH;
        }
        set
        {
            _flipH = value;
        }
    }

    public bool flipV
    {
        get
        {
            return _flipV;
        }
        set
        {
            _flipV = value;
        }
    }

    public float flipMultH
    {
        get
        {
            if (!_flipH)
            {
                return 1f;
            }
            return -1f;
        }
    }

    public float flipMultV
    {
        get
        {
            if (!_flipV)
            {
                return 1f;
            }
            return -1f;
        }
    }

    public Color color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
        }
    }

    public void CenterOrigin()
    {
        Center = new Vector2((float)Math.Round((float)width / 2f), (float)Math.Round((float)height / 2f));
    }

    public Sprite()
    {
    }

#if NO_TEX2D
    public Sprite(Texture2D tex, float x = 0f, float y = 0f)
#else
    public Sprite(Tex2D tex, float x = 0f, float y = 0f)
#endif
    {
        _texture = tex;
        Position = new Vector2(x, y);
    }

#if NO_TEX2D
    public Sprite(XnaRenderTarget2D tex, float x = 0f, float y = 0f)
#else
    public Sprite(RenderTarget2D tex, float x = 0f, float y = 0f)
#endif
    {
        _texture = tex;
        _renderTexture = tex;
        Position = new Vector2(x, y);
    }

    public Sprite(string tex, float x = 0f, float y = 0f)
    {
#if NO_TEX2D
        _texture = Content.Load<Texture2D>(tex);
#else
        _texture = Content.Load<Tex2D>(tex);
#endif
        Position = new Vector2(x, y);
    }

    public Sprite(string tex, Vector2 pCenter)
    {
#if NO_TEX2D
        _texture = Content.Load<Texture2D>(tex);
#else
        _texture = Content.Load<Tex2D>(tex);
#endif
        Center = pCenter;
    }

    public virtual void Draw()
    {
#if !NO_TEX2D
        _texture.currentObjectIndex = _globalIndex;
#endif
        Graphics.Draw(_texture, Position, null, _color * base.Alpha, Angle, Center, base.Scale, _flipH ? SpriteEffects.FlipHorizontally : (_flipV ? SpriteEffects.FlipVertically : SpriteEffects.None), base.Depth);
    }

    public virtual void Draw(RectangleF r)
    {
#if !NO_TEX2D
        _texture.currentObjectIndex = _globalIndex;
#endif
        Graphics.Draw(_texture, Position, r, _color * base.Alpha, Angle, Center, base.Scale, _flipH ? SpriteEffects.FlipHorizontally : (_flipV ? SpriteEffects.FlipVertically : SpriteEffects.None), base.Depth);
    }

    public virtual void CheapDraw(bool flipH)
    {
    }

    public virtual Sprite Clone()
    {
        return new Sprite(_texture)
        {
            flipH = _flipH,
            flipV = _flipV,
            Position = Position,
            Scale = base.Scale,
            Center = Center,
            Depth = base.Depth,
            Alpha = base.Alpha,
            Angle = Angle,
            color = color
        };
    }

    public virtual void UltraCheapStaticDraw(bool flipH)
    {
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
}
