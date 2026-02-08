using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class NMExplodingProp : NMEvent
{
    private byte _levelIndex;

    public List<Bullet> bullets;

    private AmmoType _ammoTypeInstance;

    public byte ammoType;

    public Vector2 position;

    private List<NMFireBullet> _fireEvents = new List<NMFireBullet>();

    public NMExplodingProp()
    {
    }

    public NMExplodingProp(List<Bullet> varBullets)
    {
        bullets = varBullets;
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
                position = new Vector2(b.X, b.Y);
                first = false;
            }
            NMFireBullet fire = new NMFireBullet(b.range, b.bulletSpeed, b.Angle);
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
            fireEvent.DoActivate(position, null);
        }
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
