using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialFrozen : Material
{
	private Tex2D _frozenTexture;

	private Thing _thing;

	public float intensity;

	public MaterialFrozen(Thing t)
	{
		_effect = Content.Load<MTEffect>("Shaders/frozen");
		_frozenTexture = Content.Load<Tex2D>("frozen");
		_thing = t;
	}

	public override void Apply()
	{
		if (Graphics.device.Textures[0] != null)
		{
			Tex2D tex = Graphics.device.Textures[0] as Texture2D;
			SetValue("width", tex.frameWidth / ((float)tex.width * 0.75f));
			SetValue("height", tex.frameHeight / ((float)tex.height * 0.75f));
			SetValue("xpos", _thing.x);
			SetValue("ypos", _thing.y);
			SetValue("intensity", intensity);
		}
		Graphics.device.Textures[1] = (Texture2D)_frozenTexture;
		Graphics.device.SamplerStates[1] = SamplerState.PointWrap;
		foreach (EffectPass pass in _effect.effect.CurrentTechnique.Passes)
		{
			pass.Apply();
		}
	}
}
