using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class MapPack : ContentPack
{
	private string _name;

	private Sprite _icon;

	private Mod _mod;

	private string _needsPreviewGenerationDir;

	public string path;

	public static List<MapPack> active = new List<MapPack>();

	public static List<MapPack> _mapPacks = new List<MapPack>();

	public static ReskinPack context;

	private Tex2D _preview;

	public string name
	{
		get
		{
			if (_mod == null)
			{
				return _name;
			}
			return _mod.configuration.name;
		}
	}

	public Sprite icon => _icon;

	public Mod mod => _mod;

	public Tex2D preview => _preview;

	public MapPack()
		: base(null)
	{
	}

	public static Mod LoadMapPack(string pDir, Mod pExistingMod = null, ModConfiguration pExistingConfig = null)
	{
		MapPack pack = new MapPack
		{
			_name = Path.GetFileName(pDir),
			path = pDir
		};
		_mapPacks.Add(pack);
		if (pExistingMod == null && pExistingConfig == null)
		{
			if (!DuckFile.FileExists(pDir + "/preview.png"))
			{
				File.Copy(DuckFile.contentDirectory + "/mappack_preview.pngfile", pDir + "/preview.png");
			}
			if (!DuckFile.FileExists(pDir + "/icon.png"))
			{
				File.Copy(DuckFile.contentDirectory + "/mappack_icon.pngfile", pDir + "/icon.png");
			}
			if (!DuckFile.FileExists(pDir + "/mappack_info.txt"))
			{
				string defaultAuthor = "Dan Rando";
				if (Steam.user != null)
				{
					defaultAuthor = Steam.user.name;
				}
				DuckFile.SaveString(pack.name + "\n" + defaultAuthor + "\nEdit info.txt to change this information!\n<add a 1280x720 PNG file called 'screenshot.png' to set a custom workshop image!>", pDir + "/mappack_info.txt");
			}
		}
		Mod m = pExistingMod;
		if (m == null)
		{
			m = new ClientMod(pDir + "/", pExistingConfig, "mappack_info.txt");
			m.configuration.LoadOrCreateConfig();
			m.configuration.SetModType(ModConfiguration.Type.MapPack);
			ModLoader.AddMod(m);
		}
		pack._mod = m;
		m.SetPriority(Priority.MapPack);
		m.configuration.SetMapPack(pack);
		if (DuckFile.FileExists(pDir + "/icon.png"))
		{
			try
			{
				Tex2D tex = ContentPack.LoadTexture2D(pDir + "/icon.png");
				pack._icon = new Sprite(tex);
			}
			catch (Exception)
			{
				pack._icon = new Sprite("default_mappack_icon");
			}
		}
		if (!m.configuration.disabled)
		{
			active.Add(pack);
			if (!DuckFile.FileExists(pDir + "/screenshot.png"))
			{
				if (DuckFile.FileExists(pDir + "/screenshot_autogen.png"))
				{
					pack._preview = ContentPack.LoadTexture2D(pDir + "/screenshot_autogen.png");
				}
				else
				{
					pack._needsPreviewGenerationDir = pDir;
				}
			}
			else
			{
				pack._preview = ContentPack.LoadTexture2D(pDir + "/screenshot.png");
			}
		}
		return m;
	}

	public static void InitializeMapPacks()
	{
		string[] directories = DuckFile.GetDirectories(DuckFile.mappackDirectory);
		for (int i = 0; i < directories.Length; i++)
		{
			LoadMapPack(directories[i]);
		}
		if (Steam.user != null)
		{
			directories = DuckFile.GetDirectories(DuckFile.globalMappackDirectory);
			for (int i = 0; i < directories.Length; i++)
			{
				LoadMapPack(directories[i]);
			}
		}
	}

	public static void RegeneratePreviewsIfNecessary()
	{
		try
		{
			foreach (MapPack p in _mapPacks)
			{
				if (p._needsPreviewGenerationDir != null)
				{
					p.RegeneratePreviewImage(p._needsPreviewGenerationDir + "/screenshot_autogen.png");
				}
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
		if (pPath == null)
		{
			pPath = path + "/screenshot_autogen.png";
		}
		int previewWidth = 1280;
		int previewHeight = 720;
		RenderTarget2D previewTarget = new RenderTarget2D(previewWidth, previewHeight);
		Viewport oldV = Graphics.viewport;
		RenderTarget2D oldTarget = Graphics.GetRenderTarget();
		Sprite sprite = new Sprite("shiny");
		Graphics.SetRenderTarget(previewTarget);
		Graphics.viewport = new Viewport(0, 0, previewWidth, previewHeight);
		Camera cam = new Camera(0f, 0f, previewWidth, previewHeight);
		Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, cam.getMatrix());
		Graphics.Draw(sprite.texture, 0f, 0f, 4f, 4f, 0.1f);
		string[] files = Directory.GetFiles(_mod.configuration.directory, "*.lev", SearchOption.AllDirectories);
		int num = 0;
		int rows = (int)Math.Ceiling(Math.Sqrt(files.Count()));
		float scaleFactor = 1280f / (float)rows / 1280f * 4f;
		Vec2 offset = Vec2.Zero;
		string[] array = files;
		foreach (string f in array)
		{
			if (num == rows * rows)
			{
				break;
			}
			try
			{
				LevelMetaData.PreviewPair pair = Content.GeneratePreview(f);
				if (pair.preview != null && pair.preview.Width == 320 && pair.preview.Height == 200)
				{
					float scale = 0.95f;
					Vec2 scaledSize = new Vec2((float)pair.preview.Width * scaleFactor * scale, (float)pair.preview.Height * scaleFactor * scale);
					Vec2 realSize = new Vec2((float)pair.preview.Width * scaleFactor, (float)pair.preview.Height * scaleFactor);
					Graphics.Draw(pair.preview, new Vec2(offset.x + realSize.x / 2f - scaledSize.x / 2f, offset.y + realSize.y / 2f - scaledSize.y / 2f), new Rectangle(0f, 10f, 320f, 180f), Color.White, 0f, Vec2.Zero, new Vec2(scale * scaleFactor), SpriteEffects.None, 0.9f);
				}
			}
			catch (Exception)
			{
			}
			num++;
			if (num % rows == 0)
			{
				offset.x = 0f;
				offset.y += previewHeight / rows;
			}
			else
			{
				offset.x += previewWidth / rows;
			}
		}
		Graphics.screen.End();
		Graphics.SetRenderTarget(oldTarget);
		Graphics.viewport = oldV;
		_preview = previewTarget.ToTex2D();
		FileStream fs = File.Create(pPath);
		(_preview.nativeObject as Texture2D).SaveAsPng(fs, _preview.width, _preview.height);
		fs.Close();
		return pPath;
	}
}
