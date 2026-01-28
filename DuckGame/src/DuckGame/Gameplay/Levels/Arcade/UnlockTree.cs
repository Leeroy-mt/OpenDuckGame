using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class UnlockTree : Thing
{
    private SpriteMap _box;

    private Sprite _lock;

    private SpriteMap _icons;

    private int _topLayer;

    private int _desiredLayer;

    private float _layerScroll;

    private UnlockData _selected;

    private UnlockScreen _screen;

    public UnlockData selected
    {
        get
        {
            return _selected;
        }
        set
        {
            _selected = value;
        }
    }

    public UnlockTree(UnlockScreen screen, Layer putLayer)
    {
        _box = new SpriteMap("arcade/unlockBox", 39, 40);
        _box.CenterOrigin();
        _icons = new SpriteMap("arcade/unlockIcons", 25, 25);
        _icons.CenterOrigin();
        _lock = new Sprite("arcade/unlockLock");
        _lock.CenterOrigin();
        _screen = screen;
        base.layer = putLayer;
    }

    public override void Initialize()
    {
        _selected = Unlocks.unlocks.FirstOrDefault();
        _screen.ChangeSpeech();
    }

    public override void Update()
    {
        if (base.Alpha < 0.01f)
        {
            return;
        }
        Duck dd = Level.First<Duck>();
        InputProfile p = InputProfile.DefaultPlayer1;
        if (dd != null)
        {
            p = dd.inputProfile;
        }
        UnlockData prevSelected = _selected;
        List<UnlockData> dat = Unlocks.GetTreeLayer(_selected.layer);
        if (p.Pressed("MENULEFT"))
        {
            UnlockData prev = null;
            foreach (UnlockData data in dat)
            {
                if (data == _selected)
                {
                    break;
                }
                prev = data;
            }
            if (prev != null)
            {
                _selected = prev;
                _screen.ChangeSpeech();
                SFX.Play("menuBlip01");
            }
        }
        else if (p.Pressed("MENURIGHT"))
        {
            UnlockData prev2 = null;
            UnlockData next = null;
            foreach (UnlockData data2 in dat)
            {
                if (prev2 == _selected)
                {
                    next = data2;
                    break;
                }
                prev2 = data2;
            }
            if (next != null)
            {
                _selected = next;
                _screen.ChangeSpeech();
                SFX.Play("menuBlip01");
            }
        }
        else if (p.Pressed("MENUUP"))
        {
            if (_selected.parent != null)
            {
                UnlockData upSelect = _selected.parent;
                if (upSelect != _selected)
                {
                    SFX.Play("menuBlip01");
                    _selected = upSelect;
                    _screen.ChangeSpeech();
                }
            }
        }
        else if (p.Pressed("MENUDOWN"))
        {
            bool found = false;
            if (_selected.children.Count > 0)
            {
                _selected = _selected.children[0];
                found = true;
            }
            else if (_selected.parent != null)
            {
                foreach (UnlockData d in _selected.parent.children)
                {
                    if (d != _selected && d.children.Count > 0)
                    {
                        _selected = d.children[0];
                        found = true;
                        break;
                    }
                }
            }
            if (found)
            {
                _screen.ChangeSpeech();
                SFX.Play("menuBlip01");
            }
        }
        else if (p.Pressed("SELECT") && !_selected.ProfileUnlocked(Profiles.active[0]))
        {
            bool locked = false;
            if (_selected.parent != null)
            {
                foreach (UnlockData parent in Unlocks.GetTreeLayer(_selected.parent.layer))
                {
                    if (parent.children.Contains(_selected) && !parent.ProfileUnlocked(Profiles.active[0]))
                    {
                        locked = true;
                        break;
                    }
                }
            }
            if (Profiles.active[0].ticketCount >= _selected.cost && (_selected.parent == null || !locked))
            {
                _screen.OpenBuyConfirmation(_selected);
            }
            else
            {
                SFX.Play("consoleError");
            }
        }
        if (_selected != prevSelected)
        {
            _desiredLayer = _selected.layer;
            _screen.SelectionChanged();
        }
        if (_desiredLayer == _topLayer)
        {
            return;
        }
        if (_desiredLayer < _topLayer)
        {
            _layerScroll -= 0.1f;
            if (_layerScroll <= -1f)
            {
                _layerScroll = 0f;
                _topLayer--;
            }
        }
        else if (_desiredLayer > _topLayer + 1)
        {
            _layerScroll += 0.1f;
            if (_layerScroll >= 1f)
            {
                _layerScroll = 0f;
                _topLayer++;
            }
        }
    }

    public override void Draw()
    {
        if (base.Alpha < 0.01f)
        {
            return;
        }
        int checkLayer = _topLayer;
        if (_desiredLayer < _topLayer)
        {
            checkLayer = _desiredLayer;
        }
        int drawDeep = 3;
        if (_desiredLayer > 1 || _topLayer > 0)
        {
            drawDeep = 4;
        }
        checkLayer = _topLayer - 1;
        if (checkLayer < 0)
        {
            checkLayer = 0;
        }
        List<UnlockData> unlocks = Unlocks.GetTreeLayer(checkLayer);
        float extraOff = 0f;
        if (checkLayer < _topLayer)
        {
            extraOff += (float)(_topLayer - checkLayer) * 60f;
        }
        Vec2 treePos = new Vec2(50f, 45f - _layerScroll * 67f - extraOff);
        Vec2 treeSize = new Vec2(Layer.HUD.width - 180f, 100f);
        List<UnlockData> nextLayer = new List<UnlockData>();
        int unlockLayer = 0;
        for (int i = 0; i < unlocks.Count; i++)
        {
            if (unlockLayer >= drawDeep)
            {
                break;
            }
            UnlockData unlock = unlocks[i];
            float fade = 1f;
            if (unlock != _selected)
            {
                fade = 0.5f;
            }
            Color lineColor = Color.Green;
            if (!unlock.ProfileUnlocked(Profiles.active[0]))
            {
                lineColor = Color.DarkRed;
            }
            bool parentUnlocked = true;
            if (!unlock.AllParentsUnlocked(Profiles.active[0]))
            {
                parentUnlocked = false;
                lineColor = new Color(40, 40, 40);
                fade = ((unlock == _selected) ? 0.8f : 0.2f);
            }
            lineColor = new Color((byte)((float)(int)lineColor.r * fade), (byte)((float)(int)lineColor.g * fade), (byte)((float)(int)lineColor.b * fade));
            float centerAdd = 0f;
            centerAdd = ((unlocks.Count == 1) ? (treeSize.X / 2f) : ((unlocks.Count != 2) ? ((float)i * (treeSize.X / (float)(unlocks.Count - 1))) : (treeSize.X / 2f - treeSize.X / 4f + (float)i * (treeSize.X / 2f))));
            Vec2 boxPos = new Vec2(treePos.X + centerAdd, treePos.Y + (float)(unlockLayer * 60));
            _box.Depth = 0.1f;
            _box.frame = 2;
            _box.Alpha = base.Alpha;
            _box.color = lineColor;
            Graphics.Draw(_box, boxPos.X, boxPos.Y);
            _box.Depth = 0.2f;
            _box.frame = 1;
            _box.color = new Color(fade, fade, fade);
            Graphics.Draw(_box, boxPos.X, boxPos.Y);
            if (unlock.icon != -1)
            {
                _icons.Depth = 0.2f;
                _icons.frame = (parentUnlocked ? unlock.icon : 25);
                _icons.color = new Color(fade, fade, fade);
                _icons.Alpha = base.Alpha;
                Graphics.Draw(_icons, boxPos.X - 1f, boxPos.Y - 1f);
            }
            if (unlock == _selected)
            {
                _box.frame = 0;
                Graphics.Draw(_box, boxPos.X, boxPos.Y);
            }
            foreach (UnlockData dat in unlock.children)
            {
                if (!nextLayer.Contains(dat))
                {
                    nextLayer.Add(dat);
                }
            }
            if (i != unlocks.Count - 1 || nextLayer.Count <= 0)
            {
                continue;
            }
            for (int j = 0; j < unlocks.Count; j++)
            {
                UnlockData parent = unlocks[j];
                if (parent.children.Count <= 0)
                {
                    continue;
                }
                fade = 1f;
                if (parent != _selected)
                {
                    fade = 0.5f;
                }
                lineColor = Color.Green;
                if (!parent.ProfileUnlocked(Profiles.active[0]))
                {
                    lineColor = Color.DarkRed;
                }
                if (!parent.AllParentsUnlocked(Profiles.active[0]))
                {
                    lineColor = new Color(90, 90, 90);
                }
                lineColor = new Color((byte)((float)(int)lineColor.r * fade), (byte)((float)(int)lineColor.g * fade), (byte)((float)(int)lineColor.b * fade));
                centerAdd = ((unlocks.Count == 1) ? (treeSize.X / 2f) : ((unlocks.Count != 2) ? ((float)j * (treeSize.X / (float)(unlocks.Count - 1))) : (treeSize.X / 2f - treeSize.X / 4f + (float)j * (treeSize.X / 2f))));
                boxPos = new Vec2(treePos.X + centerAdd, treePos.Y + (float)(unlockLayer * 60));
                Graphics.DrawLine(boxPos, boxPos + new Vec2(0f, 30f), lineColor * base.Alpha, 6f, -0.2f);
                Color afterUnlockColor = new Color(50, 50, 50);
                if (!parent.ProfileUnlocked(Profiles.active[0]))
                {
                    _lock.Depth = 0.5f;
                    _lock.Alpha = base.Alpha;
                    Graphics.Draw(_lock, boxPos.X, boxPos.Y + 30f);
                }
                else
                {
                    afterUnlockColor = Color.Green;
                }
                afterUnlockColor = new Color((byte)((float)(int)afterUnlockColor.r * fade), (byte)((float)(int)afterUnlockColor.g * fade), (byte)((float)(int)afterUnlockColor.b * fade));
                for (int iChild = 0; iChild < nextLayer.Count; iChild++)
                {
                    UnlockData child = nextLayer[iChild];
                    if (parent.children.Contains(child))
                    {
                        centerAdd = ((nextLayer.Count == 1) ? (treeSize.X / 2f) : ((nextLayer.Count != 2) ? ((float)iChild * (treeSize.X / (float)(nextLayer.Count - 1))) : (treeSize.X / 2f - treeSize.X / 4f + (float)iChild * (treeSize.X / 2f))));
                        Vec2 childBoxPos = new Vec2(treePos.X + centerAdd, treePos.Y + (float)((unlockLayer + 1) * 60));
                        float xOff = 0f;
                        if (childBoxPos.X < boxPos.X)
                        {
                            xOff = -3f;
                        }
                        else if (childBoxPos.X > boxPos.X)
                        {
                            xOff = 3f;
                        }
                        float xOff2 = 0f;
                        if (childBoxPos.X < boxPos.X)
                        {
                            xOff2 = 3f;
                        }
                        else if (childBoxPos.X > boxPos.X)
                        {
                            xOff2 = -3f;
                        }
                        Graphics.DrawLine(new Vec2(childBoxPos.X + xOff, boxPos.Y + 30f), new Vec2(boxPos.X + xOff2, boxPos.Y + 30f), afterUnlockColor * base.Alpha, 6f, -0.2f + ((_selected == parent) ? 0.1f : 0f));
                        Graphics.DrawLine(new Vec2(childBoxPos.X, boxPos.Y + 30f), new Vec2(childBoxPos.X, childBoxPos.Y), afterUnlockColor * base.Alpha, 6f, -0.2f + ((_selected == parent) ? 0.1f : 0f));
                    }
                }
            }
            unlocks.Clear();
            unlocks.AddRange(nextLayer);
            nextLayer.Clear();
            unlockLayer++;
            i = -1;
        }
    }
}
