using System.Collections.Generic;

namespace DuckGame;

[FixedNetworkID(14)]
public class NMRequestJoin : NMDuckNetwork
{
	public struct Info
	{
		public byte roomFlippers;

		public int flagIndex;

		public bool hasCustomHats;

		public bool parentalControlsActive;

		public static Info Construct()
		{
			return new Info
			{
				roomFlippers = Profile.CalculateLocalFlippers(),
				flagIndex = Global.data.flag,
				hasCustomHats = (Teams.core.extraTeams.Count > 0),
				parentalControlsActive = ParentalControls.AreParentalControlsActive()
			};
		}

		public void Serialize(BitBuffer pBuffer)
		{
			pBuffer.Write(roomFlippers);
			pBuffer.Write(flagIndex);
			pBuffer.Write(hasCustomHats);
			pBuffer.Write(parentalControlsActive);
		}

		public static Info Deserialize(BitBuffer pBuffer)
		{
			return new Info
			{
				roomFlippers = pBuffer.ReadByte(),
				flagIndex = pBuffer.ReadInt(),
				hasCustomHats = pBuffer.ReadBool(),
				parentalControlsActive = pBuffer.ReadBool()
			};
		}
	}

	public string password;

	public Info info;

	public bool wasInvited;

	public ulong localID;

	public List<string> names = new List<string>();

	public List<byte> personas = new List<byte>();

	public NMRequestJoin()
	{
	}

	public NMRequestJoin(List<string> pNames, List<byte> pPersonas, bool pWasInvited = false, string pPassword = "", ulong pLocalID = 0uL)
	{
		wasInvited = pWasInvited;
		localID = pLocalID;
		names = pNames;
		info = Info.Construct();
		password = pPassword;
		personas = pPersonas;
	}

	protected override void OnSerialize()
	{
		info.Serialize(_serializedData);
		_serializedData.Write(wasInvited);
		_serializedData.Write((byte)0);
		_serializedData.Write(password);
		_serializedData.Write(localID);
		_serializedData.Write((byte)names.Count);
		foreach (string s in names)
		{
			_serializedData.Write(s);
		}
		foreach (byte b in personas)
		{
			_serializedData.Write(b);
		}
	}

	public override void OnDeserialize(BitBuffer d)
	{
		info = Info.Deserialize(d);
		wasInvited = d.ReadBool();
		int numNames = d.ReadByte();
		if (numNames == 0)
		{
			password = d.ReadString();
			localID = d.ReadULong();
			numNames = d.ReadByte();
		}
		names = new List<string>();
		personas = new List<byte>();
		for (int i = 0; i < numNames; i++)
		{
			names.Add(d.ReadString());
		}
		for (int j = 0; j < numNames; j++)
		{
			personas.Add(d.ReadByte());
		}
	}
}
