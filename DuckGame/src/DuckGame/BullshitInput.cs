using System.Collections.Generic;

namespace DuckGame;

public class BullshitInput : InputProfile
{
	private class CountdownPair
	{
		public float current;

		public float previous;
	}

	private Stack<AIState> _state = new Stack<AIState>();

	private Dictionary<string, InputState> _inputState = new Dictionary<string, InputState>();

	private AILocomotion _locomotion = new AILocomotion();

	private Dictionary<string, CountdownPair> _bullshitTriggerStates = new Dictionary<string, CountdownPair>
	{
		{
			"LEFT",
			new CountdownPair()
		},
		{
			"RIGHT",
			new CountdownPair()
		},
		{
			"UP",
			new CountdownPair()
		},
		{
			"DOWN",
			new CountdownPair()
		},
		{
			"JUMP",
			new CountdownPair()
		},
		{
			"QUACK",
			new CountdownPair()
		},
		{
			"SHOOT",
			new CountdownPair()
		},
		{
			"GRAB",
			new CountdownPair()
		},
		{
			"RAGDOLL",
			new CountdownPair()
		},
		{
			"STRAFE",
			new CountdownPair()
		},
		{
			"SELECT",
			new CountdownPair()
		},
		{
			"LTRIGGER",
			new CountdownPair()
		},
		{
			"RTRIGGER",
			new CountdownPair()
		},
		{
			"LSTICK",
			new CountdownPair()
		},
		{
			"RSTICK",
			new CountdownPair()
		}
	};

	public AILocomotion locomotion => _locomotion;

	public override float leftTrigger => 0f;

	public override bool Pressed(string trigger, bool any = false)
	{
		if (_bullshitTriggerStates.ContainsKey(trigger) && _bullshitTriggerStates[trigger].current > _bullshitTriggerStates[trigger].previous)
		{
			return true;
		}
		return false;
	}

	public override bool Released(string trigger)
	{
		if (_bullshitTriggerStates.ContainsKey(trigger) && _bullshitTriggerStates[trigger].current <= 0f && _bullshitTriggerStates[trigger].previous > 0f)
		{
			return true;
		}
		return false;
	}

	public override bool Down(string trigger)
	{
		if (_bullshitTriggerStates.ContainsKey(trigger) && _bullshitTriggerStates[trigger].current > 0f)
		{
			return true;
		}
		return false;
	}

	public override void UpdateExtraInput()
	{
		foreach (KeyValuePair<string, CountdownPair> pair in _bullshitTriggerStates)
		{
			pair.Value.previous = pair.Value.current;
			if (Rando.Int(100) == 0)
			{
				_bullshitTriggerStates[pair.Key].current = Rando.Float(2f);
			}
			_bullshitTriggerStates[pair.Key].current = _bullshitTriggerStates[pair.Key].current - Maths.IncFrameTimer();
		}
	}
}
