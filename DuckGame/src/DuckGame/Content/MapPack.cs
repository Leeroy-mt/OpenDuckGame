using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

#if FACEPUNCH
using Steamworks;
#else
using Steam;
#endif

namespace DuckGame;

public class MapPack : ContentPack
{
    #region Public Fields

    public string path;

    public static ReskinPack context;

    public static List<MapPack> active = [];

    public static List<MapPack> _mapPacks = [];

    #endregion

    #region Private Fields

    string _name;
    string _needsPreviewGenerationDir;

    Sprite _icon;

    Mod _mod;

    Texture2D _preview;

    #endregion

    #region Public Properties

    public string name =>
        _mod == null
        ? _name
        : _mod.configuration.name;

    public Sprite icon => _icon;

    public Mod mod => _mod;

    public Texture2D preview => _preview;

    #endregion

    public MapPack()
        : base(null) { }

    #region Public Methods

    public static Mod LoadMapPack(string pDir, Mod pExistingMod = null, ModConfiguration pExistingConfig = null)
    {
        MapPack pack = new()
        {
            _name = Path.GetFileName(pDir),
            path = pDir
        };

        _mapPacks.Add(pack);
        if (pExistingMod == null && pExistingConfig == null)
        {
            if (!DuckFile.FileExists($"{pDir}/preview.png"))
                File.Copy($"{DuckFile.contentDirectory}/mappack_preview.pngfile", $"{pDir}/preview.png");

            if (!DuckFile.FileExists($"{pDir}/icon.png"))
                File.Copy($"{DuckFile.contentDirectory}/mappack_icon.pngfile", $"{pDir}/icon.png");

            if (!DuckFile.FileExists($"{pDir}/mappack_info.txt"))
            {
                var defaultAuthor = "Dan Rando";
#if FACEPUNCH
                if (SteamClient.SteamId != 0)
                    defaultAuthor = FacepunchSteam.Me.Name;
#else
                if (DGSteam.User != null)
                    defaultAuthor = DGSteam.User.Name;
#endif
                DuckFile.SaveString($"{pack.name}\n{defaultAuthor}\nEdit info.txt to change this information!\n<add a 1280x720 PNG file called 'screenshot.png' to set a custom workshop image!>", pDir + "/mappack_info.txt");
            }
        }

        Mod m = pExistingMod;
        if (m == null)
        {
            m = new ClientMod($"{pDir}/", pExistingConfig, "mappack_info.txt");
            m.configuration.LoadOrCreateConfig();
            m.configuration.SetModType(ModConfiguration.Type.MapPack);
            ModLoader.AddMod(m);
        }

        pack._mod = m;
        m.SetPriority(Priority.MapPack);
        m.configuration.SetMapPack(pack);
        if (DuckFile.FileExists($"{pDir}/icon.png"))
        {
            try
            {
                var tex = ContentPack.LoadTexture2D($"{pDir}/icon.png");
                pack._icon = new Sprite(tex);
            }
            catch
            {
                pack._icon = new Sprite("default_mappack_icon");
            }
        }

        if (!m.configuration.disabled)
        {
            active.Add(pack);
            if (!DuckFile.FileExists($"{pDir}/screenshot.png"))
            {
                if (DuckFile.FileExists($"{pDir}/screenshot_autogen.png"))
                    pack._preview = LoadTexture2D($"{pDir}/screenshot_autogen.png");
                else
                    pack._needsPreviewGenerationDir = pDir;
            }
            else
            {
                pack._preview = LoadTexture2D($"{pDir}/screenshot.png");
            }
        }

        return m;
    }

    public static void InitializeMapPacks()
    {
        var directories = DuckFile.GetDirectories(DuckFile.mappackDirectory);
        for (int i = 0; i < directories.Length; i++)
            LoadMapPack(directories[i]);

#if FACEPUNCH
        if (FacepunchSteam.SteamId != 0)
#else
        if (DGSteam.User != null)
#endif
        {
            directories = DuckFile.GetDirectories(DuckFile.globalMappackDirectory);
            for (int i = 0; i < directories.Length; i++)
                LoadMapPack(directories[i]);
        }
    }

    public static void RegeneratePreviewsIfNecessary()
    {
        try
        {
            foreach (MapPack p in _mapPacks)
            {
                if (p._needsPreviewGenerationDir != null)
                    p.RegeneratePreviewImage($"{p._needsPreviewGenerationDir}/screenshot_autogen.png");
            }
        }
        catch (Exception ex)
        {
            DevConsole.Log("MapPack.RegeneratePreviewsIfNecessary failed with error:");
            DevConsole.Log(ex.Message);
        }
    }

    public string RegeneratePreviewImage(string pPath)
    {
        pPath ??= $"{path}/screenshot_autogen.png";

        var previewWidth = 1280;
        var previewHeight = 720;
        var previewTarget = RenderTarget2D.CreateSetUpTarget(previewWidth, previewHeight);
        Viewport oldV = Graphics.viewport;
        var oldTarget = Graphics.GetRenderTarget();
        Sprite sprite = new("shiny");
        Graphics.SetRenderTarget(previewTarget);
        Graphics.viewport = new Viewport(0, 0, previewWidth, previewHeight);
        Camera cam = new(0, 0, previewWidth, previewHeight);
        Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, cam.getMatrix());
        Graphics.Draw(sprite.texture, 0, 0, 4, 4, 0.1f);
        var files = Directory.GetFiles(_mod.configuration.directory, "*.lev", SearchOption.AllDirectories);
        var num = 0;
        var rows = (int)Math.Ceiling(Math.Sqrt(files.Length));
        var scaleFactor = 1280F / rows / 1280F * 4;
        var offset = Vector2.Zero;
        var array = files;
        foreach (string f in array)
        {
            if (num == rows * rows)
                break;

            try
            {
                var pair = Content.GeneratePreview(f);
                if (pair.preview != null && pair.preview.Width == 320 && pair.preview.Height == 200)
                {
                    var scale = 0.95f;
                    Vector2 scaledSize = new(pair.preview.Width * scaleFactor * scale, pair.preview.Height * scaleFactor * scale);
                    Vector2 realSize = new(pair.preview.Width * scaleFactor, pair.preview.Height * scaleFactor);
                    Graphics.Draw(pair.preview, new Vector2(offset.X + realSize.X / 2 - scaledSize.X / 2, offset.Y + realSize.Y / 2 - scaledSize.Y / 2), new RectangleF(0, 10, 320, 180), Color.White, 0, Vector2.Zero, new Vector2(scale * scaleFactor), SpriteEffects.None, 0.9f);
                }
            }
            catch
            {
            }

            num++;
            if (num % rows == 0)
            {
                offset.X = 0f;
                offset.Y += previewHeight / rows;
            }
            else
            {
                offset.X += previewWidth / rows;
            }
        }

        Graphics.screen.End();
        Graphics.SetRenderTarget(oldTarget);
        Graphics.viewport = oldV;
        _preview = previewTarget.GetTexture2D();
        FileStream fs = File.Create(pPath);
        _preview.SaveAsPng(fs, _preview.Width, _preview.Height);
        fs.Close();
        return pPath;
    }

    #endregion
}