namespace DuckGame;

public static class ManagedContent
{
    #region Public Fields

    public static ManagedContentList<Thing> Things = new();

    public static ManagedContentList<AmmoType> AmmoTypes = new();

    public static ManagedContentList<DeathCrateSetting> DeathCrateSettings = new();

    public static ManagedContentList<DestroyType> DestroyTypes = new();

    #endregion

    #region Public Methods

    public static void PreInitializeMods()
    {
        if (MonoMain.moddingEnabled)
        {
            ModLoader.AddMod(CoreMod.coreMod = new CoreMod());
            DuckFile.CreatePath(DuckFile.modsDirectory);
            DuckFile.CreatePath(DuckFile.globalModsDirectory);
            ModLoader.PreLoadMods(DuckFile.modsDirectory);
        }
    }

    public static void InitializeMods()
    {
        if (MonoMain.moddingEnabled)
            ModLoader.LoadMods(DuckFile.modsDirectory);

        ModLoader.InitializeAssemblyArray();
        InitializeContentSet(Things);
        InitializeContentSet(AmmoTypes);
        InitializeContentSet(DeathCrateSettings);
        InitializeContentSet(DestroyTypes);
        ContentProperties.InitializeBags(Things.Types);
    }

    #endregion

    static void InitializeContentSet<T>(ManagedContentList<T> list)
    {
        if (MonoMain.moddingEnabled)
        {
            foreach (Mod mod in ModLoader.accessibleMods)
            {
                var typeList = mod.GetTypeList(typeof(T));
                foreach (var type in mod.configuration.contentManager.Compile<T>(mod))
                {
                    list.Add(type);
                    typeList.Add(type);
                }
            }
            return;
        }

        foreach (var t in Editor.GetSubclasses(typeof(T)))
            list.Add(t);
    }
}