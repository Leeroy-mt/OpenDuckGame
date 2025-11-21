namespace DuckGame;

public class UIMenuItemString : UIMenuItem
{
	private FieldBinding _field;

	private FieldBinding _filterBinding;

	private UIStringEntryMenu _enterStringMenu;

	private string _text;

	private string _id;

	private UIStringEntry _passwordItem;

	private UIMenuActionOpenMenu _activateFunction;

	public void SetFieldBinding(FieldBinding f)
	{
		_field = f;
	}

	public UIMenuItemString(string text, string id, UIMenuAction action = null, FieldBinding field = null, Color c = default(Color), FieldBinding filterBinding = null, bool tiny = false)
		: base(action)
	{
		_text = text;
		if (c == default(Color))
		{
			c = Colors.MenuOption;
		}
		_id = id;
		BitmapFont littleFont = null;
		if (tiny)
		{
			littleFont = new BitmapFont("smallBiosFontUI", 7, 5);
		}
		UIDivider splitter = new UIDivider(vert: true, 0f);
		if (text != "")
		{
			UIText t = new UIText(text, c);
			if (tiny)
			{
				t.SetFont(littleFont);
			}
			t.align = UIAlign.Left;
			splitter.leftSection.Add(t);
		}
		_passwordItem = new UIStringEntry(directional: false, "", Color.White);
		_passwordItem.align = UIAlign.Right;
		splitter.rightSection.Add(_passwordItem);
		base.rightSection.Add(splitter);
		if (tiny)
		{
			_arrow = new UIImage("littleContextArrowRight");
		}
		else
		{
			_arrow = new UIImage("contextArrowRight");
		}
		_arrow.align = UIAlign.Right;
		_arrow.visible = false;
		base.leftSection.Add(_arrow);
		_field = field;
		_filterBinding = filterBinding;
		controlString = "@CANCEL@BACK @WASD@ADJUST";
	}

	public override void Update()
	{
		if ((string)_field.value == "")
		{
			if (base.open && _id == "name" && Profiles.active.Count > 0)
			{
				_field.value = TeamSelect2.DefaultGameName();
				_passwordItem.text = (string)_field.value;
			}
			else
			{
				_passwordItem.text = "NONE";
			}
		}
		else
		{
			_passwordItem.text = (string)_field.value;
		}
		base.Update();
	}

	public void InitializeEntryMenu(UIComponent pGroup, UIMenu pReturn)
	{
		if (_id == "port")
		{
			_enterStringMenu = new UIStringEntryMenu(directional: false, "SET " + _text, _field, 6, pNumeric: true, 1337, 55535);
		}
		else
		{
			_enterStringMenu = new UIStringEntryMenu(directional: false, "SET " + _text, _field);
		}
		_enterStringMenu.SetBackFunction(new UIMenuActionOpenMenu(_enterStringMenu, pReturn));
		_enterStringMenu.Close();
		pGroup.Add(_enterStringMenu, doAnchor: false);
		_activateFunction = new UIMenuActionOpenMenu(pReturn, _enterStringMenu);
	}

	public override void Activate(string trigger)
	{
		if (trigger == "SELECT")
		{
			_enterStringMenu.SetValue((string)_field.value);
			_activateFunction.Activate();
		}
	}
}
