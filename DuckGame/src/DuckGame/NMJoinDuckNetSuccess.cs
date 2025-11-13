using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class NMJoinDuckNetSuccess : NMDuckNetwork
{
	public List<Profile> profiles = new List<Profile>();

	public NMJoinDuckNetSuccess()
	{
	}

	public NMJoinDuckNetSuccess(List<Profile> pProfiles)
	{
		profiles = pProfiles;
	}

	protected override void OnSerialize()
	{
		_serializedData.Write((byte)profiles.Count);
		for (int i = 0; i < profiles.Count; i++)
		{
			_serializedData.WriteProfile(profiles[i]);
			_serializedData.Write((ushort)(int)profiles[i].latestGhostIndex);
			_serializedData.WriteTeam(profiles[i].team);
			_serializedData.Write(profiles[i].reservedSpectatorPersona);
			_serializedData.Write((byte)profiles[i].persona.index);
		}
	}

	public override void OnDeserialize(BitBuffer msg)
	{
		profiles = new List<Profile>();
		byte count = msg.ReadByte();
		for (int i = 0; i < count; i++)
		{
			profiles.Add(msg.ReadProfile());
			profiles[i].latestGhostIndex = msg.ReadUShort();
			Team t = msg.ReadTeam();
			if (t != null)
			{
				profiles[i].reservedTeam = t;
			}
			sbyte persona = msg.ReadSByte();
			profiles[i].reservedSpectatorPersona = persona;
			sbyte duckSona = msg.ReadSByte();
			if (duckSona >= 0 && duckSona < Persona.all.Count())
			{
				profiles[i].persona = Persona.all.ElementAt(duckSona);
			}
		}
	}
}
