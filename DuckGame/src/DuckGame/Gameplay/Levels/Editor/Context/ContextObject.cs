using Microsoft.Xna.Framework;

namespace DuckGame;

public class ContextObject : ContextMenu
{
    private IReadOnlyPropertyBag _thingBag;

    private Thing _thing;

    private bool _placement;

    public static int lastForceGrid;

    private int _framesSinceSelected = 999;

    public Thing thing => _thing;

    public ContextObject(Thing thing, IContextListener owner, bool placement = true)
        : base(owner)
    {
        _placement = placement;
        _thing = thing;
        _image = thing.GeneratePreview(16, 16, transparentBack: true);
        itemSize.Y = 16f;
        _text = thing.editorName;
        itemSize.X = Graphics.GetFancyStringWidth(_text) + 26f;
        _thingBag = ContentProperties.GetBag(thing.GetType());
        if (Main.isDemo && !_thingBag.GetOrDefault("isInDemo", defaultValue: false))
        {
            greyOut = true;
        }
        else
        {
            greyOut = false;
        }
        if (_thingBag.GetOrDefault("previewPriority", defaultValue: false))
        {
            _previewPriority = true;
        }
        tooltip = thing.editorTooltip;
        if (!_thingBag.GetOrDefault("isOnlineCapable", defaultValue: true))
        {
            tooltip = "(OFFLINE ONLY) " + tooltip;
        }
        int thingCost = Editor.CalculatePlacementCost(thing);
        bool cost = false;
        if (thingCost > 0 && Editor.placementLimit > 0)
        {
            tooltip = "(" + thingCost + " @EDITORCURRENCY@) " + tooltip;
            cost = true;
        }
        if (tooltip == null)
        {
            tooltip = "";
        }
        if (tooltip != "" || cost)
        {
            tooltip = thing.editorName + ": " + tooltip;
        }
    }

    public override void Selected()
    {
        bool force = false;
        if (_framesSinceSelected < 20 || Editor.inputMode != EditorInput.Touch)
        {
            force = true;
        }
        _framesSinceSelected = 0;
        if (scrollButtonDirection != 0)
        {
            _owner.Selected(this);
        }
        else
        {
            if (Main.isDemo && !_thingBag.GetOrDefault("isInDemo", defaultValue: false))
            {
                return;
            }
            if (_placement)
            {
                if (Level.current is Editor editor)
                {
                    editor.placementType = _thing;
                    if (force)
                    {
                        editor.CloseMenu();
                    }
                    if (_thing.forceEditorGrid != 0)
                    {
                        editor.cellSize = _thing.forceEditorGrid;
                        lastForceGrid = (int)editor.cellSize;
                    }
                    else if (lastForceGrid != 0)
                    {
                        lastForceGrid = 0;
                        editor.cellSize = 16f;
                    }
                    SFX.Play("lowClick", 0.3f);
                }
            }
            else if (_owner != null)
            {
                _owner.Selected(this);
            }
        }
    }

    public override void Draw()
    {
        _framesSinceSelected++;
        if (_hover && !greyOut)
        {
            Graphics.DrawRect(Position, Position + itemSize, new Color(70, 70, 70), base.Depth + 1);
        }
        if (scrollButtonDirection != 0)
        {
            _arrow.Depth = base.Depth + 2;
            if (scrollButtonDirection > 0)
            {
                _arrow.flipV = true;
                Graphics.Draw(_arrow, Position.X + (_owner as ContextMenu).menuSize.X / 2f, Position.Y + 8f);
            }
            else
            {
                _arrow.flipV = false;
                Graphics.Draw(_arrow, Position.X + (_owner as ContextMenu).menuSize.X / 2f, Position.Y + 8f);
            }
            return;
        }
        Color c = Color.White;
        if (greyOut)
        {
            c = Color.White * 0.3f;
        }
        Graphics.DrawFancyString(_text, Position + new Vector2(22f, 4f), c, base.Depth + 2);
        _image.Depth = base.Depth + 3;
        _image.X = base.X + 1f;
        _image.Y = base.Y;
        _image.color = c;
        _image.Scale = new Vector2(1f);
        _image.Draw();
    }
}
