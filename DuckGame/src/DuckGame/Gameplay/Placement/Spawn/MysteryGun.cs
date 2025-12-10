using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Spawns")]
[BaggedProperty("isInDemo", true)]
public class MysteryGun : Thing, IContainPossibleThings
{
    private SpriteMap _sprite;

    public Type containedType;

    private Thing _addedThing;

    public List<TypeProbPair> contains = new List<TypeProbPair>();

    public List<TypeProbPair> possible => contains;

    public MysteryGun(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("mysteryGun", 32, 32);
        graphic = _sprite;
        center = new Vec2(16f, 16f);
        collisionSize = new Vec2(10f, 10f);
        collisionOffset = new Vec2(-5f, -5f);
        base.depth = 0.5f;
        _canFlip = false;
        editorTooltip = "Can be configured to spawn a random weapon or item from a specified list.";
        _placementCost += 4;
    }

    public void PreparePossibilities()
    {
        containedType = PickType(base.chanceGroup, contains);
    }

    public static Type PickType(int chanceGroup, List<TypeProbPair> contains)
    {
        ItemBox.GetPhysicsObjects(Editor.Placeables);
        Random generator = new Random((int)(Level.GetChanceGroup2(chanceGroup) * 2.1474836E+09f - 1f));
        Random cur = Rando.generator;
        Rando.generator = generator;
        List<TypeProbPair> list = Utils.Shuffle(contains);
        Type highest = null;
        float bestProb = 0f;
        foreach (TypeProbPair p in list)
        {
            if (Rando.Float(1f) > 1f - p.probability)
            {
                highest = p.type;
                break;
            }
            if (p.probability > bestProb)
            {
                bestProb = p.probability;
                highest = p.type;
            }
        }
        Rando.generator = cur;
        return highest;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            ReplaceSelfWithThing();
            if (Network.isActive && _addedThing != null && Thing.loadingLevel != null)
            {
                _addedThing.PrepareForHost();
            }
        }
    }

    private void ReplaceSelfWithThing()
    {
        if (containedType == null)
        {
            PreparePossibilities();
        }
        Type highest = containedType;
        if (highest != null)
        {
            _addedThing = Editor.CreateObject(highest) as Thing;
            _addedThing.position = position;
            Level.Add(_addedThing);
        }
        Level.Remove(this);
    }

    public override ContextMenu GetContextMenu()
    {
        FieldBinding binding = new FieldBinding(this, "contains");
        EditorGroupMenu obj = base.GetContextMenu() as EditorGroupMenu;
        obj.InitializeGroups(new EditorGroup(typeof(PhysicsObject)), binding);
        return obj;
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk binaryClassChunk = base.Serialize();
        binaryClassChunk.AddProperty("contains", SerializeTypeProb(contains));
        return binaryClassChunk;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        contains = DeserializeTypeProb(node.GetProperty<string>("contains"));
        return true;
    }

    public static string SerializeTypeProb(List<TypeProbPair> list)
    {
        string pairs = "";
        foreach (TypeProbPair p in list)
        {
            pairs += ModLoader.SmallTypeName(p.type);
            pairs += ":";
            pairs += p.probability;
            pairs += "|";
        }
        return pairs;
    }

    public static List<TypeProbPair> DeserializeTypeProb(string list)
    {
        List<TypeProbPair> contains = new List<TypeProbPair>();
        try
        {
            if (list == null)
            {
                return contains;
            }
            string[] array = list.Split('|');
            foreach (string s in array)
            {
                if (s.Length > 1)
                {
                    string[] parts2 = s.Split(':');
                    TypeProbPair p = new TypeProbPair
                    {
                        type = Editor.GetType(parts2[0]),
                        probability = Convert.ToSingle(parts2[1])
                    };
                    contains.Add(p);
                }
            }
        }
        catch (Exception)
        {
        }
        return contains;
    }

    public override void DrawHoverInfo()
    {
        float yOff = 0f;
        foreach (TypeProbPair p in contains)
        {
            if (p.probability > 0f)
            {
                Color c = Color.White;
                c = ((p.probability == 0f) ? Color.DarkGray : ((p.probability < 0.3f) ? Colors.DGRed : ((!(p.probability < 0.7f)) ? Color.Green : Color.Orange)));
                string s = p.type.Name + ": " + p.probability.ToString("0.000");
                Graphics.DrawString(s, position + new Vec2((0f - Graphics.GetStringWidth(s, thinButtons: false, 0.5f)) / 2f, 0f - (16f + yOff)), c, 0.9f, null, 0.5f);
                yOff += 4f;
            }
        }
    }
}
