using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;


#if FACEPUNCH
using Steamworks;
using Steamworks.Ugc;
using Steamworks.Data;
#else
using Steam;
#endif

namespace DuckGame;

internal class WorkshopBrowser : Level
{
    private class Item
    {
        public string description;

#if FACEPUNCH
        public Steamworks.Ugc.Item details;
#else
        public WorkshopQueryResultDetails details;
#endif

        Texture2D _preview;

        public PNGData _previewData;

        private static Dictionary<ulong, Item> _items = [];

        public string name => details.Title;

        public Texture2D preview
        {
            get
            {
                if (_preview == null && _previewData != null)
                {
                    _preview = Texture2D.GetTex2DLike(_previewData.width, _previewData.height);
                    _preview.SetData(_previewData.data);
                }
                return _preview;
            }
        }

        public static Item Get(ulong pID)
        {
            Item item = null;
            if (!_items.TryGetValue(pID, out item))
            {
                Item item2 = (_items[pID] = new Item());
                item = item2;
            }
            return item;
        }

        internal Item()
        {
        }
    }

    private class Group
    {
        public string name;

        public List<Item> items = [];

        public List<string> tags;

        public string searchText;

        public ulong userID;

        public WorkshopQueryFilterOrder orderMode;

#if FACEPUNCH
        Query _currentQuery;
#else
        WorkshopQueryUGC _currentQuery;
#endif

        public Group(string pName, WorkshopQueryFilterOrder pOrder, ulong pUserID, string pSearchText, params string[] pTags)
        {
            name = pName;
            orderMode = pOrder;
            tags = [.. pTags];
            searchText = pSearchText;
            userID = pUserID;
            OpenPage(0);
        }

        public void OpenPage(int pIndex)
        {
            if (userID != 0L)
            {
#if FACEPUNCH
                _currentQuery = Query.Items
                    .WhereUserSubscribed(userID)
                    .SortBySubscriptionDate();
#else
                _currentQuery = DGSteam.CreateQueryUser(userID, WorkshopList.Subscribed, WorkshopType.Items, WorkshopSortOrder.SubscriptionDateDesc);
#endif
            }
            else
            {
#if FACEPUNCH
                _currentQuery = FacepunchSteam.OrderType(Query.All, orderMode)
                                       .WhereSearchText(searchText);
#else
                _currentQuery = DGSteam.CreateQueryAll(orderMode, WorkshopType.Items);
                (_currentQuery as WorkshopQueryAll).SearchText = searchText;
#endif
            }

            foreach (string s in tags)
            {
#if FACEPUNCH
                _currentQuery = _currentQuery.WithTag(s);
#else
                _currentQuery.RequiredTags.Add(s);
#endif
            }

#if FACEPUNCH
            var page = _currentQuery.GetPageAsync(pIndex)
                .GetAwaiter()
                .GetResult();
            if (page?.ResultCount != 0)
            {
                foreach (var item in page?.Entries)
                    Fetched(page, item);
            }
            else
            {
                FinishedQuery(page);
            }
#else
            _currentQuery.JustOnePage = true;
            _currentQuery.QueryFinished += FinishedQuery;
            _currentQuery.ResultFetched += Fetched;
            _currentQuery.DataToFetch = WorkshopQueryData.AdditionalPreviews | WorkshopQueryData.PreviewURL;
            _currentQuery.Request();
#endif
        }

#if FACEPUNCH
        void Fetched(object sender, Steamworks.Ugc.Item result)
        {
            Item item = Item.Get(result.Id);
            if (item.preview == null)
            {
                string previewUrl = result.PreviewImageUrl;

                if (string.IsNullOrEmpty(previewUrl) && result.AdditionalPreviews != null)
                {
                    UgcAdditionalPreview[] additionalPreviews = result.AdditionalPreviews;
                    foreach (var p in additionalPreviews)
                    {
                        if (p.ItemPreviewType is ItemPreviewType.Image)
                        {
                            previewUrl = p.UrlOrVideoID;
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(previewUrl))
                {
                    new Task(delegate
                    {
                        using var webClient = new WebClient();
                        byte[] buffer = webClient.DownloadData(new Uri(previewUrl));
                        item._previewData = ContentPack.LoadPNGDataFromStream(new MemoryStream(buffer));
                    }).Start();
                }
            }
            item.details = result;
            items.Add(item);
        }
#else
        void Fetched(object sender, WorkshopQueryResult result)
        {
            Item item = Item.Get(result.Details.PublishedFile.Id);
            if (item.preview == null)
            {
                string previewUrl = result.PreviewURL;
                if (string.IsNullOrEmpty(previewUrl) && result.AdditionalPreviews != null)
                {
                    WorkshopQueryResultAdditionalPreview[] additionalPreviews = result.AdditionalPreviews;
                    foreach (WorkshopQueryResultAdditionalPreview p in additionalPreviews)
                    {
                        if (p.IsImage)
                        {
                            previewUrl = p.UrlOrVideoID;
                            break;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(previewUrl))
                {
                    new Task(delegate
                    {
                        using WebClient webClient = new WebClient();
                        byte[] buffer = webClient.DownloadData(new Uri(previewUrl));
                        item._previewData = ContentPack.LoadPNGDataFromStream(new MemoryStream(buffer));
                    }).Start();
                }
            }
            item.details = result.Details;
            items.Add(item);
        }
#endif

        private void FinishedQuery(object sender)
        {
        }
    }

    private List<Group> groups = new List<Group>();

    private SpriteMap _quackLoader;

    private FancyBitmapFont _font;

    private int _selectedGroup;

    private int _selectedItem;

    private Item _openedItem;

    public override void Initialize()
    {
        _quackLoader = new SpriteMap("quackLoader", 31, 31);
        _quackLoader.speed = 0.2f;
        _quackLoader.CenterOrigin();
        _quackLoader.Scale = new Vector2(0.5f, 0.5f);
        _font = new FancyBitmapFont("smallFont");
        Layer.HUD.camera.width *= 2f;
        Layer.HUD.camera.height *= 2f;
#if FACEPUNCH
        groups.Add(new Group("Subscribed", WorkshopQueryFilterOrder.RankedByVote, FacepunchSteam.SteamId, null, "Mod"));
#else
        groups.Add(new Group("Subscribed", WorkshopQueryFilterOrder.RankedByVote, DGSteam.User.Id, null, "Mod"));
#endif
        groups.Add(new Group("Hats", WorkshopQueryFilterOrder.RankedByVote, 0uL, "hat", "Mod"));
        groups.Add(new Group("Mods", WorkshopQueryFilterOrder.RankedByVote, 0uL, null, "Mod"));
        groups.Add(new Group("Maps", WorkshopQueryFilterOrder.RankedByVote, 0uL, null, "Map"));
        base.Initialize();
    }

    public override void Update()
    {
        if (Input.Pressed("UP") && _selectedGroup > 0)
        {
            SFX.Play("rainpop");
            _selectedGroup--;
        }
        if (Input.Pressed("DOWN") && _selectedGroup < groups.Count - 1)
        {
            SFX.Play("rainpop");
            _selectedGroup++;
        }
        if (Input.Pressed("LEFT") && _selectedItem > 0)
        {
            SFX.Play("rainpop");
            _selectedItem--;
        }
        if (Input.Pressed("RIGHT") && _selectedItem < 8)
        {
            SFX.Play("rainpop");
            _selectedItem++;
        }
        if (_selectedItem >= groups[_selectedGroup].items.Count)
        {
            _selectedItem = groups[_selectedGroup].items.Count - 1;
        }
        if (_selectedItem < 0)
        {
            _selectedItem = 0;
        }
        if (Input.Pressed("SELECT"))
        {
            _openedItem = groups[_selectedGroup].items[_selectedItem];
        }
        if (Input.Pressed("CANCEL"))
        {
            _openedItem = null;
        }
        base.Update();
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.HUD)
        {
            if (_openedItem != null)
            {
                _font.Scale = new Vector2(1f, 1f);
                _font.Draw(_openedItem.name, new Vector2(16f, 16f), Color.White, 0.5f);
                if (_openedItem.preview != null)
                {
                    Graphics.Draw(_openedItem.preview, 16, 32, 256F / _openedItem.preview.Height * 0.5f, 256F / _openedItem.preview.Height * 0.5f, 0.5f);
                }
                _font.maxWidth = 300;
                _font.Draw(_openedItem.description, new Vector2(16f, 170f), Color.White, 0.5f);
                _font.maxWidth = 0;
            }
            else
            {
                Vector2 groupDrawPos = new Vector2(32f, 16f);
                Vector2 itemSize = new Vector2(64f, 64f);
                int groupIndex = 0;
                foreach (Group g in groups)
                {
                    Vector2 drawPos = groupDrawPos + new Vector2(0f, 12f);
                    _font.Scale = new Vector2(1f, 1f);
                    _font.Draw(g.name, groupDrawPos, Color.White, 0.5f);
                    int itemIndex = 0;
                    foreach (Item i in g.items)
                    {
                        Vector2 extraOffset = new Vector2(0f);
                        float sizeMul = 0.25f;
                        float baseDepth = 0.1f;
                        if (groupIndex == _selectedGroup && itemIndex == _selectedItem)
                        {
                            extraOffset = new Vector2(-4f, -4f);
                            Graphics.DrawRect(drawPos + extraOffset + new Vector2(-1f, -1f), drawPos + extraOffset + itemSize + new Vector2(8f, 8f) + new Vector2(1f, 1f), Color.White, 0.5f, filled: false, 2f);
                            sizeMul = 0.28f;
                            baseDepth = 0.5f;
                        }
                        if (i.preview != null)
                        {
                            float scaleFactor = 256f / (float)i.preview.Height;
                            float xCrop = i.preview.Width / 2 - i.preview.Height / 2;
                            Graphics.Draw(i.preview, drawPos + extraOffset, new RectangleF(xCrop, 0f, i.preview.Height, i.preview.Height), Color.White, 0f, Vector2.Zero, new Vector2(scaleFactor * sizeMul, scaleFactor * sizeMul), SpriteEffects.None, baseDepth);
                        }
                        else
                        {
                            Graphics.Draw(_quackLoader, drawPos.X + itemSize.X / 2f, drawPos.Y + itemSize.Y / 2f);
                        }
                        _font.Scale = new Vector2(0.5f, 0.5f);
                        string drawName = i.name.Reduced(21);
                        _font.Draw(drawName, drawPos + extraOffset + new Vector2(2f, 2f), Color.White, baseDepth + 0.1f);
                        Graphics.DrawRect(drawPos + extraOffset + new Vector2(1f, 1f), drawPos + extraOffset + new Vector2(_font.GetWidth(drawName) + 6f, 8f), Color.Black * 0.7f, baseDepth + 0.05f);
                        drawPos.X += itemSize.X;
                        if (drawPos.X + itemSize.X > Layer.HUD.width)
                        {
                            break;
                        }
                        itemIndex++;
                    }
                    groupIndex++;
                    groupDrawPos.Y += 84f;
                }
            }
        }
        base.PostDrawLayer(layer);
    }
}