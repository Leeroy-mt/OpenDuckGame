using Microsoft.Xna.Framework.Graphics;
using System;

namespace DuckGame;

public class ContextBackgroundTile : ContextMenu
{
    private Thing _thing;

    private new Sprite _image;

    private bool _placement;

    protected Vec2 _hoverPos = Vec2.Zero;

    public bool positionCursor;

    private ContextFile _file;

    private Vec2 _rememberedMousePosition;

    private bool justOpened = true;

    public bool floatMode;

    public Thing thing => _thing;

    public ContextBackgroundTile(Thing thing, IContextListener owner, bool placement = true)
        : base(owner)
    {
        _placement = placement;
        _thing = thing;
        _image = thing.GetEditorImage();
        itemSize.x = 180f;
        itemSize.y = 16f;
        _text = thing.editorName;
        itemSize.x = _text.Length * 8 + 16;
        _canExpand = true;
        base.depth = 0.8f;
        if (_thing is CustomBackground)
        {
            _file = new ContextFile("LOAD FILE...", this, new FieldBinding(_thing, "customBackground0" + ((thing as CustomBackground).customIndex + 1)), ContextFileType.Background);
        }
        IReadOnlyPropertyBag thingProps = ContentProperties.GetBag(thing.GetType());
        if (Main.isDemo && !thingProps.GetOrDefault("isInDemo", defaultValue: false))
        {
            greyOut = true;
        }
    }

    public override bool HasOpen()
    {
        return base.opened;
    }

    public override void Selected()
    {
        if (!greyOut)
        {
            SFX.Play("highClick", 0.3f, 0.2f);
            if (_owner != null)
            {
                _owner.Selected(this);
            }
        }
    }

    public override void Closed()
    {
        base.Closed();
    }

    public override void Draw()
    {
        if (!_root)
        {
            float cMult = 1f;
            if (greyOut)
            {
                cMult = 0.3f;
            }
            if (_hover && !greyOut)
            {
                Graphics.DrawRect(position, position + itemSize, new Color(70, 70, 70), 0.82f);
            }
            Graphics.DrawFancyString(_text, position + new Vec2(2f, 4f), Color.White * cMult, 0.85f);
            _contextArrow.color = Color.White * cMult;
            Graphics.Draw(_contextArrow, base.x + itemSize.x - 11f, base.y + 3f, 0.85f);
        }
        if (base.opened)
        {
            SpriteMap map = _thing.graphic as SpriteMap;
            int wide = map.texture.width / map.w;
            int high = map.texture.height / map.h;
            if (justOpened)
            {
                tooltip = _text;
                int thingCost = Editor.CalculatePlacementCost(_thing);
                if (thingCost > 0)
                {
                    tooltip = tooltip + ": (" + thingCost + " @EDITORCURRENCY@)";
                }
                _hoverPos = new Vec2(_selectedIndex % wide * map.w, _selectedIndex / wide * map.h);
                if (Editor.inputMode == EditorInput.Mouse && positionCursor)
                {
                    _rememberedMousePosition = Mouse.position;
                    Mouse.position = _hoverPos + position + new Vec2(8f, 8f);
                    positionCursor = false;
                }
            }
            menuSize = new Vec2(map.texture.width + 2, map.texture.height + 2);
            float menuWidth = menuSize.x;
            float menuHeight = menuSize.y;
            Vec2 pos = new Vec2(base.x, base.y);
            if (Editor.inputMode != EditorInput.Mouse && !_root)
            {
                pos.y = 16f;
            }
            if (!_root)
            {
                pos.x += itemSize.x + 4f;
                pos.y -= 2f;
            }
            new Vec2(map.position);
            _thing.x = pos.x + 1f + (float)map.w / 2f;
            _thing.y = pos.y + 1f + (float)map.h / 2f;
            _thing.depth = 0.7f;
            Graphics.DrawRect(pos, pos + new Vec2(menuWidth, menuHeight), new Color(70, 70, 70), 0.5f);
            Graphics.DrawRect(pos + new Vec2(1f, 1f), pos + new Vec2(menuWidth - 1f, menuHeight - 1f), new Color(30, 30, 30), 0.6f);
            _lastDrawPos = pos;
            Graphics.Draw(map.texture, new Vec2(_thing.x, _thing.y), null, Color.White, 0f, _thing.center, _thing.scale, SpriteEffects.None, 0.7f);
            if (_root && _file != null)
            {
                Vec2 loadTL = new Vec2(pos + new Vec2(menuWidth + 4f, 0f));
                new Vec2(pos + new Vec2(menuWidth + 97f, 12f));
                _file.position = loadTL;
                _file.Update();
                _file.Draw();
            }
            if (Editor.inputMode == EditorInput.Touch && (_file == null || !_file.hover))
            {
                Vec2 touchCoords = new Vec2(-1f, -1f);
                if (TouchScreen.GetTap() != Touch.None)
                {
                    touchCoords = TouchScreen.GetTap().Transform(base.layer.camera);
                    _hoverPos = new Vec2(touchCoords.x - _thing.x, touchCoords.y - _thing.y);
                }
            }
            else if (Editor.inputMode == EditorInput.Gamepad && (_file == null || !_file.hover) && !Editor.clickedMenu)
            {
                _hoverPos = new Vec2(_selectedIndex % wide * map.w, _selectedIndex / wide * map.h);
                if (Input.Pressed("MENULEFT"))
                {
                    if (_selectedIndex == 0 && _owner != null)
                    {
                        Selected(null);
                        base.opened = false;
                    }
                    else
                    {
                        _selectedIndex--;
                    }
                }
                if (Input.Pressed("MENURIGHT"))
                {
                    if (_file != null && _selectedIndex == wide - 1)
                    {
                        _file.hover = true;
                    }
                    else
                    {
                        _selectedIndex++;
                    }
                }
                if (Input.Pressed("MENUUP"))
                {
                    _selectedIndex -= wide;
                }
                if (Input.Pressed("MENUDOWN"))
                {
                    _selectedIndex += wide;
                }
                if (_selectedIndex < 0)
                {
                    _selectedIndex = 0;
                }
                if (_selectedIndex > wide * high - 1)
                {
                    _selectedIndex = wide * high - 1;
                }
            }
            else if (Editor.inputMode == EditorInput.Mouse)
            {
                _hoverPos = new Vec2(Mouse.x - _thing.x, Mouse.y - _thing.y);
            }
            if (_file != null && _file.hover && Input.Pressed("MENULEFT"))
            {
                _file.hover = false;
                _selectedIndex = wide - 1;
            }
            Editor editor = Level.current as Editor;
            _hoverPos.x = (float)Math.Round(_hoverPos.x / (float)map.w) * (float)map.w;
            _hoverPos.y = (float)Math.Round(_hoverPos.y / (float)map.h) * (float)map.h;
            if ((_file == null || !_file.hover) && _hoverPos.x >= 0f && _hoverPos.x < (float)map.texture.width && _hoverPos.y >= 0f && _hoverPos.y < (float)map.texture.height)
            {
                Graphics.DrawRect(_hoverPos + pos, _hoverPos + pos + new Vec2(map.w + 2, map.h + 2), Color.Lime * 0.8f, 0.8f, filled: false);
                if ((Editor.inputMode == EditorInput.Mouse && Mouse.left == InputState.Pressed) || (Editor.inputMode == EditorInput.Gamepad && Input.Pressed("SELECT") && !justOpened) || (Editor.inputMode == EditorInput.Touch && TouchScreen.GetTap() != Touch.None))
                {
                    if (_thing is BackgroundTile)
                    {
                        (_thing as BackgroundTile).frame = (int)(_hoverPos.x / (float)map.w + _hoverPos.y / (float)map.h * (float)(map.texture.width / map.w));
                    }
                    else
                    {
                        map.frame = (int)(_hoverPos.x / (float)map.w + _hoverPos.y / (float)map.h * (float)(map.texture.width / map.w));
                    }
                    editor.placementType = _thing;
                    editor.placementType = _thing;
                    if (!floatMode || Editor.inputMode == EditorInput.Gamepad)
                    {
                        Disappear();
                        editor.CloseMenu();
                    }
                }
            }
            if (!justOpened && Input.Pressed("MENU1") && owner == null)
            {
                Disappear();
                editor.CloseMenu();
            }
            justOpened = false;
        }
        else
        {
            tooltip = "";
            justOpened = true;
        }
    }

    public override void Disappear()
    {
        if (_rememberedMousePosition != Vec2.Zero)
        {
            Mouse.position = _rememberedMousePosition;
            _rememberedMousePosition = Vec2.Zero;
        }
        base.Disappear();
    }
}
