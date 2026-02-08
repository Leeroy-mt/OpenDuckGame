using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public abstract class AmmoType
{
    private class ComplexDiffData
    {
        public AmmoType original;

        public List<ClassMember> bindings;

        public ComplexDiffData(Type pType)
        {
            original = Activator.CreateInstance(pType) as AmmoType;
            List<ClassMember> members = Editor.GetMembers(pType);
            bindings = new List<ClassMember>();
            foreach (ClassMember c in members)
            {
                if (!(c.name == "complexSync") && (c.type.IsPrimitive || c.type == typeof(Vector2) || c.type == typeof(Color)))
                {
                    bindings.Add(c);
                }
            }
        }
    }

    public float accuracy;

    public float range;

    public float rangeVariation;

    public float penetration;

    public float bulletSpeed = 28f;

    public float speedVariation = 3f;

    public float bulletLength = 100f;

    public Color bulletColor = Color.White;

    public bool softRebound;

    public bool rebound;

    public float bulletThickness = 1f;

    public bool affectedByGravity;

    public float airFrictionMultiplier = 1f;

    public bool deadly = true;

    public float barrelAngleDegrees;

    public bool immediatelyDeadly;

    public float weight;

    public float gravityMultiplier = 1f;

    public bool combustable;

    public float impactPower = 2f;

    public bool flawlessPipeTravel;

    public bool canBeReflected = true;

    public bool canTeleport = true;

    public int ownerSafety;

    public bool complexSync;

    private static Dictionary<Type, ComplexDiffData> _complexDiff = new Dictionary<Type, ComplexDiffData>();

    private static Map<byte, Type> _types = new Map<byte, Type>();

    public Sprite sprite;

    public Type bulletType = typeof(Bullet);

    public static Map<byte, Type> indexTypeMap => _types;

    public static void InitializeTypes()
    {
        if (MonoMain.moddingEnabled)
        {
            byte num = 0;
            {
                foreach (Type type in ManagedContent.AmmoTypes.SortedTypes)
                {
                    _types[num] = type;
                    num++;
                }
                return;
            }
        }
        List<Type> list = Editor.GetSubclasses(typeof(AmmoType)).ToList();
        byte index = 0;
        foreach (Type t in list)
        {
            _types[index] = t;
            index++;
        }
    }

    public void WriteComplexValues(BitBuffer pBuffer)
    {
        if (!complexSync)
        {
            return;
        }
        ComplexDiffData compare = null;
        if (!_complexDiff.TryGetValue(GetType(), out compare))
        {
            compare = (_complexDiff[GetType()] = new ComplexDiffData(GetType()));
        }
        foreach (ClassMember binding in compare.bindings)
        {
            object val = binding.GetValue(this);
            if (!object.Equals(binding.GetValue(compare.original), val))
            {
                pBuffer.Write(val: true);
                pBuffer.Write(val);
            }
            else
            {
                pBuffer.Write(val: false);
            }
        }
    }

    public void ReadComplexValues(BitBuffer pBuffer)
    {
        if (!complexSync)
        {
            return;
        }
        ComplexDiffData compare = null;
        if (!_complexDiff.TryGetValue(GetType(), out compare))
        {
            compare = (_complexDiff[GetType()] = new ComplexDiffData(GetType()));
        }
        foreach (ClassMember c in compare.bindings)
        {
            if (pBuffer.ReadBool())
            {
                c.SetValue(this, pBuffer.Read(c.type));
            }
        }
    }

    public virtual void MakeNetEffect(Vector2 pos, bool fromNetwork = false)
    {
    }

    public virtual void WriteAdditionalData(BitBuffer b)
    {
        WriteComplexValues(b);
    }

    public virtual void ReadAdditionalData(BitBuffer b)
    {
        ReadComplexValues(b);
    }

    public virtual void PopShell(float x, float y, int dir)
    {
    }

    public Bullet GetBullet(float x, float y, Thing owner = null, float angle = -1f, Thing firedFrom = null, float distance = -1f, bool tracer = false, bool network = true)
    {
        angle *= -1f;
        Bullet bullet = null;
        bullet = ((!(bulletType == typeof(Bullet))) ? (Activator.CreateInstance(bulletType, x, y, this, angle, owner, rebound, distance, tracer, network) as Bullet) : new Bullet(x, y, this, angle, owner, rebound, distance, tracer, network));
        bullet.firedFrom = firedFrom;
        bullet.color = bulletColor;
        return bullet;
    }

    public virtual Bullet FireBullet(Vector2 position, Thing owner = null, float angle = 0f, Thing firedFrom = null)
    {
        Bullet bullet = GetBullet(position.X, position.Y, owner, angle, firedFrom);
        Level.current.AddThing(bullet);
        return bullet;
    }

    public virtual void OnHit(bool destroyed, Bullet b)
    {
    }
}
