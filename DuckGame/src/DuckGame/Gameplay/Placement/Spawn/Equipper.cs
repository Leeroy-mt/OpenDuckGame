using System;

namespace DuckGame;

[EditorGroup("Spawns")]
[BaggedProperty("isOnlineCapable", true)]
public class Equipper : Thing
{
    private Type _previewType;

    private Type _contains;

    public EditorProperty<int> radius = new(0, null, 0, 128, 1, "INF");

    public EditorProperty<bool> infinite = new(false);

    public EditorProperty<bool> holstered = new(false);

    public EditorProperty<bool> powerHolstered = new(false);

    public EditorProperty<bool> holsterChained = new(false);

    private RenderTarget2D _preview;

    private Sprite _previewSprite;

    public Type Contains
    {
        get
        {
            return _contains;
        }
        set
        {
            _contains = value;
        }
    }

    public Equipper(float xpos, float ypos)
        : base(xpos, ypos)
    {
        serverOnly = true;
        graphic = new Sprite("equipper");
        center = new(8);
        collisionSize = new(14);
        collisionOffset = new(-7);
        depth = 0.5f;
        _canFlip = false;
        _visibleInGame = false;
        editorTooltip = "Allows equipment to automatically be equipped to all ducks on level start.";
        _placementCost += 4;
    }

    public Thing GetContainedInstance(Vec2 pos = default)
    {
        if (Contains == null)
        {
            return null;
        }
        object[] p = Editor.GetConstructorParameters(Contains);
        if (p.Length > 1)
        {
            p[0] = pos.x;
            p[1] = pos.y;
        }
        PhysicsObject o = Editor.CreateThing(Contains, p) as PhysicsObject;
        if (o is Gun gun)
            gun.infinite = infinite;
        return o;
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk binaryClassChunk = base.Serialize();
        binaryClassChunk.AddProperty("contains", Editor.SerializeTypeName(Contains));
        return binaryClassChunk;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        Contains = Editor.DeSerializeTypeName(node.GetProperty<string>("contains"));
        return true;
    }

    public override DXMLNode LegacySerialize()
    {
        DXMLNode dXMLNode = base.LegacySerialize();
        dXMLNode.Add(new DXMLNode("contains", (Contains != null) ? Contains.AssemblyQualifiedName : ""));
        return dXMLNode;
    }

    public override bool LegacyDeserialize(DXMLNode node)
    {
        base.LegacyDeserialize(node);
        DXMLNode typeNode = node.Element("contains");
        if (typeNode != null)
        {
            Type t = Editor.GetType(typeNode.Value);
            Contains = t;
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
        if (Contains != null)
        {
            containString = Contains.Name;
        }
        if (Contains == null)
        {
            return base.GetDetailsString();
        }
        return base.GetDetailsString() + "Contains: " + containString;
    }

    public override void EditorUpdate()
    {
        if (_previewSprite == null || _previewType != Contains)
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
            _previewType = Contains;
        }
        base.EditorUpdate();
    }

    public override void DrawHoverInfo()
    {
        string containString = "EMPTY";
        if (Contains != null)
        {
            containString = Contains.Name;
        }
        Graphics.DrawString(containString, position + new Vec2((0f - Graphics.GetStringWidth(containString)) / 2f, -16f), Color.White, 0.9f);
        if (radius.value != 0)
        {
            Graphics.DrawCircle(position, radius.value, Color.Red, 1f, 0.9f);
        }
    }

    public override void Draw()
    {
        base.Draw();
        if (_previewSprite != null)
        {
            _previewSprite.depth = base.depth + 1;
            _previewSprite.scale = new Vec2(0.5f, 0.5f);
            _previewSprite.CenterOrigin();
            Graphics.Draw(_previewSprite, base.x, base.y);
        }
    }
}
