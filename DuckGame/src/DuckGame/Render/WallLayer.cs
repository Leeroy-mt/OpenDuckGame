using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class WallLayer : Layer
{
    private Effect _fx;

    private float _scroll;

    public float fieldHeight;

    public Matrix _view;

    public Matrix _proj;

    private float _ypos;

    private List<Sprite> _sprites = new List<Sprite>();

    private List<Sprite> _wallSprites = new List<Sprite>();

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

    public void AddWallSprite(Sprite s)
    {
        _wallSprites.Add(s);
    }

    public WallLayer(string nameval, int depthval = 0)
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
        _fx.Parameters["WVP"].SetValue(Matrix.CreateRotationY(-(float)Math.PI / 2f) * Matrix.CreateTranslation(new Vec3(625f, 20f, 0.1f)) * _view * _proj);
        Begin(transparent);
        foreach (Sprite s in _wallSprites)
        {
            float xpos = s.X;
            Graphics.Draw(s, s.X, s.Y);
            s.X = xpos;
        }
        _batch.End();
        _fx.Parameters["WVP"].SetValue(Matrix.CreateRotationY(-(float)Math.PI / 2f) * Matrix.CreateRotationZ(-(float)Math.PI / 2f) * Matrix.CreateTranslation(new Vec3(625.5f, 160f, 0.1f)) * _view * _proj);
        Begin(transparent);
        foreach (Sprite s2 in _wallSprites)
        {
            float xpos2 = s2.X;
            Graphics.Draw(s2, s2.X, s2.Y);
            s2.X = xpos2;
        }
        _batch.End();
        Graphics.screen = null;
        Graphics.currentLayer = null;
    }
}
