using System;
using System.Collections.Generic;
using System.Reflection;

namespace DuckGame;

public class RandomLevelData
{
    public class PreparedThing
    {
        public bool mirror;

        public Thing thing;
    }

    #region Public Fields

    public bool up;
    public bool down;
    public bool left;
    public bool right;
    public bool single;
    public bool multi;
    public bool canMirror = true;
    public bool isMirrored;
    public bool flip;
    public bool symmetry;

    public int numWeapons;
    public int numSuperWeapons;
    public int numFatalWeapons;
    public int numPermanentWeapons;
    public int numPermanentSuperWeapons;
    public int numPermanentFatalWeapons;
    public int max = 2;
    public int numArmor;
    public int numEquipment;
    public int numSpawns;
    public int numTeamSpawns;
    public int numLockedDoors;
    public int numKeys;

    public float chance;
    public float boostChance;

    public string file = "";

    public List<BinaryClassChunk> data;
    public List<BinaryClassChunk> alternateData;
    public Dictionary<Type, int> weapons = [];
    public Dictionary<Type, int> spawners = [];

    #endregion

    #region Private Fields

    bool mainLoad = true;

    Vec2 posBeforeTranslate = Vec2.Zero;

    #endregion

    public int numLinkDirections
    {
        get
        {
            int num = 0;
            if (up)
                num++;
            if (down)
                num++;
            if (left)
                num++;
            if (right)
                num++;
            return num;
        }
    }

    #region Public Methods

    public void ApplyWeaponData(string data)
    {
        weapons.Clear();
        numWeapons = 0;
        numSuperWeapons = 0;
        numFatalWeapons = 0;
        string[] array = data.Split('|');
        foreach (string part in array)
        {
            if (string.IsNullOrEmpty(part))
                continue;

            try
            {
                Type t = Editor.GetType(part);
                if (!weapons.TryGetValue(t, out int value))
                {
                    value = 0;
                    weapons[t] = value;
                }
                weapons[t] = ++value;
                numWeapons++;
                IReadOnlyPropertyBag bag = ContentProperties.GetBag(t);
                if (bag.GetOrDefault("isSuperWeapon", defaultValue: false))
                    numSuperWeapons++;
                if (bag.GetOrDefault("isFatal", defaultValue: true))
                    numFatalWeapons++;
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
            if (string.IsNullOrEmpty(part))
                continue;

            try
            {
                Type t = Editor.GetType(part);
                if (!spawners.TryGetValue(t, out int value))
                {
                    value = 0;
                    spawners[t] = value;
                }
                spawners[t] = ++value;
                numPermanentWeapons++;
                IReadOnlyPropertyBag bag = ContentProperties.GetBag(t);
                if (bag.GetOrDefault("isSuperWeapon", defaultValue: false))
                    numPermanentSuperWeapons++;
                if (bag.GetOrDefault("isFatal", defaultValue: true))
                    numPermanentFatalWeapons++;
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

    public void Load(float x, float y, Level level, bool symmetric, List<PreparedThing> pPreparedThings)
    {
        if (symmetric && isMirrored)
            symmetric = false;
        if (!symmetric && flip)
            Level.flipH = true;
        if (symmetry || symmetric)
            Level.symmetry = true;
        if (data != null)
        {
            foreach (PreparedThing pPreparedThing in pPreparedThings)
            {
                Thing t = pPreparedThing.thing;
                if (t == null || !RandomLevelNode._allPreparedThings.Contains(t) || (!ContentProperties.GetBag(t.GetType()).GetOrDefault("isOnlineCapable", defaultValue: true) && Network.isActive))
                    continue;
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
                if (t is not ThingContainer)
                    continue;
                foreach (Thing thing in (t as ThingContainer).things)
                {
                    if (thing is not BackgroundTile && thing is not ForegroundTile)
                        continue;
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

    public RandomLevelData Flipped()
    {
        if (isMirrored)
            return this;
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
        newData.right = newData.left;
        newData.chance = chance;
        if (!isMirrored)
            newData.chance *= .75f;
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

    public RandomLevelData Combine(RandomLevelData dat)
    {
        RandomLevelData newDat = new RandomLevelData();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo[] fields = GetType().GetFields(flags);
        foreach (FieldInfo f in fields)
        {
            if (f.FieldType == typeof(int))
                f.SetValue(newDat, (int)f.GetValue(this) + (int)f.GetValue(dat));
            if (!f.FieldType.IsGenericType || f.FieldType.GetGenericTypeDefinition() != typeof(Dictionary<,>))
                continue;
            Dictionary<Type, int> obj = f.GetValue(this) as Dictionary<Type, int>;
            Dictionary<Type, int> second = f.GetValue(dat) as Dictionary<Type, int>;
            Dictionary<Type, int> newDictionary = [];
            foreach (KeyValuePair<Type, int> pair in obj)
            {
                if (!newDictionary.ContainsKey(pair.Key))
                    newDictionary[pair.Key] = 0;
                newDictionary[pair.Key] += pair.Value;
            }
            foreach (KeyValuePair<Type, int> pair2 in second)
            {
                if (!newDictionary.ContainsKey(pair2.Key))
                    newDictionary[pair2.Key] = 0;
                newDictionary[pair2.Key] += pair2.Value;
            }
            f.SetValue(newDat, newDictionary);
        }
        return newDat;
    }

    public List<PreparedThing> PrepareThings(bool symmetric, float x, float y)
    {
        if (symmetric && isMirrored)
            symmetric = false;
        if (!symmetric && flip)
            Level.flipH = true;
        if (symmetry || symmetric)
            Level.symmetry = true;
        List<PreparedThing> prepare = [];
        List<BinaryClassChunk> parts = (LevelGenerator.openAirMode ? alternateData : data);
        if (parts == null || parts.Count == 0)
            parts = data;
        foreach (BinaryClassChunk elly in parts)
        {
            Thing t = Thing.LoadThing(elly);
            if (t == null)
                continue;
            mainLoad = true;
            Level.leftSymmetry = true;
            Level.loadingOppositeSymmetry = false;
            t = ProcessThing(t, x, y);
            if (t == null)
                continue;
            if (t is not ThingContainer && Level.symmetry && (posBeforeTranslate.X - 8 < 80 || posBeforeTranslate.X - 8 > 96))
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

    #endregion

    #region Private Methods

    Thing AddThing(Thing t, Level level)
    {
        level.AddThing(t);
        return t;
    }

    Thing ProcessThing(Thing t, float x, float y)
    {
        if (!t.visibleInGame)
            t.visible = false;

        bool doFlip = flip;
        if (Level.symmetry)
            doFlip = false;
        if (Level.loadingOppositeSymmetry)
            doFlip = !doFlip;
        if (mainLoad && Level.symmetry && t is not ThingContainer)
        {
            if (Level.leftSymmetry && t.X > 88)
                return null;
            if (!Level.leftSymmetry && t.X < 88)
                return null;
        }
        if (doFlip && t is not ThingContainer)
        {
            if (t is not BackgroundTile)
                t.SetTranslation(new Vec2(-t.X + (192 - t.X) - 16, 0));
            t.flipHorizontal = true;
            _ = t is BackgroundTile;
            if (t is Teleporter teleporter)
            {
                if (teleporter.direction == 2)
                    teleporter.direction = 3;
                else if (teleporter.direction == 3)
                    teleporter.direction = 2;
            }
        }
        if (t is BackgroundTile { isFlipped: true })
            t.flipHorizontal = true;
        if (t is BackgroundTile { oppositeSymmetry: false })
            _ = flip;
        posBeforeTranslate = t.Position;
        if (t is not BackgroundTile)
            t.SetTranslation(new Vec2(x, y));
        return t;
    }

    #endregion
}
