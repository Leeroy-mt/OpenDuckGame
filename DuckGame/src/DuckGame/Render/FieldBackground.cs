using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class FieldBackground : Layer
{
    private Effect _fx;

    private float _scroll;

    public float fieldHeight;

    public Matrix _view;

    public Matrix _proj;

    private float _ypos;

    private List<Sprite> _sprites = new List<Sprite>();

    public float scroll
    {
        get
        {
            return _scroll;
        }
        set
        {
            _scroll = value;
        }
    }

    public new Matrix view => _view;

    public new Matrix projection => _proj;

    public float rise { get; set; }

    public float ypos
    {
        get
        {
            return _ypos;
        }
        set
        {
            _ypos = value;
        }
    }

    public void AddSprite(Sprite s)
    {
        _sprites.Add(s);
    }

    public FieldBackground(string nameval, int depthval = 0)
        : base(nameval, depthval)
    {
        _fx = Content.Load<MTEffect>("Shaders/fieldFadeAdd");
        _view = Matrix.CreateLookAt(new Vec3(0f, 0f, -5f), new Vec3(0f, 0f, 0f), Vec3.Up);
        _proj = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4f, 1.7777778f, 0.01f, 100000f);
    }

    public override void Update()
    {
        float elevate = 53f + fieldHeight + rise;
        float pitch = -0.1f;
        float xpos = scroll;
        _view = Matrix.CreateLookAt(new Vec3(xpos, 300f, 0f - elevate + pitch), new Vec3(xpos, 100f, 0f - elevate), Vec3.Down);
    }

    public override void Begin(bool transparent, bool isTargetDraw = false)
    {
        Vec3 fade = new Vec3(Graphics.fade * _fade * (1f - _darken)) * base.colorMul;
        Vec3 fadeAdd = _colorAdd + new Vec3(_fadeAdd) + new Vec3(Graphics.flashAddRenderValue) + new Vec3(Graphics.fadeAddRenderValue) - new Vec3(base.darken);
        fadeAdd = new Vec3(Maths.Clamp(fadeAdd.x, -1f, 1f), Maths.Clamp(fadeAdd.y, -1f, 1f), Maths.Clamp(fadeAdd.z, -1f, 1f));
        if (!Options.Data.flashing)
        {
            fadeAdd = new Vec3(0f, 0f, 0f);
        }
        if (_darken > 0f)
        {
            _darken -= 0.15f;
        }
        else
        {
            _darken = 0f;
        }
        if (_fx != null)
        {
            _fx.Parameters["fade"]?.SetValue(fade);
            _fx.Parameters["add"]?.SetValue(fadeAdd);
        }
        Graphics.screen = _batch;
        if (_state.ScissorTestEnable)
        {
            Graphics.SetScissorRectangle(_scissor);
        }
        _batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, _state, _fx, base.camera.getMatrix());
    }

    public override void Draw(bool transparent, bool isTargetDraw = false)
    {
        Graphics.currentLayer = this;
        _fx.Parameters["WVP"].SetValue(_view * _proj);
        Begin(transparent);
        foreach (Sprite s in _sprites)
        {
            Graphics.Draw(s, s.X, s.Y);
        }
        _batch.End();
        Graphics.screen = null;
        Graphics.currentLayer = null;
    }
}
