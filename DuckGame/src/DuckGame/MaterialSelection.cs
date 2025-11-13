using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialSelection : Material
{
	public float fade = 1f;

	public MaterialSelection()
	{
		_effect = Content.Load<MTEffect>("Shaders/selection");
	}

	public override void Apply()
	{
		SetValue("fade", fade);
		foreach (EffectPass pass in _effect.effect.CurrentTechnique.Passes)
		{
			pass.Apply();
		}
	}
}
