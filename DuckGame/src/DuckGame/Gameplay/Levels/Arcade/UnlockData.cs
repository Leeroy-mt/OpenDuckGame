using System.Collections.Generic;

namespace DuckGame;

public class UnlockData
{
    private bool _enabled;

    private bool _prevEnabled;

    private bool _filtered = true;

    private bool _onlineEnabled = true;

    private string _name;

    private string _shortName;

    private string _id;

    private int _icon = -1;

    private int _cost;

    private string _description;

    private string _longDescription = "";

    private UnlockType _type;

    private UnlockPrice _priceTier;

    private bool _unlocked;

    private List<UnlockData> _children = new List<UnlockData>();

    private UnlockData _parent;

    private int _layer;

    public bool enabled
    {
        get
        {
            return _enabled;
        }
        set
        {
            if (_enabled != value)
            {
                _prevEnabled = _enabled;
                _enabled = value;
            }
        }
    }

    public bool prevEnabled
    {
        get
        {
            return _prevEnabled;
        }
        set
        {
            _prevEnabled = value;
        }
    }

    public bool filtered
    {
        get
        {
            return _filtered;
        }
        set
        {
            _filtered = value;
        }
    }

    public bool onlineEnabled
    {
        get
        {
            return _onlineEnabled;
        }
        set
        {
            _onlineEnabled = value;
        }
    }

    public string name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value;
        }
    }

    public string shortName
    {
        get
        {
            if (_shortName != null)
            {
                return _shortName;
            }
            return name;
        }
        set
        {
            _shortName = value;
        }
    }

    public string id
    {
        get
        {
            return _id;
        }
        set
        {
            _id = value;
        }
    }

    public int icon
    {
        get
        {
            return _icon;
        }
        set
        {
            _icon = value;
        }
    }

    public int cost
    {
        get
        {
            return _cost;
        }
        set
        {
            _cost = value;
        }
    }

    public string description
    {
        get
        {
            return _description;
        }
        set
        {
            _description = value;
        }
    }

    public string longDescription
    {
        get
        {
            return _longDescription;
        }
        set
        {
            _longDescription = value;
        }
    }

    public UnlockType type
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

    public UnlockPrice priceTier
    {
        get
        {
            return _priceTier;
        }
        set
        {
            _priceTier = value;
        }
    }

    public bool unlocked
    {
        get
        {
            if (_unlocked)
            {
                return true;
            }
            foreach (Profile universalProfile in Profiles.universalProfileList)
            {
                if (universalProfile.unlocks.Contains(_id))
                {
                    return true;
                }
            }
            return false;
        }
        set
        {
            _unlocked = value;
        }
    }

    public List<UnlockData> children => _children;

    public UnlockData parent
    {
        get
        {
            return _parent;
        }
        set
        {
            _parent = value;
        }
    }

    public int layer
    {
        get
        {
            return _layer;
        }
        set
        {
            _layer = value;
        }
    }

    public string GetNameForDisplay()
    {
        return name.ToUpperInvariant();
    }

    public string GetShortNameForDisplay()
    {
        return shortName.ToUpperInvariant();
    }

    public bool ProfileUnlocked(Profile p)
    {
        return p.unlocks.Contains(_id);
    }

    public void AddChild(UnlockData child)
    {
        children.Add(child);
        child.parent = this;
        child.layer = layer + 1;
    }

    public bool AllParentsUnlocked(Profile who)
    {
        if (parent == null)
        {
            return true;
        }
        foreach (UnlockData p in Unlocks.GetTreeLayer(parent.layer))
        {
            if (p.children.Contains(this) && !p.ProfileUnlocked(who))
            {
                return false;
            }
        }
        return true;
    }

    public UnlockData GetUnlockedParent()
    {
        for (UnlockData dat = this; dat != null; dat = dat.parent)
        {
            if (dat.parent == null || dat.parent.unlocked)
            {
                return dat;
            }
        }
        return null;
    }
}
