using NAudio.Midi;

namespace DuckGame;

internal static class Midi
{
	private static MidiIn _midi;

	public static void Initialize()
	{
		string[] deviceNames = new string[MidiIn.NumberOfDevices];
		for (int device = 0; device < MidiIn.NumberOfDevices; device++)
		{
			deviceNames[device] = MidiIn.DeviceInfo(device).ProductName;
		}
		_midi = new MidiIn(0);
		_midi.MessageReceived += MidiMessage;
	}

	private static void MidiMessage(object sender, MidiInMessageEventArgs args)
	{
		_ = args.MidiEvent.CommandCode;
		_ = 144;
	}
}
