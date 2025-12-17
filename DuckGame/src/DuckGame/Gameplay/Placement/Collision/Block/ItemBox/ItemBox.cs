using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

[EditorGroup("Spawns")]
[BaggedProperty("isInDemo", true)]
[BaggedProperty("previewPriority", true)]
public class ItemBox : Block, IPathNodeBlocker, IContainAThing
{
    public StateBinding _positionBinding = new StateBinding(nameof(position));

    public StateBinding _containedObjectBinding = new StateBinding(nameof(containedObject));

    public StateBinding _boxStateBinding = new ItemBoxFlagBinding();

    public StateBinding _chargingBinding = new StateBinding(nameof(charging), 9);

    public StateBinding _netDisarmIndexBinding = new StateBinding(nameof(netDisarmIndex));

    public byte netDisarmIndex;

    public byte localNetDisarm;

    public float bounceAmount;

    public bool _hit;

    public int charging;

    public float startY = -99999f;

    protected List<PhysicsObject> _aboveList = new List<PhysicsObject>();

    private PhysicsObject _containedObject;

    protected SpriteMap _sprite;

    public bool _canBounce = true;

    private int chargeDelay;

    public PhysicsObject lastSpawnItem;

    protected PhysicsObject containContext;

    public PhysicsObject containedObject
    {
        get
        {
            return _containedObject;
        }
        set
        {
            _containedObject = value;
        }
    }

    public Type contains { get; set; }

    public bool canBounce => _canBounce;

    public ItemBox(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("itemBox", 16, 16);
        graphic = _sprite;
        base.layer = Layer.Foreground;
        center = new Vec2(8f, 8f);
        collisionSize = new Vec2(16f, 16f);
        collisionOffset = new Vec2(-8f, -8f);
        base.depth = 0.5f;
        _canFlip = false;
        _placementCost += 4;
        editorTooltip = "Spawns a copy of the contained item any time it's used. Recharges after a short duration.";
    }

    public override void Initialize()
    {
        UpdateContainedObject();
        base.Initialize();
    }

    public void Pop()
    {
        Bounce();
        if (!_hit)
        {
            SpawnItem();
        }
    }

    public void Bounce()
    {
        if (!_canBounce)
        {
            return;
        }
        bounceAmount = 8f;
        _canBounce = false;
        if (Network.isActive)
        {
            netDisarmIndex++;
            return;
        }
        _aboveList = Level.CheckRectAll<PhysicsObject>(base.topLeft + new Vec2(1f, -4f), base.bottomRight + new Vec2(-1f, -12f)).ToList();
        foreach (PhysicsObject p in _aboveList)
        {
            if (!p.grounded && !(p.vSpeed > 0f) && p.vSpeed != 0f)
            {
                continue;
            }
            Fondle(p);
            p.y -= 2f;
            p.vSpeed = -3f;
            if (p is Duck d)
            {
                if (!d.isServerForObject)
                {
                    Send.Message(new NMDisarmVertical(d, -3f), d.connection);
                }
                else
                {
                    d.Disarm(this);
                }
            }
        }
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (from == ImpactedFrom.Bottom && with.isServerForObject)
        {
            with.Fondle(this);
            if (with is Duck duck)
            {
                RumbleManager.AddRumbleEvent(duck.profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Pulse, RumbleFalloff.None));
            }
            if (containedObject != null)
            {
                with.Fondle(containedObject);
            }
            Pop();
        }
    }

    public virtual void UpdateCharging()
    {
        if (!base.isServerForObject)
        {
            return;
        }
        if (charging > 0)
        {
            chargeDelay++;
            if (chargeDelay >= 50)
            {
                charging -= 50;
                chargeDelay = 0;
            }
        }
        else
        {
            chargeDelay = 0;
            charging = 0;
            _hit = false;
        }
    }

    public override void PrepareForHost()
    {
        UpdateContainedObject();
        if (containedObject != null)
        {
            containedObject.PrepareForHost();
        }
        base.PrepareForHost();
    }

    public virtual void UpdateContainedObject()
    {
        if (Network.isActive && (base.isServerForObject || Thing.loadingLevel != null) && containedObject == null)
        {
            containedObject = GetSpawnItem();
            if (containedObject != null)
            {
                containedObject.visible = false;
                containedObject.active = false;
                containedObject.position = position;
                Level.Add(containedObject);
            }
        }
    }

    public override void Update()
    {
        UpdateContainedObject();
        _aboveList.Clear();
        if (startY < -9999f)
        {
            startY = base.y;
        }
        _sprite.frame = (_hit ? 1 : 0);
        if (contains == null && containedObject == null && !(this is ItemBoxRandom))
        {
            _sprite.frame = 1;
        }
        if (netDisarmIndex != localNetDisarm)
        {
            localNetDisarm = netDisarmIndex;
            _aboveList = Level.CheckRectAll<PhysicsObject>(base.topLeft + new Vec2(1f, -4f), base.bottomRight + new Vec2(-1f, -12f)).ToList();
            foreach (PhysicsObject p in _aboveList)
            {
                if (base.isServerForObject && p.owner == null)
                {
                    Fondle(p);
                }
                if (!p.isServerForObject || (!p.grounded && !(p.vSpeed > 0f) && p.vSpeed != 0f))
                {
                    continue;
                }
                p.y -= 2f;
                p.vSpeed = -3f;
                if (p is Duck d)
                {
                    if (!d.isServerForObject)
                    {
                        Send.Message(new NMDisarmVertical(d, -3f), d.connection);
                    }
                    else
                    {
                        d.Disarm(this);
                    }
                }
            }
        }
        UpdateCharging();
        if (bounceAmount > 0f)
        {
            bounceAmount -= 0.8f;
        }
        else
        {
            bounceAmount = 0f;
        }
        base.y -= bounceAmount;
        if (!_canBounce)
        {
            if (base.y < startY)
            {
                base.y += 0.8f + Math.Abs(base.y - startY) * 0.4f;
            }
            if (base.y > startY)
            {
                base.y -= 0.8f - Math.Abs(base.y - startY) * 0.4f;
            }
            if (Math.Abs(base.y - startY) < 0.8f)
            {
                _canBounce = true;
                base.y = startY;
            }
        }
    }

    public virtual PhysicsObject GetSpawnItem()
    {
        if (contains == null)
        {
            return null;
        }
        IReadOnlyPropertyBag containsBag = ContentProperties.GetBag(contains);
        if (Network.isActive && !containsBag.GetOrDefault("isOnlineCapable", defaultValue: true))
        {
            return Activator.CreateInstance(typeof(Pistol), Editor.GetConstructorParameters(typeof(Pistol))) as PhysicsObject;
        }
        return Editor.CreateThing(contains) as PhysicsObject;
    }

    public virtual void SpawnItem()
    {
        charging = 500;
        if (containContext == null && ((!Network.isActive && contains == null && !(this is ItemBoxRandom)) || (Network.isActive && containedObject == null)))
        {
            return;
        }
        PhysicsObject newThing = containContext;
        if (newThing == null)
        {
            if (!Network.isActive)
            {
                newThing = GetSpawnItem();
            }
            else
            {
                if (containedObject == null)
                {
                    return;
                }
                newThing = containedObject;
                newThing.active = true;
                newThing.visible = true;
            }
        }
        _hit = true;
        lastSpawnItem = newThing;
        if (newThing == null)
        {
            return;
        }
        foreach (PhysicsObject obj in _aboveList)
        {
            newThing.clip.Add(obj);
        }
        newThing.x = base.x;
        newThing.bottom = base.bottom;
        newThing.y -= 12f;
        newThing.vSpeed = -3.5f;
        newThing.clip.Add(this);
        if (newThing is Gun)
        {
            Gun g = newThing as Gun;
            if (g.CanSpin())
            {
                g.angleDegrees = 180f;
            }
        }
        Block leftWall = Level.CheckPoint<Block>(position + new Vec2(-16f, 0f));
        if (leftWall != null)
        {
            newThing.clip.Add(leftWall);
        }
        Block rightWall = Level.CheckPoint<Block>(position + new Vec2(16f, 0f));
        if (rightWall != null)
        {
            newThing.clip.Add(rightWall);
        }
        if (!Network.isActive || this is PurpleBlock)
        {
            Level.Add(newThing);
        }
        if (!Network.isActive)
        {
            SFX.Play("hitBox");
        }
        else if (base.isServerForObject)
        {
            NetSoundEffect.Play("itemBoxHit");
        }
        Thing.Fondle(newThing, DuckNetwork.localConnection);
        containedObject = null;
    }

    public static List<Type> GetPhysicsObjects(EditorGroup group)
    {
        return Editor.ThingTypes.Where(delegate (Type t)
        {
            if (t.IsAbstract || !t.IsSubclassOf(typeof(PhysicsObject)))
            {
                return false;
            }
            if (t.GetCustomAttributes(typeof(EditorGroupAttribute), inherit: false).Length == 0)
            {
                return false;
            }
            IReadOnlyPropertyBag bag = ContentProperties.GetBag(t);
            return (bag.GetOrDefault("canSpawn", defaultValue: true) && (!Network.isActive || !bag.GetOrDefault("noRandomSpawningOnline", defaultValue: false)) && (!Network.isActive || bag.GetOrDefault("isOnlineCapable", defaultValue: true)) && (Main.isDemo || !bag.GetOrDefault("onlySpawnInDemo", defaultValue: false))) ? true : false;
        }).ToList();
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk binaryClassChunk = base.Serialize();
        binaryClassChunk.AddProperty("contains", Editor.SerializeTypeName(contains));
        return binaryClassChunk;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        contains = Editor.DeSerializeTypeName(node.GetProperty<string>("contains"));
        return true;
    }

    public override DXMLNode LegacySerialize()
    {
        DXMLNode dXMLNode = base.LegacySerialize();
        dXMLNode.Add(new DXMLNode("contains", (contains != null) ? contains.AssemblyQualifiedName : ""));
        return dXMLNode;
    }

    public override bool LegacyDeserialize(DXMLNode node)
    {
        base.LegacyDeserialize(node);
        DXMLNode typeNode = node.Element("contains");
        if (typeNode != null)
        {
            Type t = Editor.GetType(typeNode.Value);
            contains = t;
        }
        return true;
    }

    public override ContextMenu GetContextMenu()
    {
        FieldBinding binding = new FieldBinding(this, "contains");
        EditorGroupMenu obj = base.GetContextMenu() as EditorGroupMenu;
        obj.InitializeGroups(new EditorGroup(typeof(PhysicsObject)), binding);
        return obj;
    }

    public override string GetDetailsString()
    {
        string containString = "EMPTY";
        if (contains != null)
        {
            containString = contains.Name;
        }
        if (contains == null)
        {
            return base.GetDetailsString();
        }
        return base.GetDetailsString() + "Contains: " + containString;
    }

    public override void DrawHoverInfo()
    {
        string containString = "EMPTY";
        if (contains != null)
        {
            containString = contains.Name;
        }
        Graphics.DrawString(containString, position + new Vec2((0f - Graphics.GetStringWidth(containString)) / 2f, -16f), Color.White, 0.9f);
    }
}
