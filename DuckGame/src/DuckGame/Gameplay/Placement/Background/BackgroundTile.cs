using System;

namespace DuckGame;

public abstract class BackgroundTile : Thing, IStaticRender, IDontUpdate
{
    protected int _frame;

    public bool cheap;

    public bool isFlipped;

    public bool oppositeSymmetry;

    public override int frame
    {
        get
        {
            return _frame;
        }
        set
        {
            _frame = value;
            (graphic as SpriteMap).frame = _frame;
        }
    }

    public BackgroundTile(float xpos, float ypos)
        : base(xpos, ypos)
    {
        base.layer = Layer.Background;
        _canBeGrouped = true;
        _isStatic = true;
        _opaque = true;
        if (Level.flipH)
        {
            flipHorizontal = true;
        }
        _placementCost = 1;
    }

    public override void Initialize()
    {
        if (Level.current is Editor)
        {
            cheap = false;
        }
        else
        {
            DoPositioning();
        }
    }

    public virtual void DoPositioning()
    {
        cheap = true;
        graphic.position = position;
        graphic.scale = base.scale;
        graphic.center = center;
        graphic.depth = base.depth;
        graphic.alpha = base.alpha;
        graphic.angle = angle;
        (graphic as SpriteMap).UpdateFrame();
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk element = base.Serialize();
        element.AddProperty("frame", _frame);
        if (flipHorizontal)
        {
            element.AddProperty("f", 1);
        }
        return element;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        _frame = node.GetProperty<int>("frame");
        (graphic as SpriteMap).frame = _frame;
        if (node.GetProperty<int>("f") == 1)
        {
            flipHorizontal = true;
        }
        return true;
    }

    public override DXMLNode LegacySerialize()
    {
        DXMLNode dXMLNode = base.LegacySerialize();
        dXMLNode.Add(new DXMLNode("frame", (graphic as SpriteMap).frame));
        return dXMLNode;
    }

    public override bool LegacyDeserialize(DXMLNode node)
    {
        base.LegacyDeserialize(node);
        DXMLNode typeNode = node.Element("frame");
        if (typeNode != null)
        {
            (graphic as SpriteMap).frame = Convert.ToInt32(typeNode.Value);
        }
        return true;
    }

    public override void Draw()
    {
        graphic.flipH = flipHorizontal;
        if (cheap)
        {
            graphic.UltraCheapStaticDraw(flipHorizontal);
        }
        else
        {
            base.Draw();
        }
    }

    public override ContextMenu GetContextMenu()
    {
        return null;
    }
}
