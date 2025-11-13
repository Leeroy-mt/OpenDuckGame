using System.Collections.Generic;

namespace DuckGame;

public class InputProfileCore
{
	public Dictionary<string, InputProfile> _profiles = new Dictionary<string, InputProfile>();

	public Dictionary<int, InputProfile> _virtualProfiles = new Dictionary<int, InputProfile>();

	public InputProfile DefaultPlayer1 => Get(InputProfile.MPPlayer1);

	public InputProfile DefaultPlayer2 => Get(InputProfile.MPPlayer2);

	public InputProfile DefaultPlayer3 => Get(InputProfile.MPPlayer3);

	public InputProfile DefaultPlayer4 => Get(InputProfile.MPPlayer4);

	public InputProfile DefaultPlayer5 => Get(InputProfile.MPPlayer5);

	public InputProfile DefaultPlayer6 => Get(InputProfile.MPPlayer6);

	public InputProfile DefaultPlayer7 => Get(InputProfile.MPPlayer7);

	public InputProfile DefaultPlayer8 => Get(InputProfile.MPPlayer8);

	public List<InputProfile> defaultProfiles => new List<InputProfile> { DefaultPlayer1, DefaultPlayer2, DefaultPlayer3, DefaultPlayer4, DefaultPlayer5, DefaultPlayer6, DefaultPlayer7, DefaultPlayer8 };

	public InputProfile Add(string name)
	{
		InputProfile newProfile = new InputProfile(name);
		if (_profiles.TryGetValue(name, out var existing))
		{
			return existing;
		}
		_profiles[name] = newProfile;
		return newProfile;
	}

	public InputProfile Get(string name)
	{
		if (_profiles.TryGetValue(name, out var existing))
		{
			return existing;
		}
		return null;
	}

	public void Update()
	{
		foreach (KeyValuePair<string, InputProfile> profile2 in _profiles)
		{
			profile2.Value.UpdateTriggerStates();
		}
	}

	public InputProfile GetVirtualInput(int index)
	{
		if (_virtualProfiles.TryGetValue(index, out var p))
		{
			return p;
		}
		p = Add("virtual" + index);
		p.dindex = NetworkDebugger.currentIndex;
		VirtualInput device = new VirtualInput(index);
		device.pdraw = NetworkDebugger.currentIndex;
		for (int i = 0; i < Network.synchronizedTriggers.Count; i++)
		{
			p.Map(device, Network.synchronizedTriggers[i], i);
		}
		device.availableTriggers = Network.synchronizedTriggers;
		_virtualProfiles[index] = p;
		return p;
	}
}
