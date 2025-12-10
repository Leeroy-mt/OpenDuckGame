using System.Collections.Generic;

namespace DuckGame;

public class TestArea : Level
{
    private Editor _editor;

    protected int _seed;

    protected RandomLevelData _center;

    public TestArea(Editor editor, string level, int seed = 0, RandomLevelData center = null)
    {
        _editor = editor;
        _level = level;
        _seed = seed;
        _center = center;
    }

    public override void Initialize()
    {
        if (_level == "RANDOM")
        {
            LevelGenerator.MakeLevel(null, (_center.left && _center.right) ? true : false, _seed).LoadParts(0f, 0f, this, _seed);
            return;
        }
        IEnumerable<DXMLNode> objectsNode = DuckXML.Load(_level).Element("Level").Elements("Objects");
        if (objectsNode == null)
        {
            return;
        }
        foreach (DXMLNode item in objectsNode.Elements("Object"))
        {
            Thing t = Thing.LegacyLoadThing(item);
            if (t != null)
            {
                AddThing(t);
            }
        }
    }

    public override void Update()
    {
        base.Update();
    }
}
