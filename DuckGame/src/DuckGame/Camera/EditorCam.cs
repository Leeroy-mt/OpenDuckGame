using Microsoft.Xna.Framework;

namespace DuckGame;

public class EditorCam : Camera
{
    protected new Vector2 _zoomPoint;

    protected float _zoomInc;

    protected float _zoom = 1f;

    public new Vector2 zoomPoint
    {
        get
        {
            return _zoomPoint;
        }
        set
        {
            if (_zoomPoint != value)
            {
                _zoomPoint = value;
                _dirty = true;
            }
        }
    }

    public float zoomInc
    {
        get
        {
            return _zoomInc;
        }
        set
        {
            if (_zoomInc != value)
            {
                _zoomInc = value;
                _dirty = true;
            }
        }
    }

    public float zoom => _zoom;

    public override void Update()
    {
    }
}
