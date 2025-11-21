using System;

namespace DuckGame;

[EditorGroup("Guns|Shotguns")]
public class CombatShotgun : Gun
{
	public StateBinding _readyToShootBinding = new StateBinding("_readyToShoot");

	private float _loadProgress = 1f;

	public float _loadWait;

	public bool _readyToShoot = true;

	private SpriteMap _loaderSprite;

	private SpriteMap _ammoSprite;

	private int _ammoMax = 6;

	public CombatShotgun(float xval, float yval)
		: base(xval, yval)
	{
		ammo = _ammoMax;
		_ammoType = new ATShotgun();
		_ammoType.range = 140f;
		wideBarrel = true;
		_type = "gun";
		graphic = new Sprite("combatShotgun");
		center = new Vec2(16f, 16f);
		collisionOffset = new Vec2(-12f, -3f);
		collisionSize = new Vec2(24f, 9f);
		_barrelOffsetTL = new Vec2(29f, 15f);
		_fireSound = "shotgunFire2";
		_kickForce = 5f;
		_fireRumble = RumbleIntensity.Kick;
		_numBulletsPerFire = 7;
		_manualLoad = true;
		_loaderSprite = new SpriteMap("combatShotgunLoader", 16, 16);
		_loaderSprite.center = new Vec2(8f, 8f);
		_ammoSprite = new SpriteMap("combatShotgunAmmo", 16, 16);
		_ammoSprite.center = new Vec2(8f, 8f);
		handOffset = new Vec2(0f, 1f);
		_holdOffset = new Vec2(4f, 0f);
		editorTooltip = "So many shells, what convenience!";
	}

	public override void Update()
	{
		_ammoSprite.frame = _ammoMax - ammo;
		base.Update();
		if (_readyToShoot)
		{
			_loadProgress = 1f;
			_loadWait = 0f;
		}
		if (!(_loadWait > 0f))
		{
			if (_loadProgress == 0f)
			{
				SFX.Play("shotgunLoad");
			}
			if (_loadProgress == 0.5f)
			{
				Reload();
			}
			_loadWait = 0f;
			if (_loadProgress < 1f)
			{
				_loadProgress += 0.1f;
				return;
			}
			_loadProgress = 1f;
			_readyToShoot = true;
			_readyToShoot = false;
		}
	}

	public override void OnPressAction()
	{
		if (_loadProgress >= 1f)
		{
			base.OnPressAction();
			_loadProgress = 0f;
			_loadWait = 1f;
		}
		else if (_loadWait == 1f)
		{
			_loadWait = 0f;
		}
	}

	public override void Draw()
	{
		base.Draw();
		Vec2 bOffset = new Vec2(13f, -1f);
		float offset = (float)Math.Sin(_loadProgress * 3.14f) * 3f;
		Draw(_loaderSprite, new Vec2(bOffset.x - 12f - offset, bOffset.y + 4f));
		Draw(_ammoSprite, new Vec2(-3f, -2f), 2);
	}
}
