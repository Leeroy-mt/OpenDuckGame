using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class SteamUploadDialog : ContextMenu
{
    private FancyBitmapFont _font;

    private Textbox _descriptionBox;

    private Textbox _nameBox;

    private MessageDialogue _confirm;

    private NotifyDialogue _notify;

    private UploadDialogue _upload;

    private DeathmatchTestDialogue _deathmatchTest;

    private TestSuccessDialogue _testSuccess;

    private ArcadeTestDialogue _arcadeTest;

    private Sprite _previewTarget;

    private SpriteMap _workshopTag;

    private Sprite _workshopTagMiddle;

    private Sprite _tagPlus;

    private static bool _editingMod;

    private LevelData _levelData;

    private Mod _mod;

    private int _arcadeTestIndex = -1;

    private static List<string> _possibleTagsLevel = new List<string> { "Dumb", "Fast", "Luck", "Weird", "Fire", "Pro", "Cute" };

    private static List<string> _possibleTagsMod = new List<string> { "Weapons", "Hats", "Items", "Equipment", "Total Conversion" };

    private Vec2 _acceptPos;

    private Vec2 _acceptSize;

    private bool _acceptHover;

    private Vec2 _cancelPos;

    private Vec2 _cancelSize;

    private bool _cancelHover;

    private EditorWorkshopItem _publishItem;

    private WorkshopItem _currentUploadItem;

    private bool _testing;

    private Vec2 _plusPosition;

    private ContextMenu _tagMenu;

    private bool _creatingSubItem;

    private int _subItemUploadIndex = -1;

    private int _subItemTries;

    private Dictionary<string, Vec2> tagPositions = new Dictionary<string, Vec2>();

    private Stack<EditorWorkshopItem> _publishStack = new Stack<EditorWorkshopItem>();

    private float hOffset;

    private float _fdHeight = 262f;

    public bool drag;

    public static List<string> possibleTags
    {
        get
        {
            if (_editingMod)
            {
                return _possibleTagsMod;
            }
            return _possibleTagsLevel;
        }
    }

    public SteamUploadDialog()
        : base(null)
    {
    }

    public override void Initialize()
    {
        base.layer = Layer.HUD;
        base.depth = 0.9f;
        _showBackground = false;
        itemSize = new Vec2(386f, 16f);
        _root = true;
        drawControls = false;
        _descriptionBox = new Textbox(base.x + 5f, base.y + 225f, 316f, 40f, 0.5f, 9, "<ENTER DESCRIPTION>");
        _nameBox = new Textbox(base.x + 5f, base.y + 255f, 316f, 12f, 1f, 1, "<ENTER NAME>");
        _font = new FancyBitmapFont("smallFont");
        _confirm = new MessageDialogue();
        Level.Add(_confirm);
        _upload = new UploadDialogue();
        Level.Add(_upload);
        _notify = new NotifyDialogue();
        Level.Add(_notify);
        _deathmatchTest = new DeathmatchTestDialogue();
        Level.Add(_deathmatchTest);
        _testSuccess = new TestSuccessDialogue();
        Level.Add(_testSuccess);
        _arcadeTest = new ArcadeTestDialogue();
        Level.Add(_arcadeTest);
    }

    public void Open(LevelData pData)
    {
        _editingMod = false;
        _publishItem = null;
        Editor.lockInput = this;
        SFX.Play("openClick", 0.4f);
        base.opened = true;
        _publishItem = new EditorWorkshopItem(pData);
        _previewTarget = new Sprite(_publishItem.preview);
        _nameBox.text = pData.workshopData.name;
        _descriptionBox.text = pData.workshopData.description;
        _workshopTag = new SpriteMap("workshopTag", 4, 8);
        _workshopTagMiddle = new Sprite("workshopTagMiddle");
        _tagPlus = new Sprite("tagPlus");
        _levelData = pData;
        _arcadeTestIndex = 0;
    }

    public void Open(Mod pData)
    {
        _editingMod = true;
        _publishItem = null;
        Editor.lockInput = this;
        SFX.Play("openClick", 0.4f);
        base.opened = true;
        _publishItem = new EditorWorkshopItem(pData);
        _previewTarget = new Sprite(_publishItem.preview);
        _nameBox.text = pData.workshopData.name;
        _descriptionBox.text = pData.workshopData.description;
        _workshopTag = new SpriteMap("workshopTag", 4, 8);
        _workshopTagMiddle = new Sprite("workshopTagMiddle");
        _tagPlus = new Sprite("tagPlus");
        _mod = pData;
        _arcadeTestIndex = 0;
    }

    public void Close()
    {
        Editor.lockInput = null;
        base.opened = false;
        _descriptionBox.LoseFocus();
        _nameBox.LoseFocus();
        _publishItem = null;
        ClearItems();
    }

    public override void Selected(ContextMenu item)
    {
        SFX.Play("highClick", 0.3f, 0.2f);
        if (item != null && item.text != "")
        {
            _publishItem.AddTag(item.text);
        }
        if (_tagMenu != null)
        {
            _tagMenu.opened = false;
            Level.Remove(_tagMenu);
            _tagMenu = null;
            if (Editor.PeekFocus() == _tagMenu)
            {
                Editor.PopFocus();
            }
        }
    }

    public override void Toggle(ContextMenu item)
    {
    }

    private void Publish()
    {
        _publishItem.name = _nameBox.text;
        _publishItem.description = _descriptionBox.text;
        if (_publishItem.PrepareItem() != SteamResult.OK)
        {
            _notify.Open("Failed with code " + (int)_publishItem.result + " (" + _publishItem.result.ToString() + ")");
            return;
        }
        _publishStack.Clear();
        _publishStack.Push(_publishItem);
        foreach (EditorWorkshopItem i in _publishItem.subItems)
        {
            _publishStack.Push(i);
        }
        UploadNext();
    }

    public bool UploadNext()
    {
        if (_publishStack.Count == 0)
        {
            return false;
        }
        EditorWorkshopItem next = _publishStack.Peek();
        next.Upload();
        if (next.subIndex == -1)
        {
            _upload.Open("Uploading...", next.item);
        }
        else
        {
            _upload.Open("Uploading Sub Item(" + next.subIndex + ")...", next.item);
        }
        return true;
    }

    public override void Update()
    {
        if (_publishStack.Count > 0)
        {
            if (!_publishStack.Peek().finishedProcessing)
            {
                return;
            }
            _publishStack.Peek().FinishUpload();
            _upload.Close();
            if (_publishStack.Peek().result == SteamResult.OK)
            {
                EditorWorkshopItem cur = _publishStack.Peek();
                _publishStack.Pop();
                if (!UploadNext())
                {
                    _upload.Close();
                    _notify.Open("Item published!");
                    Steam.ShowWorkshopLegalAgreement(cur.item.id.ToString());
                }
            }
            else
            {
                _notify.Open("Failed with code " + (int)_publishStack.Peek().result + " (" + _publishStack.Peek().result.ToString() + ")");
                _publishStack.Clear();
            }
            return;
        }
        if (!base.opened || _opening || _confirm.opened || _upload.opened || _deathmatchTest.opened || _arcadeTest.opened || _testSuccess.opened)
        {
            if (base.opened)
            {
                Keyboard.keyString = "";
            }
            if (base.opened)
            {
                Editor.lockInput = this;
            }
            _opening = false;
            {
                foreach (ContextMenu item in _items)
                {
                    item.disabled = true;
                }
                return;
            }
        }
        if (_confirm.result)
        {
            if (_publishItem.levelType == LevelType.Arcade_Machine)
            {
                _arcadeTest.Open("This machine can automatically show up in generated arcades, if you pass this validity test. You need to get the Platinum trophy on all 3 challenges (oh boy)!");
            }
            else
            {
                _deathmatchTest.Open("In order to upload this map as a deathmatch level, all ducks need to be able to be eliminated. Do you want to launch the map and show that the map is functional? You don't have to do this, but the map won't show up with the DEATHMATCH tag without completing this test. If this is a challenge map, then don't worry about it!");
            }
            _confirm.result = false;
        }
        else if (_testing)
        {
            Keyboard.keyString = "";
            if (DeathmatchTestDialogue.success)
            {
                _testSuccess.Open("Test success! The level can now be published as a deathmatch level!");
                _publishItem.deathmatchTestSuccess = true;
            }
            else if (ArcadeTestDialogue.success)
            {
                if (_arcadeTestIndex > 0 && _arcadeTestIndex < 3)
                {
                    _publishItem.subItems.ElementAt(_arcadeTestIndex).challengeTestSuccess = true;
                }
                do
                {
                    _arcadeTestIndex++;
                }
                while (_arcadeTestIndex <= 2 && _publishItem.subItems.ElementAt(_arcadeTestIndex).challengeTestSuccess);
                _arcadeTestIndex = 3;
                if (_arcadeTestIndex != 3)
                {
                    ArcadeTestDialogue.success = false;
                    ArcadeTestDialogue.currentEditor = Level.current as Editor;
                    if (_arcadeTestIndex == 0)
                    {
                        Level.current = new ChallengeLevel((ArcadeTestDialogue.currentEditor.levelThings[0] as ArcadeMachine).challenge01Data, validityTest: true);
                    }
                    else if (_arcadeTestIndex == 1)
                    {
                        Level.current = new ChallengeLevel((ArcadeTestDialogue.currentEditor.levelThings[0] as ArcadeMachine).challenge02Data, validityTest: true);
                    }
                    else if (_arcadeTestIndex == 2)
                    {
                        Level.current = new ChallengeLevel((ArcadeTestDialogue.currentEditor.levelThings[0] as ArcadeMachine).challenge03Data, validityTest: true);
                    }
                    _testing = true;
                    return;
                }
                _testSuccess.Open("Test success! The arcade machine can now be published to the workshop!");
            }
            else if (DeathmatchTestDialogue.tooSlow)
            {
                _notify.Open("Framerate too low!");
            }
            else
            {
                _notify.Open("Testing failed.");
                _arcadeTestIndex = -1;
            }
            DeathmatchTestDialogue.success = false;
            ArcadeTestDialogue.success = false;
            _testing = false;
        }
        else if (_testSuccess.result)
        {
            Publish();
            _testSuccess.result = false;
        }
        else if (_deathmatchTest.result != -1)
        {
            if (_deathmatchTest.result == 1)
            {
                Publish();
            }
            else if (_deathmatchTest.result == 0)
            {
                DeathmatchTestDialogue.success = false;
                DeathmatchTestDialogue.currentEditor = Level.current as Editor;
                int num = 4;
                if (Level.current is Editor && (Level.current as Editor).things[typeof(EightPlayer)].Count() > 0)
                {
                    num = 8;
                }
                for (int i = 0; i < num; i++)
                {
                    Profiles.defaultProfiles[i].team = Teams.allStock[i];
                    Profiles.defaultProfiles[i].persona = Profiles.defaultProfiles[i].defaultPersona;
                    Profiles.defaultProfiles[i].UpdatePersona();
                    Input.ApplyDefaultMapping(Profiles.defaultProfiles[i].inputProfile, Profiles.defaultProfiles[i]);
                }
                Level.current = new GameLevel(_levelData.GetPath(), 0, validityTest: true);
                _testing = true;
            }
            _deathmatchTest.result = -1;
        }
        else if (_arcadeTest.result != -1)
        {
            if (_arcadeTest.result == 1)
            {
                Publish();
            }
            else if (_arcadeTest.result == 0)
            {
                ArcadeTestDialogue.success = true;
                _testing = true;
                _arcadeTest.result = -1;
                return;
            }
            _arcadeTest.result = -1;
        }
        else
        {
            if (_tagMenu != null)
            {
                return;
            }
            _ = new Vec2(base.layer.width / 2f - base.width / 2f + hOffset, base.layer.height / 2f - base.height / 2f - 15f) + new Vec2(7f, 276f);
            foreach (KeyValuePair<string, Vec2> xpos in tagPositions)
            {
                if (Mouse.x > xpos.Value.x && Mouse.x < xpos.Value.x + 8f && Mouse.y > xpos.Value.y && Mouse.y < xpos.Value.y + 8f && Mouse.left == InputState.Pressed)
                {
                    _publishItem.RemoveTag(xpos.Key);
                    return;
                }
            }
            if (tagPositions.Count != possibleTags.Count)
            {
                bool hoverPlus = false;
                if (Mouse.x > _plusPosition.x && Mouse.x < _plusPosition.x + 8f && Mouse.y > _plusPosition.y && Mouse.y < _plusPosition.y + 8f)
                {
                    hoverPlus = true;
                }
                if (hoverPlus && Mouse.left == InputState.Pressed)
                {
                    ContextMenu tagMenu = new ContextMenu(this);
                    tagMenu.x = _plusPosition.x;
                    tagMenu.y = _plusPosition.y;
                    tagMenu.root = true;
                    tagMenu.depth = base.depth + 20;
                    int hi = 0;
                    foreach (string s in possibleTags)
                    {
                        if (!_publishItem.tags.Contains(s))
                        {
                            ContextMenu confirmItem = new ContextMenu(this);
                            confirmItem.itemSize.x = 40f;
                            confirmItem.text = s;
                            tagMenu.AddItem(confirmItem);
                            hi++;
                        }
                    }
                    tagMenu.y -= hi * 16 + 10;
                    Level.Add(tagMenu);
                    tagMenu.opened = true;
                    tagMenu.closeOnRight = true;
                    _tagMenu = tagMenu;
                    Editor.PopFocus();
                    return;
                }
            }
            Editor.lockInput = this;
            _descriptionBox.Update();
            _nameBox.Update();
            _acceptHover = false;
            _cancelHover = false;
            if (Mouse.x > _acceptPos.x && Mouse.x < _acceptPos.x + _acceptSize.x && Mouse.y > _acceptPos.y && Mouse.y < _acceptPos.y + _acceptSize.y)
            {
                _acceptHover = true;
            }
            if (Mouse.x > _cancelPos.x && Mouse.x < _cancelPos.x + _cancelSize.x && Mouse.y > _cancelPos.y && Mouse.y < _cancelPos.y + _cancelSize.y)
            {
                _cancelHover = true;
            }
            if (_acceptHover && Mouse.left == InputState.Pressed)
            {
                if (_nameBox.text == "")
                {
                    _notify.Open("Please enter a name :(");
                }
                else
                {
                    _confirm.Open("Upload to workshop?");
                }
            }
            if (_cancelHover && Mouse.left == InputState.Pressed)
            {
                Close();
            }
            base.Update();
        }
    }

    public override void Draw()
    {
        menuSize.y = _fdHeight;
        if (!base.opened)
        {
            return;
        }
        base.Draw();
        float width = 328f;
        float height = _fdHeight + 22f;
        Vec2 topLeft = new Vec2(base.layer.width / 2f - width / 2f + hOffset, base.layer.height / 2f - height / 2f - 15f);
        Vec2 bottomRight = new Vec2(base.layer.width / 2f + width / 2f + hOffset, base.layer.height / 2f + height / 2f - 12f);
        Graphics.DrawRect(topLeft, bottomRight, new Color(70, 70, 70), base.depth, filled: false);
        Graphics.DrawRect(topLeft, bottomRight, new Color(30, 30, 30), base.depth - 8);
        Graphics.DrawRect(topLeft + new Vec2(4f, 23f), bottomRight + new Vec2(-4f, -160f), new Color(10, 10, 10), base.depth - 4);
        Graphics.DrawRect(topLeft + new Vec2(4f, 206f), bottomRight + new Vec2(-4f, -66f), new Color(10, 10, 10), base.depth - 4);
        Graphics.DrawRect(topLeft + new Vec2(4f, 224f), bottomRight + new Vec2(-4f, -14f), new Color(10, 10, 10), base.depth - 4);
        Graphics.DrawRect(topLeft + new Vec2(3f, 3f), new Vec2(bottomRight.x - 3f, topLeft.y + 19f), new Color(70, 70, 70), base.depth - 4);
        if (_mod != null)
        {
            Graphics.DrawString("Upload Mod to Workshop", topLeft + new Vec2(5f, 7f), Color.White, base.depth + 8);
        }
        else if (Editor.arcadeMachineMode)
        {
            Graphics.DrawString("Upload " + _publishItem.levelType.ToString() + " to Workshop", topLeft + new Vec2(5f, 7f), Color.White, base.depth + 8);
        }
        else
        {
            Graphics.DrawString("Upload " + _publishItem.levelSize.ToString() + " " + _publishItem.levelType.ToString() + " to Workshop", topLeft + new Vec2(5f, 7f), Color.White, base.depth + 8);
        }
        _descriptionBox.position = topLeft + new Vec2(6f, 226f);
        _descriptionBox.depth = base.depth + 2;
        _descriptionBox.Draw();
        _nameBox.position = topLeft + new Vec2(6f, 208f);
        _nameBox.depth = base.depth + 2;
        _nameBox.Draw();
        int numTag = 0;
        Vec2 tagPos = topLeft + new Vec2(7f, 276f);
        int numX = 0;
        tagPositions.Clear();
        foreach (string tag in _publishItem.tags)
        {
            bool num = possibleTags.Contains(tag);
            _workshopTag.depth = base.depth + 8;
            _workshopTag.frame = 0;
            Graphics.Draw(_workshopTag, tagPos.x, tagPos.y);
            float stringWidth = Graphics.GetStringWidth(tag, thinButtons: false, 0.5f);
            float xSize = 4f;
            if (!num)
            {
                xSize = 0f;
            }
            else
            {
                numX++;
            }
            Graphics.DrawTexturedLine(_workshopTagMiddle.texture, tagPos + new Vec2(4f, 4f), tagPos + new Vec2(4f + stringWidth + xSize, 4f), Color.White, 1f, base.depth + 10);
            Graphics.DrawString(tag, tagPos + new Vec2(4f, 2f), Color.Black, base.depth + 14, null, 0.5f);
            if (num)
            {
                Vec2 xPos = tagPos + new Vec2(stringWidth + 6f, 2f);
                tagPositions[tag] = xPos;
                Graphics.DrawString("x", xPos, Color.Red, base.depth + 14, null, 0.5f);
            }
            _workshopTag.frame = 1;
            Graphics.Draw(_workshopTag, tagPos.x + xSize + 4f + stringWidth, tagPos.y);
            tagPos.x += stringWidth + 11f + xSize;
            numTag++;
        }
        if (numX < possibleTags.Count)
        {
            _tagPlus.depth = base.depth + 8;
            tagPos.x += 2f;
            Graphics.Draw(_tagPlus, tagPos.x, tagPos.y);
            _plusPosition = tagPos;
        }
        _acceptPos = bottomRight + new Vec2(-78f, -12f);
        _acceptSize = new Vec2(34f, 8f);
        Graphics.DrawRect(_acceptPos, _acceptPos + _acceptSize, _acceptHover ? new Color(180, 180, 180) : new Color(110, 110, 110), base.depth - 4);
        Graphics.DrawString("PUBLISH!", _acceptPos + new Vec2(2f, 2f), Color.White, base.depth + 8, null, 0.5f);
        _cancelPos = bottomRight + new Vec2(-36f, -12f);
        _cancelSize = new Vec2(32f, 8f);
        Graphics.DrawRect(_cancelPos, _cancelPos + _cancelSize, _cancelHover ? new Color(180, 180, 180) : new Color(110, 110, 110), base.depth - 4);
        Graphics.DrawString("CANCEL!", _cancelPos + new Vec2(2f, 2f), Color.White, base.depth + 8, null, 0.5f);
        if (_previewTarget.width < 300)
        {
            _previewTarget.depth = base.depth + 10;
            _previewTarget.scale = new Vec2(0.5f, 0.5f);
            Graphics.Draw(_previewTarget, topLeft.x + (bottomRight.x - topLeft.x) / 2f - (float)_previewTarget.width * _previewTarget.scale.x / 2f, topLeft.y + (bottomRight.y - topLeft.y) / 2f - (float)_previewTarget.height * _previewTarget.scale.y / 2f - 20f);
        }
        else
        {
            _previewTarget.depth = base.depth + 10;
            _previewTarget.scale = new Vec2(0.25f, 0.25f);
            Graphics.Draw(_previewTarget, topLeft.x + 4f, topLeft.y + 23f);
        }
    }
}
