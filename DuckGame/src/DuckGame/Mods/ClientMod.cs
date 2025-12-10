using System;
using System.Linq;
using System.Reflection;

namespace DuckGame;

public class ClientMod : DisabledMod
{
    public ClientMod()
    {
    }

    public ClientMod(string pPath, ModConfiguration pConfig = null, string pInfoFile = "info.txt")
    {
        string name = "null";
        string author = "null";
        string description = "null";
        bool hd = false;
        if (DuckFile.FileExists(pPath + pInfoFile))
        {
            string[] lines = DuckFile.ReadAllLines(pPath + pInfoFile);
            if (lines.Count() >= 3)
            {
                name = lines[0];
                author = lines[1];
                description = lines[2];
                if (lines.Count() > 3 && lines[3].Trim() == "hd")
                {
                    hd = true;
                }
            }
        }
        if (pConfig == null)
        {
            base.configuration = new ModConfiguration();
        }
        else
        {
            base.configuration = pConfig;
        }
        base.configuration.assembly = Assembly.GetExecutingAssembly();
        base.configuration.contentManager = ContentManagers.GetContentManager(typeof(DefaultContentManager));
        base.configuration.name = name;
        base.configuration.displayName = name;
        base.configuration.description = description;
        base.configuration.version = new Version(DG.version);
        base.configuration.author = author;
        base.configuration.contentDirectory = pPath;
        base.configuration.directory = pPath;
        base.configuration.isHighResReskin = hd;
    }
}
