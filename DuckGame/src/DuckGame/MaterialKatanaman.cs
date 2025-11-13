using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialKatanaman : Material
{
	private Tex2D _lighting;

	public TeamHat _hat;

	public MaterialKatanaman(TeamHat hat)
	{
		_effect = Content.Load<MTEffect>("shaders/katanaman");
		_lighting = Content.Load<Tex2D>("hats/katanaman_lightmap");
		_hat = hat;
	}

	public override void Apply()
	{
		Graphics.device.Textures[1] = (Texture2D)_lighting;
		Graphics.device.SamplerStates[1] = SamplerState.PointWrap;
		SetValue("xpos", _hat.position.x - 32f);
		SetValue("ypos", _hat.position.y - 32f);
		if (_hat.graphic != null)
		{
			SetValue("flipSub", _hat.graphic.flipH ? 1f : 0f);
		}
		float size = 150f;
		float mul = size / 64f;
		Vec2 lightOffset = new Vec2(15f, 11f) * mul;
		Vec2 gridTL = lightOffset - new Vec2(size / 2f);
		Vec2 snap = Maths.Snap(_hat.position - gridTL, size, size);
		SetValue("light1x", snap.x + lightOffset.x);
		SetValue("light1y", snap.y + lightOffset.y);
		lightOffset = new Vec2(49f, 25f) * mul;
		gridTL = lightOffset - new Vec2(size / 2f);
		snap = Maths.Snap(_hat.position - gridTL, size, size);
		SetValue("light2x", snap.x + lightOffset.x);
		SetValue("light2y", snap.y + lightOffset.y);
		lightOffset = new Vec2(21f, 49f) * mul;
		gridTL = lightOffset - new Vec2(size / 2f);
		snap = Maths.Snap(_hat.position - gridTL, size, size);
		SetValue("light3x", snap.x + lightOffset.x);
		SetValue("light3y", snap.y + lightOffset.y);
		SetValue("add", Layer.kGameLayerAdd);
		SetValue("fade", Layer.kGameLayerFade);
		foreach (EffectPass pass in _effect.effect.CurrentTechnique.Passes)
		{
			pass.Apply();
		}
	}
}
