using System;
using System.Collections.Generic;

namespace DuckGame;

public class NMFireGun : NMEvent
{
	public Gun gun;

	public byte fireIndex;

	public List<Bullet> bullets;

	private List<NMFireBullet> _fireEvents = new List<NMFireBullet>();

	public byte ammoType;

	public Vec2 position;

	public byte owner;

	public bool release;

	public bool onlyFireAction;

	private AmmoType _ammoTypeInstance;

	private byte _levelIndex;

	public NMFireGun()
	{
	}

	public NMFireGun(Gun g, List<Bullet> varBullets, byte fIndex, bool rel, byte ownerIndex = 4, bool onlyFireActionVar = false)
	{
		gun = g;
		bullets = varBullets;
		fireIndex = fIndex;
		owner = ownerIndex;
		release = rel;
		onlyFireAction = onlyFireActionVar;
		if (varBullets == null)
		{
			return;
		}
		bool first = true;
		foreach (Bullet b in varBullets)
		{
			if (first)
			{
				_ammoTypeInstance = b.ammo;
				ammoType = AmmoType.indexTypeMap[b.ammo.GetType()];
				position = new Vec2(b.x, b.y);
				first = false;
			}
			NMFireBullet fire = new NMFireBullet(b.range, b.bulletSpeed, b.angle);
			_fireEvents.Add(fire);
		}
	}

	public override void Activate()
	{
		if (_levelIndex != DuckNetwork.levelIndex)
		{
			return;
		}
		if (_fireEvents.Count > 0 && _fireEvents[0].typeInstance != null)
		{
			_fireEvents[0].typeInstance.MakeNetEffect(position, fromNetwork: true);
		}
		foreach (NMFireBullet fireEvent in _fireEvents)
		{
			fireEvent.connection = base.connection;
			fireEvent.DoActivate(position, (owner < DuckNetwork.profiles.Count) ? DuckNetwork.profiles[owner] : null);
		}
		if (gun == null)
		{
			return;
		}
		if (_fireEvents.Count > 0)
		{
			gun.OnNetworkBulletsFired(position);
		}
		gun.receivingPress = true;
		gun.hasFireEvents = true;
		gun.onlyFireAction = onlyFireAction;
		float w = gun._wait;
		gun._wait = 0f;
		if (!release)
		{
			bool wasLoaded = gun.loaded;
			gun.loaded = _fireEvents.Count > 0 || gun.ammo == 0;
			gun.PressAction();
			if (gun.fullAuto)
			{
				gun.HoldAction();
			}
			gun.loaded = wasLoaded;
		}
		else
		{
			gun.ReleaseAction();
		}
		gun._wait = w;
		gun.receivingPress = false;
		gun.hasFireEvents = false;
		gun.onlyFireAction = false;
		gun.bulletFireIndex = fireIndex;
	}

	protected override void OnSerialize()
	{
		base.OnSerialize();
		_serializedData.Write(DuckNetwork.levelIndex);
		_serializedData.Write((byte)_fireEvents.Count);
		foreach (NMFireBullet bullet in _fireEvents)
		{
			bullet.SerializePacketData();
			_serializedData.Write(bullet.serializedData);
			if (_fireEvents.Count > 0)
			{
				_ammoTypeInstance.WriteAdditionalData(_serializedData);
			}
		}
	}

	public override void OnDeserialize(BitBuffer d)
	{
		base.OnDeserialize(d);
		_levelIndex = d.ReadByte();
		byte num = d.ReadByte();
		for (int i = 0; i < num; i++)
		{
			NMFireBullet fire = new NMFireBullet();
			BitBuffer buf = d.ReadBitBuffer();
			fire.OnDeserialize(buf);
			AmmoType typeInstance = Activator.CreateInstance(AmmoType.indexTypeMap[ammoType]) as AmmoType;
			typeInstance.ReadAdditionalData(d);
			fire.typeInstance = typeInstance;
			_fireEvents.Add(fire);
		}
	}
}
