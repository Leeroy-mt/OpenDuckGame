using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DuckGame;

public class Album : Level
{
    private List<AlbumPic> _images = new List<AlbumPic>();

    private List<AlbumPage> _pages = new List<AlbumPage>();

    private List<Texture2D> _textures = new List<Texture2D>();

    private int _currentPage;

    private int _prevPage = -1;

    private Sprite _album;

    private Sprite _screen;

    private Material _pageMaterial;

    private BitmapFont _font;

    private List<LockerStat> _stats = new List<LockerStat>();

    private bool _quit;

    public Album()
    {
        _centeredView = true;
    }

    public override void Initialize()
    {
        _album = new Sprite("album");
        _screen = new Sprite("albumpic");
        _pageMaterial = new MaterialAlbum();
        _font = new BitmapFont("biosFont", 8);
        _stats.Add(new LockerStat("QUACKS: " + Global.data.quacks.valueInt, Color.DarkSlateGray));
        _stats.Add(new LockerStat("", Color.Red));
        _stats.Add(new LockerStat("TIME SPENT", Color.DarkSlateGray));
        _stats.Add(new LockerStat("IN MATCHES: " + TimeSpan.FromSeconds((float)Global.data.timeInMatches).ToString("hh\\:mm\\:ss"), Color.DarkSlateGray));
        _stats.Add(new LockerStat("IN ARCADE: " + TimeSpan.FromSeconds((float)Global.data.timeInArcade).ToString("hh\\:mm\\:ss"), Color.DarkSlateGray));
        _stats.Add(new LockerStat("IN EDITOR: " + TimeSpan.FromSeconds((float)Global.data.timeInEditor).ToString("hh\\:mm\\:ss"), Color.DarkSlateGray));
        string[] files = DuckFile.GetFiles(DuckFile.albumDirectory);
        foreach (string file in files)
        {
            try
            {
                DateTime date = DateTime.Parse(Path.GetFileNameWithoutExtension(file).Replace(';', ':'), CultureInfo.InvariantCulture);
                _images.Add(new AlbumPic
                {
                    file = file,
                    date = date
                });
            }
            catch
            {
            }
        }
        _pages.Add(new AlbumPage
        {
            caption = "LIFETIME STATS",
            statPage = true
        });
        _images = _images.OrderBy((AlbumPic x) => x.date).ToList();
        AlbumPage page = null;
        int numPages = 1;
        foreach (AlbumPic image in _images)
        {
            string monthName = image.date.ToString("MMMM", CultureInfo.InvariantCulture) + " " + image.date.Year;
            if (page == null)
            {
                numPages = 1;
                page = new AlbumPage();
                page.caption = monthName;
                _pages.Add(page);
            }
            if (!page.caption.Contains(monthName))
            {
                numPages = 1;
                page = new AlbumPage();
                page.caption = monthName;
                _pages.Add(page);
            }
            if (page.pics.Count == 4)
            {
                numPages++;
                page = new AlbumPage();
                page.caption = monthName + " (" + numPages + ")";
                _pages.Add(page);
            }
            page.pics.Add(image);
        }
        HUD.AddCornerControl(HUDCorner.BottomRight, "@WASD@FLIP PAGE");
        HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@BACK");
        base.Initialize();
    }

    public override void Update()
    {
        base.Update();
        if (_pages.Count > 0)
        {
            if (Input.Pressed("MENURIGHT"))
            {
                _currentPage++;
            }
            if (Input.Pressed("MENULEFT"))
            {
                _currentPage--;
            }
            if (_currentPage < 0)
            {
                _currentPage = 0;
            }
            if (_currentPage == _pages.Count)
            {
                _currentPage = _pages.Count - 1;
            }
            if (_currentPage != _prevPage)
            {
                _prevPage = _currentPage;
                foreach (Texture2D texture in _textures)
                {
                    texture.Dispose();
                }
                _textures.Clear();
                SFX.Play("page");
                foreach (AlbumPic pic in _pages[_currentPage].pics)
                {
                    try
                    {
                        Texture2D fileTexture;
                        using (FileStream fileStream = new FileStream(pic.file, FileMode.Open))
                        {
                            fileTexture = Texture2D.FromStream(Graphics.device, fileStream);
                        }
                        _textures.Add(fileTexture);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        if (Input.Pressed("CANCEL"))
        {
            _quit = true;
            HUD.CloseAllCorners();
        }
        Graphics.fade = Lerp.Float(Graphics.fade, _quit ? 0f : 1f, 0.05f);
        if (Graphics.fade < 0.01f && _quit)
        {
            Level.current = new DoorRoom();
        }
    }

    public override void Draw()
    {
        base.Draw();
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.HUD)
        {
            _album.Depth = -0.8f;
            Graphics.Draw(_album, 0f, 0f);
            _screen.Depth = -0.6f;
            if (_pages.Count > 0)
            {
                int picIndex = 0;
                if (_pages[_currentPage].statPage)
                {
                    int index = 0;
                    foreach (LockerStat stat in _stats)
                    {
                        Vec2 textPos = new Vec2(160f, 40 + index * 10);
                        string text = stat.name;
                        Graphics.DrawString(text, textPos + new Vec2((0f - Graphics.GetStringWidth(text)) / 2f, 0f), stat.color, 0.5f);
                        index++;
                    }
                }
                else
                {
                    for (int ypos = 0; ypos < 2; ypos++)
                    {
                        for (int xpos = 0; xpos < 2; xpos++)
                        {
                            if (picIndex < _textures.Count)
                            {
                                if (Graphics.width > 1280)
                                {
                                    Vec2 pos = new Vec2(52f, 35f);
                                    float scale = 0.3f;
                                    Vec2 picPos = new Vec2(pos.X + (float)(xpos * 110), pos.Y + (float)(ypos * 65));
                                    Graphics.Draw(_textures[picIndex], picPos.X, picPos.Y, scale, scale);
                                    Graphics.DrawRect(picPos + new Vec2(-3f, -3f), picPos + new Vec2((float)_textures[picIndex].Width * scale + 3f, (float)_textures[picIndex].Height * scale + 3f), Color.White, -0.7f);
                                }
                                else
                                {
                                    Vec2 pos2 = new Vec2(65f, 40f);
                                    float scale2 = 0.25f;
                                    Vec2 picPos2 = new Vec2(pos2.X + (float)(xpos * 100), pos2.Y + (float)(ypos * 65));
                                    Graphics.Draw(_textures[picIndex], picPos2.X, picPos2.Y, scale2, scale2);
                                    Graphics.DrawRect(picPos2 + new Vec2(-3f, -3f), picPos2 + new Vec2((float)_textures[picIndex].Width * scale2 + 3f, (float)_textures[picIndex].Height * scale2 + 3f), Color.White, -0.7f);
                                }
                            }
                            picIndex++;
                        }
                    }
                }
                string name = _pages[_currentPage].caption;
                _font.Draw(name, new Vec2(Layer.HUD.width / 2f - _font.GetWidth(name) / 2f - 4f, 18f), Color.DarkSlateGray, -0.5f);
            }
            else
            {
                string name2 = "EMPTY ALBUM :(";
                _font.Draw(name2, new Vec2(Layer.HUD.width / 2f - _font.GetWidth(name2) / 2f - 4f, 18f), Color.DarkSlateGray, -0.5f);
            }
        }
        base.PostDrawLayer(layer);
    }
}
