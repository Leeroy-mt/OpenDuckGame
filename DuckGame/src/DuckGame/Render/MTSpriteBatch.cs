using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text;

namespace DuckGame;

public class MTSpriteBatch : SpriteBatch
{
    #region Public Fields

    public static float edgeBias = 1E-05f;

    public Matrix fullMatrix;

    #endregion

    #region Private Fields

    bool _beginCalled;

    int _globalIndex = Thing.GetGlobalIndex();

    SpriteSortMode _sortMode;

    Vector2 _texCoordTL = new(0, 0);

    Vector2 _texCoordBR = new(0, 0);

    RectangleF _tempRect = new(0, 0, 0, 0);

    Matrix _matrix;

    Matrix _projMatrix;

    BlendState _blendState;

    SamplerState _samplerState;

    DepthStencilState _depthStencilState;

    RasterizerState _rasterizerState;

    RasterizerState _prevRast;

    Effect _effect;

    MTEffect _spriteEffect;

    MTEffect _simpleEffect;

    readonly MTSpriteBatcher _batcher;

    readonly EffectParameter _matrixTransformSprite;

    readonly EffectParameter _matrixTransformSimple;

    #endregion

    #region Public Properties

    public bool transitionEffect => Layer.basicWireframeEffect != null && (_effect == Layer.basicWireframeEffect.effect);

    public Matrix viewMatrix => _matrix;

    public Matrix projMatrix => _projMatrix;

    public MTEffect SpriteEffect => _spriteEffect;

    public MTEffect SimpleEffect => _simpleEffect;

    #endregion

    public MTSpriteBatch(GraphicsDevice graphicsDevice)
        : base(graphicsDevice)
    {
        if (graphicsDevice == null)
        {
            throw new ArgumentException("graphicsDevice");
        }
        _spriteEffect = Content.Load<MTEffect>("Shaders/SpriteEffect");
        _matrixTransformSprite = _spriteEffect.effect.Parameters["MatrixTransform"];
        _simpleEffect = Content.Load<MTEffect>("Shaders/SpriteEffectSimple");
        _matrixTransformSimple = _simpleEffect.effect.Parameters["MatrixTransform"];
        _batcher = new MTSpriteBatcher(graphicsDevice, this);
        _beginCalled = false;
    }

    public MTSpriteBatchItem StealLastSpriteBatchItem()
    {
        return _batcher.StealLastBatchItem();
    }

    public new void Begin()
    {
        Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);
    }

    public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, MTEffect effect, Matrix transformMatrix)
    {
        _ = Graphics.device;

        Graphics.currentStateIndex = _globalIndex;
        if (_beginCalled)
            throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");
        base.Begin();
        Recorder.currentRecording?.StateChange(
            sortMode,
            blendState,
            samplerState,
            depthStencilState,
            rasterizerState,
            Layer.IsBasicLayerEffect(effect)
            ? Layer.basicLayerEffect
            : effect,
            transformMatrix,
            GraphicsDevice.ScissorRectangle
            );
        _sortMode = sortMode;
        _blendState = blendState ?? BlendState.AlphaBlend;
        _samplerState = samplerState ?? SamplerState.LinearClamp;
        _depthStencilState = depthStencilState ?? DepthStencilState.None;
        _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
        _effect = effect;
        _matrix = transformMatrix;
        if (sortMode == SpriteSortMode.Immediate)
            Setup();
        _beginCalled = true;
    }

    public new void Begin(SpriteSortMode sortMode, BlendState blendState)
    {
        Begin(sortMode, blendState, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);
    }

    public new void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState)
    {
        Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, null, Matrix.Identity);
    }

    public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, MTEffect effect)
    {
        Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, Matrix.Identity);
    }

    public new void End()
    {
        _beginCalled = false;
        base.End();
        if (_batcher.hasSimpleItems)
        {
            if (_sortMode != SpriteSortMode.Immediate)
            {
                Setup(simple: true);
            }
            _batcher.DrawSimpleBatch(_sortMode);
        }
        if (_batcher.hasGeometryItems)
        {
            if (_sortMode != SpriteSortMode.Immediate)
            {
                Setup(simple: true);
            }
            _batcher.DrawGeometryBatch(_sortMode);
        }
        if (_sortMode != SpriteSortMode.Immediate)
        {
            Setup();
        }
        _batcher.DrawBatch(_sortMode);
        if (_batcher.hasTexturedGeometryItems)
        {
            if (_sortMode != SpriteSortMode.Immediate)
            {
                Setup();
            }
            _batcher.DrawTexturedGeometryBatch(_sortMode);
        }
    }

    public void ReapplyEffect(bool simple = false)
    {
        GraphicsDevice graphicsDevice = base.GraphicsDevice;
        graphicsDevice.BlendState = _blendState;
        graphicsDevice.DepthStencilState = _depthStencilState;
        graphicsDevice.RasterizerState = _rasterizerState;
        graphicsDevice.SamplerStates[0] = _samplerState;
        if (simple)
        {
            _simpleEffect.effect.CurrentTechnique.Passes[0].Apply();
        }
        else
        {
            _spriteEffect.effect.CurrentTechnique.Passes[0].Apply();
        }
    }

    public void Setup(bool simple = false)
    {
        GraphicsDevice graphicsDevice = base.GraphicsDevice;
        graphicsDevice.BlendState = _blendState;
        graphicsDevice.DepthStencilState = _depthStencilState;
        graphicsDevice.RasterizerState = _rasterizerState;
        graphicsDevice.SamplerStates[0] = _samplerState;
        Viewport vp = graphicsDevice.Viewport;
        Matrix.CreateOrthographicOffCenter(0f, vp.Width, vp.Height, 0f, 1f, -1f, out _projMatrix);
        //if (!Program.isLinux)
        //{
        //	_projMatrix.M41 += -0.5f * _projMatrix.M11;
        //	_projMatrix.M42 += -0.5f * _projMatrix.M22;
        //}
        Matrix.Multiply(ref _matrix, ref _projMatrix, out var projection);
        fullMatrix = projection;
        if (simple)
        {
            _matrixTransformSimple.SetValue(projection);
            _simpleEffect.effect.CurrentTechnique.Passes[0].Apply();
        }
        else
        {
            _matrixTransformSprite.SetValue(projection);
            _spriteEffect.effect.CurrentTechnique.Passes[0].Apply();
        }
        if (_effect != null)
        {
            if (simple && _effect.Techniques.Count > 1 && _effect.Techniques[1].Name == "BasicSimple")
            {
                _effect.CurrentTechnique = _effect.Techniques[1];
            }
            else
            {
                _effect.CurrentTechnique = _effect.Techniques[0];
            }
            _effect.Parameters["MatrixTransform"]?.SetValue(projection);
            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }

#if NO_TEX2D
    void CheckValid(Texture2D texture)
#else
    private void CheckValid(Tex2D texture)
#endif
    {
        ArgumentNullException.ThrowIfNull(texture);
        if (!_beginCalled)
            throw new InvalidOperationException("Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
    }

    private void CheckValid(SpriteFont spriteFont, string text)
    {
        ArgumentNullException.ThrowIfNull(spriteFont);
        ArgumentNullException.ThrowIfNull(text);
        if (!_beginCalled)
            throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
    }

    private void CheckValid(SpriteFont spriteFont, StringBuilder text)
    {
        ArgumentNullException.ThrowIfNull(spriteFont);
        ArgumentNullException.ThrowIfNull(text);
        if (!_beginCalled)
            throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
    }

    public GeometryItem GetGeometryItem()
    {
        return _batcher.GetGeometryItem();
    }

    public static GeometryItem CreateGeometryItem()
    {
        return MTSpriteBatcher.CreateGeometryItem();
    }

    public void SubmitGeometry(GeometryItem geo)
    {
        _batcher.SubmitGeometryItem(geo);
    }

    public static GeometryItemTexture CreateTexturedGeometryItem()
    {
        return MTSpriteBatcher.CreateTexturedGeometryItem();
    }

    public void SubmitTexturedGeometry(GeometryItemTexture geo)
    {
        _batcher.SubmitTexturedGeometryItem(geo);
    }

    /// <summary>
    /// This is a MonoGame Extension method for calling Draw() using named parameters.  It is not available in the standard XNA Framework.
    /// </summary>
    /// <param name="texture">
    /// The Texture2D to draw.  Required.
    /// </param>
    /// <param name="position">
    /// The position to draw at.  If left empty, the method will draw at drawRectangle instead.
    /// </param>
    /// <param name="drawRectangle">
    /// The rectangle to draw at.  If left empty, the method will draw at position instead.
    /// </param>
    /// <param name="sourceRectangle">
    /// The source rectangle of the texture.  Default is null
    /// </param>
    /// <param name="origin">
    /// Origin of the texture.  Default is Vector2.Zero
    /// </param>
    /// <param name="rotation">
    /// Rotation of the texture.  Default is 0f
    /// </param>
    /// <param name="scale">
    /// The scale of the texture as a Vector2.  Default is Vector2.One
    /// </param>
    /// <param name="color">
    /// Color of the texture.  Default is Color.White
    /// </param>
    /// <param name="effect">
    /// SpriteEffect to draw with.  Default is SpriteEffects.None
    /// </param>
    /// <param name="depth">
    /// Draw depth.  Default is 0f.
    /// </param>
#if NO_TEX2D
    public void Draw(Texture2D texture, Vector2? position = null, RectangleF? drawRectangle = null, RectangleF? sourceRectangle = null, Vector2? origin = null, float rotation = 0f, Vector2? scale = null, Color? color = null, SpriteEffects effect = SpriteEffects.None, float depth = 0f)
#else
    public void Draw(Tex2D texture, Vector2? position = null, RectangleF? drawRectangle = null, RectangleF? sourceRectangle = null, Vector2? origin = null, float rotation = 0f, Vector2? scale = null, Color? color = null, SpriteEffects effect = SpriteEffects.None, float depth = 0f)
#endif
    {
        if (!color.HasValue)
        {
            color = Color.White;
        }
        if (!origin.HasValue)
        {
            origin = Vector2.Zero;
        }
        if (!scale.HasValue)
        {
            scale = Vector2.One;
        }
        if (drawRectangle.HasValue == position.HasValue)
        {
            throw new InvalidOperationException("Expected drawRectangle or position, but received neither or both.");
        }
        if (position.HasValue)
        {
            Draw(texture, position.Value, sourceRectangle, color.Value, rotation, origin.Value, scale.Value, effect, depth);
        }
        else
        {
            Draw(texture, drawRectangle.Value, sourceRectangle, color.Value, rotation, origin.Value, effect, depth);
        }
    }

#if NO_TEX2D
    public void Draw(Texture2D texture, Vector2 position, RectangleF? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
#else
    public void Draw(Tex2D texture, Vector2 position, RectangleF? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
#endif
    {
        CheckValid(texture);
#if NO_TEX2D
        float w = (float)texture.Width * scale.X;
        float h = (float)texture.Height * scale.Y;
#else
        float w = (float)texture.width * scale.X;
        float h = (float)texture.height * scale.Y;
#endif
        if (sourceRectangle.HasValue)
        {
            w = sourceRectangle.Value.Width * scale.X;
            h = sourceRectangle.Value.Height * scale.Y;
        }
        DoDrawInternal(texture, new Vector4(position.X, position.Y, w, h), sourceRectangle, color, rotation, origin * scale, effect, depth, autoFlush: true, null);
    }

#if NO_TEX2D
    public void DrawWithMaterial(Texture2D texture, Vector2 position, RectangleF? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth, AutoEffect fx)
#else
    public void DrawWithMaterial(Tex2D texture, Vector2 position, RectangleF? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth, AutoEffect fx)
#endif
    {
        CheckValid(texture);
#if NO_TEX2D
        float w = texture.Width * scale.X;
        float h = texture.Height * scale.Y;
#else
        float w = (float)texture.width * scale.X;
        float h = (float)texture.height * scale.Y;
#endif
        if (sourceRectangle.HasValue)
        {
            w = sourceRectangle.Value.Width * scale.X;
            h = sourceRectangle.Value.Height * scale.Y;
        }
        DoDrawInternal(texture, new Vector4(position.X, position.Y, w, h), sourceRectangle, color, rotation, origin * scale, effect, depth, autoFlush: true, fx);
    }

#if NO_TEX2D
    public void Draw(Texture2D texture, Vector2 position, RectangleF? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, float depth)
#else
    public void Draw(Tex2D texture, Vector2 position, RectangleF? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, float depth)
#endif
    {
        CheckValid(texture);
#if NO_TEX2D
        float w = (float)texture.Width * scale;
        float h = (float)texture.Height * scale;
#else
        float w = (float)texture.width * scale;
        float h = (float)texture.height * scale;
#endif
        if (sourceRectangle.HasValue)
        {
            w = sourceRectangle.Value.Width * scale;
            h = sourceRectangle.Value.Height * scale;
        }
        DoDrawInternal(texture, new Vector4(position.X, position.Y, w, h), sourceRectangle, color, rotation, origin * scale, effect, depth, autoFlush: true, null);
    }

#if NO_TEX2D
    public void Draw(Texture2D texture, RectangleF destinationRectangle, RectangleF? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effect, float depth)
#else
    public void Draw(Tex2D texture, RectangleF destinationRectangle, RectangleF? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effect, float depth)
#endif
    {
        CheckValid(texture);
#if NO_TEX2D
        DoDrawInternal(texture, new Vector4(destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height), sourceRectangle, color, rotation, new Vector2(origin.X * (destinationRectangle.Width / ((sourceRectangle.HasValue && sourceRectangle.Value.Width != 0f) ? sourceRectangle.Value.Width : ((float)texture.Width))), origin.Y * destinationRectangle.Height / ((sourceRectangle.HasValue && sourceRectangle.Value.Height != 0f) ? sourceRectangle.Value.Height : ((float)texture.Height))), effect, depth, autoFlush: true, null);
#else
        DoDrawInternal(texture, new Vector4(destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height), sourceRectangle, color, rotation, new Vector2(origin.X * (destinationRectangle.Width / ((sourceRectangle.HasValue && sourceRectangle.Value.Width != 0f) ? sourceRectangle.Value.Width : ((float)texture.width))), origin.Y * destinationRectangle.Height / ((sourceRectangle.HasValue && sourceRectangle.Value.Height != 0f) ? sourceRectangle.Value.Height : ((float)texture.height))), effect, depth, autoFlush: true, null);
#endif
    }

#if NO_TEX2D
    public void DrawQuad(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 t1, Vector2 t2, Vector2 t3, Vector2 t4, float depth, Texture2D tex, Color c)
    {
        MTSpriteBatchItem mTSpriteBatchItem = _batcher.CreateBatchItem();
        mTSpriteBatchItem.Depth = depth;
        mTSpriteBatchItem.Texture = tex;
#else
    public void DrawQuad(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 t1, Vector2 t2, Vector2 t3, Vector2 t4, float depth, Tex2D tex, Color c)
    {
        MTSpriteBatchItem mTSpriteBatchItem = _batcher.CreateBatchItem();
        mTSpriteBatchItem.Depth = depth;
        mTSpriteBatchItem.Texture = tex?.nativeObject as Texture2D;
#endif
        mTSpriteBatchItem.Material = null;
        mTSpriteBatchItem.Set(p1, p2, p3, p4, t1, t2, t3, t4, c);
    }

#if NO_TEX2D
    internal void DoDrawInternal(Texture2D texture, Vector4 destinationRectangle, RectangleF? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effect, float depth, bool autoFlush, AutoEffect fx)
#else
    internal void DoDrawInternal(Tex2D texture, Vector4 destinationRectangle, RectangleF? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effect, float depth, bool autoFlush, AutoEffect fx)
#endif
    {
        MTSpriteBatchItem item = _batcher.CreateBatchItem();
        item.Depth = depth;
#if NO_TEX2D
        item.Texture = texture;
#else
        item.Texture = texture.nativeObject as Texture2D;
#endif
        item.Material = fx;
        if (sourceRectangle.HasValue)
        {
            _tempRect = sourceRectangle.Value;
        }
#if NO_TEX2D
        else
        {
            _tempRect.X = 0f;
            _tempRect.Y = 0f;
            _tempRect.Width = texture.Width;
            _tempRect.Height = texture.Height;
        }
        _texCoordTL.X = _tempRect.X / texture.Width + edgeBias;
        _texCoordTL.Y = _tempRect.Y / texture.Height + edgeBias;
        _texCoordBR.X = (_tempRect.X + _tempRect.Width) / texture.Width - edgeBias;
        _texCoordBR.Y = (_tempRect.Y + _tempRect.Height) / texture.Height - edgeBias;
#else
        else
        {
            _tempRect.X = 0f;
            _tempRect.Y = 0f;
            _tempRect.Width = texture.width;
            _tempRect.Height = texture.height;
        }
        _texCoordTL.X = _tempRect.X / (float)texture.width + edgeBias;
        _texCoordTL.Y = _tempRect.Y / (float)texture.height + edgeBias;
        _texCoordBR.X = (_tempRect.X + _tempRect.Width) / (float)texture.width - edgeBias;
        _texCoordBR.Y = (_tempRect.Y + _tempRect.Height) / (float)texture.height - edgeBias;
#endif
        if ((effect & SpriteEffects.FlipVertically) != SpriteEffects.None)
        {
            float temp = _texCoordBR.Y;
            _texCoordBR.Y = _texCoordTL.Y;
            _texCoordTL.Y = temp;
        }
        if ((effect & SpriteEffects.FlipHorizontally) != SpriteEffects.None)
        {
            float temp2 = _texCoordBR.X;
            _texCoordBR.X = _texCoordTL.X;
            _texCoordTL.X = temp2;
        }
        item.Set(destinationRectangle.X, destinationRectangle.Y, 0f - origin.X, 0f - origin.Y, destinationRectangle.Z, destinationRectangle.W, (float)Math.Sin(rotation), (float)Math.Cos(rotation), color, _texCoordTL, _texCoordBR);
        if (Graphics.recordMetadata)
        {
            item.MetaData = new MTSpriteBatchItemMetaData();
            item.MetaData.texture = texture;
            item.MetaData.rotation = rotation;
            item.MetaData.color = color;
            item.MetaData.tempRect = _tempRect;
            item.MetaData.effect = effect;
            item.MetaData.depth = depth;
        }
        if (!Graphics.skipReplayRender && Recorder.currentRecording != null && Graphics.currentRenderTarget == null)
        {
            Recorder.currentRecording.LogDraw(
#if NO_TEX2D
                texture.GetTextureIndex(),
#else
                texture.textureIndex,
#endif
                new Vector2(item.vertexTL.Position.X, item.vertexTL.Position.Y),
                new Vector2(item.vertexBR.Position.X, item.vertexBR.Position.Y),
                rotation,
                color,
                (short)_tempRect.X,
                (short)_tempRect.Y,
                (short)(_tempRect.Width * (((effect & SpriteEffects.FlipHorizontally) == 0) ? 1 : -1)),
                (short)(_tempRect.Height * (((effect & SpriteEffects.FlipVertically) == 0) ? 1 : -1)),
                depth
                );
        }
        if (autoFlush)
        {
            FlushIfNeeded();
        }
    }

    public void DrawExistingBatchItem(MTSpriteBatchItem item)
    {
        _batcher.SqueezeInItem(item);
#if NO_TEX2D
        Recorder.currentRecording?.LogDraw(item.MetaData.texture.GetTextureIndex(), new Vector2(item.vertexTL.Position.X, item.vertexTL.Position.Y), new Vector2(item.vertexBR.Position.X, item.vertexBR.Position.Y), item.MetaData.rotation, item.MetaData.color, (short)item.MetaData.tempRect.X, (short)item.MetaData.tempRect.Y, (short)(item.MetaData.tempRect.Width * (float)(((item.MetaData.effect & SpriteEffects.FlipHorizontally) == 0) ? 1 : (-1))), (short)(item.MetaData.tempRect.Height * (float)(((item.MetaData.effect & SpriteEffects.FlipVertically) == 0) ? 1 : (-1))), item.MetaData.depth);
#else
        Recorder.currentRecording?.LogDraw(item.MetaData.texture.textureIndex, new Vector2(item.vertexTL.Position.X, item.vertexTL.Position.Y), new Vector2(item.vertexBR.Position.X, item.vertexBR.Position.Y), item.MetaData.rotation, item.MetaData.color, (short)item.MetaData.tempRect.X, (short)item.MetaData.tempRect.Y, (short)(item.MetaData.tempRect.Width * (float)(((item.MetaData.effect & SpriteEffects.FlipHorizontally) == 0) ? 1 : (-1))), (short)(item.MetaData.tempRect.Height * (float)(((item.MetaData.effect & SpriteEffects.FlipVertically) == 0) ? 1 : (-1))), item.MetaData.depth);
#endif
    }

    public void DrawRecorderItem(ref RecorderFrameItem frame)
    {
        MTSpriteBatchItem item = _batcher.CreateBatchItem();
        item.Depth = frame.depth;
        if (frame.texture == -1)
        {
        }
        else
        {
            var tex = Content.GetTex2DFromIndex(frame.texture);
            if (tex == null)
            {
                return;
            }
#if NO_TEX2D
            item.Texture = tex;
#else
            item.Texture = tex.nativeObject as Texture2D;
#endif
        }
        if (item.Texture != null)
        {
            float w = Math.Abs(frame.texW);
            float h = Math.Abs(frame.texH);
            _texCoordTL.X = (float)frame.texX / (float)item.Texture.Width + edgeBias;
            _texCoordTL.Y = (float)frame.texY / (float)item.Texture.Height + edgeBias;
            _texCoordBR.X = ((float)frame.texX + w) / (float)item.Texture.Width - edgeBias;
            _texCoordBR.Y = ((float)frame.texY + h) / (float)item.Texture.Height - edgeBias;
            if (frame.texH < 0)
            {
                float temp = _texCoordBR.Y;
                _texCoordBR.Y = _texCoordTL.Y;
                _texCoordTL.Y = temp;
            }
            if (frame.texW < 0)
            {
                float temp2 = _texCoordBR.X;
                _texCoordBR.X = _texCoordTL.X;
                _texCoordTL.X = temp2;
            }
            Vector2 br = frame.bottomRight.Rotate(0f - frame.rotation, frame.topLeft);
            item.Set(frame.topLeft.X, frame.topLeft.Y, 0f, 0f, br.X - frame.topLeft.X, br.Y - frame.topLeft.Y, (float)Math.Sin(frame.rotation), (float)Math.Cos(frame.rotation), frame.color, _texCoordTL, _texCoordBR);
        }
    }

    public void Flush(bool doSetup)
    {
        if (doSetup)
        {
            Setup();
        }
        _batcher.DrawBatch(_sortMode);
    }

    public void FlushSettingScissor()
    {
        Setup();
        _batcher.DrawBatch(_sortMode);
        _prevRast = base.GraphicsDevice.RasterizerState;
        base.GraphicsDevice.RasterizerState = new RasterizerState
        {
            CullMode = _rasterizerState.CullMode,
            FillMode = _rasterizerState.FillMode,
            SlopeScaleDepthBias = _rasterizerState.SlopeScaleDepthBias,
            MultiSampleAntiAlias = _rasterizerState.MultiSampleAntiAlias,
            ScissorTestEnable = true
        };
    }

    public void FlushAndClearScissor()
    {
        _batcher.DrawBatch(_sortMode);
        base.GraphicsDevice.RasterizerState = _prevRast;
    }

    internal void FlushIfNeeded()
    {
        if (_sortMode == SpriteSortMode.Immediate)
        {
            _batcher.DrawBatch(_sortMode);
        }
    }

#if NO_TEX2D
    public void Draw(Texture2D texture, Vector2 position, RectangleF? sourceRectangle, Color color)
#else
    public void Draw(Tex2D texture, Vector2 position, RectangleF? sourceRectangle, Color color)
#endif
    {
        Draw(texture, position, sourceRectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
    }

#if NO_TEX2D
    public void Draw(Texture2D texture, RectangleF destinationRectangle, RectangleF? sourceRectangle, Color color)
#else
    public void Draw(Tex2D texture, RectangleF destinationRectangle, RectangleF? sourceRectangle, Color color)
#endif
    {
        Draw(texture, destinationRectangle, sourceRectangle, color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
    }

#if !NO_TEX2D
    public void Draw(Tex2D texture, Vector2 position, Color color)
    {
        Draw(texture, position, null, color);
    }
#endif

#if NO_TEX2D
    public void Draw(Texture2D texture, RectangleF rectangle, Color color)
#else
    public void Draw(Tex2D texture, RectangleF rectangle, Color color)
#endif
    {
        Draw(texture, rectangle, null, color);
    }

    protected override void Dispose(bool disposing)
    {
        if (!base.IsDisposed && disposing && _spriteEffect != null)
        {
            _spriteEffect.effect.Dispose();
            _spriteEffect = null;
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// Obsolete, use DoDrawInternal()
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="destinationRectangle"></param>
    /// <param name="sourceRectangle"></param>
    /// <param name="color"></param>
    /// <param name="rotation"></param>
    /// <param name="origin"></param>
    /// <param name="effect"></param>
    /// <param name="depth"></param>
    /// <param name="autoFlush"></param>
    /// <param name="fx"></param>
#if NO_TEX2D
    internal void DoDrawInternalTex2D(Texture2D texture, Vector4 destinationRectangle, RectangleF? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effect, float depth, bool autoFlush, AutoEffect fx)
#else
    internal void DoDrawInternalTex2D(Tex2D texture, Vector4 destinationRectangle, RectangleF? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effect, float depth, bool autoFlush, AutoEffect fx)
#endif
    {
        DoDrawInternal(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effect, depth, autoFlush, fx);
    }
}
