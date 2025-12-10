using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DuckGame;

[DebuggerDisplay("{cloudPath}")]
public class CloudFile
{
    private static Dictionary<string, CloudFile> _index = new Dictionary<string, CloudFile>();

    public const string kCloudstring = "nq500000_";

    public const string kReadstring = "nq403216_";

    public const string kBackupPrefix = "_dgbalooga_save";

    public static List<string> _cloudFolderFilters = new List<string> { "EditorPreviews", "Online", "Workshop", "Custom/Arcade/Downloaded", "Custom/Background/Downloaded", "Custom/Blocks/Downloaded", "Custom/Moji/Downloaded", "Custom/Parallax/Downloaded", "Custom/Platform/Downloaded" };

    public string localPath;

    public string cloudPath;

    public string oldCloudPath;

    private DateTime _cloudDate = DateTime.MinValue;

    private DateTime _localDate = DateTime.MinValue;

    public bool isOld
    {
        get
        {
            if (cloudPath != null)
            {
                if (cloudPath.StartsWith("nq403216_"))
                {
                    return !cloudPath.EndsWith(".lev");
                }
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// The last time the file was saved to the cloud. If cloudDate == DateTime.MinValue, the file is not indexed.
    /// </summary>
    public DateTime cloudDate
    {
        get
        {
            return _cloudDate;
        }
        set
        {
            _cloudDate = value;
        }
    }

    /// <summary>
    /// The last time steam uploaded the file.  If steamTimestamp == DateTime.MinValue, the file does not exist on the cloud.
    /// </summary>
    public DateTime steamTimestamp
    {
        get
        {
            if (Steam.FileExists(cloudPath))
            {
                return Steam.FileTimestamp(cloudPath);
            }
            return DateTime.MinValue;
        }
    }

    /// <summary>
    /// The last time the file was modified locally. If localData == DateTime.MinValue, a local version of this file does not exist.
    /// </summary>            
    public DateTime localDate
    {
        get
        {
            return _localDate;
        }
        set
        {
            _localDate = value;
        }
    }

    public CloudFile(string pCloudPath)
    {
        cloudPath = pCloudPath;
        localPath = CloudPathToFilePath(pCloudPath);
        oldCloudPath = pCloudPath.Replace("nq500000_", "nq403216_");
    }

    public CloudFile(string pCloudPath, string pLocalPath)
    {
        cloudPath = pCloudPath;
        localPath = pLocalPath;
        oldCloudPath = pCloudPath.Replace("nq500000_", "nq403216_");
    }

    public static void Initialize()
    {
    }

    public static void Clear()
    {
    }

    public static CloudFile GetLocal(string pLocalPath, bool pDelete = false)
    {
        if (pLocalPath.EndsWith(".lev") && !pLocalPath.Contains(DuckFile.levelDirectory) && !pDelete)
        {
            return null;
        }
        bool userPath = DuckFile.IsUserPath(pLocalPath);
        if (pLocalPath.Contains(":"))
        {
            pLocalPath = DuckFile.GetLocalSavePath(pLocalPath);
            if (pLocalPath == null)
            {
                return null;
            }
            pLocalPath = pLocalPath.Replace('\\', '/');
            if (pLocalPath[pLocalPath.Length - 1] == '?')
            {
                return null;
            }
            foreach (string s in _cloudFolderFilters)
            {
                if (pLocalPath.StartsWith(s))
                {
                    return null;
                }
            }
            return Get(((pLocalPath.EndsWith(".lev") || !userPath) ? "nq403216_" : "nq500000_") + pLocalPath, pDelete);
        }
        return null;
    }

    public static CloudFile Get(string pCloudPath, bool pDelete = false)
    {
        if (pDelete)
        {
            _index.Remove(pCloudPath);
        }
        CloudFile c = null;
        if (!_index.TryGetValue(pCloudPath, out c))
        {
            if (!pDelete)
            {
                if (pCloudPath == "nq500000_" || pCloudPath.Contains("localsettings.dat") || pCloudPath.Contains("_dgbalooga_save"))
                {
                    return null;
                }
                bool oldFile = pCloudPath.StartsWith("nq403216_");
                if (!pCloudPath.Contains("nq500000_") && !oldFile)
                {
                    return null;
                }
                bool levelFile = pCloudPath.EndsWith(".lev");
                if (levelFile && !oldFile)
                {
                    return null;
                }
                if (levelFile && !pCloudPath.StartsWith("nq403216_Levels") && !pDelete)
                {
                    return null;
                }
                if (oldFile && !levelFile && Steam.FileExists(pCloudPath.Replace("nq403216_", "nq500000_")))
                {
                    return null;
                }
            }
            string localFile = CloudPathToFilePath(pCloudPath);
            c = new CloudFile(pCloudPath, localFile);
            if (File.Exists(localFile))
            {
                c.localDate = File.GetLastWriteTime(localFile);
            }
        }
        return c;
    }

    public static string CloudPathToFilePath(string pPath)
    {
        if (pPath.EndsWith(".lev"))
        {
            return DuckFile.saveDirectory + pPath.Replace("nq403216_", "");
        }
        if (pPath.Contains("nq403216_"))
        {
            return DuckFile.saveDirectory + pPath.Replace("nq403216_", "");
        }
        return DuckFile.userDirectory + pPath.Replace("nq500000_", "");
    }
}
