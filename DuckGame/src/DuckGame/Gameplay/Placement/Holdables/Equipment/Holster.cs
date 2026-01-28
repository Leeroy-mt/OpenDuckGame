using System;
using System.Linq;

namespace DuckGame;

[EditorGroup("Equipment")]
[BaggedProperty("isInDemo", true)]
public class Holster : Equipment
{
    public StateBinding _containedObjectBinding = new StateBinding(nameof(netContainedObject));

    public StateBinding _netRaiseBinding = new StateBinding(nameof(netRaise));

    public StateBinding _netChainedBinding = new StateBinding(nameof(netChained));

    public bool netRaise;

    protected SpriteMap _sprite;

    protected SpriteMap _overPart;

    protected SpriteMap _underPart;

    public EditorProperty<bool> infinite = new EditorProperty<bool>(val: false);

    public EditorProperty<bool> chained = new EditorProperty<bool>(val: false);

    private Holdable _containedObject;

    private RenderTarget2D _preview;

    private Sprite _previewSprite;

    private Type _contains;

    private Sprite _chain;

    private Sprite _lock;

    private Vec2 _prevPos = Vec2.Zero;

    private float afterDrawAngle = -999999f;

    private float _chainSway;

    private float _chainSwayVel;

    protected float backOffset = -8f;

    public override NetworkConnection connection
    {
        get
        {
            return base.connection;
        }
        set
        {
            base.connection = value;
            if (containedObject != null)
            {
                containedObject.connection = value;
            }
        }
    }

    public bool netChained
    {
        get
        {
            return chained.value;
        }
        set
        {
            chained.value = value;
        }
    }

    public Holdable containedObject
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

    public Holdable netContainedObject
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

    public Type contains
    {
        get
        {
            return _contains;
        }
        set
        {
            _contains = value;
            if (!Level.skipInitialize)
            {
                if (_preview == null)
                {
                    _preview = new RenderTarget2D(32, 32);
                }
                Thing t = GetContainedInstance();
                if (t != null)
                {
                    _previewSprite = t.GetEditorImage(32, 32, transparentBack: true, null, _preview);
                }
            }
        }
    }

    public Thing GetContainedInstance(Vec2 pos = default(Vec2))
    {
        if (contains == null)
        {
            return null;
        }
        object[] p = Editor.GetConstructorParameters(contains);
        if (p.Count() > 1)
        {
            p[0] = pos.X;
            p[1] = pos.Y;
        }
        PhysicsObject o = Editor.CreateThing(contains, p) as PhysicsObject;
        if (o is Gun)
        {
            (o as Gun).infinite = infinite;
        }
        return o;
    }

    public void SetContainedObject(Holdable h)
    {
        if (_containedObject != null)
        {
            _containedObject.visible = true;
            Fondle(_containedObject);
            _containedObject.owner = null;
            _containedObject = null;
        }
        if (h != null)
        {
            _containedObject = h;
            h.lastGrounded = DateTime.Now;
            h.visible = false;
        }
    }

    public virtual void EjectItem()
    {
        if (containedObject != null)
        {
            SFX.PlaySynchronized("pelletgunBad", 1f, Rando.Float(0.1f, 0.1f));
            containedObject.hSpeed = (float)(-owner.offDir) * 6f;
            containedObject.vSpeed = -1.5f;
            if (base.duck != null)
            {
                base.duck._lastHoldItem = containedObject;
                base.duck._timeSinceThrow = 0;
            }
            SetContainedObject(null);
        }
    }

    public virtual void EjectItem(Vec2 pSpeed)
    {
        if (containedObject != null)
        {
            SFX.PlaySynchronized("pelletgunBad", 1f, Rando.Float(0.1f, 0.1f));
            containedObject.hSpeed = pSpeed.X;
            containedObject.vSpeed = pSpeed.Y;
            SetContainedObject(null);
        }
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
        Graphics.DrawString(containString, Position + new Vec2((0f - Graphics.GetStringWidth(containString)) / 2f, -16f), Color.White, 0.9f);
    }

    public Holster(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _chain = new Sprite("holsterChain");
        _chain.Center = new Vec2(3f, 3f);
        _lock = new Sprite("holsterLock");
        _lock.Center = new Vec2(3f, 2f);
        _sprite = new SpriteMap("holster", 12, 12);
        _overPart = new SpriteMap("holster_over", 10, 3);
        _overPart.Center = new Vec2(6f, -1f);
        _underPart = new SpriteMap("holster_under", 8, 9);
        _underPart.Center = new Vec2(10f, 8f);
        graphic = _sprite;
        collisionOffset = new Vec2(-5f, -5f);
        collisionSize = new Vec2(10f, 10f);
        Center = new Vec2(6f, 6f);
        physicsMaterial = PhysicsMaterial.Wood;
        _equippedDepth = 4;
        _wearOffset = new Vec2(1f, 1f);
        editorTooltip = "Lets you carry around an additional item!";
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        if (owner != null || containedObject != null)
        {
            return false;
        }
        return base.OnDestroy(type);
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor) && GetContainedInstance(Position) is Holdable t)
        {
            Level.Add(t);
            SetContainedObject(t);
            if (Network.isActive && Thing.loadingLevel != null && _containedObject != null)
            {
                _containedObject.PrepareForHost();
            }
        }
        base.Initialize();
    }

    public override void Update()
    {
        if (_equippedDuck != null && base.duck == null)
        {
            return;
        }
        if (destroyed)
        {
            base.Alpha -= 0.05f;
        }
        if (base.Alpha < 0f)
        {
            Level.Remove(this);
        }
        if (base.isServerForObject)
        {
            netRaise = false;
            if (_equippedDuck != null && _equippedDuck.inputProfile != null && _equippedDuck.inputProfile.Down("UP"))
            {
                netRaise = true;
            }
            if (owner == null && base.equippedDuck == null)
            {
                base.AngleDegrees = 0f;
            }
            if (containedObject != null)
            {
                PositionContainedObject();
                _containedObject.HolsterUpdate(this);
                weight = containedObject.weight;
                if (base.duck != null)
                {
                    containedObject.owner = base.duck;
                }
                else
                {
                    containedObject.owner = this;
                }
                if (base.duck != null && base.duck.ragdoll != null)
                {
                    containedObject.solid = false;
                    containedObject.grounded = false;
                }
                else
                {
                    if (base.equippedDuck != null)
                    {
                        containedObject.solid = base.equippedDuck.velocity.Length() < 0.05f;
                    }
                    else
                    {
                        containedObject.solid = base.velocity.Length() < 0.05f;
                    }
                    containedObject.grounded = true;
                }
                if (!containedObject.isServerForObject && !(containedObject is IAmADuck))
                {
                    Fondle(containedObject);
                }
                if (containedObject.removeFromLevel || containedObject.Y < base.level.topLeft.Y - 2000f || !containedObject.active || !containedObject.isServerForObject)
                {
                    SetContainedObject(null);
                }
            }
            if (containedObject is Gun && Level.CheckRect<FunBeam>(containedObject.Position + new Vec2(-4f, -4f), containedObject.Position + new Vec2(4f, 4f)) != null)
            {
                (containedObject as Gun).triggerAction = true;
            }
            if (containedObject is RagdollPart && (containedObject as RagdollPart).doll != null && (containedObject as RagdollPart).doll.part1 != null && (containedObject as RagdollPart).doll.part2 != null && (containedObject as RagdollPart).doll.part3 != null)
            {
                if ((containedObject as RagdollPart).doll.part1.X < (containedObject as RagdollPart).doll.part3.X - 4f)
                {
                    (containedObject as RagdollPart).doll.part1.X = (containedObject as RagdollPart).doll.part3.X - 4f;
                }
                if ((containedObject as RagdollPart).doll.part1.X > (containedObject as RagdollPart).doll.part3.X + 4f)
                {
                    (containedObject as RagdollPart).doll.part1.X = (containedObject as RagdollPart).doll.part3.X + 4f;
                }
                Vec2 topTarget = (containedObject as RagdollPart).doll.part3.Position + new Vec2(0f, -11f);
                Vec2 middleTarget = (containedObject as RagdollPart).doll.part3.Position + new Vec2(0f, -5f);
                (containedObject as RagdollPart).doll.part1.X = Lerp.FloatSmooth((containedObject as RagdollPart).doll.part1.X, topTarget.X, 0.5f);
                (containedObject as RagdollPart).doll.part1.Y = Lerp.FloatSmooth((containedObject as RagdollPart).doll.part1.Y, topTarget.Y, 0.5f);
                (containedObject as RagdollPart).doll.part2.X = Lerp.FloatSmooth((containedObject as RagdollPart).doll.part1.X, middleTarget.X, 0.5f);
                (containedObject as RagdollPart).doll.part2.Y = Lerp.FloatSmooth((containedObject as RagdollPart).doll.part1.Y, middleTarget.Y, 0.5f);
                topTarget = (topTarget - (containedObject as RagdollPart).doll.part3.Position).Normalized;
                (containedObject as RagdollPart).doll.part1.vSpeed = topTarget.Y;
                (containedObject as RagdollPart).doll.part2.vSpeed = topTarget.Y;
                (containedObject as RagdollPart).doll.part1.hSpeed = topTarget.X;
                (containedObject as RagdollPart).doll.part2.hSpeed = topTarget.X;
                (containedObject as RagdollPart).doll.part1.vSpeed *= 0.8f;
                (containedObject as RagdollPart).doll.part1.hSpeed *= 0.8f;
                (containedObject as RagdollPart).doll.part2.vSpeed *= 0.8f;
                (containedObject as RagdollPart).doll.part2.hSpeed *= 0.8f;
            }
            if (containedObject != null && !(containedObject is Equipment))
            {
                containedObject.UpdateAction();
                if (containedObject is TapedGun)
                {
                    (containedObject as TapedGun).UpdateSubActions(containedObject.triggerAction);
                }
            }
        }
        base.Update();
    }

    protected virtual void DrawParts()
    {
        if (_equippedDuck != null)
        {
            _ = owner.Depth;
            _overPart.flipH = owner.offDir <= 0;
            _overPart.Angle = Angle;
            _overPart.Alpha = base.Alpha;
            _overPart.Scale = base.Scale;
            _overPart.Depth = owner.Depth + 5;
            Graphics.Draw(_overPart, base.X, base.Y);
            _underPart.flipH = owner.offDir <= 0;
            _underPart.Angle = Angle;
            _underPart.Alpha = base.Alpha;
            _underPart.Scale = base.Scale;
            if (_equippedDuck.ragdoll != null && _equippedDuck.ragdoll.part2 != null)
            {
                _underPart.Depth = _equippedDuck.ragdoll.part2.Depth + -11;
            }
            else
            {
                _underPart.Depth = owner.Depth + -7;
            }
            Graphics.Draw(_underPart, base.X, base.Y);
        }
    }

    private void PositionContainedObject()
    {
        if (_equippedDuck != null)
        {
            _containedObject.Position = Offset(new Vec2(backOffset, -4f) + containedObject.holsterOffset);
            _containedObject.Depth = owner.Depth + -14;
            _containedObject.AngleDegrees = ((owner.offDir > 0) ? containedObject.holsterAngle : (0f - containedObject.holsterAngle)) + base.AngleDegrees;
            _containedObject.offDir = (sbyte)((owner.offDir > 0) ? 1 : (-1));
            if (containedObject is RagdollPart)
            {
                _containedObject.Position = Offset(new Vec2(backOffset, 0f));
                _containedObject.AngleDegrees += ((owner.offDir > 0) ? 90 : (-90));
                if (base.duck != null && base.duck.ragdoll == null)
                {
                    afterDrawAngle = _containedObject.AngleDegrees;
                    _containedObject.AngleDegrees -= base.duck.hSpeed * 3f;
                }
            }
            if (owner is Duck && (owner as Duck).ragdoll != null)
            {
                RagdollPart p = (owner as Duck).ragdoll.part2;
                if (p != null)
                {
                    _containedObject.Depth = p.Depth + -14;
                }
            }
        }
        else
        {
            _containedObject.Position = Offset(new Vec2(backOffset + 6f, -2f) + containedObject.holsterOffset);
            _containedObject.Depth = base.Depth + -14;
            _containedObject.AngleDegrees = ((offDir > 0) ? containedObject.holsterAngle : (0f - containedObject.holsterAngle)) + base.AngleDegrees;
            _containedObject.offDir = (sbyte)((offDir > 0) ? 1 : (-1));
        }
    }

    public override void Draw()
    {
        if (Level.current is Editor && _previewSprite != null)
        {
            _previewSprite.Depth = base.Depth + 1;
            _previewSprite.Scale = new Vec2(0.5f, 0.5f);
            _previewSprite.Center = new Vec2(16f, 16f);
            Graphics.Draw(_previewSprite, base.X, base.Y);
        }
        if (_equippedDuck != null)
        {
            graphic = null;
        }
        else
        {
            graphic = _sprite;
        }
        base.Draw();
        if (_equippedDuck != null && base.duck == null)
        {
            return;
        }
        DrawParts();
        if (_containedObject != null)
        {
            _ = offDir;
            PositionContainedObject();
            if (chained.value)
            {
                float xOffChange = ((_equippedDuck != null) ? 0f : 8f);
                _chain.CenterOrigin();
                _chain.Depth = _underPart.Depth + 1;
                _chain.AngleDegrees = base.AngleDegrees - (float)(45 * offDir);
                Vec2 chainOff = Offset(new Vec2(-11f + xOffChange, -3f));
                Graphics.Draw(_chain, chainOff.X, chainOff.Y);
                _lock.AngleDegrees = _chainSway;
                float desiredDegrees = ((owner != null) ? owner.hSpeed : hSpeed) * 10f;
                _chainSwayVel -= (_lock.AngleDegrees - desiredDegrees) * 0.1f;
                _chainSwayVel *= 0.95f;
                _chainSway += _chainSwayVel;
                _lock.Depth = _underPart.Depth + 2;
                Offset(new Vec2(-9f + xOffChange, -5f));
                Graphics.Draw(_lock, chainOff.X, chainOff.Y);
            }
            if (!(containedObject is RagdollPart) || !Network.isActive)
            {
                _containedObject.Draw();
            }
            if (afterDrawAngle > -99999f)
            {
                _containedObject.AngleDegrees = afterDrawAngle;
            }
        }
        else if (chained.value)
        {
            _chain.Depth = base.Depth + 1;
            Vec2 chainOff2 = Offset(new Vec2(-3f, -2f));
            if (base.equippedDuck != null)
            {
                chainOff2 = Offset(new Vec2(-9f, -2f));
            }
            _chain.Center = new Vec2(3f, 3f);
            Graphics.Draw(_chain, chainOff2.X, chainOff2.Y);
            Offset(new Vec2(0f, -8f));
            _chain.AngleDegrees = 90f + _chainSway;
            float desiredDegrees2 = 90f + ((owner != null) ? owner.hSpeed : hSpeed) * 10f;
            _chainSwayVel -= (_chain.AngleDegrees - desiredDegrees2) * 0.1f;
            _chainSwayVel *= 0.95f;
            _chainSway += _chainSwayVel;
        }
    }
}
