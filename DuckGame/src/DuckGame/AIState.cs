using System.Collections.Generic;

namespace DuckGame;

public class AIState
{
	protected Stack<AIState> _state = new Stack<AIState>();

	public virtual AIState DoUpdate(Duck duck, DuckAI ai)
	{
		if (_state.Count > 0)
		{
			AIState s = _state.Peek().DoUpdate(duck, ai);
			if (s == null)
			{
				_state.Pop();
			}
			else if (s != _state.Peek())
			{
				_state.Pop();
				_state.Push(s);
			}
			return this;
		}
		return Update(duck, ai);
	}

	public virtual AIState Update(Duck duck, DuckAI ai)
	{
		return this;
	}
}
