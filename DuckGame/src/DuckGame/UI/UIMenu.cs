using Microsoft.Xna.Framework;

namespace DuckGame;

public class UIMenu : UIBox
{
    #region Public Fields

    public static bool disabledDraw;

    public static bool globalUILock;

    #endregion

    #region Protected Fields

    protected string _controlString;

    protected UIDivider _splitter;

    #endregion

    #region Private Fields

    UIBox _section;

    UIText _controlText;

    InputProfile _controlProfile;

    #endregion

    public string title
    {
        get => ((UIText)_splitter.topSection.components[0]).text;
        set => ((UIText)_splitter.topSection.components[0]).text = value;
    }

    public UIMenu(string title, float xpos, float ypos, float wide = -1, float high = -1, string conString = "", InputProfile conProfile = null, bool tiny = false)
        : base(xpos, ypos, wide, high)
    {
        _controlProfile = conProfile;
        _splitter = new UIDivider(vert: false, 0, 4);
        _section = _splitter.rightSection;
        UIText optionsText = new(title, Color.White);
        if (tiny)
        {
            BitmapFont littleFont = new("smallBiosFontUI", 7, 5);
            optionsText.SetFont(littleFont);
        }
        optionsText.align |= UIAlign.Top;
        _splitter.topSection.Add(optionsText);
        _controlString = conString;
        if (_controlString != "" && _controlString != null)
        {
            UIDivider controlSplitter = new(vert: false, 0, 4);
            _controlText = new UIText(_controlString, Color.White, UIAlign.Center, 4, _controlProfile);
            controlSplitter.bottomSection.Add(_controlText);
            Add(controlSplitter);
            _section = controlSplitter.topSection;
        }
        base.Add(_splitter, doAnchor: true);
    }

    #region Public Methods

    public override void SelectLastMenuItem()
    {
        _section.SelectLastMenuItem();
    }

    public override void AssignDefaultSelection()
    {
        _section.AssignDefaultSelection();
    }

    public override void Add(UIComponent component, bool doAnchor = true)
    {
        _section.Add(component, doAnchor);
        _dirty = true;
    }

    public override void Insert(UIComponent component, int position, bool doAnchor = true)
    {
        _section.Insert(component, position, doAnchor);
        _dirty = true;
    }

    public override void Update()
    {
        _controlText?.text = _section._hoverControlString ?? _controlString;
        base.Update();
    }

    public override void Draw()
    {
        if (open || animating)
            base.Draw();
    }

    public override void Remove(UIComponent component)
    {
        _section.Remove(component);
        _dirty = true;
    }

    public void SetBackFunction(UIMenuAction pAction)
    {
        _section._backFunction = pAction;
        _backFunction = pAction;
    }

    public void SetCloseFunction(UIMenuAction pAction)
    {
        _section._closeFunction = pAction;
        _closeFunction = pAction;
    }

    public void SetAcceptFunction(UIMenuAction pAction)
    {
        _section._acceptFunction = pAction;
        _acceptFunction = pAction;
    }

    public void RunBackFunction()
    {
        _section._backFunction?.Activate();
    }

    public UIComponent AddMatchSetting(MatchSetting m, bool filterMenu, bool enabled = true)
    {
        UIComponent component = null;
        if (m.value is int)
        {
            FieldBinding upperBound = null;
            if (m.maxSyncID != null)
            {
                foreach (MatchSetting ma in TeamSelect2.matchSettings)
                    if (ma.id == m.maxSyncID)
                        upperBound = new FieldBinding(ma, "value");
            }
            FieldBinding lowerBound = null;
            if (m.minSyncID != null)
            {
                foreach (MatchSetting ma2 in TeamSelect2.matchSettings)
                    if (ma2.id == m.minSyncID)
                        lowerBound = new FieldBinding(ma2, "value");
            }
            component = new UIMenuItemNumber(m.name, null, new FieldBinding(m, "value", m.min, m.max), m.step, default(Color), upperBound, lowerBound, m.suffix, filterMenu ? new FieldBinding(m, "filtered") : null, m.valueStrings, m);
            if (m.percentageLinks != null)
            {
                foreach (string percentageLink in m.percentageLinks)
                {
                    MatchSetting set = TeamSelect2.GetMatchSetting(percentageLink);
                    (component as UIMenuItemNumber).percentageGroup.Add(new FieldBinding(set, "value", set.min, set.max, set.step));
                }
            }
        }
        else if (m.value is bool)
            component = new UIMenuItemToggle(m.name, null, new FieldBinding(m, "value"), default, filterMenu ? new FieldBinding(m, "filtered") : null);
        else if (m.value is string)
            component = new UIMenuItemString(m.name, m.id, null, new FieldBinding(m, "value"), default, filterMenu ? new FieldBinding(m, "filtered") : null);
        component.condition = m.condition;
        if (component != null)
        {
            component.isEnabled = enabled;
            _section.Add(component);
            _dirty = true;
        }
        return component;
    }

    #endregion
}
