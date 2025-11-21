namespace DuckGame;

[EditorGroup("Guns|Explosives")]
[BaggedProperty("isFatal", false)]
[BaggedProperty("previewPriority", true)]
public class BananaCluster : Gun
{
	private SpriteMap _sprite;

	private int _ammoMax = 3;

	public BananaCluster(float xval, float yval)
		: base(xval, yval)
	{
		ammo = 3;
		_ammoType = new ATShrapnel();
		_type = "gun";
		_sprite = new SpriteMap("banana", 16, 16);
		_sprite.frame = 4;
		graphic = _sprite;
		center = new Vec2(8f, 8f);
		collisionOffset = new Vec2(-6f, -4f);
		collisionSize = new Vec2(12f, 11f);
		physicsMaterial = PhysicsMaterial.Rubber;
		_holdOffset = new Vec2(0f, 2f);
		base.bouncy = 0.4f;
		friction = 0.05f;
		editorTooltip = "Need more than one banana? Have I got news for you...";
		isFatal = false;
	}

	public override void Update()
	{
		_sprite.frame = 4 + _ammoMax - ammo;
		if (ammo == 0 && owner != null)
		{
			if (owner is Duck duckOwner)
			{
				duckOwner.ThrowItem();
			}
			Level.Remove(this);
		}
		if (owner == null && ammo == 1)
		{
			Banana thing = new Banana(base.x, base.y)
			{
				hSpeed = hSpeed,
				vSpeed = vSpeed
			};
			Level.Remove(this);
			Level.Add(thing);
		}
		base.Update();
	}

	public override void OnPressAction()
	{
		if (ammo <= 0)
		{
			return;
		}
		ammo--;
		SFX.Play("smallSplat", 1f, Rando.Float(-0.6f, 0.6f));
		if (!(owner is Duck duckOwner))
		{
			return;
		}
		float addSpeedX = 0f;
		float addSpeedY = 0f;
		if (duckOwner.inputProfile.Down("LEFT"))
		{
			addSpeedX -= 3f;
		}
		if (duckOwner.inputProfile.Down("RIGHT"))
		{
			addSpeedX += 3f;
		}
		if (duckOwner.inputProfile.Down("UP"))
		{
			addSpeedY -= 3f;
		}
		if (duckOwner.inputProfile.Down("DOWN"))
		{
			addSpeedY += 3f;
		}
		if (base.isServerForObject)
		{
			Banana b = new Banana(base.barrelPosition.x, base.barrelPosition.y);
			if (!duckOwner.crouch)
			{
				b.hSpeed = (float)offDir * Rando.Float(3f, 3.5f) + addSpeedX;
				b.vSpeed = -1.5f + addSpeedY + Rando.Float(-0.5f, -1f);
			}
			b.EatBanana();
			b.clip.Add(duckOwner);
			duckOwner.clip.Add(b);
			Level.Add(b);
		}
	}
}
