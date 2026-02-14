using Microsoft.Xna.Framework;

namespace DuckGame;

public class ChallengeTrophy : Serializable
{
    private TrophyType _type;

    private int _goodies = -1;

    private int _targets = -1;

    private int _timeRequirement;

    private int _timeRequirementMilliseconds;

    private ChallengeData _owner;

    public TrophyType type
    {
        get
        {
            return _type;
        }
        set
        {
            _type = value;
        }
    }

    public int goodies
    {
        get
        {
            return _goodies;
        }
        set
        {
            _goodies = value;
            _owner.Update();
        }
    }

    public int targets
    {
        get
        {
            return _targets;
        }
        set
        {
            _targets = value;
            _owner.Update();
        }
    }

    public int timeRequirement
    {
        get
        {
            return _timeRequirement;
        }
        set
        {
            _timeRequirement = value;
            _owner.Update();
        }
    }

    public int timeRequirementMilliseconds
    {
        get
        {
            return _timeRequirementMilliseconds;
        }
        set
        {
            _timeRequirementMilliseconds = value;
            _owner.Update();
        }
    }

    public Color color
    {
        get
        {
            if (type == TrophyType.Bronze)
            {
                return Colors.Bronze;
            }
            if (type == TrophyType.Silver)
            {
                return Colors.Silver;
            }
            if (type == TrophyType.Gold)
            {
                return Colors.Gold;
            }
            if (type == TrophyType.Platinum)
            {
                return Colors.Platinum;
            }
            return Colors.Developer;
        }
    }

    public string colorString
    {
        get
        {
            if (type == TrophyType.Bronze)
            {
                return "|CBRONZE|";
            }
            if (type == TrophyType.Silver)
            {
                return "|CSILVER|";
            }
            if (type == TrophyType.Gold)
            {
                return "|CGOLD|";
            }
            if (type == TrophyType.Platinum)
            {
                return "|CPLATINUM|";
            }
            return "|CDEV|";
        }
    }

    public string name
    {
        get
        {
            if (type == TrophyType.Bronze)
            {
                return "BRONZE";
            }
            if (type == TrophyType.Silver)
            {
                return "SILVER";
            }
            if (type == TrophyType.Gold)
            {
                return "GOLD";
            }
            if (type == TrophyType.Platinum)
            {
                return "PLATINUM";
            }
            return "UR THE BEST";
        }
    }

    public ChallengeTrophy(ChallengeData owner)
    {
        _owner = owner;
    }

    public BinaryClassChunk Serialize()
    {
        BinaryClassChunk element = new BinaryClassChunk();
        SerializeField(element, "type");
        SerializeField(element, "goodies");
        SerializeField(element, "targets");
        SerializeField(element, "timeRequirement");
        SerializeField(element, "timeRequirementMilliseconds");
        return element;
    }

    public bool Deserialize(BinaryClassChunk node)
    {
        DeserializeField(node, "type");
        DeserializeField(node, "goodies");
        DeserializeField(node, "targets");
        DeserializeField(node, "timeRequirement");
        DeserializeField(node, "timeRequirementMilliseconds");
        return true;
    }

    public DXMLNode LegacySerialize()
    {
        DXMLNode element = new DXMLNode("challengeTrophy");
        LegacySerializeField(element, "type");
        LegacySerializeField(element, "goodies");
        LegacySerializeField(element, "targets");
        LegacySerializeField(element, "timeRequirement");
        LegacySerializeField(element, "timeRequirementMilliseconds");
        return element;
    }

    public bool LegacyDeserialize(DXMLNode node)
    {
        LegacyDeserializeField(node, "type");
        LegacyDeserializeField(node, "goodies");
        LegacyDeserializeField(node, "targets");
        LegacyDeserializeField(node, "timeRequirement");
        LegacyDeserializeField(node, "timeRequirementMilliseconds");
        return true;
    }
}
