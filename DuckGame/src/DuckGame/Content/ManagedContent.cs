using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DuckGame;

public static class ManagedContent
{
    public static ManagedContentList<Thing> Things = new ManagedContentList<Thing>();

    public static ManagedContentList<AmmoType> AmmoTypes = new ManagedContentList<AmmoType>();

    public static ManagedContentList<DeathCrateSetting> DeathCrateSettings = new ManagedContentList<DeathCrateSetting>();

    public static ManagedContentList<DestroyType> DestroyTypes = new ManagedContentList<DestroyType>();

    private static void InitializeContentSet<T>(ManagedContentList<T> list)
    {
        if (MonoMain.moddingEnabled)
        {
            foreach (Mod mod in ModLoader.accessibleMods)
            {
                List<Type> typeList = mod.GetTypeList(typeof(T));
                foreach (Type type in mod.configuration.contentManager.Compile<T>(mod))
                {
                    list.Add(type);
                    typeList.Add(type);
                }
            }
            return;
        }
        foreach (Type t in Editor.GetSubclasses(typeof(T)))
        {
            list.Add(t);
        }
    }

    public static Assembly ResolveModAssembly(object sender, ResolveEventArgs args)
    {
        if (ModLoader._modAssemblyNames.TryGetValue(args.Name, out var mod))
        {
            return mod.configuration.assembly;
        }
        if (args.Name.StartsWith("Steam,"))
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly x) => x.FullName.StartsWith("Steam,") || x.FullName.StartsWith("Steam.Debug,"));
        }
        return null;
    }

    public static void PreInitializeMods()
    {
        if (MonoMain.moddingEnabled)
        {
            ModLoader.AddMod(CoreMod.coreMod = new CoreMod());
            AppDomain.CurrentDomain.AssemblyResolve += ResolveModAssembly;
            DuckFile.CreatePath(DuckFile.modsDirectory);
            DuckFile.CreatePath(DuckFile.globalModsDirectory);
            ModLoader.PreLoadMods(DuckFile.modsDirectory);
        }
    }

    public static void InitializeMods()
    {
        //MonoMain.loadMessage = "Loading Mods";
        if (MonoMain.moddingEnabled)
        {
            ModLoader.LoadMods(DuckFile.modsDirectory);
        }
        ModLoader.InitializeAssemblyArray();
        InitializeContentSet(Things);
        InitializeContentSet(AmmoTypes);
        InitializeContentSet(DeathCrateSettings);
        InitializeContentSet(DestroyTypes);
        ContentProperties.InitializeBags(Things.Types);
    }
}
