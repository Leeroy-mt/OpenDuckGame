namespace DuckGame;

public class NetStepActivator
{
	public delegate void Function();

	private Function _function;

	private int _index;

	public int index
	{
		get
		{
			return _index;
		}
		set
		{
			_index = value;
			if (_index > 3)
			{
				_index = 0;
			}
		}
	}

	public NetStepActivator(Function del)
	{
		_function = del;
	}

	public void Activate()
	{
		if (_function != null)
		{
			_function();
		}
		Step();
	}

	public void Step()
	{
		index++;
	}
}
