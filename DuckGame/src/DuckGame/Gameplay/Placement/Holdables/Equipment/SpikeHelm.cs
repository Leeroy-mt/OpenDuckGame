using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Equipment")]
public class SpikeHelm : Helmet
{
    public StateBinding _pokedBinding = new StateBinding(nameof(poked));

    protected List<Type> _pokables = new List<Type>();

    public PhysicsObject poked;

    public PhysicsObject oldPoke;

    public float oldPokeCooldown;

    private Depth _pokedOldDepth;

    private Duck _prevDuckOwner;

    private Duck _filteredDuck;

    private int throwCooldown;

    private Vec2 prevPoke;

    private Ragdoll prevRagdoll;

    private Vec2 spikePoint => Offset(new Vec2(0f, -8f));

    private Vec2 spikeDir => OffsetLocal(new Vec2(0f, -8f)).Normalized;

    public override bool action
    {
        get
        {
            if (poked == null)
            {
                if (_owner == null)
                {
                    return false;
                }
                return _owner.action;
            }
            return false;
        }
    }

    public SpikeHelm(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _pickupSprite = new SpriteMap("spikehelm", 17, 22, 0);
        _sprite = new SpriteMap("spikehelmWorn", 17, 22);
        graphic = _pickupSprite;
        Center = new Vec2(9f, 10f);
        _hasUnequippedCenter = true;
        collisionOffset = new Vec2(-6f, -4f);
        collisionSize = new Vec2(11f, 10f);
        _equippedCollisionOffset = new Vec2(-4f, -2f);
        _equippedCollisionSize = new Vec2(11f, 12f);
        _hasEquippedCollision = true;
        strappedOn = true;
        _sprite.Center = new Vec2(8f, 10f);
        base.Depth = 0.0001f;
        physicsMaterial = PhysicsMaterial.Metal;
        _isArmor = true;
        _equippedThickness = 3f;
        _pokables.Add(typeof(Crate));
        _pokables.Add(typeof(YellowBarrel));
        _pokables.Add(typeof(BlueBarrel));
        _pokables.Add(typeof(ExplosiveBarrel));
        _pokables.Add(typeof(LavaBarrel));
        _pokables.Add(typeof(CookedDuck));
        _pokables.Add(typeof(ECrate));
        _pokables.Add(typeof(TV));
        _pokables.Add(typeof(RagdollPart));
        _pokables.Add(typeof(Present));
        _pokables.Add(typeof(IceBlock));
        _pokables.Add(typeof(Desk));
        _pokables.Add(typeof(DeathCrate));
        SetEditorName("Pokeyhead");
        editorTooltip = "Looks sharp!";
    }

    public override void Crush(Thing pWith)
    {
        if (poked == null && !_pokables.Contains(pWith.GetType()))
        {
            crushed = true;
        }
    }

    public override void Update()
    {
        if (base.isServerForObject && base.equippedDuck != null && poked == null && !crushed)
        {
            if (throwCooldown > 0)
            {
                throwCooldown--;
            }
            if (base.equippedDuck.GetHeldByDuck() == null && _prevDuckOwner != null)
            {
                throwCooldown = 20;
                _filteredDuck = _prevDuckOwner;
            }
            _prevDuckOwner = base.equippedDuck.GetHeldByDuck();
            IEnumerable<MaterialThing> enumerable = Level.CheckRectAll<MaterialThing>(spikePoint + new Vec2(-2f, -2f), spikePoint + new Vec2(2f, 2f));
            Vec2 spikeDirection = spikeDir;
            Vec2 vel = base.equippedDuck.velocity;
            if (base.equippedDuck.ragdoll != null && base.equippedDuck.ragdoll.part1 != null)
            {
                vel = base.equippedDuck.ragdoll.part1.velocity;
            }
            else if (base.equippedDuck._trapped != null)
            {
                vel = base.equippedDuck._trapped.velocity;
            }
            foreach (MaterialThing t in enumerable)
            {
                Vec2 dif = vel - t.velocity;
                if (t == this || t == base.equippedDuck || t == oldPoke || (t.velocity.Length() < 0.5f && !(t is IAmADuck)) || Vec2.Dot(dif.Normalized, spikeDirection) < 0.65f || dif.Length() < 1.5f || _equippedDuck == null)
                {
                    continue;
                }
                if (t is IAmADuck)
                {
                    if (t is RagdollPart)
                    {
                        RagdollPart p = t as RagdollPart;
                        if (p.doll != null && p.doll.part1 != null && p.doll.part2 != null && p.doll.part3 != null)
                        {
                            p.doll.part2.hSpeed += base.equippedDuck.hSpeed * 0.75f;
                            p.doll.part2.vSpeed += base.equippedDuck.vSpeed * 0.75f;
                            p.doll.part1.hSpeed += base.equippedDuck.hSpeed * 0.75f;
                            p.doll.part1.vSpeed += base.equippedDuck.vSpeed * 0.75f;
                            base.equippedDuck.clip.Add(p.doll.part1);
                            base.equippedDuck.clip.Add(p.doll.part2);
                            base.equippedDuck.clip.Add(p.doll.part3);
                        }
                    }
                    MaterialThing m = t;
                    if (m != null)
                    {
                        if (!(t is Duck) || !(t as Duck).HasEquipment(typeof(Boots)) || (t as Duck).sliding || !(spikeDirection.Y < 0.5f) || !(Math.Abs(spikeDirection.X) < 0.2f))
                        {
                            Duck d = Duck.GetAssociatedDuck(m);
                            if ((d == null || (d != _equippedDuck && (_equippedDuck == null || !_equippedDuck.IsOwnedBy(d)))) && (d != _filteredDuck || throwCooldown <= 0))
                            {
                                Thing.Fondle(m, DuckNetwork.localConnection);
                                m.Destroy(new DTImpale(this));
                            }
                        }
                        continue;
                    }
                }
                if (_pokables.Contains(t.GetType()) && t is PhysicsObject && t.owner == null)
                {
                    t.owner = this;
                    poked = t as PhysicsObject;
                    poked.enablePhysics = false;
                    _pokedOldDepth = poked.Depth;
                    if (poked is Holdable)
                    {
                        (poked as Holdable)._hasOldDepth = true;
                        (poked as Holdable)._oldDepth = poked.Depth;
                    }
                    if (t is YellowBarrel)
                    {
                        (t as YellowBarrel).MakeHole(spikePoint, spikeDirection);
                    }
                    t.PlayCollideSound(ImpactedFrom.Top);
                    Fondle(poked);
                    prevRagdoll = base.equippedDuck.ragdoll;
                    break;
                }
            }
        }
        prevPoke = spikePoint;
        if (_equippedDuck == null)
        {
            Center = new Vec2(9f, 10f);
            base.Depth = 0.0001f;
            collisionOffset = new Vec2(-6f, -4f);
            collisionSize = new Vec2(11f, 10f);
        }
        base.Update();
        if (oldPokeCooldown > 0f)
        {
            oldPokeCooldown -= Maths.IncFrameTimer();
            if (oldPokeCooldown <= 0f)
            {
                oldPoke = null;
            }
        }
        if (poked == null || !base.isServerForObject)
        {
            return;
        }
        if (!poked.isServerForObject)
        {
            Fondle(poked);
        }
        poked.Position = Offset(new Vec2(1f, -9f));
        poked.lastGrounded = DateTime.Now;
        poked.visible = false;
        poked.solid = false;
        poked.grounded = true;
        if (poked.removeFromLevel || poked.Y < base.level.topLeft.Y - 2000f || !poked.active)
        {
            ReleasePokedObject();
        }
        else if (base.equippedDuck != null)
        {
            poked.hSpeed = base.duck.hSpeed;
            poked.vSpeed = base.duck.vSpeed;
            if (base.equippedDuck.ragdoll == null)
            {
                poked.solid = base.equippedDuck.velocity.Length() < 0.05f;
            }
            if (base.equippedDuck.ragdoll != null && prevRagdoll == null)
            {
                ReleasePokedObject();
            }
            prevRagdoll = base.equippedDuck.ragdoll;
        }
    }

    private void ReleasePokedObject()
    {
        if (poked != null)
        {
            poked.hSpeed = 0f;
            poked.vSpeed = -2f;
            poked.Y += 8f;
            poked.owner = null;
            poked.enablePhysics = true;
            poked.Depth = _pokedOldDepth;
            poked.visible = true;
            poked.solid = true;
            poked.grounded = false;
            poked.Angle = 0f;
            oldPoke = poked;
            oldPokeCooldown = 0.5f;
        }
        poked = null;
    }

    public override void Draw()
    {
        int frm = _sprite.frame;
        _sprite.frame = (crushed ? 1 : 0);
        (_pickupSprite as SpriteMap).frame = _sprite.frame;
        base.Draw();
        _sprite.frame = frm;
        (_pickupSprite as SpriteMap).frame = frm;
        if (poked != null)
        {
            poked.Position = Offset(new Vec2(1f, -9f));
            poked.Depth = base.Depth + 2;
            poked.Angle = _sprite.Angle;
            poked.Draw();
        }
    }
}
