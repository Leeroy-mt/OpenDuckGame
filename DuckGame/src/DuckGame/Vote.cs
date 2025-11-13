using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class Vote
{
	private static List<RegisteredVote> _votes = new List<RegisteredVote>();

	private static string _voteButton = "";

	private static bool _votingOpen = false;

	public static void OpenVoting(string voteMessage, string voteButton, bool openCorners = true)
	{
		if (openCorners)
		{
			_voteButton = voteButton;
			HUD.CloseAllCorners();
			HUD.AddCornerControl(HUDCorner.BottomRight, "@" + voteButton + "@" + voteMessage);
		}
		_votingOpen = true;
	}

	public static RegisteredVote GetVote(Profile who)
	{
		return _votes.FirstOrDefault((RegisteredVote x) => x.who == who);
	}

	public static void RegisterVote(Profile who, VoteType vote)
	{
		if (_votingOpen || !Network.isActive || vote == VoteType.None)
		{
			RegisteredVote v = _votes.FirstOrDefault((RegisteredVote x) => x.who == who);
			if (v == null)
			{
				_votes.Add(new RegisteredVote
				{
					who = who,
					vote = vote
				});
			}
			else if (v.vote != VoteType.None)
			{
				v.wobble = 1f;
			}
			else
			{
				v.vote = vote;
				v.open = true;
				v.doClose = false;
			}
		}
	}

	public static void CloseVoting()
	{
		foreach (RegisteredVote vote in _votes)
		{
			vote.doClose = true;
		}
		_voteButton = "";
		_votingOpen = false;
	}

	public static void ClearVotes()
	{
		_votes.Clear();
	}

	public static bool Passed(VoteType type)
	{
		int votes = 0;
		foreach (RegisteredVote vote in _votes)
		{
			if (vote.open && vote.vote == type && (vote.who == null || vote.who.slotType != SlotType.Spectator))
			{
				votes++;
			}
		}
		IEnumerable<Profile> profiles = Profiles.all.Where((Profile x) => x.team != null && x.slotType != SlotType.Spectator);
		if (votes >= profiles.Count())
		{
			return true;
		}
		return false;
	}

	public static void Update()
	{
		if (_voteButton != "")
		{
			foreach (Profile p in Profiles.all.Where((Profile x) => x.team != null))
			{
				if (p.inputProfile != null && p.inputProfile.Pressed(_voteButton))
				{
					RegisterVote(p, VoteType.Skip);
				}
			}
		}
		if (!_votes.Exists((RegisteredVote x) => x.open && x.slide < 0.9f))
		{
			foreach (RegisteredVote vote in _votes)
			{
				if (vote.doClose)
				{
					vote.open = false;
				}
			}
		}
		foreach (RegisteredVote vote2 in _votes)
		{
			if (vote2.vote == VoteType.None)
			{
				vote2.open = false;
			}
			vote2.slide = Lerp.FloatSmooth(vote2.slide, vote2.open ? 1f : (-0.1f), 0.1f, 1.1f);
			vote2.wobble = Lerp.Float(vote2.wobble, 0f, 0.05f);
			vote2.wobbleInc += 0.5f;
			if (!vote2.open && vote2.slide < 0.01f)
			{
				vote2.vote = VoteType.None;
			}
		}
	}

	public static void Draw()
	{
		int index = 0;
		foreach (RegisteredVote vote in _votes)
		{
			if (vote.who != null && vote.who.inputProfile != null)
			{
				float wobbleOffset = (float)Math.Sin(vote.wobbleInc) * vote.wobble * 3f;
				Vec2 pos2 = (Network.isActive ? vote.leftStick : vote.who.inputProfile.leftStick);
				vote.who.persona.skipSprite.angle = wobbleOffset * 0.03f + pos2.y * 0.4f;
				float wingXOffset = 0f;
				float posMul = 3f;
				float wingPlus = 49f;
				if (vote.vote == VoteType.Skip)
				{
					vote.who.persona.skipSprite.frame = 0;
				}
				else
				{
					wingXOffset = -50f;
					posMul = 20f;
					wingPlus = 68f;
					vote.who.persona.skipSprite.frame = 1;
				}
				Graphics.Draw(vote.who.persona.skipSprite, Layer.HUD.width + wingPlus - vote.slide * 48f + pos2.x * posMul + wingXOffset, Layer.HUD.height - 28f - (float)(index * 16) - pos2.y * posMul, 0.9f);
				vote.who.persona.skipSprite.frame = 1;
				Vec2 pos3 = (Network.isActive ? vote.rightStick : vote.who.inputProfile.rightStick);
				if (vote.vote == VoteType.None)
				{
					wingXOffset = -50f;
				}
				vote.who.persona.skipSprite.angle = wobbleOffset * 0.03f + Maths.DegToRad(Maths.PointDirection(Vec2.Zero, pos3) - 180f);
				Graphics.Draw(vote.who.persona.skipSprite, Layer.HUD.width + 68f - vote.slide * 48f + pos3.x * 20f + wingXOffset, Layer.HUD.height - 32f - (float)(index * 16) - pos3.y * 20f, 0.9f);
				index++;
			}
		}
	}
}
