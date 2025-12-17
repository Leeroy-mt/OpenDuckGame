using System;

namespace DuckGame;

[EditorGroup("Stuff|Wires")]
[BaggedProperty("isOnlineCapable", true)]
public class WireMount : Thing, IWirePeripheral
{
    private SpriteMap _sprite;

    public StateBinding _containedThingBinding = new StateBinding(nameof(_containedThing));

    public StateBinding _actionBinding = new WireMountFlagBinding();

    public Thing _containedThing;

    public EditorProperty<bool> infinite = new EditorProperty<bool>(val: false);

    private Type _contains;

    public EditorProperty<float> mountAngle = new EditorProperty<float>(0f, null, -360f, 360f, 5f);

    public bool newFlipType = true;

    public Type contains
    {
        get
        {
            return _contains;
        }
        set
        {
            if (_contains != value && value != null)
            {
                _containedThing = Editor.CreateObject(value) as Thing;
                if (_containedThing != null && _containedThing is Gun)
                {
                    (_containedThing as Gun).infinite.value = infinite.value;
                }
            }
            _contains = value;
        }
    }

    public override void PrepareForHost()
    {
        base.PrepareForHost();
        if (_containedThing != null)
        {
            _containedThing.PrepareForHost();
        }
    }

    public WireMount(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("wireMount", 16, 16);
        graphic = _sprite;
        center = new Vec2(8f, 8f);
        collisionOffset = new Vec2(-8f, -8f);
        collisionSize = new Vec2(16f, 16f);
        base.depth = -0.5f;
        _editorName = "Wire Mount";
        editorTooltip = "Specifies an object to trigger whenever a connected Button is pressed.";
        base.layer = Layer.Foreground;
        _canFlip = true;
        _placementCost += 4;
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk binaryClassChunk = base.Serialize();
        binaryClassChunk.AddProperty("contains", Editor.SerializeTypeName(contains));
        binaryClassChunk.AddProperty("newFlipType", newFlipType);
        return binaryClassChunk;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        contains = Editor.DeSerializeTypeName(node.GetProperty<string>("contains"));
        newFlipType = node.GetProperty<bool>("newFlipType");
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

    public override void Initialize()
    {
        if (!(Level.current is Editor) && _containedThing != null)
        {
            _containedThing.owner = this;
            Level.Add(_containedThing);
        }
        base.Initialize();
    }

    public override void Update()
    {
        if (_containedThing != null)
        {
            _containedThing.owner = this;
            if (_containedThing.removeFromLevel)
            {
                _containedThing = null;
            }
            else
            {
                _containedThing.offDir = (sbyte)((!flipHorizontal) ? 1 : (-1));
                _containedThing.position = position;
                _containedThing.depth = base.depth + 10;
                _containedThing.layer = base.layer;
                if (newFlipType)
                {
                    _containedThing.angleDegrees = (flipHorizontal ? (0f - mountAngle.value) : mountAngle.value);
                }
                else
                {
                    _containedThing.angleDegrees = mountAngle.value;
                }
                if (_containedThing is Gun)
                {
                    Gun g = _containedThing as Gun;
                    Vec2 kickVector = -g.barrelVector * (g.kick * 5f);
                    _containedThing.position += kickVector;
                }
            }
        }
        base.Update();
    }

    public override void Terminate()
    {
        base.Terminate();
    }

    public override void Draw()
    {
        if (_containedThing != null && Level.current is Editor)
        {
            _containedThing.offDir = (sbyte)((!flipHorizontal) ? 1 : (-1));
            _containedThing.position = position;
            _containedThing.depth = base.depth + 10;
            _containedThing.layer = base.layer;
            if (newFlipType)
            {
                _containedThing.angleDegrees = (flipHorizontal ? (0f - mountAngle.value) : mountAngle.value);
            }
            else
            {
                _containedThing.angleDegrees = mountAngle.value;
            }
            _containedThing.DoEditorUpdate();
            _containedThing.Draw();
        }
        base.Draw();
    }

    public void Pulse(int type, WireTileset wire)
    {
        Thing.Fondle(this, DuckNetwork.localConnection);
        if (_containedThing is Holdable h)
        {
            Thing.Fondle(h, DuckNetwork.localConnection);
            switch (type)
            {
                case 0:
                    action = true;
                    h.UpdateAction();
                    action = false;
                    h.UpdateAction();
                    break;
                case 1:
                    action = true;
                    break;
                case 2:
                    action = false;
                    break;
            }
        }
    }
}
