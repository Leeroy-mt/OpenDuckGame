using System;

namespace DuckGame;

[EditorGroup("Guns|Misc")]
[BaggedProperty("isFatal", false)]
[BaggedProperty("isInDemo", true)]
[BaggedProperty("previewPriority", true)]
public class NetGun : Gun
{
	private SpriteMap _barrelSteam;

	private SpriteMap _netGunGuage;

	public NetGun(float xval, float yval)
		: base(xval, yval)
	{
		ammo = 4;
		_ammoType = new ATLaser();
		_ammoType.range = 170f;
		_ammoType.accuracy = 0.8f;
		_ammoType.penetration = -1f;
		_type = "gun";
		graphic = new Sprite("netGun");
		center = new Vec2(16f, 16f);
		collisionOffset = new Vec2(-8f, -4f);
		collisionSize = new Vec2(16f, 9f);
		_barrelOffsetTL = new Vec2(27f, 14f);
		_fireSound = "smg";
		_fullAuto = true;
		_fireWait = 1f;
		_kickForce = 3f;
		_fireRumble = RumbleIntensity.Kick;
		_netGunGuage = new SpriteMap("netGunGuage", 8, 8);
		_barrelSteam = new SpriteMap("steamPuff", 16, 16);
		_barrelSteam.center = new Vec2(0f, 14f);
		_barrelSteam.AddAnimation("puff", 0.4f, false, 0, 1, 2, 3, 4, 5, 6, 7);
		_barrelSteam.SetAnimation("puff");
		_barrelSteam.speed = 0f;
		_bio = "C02 powered, shoots nets, traps ducks. Is that stubborn duck not moving? Why not trap it, and put it where it belongs.";
		_editorName = "Net Gun";
		editorTooltip = "Fires entangling nets that hold Ducks in place *evil moustache twirl*";
		isFatal = false;
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	public override void Update()
	{
		_netGunGuage.frame = 4 - Math.Min(ammo + 1, 4);
		if (_barrelSteam.speed > 0f && _barrelSteam.finished)
		{
			_barrelSteam.speed = 0f;
		}
		base.Update();
	}

	public override void Draw()
	{
		base.Draw();
		if (_barrelSteam.speed > 0f)
		{
			_barrelSteam.alpha = 0.6f;
			Draw(_barrelSteam, new Vec2(9f, 1f));
		}
		Draw(_netGunGuage, new Vec2(-4f, -4f));
	}

	public override void OnPressAction()
	{
		if (ammo > 0)
		{
			ammo--;
			if (base.duck != null)
			{
				RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(_fireRumble, RumbleDuration.Pulse, RumbleFalloff.None));
			}
			SFX.Play("netGunFire");
			_barrelSteam.speed = 1f;
			_barrelSteam.frame = 0;
			ApplyKick();
			Vec2 pos = Offset(base.barrelOffset);
			if (!receivingPress)
			{
				Net n = new Net(pos.x, pos.y - 2f, base.duck);
				Level.Add(n);
				Fondle(n);
				if (owner != null)
				{
					n.responsibleProfile = owner.responsibleProfile;
				}
				n.clip.Add(owner as MaterialThing);
				n.hSpeed = base.barrelVector.x * 10f;
				n.vSpeed = base.barrelVector.y * 7f - 1.5f;
			}
		}
		else
		{
			DoAmmoClick();
		}
	}

	public override void Fire()
	{
	}
}
