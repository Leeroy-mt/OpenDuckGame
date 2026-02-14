using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuckGame;

public class LevelMetaData : BinaryClassChunk
{
    public class PreviewPair
    {
        public bool pending;

        public Texture2D preview;

        public Dictionary<string, int> invalid;

        public bool strange;

        public bool challenge;

        public bool arcade;
    }

    public class SaveLevelPreviewTask
    {
        public string savePath;

        public string levelString;

        public Texture2D levelTexture;

        public Color[] ltData;

        public int ltWidth;

        public int ltHeight;
    }

    public static List<SaveLevelPreviewTask> _completedPreviewTasks = new List<SaveLevelPreviewTask>();

    public string guid;

    public LevelType type;

    public LevelSize size;

    public ulong workshopID;

    public bool online;

    public bool onlineMode;

    public int version = 1;

    public bool deathmatchReady;

    public bool hasCustomArt;

    public bool eightPlayer;

    public bool eightPlayerRestricted;

    public PreviewPair LoadPreview()
    {
        PreviewPair ret = null;
        try
        {
            string previewString = DuckFile.LoadString(DuckFile.editorPreviewDirectory + guid);
            if (previewString != null)
            {
                int invalidIndex = previewString.IndexOf('@');
                string invalidString = previewString.Substring(0, invalidIndex);
                string textureString = previewString.Substring(invalidIndex + 1);
                ret = new PreviewPair();
                ret.strange = invalidString[0] == '1';
                ret.challenge = invalidString[1] == '1';
                ret.arcade = invalidString[2] == '1';
                invalidString = invalidString.Substring(3);
                ret.preview = Editor.MassiveBitmapStringToTexture(textureString);
                if (ret.preview == null)
                {
                    throw new Exception("PreviewPair.preview is null");
                }
                if (invalidString.Length > 0)
                {
                    Dictionary<string, int> invalid = new Dictionary<string, int>();
                    string[] array = invalidString.Split('|');
                    for (int i = 0; i < array.Length; i++)
                    {
                        string[] split = array[i].Split(',');
                        if (split.Length == 2)
                        {
                            invalid[split[0]] = Convert.ToInt32(split[1]);
                        }
                    }
                    ret.invalid = invalid;
                }
                else
                {
                    ret.invalid = new Dictionary<string, int>();
                }
            }
        }
        catch (Exception)
        {
            DevConsole.Log(DCSection.General, "Failed to load preview string in metadata for " + guid);
            ret = null;
        }
        return ret;
    }

    private void RunSaveLevelPreviewTask(SaveLevelPreviewTask pTask)
    {
        try
        {
            pTask.ltData = new Color[pTask.levelTexture.Width * pTask.levelTexture.Height];
            pTask.levelTexture.GetData(pTask.ltData);
            pTask.ltWidth = pTask.levelTexture.Width;
            pTask.ltHeight = pTask.levelTexture.Height;
            new Task(delegate
            {
                try
                {
                    pTask.levelString = pTask.levelString + "@" + Editor.TextureToMassiveBitmapString(pTask.ltData, pTask.ltWidth, pTask.ltHeight);
                    lock (_completedPreviewTasks)
                    {
                        _completedPreviewTasks.Add(pTask);
                    }
                }
                catch (Exception)
                {
                }
            }).Start();
        }
        catch (Exception)
        {
        }
    }

    public PreviewPair SavePreview(Texture2D pPreview, Dictionary<string, int> pInvalidData, bool pStrange, bool pChallenge, bool pArcade)
    {
        try
        {
            string invalidString = "";
            invalidString += (pStrange ? "1" : "0");
            invalidString += (pChallenge ? "1" : "0");
            invalidString += (pArcade ? "1" : "0");
            foreach (KeyValuePair<string, int> pair in pInvalidData)
            {
                invalidString = invalidString + pair.Key + "," + pair.Value + "|";
            }
            RunSaveLevelPreviewTask(new SaveLevelPreviewTask
            {
                levelString = invalidString,
                levelTexture = pPreview,
                savePath = DuckFile.editorPreviewDirectory + guid
            });
        }
        catch (Exception)
        {
            DevConsole.Log(DCSection.General, "Failed to save preview string in metadata for " + guid);
        }
        return new PreviewPair
        {
            preview = pPreview,
            invalid = pInvalidData,
            strange = pStrange,
            challenge = pChallenge,
            arcade = pArcade
        };
    }

    public override BitBuffer Serialize(BitBuffer data = null, bool root = true)
    {
        return base.Serialize(data, root);
    }
}
