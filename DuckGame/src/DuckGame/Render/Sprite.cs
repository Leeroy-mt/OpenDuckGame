using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DuckGame;

public class Sprite : Transform, ICloneable
{
    private int _globalIndex = Thing.GetGlobalIndex();

    protected Tex2D _texture;

    protected RenderTarget2D _renderTexture;

    protected bool _flipH;

    protected bool _flipV;

    public bool moji;

    protected Color _color = Color.White;

    public int globalIndex => _globalIndex;

    public Tex2D texture
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

    public RenderTarget2D renderTexture
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

    public virtual int width => _texture.width;

    public virtual int w => width;

    public virtual int height => _texture.height;

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

    public Sprite(Tex2D tex, float x = 0f, float y = 0f)
    {
        _texture = tex;
        Position = new Vector2(x, y);
    }

    public Sprite(RenderTarget2D tex, float x = 0f, float y = 0f)
    {
        _texture = tex;
        _renderTexture = tex;
        Position = new Vector2(x, y);
    }

    public Sprite(string tex, float x = 0f, float y = 0f)
    {
        _texture = Content.Load<Tex2D>(tex);
        Position = new Vector2(x, y);
    }

    public Sprite(string tex, Vector2 pCenter)
    {
        _texture = Content.Load<Tex2D>(tex);
        Center = pCenter;
    }

    public virtual void Draw()
    {
        _texture.currentObjectIndex = _globalIndex;
        Graphics.Draw(_texture, Position, null, _color * base.Alpha, Angle, Center, base.Scale, _flipH ? SpriteEffects.FlipHorizontally : (_flipV ? SpriteEffects.FlipVertically : SpriteEffects.None), base.Depth);
    }

    public virtual void Draw(Rectangle r)
    {
        _texture.currentObjectIndex = _globalIndex;
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
