using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MaterialEnergyBlade : Material
{
	private Tex2D _energyTexture;

	private OldEnergyScimi _thing;

	private EnergyScimitar _thing2;

	private float _time;

	public float glow;

	public MaterialEnergyBlade(OldEnergyScimi t)
	{
		_effect = Content.Load<MTEffect>("Shaders/energyBlade");
		_energyTexture = Content.Load<Tex2D>("energyTex");
		_thing = t;
	}

	public MaterialEnergyBlade(EnergyScimitar t)
	{
		_effect = Content.Load<MTEffect>("Shaders/energyBlade");
		_energyTexture = Content.Load<Tex2D>("energyTex");
		_thing2 = t;
	}

	public override void Apply()
	{
		_time += 0.016f;
		if (Graphics.device.Textures[0] != null)
		{
			Tex2D tex = Graphics.device.Textures[0] as Texture2D;
			SetValue("width", tex.frameWidth / (float)tex.width);
			SetValue("height", tex.frameHeight / (float)tex.height);
			if (_thing != null)
			{
				SetValue("xpos", _thing.x);
				SetValue("ypos", _thing.y);
				SetValue("time", _time);
				SetValue("glow", glow);
				SetValue("bladeColor", _thing.swordColor);
			}
			else
			{
				SetValue("xpos", _thing2.x);
				SetValue("ypos", _thing2.y);
				SetValue("time", _time);
				SetValue("glow", glow);
				SetValue("bladeColor", _thing2.swordColor);
			}
		}
		Graphics.device.Textures[1] = (Texture2D)_energyTexture;
		Graphics.device.SamplerStates[1] = SamplerState.PointWrap;
		foreach (EffectPass pass in _effect.effect.CurrentTechnique.Passes)
		{
			pass.Apply();
		}
	}
}
