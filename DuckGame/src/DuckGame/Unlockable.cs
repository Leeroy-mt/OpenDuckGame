using System;

namespace DuckGame;

public class Unlockable
{
	public bool allowHints;

	public bool _locked = true;

	private string _description;

	private string _name;

	private string _achievement;

	private string _id;

	protected bool _showScreen;

	private Func<bool> _condition;

	public bool locked => _locked;

	public string description => _description;

	public string name => _name;

	public string achievement => _achievement;

	public string id => _id;

	public bool showScreen => _showScreen;

	public Unlockable(string identifier, Func<bool> condition, string nam, string desc, string achieve = "")
	{
		_condition = condition;
		_description = desc;
		_name = nam;
		_achievement = achieve;
		_id = identifier;
	}

	public string GetNameForDisplay()
	{
		return name.ToUpperInvariant();
	}

	public bool CheckCondition()
	{
		if (NetworkDebugger.enabled && NetworkDebugger.currentIndex == 1)
		{
			return false;
		}
		return _condition();
	}

	public virtual void Initialize()
	{
	}

	public virtual void DoUnlock()
	{
		Unlock();
		_locked = false;
		if (_achievement != null && _achievement != "")
		{
			Global.GiveAchievement(_achievement);
		}
	}

	protected virtual void Unlock()
	{
	}

	public virtual void DoLock()
	{
		Lock();
		_locked = true;
	}

	protected virtual void Lock()
	{
	}

	public virtual void Draw(float xpos, float ypos, Depth depth)
	{
	}
}
