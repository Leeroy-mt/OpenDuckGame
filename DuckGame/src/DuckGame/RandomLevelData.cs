using System;
using System.Collections.Generic;
using System.Reflection;

namespace DuckGame;

public class RandomLevelData
{
    public class PreparedThing
    {
        public Thing thing;

        public bool mirror;
    }

    public Dictionary<Type, int> weapons = new Dictionary<Type, int>();

    public Dictionary<Type, int> spawners = new Dictionary<Type, int>();

    public int numWeapons;

    public int numSuperWeapons;

    public int numFatalWeapons;

    public int numPermanentWeapons;

    public int numPermanentSuperWeapons;

    public int numPermanentFatalWeapons;

    public bool up;

    public bool down;

    public bool left;

    public bool right;

    public float chance;

    public float boostChance;

    public int max = 2;

    public bool single;

    public bool multi;

    public bool canMirror = true;

    public bool isMirrored;

    public int numArmor;

    public int numEquipment;

    public int numSpawns;

    public int numTeamSpawns;

    public int numLockedDoors;

    public int numKeys;

    public List<BinaryClassChunk> data;

    public List<BinaryClassChunk> alternateData;

    public bool flip;

    public bool symmetry;

    public string file = "";

    private Vec2 posBeforeTranslate = Vec2.Zero;

    private bool mainLoad = true;

    public int numLinkDirections
    {
        get
        {
            int num = 0;
            if (up)
            {
                num++;
            }
            if (down)
            {
                num++;
            }
            if (left)
            {
                num++;
            }
            if (right)
            {
                num++;
            }
            return num;
        }
    }

    public RandomLevelData Flipped()
    {
        if (isMirrored)
        {
            return this;
        }
        return new RandomLevelData
        {
            data = data,
            alternateData = alternateData,
            flip = !flip,
            left = right,
            right = left,
            up = up,
            down = down,
            chance = chance,
            max = max,
            file = file,
            canMirror = canMirror,
            isMirrored = isMirrored,
            numWeapons = numWeapons,
            numSuperWeapons = numSuperWeapons,
            numFatalWeapons = numFatalWeapons,
            numPermanentWeapons = numPermanentWeapons,
            numPermanentSuperWeapons = numPermanentSuperWeapons,
            numPermanentFatalWeapons = numPermanentFatalWeapons,
            numArmor = numArmor,
            numEquipment = numEquipment,
            numSpawns = numSpawns,
            numTeamSpawns = numTeamSpawns,
            numLockedDoors = numLockedDoors,
            numKeys = numKeys
        };
    }

    public RandomLevelData Symmetric()
    {
        RandomLevelData newData = new RandomLevelData();
        newData.data = data;
        newData.alternateData = alternateData;
        newData.flip = flip;
        newData.up = up;
        newData.down = down;
        newData.symmetry = true;
        newData.file = file;
        newData.canMirror = true;
        newData.isMirrored = true;
        if (newData.left)
        {
            newData.right = true;
        }
        else
        {
            newData.right = false;
        }
        newData.chance = chance;
        if (!isMirrored)
        {
            newData.chance *= 0.75f;
        }
        newData.max = max;
        newData.numWeapons = numWeapons;
        newData.numSuperWeapons = numSuperWeapons;
        newData.numFatalWeapons = numFatalWeapons;
        newData.numPermanentWeapons = numPermanentWeapons;
        newData.numPermanentSuperWeapons = numPermanentSuperWeapons;
        newData.numPermanentFatalWeapons = numPermanentFatalWeapons;
        newData.numArmor = numArmor;
        newData.numEquipment = numEquipment;
        newData.numSpawns = 0;
        newData.numTeamSpawns = 0;
        newData.numLockedDoors = numLockedDoors;
        newData.numKeys = numKeys;
        return newData;
    }

    public void ApplyWeaponData(string data)
    {
        weapons.Clear();
        numWeapons = 0;
        numSuperWeapons = 0;
        numFatalWeapons = 0;
        string[] array = data.Split('|');
        foreach (string part in array)
        {
            if (!(part != ""))
            {
                continue;
            }
            try
            {
                Type t = Editor.GetType(part);
                if (!weapons.ContainsKey(t))
                {
                    weapons[t] = 0;
                }
                weapons[t]++;
                numWeapons++;
                IReadOnlyPropertyBag bag = ContentProperties.GetBag(t);
                if (bag.GetOrDefault("isSuperWeapon", defaultValue: false))
                {
                    numSuperWeapons++;
                }
                if (bag.GetOrDefault("isFatal", defaultValue: true))
                {
                    numFatalWeapons++;
                }
            }
            catch
            {
            }
        }
    }

    public void ApplySpawnerData(string data)
    {
        spawners.Clear();
        numPermanentWeapons = 0;
        numPermanentSuperWeapons = 0;
        numPermanentFatalWeapons = 0;
        string[] array = data.Split('|');
        foreach (string part in array)
        {
            if (!(part != ""))
            {
                continue;
            }
            try
            {
                Type t = Editor.GetType(part);
                if (!spawners.ContainsKey(t))
                {
                    spawners[t] = 0;
                }
                spawners[t]++;
                numPermanentWeapons++;
                IReadOnlyPropertyBag bag = ContentProperties.GetBag(t);
                if (bag.GetOrDefault("isSuperWeapon", defaultValue: false))
                {
                    numPermanentSuperWeapons++;
                }
                if (bag.GetOrDefault("isFatal", defaultValue: true))
                {
                    numPermanentFatalWeapons++;
                }
            }
            catch
            {
            }
        }
    }

    public void ApplyItemData(string data)
    {
        string[] array = data.Split('|');
        int index = 0;
        string[] array2 = array;
        foreach (string part in array2)
        {
            switch (index)
            {
                case 0:
                    numArmor = Convert.ToInt32(part);
                    break;
                case 1:
                    numEquipment = Convert.ToInt32(part);
                    break;
                case 2:
                    numSpawns = Convert.ToInt32(part);
                    break;
                case 3:
                    numTeamSpawns = Convert.ToInt32(part);
                    break;
                case 4:
                    numLockedDoors = Convert.ToInt32(part);
                    break;
                case 5:
                    numKeys = Convert.ToInt32(part);
                    break;
            }
            index++;
        }
    }

    public RandomLevelData Combine(RandomLevelData dat)
    {
        RandomLevelData newDat = new RandomLevelData();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo[] fields = GetType().GetFields(flags);
        foreach (FieldInfo f in fields)
        {
            if (f.FieldType == typeof(int))
            {
                f.SetValue(newDat, (int)f.GetValue(this) + (int)f.GetValue(dat));
            }
            if (!f.FieldType.IsGenericType || !(f.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
            {
                continue;
            }
            Dictionary<Type, int> obj = f.GetValue(this) as Dictionary<Type, int>;
            Dictionary<Type, int> second = f.GetValue(dat) as Dictionary<Type, int>;
            Dictionary<Type, int> newDictionary = new Dictionary<Type, int>();
            foreach (KeyValuePair<Type, int> pair in obj)
            {
                if (!newDictionary.ContainsKey(pair.Key))
                {
                    newDictionary[pair.Key] = 0;
                }
                newDictionary[pair.Key] += pair.Value;
            }
            foreach (KeyValuePair<Type, int> pair2 in second)
            {
                if (!newDictionary.ContainsKey(pair2.Key))
                {
                    newDictionary[pair2.Key] = 0;
                }
                newDictionary[pair2.Key] += pair2.Value;
            }
            f.SetValue(newDat, newDictionary);
        }
        return newDat;
    }

    private Thing AddThing(Thing t, Level level)
    {
        level.AddThing(t);
        return t;
    }

    private Thing ProcessThing(Thing t, float x, float y)
    {
        if (!t.visibleInGame)
        {
            t.visible = false;
        }
        bool doFlip = flip;
        if (Level.symmetry)
        {
            doFlip = false;
        }
        if (Level.loadingOppositeSymmetry)
        {
            doFlip = !doFlip;
        }
        if (mainLoad && Level.symmetry && !(t is ThingContainer))
        {
            if (Level.leftSymmetry && t.x > 88f)
            {
                return null;
            }
            if (!Level.leftSymmetry && t.x < 88f)
            {
                return null;
            }
        }
        if (doFlip && !(t is ThingContainer))
        {
            if (!(t is BackgroundTile))
            {
                t.SetTranslation(new Vec2(0f - t.x + (192f - t.x) - 16f, 0f));
            }
            t.flipHorizontal = true;
            _ = t is BackgroundTile;
            if (t is Teleporter)
            {
                if ((t as Teleporter).direction == 2)
                {
                    (t as Teleporter).direction = 3;
                }
                else if ((t as Teleporter).direction == 3)
                {
                    (t as Teleporter).direction = 2;
                }
            }
        }
        if (t is BackgroundTile && (t as BackgroundTile).isFlipped)
        {
            t.flipHorizontal = true;
        }
        if (t is BackgroundTile && !(t as BackgroundTile).oppositeSymmetry)
        {
            _ = flip;
        }
        posBeforeTranslate = t.position;
        if (!(t is BackgroundTile))
        {
            t.SetTranslation(new Vec2(x, y));
        }
        return t;
    }

    public List<PreparedThing> PrepareThings(bool symmetric, float x, float y)
    {
        if (symmetric && isMirrored)
        {
            symmetric = false;
        }
        if (!symmetric && flip)
        {
            Level.flipH = true;
        }
        if (symmetry || symmetric)
        {
            Level.symmetry = true;
        }
        List<PreparedThing> prepare = new List<PreparedThing>();
        List<BinaryClassChunk> parts = (LevelGenerator.openAirMode ? alternateData : data);
        if (parts == null || parts.Count == 0)
        {
            parts = data;
        }
        foreach (BinaryClassChunk elly in parts)
        {
            Thing t = Thing.LoadThing(elly);
            if (t == null)
            {
                continue;
            }
            mainLoad = true;
            Level.leftSymmetry = true;
            Level.loadingOppositeSymmetry = false;
            t = ProcessThing(t, x, y);
            if (t == null)
            {
                continue;
            }
            if (!(t is ThingContainer) && Level.symmetry && (posBeforeTranslate.x - 8f < 80f || posBeforeTranslate.x - 8f > 96f))
            {
                Thing t2 = Thing.LoadThing(elly, chance: false);
                if (t2 != null)
                {
                    mainLoad = false;
                    Level.leftSymmetry = false;
                    Level.loadingOppositeSymmetry = true;
                    t2 = ProcessThing(t2, x, y);
                    if (t2 != null)
                    {
                        prepare.Add(new PreparedThing
                        {
                            thing = t2,
                            mirror = true
                        });
                    }
                }
            }
            prepare.Add(new PreparedThing
            {
                thing = t
            });
        }
        Level.flipH = false;
        Level.symmetry = false;
        return prepare;
    }

    public void Load(float x, float y, Level level, bool symmetric, List<PreparedThing> pPreparedThings)
    {
        if (symmetric && isMirrored)
        {
            symmetric = false;
        }
        if (!symmetric && flip)
        {
            Level.flipH = true;
        }
        if (symmetry || symmetric)
        {
            Level.symmetry = true;
        }
        if (data != null)
        {
            foreach (PreparedThing pPreparedThing in pPreparedThings)
            {
                Thing t = pPreparedThing.thing;
                if (t == null || !RandomLevelNode._allPreparedThings.Contains(t) || (!ContentProperties.GetBag(t.GetType()).GetOrDefault("isOnlineCapable", defaultValue: true) && Network.isActive))
                {
                    continue;
                }
                Level.leftSymmetry = true;
                Level.loadingOppositeSymmetry = false;
                mainLoad = true;
                Thing processed = AddThing(t, level);
                mainLoad = false;
                if (Network.isActive && processed.isStateObject)
                {
                    GhostManager.context.MakeGhost(processed, -1, initLevel: true);
                    processed.ghostType = Editor.IDToType[processed.GetType()];
                    DuckNetwork.AssignToHost(processed);
                }
                if (!(t is ThingContainer))
                {
                    continue;
                }
                foreach (Thing thing in (t as ThingContainer).things)
                {
                    if (!(thing is BackgroundTile) && !(thing is ForegroundTile))
                    {
                        continue;
                    }
                    processed = ProcessThing(thing, x, y);
                    if (processed != null)
                    {
                        AddThing(t, level);
                        if (Network.isActive && processed.isStateObject)
                        {
                            GhostManager.context.MakeGhost(processed, -1, initLevel: true);
                            processed.ghostType = Editor.IDToType[processed.GetType()];
                            DuckNetwork.AssignToHost(processed);
                        }
                    }
                }
            }
            level.things.RefreshState();
        }
        Level.flipH = false;
        Level.symmetry = false;
    }
}
