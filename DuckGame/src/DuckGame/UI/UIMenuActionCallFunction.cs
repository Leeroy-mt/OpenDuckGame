namespace DuckGame;

public class UIMenuActionCallFunction : UIMenuAction
{
	public delegate void Function();

	private Function _function;

	public UIMenuActionCallFunction(Function f)
	{
		_function = f;
	}

	public override void Activate()
	{
		_function();
	}
}
