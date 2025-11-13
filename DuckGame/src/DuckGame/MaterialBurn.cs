using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialBurn : Material
{
	private Tex2D _burnTexture;

	private float _burnVal;

	public float burnVal
	{
		get
		{
			return _burnVal;
		}
		set
		{
			_burnVal = value;
		}
	}

	public MaterialBurn(float burnVal = 0f)
	{
		_effect = Content.Load<MTEffect>("Shaders/burn");
		_burnTexture = Content.Load<Tex2D>("burn");
		_burnVal = burnVal;
	}

	public override void Apply()
	{
		Tex2D tex = Graphics.device.Textures[0] as Texture2D;
		Graphics.device.Textures[1] = (Texture2D)_burnTexture;
		SetValue("width", tex.frameWidth / (float)tex.width);
		SetValue("height", tex.frameHeight / (float)tex.height);
		SetValue("burn", _burnVal);
		foreach (EffectPass pass in _effect.effect.CurrentTechnique.Passes)
		{
			pass.Apply();
		}
	}
}
