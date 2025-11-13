using System;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class GridBackground : Layer
{
	private VertexPositionColorTexture[] _vertices = new VertexPositionColorTexture[4];

	private BasicEffect _effect;

	private float _scroll;

	private Sprite _planet;

	public GridBackground(string nameval, int depthval = 0)
		: base(nameval, depthval)
	{
		float primWidth = 400f;
		float primHeight = 230f;
		_effect = new BasicEffect(Graphics.device);
		_effect.View = Matrix.CreateLookAt(new Vec3(0f, 0f, 500f), new Vec3(0f, 0f, 0f), Vec3.Up);
		_effect.Projection = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4f, (float)Graphics.viewport.Width / (float)Graphics.viewport.Height, 0.01f, 100000f);
		_effect.Texture = new Sprite("grid").texture;
		_effect.TextureEnabled = true;
		_effect.VertexColorEnabled = true;
		_vertices[0].Position = new Vec3(0f, primHeight, 0f);
		_vertices[0].TextureCoordinate = new Vec2(0f, 0f);
		_vertices[0].Color = new Color(1f, 1f, 1f, 1f);
		_vertices[1].Position = new Vec3(0f, 0f, 0f);
		_vertices[1].TextureCoordinate = new Vec2(0f, 11f);
		_vertices[1].Color = new Color(0.5f, 0.5f, 0.5f, 1f);
		_vertices[2].Position = new Vec3(primWidth, primHeight, 0f);
		_vertices[2].TextureCoordinate = new Vec2(8.5f, 0f);
		_vertices[2].Color = new Color(1f, 1f, 1f, 1f);
		_vertices[3].Position = new Vec3(primWidth, 0f, 0f);
		_vertices[3].TextureCoordinate = new Vec2(8.5f, 11f);
		_vertices[3].Color = new Color(0.5f, 0.5f, 0.5f, 1f);
		_planet = new Sprite("background/planet");
	}

	public override void Update()
	{
		_scroll += 0.4f;
		if (_scroll > 32f)
		{
			_scroll -= 32f;
		}
	}

	public override void Begin(bool transparent, bool isTargetDraw = false)
	{
		_effect.DiffuseColor = new Vec3(Graphics.fade, Graphics.fade, Graphics.fade);
		Graphics.screen = _batch;
		_batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, _effect, base.camera.getMatrix());
	}

	public override void Draw(bool transparent, bool isTargetDraw = false)
	{
		Graphics.currentLayer = this;
		_effect.World = Matrix.CreateTranslation(new Vec3(190f, -75f, 300f));
		Begin(transparent);
		_planet.flipH = true;
		Graphics.Draw(_planet, 0f, 0f);
		_batch.End();
		_effect.World = Matrix.CreateRotationX(Maths.DegToRad(90f)) * Matrix.CreateTranslation(new Vec3(-400f, 52f, 0f));
		Begin(transparent);
		float squareSize = 32f;
		int numSquaresHor = 26;
		int numSquaresVert = 16;
		for (int ypos = 0; ypos < numSquaresVert; ypos++)
		{
			Graphics.DrawLine(new Vec2(0f + _scroll, (float)ypos * squareSize), new Vec2(squareSize * (float)(numSquaresHor - 1), (float)ypos * squareSize), Color.DarkGray, 3f);
		}
		for (int xpos = 0; xpos < numSquaresHor; xpos++)
		{
			Graphics.DrawLine(new Vec2((float)xpos * squareSize + _scroll, 0f), new Vec2((float)xpos * squareSize + _scroll, squareSize * (float)(numSquaresVert - 1)), Color.DarkGray, 3f);
		}
		_batch.End();
		_effect.World = Matrix.CreateRotationX(Maths.DegToRad(90f)) * Matrix.CreateTranslation(new Vec3(-400f, 62f, 0f));
		Begin(transparent);
		for (int i = 0; i < numSquaresVert; i++)
		{
			Graphics.DrawLine(new Vec2(0f + _scroll, (float)i * squareSize), new Vec2(squareSize * (float)(numSquaresHor - 1), (float)i * squareSize), Color.DarkGray * 0.2f, 3f);
		}
		for (int j = 0; j < numSquaresHor; j++)
		{
			Graphics.DrawLine(new Vec2((float)j * squareSize + _scroll, 0f), new Vec2((float)j * squareSize + _scroll, squareSize * (float)(numSquaresVert - 1)), Color.DarkGray * 0.2f, 3f);
		}
		_batch.End();
		_effect.World = Matrix.CreateRotationX(Maths.DegToRad(90f)) * Matrix.CreateTranslation(new Vec3(-400f, -52f, 0f));
		Begin(transparent);
		for (int k = 0; k < numSquaresVert; k++)
		{
			Graphics.DrawLine(new Vec2(0f + _scroll, (float)k * squareSize), new Vec2(squareSize * (float)(numSquaresHor - 1), (float)k * squareSize), Color.DarkGray, 3f);
		}
		for (int l = 0; l < numSquaresHor; l++)
		{
			Graphics.DrawLine(new Vec2((float)l * squareSize + _scroll, 0f), new Vec2((float)l * squareSize + _scroll, squareSize * (float)(numSquaresVert - 1)), Color.DarkGray, 3f);
		}
		_batch.End();
		Graphics.screen = null;
		Graphics.currentLayer = null;
	}
}
