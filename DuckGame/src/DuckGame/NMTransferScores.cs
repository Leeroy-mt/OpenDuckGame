using System.Collections.Generic;

namespace DuckGame;

public class NMTransferScores : NMEvent
{
	public List<int> scores = new List<int>();

	public NMTransferScores(List<int> scrs)
	{
		scores = scrs;
	}

	public NMTransferScores()
	{
	}

	protected override void OnSerialize()
	{
		base.OnSerialize();
		_serializedData.Write((byte)scores.Count);
		foreach (int index in scores)
		{
			_serializedData.Write((byte)index);
		}
	}

	public override void OnDeserialize(BitBuffer d)
	{
		base.OnDeserialize(d);
		byte num = d.ReadByte();
		for (int i = 0; i < num; i++)
		{
			scores.Add(d.ReadByte());
		}
	}

	public override void Activate()
	{
		int index = 0;
		foreach (Profile p in DuckNetwork.profiles)
		{
			if (p.team != null && index < scores.Count)
			{
				p.team.score = scores[index];
			}
			index++;
		}
		GameMode.RunPostRound(testMode: false);
		Send.Message(new NMScoresReceived());
	}
}
