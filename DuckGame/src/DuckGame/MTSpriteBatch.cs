using System;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MTSpriteBatch : SpriteBatch
{
	private int _globalIndex = Thing.GetGlobalIndex();

	private readonly MTSpriteBatcher _batcher;

	private SpriteSortMode _sortMode;

	private BlendState _blendState;

	private SamplerState _samplerState;

	private DepthStencilState _depthStencilState;

	private RasterizerState _rasterizerState;

	private Effect _effect;

	private bool _beginCalled;

	private MTEffect _spriteEffect;

	private MTEffect _simpleEffect;

	private readonly EffectParameter _matrixTransformSprite;

	private readonly EffectParameter _matrixTransformSimple;

	private Matrix _matrix;

	private Rectangle _tempRect = new Rectangle(0f, 0f, 0f, 0f);

	private Vec2 _texCoordTL = new Vec2(0f, 0f);

	private Vec2 _texCoordBR = new Vec2(0f, 0f);

	private Matrix _projMatrix;

	public Matrix fullMatrix;

	public static float edgeBias = 1E-05f;

	private RasterizerState _prevRast;

	public bool transitionEffect
	{
		get
		{
			if (Layer.basicWireframeEffect != null)
			{
				return _effect == Layer.basicWireframeEffect.effect;
			}
			return false;
		}
	}

	public MTEffect SpriteEffect => _spriteEffect;

	public MTEffect SimpleEffect => _simpleEffect;

	public Matrix viewMatrix => _matrix;

	public Matrix projMatrix => _projMatrix;

	public MTSpriteBatchItem StealLastSpriteBatchItem()
	{
		return _batcher.StealLastBatchItem();
	}

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

	public new void Begin()
	{
		Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);
	}

	public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, MTEffect effect, Matrix transformMatrix)
	{
		_ = Graphics.device;
		_ = base.GraphicsDevice;
		Graphics.currentStateIndex = _globalIndex;
		if (_beginCalled)
		{
			throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");
		}
		base.Begin();
		if (Recorder.currentRecording != null)
		{
			Recorder.currentRecording.StateChange(sortMode, blendState, samplerState, depthStencilState, rasterizerState, Layer.IsBasicLayerEffect(effect) ? Layer.basicLayerEffect : effect, transformMatrix, base.GraphicsDevice.ScissorRectangle);
		}
		_sortMode = sortMode;
		_blendState = blendState ?? BlendState.AlphaBlend;
		_samplerState = samplerState ?? SamplerState.LinearClamp;
		_depthStencilState = depthStencilState ?? DepthStencilState.None;
		_rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
		_effect = effect;
		_matrix = transformMatrix;
		if (sortMode == SpriteSortMode.Immediate)
		{
			Setup();
		}
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
		if (Graphics.recordOnly)
		{
			return;
		}
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
		if (!Program.isLinux)
		{
			_projMatrix.M41 += -0.5f * _projMatrix.M11;
			_projMatrix.M42 += -0.5f * _projMatrix.M22;
		}
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

	private void CheckValid(Tex2D texture)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		if (!_beginCalled)
		{
			throw new InvalidOperationException("Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
		}
	}

	private void CheckValid(SpriteFont spriteFont, string text)
	{
		if (spriteFont == null)
		{
			throw new ArgumentNullException("spriteFont");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (!_beginCalled)
		{
			throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
		}
	}

	private void CheckValid(SpriteFont spriteFont, StringBuilder text)
	{
		if (spriteFont == null)
		{
			throw new ArgumentNullException("spriteFont");
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (!_beginCalled)
		{
			throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
		}
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
	public void Draw(Tex2D texture, Vec2? position = null, Rectangle? drawRectangle = null, Rectangle? sourceRectangle = null, Vec2? origin = null, float rotation = 0f, Vec2? scale = null, Color? color = null, SpriteEffects effect = SpriteEffects.None, float depth = 0f)
	{
		if (!color.HasValue)
		{
			color = Color.White;
		}
		if (!origin.HasValue)
		{
			origin = Vec2.Zero;
		}
		if (!scale.HasValue)
		{
			scale = Vec2.One;
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

	public void Draw(Tex2D texture, Vec2 position, Rectangle? sourceRectangle, Color color, float rotation, Vec2 origin, Vec2 scale, SpriteEffects effect, float depth)
	{
		CheckValid(texture);
		float w = (float)texture.width * scale.x;
		float h = (float)texture.height * scale.y;
		if (sourceRectangle.HasValue)
		{
			w = sourceRectangle.Value.width * scale.x;
			h = sourceRectangle.Value.height * scale.y;
		}
		DoDrawInternal(texture, new Vec4(position.x, position.y, w, h), sourceRectangle, color, rotation, origin * scale, effect, depth, autoFlush: true, null);
	}

	public void DrawWithMaterial(Tex2D texture, Vec2 position, Rectangle? sourceRectangle, Color color, float rotation, Vec2 origin, Vec2 scale, SpriteEffects effect, float depth, Material fx)
	{
		CheckValid(texture);
		float w = (float)texture.width * scale.x;
		float h = (float)texture.height * scale.y;
		if (sourceRectangle.HasValue)
		{
			w = sourceRectangle.Value.width * scale.x;
			h = sourceRectangle.Value.height * scale.y;
		}
		DoDrawInternal(texture, new Vec4(position.x, position.y, w, h), sourceRectangle, color, rotation, origin * scale, effect, depth, autoFlush: true, fx);
	}

	public void Draw(Tex2D texture, Vec2 position, Rectangle? sourceRectangle, Color color, float rotation, Vec2 origin, float scale, SpriteEffects effect, float depth)
	{
		CheckValid(texture);
		float w = (float)texture.width * scale;
		float h = (float)texture.height * scale;
		if (sourceRectangle.HasValue)
		{
			w = sourceRectangle.Value.width * scale;
			h = sourceRectangle.Value.height * scale;
		}
		DoDrawInternal(texture, new Vec4(position.x, position.y, w, h), sourceRectangle, color, rotation, origin * scale, effect, depth, autoFlush: true, null);
	}

	public void Draw(Tex2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vec2 origin, SpriteEffects effect, float depth)
	{
		CheckValid(texture);
		DoDrawInternal(texture, new Vec4(destinationRectangle.x, destinationRectangle.y, destinationRectangle.width, destinationRectangle.height), sourceRectangle, color, rotation, new Vec2(origin.x * (destinationRectangle.width / ((sourceRectangle.HasValue && sourceRectangle.Value.width != 0f) ? sourceRectangle.Value.width : ((float)texture.width))), origin.y * destinationRectangle.height / ((sourceRectangle.HasValue && sourceRectangle.Value.height != 0f) ? sourceRectangle.Value.height : ((float)texture.height))), effect, depth, autoFlush: true, null);
	}

	public void DrawQuad(Vec2 p1, Vec2 p2, Vec2 p3, Vec2 p4, Vec2 t1, Vec2 t2, Vec2 t3, Vec2 t4, float depth, Tex2D tex, Color c)
	{
		Graphics.currentDrawIndex++;
		MTSpriteBatchItem mTSpriteBatchItem = _batcher.CreateBatchItem();
		mTSpriteBatchItem.Depth = depth;
		mTSpriteBatchItem.Texture = tex.nativeObject as Texture2D;
		mTSpriteBatchItem.Material = null;
		mTSpriteBatchItem.Set(p1, p2, p3, p4, t1, t2, t3, t4, c);
	}

	internal void DoDrawInternal(Tex2D texture, Vec4 destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vec2 origin, SpriteEffects effect, float depth, bool autoFlush, Material fx)
	{
		Graphics.currentDrawIndex++;
		MTSpriteBatchItem item = _batcher.CreateBatchItem();
		item.Depth = depth;
		item.Texture = texture.nativeObject as Texture2D;
		item.Material = fx;
		if (sourceRectangle.HasValue)
		{
			_tempRect = sourceRectangle.Value;
		}
		else
		{
			_tempRect.x = 0f;
			_tempRect.y = 0f;
			_tempRect.width = texture.width;
			_tempRect.height = texture.height;
		}
		_texCoordTL.x = _tempRect.x / (float)texture.width + edgeBias;
		_texCoordTL.y = _tempRect.y / (float)texture.height + edgeBias;
		_texCoordBR.x = (_tempRect.x + _tempRect.width) / (float)texture.width - edgeBias;
		_texCoordBR.y = (_tempRect.y + _tempRect.height) / (float)texture.height - edgeBias;
		if ((effect & SpriteEffects.FlipVertically) != SpriteEffects.None)
		{
			float temp = _texCoordBR.y;
			_texCoordBR.y = _texCoordTL.y;
			_texCoordTL.y = temp;
		}
		if ((effect & SpriteEffects.FlipHorizontally) != SpriteEffects.None)
		{
			float temp2 = _texCoordBR.x;
			_texCoordBR.x = _texCoordTL.x;
			_texCoordTL.x = temp2;
		}
		item.Set(destinationRectangle.x, destinationRectangle.y, 0f - origin.x, 0f - origin.y, destinationRectangle.z, destinationRectangle.w, (float)Math.Sin(rotation), (float)Math.Cos(rotation), color, _texCoordTL, _texCoordBR);
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
			Recorder.currentRecording.LogDraw(texture.textureIndex, new Vec2(item.vertexTL.Position.X, item.vertexTL.Position.Y), new Vec2(item.vertexBR.Position.X, item.vertexBR.Position.Y), rotation, color, (short)_tempRect.x, (short)_tempRect.y, (short)(_tempRect.width * (float)(((effect & SpriteEffects.FlipHorizontally) == 0) ? 1 : (-1))), (short)(_tempRect.height * (float)(((effect & SpriteEffects.FlipVertically) == 0) ? 1 : (-1))), depth);
		}
		if (autoFlush)
		{
			FlushIfNeeded();
		}
	}

	public void DrawExistingBatchItem(MTSpriteBatchItem item)
	{
		Graphics.currentDrawIndex++;
		_batcher.SqueezeInItem(item);
		if (Recorder.currentRecording != null)
		{
			Recorder.currentRecording.LogDraw(item.MetaData.texture.textureIndex, new Vec2(item.vertexTL.Position.X, item.vertexTL.Position.Y), new Vec2(item.vertexBR.Position.X, item.vertexBR.Position.Y), item.MetaData.rotation, item.MetaData.color, (short)item.MetaData.tempRect.x, (short)item.MetaData.tempRect.y, (short)(item.MetaData.tempRect.width * (float)(((item.MetaData.effect & SpriteEffects.FlipHorizontally) == 0) ? 1 : (-1))), (short)(item.MetaData.tempRect.height * (float)(((item.MetaData.effect & SpriteEffects.FlipVertically) == 0) ? 1 : (-1))), item.MetaData.depth);
		}
	}

	public void DrawRecorderItem(ref RecorderFrameItem frame)
	{
		MTSpriteBatchItem item = _batcher.CreateBatchItem();
		item.Depth = frame.depth;
		if (frame.texture == -1)
		{
			item.Texture = Graphics.blankWhiteSquare.nativeObject as Texture2D;
		}
		else
		{
			Tex2D tex = Content.GetTex2DFromIndex(frame.texture);
			if (tex == null)
			{
				return;
			}
			item.Texture = tex.nativeObject as Texture2D;
		}
		if (item.Texture != null)
		{
			float w = Math.Abs(frame.texW);
			float h = Math.Abs(frame.texH);
			_texCoordTL.x = (float)frame.texX / (float)item.Texture.Width + edgeBias;
			_texCoordTL.y = (float)frame.texY / (float)item.Texture.Height + edgeBias;
			_texCoordBR.x = ((float)frame.texX + w) / (float)item.Texture.Width - edgeBias;
			_texCoordBR.y = ((float)frame.texY + h) / (float)item.Texture.Height - edgeBias;
			if (frame.texH < 0)
			{
				float temp = _texCoordBR.y;
				_texCoordBR.y = _texCoordTL.y;
				_texCoordTL.y = temp;
			}
			if (frame.texW < 0)
			{
				float temp2 = _texCoordBR.x;
				_texCoordBR.x = _texCoordTL.x;
				_texCoordTL.x = temp2;
			}
			Vec2 br = frame.bottomRight.Rotate(0f - frame.rotation, frame.topLeft);
			item.Set(frame.topLeft.x, frame.topLeft.y, 0f, 0f, br.x - frame.topLeft.x, br.y - frame.topLeft.y, (float)Math.Sin(frame.rotation), (float)Math.Cos(frame.rotation), frame.color, _texCoordTL, _texCoordBR);
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

	public void Draw(Tex2D texture, Vec2 position, Rectangle? sourceRectangle, Color color)
	{
		Draw(texture, position, sourceRectangle, color, 0f, Vec2.Zero, 1f, SpriteEffects.None, 0f);
	}

	public void Draw(Tex2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
	{
		Draw(texture, destinationRectangle, sourceRectangle, color, 0f, Vec2.Zero, SpriteEffects.None, 0f);
	}

	public void Draw(Tex2D texture, Vec2 position, Color color)
	{
		Draw(texture, position, null, color);
	}

	public void Draw(Tex2D texture, Rectangle rectangle, Color color)
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
	internal void DoDrawInternalTex2D(Tex2D texture, Vec4 destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vec2 origin, SpriteEffects effect, float depth, bool autoFlush, Material fx)
	{
		DoDrawInternal(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effect, depth, autoFlush, fx);
	}
}
