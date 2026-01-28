using System;

namespace DuckGame;

public class Camera
{
    protected Matrix _matrix;

    protected bool _dirty = true;

    protected Vec2 _position;

    protected Vec2 _size = new Vec2(320f, 320f * Graphics.aspect);

    protected Vec2 _zoomPoint = new Vec2(0f, 0f);

    public bool skipUpdate;

    private Rectangle _rectangle;

    public Vec2 _viewSize;

    public Vec2 position
    {
        get
        {
            return _position;
        }
        set
        {
            if (_position != value)
            {
                _position = value;
                _dirty = true;
            }
        }
    }

    public float x
    {
        get
        {
            return _position.X;
        }
        set
        {
            if (_position.X != value)
            {
                _position.X = value;
                _dirty = true;
            }
        }
    }

    public float y
    {
        get
        {
            return _position.Y;
        }
        set
        {
            if (_position.Y != value)
            {
                _position.Y = value;
                _dirty = true;
            }
        }
    }

    public Vec2 center
    {
        get
        {
            return new Vec2(centerX, centerY);
        }
        set
        {
            centerX = value.X;
            centerY = value.Y;
        }
    }

    public float centerX
    {
        get
        {
            return _position.X + width / 2f;
        }
        set
        {
            if (centerX != value)
            {
                _position.X = value - width / 2f;
                _dirty = true;
            }
        }
    }

    public float centerY
    {
        get
        {
            return _position.Y + height / 2f;
        }
        set
        {
            if (centerY != value)
            {
                _position.Y = value - height / 2f;
                _dirty = true;
            }
        }
    }

    public float aspect => width / height;

    public virtual bool sixteenNine => Math.Abs(aspect - 1.7777778f) < 0.02f;

    public float top => y;

    public float bottom => y + height;

    public float left => x;

    public float right => x + width;

    public Vec2 size
    {
        get
        {
            return _size;
        }
        set
        {
            _size = value;
            _dirty = true;
        }
    }

    public float width
    {
        get
        {
            return _size.X;
        }
        set
        {
            if (_size.X != value)
            {
                _size.X = value;
                _dirty = true;
            }
        }
    }

    public float height
    {
        get
        {
            return _size.Y;
        }
        set
        {
            if (_size.Y != value)
            {
                _size.Y = value;
                _dirty = true;
            }
        }
    }

    public Vec2 zoomPoint
    {
        get
        {
            return _zoomPoint;
        }
        set
        {
            _zoomPoint = value;
        }
    }

    public Rectangle rectangle => _rectangle;

    public void InitializeToScreenAspect()
    {
        _size = new Vec2(320f, 320f / Resolution.current.aspect);
        _dirty = true;
    }

    public float PercentW(float percent)
    {
        return width * (percent / 100f);
    }

    public float PercentH(float percent)
    {
        return height * (percent / 100f);
    }

    public Vec2 PercentWH(float wide, float high)
    {
        return new Vec2(width * (wide / 100f), height * (high / 100f));
    }

    public Vec2 OffsetTL(float t, float l)
    {
        return new Vec2(x + t, y + l);
    }

    public Vec2 OffsetBR(float t, float l)
    {
        return new Vec2(x + width + t, y + height + l);
    }

    public Vec2 OffsetCenter(float t, float l)
    {
        return new Vec2(x + PercentW(50f) + t, y + PercentH(50f) + l);
    }

    public Camera()
    {
    }

    public Camera(float xval, float yval, float wval = -1f, float hval = -1f)
    {
        if (wval < 0f)
        {
            wval = 320f;
        }
        if (hval < 0f)
        {
            hval = 320f * Graphics.aspect;
        }
        x = xval;
        y = yval;
        width = wval;
        height = hval;
    }

    public void DoUpdate()
    {
        if (skipUpdate)
        {
            skipUpdate = false;
        }
        else
        {
            Update();
        }
    }

    public virtual void Update()
    {
    }

    public virtual Vec2 transformScreenVector(Vec2 vector)
    {
        Vec3 newvec3 = Vec3.Transform(new Vec3(vector.X, vector.Y, 0f), Matrix.Invert(getMatrix()));
        return new Vec2(newvec3.x, newvec3.y);
    }

    public virtual Vec2 transformTime(Vec2 vector)
    {
        Vec3 newvec3 = Vec3.Transform(new Vec3(vector.X, vector.Y, 0f), Resolution.getTransformationMatrix() * getMatrix());
        return new Vec2(newvec3.x, newvec3.y);
    }

    public virtual Vec2 transformWorldVector(Vec2 vector)
    {
        Vec3 newvec3 = Vec3.Transform(new Vec3(vector.X, vector.Y, 0f), Matrix.Invert(Resolution.getTransformationMatrix()) * getMatrix());
        return new Vec2(newvec3.x, newvec3.y);
    }

    public virtual Vec2 transform(Vec2 vector)
    {
        Vec3 newvec3 = Vec3.Transform(new Vec3(vector.X, vector.Y, 0f), getMatrix());
        return new Vec2(newvec3.x, newvec3.y);
    }

    public virtual Vec2 transformInverse(Vec2 vector)
    {
        Vec3 newvec3 = Vec3.Transform(new Vec3(vector.X, vector.Y, 0f), Matrix.Invert(getMatrix()));
        return new Vec2(newvec3.x, newvec3.y);
    }

    public virtual Matrix getMatrix()
    {
        if (_dirty || (float)Graphics.viewport.Width != _viewSize.X || (float)Graphics.viewport.Height != _viewSize.Y)
        {
            _rectangle = new Rectangle(left - 16f, top - 16f, size.X + 32f, size.Y + 32f);
            _viewSize = new Vec2(Graphics.viewport.Width, Graphics.viewport.Height);
            Vec2 pos = position;
            float wid = width;
            float hig = height;
            _matrix = Matrix.CreateTranslation(new Vec3(0f - pos.X, 0f - pos.Y, 0f)) * Matrix.CreateScale(_viewSize.X / wid, _viewSize.Y / hig, 1f);
            _dirty = false;
        }
        return _matrix;
    }
}
