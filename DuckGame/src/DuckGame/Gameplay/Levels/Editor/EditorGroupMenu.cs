using System;
using System.Collections;
using System.Collections.Generic;

namespace DuckGame;

public class EditorGroupMenu : ContextMenu
{
    protected bool willOnlineGrayout = true;

    private static int deep;

    public EditorGroupMenu(IContextListener owner, bool root = false, SpriteMap image = null)
        : base(owner, image)
    {
        itemSize.X = 100f;
        itemSize.Y = 16f;
        _root = root;
        if (!_root)
        {
            greyOut = Main.isDemo || Editor._currentLevelData.metaData.onlineMode;
        }
        _maxNumToDraw = 20;
    }

    public override void Update()
    {
        if (Editor.bigInterfaceMode)
        {
            _maxNumToDraw = 13;
        }
        else
        {
            _maxNumToDraw = 20;
        }
        base.Update();
    }

    public void UpdateGrayout()
    {
        greyOut = false;
        if (Editor._currentLevelData.metaData.onlineMode && willOnlineGrayout)
        {
            greyOut = true;
        }
        foreach (ContextMenu i in _items)
        {
            if (i is EditorGroupMenu)
            {
                (i as EditorGroupMenu).UpdateGrayout();
            }
            else if (i.contextThing != null)
            {
                i.greyOut = false;
                IReadOnlyPropertyBag contextBag = ContentProperties.GetBag(i.contextThing.GetType());
                if (Editor._currentLevelData.metaData.onlineMode && !contextBag.GetOrDefault("isOnlineCapable", defaultValue: true))
                {
                    i.greyOut = true;
                }
            }
        }
    }

    public void InitializeGroups(EditorGroup group, FieldBinding radioBinding = null, EditorGroup scriptingGroup = null, bool setPinnable = false)
    {
        deep++;
        _text = group.Name;
        itemSize.X = Graphics.GetFancyStringWidth(_text) + 16f;
        foreach (EditorGroup g in group.SubGroups)
        {
            EditorGroupMenu menu = new EditorGroupMenu(this);
            menu.fancy = fancy;
            menu.InitializeGroups(g, radioBinding, null, setPinnable);
            if (!menu.greyOut)
            {
                greyOut = false;
            }
            if (!menu.willOnlineGrayout)
            {
                willOnlineGrayout = false;
            }
            menu.isPinnable = setPinnable;
            AddItem(menu);
        }
        if (scriptingGroup != null)
        {
            EditorGroupMenu menu2 = new EditorGroupMenu(this);
            menu2.InitializeGroups(scriptingGroup, radioBinding);
            if (!menu2.greyOut)
            {
                greyOut = false;
            }
            if (!menu2.willOnlineGrayout)
            {
                willOnlineGrayout = false;
            }
            AddItem(menu2);
        }
        foreach (Thing t in group.AllThings)
        {
            IReadOnlyPropertyBag tBag = ContentProperties.GetBag(t.GetType());
            if (Main.isDemo && tBag.GetOrDefault("isInDemo", defaultValue: false))
            {
                greyOut = false;
            }
            if (tBag.GetOrDefault("isOnlineCapable", defaultValue: true))
            {
                greyOut = false;
                willOnlineGrayout = false;
            }
            if (t is BackgroundTile || t is ForegroundTile || t is SubBackgroundTile)
            {
                ContextBackgroundTile obj = new ContextBackgroundTile(t, this);
                obj.contextThing = t;
                AddItem(obj);
            }
            else if (radioBinding != null)
            {
                if (radioBinding.value is IList)
                {
                    if (radioBinding.value is List<TypeProbPair>)
                    {
                        ContextSlider obj2 = new ContextSlider(t.editorName, this, radioBinding, 0.05f, null, time: false, t.GetType());
                        obj2.greyOut = Main.isDemo && !tBag.GetOrDefault("isInDemo", defaultValue: false);
                        obj2.contextThing = t;
                        if (tBag.GetOrDefault("isOnlineCapable", defaultValue: true))
                        {
                            willOnlineGrayout = false;
                        }
                        AddItem(obj2);
                    }
                    else
                    {
                        ContextCheckBox obj3 = new ContextCheckBox(t.editorName, this, radioBinding, t.GetType());
                        obj3.greyOut = Main.isDemo && !tBag.GetOrDefault("isInDemo", defaultValue: false);
                        obj3.contextThing = t;
                        if (tBag.GetOrDefault("isOnlineCapable", defaultValue: true))
                        {
                            willOnlineGrayout = false;
                        }
                        AddItem(obj3);
                    }
                }
                else
                {
                    ContextRadio obj4 = new ContextRadio(t.editorName, selected: false, t.GetType(), this, radioBinding);
                    obj4.greyOut = Main.isDemo && !tBag.GetOrDefault("isInDemo", defaultValue: false);
                    obj4.contextThing = t;
                    if (tBag.GetOrDefault("isOnlineCapable", defaultValue: true))
                    {
                        willOnlineGrayout = false;
                    }
                    AddItem(obj4);
                }
            }
            else
            {
                ContextObject obj5 = new ContextObject(t, this);
                obj5.contextThing = t;
                obj5.isPinnable = setPinnable;
                AddItem(obj5);
            }
        }
        deep--;
        if (deep == 0)
        {
            UpdateGrayout();
        }
    }

    public void InitializeTypelist(Type pType, FieldBinding pBinding)
    {
        ContextRadio obj = new ContextRadio("None", selected: false, null, this, pBinding);
        AddItem(obj);
        InitializeGroups(new EditorGroup(pType), pBinding);
    }

    public void InitializeTeams(FieldBinding radioBinding)
    {
        ContextRadio obj = new ContextRadio("None", selected: false, 0, this, radioBinding);
        AddItem(obj);
        EditorGroupMenu currentGroup = null;
        int groupIndex = 0;
        if (Teams.all.Count > 10)
        {
            currentGroup = new EditorGroupMenu(this);
            currentGroup.text = "Hats " + groupIndex;
        }
        int curCount = 0;
        for (int i = 0; i < Teams.all.Count; i++)
        {
            if (i >= 4)
            {
                obj = new ContextRadio(Teams.all[i].name, selected: false, i, this, radioBinding);
                if (currentGroup != null)
                {
                    currentGroup.AddItem(obj);
                }
                else
                {
                    AddItem(obj);
                }
                curCount++;
                if (curCount == 10)
                {
                    groupIndex++;
                    curCount = 0;
                    AddItem(currentGroup);
                    currentGroup = new EditorGroupMenu(this);
                    currentGroup.text = "Hats " + groupIndex;
                }
            }
        }
        if (currentGroup != null && curCount > 0)
        {
            AddItem(currentGroup);
        }
    }
}
