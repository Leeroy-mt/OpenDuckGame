namespace DuckGame;

[EditorGroup("Guns|Explosives")]
[BaggedProperty("isInDemo", true)]
public class Bazooka : TampingWeapon
{
	public Bazooka(float xval, float yval)
		: base(xval, yval)
	{
		ammo = 99;
		_ammoType = new ATMissile();
		_type = "gun";
		graphic = new Sprite("bazooka");
		center = new Vec2(15f, 5f);
		collisionOffset = new Vec2(-15f, -4f);
		collisionSize = new Vec2(30f, 10f);
		_barrelOffsetTL = new Vec2(29f, 4f);
		_fireSound = "missile";
		_kickForce = 4f;
		_fireRumble = RumbleIntensity.Light;
		_holdOffset = new Vec2(-2f, -2f);
		loseAccuracy = 0.1f;
		maxAccuracyLost = 0.6f;
		_bio = "Old faithful, the 9MM pistol.";
		_editorName = "Bazooka";
		editorTooltip = "Funny name, serious firepower. Launches an explosive missile that can destroy terrain.";
		physicsMaterial = PhysicsMaterial.Metal;
	}

	public override void OnPressAction()
	{
		if (_tamped)
		{
			base.OnPressAction();
			int num = 0;
			for (int i = 0; i < 14; i++)
			{
				MusketSmoke smoke = new MusketSmoke(base.barrelPosition.x - 16f + Rando.Float(32f), base.barrelPosition.y - 16f + Rando.Float(32f));
				smoke.depth = 0.9f + (float)i * 0.001f;
				if (num < 6)
				{
					smoke.move -= base.barrelVector * Rando.Float(0.1f);
				}
				if (num > 5 && num < 10)
				{
					smoke.fly += base.barrelVector * (2f + Rando.Float(7.8f));
				}
				Level.Add(smoke);
				num++;
			}
			_tampInc = 0f;
			if (infinite.value)
			{
				_tampTime = 0.8f;
			}
			else
			{
				_tampTime = 0.5f;
			}
			_tamped = false;
		}
		else if (!_raised && owner is Duck { grounded: not false } duckOwner)
		{
			duckOwner.immobilized = true;
			duckOwner.sliding = false;
			_rotating = true;
		}
	}
}
