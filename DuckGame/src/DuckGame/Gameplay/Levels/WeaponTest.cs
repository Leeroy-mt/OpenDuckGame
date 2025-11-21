using System;

namespace DuckGame;

public class WeaponTest : DuckGameTestArea
{
	private Type[] _types;

	public WeaponTest(Editor e, params Type[] types)
		: base(e, "Content\\levels\\weaponTest.lev")
	{
		_types = types;
	}

	public override void Initialize()
	{
		base.Initialize();
		int index = 0;
		int xpos = 240;
		Type[] types = _types;
		foreach (Type t in types)
		{
			Thing thing = Thing.Instantiate(t);
			thing.x = xpos + index * 22;
			thing.y = 200f;
			Level.Add(thing);
			Thing thing2 = Thing.Instantiate(t);
			thing2.x = xpos + index * 22 + 8;
			thing2.y = 200f;
			Level.Add(thing2);
			index++;
		}
		Duck duk = new Duck(210f, 200f, Profiles.DefaultPlayer1);
		Level.Add(duk);
		(Level.current as DeathmatchLevel).followCam.Add(duk);
		duk = new Duck(400f, 200f, Profiles.DefaultPlayer2);
		Level.Add(duk);
		(Level.current as DeathmatchLevel).followCam.Add(duk);
		Level.Add(new PhysicsChain(300f, 100f));
		Level.Add(new PhysicsRope(350f, 100f));
	}
}
