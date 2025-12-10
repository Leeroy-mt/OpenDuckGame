using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DuckGame;

internal class WorkshopBrowser : Level
{
    private class Item
    {
        public string description;

        public WorkshopQueryResultDetails details;

        private Tex2D _preview;

        public PNGData _previewData;

        private static Dictionary<ulong, Item> _items = new Dictionary<ulong, Item>();

        public string name => details.title;

        public Tex2D preview
        {
            get
            {
                if (_preview == null && _previewData != null)
                {
                    _preview = new Tex2D(_previewData.width, _previewData.height);
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

        public List<Item> items = new List<Item>();

        public List<string> tags;

        public string searchText;

        public ulong userID;

        public WorkshopQueryFilterOrder orderMode;

        private WorkshopQueryUGC _currentQuery;

        public Group(string pName, WorkshopQueryFilterOrder pOrder, ulong pUserID, string pSearchText, params string[] pTags)
        {
            name = pName;
            orderMode = pOrder;
            tags = pTags.ToList();
            searchText = pSearchText;
            userID = pUserID;
            OpenPage(0);
        }

        public void OpenPage(int pIndex)
        {
            if (userID != 0L)
            {
                _currentQuery = Steam.CreateQueryUser(userID, WorkshopList.Subscribed, WorkshopType.Items, WorkshopSortOrder.SubscriptionDateDesc);
            }
            else
            {
                _currentQuery = Steam.CreateQueryAll(orderMode, WorkshopType.Items);
                (_currentQuery as WorkshopQueryAll).searchText = searchText;
            }
            foreach (string s in tags)
            {
                _currentQuery.requiredTags.Add(s);
            }
            _currentQuery.justOnePage = true;
            _currentQuery.QueryFinished += FinishedQuery;
            _currentQuery.ResultFetched += Fetched;
            _currentQuery._dataToFetch = WorkshopQueryData.AdditionalPreviews | WorkshopQueryData.PreviewURL;
            _currentQuery.Request();
        }

        private void Fetched(object sender, WorkshopQueryResult result)
        {
            Item item = Item.Get(result.details.publishedFile.id);
            if (item.preview == null)
            {
                string previewUrl = result.previewURL;
                if (string.IsNullOrEmpty(previewUrl) && result.additionalPreviews != null)
                {
                    WorkshopQueryResultAdditionalPreview[] additionalPreviews = result.additionalPreviews;
                    foreach (WorkshopQueryResultAdditionalPreview p in additionalPreviews)
                    {
                        if (p.isImage)
                        {
                            previewUrl = p.urlOrVideoID;
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
            item.details = result.details;
            items.Add(item);
        }

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
        _quackLoader.scale = new Vec2(0.5f, 0.5f);
        _font = new FancyBitmapFont("smallFont");
        Layer.HUD.camera.width *= 2f;
        Layer.HUD.camera.height *= 2f;
        groups.Add(new Group("Subscribed", WorkshopQueryFilterOrder.RankedByVote, Steam.user.id, null, "Mod"));
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
                _font.scale = new Vec2(1f, 1f);
                _font.Draw(_openedItem.name, new Vec2(16f, 16f), Color.White, 0.5f);
                if (_openedItem.preview != null)
                {
                    Graphics.Draw(_openedItem.preview, 16f, 32f, 256f / (float)_openedItem.preview.height * 0.5f, 256f / (float)_openedItem.preview.height * 0.5f, 0.5f);
                }
                _font.maxWidth = 300;
                _font.Draw(_openedItem.description, new Vec2(16f, 170f), Color.White, 0.5f);
                _font.maxWidth = 0;
            }
            else
            {
                Vec2 groupDrawPos = new Vec2(32f, 16f);
                Vec2 itemSize = new Vec2(64f, 64f);
                int groupIndex = 0;
                foreach (Group g in groups)
                {
                    Vec2 drawPos = groupDrawPos + new Vec2(0f, 12f);
                    _font.scale = new Vec2(1f, 1f);
                    _font.Draw(g.name, groupDrawPos, Color.White, 0.5f);
                    int itemIndex = 0;
                    foreach (Item i in g.items)
                    {
                        Vec2 extraOffset = new Vec2(0f);
                        float sizeMul = 0.25f;
                        float baseDepth = 0.1f;
                        if (groupIndex == _selectedGroup && itemIndex == _selectedItem)
                        {
                            extraOffset = new Vec2(-4f, -4f);
                            Graphics.DrawRect(drawPos + extraOffset + new Vec2(-1f, -1f), drawPos + extraOffset + itemSize + new Vec2(8f, 8f) + new Vec2(1f, 1f), Color.White, 0.5f, filled: false, 2f);
                            sizeMul = 0.28f;
                            baseDepth = 0.5f;
                        }
                        if (i.preview != null)
                        {
                            float scaleFactor = 256f / (float)i.preview.height;
                            float xCrop = i.preview.width / 2 - i.preview.height / 2;
                            Graphics.Draw(i.preview, drawPos + extraOffset, new Rectangle(xCrop, 0f, i.preview.height, i.preview.height), Color.White, 0f, Vec2.Zero, new Vec2(scaleFactor * sizeMul, scaleFactor * sizeMul), SpriteEffects.None, baseDepth);
                        }
                        else
                        {
                            Graphics.Draw(_quackLoader, drawPos.x + itemSize.x / 2f, drawPos.y + itemSize.y / 2f);
                        }
                        _font.scale = new Vec2(0.5f, 0.5f);
                        string drawName = i.name.Reduced(21);
                        _font.Draw(drawName, drawPos + extraOffset + new Vec2(2f, 2f), Color.White, baseDepth + 0.1f);
                        Graphics.DrawRect(drawPos + extraOffset + new Vec2(1f, 1f), drawPos + extraOffset + new Vec2(_font.GetWidth(drawName) + 6f, 8f), Color.Black * 0.7f, baseDepth + 0.05f);
                        drawPos.x += itemSize.x;
                        if (drawPos.x + itemSize.x > Layer.HUD.width)
                        {
                            break;
                        }
                        itemIndex++;
                    }
                    groupIndex++;
                    groupDrawPos.y += 84f;
                }
            }
        }
        base.PostDrawLayer(layer);
    }
}
