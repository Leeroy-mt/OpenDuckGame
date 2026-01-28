using System;

namespace DuckGame;

public class UIMenuItem : UIDivider
{
    public string controlString;

    #region Protected Fields

    protected bool _isBackButton;

    protected UIImage _arrow;

    protected UIMenuAction _action;

    protected UIText _textElement;

    #endregion

    bool _selected;

    #region Public Properties

    public bool selected
    {
        get => _selected;
        set => _selected = value;
    }

    public bool isBackButton => _isBackButton;

    public string text
    {
        get => _textElement.text;
        set
        {
            _textElement.text = value;
            _dirty = _textElement.dirty = true;
        }
    }

    public UIMenuAction menuAction
    {
        get => _action;
        set => _action = value;
    }

    #endregion

    #region Public Constructors

    public UIMenuItem(string text, UIMenuAction action = null, UIAlign al = UIAlign.Center, Color c = default, bool backButton = false)
        : base(vert: true, 8)
    {
        if (c == default)
            c = Colors.MenuOption;
        _textElement = new UIText(text, c)
        {
            align = UIAlign.Left
        };
        rightSection.Add(_textElement);
        _arrow = new UIImage("contextArrowRight")
        {
            align = UIAlign.Right,
            visible = false
        };
        leftSection.Add(_arrow);
        _action = action;
        align = al;
        _isBackButton = backButton;
    }

    public UIMenuItem(Func<string> pTextFunc, UIMenuAction action = null, UIAlign al = UIAlign.Center, Color c = default, bool backButton = false)
        : base(vert: true, 8)
    {
        if (c == default)
            c = Colors.MenuOption;
        _textElement = new UIText(pTextFunc, c)
        {
            align = UIAlign.Left
        };
        rightSection.Add(_textElement);
        _arrow = new UIImage("contextArrowRight")
        {
            align = UIAlign.Right,
            visible = false
        };
        leftSection.Add(_arrow);
        _action = action;
        align = al;
        _isBackButton = backButton;
    }

    public UIMenuItem(UIMenuAction action = null, Color c = default)
        : base(vert: true, 8)
    {
        _action = action;
    }

    #endregion

    #region Public Methods

    public virtual void Activate(string trigger)
    {
        if (_action != null && trigger == "SELECT")
            _action.Activate();
    }

    public override void Update()
    {
        _arrow.visible = _selected;
        _action?.Update();
        base.Update();
    }

    public void SetFont(BitmapFont font)
    {
        _textElement.SetFont(font);
    }

    #endregion
}
