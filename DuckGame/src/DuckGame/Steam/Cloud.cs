using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace DuckGame;

public static class Cloud
{
    public class UICloudProcess : UIMenu
    {
        private UIComponent _openOnClose;

        private UIBox _box;

        private bool _closing;

        public UICloudProcess(string pProcessName, UIComponent pOpenOnClose)
            : base(pProcessName, Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 180f, 50f)
        {
            if (pOpenOnClose != null)
            {
                _openOnClose = pOpenOnClose.rootMenu;
            }
            _box = new UIBox(0f, 0f, -1f, 26f, vert: true, isVisible: false);
            Add(_box);
        }

        public override void Close()
        {
            if (!_closing)
            {
                _closing = true;
                if (_openOnClose != null)
                {
                    MonoMain.pauseMenu = _openOnClose;
                    _openOnClose.Open();
                }
                base.Close();
            }
        }

        public override void Draw()
        {
            if (base.open)
            {
                string text = "" + "Working... (" + (int)(progress * 100f) + "%)";
                Graphics.DrawRect(new Rectangle(_box.X - _box.halfWidth + 8f, _box.Y, _box.width - 16f, 10f), Color.LightGray, 0.8f);
                Graphics.DrawRect(new Rectangle(_box.X - _box.halfWidth + 8f, _box.Y, Lerp.FloatSmooth(0f, _box.width - 16f, progress), 10f), Color.White, 0.8f);
                float width = Graphics.GetStringWidth(text);
                Graphics.DrawString(text, new Vec2(_box.X - width / 2f, _box.Y - 10f), Color.White, 0.8f);
                if (!processing)
                {
                    Close();
                }
            }
            base.Draw();
        }
    }

    private class CloudOperation
    {
        public enum Type
        {
            Delete,
            ReadFromCloud,
            WriteToCloud,
            FinishDeletingCloud,
            FinishDeletingFiles,
            FinishRecoveringFiles,
            UploadNewFiles
        }

        public Type type;

        public CloudFile file;

        private static BitBuffer _backupBuffer;

        private static int _backupPart;

        public void Execute()
        {
            if (type == Type.FinishDeletingCloud)
            {
                HUD.AddPlayerChangeDisplay("@PLUG@|LIME|Cloud data cleared!");
            }
            else if (type == Type.FinishDeletingFiles)
            {
                HUD.AddPlayerChangeDisplay("@PLUG@|LIME|Local deletions applied!");
            }
            else if (type == Type.FinishRecoveringFiles)
            {
                HUD.AddPlayerChangeDisplay("@PLUG@|LIME|Local files recovered!");
            }
            else if (type == Type.Delete)
            {
                if (!uploadEnabled)
                {
                    if (MonoMain.logFileOperations)
                    {
                        DevConsole.Log(DCSection.General, "Cloud.Execute.Delete(" + file.cloudPath + ") skipped (uploadEnabled == false)");
                    }
                    return;
                }
                if (MonoMain.logFileOperations)
                {
                    DevConsole.Log(DCSection.General, "Cloud.Execute.Delete(" + file.cloudPath + ")");
                }
                Steam.FileDelete(file.cloudPath);
                file.cloudDate = DateTime.MinValue;
            }
            else if (type == Type.ReadFromCloud)
            {
                if (!downloadEnabled)
                {
                    if (MonoMain.logFileOperations)
                    {
                        DevConsole.Log(DCSection.General, "Cloud.Execute.ReadFromCloud(" + file?.ToString() + ") skipped (downloadEnabled == false)");
                    }
                }
                else
                {
                    ReplaceLocalFileWithCloudFile(file);
                }
            }
            else if (type == Type.WriteToCloud)
            {
                if (!uploadEnabled)
                {
                    if (MonoMain.logFileOperations)
                    {
                        DevConsole.Log(DCSection.General, "Cloud.Execute.WriteToCloud(" + file.cloudPath + ") skipped (uploadEnabled == false)");
                    }
                    return;
                }
                byte[] writeData = File.ReadAllBytes(file.localPath);
                Steam.FileWrite(file.cloudPath, writeData, writeData.Length);
                if (MonoMain.logFileOperations)
                {
                    DevConsole.Log(DCSection.General, "Cloud.Execute.WriteToCloud(" + file.cloudPath + ")");
                }
                file.cloudDate = DateTime.Now;
            }
            else if (type == Type.UploadNewFiles)
            {
                loaded = true;
            }
        }
    }

    public static bool uploadEnabled = true;

    public static bool downloadEnabled = true;

    public static bool nocloud;

    public static HashSet<string> deletedFiles = new HashSet<string>();

    private static Dictionary<string, DateTime> _indexTable = new Dictionary<string, DateTime>();

    private static Queue<CloudOperation> _operations = new Queue<CloudOperation>();

    private static int _totalOperations;

    private static bool _initializedCloud;

    public static bool loaded = false;

    public static bool enabled
    {
        get
        {
            if (!_initializedCloud || nocloud)
            {
                return false;
            }
            if (!Steam.IsInitialized() || !Steam.CloudEnabled())
            {
                return false;
            }
            return true;
        }
    }

    public static bool hasPendingDeletions => deletedFiles.Count > 0;

    public static bool processing => _operations.Count > 0;

    public static float progress
    {
        get
        {
            if (_totalOperations != 0 && _operations.Count != 0)
                return Math.Max(1 - _operations.Count / (float)_totalOperations, 0);
            return 1;
        }
    }

    public static void Initialize()
    {
        if (Steam.IsInitialized())
        {
            DownloadLatestData();
        }
    }

    public static void Update()
    {
        if (_operations.Count <= 0)
        {
            return;
        }
        if (_operations.Count > _totalOperations)
        {
            _totalOperations = _operations.Count;
        }
        int numToProcess = 10;
        while (_operations.Count > 0)
        {
            _operations.Dequeue().Execute();
            if (_operations.Count == 0)
            {
                _totalOperations = 0;
                break;
            }
            numToProcess--;
            if (numToProcess < 0)
            {
                break;
            }
        }
    }

    public static void Delete(string pPath)
    {
        if (!uploadEnabled)
        {
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Cloud.Delete(" + pPath + ") skipped (uploadEnabled = false)");
            }
            return;
        }
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "Cloud.Delete(" + pPath + ")");
        }
        if (!Steam.IsInitialized())
        {
            return;
        }
        CloudFile c = CloudFile.GetLocal(pPath, pDelete: true);
        if (c != null)
        {
            Steam.FileDelete(c.cloudPath);
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Steam.FileDelete(" + c.cloudPath + ")");
            }
            c.cloudDate = DateTime.MinValue;
        }
    }

    public static void Write(string path)
    {
        if (!uploadEnabled)
        {
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Cloud.Write(" + path + ") skipped (uploadEnabled = false)");
            }
            return;
        }
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "Cloud.Write(" + path + ")");
        }
        if (!Steam.IsInitialized())
        {
            return;
        }
        CloudFile c = CloudFile.GetLocal(path);
        if (c != null && !c.isOld)
        {
            byte[] docData = File.ReadAllBytes(c.localPath);
            Steam.FileWrite(c.cloudPath, docData, docData.Length);
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Steam.FileWrite(" + c.cloudPath + ")");
            }
            c.localDate = DateTime.Now;
            c.cloudDate = DateTime.Now;
            File.SetLastWriteTime(c.localPath, DateTime.Now);
        }
    }

    public static void ReplaceLocalFileWithCloudFile(string pLocalPath, string pCloudString = null)
    {
        if (MonoMain.editSave)
        {
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Cloud.ReplaceLocalFileWithCloudFile(" + pLocalPath + ") skipped (MonoMain.editSave)");
            }
            return;
        }
        pLocalPath = DuckFile.PreparePath(pLocalPath);
        if (Steam.IsInitialized())
        {
            ReplaceLocalFileWithCloudFile(CloudFile.GetLocal(pLocalPath));
        }
    }

    public static void ReplaceLocalFileWithCloudFile(CloudFile pFile)
    {
        if (!enabled)
        {
            return;
        }
        if (!downloadEnabled)
        {
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Cloud.ReplaceLocalFileWithCloudFile(" + pFile.cloudPath + ") skipped (downloadEnabled = false)");
            }
        }
        else if (MonoMain.editSave)
        {
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Cloud.ReplaceLocalFileWithCloudFile(" + pFile.cloudPath + ") skipped (MonoMain.editSave)");
            }
        }
        else
        {
            if (pFile == null)
            {
                return;
            }
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Cloud.ReplaceLocalFileWithCloudFile(" + pFile.cloudPath + ")");
            }
            byte[] data = Steam.FileRead(pFile.cloudPath);
            if (data == null)
            {
                return;
            }
            DuckFile.TryFileOperation(delegate
            {
                DuckFile.TryClearAttributes(pFile.localPath);
                if (File.Exists(pFile.localPath))
                {
                    File.Delete(pFile.localPath);
                }
                DuckFile.CreatePath(pFile.localPath, ignoreLast: true);
                FileStream fileStream = File.Create(pFile.localPath);
                fileStream.Write(data, 0, data.Length);
                fileStream.Close();
                pFile.localDate = DateTime.Now;
            }, "ReplaceLocalFileWithCloudFile(" + pFile.localPath + ")");
        }
    }

    public static void ZipUpCloudData(string pFile)
    {
        using FileStream fileStream = new FileStream(pFile, FileMode.Create);
        using ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Create, leaveOpen: true);
        int num = Steam.FileGetCount();
        for (int i = 0; i < num; i++)
        {
            string file = Steam.FileGetName(i);
            if (!file.EndsWith(".lev") && !file.EndsWith(".png") && !file.EndsWith(".play"))
            {
                using Stream entryStream = archive.CreateEntry(file).Open();
                byte[] data = Steam.FileRead(file);
                entryStream.Write(data, 0, data.Length);
            }
        }
    }

    public static void DeleteAllCloudData(bool pNewDataOnly)
    {
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "Cloud.DeleteAllCloudData(" + pNewDataOnly + ")");
        }
        UICloudProcess uICloudProcess = new UICloudProcess("DELETING CLOUD", MonoMain.pauseMenu);
        Level.Add(uICloudProcess);
        uICloudProcess.Open();
        MonoMain.pauseMenu = uICloudProcess;
        int num = Steam.FileGetCount();
        for (int i = 0; i < num; i++)
        {
            CloudFile c = CloudFile.Get(Steam.FileGetName(i));
            if (c != null && (!pNewDataOnly || c.cloudPath.StartsWith("nq500000_")))
            {
                _operations.Enqueue(new CloudOperation
                {
                    type = CloudOperation.Type.Delete,
                    file = c
                });
            }
        }
        _operations.Enqueue(new CloudOperation
        {
            type = CloudOperation.Type.FinishDeletingCloud
        });
    }

    private static void DownloadLatestData()
    {
        _initializedCloud = true;
        CloudFile.Initialize();
        if (!enabled)
        {
            return;
        }
        int num = Steam.FileGetCount();
        for (int i = 0; i < num; i++)
        {
            CloudFile c = CloudFile.Get(Steam.FileGetName(i));
            if (c == null)
            {
                continue;
            }
            CloudOperation pOperation = null;
            if (MonoMain.recoversave && c.cloudPath.StartsWith("nq500000_"))
            {
                pOperation = new CloudOperation
                {
                    type = CloudOperation.Type.WriteToCloud,
                    file = c
                };
            }
            else if (num > 100 && c.cloudPath.StartsWith("nq403216_") && (c.cloudPath.Contains("nq403216_Levels/Btooom Mod/") || c.cloudPath.Contains("nq403216_Levels/Basket/") || c.cloudPath.Contains("nq403216_Levels/Dorito/") || c.cloudPath.Contains("nq403216_Levels/DWEP levels") || c.cloudPath.Contains("nq403216_Levels/DWEPdev levels") || c.cloudPath.Contains("nq403216_Levels/EDMP/") || c.cloudPath.Contains("nq403216_Levels/Fair_Spawn_Maps/") || c.cloudPath.Contains("nq403216_Levels/NDC_1v1") || c.cloudPath.Contains("nq403216_Levels/QC 1V1/") || c.cloudPath.Contains("nq403216_Levels/Random Stuff/") || c.cloudPath.Contains("nq403216_Levels/SD 1v1 Pack/") || c.cloudPath.Contains("nq403216_Levels/UFF/") || c.cloudPath.Contains("nq403216_Levels/TMG/") || c.cloudPath.Contains("nq403216_Levels/C44PMaps/") || c.cloudPath.Contains("nq403216_Levels/DuckUnbreakable/") || c.cloudPath.Contains("nq403216_Levels/DWEP levels 1point3 edition/") || c.cloudPath.Contains("nq403216_Levels/antikore/")))
            {
                continue;
            }
            if (pOperation == null && c.localDate == DateTime.MinValue)
            {
                pOperation = new CloudOperation
                {
                    type = CloudOperation.Type.ReadFromCloud,
                    file = c
                };
            }
            if (pOperation != null)
            {
                if (c.localPath.EndsWith("options.dat") || c.localPath.EndsWith("localsettings.dat") || c.localPath.EndsWith("global.dat"))
                {
                    pOperation.Execute();
                }
                else
                {
                    _operations.Enqueue(pOperation);
                }
            }
            if (c.cloudDate == DateTime.MinValue)
            {
                c.cloudDate = DateTime.Now;
            }
        }
        _operations.Enqueue(new CloudOperation
        {
            type = CloudOperation.Type.UploadNewFiles
        });
    }
}
