using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace DuckGame;

public class Custom
{
    private static Dictionary<CustomType, Dictionary<string, CustomTileData>> _customTilesetDataInternal = new Dictionary<CustomType, Dictionary<string, CustomTileData>>
    {
        {
            CustomType.Block,
            new Dictionary<string, CustomTileData>()
        },
        {
            CustomType.Platform,
            new Dictionary<string, CustomTileData>()
        },
        {
            CustomType.Background,
            new Dictionary<string, CustomTileData>()
        },
        {
            CustomType.Parallax,
            new Dictionary<string, CustomTileData>()
        }
    };

    private static Dictionary<CustomType, Dictionary<string, CustomTileData>> _customTilesetDataInternalPreview = new Dictionary<CustomType, Dictionary<string, CustomTileData>>
    {
        {
            CustomType.Block,
            new Dictionary<string, CustomTileData>()
        },
        {
            CustomType.Platform,
            new Dictionary<string, CustomTileData>()
        },
        {
            CustomType.Background,
            new Dictionary<string, CustomTileData>()
        },
        {
            CustomType.Parallax,
            new Dictionary<string, CustomTileData>()
        }
    };

    private static Dictionary<CustomType, string[]> _dataInternal = new Dictionary<CustomType, string[]>
    {
        {
            CustomType.Block,
            new string[3] { "", "", "" }
        },
        {
            CustomType.Platform,
            new string[3] { "", "", "" }
        },
        {
            CustomType.Background,
            new string[3] { "", "", "" }
        },
        {
            CustomType.Parallax,
            new string[1] { "" }
        }
    };

    private static Dictionary<CustomType, string[]> _dataInternalPreview = new Dictionary<CustomType, string[]>
    {
        {
            CustomType.Block,
            new string[3] { "", "", "" }
        },
        {
            CustomType.Platform,
            new string[3] { "", "", "" }
        },
        {
            CustomType.Background,
            new string[3] { "", "", "" }
        },
        {
            CustomType.Parallax,
            new string[1] { "" }
        }
    };

    private static Dictionary<CustomType, CustomTileDataChunk[]> _previewDataInternal = new Dictionary<CustomType, CustomTileDataChunk[]>
    {
        {
            CustomType.Block,
            new CustomTileDataChunk[3]
        },
        {
            CustomType.Platform,
            new CustomTileDataChunk[3]
        },
        {
            CustomType.Background,
            new CustomTileDataChunk[3]
        },
        {
            CustomType.Parallax,
            new CustomTileDataChunk[1]
        }
    };

    private static Dictionary<CustomType, CustomTileDataChunk[]> _previewDataInternalPreview = new Dictionary<CustomType, CustomTileDataChunk[]>
    {
        {
            CustomType.Block,
            new CustomTileDataChunk[3]
        },
        {
            CustomType.Platform,
            new CustomTileDataChunk[3]
        },
        {
            CustomType.Background,
            new CustomTileDataChunk[3]
        },
        {
            CustomType.Parallax,
            new CustomTileDataChunk[1]
        }
    };

    private static Dictionary<CustomType, Dictionary<string, CustomTileData>> _customTilesetData
    {
        get
        {
            if (!Content.renderingPreview)
            {
                return _customTilesetDataInternal;
            }
            return _customTilesetDataInternalPreview;
        }
    }

    public static Dictionary<CustomType, string[]> data
    {
        get
        {
            if (!Content.renderingPreview)
            {
                return _dataInternal;
            }
            return _dataInternalPreview;
        }
    }

    public static Dictionary<CustomType, CustomTileDataChunk[]> previewData
    {
        get
        {
            if (!Content.renderingPreview)
            {
                return _previewDataInternal;
            }
            return _previewDataInternalPreview;
        }
    }

    public static void Clear(CustomType pType, string pPath)
    {
    }

    public static void ClearCustomData()
    {
        foreach (KeyValuePair<CustomType, string[]> pair in data)
        {
            for (int i = 0; i < pair.Value.Length; i++)
            {
                pair.Value[i] = "";
                previewData[pair.Key][i] = null;
            }
        }
    }

    public static string ApplyCustomData(CustomTileData tData, int index, CustomType type)
    {
        string namEnd = tData.path + "@" + tData.checksum + ".png";
        if (tData.texture != null)
        {
            string nam = DuckFile.GetCustomDownloadDirectory(type) + namEnd;
            try
            {
                if (!DuckFile.FileExists(nam))
                {
                    DuckFile.CreatePath(nam);
                    FileStream f = File.Create(nam);
                    tData.texture.SaveAsPng(f, tData.texture.Width, tData.texture.Height);
                    f.Close();
                }
            }
            catch (Exception ex)
            {
                DevConsole.Log(DCSection.General, "Access error saving " + nam + ":");
                DevConsole.Log(DCSection.General, ex.Message);
            }
        }
        else if (tData.path == null)
        {
            return "";
        }
        _customTilesetData[type][namEnd] = tData;
        data[type][index] = namEnd;
        return namEnd;
    }

    public static CustomTileData LoadTileData(string path, CustomType type)
    {
        CustomTileData tileData = new CustomTileData();
        if (path == "" || path == null)
        {
            return tileData;
        }
        Texture2D tex = ContentPack.LoadTexture2D(DuckFile.GetCustomDirectory(type) + path + ".png");
        if (tex != null)
        {
            try
            {
                Color[] data = new Color[tex.Width * tex.Height];
                tex.GetData(data);
                for (int i = 0; i < 5; i++)
                {
                    int startX = 112;
                    int startY = 64 + i * 16;
                    if (i == 1)
                    {
                        int num = startY;
                        startY = startX;
                        startX = num;
                    }
                    switch (i)
                    {
                        case 3:
                            startX = 96;
                            startY = 112;
                            break;
                        case 4:
                            startX = 112;
                            startY = 112;
                            break;
                    }
                    int leftPos = -1;
                    int wide = 0;
                    for (int ypixel = 0; ypixel < 16; ypixel++)
                    {
                        bool found = false;
                        for (int xpixel = 0; xpixel < 16; xpixel++)
                        {
                            int pix = 0;
                            pix = ((i != 1) ? (startX + xpixel + (startY + ypixel) * tex.Width) : (startY + ypixel + (startX + xpixel) * tex.Width));
                            bool check = false;
                            if ((i != 3 && i != 4) ? (data[pix].r == 0 && data[pix].g == byte.MaxValue && data[pix].b == 0 && data[pix].a == byte.MaxValue) : (data[pix].a != 0))
                            {
                                if (leftPos == -1)
                                {
                                    leftPos = xpixel;
                                }
                            }
                            else if (leftPos != -1)
                            {
                                wide = xpixel - leftPos;
                                found = true;
                                break;
                            }
                        }
                        if (found)
                        {
                            break;
                        }
                    }
                    switch (i)
                    {
                        case 0:
                            tileData.verticalWidth = wide;
                            break;
                        case 1:
                            tileData.horizontalHeight = wide;
                            break;
                        case 2:
                            tileData.verticalWidthThick = wide;
                            break;
                        case 3:
                            tileData.leftNubber = wide != 0;
                            break;
                        case 4:
                            tileData.rightNubber = wide != 0;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                tex = null;
            }
            tileData.texture = tex;
            tileData.path = path;
            _customTilesetData[type][path] = tileData;
        }
        return tileData;
    }

    public static CustomTileData GetData(int index, CustomType type)
    {
        CustomTileData tileData = null;
        if (!_customTilesetData[type].TryGetValue(data[type][index], out tileData))
        {
            tileData = LoadTileData(data[type][index], type);
        }
        return tileData;
    }
}
