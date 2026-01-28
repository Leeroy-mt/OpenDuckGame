using System;

namespace DuckGame;

public class SubBackgroundTile : Thing
{
    public SubBackgroundTile(float xpos, float ypos)
        : base(xpos, ypos)
    {
    }

    #region Public Methods

    public override void Initialize() { }

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
            (graphic as SpriteMap).frame = Convert.ToInt32(typeNode.Value);
        return true;
    }

    public override ContextMenu GetContextMenu()
    {
        return null;
    }

    #endregion
}
